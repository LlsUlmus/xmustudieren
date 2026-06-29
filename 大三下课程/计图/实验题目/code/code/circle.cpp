#include <GL/freeglut.h>
#include <cmath>

namespace {
constexpr float kPi = 3.14159265358979323846f;
constexpr int kSegments = 100;
}

void init()
{
	glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
}

void display()
{
	glClear(GL_COLOR_BUFFER_BIT);
	glLoadIdentity();

	// 使用三角扇绘制实心圆
	glColor3f(0.2f, 0.8f, 1.0f);
	glBegin(GL_TRIANGLE_FAN);
	glVertex2f(0.0f, 0.0f);
	for (int i = 0; i <= kSegments; ++i) {
		float angle = 2.0f * kPi * static_cast<float>(i) / static_cast<float>(kSegments);
		float x = 0.6f * std::cos(angle);
		float y = 0.6f * std::sin(angle);
		glVertex2f(x, y);
        // if (i >= kSegments / 2)
        // {
        //     glColor3f(1.0f, 0.2f, 0.8f); // 从半圆开始改变颜色
        // }
        
	}
	glEnd();

	glFlush();
}

void reshape(int w, int h)
{
	glViewport(0, 0, w, h);
	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();

	if (w <= h) {
		float ratio = static_cast<float>(h) / static_cast<float>(w);
		glOrtho(-1.0, 1.0, -ratio, ratio, -1.0, 1.0);
	} else {
		float ratio = static_cast<float>(w) / static_cast<float>(h);
		glOrtho(-ratio, ratio, -1.0, 1.0, -1.0, 1.0);
	}

	glMatrixMode(GL_MODELVIEW);
}

void keyboard(unsigned char key, int, int)
{
	if (key == 27) {
		exit(0);
	}
}

int main(int argc, char** argv)
{
	glutInit(&argc, argv);
	glutInitDisplayMode(GLUT_SINGLE | GLUT_RGB);
	glutInitWindowSize(600, 600);
	glutInitWindowPosition(100, 100);
	glutCreateWindow("FreeGLUT Circle");

	init();
	glutDisplayFunc(display);
	glutReshapeFunc(reshape);
	glutKeyboardFunc(keyboard);
	glutMainLoop();

	return 0;
}
