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

POP_SIZE, MAX_GEN, PC, PM, ELITE = 200, 500, 0.85, 0.02, 2


def path_distance(route):
    return sum(DIST[route[i]][route[(i + 1) % N]] for i in range(N))


def initialize(pop_size, num_cities):
    population = []
    base = list(range(num_cities))
    for _ in range(pop_size):
        ind = base[:]
        random.shuffle(ind)
        population.append(ind)
    return population


def fitness(route):
    return 1.0 / (1.0 + path_distance(route))


def selection(population, fit_values):
    total = sum(fit_values)
    if total == 0:
        return random.choice(population)[:]
    pick = random.uniform(0, total)
    acc = 0.0
    for i, fv in enumerate(fit_values):
        acc += fv
        if acc >= pick:
            return population[i][:]
    return population[-1][:]


def select_population(population, fit_values, pop_size):
    order = sorted(range(len(fit_values)), key=lambda i: fit_values[i], reverse=True)
    new_pop = [population[order[i]][:] for i in range(ELITE)]
    while len(new_pop) < pop_size:
        new_pop.append(selection(population, fit_values))
    return new_pop


def crossover(p1, p2):
    n = len(p1)
    start, end = sorted(random.sample(range(n), 2))

    def ox(a, b):
        child = [-1] * n
        child[start:end + 1] = a[start:end + 1]
        used = set(child[start:end + 1])
        pos = (end + 1) % n
        for city in b:
            if city not in used:
                child[pos] = city
                pos = (pos + 1) % n
        return child

    return ox(p1, p2), ox(p2, p1)


def crossover_population(population, pc):
    random.shuffle(population)
    new_pop = []
    for i in range(0, len(population) - 1, 2):
        a, b = population[i], population[i + 1]
        if random.random() < pc:
            c1, c2 = crossover(a, b)
            new_pop.extend([c1, c2])
        else:
            new_pop.extend([a[:], b[:]])
    if len(population) % 2:
        new_pop.append(population[-1][:])
    return new_pop[:len(population)]


def mutate(individual, pm):
    if random.random() < pm:
        i, j = random.sample(range(len(individual)), 2)
        individual[i], individual[j] = individual[j], individual[i]
    return individual


def mutate_population(population, pm):
    return [mutate(ind[:], pm) for ind in population]


def main():
    random.seed(42)
    pop = initialize(POP_SIZE, N)
    fits = [fitness(ind) for ind in pop]

    best_idx = max(range(len(fits)), key=lambda i: fits[i])
    best_route = pop[best_idx][:]
    best_dist = path_distance(best_route)
    history = [best_dist]

    for gen in range(1, MAX_GEN + 1):
        pop = select_population(pop, fits, POP_SIZE)
        pop = crossover_population(pop, PC)
        pop = mutate_population(pop, PM)
        fits = [fitness(ind) for ind in pop]

        idx = max(range(len(fits)), key=lambda i: fits[i])
        dist = path_distance(pop[idx])
        if dist < best_dist:
            best_dist, best_route = dist, pop[idx][:]
        history.append(best_dist)

        if gen % 100 == 0 or gen == MAX_GEN:
            print(f"第{gen}代 最优距离: {best_dist:.2f}")

    print(f"最优距离: {best_dist:.2f}")
    print(f"最优路径: {best_route}")

    plt.figure(figsize=(8, 5))
    plt.plot(history)
    plt.xlabel("迭代代数")
    plt.ylabel("最优路径距离")
    plt.title("遗传算法收敛曲线")
    plt.grid(True, alpha=0.3)
    plt.savefig("convergence.png", dpi=150)
    plt.close()

    xs = [CITIES[i][0] for i in best_route] + [CITIES[best_route[0]][0]]
    ys = [CITIES[i][1] for i in best_route] + [CITIES[best_route[0]][1]]
    plt.figure(figsize=(8, 7))
    plt.plot(xs, ys, "o-")
    plt.xlabel("X")
    plt.ylabel("Y")
    plt.title(f"最优路径 (距离={best_dist:.2f})")
    plt.grid(True, alpha=0.3)
    plt.savefig("best_route.png", dpi=150)
    plt.close()


if __name__ == "__main__":
    main()
