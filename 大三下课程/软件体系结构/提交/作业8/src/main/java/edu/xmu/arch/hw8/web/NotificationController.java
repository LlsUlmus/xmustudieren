package edu.xmu.arch.hw8.web;

import edu.xmu.arch.hw8.channel.MessageChannel;
import edu.xmu.arch.hw8.service.NotificationService;
import edu.xmu.arch.hw8.web.dto.NotifyRequest;
import edu.xmu.arch.hw8.web.dto.NotifyResponse;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.Comparator;
import java.util.List;
import java.util.Locale;

@RestController
@RequestMapping("/api")
public class NotificationController {

    private final NotificationService notificationService;
    private final List<MessageChannel> channels;

    public NotificationController(NotificationService notificationService, List<MessageChannel> channels) {
        this.notificationService = notificationService;
        this.channels = channels;
    }

    @PostMapping("/notify")
    public NotifyResponse notify(@Valid @RequestBody NotifyRequest request) {
        return notificationService.notify(request);
    }

    @GetMapping("/channels")
    public List<String> channels() {
        return channels.stream()
                .map(MessageChannel::getChannelId)
                .sorted(Comparator.comparing(s -> s.toLowerCase(Locale.ROOT)))
                .toList();
    }
}
