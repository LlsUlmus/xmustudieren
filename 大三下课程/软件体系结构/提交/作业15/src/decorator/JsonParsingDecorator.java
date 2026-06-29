package decorator;

import java.util.Collections;
import java.util.HashMap;
import java.util.Map;

/**
 * 半透明装饰器示例：除 handleRequest 外，还提供 getJsonField 等装饰器特有方法。
 */
public class JsonParsingDecorator extends RequestHandlerDecorator {

    private final Map<String, String> parsedData = new HashMap<>();

    public JsonParsingDecorator(HttpRequestHandler wrapped) {
        super(wrapped);
    }

    @Override
    public String handleRequest(String request) {
        extractKeyValuePairs(request);
        String enriched = "JSON Processed: " + formatEntries();
        return super.handleRequest(enriched);
    }

    /** 装饰器特有接口，客户端需向下转型或持有具体类型才能调用。 */
    public String getJsonField(String field) {
        return parsedData.getOrDefault(field, "Field not found");
    }

    public Map<String, String> getParsedData() {
        return Collections.unmodifiableMap(parsedData);
    }

    private void extractKeyValuePairs(String raw) {
        parsedData.clear();
        if (raw == null || raw.isBlank()) {
            return;
        }
        String body = extractJsonBody(raw);
        if (body == null) {
            return;
        }
        if (body.startsWith("{")) {
            body = body.substring(1);
        }
        if (body.endsWith("}")) {
            body = body.substring(0, body.length() - 1);
        }
        if (body.isEmpty()) {
            return;
        }
        int start = 0;
        while (start < body.length()) {
            int comma = body.indexOf(',', start);
            String segment = comma < 0 ? body.substring(start) : body.substring(start, comma);
            putSegment(segment.trim());
            if (comma < 0) {
                break;
            }
            start = comma + 1;
        }
    }

    private void putSegment(String segment) {
        int colon = segment.indexOf(':');
        if (colon <= 0) {
            return;
        }
        String key = stripQuotes(segment.substring(0, colon).trim());
        String value = stripQuotes(segment.substring(colon + 1).trim());
        parsedData.put(key, value);
    }

    /** 从混合请求中提取 {...} 片段，便于与认证前缀共存。 */
    private String extractJsonBody(String raw) {
        int left = raw.indexOf('{');
        int right = raw.lastIndexOf('}');
        if (left >= 0 && right > left) {
            return raw.substring(left, right + 1);
        }
        return raw.trim();
    }

    private String stripQuotes(String text) {
        if (text.length() >= 2 && text.startsWith("\"") && text.endsWith("\"")) {
            return text.substring(1, text.length() - 1);
        }
        return text;
    }

    private String formatEntries() {
        StringBuilder sb = new StringBuilder("{");
        boolean first = true;
        for (Map.Entry<String, String> entry : parsedData.entrySet()) {
            if (!first) {
                sb.append(", ");
            }
            sb.append(entry.getKey()).append('=').append(entry.getValue());
            first = false;
        }
        sb.append('}');
        return sb.toString();
    }
}
