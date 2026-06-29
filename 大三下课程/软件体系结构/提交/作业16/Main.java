import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Dimension;
import java.awt.Font;
import java.awt.GridLayout;
import java.awt.event.ActionEvent;

import javax.swing.BorderFactory;
import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.SwingConstants;
import javax.swing.SwingUtilities;
import javax.swing.UIManager;

/**
 * 口香糖机状态模式 — Swing 图形界面
 */
public class Main {

	private JLabel statusLabel;
	private JLabel feedbackLabel;
	private GumballMachine machine;

	private void buildUi() {
		machine = new GumballMachine(5);

		JFrame frame = new JFrame("口香糖机 — 状态模式");
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		frame.setLayout(new BorderLayout(10, 10));

		JLabel title = new JLabel("口香糖自动售货机", SwingConstants.CENTER);
		title.setFont(title.getFont().deriveFont(Font.BOLD, 18f));
		title.setForeground(new Color(183, 28, 28));
		title.setBorder(BorderFactory.createEmptyBorder(12, 0, 4, 0));

		statusLabel = new JLabel("<html>" + formatStatus(machine.toString()) + "</html>");
		statusLabel.setVerticalAlignment(SwingConstants.TOP);
		statusLabel.setOpaque(true);
		statusLabel.setBackground(Color.WHITE);
		statusLabel.setBorder(BorderFactory.createCompoundBorder(
				BorderFactory.createLineBorder(new Color(239, 154, 154)),
				BorderFactory.createEmptyBorder(10, 12, 10, 12)));

		feedbackLabel = new JLabel("欢迎使用！请点击下方按钮操作。", SwingConstants.CENTER);
		feedbackLabel.setFont(feedbackLabel.getFont().deriveFont(Font.BOLD, 14f));
		feedbackLabel.setForeground(new Color(21, 101, 192));

		JPanel center = new JPanel(new BorderLayout(0, 12));
		center.setBorder(BorderFactory.createEmptyBorder(8, 16, 8, 16));
		center.setBackground(new Color(255, 245, 245));
		center.add(statusLabel, BorderLayout.CENTER);
		center.add(feedbackLabel, BorderLayout.SOUTH);

		JPanel buttonPanel = new JPanel(new GridLayout(2, 2, 12, 12));
		buttonPanel.setBorder(BorderFactory.createEmptyBorder(0, 24, 16, 24));
		buttonPanel.setBackground(new Color(255, 245, 245));

		buttonPanel.add(makeButton("投币", e -> applyAction(machine.insertQuarter())));
		buttonPanel.add(makeButton("转曲柄", e -> applyAction(machine.turnCrank())));
		buttonPanel.add(makeButton("退币", e -> applyAction(machine.ejectQuarter())));
		buttonPanel.add(makeButton("补货 (+5)", e -> applyAction(machine.refill(5))));

		frame.add(title, BorderLayout.NORTH);
		frame.add(center, BorderLayout.CENTER);
		frame.add(buttonPanel, BorderLayout.SOUTH);
		frame.getContentPane().setBackground(new Color(255, 245, 245));
		frame.setMinimumSize(new Dimension(400, 300));
		frame.pack();
		frame.setLocationRelativeTo(null);
		frame.setVisible(true);
	}

	private JButton makeButton(String text, java.util.function.Consumer<ActionEvent> handler) {
		JButton button = new JButton(text);
		button.setBackground(new Color(229, 57, 53));
		button.setForeground(Color.WHITE);
		button.setFocusPainted(false);
		button.addActionListener(handler::accept);
		return button;
	}

	private void applyAction(String message) {
		statusLabel.setText("<html>" + formatStatus(machine.toString()) + "</html>");
		feedbackLabel.setText(message.replace("\n", "  "));
	}

	private static String formatStatus(String text) {
		return text.replace("\n", "<br/>");
	}

	public static void main(String[] args) {
		try {
			UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
		} catch (Exception ignored) {
		}
		SwingUtilities.invokeLater(() -> new Main().buildUi());
	}
}
