import math
import random
from dataclasses import dataclass

import matplotlib.pyplot as plt
import numpy as np

plt.rcParams["font.sans-serif"] = ["SimHei", "Microsoft YaHei"]
plt.rcParams["axes.unicode_minus"] = False

X_LOW, X_HIGH = 0.0, 2.0 * math.pi
L_CAP = 5000


def f(x):
    return 11 * np.sin(6 * x) + 7 * np.cos(5 * x)


@dataclass
class CoolingSchedule:
    T0: float  # ① 初始温度（充分大，便于早期接受劣解）
    K: float  # ② 温度衰减因子，T(n+1)=K*T(n)，K∈(0,1) 接近 1
    step_factor: float  # ③ 搜索步长因子（邻域扰动幅度 ∝ T * step_factor）
    L: int  # ④ 马可夫链长度（每个温度下的内循环次数）
    x_init: float  # ⑤ 初始解状态
    T_min: float  # ⑥ 终止温度
    max_reject: int  # ⑥ 连续拒绝次数达到该值也可终止（辅助条件）


def chain_length(T, cfg: CoolingSchedule) -> int:
    ratio = T / cfg.T0
    return min(L_CAP, cfg.L + int((L_CAP - cfg.L) * ratio))


def simulated_annealing(cfg: CoolingSchedule):
    x = np.clip(cfg.x_init, X_LOW, X_HIGH)
    fx = float(f(x))
    x_best, f_best = x, fx
    T = cfg.T0
    span = X_HIGH - X_LOW
    rejects = 0

    while T > cfg.T_min:
        for _ in range(chain_length(T, cfg)):
            x_new = x + random.uniform(-1, 1) * span * (T / cfg.T0) * cfg.step_factor
            x_new = float(np.clip(x_new, X_LOW, X_HIGH))
            f_new = float(f(x_new))
            df = f_new - fx

            if df < 0 or random.random() < math.exp(-df / T):
                x, fx = x_new, f_new
                rejects = 0
                if f_new < f_best:
                    x_best, f_best = x_new, f_new
            else:
                rejects += 1
                if rejects >= cfg.max_reject:
                    return x_best, f_best
        T *= cfg.K
    return x_best, f_best


def plot_curve(x_best, f_best):
    xs = np.linspace(X_LOW, X_HIGH, 1000)
    ys = f(xs)
    plt.figure(figsize=(9, 4))
    plt.plot(xs, ys, label=r"$11\sin(6x)+7\cos(5x)$")
    plt.scatter([x_best], [f_best], c="blue", zorder=5, label=f"最优 ({x_best:.2f}, {f_best:.2f})")
    plt.xlabel("x")
    plt.ylabel("f(x)")
    plt.title("目标函数与模拟退火求得的最小值")
    plt.legend()
    plt.grid(alpha=0.3)
    plt.tight_layout()
    plt.savefig("results.png", dpi=150)
    plt.close()


def main():
    cfg = CoolingSchedule(
        T0=100.0,
        K=0.95,
        step_factor=1.0,
        L=120,
        x_init=float(np.random.uniform(X_LOW, X_HIGH)),
        T_min=0.01,
        max_reject=3000,
    )

    print("【冷却参数表】")
    print(f"  T0={cfg.T0}, K={cfg.K}, step_factor={cfg.step_factor}")
    print(f"  L={cfg.L}~{L_CAP}(随温度), x_init={cfg.x_init:.4f}")
    print(f"  T_min={cfg.T_min}, max_reject={cfg.max_reject}")
    print()

    x_best, f_best = simulated_annealing(cfg)
    print(f"在 x = {x_best} 处找到最小值, f(x) = {f_best}")
    plot_curve(x_best, f_best)
    print("函数图像已保存: results.png")


if __name__ == "__main__":
    main()
