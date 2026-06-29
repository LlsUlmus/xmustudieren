#include <GL/freeglut.h>
#include <cmath>

#define PI 3.14159265358979323846

float colors[6][3] = {
    {0.5f, 0.5f, 0.0f}, {0.0f, 0.5f, 0.5f}, {0.5f, 0.0f, 0.5f},
    {1.0f, 0.0f, 0.0f}, {0.0f, 1.0f, 0.0f}, {0.0f, 0.0f, 1.0f}
};

// 全局变量控制旋转角度
float currentAngle = 0.0f;

// 绘制单个扇形的函数
// 参数：圆心(x0,y0), 半径R, 起始角度theta_start, 终止角度theta_end, 逼近的点数n
void drawSector(float x0, float y0, float R, float theta_start, float theta_end, int n) {
    glBegin(GL_POLYGON);
    glVertex2f(x0, y0);
    
    // 把 (theta_start, theta_end) 均分成 n 份
    float delta = (theta_end - theta_start) / n; // 每份的步长
    
    for (int i = 0; i <= n; ++i) {
        float theta = theta_start + i * delta; // 计算第 i 个点对应的角度
        float radian = theta * PI / 180.0f;    
        
        // 计算这n个点的坐标来逼近扇形
        float x = x0 + R * cos(radian);
        float y = y0 + R * sin(radian);
        glVertex2f(x, y);
    }
    glEnd();
}

void display() {

    glClear(GL_COLOR_BUFFER_BIT);

    glPushMatrix(); // 保护当前的矩阵状态
    
    // 利用 glRotatef 控制圆盘整体旋转
    // 绕Z轴旋转 (0, 0, 1)
    glRotatef(currentAngle, 0.0f, 0.0f, 1.0f);

    int numSectors = 6;
    float sectorAngle = 360.0f / numSectors;

    // 循环绘制多个扇形堆叠成圆盘
    for (int i = 0; i < numSectors; ++i) {
        glColor3fv(colors[i]); // 对扇形赋予不同的颜色
        
        // 调用我们封装的扇形绘制函数，用20个点来逼近60度的弧
        drawSector(0.0f, 0.0f, 0.8f, i * sectorAngle, (i + 1) * sectorAngle, 20); 
    }

    glPopMatrix(); 

    glutSwapBuffers(); 
}

void idle() {
    // 持续更新图形，每次更新都令圆盘旋转一定角度
    currentAngle += 0.1f; 
    if (currentAngle >= 360.0f) {
        currentAngle -= 360.0f;
    }

    glutPostRedisplay(); 
}

void reshape(int w, int h) {
    if (h == 0) h = 1;
    glViewport(0, 0, w, h);
    
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();

    // 计算窗口宽高比，利用 glOrtho 动态调整视景体的边界
    float aspect = (float)w / (float)h;
    if (w <= h) {
        // 如果窗口更窄高，上下边界需要按比例拉伸
        glOrtho(-1.0, 1.0, -1.0 / aspect, 1.0 / aspect, -1.0, 1.0); 
    } else {
        // 如果窗口更宽扁，左右边界需要按比例拉伸
        glOrtho(-1.0 * aspect, 1.0 * aspect, -1.0, 1.0, -1.0, 1.0);
    }

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
}

void init() {
    glClearColor(1.0f, 1.0f, 1.0f, 1.0f);
    
    // GL_FLAT 单一着色，避免扇形内部出现颜色插值过渡 
    glShadeModel(GL_FLAT); 
}

int main(int argc, char** argv) {
    glutInit(&argc, argv);
    
    // 动画的关键：开启双缓冲区 GLUT_DOUBLE
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB); 
    glutInitWindowSize(600, 600);
    glutCreateWindow("Task: Rotating Multi-color Disk");

    init();

    glutDisplayFunc(display);
    glutReshapeFunc(reshape);

    // 注册闲置回调函数，程序空闲时运行 idle 函数更新角度
    glutIdleFunc(idle); 

    glutMainLoop();
    return 0;
}