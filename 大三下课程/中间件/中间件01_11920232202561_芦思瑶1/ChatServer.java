import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpHandler;
import com.sun.net.httpserver.HttpServer;
import org.apache.activemq.ActiveMQConnectionFactory;

import javax.jms.Connection;
import javax.jms.DeliveryMode;
import javax.jms.Destination;
import javax.jms.JMSException;
import javax.jms.MessageConsumer;
import javax.jms.MessageListener;
import javax.jms.MessageProducer;
import javax.jms.Session;
import javax.jms.TextMessage;
import java.io.IOException;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.net.URLDecoder;
import java.nio.charset.StandardCharsets;
import java.time.Instant;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

/**
 * Step4 独立版：JMS Queue 即时通讯服务端
 * 功能：文字消息、群发、watchdog 心跳、离线存储转发（由 Queue 天然支持）
 */
public class ChatServer {
    private static final int HTTP_PORT = 8080;
    private static final long OFFLINE_TIMEOUT_MS = 30_000;
    private static final String BROKER_URL = "vm://chat-broker?broker.persistent=false&broker.useJmx=false";
    private static final String QUEUE_PREFIX = "chat.user.";

    private static final Map<String, UserSession> USERS = new ConcurrentHashMap<>();
    private static final ScheduledExecutorService WATCHDOG = Executors.newSingleThreadScheduledExecutor();
    private static Connection producerConnection;
    private static Session producerSession;
    private static MessageProducer producer;

    public static void main(String[] args) throws Exception {
        initProducer();
        startWatchdog();
        startHttpServer();
    }

    private static void initProducer() throws JMSException {
        ActiveMQConnectionFactory factory = new ActiveMQConnectionFactory(BROKER_URL);
        producerConnection = factory.createConnection();
        producerConnection.start();
        producerSession = producerConnection.createSession(false, Session.AUTO_ACKNOWLEDGE);
        Destination defaultQueue = producerSession.createQueue(QUEUE_PREFIX + "default");
        producer = producerSession.createProducer(defaultQueue);
        producer.setDeliveryMode(DeliveryMode.PERSISTENT);
    }

    private static void startHttpServer() throws IOException {
        HttpServer server = HttpServer.create(new InetSocketAddress(HTTP_PORT), 0);
        server.createContext("/register", new RegisterHandler());
        server.createContext("/heartbeat", new HeartbeatHandler());
        server.createContext("/send", new SendHandler());
        server.createContext("/broadcast", new BroadcastHandler());
        server.createContext("/poll", new PollHandler());
        server.createContext("/online", new OnlineHandler());
        server.setExecutor(Executors.newCachedThreadPool());
        server.start();
        System.out.println("ChatServer started at http://localhost:" + HTTP_PORT);
        System.out.println("Open chat page: step4_standalone_jms/chat.html");
    }

    private static void startWatchdog() {
        WATCHDOG.scheduleAtFixedRate(() -> {
            long now = System.currentTimeMillis();
            for (Map.Entry<String, UserSession> entry : USERS.entrySet()) {
                UserSession user = entry.getValue();
                if (now - user.lastSeen > OFFLINE_TIMEOUT_MS) {
                    System.out.println("watchdog: " + user.username + " offline");
                    USERS.remove(user.username);
                    closeQuietly(user);
                }
            }
        }, 5, 5, TimeUnit.SECONDS);
    }

    private static class RegisterHandler implements HttpHandler {
        @Override
        public void handle(HttpExchange exchange) throws IOException {
            if (handlePreflight(exchange)) {
                return;
            }
            if (!"POST".equalsIgnoreCase(exchange.getRequestMethod())) {
                writeJson(exchange, 405, "{\"ok\":false,\"error\":\"method not allowed\"}");
                return;
            }
            Map<String, String> form = parseForm(exchange);
            String username = safeTrim(form.get("username"));
            if (username.isEmpty()) {
                writeJson(exchange, 400, "{\"ok\":false,\"error\":\"username required\"}");
                return;
            }
            try {
                UserSession existing = USERS.get(username);
                if (existing != null) {
                    existing.lastSeen = System.currentTimeMillis();
                    writeJson(exchange, 200, "{\"ok\":true,\"msg\":\"already online\"}");
                    return;
                }
                UserSession session = createConsumer(username);
                USERS.put(username, session);
                writeJson(exchange, 200, "{\"ok\":true,\"msg\":\"registered\"}");
                System.out.println("register: " + username);
            } catch (Exception e) {
                writeJson(exchange, 500, "{\"ok\":false,\"error\":\"" + esc(e.getMessage()) + "\"}");
            }
        }
    }

    private static class HeartbeatHandler implements HttpHandler {
        @Override
        public void handle(HttpExchange exchange) throws IOException {
            if (handlePreflight(exchange)) {
                return;
            }
            if (!"POST".equalsIgnoreCase(exchange.getRequestMethod())) {
                writeJson(exchange, 405, "{\"ok\":false,\"error\":\"method not allowed\"}");
                return;
            }
            Map<String, String> form = parseForm(exchange);
            String username = safeTrim(form.get("username"));
            UserSession user = USERS.get(username);
            if (user == null) {
                writeJson(exchange, 404, "{\"ok\":false,\"error\":\"user not online\"}");
                return;
            }
            user.lastSeen = System.currentTimeMillis();
            writeJson(exchange, 200, "{\"ok\":true}");
        }
    }

    private static class SendHandler implements HttpHandler {
        @Override
        public void handle(HttpExchange exchange) throws IOException {
            if (handlePreflight(exchange)) {
                return;
            }
            if (!"POST".equalsIgnoreCase(exchange.getRequestMethod())) {
                writeJson(exchange, 405, "{\"ok\":false,\"error\":\"method not allowed\"}");
                return;
            }
            Map<String, String> form = parseForm(exchange);
            String from = safeTrim(form.get("from"));
            String to = safeTrim(form.get("to"));
            String content = safeTrim(form.get("content"));
            if (from.isEmpty() || to.isEmpty() || content.isEmpty()) {
                writeJson(exchange, 400, "{\"ok\":false,\"error\":\"from/to/content required\"}");
                return;
            }
            try {
                sendMessage(from, to, content, false);
                writeJson(exchange, 200, "{\"ok\":true}");
            } catch (Exception e) {
                writeJson(exchange, 500, "{\"ok\":false,\"error\":\"" + esc(e.getMessage()) + "\"}");
            }
        }
    }

    private static class BroadcastHandler implements HttpHandler {
        @Override
        public void handle(HttpExchange exchange) throws IOException {
            if (handlePreflight(exchange)) {
                return;
            }
            if (!"POST".equalsIgnoreCase(exchange.getRequestMethod())) {
                writeJson(exchange, 405, "{\"ok\":false,\"error\":\"method not allowed\"}");
                return;
            }
            Map<String, String> form = parseForm(exchange);
            String from = safeTrim(form.get("from"));
            String content = safeTrim(form.get("content"));
            if (from.isEmpty() || content.isEmpty()) {
                writeJson(exchange, 400, "{\"ok\":false,\"error\":\"from/content required\"}");
                return;
            }
            int count = 0;
            for (String to : USERS.keySet()) {
                if (!to.equals(from)) {
                    try {
                        sendMessage(from, to, content, true);
                        count++;
                    } catch (Exception ignored) {
                    }
                }
            }
            writeJson(exchange, 200, "{\"ok\":true,\"delivered\":" + count + "}");
        }
    }

    private static class PollHandler implements HttpHandler {
        @Override
        public void handle(HttpExchange exchange) throws IOException {
            if (handlePreflight(exchange)) {
                return;
            }
            if (!"GET".equalsIgnoreCase(exchange.getRequestMethod())) {
                writeJson(exchange, 405, "{\"ok\":false,\"error\":\"method not allowed\"}");
                return;
            }
            Map<String, String> query = parseQuery(exchange.getRequestURI().getRawQuery());
            String username = safeTrim(query.get("user"));
            UserSession user = USERS.get(username);
            if (user == null) {
                writeJson(exchange, 404, "{\"ok\":false,\"error\":\"user not online\"}");
                return;
            }
            List<String> list = new ArrayList<>();
            String msg;
            while ((msg = user.inbox.poll()) != null) {
                list.add(msg);
            }
            StringBuilder sb = new StringBuilder();
            sb.append("{\"ok\":true,\"messages\":[");
            for (int i = 0; i < list.size(); i++) {
                if (i > 0) {
                    sb.append(",");
                }
                sb.append(list.get(i));
            }
            sb.append("]}");
            writeJson(exchange, 200, sb.toString());
        }
    }

    private static class OnlineHandler implements HttpHandler {
        @Override
        public void handle(HttpExchange exchange) throws IOException {
            if (handlePreflight(exchange)) {
                return;
            }
            if (!"GET".equalsIgnoreCase(exchange.getRequestMethod())) {
                writeJson(exchange, 405, "{\"ok\":false,\"error\":\"method not allowed\"}");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.append("{\"ok\":true,\"users\":[");
            int i = 0;
            for (String u : USERS.keySet()) {
                if (i++ > 0) {
                    sb.append(",");
                }
                sb.append("\"").append(esc(u)).append("\"");
            }
            sb.append("]}");
            writeJson(exchange, 200, sb.toString());
        }
    }

    private static UserSession createConsumer(String username) throws JMSException {
        ActiveMQConnectionFactory factory = new ActiveMQConnectionFactory(BROKER_URL);
        Connection conn = factory.createConnection();
        conn.start();
        Session session = conn.createSession(false, Session.AUTO_ACKNOWLEDGE);
        Destination dest = session.createQueue(QUEUE_PREFIX + username);
        MessageConsumer consumer = session.createConsumer(dest);
        UserSession user = new UserSession(username, conn, session, consumer);
        MessageListener listener = message -> {
            try {
                if (message instanceof TextMessage) {
                    user.inbox.add(((TextMessage) message).getText());
                }
            } catch (JMSException ignored) {
            }
        };
        consumer.setMessageListener(listener);
        return user;
    }

    private static void sendMessage(String from, String to, String content, boolean group) throws JMSException {
        Destination queue = producerSession.createQueue(QUEUE_PREFIX + to);
        MessageProducer p = producerSession.createProducer(queue);
        p.setDeliveryMode(DeliveryMode.PERSISTENT);
        String payload = "{\"from\":\"" + esc(from) + "\",\"to\":\"" + esc(to) + "\",\"content\":\"" + esc(content)
                + "\",\"group\":" + group + ",\"time\":\"" + esc(Instant.now().toString()) + "\"}";
        TextMessage message = producerSession.createTextMessage(payload);
        p.send(message);
        p.close();
    }

    private static void closeQuietly(UserSession user) {
        try {
            user.consumer.close();
        } catch (Exception ignored) {
        }
        try {
            user.jmsSession.close();
        } catch (Exception ignored) {
        }
        try {
            user.connection.close();
        } catch (Exception ignored) {
        }
    }

    private static String safeTrim(String v) {
        return v == null ? "" : v.trim();
    }

    private static Map<String, String> parseForm(HttpExchange exchange) throws IOException {
        byte[] data = exchange.getRequestBody().readAllBytes();
        String body = new String(data, StandardCharsets.UTF_8);
        return parseQuery(body);
    }

    private static Map<String, String> parseQuery(String raw) {
        Map<String, String> map = new ConcurrentHashMap<>();
        if (raw == null || raw.isEmpty()) {
            return map;
        }
        String[] pairs = raw.split("&");
        for (String pair : pairs) {
            String[] kv = pair.split("=", 2);
            String k = decode(kv[0]);
            String v = kv.length > 1 ? decode(kv[1]) : "";
            map.put(k, v);
        }
        return map;
    }

    private static String decode(String s) {
        return URLDecoder.decode(s, StandardCharsets.UTF_8);
    }

    private static String esc(String s) {
        if (s == null) {
            return "";
        }
        return s.replace("\\", "\\\\").replace("\"", "\\\"").replace("\n", "\\n").replace("\r", "\\r");
    }

    private static boolean handlePreflight(HttpExchange exchange) throws IOException {
        if (!"OPTIONS".equalsIgnoreCase(exchange.getRequestMethod())) {
            return false;
        }
        addCorsHeaders(exchange);
        exchange.sendResponseHeaders(204, -1);
        return true;
    }

    private static void addCorsHeaders(HttpExchange exchange) {
        exchange.getResponseHeaders().set("Access-Control-Allow-Origin", "*");
        exchange.getResponseHeaders().set("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        exchange.getResponseHeaders().set("Access-Control-Allow-Headers", "Content-Type");
    }

    private static void writeJson(HttpExchange exchange, int code, String json) throws IOException {
        byte[] out = json.getBytes(StandardCharsets.UTF_8);
        addCorsHeaders(exchange);
        exchange.getResponseHeaders().set("Content-Type", "application/json; charset=UTF-8");
        exchange.sendResponseHeaders(code, out.length);
        try (OutputStream os = exchange.getResponseBody()) {
            os.write(out);
        }
    }

    private static class UserSession {
        final String username;
        final Connection connection;
        final Session jmsSession;
        final MessageConsumer consumer;
        final ConcurrentLinkedQueue<String> inbox = new ConcurrentLinkedQueue<>();
        volatile long lastSeen;

        UserSession(String username, Connection connection, Session jmsSession, MessageConsumer consumer) {
            this.username = username;
            this.connection = connection;
            this.jmsSession = jmsSession;
            this.consumer = consumer;
            this.lastSeen = System.currentTimeMillis();
        }
    }
}
