#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/wait.h>
#include <dirent.h>
#include <limits.h>
#include <pwd.h>

#include "myshell.h"

/* 声明由 libc 提供的环境变量表 */
extern char **environ;

/* 打印错误信息的简单封装 */
void print_error(const char *msg) {
    perror(msg);
}

/* 去掉字符串首尾空白（空格和制表符） */
void trim_whitespace(char *s) {
    if (s == NULL) {
        return;
    }

    char *start = s;
    while (*start == ' ' || *start == '\t') {
        start++;
    }

    char *end = s + strlen(s);
    while (end > start && (end[-1] == ' ' || end[-1] == '\t' || end[-1] == '\n')) {
        end--;
    }
    *end = '\0';

    if (start != s) {
        memmove(s, start, end - start + 1);
    }
}

/* 打印命令提示符：格式为 "当前工作目录> " */
void print_prompt(void) {
    char cwd[PATH_MAX];
    if (getcwd(cwd, sizeof(cwd)) != NULL) {
        printf("%s> ", cwd);
    } else {
        printf("myshell> ");
    }
    fflush(stdout);
}

/*
 * 解析一行命令。
 * 约定：
 * - 所有 Token 已使用空白分隔（空格或制表符）
 * - 支持的特殊符号：<  >  >>  &
 */
int parse_command(char *line, struct Command *cmd) {
    int argc = 0;

    /* 初始化 Command 结构体 */
    memset(cmd->argv, 0, sizeof(cmd->argv));
    cmd->input_file = NULL;
    cmd->output_file = NULL;
    cmd->append = 0;
    cmd->background = 0;

    trim_whitespace(line);
    if (line[0] == '\0') {
        return -1;  /* 空命令行 */
    }

    /* 先用 strtok 按空白分割所有 token */
    char *tokens[MAX_ARGS] = {0};
    int token_count = 0;

    char *token = strtok(line, " \t");
    while (token != NULL && token_count < MAX_ARGS - 1) {
        tokens[token_count++] = token;
        token = strtok(NULL, " \t");
    }

    /* 再顺序扫描 token，识别 <、>、>>、&，剩下的放到 argv */
    for (int i = 0; i < token_count; i++) {
        char *t = tokens[i];

        if (strcmp(t, "<") == 0) {
            if (i + 1 < token_count) {
                cmd->input_file = tokens[++i];
            } else {
                fprintf(stderr, "myshell: missing input file for '<'\n");
                return -1;
            }
        } else if (strcmp(t, ">") == 0 || strcmp(t, ">>") == 0) {
            cmd->append = (strcmp(t, ">>") == 0);
            if (i + 1 < token_count) {
                cmd->output_file = tokens[++i];
            } else {
                fprintf(stderr, "myshell: missing output file for '>' or '>>'\n");
                return -1;
            }
        } else if (strcmp(t, "&") == 0) {
            /* & 约定出现在命令末尾，如果在中间出现也视为后台标志 */
            cmd->background = 1;
        } else {
            /* 普通参数 */
            if (argc < MAX_ARGS - 1) {
                cmd->argv[argc++] = t;
            } else {
                fprintf(stderr, "myshell: too many arguments\n");
                break;
            }
        }
    }

    cmd->argv[argc] = NULL;

    if (argc == 0) {
        /* 例如只有重定向符号而没有命令 */
        return -1;
    }

    return 0;
}

/* 展开 ~ 为 home 目录路径 */
static void expand_tilde(char *path, size_t path_size) {
    if (path[0] == '~') {
        const char *home = getenv("HOME");
        if (home == NULL) {
            /* 如果 HOME 环境变量不存在，尝试从 passwd 获取 */
            struct passwd *pw = getpwuid(getuid());
            if (pw != NULL && pw->pw_dir != NULL) {
                home = pw->pw_dir;
            }
        }
        
        if (home != NULL) {
            char expanded[PATH_MAX];
            if (path[1] == '\0' || path[1] == '/') {
                /* ~ 或 ~/xxx 的情况 */
                snprintf(expanded, sizeof(expanded), "%s%s", home, path + 1);
            } else {
                /* ~username 的情况（简化处理，只支持当前用户） */
                snprintf(expanded, sizeof(expanded), "%s%s", home, path + 1);
            }
            strncpy(path, expanded, path_size - 1);
            path[path_size - 1] = '\0';
        }
    }
}

/* 内部命令：cd */
static int builtin_cd(struct Command *cmd) {
    if (cmd->argv[1] == NULL) {
        /* 若无参数，显示当前目录 */
        char cwd[PATH_MAX];
        if (getcwd(cwd, sizeof(cwd)) != NULL) {
            printf("%s\n", cwd);
        } else {
            print_error("cd");
        }
        return 1;
    }

    /* 展开 ~ 符号 */
    char path[PATH_MAX];
    strncpy(path, cmd->argv[1], sizeof(path) - 1);
    path[sizeof(path) - 1] = '\0';
    expand_tilde(path, sizeof(path));

    if (chdir(path) != 0) {
        print_error("cd");
        return 1;
    }

    /* 同步更新 PWD 环境变量 */
    char cwd[PATH_MAX];
    if (getcwd(cwd, sizeof(cwd)) != NULL) {
        setenv("PWD", cwd, 1);
    }
    return 1;
}

/* 内部命令：clr —— 清屏 */
static int builtin_clr(struct Command *cmd) {
    (void)cmd; /* 未使用参数 */
    /* 使用 ANSI 转义序列清屏并把光标移到左上角 */
    printf("\033[H\033[J");
    fflush(stdout);
    return 1;
}

/* 内部命令：dir <directory> —— 列出目录内容 */
static int builtin_dir(struct Command *cmd) {
    char dir_path[PATH_MAX];
    const char *dir_name = ".";
    
    if (cmd->argv[1] != NULL) {
        strncpy(dir_path, cmd->argv[1], sizeof(dir_path) - 1);
        dir_path[sizeof(dir_path) - 1] = '\0';
        expand_tilde(dir_path, sizeof(dir_path));
        dir_name = dir_path;
    }

    /* 如果指定了输出重定向，设置输出重定向 */
    int original_stdout = -1;
    int fd_out = -1;
    if (cmd->output_file != NULL) {
        original_stdout = dup(STDOUT_FILENO);
        if (original_stdout < 0) {
            print_error("dup");
            return 1;
        }

        int flags = O_WRONLY | O_CREAT;
        if (cmd->append) {
            flags |= O_APPEND;
        } else {
            flags |= O_TRUNC;
        }

        fd_out = open(cmd->output_file, flags, 0644);
        if (fd_out < 0) {
            print_error("open output");
            close(original_stdout);
            return 1;
        }

        if (dup2(fd_out, STDOUT_FILENO) < 0) {
            print_error("dup2 output");
            close(original_stdout);
            close(fd_out);
            return 1;
        }
        close(fd_out);
    }

    DIR *dir = opendir(dir_name);
    if (!dir) {
        print_error("dir");
        if (original_stdout >= 0) {
            dup2(original_stdout, STDOUT_FILENO);
            close(original_stdout);
        }
        return 1;
    }

    struct dirent *entry;
    while ((entry = readdir(dir)) != NULL) {
        printf("%s\n", entry->d_name);
    }
    closedir(dir);

    /* 恢复原始 stdout */
    if (original_stdout >= 0) {
        fflush(stdout);
        dup2(original_stdout, STDOUT_FILENO);
        close(original_stdout);
    }

    return 1;
}

/* 内部命令：environ —— 列出所有环境变量 */
static int builtin_environ(struct Command *cmd) {
    (void)cmd;
    for (char **env = environ; *env != NULL; env++) {
        printf("%s\n", *env);
    }
    return 1;
}

/* 内部命令：echo <comment> —— 输出参数并换行 */
static int builtin_echo(struct Command *cmd) {
    /* 如果指定了输出重定向，设置输出重定向 */
    int original_stdout = -1;
    int fd_out = -1;
    if (cmd->output_file != NULL) {
        original_stdout = dup(STDOUT_FILENO);
        if (original_stdout < 0) {
            print_error("dup");
            return 1;
        }

        int flags = O_WRONLY | O_CREAT;
        if (cmd->append) {
            flags |= O_APPEND;
        } else {
            flags |= O_TRUNC;
        }

        fd_out = open(cmd->output_file, flags, 0644);
        if (fd_out < 0) {
            print_error("open output");
            close(original_stdout);
            return 1;
        }

        if (dup2(fd_out, STDOUT_FILENO) < 0) {
            print_error("dup2 output");
            close(original_stdout);
            close(fd_out);
            return 1;
        }
        close(fd_out);
    }

    /* 说明：经过解析后，多重空格会被压缩为一个，这是可接受的 */
    int i = 1;
    while (cmd->argv[i] != NULL) {
        printf("%s", cmd->argv[i]);
        if (cmd->argv[i + 1] != NULL) {
            putchar(' ');
        }
        i++;
    }
    putchar('\n');

    /* 恢复原始 stdout */
    if (original_stdout >= 0) {
        fflush(stdout);
        dup2(original_stdout, STDOUT_FILENO);
        close(original_stdout);
    }

    return 1;
}

/* 内部命令：help —— 使用 more 显示 readme */
static int builtin_help(struct Command *cmd) {
    (void)cmd;
    /* 这里假设 readme 文件在当前工作目录下 */
    pid_t pid = fork();
    if (pid < 0) {
        print_error("fork");
        return 1;
    } else if (pid == 0) {
        char *argv[] = {"more", "readme", NULL};
        execvp("more", argv);
        print_error("execvp");
        _exit(127);
    } else {
        int status;
        (void)waitpid(pid, &status, 0);
        return 1;
    }
}

/* 内部命令：pause —— 等待用户按回车 */
static int builtin_pause(struct Command *cmd) {
    (void)cmd;
    printf("Press ENTER to continue...");
    fflush(stdout);

    int c;
    /* 读到换行符为止 */
    while ((c = getchar()) != '\n' && c != EOF) {
        /* 丢弃输入 */
    }
    return 1;
}

/*
 * 处理内部命令。
 * 返回值：
 *   1  : 是内部命令并已执行
 *   0  : 不是内部命令
 *  -1  : 是内部命令并请求退出 shell（quit）
 */
int handle_builtin(struct Command *cmd) {
    if (cmd->argv[0] == NULL) {
        return 0;
    }

    if (strcmp(cmd->argv[0], "cd") == 0) {
        return builtin_cd(cmd);
    } else if (strcmp(cmd->argv[0], "clr") == 0) {
        return builtin_clr(cmd);
    } else if (strcmp(cmd->argv[0], "dir") == 0) {
        return builtin_dir(cmd);
    } else if (strcmp(cmd->argv[0], "environ") == 0) {
        return builtin_environ(cmd);
    } else if (strcmp(cmd->argv[0], "echo") == 0) {
        return builtin_echo(cmd);
    } else if (strcmp(cmd->argv[0], "help") == 0) {
        return builtin_help(cmd);
    } else if (strcmp(cmd->argv[0], "pause") == 0) {
        return builtin_pause(cmd);
    } else if (strcmp(cmd->argv[0], "quit") == 0) {
        /* 返回 -1 表示请求退出 shell */
        return -1;
    }

    return 0; /* 不是内部命令 */
}

/*
 * 执行外部命令：
 * - 支持输入重定向：< inputfile
 * - 支持输出重定向：> outputfile 或 >> outputfile
 * - 支持后台执行：命令末尾出现 &
 */
void execute_command(struct Command *cmd) {
    pid_t pid = fork();
    if (pid < 0) {
        print_error("fork");
        return;
    } else if (pid == 0) {
        /* 子进程：设置 I/O 重定向并执行命令 */
        if (cmd->input_file != NULL) {
            int fd_in = open(cmd->input_file, O_RDONLY);
            if (fd_in < 0) {
                print_error("open input");
                _exit(1);
            }
            if (dup2(fd_in, STDIN_FILENO) < 0) {
                print_error("dup2 input");
                _exit(1);
            }
            close(fd_in);
        }

        if (cmd->output_file != NULL) {
            int flags = O_WRONLY | O_CREAT;
            if (cmd->append) {
                flags |= O_APPEND;
            } else {
                flags |= O_TRUNC;
            }

            int fd_out = open(cmd->output_file, flags, 0644);
            if (fd_out < 0) {
                print_error("open output");
                _exit(1);
            }
            if (dup2(fd_out, STDOUT_FILENO) < 0) {
                print_error("dup2 output");
                _exit(1);
            }
            close(fd_out);
        }

        /* 执行外部程序 */
        execvp(cmd->argv[0], cmd->argv);
        print_error("execvp");
        _exit(127);
    } else {
        /* 父进程 */
        if (cmd->background) {
            /* 后台执行：打印 PID，不等待 */
            printf("[background pid %d]\n", pid);
        } else {
            int status;
            (void)waitpid(pid, &status, 0);
        }
    }
}




