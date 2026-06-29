#include <GL/glut.h>
#include <cmath>
#include <cstring>
#include <cstdlib>

struct Vec3 {
    float x;
    float y;
    float z;
};

static Vec3 g_camPos = {0.0f, 0.0f, 6.0f};
static float g_yawDeg = -90.0f;
static float g_pitchDeg = 0.0f;
static float g_cubeRot = 0.0f;

static bool g_lockCamera = false;
static bool g_depthEnabled = true;
static bool g_mouseLook = false;
static bool g_keys[256];

static int g_lastMouseX = -1;
static int g_lastMouseY = -1;
static int g_winW = 1000;
static int g_winH = 700;

static float toRad(float deg) { return deg * 0.0174532925f; }

static Vec3 normalize(Vec3 v) {
    float len = std::sqrtf(v.x * v.x + v.y * v.y + v.z * v.z);
    if (len < 1e-6f) return {0.0f, 0.0f, -1.0f};
    return {v.x / len, v.y / len, v.z / len};
}

static Vec3 cross(Vec3 a, Vec3 b) {
    return {a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x};
}

static Vec3 frontDir() {
    float yaw = toRad(g_yawDeg);
    float pitch = toRad(g_pitchDeg);
    Vec3 f = {std::cosf(yaw) * std::cosf(pitch), std::sinf(pitch), std::sinf(yaw) * std::cosf(pitch)};
    return normalize(f);
}

static void drawCube(float s) {
    float h = s * 0.5f;
    glBegin(GL_TRIANGLES);

    glColor3f(1.0f, 0.2f, 0.2f);
    glVertex3f(-h, h, h); glVertex3f(-h, -h, h); glVertex3f(h, -h, h);
    glVertex3f(-h, h, h); glVertex3f(h, -h, h); glVertex3f(h, h, h);

    glColor3f(0.2f, 0.9f, 0.3f);
    glVertex3f(-h, h, -h); glVertex3f(h, -h, -h); glVertex3f(-h, -h, -h);
    glVertex3f(-h, h, -h); glVertex3f(h, h, -h); glVertex3f(h, -h, -h);

    glColor3f(0.2f, 0.5f, 1.0f);
    glVertex3f(-h, h, -h); glVertex3f(-h, -h, h); glVertex3f(-h, h, h);
    glVertex3f(-h, h, -h); glVertex3f(-h, -h, -h); glVertex3f(-h, -h, h);

    glColor3f(1.0f, 0.85f, 0.2f);
    glVertex3f(h, h, -h); glVertex3f(h, h, h); glVertex3f(h, -h, h);
    glVertex3f(h, h, -h); glVertex3f(h, -h, h); glVertex3f(h, -h, -h);

    glColor3f(0.8f, 0.3f, 1.0f);
    glVertex3f(-h, h, -h); glVertex3f(-h, h, h); glVertex3f(h, h, h);
    glVertex3f(-h, h, -h); glVertex3f(h, h, h); glVertex3f(h, h, -h);

    glColor3f(0.6f, 0.6f, 0.6f);
    glVertex3f(-h, -h, -h); glVertex3f(h, -h, h); glVertex3f(-h, -h, h);
    glVertex3f(-h, -h, -h); glVertex3f(h, -h, -h); glVertex3f(h, -h, h);

    glEnd();
}

static void display() {
    if (g_depthEnabled) glEnable(GL_DEPTH_TEST);
    else glDisable(GL_DEPTH_TEST);

    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();

    Vec3 front = frontDir();
    gluLookAt(g_camPos.x, g_camPos.y, g_camPos.z,
              g_camPos.x + front.x, g_camPos.y + front.y, g_camPos.z + front.z,
              0.0f, 1.0f, 0.0f);

    glPushMatrix();
    glRotatef(g_cubeRot, 0.0f, 1.0f, 0.0f);
    glRotatef(g_cubeRot * 0.35f, 1.0f, 0.0f, 0.0f);
    drawCube(2.0f);
    glPopMatrix();

    glutSwapBuffers();
}

static void reshape(int w, int h) {
    g_winW = (w > 1) ? w : 1;
    g_winH = (h > 1) ? h : 1;

    glViewport(0, 0, g_winW, g_winH);
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    gluPerspective(60.0, static_cast<double>(g_winW) / static_cast<double>(g_winH), 0.1, 100.0);
}

static void idle() {
    g_cubeRot += 0.25f;
    if (g_cubeRot >= 360.0f) g_cubeRot -= 360.0f;

    if (!g_lockCamera) {
        const float step = 0.08f;
        Vec3 front = frontDir();
        Vec3 right = normalize(cross(front, {0.0f, 1.0f, 0.0f}));

        if (g_keys['w'] || g_keys['W']) { g_camPos.x += front.x * step; g_camPos.y += front.y * step; g_camPos.z += front.z * step; }
        if (g_keys['s'] || g_keys['S']) { g_camPos.x -= front.x * step; g_camPos.y -= front.y * step; g_camPos.z -= front.z * step; }
        if (g_keys['a'] || g_keys['A']) { g_camPos.x -= right.x * step; g_camPos.y -= right.y * step; g_camPos.z -= right.z * step; }
        if (g_keys['d'] || g_keys['D']) { g_camPos.x += right.x * step; g_camPos.y += right.y * step; g_camPos.z += right.z * step; }
        if (g_keys['q'] || g_keys['Q']) { g_camPos.y += step; }
        if (g_keys['e'] || g_keys['E']) { g_camPos.y -= step; }
    }

    glutPostRedisplay();
}

static void keyboardDown(unsigned char key, int, int) {
    if (key == 27) std::exit(0);
    if (key == 'l' || key == 'L') { g_lockCamera = !g_lockCamera; return; }
    if (key == 't' || key == 'T') { g_depthEnabled = !g_depthEnabled; return; }
    g_keys[key] = true;
}

static void keyboardUp(unsigned char key, int, int) {
    g_keys[key] = false;
}

static void mouseButton(int button, int state, int x, int y) {
    if (button == GLUT_RIGHT_BUTTON) {
        if (state == GLUT_DOWN) {
            g_mouseLook = true;
            g_lastMouseX = x;
            g_lastMouseY = y;
        } else {
            g_mouseLook = false;
            g_lastMouseX = -1;
            g_lastMouseY = -1;
        }
    }
}

static void passiveMotion(int x, int y) {
    if (g_lockCamera || !g_mouseLook) return;
    if (g_lastMouseX < 0 || g_lastMouseY < 0) {
        g_lastMouseX = x;
        g_lastMouseY = y;
        return;
    }

    int dx = x - g_lastMouseX;
    int dy = y - g_lastMouseY;
    g_lastMouseX = x;
    g_lastMouseY = y;

    const float sens = 0.12f;
    g_yawDeg += dx * sens;
    g_pitchDeg -= dy * sens;
    if (g_pitchDeg > 89.0f) g_pitchDeg = 89.0f;
    if (g_pitchDeg < -89.0f) g_pitchDeg = -89.0f;
}

static void initGL() {
    std::memset(g_keys, 0, sizeof(g_keys));
    glClearColor(0.08f, 0.08f, 0.1f, 1.0f);
    glEnable(GL_DEPTH_TEST);
    glShadeModel(GL_SMOOTH);
}

int main(int argc, char** argv) {
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH);
    glutInitWindowSize(g_winW, g_winH);
    glutCreateWindow("exp02_task2 - Cube Camera");

    initGL();
    glutDisplayFunc(display);
    glutReshapeFunc(reshape);
    glutIdleFunc(idle);
    glutKeyboardFunc(keyboardDown);
    glutKeyboardUpFunc(keyboardUp);
    glutMouseFunc(mouseButton);
    glutPassiveMotionFunc(passiveMotion);

    glutMainLoop();
    return 0;
}
