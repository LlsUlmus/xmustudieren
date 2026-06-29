package com.middleware.lab04.config;

import org.springframework.boot.context.properties.ConfigurationProperties;

@ConfigurationProperties(prefix = "lab")
public record LabProperties(Webhook webhook, Scheduled scheduled, Exp3Bridge exp3Bridge) {

    public record Webhook(String secret, boolean authDisabled) {}

    public record Scheduled(String cleanupCron, int retentionDays) {}

    public record Exp3Bridge(boolean enabled, String wsUrl, String topic, String sourceName) {}
}
