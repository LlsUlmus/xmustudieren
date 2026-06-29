import random
import math
import matplotlib.pyplot as plt
import matplotlib

matplotlib.rcParams["font.sans-serif"] = ["SimHei", "Microsoft YaHei", "DejaVu Sans"]
matplotlib.rcParams["axes.unicode_minus"] = False

CITIES = [
    (41, 49), (35, 17), (55, 45), (55, 20), (15, 30),
    (25, 30), (20, 50), (10, 43), (55, 60), (30, 60),
    (20, 65), (50, 35), (30, 25), (15, 10), (30, 5),
    (10, 20), (5, 30), (20, 40), (15, 60), (45, 65),
    (45, 20), (45, 10), (55, 5), (65, 35), (65, 20),
    (45, 30), (35, 40), (41, 37), (64, 42), (40, 60),
]
N = len(CITIES)
DIST = [[0.0] * N for _ in range(N)]
for i in range(N):
    for j in range(N):
        if i != j:
            dx, dy = CITIES[i][0] - CITIES[j][0], CITIES[i][1] - CITIES[j][1]
            DIST[i][j] = math.hypot(dx, dy)

# 粒子群参数
SWARM_SIZE = 50
MAX_ITER = 500
C1, C2 = 2.0, 2.0
VMAX = 0.5
POS_LOW, POS_HIGH = 0.0, 1.0
OMEGA_INI, OMEGA_END = 0.9, 0.4


def path_distance(route):
    return sum(DIST[route[i]][route[(i + 1) % N]] for i in range(N))


def rov_to_route(position):
    """连续位置向量经排序映射为合法城市排列（ROV 合法性调整）。"""
    return list(sorted(range(N), key=lambda i: position[i]))


def clip_position(position):
    return [max(POS_LOW, min(POS_HIGH, v)) for v in position]


def clip_velocity(velocity):
    return [max(-VMAX, min(VMAX, v)) for v in velocity]


def random_position():
    return [random.uniform(POS_LOW, POS_HIGH) for _ in range(N)]


def random_velocity():
    return [random.uniform(-VMAX, VMAX) for _ in range(N)]


def omega_at(iteration):
    """线性递减惯性权重。"""
    return (OMEGA_INI - OMEGA_END) * (MAX_ITER - iteration) / MAX_ITER + OMEGA_END


def update_velocity(velocity, position, pbest_pos, gbest_pos, omega):
    new_v = []
    for d in range(N):
        r1, r2 = random.random(), random.random()
        cognitive = C1 * r1 * (pbest_pos[d] - position[d])
        social = C2 * r2 * (gbest_pos[d] - position[d])
        v = omega * velocity[d] + cognitive + social
        new_v.append(v)
    return clip_velocity(new_v)


def update_position(position, velocity):
    new_pos = [position[d] + velocity[d] for d in range(N)]
    return clip_position(new_pos)


def evaluate(position):
    route = rov_to_route(position)
    return path_distance(route), route


def initialize_swarm():
    swarm = []
    for _ in range(SWARM_SIZE):
        pos = random_position()
        vel = random_velocity()
        dist, route = evaluate(pos)
        swarm.append({
            "position": pos,
            "velocity": vel,
            "pbest_pos": pos[:],
            "pbest_dist": dist,
            "pbest_route": route[:],
            "dist": dist,
            "route": route[:],
        })
    gbest = min(swarm, key=lambda p: p["pbest_dist"])
    return swarm, gbest["pbest_pos"][:], gbest["pbest_dist"], gbest["pbest_route"][:]


def run_pso(seed=42, verbose=True):
    random.seed(seed)
    swarm, gbest_pos, gbest_dist, gbest_route = initialize_swarm()
    history = [gbest_dist]

    for iteration in range(1, MAX_ITER + 1):
        omega = omega_at(iteration)
        for particle in swarm:
            particle["velocity"] = update_velocity(
                particle["velocity"],
                particle["position"],
                particle["pbest_pos"],
                gbest_pos,
                omega,
            )
            particle["position"] = update_position(
                particle["position"],
                particle["velocity"],
            )
            dist, route = evaluate(particle["position"])
            particle["dist"] = dist
            particle["route"] = route

            if dist < particle["pbest_dist"]:
                particle["pbest_dist"] = dist
                particle["pbest_pos"] = particle["position"][:]
                particle["pbest_route"] = route[:]

            if dist < gbest_dist:
                gbest_dist = dist
                gbest_pos = particle["position"][:]
                gbest_route = route[:]

        history.append(gbest_dist)
        if verbose and (iteration % 100 == 0 or iteration == MAX_ITER):
            print(f"第{iteration}代 最优距离: {gbest_dist:.2f}")

    return gbest_dist, gbest_route, history


def plot_results(best_route, best_dist, history):
    plt.figure(figsize=(8, 5))
    plt.plot(history)
    plt.xlabel("迭代次数")
    plt.ylabel("最优路径距离")
    plt.title("粒子群算法收敛曲线")
    plt.grid(True, alpha=0.3)
    plt.savefig("pso_convergence.png", dpi=150)
    plt.close()

    xs = [CITIES[i][0] for i in best_route] + [CITIES[best_route[0]][0]]
    ys = [CITIES[i][1] for i in best_route] + [CITIES[best_route[0]][1]]
    plt.figure(figsize=(8, 7))
    plt.plot(xs, ys, "o-")
    plt.xlabel("X")
    plt.ylabel("Y")
    plt.title(f"PSO 最优路径 (距离={best_dist:.2f})")
    plt.grid(True, alpha=0.3)
    plt.savefig("pso_best_route.png", dpi=150)
    plt.close()


def main():
    print("=== 粒子群优化算法求解 TSP ===")
    print(f"粒子数: {SWARM_SIZE}, 粒子长度: {N}, 位置范围: [{POS_LOW}, {POS_HIGH}]")
    print(f"Vmax: {VMAX}, c1: {C1}, c2: {C2}, 最大迭代: {MAX_ITER}")
    print(f"惯性权重: {OMEGA_INI} -> {OMEGA_END} (线性递减)\n")

    best_dist, best_route, history = run_pso()
    print(f"\n最优距离: {best_dist:.2f}")
    print(f"最优路径: {best_route}")
    plot_results(best_route, best_dist, history)


if __name__ == "__main__":
    main()
