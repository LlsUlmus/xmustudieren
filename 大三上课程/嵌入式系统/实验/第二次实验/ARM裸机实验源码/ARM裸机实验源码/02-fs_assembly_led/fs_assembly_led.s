.text
.global _start

_start:
	//设置 LED3(GPIO0_B4) 为输出模式
	ldr w0,=0xFF720004
	ldr w1,[x0]
	orr w1,w1,#(0x1 << 12)
	str w1,[x0]
	
	//设置 LED1(GPIO4_C6) 为输出模式
	ldr w0,=0xFF790004
	ldr w1,[x0]
	orr w1,w1,#(0x1 << 22)
	str w1,[x0]

LOOP:
	ldr w0,=0xFF720000  //设置 LED3(GPIO0_B4)输出高电平 
	ldr w1,[x0]
	orr w1,w1,#(0x1 << 12)
	str w1,[x0]

	ldr w1,=0xFF790000  //设置LED1(GPIO4_C6)输出高电平
	ldr w2,[x1]
	orr w2,w2,#(0x1 << 22)
	str w2,[x1]

	ldr w3,=0xFFFFFF   //设置一个计数值

LOOP1:
	//循环执行，直到计数值减到1
	sub w3,w3,#1
    cmp w3,#0
    bne LOOP1
	//计数减到1时，关闭LED3,LED1
    mov w2,#0x0
    str w2,[x0]
    str w2,[x1]
	
	//设置一个计数值
    ldr w3,=0xFFFFFF

LOOP2:
	//循环执行，直到计数值减到1时跳转回LOOP继续执行
    sub w3,w3,#1
    cmp w3,#0
    bne LOOP2
    b LOOP

stop:
	b stop
