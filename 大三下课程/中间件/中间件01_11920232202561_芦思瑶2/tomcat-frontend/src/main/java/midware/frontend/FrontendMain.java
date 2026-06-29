package midware.frontend;

import org.apache.catalina.Context;
import org.apache.catalina.Wrapper;
import org.apache.catalina.startup.Tomcat;

import java.io.File;

public class FrontendMain {
    public static void main(String[] args) throws Exception {
        int port = 8080;
        Tomcat tomcat = new Tomcat();
        tomcat.setBaseDir(new File("target/tomcat-base").getAbsolutePath());
        tomcat.setPort(port);
        // 创建默认 connector，否则可能出现“日志显示启动但端口未监听”
        tomcat.getConnector();

        Context ctx = tomcat.addContext("", new File(".").getAbsolutePath());

        Tomcat.addServlet(ctx, "indexServlet", new IndexServlet());
        ctx.addServletMappingDecoded("/", "indexServlet");

        Tomcat.addServlet(ctx, "publishServlet", new PublishServlet());
        ctx.addServletMappingDecoded("/api/publish", "publishServlet");

        Wrapper events = Tomcat.addServlet(ctx, "eventsServlet", new EventsServlet());
        events.setAsyncSupported(true);
        ctx.addServletMappingDecoded("/api/events", "eventsServlet");

        tomcat.start();
        System.out.println("Frontend running: http://localhost:" + port + "/");
        tomcat.getServer().await();
    }
}

