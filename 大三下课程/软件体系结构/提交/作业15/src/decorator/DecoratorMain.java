package decorator;

public class DecoratorMain {

    public static void main(String[] args) {
        HttpRequestHandler handler = new BasicRequestHandler();
        handler = new LoggingDecorator(handler);
        JsonParsingDecorator jsonHandler = new JsonParsingDecorator(handler);
        AuthenticationDecorator authHandler = new AuthenticationDecorator(jsonHandler);

        String jsonBody = "{\"user\":\"alice\",\"action\":\"login\"}";
        String withToken = "auth_token=xyz " + jsonBody;  // 认证前缀 + JSON 正文
        String withoutToken = jsonBody;

        System.out.println("=== 带认证令牌 ===");
        String ok = authHandler.handleRequest(withToken);
        System.out.println("Response: " + ok);
        System.out.println("Parsed user field: " + jsonHandler.getJsonField("user"));

        System.out.println("\n=== 无认证令牌 ===");
        String denied = authHandler.handleRequest(withoutToken);
        System.out.println("Response: " + denied);
    }
}
