package com.middleware.lab04.integration.dto;

import com.fasterxml.jackson.annotation.JsonAlias;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

import java.time.Instant;

/**
 * 通用入站事件体：后续可由 OpenClaw/Discord Webhook 以相同 JSON 投递，本模块不依赖任何 Bot SDK。
 */
public record InboundEventRequest(
        @NotBlank String source,
        @NotNull Instant timestamp,
        @JsonAlias("guild_id")
        String guildId,
        @JsonAlias("channel_id")
        String channelId,
        @JsonAlias("message_id")
        String messageId,
        @JsonAlias("author_id")
        String authorId,
        @JsonAlias("raw_text")
        @NotBlank String rawText,
        String summary,
        String intent
) {}
