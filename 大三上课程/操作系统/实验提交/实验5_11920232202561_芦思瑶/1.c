#include <stdio.h>
#include <stdbool.h>
#include <string.h>

#define PROCESS_COUNT 5   // 进程总数（P0~P4）
#define RESOURCE_COUNT 3  // 资源种类数（R0~R2）

// 核心数据结构
int available[RESOURCE_COUNT];          // 系统可用资源向量V
int maximum[PROCESS_COUNT][RESOURCE_COUNT]; // 进程最大需求矩阵C
int allocation[PROCESS_COUNT][RESOURCE_COUNT]; // 资源分配矩阵A
int need[PROCESS_COUNT][RESOURCE_COUNT];  // 进程剩余需求矩阵Need
bool is_process_started[PROCESS_COUNT];   // 进程启动状态标记（true=已启动）

// 初始化实验参数：资源向量、需求矩阵、分配矩阵及进程状态
void initialize() {
    // 初始可用资源向量
    available[0] = 3; available[1] = 3; available[2] = 2;

    // 进程最大需求矩阵
    int max[PROCESS_COUNT][RESOURCE_COUNT] = {
        {7, 5, 3},
        {3, 2, 2},
        {9, 0, 2},
        {2, 2, 2},
        {4, 3, 3}
    };

    // 初始资源分配矩阵
    int alloc[PROCESS_COUNT][RESOURCE_COUNT] = {
        {0, 1, 0},
        {2, 0, 0},
        {3, 0, 2},
        {2, 1, 1},
        {0, 0, 2}
    };

    // 复制矩阵并计算剩余需求（Need = maximum - allocation）
    for (int i = 0; i < PROCESS_COUNT; i++) {
        for (int j = 0; j < RESOURCE_COUNT; j++) {
            maximum[i][j] = max[i][j];
            allocation[i][j] = alloc[i][j];
            need[i][j] = maximum[i][j] - allocation[i][j];
        }
        is_process_started[i] = false; // 初始所有进程未启动
    }
}

// 安全状态检查：返回true=安全（输出安全序列），false=不安全
bool check_safety() {
    int work[RESOURCE_COUNT];
    bool finish[PROCESS_COUNT] = { false };
    int safe_sequence[PROCESS_COUNT];
    int count = 0;

    // 初始化工作向量为当前可用资源
    memcpy(work, available, sizeof(work));

    while (count < PROCESS_COUNT) {
        bool found = false;
        // 遍历所有已启动且未完成的进程
        for (int p = 0; p < PROCESS_COUNT; p++) {
            if (!is_process_started[p] || finish[p]) continue;

            // 检查进程剩余需求是否≤工作向量
            int j;
            for (j = 0; j < RESOURCE_COUNT; j++) {
                if (need[p][j] > work[j]) break;
            }

            // 满足需求：释放资源，记录安全序列
            if (j == RESOURCE_COUNT) {
                for (int k = 0; k < RESOURCE_COUNT; k++) {
                    work[k] += allocation[p][k];
                }
                safe_sequence[count++] = p;
                finish[p] = true;
                found = true;
            }
        }

        if (!found) {
            printf("❌ 系统处于不安全状态\n");
            return false;
        }
    }

    // 输出安全序列
    printf("✅ 系统处于安全状态，安全序列：");
    for (int i = 0; i < PROCESS_COUNT; i++) {
        printf("P%d", safe_sequence[i]);
        if (i != PROCESS_COUNT - 1) printf(" → ");
    }
    printf("\n");
    return true;
}

// 进程启动处理：返回true=启动成功，false=启动失败
bool start_process(int p) {
    if (is_process_started[p]) {
        printf("❌ 进程P%d已启动，无需重复操作\n", p);
        return true;
    }

    // 检查最大需求是否超出可用资源（启动前提）
    for (int j = 0; j < RESOURCE_COUNT; j++) {
        if (maximum[p][j] > available[j]) {
            printf("❌ 进程P%d启动拒绝：最大需求[%d,%d,%d] > 可用资源[%d,%d,%d]\n",
                p, maximum[p][0], maximum[p][1], maximum[p][2],
                available[0], available[1], available[2]);
            return false;
        }
    }

    // 标记进程启动，检查启动后系统安全性
    is_process_started[p] = true;
    printf("🔍 进程P%d启动后，检查系统安全性...\n", p);

    if (check_safety()) {
        printf("✅ 进程P%d启动成功\n", p);
        // 输出当前核心状态
        printf("当前分配矩阵A：\n");
        for (int i = 0; i < PROCESS_COUNT; i++) {
            if (is_process_started[i]) {
                printf("P%d: [%d, %d, %d]\n", i,
                    allocation[i][0], allocation[i][1], allocation[i][2]);
            }
        }
        printf("当前可用资源V：[%d, %d, %d]\n",
            available[0], available[1], available[2]);
        return true;
    }
    else {
        printf("❌ 进程P%d启动拒绝：启动后系统不安全\n", p);
        is_process_started[p] = false; // 回滚状态
        return false;
    }
}

// 资源请求处理（银行家算法）：返回true=分配成功，false=分配失败
bool request_resources(int p, int req[]) {
    // 检查进程是否已启动
    if (!is_process_started[p]) {
        printf("❌ 资源分配拒绝：进程P%d未启动，请先启动\n", p);
        return false;
    }

    // 检查请求是否超过剩余需求
    for (int i = 0; i < RESOURCE_COUNT; i++) {
        if (req[i] > need[p][i]) {
            printf("❌ 进程P%d请求[%d,%d,%d]超过最大需求，拒绝分配\n",
                p, req[0], req[1], req[2]);
            return false;
        }
    }

    // 检查资源是否可用
    for (int i = 0; i < RESOURCE_COUNT; i++) {
        if (req[i] > available[i]) {
            printf("❌ 进程P%d请求[%d,%d,%d]：资源不足，需等待\n",
                p, req[0], req[1], req[2]);
            return false;
        }
    }

    // 暂时分配资源
    for (int i = 0; i < RESOURCE_COUNT; i++) {
        available[i] -= req[i];
        allocation[p][i] += req[i];
        need[p][i] -= req[i];
    }

    // 检查预分配后系统安全性
    printf("🔍 进程P%d请求[%d,%d,%d]，预分配后检查安全性...\n",
        p, req[0], req[1], req[2]);
    if (check_safety()) {
        printf("✅ 进程P%d资源分配成功\n", p);
        printf("更新后可用资源V：[%d, %d, %d]\n",
            available[0], available[1], available[2]);
        printf("更新后P%d分配矩阵：[%d, %d, %d]\n", p,
            allocation[p][0], allocation[p][1], allocation[p][2]);
        return true;
    }

    // 不安全则回滚分配
    for (int i = 0; i < RESOURCE_COUNT; i++) {
        available[i] += req[i];
        allocation[p][i] -= req[i];
        need[p][i] += req[i];
    }
    printf("❌ 进程P%d资源分配失败，已恢复系统状态\n", p);
    return false;
}

int main() {
    initialize();

    // 实验流程：先启动进程，再处理资源请求
    int start_sequence[] = { 0, 1, 2, 3, 4 }; // 进程启动序列
    int requests[5][RESOURCE_COUNT] = {
        {1, 0, 2},  // P1请求
        {3, 3, 0},  // P0请求
        {2, 0, 1},  // P2请求
        {0, 2, 0},  // P3请求
        {3, 3, 3}   // P4请求
    };
    int req_pids[] = { 1, 0, 2, 3, 4 }; // 请求对应的进程ID

    // 实验启动提示
    printf("=============================================\n");
    printf("         实验五：死锁与饥饿（银行家算法）\n");
    printf("=============================================\n");
    printf("初始可用资源V：[%d, %d, %d]\n", available[0], available[1], available[2]);
    printf("提示：输入任意空格+回车，继续下一步\n");
    getchar();

    // 阶段1：启动所有进程
    printf("\n=============================================\n");
    printf("               阶段1：启动进程\n");
    printf("=============================================\n");
    for (int i = 0; i < PROCESS_COUNT; i++) {
        int p = start_sequence[i];
        printf("\n【第%d步：启动进程P%d】\n", i + 1, p);
        start_process(p);
        printf("\n---------------------------------------------\n");
        printf("输入任意空格+回车，继续下一步\n");
        getchar();
    }

    // 阶段2：处理资源请求
    printf("\n=============================================\n");
    printf("               阶段2：资源分配\n");
    printf("=============================================\n");
    for (int i = 0; i < 5; i++) {
        int p = req_pids[i];
        int* req = requests[i];
        printf("\n【第%d步：处理进程P%d的请求】\n", i + 1, p);
        request_resources(p, req);
        printf("\n---------------------------------------------\n");
        printf("输入任意空格+回车，继续下一步\n");
        getchar();
    }

    printf("\n=============================================\n");
    printf("               实验结束\n");
    printf("=============================================\n");
    return 0;
}