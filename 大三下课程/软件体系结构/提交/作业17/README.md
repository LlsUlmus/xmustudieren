# 作业17：观察者模式（java.util.Observable / Observer）

## 说明

使用 JDK `java.util` 包中的 `Observable` 与 `Observer`，实现课堂随机数生成与多路输出示例，并做如下改进：

- **MVC 结构**：`RandomNumberModel`（Model）、多种 Observer（View）、`RandomAppController`（Controller）
- **统计观察者**：实时输出最小/最大/平均值
- **循环通知防护**：Model 中使用标志位避免 Observer 间接再次触发通知
- **可配置参数**：运行次数、随机上界、输出间隔

## 编译与运行

**注意：源码在 `src` 目录下，不要在项目根目录直接执行 `javac model/...`。**

### 方式一：在项目根目录双击或执行脚本（推荐）

```powershell
cd "E:\大三下提交\软件体系结构\提交\作业17"
.\compile.bat
.\run.bat
```

带参数：`.\run.bat 10 50 80`

### 方式二：先进入 src 再编译

```powershell
cd "E:\大三下提交\软件体系结构\提交\作业17\src"
javac model/RandomNumberModel.java observer/DigitDisplayObserver.java observer/BarChartObserver.java observer/StatisticsObserver.java controller/RandomAppController.java Main.java
java Main
```

带参数：`java Main 10 50 80`

## 类结构

| 类 | 职责 |
|---|---|
| `model.RandomNumberModel` | 继承 `Observable`，生成随机数并 `notifyObservers` |
| `observer.DigitDisplayObserver` | 数字形式输出 |
| `observer.BarChartObserver` | 星号条形图输出 |
| `observer.StatisticsObserver` | 累计统计（改进） |
| `controller.RandomAppController` | 组装与驱动（MVC） |
| `Main` | 程序入口 |

## 提交

按作业要求将**源代码**与**运行截图**整理到 Word 文档中提交。
