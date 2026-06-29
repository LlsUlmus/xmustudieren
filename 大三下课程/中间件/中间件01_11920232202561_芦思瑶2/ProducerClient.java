import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.Scanner;

public class ProducerClient {
    public static void main(String[] args) throws Exception {
        Socket socket = new Socket("127.0.0.1", 9999);
        BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
        PrintWriter out = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8), true);

        System.out.println(in.readLine());
        System.out.println("输入格式：topic 消息内容，例如 news 今天天气不错（输入 exit 退出）");

        try (Scanner scanner = new Scanner(System.in, StandardCharsets.UTF_8)) {
            while (true) {
                String line = scanner.nextLine();
                if ("exit".equalsIgnoreCase(line.trim())) {
                    break;
                }
                int idx = line.indexOf(' ');
                if (idx <= 0) {
                    System.out.println("格式错误，请重新输入");
                    continue;
                }
                String topic = line.substring(0, idx).trim();
                String msg = line.substring(idx + 1).trim();
                if (topic.isEmpty() || msg.isEmpty()) {
                    System.out.println("topic 或消息不能为空");
                    continue;
                }
                out.println("PUBLISH " + topic + " " + msg);
                System.out.println(in.readLine());
            }
        }

        socket.close();
    }
}
