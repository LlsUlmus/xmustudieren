package edu.xmu.arch.hw8.web;

import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import static org.hamcrest.Matchers.containsString;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
class NotificationControllerMvcTest {

    @Autowired
    private MockMvc mockMvc;

    @Test
    void notifySuccess() throws Exception {
        mockMvc.perform(post("/api/notify")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {"channel":"email","recipient":"u@test.com","content":"你好DIP"}
                                """))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.ok").value(true))
                .andExpect(jsonPath("$.channelUsed").value("email"));
    }

    @Test
    void validationBlankContent() throws Exception {
        mockMvc.perform(post("/api/notify")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {"channel":"email","recipient":"u@test.com","content":""}
                                """))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.error").value("参数校验失败"));
    }

    @Test
    void unknownChannel() throws Exception {
        mockMvc.perform(post("/api/notify")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {"channel":"wechat","recipient":"x","content":"y"}
                                """))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.error").value("业务规则错误"))
                .andExpect(jsonPath("$.messages[0]", containsString("wechat")));
    }

    @Test
    void listChannels() throws Exception {
        mockMvc.perform(get("/api/channels"))
                .andExpect(status().isOk());
    }
}
