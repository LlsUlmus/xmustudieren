// 实验3 附加题2：在实验2旋转立方体基础上加入光照/材质/光源切换 + 滑动条交互
// 编译(Windows/VS 或 MinGW)：链接 opengl32 glu32 glut32
#include <GL/glut.h>

#include <algorithm>
#include <cmath>
#include <cstdlib>
#include <string>

static constexpr float DEG_TO_RAD = 3.1415926f / 180.0f;
static const GLfloat RR = 6.0f;
static GLfloat g_beta = 0.0f;
static GLfloat g_cubeRot = 0.0f;

static bool g_useColoredLight = false; // o:白光, p:彩色光
static int g_matPreset = 0;            // 0:brass 1:red plastic 2:shiny white
static float g_lightAmbientScale = 0.18f;
static float g_lightDiffuseScale = 0.95f;
static float g_matDiffuseScale = 1.00f;
static float g_matShininess = 64.0f;

struct Slider {
    float x = 30, y = 30, w = 260, h = 18;
    float v = 0.5f;
    bool dragging = false;
    std::string label;
};

static int g_winW = 900, g_winH = 700;
static Slider s_lightAmb{ 30, 30, 300, 18, 0.18f, false, "Light Ambient" };
static Slider s_lightDiff{ 30, 60, 300, 18, 0.95f, false, "Light Diffuse" };
static Slider s_matDiff{ 30, 90, 300, 18, 0.83f, false, "Mat Diffuse" };
static Slider s_shiny{ 30, 120, 300, 18, 0.50f, false, "Shininess" };

static void applyMaterialPreset() {
    GLfloat amb[4]{}, diff[4]{}, spec[4]{};
    GLfloat shin[1]{ g_matShininess };

    if (g_matPreset == 0) {
        amb[0] = 0.329412f; amb[1] = 0.223529f; amb[2] = 0.027451f; amb[3] = 1.0f;
        diff[0] = 0.780392f; diff[1] = 0.568627f; diff[2] = 0.113725f; diff[3] = 1.0f;
        spec[0] = 0.992157f; spec[1] = 0.941176f; spec[2] = 0.807843f; spec[3] = 1.0f;
    } else if (g_matPreset == 1) {
        amb[0] = 0.0f; amb[1] = 0.0f; amb[2] = 0.0f; amb[3] = 1.0f;
        diff[0] = 0.5f; diff[1] = 0.0f; diff[2] = 0.0f; diff[3] = 1.0f;
        spec[0] = 0.7f; spec[1] = 0.6f; spec[2] = 0.6f; spec[3] = 1.0f;
    } else {
        amb[0] = 0.2f; amb[1] = 0.2f; amb[2] = 0.2f; amb[3] = 1.0f;
        diff[0] = 0.9f; diff[1] = 0.9f; diff[2] = 0.9f; diff[3] = 1.0f;
        spec[0] = 1.0f; spec[1] = 1.0f; spec[2] = 1.0f; spec[3] = 1.0f;
    }

    for (int i = 0; i < 3; ++i) {
        diff[i] = std::min(1.0f, diff[i] * g_matDiffuseScale);
        amb[i] = std::min(1.0f, amb[i] * (0.6f + 0.4f * g_matDiffuseScale));
    }
    shin[0] = std::max(1.0f, g_matShininess);

    glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, amb);
    glMaterialfv(GL_FRONT_AND_BACK, GL_DIFFUSE, diff);
    glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, spec);
    glMaterialfv(GL_FRONT_AND_BACK, GL_SHININESS, shin);
}

static void applyLight() {
    glEnable(GL_LIGHTING);
    glEnable(GL_LIGHT0);
    glEnable(GL_NORMALIZE);

    const GLfloat baseAmb = std::clamp(g_lightAmbientScale, 0.0f, 1.0f);
    const GLfloat baseDiff = std::clamp(g_lightDiffuseScale, 0.0f, 1.0f);

    GLfloat amb[4] = { baseAmb, baseAmb, baseAmb, 1.0f };
    GLfloat diff[4] = { baseDiff, baseDiff, baseDiff, 1.0f };
    GLfloat spec[4] = { 0.75f * baseDiff, 0.75f * baseDiff, 0.75f * baseDiff, 1.0f };

    if (g_useColoredLight) {
        diff[0] *= 1.0f; diff[1] *= 0.65f; diff[2] *= 0.25f;
        spec[0] *= 1.0f; spec[1] *= 0.65f; spec[2] *= 0.25f;
    }

    glLightfv(GL_LIGHT0, GL_AMBIENT, amb);
    glLightfv(GL_LIGHT0, GL_DIFFUSE, diff);
    glLightfv(GL_LIGHT0, GL_SPECULAR, spec);

    GLfloat pos[4] = { 2.5f, 3.0f, 3.5f, 1.0f };
    glLightfv(GL_LIGHT0, GL_POSITION, pos);
}

static void drawString(float x, float y, const char* s) {
    glRasterPos2f(x, y);
    for (const char* p = s; *p; ++p) glutBitmapCharacter(GLUT_BITMAP_8_BY_13, *p);
}

static float clamp01(float t) { return std::max(0.0f, std::min(1.0f, t)); }

static void sliderSetFromMouse(Slider& s, int mx) {
    s.v = clamp01((mx - s.x) / s.w);
}

static void syncParamsFromSliders() {
    g_lightAmbientScale = s_lightAmb.v;
    g_lightDiffuseScale = s_lightDiff.v;
    g_matDiffuseScale = s_matDiff.v * 1.2f;
    g_matShininess = s_shiny.v * 128.0f;
}

static void syncSlidersFromParams() {
    s_lightAmb.v = clamp01(g_lightAmbientScale);
    s_lightDiff.v = clamp01(g_lightDiffuseScale);
    s_matDiff.v = clamp01(g_matDiffuseScale / 1.2f);
    s_shiny.v = clamp01(g_matShininess / 128.0f);
}

static void drawSlider(const Slider& s) {
    glColor3f(0.25f, 0.25f, 0.25f);
    glBegin(GL_QUADS);
    glVertex2f(s.x, s.y); glVertex2f(s.x + s.w, s.y);
    glVertex2f(s.x + s.w, s.y + s.h); glVertex2f(s.x, s.y + s.h);
    glEnd();

    glColor3f(0.15f, 0.65f, 0.95f);
    glBegin(GL_QUADS);
    glVertex2f(s.x, s.y); glVertex2f(s.x + s.w * s.v, s.y);
    glVertex2f(s.x + s.w * s.v, s.y + s.h); glVertex2f(s.x, s.y + s.h);
    glEnd();

    const float kx = s.x + s.w * s.v;
    glColor3f(0.95f, 0.95f, 0.95f);
    glBegin(GL_QUADS);
    glVertex2f(kx - 4, s.y - 2); glVertex2f(kx + 4, s.y - 2);
    glVertex2f(kx + 4, s.y + s.h + 2); glVertex2f(kx - 4, s.y + s.h + 2);
    glEnd();

    glColor3f(0.95f, 0.95f, 0.95f);
    drawString(s.x + s.w + 12, s.y + s.h - 4, s.label.c_str());
}

static bool hitSlider(const Slider& s, int mx, int my) {
    const int yGL = g_winH - my;
    return mx >= (int)s.x && mx <= (int)(s.x + s.w) && yGL >= (int)s.y && yGL <= (int)(s.y + s.h);
}

static void init() {
    glClearColor(0.06f, 0.06f, 0.06f, 1.0f);
    glEnable(GL_DEPTH_TEST);
    glShadeModel(GL_SMOOTH);
    glEnable(GL_LIGHTING);
    glEnable(GL_LIGHT0);
    glEnable(GL_NORMALIZE);
    syncSlidersFromParams();
}

static void display() {
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    gluPerspective(45.0, (double)g_winW / (double)g_winH, 0.1, 100.0);

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
    gluLookAt(RR * std::sin(g_beta * DEG_TO_RAD), 2.8, RR * std::cos(g_beta * DEG_TO_RAD),
              0.0, 0.0, 0.0, 0.0, 1.0, 0.0);

    syncParamsFromSliders();
    applyLight();
    applyMaterialPreset();

    glPushMatrix();
    glRotatef(g_cubeRot, 0.0f, 1.0f, 0.0f);
    glRotatef(g_cubeRot * 0.6f, 1.0f, 0.0f, 0.0f);
    glutSolidCube(2.2);
    glPopMatrix();

    glDisable(GL_LIGHTING);
    glMatrixMode(GL_PROJECTION);
    glPushMatrix();
    glLoadIdentity();
    gluOrtho2D(0, g_winW, 0, g_winH);
    glMatrixMode(GL_MODELVIEW);
    glPushMatrix();
    glLoadIdentity();

    drawSlider(s_lightAmb);
    drawSlider(s_lightDiff);
    drawSlider(s_matDiff);
    drawSlider(s_shiny);
    glColor3f(0.95f, 0.95f, 0.95f);
    drawString(30, 160, "Keys: b(brass) n(red plastic) m(shiny white) | o(white light) p(colored light) | ESC quit");

    glPopMatrix();
    glMatrixMode(GL_PROJECTION);
    glPopMatrix();
    glMatrixMode(GL_MODELVIEW);
    glEnable(GL_LIGHTING);

    glutSwapBuffers();
}

static void reshape(int w, int h) {
    g_winW = std::max(1, w);
    g_winH = std::max(1, h);
    glViewport(0, 0, (GLsizei)g_winW, (GLsizei)g_winH);
}

static void idle() {
    g_beta += 0.15f;
    if (g_beta > 360.0f) g_beta -= 360.0f;
    g_cubeRot += 0.40f;
    if (g_cubeRot > 360.0f) g_cubeRot -= 360.0f;
    glutPostRedisplay();
}

static void keyboard(unsigned char key, int, int) {
    switch (key) {
    case 27: std::exit(0); break;
    case 'b': case 'B': g_matPreset = 0; glutPostRedisplay(); break;
    case 'n': case 'N': g_matPreset = 1; glutPostRedisplay(); break;
    case 'm': case 'M': g_matPreset = 2; glutPostRedisplay(); break;
    case 'o': case 'O': g_useColoredLight = false; glutPostRedisplay(); break;
    case 'p': case 'P': g_useColoredLight = true; glutPostRedisplay(); break;
    default: break;
    }
}

static void mouse(int button, int state, int x, int y) {
    if (button != GLUT_LEFT_BUTTON) return;
    if (state == GLUT_DOWN) {
        if (hitSlider(s_lightAmb, x, y)) { s_lightAmb.dragging = true; sliderSetFromMouse(s_lightAmb, x); }
        else if (hitSlider(s_lightDiff, x, y)) { s_lightDiff.dragging = true; sliderSetFromMouse(s_lightDiff, x); }
        else if (hitSlider(s_matDiff, x, y)) { s_matDiff.dragging = true; sliderSetFromMouse(s_matDiff, x); }
        else if (hitSlider(s_shiny, x, y)) { s_shiny.dragging = true; sliderSetFromMouse(s_shiny, x); }
        glutPostRedisplay();
    } else if (state == GLUT_UP) {
        s_lightAmb.dragging = s_lightDiff.dragging = s_matDiff.dragging = s_shiny.dragging = false;
        glutPostRedisplay();
    }
}

static void motion(int x, int) {
    bool changed = false;
    if (s_lightAmb.dragging) { sliderSetFromMouse(s_lightAmb, x); changed = true; }
    if (s_lightDiff.dragging) { sliderSetFromMouse(s_lightDiff, x); changed = true; }
    if (s_matDiff.dragging) { sliderSetFromMouse(s_matDiff, x); changed = true; }
    if (s_shiny.dragging) { sliderSetFromMouse(s_shiny, x); changed = true; }
    if (changed) glutPostRedisplay();
}

int main(int argc, char** argv) {
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH);
    glutInitWindowSize(g_winW, g_winH);
    glutInitWindowPosition(80, 60);
    glutCreateWindow("exp03_task3 - Lighting cube (b/n/m material, o/p light, sliders)");

    init();
    glutDisplayFunc(display);
    glutReshapeFunc(reshape);
    glutIdleFunc(idle);
    glutKeyboardFunc(keyboard);
    glutMouseFunc(mouse);
    glutMotionFunc(motion);
    glutMainLoop();
    return 0;
}
