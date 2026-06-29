# Qt实验代码剖析报告

## 选择的三个工程

1. TCP网络编程（tcpServer + tcpClient）
2. Circularprogressbar（自定义控件）
3. Stopwatch（信号与槽的应用）

---

## 一、TCP网络编程工程

### 1.1 文件关系结构

```
TCP/
├── tcpServer/                    # TCP服务器端
│   ├── main.cpp                  # 程序入口，创建Widget并显示
│   ├── widget.h                  # Widget类声明（服务器端）
│   ├── widget.cpp                # Widget类实现（核心文件）
│   ├── widget.ui                 # UI界面设计文件
│   └── tcpServer.pro             # 项目配置文件
│
└── tcpClient/                    # TCP客户端
    ├── main.cpp                  # 程序入口，创建Widget并显示
    ├── widget.h                  # Widget类声明（客户端）
    ├── widget.cpp                # Widget类实现（核心文件）
    ├── widget.ui                 # UI界面设计文件
    └── tcpClient.pro             # 项目配置文件
```

文件关系说明：
1. main.cpp 创建并启动 Widget 窗口
2. widget.h 定义 Widget 类的接口和成员变量
3. widget.cpp 实现 Widget 类的功能逻辑
4. widget.ui 通过Qt Designer设计的界面布局

### 1.2 工作原理分析

#### TCP服务器端（tcpServer）工作流程：

1. 初始化阶段：
   创建 QTcpServer 对象
   监听本地主机的6666端口
   连接 newConnection() 信号到 sendMessage() 槽函数

2. 连接处理：
   当有新客户端连接时，触发 newConnection() 信号
   自动调用 sendMessage() 槽函数

3. 数据发送：
   使用 QDataStream 将文本数据序列化
   采用数据包格式：前2字节存储数据长度，后面是实际数据
   通过 QTcpSocket 发送数据给客户端
   发送完成后断开连接

#### TCP客户端（tcpClient）工作流程：

1. 初始化阶段：
   创建 QTcpSocket 对象
   连接 readyRead() 信号到 readMessage() 槽函数（接收数据）
   连接 error() 信号到 displayError() 槽函数（错误处理）

2. 连接服务器：
   用户点击连接按钮，调用 newConnect()
   连接到服务器指定的IP和端口（默认localhost:6666）

3. 数据接收：
   当有数据到达时，触发 readyRead() 信号
   调用 readMessage() 槽函数
   先读取2字节的数据长度信息
   等待完整数据包到达后，读取实际数据
   在界面上显示接收到的消息

#### 通信协议：
使用TCP协议，保证数据传输的可靠性
数据包格式：[2字节长度][实际数据]
服务器端发送数据后立即断开连接（短连接模式）

---

## 二、Circularprogressbar自定义控件工程

### 2.1 文件关系结构

```
Circularprogressbar/
├── main.cpp                      # 程序入口，创建并显示自定义控件
├── circularprogressbar.h        # Circularprogressbar类声明
├── circularprogressbar.cpp      # Circularprogressbar类实现（核心文件）
├── circularprogressbar.ui        # UI界面文件（可能为空，因为是自定义控件）
└── Circularprogressbar.pro      # 项目配置文件
```

文件关系说明：
1. main.cpp 创建 Circularprogressbar 控件并显示
2. circularprogressbar.h 定义自定义控件的接口
3. circularprogressbar.cpp 实现自定义控件的绘制和交互逻辑
4. 继承自 QWidget，通过重写 paintEvent() 实现自定义绘制

### 2.2 工作原理分析

#### 控件结构：
1. 外圈大圆：暗灰色背景圆环（半径 = 窗口最小边/2）
2. 彩色进度环：使用圆锥渐变（QConicalGradient）绘制的扇形进度条
3. 内圈小圆：与窗口背景色相同的圆，形成环形效果

#### 工作流程：

1. 初始化：
   创建 QTimer 定时器
   连接定时器的 timeout() 信号到 decreaseColorProgress() 槽函数

2. 绘制过程（paintEvent()）：
   将坐标原点移动到窗口中心
   启用抗锯齿渲染
   依次绘制：外圈大圆 → 彩色进度环 → 内圈小圆

3. 交互控制：
   按下空格键：启动定时器，direction = true，进度增加
   释放空格键：direction = false，进度减少
   定时器触发：
     如果 direction = true：进度每次增加3.6度（1%）
     如果 direction = false：进度每次减少1度
     进度范围限制在0-360度之间

4. 视觉效果：
   使用圆锥渐变实现彩虹色效果（紫色→红色→橙色→绿色→青色→蓝色→紫色）
   进度环从-180度开始，逆时针绘制
   当进度达到360度时，形成完整的彩色圆环

---

## 三、Stopwatch秒表工程

### 3.1 文件关系结构

```
Stopwatch/
├── main.cpp                      # 程序入口，创建Dialog并显示
├── dialog.h                      # Dialog类声明（主窗口）
├── dialog.cpp                    # Dialog类实现（核心文件）
├── dialog.ui                     # Dialog的UI界面设计
├── timerr.h                      # Timerr类声明（秒表窗口）
├── timerr.cpp                    # Timerr类实现（核心文件）
├── timerr.ui                     # Timerr的UI界面设计
└── stopwatch.pro                 # 项目配置文件
```

文件关系说明：
1. main.cpp 创建 Dialog 主窗口
2. dialog.h/cpp 主窗口类，包含开始/停止按钮
3. timerr.h/cpp 秒表窗口类，负责计时和显示
4. Dialog 通过信号槽机制控制 Timerr 的启动和停止

### 3.2 工作原理分析

#### 信号槽连接关系：

```
Dialog (主窗口)
├── pushButton_start (开始按钮)
│   └── clicked() 信号 → startSlot() 槽函数
│
└── pushButton_stop (停止按钮)
    └── clicked() 信号 → stopSlot() 槽函数

Timerr (秒表窗口)
├── QTimer (定时器)
│   └── timeout() 信号 → timeUpdateSlot() 槽函数
│
└── LCD显示组件
    └── 显示格式：秒:毫秒.十分之一秒
```

#### 工作流程：

1. 主窗口（Dialog）初始化：
   连接开始按钮的 clicked() 信号到 startSlot() 槽
   连接停止按钮的 clicked() 信号到 stopSlot() 槽

2. 启动秒表（startSlot()）：
   创建 Timerr 对象（秒表窗口）
   显示秒表窗口
   调用 Timerr::start() 启动计时

3. 计时逻辑（Timerr::start()）：
   创建 QTimer 定时器，设置间隔为10毫秒
   连接定时器的 timeout() 信号到 timeUpdateSlot() 槽
   启动定时器
   立即调用一次 timeUpdateSlot() 显示初始时间

4. 时间更新（timeUpdateSlot()）：
   timerBegin 每次加1（十分之一秒计数器）
   当 timerBegin == 10 时，ms 加1，timerBegin 归零
   当 ms == 10 时，s 加1，ms 归零
   当 s == 60 时，s 归零（分钟进位，但这里只显示到秒）
   格式化显示：s:ms.timerBegin
   更新LCD显示组件

5. 停止秒表（stopSlot()）：
   调用 Timerr::stop() 停止定时器
   计时停止，但窗口和显示保持不变

#### 时间精度：
定时器间隔：10毫秒
显示精度：十分之一秒（0.1秒）
时间格式：秒:毫秒.十分之一秒（例如：5:3.7 表示5秒3.7）

---

## 总结

### 三个工程的技术要点：

1. TCP网络编程：
   使用 QTcpServer 和 QTcpSocket 实现TCP通信
   通过信号槽机制处理连接和数据传输
   使用 QDataStream 进行数据序列化

2. 自定义控件：
   继承 QWidget 并重写 paintEvent() 实现自定义绘制
   使用 QTimer 实现动画效果
   重写事件处理函数（keyPressEvent、keyReleaseEvent）实现交互

3. 信号与槽应用：
   使用Qt的信号槽机制实现对象间通信
   通过 QTimer 实现定时功能
   多窗口之间的协调控制

---

## 核心代码文件位置

详细注释版本的代码文件（位于根目录）：
1. tcpServer_widget_注释版.cpp TCP服务器核心代码（带详细注释）
2. tcpClient_widget_注释版.cpp TCP客户端核心代码（带详细注释）
3. circularprogressbar_注释版.cpp 自定义控件核心代码（带详细注释）
4. stopwatch_dialog_注释版.cpp 秒表主窗口核心代码（带详细注释）
5. stopwatch_timerr_注释版.cpp 秒表计时核心代码（带详细注释）

