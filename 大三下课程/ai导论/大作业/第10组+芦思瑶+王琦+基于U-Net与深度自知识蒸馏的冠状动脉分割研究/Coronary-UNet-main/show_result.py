import os
import torch
import numpy as np
from PIL import Image
from torchvision import transforms
import matplotlib.pyplot as plt

# ===================== 路径（与脚本同目录下的 Database_134_Angiograms）=====================
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Database_134_Angiograms")
MODEL_PATH = "best_model.pth"
DEVICE = "cuda" if torch.cuda.is_available() else "cpu"

# ===================== 模型 =====================
class DoubleConv(torch.nn.Module):
    def __init__(self, in_c, out_c):
        super().__init__()
        self.conv = torch.nn.Sequential(
            torch.nn.Conv2d(in_c, out_c, 3, padding=1),
            torch.nn.BatchNorm2d(out_c),
            torch.nn.ReLU(inplace=True),
            torch.nn.Conv2d(out_c, out_c, 3, padding=1),
            torch.nn.BatchNorm2d(out_c),
            torch.nn.ReLU(inplace=True)
        )
    def forward(self, x):
        return self.conv(x)

class UNet(torch.nn.Module):
    def __init__(self):
        super().__init__()
        self.d1 = DoubleConv(1, 64)
        self.d2 = DoubleConv(64, 128)
        self.d3 = DoubleConv(128, 256)
        self.pool = torch.nn.MaxPool2d(2)
        self.up3 = torch.nn.ConvTranspose2d(256, 128, 2, stride=2)
        self.up2 = torch.nn.ConvTranspose2d(128, 64, 2, stride=2)
        self.u3 = DoubleConv(256, 128)
        self.u2 = DoubleConv(128, 64)
        self.out = torch.nn.Conv2d(64, 1, 1)
        self.sigmoid = torch.nn.Sigmoid()

    def forward(self, x):
        c1 = self.d1(x)
        c2 = self.d2(self.pool(c1))
        c3 = self.d3(self.pool(c2))
        up3 = self.up3(c3)
        u3 = self.u3(torch.cat([up3, c2], dim=1))
        up2 = self.up2(u3)
        u2 = self.u2(torch.cat([up2, c1], dim=1))
        return self.sigmoid(self.out(u2))

# ===================== 加载模型 =====================
model = UNet().to(DEVICE)
model.load_state_dict(torch.load(MODEL_PATH, map_location=DEVICE))
model.eval()

# ===================== 随便选一张图看效果 =====================
idx = 10  # 你可以改 0~133 任意数字
files = os.listdir(ROOT)
img_file = [f for f in files if f.endswith(".pgm") and not "_gt" in f][idx]
base = img_file[:-4]

img_path = os.path.join(ROOT, f"{base}.pgm")
mask_path = os.path.join(ROOT, f"{base}_gt.pgm")

# 读取
img = Image.open(img_path).convert("L")
mask = Image.open(mask_path).convert("L")

# 预处理
transform = transforms.Compose([
    transforms.Resize((256,256)),
    transforms.ToTensor()
])

x = transform(img).unsqueeze(0).to(DEVICE)

# 预测
with torch.no_grad():
    pred = model(x)

# 转成图片
img_np = np.array(img.resize((256,256)))
mask_np = np.array(mask.resize((256,256)))
pred_np = (pred[0,0].cpu().numpy() > 0.5) * 255

# 画图
plt.figure(figsize=(15,5))
plt.subplot(1,3,1); plt.imshow(img_np, cmap="gray"); plt.title("Input Image")
plt.subplot(1,3,2); plt.imshow(mask_np, cmap="gray"); plt.title("Ground Truth")
plt.subplot(1,3,3); plt.imshow(pred_np, cmap="gray"); plt.title("Prediction")
plt.savefig("verify.png")
print("对比图：verify.png")
print("原图 → 真实血管 → 模型分割的血管")