package decorator;

public class AuthenticationDecorator extends RequestHandlerDecorator {

    public AuthenticationDecorator(HttpRequestHandler wrapped) {
        super(wrapped);
    }

    @Override
    public String handleRequest(String request) {
        if (!authenticate(request)) {
            return "Authentication Failed: Access Denied";
        }
        return super.handleRequest(request);
    }

    private boolean authenticate(String request) {
        return request != null && request.contains("auth_token");
    }
}
