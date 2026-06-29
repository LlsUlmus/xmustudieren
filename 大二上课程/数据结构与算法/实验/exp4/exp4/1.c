#define _CRT_SECURE_NO_WARNINGS 1
#include <stdio.h>
#include <malloc.h>
#define MAX_VERTICES 100

typedef struct graph
{
	char vex[MAX_VERTICES];//顶点数组
	int A[MAX_VERTICES][MAX_VERTICES];//邻接矩阵
	int n;//顶点数
	int edgenum;//边数
}MGraph;

void CreatGraph(MGraph* G);
int FindVex(MGraph G, char x);
void PrintGraph(MGraph G);

int main()
{
	MGraph G;
	CreatGraph(&G);
	PrintGraph(G);
	return 0;
}

int FindVex(MGraph G, char x)
{
	for (int i = 0; i < G.n; i++)
	{
		if (G.vex[i] == x)
		{
			return i;
		}
	}
	return -1;
}

void CreatGraph(MGraph* G)
{
	printf("请输入顶点数：\n");
	scanf(" %d", &G->n);
	printf("请输入边数：\n");
	scanf(" %d", &G->edgenum);
	printf("请输入顶点信息\n");
	for (int i = 0; i < G->n; i++)
	{
		scanf(" %c", &G->vex[i]);
	}
	//初始化邻接矩阵
	for (int i = 0; i < G->n; i++)
	{
		for (int j = 0; j < G->n; j++)
		{
			G->A[i][j] = 0;
		}
	}
	for (int i = 0; i < G->edgenum; i++)
	{
		char u, v;
		printf("请输入每条边的两个顶点\n");
		scanf(" %c %c", &u, &v);
		int a = FindVex(*G, u);
		int b = FindVex(*G, v);
		if ((a != -1) && (b != -1))
		{
			printf("请输入权值：\n");
			scanf(" %d", &G->A[a][b]);
			G->A[b][a] = G->A[a][b];
			/*G->A[a][b] = 1;
			G->A[b][a] = 1;*/
		}
		else
		{
			printf("输入有误，请重新输入：\n");
			i--;
		}
	}
}

void PrintGraph(MGraph G)
{
	printf("邻接矩阵如下：\n  ");
	for (int i = 0; i < G.n; i++)
	{
		printf("%c ", G.vex[i]);
	}
	printf("\n");
	for (int i = 0; i < G.n; i++)
	{
		printf("%c ", G.vex[i]);
		for (int j = 0; j < G.n; j++)
		{
			printf("%d ", G.A[i][j]);
		}
		printf("\n");
	}
}