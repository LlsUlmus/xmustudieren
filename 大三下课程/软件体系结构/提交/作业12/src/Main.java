/**
 * 客户端：根据参数选择具体建造者
 */
public class Main {
    public static void main(String[] args) {
        if (args.length != 1) {
            usage();
            return;
        }

        Builder builder;
        switch (args[0]) {
            case "plain":
                builder = new TextBuilder();
                break;
            case "html":
                builder = new HTMLBuilder();
                break;
            case "markdown":
                builder = new MarkdownBuilder();
                break;
            default:
                usage();
                return;
        }

        Director director = new Director(builder);
        Object result = director.construct();

        switch (args[0]) {
            case "plain":
                System.out.println(result);
                break;
            case "html":
                System.out.println("已生成 HTML 文件：" + result);
                break;
            case "markdown":
                System.out.println("Markdown 内容：\n" + result);
                break;
            default:
                usage();
        }
    }

    private static void usage() {
        System.out.println("用法示例：");
        System.out.println("  java Main plain      输出纯文本");
        System.out.println("  java Main html       生成 daily_message.html");
        System.out.println("  java Main markdown   输出 Markdown（新增建造者）");
    }
}
