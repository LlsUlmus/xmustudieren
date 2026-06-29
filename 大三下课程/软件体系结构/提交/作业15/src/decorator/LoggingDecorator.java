package decorator;

public class LoggingDecorator extends RequestHandlerDecorator {

    public LoggingDecorator(HttpRequestHandler wrapped) {
        super(wrapped);
    }

    @Override
    public String handleRequest(String request) {
        System.out.println("[LOG] Incoming request: " + request);
        String result = super.handleRequest(request);
        System.out.println("[LOG] Outgoing response: " + result);
        return result;
    }
}
