#include "gpio.h"
#include "Remotelnfrared.h"

// GPIO初始化结构体（全局，复用自13_IR_Receive工程）
GPIO_InitTypeDef GPIO_InitStruct = { 0 };

/**
  * @brief  GPIO初始化：红外接收引脚（中断）+LED引脚（输出），直接复用头文件宏定义
  * @param  无
  * @retval 无
  */
void MX_GPIO_Init(void)
{
    // -------------------------- 红外接收引脚配置（PF15，来自13_IR_Receive，复用gpio.h宏定义）--------------------------
    __HAL_RCC_GPIOF_CLK_ENABLE(); // 使能PF端口时钟（复用gpio.h中IR_RX_CLK_EN()逻辑）
    GPIO_InitStruct.Pin = GPIO_PIN_15; // 红外接收引脚PF15（复用gpio.h中IR_RX_PIN）
    GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING_FALLING; // 上升沿+下降沿中断
    GPIO_InitStruct.Pull = GPIO_NOPULL;
    HAL_GPIO_Init(GPIOF, &GPIO_InitStruct);

    // 配置红外中断优先级（EXTI15_10_IRQn），复用13_IR_Receive工程配置
    HAL_NVIC_SetPriority(EXTI15_10_IRQn, 2, 2);
    HAL_NVIC_EnableIRQ(EXTI15_10_IRQn);

    // -------------------------- LED引脚配置（来自1_Led工程，复用gpio.h宏定义）--------------------------
    // 使能LED对应GPIO时钟（复用gpio.h中LED1-LED4_CLK_EN()逻辑）
    __HAL_RCC_GPIOF_CLK_ENABLE(); // LED1（PF10）
    __HAL_RCC_GPIOC_CLK_ENABLE(); // LED2（PC0）
    __HAL_RCC_GPIOB_CLK_ENABLE(); // LED3（PB15）
    __HAL_RCC_GPIOH_CLK_ENABLE(); // LED4（PH15）

    // LED1（PF10）：推挽输出，复用gpio.h中LED1_PIN/PORT定义
    GPIO_InitStruct.Pin = GPIO_PIN_10;
    GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
    GPIO_InitStruct.Pull = GPIO_NOPULL;
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
    HAL_GPIO_Init(GPIOF, &GPIO_InitStruct);

    // LED2（PC0）：同LED1配置，复用gpio.h中LED2_PIN/PORT定义
    GPIO_InitStruct.Pin = GPIO_PIN_0;
    HAL_GPIO_Init(GPIOC, &GPIO_InitStruct);

    // LED3（PB15）：同LED1配置，复用gpio.h中LED3_PIN/PORT定义
    GPIO_InitStruct.Pin = GPIO_PIN_15;
    HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

    // LED4（PH15）：同LED1配置，复用gpio.h中LED4_PIN/PORT定义
    GPIO_InitStruct.Pin = GPIO_PIN_15;
    HAL_GPIO_Init(GPIOH, &GPIO_InitStruct);

    // 初始化LED为熄灭状态（高电平熄灭，复用1_Led工程逻辑）
    HAL_GPIO_WritePin(GPIOF, GPIO_PIN_10, GPIO_PIN_SET);
    HAL_GPIO_WritePin(GPIOC, GPIO_PIN_0, GPIO_PIN_SET);
    HAL_GPIO_WritePin(GPIOB, GPIO_PIN_15, GPIO_PIN_SET);
    HAL_GPIO_WritePin(GPIOH, GPIO_PIN_15, GPIO_PIN_SET);
}

/**
  * @brief  红外接收中断回调函数，转发给红外解码库（复用13_IR_Receive工程逻辑）
  * @param  GPIO_Pin：触发中断的引脚
  * @retval 无
  */
void HAL_GPIO_EXTI_Callback(uint16_t GPIO_Pin)
{
    if (GPIO_Pin == GPIO_PIN_15) // 红外接收引脚PF15，复用gpio.h中IR_RX_PIN定义
    {
        Remote_Infrared_KEY_ISR(); // 调用红外解码库中断处理函数（来自Remotelnfrared.c）
    }
}