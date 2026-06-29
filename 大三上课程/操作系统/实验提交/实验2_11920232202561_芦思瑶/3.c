#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <signal.h>
#include <sys/wait.h>

int main() {
    pid_t pid1, pid2;  
    int status;        

    // 创建第一个子进程
    pid1 = fork();
    if (pid1 < 0) {
        // 创建失败处理
        perror("创建第一个子进程失败");
        exit(EXIT_FAILURE);
    }
    else if (pid1 == 0) {
        // 子进程1的代码
        while (1) {  // 无限循环，直到被父进程杀死
            printf("子进程1正在执行.....(PID: %d)\n", getpid());
            sleep(1);  
        }
        exit(EXIT_SUCCESS);
    }

    // 创建第二个子进程
    pid2 = fork();
    if (pid2 < 0) {
        perror("创建第二个子进程失败");
        kill(pid1, SIGTERM);
        exit(EXIT_FAILURE);
    }
    else if (pid2 == 0) {
        // 子进程2的代码
        while (1) { 
            printf("子进程2正在执行.....(PID: %d)\n", getpid());
            sleep(1); 
        }
        exit(EXIT_SUCCESS);
    }

    // 父进程的代码
    printf("父进程正在执行.....(PID: %d)\n", getpid());

    sleep(5);

    // 杀死子进程1
    kill(pid1, SIGTERM);
    waitpid(pid1, &status, 0);
    printf("子进程1被父进程杀死\n");

    // 杀死子进程2
    kill(pid2, SIGTERM);
    waitpid(pid2, &status, 0);
    printf("子进程2被父进程杀死\n");

    // 父进程结束
    printf("父进程结束\n");
    exit(EXIT_SUCCESS);
}
