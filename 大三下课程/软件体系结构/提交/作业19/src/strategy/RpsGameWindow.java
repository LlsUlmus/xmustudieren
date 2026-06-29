package strategy;

import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;

/**
 * 用 Swing GUI 演示策略模式：两位玩家可选用不同出招策略进行对战
 */
public class RpsGameWindow extends JFrame {

    private final JTextField nameField1 = new JTextField("Taro", 8);
    private final JTextField nameField2 = new JTextField("Hana", 8);
    private final JTextField seedField1 = new JTextField("1", 5);
    private final JTextField seedField2 = new JTextField("2", 5);
    private final JComboBox<String> strategyBox1 = new JComboBox<>(new String[]{"胜则沿用", "概率统计"});
    private final JComboBox<String> strategyBox2 = new JComboBox<>(new String[]{"胜则沿用", "概率统计"});
    private final JSpinner roundSpinner = new JSpinner(new SpinnerNumberModel(10, 1, 1000, 1));
    private final JTextArea logArea = new JTextArea(14, 50);
    private final JLabel statsLabel = new JLabel(" ");

    private Player player1;
    private Player player2;

    public RpsGameWindow() {
        super("策略模式 — 石头剪刀布");
        setDefaultCloseOperation(WindowConstants.EXIT_ON_CLOSE);
        buildUi();
        rebuildPlayers();
        refreshStats();
        setMinimumSize(new Dimension(620, 480));
        pack();
        setSize(720, 520);
        setLocationRelativeTo(null);
    }

    private void buildUi() {
        JPanel config = new JPanel(new GridLayout(2, 1, 0, 6));
        config.add(buildPlayerRow("玩家一", nameField1, strategyBox1, seedField1));
        config.add(buildPlayerRow("玩家二", nameField2, strategyBox2, seedField2));

        JPanel control = new JPanel(new FlowLayout(FlowLayout.LEFT, 8, 4));
        control.add(new JLabel("对战局数:"));
        control.add(roundSpinner);
        JButton oneRoundBtn = new JButton("进行一局");
        JButton multiBtn = new JButton("批量对战");
        JButton resetBtn = new JButton("重置统计");
        control.add(oneRoundBtn);
        control.add(multiBtn);
        control.add(resetBtn);

        logArea.setEditable(false);
        logArea.setLineWrap(true);
        logArea.setWrapStyleWord(true);
        logArea.setFont(new Font(Font.MONOSPACED, Font.PLAIN, 13));
        JScrollPane scroll = new JScrollPane(logArea);
        scroll.setPreferredSize(new Dimension(680, 280));

        JPanel footer = new JPanel(new BorderLayout(4, 4));
        footer.add(control, BorderLayout.NORTH);
        footer.add(statsLabel, BorderLayout.SOUTH);

        JPanel root = new JPanel(new BorderLayout(8, 8));
        root.setBorder(BorderFactory.createEmptyBorder(10, 10, 10, 10));
        root.add(config, BorderLayout.NORTH);
        root.add(scroll, BorderLayout.CENTER);
        root.add(footer, BorderLayout.SOUTH);

        setContentPane(root);

        oneRoundBtn.addActionListener(this::onOneRound);
        multiBtn.addActionListener(this::onMultiRound);
        resetBtn.addActionListener(e -> {
            rebuildPlayers();
            logArea.setText("");
            refreshStats();
        });
    }

    private JPanel buildPlayerRow(String title, JTextField nameField,
                                  JComboBox<String> strategyBox, JTextField seedField) {
        JPanel row = new JPanel(new FlowLayout(FlowLayout.LEFT, 6, 2));
        row.setBorder(BorderFactory.createTitledBorder(title));
        row.add(new JLabel("姓名"));
        row.add(nameField);
        row.add(new JLabel("策略"));
        row.add(strategyBox);
        row.add(new JLabel("随机种子"));
        row.add(seedField);
        return row;
    }

    private void onOneRound(ActionEvent e) {
        ensurePlayers();
        playSingleRound();
        refreshStats();
    }

    private void onMultiRound(ActionEvent e) {
        rebuildPlayers();
        logArea.setText("");
        int n = (Integer) roundSpinner.getValue();
        for (int i = 0; i < n; i++) {
            playSingleRound();
        }
        appendLog("—— 批量对战结束 ——");
        refreshStats();
    }

    private void playSingleRound() {
        Hand h1 = player1.play();
        Hand h2 = player2.play();
        String line = player1.getName() + " 出 " + h1 + "，"
                + player2.getName() + " 出 " + h2 + " → ";

        if (h1.beats(h2)) {
            player1.recordWin();
            player2.recordLoss();
            line += "胜: " + player1.getName();
        } else if (h2.beats(h1)) {
            player1.recordLoss();
            player2.recordWin();
            line += "胜: " + player2.getName();
        } else {
            player1.recordDraw();
            player2.recordDraw();
            line += "平局";
        }
        appendLog(line);
    }

    private void appendLog(String text) {
        logArea.append(text + "\n");
        logArea.setCaretPosition(logArea.getDocument().getLength());
    }

    private void refreshStats() {
        statsLabel.setText("统计: " + player1 + "  |  " + player2);
    }

    private void ensurePlayers() {
        if (player1 == null || player2 == null) {
            rebuildPlayers();
        }
    }

    private void rebuildPlayers() {
        try {
            int seed1 = Integer.parseInt(seedField1.getText().trim());
            int seed2 = Integer.parseInt(seedField2.getText().trim());
            player1 = new Player(nameField1.getText().trim(), createStrategy(strategyBox1, seed1));
            player2 = new Player(nameField2.getText().trim(), createStrategy(strategyBox2, seed2));
        } catch (NumberFormatException ex) {
            JOptionPane.showMessageDialog(this, "随机种子必须是整数", "输入错误", JOptionPane.WARNING_MESSAGE);
            player1 = new Player("Taro", new WinningStrategy(1));
            player2 = new Player("Hana", new ProbStrategy(2));
        }
    }

    private Strategy createStrategy(JComboBox<String> box, int seed) {
        if (box.getSelectedIndex() == 0) {
            return new WinningStrategy(seed);
        }
        return new ProbStrategy(seed);
    }
}
