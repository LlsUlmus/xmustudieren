package midware.frontend;

import jakarta.servlet.AsyncContext;
import jakarta.servlet.http.HttpServlet;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import java.io.IOException;
import java.io.PrintWriter;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.function.Consumer;

public class EventsServlet extends HttpServlet {
    private final ExecutorService pool = Executors.newCachedThreadPool();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws IOException {
        req.setCharacterEncoding("UTF-8");
        String topic = req.getParameter("topic");
        if (topic == null || topic.isBlank()) {
            resp.setStatus(400);
            resp.setCharacterEncoding("UTF-8");
            resp.setContentType("text/plain;charset=UTF-8");
            resp.getWriter().write("ERROR: topic 不能为空");
            return;
        }
        String topicTrim = topic.trim();

        resp.setCharacterEncoding("UTF-8");
        resp.setContentType("text/event-stream;charset=UTF-8");
        resp.setHeader("Cache-Control", "no-cache");
        resp.setHeader("Connection", "keep-alive");

        // 必须启用异步上下文，避免 doGet 返回后容器关闭响应流导致 EventSource 反复断开。
        AsyncContext async = req.startAsync();
        async.setTimeout(0);

        pool.submit(() -> {
            PrintWriter writer = null;
            try {
                writer = async.getResponse().getWriter();
                writer.write(": connected\n\n");
                writer.flush();

                PrintWriter finalWriter = writer;
                Consumer<String> push = (line) -> {
                    if (line != null && line.startsWith("MESSAGE " + topicTrim + " ")) {
                        finalWriter.write("data: " + escapeForSse(line) + "\n\n");
                        finalWriter.flush();
                    }
                };

                BrokerBridge.subscribeStream(topicTrim, push);
            } catch (Exception ignored) {
                // 客户端断开或 broker 异常时结束该 SSE 会话
            } finally {
                try { if (writer != null) writer.close(); } catch (Exception ignored) {}
                try { async.complete(); } catch (Exception ignored) {}
            }
        });
    }

    private static String escapeForSse(String s) {
        // SSE 简单转义：避免 data 行里出现过多换行导致协议破坏
        return s.replace("\r", "").replace("\n", "\\n");
    }
}

