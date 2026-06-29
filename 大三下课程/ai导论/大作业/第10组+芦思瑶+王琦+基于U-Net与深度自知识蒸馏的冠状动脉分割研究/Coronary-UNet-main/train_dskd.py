"""
核心组件:
  L_DDL   - Deep Distribution Loss (KL散度, 解码器各层分布对齐)
  L_PSDL  - Pixel-wise Self-knowledge Distillation Loss (软标签监督)
  L_DICE  - Dice Loss

Teacher = 上一 epoch 结束时的模型参数快照
Student = 当前 epoch 正在训练的模型
"""

import os
import copy
import json
import torch
import torch.nn as nn
import torch.nn.functional as F
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
from torchvision import transforms
from sklearn.model_selection import train_test_split
from tqdm import tqdm
from PIL import Image
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

# ===== 超参数 =====
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Database_134_Angiograms")
DEVICE      = "cuda" if torch.cuda.is_available() else "cpu"
EPOCHS      = 50
BATCH_SIZE  = 4
LR          = 1e-3
WEIGHT_DECAY = 1e-5
TAU         = 3.0    # 温度缩放因子（论文消融最优）
N_PATCHES   = 16     # patch 数量 4×4（论文消融最优）
ALPHA_T     = 0.5   # PSDL 最终 alpha 值（论文消融最优）
SEED        = 42


# ===================== 数据集 =====================
class DCA1Dataset(Dataset):
    def __init__(self, file_pairs):
        self.file_pairs = file_pairs
        self.transform = transforms.Compose([
            transforms.Resize((256, 256)),
            transforms.ToTensor(),
        ])

    def __len__(self):
        return len(self.file_pairs)

    def __getitem__(self, idx):
        img_path, mask_path = self.file_pairs[idx]
        img  = self.transform(Image.open(img_path).convert("L"))
        mask = self.transform(Image.open(mask_path).convert("L"))
        mask = (mask > 0.5).float()
        return img, mask


# ===================== 模型 =====================
class DoubleConv(nn.Module):
    def __init__(self, in_c, out_c):
        super().__init__()
        self.conv = nn.Sequential(
            nn.Conv2d(in_c, out_c, 3, padding=1), nn.BatchNorm2d(out_c), nn.ReLU(inplace=True),
            nn.Conv2d(out_c, out_c, 3, padding=1), nn.BatchNorm2d(out_c), nn.ReLU(inplace=True),
        )
    def forward(self, x): return self.conv(x)


class UNetDSKD(nn.Module):
    """
    U-Net 带多层侧输出头，用于 Deep Distribution Loss
    侧输出从各解码器层引出，上采样至输入尺寸后通过 Sigmoid 输出概率图
    空间尺寸（输入 256×256）:
        u3 → 128×128 → 2× bilinear → 256×256
        u2 → 256×256 （主输出）
    forward 返回: (main_out, [side_out_deep, side_out_shallow])
        side_out_deep   = u3 引出的侧输出（较深层，粗粒度特征）
        side_out_shallow= u2 引出的侧输出 = main_out（最浅层，最终预测）
    """
    def __init__(self):
        super().__init__()
        self.d1   = DoubleConv(1, 64)
        self.d2   = DoubleConv(64, 128)
        self.d3   = DoubleConv(128, 256)
        self.pool = nn.MaxPool2d(2)
        self.up3  = nn.ConvTranspose2d(256, 128, 2, stride=2)
        self.up2  = nn.ConvTranspose2d(128, 64,  2, stride=2)
        self.u3   = DoubleConv(256, 128)
        self.u2   = DoubleConv(128, 64)
        # 主输出头
        self.out  = nn.Conv2d(64, 1, 1)
        # 侧输出头（u3 层：3×3 降维卷积 + bilinear 上采样）
        self.side_conv = nn.Conv2d(128, 1, 3, padding=1)
        self.sigmoid   = nn.Sigmoid()

    def forward(self, x):
        c1 = self.d1(x)
        c2 = self.d2(self.pool(c1))
        c3 = self.d3(self.pool(c2))
        u3 = self.u3(torch.cat([self.up3(c3), c2], 1))   # 128×128
        u2 = self.u2(torch.cat([self.up2(u3), c1], 1))   # 256×256

        main = self.sigmoid(self.out(u2))                  # 主输出 256×256

        # 侧输出：u3 → 3×3 conv → bilinear upsample → sigmoid
        side_deep = self.sigmoid(
            F.interpolate(self.side_conv(u3),
                          size=x.shape[2:], mode='bilinear', align_corners=True)
        )  # 256×256

        # 从深到浅排列（论文中 i=1 为最深层）
        return main, [side_deep, main]


# ===================== 损失函数 =====================

def dice_loss(pred, target, smooth=1.0):
    p = pred.view(-1); t = target.view(-1)
    return 1 - (2.0 * (p * t).sum() + smooth) / (p.sum() + t.sum() + smooth)


def to_distribution_vector(side_out, n_patches=16, tau=3.0):
    """
    将侧输出转换为 patch 概率分布向量（论文 Eq.2-3）
    side_out : [B, 1, H, W]  sigmoid 输出
    returns  : [B, n_patches, 2]  温度缩放后的 softmax 概率
    """
    B, _, H, W = side_out.shape
    n_side = int(n_patches ** 0.5)          # 每边 patch 数，4
    ph, pw = H // n_side, W // n_side       # 单个 patch 尺寸

    x = side_out.squeeze(1)                 # [B, H, W]
    x = x.unfold(1, ph, ph).unfold(2, pw, pw)  # [B, n_side, n_side, ph, pw]
    x = x.contiguous().view(B, n_patches, ph * pw)  # [B, n, patch_pixels]

    # 软计数（可微分，近似二值化后的计数）
    ones  = x.sum(dim=-1)                   # [B, n]
    zeros = ph * pw - ones                  # [B, n]
    Z = torch.stack([ones, zeros], dim=-1)  # [B, n, 2]

    return F.softmax(Z / tau, dim=-1)       # [B, n, 2]


def deep_distribution_loss(sides_S, sides_T, n_patches=16, tau=3.0, eps=1e-8):
    """
    L_DDL = Σ_i KL(P_S^i || P_T^i)  (论文 Eq.4)
    """
    loss = 0.0
    for s_S, s_T in zip(sides_S, sides_T):
        P_S = to_distribution_vector(s_S, n_patches, tau)
        P_T = to_distribution_vector(s_T, n_patches, tau)
        # KL 散度（只对 P_S 求梯度）
        loss += (P_S * (torch.log(P_S + eps) - torch.log(P_T.detach() + eps))).sum(-1).mean()
    return loss


def pixel_wise_skd_loss(pred_S, pred_T, mask, alpha):
    """
    L_PSDL = CE(ỹ_S, ŷ)，ŷ = α·ỹ_T + (1-α)·y  (论文 Eq.5-6)
    """
    soft_label = alpha * pred_T.detach() + (1.0 - alpha) * mask
    return F.binary_cross_entropy(pred_S, soft_label)


# ===================== 评估指标（含 IoU）=====================
def compute_metrics(pred, mask):
    pred_b = (pred > 0.5).float()
    tp = (pred_b * mask).sum()
    fp =  pred_b.sum() - tp
    fn =  mask.sum() - tp
    tn = ((1 - pred_b) * (1 - mask)).sum()
    s  = 1e-6
    f1  = (2 * tp + s) / (2 * tp + fp + fn + s)
    iou = (tp + s) / (tp + fp + fn + s)
    acc = (pred_b == mask).float().mean()
    sen = (tp + s) / (tp + fn + s)
    spe = (tn + s) / (tn + fp + s)
    return acc.item(), f1.item(), iou.item(), sen.item(), spe.item()


# ===================== 数据准备（7:1:2 划分）=====================
def prepare_data():
    all_pairs = []
    for fn in os.listdir(ROOT):
        if fn.endswith(".pgm") and not fn.endswith("_gt.pgm"):
            mask_p = os.path.join(ROOT, fn[:-4] + "_gt.pgm")
            if os.path.exists(mask_p):
                all_pairs.append((os.path.join(ROOT, fn), mask_p))

    # 先切出 20% 作为测试集，再从剩余 80% 中切 12.5% 作为验证集
    # 结果: 70% train / 10% val / 20% test  ≈  7:1:2
    train_val, test   = train_test_split(all_pairs, test_size=0.20,  random_state=SEED)
    train,     val    = train_test_split(train_val, test_size=0.125, random_state=SEED)

    print(f"数据划分: 训练={len(train)} / 验证={len(val)} / 测试={len(test)}")
    return train, val, test


# ===================== 主训练流程 =====================
def main():
    print(f"训练设备: {DEVICE}")
    train_pairs, val_pairs, test_pairs = prepare_data()

    train_loader = DataLoader(DCA1Dataset(train_pairs), batch_size=BATCH_SIZE, shuffle=True)
    val_loader   = DataLoader(DCA1Dataset(val_pairs),   batch_size=1, shuffle=False)
    test_loader  = DataLoader(DCA1Dataset(test_pairs),  batch_size=1, shuffle=False)

    model     = UNetDSKD().to(DEVICE)
    optimizer = optim.AdamW(model.parameters(), lr=LR, weight_decay=WEIGHT_DECAY)
    # 每 10 epoch 学习率降为 30%（论文设置）
    scheduler = optim.lr_scheduler.StepLR(optimizer, step_size=10, gamma=0.3)

    teacher_model = None   # epoch 0 无 teacher
    best_f1       = 0.0

    history = {k: [] for k in
               ["train_loss", "val_f1", "val_iou", "val_acc", "val_sen", "val_spe"]}

    for epoch in range(EPOCHS):
        # ---- 更新 teacher（本 epoch 开始前，复制上一 epoch 末的模型）----
        if epoch > 0:
            teacher_model = copy.deepcopy(model).to(DEVICE)
            teacher_model.eval()
            for p in teacher_model.parameters():
                p.requires_grad_(False)

        # alpha 线性增大 (论文 Eq.7)
        alpha_t = ALPHA_T * epoch / EPOCHS

        # ---- 训练 ----
        model.train()
        loss_sum = 0.0
        for img, mask in tqdm(train_loader, desc=f"Ep{epoch:02d} TRAIN", leave=False):
            img, mask = img.to(DEVICE), mask.to(DEVICE)
            optimizer.zero_grad()

            main_S, sides_S = model(img)
            L_dice = dice_loss(main_S, mask)

            if teacher_model is None:
                # epoch 0：只用 Dice Loss（无 teacher）
                loss = L_dice
            else:
                with torch.no_grad():
                    main_T, sides_T = teacher_model(img)

                L_ddl  = deep_distribution_loss(sides_S, sides_T, N_PATCHES, TAU)
                L_psdl = pixel_wise_skd_loss(main_S, main_T, mask, alpha_t)
                loss   = L_ddl + L_psdl + L_dice

            loss.backward()
            optimizer.step()
            loss_sum += loss.item()

        scheduler.step()
        history["train_loss"].append(loss_sum / len(train_loader))

        # ---- 验证 ----
        model.eval()
        sums = {k: 0.0 for k in ["acc", "f1", "iou", "sen", "spe"]}
        with torch.no_grad():
            for img, mask in val_loader:
                img, mask = img.to(DEVICE), mask.to(DEVICE)
                main_out, _ = model(img)
                a, f, i, se, sp = compute_metrics(main_out, mask)
                sums["acc"] += a; sums["f1"] += f; sums["iou"] += i
                sums["sen"] += se; sums["spe"] += sp

        n = len(val_loader)
        val_f1 = sums["f1"] / n
        for k in sums:
            history[f"val_{k}"].append(sums[k] / n)

        print(f"Ep{epoch:02d} | Loss={history['train_loss'][-1]:.4f} | "
              f"F1={val_f1:.4f} | IoU={sums['iou']/n:.4f} | "
              f"Sen={sums['sen']/n:.4f} | Spe={sums['spe']/n:.4f} | "
              f"α={alpha_t:.3f} | LR={scheduler.get_last_lr()[0]:.2e}")

        if val_f1 > best_f1:
            best_f1 = val_f1
            torch.save(model.state_dict(), "best_dskd_model.pth")

    print(f"\n训练完成！验证集最佳 F1: {best_f1:.4f}")

    # ---- 测试集最终评估 ----
    model.load_state_dict(torch.load("best_dskd_model.pth", map_location=DEVICE))
    model.eval()
    sums = {k: 0.0 for k in ["acc", "f1", "iou", "sen", "spe"]}
    with torch.no_grad():
        for img, mask in test_loader:
            img, mask = img.to(DEVICE), mask.to(DEVICE)
            main_out, _ = model(img)
            a, f, i, se, sp = compute_metrics(main_out, mask)
            sums["acc"] += a; sums["f1"] += f; sums["iou"] += i
            sums["sen"] += se; sums["spe"] += sp

    n = len(test_loader)
    test_results = {k: sums[k] / n for k in sums}
    print("\n===== 测试集最终结果 =====")
    print(f"  F1 (DSC) : {test_results['f1']:.4f}")
    print(f"  IoU      : {test_results['iou']:.4f}")
    print(f"  Accuracy : {test_results['acc']:.4f}")
    print(f"  Sensitivity: {test_results['sen']:.4f}")
    print(f"  Specificity: {test_results['spe']:.4f}")

    print("\n===== 与论文 DCA1 结果对比 =====")
    print(f"{'指标':<14} {'论文结果':>10} {'本实验':>10}")
    print("-" * 36)
    paper = {"F1(DSC)": 81.06, "IoU": 66.95, "Accuracy": 97.85, "Sensitivity": 81.36}
    our   = {"F1(DSC)": test_results['f1']*100, "IoU": test_results['iou']*100,
             "Accuracy": test_results['acc']*100, "Sensitivity": test_results['sen']*100}
    for k in paper:
        print(f"  {k:<12} {paper[k]:>9.2f}%  {our[k]:>9.2f}%")

    with open("dskd_test_results.json", "w") as f:
        json.dump(test_results, f, indent=2)

    # ---- 绘图 ----
    fig, axes = plt.subplots(1, 3, figsize=(15, 4))
    fig.suptitle("DSKD Training Curves", fontsize=13, fontweight="bold")

    axes[0].plot(history["train_loss"], color="#2196F3")
    axes[0].set_title("Training Loss"); axes[0].set_xlabel("Epoch"); axes[0].grid(alpha=0.3)

    axes[1].plot(history["val_f1"],  label="F1",  color="#4CAF50")
    axes[1].plot(history["val_iou"], label="IoU", color="#FF9800")
    axes[1].set_title("Val F1 & IoU"); axes[1].set_xlabel("Epoch")
    axes[1].legend(); axes[1].grid(alpha=0.3)

    axes[2].plot(history["val_sen"], label="Sensitivity", color="#F44336")
    axes[2].plot(history["val_spe"], label="Specificity",  color="#9C27B0")
    axes[2].set_title("Val Sensitivity & Specificity"); axes[2].set_xlabel("Epoch")
    axes[2].legend(); axes[2].grid(alpha=0.3)

    plt.tight_layout()
    plt.savefig("dskd_train_result.png", dpi=150, bbox_inches="tight")
    print("\n训练曲线已保存: dskd_train_result.png")


if __name__ == "__main__":
    main()
