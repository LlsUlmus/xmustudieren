import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.concurrent.CopyOnWriteArrayList;

public class BrokerServer {
    private static final int PORT = 9999;

    // topic -> subscribers
    private static final ConcurrentHashMap<String, CopyOnWriteArrayList<ClientHandler>> TOPIC_SUBSCRIBERS =
            new ConcurrentHashMap<>();

    // topic -> stored messages (store-and-forward queue)
    private static final ConcurrentHashMap<String, ConcurrentLinkedQueue<String>> TOPIC_STORE =
            new ConcurrentHashMap<>();

    public static void main(String[] args) throws Exception {
        try (ServerSocket serverSocket = new ServerSocket(PORT)) {
            System.out.println("Broker started on port " + PORT);
            while (true) {
                Socket socket = serverSocket.accept();
                ClientHandler handler = new ClientHandler(socket);
                new Thread(handler).start();
            }
        }
    }

    static class ClientHandler implements Runnable {
        private final Socket socket;
        private BufferedReader in;
        private PrintWriter out;
        private final Set<String> subscribedTopics = new HashSet<>();

        ClientHandler(Socket socket) {
            this.socket = socket;
        }

        @Override
        public void run() {
            try {
                in = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
                out = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8), true);
                out.println("CONNECTED");

                String line;
                while ((line = in.readLine()) != null) {
                    handleLine(line.trim());
                }
            } catch (Exception ignored) {
            } finally {
                cleanup();
            }
        }

        private void handleLine(String line) {
            if (line.startsWith("SUBSCRIBE ")) {
                String topic = line.substring("SUBSCRIBE ".length()).trim();
                if (topic.isEmpty()) {
                    out.println("ERROR empty topic");
                    return;
                }
                TOPIC_SUBSCRIBERS.putIfAbsent(topic, new CopyOnWriteArrayList<>());
                TOPIC_SUBSCRIBERS.get(topic).add(this);
                subscribedTopics.add(topic);
                out.println("OK SUBSCRIBE " + topic);

                // Store-and-forward: push stored messages after subscription.
                TOPIC_STORE.putIfAbsent(topic, new ConcurrentLinkedQueue<>());
                String msg;
                while ((msg = TOPIC_STORE.get(topic).poll()) != null) {
                    out.println("MESSAGE " + topic + " " + msg + " [stored]");
                }
                return;
            }

            if (line.startsWith("PUBLISH ")) {
                String remain = line.substring("PUBLISH ".length()).trim();
                int firstSpace = remain.indexOf(' ');
                if (firstSpace <= 0) {
                    out.println("ERROR bad publish format");
                    return;
                }
                String topic = remain.substring(0, firstSpace).trim();
                String msg = remain.substring(firstSpace + 1).trim();
                if (topic.isEmpty() || msg.isEmpty()) {
                    out.println("ERROR empty topic or message");
                    return;
                }

                broadcastOrStore(topic, msg);
                out.println("OK PUBLISH " + topic);
                return;
            }

            out.println("ERROR unknown command");
        }

        private void broadcastOrStore(String topic, String msg) {
            TOPIC_SUBSCRIBERS.putIfAbsent(topic, new CopyOnWriteArrayList<>());
            List<ClientHandler> subs = TOPIC_SUBSCRIBERS.get(topic);

            if (subs.isEmpty()) {
                TOPIC_STORE.putIfAbsent(topic, new ConcurrentLinkedQueue<>());
                TOPIC_STORE.get(topic).offer(msg);
                return;
            }

            boolean delivered = false;
            for (ClientHandler handler : subs) {
                // 这里允许发布者也是订阅者时收到自己的消息，更贴近 pub/sub 行为
                handler.out.println("MESSAGE " + topic + " " + msg);
                delivered = true;
            }

            // 极端情况下如果都发送失败（比如连接已断但尚未清理），依然存储
            if (!delivered) {
                TOPIC_STORE.putIfAbsent(topic, new ConcurrentLinkedQueue<>());
                TOPIC_STORE.get(topic).offer(msg);
            }
        }

        private void cleanup() {
            for (String topic : subscribedTopics) {
                List<ClientHandler> subs = TOPIC_SUBSCRIBERS.get(topic);
                if (subs != null) {
                    subs.remove(this);
                }
            }
            try {
                socket.close();
            } catch (IOException ignored) {
            }
        }
    }
}
