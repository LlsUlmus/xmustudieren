#define _CRT_SECURE_NO_WARNINGS 1
#include <stdio.h>
#include <stdlib.h>
#define MAXLENGTH 10010

void coloursort(int *num,int n)
{
	int low = 0;//红 低位下标
	int mid = 0;//现位置
	int high = n - 1;//蓝 高位
	for (int mid = 0; mid<= high;)
	{
		if (num[mid] == 0)
		{
			int tmp = num[mid];
			num[mid] = num[low];
			num[low] = tmp;
			low++;
			mid++;//前面部分已经排序 可以++
		}
		else if (num[mid] == 1)
		{
			mid++;
		}
		else
		{
			int tmp = num[mid];
			num[mid] = num[high];
			num[high] = tmp;
			high--;//后方元素没有排序 不能直接++
		}
	}
}


int main()
{
	int n = 0;
	printf("请输入序列的长度\n");
	scanf("%d", &n);
	int num[MAXLENGTH] = {0};
	printf("请输入序列的内容，红为0；白为1；蓝为2\n");
	for (int i = 0; i < n; i++)
	{
		scanf("%d", &num[i]);
	}
	coloursort(num,n);
	for (int i = 0; i < n; i++)
	{
		printf("%d ", num[i]);
	}
	return 0;
}