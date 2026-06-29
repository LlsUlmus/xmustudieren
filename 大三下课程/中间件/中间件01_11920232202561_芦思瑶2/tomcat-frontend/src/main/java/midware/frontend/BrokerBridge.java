package midware.frontend;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.Socket;
import java.nio.charset.StandardCharsets;

final class BrokerBridge {
    private BrokerBridge() {}

    static final String BROKER_HOST = "127.0.0.1";
    static final int BROKER_PORT = 9999;

    static void publish(String topic, String message) throws IOException {
        try (Socket socket = new Socket(BROKER_HOST, BROKER_PORT);
             BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
             PrintWriter out = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8), true)) {
            in.readLine(); // CONNECTED
            out.println("PUBLISH " + topic + " " + message);
            // OK PUBLISH ...
            in.readLine();
        }
    }

    /**
     * 连接 broker，发送订阅命令，然后把后续收到的行交给 consumer 处理。
     * 注意：调用方负责中断/关闭连接以结束循环。
     */
    static void subscribeStream(String topic, java.util.function.Consumer<String> consumer) throws IOException {
        Socket socket = new Socket(BROKER_HOST, BROKER_PORT);
        BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
        PrintWriter out = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8), true);

        // 订阅前读取 CONNECTED
        in.readLine();
        out.println("SUBSCRIBE " + topic);

        // OK SUBSCRIBE ...
        String ok = in.readLine();
        if (ok != null) consumer.accept(ok);

        try {
            String line;
            while ((line = in.readLine()) != null) {
                consumer.accept(line);
            }
        } finally {
            try { socket.close(); } catch (Exception ignored) {}
        }
    }
}

