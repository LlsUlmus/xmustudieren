#include <GL/glut.h>
#include <cmath>

struct Vec3 {
    float x;
    float y;
    float z;
};

static int g_winW = 1000;
static int g_winH = 700;

static Vec3 g_camPos = {0.0f, 0.0f, 5.5f};
static float g_yawDeg = -90.0f;
static float g_pitchDeg = 0.0f;
static bool g_lockCamera = false;
static int g_lastMouseX = -1;
static int g_lastMouseY = -1;

static float g_spinDeg = 0.0f;

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

static void drawWireSphere(float radius, int stacks, int slices) {
    glColor3f(0.95f, 0.2f, 0.2f);

    for (int i = 0; i <= stacks; ++i) {
        float v = -3.1415926f / 2.0f + (3.1415926f * i) / stacks;
        float y = radius * std::sinf(v);
        float r = radius * std::cosf(v);

        glBegin(GL_LINE_LOOP);
        for (int j = 0; j < slices; ++j) {
            float u = (2.0f * 3.1415926f * j) / slices;
            float x = r * std::cosf(u);
            float z = r * std::sinf(u);
            glVertex3f(x, y, z);
        }
        glEnd();
    }

    for (int j = 0; j < slices; ++j) {
        float u = (2.0f * 3.1415926f * j) / slices;
        glBegin(GL_LINE_STRIP);
        for (int i = 0; i <= stacks; ++i) {
            float v = -3.1415926f / 2.0f + (3.1415926f * i) / stacks;
            float y = radius * std::sinf(v);
            float r = radius * std::cosf(v);
            float x = r * std::cosf(u);
            float z = r * std::sinf(u);
            glVertex3f(x, y, z);
        }
        glEnd();
    }
}

static void display() {
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
    Vec3 front = frontDir();
    gluLookAt(g_camPos.x, g_camPos.y, g_camPos.z,
              g_camPos.x + front.x, g_camPos.y + front.y, g_camPos.z + front.z,
              0.0f, 1.0f, 0.0f);

    glPushMatrix();
    glRotatef(g_spinDeg, 0.0f, 1.0f, 0.0f);
    glRotatef(g_spinDeg * 0.6f, 1.0f, 0.0f, 0.0f);
    drawWireSphere(1.5f, 36, 72);
    glPopMatrix();

    glutSwapBuffers();
}

static void reshape(int w, int h) {
    g_winW = (w > 1) ? w : 1;
    g_winH = (h > 1) ? h : 1;

    glViewport(0, 0, g_winW, g_winH);
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    gluPerspective(55.0, static_cast<double>(g_winW) / static_cast<double>(g_winH), 0.1, 100.0);
}

static void idle() {
    g_spinDeg += 0.3f;
    if (g_spinDeg >= 360.0f) g_spinDeg -= 360.0f;
    glutPostRedisplay();
}

static void moveCamera(Vec3 dir, float amount) {
    g_camPos.x += dir.x * amount;
    g_camPos.y += dir.y * amount;
    g_camPos.z += dir.z * amount;
}

static void keyboard(unsigned char key, int, int) {
    if (key == 27) std::exit(0);

    if (key == 'l' || key == 'L') {
        g_lockCamera = !g_lockCamera;
        return;
    }
    if (g_lockCamera) return;

    float moveStep = 0.14f;
    Vec3 front = frontDir();
    Vec3 up = {0.0f, 1.0f, 0.0f};
    Vec3 right = normalize(cross(front, up));

    switch (key) {
        case 'w': case 'W': moveCamera(front, moveStep); break;
        case 's': case 'S': moveCamera(front, -moveStep); break;
        case 'a': case 'A': moveCamera(right, -moveStep); break;
        case 'd': case 'D': moveCamera(right, moveStep); break;
        case 'q': case 'Q': g_camPos.y += moveStep; break;
        case 'e': case 'E': g_camPos.y -= moveStep; break;
        default: break;
    }
}

static void passiveMotion(int x, int y) {
    if (g_lockCamera) {
        g_lastMouseX = x;
        g_lastMouseY = y;
        return;
    }

    if (g_lastMouseX < 0 || g_lastMouseY < 0) {
        g_lastMouseX = x;
        g_lastMouseY = y;
        return;
    }

    int dx = x - g_lastMouseX;
    int dy = y - g_lastMouseY;
    g_lastMouseX = x;
    g_lastMouseY = y;

    const float sens = 0.18f;
    g_yawDeg += dx * sens;
    g_pitchDeg -= dy * sens;
    if (g_pitchDeg > 89.0f) g_pitchDeg = 89.0f;
    if (g_pitchDeg < -89.0f) g_pitchDeg = -89.0f;
}

static void initGL() {
    glClearColor(0.06f, 0.06f, 0.08f, 1.0f);
    glEnable(GL_DEPTH_TEST);
    glShadeModel(GL_SMOOTH);
}

int main(int argc, char** argv) {
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH);
    glutInitWindowSize(g_winW, g_winH);
    glutCreateWindow("exp02_task3 - Wire Sphere");

    initGL();
    glutDisplayFunc(display);
    glutReshapeFunc(reshape);
    glutIdleFunc(idle);
    glutKeyboardFunc(keyboard);
    glutPassiveMotionFunc(passiveMotion);

    glutMainLoop();
    return 0;
}
