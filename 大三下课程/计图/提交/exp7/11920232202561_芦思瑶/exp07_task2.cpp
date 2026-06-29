
#include <GL/freeglut.h>
#include <cmath>
#include <cstdio>
#include <vector>
#include <string>

struct Vec3 {
    float x, y, z;
};

static std::vector<std::vector<Vec3>> g_grid;
static int g_gridSize = 3;
static float g_animU = 0.0f;
static float g_animV = 0.0f;
static float g_surfaceProgress = 0.0f;
static bool g_animPlaying = true;
static bool g_showConstruction = true;
static bool g_showWireframe = true;
static bool g_showFilled = true;
static int g_selectedRow = -1;
static int g_selectedCol = -1;
static float g_rotX = 25.0f;
static float g_rotY = -35.0f;
static int g_lastMouseX = 0;
static int g_lastMouseY = 0;
static bool g_rotating = false;
static bool g_draggingPoint = false;
static float g_dragPlaneY = 0.0f;
static int g_dragStartMouseY = 0;
static int g_windowWidth = 960;
static int g_windowHeight = 720;
static GLdouble g_model[16], g_proj[16];
static GLint g_viewport[4];

static Vec3 lerp3(const Vec3& a, const Vec3& b, float t) {
    return {a.x + t * (b.x - a.x), a.y + t * (b.y - a.y), a.z + t * (b.z - a.z)};
}

static Vec3 deCasteljau1D(const std::vector<Vec3>& pts, float t) {
    std::vector<Vec3> level = pts;
    while (level.size() > 1) {
        std::vector<Vec3> next;
        for (size_t i = 0; i + 1 < level.size(); ++i) {
            next.push_back(lerp3(level[i], level[i + 1], t));
        }
        level = next;
    }
    return level[0];
}

static Vec3 surfacePoint(float u, float v) {
    std::vector<Vec3> rowResults;
    rowResults.reserve(g_grid.size());
    for (const auto& row : g_grid) {
        rowResults.push_back(deCasteljau1D(row, u));
    }
    return deCasteljau1D(rowResults, v);
}

static std::vector<std::vector<Vec3>> surfaceRowLevels(float u) {
    std::vector<std::vector<Vec3>> levels;
    levels.reserve(g_grid.size());
    for (const auto& row : g_grid) {
        std::vector<Vec3> level = row;
        std::vector<Vec3> resultRow;
        while (level.size() > 1) {
            std::vector<Vec3> next;
            for (size_t i = 0; i + 1 < level.size(); ++i) {
                next.push_back(lerp3(level[i], level[i + 1], u));
            }
            resultRow.push_back(next[0]);
            level = next;
        }
        if (!level.empty()) {
            resultRow.push_back(level[0]);
        }
        levels.push_back(resultRow);
    }
    return levels;
}

static void initGrid(int n) {
    g_gridSize = n;
    if (g_gridSize < 3) g_gridSize = 3;
    if (g_gridSize > 5) g_gridSize = 5;
    g_grid.assign(g_gridSize, std::vector<Vec3>(g_gridSize));
    for (int i = 0; i < g_gridSize; ++i) {
        for (int j = 0; j < g_gridSize; ++j) {
            float u = i / (float)(g_gridSize - 1);
            float v = j / (float)(g_gridSize - 1);
            float x = (u - 0.5f) * 2.4f;
            float z = (v - 0.5f) * 2.4f;
            float y = 0.35f * std::sin(u * 3.14159f) * std::cos(v * 3.14159f);
            g_grid[i][j] = {x, y, z};
        }
    }
    g_selectedRow = -1;
    g_selectedCol = -1;
    g_animU = 0.0f;
    g_animV = 0.0f;
    g_surfaceProgress = 0.0f;
}

static void setup3DView() {
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    gluPerspective(45.0, g_windowWidth / (double)g_windowHeight, 0.1, 100.0);

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
    gluLookAt(0.0, 2.8, 4.5, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
    glRotatef(g_rotX, 1.0f, 0.0f, 0.0f);
    glRotatef(g_rotY, 0.0f, 1.0f, 0.0f);

    glGetDoublev(GL_MODELVIEW_MATRIX, g_model);
    glGetDoublev(GL_PROJECTION_MATRIX, g_proj);
    glGetIntegerv(GL_VIEWPORT, g_viewport);
}

static bool unprojectToPlane(int sx, int sy, float planeY, Vec3& out) {
    GLdouble x0, y0, z0, x1, y1, z1;
    double winY = g_viewport[3] - sy;
    if (!gluUnProject(sx, winY, 0.0, g_model, g_proj, g_viewport, &x0, &y0, &z0)) {
        return false;
    }
    if (!gluUnProject(sx, winY, 1.0, g_model, g_proj, g_viewport, &x1, &y1, &z1)) {
        return false;
    }
    double dy = y1 - y0;
    if (std::fabs(dy) < 1e-8) {
        return false;
    }
    double t = (planeY - y0) / dy;
    out.x = (float)(x0 + t * (x1 - x0));
    out.y = planeY;
    out.z = (float)(z0 + t * (z1 - z0));
    return true;
}

static void drawText2D(float x, float y, const char* text) {
    glMatrixMode(GL_PROJECTION);
    glPushMatrix();
    glLoadIdentity();
    glOrtho(0, g_windowWidth, 0, g_windowHeight, -1, 1);
    glMatrixMode(GL_MODELVIEW);
    glPushMatrix();
    glLoadIdentity();
    glColor3f(0.92f, 0.92f, 0.92f);
    glRasterPos2f(x, y);
    for (const char* p = text; *p; ++p) {
        glutBitmapCharacter(GLUT_BITMAP_9_BY_15, *p);
    }
    glPopMatrix();
    glMatrixMode(GL_PROJECTION);
    glPopMatrix();
    glMatrixMode(GL_MODELVIEW);
}

static void drawControlNet() {
    glColor3f(0.35f, 0.70f, 0.95f);
    glBegin(GL_LINES);
    for (int i = 0; i < g_gridSize; ++i) {
        for (int j = 0; j < g_gridSize - 1; ++j) {
            glVertex3f(g_grid[i][j].x, g_grid[i][j].y, g_grid[i][j].z);
            glVertex3f(g_grid[i][j + 1].x, g_grid[i][j + 1].y, g_grid[i][j + 1].z);
        }
    }
    for (int j = 0; j < g_gridSize; ++j) {
        for (int i = 0; i < g_gridSize - 1; ++i) {
            glVertex3f(g_grid[i][j].x, g_grid[i][j].y, g_grid[i][j].z);
            glVertex3f(g_grid[i + 1][j].x, g_grid[i + 1][j].y, g_grid[i + 1][j].z);
        }
    }
    glEnd();
}

static void drawConstruction(float u, float v) {
    auto rowLevels = surfaceRowLevels(u);
    const float colors[][3] = {
        {1.0f, 0.55f, 0.20f}, {0.95f, 0.35f, 0.55f}, {0.55f, 0.85f, 0.35f},
        {0.85f, 0.65f, 0.95f}, {0.95f, 0.85f, 0.25f},
    };
    for (int r = 0; r < g_gridSize; ++r) {
        const float* c = colors[r % 5];
        glColor3f(c[0], c[1], c[2]);
        glBegin(GL_LINES);
        for (size_t k = 0; k + 1 < rowLevels[r].size(); ++k) {
            glVertex3f(rowLevels[r][k].x, rowLevels[r][k].y, rowLevels[r][k].z);
            glVertex3f(rowLevels[r][k + 1].x, rowLevels[r][k + 1].y, rowLevels[r][k + 1].z);
        }
        glEnd();
    }

    std::vector<Vec3> rowPoints;
    for (int r = 0; r < g_gridSize; ++r) {
        rowPoints.push_back(deCasteljau1D(g_grid[r], u));
    }
    glColor3f(0.95f, 0.95f, 0.30f);
    glBegin(GL_LINES);
    for (size_t k = 0; k + 1 < rowPoints.size(); ++k) {
        glVertex3f(rowPoints[k].x, rowPoints[k].y, rowPoints[k].z);
        glVertex3f(rowPoints[k + 1].x, rowPoints[k + 1].y, rowPoints[k + 1].z);
    }
    glEnd();

    std::vector<Vec3> vLevel = rowPoints;
    int lv = 0;
    while (vLevel.size() > 1) {
        std::vector<Vec3> next;
        const float* c = colors[lv % 5];
        glColor3f(c[0] * 0.8f, c[1] * 0.8f, c[2] * 0.8f);
        glBegin(GL_LINES);
        for (size_t k = 0; k + 1 < vLevel.size(); ++k) {
            Vec3 p = lerp3(vLevel[k], vLevel[k + 1], v);
            next.push_back(p);
            glVertex3f(vLevel[k].x, vLevel[k].y, vLevel[k].z);
            glVertex3f(vLevel[k + 1].x, vLevel[k + 1].y, vLevel[k + 1].z);
        }
        glEnd();
        vLevel = next;
        ++lv;
    }
    if (!vLevel.empty()) {
        glColor3f(1.0f, 1.0f, 0.3f);
        glBegin(GL_POINTS);
        glVertex3f(vLevel[0].x, vLevel[0].y, vLevel[0].z);
        glEnd();
    }
}

static void drawSurface(float maxProgress) {
    const int res = 24;
    int maxI = (int)(maxProgress * res);
    if (maxI < 1) return;

    for (int i = 0; i < maxI; ++i) {
        for (int j = 0; j < res; ++j) {
            float u0 = i / (float)res;
            float u1 = (i + 1) / (float)res;
            float v0 = j / (float)res;
            float v1 = (j + 1) / (float)res;
            Vec3 p00 = surfacePoint(u0, v0);
            Vec3 p10 = surfacePoint(u1, v0);
            Vec3 p11 = surfacePoint(u1, v1);
            Vec3 p01 = surfacePoint(u0, v1);

            float shade = 0.45f + 0.35f * (0.5f + 0.5f * std::sin(u0 * 6.28f) * std::cos(v0 * 6.28f));
            if (g_showFilled) {
                glColor3f(0.25f * shade + 0.1f, 0.55f * shade + 0.15f, 0.85f * shade + 0.1f);
                glBegin(GL_QUADS);
                glVertex3f(p00.x, p00.y, p00.z);
                glVertex3f(p10.x, p10.y, p10.z);
                glVertex3f(p11.x, p11.y, p11.z);
                glVertex3f(p01.x, p01.y, p01.z);
                glEnd();
            }
            if (g_showWireframe) {
                glColor3f(0.15f, 0.25f, 0.35f);
                glBegin(GL_LINES);
                glVertex3f(p00.x, p00.y, p00.z);
                glVertex3f(p10.x, p10.y, p10.z);
                glVertex3f(p10.x, p10.y, p10.z);
                glVertex3f(p11.x, p11.y, p11.z);
                glVertex3f(p11.x, p11.y, p11.z);
                glVertex3f(p01.x, p01.y, p01.z);
                glVertex3f(p01.x, p01.y, p01.z);
                glVertex3f(p00.x, p00.y, p00.z);
                glEnd();
            }
        }
    }
}

static void drawControlPoints() {
    for (int i = 0; i < g_gridSize; ++i) {
        for (int j = 0; j < g_gridSize; ++j) {
            bool sel = (i == g_selectedRow && j == g_selectedCol);
            glColor3f(sel ? 1.0f : 0.25f, sel ? 0.95f : 0.85f, sel ? 0.2f : 0.35f);
            glBegin(GL_POINTS);
            glVertex3f(g_grid[i][j].x, g_grid[i][j].y, g_grid[i][j].z);
            glEnd();
            const float r = sel ? 0.04f : 0.028f;
            glColor3f(sel ? 1.0f : 0.3f, sel ? 1.0f : 0.8f, sel ? 0.2f : 0.4f);
            glBegin(GL_LINES);
            for (int k = 0; k < 8; ++k) {
                float a0 = k * 6.2831853f / 8.0f;
                float a1 = (k + 1) * 6.2831853f / 8.0f;
                Vec3 c = g_grid[i][j];
                glVertex3f(c.x + r * std::cos(a0), c.y, c.z + r * std::sin(a0));
                glVertex3f(c.x + r * std::cos(a1), c.y, c.z + r * std::sin(a1));
            }
            glEnd();
        }
    }
}

static void display() {
    glClearColor(0.06f, 0.07f, 0.10f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glEnable(GL_DEPTH_TEST);

    setup3DView();

    drawControlNet();
    if (g_showConstruction) {
        drawConstruction(g_animU, g_animV);
    }
    drawSurface(g_surfaceProgress);
    drawControlPoints();

    char info[320];
    std::snprintf(info, sizeof(info),
                  "Task2 Bezier Surface | Grid: %dx%d | u=%.3f v=%.3f | Progress=%.0f%%",
                  g_gridSize, g_gridSize, g_animU, g_animV, g_surfaceProgress * 100.0f);
    drawText2D(12.0f, g_windowHeight - 24.0f, info);
    drawText2D(12.0f, g_windowHeight - 44.0f,
               "Keys: 3/4/5 grid | Space pause | C construction | W wireframe | F filled | R reset");
    drawText2D(12.0f, g_windowHeight - 64.0f,
               "Mouse L: click/drag control points | R: rotate view");

    glutSwapBuffers();
}

static void update(int value) {
    (void)value;
    if (g_animPlaying) {
        g_animU += 0.010f;
        if (g_animU > 1.0f) {
            g_animU = 0.0f;
            g_animV += 0.08f;
            if (g_animV > 1.0f) {
                g_animV = 0.0f;
                g_surfaceProgress = 0.0f;
            }
        }
        g_surfaceProgress += 0.006f;
        if (g_surfaceProgress > 1.0f) g_surfaceProgress = 1.0f;
    }
    glutPostRedisplay();
    glutTimerFunc(16, update, 0);
}

static void reshape(int w, int h) {
    g_windowWidth = w;
    g_windowHeight = h > 0 ? h : 1;
    glViewport(0, 0, g_windowWidth, g_windowHeight);
}

static void keyboard(unsigned char key, int, int) {
    if (key == '3' || key == '4' || key == '5') {
        initGrid(key - '0');
    } else if (key == ' ' || key == 'p' || key == 'P') {
        g_animPlaying = !g_animPlaying;
    } else if (key == 'c' || key == 'C') {
        g_showConstruction = !g_showConstruction;
    } else if (key == 'w' || key == 'W') {
        g_showWireframe = !g_showWireframe;
    } else if (key == 'f' || key == 'F') {
        g_showFilled = !g_showFilled;
    } else if (key == 'r' || key == 'R') {
        g_animU = 0.0f;
        g_animV = 0.0f;
        g_surfaceProgress = 0.0f;
    } else if (key == 27) {
        exit(0);
    }
    glutPostRedisplay();
}

static bool projectToScreen(const Vec3& p, int& sx, int& sy) {
    GLdouble wx, wy, wz;
    if (!gluProject(p.x, p.y, p.z, g_model, g_proj, g_viewport, &wx, &wy, &wz)) {
        return false;
    }
    sx = (int)wx;
    sy = (int)(g_viewport[3] - wy);
    return true;
}

static void pickControlPoint(int mx, int my) {
    g_selectedRow = -1;
    g_selectedCol = -1;
    const int pickRadius = 28;
    int bestDist = pickRadius;
    for (int i = 0; i < g_gridSize; ++i) {
        for (int j = 0; j < g_gridSize; ++j) {
            int sx, sy;
            if (!projectToScreen(g_grid[i][j], sx, sy)) continue;
            int dx = sx - mx;
            int dy = sy - my;
            int dist = (int)std::sqrt(dx * dx + dy * dy);
            if (dist < bestDist) {
                bestDist = dist;
                g_selectedRow = i;
                g_selectedCol = j;
            }
        }
    }
}

static void mouse(int button, int state, int x, int y) {
    g_lastMouseX = x;
    g_lastMouseY = y;
    if (button == GLUT_LEFT_BUTTON) {
        if (state == GLUT_DOWN) {
            pickControlPoint(x, y);
            g_draggingPoint = (g_selectedRow >= 0);
            if (g_draggingPoint) {
                g_dragPlaneY = g_grid[g_selectedRow][g_selectedCol].y;
                g_dragStartMouseY = y;
            }
        } else {
            g_draggingPoint = false;
            g_selectedRow = -1;
            g_selectedCol = -1;
        }
    } else if (button == GLUT_RIGHT_BUTTON) {
        g_rotating = (state == GLUT_DOWN);
    }
    glutPostRedisplay();
}

static void motion(int x, int y) {
    if (g_rotating) {
        g_rotY += (x - g_lastMouseX) * 0.4f;
        g_rotX += (y - g_lastMouseY) * 0.4f;
        g_lastMouseX = x;
        g_lastMouseY = y;
        glutPostRedisplay();
    } else if (g_draggingPoint && g_selectedRow >= 0 && g_selectedCol >= 0) {
        Vec3& pt = g_grid[g_selectedRow][g_selectedCol];
        Vec3 onPlane;
        if (unprojectToPlane(x, y, g_dragPlaneY, onPlane)) {
            pt.x = onPlane.x;
            pt.z = onPlane.z;
        }
        pt.y = g_dragPlaneY - (y - g_dragStartMouseY) * 0.006f;
        if (pt.y < -1.0f) pt.y = -1.0f;
        if (pt.y > 1.2f) pt.y = 1.2f;
        g_lastMouseX = x;
        g_lastMouseY = y;
        glutPostRedisplay();
    }
}

int main(int argc, char** argv) {
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH);
    glutInitWindowSize(g_windowWidth, g_windowHeight);
    glutCreateWindow("Exp07 Task2 - Bezier Surface (de Casteljau)");

    initGrid(3);

    glutDisplayFunc(display);
    glutReshapeFunc(reshape);
    glutKeyboardFunc(keyboard);
    glutMouseFunc(mouse);
    glutMotionFunc(motion);
    glutTimerFunc(16, update, 0);

    glutMainLoop();
    return 0;
}
