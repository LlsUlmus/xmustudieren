package com.middleware.lab04.chat.controller;

import com.middleware.lab04.chat.dto.SentimentRequest;
import com.middleware.lab04.chat.service.MiniMaxService;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.LinkedHashMap;
import java.util.Map;

@RestController
@RequestMapping("/api")
public class ApiController {

    private final MiniMaxService miniMaxService;

    public ApiController(MiniMaxService miniMaxService) {
        this.miniMaxService = miniMaxService;
    }

    @GetMapping("/health")
    public Map<String, Object> health() {
        Map<String, Object> m = new LinkedHashMap<>();
        m.put("ok", true);
        m.put("keyConfigured", miniMaxService.isKeyConfigured());
        if (!miniMaxService.isKeyConfigured()) {
            m.put("hint", "未配置 API Key 时翻译与情感分析不可用，请设置 MINIMAX_API_KEY 或 minimax.api-key。");
        }
        return m;
    }

    @PostMapping(value = "/sentiment", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
    public Map<String, Object> sentiment(@RequestBody SentimentRequest req) {
        Map<String, Object> m = new LinkedHashMap<>();
        if (req == null || req.text() == null || req.text().isBlank()) {
            m.put("ok", false);
            m.put("error", "empty");
            return m;
        }
        m.put("ok", true);
        m.put("result", miniMaxService.sentiment(req.text()));
        return m;
    }
}
