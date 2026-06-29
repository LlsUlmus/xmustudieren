# 1. 设定 CMake 的最低版本要求
cmake_minimum_required(VERSION 3.10)

# 2. 设定你的项目名称
project(MyOpenGLProject)

# 3. 设置 C++ 标准
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# 新增的第1行：告诉 CMake 在系统中寻找 Eigen3
find_package(Eigen3 REQUIRED)

# 4. 收集所有的源文件
file(GLOB_RECURSE SOURCES "src/*.cpp")

# 新增的第2行：强制将 C++ 基础库打包进 exe 中，彻底隔绝 Anaconda 环境冲突
set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -static-libgcc -static-libstdc++")
# 5. 生成可执行文件
add_executable(${PROJECT_NAME} ${SOURCES})

# 6. 链接 freeglut 和 Windows 自带的 OpenGL 旧版基础库 (opengl32 和 glu32)
# 新增的第3行：在原有的链接命令最后，追加 Eigen3::Eigen
target_link_libraries(${PROJECT_NAME} PRIVATE freeglut opengl32 glu32 Eigen3::Eigen)