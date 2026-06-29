#include "fs3399_led.h"

/**
  * @brief  LED初始化：使能时钟+配置输出模式+初始熄灭
  * @param  无
  * @retval 0：初始化成功
  */
int FsLedInit(void)
{
    // 1. 使能LED对应GPIO时钟
    LED1_CLK_EN;  // 使能GPIO4时钟
    LED2_CLK_EN;  // 使能GPIO0时钟（LED2、LED3共享）

    // 2. 配置LED1为输出模式（SWPORTA_DDR：1=输出，0=输入）
    LED1_GPIO->SWPORTA_DDR |= LED1_PIN;
    // 配置LED2为输出模式
    LED2_GPIO->SWPORTA_DDR |= LED2_PIN;
    // 配置LED3为输出模式
    LED3_GPIO->SWPORTA_DDR |= LED3_PIN;

    // 3. 初始状态：所有LED熄灭（SWPORTA_DR：1=高电平熄灭，0=低电平点亮）
    FsLedOff(1);
    FsLedOff(2);
    FsLedOff(3);

    return 0;
}

/**
  * @brief  点亮指定LED
  * @param  led：LED编号（1=LED1，2=LED2，3=LED3）
  * @retval 0：成功；-1：无效编号
  */
int FsLedOn(int led)
{
    switch (led)
    {
    case 1:
        LED1_GPIO->SWPORTA_DR &= ~LED1_PIN;  // 拉低LED1引脚（点亮）
        break;
    case 2:
        LED2_GPIO->SWPORTA_DR &= ~LED2_PIN;  // 拉低LED2引脚（点亮）
        break;
    case 3:
        LED3_GPIO->SWPORTA_DR &= ~LED3_PIN;  // 拉低LED3引脚（点亮）
        break;
    default:
        return -1;  // 无效LED编号
    }
    return 0;
}

/**
  * @brief  熄灭指定LED
  * @param  led：LED编号（1=LED1，2=LED2，3=LED3）
  * @retval 0：成功；-1：无效编号
  */
int FsLedOff(int led)
{
    switch (led)
    {
    case 1:
        LED1_GPIO->SWPORTA_DR |= LED1_PIN;  // 拉高LED1引脚（熄灭）
        break;
    case 2:
        LED2_GPIO->SWPORTA_DR |= LED2_PIN;  // 拉高LED2引脚（熄灭）
        break;
    case 3:
        LED3_GPIO->SWPORTA_DR |= LED3_PIN;  // 拉高LED3引脚（熄灭）
        break;
    default:
        return -1;  // 无效LED编号
    }
    return 0;
}