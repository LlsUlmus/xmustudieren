#include <stdio.h>
#include <unistd.h>
#include <sys/types.h>

int main() {
    pid_t pid;

    printf("_before fork\n");

    pid = fork();

    if (pid < 0) {
        // 错误处理
        perror("fork failed");
        return 1;
    }
    else if (pid == 0) {
        // 子进程执行区域
        printf("这是子进程，PID: %d，父进程PID: %d\n", getpid(), getppid());
    }
    else {
        // 父进程执行区域
        printf("这是父进程，PID: %d，子进程PID: %d\n", getpid(), pid);
    }

    printf("after fork\n");
    return 0;
}