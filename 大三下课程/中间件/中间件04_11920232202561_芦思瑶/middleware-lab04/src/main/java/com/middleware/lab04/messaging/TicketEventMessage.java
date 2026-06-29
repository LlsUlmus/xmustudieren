package com.middleware.lab04.messaging;

import java.io.Serializable;
import java.time.Instant;

/**
 * 经 RabbitMQ 传递的业务载荷；与外部通道（后续 OpenClaw/Discord）解耦。
 */
public record TicketEventMessage(
        String eventId,
        String source,
        Instant occurredAt,
        String guildId,
        String channelId,
        String messageId,
        String authorId,
        String rawText,
        String summary,
        String intent
) implements Serializable {}
