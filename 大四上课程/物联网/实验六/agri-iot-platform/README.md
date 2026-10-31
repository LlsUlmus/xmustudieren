# 智慧番茄温室物联网监测平台

场景：日光温室番茄种植基地，3 栋大棚分别处于**结果期 / 开花坐果期 / 育苗期**，灌溉阈值各不相同。
平台集成五项功能：土壤与环境监测、灌溉控制、虫情监测、无人机巡检识别、摄像头监控，支持故障模拟与功能联动。

## 架构

```
 虚拟设备(simulator)  ──MQTT──▶  MQTT Broker(内置 amqtt :1883)  ──▶  服务器(server, FastAPI :8000)
   土壤/环境传感器 ×3          ◀── 灌溉指令/无人机任务/故障注入 ──        数据校验·存储(SQLite)·分析·告警·联动
   电磁阀+流量计 ×3                                                         │
   虫情测报灯 ×3                                                REST API ▲  │ WebSocket 每秒推送快照
   摄像头 ×3 / 无人机 ×1                                                 │  ▼
                                                                网页端(web, 手机/电脑自适应)
```

## 运行

需要 Python ≥ 3.10。

```bash
pip install -r requirements.txt
python run_all.py
```

浏览器打开 http://127.0.0.1:8000 。手机与电脑在同一局域网时，用手机访问 `http://电脑IP:8000`。
Windows 可直接双击 `start.bat`。

也可分开启动：`python -m server.app`（含内置 Broker），`python -m simulator.sim`。
如使用外部 Broker（如 Mosquitto），设置环境变量 `AGRI_EMBED_BROKER=0`，并用 `AGRI_MQTT_HOST/AGRI_MQTT_PORT` 指定地址。

## 演示

网页右侧「故障与场景模拟」面板可对当前大棚注入 6 类故障，以及「快速干旱」「真实虫害爆发」「全部恢复」场景。
建议平台运行 70 秒以上再演示虫情误报（z-score 需要历史基线）。

`tools/demo_capture.py` 可自动完成一遍演示，并录制视频、截取截图（需要 `pip install playwright imageio-ffmpeg` 和本机 Edge）。

## 目录

| 路径 | 说明 |
|---|---|
| `common/config.py` | 场景、设备、阈值、MQTT 主题配置 |
| `simulator/sim.py` | 虚拟设备：物理过程模型、MQTT 上报、指令响应、故障注入 |
| `simulator/imagegen.py` | 程序化生成无人机俯拍图与摄像头画面 |
| `server/app.py` | 内置 Broker、MQTT 接入、REST API、WebSocket |
| `server/core.py` | 数据校验、存储、故障检测、告警、联动规则 |
| `server/analysis.py` | 统计与趋势分析方法 |
| `server/recognition.py` | 无人机图像病虫害识别、摄像头帧差检测 |
| `web/` | 网页端（原生 JS + 自绘 SVG 图表，无外部依赖，离线可用） |
| `data/` | 运行时生成：SQLite 数据库、无人机图像、抓拍图 |

## MQTT 主题

| 主题 | 方向 | 内容 |
|---|---|---|
| `agri/{gh}/soil` | 设备→平台 | 土壤湿度/温度/EC、空气温湿度、光照、CO₂（2s） |
| `agri/{gh}/irrigation/status` | 设备→平台 | 阀门状态、流量、累计水量、压力（2s） |
| `agri/{gh}/irrigation/cmd` | 平台→设备 | 开/关阀指令（cmd_id、时长） |
| `agri/{gh}/irrigation/ack` | 设备→平台 | 指令应答 |
| `agri/{gh}/pest` | 设备→平台 | 白粉虱/蚜虫/蓟马诱捕计数（5s） |
| `agri/drone/cmd` | 平台→无人机 | 巡检任务（大棚、原因） |
| `agri/{gh}/drone/image` | 无人机→平台 | 俯拍图像（Base64 JPEG） |
| `agri/{gh}/camera/frame` | 设备→平台 | 摄像头画面（2s） |
| `agri/sim/fault`、`agri/sim/scenario` | 平台→模拟器 | 故障注入、场景切换 |
| `agri/sim/status` | 模拟器→平台 | 模拟时刻、已注入故障 |

## REST API

`GET /api/snapshot`、`/api/history/{soil|pest|irrigation}?gh=`、`/api/drone/missions`、`/api/alarms`、`/api/commands`、`/api/linkage`、`/api/camera/{gh}/latest.jpg`
`POST /api/irrigation/{gh}/command`、`/api/irrigation/{gh}/mode`、`/api/drone/{gh}/mission`、`/api/camera/{gh}/snapshot`、`/api/fault`、`/api/scenario`
FastAPI 自动文档：http://127.0.0.1:8000/docs
