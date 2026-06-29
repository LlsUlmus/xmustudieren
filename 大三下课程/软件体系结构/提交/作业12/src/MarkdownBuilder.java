/**
 * 新增的具体建造者：Markdown 格式（作业扩展）
 */
public class MarkdownBuilder extends Builder {
    private final StringBuilder md = new StringBuilder();

    @Override
    public void makeTitle(String title) {
        md.append("# ").append(title).append("\n\n");
    }

    @Override
    public void makeString(String str) {
        md.append("> ").append(str).append("\n\n");
    }

    @Override
    public void makeItems(String[] items) {
        for (int i = 0; i < items.length; i++) {
            md.append(i + 1).append(". ").append(items[i]).append('\n');
        }
        md.append('\n');
    }

    @Override
    public Object getResult() {
        md.append("---\n*由 MarkdownBuilder 生成*\n");
        return md.toString();
    }
}
