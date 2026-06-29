#define _CRT_SECURE_NO_WARNINGS 1
#include <stdio.h>
#define MAX 100000

void search(int A[], int x, int left, int right)
{
	int mid = (left + right) / 2;
	if (x == A[mid])
	{
		printf("%d is Found at %d\n", x, mid);
		return;
	}
	else if (x > A[mid])
	{
		left = mid + 1;
		search(A, x, left, right);
	}
	else
	{
		right = mid - 1;
		search(A, x, left, right);
	}
}

int main()
{
	int A[MAX] = { 0 };
	int n, x = 0;
	printf("请输入数组元素的数目和要查找的值\n");
	scanf("%d %d", &n, &x);
	int left = 0;
	int right = n - 1;
	printf("请输入数组元素\n");
	for (int i = 0; i < n; i++)
	{
		scanf("%d", &A[i]);
	}
	search(A, x, left, right);//折半查找
	return 0;
}


