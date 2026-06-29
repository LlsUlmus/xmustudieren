package edu.xmu.arch.hw8.web.dto;

public record NotifyResponse(boolean ok, String channelUsed, String messageId, String detail) {
}
