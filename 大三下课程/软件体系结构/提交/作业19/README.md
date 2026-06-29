# 作业19：用 GUI 改写策略模式示例

基于课程 PPT 中的**石头剪刀布 + 策略模式**（`Strategy`、`WinningStrategy`、`ProbStrategy`、`Player`），使用 Java Swing 提供图形界面，可在界面中选择策略、设置随机种子并查看对战过程与统计。

## 项目结构

```
src/strategy/
  Hand.java           - 手势（石头/剪刀/布）
  Strategy.java       - 策略接口
  WinningStrategy.java - 胜则沿用策略
  ProbStrategy.java   - 概率统计策略
  Player.java         - 上下文类
  RpsGameWindow.java  - Swing 图形界面
  Main.java           - 程序入口
```

## 编译与运行

```bash
cd 作业19
javac -encoding UTF-8 -d out src/strategy/*.java
java -cp out strategy.Main
```

或在 IDE 中直接运行 `strategy.Main`。

## 提交说明

- 运行程序后截图 GUI 界面与对战日志，放入 Word 文档。
- 一并提交本目录下的 `src` 源码。
