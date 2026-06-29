#define _CRT_SECURE_NO_WARNINGS 1
#include <stdio.h>
#include <stdlib.h>
#define KeyType int
#define ElemType int

typedef struct BiTnode
{
	KeyType key;  //关键字域
	//ElemType* otherinfo;  //其它数据项(可以忽略)
	struct BiTnode* Lchild;  //左指针域
	struct BiTnode* Rchild;  //右指针域
} BiTnode, * BiTree;

void Create(BiTree* T);
void InsertBiTree(BiTree* T, KeyType Key);
void inorderPrint(BiTree T);
BiTree Search(BiTree T, KeyType key);
BiTree Delete(BiTree* T, KeyType key);
BiTnode* Findmin(BiTnode* T);

int main()
{
	BiTree T;
	T = NULL;
	Create(&T);//创建并插入
	inorderPrint(T);//中序遍历输出
	printf("请输入你想要查找的关键字\n");
	KeyType key;
	scanf("%d", &key);
	Search(T, key);
	printf("请输入你想要删除的关键字\n");
	scanf("%d", &key);
	Delete(&T, key);
	inorderPrint(T);//中序遍历输出
	return 0;
}


void Create(BiTree* T)
{
	KeyType Key;
	printf("请输入关键字（输入-1结束）\n");
	while (1) {
		scanf("%d", &Key);

		if (Key == -1) {
			break;  // 输入-1结束
		}

		// 插入关键字到树中
		InsertBiTree(T, Key);
	}
}

void InsertBiTree(BiTree* T, KeyType Key)
{
	if (*T == NULL)
	{
		*T = (BiTree)malloc(sizeof(BiTnode));  // 根节点分配内存
		(*T)->key = Key;
		(*T)->Lchild = NULL;  // 左孩子为空
		(*T)->Rchild = NULL;  // 右孩子为空
	}
	else if (Key < (*T)->key)
	{
		InsertBiTree((&(*T)->Lchild), Key);//插入左子树
	}
	else if (Key > (*T)->key)
	{
		InsertBiTree(&((*T)->Rchild), Key);  // 添加条件避免重复插入
	}
	else {
		printf("关键字 %d 已存在。\n", Key);  // 忽略重复
	}
}

void inorderPrint(BiTree T)
{
	if (T != NULL)
	{
		inorderPrint(T->Lchild);
		printf("%d ", T->key);
		inorderPrint(T->Rchild);
	}
}

BiTree Search(BiTree T, KeyType key)
{
	if (T == NULL) {
		printf("Not Found\n");
		return NULL;  // 树为空，查找失败
	}

	if (key == T->key) {
		printf("Found %d\n", key);
		return T;  // 找到目标节点，返回节点指针
	}
	else if (key < T->key) {
		return Search(T->Lchild, key);  // 递归查找左子树
	}
	else {
		return Search(T->Rchild, key);  // 递归查找右子树
	}
}

BiTree Delete(BiTree* T, KeyType key)
{
	if (*T == NULL) {
		printf("Not Found\n");
		return NULL;  // 树为空，查找失败
	}

	if (key < (*T)->key) {
		(*T)->Lchild = Delete(&((*T)->Lchild), key);  // 递归查找左子树
	}
	else if (key > (*T)->key) {
		(*T)->Rchild = Delete(&((*T)->Rchild), key);  // 递归查找右子树
	}
	else {
		if (((*T)->Lchild != NULL) && ((*T)->Rchild == NULL))//只有左子树有叶子节点 那就指向左子树
		{
			BiTnode* tmp = *T;
			*T = (*T)->Lchild;
			free(tmp);
		}
		else if (((*T)->Lchild == NULL) && ((*T)->Rchild != NULL))//只有右子树有
		{
			BiTnode* tmp = *T;
			*T = (*T)->Rchild;
			free(tmp);
		}
		else if (((*T)->Lchild != NULL) && ((*T)->Rchild != NULL))//两个 寻找右子树的最小节点作为根结点 左子树都比它小 右子树都比它大
		{
			BiTnode* tmp = Findmin((*T)->Rchild);
			(*T)->key = tmp->key;
			Delete(&((*T)->Rchild), tmp->key);  // 删除右子树中最小节点
		}
		else//没有叶子节点
		{
			free(*T);
			return NULL;
		}
	}
	return *T;  // 找到目标节点，返回节点指针
}

BiTnode* Findmin(BiTnode* T) {
	while (T->Lchild != NULL) {
		T = T->Lchild;  // 一直向左子树走，直到找到最左下角的节点
	}
	return T;
}
