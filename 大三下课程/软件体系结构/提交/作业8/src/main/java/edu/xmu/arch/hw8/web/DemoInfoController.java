package edu.xmu.arch.hw8.web;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.LinkedHashMap;
import java.util.Map;

@RestController
public class DemoInfoController {

    @GetMapping("/api/concepts")
    public Map<String, Object> concepts() {
        Map<String, Object> root = new LinkedHashMap<>();
        root.put("ioc", Map.of(
                "含义", "控制反转：由 Spring 容器负责创建、装配 Bean，业务代码不自行 new 出依赖链。",
                "本示例", "启动时注册 EmailChannel、SmsChannel、NotificationServiceImpl 等。"));
        root.put("dip", Map.of(
                "含义", "依赖倒置：高层依赖抽象（MessageChannel），具体邮件/短信实现该抽象。",
                "本示例", "NotificationServiceImpl 只面向 MessageChannel。"));
        root.put("di", Map.of(
                "含义", "依赖注入：依赖由外部传入（常用构造器注入），不由类内部创建。",
                "本示例", "Controller、Service 通过构造器注入所需 Bean。"));
        return root;
    }
}
