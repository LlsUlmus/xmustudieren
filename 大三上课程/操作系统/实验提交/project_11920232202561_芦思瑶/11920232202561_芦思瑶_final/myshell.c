/*
 * myshell.c
 *
 * 主程序入口。
 * 实现一个简易 shell，支持：
 *  - 内部命令：cd, clr, dir, environ, echo, help, pause, quit
 *  - 外部命令执行
 *  - I/O 重定向：<、>、>>
 *  - 后台执行：&
 *  - 批处理文件：myshell batchfile
 *
 * 编译方式：
 *   在含有 makefile 的目录下执行：make
 * 生成的可执行文件名称为：myshell
 */

#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>   /* setenv */
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <limits.h>

#include "myshell.h"

/*
 * 设置 shell 环境变量：
 *   shell=<pathname>/myshell
 *
 * 其中 <pathname> 为 myshell 可执行文件的绝对路径。
 */
static void setup_shell_env(const char *argv0) {
    char path[PATH_MAX];

    /* 若 argv0 为绝对路径，直接使用 */
    if (argv0[0] == '/') {
        strncpy(path, argv0, sizeof(path) - 1);
        path[sizeof(path) - 1] = '\0';
    } else {
        /* 相对路径：使用 getcwd 拼接 */
        char cwd[PATH_MAX];
        if (getcwd(cwd, sizeof(cwd)) == NULL) {
            /* 退化为简单路径 */
            strncpy(path, argv0, sizeof(path) - 1);
            path[sizeof(path) - 1] = '\0';
        } else {
            snprintf(path, sizeof(path), "%s/%s", cwd, argv0);
        }
    }

    /* 设置 shell 环境变量 */
    setenv("shell", path, 1);
}

/*
 * 处理一个命令行字符串：
 * - 解析到 Command 结构体
 * - 先尝试内部命令
 * - 否则执行外部命令
 * 返回值：
 *  -1 : 收到 quit 命令，请求退出 shell
 *   0 : 正常处理完成，继续
 */
static int process_line(char *line) {
    struct Command cmd;

    if (parse_command(line, &cmd) != 0) {
        /* 空行或解析失败 */
        return 0;
    }

    int builtin_result = handle_builtin(&cmd);
    if (builtin_result == -1) {
        /* quit */
        return -1;
    } else if (builtin_result == 1) {
        /* 已成功执行内部命令 */
        return 0;
    }

    /* 非内部命令，尝试执行外部程序 */
    execute_command(&cmd);
    return 0;
}

/*
 * 批处理模式：
 *   myshell batchfile
 * 从 batchfile 中依次读取命令并执行，直到文件结束或遇到 quit。
 */
static void run_batch_mode(const char *filename) {
    FILE *fp = fopen(filename, "r");
    if (!fp) {
        perror("open batch file");
        return;
    }

    char line[MAX_LINE];
    while (fgets(line, sizeof(line), fp) != NULL) {
        /* 显示从批处理文件读取的命令，便于调试 */
        printf("# %s", line);
        if (process_line(line) == -1) {
            /* quit */
            break;
        }
    }

    fclose(fp);
}

/*
 * 交互模式：
 *   myshell
 * 在终端上循环读取命令并执行。
 */
static void run_interactive_mode(void) {
    char line[MAX_LINE];

    while (1) {
        print_prompt();

        if (fgets(line, sizeof(line), stdin) == NULL) {
            /* EOF（如 Ctrl+D）或读取错误，退出 shell */
            putchar('\n');
            break;
        }

        if (process_line(line) == -1) {
            /* 收到 quit 命令 */
            break;
        }
    }
}

int main(int argc, char *argv[]) {
    /* 设置 shell 环境变量 */
    setup_shell_env(argv[0]);

    if (argc > 2) {
        fprintf(stderr, "Usage: %s [batchfile]\n", argv[0]);
        return EXIT_FAILURE;
    }

    if (argc == 2) {
        /* 批处理模式 */
        run_batch_mode(argv[1]);
    } else {
        /* 交互模式 */
        run_interactive_mode();
    }

    return EXIT_SUCCESS;
}




