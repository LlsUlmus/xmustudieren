#include "fs3399_gpio.h"
#include "fs3399_timer.h"

//按键轮询检测：key1->beep   
int main()
{
	//灯的状态标志位
	int beep_flag = 1;

	//设置GPIO1_B2(key1) 为输入模式
	GPIO1->SWPORTA_DDR |= (~(0x1 << (1*8 + 2)));
	
	//设置GPIO1_C7(beep)为输出模式
	GPIO1->SWPORTA_DDR |= (0x1 << 23);

	while(1)
	{
		//轮询检测KEY1状态
		if (0 == (GPIO1->EXT_PORTA & (0x1 << (1*8 + 2)))) 			//判断  GPIO1_B2     KEY1  是否按下？
		{
			fs_delay_ms(100);	//软件消抖
			if (0 == (GPIO1->EXT_PORTA & (0x1 << (1*8 + 2)))) 		//判断  GPIO1_B2     KEY1  是否按下？    确定key1确实按下
			{		
				if (1 == beep_flag) 
				{
					GPIO1->SWPORTA_DR |= (0x1 << 23);		//设置GPIO1_C7 输出高电平（蜂鸣器响）
					beep_flag = 0;
				} 
				else 
				{
					GPIO1->SWPORTA_DR &= (~(0x1 << 23));	//设置GPIO1_C7 输出低电平（蜂鸣器不响）
					beep_flag = 1;
				}
			}
		}
	}
}
