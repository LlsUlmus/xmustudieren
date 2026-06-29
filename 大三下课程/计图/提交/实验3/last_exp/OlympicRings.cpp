#include <GL/freeglut.h>
#include <cmath>

#define PI 3.14159265358979323846

// 绘制圆环的一个扇形片段
// 参数：内径，外径，起始角度，终止角度，深度Z
void drawRingSegment(float innerR, float outerR, float startAngle, float endAngle, float z) {
    glBegin(GL_TRIANGLE_STRIP); // 使用三角形带逼近圆环
    int segments = 60; // 细分数，让圆滑顺畅
    float angleStep = (endAngle - startAngle) / segments;
    
    for (int i = 0; i <= segments; ++i) {
        float angle = startAngle + i * angleStep;
        float rad = angle * PI / 180.0f;
        float cosA = cos(rad);
        float sinA = sin(rad);
        
        // 交替指定内侧顶点和外侧顶点，构建 Triangle Strip
        glVertex3f(innerR * cosA, innerR * sinA, z);
        glVertex3f(outerR * cosA, outerR * sinA, z);
    }
    glEnd();
}

void display() {
    // 动画/重绘流程第一步：清除颜色缓冲区 和 深度缓冲区（必须！）
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    glLoadIdentity();

    // 环的参数
    float inR = 0.8f;
    float outR = 1.0f;

    // 1. 黄色环 (左下)
    glPushMatrix();
    glTranslatef(-1.2f, 0.03f, 0.0f); // 通过 glTranslatef 控制相对位置
    glColor3f(0.988f, 0.694f, 0.192f);
    drawRingSegment(inR, outR, 0.0f, 360.0f, 0.0f);
    glPopMatrix();

    // 2. 绿色环 (右下)
    glPushMatrix();
    glTranslatef(1.2f, 0.03f, 0.0f);
    glColor3f(0.0f, 0.651f, 0.318f);
    drawRingSegment(inR, outR, 0.0f, 360.0f, 0.0f);
    glPopMatrix();

    // 3. 蓝色环 (左上)
    glPushMatrix();
    glTranslatef(-2.4f, 1.0f, 0.0f);
    glColor3f(0.0f, 0.506f, 0.784f);

    drawRingSegment(inR, outR, 0.0f, -30.0f, 0.1f); 
    drawRingSegment(inR, outR, -30.0f, -360.0f, -0.1f);
    glPopMatrix();

    // 4. 黑色环 (中上)
    glPushMatrix();
    glTranslatef(0.0f, 1.0f, 0.0f);
    glColor3f(0.0f, 0.0f, 0.0f);

    drawRingSegment(inR, outR, 0.0f, 180.0f, 0.0f);
    drawRingSegment(inR, outR, 180.0f, 225.0f, -0.1f);
    drawRingSegment(inR, outR, 225.0f, 270.0f, 0.1f);
    drawRingSegment(inR, outR, 270.0f, 315.0f, -0.1f);
    drawRingSegment(inR, outR, 315.0f, 360.0f, 0.1f);
    glPopMatrix();

    // 5. 红色环 (右上)
    glPushMatrix();
    glTranslatef(2.4f, 1.0f, 0.0f);
    glColor3f(0.933f, 0.200f, 0.306f);

    drawRingSegment(inR, outR, 135.0f, 225.0f, -0.1f);
    drawRingSegment(inR, outR, -135.0f, 135.0f, 0.1f);
    glPopMatrix();

    glutSwapBuffers(); // 双缓冲交换
}

// 解决拉伸变形的回调函数
void reshape(int w, int h) {
    if (h == 0) h = 1;
    glViewport(0, 0, w, h);

    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();

    // 使用 glOrtho 设置正交投影矩阵，根据宽高比调整视野边界
    // 保证圆环始终是正圆，不发生椭圆形变
    float aspect = (float)w / (float)h;
    if (w <= h) {
        glOrtho(-4.0, 4.0, -4.0 / aspect, 4.0 / aspect, -1.0, 1.0); 
    } else {
        glOrtho(-4.0 * aspect, 4.0 * aspect, -4.0, 4.0, -1.0, 1.0);
    }

    glMatrixMode(GL_MODELVIEW);
}

void init() {
    glClearColor(1.0f, 1.0f, 1.0f, 1.0f); 

    glEnable(GL_DEPTH_TEST); 
}

int main(int argc, char** argv) {
    glutInit(&argc, argv);
    
    // 初始化时请求双缓冲区、RGB模式 和 深度缓冲区(GLUT_DEPTH)
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH | GLUT_MULTISAMPLE); 
    glutInitWindowSize(800, 600);
    glutCreateWindow("Task: Olympic Rings - Depth Test");

    init();

    glutDisplayFunc(display);
    glutReshapeFunc(reshape); // 注册 reshape 函数应对窗口改变大小 

    glutMainLoop();
    return 0;
}