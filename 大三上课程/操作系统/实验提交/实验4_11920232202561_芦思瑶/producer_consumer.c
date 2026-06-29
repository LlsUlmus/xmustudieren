#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/sem.h>
#include <sys/wait.h>
#include <sys/shm.h>  // 新增共享内存头文件

// 信号量操作共用体
union semun {
    int val;
    struct semid_ds* buf;
    unsigned short* array;
};

#define BUFFER_SIZE 5  // 缓冲区大小
#define SEM_KEY 0x123456  // 信号量键值
#define SHM_KEY 0x654321  // 共享内存键值

// 共享内存结构：缓冲区 + 读写索引
struct SharedMem {
    int buffer[BUFFER_SIZE];
    int in;   // 生产者写入索引
    int out;  // 消费者读取索引
};

// 信号量ID
int sem_id;
// 共享内存ID
int shm_id;
// 共享内存指针
struct SharedMem* shared_mem;

// 信号量P操作（申请资源）
void sem_p(int sem_num) {
    struct sembuf sem_op;
    sem_op.sem_num = sem_num;
    sem_op.sem_op = -1;
    sem_op.sem_flg = SEM_UNDO;
    if (semop(sem_id, &sem_op, 1) == -1) {
        perror("semop P failed");
        exit(EXIT_FAILURE);
    }
}

// 信号量V操作（释放资源）
void sem_v(int sem_num) {
    struct sembuf sem_op;
    sem_op.sem_num = sem_num;
    sem_op.sem_op = 1;
    sem_op.sem_flg = SEM_UNDO;
    if (semop(sem_id, &sem_op, 1) == -1) {
        perror("semop V failed");
        exit(EXIT_FAILURE);
    }
}

// 初始化信号量和共享内存
void init() {
    // 创建信号量集（3个信号量：empty, full, mutex）
    sem_id = semget(SEM_KEY, 3, IPC_CREAT | 0666);
    if (sem_id == -1) {
        perror("semget failed");
        exit(EXIT_FAILURE);
    }

    union semun sem_arg;
    // 初始化empty（空闲缓冲区数量=BUFFER_SIZE）
    sem_arg.val = BUFFER_SIZE;
    if (semctl(sem_id, 0, SETVAL, sem_arg) == -1) {
        perror("semctl init empty failed");
        exit(EXIT_FAILURE);
    }
    // 初始化full（已用缓冲区数量=0）
    sem_arg.val = 0;
    if (semctl(sem_id, 1, SETVAL, sem_arg) == -1) {
        perror("semctl init full failed");
        exit(EXIT_FAILURE);
    }
    // 初始化mutex（互斥信号量=1）
    sem_arg.val = 1;
    if (semctl(sem_id, 2, SETVAL, sem_arg) == -1) {
        perror("semctl init mutex failed");
        exit(EXIT_FAILURE);
    }

    // 创建共享内存
    shm_id = shmget(SHM_KEY, sizeof(struct SharedMem), IPC_CREAT | 0666);
    if (shm_id == -1) {
        perror("shmget failed");
        exit(EXIT_FAILURE);
    }

    // 附加共享内存到进程地址空间
    shared_mem = (struct SharedMem*)shmat(shm_id, NULL, 0);
    if (shared_mem == (void*)-1) {
        perror("shmat failed");
        exit(EXIT_FAILURE);
    }

    // 初始化读写索引
    shared_mem->in = 0;
    shared_mem->out = 0;
}

// 生产者逻辑
void producer() {
    int item = 1;
    while (1) {
        // 生产数据（模拟）
        item = (item % 100) + 1;

        // P(empty)：申请空闲缓冲区
        sem_p(0);
        // P(mutex)：申请互斥锁
        sem_p(2);

        // 写入缓冲区
        shared_mem->buffer[shared_mem->in] = item;
        printf("生产者生产：%d，位置：%d\n", item, shared_mem->in);
        shared_mem->in = (shared_mem->in + 1) % BUFFER_SIZE;

        // V(mutex)：释放互斥锁
        sem_v(2);
        // V(full)：释放已用缓冲区计数
        sem_v(1);

        sleep(1);  // 模拟生产耗时
    }
}

// 消费者逻辑
void consumer() {
    int item;
    while (1) {
        // P(full)：申请已用缓冲区
        sem_p(1);
        // P(mutex)：申请互斥锁
        sem_p(2);

        // 读取缓冲区
        item = shared_mem->buffer[shared_mem->out];
        printf("消费者消费：%d，位置：%d\n", item, shared_mem->out);
        shared_mem->out = (shared_mem->out + 1) % BUFFER_SIZE;

        // V(mutex)：释放互斥锁
        sem_v(2);
        // V(empty)：释放空闲缓冲区计数
        sem_v(0);

        sleep(2);  // 模拟消费耗时
    }
}

// 清理资源
void clean() {
    // 分离共享内存
    if (shmdt(shared_mem) == -1) {
        perror("shmdt failed");
    }
    // 删除共享内存
    if (shmctl(shm_id, IPC_RMID, NULL) == -1) {
        perror("shmctl IPC_RMID failed");
    }
    // 删除信号量集
    if (semctl(sem_id, 0, IPC_RMID) == -1) {
        perror("semctl IPC_RMID failed");
    }
}

int main() {
    pid_t pid;

    init();

    // 创建子进程
    pid = fork();
    if (pid == -1) {
        perror("fork failed");
        clean();
        exit(EXIT_FAILURE);
    }
    else if (pid == 0) {
        // 子进程：消费者
        consumer();
    }
    else {
        // 父进程：生产者
        producer();
        wait(NULL);  // 等待子进程（实际不会执行，因生产者循环）
    }

    clean();
    return 0;
}