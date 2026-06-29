package com.middleware.lab04.chat.service;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestClient;

import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

@Service
public class MiniMaxService {

    private final RestClient client;
    private final String model;
    private final String apiKey;
    private final ObjectMapper mapper = new ObjectMapper();

    public MiniMaxService(
            @Value("${minimax.api-key}") String apiKey,
            @Value("${minimax.base-url}") String baseUrl,
            @Value("${minimax.model}") String model
    ) {
        this.apiKey = apiKey != null ? apiKey : "";
        this.model = model;
        this.client = RestClient.builder()
                .baseUrl(baseUrl)
                .defaultHeader(HttpHeaders.AUTHORIZATION, "Bearer " + this.apiKey)
                .defaultHeader(HttpHeaders.CONTENT_TYPE, MediaType.APPLICATION_JSON_VALUE)
                .build();
    }

    public boolean isKeyConfigured() {
        return !apiKey.isBlank() && !apiKey.contains("粘贴");
    }

    private void ensureKeyConfigured() {
        if (!isKeyConfigured()) {
            throw new IllegalStateException(
                    "未配置 MiniMax API Key：请在环境变量 MINIMAX_API_KEY 或 application.yml 的 minimax.api-key 中填写有效密钥。");
        }
    }

    public String translate(String text, String targetLang) {
        String system = "你是机器翻译引擎。自动识别输入语言，"
                + "只输出一句/一段「" + targetLang + "」译文本身。"
                + "禁止解释、禁止输出思考过程、禁止加引号或「译文：」等前缀。";
        return chatCompletion(system, text.trim(), 0.15, 1024);
    }

    public String sentiment(String text) {
        String system =
                "分析用户这句话的情感倾向，并给出中文回复建议。只输出一个 JSON 对象，不要其它文字，格式："
                        + "{\"sentiment\":\"积极|中性|消极\",\"score\":0.0,\"reason\":\"\",\"suggestions\":[\"\",\"\"]}";
        return chatCompletion(system, text.trim(), 0.2, 512);
    }

    private String chatCompletion(String system, String user, double temperature, int maxTokens) {
        ensureKeyConfigured();
        Map<String, Object> body = new LinkedHashMap<>();
        body.put("model", model);
        body.put(
                "messages",
                List.of(
                        Map.of("role", "system", "content", system),
                        Map.of("role", "user", "content", user)));
        body.put("temperature", temperature);
        body.put("max_tokens", maxTokens);
        body.put("reasoning_split", true);

        String raw =
                client.post().uri("/chat/completions").body(body).retrieve().body(String.class);

        try {
            JsonNode root = mapper.readTree(raw);
            String content = root.path("choices").get(0).path("message").path("content").asText("");
            return clean(content);
        } catch (Exception e) {
            throw new IllegalStateException("MiniMax 响应解析失败: " + e.getMessage(), e);
        }
    }

    private static String clean(String raw) {
        if (raw == null || raw.isEmpty()) {
            return "";
        }
        String s = raw;
        s = s.replaceAll("(?is)<redacted_thinking>.*?</redacted_thinking>", "");
        s = s.replaceAll("(?is)<thinking>.*?</thinking>", "");
        s = s.replaceAll("\n{3,}", "\n\n");
        return s.trim();
    }
}
