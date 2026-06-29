package com.middleware.lab04.chat.dto;

public class ChatMessageOut {

    private String username;
    private String original;
    private String translation;
    private String targetLang;
    private long serverTime;

    public ChatMessageOut() {}

    public ChatMessageOut(String username, String original, String translation, String targetLang, long serverTime) {
        this.username = username;
        this.original = original;
        this.translation = translation;
        this.targetLang = targetLang;
        this.serverTime = serverTime;
    }

    public String getUsername() {
        return username;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public String getOriginal() {
        return original;
    }

    public void setOriginal(String original) {
        this.original = original;
    }

    public String getTranslation() {
        return translation;
    }

    public void setTranslation(String translation) {
        this.translation = translation;
    }

    public String getTargetLang() {
        return targetLang;
    }

    public void setTargetLang(String targetLang) {
        this.targetLang = targetLang;
    }

    public long getServerTime() {
        return serverTime;
    }

    public void setServerTime(long serverTime) {
        this.serverTime = serverTime;
    }
}
