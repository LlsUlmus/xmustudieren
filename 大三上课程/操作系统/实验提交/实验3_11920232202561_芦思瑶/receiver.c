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
    long int msg_to_receive = 0;

    // 获取消息队列，key为1234，权限0666
    msgid = msgget(1234, 0666 | IPC_CREAT);
    if (msgid == -1) {
        perror("msgget failed");
        exit(EXIT_FAILURE);
    }

    while (1) {
        // 接收消息
        if (msgrcv(msgid, &msg, BUFSIZ, msg_to_receive, 0) == -1) {
            perror("msgrcv failed");
            exit(EXIT_FAILURE);
        }
        // 显示消息内容
        printf("You wrote:%s\n", msg.some_text);
        // 收到"end"则退出并删除消息队列
        if (strncmp(msg.some_text, "end", 3) == 0) {
            break;
        }
    }
    // 删除消息队列
    if (msgctl(msgid, IPC_RMID, NULL) == -1) {
        perror("msgctl failed");
        exit(EXIT_FAILURE);
    }
    exit(EXIT_SUCCESS);
}
