#define _CRT_SECURE_NO_WARNINGS 1
#include <stdio.h>
#include <stdlib.h>
#define Type int
#define N1 1000//
#define INF 99999//无限大

typedef struct SeqList{
	Type *elem;//int型
	int n;//线性表的长度
	int N;//最多存放的数目
}SList;//顺序表

typedef struct TreeNode {
    Type value;
    int underline;//下标
    int left;
    int right;
} TNode;

void IniSlist(SList *L);
void InsertSList(SList *L,Type num);
void TreeSelect(SList L);
void heapify(TNode* ptr, int start, int i);
void PrintSList(SList *L);

int main()
{
	SList L;
	IniSlist(&L);
	Type sort[N1] = { 0 };
	int sortnum = 0;
	printf("请输入待排序的数组长度\n");
	scanf("%d", &sortnum);
	printf("请输入待排序的数组元素\n");
	for (int i = 0; i < sortnum; i++)
	{
		scanf("%d", &sort[i]);
		InsertSList(&L, sort[i]);
	}
	TreeSelect(L);
	PrintSList(&L);
	return 0;
}

void IniSlist(SList* L)
{
	L->elem = (Type*)malloc(N1 * sizeof(Type));
	if (L->elem == NULL){
		printf("分配内存失败\n");
		return;
	}
    L->n = 0;
	L->N = N1;
}

void InsertSList(SList* L, Type num)
{
	if (L->n == L->N) {
		printf("顺序表已满\n");
		return;
	}
	L->elem[L->n] = num;
	L->n++;
}

void TreeSelect(SList L)
{
    TNode* ptr = (TNode*)malloc(L.n * sizeof(TNode));
    if (ptr == NULL) {
        printf("分配内存失败\n");
        return;
    }

    // 初始化堆，按顺序将顺序表元素对应到节点数组，
    for (int i = 0; i < L.n; i++) {
        ptr[i].value = L.elem[i];
        ptr[i].underline = i;
    }
    // 构建大顶堆
    for (int i = (L.n - 2) / 2; i >= 0; i--) {
        heapify(ptr, i, L.n);
    }

    // 排序，将堆顶元素与未排序最后一个交换重新调整堆
    for (int i = L.n - 1; i > 0; i--) {
        // 交换堆顶元素与当前未排序部分的最后一个元素
        Type tmp_value = ptr[0].value;
        int tmp_underline = ptr[0].underline;
        ptr[0].value = ptr[i].value;
        ptr[0].underline = ptr[i].underline;
        ptr[i].value = tmp_value;
        ptr[i].underline = tmp_underline;

        // 已排序部分增加，堆大小减1
        heapify(ptr, 0, i);
    }
    // 复制回原顺序表
    for (int i = 0; i < L.n; i++) {
        L.elem[i] = ptr[i].value;
    }

    free(ptr);
}

// 维护堆性质
void heapify(TNode* ptr, int i, int size)
{
    int largest = i;
    int left = 2 * i + 1;
    int right = 2 * i + 2;

    // 左子节点是否存在且大于根节点
    if (left < size && ptr[left].value > ptr[largest].value) {
        largest = left;
    }
    // 右子节点是否存在且大于当前最大节点
    if (right < size && ptr[right].value > ptr[largest].value) {
        largest = right;
    }
    if (largest != i) {
        // 交换根节点与最大子节点的值和下标
        Type tmp_value = ptr[i].value;
        int tmp_underline = ptr[i].underline;
        ptr[i].value = ptr[largest].value;
        ptr[i].underline = ptr[largest].underline;
        ptr[largest].value = tmp_value;
        ptr[largest].underline = tmp_underline;
        // 递归向下调整交换后的子树，保持堆性质
        heapify(ptr, largest, size);
    }
}


void PrintSList(SList* L)
{
	for (int i = 0; i < L->n; i++)
	{
		printf("%d ", L->elem[i]);
	}
	printf("\n");
}