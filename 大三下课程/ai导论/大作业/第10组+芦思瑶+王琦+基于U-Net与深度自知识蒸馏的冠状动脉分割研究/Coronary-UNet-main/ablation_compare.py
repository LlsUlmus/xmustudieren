"""
消融实验结果可视化
"""

import os
import json
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
import numpy as np

RESULTS_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "results")

EXP_ORDER = [
    "exp1_baseline",
    "exp2_no_skip",
    "exp3_dice_loss",
    "exp4_bce_dice",
    "exp5_augment",
]

LABELS = {
    "exp1_baseline":  "Baseline\n(U-Net+BCE)",
    "exp2_no_skip":   "No Skip\nConnections",
    "exp3_dice_loss": "Dice Loss",
    "exp4_bce_dice":  "BCE+Dice\nLoss",
    "exp5_augment":   "U-Net+\nAugmentation",
}

COLORS = ["#2196F3", "#F44336", "#FF9800", "#4CAF50", "#9C27B0"]


def load_results():
    results = {}
    for name in EXP_ORDER:
        path = os.path.join(RESULTS_DIR, f"{name}_result.json")
        if os.path.exists(path):
            with open(path) as f:
                results[name] = json.load(f)
        else:
            print(f"  [警告] 未找到 {path}，跳过")
    return results


def plot_training_curves(results):
    fig, axes = plt.subplots(1, 2, figsize=(14, 5))
    fig.suptitle("Ablation Study: Training Curves", fontsize=14, fontweight="bold")

    for i, (name, data) in enumerate(results.items()):
        h = data["history"]
        label = LABELS[name].replace("\n", " ")
        c = COLORS[i % len(COLORS)]
        axes[0].plot(h["train_loss"], label=label, color=c, linewidth=1.8)
        axes[1].plot(h["val_f1"],    label=label, color=c, linewidth=1.8)

    axes[0].set_title("Training Loss")
    axes[0].set_xlabel("Epoch")
    axes[0].set_ylabel("BCE / Dice Loss")
    axes[0].legend(fontsize=8)
    axes[0].grid(True, alpha=0.3)

    axes[1].set_title("Validation F1 Score")
    axes[1].set_xlabel("Epoch")
    axes[1].set_ylabel("F1 Score")
    axes[1].legend(fontsize=8)
    axes[1].grid(True, alpha=0.3)
    axes[1].yaxis.set_major_formatter(ticker.FormatStrFormatter("%.3f"))

    plt.tight_layout()
    out = os.path.join(RESULTS_DIR, "ablation_curves.png")
    plt.savefig(out, dpi=150, bbox_inches="tight")
    print(f"训练曲线已保存: {out}")
    plt.close()


def plot_bar_comparison(results):
    metrics = ["best_f1", "best_acc", "best_sensitivity", "best_specificity"]
    metric_labels = ["F1 Score", "Accuracy", "Sensitivity", "Specificity"]

    names = [n for n in EXP_ORDER if n in results]
    x = np.arange(len(names))
    width = 0.2

    fig, ax = plt.subplots(figsize=(13, 5))
    fig.suptitle("Ablation Study: Metric Comparison (Best Epoch)", fontsize=14, fontweight="bold")

    for j, (metric, mlabel) in enumerate(zip(metrics, metric_labels)):
        vals = [results[n]["summary"][metric] for n in names]
        bars = ax.bar(x + j * width, vals, width, label=mlabel,
                      color=COLORS[j % len(COLORS)], alpha=0.85)
        for bar, v in zip(bars, vals):
            ax.text(bar.get_x() + bar.get_width() / 2, bar.get_height() + 0.002,
                    f"{v:.3f}", ha="center", va="bottom", fontsize=7)

    ax.set_xticks(x + width * 1.5)
    ax.set_xticklabels([LABELS[n] for n in names], fontsize=9)
    ax.set_ylabel("Score")
    ax.set_ylim(0, 1.12)
    ax.legend(loc="upper right", fontsize=9)
    ax.grid(axis="y", alpha=0.3)

    plt.tight_layout()
    out = os.path.join(RESULTS_DIR, "ablation_bars.png")
    plt.savefig(out, dpi=150, bbox_inches="tight")
    print(f"柱状图对比已保存: {out}")
    plt.close()


def print_latex_table(results):
    print("\n" + "=" * 75)
    print("消融实验结果汇总")
    print("=" * 75)
    header = f"{'实验配置':<24} {'F1 Score':>10} {'Accuracy':>10} {'Sensitivity':>13} {'Specificity':>13}"
    print(header)
    print("-" * 75)

    label_map = {
        "exp1_baseline":  "Baseline (U-Net + BCE)",
        "exp2_no_skip":   "w/o Skip Connections",
        "exp3_dice_loss": "Dice Loss (vs BCE)",
        "exp4_bce_dice":  "BCE + Dice Loss",
        "exp5_augment":   "U-Net + Augmentation",
    }

    baseline_f1 = results.get("exp1_baseline", {}).get("summary", {}).get("best_f1", 0)

    for name in EXP_ORDER:
        if name not in results:
            continue
        s = results[name]["summary"]
        diff = s["best_f1"] - baseline_f1
        diff_str = f"({diff:+.4f})" if name != "exp1_baseline" else "  baseline "
        print(f"{label_map[name]:<24} {s['best_f1']:>10.4f} {s['best_acc']:>10.4f} "
              f"{s['best_sensitivity']:>13.4f} {s['best_specificity']:>13.4f}  {diff_str}")

    print("=" * 75)

    # LaTeX 格式
    print("\n--- LaTeX 表格代码 ---")
    print(r"\begin{table}[h]")
    print(r"\centering")
    print(r"\caption{Ablation Study on DCA1 Coronary Artery Segmentation Dataset}")
    print(r"\begin{tabular}{lcccc}")
    print(r"\hline")
    print(r"Method & F1 Score & Accuracy & Sensitivity & Specificity \\")
    print(r"\hline")
    for name in EXP_ORDER:
        if name not in results:
            continue
        s = results[name]["summary"]
        lbl = label_map[name].replace("w/o", r"w/o").replace("&", r"\&")
        print(f"{lbl} & {s['best_f1']:.4f} & {s['best_acc']:.4f} & "
              f"{s['best_sensitivity']:.4f} & {s['best_specificity']:.4f} \\\\")
    print(r"\hline")
    print(r"\end{tabular}")
    print(r"\end{table}")


def main():
    results = load_results()
    if not results:
        print("没有找到任何实验结果，请先运行 ablation_train.py")
        return

    print(f"找到 {len(results)} 个实验结果")
    plot_training_curves(results)
    plot_bar_comparison(results)
    print_latex_table(results)
    print(f"\n所有图表已保存到 {RESULTS_DIR}/")


if __name__ == "__main__":
    main()
