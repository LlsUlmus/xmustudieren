#include <GL/glut.h>
#include <cmath>

static float g_timeSec = 0.0f;
static float g_rotDeg = 0.0f;
static float g_scale = 1.0f;
static int g_lastTickMs = 0;
static int g_winW = 800;
static int g_winH = 600;

static void HSV2RGB(float h, float s, float v, float &r, float &g, float &b) {
    float c = v * s;
    float hh = h / 60.0f;
    float x = c * (1.0f - std::fabsf(std::fmod(hh, 2.0f) - 1.0f));
    float m = v - c;
    float rr = 0.0f, gg = 0.0f, bb = 0.0f;

    if (0.0f <= hh && hh < 1.0f) { rr = c; gg = x; bb = 0.0f; }
    else if (1.0f <= hh && hh < 2.0f) { rr = x; gg = c; bb = 0.0f; }
    else if (2.0f <= hh && hh < 3.0f) { rr = 0.0f; gg = c; bb = x; }
    else if (3.0f <= hh && hh < 4.0f) { rr = 0.0f; gg = x; bb = c; }
    else if (4.0f <= hh && hh < 5.0f) { rr = x; gg = 0.0f; bb = c; }
    else { rr = c; gg = 0.0f; bb = x; }

    r = rr + m;
    g = gg + m;
    b = bb + m;
}

static void drawTriangle(float x1, float y1, float x2, float y2, float x3, float y3, int depth) {
    if (depth == 0) {
        float cx = (x1 + x2 + x3) / 3.0f;
        float cy = (y1 + y2 + y3) / 3.0f;
        float dist = std::sqrtf(cx * cx + cy * cy);

        float hue = std::fmod(100.0f * dist + 120.0f * g_timeSec, 360.0f);
        float sat = 0.7f + 0.25f * std::sinf(g_timeSec + dist * 5.0f);
        if (sat < 0.3f) sat = 0.3f;
        if (sat > 1.0f) sat = 1.0f;
        float val = 0.75f + 0.2f * std::cosf(g_timeSec * 1.5f + dist * 4.0f);
        if (val < 0.35f) val = 0.35f;
        if (val > 1.0f) val = 1.0f;

        float r, g, b;
        HSV2RGB(hue, sat, val, r, g, b);
        glColor3f(r, g, b);

        glBegin(GL_TRIANGLES);
        glVertex2f(x1, y1);
        glVertex2f(x2, y2);
        glVertex2f(x3, y3);
        glEnd();
        return;
    }

    float x12 = (x1 + x2) * 0.5f, y12 = (y1 + y2) * 0.5f;
    float x23 = (x2 + x3) * 0.5f, y23 = (y2 + y3) * 0.5f;
    float x31 = (x3 + x1) * 0.5f, y31 = (y3 + y1) * 0.5f;

    drawTriangle(x1, y1, x12, y12, x31, y31, depth - 1);
    drawTriangle(x12, y12, x2, y2, x23, y23, depth - 1);
    drawTriangle(x31, y31, x23, y23, x3, y3, depth - 1);
}

static void display() {
    glClear(GL_COLOR_BUFFER_BIT);

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();

    glTranslatef(0.0f, 0.0f, 0.0f);
    glRotatef(g_rotDeg, 0.0f, 0.0f, 1.0f);
    glScalef(g_scale, g_scale, 1.0f);

    drawTriangle(-0.9f, -0.8f, 0.9f, -0.8f, 0.0f, 0.9f, 6);

    glutSwapBuffers();
}

static void reshape(int w, int h) {
    g_winW = (w > 1) ? w : 1;
    g_winH = (h > 1) ? h : 1;

    glViewport(0, 0, g_winW, g_winH);

    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();

    float aspect = static_cast<float>(g_winW) / static_cast<float>(g_winH);
    if (aspect >= 1.0f) {
        glOrtho(-aspect, aspect, -1.0, 1.0, -1.0, 1.0);
    } else {
        glOrtho(-1.0, 1.0, -1.0f / aspect, 1.0f / aspect, -1.0, 1.0);
    }
}

static void idle() {
    int now = glutGet(GLUT_ELAPSED_TIME);
    if (g_lastTickMs == 0) {
        g_lastTickMs = now;
    }

    float dt = (now - g_lastTickMs) * 0.001f;
    g_lastTickMs = now;
    if (dt < 0.0f) dt = 0.0f;
    if (dt > 0.05f) dt = 0.05f;

    g_timeSec += dt;
    g_rotDeg += 25.0f * dt;
    if (g_rotDeg >= 360.0f) g_rotDeg -= 360.0f;

    // 缩放区间扩大，确保肉眼可见明显“呼吸感”
    g_scale = 0.90f + 0.35f * std::sinf(g_timeSec * 1.8f);
    glutPostRedisplay();
}

static void keyboard(unsigned char key, int, int) {
    if (key == 27) {
        std::exit(0);
    }
}

int main(int argc, char** argv) {
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB);
    glutInitWindowSize(g_winW, g_winH);
    glutCreateWindow("exp02_task1 - Sierpinski");

    glClearColor(0.02f, 0.02f, 0.03f, 1.0f);

    glutDisplayFunc(display);
    glutReshapeFunc(reshape);
    glutIdleFunc(idle);
    glutKeyboardFunc(keyboard);

    glutMainLoop();
    return 0;
}
