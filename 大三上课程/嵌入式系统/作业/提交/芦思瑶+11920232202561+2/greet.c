#include <stdio.h>
#include <string.h>
#include <stdlib.h>

int display1(char* string);
int display2(char* string);

int main(int argc, char** argv) {
    char string[] = "Embedded Linux";
    display1(string);
    display2(string);
    return 0;
}

int display1(char* string) {
    printf("The original string is %s\n", string);
    return 0;
}

int display2(char* string) {
    char* string2;
    int size, i;
    size = strlen(string);
    string2 = (char*)malloc(size + 1);

    // 修正1：正确倒序索引（size-1-i）
    for (i = 0; i < size; i++) {
        string2[size - 1 - i] = string[i];
    }
    // 修正2：正确设置结束符（\0+下标size）
    string2[size] = '\0';

    printf("The string afterward is %s\n", string2);
    free(string2);
    return 0;
}