#include <stdio.h>

#define N 100 // ����һ���㹻���N���ɸ���ʵ���������

int Sum(int n) {
    int a[N][N];
    for (int i = 0; i <= n; i++) {
        for (int j = 0; j <= n; j++) {
            a[i][j] = 0;
            if (i == j)
                a[i][j] = 1;
        }
    }
    for (int i = 1; i <= n; i++) {
        for (int j = 1; j <= n; j++) {
            if (i >= j) {
                a[i][j] = j * a[i - 1][j] + a[i - 1][j - 1];
            }
        }
    }
    int sum = 0;
    for (int i = 1; i <= n; i++) {
        sum += a[n][i];
    }
    return sum;
}

int main() {
    int n;
    printf("������������n: ");
    scanf("%d", &n);
    int result = Sum(n);
    printf("%d��Ԫ�صļ��Ͽ��Ի���Ϊ %d����ͬ�ķǿ��Ӽ�\n", n, result);
    return 0;
}