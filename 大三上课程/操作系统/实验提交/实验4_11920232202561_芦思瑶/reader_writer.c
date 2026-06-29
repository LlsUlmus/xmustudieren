/*
 * 读者-写者问题 - 详细注释版本
 * 使用信号量实现读者优先的同步访问
 */

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/sem.h>
#include <sys/wait.h>
#include <sys/shm.h>  // 新增头文件包含

 // 信号量操作函数
 // P操作：等待信号量（减1操作）
void P(int semid, int sem_num) {
    struct sembuf op = { sem_num, -1, SEM_UNDO };  // SEM_UNDO确保进程退出时自动释放
    if (semop(semid, &op, 1) == -1) {  // 增加错误检查
        perror("semop P error");
        exit(EXIT_FAILURE);
    }
}

// V操作：释放信号量（加1操作）
void V(int semid, int sem_num) {
    struct sembuf op = { sem_num, 1, SEM_UNDO };
    if (semop(semid, &op, 1) == -1) {  // 增加错误检查
        perror("semop V error");
        exit(EXIT_FAILURE);
    }
}

int main() {
    int semid, shmid;        // 信号量ID和共享内存ID
    key_t key;               // 系统键值
    pid_t pid;               // 进程ID
    int* data, * readcount;   // 共享数据和读者计数

    // ========== 创建共享内存 ==========
    key = ftok(".", 'r');    // 生成键值
    if (key == -1) {  // 增加错误检查
        perror("ftok error for shm");
        exit(EXIT_FAILURE);
    }
    // 创建共享内存：数据 + 读者计数
    shmid = shmget(key, sizeof(int) * 2, IPC_CREAT | 0666);
    if (shmid == -1) {  // 增加错误检查
        perror("shmget error");
        exit(EXIT_FAILURE);
    }
    data = (int*)shmat(shmid, NULL, 0);      // 附加到进程地址空间
    if (data == (int*)-1) {  // 增加错误检查
        perror("shmat error");
        exit(EXIT_FAILURE);
    }
    readcount = data + 1;                     // 读者计数指针
    *data = 0;                               // 初始化共享数据
    *readcount = 0;                          // 初始化读者计数

    // ========== 创建信号量集 ==========
    key = ftok(".", 'w');
    if (key == -1) {  // 增加错误检查
        perror("ftok error for sem");
        exit(EXIT_FAILURE);
    }
    semid = semget(key, 2, IPC_CREAT | 0666);  // 创建2个信号量
    if (semid == -1) {  // 增加错误检查
        perror("semget error");
        exit(EXIT_FAILURE);
    }
    if (semctl(semid, 0, SETVAL, 1) == -1) {   // mutex=1 (保护readcount的互斥信号量)
        perror("semctl set mutex error");
        exit(EXIT_FAILURE);
    }
    if (semctl(semid, 1, SETVAL, 1) == -1) {   // wrt=1 (写者互斥信号量)
        perror("semctl set wrt error");
        exit(EXIT_FAILURE);
    }

    printf("=== 读者-写者问题演示（读者优先） ===\n");
    printf("信号量: mutex=%d, wrt=%d\n", 1, 1);
    printf("共享数据初始值: %d\n\n", *data);

    // ========== 创建读者进程 ==========
    for (int i = 0; i < 3; i++) {
        pid = fork();
        if (pid == -1) {  // 增加错误检查
            perror("fork error for reader");
            exit(EXIT_FAILURE);
        }
        if (pid == 0) {  // 子进程
            for (int j = 0; j < 3; j++) {  // 每个读者读3次
                // 读者算法（读者优先）：
                // 1. 等待互斥访问readcount (P(mutex))
                // 2. 增加读者计数
                // 3. 如果是第一个读者，等待写者完成 (P(wrt))
                // 4. 释放互斥访问readcount (V(mutex))
                // 5. 执行读操作
                // 6. 等待互斥访问readcount (P(mutex))
                // 7. 减少读者计数
                // 8. 如果是最后一个读者，释放写者信号量 (V(wrt))
                // 9. 释放互斥访问readcount (V(mutex))

                P(semid, 0);  // 等待互斥访问readcount
                (*readcount)++;
                if (*readcount == 1) {
                    P(semid, 1);  // 第一个读者，等待写者完成
                    printf("第一个读者%d进入，阻止写者\n", i);
                }
                V(semid, 0);  // 释放互斥访问readcount

                // 执行读操作
                printf("读者%d读取: %d (当前读者数: %d)\n", i, *data, *readcount);
                sleep(1);  // 模拟读操作时间

                P(semid, 0);  // 等待互斥访问readcount
                (*readcount)--;
                if (*readcount == 0) {
                    V(semid, 1);  // 最后一个读者，释放写者信号量
                    printf("最后一个读者%d离开，允许写者\n", i);
                }
                V(semid, 0);  // 释放互斥访问readcount
                sleep(1);     // 模拟读操作间隔
            }
            exit(0);  // 子进程结束
        }
    }

    // ========== 创建写者进程 ==========
    for (int i = 0; i < 2; i++) {
        pid = fork();
        if (pid == -1) {  // 增加错误检查
            perror("fork error for writer");
            exit(EXIT_FAILURE);
        }
        if (pid == 0) {  // 子进程
            for (int j = 0; j < 2; j++) {  // 每个写者写2次
                // 写者算法：
                // 1. 等待写者互斥访问 (P(wrt))
                // 2. 执行写操作
                // 3. 释放写者互斥访问 (V(wrt))

                P(semid, 1);  // 等待写者互斥访问

                // 执行写操作
                *data = i * 10 + j;
                printf("写者%d写入: %d\n", i, *data);
                sleep(1);  // 模拟写操作时间

                V(semid, 1);  // 释放写者互斥访问
                sleep(1);     // 模拟写操作间隔
            }
            exit(0);  // 子进程结束
        }
    }

    // ========== 等待所有子进程结束 ==========
    for (int i = 0; i < 5; i++) {
        if (wait(NULL) == -1) {  // 增加错误检查
            perror("wait error");
            exit(EXIT_FAILURE);
        }
    }

    printf("\n=== 所有进程执行完毕 ===\n");
    printf("最终数据值: %d\n", *data);

    // ========== 清理系统资源 ==========
    if (shmdt(data) == -1) {  // 增加错误检查
        perror("shmdt error");
        exit(EXIT_FAILURE);
    }
    if (shmctl(shmid, IPC_RMID, NULL) == -1) {  // 删除共享内存
        perror("shmctl error");
        exit(EXIT_FAILURE);
    }
    if (semctl(semid, 0, IPC_RMID) == -1) {      // 删除信号量集
        perror("semctl remove error");
        exit(EXIT_FAILURE);
    }
    return 0;
}

/*
 * 核心同步机制说明：
 *
 * 1. 读者优先策略：
 *    - 多个读者可以同时读
 *    - 写者必须独占访问
 *    - 第一个读者阻止写者，最后一个读者允许写者
 *
 * 2. 互斥信号量 (mutex)：
 *    - 保护读者计数 (readcount) 的访问
 *    - 确保读者计数的增减操作是原子的
 *
 * 3. 写者互斥信号量 (wrt)：
 *    - 写者必须获得此信号量才能写
 *    - 第一个读者获得此信号量阻止写者
 *    - 最后一个读者释放此信号量允许写者
 *
 * 4. 读者计数 (readcount)：
 *    - 记录当前正在读的读者数量
 *    - 用于判断是否为第一个或最后一个读者
 *
 * 5. 同步流程：
 *    - 读者：增加计数 → 第一个读者阻止写者 → 读操作 → 减少计数 → 最后一个读者允许写者
 *    - 写者：等待写者信号量 → 写操作 → 释放写者信号量
 *
 * 6. 读者优先体现：
 *    - 读者可以"插队"：即使写者在等待，新来的读者也能先读
 *    - 写者必须等待所有读者完成
 *    - 避免了写者饥饿问题（在写者优先策略中）
 */