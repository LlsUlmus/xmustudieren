import os
import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
from torchvision import transforms
from sklearn.model_selection import train_test_split
from tqdm import tqdm
from PIL import Image
import matplotlib.pyplot as plt

# ===================== 数据路径（与脚本同目录下的 Database_134_Angiograms）=====================
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Database_134_Angiograms")
DEVICE = "cuda" if torch.cuda.is_available() else "cpu"

# ===================== 数据集 =====================
class DCA1Dataset(Dataset):
    def __init__(self, file_pairs, transform=None):
        self.file_pairs = file_pairs
        self.transform = transform

    def __len__(self):
        return len(self.file_pairs)

    def __getitem__(self, idx):
        img_path, mask_path = self.file_pairs[idx]

        img = Image.open(img_path).convert("L")
        mask = Image.open(mask_path).convert("L")

        if self.transform:
            img = self.transform(img)
            mask = self.transform(mask)

        mask = (mask > 0.5).float()
        return img, mask

# ===================== 自动匹配 img & mask =====================
all_pairs = []
files = os.listdir(ROOT)

# 收集所有图片
for f in files:
    if f.endswith("_gt.pgm"):
        continue
    if f.endswith(".pgm"):
        base = f[:-4]
        img_path = os.path.join(ROOT, f"{base}.pgm")
        mask_path = os.path.join(ROOT, f"{base}_gt.pgm")

        if os.path.exists(mask_path):
            all_pairs.append((img_path, mask_path))

print(f"找到 {len(all_pairs)} 对图片+标签")

# ===================== 划分训练/测试 =====================
train_pairs, test_pairs = train_test_split(all_pairs, test_size=0.2, random_state=42)

# ===================== 数据变换 =====================
transform = transforms.Compose([
    transforms.Resize((256, 256)),
    transforms.ToTensor(),
])

train_ds = DCA1Dataset(train_pairs, transform)
test_ds = DCA1Dataset(test_pairs, transform)

train_loader = DataLoader(train_ds, batch_size=4, shuffle=True)
test_loader = DataLoader(test_ds, batch_size=1, shuffle=False)

# ===================== U-Net 模型 =====================
class DoubleConv(nn.Module):
    def __init__(self, in_c, out_c):
        super().__init__()
        self.conv = nn.Sequential(
            nn.Conv2d(in_c, out_c, 3, padding=1),
            nn.BatchNorm2d(out_c),
            nn.ReLU(inplace=True),
            nn.Conv2d(out_c, out_c, 3, padding=1),
            nn.BatchNorm2d(out_c),
            nn.ReLU(inplace=True)
        )
    def forward(self, x):
        return self.conv(x)

class UNet(nn.Module):
    def __init__(self):
        super().__init__()
        self.d1 = DoubleConv(1, 64)
        self.d2 = DoubleConv(64, 128)
        self.d3 = DoubleConv(128, 256)
        self.pool = nn.MaxPool2d(2)
        self.up3 = nn.ConvTranspose2d(256, 128, 2, stride=2)
        self.up2 = nn.ConvTranspose2d(128, 64, 2, stride=2)
        self.u3 = DoubleConv(256, 128)
        self.u2 = DoubleConv(128, 64)
        self.out = nn.Conv2d(64, 1, 1)
        self.sigmoid = nn.Sigmoid()

    def forward(self, x):
        c1 = self.d1(x)
        p1 = self.pool(c1)
        c2 = self.d2(p1)
        p2 = self.pool(c2)
        c3 = self.d3(p2)
        up3 = self.up3(c3)
        concat3 = torch.cat([up3, c2], dim=1)
        u3 = self.u3(concat3)
        up2 = self.up2(u3)
        concat2 = torch.cat([up2, c1], dim=1)
        u2 = self.u2(concat2)
        out = self.out(u2)
        return self.sigmoid(out)

# ===================== 指标 =====================
def compute_metrics(pred, mask):
    pred = (pred > 0.5).float()
    tp = (pred * mask).sum()
    fp = pred.sum() - tp
    fn = mask.sum() - tp
    smooth = 1e-6
    f1 = (2 * tp + smooth) / (2 * tp + fp + fn + smooth)
    acc = (pred == mask).float().mean()
    return acc, f1

# ===================== 训练 =====================
model = UNet().to(DEVICE)
criterion = nn.BCELoss()
optimizer = optim.Adam(model.parameters(), lr=1e-3)

best_f1 = 0.
train_losses = []
val_f1s = []

print(f"训练设备: {DEVICE}")

for epoch in range(30):
    # 训练
    model.train()
    loss_sum = 0.
    for img, mask in tqdm(train_loader, desc=f"Epoch {epoch} [TRAIN]"):
        img = img.to(DEVICE)
        mask = mask.to(DEVICE)

        out = model(img)
        loss = criterion(out, mask)

        optimizer.zero_grad()
        loss.backward()
        optimizer.step()

        loss_sum += loss.item()

    train_loss = loss_sum / len(train_loader)
    train_losses.append(train_loss)

    # 验证
    model.eval()
    total_f1 = 0.
    with torch.no_grad():
        for img, mask in tqdm(test_loader, desc=f"Epoch {epoch} [VAL]"):
            img = img.to(DEVICE)
            mask = mask.to(DEVICE)

            out = model(img)
            acc, f1 = compute_metrics(out, mask)
            total_f1 += f1.item()

    val_f1 = total_f1 / len(test_loader)
    val_f1s.append(val_f1)

    print(f"Epoch {epoch} | Train Loss={train_loss:.4f} Val F1={val_f1:.4f}")

    if val_f1 > best_f1:
        best_f1 = val_f1
        torch.save(model.state_dict(), "best_model.pth")

print(f"训练完成！最佳 F1: {best_f1:.4f}")

# 画图
plt.figure(figsize=(8, 3))
plt.subplot(1,2,1)
plt.plot(train_losses, label="Train Loss")
plt.legend()
plt.subplot(1,2,2)
plt.plot(val_f1s, label="Val F1")
plt.legend()
plt.savefig("train_result.png")