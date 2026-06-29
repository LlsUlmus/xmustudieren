CC = gcc          # 自定义变量：编译器
TARGET = hello    # 自定义变量：目标文件名
SRCS = hello.c    # 自定义变量：源文件
HDRS = hello.h    # 自定义变量：头文件

$(TARGET): $(SRCS) $(HDRS)
	$(CC) $(SRCS) -o $(TARGET)  # 变量替换，简化修改
clean:
	rm -f $(TARGET)