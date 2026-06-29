#include<stdio.h>
#include<stdlib.h>
#include<signal.h>
#include<unistd.h>
#include<sys/wait.h>

pid_t child1, child2; // 保存两个子进程ID

// 信号处理函数：父进程捕捉到SIGINT后终止子进程
void sig_handler(int sig) {
    if (sig == SIGINT) {
        // 向两个子进程发送终止信号
        kill(child1, SIGTERM);
        kill(child2, SIGTERM);
        printf("\nParent is killing child processes...\n");
    }
}

// 子进程1信号处理函数
void child1_handler(int sig) {
    if (sig == SIGTERM) {
        printf("Child process 1 is killed by parent !\n");
        exit(EXIT_SUCCESS);
    }
}

// 子进程2信号处理函数
void child2_handler(int sig) {
    if (sig == SIGTERM) {
        printf("Child process 2 is killed by parent !\n");
        exit(EXIT_SUCCESS);
    }
}

int main() {
    // 父进程注册SIGINT信号处理函数
    if (signal(SIGINT, sig_handler) == SIG_ERR) {
        perror("signal failed");
        exit(EXIT_FAILURE);
    }

    // 创建第一个子进程
    child1 = fork();
    if (child1 == -1) {
        perror("fork child1 failed");
        exit(EXIT_FAILURE);
    }
    else if (child1 == 0) {
        // 子进程1注册SIGTERM信号处理函数
        if (signal(SIGTERM, child1_handler) == SIG_ERR) {
            perror("signal child1 failed");
            exit(EXIT_FAILURE);
        }
        // 子进程1循环等待信号
        while (1) {
            sleep(1);
        }
    }

    // 创建第二个子进程
    child2 = fork();
    if (child2 == -1) {
        perror("fork child2 failed");
        exit(EXIT_FAILURE);
    }
    else if (child2 == 0) {
        // 子进程2注册SIGTERM信号处理函数
        if (signal(SIGTERM, child2_handler) == SIG_ERR) {
            perror("signal child2 failed");
            exit(EXIT_FAILURE);
        }
        // 子进程2循环等待信号
        while (1) {
            sleep(1);
        }
    }

    // 父进程等待两个子进程终止
    waitpid(child1, NULL, 0);
    waitpid(child2, NULL, 0);

    // 父进程输出信息后终止
    printf("Parent process is killed!\n");
    return 0;
}