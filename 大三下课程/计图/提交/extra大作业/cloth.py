"""
质点-弹簧布料模拟
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import List, Tuple

import numpy as np


@dataclass
class Spring:
    i: int
    j: int
    rest_length: float
    stiffness: float
    kind: str


class ClothSimulation:
    """矩形布料网格的质点-弹簧物理模拟。"""

    def __init__(
        self,
        cols: int = 32,
        rows: int = 22,
        spacing: float = 0.12,
        origin: np.ndarray | None = None,
        structural_k: float = 680.0,
        shear_k: float = 420.0,
        bending_k: float = 120.0,
        damping: float = 0.12,
        air_drag: float = 4.5,
        gravity: float = 9.8,
        mass: float = 1.0,
    ) -> None:
        self.cols = cols
        self.rows = rows
        self.spacing = spacing
        self.origin = np.array([0.0, 3.05, 0.0]) if origin is None else origin.astype(float)
        self.structural_k = structural_k
        self.shear_k = shear_k
        self.bending_k = bending_k
        self.damping = damping
        self.air_drag = air_drag
        self.gravity = gravity
        self.mass = mass

        count = cols * rows
        self.positions = np.zeros((count, 3), dtype=np.float64)
        self.prev_positions = np.zeros((count, 3), dtype=np.float64)
        self.pinned = np.zeros(count, dtype=bool)

        self._init_grid()
        self.springs: List[Spring] = []
        self._build_springs()
        self._pack_springs()

    def _index(self, col: int, row: int) -> int:
        return row * self.cols + col

    def _init_grid(self) -> None:
        for row in range(self.rows):
            for col in range(self.cols):
                idx = self._index(col, row)
                x = self.origin[0] + col * self.spacing
                y = self.origin[1] - row * self.spacing
                z = self.origin[2]
                self.positions[idx] = (x, y, z)
                self.prev_positions[idx] = (x, y, z)

                if col == 0:                       # 左侧整条边固定在旗杆上
                    self.pinned[idx] = True

    def _add_spring(self, i: int, j: int, stiffness: float, kind: str) -> None:
        rest = float(np.linalg.norm(self.positions[i] - self.positions[j]))
        self.springs.append(Spring(i=i, j=j, rest_length=rest, stiffness=stiffness, kind=kind))

    def _build_springs(self) -> None:
        for row in range(self.rows):
            for col in range(self.cols):
                i = self._index(col, row)

                if col < self.cols - 1:
                    self._add_spring(i, self._index(col + 1, row), self.structural_k, "structural")
                if row < self.rows - 1:
                    self._add_spring(i, self._index(col, row + 1), self.structural_k, "structural")

                if col < self.cols - 1 and row < self.rows - 1:
                    self._add_spring(i, self._index(col + 1, row + 1), self.shear_k, "shear")
                    self._add_spring(
                        self._index(col + 1, row),
                        self._index(col, row + 1),
                        self.shear_k,
                        "shear",
                    )

                if col < self.cols - 2:
                    self._add_spring(i, self._index(col + 2, row), self.bending_k, "bending")
                if row < self.rows - 2:
                    self._add_spring(i, self._index(col, row + 2), self.bending_k, "bending")

    def _pack_springs(self) -> None:
        self.spring_i = np.array([s.i for s in self.springs], dtype=np.int32)
        self.spring_j = np.array([s.j for s in self.springs], dtype=np.int32)
        self.spring_rest = np.array([s.rest_length for s in self.springs], dtype=np.float64)
        self.spring_k = np.array([s.stiffness for s in self.springs], dtype=np.float64)

        # 距离约束用结构 + 剪切弹簧，稳住自由边角点
        constr = [idx for idx, s in enumerate(self.springs) if s.kind in ("structural", "shear")]
        self.constr_i = self.spring_i[constr]
        self.constr_j = self.spring_j[constr]
        self.constr_rest = self.spring_rest[constr]

    def _apply_spring_forces(self, forces: np.ndarray) -> None:
        p_i = self.positions[self.spring_i]
        p_j = self.positions[self.spring_j]
        delta = p_i - p_j
        dist = np.linalg.norm(delta, axis=1)
        dist = np.maximum(dist, 1e-8)

        direction = delta / dist[:, None]
        strain = (dist - self.spring_rest) / np.maximum(self.spring_rest, 1e-6)
        strain = np.clip(strain, -0.35, 0.35)
        magnitude = self.spring_k * strain * self.spring_rest
        spring_forces = magnitude[:, None] * direction

        np.add.at(forces, self.spring_i, spring_forces)
        np.add.at(forces, self.spring_j, -spring_forces)

    def _apply_wind(self, forces: np.ndarray, time_s: float, wind_strength: float) -> None:
        movable = ~self.pinned
        pos = self.positions[movable]

        phase_a = np.sin(time_s * 2.1 + pos[:, 0] * 5.0)
        phase_b = np.sin(time_s * 3.3 + pos[:, 1] * 4.0 + 1.2)
        phase_c = np.cos(time_s * 1.7 + pos[:, 0] * 2.0 - pos[:, 1] * 3.0)
        gust = 0.45 + 0.22 * phase_a + 0.13 * phase_b + 0.05 * phase_c

        movable_idx = np.flatnonzero(movable)
        rows = movable_idx // self.cols
        row_factor = 1.0 + 0.6 * rows / max(self.rows - 1, 1)

        forces[movable, 2] += wind_strength * gust * row_factor

    def _apply_air_drag(self, forces: np.ndarray, dt: float) -> None:
        safe_dt = max(dt, 1e-6)
        movable = ~self.pinned
        velocity = (self.positions[movable] - self.prev_positions[movable]) / safe_dt
        forces[movable] -= self.air_drag * velocity

    def step(self, dt: float, time_s: float, wind_strength: float = 6.0) -> None:
        count = len(self.positions)
        forces = np.zeros((count, 3), dtype=np.float64)
        forces[:, 1] -= self.gravity * self.mass

        self._apply_wind(forces, time_s, wind_strength)
        self._apply_air_drag(forces, dt)
        self._apply_spring_forces(forces)

        safe_dt = max(dt, 1e-6)
        inv_mass = 1.0 / self.mass
        movable = ~self.pinned

        velocity = (self.positions[movable] - self.prev_positions[movable]) / safe_dt
        velocity *= (1.0 - self.damping)
        velocity += forces[movable] * inv_mass * safe_dt

        speed = np.linalg.norm(velocity, axis=1)
        too_fast = speed > 5.0
        if np.any(too_fast):
            scale = np.ones_like(speed)
            scale[too_fast] = 5.0 / speed[too_fast]
            velocity *= scale[:, None]

        self.prev_positions[movable] = self.positions[movable]
        self.positions[movable] += velocity * safe_dt
        self.prev_positions[self.pinned] = self.positions[self.pinned]
        self._satisfy_constraints(iterations=4)
        self._resolve_collisions()

    def _satisfy_constraints(self, iterations: int = 6) -> None:
        """对结构+剪切弹簧做距离约束投影，限制最大形变，兼顾稳定性与实时性。"""
        pin_i = self.pinned[self.constr_i]
        pin_j = self.pinned[self.constr_j]
        pin_weight_i = np.where(pin_i, 0.0, 0.5)
        pin_weight_j = np.where(pin_j, 0.0, 0.5)
        total_weight = pin_weight_i + pin_weight_j
        total_weight = np.maximum(total_weight, 1e-6)

        max_dist = self.constr_rest * 1.10
        min_dist = self.constr_rest * 0.92

        for _ in range(iterations):
            p_i = self.positions[self.constr_i]
            p_j = self.positions[self.constr_j]
            delta = p_i - p_j
            dist = np.linalg.norm(delta, axis=1)
            dist = np.maximum(dist, 1e-8)

            too_long = dist > max_dist
            too_short = dist < min_dist
            active = too_long | too_short
            if not np.any(active):
                continue

            direction = delta / dist[:, None]
            target = np.where(too_long, max_dist, min_dist)
            correction_mag = (dist - target) * 0.5
            correction = direction * correction_mag[:, None]

            corr_i = np.zeros((len(self.constr_i), 3), dtype=np.float64)
            corr_j = np.zeros_like(corr_i)
            corr_i[active] = correction[active] * (pin_weight_i[active] / total_weight[active])[:, None]
            corr_j[active] = -correction[active] * (pin_weight_j[active] / total_weight[active])[:, None]

            np.add.at(self.positions, self.constr_i, -corr_i)
            np.add.at(self.positions, self.constr_j, -corr_j)

    def _resolve_collisions(self) -> None:
        ground_y = 0.0
        for idx in range(len(self.positions)):
            if self.positions[idx, 1] < ground_y:
                self.positions[idx, 1] = ground_y
                self.prev_positions[idx, 1] = ground_y

    def get_faces(self) -> List[Tuple[int, int, int, int]]:
        """返回四边形面 (a, b, c, d)，用于渲染彩旗网格。"""
        faces: List[Tuple[int, int, int, int]] = []
        for row in range(self.rows - 1):
            for col in range(self.cols - 1):
                a = self._index(col, row)
                b = self._index(col + 1, row)
                c = self._index(col + 1, row + 1)
                d = self._index(col, row + 1)
                faces.append((a, b, c, d))
        return faces

    def get_face_color(self, col: int, row: int) -> Tuple[int, int, int]:
        """彩旗配色：红、黄、蓝、绿交替。"""
        palette = [
            (220, 53, 69),
            (255, 193, 7),
            (13, 110, 253),
            (25, 135, 84),
            (255, 111, 0),
            (111, 66, 193),
        ]
        return palette[(col // 2 + row // 2) % len(palette)]
