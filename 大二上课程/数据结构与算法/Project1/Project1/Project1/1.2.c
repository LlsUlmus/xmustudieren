//#define _CRT_SECURE_NO_WARNINGS 1
//#include <stdio.h>
//#include <stdlib.h>

//typedef struct Node
//{
//	char charactor;
//	struct Node *next;
//}Node,*LinkList;
//
//LinkList Creat()
//{
//	Node *L = (Node*)malloc(sizeof(Node));
//	if (!L)return NULL;
//	L->next = NULL;
//	return L;
//}
//
//void Insert(Node *L,char c)
//{
//	Node* p = (Node*)malloc(sizeof(Node));
//	p->charactor = c;
//	p->next = L->next;
//	L->next = p;
//}
//
//LinkList reserv(Node* head)
//{
//	Node* x1 = NULL;
//	Node* x2 = head->next;
//	Node* x3 = NULL;
//	while (x2)
//	{
//		x3 = x2->next;
//		x2->next = x1;
//		x1 = x2;
//		x2 = x3;
//	}
//	head->next = x1;
//	return head;
//}
//
//void Compare(Node* L, Node* r)
//{
//	Node* p = L->next, *p1 = r->next;
//	int flag = 1;
//	while (p&&p1)
//	{
//		if (p->charactor != p1->charactor)
//		{
//			flag = 0;
//			break; // 不相等  
//		}
//		p = p->next;
//		p1 = p1->next;
//	}
//	if (flag == 1)
//	{
//		printf("reserve\n");
//	}
//	else
//	{
//		printf("not reserve\n");
//	}
//}
//
//void destroy(Node *L)
//{
//	Node* p = L->next;
//	while (p)
//	{
//		Node* q = p->next;
//		free (p);
//		p = q;
//	}
//	L->next = NULL;
//	free(L);
//	L = NULL;
//}
//
//int main()
//{
//	int exit;
//	printf("如若退出，请输入0，不退出，请输入1");
//	scanf("%d", &exit);
//	while (exit)
//	{
//		Node* L = Creat();
//		Node* X = Creat();
//		int n;//序列个数
//		printf("请输入字符串长度");
//		scanf("%d", &n);
//		printf("请输入字符串内容");
//		for (int j = 0; j < n; j++)
//		{
//			char c;
//			scanf(" %c", &c);//跳过换行符
//			Insert(L,c);
//		}
//		X=reserv(L);
//		Compare(L, X);
//		destroy(L);
//		destroy(X);
//		printf("如若退出，请输入0，不退出，请输入1");
//		scanf("%d", &exit);
//	}
//	return 0;
//}