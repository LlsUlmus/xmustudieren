#ifndef __FS3399_LED_H__
#define __FS3399_LED_H__

#include "fs3399_gpio.h"  // 复用“03-fs_beep”工程的GPIO基础定义

// 1. 时钟控制寄存器（与“03-fs_beep”工程的PMUCRU_BASE地址一致）
#define PMUCRU_BASE          0xFF750000
#define PMUCRU_CLKGATE_CON1  (*((volatile unsigned int *)(PMUCRU_BASE + 0x0104)))  // GPIO时钟使能寄存器

// 2. LED引脚定义（FS3399开发板硬件对应关系）
// LED1：GPIO4_C6（寄存器第22位），时钟使能位为PMUCRU_CLKGATE_CON1第2位
#define LED1_GPIO           GPIO4
#define LED1_PIN            (0x1 << 22)
#define LED1_CLK_EN         (PMUCRU_CLKGATE_CON1 |= (0x1 << 2))

// LED2：GPIO0_A2（寄存器第2位），时钟使能位为PMUCRU_CLKGATE_CON1第0位
#define LED2_GPIO           GPIO0
#define LED2_PIN            (0x1 << 2)
#define LED2_CLK_EN         (PMUCRU_CLKGATE_CON1 |= (0x1 << 0))

// LED3：GPIO0_B4（寄存器第12位），复用GPIO0时钟（与LED2共享）
#define LED3_GPIO           GPIO0
#define LED3_PIN            (0x1 << 12)

// 3. 函数声明（LED初始化、点亮、熄灭）
int FsLedInit(void);    // LED引脚初始化（配置为输出模式）
int FsLedOn(int led);   // 点亮指定LED（1=LED1，2=LED2，3=LED3）
int FsLedOff(int led);  // 熄灭指定LED（1=LED1，2=LED2，3=LED3）
void fs_delay_ms(uint32_t ms);  // 汇编延时函数（声明，定义在start.S中）

#endif // __FS3399_LED_H__