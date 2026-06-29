#include <cmath>

#if defined(__has_include)
#  if __has_include(<GL/freeglut.h>)
#    include <GL/freeglut.h>
#  else
#    include <GL/glut.h>
#  endif
#else
#  include <GL/glut.h>
#endif

namespace {
constexpr float kPi = 3.14159265358979323846f;

struct Color {
  float r, g, b;
};

constexpr float kOuterR = 0.25f;
constexpr float kInnerR = 0.2f;
constexpr int kRingSamples = 3600;

constexpr float kZFull = 0.02f;
constexpr float kZOverlay = -0.02f;

int gMode = 2;       // 1: 单环(蓝)  2: 五环(不遮挡)  3: 五环(遮挡)
bool gDepthOn = true;

inline int clampWrapIndex(int i) {
  int r = i % kRingSamples;
  if (r < 0) r += kRingSamples;
  return r;
}

int angleToSampleIndex(float angleRad) {
  float twoPi = 2.0f * kPi;
  float a = std::fmod(angleRad, twoPi);
  if (a < 0.0f) a += twoPi;
  float t = a / twoPi;
  return clampWrapIndex(static_cast<int>(std::lround(t * kRingSamples)));
}

void drawRingBySampleRange(int iStart, int iEnd, float cx, float cy, const Color& color, float z) {
  glColor3f(color.r, color.g, color.b);
  glBegin(GL_LINES);

  if (iStart <= iEnd) {
    for (int i = iStart; i <= iEnd; ++i) {
      const float ang = 2.0f * kPi * static_cast<float>(i) / static_cast<float>(kRingSamples);
      const float x0 = cx + kInnerR * std::sin(ang);
      const float y0 = cy + kInnerR * std::cos(ang);
      const float x1 = cx + kOuterR * std::sin(ang);
      const float y1 = cy + kOuterR * std::cos(ang);
      glVertex3f(x0, y0, z);
      glVertex3f(x1, y1, z);
    }
  } else {
    for (int i = iStart; i < kRingSamples; ++i) {
      const float ang = 2.0f * kPi * static_cast<float>(i) / static_cast<float>(kRingSamples);
      const float x0 = cx + kInnerR * std::sin(ang);
      const float y0 = cy + kInnerR * std::cos(ang);
      const float x1 = cx + kOuterR * std::sin(ang);
      const float y1 = cy + kOuterR * std::cos(ang);
      glVertex3f(x0, y0, z);
      glVertex3f(x1, y1, z);
    }
    for (int i = 0; i <= iEnd; ++i) {
      const float ang = 2.0f * kPi * static_cast<float>(i) / static_cast<float>(kRingSamples);
      const float x0 = cx + kInnerR * std::sin(ang);
      const float y0 = cy + kInnerR * std::cos(ang);
      const float x1 = cx + kOuterR * std::sin(ang);
      const float y1 = cy + kOuterR * std::cos(ang);
      glVertex3f(x0, y0, z);
      glVertex3f(x1, y1, z);
    }
  }

  glEnd();
}

void drawRingFull(float cx, float cy, const Color& color, float z) {
  drawRingBySampleRange(0, kRingSamples - 1, cx, cy, color, z);
}

void drawRingArcByAngle(float cx, float cy, float startAngleRad, float endAngleRad, const Color& color, float z) {
  int iStart = angleToSampleIndex(startAngleRad);
  int iEnd = angleToSampleIndex(endAngleRad);
  drawRingBySampleRange(iStart, iEnd, cx, cy, color, z);
}

void ApplyOrtho(int w, int h) {
  if (h <= 0) h = 1;
  const float aspect = static_cast<float>(w) / static_cast<float>(h);

  glViewport(0, 0, w, h);
  glMatrixMode(GL_PROJECTION);
  glLoadIdentity();

  if (aspect >= 1.0f) {
    glOrtho(-aspect, aspect, -1.0f, 1.0f, -1.0f, 1.0f);
  } else {
    glOrtho(-1.0f, 1.0f, -1.0f / aspect, 1.0f / aspect, -1.0f, 1.0f);
  }

  glMatrixMode(GL_MODELVIEW);
  glLoadIdentity();
}

void Display() {
  glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
  glLoadIdentity();

  const Color blue{0.0f, 0.5f, 0.8f};
  const Color black{0.0f, 0.0f, 0.0f};
  const Color red{1.0f, 0.0f, 0.0f};
  const Color yellow{1.0f, 0.8f, 0.0f};
  const Color green{0.1f, 0.6f, 0.0f};

  constexpr float spacingX = 0.55f;
  constexpr float topY = 0.25f;
  constexpr float bottomY = 0.0f;
  const float cxBlack = 0.0f, cyBlack = topY;
  const float cxRed = spacingX, cyRed = topY;
  const float cxBlue = -spacingX, cyBlue = topY;
  const float cxYellow = -spacingX / 2.0f, cyYellow = bottomY;
  const float cxGreen = spacingX / 2.0f, cyGreen = bottomY;

  const float a1 = kPi / 2.0f;
  const float a2 = 13.0f * kPi / 18.0f;
  const float b1 = kPi;
  const float b2 = 11.0f * kPi / 9.0f;

  if (gMode == 1) {
    drawRingFull(cxBlue, cyBlue, blue, kZFull);
  } else if (gMode == 2) {
    drawRingFull(cxBlack, cyBlack, black, kZFull);
    drawRingFull(cxRed, cyRed, red, kZFull);
    drawRingFull(cxBlue, cyBlue, blue, kZFull);
    drawRingFull(cxYellow, cyYellow, yellow, kZFull);
    drawRingFull(cxGreen, cyGreen, green, kZFull);
  } else {
    if (gDepthOn) glDepthFunc(GL_LESS);
    if (gDepthOn) {
      drawRingArcByAngle(cxBlack, cyBlack, a1, a2, black, kZOverlay);
      drawRingArcByAngle(cxBlack, cyBlack, b1, b2, black, kZOverlay);
      drawRingArcByAngle(cxRed, cyRed, b1, b2, red, kZOverlay);
      drawRingArcByAngle(cxBlue, cyBlue, a1, a2, blue, kZOverlay);

      drawRingFull(cxBlack, cyBlack, black, kZFull);
      drawRingFull(cxRed, cyRed, red, kZFull);
      drawRingFull(cxBlue, cyBlue, blue, kZFull);
      drawRingFull(cxYellow, cyYellow, yellow, kZFull);
      drawRingFull(cxGreen, cyGreen, green, kZFull);
    } else {
      drawRingArcByAngle(cxBlack, cyBlack, a1, a2, black, 0.0f);
      drawRingArcByAngle(cxBlack, cyBlack, b1, b2, black, 0.0f);
      drawRingArcByAngle(cxRed, cyRed, b1, b2, red, 0.0f);
      drawRingArcByAngle(cxBlue, cyBlue, a1, a2, blue, 0.0f);

      drawRingFull(cxBlack, cyBlack, black, 0.0f);
      drawRingFull(cxRed, cyRed, red, 0.0f);
      drawRingFull(cxBlue, cyBlue, blue, 0.0f);
      drawRingFull(cxYellow, cyYellow, yellow, 0.0f);
      drawRingFull(cxGreen, cyGreen, green, 0.0f);
    }
  }

  glutSwapBuffers();
}

void Reshape(int w, int h) {
  ApplyOrtho(w, h);
}

void Keyboard(unsigned char key, int /*x*/, int /*y*/) {
  if (key == '1') gMode = 1;
  else if (key == '2') gMode = 2;
  else if (key == '3') gMode = 3;
  else if (key == 'd' || key == 'D') gDepthOn = !gDepthOn;

  if (gDepthOn) glEnable(GL_DEPTH_TEST);
  else glDisable(GL_DEPTH_TEST);

  glutPostRedisplay();
}

} // namespace

int main(int argc, char** argv) {
  glutInit(&argc, argv);
  glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH);
  glutInitWindowSize(700, 400);
  glutCreateWindow("exp01_task3");

  glClearColor(1.0f, 1.0f, 1.0f, 1.0f);
  glEnable(GL_DEPTH_TEST);
  glDepthFunc(GL_LESS);
  glClearDepth(1.0);

  glutReshapeFunc(Reshape);
  glutDisplayFunc(Display);
  glutKeyboardFunc(Keyboard);
  glutPostRedisplay();
  glutMainLoop();
  return 0;
}

