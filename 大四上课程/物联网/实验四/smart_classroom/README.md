# 智慧教室环境监测与节能控制系统（实验四）

Python 模拟教室里的环境监测节点和智能控制终端，通过 MQTT 每 5 秒向 OneNET 上报一次数据；
监控后端通过 OneNET 开放 API 读取云端最新数据，网页大屏用仪表盘、实时曲线、状态指示灯动态展示，并可远程开关空调、灯光、新风。

```
device_simulator.py ──MQTT──▶ OneNET 云平台 ──HTTP API──▶ server.py ──▶ 浏览器大屏
        ▲                         │                          │
        └──── 属性设置（MQTT 下行）◀──── set-device-property ◀─┘（点击“切换”）
```

## 文件说明

| 文件 | 作用 |
| --- | --- |
| `config.json` | OneNET 产品 ID、密钥、设备列表、上报周期 |
| `onenet.py` | 生成 OneNET 鉴权 token、拼接 Topic |
| `classroom_model.py` | 教室环境仿真模型（数据生成 + 节能联动逻辑） |
| `device_simulator.py` | 模拟设备：MQTT 连接、周期上报、接收控制命令 |
| `server.py` | 监控后端：轮询 OneNET API，向网页提供数据 |
| `static/index.html` | 可视化大屏（ECharts） |

## 一、OneNET 平台配置

1. 登录 [OneNET](https://open.iot.10086.cn/) → 控制台 → 产品开发 → 创建产品。
   接入协议选 **MQTT**，数据协议选 **OneJson**，联网方式选 Wi-Fi。
2. 在产品的“物模型”里添加以下 9 个自定义属性（标识符必须完全一致）：

   | 名称 | 标识符 | 数据类型 | 读写 | 取值 |
   | --- | --- | --- | --- | --- |
   | 温度 | `temperature` | float | 只读 | -10 ~ 60，步长 0.1，℃ |
   | 湿度 | `humidity` | float | 只读 | 0 ~ 100，步长 0.1，%RH |
   | CO2 浓度 | `co2` | int32 | 只读 | 400 ~ 5000，ppm |
   | PM2.5 | `pm25` | int32 | 只读 | 0 ~ 500，μg/m³ |
   | 光照强度 | `lux` | int32 | 只读 | 0 ~ 65535，lx |
   | 人员存在 | `occupied` | bool | 只读 | true 有人 / false 无人 |
   | 空调开关 | `ac` | bool | **读写** | true 开 / false 关 |
   | 灯光开关 | `light` | bool | **读写** | true 开 / false 关 |
   | 新风开关 | `fan` | bool | **读写** | true 开 / false 关 |

3. 设备管理 → 添加设备，设备名称填 `classroom-101`。
4. 把以下信息填进 `config.json`：
   - `product_id`：产品 ID
   - `devices[].key`：设备详情页的设备密钥
   - `mqtt_host`：产品详情页“接入域名”中的 `{产品ID}.mqtts.acc.cmcconenet.cn`

   监控后端默认用设备密钥生成设备级 token 调用 OneNET API，`product_access_key`、
   `user_id` / `user_access_key` 留空即可；如果填写，则优先使用用户级、其次产品级鉴权。

## 二、运行

```bash
pip install -r requirements.txt
```

开两个终端，分别运行：

```bash
python device_simulator.py
```

```bash
python server.py
```

然后浏览器打开 <http://127.0.0.1:5000>。Windows 下也可以直接双击 `start.bat` 一键启动
（其中 Python 路径为 `D:\Miniforge3\python.exe`，换电脑运行时改成自己的 `python`）。

还没配好云平台时，可以用 `python server.py --offline` 先看界面效果（数据在本地生成，不经过云平台，**不能用于实验结果验证**）。

## 三、数据与联动逻辑

- 上报周期：5 秒（`config.json` 的 `report_interval`）。
- 上报 Topic：`$sys/{产品ID}/{设备名}/thing/property/post`
- 控制 Topic：`$sys/{产品ID}/{设备名}/thing/property/set`
- 仿真联动（时间做了压缩，方便 1 分钟内演示）：
  - 检测到有人 → 自动开灯，温度高于 26.5 ℃ 时开空调；
  - CO2 > 1000 ppm → 自动开新风，降到 700 ppm 以下自动关闭；
  - 连续 60 秒无人（实际方案为 15 分钟）→ 自动关空调和灯光。
