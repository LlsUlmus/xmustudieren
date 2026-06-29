"""OpenGL 渲染器（GLFW + PyOpenGL 可编程管线。"""

from __future__ import annotations

from typing import List, Tuple

import glfw
import numpy as np
from OpenGL import GL
from OpenGL.GL import shaders

from cloth import ClothSimulation


LIT_VERTEX = """
#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aColor;
layout(location = 2) in vec3 aNormal;

uniform mat4 uMVP;
uniform mat3 uNormalMat;

out vec3 vColor;
out vec3 vNormal;
out vec3 vWorldPos;

void main() {
    vColor = aColor;
    vNormal = uNormalMat * aNormal;
    vWorldPos = aPos;
    gl_Position = uMVP * vec4(aPos, 1.0);
}
"""

LIT_FRAGMENT = """
#version 330 core
in vec3 vColor;
in vec3 vNormal;
in vec3 vWorldPos;

out vec4 FragColor;

uniform vec3 uLightDir;
uniform vec3 uViewPos;

void main() {
    vec3 n = normalize(vNormal);
    if (!gl_FrontFacing) {
        n = -n;                            // 双面光照：旗子正反面都受光
    }
    vec3 l = normalize(uLightDir);
    vec3 v = normalize(uViewPos - vWorldPos);
    vec3 h = normalize(l + v);

    float hemi = 0.5 + 0.5 * n.y;          // 半球环境光
    vec3 ambient = mix(vec3(0.22, 0.24, 0.20), vec3(0.42, 0.46, 0.54), hemi);

    float diff = max(dot(n, l), 0.0);
    float fill = max(dot(n, -l), 0.0) * 0.15;
    float spec = pow(max(dot(n, h), 0.0), 28.0) * 0.10;

    vec3 color = vColor * (ambient + 0.85 * diff + fill) + vec3(spec);
    FragColor = vec4(color, 1.0);
}
"""

FLAT_VERTEX = """
#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec3 aColor;
out vec3 vColor;
void main() {
    vColor = aColor;
    gl_Position = vec4(aPos, 0.0, 1.0);
}
"""

FLAT_FRAGMENT = """
#version 330 core
in vec3 vColor;
out vec4 FragColor;
void main() {
    FragColor = vec4(vColor, 1.0);
}
"""


def perspective(fovy_deg: float, aspect: float, near: float, far: float) -> np.ndarray:
    f = 1.0 / np.tan(np.radians(fovy_deg) / 2.0)
    mat = np.zeros((4, 4), dtype=np.float32)
    mat[0, 0] = f / aspect
    mat[1, 1] = f
    mat[2, 2] = (far + near) / (near - far)
    mat[2, 3] = (2.0 * far * near) / (near - far)
    mat[3, 2] = -1.0
    return mat


def look_at(eye: np.ndarray, center: np.ndarray, up: np.ndarray) -> np.ndarray:
    f = center - eye
    f = f / np.linalg.norm(f)
    up_n = up / np.linalg.norm(up)
    s = np.cross(f, up_n)
    s = s / np.linalg.norm(s)
    u = np.cross(s, f)

    mat = np.eye(4, dtype=np.float32)
    mat[0, :3] = s
    mat[1, :3] = u
    mat[2, :3] = -f
    mat[:3, 3] = -mat[:3, :3] @ eye
    return mat


def _hsv_to_rgb(h: float, s: float, v: float) -> Tuple[float, float, float]:
    i = int(h * 6.0) % 6
    f = h * 6.0 - int(h * 6.0)
    p = v * (1.0 - s)
    q = v * (1.0 - f * s)
    t = v * (1.0 - (1.0 - f) * s)
    return [(v, t, p), (q, v, p), (p, v, t), (p, q, v), (t, p, v), (v, p, q)][i]


class GLRenderer:
    POLE_HEIGHT = 3.35
    POLE_RADIUS = 0.05

    SKY_TOP = (0.30, 0.52, 0.82)
    SKY_BOTTOM = (0.78, 0.88, 0.96)

    def __init__(self, width: int = 1280, height: int = 720) -> None:
        if not glfw.init():
            raise RuntimeError("GLFW 初始化失败")

        glfw.window_hint(glfw.CONTEXT_VERSION_MAJOR, 3)
        glfw.window_hint(glfw.CONTEXT_VERSION_MINOR, 3)
        glfw.window_hint(glfw.OPENGL_PROFILE, glfw.OPENGL_CORE_PROFILE)
        glfw.window_hint(glfw.OPENGL_FORWARD_COMPAT, glfw.TRUE)
        glfw.window_hint(glfw.SAMPLES, 4)

        self.width = width
        self.height = height
        self.window = glfw.create_window(width, height, "计算机图形学作业 - 布料模拟", None, None)
        if not self.window:
            glfw.terminate()
            raise RuntimeError("无法创建 OpenGL 窗口")

        glfw.make_context_current(self.window)
        glfw.swap_interval(1)
        glfw.set_framebuffer_size_callback(self.window, self._on_resize)

        self.lit_shader = shaders.compileProgram(
            shaders.compileShader(LIT_VERTEX, GL.GL_VERTEX_SHADER),
            shaders.compileShader(LIT_FRAGMENT, GL.GL_FRAGMENT_SHADER),
        )
        self.flat_shader = shaders.compileProgram(
            shaders.compileShader(FLAT_VERTEX, GL.GL_VERTEX_SHADER),
            shaders.compileShader(FLAT_FRAGMENT, GL.GL_FRAGMENT_SHADER),
        )

        self.vao = GL.glGenVertexArrays(1)          # 3D: pos3 + color3 + normal3
        self.vbo = GL.glGenBuffers(1)
        self.vao2d = GL.glGenVertexArrays(1)        # 2D: pos2 + color3
        self.vbo2d = GL.glGenBuffers(1)

        GL.glEnable(GL.GL_DEPTH_TEST)
        GL.glEnable(GL.GL_MULTISAMPLE)
        GL.glClearColor(*self.SKY_BOTTOM, 1.0)

        self.eye = np.array([6.2, 2.9, 9.0], dtype=np.float32)
        self.center = np.array([1.7, 1.65, 0.0], dtype=np.float32)
        self.up = np.array([0.0, 1.0, 0.0], dtype=np.float32)
        self.light_dir = np.array([-0.35, 0.80, 0.55], dtype=np.float32)

        self._scene_static = self._build_static_scene()
        self._sky_vertices = self._build_sky()
        self._topo: dict | None = None

    def _on_resize(self, _window, width: int, height: int) -> None:
        self.width = max(width, 1)
        self.height = max(height, 1)
        GL.glViewport(0, 0, self.width, self.height)

    @staticmethod
    def _rgb(color: Tuple[int, int, int]) -> Tuple[float, float, float]:
        return color[0] / 255.0, color[1] / 255.0, color[2] / 255.0

    @staticmethod
    def _tri_normal(p0: np.ndarray, p1: np.ndarray, p2: np.ndarray) -> np.ndarray:
        n = np.cross(p1 - p0, p2 - p0)
        norm = float(np.linalg.norm(n))
        if norm < 1e-8:
            return np.array([0.0, 1.0, 0.0], dtype=np.float32)
        return (n / norm).astype(np.float32)

    def _append_tri(self, data, p0, p1, p2, color, normal=None) -> None:
        n = normal if normal is not None else self._tri_normal(p0, p1, p2)
        for p in (p0, p1, p2):
            data.extend([float(p[0]), float(p[1]), float(p[2]),
                         color[0], color[1], color[2],
                         float(n[0]), float(n[1]), float(n[2])])

    def _append_quad(self, data, p0, p1, p2, p3, color, normal) -> None:
        self._append_tri(data, p0, p1, p2, color, normal)
        self._append_tri(data, p0, p2, p3, color, normal)

    def _cylinder(self, data, cx, cz, y0, y1, radius, color, segments=24) -> None:
        for i in range(segments):
            a0 = 2 * np.pi * i / segments
            a1 = 2 * np.pi * (i + 1) / segments
            x0, z0 = cx + radius * np.cos(a0), cz + radius * np.sin(a0)
            x1, z1 = cx + radius * np.cos(a1), cz + radius * np.sin(a1)
            n0 = np.array([np.cos(a0), 0.0, np.sin(a0)], dtype=np.float32)
            n1 = np.array([np.cos(a1), 0.0, np.sin(a1)], dtype=np.float32)
            data.extend([x0, y0, z0, *color, *n0,  x1, y0, z1, *color, *n1,  x1, y1, z1, *color, *n1])
            data.extend([x0, y0, z0, *color, *n0,  x1, y1, z1, *color, *n1,  x0, y1, z0, *color, *n0])

    def _sphere(self, data, cx, cy, cz, r, color, seg=14) -> None:
        for i in range(seg):
            lat0 = np.pi * (i / seg - 0.5)
            lat1 = np.pi * ((i + 1) / seg - 0.5)
            for j in range(seg):
                lon0 = 2 * np.pi * j / seg
                lon1 = 2 * np.pi * (j + 1) / seg

                def vert(lat, lon):
                    nx = np.cos(lat) * np.cos(lon)
                    ny = np.sin(lat)
                    nz = np.cos(lat) * np.sin(lon)
                    p = np.array([cx + r * nx, cy + r * ny, cz + r * nz])
                    n = np.array([nx, ny, nz], dtype=np.float32)
                    return p, n

                p00, n00 = vert(lat0, lon0)
                p01, n01 = vert(lat0, lon1)
                p10, n10 = vert(lat1, lon0)
                p11, n11 = vert(lat1, lon1)
                for (p, n) in ((p00, n00), (p01, n01), (p11, n11)):
                    data.extend([*p, *color, *n])
                for (p, n) in ((p00, n00), (p11, n11), (p10, n10)):
                    data.extend([*p, *color, *n])

    def _build_static_scene(self) -> np.ndarray:
        data: List[float] = []

        light = self._rgb((96, 152, 92))
        dark = self._rgb((78, 134, 78))
        up = np.array([0.0, 1.0, 0.0], dtype=np.float32)
        for gx in range(-12, 24):
            for gz in range(-12, 16):
                color = light if (gx + gz) % 2 == 0 else dark
                self._append_quad(
                    data,
                    np.array([gx, 0.0, gz]),
                    np.array([gx, 0.0, gz + 1]),
                    np.array([gx + 1, 0.0, gz + 1]),
                    np.array([gx + 1, 0.0, gz]),
                    color, up,
                )

        pole = self._rgb((205, 208, 214))
        self._cylinder(data, cx=0.0, cz=0.0, y0=0.0, y1=self.POLE_HEIGHT,
                       radius=self.POLE_RADIUS, color=pole)
        gold = self._rgb((230, 196, 92))
        self._sphere(data, cx=0.0, cy=self.POLE_HEIGHT + 0.09, cz=0.0, r=0.10, color=gold)

        return np.array(data, dtype=np.float32)

    def _build_sky(self) -> np.ndarray:
        top = self.SKY_TOP
        bot = self.SKY_BOTTOM
        return np.array([
            -1, -1, *bot,   1, -1, *bot,   1, 1, *top,
            -1, -1, *bot,   1,  1, *top,  -1, 1, *top,
        ], dtype=np.float32)

    def _ensure_topology(self, cloth: ClothSimulation) -> dict:
        if self._topo is not None and self._topo["rows"] == cloth.rows and self._topo["cols"] == cloth.cols:
            return self._topo

        rows, cols = cloth.rows, cloth.cols
        tri = []
        for r in range(rows - 1):
            for c in range(cols - 1):
                a = r * cols + c
                b = a + 1
                cc = a + cols + 1
                d = a + cols
                tri.extend([a, b, cc, a, cc, d])
        tri_idx = np.array(tri, dtype=np.int64)

        # 按行铺一道彩虹，飘扬时色带随波纹流动 → 彩旗
        colors = np.zeros((rows * cols, 3), dtype=np.float32)
        for r in range(rows):
            hue = 0.02 + 0.80 * (r / max(rows - 1, 1))
            colors[r * cols:(r + 1) * cols] = _hsv_to_rgb(hue, 0.78, 0.96)

        self._topo = {"rows": rows, "cols": cols, "tri_idx": tri_idx, "colors": colors}
        return self._topo

    @staticmethod
    def _vertex_normals(cloth: ClothSimulation) -> np.ndarray:
        """相邻质点切向量叉乘 → 平滑的逐顶点法线。"""
        P = cloth.positions.reshape(cloth.rows, cloth.cols, 3)
        du = np.zeros_like(P)
        dv = np.zeros_like(P)
        du[:, 1:-1] = P[:, 2:] - P[:, :-2]
        du[:, 0] = P[:, 1] - P[:, 0]
        du[:, -1] = P[:, -1] - P[:, -2]
        dv[1:-1, :] = P[2:] - P[:-2]
        dv[0] = P[1] - P[0]
        dv[-1] = P[-1] - P[-2]
        n = np.cross(du, dv)
        ln = np.linalg.norm(n, axis=2, keepdims=True)
        ln[ln < 1e-8] = 1.0
        return (n / ln).reshape(-1, 3)

    def _build_cloth_vertices(self, cloth: ClothSimulation) -> np.ndarray:
        topo = self._ensure_topology(cloth)
        idx = topo["tri_idx"]
        normals = self._vertex_normals(cloth)

        pos = cloth.positions[idx].astype(np.float32)
        col = topo["colors"][idx]
        nrm = normals[idx].astype(np.float32)
        return np.concatenate([pos, col, nrm], axis=1).ravel()

    def _draw_lit(self, vertices: np.ndarray, mvp, normal_mat, cull: bool) -> None:
        if len(vertices) == 0:
            return
        count = len(vertices) // 9
        GL.glBindVertexArray(self.vao)
        GL.glBindBuffer(GL.GL_ARRAY_BUFFER, self.vbo)
        GL.glBufferData(GL.GL_ARRAY_BUFFER, vertices.nbytes, vertices, GL.GL_DYNAMIC_DRAW)
        stride = 9 * 4
        GL.glEnableVertexAttribArray(0)
        GL.glVertexAttribPointer(0, 3, GL.GL_FLOAT, GL.GL_FALSE, stride, GL.GLvoidp(0))
        GL.glEnableVertexAttribArray(1)
        GL.glVertexAttribPointer(1, 3, GL.GL_FLOAT, GL.GL_FALSE, stride, GL.GLvoidp(12))
        GL.glEnableVertexAttribArray(2)
        GL.glVertexAttribPointer(2, 3, GL.GL_FLOAT, GL.GL_FALSE, stride, GL.GLvoidp(24))

        GL.glUseProgram(self.lit_shader)
        GL.glUniformMatrix4fv(GL.glGetUniformLocation(self.lit_shader, "uMVP"), 1, GL.GL_TRUE, mvp)
        GL.glUniformMatrix3fv(GL.glGetUniformLocation(self.lit_shader, "uNormalMat"), 1, GL.GL_TRUE, normal_mat)
        GL.glUniform3f(GL.glGetUniformLocation(self.lit_shader, "uLightDir"), *self.light_dir)
        GL.glUniform3f(GL.glGetUniformLocation(self.lit_shader, "uViewPos"), *self.eye)

        if cull:
            GL.glEnable(GL.GL_CULL_FACE)
        else:
            GL.glDisable(GL.GL_CULL_FACE)
        GL.glDrawArrays(GL.GL_TRIANGLES, 0, count)

    def _draw_flat(self, vertices: np.ndarray) -> None:
        if len(vertices) == 0:
            return
        count = len(vertices) // 5
        GL.glBindVertexArray(self.vao2d)
        GL.glBindBuffer(GL.GL_ARRAY_BUFFER, self.vbo2d)
        GL.glBufferData(GL.GL_ARRAY_BUFFER, vertices.nbytes, vertices, GL.GL_DYNAMIC_DRAW)
        stride = 5 * 4
        GL.glEnableVertexAttribArray(0)
        GL.glVertexAttribPointer(0, 2, GL.GL_FLOAT, GL.GL_FALSE, stride, GL.GLvoidp(0))
        GL.glEnableVertexAttribArray(1)
        GL.glVertexAttribPointer(1, 3, GL.GL_FLOAT, GL.GL_FALSE, stride, GL.GLvoidp(8))
        GL.glUseProgram(self.flat_shader)
        GL.glDrawArrays(GL.GL_TRIANGLES, 0, count)

    def _build_hud(self, wind: float) -> np.ndarray:
        wind_norm = float(np.clip(wind / 12.0, 0.0, 1.0))

        x0, x1 = -0.96, -0.40
        y0, y1 = -0.95, -0.90
        bg = (0.10, 0.11, 0.14)
        border = (0.85, 0.87, 0.90)
        fill = _hsv_to_rgb(0.55 - 0.45 * wind_norm, 0.75, 0.95)

        def quad(ax, ay, bx, by, c):
            return [ax, ay, *c, bx, ay, *c, bx, by, *c,
                    ax, ay, *c, bx, by, *c, ax, by, *c]

        verts: List[float] = []
        t = 0.008
        verts += quad(x0 - t, y0 - t, x1 + t, y1 + t, border)
        verts += quad(x0, y0, x1, y1, bg)
        verts += quad(x0, y0, x0 + (x1 - x0) * wind_norm, y1, fill)
        return np.array(verts, dtype=np.float32)

    def draw_scene(self, cloth: ClothSimulation, wind: float) -> None:
        GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT)

        # 天空渐变背景（不写深度）
        GL.glDisable(GL.GL_DEPTH_TEST)
        GL.glDepthMask(GL.GL_FALSE)
        self._draw_flat(self._sky_vertices)
        GL.glDepthMask(GL.GL_TRUE)
        GL.glEnable(GL.GL_DEPTH_TEST)

        # 3D 场景：以 GL_TRUE 上传，故直接传矩阵本身（不再额外转置）
        view = look_at(self.eye, self.center, self.up)
        proj = perspective(45.0, self.width / max(self.height, 1), 0.1, 100.0)
        mvp = np.ascontiguousarray(proj @ view, dtype=np.float32)
        normal_mat = np.eye(3, dtype=np.float32)

        self._draw_lit(self._scene_static, mvp, normal_mat, cull=True)
        self._draw_lit(self._build_cloth_vertices(cloth), mvp, normal_mat, cull=False)

        # 风力 HUD
        GL.glDisable(GL.GL_DEPTH_TEST)
        self._draw_flat(self._build_hud(wind))
        GL.glEnable(GL.GL_DEPTH_TEST)

    def update_title(self, wind: float, fps: float, paused: bool) -> None:
        status = "暂停" if paused else "运行"
        title = (
            f"布料模拟 | 风力={wind:.1f} | {fps:.0f}FPS | {status} | "
            f"↑↓/WS调风 空格暂停 R重置 ESC退出"
        )
        glfw.set_window_title(self.window, title)

    def should_close(self) -> bool:
        return glfw.window_should_close(self.window)

    def poll_events(self) -> None:
        glfw.poll_events()

    def swap_buffers(self) -> None:
        glfw.swap_buffers(self.window)

    def destroy(self) -> None:
        GL.glDeleteBuffers(2, [self.vbo, self.vbo2d])
        GL.glDeleteVertexArrays(2, [self.vao, self.vao2d])
        GL.glDeleteProgram(self.lit_shader)
        GL.glDeleteProgram(self.flat_shader)
        glfw.destroy_window(self.window)
        glfw.terminate()
