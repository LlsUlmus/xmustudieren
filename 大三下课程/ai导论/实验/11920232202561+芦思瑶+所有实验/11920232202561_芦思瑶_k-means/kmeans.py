import os
import sys

import matplotlib.pyplot as plt
import numpy as np
from itertools import permutations
from matplotlib import rcParams

ATTR_NAMES = ["花萼长度", "花萼宽度", "花瓣长度", "花瓣宽度"]
SPECIES = ["Iris-setosa", "Iris-versicolor", "Iris-virginica"]


def get_base_dir():
    if getattr(sys, "frozen", False):
        return os.path.dirname(sys.executable)
    return os.path.dirname(os.path.abspath(__file__))


def load_iris(path):
    features, labels = [], []
    with open(path, encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            parts = line.split(",")
            features.append([float(x) for x in parts[:4]])
            labels.append(parts[4])
    X = np.array(features)
    label_to_id = {name: i for i, name in enumerate(SPECIES)}
    y = np.array([label_to_id[name] for name in labels])
    return X, y


class KMeans:
    def __init__(self, k=3, max_iter=300, tol=1e-4, random_state=42):
        self.k = k
        self.max_iter = max_iter
        self.tol = tol
        self.random_state = random_state
        self.centroids = None
        self.labels_ = None

    def _assign(self, X):
        diff = X[:, np.newaxis, :] - self.centroids[np.newaxis, :, :]
        distances = np.sqrt((diff ** 2).sum(axis=2))
        return np.argmin(distances, axis=1)

    def fit(self, X):
        rng = np.random.default_rng(self.random_state)
        n_samples = X.shape[0]
        init_idx = rng.choice(n_samples, self.k, replace=False)
        self.centroids = X[init_idx].copy()

        for _ in range(self.max_iter):
            labels = self._assign(X)
            new_centroids = np.zeros_like(self.centroids)
            for i in range(self.k):
                cluster_points = X[labels == i]
                if len(cluster_points) == 0:
                    new_centroids[i] = X[rng.integers(n_samples)]
                else:
                    new_centroids[i] = cluster_points.mean(axis=0)

            shift = np.linalg.norm(new_centroids - self.centroids)
            self.centroids = new_centroids
            if shift < self.tol:
                break

        self.labels_ = self._assign(X)
        return self

    def sse(self, X):
        total = 0.0
        for i in range(self.k):
            points = X[self.labels_ == i]
            if len(points) == 0:
                continue
            total += np.sum((points - self.centroids[i]) ** 2)
        return total


def best_label_mapping(cluster_labels, true_labels, k):
    best_mapping = {i: i for i in range(k)}
    best_score = -1
    for perm in permutations(range(k)):
        mapping = {cluster_id: true_id for cluster_id, true_id in enumerate(perm)}
        mapped = np.array([mapping[c] for c in cluster_labels])
        score = np.sum(mapped == true_labels)
        if score > best_score:
            best_score = score
            best_mapping = mapping
    return best_mapping


def clustering_accuracy(cluster_labels, true_labels, k):
    mapping = best_label_mapping(cluster_labels, true_labels, k)
    mapped = np.array([mapping[c] for c in cluster_labels])
    return np.mean(mapped == true_labels), mapping


def confusion_matrix(cluster_labels, true_labels, k):
    mapping = best_label_mapping(cluster_labels, true_labels, k)
    mapped = np.array([mapping[c] for c in cluster_labels])
    matrix = np.zeros((k, k), dtype=int)
    for pred, actual in zip(mapped, true_labels):
        matrix[pred, actual] += 1
    return matrix, mapping


def setup_plot_style():
    rcParams["font.sans-serif"] = ["SimHei", "Microsoft YaHei", "DejaVu Sans"]
    rcParams["axes.unicode_minus"] = False


def plot_results(X, cluster_labels, true_labels, centroids, save_path):
    setup_plot_style()
    fig, axes = plt.subplots(1, 2, figsize=(12, 5))

    x_idx, y_idx = 2, 3
    x_name, y_name = ATTR_NAMES[x_idx], ATTR_NAMES[y_idx]
    colors = ["#1f77b4", "#2ca02c", "#d946ef"]

    ax = axes[0]
    for i in range(3):
        mask = cluster_labels == i
        ax.scatter(
            X[mask, x_idx],
            X[mask, y_idx],
            c=colors[i],
            label=f"簇 {i}",
            alpha=0.8,
            edgecolors="k",
            linewidths=0.3,
        )
    ax.scatter(
        centroids[:, x_idx],
        centroids[:, y_idx],
        c="black",
        marker="X",
        s=180,
        linewidths=1.5,
        label="簇中心",
        zorder=5,
    )
    ax.set_xlabel(x_name)
    ax.set_ylabel(y_name)
    ax.set_title("K-Means 聚类结果")
    ax.legend()
    ax.grid(True, alpha=0.3)

    ax = axes[1]
    for i in range(3):
        mask = true_labels == i
        ax.scatter(
            X[mask, x_idx],
            X[mask, y_idx],
            c=colors[i],
            label=SPECIES[i],
            alpha=0.8,
            edgecolors="k",
            linewidths=0.3,
        )
    ax.set_xlabel(x_name)
    ax.set_ylabel(y_name)
    ax.set_title("真实分类标签")
    ax.legend()
    ax.grid(True, alpha=0.3)

    plt.tight_layout()
    plt.savefig(save_path, dpi=150, bbox_inches="tight")
    plt.close()


def print_results(acc, matrix, mapping, sse, centroids):
    print("K-Means 鸢尾花数据集聚类结果")
    print(f"K = 3")
    print(f"SSE = {sse:.4f}")
    print(f"Accuracy = {acc * 100:.2f}%")

    print("\nCluster -> Label mapping:")
    for cluster_id, true_id in sorted(mapping.items()):
        print(f"  Cluster {cluster_id} -> {SPECIES[true_id]}")

    print("\nCentroids:")
    for i, c in enumerate(centroids):
        values = ", ".join(f"{v:.2f}" for v in c)
        print(f"  Cluster {i}: [{values}]")

    print("\nConfusion matrix (row=predicted, col=true):")
    header = " " * 18 + "".join(f"{name:>18}" for name in SPECIES)
    print(header)
    for i in range(len(SPECIES)):
        row = "".join(f"{matrix[i, j]:>18}" for j in range(len(SPECIES)))
        print(f"{SPECIES[i]:>18}{row}")
    print("=" * 50)


def main():
    base_dir = get_base_dir()
    data_path = os.path.join(base_dir, "Iris.txt")
    plot_path = os.path.join(base_dir, "kmeans_result.png")

    X, y = load_iris(data_path)

    kmeans = KMeans(k=3, random_state=42)
    kmeans.fit(X)

    acc, mapping = clustering_accuracy(kmeans.labels_, y, 3)
    matrix, _ = confusion_matrix(kmeans.labels_, y, 3)
    sse = kmeans.sse(X)

    plot_results(X, kmeans.labels_, y, kmeans.centroids, plot_path)
    print_results(acc, matrix, mapping, sse, kmeans.centroids)
    print(f"Plot saved to: {plot_path}")


if __name__ == "__main__":
    main()
