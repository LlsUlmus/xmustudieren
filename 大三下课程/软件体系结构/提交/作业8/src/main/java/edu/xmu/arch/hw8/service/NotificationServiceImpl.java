package edu.xmu.arch.hw8.service;

import edu.xmu.arch.hw8.channel.MessageChannel;
import edu.xmu.arch.hw8.web.dto.NotifyRequest;
import edu.xmu.arch.hw8.web.dto.NotifyResponse;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Optional;
import java.util.function.Function;
import java.util.stream.Collectors;

@Service
public class NotificationServiceImpl implements NotificationService {

    private final Map<String, MessageChannel> channelsById;

    public NotificationServiceImpl(List<MessageChannel> channels) {
        this.channelsById = channels.stream()
                .collect(Collectors.toMap(
                        c -> c.getChannelId().toLowerCase(Locale.ROOT),
                        Function.identity(),
                        (a, b) -> a));
    }

    @Override
    public NotifyResponse notify(NotifyRequest request) {
        String key = request.channel().trim().toLowerCase(Locale.ROOT);
        MessageChannel channel = Optional.ofNullable(channelsById.get(key))
                .orElseThrow(() -> new IllegalArgumentException(
                        "未知通道: " + request.channel() + "。可用: " + channelsById.keySet()));

        String messageId = channel.send(request.recipient().trim(), request.content().trim());
        return new NotifyResponse(true, channel.getChannelId(), messageId, "已投递（模拟）");
    }
}
