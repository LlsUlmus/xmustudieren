package gui;

import fs.Directory;
import fs.Entry;
import fs.FileSystemLoader;
import fs.FsFile;

import javax.swing.*;
import javax.swing.border.EmptyBorder;
import javax.swing.tree.DefaultTreeCellRenderer;
import java.awt.*;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.PrintStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Path;

public class FileSystemGUI extends JFrame {

    private static final int DEFAULT_MAX_DEPTH = 6;

    private final JLabel pathLabel = new JLabel("尚未选择目录");
    private final JLabel sizeLabel = new JLabel("总大小: -");
    private final JTree tree = new JTree();
    private final JTextArea detailArea = new JTextArea(12, 40);
    private final JSpinner depthSpinner = new JSpinner(
            new SpinnerNumberModel(DEFAULT_MAX_DEPTH, 1, 20, 1));

    private Directory currentRoot;

    public FileSystemGUI() {
        super("组合模式 - 文件系统浏览器 (Homework 20)");
        setDefaultCloseOperation(WindowConstants.EXIT_ON_CLOSE);
        setMinimumSize(new Dimension(900, 560));
        setLocationRelativeTo(null);

        pathLabel.setFont(pathLabel.getFont().deriveFont(Font.BOLD, 13f));
        detailArea.setEditable(false);
        detailArea.setFont(new Font(Font.MONOSPACED, Font.PLAIN, 13));

        tree.setCellRenderer(new EntryRenderer());
        tree.addTreeSelectionListener(e -> updateDetail());

        JButton openBtn = new JButton("选择子目录…");
        openBtn.addActionListener(e -> chooseDirectory());

        JButton refreshBtn = new JButton("重新加载");
        refreshBtn.addActionListener(e -> reloadCurrent());

        JButton printBtn = new JButton("printList 输出");
        printBtn.addActionListener(e -> showPrintList());

        JPanel top = new JPanel(new BorderLayout(8, 8));
        top.setBorder(new EmptyBorder(10, 10, 6, 10));
        JPanel topButtons = new JPanel(new FlowLayout(FlowLayout.LEFT, 8, 0));
        topButtons.add(openBtn);
        topButtons.add(refreshBtn);
        topButtons.add(printBtn);
        topButtons.add(new JLabel("扫描深度:"));
        topButtons.add(depthSpinner);
        top.add(topButtons, BorderLayout.NORTH);
        top.add(pathLabel, BorderLayout.CENTER);
        top.add(sizeLabel, BorderLayout.SOUTH);

        JSplitPane split = new JSplitPane(
                JSplitPane.HORIZONTAL_SPLIT,
                new JScrollPane(tree),
                new JScrollPane(detailArea));
        split.setResizeWeight(0.42);
        split.setBorder(new EmptyBorder(0, 10, 10, 10));

        getContentPane().setLayout(new BorderLayout());
        getContentPane().add(top, BorderLayout.NORTH);
        getContentPane().add(split, BorderLayout.CENTER);
    }

    private void chooseDirectory() {
        JFileChooser chooser = new JFileChooser();
        chooser.setFileSelectionMode(JFileChooser.DIRECTORIES_ONLY);
        chooser.setDialogTitle("选择要浏览的子目录");
        if (chooser.showOpenDialog(this) != JFileChooser.APPROVE_OPTION) {
            return;
        }
        loadDirectory(chooser.getSelectedFile().toPath());
    }

    private void reloadCurrent() {
        if (currentRoot == null) {
            JOptionPane.showMessageDialog(this, "请先选择一个目录。", "提示",
                    JOptionPane.INFORMATION_MESSAGE);
            return;
        }
        Path path = Path.of(pathLabel.getToolTipText());
        loadDirectory(path);
    }

    private void loadDirectory(Path path) {
        int depth = (Integer) depthSpinner.getValue();
        try {
            setCursor(Cursor.getPredefinedCursor(Cursor.WAIT_CURSOR));
            currentRoot = FileSystemLoader.load(path, depth);
            tree.setModel(new EntryTreeModel(currentRoot));
            for (int i = 0; i < tree.getRowCount(); i++) {
                tree.expandRow(i);
            }
            pathLabel.setText("当前目录: " + path.toAbsolutePath());
            pathLabel.setToolTipText(path.toAbsolutePath().toString());
            sizeLabel.setText("总大小: " + Entry.formatSize(currentRoot.getSizeLong())
                    + "  （通过 Entry.getSize() 递归汇总）");
            updateDetail();
        } catch (IOException ex) {
            JOptionPane.showMessageDialog(this,
                    "加载失败: " + ex.getMessage(),
                    "错误",
                    JOptionPane.ERROR_MESSAGE);
        } finally {
            setCursor(Cursor.getDefaultCursor());
        }
    }

    private void updateDetail() {
        Object node = tree.getLastSelectedPathComponent();
        if (node == null) {
            detailArea.setText("在左侧选择文件或目录查看详情。");
            return;
        }
        Entry entry = (Entry) node;
        StringBuilder sb = new StringBuilder();
        sb.append("名称: ").append(entry.getName()).append('\n');
        if (entry instanceof FsFile file) {
            sb.append("类型: 文件 (Leaf)\n");
            sb.append("大小: ").append(Entry.formatSize(file.getSizeLong())).append('\n');
        } else if (entry instanceof Directory dir) {
            sb.append("类型: 目录 (Composite)\n");
            sb.append("直接子项: ").append(dir.getChildren().size()).append('\n');
            sb.append("递归总大小: ").append(Entry.formatSize(dir.getSizeLong())).append('\n');
        }
        sb.append("\n组合模式说明:\n");
        sb.append("客户端通过 Entry 接口统一访问文件与目录，\n");
        sb.append("无需区分 Leaf 与 Composite 即可完成 getSize() 等操作。\n");
        detailArea.setText(sb.toString());
        detailArea.setCaretPosition(0);
    }

    private void showPrintList() {
        if (currentRoot == null) {
            JOptionPane.showMessageDialog(this, "请先加载目录。", "提示",
                    JOptionPane.INFORMATION_MESSAGE);
            return;
        }
        ByteArrayOutputStream buffer = new ByteArrayOutputStream();
        PrintStream old = System.out;
        try (PrintStream capture = new PrintStream(buffer, true, StandardCharsets.UTF_8)) {
            System.setOut(capture);
            currentRoot.printList();
        } finally {
            System.setOut(old);
        }
        String text = buffer.toString(StandardCharsets.UTF_8);
        JTextArea area = new JTextArea(text, 22, 50);
        area.setEditable(false);
        area.setFont(new Font(Font.MONOSPACED, Font.PLAIN, 13));
        JOptionPane.showMessageDialog(this, new JScrollPane(area),
                "printList() 递归列表", JOptionPane.PLAIN_MESSAGE);
    }

    private static class EntryRenderer extends DefaultTreeCellRenderer {
        @Override
        public Component getTreeCellRendererComponent(JTree tree, Object value,
                boolean selected, boolean expanded, boolean leaf, int row,
                boolean hasFocus) {
            super.getTreeCellRendererComponent(tree, value, selected, expanded, leaf, row, hasFocus);
            if (value instanceof FsFile) {
                setIcon(UIManager.getIcon("FileView.fileIcon"));
            } else if (value instanceof Directory) {
                setIcon(UIManager.getIcon("FileView.directoryIcon"));
            }
            if (value instanceof Entry entry) {
                setText(entry.toString());
            }
            return this;
        }
    }

    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> {
            try {
                UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
            } catch (Exception ignored) {
            }
            FileSystemGUI gui = new FileSystemGUI();
            gui.setVisible(true);
            if (args.length > 0) {
                gui.loadDirectory(Path.of(args[0]));
            }
        });
    }
}
