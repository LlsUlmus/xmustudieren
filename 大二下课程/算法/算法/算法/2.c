#include <stdio.h>

#define N 100 // 定义一个足够大的N，可根据实际需求调整

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
    printf("请输入正整数n: ");
    scanf("%d", &n);
    int result = Sum(n);
    printf("%d个元素的集合可以划分为 %d个不同的非空子集\n", n, result);
    return 0;
}