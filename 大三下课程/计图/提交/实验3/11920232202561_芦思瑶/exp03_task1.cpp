// 实验3 Task1：PLY 读取 + 纯色显示
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
static std::vector<unsigned int> g_triIndices; // 按三角形展开：i0,i1,i2,...

static Vec3 g_center{};
static float g_scale = 1.0f;

static int g_winW = 900, g_winH = 700;

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
    if (!in) {
        std::cerr << "无法打开 PLY: " << path << "\n";
        return false;
    }

    std::string line;
    if (!std::getline(in, line) || line != "ply") {
        std::cerr << "不是合法 PLY 文件头\n";
        return false;
    }

    size_t vertexCount = 0, faceCount = 0;
    bool isAscii = false;
    bool hasNormals = false;

    // 读 header
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

    if (!isAscii) {
        std::cerr << "当前仅支持 ascii 1.0 PLY\n";
        return false;
    }
    if (vertexCount == 0 || faceCount == 0) {
        std::cerr << "PLY 顶点/面数量为 0\n";
        return false;
    }

    g_vertices.clear();
    g_normals.clear();
    g_triIndices.clear();
    g_vertices.reserve(vertexCount);
    if (hasNormals) g_normals.reserve(vertexCount);

    // 读 vertex：支持 x y z [nx ny nz]，多余属性忽略
    Vec3 mn = vmin3(), mx = vmax3();
    for (size_t i = 0; i < vertexCount; ++i) {
        if (!std::getline(in, line)) {
            std::cerr << "读取顶点失败，数量不足\n";
            return false;
        }
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
    const float dx = mx.x - mn.x, dy = mx.y - mn.y, dz = mx.z - mn.z;
    const float maxd = std::max({ dx, dy, dz });
    g_scale = (maxd > 1e-6f) ? (2.0f / maxd) : 1.0f; // 归一化到 [-1,1] 量级

    // 读 face：property list uchar int vertex_indices
    for (size_t f = 0; f < faceCount; ++f) {
        if (!std::getline(in, line)) {
            std::cerr << "读取面失败，数量不足\n";
            return false;
        }
        std::istringstream ss(line);
        int n = 0;
        ss >> n;
        if (n < 3) continue;
        std::vector<unsigned int> idx(static_cast<size_t>(n));
        for (int k = 0; k < n; ++k) ss >> idx[static_cast<size_t>(k)];
        // 扇形三角化 (0, i, i+1)
        for (int i = 1; i + 1 < n; ++i) {
            g_triIndices.push_back(idx[0]);
            g_triIndices.push_back(idx[static_cast<size_t>(i)]);
            g_triIndices.push_back(idx[static_cast<size_t>(i + 1)]);
        }
    }

    if (g_triIndices.empty()) {
        std::cerr << "没有三角形可绘制\n";
        return false;
    }
    return true;
}

static void initGL() {
    glClearColor(0.06f, 0.06f, 0.06f, 1.0f);
    glEnable(GL_DEPTH_TEST);
    glShadeModel(GL_SMOOTH);
    glDisable(GL_LIGHTING);
    glDisable(GL_LIGHT0);
}

static void drawModelSolid() {
    // Task1：纯色显示（无光照）
    glDisable(GL_LIGHTING);
    glColor3f(1.0f, 0.2f, 0.2f); // 纯色（可改）
}

static void display() {
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
    gluLookAt(0.0, 0.6, g_cameraDist,
              0.0, 0.0, 0.0,
              0.0, 1.0, 0.0);

    drawModelSolid();

    glPushMatrix();
    glScalef(g_scale, g_scale, g_scale);
    glTranslatef(-g_center.x, -g_center.y, -g_center.z);

    glBegin(GL_TRIANGLES);
    for (size_t i = 0; i + 2 < g_triIndices.size(); i += 3) {
        const unsigned int ia = g_triIndices[i];
        const unsigned int ib = g_triIndices[i + 1];
        const unsigned int ic = g_triIndices[i + 2];

        const Vec3& a = g_vertices[ia];
        const Vec3& b = g_vertices[ib];
        const Vec3& c = g_vertices[ic];
        glVertex3f(a.x, a.y, a.z);
        glVertex3f(b.x, b.y, b.z);
        glVertex3f(c.x, c.y, c.z);
    }
    glEnd();

    glPopMatrix();

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

static void idle() {}

static void keyboard(unsigned char key, int, int) {
    switch (key) {
    case 27: // ESC
        std::exit(0);
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
        std::cerr << "PLY 加载失败。你可以把 ply 路径作为命令行参数传入。\n";
        return 1;
    }

    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH);
    glutInitWindowSize(g_winW, g_winH);
    glutInitWindowPosition(80, 60);
    glutCreateWindow("exp03_task1 - PLY pure color viewer (+/- zoom)");

    initGL();
    glutDisplayFunc(display);
    glutReshapeFunc(reshape);
    glutKeyboardFunc(keyboard);
    glutIdleFunc(idle);

    glutMainLoop();
    return 0;
}
