#include<stdio.h>
#include<stdlib.h>
#include<string.h>
#include<unistd.h>
#include<sys/types.h>
#include<sys/msg.h>
#define MAXMSG 512 // 定义消息长度

// 定义消息缓冲区数据结构
struct my_msg {
    long int my_msg_type;
    char some_text[MAXMSG];
} msg;

int main() {
    int msgid;
    char buffer[BUFSIZ];

    // 创建消息队列，key为1234，权限0666
    msgid = msgget(1234, 0666 | IPC_CREAT);
    if (msgid == -1) {
        perror("msgget failed");
        exit(EXIT_FAILURE);
    }

    while (1) {
        puts("Enter some text:");
        // 读取输入内容
        if (fgets(buffer, BUFSIZ, stdin) == NULL) {
            perror("fgets failed");
            exit(EXIT_FAILURE);
        }
        // 去除换行符
        buffer[strcspn(buffer, "\n")] = '\0';
        msg.my_msg_type = 1;
        // 复制内容到消息缓冲区
        strcpy(msg.some_text, buffer);
        // 发送消息
        if (msgsnd(msgid, &msg, MAXMSG, 0) == -1) {
            perror("msgsnd failed");
            exit(EXIT_FAILURE);
        }
        // 收到"end"则退出
        if (strncmp(msg.some_text, "end", 3) == 0) {
            break;
        }
    }
    exit(EXIT_SUCCESS);
}