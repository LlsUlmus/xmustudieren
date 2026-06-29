import java.io.IOException;
import java.io.PrintWriter;
import java.nio.charset.StandardCharsets;

public class HTMLBuilder extends Builder {
    private String filename;
    private PrintWriter writer;

    @Override
    public void makeTitle(String title) {
        filename = "daily_message.html";
        try {
            writer = new PrintWriter(filename, StandardCharsets.UTF_8);
        } catch (IOException e) {
            throw new RuntimeException("无法创建 HTML 文件", e);
        }
        writer.println("<!DOCTYPE html>");
        writer.println("<html lang=\"zh-CN\">");
        writer.println("<head>");
        writer.println("<meta charset=\"UTF-8\">");
        writer.println("<title>" + escape(title) + "</title>");
        writer.println("</head>");
        writer.println("<body>");
        writer.println("<h1>" + escape(title) + "</h1>");
    }

    @Override
    public void makeString(String str) {
        writer.println("<p class=\"text\">" + escape(str) + "</p>");
    }

    @Override
    public void makeItems(String[] items) {
        writer.println("<ul class=\"list\">");
        for (String item : items) {
            writer.println("<li>" + escape(item) + "</li>");
        }
        writer.println("</ul>");
    }

    @Override
    public Object getResult() {
        writer.println("</body>");
        writer.println("</html>");
        writer.close();
        return filename;
    }

    private static String escape(String raw) {
        return raw.replace("&", "&amp;")
                .replace("<", "&lt;")
                .replace(">", "&gt;")
                .replace("\"", "&quot;");
    }
}
