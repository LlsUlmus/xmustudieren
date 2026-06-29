# 作业12 - 建造者模式（课件例题）

按课程 **问候文档** 示例实现，在 `TextBuilder`、`HTMLBuilder` 基础上新增 **`MarkdownBuilder`**。

## 类与角色

| 角色 | 类 |
|------|-----|
| 抽象建造者 | `Builder` |
| 具体建造者 | `TextBuilder`、`HTMLBuilder`、**`MarkdownBuilder`（新增）** |
| 指挥者 | `Director` |
| 客户端 | `Main` |

## 编译运行

```powershell
cd src
javac *.java
java Main plain
java Main html
java Main markdown
```

## 与学长作业的区分

- `Director.construct()` 使用「校园一日」主题，非学长「Greeting」固定台词
- 纯文本用 `┌─┐`、`◆` 等符号，非等号边框
- HTML 输出文件名为 `daily_message.html`，并带 `class` 属性
- Markdown 用引用块 `>` 与有序列表，结尾有分隔线与说明
