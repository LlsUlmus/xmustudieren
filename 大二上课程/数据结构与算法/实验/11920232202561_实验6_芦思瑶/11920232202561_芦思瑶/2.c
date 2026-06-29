#define _CRT_SECURE_NO_WARNINGS 1
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

typedef struct LinkListNode{
	char* chinese;
	struct LinkListNode* next;
}node,*LinkList;

node* createNode(char* input);
void insertNode(LinkList* head, LinkList* tail, node* newNode);
void bubbleSort(LinkList head);
void freeList(LinkList head);
void swapNodes(node* a, node*b);

int main()
{
	LinkList L;
	LinkList head = NULL;//表头
	LinkList tail = NULL;//表尾
	int n = 0;//部门数小于等于20
	printf("请输入部门的数量\n");
	scanf("%d", &n);
	char input[100] = { '0' };
	printf("请输入部门的名称\n");
	for (int i = 0; i < n; i++)
	{
		scanf("%99s", input);  // 假设MAX_CHINESE_LENGTH的值为100，这里读取最多99个字符
		while (getchar() != '\n');
		node* newNode = createNode(input);
		if (newNode == NULL) {
			break;
		}
		insertNode(&head, &tail, newNode);
	}
	bubbleSort(head);
	LinkList current = head;
	while (current != NULL) {
		printf("%s\n", current->chinese);
		current = current->next;
	}

	// 释放链表内存
	freeList(head);
	return 0;
}

node* createNode(char* input)
{
	node* ptr = (node*)malloc(sizeof(node));
	if (ptr == NULL) {
		printf("内存分配失败！\n");
		return NULL;
	}
	ptr->chinese = (char*)malloc(strlen(input) + 1);
	if (ptr->chinese == NULL) {
		printf("内存分配失败！\n");
		free(ptr);
		return NULL;
	}
	strcpy(ptr->chinese, input);  // 添加这行代码，将输入字符串复制到节点的chinese成员指向的内存空间
	ptr->next = NULL;  // 同时建议添加这行，明确将新节点的next指针初始化为NULL
	return ptr;
}

void insertNode(LinkList* head, LinkList* tail, node* newNode)
{
	if (*head == NULL) {
		*head = newNode;
		*tail = newNode;
	}
	else {
		(*tail)->next = newNode;
		*tail = newNode;
	}
}

void bubbleSort(LinkList head)
{
	int swapped;
	LinkList ptr;
	LinkList lptr = NULL;
	if (head == NULL) {
		return;
	}
	do {
		swapped = 0;
		ptr = head;
		while (ptr->next != lptr) {
			// 使用strcmp比较两个节点中部门名称的汉字字典序
			if (strcmp(ptr->chinese, ptr->next->chinese) > 0) {
				swapNodes(ptr, ptr->next);
				swapped = 1;
			}
			ptr = ptr->next;
		}
		lptr = ptr;
	} while (swapped);
}

void freeList(LinkList head)
{
	LinkList current = head;
	LinkList next;
	while (current != NULL) {
		next = current->next;
		free(current->chinese);
		free(current);
		current = next;
	}
}

void swapNodes(node* a, node* b)
{
	char* temp = a->chinese;
	a->chinese = b->chinese;
	b->chinese = temp;
}