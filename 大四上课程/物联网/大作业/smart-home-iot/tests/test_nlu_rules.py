"""单元测试：自然语言解析 + 自动化规则引擎。运行：python -m unittest discover tests -v"""
import os
import sys
import unittest

sys.path.insert(0, os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "hub"))
import nlu  # noqa: E402
from rules import RuleEngine  # noqa: E402


def make_state(**env):
    base = {"temperature": 27.0, "humidity": 60, "illuminance": 20, "hour": 20, "presence": True,
            "person": "小明", "vacant_minutes": 0, "outdoor_temp": 30}
    base.update(env)
    return {"env": base, "devices": {
        "light": {"on": False, "brightness": 60, "color_temp": 4000},
        "fan": {"on": False, "speed": 1},
        "curtain": {"position": 0},
        "ac": {"on": False, "mode": "cool", "target": 26},
        "speaker": {"on": False, "track": "轻音乐", "volume": 40}}}


def cmds(text, state=None, profile=None):
    return nlu.parse(text, state or make_state(), profile).commands


class TestNLU(unittest.TestCase):
    def test_basic_on_off(self):
        self.assertEqual(cmds("打开灯"), [{"device": "light", "params": {"on": True}}])
        self.assertEqual(cmds("把空调关了"), [{"device": "ac", "params": {"on": False}}])

    def test_chinese_number(self):
        self.assertEqual(cmds("空调调到二十四度")[0]["params"]["target"], 24)
        self.assertEqual(cmds("亮度调到百分之三十")[0]["params"]["brightness"], 30)
        self.assertEqual(cmds("风扇开到3档")[0]["params"]["speed"], 3)

    def test_multi_device_and_clause(self):
        self.assertEqual([c["device"] for c in cmds("打开风扇和音乐")], ["fan", "speaker"])
        c = cmds("关闭空调，打开窗帘")
        self.assertEqual(c[0], {"device": "ac", "params": {"on": False}})
        self.assertEqual(c[1], {"device": "curtain", "params": {"position": 100}})

    def test_relative_adjust_uses_current_state(self):
        st = make_state()
        st["devices"]["light"].update(on=True, brightness=50)
        self.assertEqual(cmds("灯调亮一点", st)[0]["params"]["brightness"], 70)

    def test_context_aware_hot(self):
        # 室温高 → 开空调+风扇；室温不高 → 只开风扇；空调已开 → 降低目标温度
        self.assertEqual({c["device"] for c in cmds("我好热", make_state(temperature=30))}, {"ac", "fan"})
        self.assertEqual(cmds("我好热", make_state(temperature=24)), [{"device": "fan", "params": {"on": True, "speed": 1}}])
        st = make_state(temperature=27)
        st["devices"]["ac"].update(on=True, target=26)
        self.assertEqual(cmds("有点热", st), [{"device": "ac", "params": {"target": 25}}])

    def test_context_aware_dark(self):
        self.assertEqual(cmds("太暗了", make_state(hour=10))[0]["device"], "curtain")
        self.assertEqual(cmds("太暗了", make_state(hour=21))[0]["device"], "light")

    def test_scene_uses_profile(self):
        profile = {"color_temp": 6000, "brightness": 95, "ac_target": 24, "music": "流行歌曲"}
        r = nlu.parse("我回来了", make_state(temperature=29), profile, "小明")
        self.assertEqual(r.intent, "scene")
        light = next(c for c in r.commands if c["device"] == "light")
        self.assertEqual(light["params"]["color_temp"], 6000)
        self.assertIn("小明", r.reply)

    def test_query_and_unknown(self):
        self.assertEqual(nlu.parse("现在温度多少", make_state()).intent, "query")
        self.assertEqual(nlu.parse("今天股票涨了吗", make_state()).intent, "unknown")


class TestRules(unittest.TestCase):
    def test_presence_light_rising_edge(self):
        eng = RuleEngine()
        st = make_state(presence=False)
        self.assertEqual(eng.evaluate(st, {}), [])
        st["env"]["presence"] = True
        fired = eng.evaluate(st, {"color_temp": 6000, "brightness": 90})
        self.assertEqual([r.rule_id for r, _ in fired], ["R1"])
        self.assertLessEqual(fired[0][1][0]["params"]["color_temp"], 3500)  # 夜间强制暖光
        self.assertEqual(eng.evaluate(st, {}), [])  # 条件持续成立不重复触发

    def test_manual_override(self):
        eng = RuleEngine()
        st = make_state(presence=False)
        eng.evaluate(st, {})
        eng.note_manual("light")
        st["env"]["presence"] = True
        self.assertEqual(eng.evaluate(st, {}), [])

    def test_vacant_turn_off(self):
        eng = RuleEngine()
        st = make_state(presence=False, vacant_minutes=3)
        st["devices"]["light"]["on"] = True
        self.assertEqual(eng.evaluate(st, {}), [])
        st["env"]["vacant_minutes"] = 12
        fired = eng.evaluate(st, {})
        self.assertEqual(fired[0][0].rule_id, "R2")
        self.assertEqual(fired[0][1], [{"device": "light", "params": {"on": False}}])

    def test_high_temp_and_disabled(self):
        eng = RuleEngine()
        st = make_state(temperature=30, illuminance=500)
        self.assertEqual([r.rule_id for r, _ in eng.evaluate(st, {"ac_target": 25})], ["R3"])
        eng2 = RuleEngine()
        eng2.enabled = False
        self.assertEqual(eng2.evaluate(st, {}), [])


if __name__ == "__main__":
    unittest.main()
