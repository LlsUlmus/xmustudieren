package com.middleware.lab04;

import com.middleware.lab04.config.LabProperties;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.context.properties.EnableConfigurationProperties;
import org.springframework.scheduling.annotation.EnableScheduling;

@SpringBootApplication
@EnableScheduling
@EnableConfigurationProperties(LabProperties.class)
public class MiddlewareLab04Application {

    public static void main(String[] args) {
        SpringApplication.run(MiddlewareLab04Application.class, args);
    }
}
