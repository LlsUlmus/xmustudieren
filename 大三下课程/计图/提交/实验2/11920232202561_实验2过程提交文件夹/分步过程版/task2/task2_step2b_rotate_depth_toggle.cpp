#include <GL/glut.h>

float ang=0.0f;
bool useDepth=true;
void drawCube(){ float h=1; glBegin(GL_QUADS);
glColor3f(1,0,0); glVertex3f(-h,-h,h); glVertex3f(h,-h,h); glVertex3f(h,h,h); glVertex3f(-h,h,h);
glColor3f(0,1,0); glVertex3f(-h,-h,-h); glVertex3f(-h,h,-h); glVertex3f(h,h,-h); glVertex3f(h,-h,-h);
glColor3f(0,0,1); glVertex3f(-h,-h,-h); glVertex3f(-h,-h,h); glVertex3f(-h,h,h); glVertex3f(-h,h,-h);
glColor3f(1,1,0); glVertex3f(h,-h,-h); glVertex3f(h,h,-h); glVertex3f(h,h,h); glVertex3f(h,-h,h);
glColor3f(1,0,1); glVertex3f(-h,h,-h); glVertex3f(-h,h,h); glVertex3f(h,h,h); glVertex3f(h,h,-h);
glColor3f(0,1,1); glVertex3f(-h,-h,-h); glVertex3f(h,-h,-h); glVertex3f(h,-h,h); glVertex3f(-h,-h,h);
 glEnd(); }
void display(){ if(useDepth) glEnable(GL_DEPTH_TEST); else glDisable(GL_DEPTH_TEST); glClear(GL_COLOR_BUFFER_BIT|GL_DEPTH_BUFFER_BIT); glMatrixMode(GL_MODELVIEW); glLoadIdentity(); gluLookAt(0,0,6,0,0,0,0,1,0); glRotatef(ang,1,1,0); drawCube(); glutSwapBuffers(); }
void reshape(int w,int h){ if(h==0)h=1; glViewport(0,0,w,h); glMatrixMode(GL_PROJECTION); glLoadIdentity(); gluPerspective(60.0,(double)w/h,0.1,100); }
void idle(){ ang+=0.3f; if(ang>360)ang-=360; glutPostRedisplay(); }
void key(unsigned char k,int,int){ if(k==27) exit(0); if(k=='t'||k=='T') useDepth=!useDepth; }
int main(int argc,char**argv){ glutInit(&argc,argv); glutInitDisplayMode(GLUT_DOUBLE|GLUT_RGB|GLUT_DEPTH); glutInitWindowSize(900,600); glutCreateWindow("task2 step2b"); glClearColor(0.08f,0.08f,0.1f,1); glutDisplayFunc(display); glutReshapeFunc(reshape); glutIdleFunc(idle); glutKeyboardFunc(key); glutMainLoop(); return 0; }
