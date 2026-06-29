#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <linux/input.h>

#define SET_VAL _IO('Z', 0)
#define GET_KEY _IO('Z', 1)

char int_to_char(int i)
{
	switch(i)
	{
		case 0: return '0';
		case 1: return '1';
		case 2: return '2';
		case 3: return '3';
		case 4: return '4';
		case 5: return '5';
		case 6: return '6';
		case 7: return '7';
		case 8: return '8';
		case 9: return '9';
		default: return '0';
	}
}

char int_to_char_dot(int i)
{
	switch(i)
	{
		case 0: return 'G';
		case 1: return 'H';
		case 2: return 'I';
		case 3: return 'J';
		case 4: return 'K';
		case 5: return 'L';
		case 6: return 'M';
		case 7: return 'N';
		case 8: return 'O';
		case 9: return 'P';
		default: return '0';
	}
}

int main(int argc, const char *argv[])
{
	int fd_lm75;
	int fd_zlg;
	int data;
	int temp_int, temp_frac;
	float temp;
	char buf[8] = {0};
	ssize_t ret;

	fd_lm75 = open("/dev/temp", O_RDWR);
	if (fd_lm75 < 0) {
		perror("open");
		exit(0);
	}

	fd_zlg = open("/dev/zlg72xx", O_RDWR);
	if (fd_zlg < 0) {
		perror("open");
		exit(1);
	}

	printf("Please press your finger on the temperature sensor chip\n");

	while (1) {
		read(fd_lm75, (char *)&data, sizeof(data));

		/* LM75温度计算：温度 = 0.125 * (data >> 5) */
		temp = 0.125 * (data >> 5);

		/* 提取整数部分和小数部分 */
		temp_int = (int)temp;
		temp_frac = (int)((temp - temp_int) * 1000);  /* 保留3位小数 */

		/* 格式化显示：显示为 "XX.X" 格式 */
		buf[0] = ' ';
		buf[1] = ' ';
		buf[2] = ' ';
		buf[3] = int_to_char(temp_int / 10);        /* 十位 */
		buf[4] = int_to_char_dot(temp_int % 10);    /* 个位（带小数点） */
		buf[5] = int_to_char(temp_frac / 100);      /* 小数第1位 */
		buf[6] = int_to_char((temp_frac / 10) % 10); /* 小数第2位 */
		buf[7] = int_to_char(temp_frac % 10);       /* 小数第3位 */

		ioctl(fd_zlg, SET_VAL, buf);
		sleep(5);
	}

	close(fd_lm75);
	close(fd_zlg);
	return 0;
}

