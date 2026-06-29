#include <Eigen/Core>
#include "ppm.hpp"
#include "camera.hpp"
#include "raytracer.hpp"
#include <iostream>
#include <cstdio>
#include <cmath>
using namespace std;

int main()
{
	int width = 1280, height = 960;
	//PPM outrlt(width, height);
	Camera cam;
	Scene world;
	Sphere sp1(Vector3f(0, 0.5, -2), 1.0);
	Sphere sp2(Vector3f(0, -10000.5, -2), 10000.0);
	Sphere sp3(Vector3f(-2.0, 0, -2), 0.5);
	Sphere sp4(Vector3f(1.0, -0.2, -1.0), 0.3);
	Sphere sp5(Vector3f(3.0, 0, -2.2), 0.4);
	Sphere sp6(Vector3f(-1, 0.0, -0.8), 0.2);

	Material mtl, mtl2, mtl3, mtl4, mtl5;//mLx
	mtl.SetKa(Vector3f(0.5, 0.5, 0.5));
	mtl.SetKd(Vector3f(0.8, 0.6, 0.0));
	mtl.SetKs(Vector3f(0.7, 0.8, 0.8));
	mtl.SetTransparent(false);
	mtl.SetReflective(true);
	mtl.SetShiness(50);

	mtl2.SetKa(Vector3f(0.5, 0.5, 0.5));
	mtl2.SetKd(Vector3f(0.0, 0.6, 0.2));
	mtl2.SetKs(Vector3f(0.1, 1.0, 0.8));
	mtl2.SetTransparent(false);
	mtl2.SetReflective(false);
	mtl2.SetShiness(10);

	mtl3.SetKa(Vector3f(0.5, 0.5, 0.5));
	mtl3.SetKd(Vector3f(0.0, 0.3, 0.6));
	mtl3.SetKs(Vector3f(0.0, 0.0, 0.0));
	mtl3.SetTransparent(false);
	mtl3.SetReflective(false);
	mtl3.SetShiness(10);

	mtl4.SetKa(Vector3f(0.1, 0.1, 0.1));
	mtl4.SetKd(Vector3f(0.0, 0.0, 0.0));
	mtl4.SetKs(Vector3f(0.0, 0.0, 0.0));
	mtl4.SetTransparent(true);
	mtl4.SetReflective(false);
	mtl4.SetRefraction(1.33);
	mtl4.SetShiness(200);

	mtl5.SetKa(Vector3f(0.5, 0.5, 0.5));
	mtl5.SetKd(Vector3f(0.75, 0.56, 0.0));
	mtl5.SetKs(Vector3f(0.75, 0.56, 0.0));
	mtl5.SetTransparent(false);
	mtl5.SetReflective(false);
	mtl5.SetRefraction(1.33);
	mtl5.SetShiness(100);

	world.Insert(make_pair(sp1, mtl));
	world.Insert(make_pair(sp2, mtl2));
	world.Insert(make_pair(sp3, mtl3));
	world.Insert(make_pair(sp4, mtl4));
	world.Insert(make_pair(sp5, mtl5));
	world.Insert(make_pair(sp6, mtl5));   // 下落的小球（sp6）
	int ballIdx = world.ObjectCount() - 1; // 小球在场景里的下标

	//for (int j = height-1; j >= 0; --j)
	//{
		//for (int i = 0; i < width; ++i)
		//{	
			//bool test = false;
			// 生成光线
			//float u = float(i) / float(width);
			//float v = float(j) / float(height);
			//Ray ray = cam.GenerateRay(u, v);
			//Vector3f color = RayColor(ray, world, test);

			//outrlt.SetPixel(j, i, 
				//int(255.99*color[0]), int(255.99*color[1]), int(255.99*color[2]));
		//}
	//}
	//outrlt.Write2File();
	// -------- 动画参数（可按需要改）--------
const int numFrames = 40;
const float dt = 0.05f;
const float gravity = 9.8f;
const float restitution = 0.82f;   // 弹性系数 <1，越弹越低
const float groundY = -0.3f;       // 球心贴地时的高度（大地面 sp2 附近）

float vy = 0.0f;
Vector3f ballPos(-1.0f, 2.8f, -0.8f);  // 初始：空中

for (int frame = 0; frame < numFrames; ++frame)
{
	// 物理更新
	vy -= gravity * dt;
	ballPos.y() += vy * dt;

	if (ballPos.y() < groundY)
	{
		ballPos.y() = groundY;
		vy = -restitution * vy;
		if (fabs(vy) < 0.08f)
			vy = 0.0f;
	}

	// 更新场景里小球位置
	world.GetObjectPtr(ballIdx)->_center = ballPos;

	// 渲染当前帧
	PPM outrlt(width, height);
	for (int j = height - 1; j >= 0; --j)
	{
		for (int i = 0; i < width; ++i)
		{
			float u = float(i) / float(width);
			float v = float(j) / float(height);
			Ray ray = cam.GenerateRay(u, v);
			Vector3f color = RayColor(ray, world, 0);

			outrlt.SetPixel(j, i,
				int(255.99f * color[0]),
				int(255.99f * color[1]),
				int(255.99f * color[2]));
		}
	}

	char filename[64];
	sprintf(filename, "frame_%03d.ppm", frame);
	outrlt.Write2File(filename);
	cout << "saved " << filename << endl;

}
	return 0;
}