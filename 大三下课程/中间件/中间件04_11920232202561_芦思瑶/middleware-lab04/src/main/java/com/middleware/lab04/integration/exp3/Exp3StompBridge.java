package com.middleware.lab04.integration.exp3;

import com.middleware.lab04.config.LabProperties;
import com.middleware.lab04.messaging.TicketEventMessage;
import com.middleware.lab04.messaging.TicketEventPublisher;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.context.event.ApplicationReadyEvent;
import org.springframework.context.event.EventListener;
import org.springframework.lang.NonNull;
import org.springframework.messaging.converter.MappingJackson2MessageConverter;
import org.springframework.messaging.simp.stomp.StompCommand;
import org.springframework.messaging.simp.stomp.StompFrameHandler;
import org.springframework.messaging.simp.stomp.StompHeaders;
import org.springframework.messaging.simp.stomp.StompSession;
import org.springframework.messaging.simp.stomp.StompSessionHandlerAdapter;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.client.standard.StandardWebSocketClient;
import org.springframework.web.socket.messaging.WebSocketStompClient;

import java.lang.reflect.Type;
import java.time.Instant;
import java.util.UUID;
import java.util.concurrent.atomic.AtomicBoolean;

@Component
public class Exp3StompBridge {

    private static final Logger log = LoggerFactory.getLogger(Exp3StompBridge.class);

    private final LabProperties properties;
    private final TicketEventPublisher publisher;
    private final WebSocketStompClient stompClient;
    private final AtomicBoolean connected = new AtomicBoolean(false);

    public Exp3StompBridge(LabProperties properties, TicketEventPublisher publisher) {
        this.properties = properties;
        this.publisher = publisher;
        this.stompClient = new WebSocketStompClient(new StandardWebSocketClient());
        this.stompClient.setMessageConverter(new MappingJackson2MessageConverter());
    }

    @EventListener(ApplicationReadyEvent.class)
    public void onReady() {
        tryConnect();
    }

    @Scheduled(fixedDelayString = "10000")
    public void reconnect() {
        if (properties.exp3Bridge().enabled() && !connected.get()) {
            tryConnect();
        }
    }

    private void tryConnect() {
        if (!properties.exp3Bridge().enabled()) {
            return;
        }
        String wsUrl = properties.exp3Bridge().wsUrl();
        String topic = properties.exp3Bridge().topic();
        try {
            stompClient.connectAsync(wsUrl, new StompSessionHandlerAdapter() {
                @Override
                public void afterConnected(@NonNull StompSession session, @NonNull StompHeaders connectedHeaders) {
                    connected.set(true);
                    log.info("Connected to exp3 websocket: {}", wsUrl);
                    session.subscribe(topic, new StompFrameHandler() {
                        @Override
                        public Type getPayloadType(@NonNull StompHeaders headers) {
                            return Exp3ChatMessageOut.class;
                        }

                        @Override
                        public void handleFrame(@NonNull StompHeaders headers, Object payload) {
                            if (payload instanceof Exp3ChatMessageOut out) {
                                publishChatEvent(out);
                            }
                        }
                    });
                }

                @Override
                public void handleTransportError(@NonNull StompSession session, @NonNull Throwable exception) {
                    connected.set(false);
                    log.warn("Exp3 bridge transport error: {}", exception.getMessage());
                }

                @Override
                public void handleException(
                        @NonNull StompSession session,
                        StompCommand command,
                        @NonNull StompHeaders headers,
                        byte[] payload,
                        @NonNull Throwable exception
                ) {
                    connected.set(false);
                    log.warn("Exp3 bridge frame exception: {}", exception.getMessage());
                }
            });
        } catch (Exception ex) {
            connected.set(false);
            log.warn("Exp3 bridge connect failed: {}", ex.getMessage());
        }
    }

    private void publishChatEvent(Exp3ChatMessageOut out) {
        String eventId = "E3-" + UUID.randomUUID().toString().replace("-", "").substring(0, 12).toUpperCase();
        String raw = out.getOriginal() == null ? "" : out.getOriginal();
        String summary = out.getTranslation() == null ? raw : out.getTranslation();
        Instant ts = out.getServerTime() > 0 ? Instant.ofEpochMilli(out.getServerTime()) : Instant.now();
        TicketEventMessage message = new TicketEventMessage(
                eventId,
                properties.exp3Bridge().sourceName(),
                ts,
                null,
                null,
                null,
                out.getUsername(),
                raw,
                summary,
                "chat_message"
        );
        publisher.publish(message);
    }
}
