#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/ioctl.h>

#define LED1_ON    _IO('G',1)
#define LED1_OFF   _IO('G',2)
#define LED2_ON    _IO('G',3)
#define LED2_OFF   _IO('G',4)
#define LED3_ON    _IO('G',5)
#define LED3_OFF   _IO('G',6) 

#define KeyDevice "/dev/farsight_keys"	//按键的设备号
#define LedDevice "/dev/leds_ctl"		//基于GPIO的LED灯设备号

static struct input_event inputevent;

int main(void)
{
	int fd_key;
	int fd_led;
	ssize_t err;
	int buf[2] = {0};
	int led1 = 0;
	int led2 = 0;

	fd_key = open(KeyDevice, O_RDWR);
	if (fd_key < 0) {
		perror("Can't open file farsight_keys,Check your path");
		return -1;
	}

	fd_led = open(LedDevice, O_RDWR);
	if (fd_led < 0) {
		perror("Can't open file farsight_led,Check your path");
		return -1;
	}

	ioctl(fd_led, LED1_OFF);
	ioctl(fd_led, LED2_OFF);

	printf("Push KEY1 than light LED1, Push KEY1 again than extinguish LED1; Push KEY2 than light LED2,  Push KEY2  again than extinguish LED2\n");

	while (1) {
		err = read(fd_key, buf, sizeof(buf));

		if (err > 0) {
			if (buf[0] == 1) {
				if (buf[1] == 0) {
					if (led1 == 0) {
						ioctl(fd_led, LED1_ON);
						led1 = 1;
					} else {
						ioctl(fd_led, LED1_OFF);
						led1 = 0;
					}
				}
			}
			if (buf[0] == 2) {
				if (buf[1] == 0) {
					if (led2 == 0) {
						ioctl(fd_led, LED2_ON);
						led2 = 1;
					} else {
						ioctl(fd_led, LED2_OFF);
						led2 = 0;
					}
				}
			}
		}
	}

	close(fd_led);
	close(fd_key);
	return 0;
}




