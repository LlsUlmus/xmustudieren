#include <stdio.h>
#include <stdlib.h>
#include <string.h>

// 进程结构体
typedef struct Process {
    char id[2];          // 进程ID
    int arrivalTime;     // 到达时间
    int serviceTime;     // 服务时间
    int remainingTime;   // 剩余服务时间
    int finishTime;      // 完成时间
    int turnaroundTime;  // 周转时间（完成-到达）
    double responseRatio;// 响应比（(等待+服务)/服务）
} Process;

// 复制进程数组（避免原数据污染）
void copyProcesses(Process* src, Process* dest, int n) {
    for (int i = 0; i < n; i++) {
        strcpy(dest[i].id, src[i].id);
        dest[i].arrivalTime = src[i].arrivalTime;
        dest[i].serviceTime = src[i].serviceTime;
        dest[i].remainingTime = src[i].serviceTime;
        dest[i].finishTime = 0;
        dest[i].turnaroundTime = 0;
        dest[i].responseRatio = 0.0;
    }
}

// 1. 先来先服务（FCFS）
void fcfs(Process* original, int n, Process* result) {
    copyProcesses(original, result, n);
    int currentTime = 0;

    // 按到达时间排序
    for (int i = 0; i < n - 1; i++) {
        for (int j = 0; j < n - i - 1; j++) {
            if (result[j].arrivalTime > result[j + 1].arrivalTime) {
                Process temp = result[j];
                result[j] = result[j + 1];
                result[j + 1] = temp;
            }
        }
    }

    // 执行调度
    for (int i = 0; i < n; i++) {
        currentTime = currentTime < result[i].arrivalTime ? result[i].arrivalTime : currentTime;
        result[i].finishTime = currentTime + result[i].serviceTime;
        result[i].turnaroundTime = result[i].finishTime - result[i].arrivalTime;
        int waitingTime = result[i].turnaroundTime - result[i].serviceTime;
        result[i].responseRatio = (waitingTime + result[i].serviceTime) / (double)result[i].serviceTime;
        currentTime = result[i].finishTime;
    }
}

// 2. 轮转调度（RR q=1）- 修复就绪队列大小
void rr(Process* original, int n, Process* result) {
    copyProcesses(original, result, n);
    int currentTime = 0, completed = 0;
    int* inQueue = (int*)calloc(n, sizeof(int));
    int front = 0, rear = 0;
    // 就绪队列大小设为n*10（足够容纳多次入队的进程）
    Process* readyQueue = (Process*)malloc(n * 10 * sizeof(Process));

    while (completed < n) {
        // 加入就绪进程
        for (int i = 0; i < n; i++) {
            if (result[i].arrivalTime <= currentTime && result[i].remainingTime > 0 && inQueue[i] == 0) {
                readyQueue[rear++] = result[i];
                inQueue[i] = 1;
            }
        }

        if (front < rear) {
            Process p = readyQueue[front++];
            p.remainingTime--;
            currentTime++;

            // 找到原进程索引
            int idx = -1;
            for (int i = 0; i < n; i++) {
                if (strcmp(result[i].id, p.id) == 0) { idx = i; break; }
            }

            if (p.remainingTime == 0) {
                result[idx].finishTime = currentTime;
                result[idx].turnaroundTime = result[idx].finishTime - result[idx].arrivalTime;
                int waitingTime = result[idx].turnaroundTime - result[idx].serviceTime;
                result[idx].responseRatio = (waitingTime + result[idx].serviceTime) / (double)result[idx].serviceTime;
                completed++;
                inQueue[idx] = 0; // 进程完成，重置入队标记
            }
            else {
                readyQueue[rear++] = p;
                result[idx].remainingTime = p.remainingTime;
            }
        }
        else {
            currentTime++;
        }
    }

    free(inQueue);
    free(readyQueue);
}

// 3. 最短进程优先（SPN）- 非抢占式
void spn(Process* original, int n, Process* result) {
    copyProcesses(original, result, n);
    int currentTime = 0, completed = 0;
    int* isCompleted = (int*)calloc(n, sizeof(int));

    while (completed < n) {
        int selectedIdx = -1;
        // 选择就绪且服务时间最短的进程
        for (int i = 0; i < n; i++) {
            if (result[i].arrivalTime <= currentTime && isCompleted[i] == 0) {
                if (selectedIdx == -1 || result[i].serviceTime < result[selectedIdx].serviceTime) {
                    selectedIdx = i;
                }
            }
        }

        if (selectedIdx != -1) {
            Process* p = &result[selectedIdx];
            p->finishTime = currentTime + p->serviceTime;
            p->turnaroundTime = p->finishTime - p->arrivalTime;
            int waitingTime = p->turnaroundTime - p->serviceTime;
            p->responseRatio = (waitingTime + p->serviceTime) / (double)p->serviceTime;

            isCompleted[selectedIdx] = 1;
            currentTime = p->finishTime;
            completed++;
        }
        else {
            currentTime++;
        }
    }

    free(isCompleted);
}

// 4. 最短剩余时间（SRT）- 抢占式
void srt(Process* original, int n, Process* result) {
    copyProcesses(original, result, n);
    int currentTime = 0, completed = 0;
    int* isCompleted = (int*)calloc(n, sizeof(int));

    while (completed < n) {
        int selectedIdx = -1;
        // 选择就绪且剩余时间最短的进程
        for (int i = 0; i < n; i++) {
            if (result[i].arrivalTime <= currentTime && isCompleted[i] == 0) {
                if (selectedIdx == -1 || result[i].remainingTime < result[selectedIdx].remainingTime) {
                    selectedIdx = i;
                }
            }
        }

        if (selectedIdx != -1) {
            Process* p = &result[selectedIdx];
            p->remainingTime--;
            currentTime++;

            if (p->remainingTime == 0) {
                p->finishTime = currentTime;
                p->turnaroundTime = p->finishTime - p->arrivalTime;
                int waitingTime = p->turnaroundTime - p->serviceTime;
                p->responseRatio = (waitingTime + p->serviceTime) / (double)p->serviceTime;

                isCompleted[selectedIdx] = 1;
                completed++;
            }
        }
        else {
            currentTime++;
        }
    }

    free(isCompleted);
}

// 5. 最高响应比优先（HRRN）- 非抢占式
void hrrn(Process* original, int n, Process* result) {
    copyProcesses(original, result, n);
    int currentTime = 0, completed = 0;
    int* isCompleted = (int*)calloc(n, sizeof(int));

    while (completed < n) {
        int selectedIdx = -1;
        double maxRatio = -1.0;

        // 计算响应比，选择最高的进程
        for (int i = 0; i < n; i++) {
            if (result[i].arrivalTime <= currentTime && isCompleted[i] == 0) {
                int waitingTime = currentTime - result[i].arrivalTime;
                double ratio = (waitingTime + result[i].serviceTime) / (double)result[i].serviceTime;

                if (ratio > maxRatio || (ratio == maxRatio && result[i].arrivalTime < result[selectedIdx].arrivalTime)) {
                    maxRatio = ratio;
                    selectedIdx = i;
                }
            }
        }

        if (selectedIdx != -1) {
            Process* p = &result[selectedIdx];
            p->finishTime = currentTime + p->serviceTime;
            p->turnaroundTime = p->finishTime - p->arrivalTime;
            int waitingTime = p->turnaroundTime - p->serviceTime;
            p->responseRatio = (waitingTime + p->serviceTime) / (double)p->serviceTime;

            isCompleted[selectedIdx] = 1;
            currentTime = p->finishTime;
            completed++;
        }
        else {
            currentTime++;
        }
    }

    free(isCompleted);
}

// 打印调度结果
void printResult(const char* algorithm, Process* processes, int n) {
    printf("\n%s 调度结果\n", algorithm);
    printf("%-6s %-8s %-8s %-8s %-8s %-8s\n",
        "进程ID", "到达时间", "服务时间", "完成时间", "周转时间", "响应比");

    double avgTurnaround = 0.0, avgRatio = 0.0;
    for (int i = 0; i < n; i++) {
        printf("%-6s %-8d %-8d %-8d %-8d %-8.2f\n",
            processes[i].id, processes[i].arrivalTime, processes[i].serviceTime,
            processes[i].finishTime, processes[i].turnaroundTime, processes[i].responseRatio);
        avgTurnaround += processes[i].turnaroundTime;
        avgRatio += processes[i].responseRatio;
    }

    avgTurnaround /= n;
    avgRatio /= n;
    printf("平均周转时间：%.2f | 平均响应比：%.2f\n", avgTurnaround, avgRatio);
}

// 主函数
int main() {
    int n = 5;
    Process original[] = {
        {"A", 0, 3, 3, 0, 0, 0.0},
        {"B", 2, 6, 6, 0, 0, 0.0},
        {"C", 4, 4, 4, 0, 0, 0.0},
        {"D", 6, 5, 5, 0, 0, 0.0},
        {"E", 8, 2, 2, 0, 0, 0.0}
    };

    Process fcfsResult[n], rrResult[n], spnResult[n], srtResult[n], hrrnResult[n];

    fcfs(original, n, fcfsResult);
    printResult("先来先服务（FCFS）", fcfsResult, n);

    rr(original, n, rrResult);
    printResult("轮转调度（RR q=1）", rrResult, n);

    spn(original, n, spnResult);
    printResult("最短进程优先（SPN）", spnResult, n);

    srt(original, n, srtResult);
    printResult("最短剩余时间（SRT）", srtResult, n);

    hrrn(original, n, hrrnResult);
    printResult("最高响应比优先（HRRN）", hrrnResult, n);

    return 0;
}