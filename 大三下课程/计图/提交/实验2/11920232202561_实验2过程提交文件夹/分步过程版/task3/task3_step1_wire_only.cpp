#include <GL/glut.h>
#include <cmath>

void drawWireSphere(float radius,int stacks,int slices){
    glColor3f(1,0.2f,0.2f);
    for(int i=0;i<=stacks;++i){ float v=-3.1415926f/2.0f + 3.1415926f*i/stacks; float y=radius*sinf(v), r=radius*cosf(v); glBegin(GL_LINE_LOOP); for(int j=0;j<slices;++j){ float u=2.0f*3.1415926f*j/slices; glVertex3f(r*cosf(u),y,r*sinf(u)); } glEnd(); }
    for(int j=0;j<slices;++j){ float u=2.0f*3.1415926f*j/slices; glBegin(GL_LINE_STRIP); for(int i=0;i<=stacks;++i){ float v=-3.1415926f/2.0f + 3.1415926f*i/stacks; float y=radius*sinf(v), r=radius*cosf(v); glVertex3f(r*cosf(u),y,r*sinf(u)); } glEnd(); }
}
void display(){ glClear(GL_COLOR_BUFFER_BIT|GL_DEPTH_BUFFER_BIT); glLoadIdentity(); glTranslatef(0,0,-5); drawWireSphere(1.5f,28,56); glutSwapBuffers(); }
void reshape(int w,int h){ if(h==0)h=1; glViewport(0,0,w,h); glMatrixMode(GL_PROJECTION); glLoadIdentity(); gluPerspective(55.0,(double)w/h,0.1,100); glMatrixMode(GL_MODELVIEW);} 
int main(int argc,char**argv){ glutInit(&argc,argv); glutInitDisplayMode(GLUT_DOUBLE|GLUT_RGB|GLUT_DEPTH); glutInitWindowSize(900,600); glutCreateWindow("task3 step1"); glClearColor(0.06f,0.06f,0.08f,1); glEnable(GL_DEPTH_TEST); glutDisplayFunc(display); glutReshapeFunc(reshape); glutMainLoop(); return 0; }
