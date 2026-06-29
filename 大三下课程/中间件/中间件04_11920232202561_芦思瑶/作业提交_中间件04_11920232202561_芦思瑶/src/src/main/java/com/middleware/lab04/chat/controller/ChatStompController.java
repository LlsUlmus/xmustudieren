package com.middleware.lab04.chat.controller;

import com.middleware.lab04.chat.dto.ChatMessageIn;
import com.middleware.lab04.chat.dto.ChatMessageOut;
import com.middleware.lab04.chat.service.MiniMaxService;
import com.middleware.lab04.messaging.TicketEventMessage;
import com.middleware.lab04.messaging.TicketEventPublisher;
import org.springframework.messaging.handler.annotation.MessageMapping;
import org.springframework.messaging.handler.annotation.SendTo;
import org.springframework.stereotype.Controller;

import java.time.Instant;
import java.util.UUID;

@Controller
public class ChatStompController {

    private final MiniMaxService miniMaxService;
    private final TicketEventPublisher eventPublisher;

    public ChatStompController(MiniMaxService miniMaxService, TicketEventPublisher eventPublisher) {
        this.miniMaxService = miniMaxService;
        this.eventPublisher = eventPublisher;
    }

    @MessageMapping("/chat.send")
    @SendTo("/topic/messages")
    public ChatMessageOut broadcast(ChatMessageIn in) {
        if (in.getUsername() == null || in.getUsername().isBlank()) {
            in.setUsername("匿名");
        }
        String original = in.getText() == null ? "" : in.getText().trim();
        String translated;
        try {
            translated = miniMaxService.translate(original, in.getTargetLang());
        } catch (Exception e) {
            String hint = e.getMessage() != null ? e.getMessage() : e.getClass().getSimpleName();
            translated = "【翻译失败】" + hint;
        }
        long now = System.currentTimeMillis();

        String eventId = "E4-" + UUID.randomUUID().toString().replace("-", "").substring(0, 12).toUpperCase();
        eventPublisher.publish(new TicketEventMessage(
                eventId,
                "exp4-stomp",
                Instant.ofEpochMilli(now),
                null,
                null,
                null,
                in.getUsername(),
                original,
                translated,
                "chat_message"
        ));

        return new ChatMessageOut(
                in.getUsername(),
                original,
                translated,
                in.getTargetLang(),
                now
        );
    }
}
