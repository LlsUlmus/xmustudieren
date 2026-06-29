#include <cmath>
#include <array>

#if defined(__has_include)
#  if __has_include(<GL/freeglut.h>)
#    include <GL/freeglut.h>
#  else
#    include <GL/glut.h>
#  endif
#else
#  include <GL/glut.h>
#endif

// 实心圆
namespace {
constexpr float kPi = 3.14159265358979323846f;

struct Color {
  float r, g, b;
};

constexpr int kSegments = 6;  
constexpr float kRadius = 0.85f;

std::array<Color, kSegments> kColors{
    Color{0.0f, 0.55f, 1.0f},  // 蓝
    Color{1.0f, 0.1f, 0.1f},   // 红
    Color{1.0f, 0.9f, 0.1f},   // 黄
    Color{0.1f, 0.85f, 0.25f}, // 绿
    Color{0.75f, 0.15f, 0.9f},  // 紫
    Color{1.0f, 0.55f, 0.05f},  // 橙
};

// 圆饼
float gAngleDeg = 0.0f; 
bool gAutoRotate = false;

void ApplyOrtho(int w, int h) {
  if (h <= 0) h = 1;
  const float aspect = static_cast<float>(w) / static_cast<float>(h);

  glViewport(0, 0, w, h);
  glMatrixMode(GL_PROJECTION);
  glLoadIdentity();

  // 保持宽高比，避免窗口拉伸导致圆变形
  if (aspect >= 1.0f) {
    glOrtho(-aspect, aspect, -1.0, 1.0, -1.0, 1.0);
  } else {
    glOrtho(-1.0, 1.0, -1.0f / aspect, 1.0f / aspect, -1.0, 1.0);
  }

  glMatrixMode(GL_MODELVIEW);
  glLoadIdentity();
}

void DrawPie() {
  const float angleRad = gAngleDeg * kPi / 180.0f;
  const float startOffset = -kPi / 2.0f; // 从上方开始

  // 更密的圆周采样
  constexpr int kSubPerSector = 32; // 每个扇形细分多少段
  const float sectorSpan = 2.0f * kPi / static_cast<float>(kSegments);
  const int totalSub = kSegments * kSubPerSector;

  glBegin(GL_TRIANGLES);
  for (int i = 0; i < totalSub; ++i) {
    const float t0 = startOffset + angleRad + static_cast<float>(i) * (sectorSpan / kSubPerSector);
    const float t1 = startOffset + angleRad + static_cast<float>(i + 1) * (sectorSpan / kSubPerSector);

    const int sectorIdx = i / kSubPerSector;
    const Color c = kColors[static_cast<size_t>(sectorIdx)];
    glColor3f(c.r, c.g, c.b);

    // 小三角扇：圆心 + 两个相邻圆周点
    glVertex2f(0.0f, 0.0f);
    glVertex2f(kRadius * std::cos(t0), kRadius * std::sin(t0));
    glVertex2f(kRadius * std::cos(t1), kRadius * std::sin(t1));
  }
  glEnd();
}

void Display() {
  glClear(GL_COLOR_BUFFER_BIT);
  glLoadIdentity();

  DrawPie();

  glutSwapBuffers();
}

void Idle() {
  if (gAutoRotate) {
    gAngleDeg += 0.5f;
    if (gAngleDeg >= 360.0f) gAngleDeg -= 360.0f;
  }
  glutPostRedisplay();
}

void Reshape(int w, int h) {
  ApplyOrtho(w, h);
}

void Keyboard(unsigned char key, int /*x*/, int /*y*/) {
  switch (key) {
    case 'p':
    case 'P':
      gAutoRotate = !gAutoRotate; // 附加题可选：暂停/继续
      break;
    case 'r':
    case 'R':
      gAngleDeg = 0.0f;
      break;
    default:
      break;
  }
}
} // namespace

int main(int argc, char** argv) {
  glutInit(&argc, argv);
  glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB);
  glutInitWindowSize(600, 600);
  glutCreateWindow("exp01_task2");

  glClearColor(1.0f, 1.0f, 1.0f, 1.0f);

  glutReshapeFunc(Reshape);
  glutDisplayFunc(Display);
  glutIdleFunc(Idle);
  glutKeyboardFunc(Keyboard);

  // 初次设置投影
  Reshape(600, 600);
  glutMainLoop();
  return 0;
}

