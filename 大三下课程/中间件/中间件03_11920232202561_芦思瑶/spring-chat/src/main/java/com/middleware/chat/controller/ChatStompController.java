package com.middleware.chat.controller;

import com.middleware.chat.dto.ChatMessageIn;
import com.middleware.chat.dto.ChatMessageOut;
import com.middleware.chat.service.MiniMaxService;
import org.springframework.messaging.handler.annotation.MessageMapping;
import org.springframework.messaging.handler.annotation.SendTo;
import org.springframework.stereotype.Controller;

@Controller
public class ChatStompController {

    private final MiniMaxService miniMaxService;

    public ChatStompController(MiniMaxService miniMaxService) {
        this.miniMaxService = miniMaxService;
    }

    @MessageMapping("/chat.send")
    @SendTo("/topic/messages")
    public ChatMessageOut broadcast(ChatMessageIn in) {
        if (in.getText() == null || in.getText().isBlank()) {
            return new ChatMessageOut(
                    in.getUsername(),
                    "",
                    "",
                    in.getTargetLang(),
                    System.currentTimeMillis()
            );
        }
        if (in.getUsername() == null || in.getUsername().isBlank()) {
            in.setUsername("—");
        }
        String original = in.getText().trim();
        try {
            String translated = miniMaxService.translate(original, in.getTargetLang());
            return new ChatMessageOut(
                    in.getUsername(),
                    original,
                    translated,
                    in.getTargetLang(),
                    System.currentTimeMillis()
            );
        } catch (Exception e) {
            String hint = e.getMessage() != null ? e.getMessage() : e.getClass().getSimpleName();
            return new ChatMessageOut(
                    in.getUsername(),
                    original,
                    "【翻译失败】" + hint,
                    in.getTargetLang(),
                    System.currentTimeMillis()
            );
        }
    }
}
