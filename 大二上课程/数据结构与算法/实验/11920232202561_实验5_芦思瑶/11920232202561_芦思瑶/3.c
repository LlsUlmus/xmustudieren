#define _CRT_SECURE_NO_WARNINGS 1
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#define MAX_NAME_LEN 50
#define TABLE_SIZE 37  // ASL <= 2

typedef struct {  // 每个槽位
    char surname[MAX_NAME_LEN];  // 存储拼音
    int isoccupied;  // 该槽位是否被占用
} HashTableEntry;

typedef struct {
    HashTableEntry table[TABLE_SIZE];  // 哈希表数组
} HashTable;

void InsertTable(HashTable* neun, char* name);
void InitTable(HashTable* neun);
int hash(char* name);
int SearchTable(HashTable* neun, char* name);
void PrintHashTable(HashTable* neun);
int IsTableFull(HashTable* neun);

int main() {
    HashTable neun;
    InitTable(&neun);
    char name[MAX_NAME_LEN];
    printf("请输入最多30个拼音（输入'end'结束）：\n");

    for (int i = 0; i < 30; i++) {
        scanf("%s", name);
        if (strcmp(name, "end") == 0) {
            break;
        }
        if (IsTableFull(&neun)) {
            printf("哈希表已满，无法插入更多数据。\n");
            break;
        }
        InsertTable(&neun, name);
    }

    printf("请输入要查找的拼音：\n");
    scanf("%s", name);
    int result = SearchTable(&neun, name);
    if (result != -1) {
        printf("找到了拼音 %s，索引位置为 %d\n", name, result);
    }
    else {
        printf("未找到拼音 %s\n", name);
    }

    PrintHashTable(&neun);
    return 0;
}

void InitTable(HashTable* neun) {
    for (int i = 0; i < TABLE_SIZE; i++) {
        neun->table[i].isoccupied = 0;  // 标记为未占用
        neun->table[i].surname[0] = '\0';  // 初始化为空字符串
    }
}

int IsTableFull(HashTable* neun) {
    for (int i = 0; i < TABLE_SIZE; i++) {
        if (!neun->table[i].isoccupied) {
            return 0;  // 找到空槽，表未满
        }
    }
    return 1;  // 表已满
}

void InsertTable(HashTable* neun, char* name) {
    int value = hash(name);  // 计算哈希值
    int original_value = value;  // 记录初始哈希值

    // 线性探测法处理冲突
    while (neun->table[value].isoccupied) {
        if (strcmp(neun->table[value].surname, name) == 0) {
            printf("拼音 %s 已存在。\n", name);  // 如果已存在，则不插入
            return;
        }
        value = (value + 1) % TABLE_SIZE;  // 线性探测
        if (value == original_value) {  // 回到起始位置，表已满
            printf("哈希表已满，无法插入拼音 %s。\n", name);
            return;
        }
    }

    // 插入新数据
    strcpy(neun->table[value].surname, name);
    neun->table[value].isoccupied = 1;
    printf("插入拼音：%s 到索引 %d\n", name, value);
}

int hash(char* name) {
    int count = 0;
    for (int i = 0; name[i] != '\0'; i++) {
        count += name[i];  // 计算字符的ASCII值之和
    }
    int result = count % TABLE_SIZE;  // 对哈希表的大小取模
    if (result < 0) result += TABLE_SIZE;  // 确保返回非负值
    return result;
}

int SearchTable(HashTable* neun, char* name) {
    int value = hash(name);  // 计算哈希值
    int original_value = value;  // 记录初始哈希值

    while (neun->table[value].isoccupied) {
        if (strcmp(neun->table[value].surname, name) == 0) {
            return value;  // 找到目标值，返回索引
        }
        value = (value + 1) % TABLE_SIZE;  // 线性探测
        if (value == original_value) {  // 回到起始位置，说明未找到
            break;
        }
    }
    return -1;  // 查找失败
}

void PrintHashTable(HashTable* neun) {
    printf("\n哈希表内容：\n");
    for (int i = 0; i < TABLE_SIZE; i++) {
        if (neun->table[i].isoccupied) {
            printf("索引 %d: %s\n", i, neun->table[i].surname);
        }
        else {
            printf("索引 %d: [空]\n", i);
        }
    }
}