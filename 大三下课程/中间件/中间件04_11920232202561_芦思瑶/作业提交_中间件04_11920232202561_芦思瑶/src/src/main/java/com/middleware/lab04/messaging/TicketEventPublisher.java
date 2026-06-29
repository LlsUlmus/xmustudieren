package com.middleware.lab04.messaging;

import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

@Component
public class TicketEventPublisher {

    private final RabbitTemplate rabbitTemplate;
    private final String exchange;
    private final String routingKey;

    public TicketEventPublisher(
            RabbitTemplate rabbitTemplate,
            @Value("${messaging.chat-event.exchange}") String exchange,
            @Value("${messaging.chat-event.routing-key}") String routingKey
    ) {
        this.rabbitTemplate = rabbitTemplate;
        this.exchange = exchange;
        this.routingKey = routingKey;
    }

    public void publish(TicketEventMessage message) {
        rabbitTemplate.convertAndSend(exchange, routingKey, message);
    }
}
