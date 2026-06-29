/*
 * myshell.h
 *
 * 头文件：保存常量、数据结构和函数声明。
 * 本 shell 程序在 Linux 平台下编译和运行。
 */

#ifndef MYSHELL_H
#define MYSHELL_H

#include <limits.h>

/* 一行命令的最大长度 */
#define MAX_LINE 1024

/* 参数（含程序名）最大个数 */
#define MAX_ARGS 128

/*
 * Command 结构体用来保存一次解析后的命令信息：
 * - argv       : 程序名和参数列表（以 NULL 结尾，可直接传给 execvp）
 * - input_file : 输入重定向文件名（如果没有则为 NULL）
 * - output_file: 输出重定向文件名（如果没有则为 NULL）
 * - append     : 输出重定向是否追加（1 表示使用 >>，0 表示使用 >）
 * - background : 是否后台执行（1 表示命令行末尾带有 &）
 */
struct Command {
    char *argv[MAX_ARGS];
    char *input_file;
    char *output_file;
    int append;
    int background;
};

/* 打印提示符：包含当前工作目录 */
void print_prompt(void);

/* 去掉字符串首尾的空白字符 */
void trim_whitespace(char *s);

/*
 * 解析一行命令到 Command 结构体中。
 * 返回值：
 *   0  : 成功解析
 *  -1  : 空命令行或解析失败
 */
int parse_command(char *line, struct Command *cmd);

/*
 * 处理内部命令。
 * 返回值：
 *   1  : 该命令是内部命令并已执行
 *   0  : 不是内部命令，交由外部程序处理
 *  -1  : 是内部命令并请求退出 shell（quit）
 */
int handle_builtin(struct Command *cmd);

/*
 * 执行外部命令：处理 I/O 重定向和后台执行。
 */
void execute_command(struct Command *cmd);

/*
 * 打印错误信息（简单封装，便于统一风格）。
 */
void print_error(const char *msg);

#endif /* MYSHELL_H */




