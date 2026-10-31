"""全局配置：场景、地块、设备、阈值、MQTT 主题。服务器与模拟器共用。"""
import os

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DATA_DIR = os.path.join(BASE_DIR, "data")
IMG_DIR = os.path.join(DATA_DIR, "images")
DB_PATH = os.path.join(DATA_DIR, "agri.db")

MQTT_HOST = os.environ.get("AGRI_MQTT_HOST", "127.0.0.1")
MQTT_PORT = int(os.environ.get("AGRI_MQTT_PORT", "1883"))
HTTP_HOST = os.environ.get("AGRI_HTTP_HOST", "0.0.0.0")
HTTP_PORT = int(os.environ.get("AGRI_HTTP_PORT", "8000"))

# 模拟时间加速：真实 1 秒 = 模拟 TIME_SCALE 秒（用于昼夜光照/温度变化）
TIME_SCALE = 120

# 场景：日光温室番茄种植基地，3 个大棚处于不同生育期，因此灌溉阈值不同
GREENHOUSES = [
    {
        "id": "GH-1", "name": "1号棚", "crop": "樱桃番茄", "stage": "结果期", "area_m2": 667,
        "moisture_low": 60.0, "moisture_high": 75.0,
        "devices": {"soil": "SN-101", "valve": "VL-101", "pest": "PT-101", "camera": "CAM-101"},
    },
    {
        "id": "GH-2", "name": "2号棚", "crop": "大果番茄", "stage": "开花坐果期", "area_m2": 800,
        "moisture_low": 55.0, "moisture_high": 70.0,
        "devices": {"soil": "SN-201", "valve": "VL-201", "pest": "PT-201", "camera": "CAM-201"},
    },
    {
        "id": "GH-3", "name": "3号棚", "crop": "番茄穴盘苗", "stage": "育苗期", "area_m2": 400,
        "moisture_low": 65.0, "moisture_high": 80.0,
        "devices": {"soil": "SN-301", "valve": "VL-301", "pest": "PT-301", "camera": "CAM-301"},
    },
]
GH_IDS = [g["id"] for g in GREENHOUSES]
DRONE_ID = "UAV-01"
PUMP_ID = "PUMP-01"

# 采集/上报周期（秒）
SOIL_INTERVAL = 2
IRR_INTERVAL = 2
PEST_INTERVAL = 5
CAMERA_INTERVAL = 2
DRONE_INTERVAL = 25          # 无人机例行巡检：每 25 秒巡检一个大棚（轮询）

# 故障检测参数
SOIL_TIMEOUT = 8             # 超过该秒数无土壤数据 → 传感器离线
CAMERA_TIMEOUT = 8           # 超过该秒数无画面 → 摄像头离线
CMD_ACK_TIMEOUT = 4          # 灌溉指令应答超时
CMD_MAX_RETRY = 1
STUCK_COUNT = 6              # 连续相同值次数 → 数据卡死

# 传感器合理量程（超出即判为异常数据）
VALID_RANGE = {
    "soil_moisture": (0.5, 100.0),
    "soil_temp": (-10.0, 50.0),
    "ec": (0.0, 8.0),
    "air_temp": (-15.0, 55.0),
    "air_humidity": (1.0, 100.0),
    "light": (0.0, 150000.0),
    "co2": (200.0, 3000.0),
}

FAULT_TYPES = {
    "sensor_abnormal": "传感器数据异常",
    "sensor_offline": "传感器中断",
    "valve_no_response": "灌溉阀门不响应",
    "pest_false_alarm": "虫情监测误报",
    "drone_image_missing": "无人机图像缺失/识别失败",
    "camera_offline": "摄像头离线",
}

PEST_SPECIES = {"whitefly": "白粉虱", "aphid": "蚜虫", "thrips": "蓟马"}

# MQTT 主题
T_SOIL = "agri/{gh}/soil"
T_IRR_STATUS = "agri/{gh}/irrigation/status"
T_IRR_CMD = "agri/{gh}/irrigation/cmd"
T_IRR_ACK = "agri/{gh}/irrigation/ack"
T_PEST = "agri/{gh}/pest"
T_DRONE_IMG = "agri/{gh}/drone/image"
T_DRONE_CMD = "agri/drone/cmd"
T_CAMERA = "agri/{gh}/camera/frame"
T_SIM_FAULT = "agri/sim/fault"
T_SIM_SCENARIO = "agri/sim/scenario"
T_SIM_STATUS = "agri/sim/status"
