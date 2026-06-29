package edu.xmu.arch.hw8.channel;

public interface MessageChannel {

    String getChannelId();

    String send(String recipient, String content);
}
