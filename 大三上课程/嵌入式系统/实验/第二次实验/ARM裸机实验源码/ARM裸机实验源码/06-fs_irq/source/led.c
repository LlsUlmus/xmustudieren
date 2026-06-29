/*************
Function£ºLEDS
WORK_LED     BLUE    GPIO0_A3   High_level_on
DIY_LED      YELLOW  GPIO0_B4   High_level_on
**************/

#include "led.h"

void led_delay(void)
{
    volatile unsigned long int i, j;
    for(i = 20; i > 0; i--)
        for(j = 10000; j > 0; j--);
}

void led_mode(int mode)
{
    GPIO0->SWPORTA_DDR |=  (0x01 << (1 * 8 + 4));
    GPIO0->SWPORTA_DDR |=  (0x01 << 3);

    GPIO0->SWPORTA_DR  &= ~(0x01 << (1 * 8 + 4));
    GPIO0->SWPORTA_DR  &= ~(0x01 << 3);

    if(0 == mode)
    {
        GPIO0->SWPORTA_DR  &= ~(0x01 << (1 * 8 + 4));
        GPIO0->SWPORTA_DR  &= ~(0x01 << 3);
    }
    else if(1 == mode)
    {
        GPIO0->SWPORTA_DR  |=  (0x01 << (1 * 8 + 4));
    }
    else if(2 == mode)
    {
        GPIO0->SWPORTA_DR  |=  (0x01 << 3);
    }
    else if(3 == mode)
    {
        GPIO0->SWPORTA_DR  |=  (0x01 << (1 * 8 + 4));
        GPIO0->SWPORTA_DR  |=  (0x01 << 3);
    }

}








