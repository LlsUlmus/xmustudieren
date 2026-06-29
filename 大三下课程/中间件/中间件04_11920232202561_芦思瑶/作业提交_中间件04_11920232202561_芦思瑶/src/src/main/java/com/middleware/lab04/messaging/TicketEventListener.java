package com.middleware.lab04.messaging;

import com.middleware.lab04.ticket.Ticket;
import com.middleware.lab04.ticket.TicketRepository;
import com.middleware.lab04.ticket.TicketStatus;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

@Component
public class TicketEventListener {

    private static final Logger log = LoggerFactory.getLogger(TicketEventListener.class);

    private final TicketRepository ticketRepository;

    public TicketEventListener(TicketRepository ticketRepository) {
        this.ticketRepository = ticketRepository;
    }

    @RabbitListener(queues = "${messaging.chat-event.queue}")
    @Transactional
    public void onTicketCreated(TicketEventMessage msg) {
        Ticket t = new Ticket();
        t.setId(msg.eventId());
        t.setSource(msg.source());
        t.setRawText(msg.rawText());
        t.setGuildId(msg.guildId());
        t.setChannelId(msg.channelId());
        t.setMessageId(msg.messageId());
        t.setAuthorId(msg.authorId());
        t.setSummary(msg.summary());
        t.setIntent(msg.intent());
        t.setStatus(TicketStatus.PROCESSED);
        if (msg.occurredAt() != null) {
            t.setCreatedAt(msg.occurredAt());
        }
        ticketRepository.save(t);
        log.info("Chat event persisted from queue: {}", msg.eventId());
    }
}
