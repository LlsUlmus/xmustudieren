//#include "stdio.h"
//#include "stdlib.h"
//
//void Split(int a[], int n, int* l, int* r) {
//    int mid = n / 2;
//    for (*l = 0; *l <= mid; (*l)++) {
//        if (a[*l] == a[mid])
//            break;
//    }
//    for (*r = mid + 1; *r < n; (*r)++) {
//        if (a[*r] != a[mid])
//            break;
//    }
//}
//
//void getMaxNum(int* num, int* maxnum, int a[], int n) {
//    int l, r, s;
//    int mid = n / 2;
//    Split(a, n, &l, &r);
//    s = r - l;
//    if (s > *maxnum) {
//        *num = a[mid];
//        *maxnum = s;
//    }
//    else if (s == *maxnum) {
//        if (*num > a[mid]) {
//            *num = a[mid];
//        }
//    }
//    if (l + 1 > *maxnum) {
//        getMaxNum(num, maxnum, a, l + 1);
//    }
//    if (n - r > *maxnum) {
//        getMaxNum(num, maxnum, a + r, n - r);
//    }
//}
//
//int main() {
//    int n;
//    printf("请输入多重集元素的个数: ");
//    scanf("%d", &n);
//    int* a = (int*)malloc(n * sizeof(int));
//    if (a == NULL) {
//        printf("内存分配失败\n");
//        return 1;
//    }
//    printf("请输入 %d 个自然数: ", n);
//    for (int i = 0; i < n; i++) {
//        scanf("%d", &a[i]);
//    }
//
//    int num = 0, maxnum = 0;
//    getMaxNum(&num, &maxnum, a, n);
//
//    printf("众数是: %d，其重数是: %d\n", num, maxnum);
//
//    free(a);
//    return 0;
//}