"""教室环境仿真模型：模拟环境监测节点 + 智能控制终端的数据变化。

为了在 1 分钟的演示里能看到效果，时间被压缩了：
实际方案中“连续 15 分钟无人自动断电”在这里是连续 60 秒无人。
"""
import random

CO2_FAN_ON = 1000    # CO2 超过该值自动开新风
CO2_ALARM = 1500     # CO2 告警阈值
CO2_FAN_OFF = 700    # CO2 降到该值以下自动关新风
VACANT_STEPS = 12    # 连续无人多少个周期后自动关空调和灯（12 × 5s = 60s）

CONTROLLABLE = ("ac", "light", "fan")


class ClassroomModel:
    def __init__(self):
        self.temperature = 27.5
        self.humidity = 62.0
        self.co2 = 520.0
        self.pm25 = 28.0
        self.lux = 130.0
        self.occupied = False
        self.ac = False
        self.light = False
        self.fan = False
        self._people = 0
        self._vacant = 0
        self._fan_auto = False

    def set(self, key, value):
        """执行云端下发的控制命令。"""
        if key not in CONTROLLABLE:
            return False
        setattr(self, key, bool(value))
        if key == "fan":
            self._fan_auto = False
        return True

    def step(self):
        """推进一个采集周期，返回本周期触发的联动事件列表。"""
        events = []

        # 人员进出
        if self.occupied and random.random() < 0.04:
            self.occupied, self._people = False, 0
            events.append("教室人员离开")
        elif not self.occupied and random.random() < 0.10:
            self.occupied, self._people = True, random.randint(25, 55)
            self._vacant = 0
            self.light = True
            if self.temperature > 26.5:
                self.ac = True
            events.append(f"检测到有人（约 {self._people} 人），自动开灯")

        # 无人节能
        if not self.occupied:
            self._vacant += 1
            if self._vacant == VACANT_STEPS and (self.ac or self.light):
                self.ac = self.light = False
                events.append("连续无人，自动关闭空调和灯光")

        # CO2：有人时上升，新风开启时下降
        per_person = 9 if self.fan else 30
        co2_target = 420 + self._people * per_person
        self.co2 += (co2_target - self.co2) * 0.10 + random.uniform(-8, 8)
        self.co2 = max(400.0, self.co2)

        if self.co2 > CO2_FAN_ON and not self.fan:
            self.fan = self._fan_auto = True
            events.append(f"CO2 {int(self.co2)} ppm 超标，自动开启新风")
        elif self.co2 < CO2_FAN_OFF and self.fan and self._fan_auto:
            self.fan = self._fan_auto = False
            events.append("CO2 恢复正常，自动关闭新风")

        # 温湿度：开空调趋向 24℃ / 50%，否则趋向室外环境
        temp_target = 24.0 if self.ac else 29.0 + (1.0 if self.occupied else 0.0)
        hum_target = 50.0 if self.ac else 66.0
        self.temperature += (temp_target - self.temperature) * 0.06 + random.uniform(-0.12, 0.12)
        self.humidity += (hum_target - self.humidity) * 0.06 + random.uniform(-0.4, 0.4)

        # PM2.5 与光照
        pm_target = 12.0 if self.fan else 35.0
        self.pm25 += (pm_target - self.pm25) * 0.08 + random.uniform(-1.5, 1.5)
        self.pm25 = max(3.0, self.pm25)
        self.lux = (460.0 if self.light else 120.0) + random.uniform(-15, 15)

        return events

    def properties(self):
        """按物模型标识符输出一次上报的全部属性。"""
        return {
            "temperature": round(self.temperature, 1),
            "humidity": round(self.humidity, 1),
            "co2": int(self.co2),
            "pm25": int(self.pm25),
            "lux": int(self.lux),
            "occupied": self.occupied,
            "ac": self.ac,
            "light": self.light,
            "fan": self.fan,
        }
