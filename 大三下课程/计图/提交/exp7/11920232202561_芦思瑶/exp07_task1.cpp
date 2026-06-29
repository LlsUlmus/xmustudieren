
#include <GL/freeglut.h>
#include <cmath>
#include <cstdio>
#include <vector>
#include <string>

struct Vec2 {
    float x, y;
};

static std::vector<Vec2> g_controlPoints;
static int g_numPoints = 3;
static float g_animT = 0.0f;
static float g_curveProgress = 0.0f;
static bool g_animPlaying = true;
static bool g_showConstruction = true;
static int g_selectedPoint = -1;
static int g_windowWidth = 900;
static int g_windowHeight = 650;

static const Vec2 kDefaultPoints[] = {
    {-0.70f, -0.35f}, {-0.35f, 0.55f}, {0.00f, -0.10f}, {0.35f, 0.45f},
    {0.55f, -0.25f}, {0.75f, 0.30f}, {0.85f, -0.05f}, {0.95f, 0.15f},
};

static Vec2 lerp(const Vec2& a, const Vec2& b, float t) {
    return {a.x + t * (b.x - a.x), a.y + t * (b.y - a.y)};
}

static Vec2 deCasteljauPoint(const std::vector<Vec2>& pts, float t) {
    std::vector<Vec2> level = pts;
    while (level.size() > 1) {
        std::vector<Vec2> next;
        for (size_t i = 0; i + 1 < level.size(); ++i) {
            next.push_back(lerp(level[i], level[i + 1], t));
        }
        level = next;
    }
    return level[0];
}

static std::vector<std::vector<Vec2>> deCasteljauLevels(const std::vector<Vec2>& pts, float t) {
    std::vector<std::vector<Vec2>> levels;
    levels.push_back(pts);
    std::vector<Vec2> level = pts;
    while (level.size() > 1) {
        std::vector<Vec2> next;
        for (size_t i = 0; i + 1 < level.size(); ++i) {
            next.push_back(lerp(level[i], level[i + 1], t));
        }
        levels.push_back(next);
        level = next;
    }
    return levels;
}

static void resetControlPoints(int n) {
    g_numPoints = n;
    if (g_numPoints < 3) g_numPoints = 3;
    if (g_numPoints > 8) g_numPoints = 8;
    g_controlPoints.clear();
    for (int i = 0; i < g_numPoints; ++i) {
        g_controlPoints.push_back(kDefaultPoints[i]);
    }
    g_selectedPoint = -1;
    g_animT = 0.0f;
    g_curveProgress = 0.0f;
}

static void screenToWorld(int sx, int sy, float& wx, float& wy) {
    wx = (sx / (float)g_windowWidth) * 2.0f - 1.0f;
    wy = 1.0f - (sy / (float)g_windowHeight) * 2.0f;
}

static int pickControlPoint(int sx, int sy) {
    float wx, wy;
    screenToWorld(sx, sy, wx, wy);
    const float pickRadius = 0.06f;
    int best = -1;
    float bestDist = pickRadius;
    for (int i = 0; i < g_numPoints; ++i) {
        float dx = g_controlPoints[i].x - wx;
        float dy = g_controlPoints[i].y - wy;
        float dist = std::sqrt(dx * dx + dy * dy);
        if (dist < bestDist) {
            bestDist = dist;
            best = i;
        }
    }
    return best;
}

static void drawPoint(float x, float y, float r, float cr, float cg, float cb) {
    const int segments = 12;
    glColor3f(cr, cg, cb);
    glBegin(GL_LINES);
    for (int i = 0; i < segments; ++i) {
        float a0 = (float)i / segments * 6.2831853f;
        float a1 = (float)(i + 1) / segments * 6.2831853f;
        glVertex2f(x + r * std::cos(a0), y + r * std::sin(a0));
        glVertex2f(x + r * std::cos(a1), y + r * std::sin(a1));
    }
    glEnd();
}

static void drawLine(const Vec2& a, const Vec2& b) {
    glVertex2f(a.x, a.y);
    glVertex2f(b.x, b.y);
}

static void drawText(float x, float y, const char* text) {
    glColor3f(0.9f, 0.9f, 0.9f);
    glRasterPos2f(x, y);
    for (const char* p = text; *p; ++p) {
        glutBitmapCharacter(GLUT_BITMAP_9_BY_15, *p);
    }
}

static void display() {
    glClearColor(0.08f, 0.09f, 0.12f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT);
    glLoadIdentity();

    // 控制多边形
    glColor3f(0.45f, 0.75f, 0.95f);
    glBegin(GL_LINES);
    for (int i = 0; i < g_numPoints - 1; ++i) {
        drawLine(g_controlPoints[i], g_controlPoints[i + 1]);
    }
    glEnd();

    // de Casteljau
    if (g_showConstruction) {
        auto levels = deCasteljauLevels(g_controlPoints, g_animT);
        const float levelColors[][3] = {
            {0.45f, 0.75f, 0.95f},
            {1.0f, 0.55f, 0.20f},
            {0.95f, 0.35f, 0.55f},
            {0.55f, 0.85f, 0.35f},
            {0.85f, 0.65f, 0.95f},
            {0.95f, 0.85f, 0.25f},
            {0.35f, 0.95f, 0.85f},
            {0.95f, 0.45f, 0.45f},
        };
        for (size_t lv = 1; lv < levels.size(); ++lv) {
            const float* c = levelColors[(lv - 1) % 8];
            glColor3f(c[0], c[1], c[2]);
            glBegin(GL_LINES);
            for (size_t i = 0; i + 1 < levels[lv].size(); ++i) {
                drawLine(levels[lv][i], levels[lv][i + 1]);
            }
            glEnd();
            for (const auto& p : levels[lv]) {
                drawPoint(p.x, p.y, 0.012f, c[0], c[1], c[2]);
            }
        }
        if (!levels.empty() && !levels.back().empty()) {
            const Vec2& tip = levels.back()[0];
            drawPoint(tip.x, tip.y, 0.022f, 1.0f, 1.0f, 0.2f);
        }
    }

    // 已生成的曲线段
    const int steps = 240;
    int maxStep = (int)(g_curveProgress * steps);
    if (maxStep > 0) {
        glColor3f(1.0f, 0.95f, 0.25f);
        glBegin(GL_LINES);
        for (int i = 0; i < maxStep; ++i) {
            float t0 = i / (float)steps;
            float t1 = (i + 1) / (float)steps;
            Vec2 p0 = deCasteljauPoint(g_controlPoints, t0);
            Vec2 p1 = deCasteljauPoint(g_controlPoints, t1);
            drawLine(p0, p1);
        }
        glEnd();
    }

    // 控制点
    for (int i = 0; i < g_numPoints; ++i) {
        bool selected = (i == g_selectedPoint);
        drawPoint(g_controlPoints[i].x, g_controlPoints[i].y, selected ? 0.028f : 0.020f,
                  selected ? 1.0f : 0.2f, selected ? 1.0f : 0.85f, selected ? 0.2f : 0.35f);
        char label[8];
        std::snprintf(label, sizeof(label), "P%d", i);
        drawText(g_controlPoints[i].x + 0.03f, g_controlPoints[i].y + 0.03f, label);
    }

    char info[256];
    std::snprintf(info, sizeof(info),
                  "Task1 Bezier Curve | Control Points: %d | t=%.3f | Progress=%.1f%%",
                  g_numPoints, g_animT, g_curveProgress * 100.0f);
    drawText(-0.98f, 0.92f, info);
    drawText(-0.98f, 0.85f, "Keys: 3-8 points | Space pause | C toggle construction | R reset");
    drawText(-0.98f, 0.78f, "Mouse: click/drag control points");

    glutSwapBuffers();
}

static void update(int value) {
    (void)value;
    if (g_animPlaying) {
        g_animT += 0.012f;
        if (g_animT > 1.0f) {
            g_animT = 0.0f;
            g_curveProgress = 0.0f;
        }
        g_curveProgress += 0.012f;
        if (g_curveProgress > 1.0f) g_curveProgress = 1.0f;
    }
    glutPostRedisplay();
    glutTimerFunc(16, update, 0);
}

static void reshape(int w, int h) {
    g_windowWidth = w;
    g_windowHeight = h > 0 ? h : 1;
    glViewport(0, 0, g_windowWidth, g_windowHeight);
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    glOrtho(-1.0, 1.0, -1.0, 1.0, -1.0, 1.0);
    glMatrixMode(GL_MODELVIEW);
}

static void keyboard(unsigned char key, int, int) {
    if (key >= '3' && key <= '8') {
        resetControlPoints(key - '0');
    } else if (key == ' ' || key == 'p' || key == 'P') {
        g_animPlaying = !g_animPlaying;
    } else if (key == 'c' || key == 'C') {
        g_showConstruction = !g_showConstruction;
    } else if (key == 'r' || key == 'R') {
        g_animT = 0.0f;
        g_curveProgress = 0.0f;
    } else if (key == 27) {
        exit(0);
    }
    glutPostRedisplay();
}

static void mouse(int button, int state, int x, int y) {
    if (button == GLUT_LEFT_BUTTON) {
        if (state == GLUT_DOWN) {
            g_selectedPoint = pickControlPoint(x, y);
        } else if (state == GLUT_UP) {
            g_selectedPoint = -1;
        }
    }
    glutPostRedisplay();
}

static void motion(int x, int y) {
    if (g_selectedPoint >= 0) {
        float wx, wy;
        screenToWorld(x, y, wx, wy);
        if (wx < -0.98f) wx = -0.98f;
        if (wx > 0.98f) wx = 0.98f;
        if (wy < -0.90f) wy = -0.90f;
        if (wy > 0.90f) wy = 0.90f;
        g_controlPoints[g_selectedPoint] = {wx, wy};
        glutPostRedisplay();
    }
}

int main(int argc, char** argv) {
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB);
    glutInitWindowSize(g_windowWidth, g_windowHeight);
    glutCreateWindow("Exp07 Task1 - Bezier Curve (de Casteljau)");

    resetControlPoints(3);

    glutDisplayFunc(display);
    glutReshapeFunc(reshape);
    glutKeyboardFunc(keyboard);
    glutMouseFunc(mouse);
    glutMotionFunc(motion);
    glutTimerFunc(16, update, 0);

    glutMainLoop();
    return 0;
}
