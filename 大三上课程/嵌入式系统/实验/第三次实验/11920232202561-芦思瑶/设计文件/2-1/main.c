#include "fs3399_led.h"  // 包含LED控制相关声明
#include "fs3399_timer.h"// 复用“03-fs_beep”工程的定时器基础定义

/**
  * @brief  主函数：初始化LED后，循环点亮LED1-LED3（每个亮100ms后熄灭）
  * @param  无
  * @retval 0（理论不返回）
  */
int main(void)
{
    // 1. LED初始化（调用fs3399_led.c中的驱动函数）
    FsLedInit();

    // 2. 串口打印实验信息（若工程支持串口，可复用“03-fs_beep”的串口函数）
    // printf("FS3399 LED Test (01-fs_led)\r\n");

    // 3. 主循环：LED1→LED2→LED3循环亮灭（延时100ms，复用汇编实现的fs_delay_ms）
    while (1)
    {
        // LED1：亮100ms → 灭100ms
        FsLedOn(1);
        fs_delay_ms(100);
        FsLedOff(1);
        fs_delay_ms(100);

        // LED2：亮100ms → 灭100ms
        FsLedOn(2);
        fs_delay_ms(100);
        FsLedOff(2);
        fs_delay_ms(100);

        // LED3：亮100ms → 灭100ms
        FsLedOn(3);
        fs_delay_ms(100);
        FsLedOff(3);
        fs_delay_ms(100);
    }

    return 0;  //  unreachable
}