# 智慧客厅 · 物联网智能交互系统

物联网课程期末大作业：一个具备 **“感知 — 判断 — 响应”** 完整闭环的智能交互系统（纯软件仿真）。

- **设备模拟**：客厅网关（温度 / 湿度 / 光照 / 人体存在传感器 + 灯 / 空调 / 风扇 / 窗帘 / 音箱执行器），内置环境物理模型
- **数据通信**：设备 ⇄ 中枢走 WebSocket（MQTT 风格 topic），界面 ⇄ 中枢走 WebSocket + HTTP REST
- **交互方式**：文字指令、🎤 语音指令（浏览器 Web Speech API）、按钮 / 滑块、环境自动感知
- **交互决策**：中文关键词匹配 NLU（含中文数字、多子句、场景、模糊表达结合环境推理）+ 自动化规则引擎（上升沿触发、手动优先）
- **扩展功能**：个性化用户画像、多设备场景联动、语音播报、事件流可视化

## 运行环境

- Python ≥ 3.8（已在 3.10 / Windows 11 测试）
- 浏览器：Edge / Chrome（语音识别需要 Edge 或 Chrome）

## 快速开始

```bash
pip install -r requirements.txt
python start.py
```

浏览器会自动打开 http://127.0.0.1:8080/ 。`start.py` 会同时启动两个进程，也可以分别手动启动：

```bash
python hub/server.py --port 8080
python devices/simulator.py --hub ws://127.0.0.1:8080/ws/device
```

## 使用说明

| 操作 | 位置 | 示例 |
|---|---|---|
| 文字 / 语音指令 | 右侧“语音 / 文字交互” | 打开灯、把灯调成暖光、空调调到二十四度、打开风扇和音乐、亮度调到百分之三十 |
| 模糊表达 | 同上 | 我有点热、好冷、太暗了、有点闷 |
| 场景模式 | 同上 | 我回来了、我要看电影、晚安、我出门了、起床了、看书 |
| 状态查询 | 同上 | 现在温度多少 |
| 按钮控制 | 左下设备卡片 | 开关、亮度/色温滑块、空调模式与温度、风扇档位 |
| 环境仿真 | 中间“环境仿真控制” | 选择身份后“人员进入/离开”、时间跳转、热浪/降温、时钟倍速 |
| 自动规则开关 | 右上角“自动规则” | 关闭后只响应用户指令 |

自动化规则：

| 编号 | 名称 | 条件 → 动作 |
|---|---|---|
| R1 | 人来灯亮 | 有人且照度 < 150 lux 且灯关 → 按用户偏好开灯（夜间强制暖光 ≤ 3500K） |
| R2 | 人走设备关 | 无人 ≥ 10 分钟（模拟时间）→ 关闭灯/风扇/空调/音箱 |
| R3 | 高温自动制冷 | 有人且室温 > 28℃ 且空调关 → 制冷到用户偏好温度 |
| R4 | 日落关窗帘 | 19:00 以后窗帘未关 → 关闭窗帘 |
| R5 | 潮湿自动除湿 | 有人且湿度 > 78% 且空调关 → 除湿模式 |

## HTTP 接口

```bash
curl http://127.0.0.1:8080/api/state
curl http://127.0.0.1:8080/api/rules
curl -X POST http://127.0.0.1:8080/api/command -H "Content-Type: application/json" -d "{\"text\": \"打开灯\"}"
curl -X POST http://127.0.0.1:8080/api/command -H "Content-Type: application/json" -d "{\"device\": \"fan\", \"params\": {\"on\": true, \"speed\": 2}}"
```

## 测试

```bash
python -m unittest discover tests -v     # 单元测试：NLU + 规则引擎（12 项）
python tests/test_system.py              # 端到端测试：自动启动中枢和模拟器，验证全链路（18 项）
```

## 演示视频自动录制（可选）

```bash
pip install playwright imageio-ffmpeg
python tools/record_demo.py              # 使用本机 Edge，输出 docs/演示视频.mp4 与 docs/screenshots/
```

## 目录结构

```
smart-home-iot/
├── start.py                 一键启动
├── requirements.txt
├── hub/
│   ├── server.py            中枢：HTTP + WebSocket 服务、设备影子、事件流
│   ├── nlu.py               中文指令解析（关键词匹配 + 环境推理）
│   └── rules.py             自动化规则引擎
├── devices/
│   └── simulator.py         客厅网关设备模拟器（传感器 + 执行器 + 环境模型）
├── web/                     交互界面（原生 HTML/CSS/JS，无外部依赖，可离线运行）
├── tests/                   单元测试与端到端测试
├── tools/                   演示录制脚本、架构图
└── docs/                    截图与演示视频
```

## 消息格式（MQTT 风格 topic）

```json
// 设备 → 中枢：遥测
{"type": "telemetry", "topic": "home/livingroom/telemetry", "ts": 1730000000.0,
 "payload": {"env": {"time": "20:15", "temperature": 28.7, "humidity": 62, "illuminance": 3,
                      "presence": true, "person": "小明"},
             "devices": {"light": {"on": false, "brightness": 60, "color_temp": 4000}, "...": {}}}}
// 中枢 → 设备：命令
{"type": "command", "cmd_id": 7, "topic": "home/livingroom/light/set",
 "device": "light", "params": {"on": true, "brightness": 95, "color_temp": 3500}, "source": "rule:R1"}
// 设备 → 中枢：回执
{"type": "ack", "cmd_id": 7, "device": "light", "ok": true, "state": {"on": true, "...": "..."}}
```
