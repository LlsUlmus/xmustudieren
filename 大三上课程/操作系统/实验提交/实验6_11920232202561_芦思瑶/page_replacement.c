#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>

// 页面序列最大长度
#define MAX_PAGE_SEQ 100
// 内存块最大数量
#define MAX_FRAME 10

// 函数声明
void init(int* page_seq, int* seq_len, int* frame_num);
int fifo(int* page_seq, int seq_len, int frame_num);
int lru(int* page_seq, int seq_len, int frame_num);
int opt(int* page_seq, int seq_len, int frame_num);
bool is_in_frame(int page, int* frame, int frame_num);
void print_result(int page_fault, int seq_len, char* algorithm);

int main() {
    int page_seq[MAX_PAGE_SEQ];  // 页面序列
    int seq_len;                 // 页面序列长度
    int frame_num;               // 内存块数量
    int page_fault;              // 缺页数

    // 初始化页面序列、序列长度、内存块数量
    init(page_seq, &seq_len, &frame_num);

    // 执行FIFO算法
    page_fault = fifo(page_seq, seq_len, frame_num);
    print_result(page_fault, seq_len, "FIFO");

    // 执行LRU算法
    page_fault = lru(page_seq, seq_len, frame_num);
    print_result(page_fault, seq_len, "LRU");

    // 执行OPT算法
    page_fault = opt(page_seq, seq_len, frame_num);
    print_result(page_fault, seq_len, "OPT");

    return 0;
}

// 初始化函数：输入页面序列、序列长度、内存块数量
void init(int* page_seq, int* seq_len, int* frame_num) {
    printf("===== 内存页面置换算法模拟 =====\n");
    printf("请输入页面序列长度（不超过100）：");
    scanf("%d", seq_len);
    while (*seq_len <= 0 || *seq_len > MAX_PAGE_SEQ) {
        printf("输入非法！请输入1-%d之间的整数：", MAX_PAGE_SEQ);
        scanf("%d", seq_len);
    }

    printf("请输入页面序列（空格分隔，如1 5 3 4 2）：");
    for (int i = 0; i < *seq_len; i++) {
        scanf("%d", &page_seq[i]);
    }

    printf("请输入内存块数量（不超过10）：");
    scanf("%d", frame_num);
    while (*frame_num <= 0 || *frame_num > MAX_FRAME) {
        printf("输入非法！请输入1-%d之间的整数：", MAX_FRAME);
        scanf("%d", frame_num);
    }
    printf("\n");
}

// 判断页面是否在内存块中
bool is_in_frame(int page, int* frame, int frame_num) {
    for (int i = 0; i < frame_num; i++) {
        if (frame[i] == page) {
            return true;
        }
    }
    return false;
}

// FIFO算法：返回缺页数
int fifo(int* page_seq, int seq_len, int frame_num) {
    int frame[MAX_FRAME] = { -1 };  // 内存块，初始化为-1（表示空）
    int page_fault = 0;           // 缺页数
    int ptr = 0;                  // 指向要替换的内存块（先进先出指针）

    for (int i = 0; i < seq_len; i++) {
        int page = page_seq[i];
        // 页面不在内存中，产生缺页
        if (!is_in_frame(page, frame, frame_num)) {
            frame[ptr] = page;    // 替换指针指向的内存块
            ptr = (ptr + 1) % frame_num;  // 指针循环移动
            page_fault++;
        }
    }
    return page_fault;
}

// LRU算法：返回缺页数
int lru(int* page_seq, int seq_len, int frame_num) {
    int frame[MAX_FRAME] = { -1 };  // 内存块，初始化为-1（表示空）
    int page_fault = 0;           // 缺页数
    int last_used[MAX_FRAME];     // 记录每个内存块的最近使用时间（页面序列索引）

    for (int i = 0; i < seq_len; i++) {
        int page = page_seq[i];
        // 页面不在内存中，产生缺页
        if (!is_in_frame(page, frame, frame_num)) {
            page_fault++;
            // 内存块未满，直接放入空块
            int empty_idx = -1;
            for (int j = 0; j < frame_num; j++) {
                if (frame[j] == -1) {
                    empty_idx = j;
                    break;
                }
            }
            if (empty_idx != -1) {
                frame[empty_idx] = page;
                last_used[empty_idx] = i;
            }
            else {
                // 内存块已满，找到最近最少使用的块（last_used最小）
                int lru_idx = 0;
                for (int j = 1; j < frame_num; j++) {
                    if (last_used[j] < last_used[lru_idx]) {
                        lru_idx = j;
                    }
                }
                frame[lru_idx] = page;
                last_used[lru_idx] = i;
            }
        }
        else {
            // 页面在内存中，更新最近使用时间
            for (int j = 0; j < frame_num; j++) {
                if (frame[j] == page) {
                    last_used[j] = i;
                    break;
                }
            }
        }
    }
    return page_fault;
}

// OPT算法：返回缺页数
int opt(int* page_seq, int seq_len, int frame_num) {
    int frame[MAX_FRAME] = { -1 };  // 内存块，初始化为-1（表示空）
    int page_fault = 0;           // 缺页数

    for (int i = 0; i < seq_len; i++) {
        int page = page_seq[i];
        // 页面不在内存中，产生缺页
        if (!is_in_frame(page, frame, frame_num)) {
            page_fault++;
            // 内存块未满，直接放入空块
            int empty_idx = -1;
            for (int j = 0; j < frame_num; j++) {
                if (frame[j] == -1) {
                    empty_idx = j;
                    break;
                }
            }
            if (empty_idx != -1) {
                frame[empty_idx] = page;
            }
            else {
                // 内存块已满，找到未来最久不使用的块
                int opt_idx = 0;
                int max_dist = -1;  // 记录最远使用距离
                for (int j = 0; j < frame_num; j++) {
                    int dist = 0;
                    // 查找当前块中的页面在未来何时使用
                    for (int k = i + 1; k < seq_len; k++) {
                        if (frame[j] == page_seq[k]) {
                            dist = k - i;
                            break;
                        }
                    }
                    // 未来不再使用的页面，直接替换
                    if (dist == 0) {
                        opt_idx = j;
                        break;
                    }
                    // 选择最远使用的页面
                    if (dist > max_dist) {
                        max_dist = dist;
                        opt_idx = j;
                    }
                }
                frame[opt_idx] = page;
            }
        }
    }
    return page_fault;
}

// 打印算法结果（缺页数、缺页率、命中率）
void print_result(int page_fault, int seq_len, char* algorithm) {
    float fault_rate = (float)page_fault / seq_len;  // 缺页率
    float hit_rate = 1 - fault_rate;                 // 命中率
    printf("===== %s 算法结果 =====\n", algorithm);
    printf("缺页数：%d\n", page_fault);
    printf("缺页率：%.2f%%\n", fault_rate * 100);
    printf("命中率：%.2f%%\n\n", hit_rate * 100);
}