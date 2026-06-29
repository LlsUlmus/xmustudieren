#define _CRT_SECURE_NO_WARNINGS 1
#include <stdio.h>
#include <stdlib.h>
typedef struct Node
{
	int data;
	struct Node *next;
}Node, *LinkList;

 LinkList Creat(Node *L)
 {
	 L = (Node*)malloc(sizeof(Node));
	 if (!L)return 0;
	 L->next = NULL;
	 return L;
 }

 void Insert(Node* L)
 {
	 Node* q = L;
	 int i = 0;
	 printf("请键入您想输入元素的个数\n");
	 scanf("%d", &i);//输入元素个数
	 for (int j = 0; j < i; j++)
	 {
		 LinkList p = (Node*)malloc(sizeof(Node));
		 printf("请键入数据\n");
		 scanf("%d", &p->data);//输入数据
		 q->next = p;
		 q = q->next;
	 }
	 q->next = NULL;
 }

 void Print(Node* L)
 {
	 Node* p = L->next;
	 while (p)
	 {
		 printf("%d ", p->data);
		 p = p->next;
	 }
 }

 void Delet(Node* L)
 {
	 int i;
	 printf("请键入您想删除第几个结点\n");
	 scanf("%d", &i);//输入删除的节点
	 Node* p = L;
	 Node* q;
	 int j = 0; 
	 while (p && (j < i - 1))
	 {
		 p = p->next;
		 j++;

		 if (p)
		 {
			 q = p->next;
			 q->next = p->next;
			 free(p);
		 }
	 }
 }

 void Destroy(Node* L)
 {
	 Node* p = L->next;
	 while (p)
	 {
		 Node* q = p->next;
		 free(p);
		 p = q;
	 }
	 L->next = NULL;
	 L = NULL;
 }

int main()
{
	Node *L= (Node*)malloc(sizeof(Node));
	L->next = NULL;
	Creat(L);//创建
	Insert(L);//插入and输入
	Print(L);//打印
	Delet(L);//删除
	Print(L);
	Destroy(L);//销毁
	return 0;
}