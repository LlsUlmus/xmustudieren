// 实验3 附加题1：PLY 读取 + 光照明暗 + 旋转光源动画
// 编译(Windows/VS 或 MinGW)：链接 opengl32 glu32 glut32
#include <GL/glut.h>

#include <algorithm>
#include <cmath>
#include <cstddef>
#include <fstream>
#include <iostream>
#include <limits>
#include <sstream>
#include <string>
#include <vector>

struct Vec3 {
    float x = 0, y = 0, z = 0;
};

static std::vector<Vec3> g_vertices;
static std::vector<Vec3> g_normals;
static std::vector<unsigned int> g_triIndices;

static Vec3 g_center{};
static float g_scale = 1.0f;
static int g_winW = 900, g_winH = 700;

static bool g_rotateLight = true;
static float g_lightAngleDeg = 0.0f;
static float g_cameraDist = 2.6f;

static Vec3 vmin3() {
    return { std::numeric_limits<float>::infinity(),
             std::numeric_limits<float>::infinity(),
             std::numeric_limits<float>::infinity() };
}
static Vec3 vmax3() {
    return { -std::numeric_limits<float>::infinity(),
             -std::numeric_limits<float>::infinity(),
             -std::numeric_limits<float>::infinity() };
}
static void expandMinMax(Vec3& mn, Vec3& mx, const Vec3& v) {
    mn.x = std::min(mn.x, v.x); mn.y = std::min(mn.y, v.y); mn.z = std::min(mn.z, v.z);
    mx.x = std::max(mx.x, v.x); mx.y = std::max(mx.y, v.y); mx.z = std::max(mx.z, v.z);
}

static bool loadPlyAscii(const std::string& path) {
    std::ifstream in(path);
    if (!in) return false;

    std::string line;
    if (!std::getline(in, line) || line != "ply") return false;

    size_t vertexCount = 0, faceCount = 0;
    bool isAscii = false;
    bool hasNormals = false;

    while (std::getline(in, line)) {
        if (line.rfind("format", 0) == 0) {
            if (line.find("ascii") != std::string::npos) isAscii = true;
        } else if (line.rfind("element vertex", 0) == 0) {
            std::istringstream ss(line);
            std::string a, b;
            ss >> a >> b >> vertexCount;
        } else if (line.rfind("element face", 0) == 0) {
            std::istringstream ss(line);
            std::string a, b;
            ss >> a >> b >> faceCount;
        } else if (line == "property float nx" || line == "property float normal_x") {
            hasNormals = true;
        } else if (line == "end_header") {
            break;
        }
    }

    if (!isAscii || vertexCount == 0 || faceCount == 0) return false;

    g_vertices.clear();
    g_normals.clear();
    g_triIndices.clear();
    g_vertices.reserve(vertexCount);
    if (hasNormals) g_normals.reserve(vertexCount);

    Vec3 mn = vmin3(), mx = vmax3();
    for (size_t i = 0; i < vertexCount; ++i) {
        if (!std::getline(in, line)) return false;
        std::istringstream ss(line);
        Vec3 p{}, n{};
        ss >> p.x >> p.y >> p.z;
        if (hasNormals) {
            ss >> n.x >> n.y >> n.z;
            g_normals.push_back(n);
        }
        g_vertices.push_back(p);
        expandMinMax(mn, mx, p);
    }

    g_center = { (mn.x + mx.x) * 0.5f, (mn.y + mx.y) * 0.5f, (mn.z + mx.z) * 0.5f };
    const float maxd = std::max({ mx.x - mn.x, mx.y - mn.y, mx.z - mn.z });
    g_scale = (maxd > 1e-6f) ? (2.0f / maxd) : 1.0f;

    for (size_t f = 0; f < faceCount; ++f) {
        if (!std::getline(in, line)) return false;
        std::istringstream ss(line);
        int n = 0;
        ss >> n;
        if (n < 3) continue;
        std::vector<unsigned int> idx(static_cast<size_t>(n));
        for (int k = 0; k < n; ++k) ss >> idx[static_cast<size_t>(k)];
        for (int i = 1; i + 1 < n; ++i) {
            g_triIndices.push_back(idx[0]);
            g_triIndices.push_back(idx[static_cast<size_t>(i)]);
            g_triIndices.push_back(idx[static_cast<size_t>(i + 1)]);
        }
    }

    return !g_triIndices.empty();
}

static void initGL() {
    glClearColor(0.94f, 0.94f, 0.94f, 1.0f);
    glEnable(GL_DEPTH_TEST);
    glShadeModel(GL_SMOOTH);
    glEnable(GL_LIGHTING);
    glEnable(GL_LIGHT0);
    glEnable(GL_NORMALIZE);

    const GLfloat lightAmbient[] = { 0.12f, 0.12f, 0.12f, 1.0f };
    const GLfloat lightDiffuse[] = { 1.0f, 1.0f, 1.0f, 1.0f };
    const GLfloat lightSpecular[] = { 0.75f, 0.75f, 0.75f, 1.0f };
    glLightfv(GL_LIGHT0, GL_AMBIENT, lightAmbient);
    glLightfv(GL_LIGHT0, GL_DIFFUSE, lightDiffuse);
    glLightfv(GL_LIGHT0, GL_SPECULAR, lightSpecular);

    const GLfloat matAmbientDiffuse[] = { 0.40f, 0.55f, 0.52f, 1.0f };
    const GLfloat matSpecular[] = { 0.35f, 0.35f, 0.35f, 1.0f };
    const GLfloat matShininess[] = { 28.0f };
    glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT_AND_DIFFUSE, matAmbientDiffuse);
    glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, matSpecular);
    glMaterialfv(GL_FRONT_AND_BACK, GL_SHININESS, matShininess);
}

static void setLightPosition() {
    const float rad = g_lightAngleDeg * 3.1415926f / 180.0f;
    const float r = 3.0f;
    GLfloat pos[] = { r * std::cos(rad), 1.6f, r * std::sin(rad), 1.0f };
    glLightfv(GL_LIGHT0, GL_POSITION, pos);
}

static void display() {
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
    gluLookAt(0.0, 0.6, g_cameraDist,
              0.0, 0.0, 0.0,
              0.0, 1.0, 0.0);

    setLightPosition();

    glPushMatrix();
    glScalef(g_scale, g_scale, g_scale);
    glTranslatef(-g_center.x, -g_center.y, -g_center.z);

    glBegin(GL_TRIANGLES);
    for (size_t i = 0; i + 2 < g_triIndices.size(); i += 3) {
        const unsigned int ia = g_triIndices[i];
        const unsigned int ib = g_triIndices[i + 1];
        const unsigned int ic = g_triIndices[i + 2];
        const Vec3& na = g_normals[ia];
        const Vec3& nb = g_normals[ib];
        const Vec3& nc = g_normals[ic];
        const Vec3& a = g_vertices[ia];
        const Vec3& b = g_vertices[ib];
        const Vec3& c = g_vertices[ic];
        glNormal3f(na.x, na.y, na.z); glVertex3f(a.x, a.y, a.z);
        glNormal3f(nb.x, nb.y, nb.z); glVertex3f(b.x, b.y, b.z);
        glNormal3f(nc.x, nc.y, nc.z); glVertex3f(c.x, c.y, c.z);
    }
    glEnd();

    glutSwapBuffers();
}

static void reshape(int w, int h) {
    g_winW = std::max(1, w);
    g_winH = std::max(1, h);
    glViewport(0, 0, (GLsizei)g_winW, (GLsizei)g_winH);
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    gluPerspective(45.0, (double)g_winW / (double)g_winH, 0.1, 100.0);
    glMatrixMode(GL_MODELVIEW);
}

static void idle() {
    if (g_rotateLight) {
        g_lightAngleDeg += 0.35f;
        if (g_lightAngleDeg >= 360.0f) g_lightAngleDeg -= 360.0f;
        glutPostRedisplay();
    }
}

static void keyboard(unsigned char key, int, int) {
    switch (key) {
    case 27:
        std::exit(0);
        break;
    case 'r':
    case 'R':
        g_rotateLight = !g_rotateLight;
        glutPostRedisplay();
        break;
    case '+':
    case '=':
        g_cameraDist = std::max(0.8f, g_cameraDist - 0.15f);
        glutPostRedisplay();
        break;
    case '-':
    case '_':
        g_cameraDist = std::min(10.0f, g_cameraDist + 0.15f);
        glutPostRedisplay();
        break;
    default:
        break;
    }
}

int main(int argc, char** argv) {
    std::string plyPath = "lizhenxiout.ply";
    if (argc >= 2) plyPath = argv[1];
    if (!loadPlyAscii(plyPath)) {
        std::cerr << "PLY 加载失败\n";
        return 1;
    }
    if (g_normals.empty()) {
        std::cerr << "当前 PLY 不含法向量，无法实现光照明暗效果\n";
        return 1;
    }

    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH);
    glutInitWindowSize(g_winW, g_winH);
    glutInitWindowPosition(80, 60);
    glutCreateWindow("exp03_task2 - PLY lighting viewer (r:rotate light, +/- zoom)");

    initGL();
    glutDisplayFunc(display);
    glutReshapeFunc(reshape);
    glutKeyboardFunc(keyboard);
    glutIdleFunc(idle);
    glutMainLoop();
    return 0;
}
