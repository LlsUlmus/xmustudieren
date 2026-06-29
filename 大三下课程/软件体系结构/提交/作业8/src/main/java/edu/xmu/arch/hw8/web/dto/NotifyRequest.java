package edu.xmu.arch.hw8.web.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;

public record NotifyRequest(
        @NotBlank(message = "channel 不能为空")
        @Size(max = 32, message = "channel 过长")
        String channel,

        @NotBlank(message = "recipient 不能为空")
        @Size(max = 256, message = "recipient 过长")
        String recipient,

        @NotBlank(message = "content 不能为空")
        @Size(min = 1, max = 500, message = "content 长度须在 1~500")
        String content
) {
}
