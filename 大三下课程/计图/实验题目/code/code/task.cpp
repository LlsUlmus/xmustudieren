#include <GL/freeglut.h>
#include <cstdlib>

void drawQuad(float x, float y, float z, float r, float g, float b)
{
    constexpr float halfSize = 0.35f;
    glColor3f(r, g, b);
    glBegin(GL_QUADS);
    glVertex3f(x - halfSize, y - halfSize, z);
    glVertex3f(x + halfSize, y - halfSize, z);
    glVertex3f(x + halfSize, y + halfSize, z);
    glVertex3f(x - halfSize, y + halfSize, z);
    glEnd();
}

void display()
{
    // 每帧清空颜色缓存和深度缓存。
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();

    // 红色在前，蓝色在后。
    drawQuad(0.2f, 0.0f, -2.5f, 1.0f, 0.2f, 0.2f);
    drawQuad(-0.2f, 0.0f, -3.0f, 0.2f, 0.4f, 1.0f);

    glutSwapBuffers();
}

void reshape(int w, int h)
{
    if (h == 0) {
        h = 1;
    }

    glViewport(0, 0, w, h);
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    gluPerspective(45.0, static_cast<double>(w) / static_cast<double>(h), 0.1, 100.0);
    glMatrixMode(GL_MODELVIEW);
}

void keyboard(unsigned char key, int, int)
{
    if (key == 27) {
        std::exit(0);
    }
}

int main(int argc, char** argv)
{
    glutInit(&argc, argv);

    // 请求深度缓存。
    glutInitDisplayMode(GLUT_RGB | GLUT_DOUBLE | GLUT_DEPTH);
    glutInitWindowSize(640, 480);
    glutCreateWindow("Depth Buffer Request and Clear Demo");

    glClearColor(0.08f, 0.08f, 0.1f, 1.0f);

    // 开启深度测试。
    glEnable(GL_DEPTH_TEST);
    // glDisable(GL_DEPTH_TEST);

    glutDisplayFunc(display);
    glutReshapeFunc(reshape);
    glutKeyboardFunc(keyboard);

    glutMainLoop();
    return 0;
}
