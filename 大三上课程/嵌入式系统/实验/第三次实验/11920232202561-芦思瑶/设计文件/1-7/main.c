#include "main.h"
#include "i2c.h"
#include "usart.h"
#include "gpio.h"
#include "zlg72128.h"
#include "stdio.h"

// 1. 数码管地址定义（与图片要求完全一致，左数第1个为最左边，第8个为最右边）
#define ZLG_WRITE_ADDRESS1 0x17  // 最右边的数码管（秒低位）
#define ZLG_WRITE_ADDRESS2 0x16  // 左数第7个数码管（秒高位）
#define ZLG_WRITE_ADDRESS3 0x15  // 左数第6个数码管（分隔符“-”）
#define ZLG_WRITE_ADDRESS4 0x14  // 左数第5个数码管（分低位）
#define ZLG_WRITE_ADDRESS5 0x13  // 左数第4个数码管（分高位）
#define ZLG_WRITE_ADDRESS6 0x12  // 左数第3个数码管（分隔符“-”）
#define ZLG_WRITE_ADDRESS7 0x11  // 左数第2个数码管（时低位）
#define ZLG_WRITE_ADDRESS8 0x10  // 最左边的数码管（时高位）

// 2. 七段码定义（要求：0-9共阴极七段码，“-”为0x40）
#define SEG_0 0x3f  // 0
#define SEG_1 0x06  // 1
#define SEG_2 0x5b  // 2
#define SEG_3 0x4f  // 3
#define SEG_4 0x66  // 4
#define SEG_5 0x6d  // 5
#define SEG_6 0x7d  // 6
#define SEG_7 0x07  // 7
#define SEG_8 0x7f  // 8
#define SEG_9 0x6f  // 9
#define SEG_DASH 0x40// “-”

// 3. 全局变量：时间变量（初始时间设为23:59:50）
uint8_t hour = 23;    // 时
uint8_t minute = 59;  // 分
uint8_t second = 50;  // 秒
// 时间拆分变量（高位+低位，如23拆为hour_high=2，hour_low=3）
uint8_t hour_high = 0, hour_low = 0;
uint8_t minute_high = 0, minute_low = 0;
uint8_t second_high = 0, second_low = 0;

// 4. 数码管发送缓存（1字节，存储单个七段码）
uint8_t Tx1_Buffer[8] = { 0 };

// 函数声明
void SystemClock_Config(void);
uint8_t convert(uint8_t num);  // 数字转七段码
void update_time(void);        // 更新时间（秒递增+进位逻辑）
void display_time(void);       // 数码管显示时/分/秒

int main(void)
{
    // 1. 初始化HAL库、系统时钟、GPIO、I2C、串口（复用11_ZLG72128工程逻辑）
    HAL_Init();
    SystemClock_Config();
    MX_GPIO_Init();         // 数码管GPIO初始化（直接复用）
    MX_I2C1_Init();         // I2C初始化（数码管通信）
    MX_USART1_UART_Init();  // 串口初始化（波特率115200，调试用）

    // 2. 串口打印实验信息
    printf("\r\n=====================================\r\n");
    printf("  FS-STM32电子钟实验（25_Clock）\r\n");
    printf("  数码管显示格式：HH-MM-SS（每1秒更新）\r\n");
    printf("=====================================\r\n");

    // 3. 主循环：更新时间→显示时间→延时1秒
    while (1)
    {
        update_time();   // 更新时/分/秒（秒+1，满60进位）
        display_time();  // 在8个数码管上显示时间
        HAL_Delay(1000); // 每1秒更新一次
    }
}

/**
  * @brief  数字转七段码（输入0-9，返回对应七段码）
  * @param  num：要转换的数字（0-9）
  * @retval 对应的七段码（如num=1返回0x06）
  */
uint8_t convert(uint8_t num)
{
    switch (num)
    {
    case 0: return SEG_0;
    case 1: return SEG_1;
    case 2: return SEG_2;
    case 3: return SEG_3;
    case 4: return SEG_4;
    case 5: return SEG_5;
    case 6: return SEG_6;
    case 7: return SEG_7;
    case 8: return SEG_8;
    case 9: return SEG_9;
    default: return 0x00; // 无效数字，不显示
    }
}

/**
  * @brief  更新时间（秒递增，满60进1；分满60进1；时满24归0）
  * @param  无
  * @retval 无
  */
void update_time(void)
{
    second++;  // 秒递增
    if (second == 60)
    {
        second = 0;  // 秒满60归0
        minute++;    // 分+1
        if (minute == 60)
        {
            minute = 0;  // 分满60归0
            hour++;      // 时+1
            if (hour == 24)
            {
                hour = 0;  // 时满24归0
            }
        }
    }

    // 拆分时间为“高位+低位”（如hour=23→hour_high=2，hour_low=3）
    hour_high = hour / 10;
    hour_low = hour % 10;
    minute_high = minute / 10;
    minute_low = minute % 10;
    second_high = second / 10;
    second_low = second % 10;
}

/**
  * @brief  数码管显示时/分/秒（格式：HH-MM-SS，8个数码管分配如下）
  *         左数1：时高位 | 左数2：时低位 | 左数3：- | 左数4：分高位 | 左数5：分低位 | 左数6：- | 左数7：秒高位 | 左数8（最右）：秒低位
  * @param  无
  * @retval 无
  */
void display_time(void)
{
    // 1. 显示“时高位”（左数第1个数码管，ZLG_WRITE_ADDRESS8）
    Tx1_Buffer[0] = convert(hour_high);
    I2C_ZLG72128_Write_char(&hi2c1, 0x60, ZLG_WRITE_ADDRESS8, Tx1_Buffer);

    // 2. 显示“时低位”（左数第2个数码管，ZLG_WRITE_ADDRESS7）
    Tx1_Buffer[0] = convert(hour_low);
    I2C_ZLG72128_Write_char(&hi2c1, 0x60, ZLG_WRITE_ADDRESS7, Tx1_Buffer);

    // 3. 显示分隔符“-”（左数第3个数码管，ZLG_WRITE_ADDRESS6）
    Tx1_Buffer[0] = SEG_DASH;
    I2C_ZLG72128_Write_char(&hi2c1, 0x60, ZLG_WRITE_ADDRESS6, Tx1_Buffer);

    // 4. 显示“分高位”（左数第4个数码管，ZLG_WRITE_ADDRESS5）
    Tx1_Buffer[0] = convert(minute_high);
    I2C_ZLG72128_Write_char(&hi2c1, 0x60, ZLG_WRITE_ADDRESS5, Tx1_Buffer);

    // 5. 显示“分低位”（左数第5个数码管，ZLG_WRITE_ADDRESS4）
    Tx1_Buffer[0] = convert(minute_low);
    I2C_ZLG72128_Write_char(&hi2c1, 0x60, ZLG_WRITE_ADDRESS4, Tx1_Buffer);

    // 6. 显示分隔符“-”（左数第6个数码管，ZLG_WRITE_ADDRESS3）
    Tx1_Buffer[0] = SEG_DASH;
    I2C_ZLG72128_Write_char(&hi2c1, 0x60, ZLG_WRITE_ADDRESS3, Tx1_Buffer);

    // 7. 显示“秒高位”（左数第7个数码管，ZLG_WRITE_ADDRESS2）
    Tx1_Buffer[0] = convert(second_high);
    I2C_ZLG72128_Write_char(&hi2c1, 0x60, ZLG_WRITE_ADDRESS2, Tx1_Buffer);

    // 8. 显示“秒低位”（最右边数码管，ZLG_WRITE_ADDRESS1）
    Tx1_Buffer[0] = convert(second_low);
    I2C_ZLG72128_Write_char(&hi2c1, 0x60, ZLG_WRITE_ADDRESS1, Tx1_Buffer);
}

/**
  * @brief  系统时钟配置（STM32F407通用，复用11_ZLG72128工程）
  * @param  无
  * @retval 无
  */
void SystemClock_Config(void)
{
    RCC_OscInitTypeDef RCC_OscInitStruct = { 0 };
    RCC_ClkInitTypeDef RCC_ClkInitStruct = { 0 };

    // 使能PWR时钟并配置电压缩放
    __HAL_RCC_PWR_CLK_ENABLE();
    __HAL_PWR_VOLTAGESCALING_CONFIG(PWR_REGULATOR_VOLTAGE_SCALE1);

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

    // 配置系统时钟及总线分频
    RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK | RCC_CLOCKTYPE_SYSCLK |
        RCC_CLOCKTYPE_PCLK1 | RCC_CLOCKTYPE_PCLK2;
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
  * @brief  串口重定向（printf支持，调试用，复用11_ZLG72128工程）
  * @param  ch：要发送的字符，f：文件指针（忽略）
  * @retval 发送的字符
  */
int fputc(int ch, FILE* f)
{
    uint8_t tmp[1] = { 0 };
    tmp[0] = (uint8_t)ch;
    HAL_UART_Transmit(&huart1, tmp, 1, 10);  // 串口1发送字符
    return ch;
}

/**
  * @brief  错误处理函数（LED闪烁提示，复用11_ZLG72128工程）
  * @param  无
  * @retval 无
  */
void Error_Handler(void)
{
    while (1)
    {
        // 若工程中有LED引脚，可添加LED闪烁逻辑（如PF10）
        HAL_GPIO_TogglePin(GPIOF, GPIO_PIN_10);
        HAL_Delay(500);
    }
}

#ifdef USE_FULL_ASSERT
/**
  * @brief  断言失败处理（调试用）
  * @param  file：文件名，line：行号
  * @retval 无
  */
void assert_failed(uint8_t* file, uint32_t line)
{
    printf("Assert failed: file %s, line %d\r\n", file, line);
}
#endif