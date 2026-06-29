.text
.global _start

_start:
	mov w0,#9
	mov w1,#15
	add w1,w1,w0	//将寄存器w0和w1相加的结果放到w1中
stop:
	b stop
