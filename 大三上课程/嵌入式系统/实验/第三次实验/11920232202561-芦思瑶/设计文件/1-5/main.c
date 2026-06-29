#include "main.h"
#include "usart.h"
#include "gpio.h"
#include "stdio.h"
#include "Remotelnfrared.h"

// 全局变量：红外键值标志位（0xff表示无按键，复用13_IR_Receive工程定义）
uint8_t ir_key_flag = 0xff;
// 全局延时计数（供红外解码库使用，复用13_IR_Receive工程定义）
__IO uint32_t GlobalTimingDelay100us;

// 函数声明：红外键值控制LED逻辑
void IR_Key_Control_LED(uint8_t ir_key);

int main(void)
{
    // 1. 初始化HAL库、系统时钟、GPIO、串口（复用13_IR_Receive和1_Led工程初始化逻辑）
    HAL_Init();
    SystemClock_Config(); // 系统时钟配置（168MHz，复用原有工程）
    MX_GPIO_Init();       // 初始化红外接收+LED引脚（复用上述gpio.c函数）
    MX_USART1_UART_Init();// 串口初始化（波特率115200，复用13_IR_Receive工程）

    // 2. 串口打印实验信息（调试用，复用usart.c串口功能）
    printf("\r\nFS-STM32开发板IR红外线接收控制LED实验\r\n");
    printf("按遥控器1-4键：LED1-LED4亮；按5-8键：LED1-LED4灭\r\n");

    // 3. 主循环：读取红外键值→控制LED（综合13_IR_Receive解码与1_Led控制逻辑）
    while (1)
    {
        // 读取红外遥控器键值（调用Remotelnfrared.c解码函数，复用13_IR_Receive工程）
        ir_key_flag = Remote_Infrared_KeyDeCode();

        // 若有有效键值，控制LED并打印
        if (ir_key_flag != 0xff)
        {
            printf("红外键值：%d → ", ir_key_flag);
            IR_Key_Control_LED(ir_key_flag); // 按实验要求控制LED
            ir_key_flag = 0xff; // 重置键值标志位
        }
    }
}

/**
  * @brief  红外键值控制LED亮灭（按实验要求映射）
  * @param  ir_key：红外解码键值（来自Remotelnfrared.c，复用13_IR_Receive工程）
  * @retval 无
  */
void IR_Key_Control_LED(uint8_t ir_key)
{
    switch (ir_key)
    {
    case 8:  // 遥控器"1"键→LED1亮
        HAL_GPIO_WritePin(GPIOF, GPIO_PIN_10, GPIO_PIN_RESET);
        printf("LED1亮\r\n");
        break;
    case 168:// 遥控器"2"键→LED2亮
        HAL_GPIO_WritePin(GPIOC, GPIO_PIN_0, GPIO_PIN_RESET);
        printf("LED2亮\r\n");
        break;
    case 72: // 遥控器"3"键→LED3亮
        HAL_GPIO_WritePin(GPIOB, GPIO_PIN_15, GPIO_PIN_RESET);
        printf("LED3亮\r\n");
        break;
    case 24: // 遥控器"4"键→LED4亮
        HAL_GPIO_WritePin(GPIOH, GPIO_PIN_15, GPIO_PIN_RESET);
        printf("LED4亮\r\n");
        break;
    case 40: // 遥控器"5"键→LED1灭
        HAL_GPIO_WritePin(GPIOF, GPIO_PIN_10, GPIO_PIN_SET);
        printf("LED1灭\r\n");
        break;
    case 136:// 遥控器"6"键→LED2灭
        HAL_GPIO_WritePin(GPIOC, GPIO_PIN_0, GPIO_PIN_SET);
        printf("LED2灭\r\n");
        break;
    case 232:// 遥控器"7"键→LED3灭
        HAL_GPIO_WritePin(GPIOB, GPIO_PIN_15, GPIO_PIN_SET);
        printf("LED3灭\r\n");
        break;
    case 200:// 遥控器"8"键→LED4灭
        HAL_GPIO_WritePin(GPIOH, GPIO_PIN_15, GPIO_PIN_SET);
        printf("LED4灭\r\n");
        break;
    default: // 其他键→无动作
        printf("无对应动作\r\n");
        break;
    }
}

// 系统时钟配置（复用13_IR_Receive工程，STM32F407通用）
void SystemClock_Config(void)
{
    RCC_OscInitTypeDef RCC_OscInitStruct = { 0 };
    RCC_ClkInitTypeDef RCC_ClkInitStruct = { 0 };

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

// 错误处理函数（LED1闪烁，复用1_Led工程逻辑）
void Error_Handler(void)
{
    while (1)
    {
        HAL_GPIO_TogglePin(GPIOF, GPIO_PIN_10);
        HAL_Delay(500);
    }
}

// 串口重定向（printf支持，复用usart.c逻辑）
#ifdef __GNUC__
#define PUTCHAR_PROTOTYPE int __io_putchar(int ch)
#else
#define PUTCHAR_PROTOTYPE int fputc(int ch, FILE *f)
#endif
PUTCHAR_PROTOTYPE
{
    HAL_UART_Transmit(&huart1, (uint8_t*)&ch, 1, HAL_MAX_DELAY);
    return ch;
}