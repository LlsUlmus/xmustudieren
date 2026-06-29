#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <pthread.h>

#define SET_VAL _IO('Z', 0)
#define GET_KEY _IO('Z', 1)

#define STEPPER_ON 	0 
#define STEPPER_OFF 	1 

#if 0
#define IOCTL_MAGIC 	'S'
#define STEPPER_ON 	_IOW(IOCTL_MAGIC, 0, int) 
#define STEPPER_OFF 	_IOW(IOCTL_MAGIC, 1, int) 
#endif

#define KeyDevice 		"/dev/zlg72xx"
#define StepperDevice 	"/dev/stepper"

static int fd_key;
static int fd_stepper;
static volatile int key1 = 0;
static volatile int key2 = 0;
static const int times = 900;  /* 步进电机延时时间（微秒） */

/* 步进电机8拍序列：顺时针旋转 */
static const int clockwise_seq[8][4] = {
	{1, 0, 0, 0}, {1, 1, 0, 0}, {0, 1, 0, 0}, {0, 1, 1, 0},
	{0, 0, 1, 0}, {0, 0, 1, 1}, {0, 0, 0, 1}, {1, 0, 0, 1}
};

void *stepper_clockwise(void *data)
{
	int i;
	while (1) {
		if (key1 == 1) {
			for (i = 0; i < 8; i++) {
				if (key1 == 0) break;  /* 检查是否停止 */
				ioctl(fd_stepper, clockwise_seq[i][0] ? STEPPER_ON : STEPPER_OFF, 0);
				ioctl(fd_stepper, clockwise_seq[i][1] ? STEPPER_ON : STEPPER_OFF, 1);
				ioctl(fd_stepper, clockwise_seq[i][2] ? STEPPER_ON : STEPPER_OFF, 2);
				ioctl(fd_stepper, clockwise_seq[i][3] ? STEPPER_ON : STEPPER_OFF, 3);
				usleep(times);
			}
		} else {
			usleep(10000);  /* 无操作时短暂休眠 */
		}
	}
	return NULL;
}

/* 步进电机8拍序列：逆时针旋转 */
static const int anticlockwise_seq[8][4] = {
	{1, 0, 0, 1}, {0, 0, 0, 1}, {0, 0, 1, 1}, {0, 0, 1, 0},
	{0, 1, 1, 0}, {0, 1, 0, 0}, {1, 1, 0, 0}, {1, 0, 0, 0}
};

void *stepper_anticlockwise(void *data)
{
	int i;
	while (1) {
		if (key2 == 1) {
			for (i = 0; i < 8; i++) {
				if (key2 == 0) break;  /* 检查是否停止 */
				ioctl(fd_stepper, anticlockwise_seq[i][0] ? STEPPER_ON : STEPPER_OFF, 0);
				ioctl(fd_stepper, anticlockwise_seq[i][1] ? STEPPER_ON : STEPPER_OFF, 1);
				ioctl(fd_stepper, anticlockwise_seq[i][2] ? STEPPER_ON : STEPPER_OFF, 2);
				ioctl(fd_stepper, anticlockwise_seq[i][3] ? STEPPER_ON : STEPPER_OFF, 3);
				usleep(times);
			}
		} else {
			usleep(10000);  /* 无操作时短暂休眠 */
		}
	}
	return NULL;
}

int main(int argc, const char *argv[])
{
	int key = 0;
	char value;
	pthread_t th_stepper_clockwise, th_stepper_anticlockwise;

	fd_key = open(KeyDevice, O_RDWR);
	if (fd_key < 0) {
		perror("open");
		exit(1);
	}

	fd_stepper = open(StepperDevice, O_RDWR);
	if (fd_stepper < 0) {
		perror("Can't open file farsight_stepper,Check your path");
		return -1;
	}

	ioctl(fd_stepper, STEPPER_OFF, 0);
	ioctl(fd_stepper, STEPPER_OFF, 1);
	ioctl(fd_stepper, STEPPER_OFF, 2);
	ioctl(fd_stepper, STEPPER_OFF, 3);

	pthread_create(&th_stepper_clockwise, NULL, stepper_clockwise, 0);
	pthread_create(&th_stepper_anticlockwise, NULL, stepper_anticlockwise, 0);

	printf("Turn D8 and D9 and D10 and D11 to the left, others to the right\n\n");

	printf("Push 1 than Stepper motor rotates clockwise, Push 1 again than Stepper motor stop; Push 2 than Stepper motor rotates counterclockwise,  Push 2  again than Stepper motor stop\n");

	while (1) {
		ioctl(fd_key, GET_KEY, &key);

		switch (key) {
		case 28:
			printf("put is '1'\n");
			value = '1';
			break;
		case 27:
			printf("put is '2'\n");
			value = '2';
			break;
		case 26:
			printf("put is '3'\n");
			value = '3';
			break;
		case 25:
			printf("put is 'A'\n");
			value = 'A';
			break;
		case 20:
			printf("put is '4'\n");
			value = '4';
			break;
		case 19:
			printf("put is '5'\n");
			value = '5';
			break;
		case 18:
			printf("put is '6'\n");
			value = '6';
			break;
		case 17:
			printf("put is 'B'\n");
			value = 'B';
			break;
		case 12:
			printf("put is '7'\n");
			value = '7';
			break;
		case 11:
			printf("put is '8'\n");
			value = '8';
			break;
		case 10:
			printf("put is '9'\n");
			value = '9';
			break;
		case 9:
			printf("put is 'C'\n");
			value = 'C';
			break;
		case 4:
			printf("put is '*'\n");
			value = '*';
			break;
		case 3:
			printf("put is '0'\n");
			value = '0';
			break;
		case 2:
			printf("put is '#'\n");
			value = '#';
			break;
		case 1:
			printf("put is 'D'\n");
			value = 'D';
			break;
		default:
			value = 0x00;
			break;
		}

		if (value == '1') {
			if (key1 == 0) {
				key1 = 1;
				key2 = 0;
			} else {
				key1 = 0;
				key2 = 0;
			}
		}

		if (value == '2') {
			if (key2 == 0) {
				key1 = 0;
				key2 = 1;
			} else {
				key1 = 0;
				key2 = 0;
			}
		}
	}

	pthread_join(th_stepper_clockwise, NULL);
	pthread_join(th_stepper_anticlockwise, NULL);

	close(fd_stepper);
	close(fd_key);
	return 0;
}






