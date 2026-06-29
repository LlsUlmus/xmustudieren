package decorator;

/**
 * 抽象装饰器：持有被包装对象，并将请求委托给它。
 */
public abstract class RequestHandlerDecorator implements HttpRequestHandler {

    protected final HttpRequestHandler wrapped;

    public RequestHandlerDecorator(HttpRequestHandler wrapped) {
        this.wrapped = wrapped;
    }

    @Override
    public String handleRequest(String request) {
        return wrapped.handleRequest(request);
    }
}
