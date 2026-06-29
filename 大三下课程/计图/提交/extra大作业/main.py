from __future__ import annotations

import sys
import time
from dataclasses import dataclass, field

import glfw

from cloth import ClothSimulation
from gl_renderer import GLRenderer

try:
    sys.stdout.reconfigure(encoding="utf-8")
except Exception:
    pass


WIND_MIN = 0.0
WIND_MAX = 12.0
WIND_STEP = 1.0


@dataclass
class AppState:
    cloth: ClothSimulation
    paused: bool = False
    wind_strength: float = 4.0
    sim_time: float = 0.0
    wind_delta: float = 0.0
    reset_requested: bool = False
    toggle_pause_requested: bool = False
    quit_requested: bool = False
    keys_held: set = field(default_factory=set)


def create_cloth() -> ClothSimulation:
    return ClothSimulation(
        cols=28,
        rows=18,
        spacing=0.13,
        structural_k=680.0,
        shear_k=520.0,
        bending_k=180.0,
        damping=0.15,
        air_drag=5.0,
    )


def on_key(window, key, scancode, action, mods):
    state: AppState = glfw.get_window_user_pointer(window)

    if action == glfw.PRESS:
        if key == glfw.KEY_ESCAPE:
            state.quit_requested = True
        elif key == glfw.KEY_SPACE:
            state.toggle_pause_requested = True
        elif key == glfw.KEY_R:
            state.reset_requested = True
        elif key in (glfw.KEY_UP, glfw.KEY_W, glfw.KEY_EQUAL, glfw.KEY_KP_ADD):
            state.wind_delta += WIND_STEP
            print(f"[按键] 风力 +{WIND_STEP} → {min(WIND_MAX, state.wind_strength + state.wind_delta):.1f}")
        elif key in (glfw.KEY_DOWN, glfw.KEY_S, glfw.KEY_MINUS, glfw.KEY_KP_SUBTRACT):
            state.wind_delta -= WIND_STEP
            print(f"[按键] 风力 -{WIND_STEP} → {max(WIND_MIN, state.wind_strength + state.wind_delta):.1f}")

    if action == glfw.PRESS or action == glfw.REPEAT:
        if key in (glfw.KEY_UP, glfw.KEY_W, glfw.KEY_EQUAL, glfw.KEY_KP_ADD):
            state.keys_held.add("up")
        elif key in (glfw.KEY_DOWN, glfw.KEY_S, glfw.KEY_MINUS, glfw.KEY_KP_SUBTRACT):
            state.keys_held.add("down")

    if action == glfw.RELEASE:
        state.keys_held.discard("up")
        state.keys_held.discard("down")


def main() -> None:
    renderer = GLRenderer()
    state = AppState(cloth=create_cloth())

    glfw.set_window_user_pointer(renderer.window, state)
    glfw.set_key_callback(renderer.window, on_key)

    substeps = 8
    dt = 1.0 / 320.0

    last_frame_time = time.perf_counter()
    fps = 0.0

    print("OpenGL 布料模拟已启动")
    print("调节风力: ↑ ↓ 方向键，或 W/S 键，或小键盘 +/-")
    print("其他: 空格暂停 | R重置 | ESC退出")
    print("提示: 请先点击窗口确保焦点在程序上，左下角色条=风力大小")

    while not renderer.should_close() and not state.quit_requested:
        renderer.poll_events()

        now = time.perf_counter()
        frame_dt = now - last_frame_time
        last_frame_time = now
        fps = 0.9 * fps + 0.1 * (1.0 / max(frame_dt, 1e-6))

        if "up" in state.keys_held:
            state.wind_delta += 5.0 * frame_dt
        if "down" in state.keys_held:
            state.wind_delta -= 5.0 * frame_dt

        if state.wind_delta != 0.0:
            state.wind_strength = max(WIND_MIN, min(WIND_MAX, state.wind_strength + state.wind_delta))
            state.wind_delta = 0.0

        if state.toggle_pause_requested:
            state.paused = not state.paused
            state.toggle_pause_requested = False
            print("暂停" if state.paused else "继续")

        if state.reset_requested:
            state.cloth = create_cloth()
            state.sim_time = 0.0
            state.reset_requested = False
            print("布料已重置")

        if not state.paused:
            for _ in range(substeps):
                state.cloth.step(dt, state.sim_time, wind_strength=state.wind_strength)
                state.sim_time += dt

        renderer.draw_scene(state.cloth, state.wind_strength)
        renderer.update_title(state.wind_strength, fps, state.paused)
        renderer.swap_buffers()

    renderer.destroy()
    sys.exit(0)


if __name__ == "__main__":
    main()
