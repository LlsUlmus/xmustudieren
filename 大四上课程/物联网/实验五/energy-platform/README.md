# 绿源零碳产业园 · 能源互联网监测平台

面向**零碳园区**场景的能耗监测与可视化平台，由三部分组成：

| 组成 | 文件 | 说明 |
|---|---|---|
| 模拟数据端 | `simulator.py` | 模拟 7 类用电对象 + 屋顶光伏 + 储能电站的电表/逆变器/BMS 数据，HTTP(JSON) 周期上报，支持故障注入 |
| 服务器端 | `server.py`、`engine.py` | Flask REST 接口接入数据，SQLite 存储，统计分析、异常告警，SSE 实时推送 |
| 可视化网页端 | `static/` | ECharts 数字看板：实时看板 / 统计分析 / 告警中心 / 设备详情 / 模拟控制台 |
| 公共配置 | `config.py` | 监测对象、额定功率与告警阈值、分时电价、碳排放因子、告警规则 |

## 运行环境

- Python 3.9+（已在 3.10 上测试）
- `pip install flask requests`
- 浏览器：Chrome / Edge（ECharts 已放在 `static/echarts.min.js`，无需联网）

## 启动

Windows 下双击 `start.bat`，或手动打开两个终端：

```bash
python server.py
```

```bash
python simulator.py --start 07:00
```

浏览器访问 <http://127.0.0.1:5000/>。

模拟器参数：

| 参数 | 默认 | 含义 |
|---|---|---|
| `--interval` | 1 | 上报周期（真实秒） |
| `--step` | 5 | 每周期推进的仿真分钟数（1 仿真日 ≈ 4.8 分钟） |
| `--start` | 07:00 | 仿真起始时刻（日期为今天） |
| `--no-backfill` | - | 不补录昨日 0 点以来的历史数据（默认补录，用于今昨对比） |
| `--random-fault` | 0 | 每周期随机注入异常的概率，如 0.02 |

## 功能

1. **数据模拟与接入**：注塑车间A、装配车间B、空压站及公辅、数据中心 IT 机柜、数据中心制冷、研发办公楼、充电桩群、屋顶光伏（1.2 MWp）、储能电站（1 MWh/500 kW）；上报功率、电表累计电量、运行状态、辐照度、SOC 等。
2. **数字看板**：KPI 概览、负荷/PUE/SOC 仪表盘、带电价时段底色的实时功率曲线、监测对象卡片、园区能流桑基图、能耗占比饼图、排名、实时告警。
3. **统计分析**：分时段能耗今昨对比、区域堆叠、峰谷平用电与电费、负荷特性（峰谷差/负荷率）、对象电费估算排名、光伏与用电对比、碳排放追踪、数据中心 PUE；支持导出 CSV。
4. **异常告警**：功率超限、功率突变、数据中断、PUE 超限、光伏出力异常、储能 SOC 过低；告警横幅、卡片变色、告警确认、自动恢复。
5. **交互**：视图切换、曲线/饼图维度切换、点击设备查看详情、在线修改告警阈值、模拟控制台注入异常。

## 接口

| 方法 | 路径 | 说明 |
|---|---|---|
| POST | `/api/ingest` | 上报一个周期数据 |
| POST | `/api/ingest/batch` | 批量上报（历史补录） |
| GET | `/api/stream` | SSE 实时推送看板快照 |
| GET | `/api/snapshot` | 当前快照 JSON |
| GET | `/api/sim/commands` | 模拟器拉取故障注入指令 |
| POST | `/api/fault` | 下发故障注入 `{device, type, ticks}` |
| POST | `/api/threshold` | 修改阈值 `{device, threshold}` |
| POST | `/api/alarm/ack` | 告警确认 `{id}` |
| GET | `/api/export.csv` | 导出今日分时统计 |

## 辅助工具（可选）

- `tools/shot.py`：Playwright 自动截图
- `tools/record_demo.py`：自动录制演示视频（需 `pip install playwright imageio-ffmpeg`）
