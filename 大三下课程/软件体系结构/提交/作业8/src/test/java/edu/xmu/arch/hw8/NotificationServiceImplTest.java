package edu.xmu.arch.hw8;

import edu.xmu.arch.hw8.channel.EmailChannel;
import edu.xmu.arch.hw8.channel.MessageChannel;
import edu.xmu.arch.hw8.channel.SmsChannel;
import edu.xmu.arch.hw8.service.NotificationServiceImpl;
import edu.xmu.arch.hw8.web.dto.NotifyRequest;
import org.junit.jupiter.api.Test;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class NotificationServiceImplTest {

    @Test
    void sendsViaEmailWhenChannelIsEmail() {
        List<MessageChannel> channels = List.of(new EmailChannel(), new SmsChannel());
        var svc = new NotificationServiceImpl(channels);
        var res = svc.notify(new NotifyRequest("email", "a@b.com", "hello"));
        assertThat(res.ok()).isTrue();
        assertThat(res.channelUsed()).isEqualTo("email");
        assertThat(res.messageId()).startsWith("email-msg-");
    }

    @Test
    void rejectsUnknownChannel() {
        List<MessageChannel> channels = List.of(new EmailChannel());
        var svc = new NotificationServiceImpl(channels);
        assertThatThrownBy(() -> svc.notify(new NotifyRequest("sms", "13800138000", "x")))
                .isInstanceOf(IllegalArgumentException.class)
                .hasMessageContaining("未知通道");
    }
}
