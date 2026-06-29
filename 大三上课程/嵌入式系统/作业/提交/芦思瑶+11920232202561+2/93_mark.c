#include <stdio.h>
#include <sys/types.h>
#include <sys/wait.h>
#include <unistd.h>
#include <stdlib.h>
#include <fcntl.h>

int	global=22;
char	buf[]="the test content!\n";

int main(void)
{	
	int test=0,stat;
	pid_t pid;// 存储fork返回值（区分父子进程）
	// 1. 输出测试内容（write：系统调用，fork前执行，仅父进程执行1次）
	if(write(STDOUT_FILENO, buf, sizeof(buf)) != sizeof(buf))
	{  perror("write error!"); }
	printf(" fork test!\n");
	/* fork */
	 // 2. 创建子进程（核心：fork后父子进程内存独立，代码共享）
	pid = fork();   /*we should check the error*/
	if (pid == -1)
	{
		perror("fork");
		exit(0);	
	}
	// 3. 子进程分支（fork返回0给子进程）
	else if (pid == 0)
	{
		global++;   test++;
		printf("global=%d test=%d Child,my PID is %d\n",global,test,getpid());
		exit(0);
	}
	/*else be the parent*/
	 // 4. 父进程分支（fork返回子进程PID给父进程）
	global+=2;
	test+=2;
	printf("global=%d test=%d Parent,my PID is %d\n",global,test,getpid());
	exit(0);
	//printf("global=%d test=%d Parent,my PID is %d",global,test,getpid());
	//_exit(0);
}
	
