package edu.xmu.arch.hw8.channel;

import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.stereotype.Component;

@Component
@ConditionalOnProperty(name = "app.notification.sms.enabled", havingValue = "true", matchIfMissing = true)
public class SmsChannel implements MessageChannel {

    @Override
    public String getChannelId() {
        return "sms";
    }

    @Override
    public String send(String recipient, String content) {
        return "sms-msg-" + Integer.toHexString((recipient + content).hashCode());
    }
}
