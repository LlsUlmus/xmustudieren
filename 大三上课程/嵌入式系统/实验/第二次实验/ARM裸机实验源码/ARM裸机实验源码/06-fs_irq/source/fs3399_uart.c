#include "fs3399_uart.h"
#include "fs3399_grf.h"

//uart init function
void fs_uart_init(unsigned long baudrate)
{
	volatile unsigned int rate;

	/*Set GPIO4_C0 GPIO4_C1 IOMUX*/
	GRF_GPIO4C_IOMUX = (0x3 << 16) | (0x3 << (16+2)) | (0x2 << 0) | (0x2 << 2);
	
	/*software reset uart, rcvr_fifo_reset, xmit_fifo_reset*/
	UART2_SRR = (0x1 << 0) | (0x1 << 1) | (0x1 << 2); 

	/*interrupt disable*/
	UART2_IER = 0x00;

	/*modem control register disable*/
	UART2_MCR = 0x00;

	/* set Serial protocol*/
	UART2_LCR &= (~(0x3 << 0));			//bic bit[1:0]
    UART2_LCR |=  (0x3 << 0); 			//set data length : 8 bit
    UART2_LCR &= (~(0x1 << 2)); 		//set stop bits num : 1 stop bit
    UART2_LCR &= (~(0x1 << 3)); 		//parity disable

	/* set Baud rate*/
	rate =  24000000/16/baudrate;			//baud rate = 24000000/16/rate;

	UART2_LCR |= (0x01 << 7);			//This bit is used to enable reading and writing of the
										//Divisor Latch register (DLL and DLH) to set the baud rate of the UART		

    UART2_DLL = (rate & 0xFF);			//set baud rate

    UART2_LCR &= ~(0x01 << 7);			//This bit must be cleared after initial baud rate setup 
										//in order to access other registers

    UART2_SFE = 0x1;		//shadow FIFO enable
    UART2_SRT = 0x3;		//set rcvr_trigger : FIFO 2 less than full
    UART2_STET = 0x1;		//set tx_empty_trigger : 2 characters in the FIFO
}

//Transimt byte
void fs_putc(unsigned char byte)
{
	 while ((UART2_USR & (0x1 << 1)) == 0);		//Transmit FIFO is full or not

    UART2_THR = byte;			//if not full. write character to this register
}

//Transmit string
void fs_puts(char *str)
{
	while (*str != '\0') {
		fs_putc(*str);
		str++;
	}
}

//revive byte
char fs_getc()
{
	while ((UART2_USR & (0x1 << 3)) == 0);		//recv FIFO is empty or not

	return UART2_RBR;
}
