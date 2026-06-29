#include <GL/freeglut.h>
#include <cmath>

const int NUM_POINTS = 50;
const float RADIUS = 0.5f;

void display(void)
{
    glClear(GL_COLOR_BUFFER_BIT);
    
    glClear(GL_COLOR_BUFFER_BIT);
    glLoadIdentity();
    // 变换：观察方式 - 改变位置
    glTranslatef(0.3f, 0.2f, 0.0f);  // 右移0.3，上移0.2
        // 画彩色圆形
    glBegin(GL_TRIANGLE_FAN);
    glColor3f(1.0f, 0.0f, 0.0f);
    glVertex2f(0.0f, 0.0f);
    
    for (int i = 0; i <= NUM_POINTS; i++) {
        float angle = 2.0f * 3.14159f * i / NUM_POINTS;
        float x = RADIUS * cos(angle);
        float y = RADIUS * sin(angle);
        glColor3f(1.0f - (float)i/NUM_POINTS, 0.0f, (float)i/NUM_POINTS);
        glVertex2f(x, y);
    }
    glEnd();
    
    glFlush();
    }

void init()
{
    glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
}

int main(int argc, char** argv)
{
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_SINGLE | GLUT_RGB);
    glutInitWindowSize(500, 500);
    glutInitWindowPosition(100, 100);
    glutCreateWindow("彩色圆形");
    glutDisplayFunc(display);
        init();
    glutMainLoop();
    return 0;
}