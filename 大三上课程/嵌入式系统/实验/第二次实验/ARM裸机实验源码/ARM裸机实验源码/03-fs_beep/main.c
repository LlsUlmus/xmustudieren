#include "fs_beep.h"
#include "fs3399_timer.h"

/*-------------------------MAIN FUNCTION------------------------------*/
/**********************************************************************
 * @brief       Main program body
 * @param[in]   None
 * @return      int
 **********************************************************************/
int main()
{
	//设置GPIO1_C7 为输出模式
	FsBeepInit();

	//打开蜂鸣器
	FsBeepOn();
	
	//延时3s
	fs_delay_s(3);

	//关闭蜂鸣器
	FsBeepOff();

	while(1)
	{}
}
