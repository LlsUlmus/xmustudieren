#include <GL/glut.h>

void tri(float x1,float y1,float x2,float y2,float x3,float y3,int d){
    if(d==0){ glColor3f(0.2f,0.5f,1.0f); glBegin(GL_TRIANGLES); glVertex2f(x1,y1); glVertex2f(x2,y2); glVertex2f(x3,y3); glEnd(); return; }
    float x12=(x1+x2)*0.5f,y12=(y1+y2)*0.5f,x23=(x2+x3)*0.5f,y23=(y2+y3)*0.5f,x31=(x3+x1)*0.5f,y31=(y3+y1)*0.5f;
    tri(x1,y1,x12,y12,x31,y31,d-1); tri(x12,y12,x2,y2,x23,y23,d-1); tri(x31,y31,x23,y23,x3,y3,d-1);
}
void display(){ glClear(GL_COLOR_BUFFER_BIT); glLoadIdentity(); tri(-0.9f,-0.8f,0.9f,-0.8f,0.0f,0.9f,6); glutSwapBuffers(); }
void reshape(int w,int h){ if(h==0)h=1; glViewport(0,0,w,h); glMatrixMode(GL_PROJECTION); glLoadIdentity(); float a=(float)w/h; if(a>=1) glOrtho(-a,a,-1,1,-1,1); else glOrtho(-1,1,-1/a,1/a,-1,1); glMatrixMode(GL_MODELVIEW); }
int main(int argc,char**argv){ glutInit(&argc,argv); glutInitDisplayMode(GLUT_DOUBLE|GLUT_RGB); glutInitWindowSize(800,600); glutCreateWindow("task1 step1"); glClearColor(0,0,0,1); glutDisplayFunc(display); glutReshapeFunc(reshape); glutMainLoop(); return 0; }
