"""
自然语言指令解析（关键词匹配 + 规则推理）

输入：用户的文字/语音转写文本、当前设备与环境状态、当前用户偏好
输出：ParseResult，包含识别出的意图、要下发的设备命令、回复语以及判断过程(trace)
"""
import re
from dataclasses import dataclass, field

# ---------------- 词典 ----------------
DEVICE_WORDS = {
    "light": ["灯光", "电灯", "照明", "台灯", "吊灯", "灯", "亮度", "色温", "暖光", "冷光"],
    "fan": ["电风扇", "风扇", "电扇"],
    "curtain": ["窗帘", "帘子"],
    "ac": ["空调", "冷气"],
    "speaker": ["音箱", "音响", "音乐", "歌曲", "歌", "音量", "播放", "下一首", "换一首"],
}
DEVICE_NAMES = {"light": "灯光", "fan": "风扇", "curtain": "窗帘", "ac": "空调", "speaker": "音箱"}

SCENES = {
    "home": ["回家", "回来了", "到家"],
    "away": ["离家", "出门", "我走了", "上班去"],
    "sleep": ["睡觉", "晚安", "睡眠", "休息了"],
    "movie": ["观影", "看电影", "电影模式"],
    "reading": ["阅读", "看书", "学习模式"],
    "morning": ["起床", "早上好", "早安"],
}
SCENE_NAMES = {"home": "回家模式", "away": "离家模式", "sleep": "睡眠模式",
               "movie": "观影模式", "reading": "阅读模式", "morning": "起床模式"}

ON_WORDS = ["打开", "开启", "启动", "开一下", "开开", "播放", "放首", "放一首", "来点", "来首"]
OFF_WORDS = ["关闭", "关掉", "关上", "关了", "停止", "暂停", "停掉", "别开"]

CN_NUM = {"零": 0, "一": 1, "二": 2, "两": 2, "三": 3, "四": 4, "五": 5,
          "六": 6, "七": 7, "八": 8, "九": 9, "十": 10, "半": 50}


def cn_to_digits(text: str) -> str:
    """把常见中文数字（如 二十六、三十、一百）转换为阿拉伯数字"""
    def conv(m):
        s = m.group(0)
        if s == "百" or s == "一百":
            return "100"
        if "十" in s:
            a, _, b = s.partition("十")
            tens = CN_NUM.get(a, 1) if a else 1
            ones = CN_NUM.get(b, 0) if b else 0
            return str(tens * 10 + ones)
        return str(CN_NUM[s]) if len(s) == 1 else s
    # 只转换“数字+量词/单位”附近的中文数字，避免把“一下”“一点”转成数字
    text = re.sub(r"[一二两三四五六七八九]?十[一二三四五六七八九]?|一百|[一二两三四五六七八九](?=\s*(度|档|级|%|成))",
                  conv, text)
    return re.sub(r"百分之(\d+)", lambda m: m.group(1) + "%", text)


@dataclass
class ParseResult:
    intent: str = "unknown"
    commands: list = field(default_factory=list)  # [{"device":..., "params": {...}}]
    reply: str = ""
    trace: list = field(default_factory=list)
    scene: str = ""
    user: str = ""

    def to_dict(self):
        return {"intent": self.intent, "commands": self.commands, "reply": self.reply,
                "trace": self.trace, "scene": self.scene}


def _find_devices(clause):
    found = []
    for dev, words in DEVICE_WORDS.items():
        for w in words:
            if w in clause:
                found.append(dev)
                break
    return found


def _find_number(clause, units):
    m = re.search(r"(\d+)\s*(" + "|".join(units) + ")", clause)
    return int(m.group(1)) if m else None


def scene_commands(scene, profile, env):
    """多设备联动场景，参数取自用户个性化偏好"""
    p = profile or {}
    night = env.get("hour", 12) >= 18 or env.get("hour", 12) < 6
    if scene == "home":
        cmds = [{"device": "light", "params": {"on": True, "brightness": p.get("brightness", 80),
                                                "color_temp": p.get("color_temp", 4000)}},
                {"device": "speaker", "params": {"on": True, "track": p.get("music", "轻音乐"), "volume": 35}}]
        if env.get("temperature", 25) > 26:
            cmds.append({"device": "ac", "params": {"on": True, "mode": "cool", "target": p.get("ac_target", 26)}})
        cmds.append({"device": "curtain", "params": {"position": 0 if night else 100}})
        return cmds
    if scene == "away":
        return [{"device": d, "params": {"on": False}} for d in ("light", "fan", "ac", "speaker")] + \
               [{"device": "curtain", "params": {"position": 0}}]
    if scene == "sleep":
        return [{"device": "light", "params": {"on": True, "brightness": 10, "color_temp": 2700}},
                {"device": "curtain", "params": {"position": 0}},
                {"device": "speaker", "params": {"on": True, "track": "白噪音", "volume": 15}},
                {"device": "ac", "params": {"on": True, "mode": "cool", "target": p.get("ac_target", 26) + 1}},
                {"device": "fan", "params": {"on": False}}]
    if scene == "movie":
        return [{"device": "light", "params": {"on": True, "brightness": 15, "color_temp": 3000}},
                {"device": "curtain", "params": {"position": 0}},
                {"device": "speaker", "params": {"on": True, "track": "电影原声", "volume": 60}}]
    if scene == "reading":
        return [{"device": "light", "params": {"on": True, "brightness": 90, "color_temp": 5000}},
                {"device": "speaker", "params": {"on": True, "track": "轻音乐", "volume": 20}}]
    if scene == "morning":
        return [{"device": "curtain", "params": {"position": 100}},
                {"device": "light", "params": {"on": True, "brightness": 70, "color_temp": 5500}},
                {"device": "speaker", "params": {"on": True, "track": "晨间新闻", "volume": 30}}]
    return []


def _status_reply(env, dev):
    def onoff(d):
        return "开" if dev.get(d, {}).get("on") else "关"
    person = env.get("person") or "无人"
    return (f"当前室内 {env.get('temperature', 0):.1f}℃，湿度 {env.get('humidity', 0):.0f}%，"
            f"照度 {env.get('illuminance', 0):.0f} lux，室外 {env.get('outdoor_temp', 0):.1f}℃；"
            f"人员：{person}。灯光{onoff('light')}、风扇{onoff('fan')}、空调{onoff('ac')}、"
            f"窗帘开合 {dev.get('curtain', {}).get('position', 0)}%。")


def parse(text: str, state: dict, profile: dict = None, user: str = "") -> ParseResult:
    env = state.get("env", {})
    dev = state.get("devices", {})
    r = ParseResult(user=user)
    raw = text.strip()
    t = cn_to_digits(raw.replace(" ", ""))
    r.trace.append(f"输入文本：「{raw}」")
    if t != raw.replace(" ", ""):
        r.trace.append(f"数字归一化：「{t}」")
    if not t:
        r.reply = "我没有听清，请再说一次。"
        return r

    # 0. 问候 / 帮助
    if re.fullmatch(r"(你好|您好|hi|hello|在吗|帮助|help|你能做什么|你会什么)[！!？?。]*", t, re.I):
        r.intent = "help"
        r.trace.append("匹配问候/帮助关键词 → 意图：帮助")
        r.reply = ("你好，我是智慧客厅助手。可以控制灯光、空调、风扇、窗帘、音箱，"
                   "例如：“打开灯”“空调调到24度”“我有点热”“我要看电影”“晚安”“现在温度多少”。")
        return r

    # 1. 状态查询
    if re.search(r"(多少度|几度|温度多少|湿度|状态|怎么样|情况)", t) and not re.search(r"(调|设|开|关)", t):
        r.intent = "query"
        r.trace.append("匹配查询类关键词 → 意图：状态查询")
        r.reply = _status_reply(env, dev)
        return r

    # 2. 场景模式（多设备联动）
    for scene, words in SCENES.items():
        hit = next((w for w in words if w in t), None)
        if hit:
            r.intent, r.scene = "scene", scene
            r.commands = scene_commands(scene, profile, env)
            r.trace.append(f"命中场景关键词「{hit}」→ {SCENE_NAMES[scene]}")
            if profile and scene in ("home", "sleep"):
                r.trace.append(f"读取用户「{user}」偏好：色温{profile.get('color_temp')}K，"
                               f"亮度{profile.get('brightness')}%，空调{profile.get('ac_target')}℃")
            r.trace.append(f"生成联动命令 {len(r.commands)} 条")
            greet = {"home": f"欢迎回家{('，' + user) if user else ''}！", "away": "已切换离家模式，注意安全。",
                     "sleep": "晚安，已为你准备好睡眠环境。", "movie": "观影模式已开启，享受电影吧。",
                     "reading": "阅读模式已开启。", "morning": "早上好！窗帘已为你打开。"}
            r.reply = greet[scene]
            return r

    # 3. 设备控制（支持一句话多子句、多设备）
    clauses = [c for c in re.split(r"[，,。；;！!]|然后|并且|再把|同时", t) if c]
    last_action = None
    for clause in clauses:
        devices = _find_devices(clause)
        if not devices:
            continue
        params = {}
        if any(w in clause for w in OFF_WORDS) or re.search(r"关(?!于)", clause):
            params["on"] = False
            last_action = "off"
        elif any(w in clause for w in ON_WORDS) or "开" in clause:
            params["on"] = True
            last_action = "on"
        for d in devices:
            p = dict(params)
            if d == "light":
                b = _find_number(clause, ["%", "成", "亮度"])
                if b is None:
                    m = re.search(r"亮度(\d+)", clause)
                    b = int(m.group(1)) if m else None
                if b is not None:
                    p["brightness"] = b * 10 if "成" in clause and b <= 10 else b
                if re.search(r"(调亮|亮一[点些]|更亮|亮点)", clause):
                    p["brightness"] = min(100, dev.get("light", {}).get("brightness", 60) + 20)
                if re.search(r"(调暗|暗一[点些]|更暗|暗点)", clause):
                    p["brightness"] = max(5, dev.get("light", {}).get("brightness", 60) - 20)
                if re.search(r"(暖光|暖色|黄光|温馨)", clause):
                    p["color_temp"] = 2700
                if re.search(r"(冷光|白光|冷色|自然光)", clause):
                    p["color_temp"] = 6000
                if ("brightness" in p or "color_temp" in p) and "on" not in p:
                    p["on"] = True
            elif d == "fan":
                s = _find_number(clause, ["档", "级"])
                if s is not None:
                    p["speed"] = s
                if re.search(r"(大一[点些]|调大|快一[点些])", clause):
                    p["speed"] = min(3, dev.get("fan", {}).get("speed", 1) + 1)
                if re.search(r"(小一[点些]|调小|慢一[点些])", clause):
                    p["speed"] = max(1, dev.get("fan", {}).get("speed", 1) - 1)
                if "speed" in p and "on" not in p:
                    p["on"] = True
            elif d == "curtain":
                pos = _find_number(clause, ["%"])
                if "50" in clause or "一半" in clause:
                    pos = 50
                if pos is not None:
                    p = {"position": pos}
                elif p.get("on") is True or "拉开" in clause:
                    p = {"position": 100}
                elif p.get("on") is False or "拉上" in clause or "合上" in clause:
                    p = {"position": 0}
            elif d == "ac":
                tgt = _find_number(clause, ["度", "℃"])
                if tgt is not None:
                    p["target"] = tgt
                if "制冷" in clause:
                    p["mode"] = "cool"
                elif "制热" in clause or "暖风" in clause:
                    p["mode"] = "heat"
                elif "除湿" in clause:
                    p["mode"] = "dry"
                if re.search(r"(调高|高一点|升高)", clause):
                    p["target"] = dev.get("ac", {}).get("target", 26) + 1
                if re.search(r"(调低|低一点|降低)", clause):
                    p["target"] = dev.get("ac", {}).get("target", 26) - 1
                if ("target" in p or "mode" in p) and "on" not in p:
                    p["on"] = True
            elif d == "speaker":
                v = _find_number(clause, ["%"]) or (int(m.group(1)) if (m := re.search(r"音量(\d+)", clause)) else None)
                if v is not None:
                    p["volume"] = v
                if re.search(r"(大声|音量大|调大)", clause):
                    p["volume"] = min(100, dev.get("speaker", {}).get("volume", 40) + 15)
                if re.search(r"(小声|音量小|调小)", clause):
                    p["volume"] = max(0, dev.get("speaker", {}).get("volume", 40) - 15)
                if "下一首" in clause or "换一首" in clause:
                    p["next"] = True
                m = re.search(r"(?:播放|放首|放一首|来点|来首)(.{1,6}?)(?:音乐|歌|$)", clause)
                if m and m.group(1):
                    p["track"] = m.group(1)
                if ("volume" in p or "next" in p or "track" in p) and "on" not in p:
                    p["on"] = True
            if not p and last_action:  # “打开灯和风扇”：后面的设备继承前面的动作
                p = {"on": last_action == "on"}
                if d == "curtain":
                    p = {"position": 100 if last_action == "on" else 0}
            if p:
                r.commands.append({"device": d, "params": p})
                r.trace.append(f"子句「{clause}」→ 设备：{DEVICE_NAMES[d]}，参数：{p}")
    if r.commands:
        # 同一设备的多个子句合并为一条命令（如“灯调成冷光，亮度调到60%”）
        merged = {}
        for c in r.commands:
            merged.setdefault(c["device"], {}).update(c["params"])
        if len(merged) < len(r.commands):
            r.trace.append("合并同一设备的多条参数")
        r.commands = [{"device": d, "params": p} for d, p in merged.items()]
        r.intent = "control"
        r.reply = "好的，已执行：" + "；".join(_describe(c) for c in r.commands) + "。"
        return r

    # 3.5 全屋开关
    if re.search(r"(所有|全部|全屋)", t) and re.search(r"(开|关)", t):
        on = not (any(w in t for w in OFF_WORDS) or "关" in t)
        r.intent = "control"
        r.commands = [{"device": d, "params": {"on": on}} for d in ("light", "fan", "ac", "speaker")]
        r.trace.append(f"命中全屋关键词 → {'打开' if on else '关闭'}全部用电设备")
        r.reply = f"好的，已{'打开' if on else '关闭'}全部设备。"
        return r

    # 4. 模糊感受类表达 —— 结合当前环境状态推理
    temp = env.get("temperature", 25)
    ac = dev.get("ac", {})
    if re.search(r"(好热|太热|有点热|热死|很热|闷热|热)", t):
        r.intent = "feeling_hot"
        r.trace.append(f"感受类表达「热」，读取环境：室温 {temp:.1f}℃，空调{'开' if ac.get('on') else '关'}")
        if ac.get("on") and ac.get("mode") == "cool":
            nt = max(20, ac.get("target", 26) - 1)
            r.commands.append({"device": "ac", "params": {"target": nt}})
            r.trace.append(f"规则：空调已在制冷 → 目标温度降 1℃ 至 {nt}℃")
            r.reply = f"空调已在制冷，帮你把温度调低到 {nt}℃。"
        elif temp >= 26:
            nt = (profile or {}).get("ac_target", 26)
            r.commands.append({"device": "ac", "params": {"on": True, "mode": "cool", "target": nt}})
            r.commands.append({"device": "fan", "params": {"on": True, "speed": 2}})
            r.trace.append(f"规则：室温 ≥ 26℃ → 开空调制冷 {nt}℃ 并开风扇 2 档加快降温")
            r.reply = f"室温 {temp:.1f}℃ 确实偏高，已开启空调制冷到 {nt}℃，并打开风扇。"
        else:
            r.commands.append({"device": "fan", "params": {"on": True, "speed": 1}})
            r.trace.append("规则：室温 < 26℃ → 仅开风扇 1 档，节能优先")
            r.reply = f"室温 {temp:.1f}℃ 并不高，先为你打开风扇吹一吹。"
        return r
    if re.search(r"(好冷|太冷|有点冷|冷)", t):
        r.intent = "feeling_cold"
        r.trace.append(f"感受类表达「冷」，读取环境：室温 {temp:.1f}℃，空调{'开' if ac.get('on') else '关'}")
        r.commands.append({"device": "fan", "params": {"on": False}})
        if ac.get("on") and ac.get("mode") == "cool":
            nt = ac.get("target", 26) + 2
            r.commands.append({"device": "ac", "params": {"target": nt}})
            r.trace.append(f"规则：空调制冷中 → 关风扇，目标温度升 2℃ 至 {nt}℃")
            r.reply = f"已关闭风扇，并把空调调高到 {nt}℃。"
        elif temp < 20:
            r.commands.append({"device": "ac", "params": {"on": True, "mode": "heat", "target": 24}})
            r.trace.append("规则：室温 < 20℃ → 空调制热 24℃")
            r.reply = "已为你开启空调制热到 24℃。"
        else:
            r.trace.append("规则：室温适中 → 只关风扇")
            r.reply = f"室温 {temp:.1f}℃，已关闭风扇，可以加件衣服哦。"
        return r
    if re.search(r"(太暗|好暗|看不清|有点暗|黑)", t):
        r.intent = "feeling_dark"
        hour = env.get("hour", 12)
        r.trace.append(f"感受类表达「暗」，读取环境：照度 {env.get('illuminance', 0):.0f} lux，时间 {hour}点")
        if 7 <= hour < 17 and dev.get("curtain", {}).get("position", 0) < 100:
            r.commands.append({"device": "curtain", "params": {"position": 100}})
            r.trace.append("规则：白天且窗帘未全开 → 优先拉开窗帘利用自然光")
            r.reply = "现在是白天，已为你拉开窗帘。"
        else:
            b = min(100, dev.get("light", {}).get("brightness", 60) + 30) if dev.get("light", {}).get("on") else 80
            r.commands.append({"device": "light", "params": {"on": True, "brightness": b}})
            r.trace.append(f"规则：夜间/窗帘已开 → 开灯并调到 {b}%")
            r.reply = f"已把灯光调到 {b}%。"
        return r
    if re.search(r"(刺眼|太亮|晃眼)", t):
        r.intent = "feeling_bright"
        r.commands.append({"device": "light", "params": {"brightness": max(10, dev.get("light", {}).get("brightness", 60) - 30)}})
        if dev.get("curtain", {}).get("position", 0) > 50:
            r.commands.append({"device": "curtain", "params": {"position": 50}})
        r.trace.append("感受类表达「刺眼」→ 降低亮度 30%，窗帘半开遮光")
        r.reply = "已为你调暗灯光并半拉窗帘。"
        return r
    if re.search(r"(闷|潮|湿)", t):
        r.intent = "feeling_humid"
        r.commands.append({"device": "ac", "params": {"on": True, "mode": "dry", "target": 26}})
        r.trace.append(f"感受类表达「闷/潮」，湿度 {env.get('humidity', 0):.0f}% → 空调除湿")
        r.reply = "已开启空调除湿模式。"
        return r

    r.trace.append("未匹配到任何规则")
    r.reply = "抱歉，我还不太理解。可以试试：“打开灯”“空调调到26度”“我有点热”“晚安”。"
    return r


def _describe(cmd):
    d, p = cmd["device"], cmd["params"]
    name = DEVICE_NAMES[d]
    parts = []
    if p.get("on") is True:
        parts.append("打开")
    if p.get("on") is False:
        parts.append("关闭")
    if "brightness" in p:
        parts.append(f"亮度{p['brightness']}%")
    if "color_temp" in p:
        parts.append(f"色温{p['color_temp']}K")
    if "speed" in p:
        parts.append(f"{p['speed']}档")
    if "position" in p:
        parts.append(f"开合{p['position']}%")
    if "target" in p:
        parts.append(f"{p['target']}℃")
    if "mode" in p:
        parts.append({"cool": "制冷", "heat": "制热", "dry": "除湿"}.get(p["mode"], p["mode"]))
    if "volume" in p:
        parts.append(f"音量{p['volume']}")
    if "track" in p:
        parts.append(f"播放《{p['track']}》")
    if p.get("next"):
        parts.append("下一首")
    return name + "、".join(parts)
