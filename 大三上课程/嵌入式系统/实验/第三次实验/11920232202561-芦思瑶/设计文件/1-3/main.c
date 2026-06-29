#include "stdio.h"
#include "main.h"
#include "i2c.h"
#include "usart.h"
#include "gpio.h"
#include "zlg72128.h"

// ZLG72128按键寄存器与显示地址定义
#define ZLG_READ_ADDRESS1        0x01    // 普通键值寄存器
#define ZLG_READ_FUNCTION_ADDRESS 0x03   // 功能键值寄存器
#define ZLG_WRITE_ADDRESS1       0x17    // 数码管显示缓冲区首地址（备用）
#define ZLG_WRITE_ADDRESS2       0x16    // 数码管显示缓冲区（备用）
#define countof(a)               (sizeof(a)/sizeof(*(a)))

// 全局变量：按键标志位与接收缓存
uint8_t flag = 0xff;                  // 按键标志位（1-4：亮灯，5-6、10-11：灭灯）
uint8_t Rx1_Buffer_P[1] = { 0 };        // 普通按键接收缓存（12个普通键：1-6等）
uint8_t Rx1_Buffer_T[1] = { 0 };        // 功能按键接收缓存（4个功能键：A/B等）
uint8_t reset[1] = { 0xff };            // 复位缓存（备用）

// 函数声明
void SystemClock_Config(void);
void switch_key(void);                // 普通按键值转换（1-6键）
void switch_key_func(void);           // 功能按键值转换（A/B键）
void delay_my(uint8_t time);          // 自定义短延时（备用）

int main(void)
{
    // 1. 初始化HAL库、系统时钟、GPIO、I2C、串口
    HAL_Init();
    SystemClock_Config();
    MX_GPIO_Init();         // 直接调用1_Led工程的GPIO初始化，无需修改
    MX_I2C1_Init();
    MX_USART1_UART_Init();

    // 2. 初始化LED为熄灭状态（高电平熄灭）
    HAL_GPIO_WritePin(GPIOF, GPIO_PIN_10, GPIO_PIN_SET);  // LED1（PF10）灭
    HAL_GPIO_WritePin(GPIOC, GPIO_PIN_0, GPIO_PIN_SET);   // LED2（PC0）灭
    HAL_GPIO_WritePin(GPIOB, GPIO_PIN_15, GPIO_PIN_SET);  // LED3（PB15）灭
    HAL_GPIO_WritePin(GPIOH, GPIO_PIN_15, GPIO_PIN_SET);  // LED4（PH15）灭

    // 3. 串口打印实验信息（调试用）
    printf("\r\nFS-STM32开发板小键盘控制LED亮灭实验（21_Key_Led）\r\n");

    // 4. 主循环：读取按键→转换标志位→控制LED
    while (1)
    {
        // 读取小键盘键值（普通键+功能键）
        I2C_ZLG72128_Read(&hi2c1, 0x61, ZLG_READ_ADDRESS1, Rx1_Buffer_P, 1);    // 读普通键
        I2C_ZLG72128_Read(&hi2c1, 0x61, ZLG_READ_FUNCTION_ADDRESS, Rx1_Buffer_T, 1);  // 读功能键

        // 普通按键处理（1-6键：控制LED亮/灭）
        if (Rx1_Buffer_P[0] != 0x0)
        {
            switch_key();
            printf("普通按键：flag = %d\r\n", flag);
        }

        // 功能按键处理（A/B键：控制LED4亮/灭）
        if (Rx1_Buffer_T[0] != 0xff)
        {
            switch_key_func();
            printf("功能按键：flag = %d\r\n", flag);
        }

        // 根据flag控制LED亮灭（文档）
        switch (flag)
        {
        case 1:  // 按"1"键：LED1亮
            HAL_GPIO_WritePin(GPIOF, GPIO_PIN_10, GPIO_PIN_RESET);
            break;
        case 2:  // 按"2"键：LED2亮
            HAL_GPIO_WritePin(GPIOC, GPIO_PIN_0, GPIO_PIN_RESET);
            break;
        case 3:  // 按"3"键：LED3亮
            HAL_GPIO_WritePin(GPIOB, GPIO_PIN_15, GPIO_PIN_RESET);
            break;
        case 4:  // 按"A"键：LED4亮
            HAL_GPIO_WritePin(GPIOH, GPIO_PIN_15, GPIO_PIN_RESET);
            break;
        case 5:  // 按"4"键：LED1灭
            HAL_GPIO_WritePin(GPIOF, GPIO_PIN_10, GPIO_PIN_SET);
            break;
        case 6:  // 按"5"键：LED2灭
            HAL_GPIO_WritePin(GPIOC, GPIO_PIN_0, GPIO_PIN_SET);
            break;
        case 10: // 按"6"键：LED3灭
            HAL_GPIO_WritePin(GPIOB, GPIO_PIN_15, GPIO_PIN_SET);
            break;
        case 11: // 按"B"键：LED4灭
            HAL_GPIO_WritePin(GPIOH, GPIO_PIN_15, GPIO_PIN_SET);
            break;
        default: // 其他按键：LED状态不变
            break;
        }
    }
}

/**
  * @brief  普通按键值转换（1-6键映射到flag）
  * @param  无
  * @retval 无
  */
void switch_key(void)
{
    switch (Rx1_Buffer_P[0])
    {
    case 0x01: flag = 1;  // 小键盘"1"→flag=1（LED1亮）
        break;
    case 0x02: flag = 2;  // 小键盘"2"→flag=2（LED2亮）
        break;
    case 0x03: flag = 3;  // 小键盘"3"→flag=3（LED3亮）
        break;
    case 0x04: flag = 5;  // 小键盘"4"→flag=5（LED1灭）
        break;
    case 0x05: flag = 6;  // 小键盘"5"→flag=6（LED2灭）
        break;
    case 0x06: flag = 10; // 小键盘"6"→flag=10（LED3灭）
        break;
    default: flag = 0xff; // 其他普通键→flag复位（无动作）
        break;
    }
}

/**
  * @brief  功能按键值转换（A/B键映射到flag）
  * @param  无
  * @retval 无
  */
void switch_key_func(void)
{
    switch (Rx1_Buffer_T[0])
    {
    case 0x01: flag = 4;  // 功能键"A"→flag=4（LED4亮）
        break;
    case 0x02: flag = 11; // 功能键"B"→flag=11（LED4灭）
        break;
    default: flag = 0xff; // 其他功能键→flag复位（无动作）
        break;
    }
}

/**
  * @brief  系统时钟配置（STM32F407通用，文档隐含工程基础配置）
  * @param  无
  * @retval 无
  */
void SystemClock_Config(void)
{
    RCC_OscInitTypeDef RCC_OscInitStruct = { 0 };
    RCC_ClkInitTypeDef RCC_ClkInitStruct = { 0 };

    // 配置HSE外部晶振（8MHz）→PLL→168MHz系统时钟
    RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSE;
    RCC_OscInitStruct.HSEState = RCC_HSE_ON;
    RCC_OscInitStruct.PLL.PLLState = RCC_PLL_ON;
    RCC_OscInitStruct.PLL.PLLSource = RCC_PLLSOURCE_HSE;
    RCC_OscInitStruct.PLL.PLLM = 8;
    RCC_OscInitStruct.PLL.PLLN = 336;
    RCC_OscInitStruct.PLL.PLLP = RCC_PLLP_DIV2;
    RCC_OscInitStruct.PLL.PLLQ = 7;
    if (HAL_RCC_OscConfig(&RCC_OscInitStruct) != HAL_OK)
    {
        Error_Handler();
    }

    // 配置AHB/APB时钟分频
    RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK | RCC_CLOCKTYPE_SYSCLK
        | RCC_CLOCKTYPE_PCLK1 | RCC_CLOCKTYPE_PCLK2;
    RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_PLLCLK;
    RCC_ClkInitStruct.AHBCLKDivider = RCC_SYSCLK_DIV1;
    RCC_ClkInitStruct.APB1CLKDivider = RCC_HCLK_DIV4;
    RCC_ClkInitStruct.APB2CLKDivider = RCC_HCLK_DIV2;

    if (HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_5) != HAL_OK)
    {
        Error_Handler();
    }
}

/**
  * @brief  错误处理（LED1闪烁提示，文档隐含调试逻辑）
  * @param  无
  * @retval 无
  */
void Error_Handler(void)
{
    while (1)
    {
        HAL_GPIO_TogglePin(GPIOF, GPIO_PIN_10);
        HAL_Delay(500);
    }
}

#ifdef USE_FULL_ASSERT
void assert_failed(uint8_t* file, uint32_t line)
{
    printf("Wrong parameters value: file %s on line %d\r\n", file, line);
}
#endif