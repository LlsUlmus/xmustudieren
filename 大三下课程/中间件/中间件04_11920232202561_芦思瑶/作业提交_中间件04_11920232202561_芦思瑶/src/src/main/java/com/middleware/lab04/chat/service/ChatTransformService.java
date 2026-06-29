package com.middleware.lab04.chat.service;

import org.springframework.stereotype.Service;

@Service
public class ChatTransformService {

    /**
     * 单独提交实验四时不依赖实验三第三方翻译能力；
     * 这里保留“处理后文本”语义，后续可替换为真实翻译/智能服务。
     */
    public String toProcessedText(String original, String targetLang) {
        String safeOriginal = original == null ? "" : original;
        String safeLang = (targetLang == null || targetLang.isBlank()) ? "英语" : targetLang;
        return "[目标语言:" + safeLang + "] " + safeOriginal;
    }
}
