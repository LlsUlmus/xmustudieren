package edu.xmu.arch.hw8.channel;

import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.stereotype.Component;

@Component
@ConditionalOnProperty(name = "app.notification.email.enabled", havingValue = "true", matchIfMissing = true)
public class EmailChannel implements MessageChannel {

    @Override
    public String getChannelId() {
        return "email";
    }

    @Override
    public String send(String recipient, String content) {
        return "email-msg-" + Integer.toHexString((recipient + content).hashCode());
    }
}
