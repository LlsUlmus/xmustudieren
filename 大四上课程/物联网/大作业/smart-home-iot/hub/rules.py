"""
环境自动化规则引擎（感知 → 判断 → 响应）

每条规则 = 条件函数 + 动作函数。规则在条件由“假”变“真”（上升沿）时触发一次，
避免在条件持续成立时反复下发命令；用户手动操作某设备后，该设备在一段时间内
不被自动规则改写（用户优先）。
"""
import time
from dataclasses import dataclass
from typing import Callable

MANUAL_OVERRIDE_SECONDS = 60  # 手动操作后自动规则让步的时长（真实秒）


@dataclass
class Rule:
    rule_id: str
    name: str
    description: str
    condition: Callable[[dict], bool]
    action: Callable[[dict, dict], list]  # (state, profile) -> commands
    devices: tuple = ()
    respect_override: bool = True  # 是否对用户手动操作让步


def _night(env):
    h = env.get("hour", 12)
    return h >= 18 or h < 6


def _light_cmd(state, profile):
    env = state["env"]
    if _night(env):
        ct, br = profile.get("color_temp", 3000), profile.get("brightness", 70)
        ct = min(ct, 3500)  # 夜间偏暖，减少蓝光
    else:
        ct, br = profile.get("color_temp", 4500), profile.get("brightness", 80)
    return [{"device": "light", "params": {"on": True, "brightness": br, "color_temp": ct}}]


RULES = [
    Rule("R1", "人来灯亮", "检测到有人且照度 < 150 lux 且灯关闭 → 按用户偏好开灯（夜间自动暖光）",
         lambda s: s["env"].get("presence") and s["env"].get("illuminance", 999) < 150
         and not s["devices"]["light"].get("on"),
         _light_cmd, ("light",)),
    Rule("R2", "人走设备关", "无人持续 ≥ 10 分钟（模拟时间）且仍有设备开启 → 关闭灯/风扇/空调/音箱节能",
         lambda s: (not s["env"].get("presence")) and s["env"].get("vacant_minutes", 0) >= 10
         and any(s["devices"][d].get("on") for d in ("light", "fan", "ac", "speaker")),
         lambda s, p: [{"device": d, "params": {"on": False}} for d in ("light", "fan", "ac", "speaker")
                       if s["devices"][d].get("on")],
         ("light", "fan", "ac", "speaker"), respect_override=False),  # 无人时不存在“用户优先”
    Rule("R3", "高温自动制冷", "有人且室温 > 28℃ 且空调关闭 → 空调制冷到用户偏好温度",
         lambda s: s["env"].get("presence") and s["env"].get("temperature", 0) > 28
         and not s["devices"]["ac"].get("on"),
         lambda s, p: [{"device": "ac", "params": {"on": True, "mode": "cool", "target": p.get("ac_target", 26)}}],
         ("ac",)),
    Rule("R4", "日落关窗帘", "时间到 19:00 以后且窗帘未关 → 自动关闭窗帘保护隐私",
         lambda s: (s["env"].get("hour", 12) >= 19 or s["env"].get("hour", 12) < 6)
         and s["devices"]["curtain"].get("position", 0) > 0,
         lambda s, p: [{"device": "curtain", "params": {"position": 0}}],
         ("curtain",)),
    Rule("R5", "潮湿自动除湿", "有人且湿度 > 78% 且空调关闭 → 空调除湿模式",
         lambda s: s["env"].get("presence") and s["env"].get("humidity", 0) > 78
         and not s["devices"]["ac"].get("on"),
         lambda s, p: [{"device": "ac", "params": {"on": True, "mode": "dry", "target": 26}}],
         ("ac",)),
]


class RuleEngine:
    def __init__(self):
        self.enabled = True
        self.prev = {r.rule_id: False for r in RULES}
        self.manual = {}  # device -> 最近一次手动操作时间

    def note_manual(self, device):
        self.manual[device] = time.time()

    def overridden(self, rule):
        if not rule.respect_override:
            return False
        now = time.time()
        return any(now - self.manual.get(d, 0) < MANUAL_OVERRIDE_SECONDS for d in rule.devices)

    def evaluate(self, state, profile):
        """返回 [(rule, commands), ...]"""
        fired = []
        for rule in RULES:
            try:
                cond = bool(rule.condition(state))
            except (KeyError, TypeError):
                cond = False
            rising = cond and not self.prev[rule.rule_id]
            self.prev[rule.rule_id] = cond
            if rising and self.enabled and not self.overridden(rule):
                cmds = rule.action(state, profile or {})
                if cmds:
                    fired.append((rule, cmds))
        return fired

    def describe(self):
        return [{"id": r.rule_id, "name": r.name, "description": r.description} for r in RULES]
