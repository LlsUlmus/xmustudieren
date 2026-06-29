import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.Scanner;

public class ConsumerClient {
    public static void main(String[] args) throws Exception {
        Socket socket = new Socket("127.0.0.1", 9999);
        BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
        PrintWriter out = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8), true);

        System.out.println(in.readLine());
        try (Scanner scanner = new Scanner(System.in, StandardCharsets.UTF_8)) {
            System.out.print("请输入订阅的 topic：");
            String topic = scanner.nextLine().trim();

            out.println("SUBSCRIBE " + topic);
            System.out.println(in.readLine());
            System.out.println("开始接收消息...");

            String line;
            while ((line = in.readLine()) != null) {
                System.out.println(line);
            }
        }
        socket.close();
    }
}
