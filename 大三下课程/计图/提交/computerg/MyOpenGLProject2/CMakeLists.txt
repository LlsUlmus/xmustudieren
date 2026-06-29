# 1. 设定 CMake 的最低版本要求
cmake_minimum_required(VERSION 3.10)

# 2. 设定你的项目名称
project(MyOpenGLProject)#此处名字需跟你所在文件夹名字完全相同

# 3. 设置 C++ 标准
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# 4. 收集所有的源文件
file(GLOB_RECURSE SOURCES "src/*.cpp")#此处src需跟你存放cpp的文件夹相同

# 5. 生成可执行文件
add_executable(${PROJECT_NAME} ${SOURCES})

# 6. 链接 freeglut 和 Windows 自带的 OpenGL 旧版基础库 (opengl32 和 glu32)
target_link_libraries(${PROJECT_NAME} PRIVATE freeglut opengl32 glu32)
