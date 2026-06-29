# CORTEX-A53 PERI DRIVER CODE
# VERSION 3.0
# AUTHOR www.hqyj.com
# MODIFY DATE 2024.10.22
# 适配01-fs_led工程，基于03-fs_beep修改

# 1. 交叉编译器配置（与“03-fs_beep”工程完全一致）
CROSS_COMPILE   := ~/toolchain/6.4-aarch64/bin/aarch64-linux-

# 2. 编译选项：调试模式、无标准库、指定头文件路径（复用“03-fs_beep”配置）
CFLAGS += -g -O0 -fno-builtin -nostdinc -I./common/include -I./include

# 3. 工具链定义（复用“03-fs_beep”）
CC = 		$(CROSS_COMPILE)gcc
LD = 		$(CROSS_COMPILE)ld
OBJCOPY = 	$(CROSS_COMPILE)objcopy
OBJDUMP = 	$(CROSS_COMPILE)objdump
OUTPUT_DIR = ./output  # 输出目录（bin/elf/dis文件）

# 4. 目标文件名称：修改为“fs_led”，区别于“03-fs_beep”的“fs_beep”
NAME 	= $(OUTPUT_DIR)/fs_led

# 5. 源文件路径：复用“03-fs_beep”的汇编文件（start.S）和通用C文件，新增LED驱动文件
OBJSss := $(wildcard ./start/*.S) $(wildcard ./common/src/*.S) \
		$(wildcard ./source/*.S) $(wildcard ./*.S) \
		$(wildcard ./start/*.c) $(wildcard ./common/src/*.c) \
		$(wildcard ./source/*.c) $(wildcard ./*.c) \
		./fs3399_led.c  # 新增LED驱动文件（核心修改点）
OBJSs  := $(patsubst %.S,%.o,$(OBJSss))  # 汇编文件转目标文件
OBJS   := $(patsubst %.c,%.o,$(OBJSs))   # C文件转目标文件

# 6. 编译目标：链接→生成bin→生成反汇编（复用“03-fs_beep”逻辑）
all:$(OBJS)
	@ mkdir -p $(OUTPUT_DIR)  # 若输出目录不存在则创建
	@ echo "  LD   Linking  $(NAME).elf"
	@ $(LD) -Tmap.lds -o $(NAME).elf $^  # 用map.lds链接脚本生成elf文件
	@ echo "  OBJCOPY Objcopying  $(NAME).bin"
	@ $(OBJCOPY) -O binary -S $(NAME).elf $(NAME).bin  # 转换为二进制文件（下载用）
	@ echo "  OBJDUMP Objdumping  $(NAME).dis"
	@ $(OBJDUMP) -D $(NAME).elf > $(NAME).dis  # 生成反汇编文件（调试用）

# 7. 汇编文件编译规则（复用“03-fs_beep”）
%.o : %.S
	@ echo "  AS   $@"
	@ $(CC) -o $@ $< -c $(CFLAGS)

# 8. C文件编译规则（复用“03-fs_beep”）
%.o : %.c
	@ echo "  CC   $@"
	@ $(CC) -o $@ $< -c $(CFLAGS)

# 9. 清理目标：删除目标文件和输出文件（复用“03-fs_beep”）
distclean clean:
	@ echo "  CLEAN  complete."
	@ rm -rf $(OBJS) $(NAME).* main.o $(OUTPUT_DIR)  # 清理所有编译产物

# 10. 安装：将bin文件复制到共享目录（供开发板下载，复用“03-fs_beep”）
install:
	@ echo "  INSTALL  $(NAME).bin to /mnt/hgfs/share"
	cp $(NAME).bin /mnt/hgfs/share