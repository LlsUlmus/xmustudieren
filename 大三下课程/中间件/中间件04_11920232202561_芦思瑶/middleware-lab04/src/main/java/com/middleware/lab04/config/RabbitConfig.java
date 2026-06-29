package com.middleware.lab04.config;

import org.springframework.amqp.core.Binding;
import org.springframework.amqp.core.BindingBuilder;
import org.springframework.amqp.core.Queue;
import org.springframework.amqp.core.TopicExchange;
import org.springframework.amqp.support.converter.Jackson2JsonMessageConverter;
import org.springframework.amqp.support.converter.MessageConverter;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class RabbitConfig {

    @Bean
    public MessageConverter jsonMessageConverter() {
        return new Jackson2JsonMessageConverter();
    }

    @Bean
    public Queue chatEventQueue(@Value("${messaging.chat-event.queue}") String name) {
        return new Queue(name, true);
    }

    @Bean
    public TopicExchange chatEventExchange(@Value("${messaging.chat-event.exchange}") String name) {
        return new TopicExchange(name);
    }

    @Bean
    public Binding chatEventBinding(
            Queue chatEventQueue,
            TopicExchange chatEventExchange,
            @Value("${messaging.chat-event.routing-key}") String routingKey
    ) {
        return BindingBuilder.bind(chatEventQueue).to(chatEventExchange).with(routingKey);
    }
}
