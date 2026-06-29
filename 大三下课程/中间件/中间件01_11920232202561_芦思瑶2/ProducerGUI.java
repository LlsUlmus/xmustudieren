import javax.swing.*;
import java.awt.*;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.Socket;
import java.nio.charset.StandardCharsets;

public class ProducerGUI {
    private Socket socket;
    private BufferedReader in;
    private PrintWriter out;

    private final JTextField topicField = new JTextField("news", 20);
    private final JTextField msgField = new JTextField("", 30);
    private final JTextArea logArea = new JTextArea(12, 45);

    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> new ProducerGUI().show());
    }

    private void show() {
        JFrame frame = new JFrame("Producer - Publish to Topic");
        frame.setDefaultCloseOperation(WindowConstants.DISPOSE_ON_CLOSE);
        frame.setLayout(new BorderLayout());

        JPanel top = new JPanel(new FlowLayout(FlowLayout.LEFT));
        top.add(new JLabel("Topic:"));
        top.add(topicField);
        top.add(new JLabel("Message:"));
        top.add(msgField);

        JButton btn = new JButton("发布(PUBLISH)");
        btn.addActionListener(e -> onPublish());
        top.add(btn);

        logArea.setEditable(false);
        frame.add(top, BorderLayout.NORTH);
        frame.add(new JScrollPane(logArea), BorderLayout.CENTER);

        frame.addWindowListener(new WindowAdapter() {
            @Override
            public void windowClosing(WindowEvent e) {
                closeQuietly();
            }
        });

        frame.pack();
        frame.setLocationRelativeTo(null);
        frame.setVisible(true);
    }

    private void onPublish() {
        String topic = topicField.getText().trim();
        String msg = msgField.getText().trim();
        if (topic.isEmpty() || msg.isEmpty()) {
            appendLog("topic 或 message 不能为空");
            return;
        }
        try {
            ensureConnected();
            out.println("PUBLISH " + topic + " " + msg);
            String resp = in.readLine(); // OK PUBLISH ...
            appendLog(resp);
        } catch (Exception ex) {
            appendLog("发送失败: " + ex.getMessage());
        }
    }

    private void ensureConnected() throws Exception {
        if (socket != null && socket.isConnected() && !socket.isClosed()) return;

        socket = new Socket("127.0.0.1", 9999);
        in = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
        out = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8), true);

        String connected = in.readLine(); // CONNECTED
        appendLog(connected);
    }

    private void appendLog(String s) {
        logArea.append(s + "\n");
        logArea.setCaretPosition(logArea.getDocument().getLength());
    }

    private void closeQuietly() {
        try {
            if (socket != null) socket.close();
        } catch (Exception ignored) {
        }
    }
}

