"""
消融实验训练脚本 —— 冠状动脉分割
Skip Connection / Attention Gate / DSKD 各组件贡献
"""

import os
import json
import random
import torch
import torch.nn as nn
import torch.nn.functional as F
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
from torchvision import transforms
import torchvision.transforms.functional as TF
from sklearn.model_selection import train_test_split
from tqdm import tqdm
from PIL import Image

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Database_134_Angiograms")
RESULTS_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "results")
DEVICE = "cuda" if torch.cuda.is_available() else "cpu"
EPOCHS = 30
SEED = 42

os.makedirs(RESULTS_DIR, exist_ok=True)


# ===================== 数据集 =====================
class DCA1Dataset(Dataset):
    def __init__(self, file_pairs, augment=False):
        self.file_pairs = file_pairs
        self.augment = augment
        self.resize = transforms.Resize((256, 256))
        self.to_tensor = transforms.ToTensor()

    def __len__(self):
        return len(self.file_pairs)

    def __getitem__(self, idx):
        img_path, mask_path = self.file_pairs[idx]
        img = Image.open(img_path).convert("L")
        mask = Image.open(mask_path).convert("L")
        img = self.resize(img)
        mask = self.resize(mask)
        if self.augment:
            if random.random() > 0.5:
                img = TF.hflip(img); mask = TF.hflip(mask)
            if random.random() > 0.5:
                img = TF.vflip(img); mask = TF.vflip(mask)
            angle = random.uniform(-15, 15)
            img = TF.rotate(img, angle)
            mask = TF.rotate(mask, angle)
        img = self.to_tensor(img)
        mask = self.to_tensor(mask)
        mask = (mask > 0.5).float()
        return img, mask


# ===================== 公共模块 =====================
class DoubleConv(nn.Module):
    def __init__(self, in_c, out_c):
        super().__init__()
        self.conv = nn.Sequential(
            nn.Conv2d(in_c, out_c, 3, padding=1), nn.BatchNorm2d(out_c), nn.ReLU(inplace=True),
            nn.Conv2d(out_c, out_c, 3, padding=1), nn.BatchNorm2d(out_c), nn.ReLU(inplace=True),
        )
    def forward(self, x): return self.conv(x)


class AttentionGate(nn.Module):
    """注意力门控：对跳跃连接的特征做自适应加权"""
    def __init__(self, F_g, F_l, F_int):
        super().__init__()
        self.W_g = nn.Sequential(nn.Conv2d(F_g, F_int, 1), nn.BatchNorm2d(F_int))
        self.W_x = nn.Sequential(nn.Conv2d(F_l, F_int, 1), nn.BatchNorm2d(F_int))
        self.psi = nn.Sequential(nn.Conv2d(F_int, 1, 1), nn.BatchNorm2d(1), nn.Sigmoid())

    def forward(self, g, x):
        return x * self.psi(F.relu(self.W_g(g) + self.W_x(x), inplace=True))


# ===================== 模型定义 =====================

# Exp1: 基线 U-Net
class UNet(nn.Module):
    def __init__(self):
        super().__init__()
        self.d1 = DoubleConv(1, 64);  self.d2 = DoubleConv(64, 128); self.d3 = DoubleConv(128, 256)
        self.pool = nn.MaxPool2d(2)
        self.up3 = nn.ConvTranspose2d(256, 128, 2, stride=2)
        self.up2 = nn.ConvTranspose2d(128, 64, 2, stride=2)
        self.u3 = DoubleConv(256, 128); self.u2 = DoubleConv(128, 64)
        self.out = nn.Conv2d(64, 1, 1); self.sigmoid = nn.Sigmoid()

    def forward(self, x):
        c1 = self.d1(x); c2 = self.d2(self.pool(c1)); c3 = self.d3(self.pool(c2))
        u3 = self.u3(torch.cat([self.up3(c3), c2], 1))
        u2 = self.u2(torch.cat([self.up2(u3), c1], 1))
        return self.sigmoid(self.out(u2))


# Exp2: 去掉跳跃连接（验证 skip connection 作用）
class UNetNoSkip(nn.Module):
    def __init__(self):
        super().__init__()
        self.d1 = DoubleConv(1, 64);  self.d2 = DoubleConv(64, 128); self.d3 = DoubleConv(128, 256)
        self.pool = nn.MaxPool2d(2)
        self.up3 = nn.ConvTranspose2d(256, 128, 2, stride=2)
        self.up2 = nn.ConvTranspose2d(128, 64, 2, stride=2)
        self.u3 = DoubleConv(128, 128); self.u2 = DoubleConv(64, 64)
        self.out = nn.Conv2d(64, 1, 1); self.sigmoid = nn.Sigmoid()

    def forward(self, x):
        c1 = self.d1(x); c2 = self.d2(self.pool(c1)); c3 = self.d3(self.pool(c2))
        u3 = self.u3(self.up3(c3))
        u2 = self.u2(self.up2(u3))
        return self.sigmoid(self.out(u2))


# Exp3: Attention U-Net（验证注意力机制作用）
class AttentionUNet(nn.Module):
    def __init__(self):
        super().__init__()
        self.d1 = DoubleConv(1, 64);  self.d2 = DoubleConv(64, 128); self.d3 = DoubleConv(128, 256)
        self.pool = nn.MaxPool2d(2)
        self.up3 = nn.ConvTranspose2d(256, 128, 2, stride=2)
        self.up2 = nn.ConvTranspose2d(128, 64, 2, stride=2)
        self.att3 = AttentionGate(F_g=128, F_l=128, F_int=64)
        self.att2 = AttentionGate(F_g=64,  F_l=64,  F_int=32)
        self.u3 = DoubleConv(256, 128); self.u2 = DoubleConv(128, 64)
        self.out = nn.Conv2d(64, 1, 1); self.sigmoid = nn.Sigmoid()

    def forward(self, x):
        c1 = self.d1(x); c2 = self.d2(self.pool(c1)); c3 = self.d3(self.pool(c2))
        up3 = self.up3(c3)
        u3 = self.u3(torch.cat([up3, self.att3(g=up3, x=c2)], 1))
        up2 = self.up2(u3)
        u2 = self.u2(torch.cat([up2, self.att2(g=up2, x=c1)], 1))
        return self.sigmoid(self.out(u2))


# Exp4: U-Net + 深度自知识蒸馏（DSKD）
class UNetDSKD(nn.Module):
    """
    在解码器中间层增加辅助输出分支，主输出作为软标签指导辅助分支学习。
    推理时只使用主输出，辅助分支仅在训练时参与 loss 计算。
    """
    def __init__(self):
        super().__init__()
        self.d1 = DoubleConv(1, 64);  self.d2 = DoubleConv(64, 128); self.d3 = DoubleConv(128, 256)
        self.pool = nn.MaxPool2d(2)
        self.up3 = nn.ConvTranspose2d(256, 128, 2, stride=2)
        self.up2 = nn.ConvTranspose2d(128, 64, 2, stride=2)
        self.u3 = DoubleConv(256, 128); self.u2 = DoubleConv(128, 64)
        self.out = nn.Conv2d(64, 1, 1)
        # 辅助分支：从 u3 上采样，生成与主输出同尺寸的分割图
        self.aux_up  = nn.ConvTranspose2d(128, 64, 2, stride=2)
        self.aux_out = nn.Conv2d(64, 1, 1)
        self.sigmoid = nn.Sigmoid()

    def forward(self, x):
        c1 = self.d1(x); c2 = self.d2(self.pool(c1)); c3 = self.d3(self.pool(c2))
        u3 = self.u3(torch.cat([self.up3(c3), c2], 1))
        u2 = self.u2(torch.cat([self.up2(u3), c1], 1))
        main = self.sigmoid(self.out(u2))
        aux  = self.sigmoid(self.aux_out(self.aux_up(u3)))
        return main, aux   


# Exp5: Attention U-Net + DSKD（完整模型）
class AttentionUNetDSKD(nn.Module):
    def __init__(self):
        super().__init__()
        self.d1 = DoubleConv(1, 64);  self.d2 = DoubleConv(64, 128); self.d3 = DoubleConv(128, 256)
        self.pool = nn.MaxPool2d(2)
        self.up3 = nn.ConvTranspose2d(256, 128, 2, stride=2)
        self.up2 = nn.ConvTranspose2d(128, 64, 2, stride=2)
        self.att3 = AttentionGate(F_g=128, F_l=128, F_int=64)
        self.att2 = AttentionGate(F_g=64,  F_l=64,  F_int=32)
        self.u3 = DoubleConv(256, 128); self.u2 = DoubleConv(128, 64)
        self.out = nn.Conv2d(64, 1, 1)
        self.aux_up  = nn.ConvTranspose2d(128, 64, 2, stride=2)
        self.aux_out = nn.Conv2d(64, 1, 1)
        self.sigmoid = nn.Sigmoid()

    def forward(self, x):
        c1 = self.d1(x); c2 = self.d2(self.pool(c1)); c3 = self.d3(self.pool(c2))
        up3 = self.up3(c3)
        u3 = self.u3(torch.cat([up3, self.att3(g=up3, x=c2)], 1))
        up2 = self.up2(u3)
        u2 = self.u2(torch.cat([up2, self.att2(g=up2, x=c1)], 1))
        main = self.sigmoid(self.out(u2))
        aux  = self.sigmoid(self.aux_out(self.aux_up(u3)))
        return main, aux


# ===================== 损失函数 =====================
def bce_loss(pred, mask):
    return nn.BCELoss()(pred, mask)


def dskd_loss(main, aux, mask, alpha=0.4, beta=0.3):
    """
    alpha: 辅助分支 GT 监督权重
    beta:  自知识蒸馏权重（辅助向主输出学习）
    """
    loss_main = nn.BCELoss()(main, mask)
    loss_aux  = nn.BCELoss()(aux, mask)
    loss_kd   = nn.MSELoss()(aux, main.detach())  # 主输出作为软标签
    return loss_main + alpha * loss_aux + beta * loss_kd


# ===================== 指标 =====================
def compute_metrics(pred, mask):
    pred_b = (pred > 0.5).float()
    tp = (pred_b * mask).sum()
    fp = pred_b.sum() - tp
    fn = mask.sum() - tp
    tn = ((1 - pred_b) * (1 - mask)).sum()
    s = 1e-6
    f1  = (2 * tp + s) / (2 * tp + fp + fn + s)
    acc = (pred_b == mask).float().mean()
    sen = (tp + s) / (tp + fn + s)
    spe = (tn + s) / (tn + fp + s)
    return acc.item(), f1.item(), sen.item(), spe.item()


# ===================== 训练单个实验 =====================
def run_experiment(name, model, use_dskd, train_loader, test_loader):
    print(f"\n{'='*60}\n  实验: {name}\n{'='*60}")
    optimizer = optim.Adam(model.parameters(), lr=1e-3)
    best_f1 = 0.0
    history = {"train_loss": [], "val_f1": [], "val_acc": [],
               "val_sensitivity": [], "val_specificity": []}

    for epoch in range(EPOCHS):
        # ---- 训练 ----
        model.train()
        loss_sum = 0.0
        for img, mask in tqdm(train_loader, desc=f"Ep{epoch:02d} TRAIN", leave=False):
            img, mask = img.to(DEVICE), mask.to(DEVICE)
            optimizer.zero_grad()
            out = model(img)
            if use_dskd:
                main, aux = out
                loss = dskd_loss(main, aux, mask)
            else:
                loss = bce_loss(out, mask)
            loss.backward()
            optimizer.step()
            loss_sum += loss.item()

        history["train_loss"].append(loss_sum / len(train_loader))

        # ---- 验证 ----
        model.eval()
        acc_s = f1_s = sen_s = spe_s = 0.0
        with torch.no_grad():
            for img, mask in test_loader:
                img, mask = img.to(DEVICE), mask.to(DEVICE)
                out = model(img)
                pred = out[0] if use_dskd else out
                a, f, se, sp = compute_metrics(pred, mask)
                acc_s += a; f1_s += f; sen_s += se; spe_s += sp

        n = len(test_loader)
        val_f1 = f1_s / n
        history["val_f1"].append(val_f1)
        history["val_acc"].append(acc_s / n)
        history["val_sensitivity"].append(sen_s / n)
        history["val_specificity"].append(spe_s / n)
        print(f"  Ep{epoch:02d} | Loss={history['train_loss'][-1]:.4f} | "
              f"F1={val_f1:.4f} | Acc={acc_s/n:.4f} | Sen={sen_s/n:.4f} | Spe={spe_s/n:.4f}")

        if val_f1 > best_f1:
            best_f1 = val_f1
            torch.save(model.state_dict(), os.path.join(RESULTS_DIR, f"{name}_best.pth"))

    best_ep = history["val_f1"].index(max(history["val_f1"]))
    summary = {
        "best_f1":          history["val_f1"][best_ep],
        "best_acc":         history["val_acc"][best_ep],
        "best_sensitivity": history["val_sensitivity"][best_ep],
        "best_specificity": history["val_specificity"][best_ep],
        "best_epoch":       best_ep,
    }
    result = {"summary": summary, "history": history}
    with open(os.path.join(RESULTS_DIR, f"{name}_result.json"), "w") as f:
        json.dump(result, f, indent=2)
    print(f"\n  [{name}] 最佳 F1={best_f1:.4f}  (Epoch {best_ep})")
    return result


# ===================== 数据准备 =====================
def prepare_data():
    all_pairs = []
    for fn in os.listdir(ROOT):
        if fn.endswith(".pgm") and not fn.endswith("_gt.pgm"):
            mask_p = os.path.join(ROOT, fn[:-4] + "_gt.pgm")
            if os.path.exists(mask_p):
                all_pairs.append((os.path.join(ROOT, fn), mask_p))
    print(f"找到 {len(all_pairs)} 对图片+标签")
    return train_test_split(all_pairs, test_size=0.2, random_state=SEED)


# ===================== 主流程 =====================
def main():
    print(f"训练设备: {DEVICE}")
    train_pairs, test_pairs = prepare_data()

    def make_loaders(augment=False):
        tr = DataLoader(DCA1Dataset(train_pairs, augment=augment), batch_size=4, shuffle=True)
        te = DataLoader(DCA1Dataset(test_pairs),                   batch_size=1, shuffle=False)
        return tr, te

    train_loader, test_loader = make_loaders(augment=False)

    # ============================================================
    # 消融实验配置：(名称, 模型, 是否使用DSKD多输出)
    # ============================================================
    experiments = [
        ("exp1_baseline",      UNet().to(DEVICE),              False),
        ("exp2_no_skip",       UNetNoSkip().to(DEVICE),         False),
        ("exp3_attention",     AttentionUNet().to(DEVICE),      False),
        ("exp4_dskd",          UNetDSKD().to(DEVICE),           True),
        ("exp5_attn_dskd",     AttentionUNetDSKD().to(DEVICE),  True),
    ]

    all_results = {}
    for name, model, use_dskd in experiments:
        result = run_experiment(name, model, use_dskd, train_loader, test_loader)
        all_results[name] = result["summary"]

    # 汇总表格
    LABELS = {
        "exp1_baseline":   "Baseline U-Net",
        "exp2_no_skip":    "w/o Skip Connections",
        "exp3_attention":  "Attention U-Net",
        "exp4_dskd":       "U-Net + DSKD",
        "exp5_attn_dskd":  "Attention U-Net + DSKD (Full)",
    }
    print("\n" + "=" * 72)
    print(f"{'实验配置':<32} {'F1':>8} {'Acc':>8} {'Sen':>8} {'Spe':>8}")
    print("-" * 72)
    baseline_f1 = all_results["exp1_baseline"]["best_f1"]
    for name, s in all_results.items():
        diff = f"({s['best_f1']-baseline_f1:+.4f})" if name != "exp1_baseline" else " baseline"
        print(f"{LABELS[name]:<32} {s['best_f1']:>8.4f} {s['best_acc']:>8.4f} "
              f"{s['best_sensitivity']:>8.4f} {s['best_specificity']:>8.4f}  {diff}")
    print("=" * 72)

    with open(os.path.join(RESULTS_DIR, "ablation_summary.json"), "w") as f:
        json.dump(all_results, f, indent=2)
    print(f"\n结果已保存到 results/ablation_summary.json")


if __name__ == "__main__":
    main()
