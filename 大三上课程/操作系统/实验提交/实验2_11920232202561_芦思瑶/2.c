#include <stdio.h>
#include <unistd.h>
#include <sys/types.h>

int main() {
    pid_t pid;
    int shared_var = 10; 

    pid = fork();

    if (pid < 0) {
        perror("fork failed");
        return 1;
    }
    else if (pid == 0) {
        // 子进程
        printf("子进程：修改前 shared_var = %d\n", shared_var);
        shared_var = 20;  
        printf("子进程：修改后 shared_var = %d (地址：%p)\n", shared_var, &shared_var);
        sleep(2);  
        printf("子进程：最终 shared_var = %d\n", shared_var);
    }
    else {
        // 父进程
        printf("父进程：修改前 shared_var = %d\n", shared_var);
        shared_var = 30;  
        printf("父进程：修改后 shared_var = %d (地址：%p)\n", shared_var, &shared_var);
        sleep(3);  
        printf("父进程：最终 shared_var = %d\n", shared_var);
    }

    return 0;
}
