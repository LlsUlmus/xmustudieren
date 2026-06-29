import math
import os
import sys
from collections import Counter

import numpy as np

ATTR_NAMES = ["花萼长度", "花萼宽度", "花瓣长度", "花瓣宽度"]


def load_data(path):
    data = np.loadtxt(path)
    if data.ndim == 1:
        data = data.reshape(1, -1)
    return data[:, :-1], data[:, -1].astype(int)


def entropy(labels):
    if len(labels) == 0:
        return 0.0
    n = len(labels)
    ent = 0.0
    for c in Counter(labels.tolist()).values():
        p = c / n
        if p > 0:
            ent -= p * math.log2(p)
    return ent


def best_continuous_split(features, labels, attr):
    order = np.argsort(features[:, attr])
    sorted_x = features[order, attr]
    if len(sorted_x) <= 1:
        return entropy(labels), float("-inf")
    candidates = np.unique((sorted_x[:-1] + sorted_x[1:]) / 2.0)
    best_info = float("inf")
    best_threshold = candidates[0]
    n = len(labels)
    for th in candidates:
        left_mask = features[:, attr] <= th
        right_mask = ~left_mask
        if left_mask.sum() == 0 or right_mask.sum() == 0:
            continue
        info_split = (left_mask.sum() / n) * entropy(labels[left_mask]) + (
            right_mask.sum() / n
        ) * entropy(labels[right_mask])
        if info_split < best_info:
            best_info = info_split
            best_threshold = th
    if best_info == float("inf"):
        return entropy(labels), float(sorted_x.mean())
    return best_info, float(best_threshold)


def choose_best_attribute(features, labels, attrs):
    base_ent = entropy(labels)
    best_attr, best_gain, best_threshold = None, -1.0, 0.0
    for attr in attrs:
        info_a, threshold = best_continuous_split(features, labels, attr)
        gain = base_ent - info_a
        if gain > best_gain:
            best_gain, best_attr, best_threshold = gain, attr, threshold
    return best_attr, best_gain, best_threshold


def majority_label(labels):
    return Counter(labels.tolist()).most_common(1)[0][0]


def build_tree(features, labels, attrs):
    if len(set(labels.tolist())) == 1:
        return int(labels[0])
    attr, gain, threshold = choose_best_attribute(features, labels, attrs)
    if attr is None or gain <= 1e-12:
        return majority_label(labels)
    left_mask = features[:, attr] <= threshold
    right_mask = ~left_mask
    if left_mask.sum() == 0 or right_mask.sum() == 0:
        return majority_label(labels)
    return {
        "attr": attr,
        "threshold": threshold,
        "gain": gain,
        "left": build_tree(features[left_mask], labels[left_mask], attrs),
        "right": build_tree(features[right_mask], labels[right_mask], attrs),
    }


def predict_one(sample, tree):
    if isinstance(tree, int):
        return tree
    if sample[tree["attr"]] <= tree["threshold"]:
        return predict_one(sample, tree["left"])
    return predict_one(sample, tree["right"])


def predict(features, tree):
    return np.array([predict_one(features[i], tree) for i in range(len(features))])


def tree_to_lines(tree, indent="", branch="根"):
    if isinstance(tree, int):
        return [f"{indent}{branch} -> 类别 {tree}"]
    name = ATTR_NAMES[tree["attr"]]
    th, gain = tree["threshold"], tree["gain"]
    lines = [f"{indent}{branch}: {name} <= {th:.4f} ? (增益={gain:.4f})"]
    sub = indent + "  "
    lines += tree_to_lines(tree["left"], sub, f"是(≤{th:.4f})")
    lines += tree_to_lines(tree["right"], sub, f"否(>{th:.4f})")
    return lines


def main():
    if hasattr(sys.stdout, "reconfigure"):
        try:
            sys.stdout.reconfigure(encoding="utf-8")
        except Exception:
            pass

    base = os.path.dirname(os.path.abspath(__file__))
    train_path = os.path.join(base, "traindata.txt")
    test_path = os.path.join(base, "testdata.txt")

    x_train, y_train = load_data(train_path)
    x_test, y_test = load_data(test_path)
    attrs = list(range(x_train.shape[1]))
    tree = build_tree(x_train, y_train, attrs)

    y_pred = predict(x_test, tree)
    train_acc = (predict(x_train, tree) == y_train).mean()
    test_acc = (y_pred == y_test).mean()
    correct = int((y_pred == y_test).sum())

    print("ID3 决策树")
    print(f"训练集: traindata.txt  样本数: {len(y_train)}")
    print(f"测试集: testdata.txt  样本数: {len(y_test)}")
    print("\n【决策树结构】")
    for line in tree_to_lines(tree):
        print(line)
    print(f"\n训练集准确率: {train_acc * 100:.2f}%")
    print(f"测试集准确率: {test_acc * 100:.2f}%  ({correct}/{len(y_test)})")


if __name__ == "__main__":
    main()
