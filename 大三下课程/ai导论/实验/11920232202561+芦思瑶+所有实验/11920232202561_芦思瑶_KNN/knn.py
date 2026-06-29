import math
import os
import sys
from collections import Counter

import matplotlib.pyplot as plt
import numpy as np
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


def train_test_split(X, y, test_size=0.3, random_state=42):
    rng = np.random.default_rng(random_state)
    n = len(y)
    indices = rng.permutation(n)
    test_count = int(n * test_size)
    test_idx = indices[:test_count]
    train_idx = indices[test_count:]
    return X[train_idx], X[test_idx], y[train_idx], y[test_idx]


def z_score_fit(X_train):
    mean = X_train.mean(axis=0)
    std = X_train.std(axis=0)
    std[std == 0] = 1.0
    return mean, std


def z_score_transform(X, mean, std):
    return (X - mean) / std


def euclidean_distance(x, sample):
    return np.sqrt(np.sum((x - sample) ** 2))


class KNN:
    def __init__(self, k=3):
        self.k = k
        self.X_train = None
        self.y_train = None

    def fit(self, X, y):
        self.X_train = np.asarray(X, dtype=float)
        self.y_train = np.asarray(y)
        return self

    def predict(self, X):
        return np.array([self._predict_one(x) for x in X])

    def _predict_one(self, x):
        distances = [euclidean_distance(x, sample) for sample in self.X_train]
        k_indices = np.argsort(distances)[: self.k]
        neighbor_labels = self.y_train[k_indices]
        return Counter(neighbor_labels.tolist()).most_common(1)[0][0]


def cross_val_score(model_factory, X, y, folds=5):
    n = len(y)
    fold_sizes = [n // folds] * folds
    for i in range(n % folds):
        fold_sizes[i] += 1

    scores = []
    start = 0
    for size in fold_sizes:
        end = start + size
        val_idx = np.arange(start, end)
        train_idx = np.concatenate([np.arange(0, start), np.arange(end, n)])
        start = end

        model = model_factory()
        model.fit(X[train_idx], y[train_idx])
        pred = model.predict(X[val_idx])
        scores.append(np.mean(pred == y[val_idx]))
    return float(np.mean(scores))


def candidate_k_values(n_train):
    upper = min(20, int(math.sqrt(n_train)))
    if upper < 1:
        upper = 1
    return [k for k in range(1, upper + 1) if k % 2 == 1]


def select_best_k(X_train, y_train):
    k_values = candidate_k_values(len(y_train))
    cv_scores = []
    for k in k_values:
        score = cross_val_score(lambda k=k: KNN(k=k), X_train, y_train, folds=5)
        cv_scores.append(score)
    best_idx = int(np.argmax(cv_scores))
    return k_values[best_idx], k_values, cv_scores


def confusion_matrix(y_true, y_pred, n_classes):
    matrix = np.zeros((n_classes, n_classes), dtype=int)
    for true, pred in zip(y_true, y_pred):
        matrix[pred, true] += 1
    return matrix


def setup_plot_style():
    rcParams["font.sans-serif"] = ["SimHei", "Microsoft YaHei", "DejaVu Sans"]
    rcParams["axes.unicode_minus"] = False


def plot_k_scores(k_values, cv_scores, best_k, save_path):
    setup_plot_style()
    plt.figure(figsize=(8, 5))
    plt.plot(k_values, cv_scores, marker="o", linewidth=1.8)
    plt.axvline(best_k, color="crimson", linestyle="--", label=f"最佳 K = {best_k}")
    plt.xlabel("K 值")
    plt.ylabel("5 折交叉验证准确率")
    plt.title("K 值选择")
    plt.grid(True, alpha=0.3)
    plt.legend()
    plt.tight_layout()
    plt.savefig(save_path, dpi=150, bbox_inches="tight")
    plt.close()


def print_results(best_k, k_values, cv_scores, acc, matrix):
    print("KNN 鸢尾花数据集分类结果")
    print(f"最佳 K = {best_k}")
    print(f"测试集准确率 = {acc * 100:.2f}%")

    print("\n各 K 的交叉验证准确率:")
    for k, score in zip(k_values, cv_scores):
        mark = " <-- best" if k == best_k else ""
        print(f"  K={k:2d}: {score * 100:.2f}%{mark}")

    print("\n混淆矩阵:")
    header = " " * 18 + "".join(f"{name:>18}" for name in SPECIES)
    print(header)
    for i in range(len(SPECIES)):
        row = "".join(f"{matrix[i, j]:>18}" for j in range(len(SPECIES)))
        print(f"{SPECIES[i]:>18}{row}")



def main():
    base_dir = get_base_dir()
    data_path = os.path.join(base_dir, "Iris.txt")
    plot_path = os.path.join(base_dir, "knn_k_selection.png")

    X, y = load_iris(data_path)
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.3, random_state=42)

    mean, std = z_score_fit(X_train)
    X_train = z_score_transform(X_train, mean, std)
    X_test = z_score_transform(X_test, mean, std)

    best_k, k_values, cv_scores = select_best_k(X_train, y_train)

    model = KNN(k=best_k)
    model.fit(X_train, y_train)
    y_pred = model.predict(X_test)

    acc = np.mean(y_pred == y_test)
    matrix = confusion_matrix(y_test, y_pred, len(SPECIES))

    plot_k_scores(k_values, cv_scores, best_k, plot_path)
    print_results(best_k, k_values, cv_scores, acc, matrix)
    print(f"Plot saved to: {plot_path}")


if __name__ == "__main__":
    main()
