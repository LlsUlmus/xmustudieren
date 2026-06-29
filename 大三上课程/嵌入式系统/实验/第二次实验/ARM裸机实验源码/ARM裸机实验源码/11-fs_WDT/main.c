#include "fs3399_WDT.h"
#include "fs3399_gpio.h"
#include "fs3399_timer.h"
#include "common.h"
#include "fs3399_uart.h"

//看门狗
int main()
{
	int count = 10;

	//初始化串口
	fs_uart_init(115200);
	//初始化看门狗
	WDT_init();

	while (1)
	{
		count = 10;
		for (count = 10; count >= 0; count--) {
			fs_delay_s(1);
			printf("count down : %d\n\r",count);
			if (0 == count) {
				printf("feed the dog\n\r");
				//喂狗
				WDT0->WDT_CRR = 0x76;
			}
		}
	};

	return 0;
}

