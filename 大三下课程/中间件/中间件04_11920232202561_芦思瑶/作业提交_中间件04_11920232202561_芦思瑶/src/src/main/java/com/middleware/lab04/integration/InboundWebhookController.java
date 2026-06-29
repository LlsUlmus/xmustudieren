package com.middleware.lab04.integration;

import com.middleware.lab04.integration.dto.InboundEventRequest;
import com.middleware.lab04.integration.dto.InboundEventResponse;
import com.middleware.lab04.messaging.TicketEventMessage;
import com.middleware.lab04.messaging.TicketEventPublisher;
import jakarta.validation.Valid;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestController;

import java.util.UUID;

/**
 * 外部系统入口：仅暴露 HTTP。与「智能化 / OpenClaw」相关的改造应放在对方系统或本控制器的 DTO 映射层，不侵入领域与消息层。
 */
@RestController
@RequestMapping("/api/inbound")
public class InboundWebhookController {

    private final TicketEventPublisher ticketEventPublisher;

    public InboundWebhookController(TicketEventPublisher ticketEventPublisher) {
        this.ticketEventPublisher = ticketEventPublisher;
    }

    @PostMapping("/events")
    @ResponseStatus(HttpStatus.ACCEPTED)
    public InboundEventResponse accept(@Valid @RequestBody InboundEventRequest body) {
        String eventId = "E-" + UUID.randomUUID().toString().replace("-", "").substring(0, 12).toUpperCase();
        TicketEventMessage msg = new TicketEventMessage(
                eventId,
                body.source(),
                body.timestamp(),
                body.guildId(),
                body.channelId(),
                body.messageId(),
                body.authorId(),
                body.rawText(),
                body.summary() != null ? body.summary() : body.rawText(),
                body.intent()
        );
        ticketEventPublisher.publish(msg);
        return new InboundEventResponse(true, eventId, "queued");
    }
}
