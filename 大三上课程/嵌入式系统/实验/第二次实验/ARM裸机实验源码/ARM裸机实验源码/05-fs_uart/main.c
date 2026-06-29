#include "fs3399_uart.h"
#include "common.h"

int main()
{
	char ch;
	char str[] = "FS3399 UART test string !";

	fs_uart_init(115200);		//串口初始化，115200 is baud rate

#if 1	//测试串口发送数据
	//发送字符
	fs_putc('A');
	fs_putc('B');
	fs_putc('C');
	//测试发送字符串
	fs_puts(str);

	//printf函数测试
	printf("\n\r");
	printf("fs3399 test printf function\n\r");

	while (1)
	{}
#endif

	return 0;
}
