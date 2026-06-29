package midware.frontend;

import jakarta.servlet.http.HttpServlet;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import java.io.IOException;

public class PublishServlet extends HttpServlet {
    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp) throws IOException {
        req.setCharacterEncoding("UTF-8");
        resp.setCharacterEncoding("UTF-8");
        resp.setContentType("text/plain;charset=UTF-8");

        String topic = req.getParameter("topic");
        String message = req.getParameter("message");
        if (topic == null || message == null || topic.isBlank() || message.isBlank()) {
            resp.getWriter().write("ERROR: topic 或 message 不能为空");
            return;
        }

        try {
            BrokerBridge.publish(topic.trim(), message.trim());
            resp.getWriter().write("OK");
        } catch (Exception e) {
            resp.getWriter().write("ERROR: " + e.getMessage());
        }
    }
}

