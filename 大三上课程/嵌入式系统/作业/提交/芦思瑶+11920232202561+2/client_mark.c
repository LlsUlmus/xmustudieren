#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <errno.h>
#include <string.h>
#include <arpa/inet.h>
#include <netinet/in.h>


#define N 64

int main(int argc, char *argv[]) // 期望形如: ./client <server_ip> <port>
{
    int sockfd; // 套接字文件描述符
    struct sockaddr_in servaddr; // 服务器地址结构体
    char buf[N] = {0}; // 读写缓冲区

    // 基本的命令行参数检查
    if (argc < 3)
    {
        printf("usage:%s ip port\n", argv[0]);
        return 0;
    }

    // 1. 创建 UDP 套接字；第三个参数为 0 表示使用默认协议（UDP）
    if ((sockfd = socket(AF_INET, SOCK_DGRAM, 0)) == -1)
    {
        perror("socket");
        exit(-1);
    }

    // 2. 准备服务器的 IPv4 地址与端口
    memset(&servaddr, 0, sizeof(servaddr));
    servaddr.sin_family = AF_INET;
    servaddr.sin_port = htons(atoi(argv[2])); // 主机字节序转网络字节序
    servaddr.sin_addr.s_addr = inet_addr(argv[1]); // 点分十进制转网络字节序

    // 3. 循环发送并接收数据，实现简单的回显
    while (1)
    {
        printf(">");
        if (fgets(buf, N, stdin) == NULL) // 从标准输入读取一行
            break;

        // 发送到服务器，目标地址为 servaddr
        sendto(sockfd, buf, strlen(buf)+1, 0, (struct sockaddr *) &servaddr, sizeof(servaddr));

        // 接收来自任意对端的回包，这里无需关心来源地址
        memset(buf, 0, sizeof(buf));
        recvfrom(sockfd, buf, N, 0, NULL, NULL);
        printf("%s\n", buf);
    }

    // 4. 关闭套接字
    close(sockfd);
    return 0;
}


