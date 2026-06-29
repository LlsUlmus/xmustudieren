# Android实验项目分析报告

## 文档说明

本目录包含Android实验项目的完整分析报告，基于两次Android实验的源代码进行深入剖析。

## 文件结构

```
第四次作业/
├── README.md                          # 本文件，文档说明
├── Android实验项目分析报告.md          # 主分析报告（详细代码剖析）
└── 流程图说明.md                      # Mermaid流程图（可视化）
```

## 报告内容

### 第一次Android实验分析（5个案例）

1. **ActionBarDemo**（ActionBar和菜单）
   - 工程文件结构分析
   - Activity和Handler机制剖析
   - 文件调用关系图
   - 流程图

2. **ActivityCommunication**（组件间通讯）
   - Intent机制详解
   - Activity间通信流程
   - startActivityForResult和onActivityResult应用
   - 文件调用关系图

3. **SQLiteExam2**（数据库系统与访问）
   - SQLiteOpenHelper应用
   - 数据库CRUD操作
   - Cursor遍历机制
   - 数据库操作流程图

4. **GraphicAnimation**（图形图像）
   - Animation动画机制
   - XML和代码两种创建方式
   - AnimationSet组合动画
   - 动画执行流程

5. **NoteApp**（Android应用项目）
   - 完整应用架构分析
   - Activity + Intent + SQLite综合应用
   - RecyclerView适配器模式
   - 应用流程图

### 第二次Android实验分析（4个GPIO案例）

1. **LED灯控制**
   - JNI机制详解
   - GPIO驱动流程
   - 设备文件操作（open/ioctl/close）
   - 完整的驱动调用链

2. **蜂鸣器控制**
   - 类似LED的控制方式
   - GPIO输出控制

3. **温度采集**
   - 多线程数据采集
   - Handler机制应用
   - 持续读取传感器数据

4. **串口通信**
   - 双向数据传输
   - 串口参数配置
   - 多线程处理机制

### Android与Linux驱动对比分析

- Android方式架构分析
- Linux原生方式架构分析
- 关键差异对比
- 优势分析

## 核心知识点

### Android四大组件应用

1. **Activity**：所有案例都涉及Activity的使用
2. **Intent**：ActivityCommunication和NoteApp中详细分析
3. **Content Provider**：ContentResolverSample案例（可选）
4. **Service**：在串口通信中可扩展使用

### JNI机制

- Java到C++的桥接
- 本地库加载（System.loadLibrary）
- native方法声明和实现
- JNI函数命名规则

### GPIO驱动控制

- Linux字符设备驱动
- 设备文件操作
- ioctl系统调用
- GPIO寄存器控制

## 使用方法

### 查看主报告

直接打开 `Android实验项目分析报告.md` 文件，包含：
- 详细的代码注释
- 工程文件结构分析
- 文件调用关系图（文本格式）
- 流程图（文本格式）

### 查看流程图

打开 `流程图说明.md` 文件，包含：
- Mermaid格式的可视化流程图
- 可在支持Mermaid的环境中查看（GitHub、VS Code、Typora等）

### 在线查看流程图

如果您的Markdown查看器不支持Mermaid，可以：
1. 访问 https://mermaid.live/
2. 复制流程图代码
3. 在线查看和导出

## 技术栈

- **Android开发**：Java、XML、SQLite
- **JNI开发**：C++、JNI API
- **Linux驱动**：字符设备驱动、GPIO控制
- **系统调用**：open、ioctl、close、read、write

## 实验环境

- **第一次实验**：Android应用开发（Application项目）
- **第二次实验**：Android硬件控制（FS3399项目）

## 参考资源

- Android官方文档
- Linux设备驱动开发指南
- JNI开发文档

## 注意事项

1. 代码示例来自实际实验项目
2. 文件路径基于Windows系统
3. 流程图使用Mermaid语法，需要支持Mermaid的查看器
4. 所有代码都包含详细的中文注释

## 作者

基于Android实验源代码分析整理

## 日期

2024年


