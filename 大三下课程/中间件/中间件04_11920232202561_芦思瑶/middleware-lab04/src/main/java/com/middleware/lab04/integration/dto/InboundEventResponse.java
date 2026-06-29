package com.middleware.lab04.integration.dto;

public record InboundEventResponse(boolean ok, String eventId, String message) {}
