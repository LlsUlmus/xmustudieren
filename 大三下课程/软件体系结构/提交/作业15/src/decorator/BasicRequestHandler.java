package decorator;

public class BasicRequestHandler implements HttpRequestHandler {

    @Override
    public String handleRequest(String request) {
        return "Basic Handler Processed: " + request;
    }
}
