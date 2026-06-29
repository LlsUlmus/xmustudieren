/**
 * 具体建造者：纯文本格式
 */
public class TextBuilder extends Builder {
    private final StringBuilder buffer = new StringBuilder();

    @Override
    public void makeTitle(String title) {
        buffer.append("┌──────────────────────────────┐\n");
        buffer.append("│ ").append(title).append('\n');
        buffer.append("└──────────────────────────────┘\n\n");
    }

    @Override
    public void makeString(String str) {
        buffer.append("【段落】 ").append(str).append('\n');
    }

    @Override
    public void makeItems(String[] items) {
        for (String item : items) {
            buffer.append("  ◆ ").append(item).append('\n');
        }
        buffer.append('\n');
    }

    @Override
    public Object getResult() {
        buffer.append("──────── 全文结束 ────────\n");
        return buffer.toString();
    }
}
