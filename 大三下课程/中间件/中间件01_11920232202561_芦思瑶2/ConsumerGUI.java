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

public class ConsumerGUI {
    private Socket socket;
    private BufferedReader in;
    private PrintWriter out;

    private final JTextField topicField = new JTextField("news", 20);
    private final JTextArea logArea = new JTextArea(14, 45);
    private Thread readerThread;

    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> new ConsumerGUI().show());
    }

    private void show() {
        JFrame frame = new JFrame("Consumer - Subscribe & Receive");
        frame.setDefaultCloseOperation(WindowConstants.DISPOSE_ON_CLOSE);
        frame.setLayout(new BorderLayout());

        JPanel top = new JPanel(new FlowLayout(FlowLayout.LEFT));
        top.add(new JLabel("Topic:"));
        top.add(topicField);

        JButton btn = new JButton("订阅(SUBSCRIBE)");
        btn.addActionListener(e -> onSubscribe());
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

    private void onSubscribe() {
        String topic = topicField.getText().trim();
        if (topic.isEmpty()) {
            appendLog("topic 不能为空");
            return;
        }
        try {
            ensureConnected();
            out.println("SUBSCRIBE " + topic);
            String resp = in.readLine(); // OK SUBSCRIBE ...
            appendLog(resp);

            startReaderLoop();
        } catch (Exception ex) {
            appendLog("订阅失败: " + ex.getMessage());
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

    private void startReaderLoop() {
        if (readerThread != null && readerThread.isAlive()) return;
        readerThread = new Thread(() -> {
            try {
                String line;
                while ((line = in.readLine()) != null) {
                    final String msg = line;
                    SwingUtilities.invokeLater(() -> appendLog(msg));
                }
            } catch (Exception ignored) {
            }
        });
        readerThread.setDaemon(true);
        readerThread.start();
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

