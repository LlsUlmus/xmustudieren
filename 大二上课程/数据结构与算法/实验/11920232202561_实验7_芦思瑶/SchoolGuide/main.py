import tkinter as tk
from tkinter import ttk, messagebox
import networkx as nx
import matplotlib
matplotlib.use('TkAgg')
import matplotlib.pyplot as plt
from matplotlib.image import imread
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
from itertools import permutations
import os  # 添加 os 模块导入
import sys

def resource_path(relative_path):
    if getattr(sys, 'frozen', False):  # 是否是打包环境
        base_path = sys._MEIPASS
    else:
        base_path = os.path.abspath(".")
    return os.path.join(base_path, relative_path)

# 使用这个函数获取背景图片路径
background_path = resource_path("01.png")

# 创建校园地图数据
def create_campus_map():
    G = nx.Graph()
    nodes = {
        "A": ("信息学院", "信息学院楼群，院系设置有人工智能系、计算机科学与技术系、软件工程系、信息与通信工程系。学科建设—计算机科学与技术、通信工程、人工智能、网络空间安全、软件工程、数字媒体技术6个本科招生专业。"),
        "B": ("文宣楼", "文宣楼：承接实验用途的教学楼，同时也是研究生据点之一。"),
        "C": ("国光宿舍楼群", "国光宿舍群：学生的根据地。"),
        "D": ("德旺图书馆", "德旺图书馆：厦门大学德旺图书馆坐落于翔安校区主楼群三号楼，2014年投入使用。建筑高度47.7米，地上9层，地下1层，建筑单体面积7.3724万平方米。德旺图书馆可收藏书、刊、报纸约300 万册，提供阅览座位 3500个，是一座互动良好、体验充分、服务优良、能够充分满足翔安校区师生员工需要的现代化智能图书馆。"),
        "E": ("药学院", "药学院：现有药学本科专业、药学第二学士学位专业、药学一级学科博士学位授权点、药学硕士专业学位授权点。招收药学专业本科生，药学学术型博士研究生，药理学、药物化学、药剂学、药物分析学学术型硕士研究生，药学专业学位硕士研究生。"),
        "F": ("航院", "航空与航天学院：厦门大学航空航天学院设飞行器系、动力工程系、机电工程系、仪器与电气系、自动化系等5个系，培养了一大批优秀人才，以艾兴、阙端麟、陈一坚、闵桂荣、张启先等院士为代表的院友为推进中国航空航天事业飞速发展做出了重要贡献。"),
        "G": ("学武楼", "学武楼：所有学院学生基本都会在此上课，承接全校课程的教学阵地。"),
        "H": ("一期操场", "一期操场：乐跑，体测，不只体育活动在此举办，还有各种各样的文娱。"),
        "I": ("东门", "学校东门：校门对面在中午和傍晚会有小摊贩出现，改善伙食的好地方。"),
        "J": ("思源餐厅", "思源餐厅：能吃"),
        "K": ("新工科大楼", "新工科大楼，孵化各种创意的根据地。许多社团在此扎根落地大放光彩，并对外代表厦门大学参加比赛。同样承接部分实验场地。"),
    }
    edges = [
        ("A", "B", 4.06),
        ("A", "C", 3.54),
        ("B", "D", 0.5),
        ("B", "E", 1.58),
        ("D", "G", 1.58),
        ("G", "H", 2.24),
        ("H", "I", 0.71),
        ("E", "F", 2.06),
        ("F", "G", 1.58),
        ("J", "F", 1.12),
        ("J", "K", 1.58),
    ]
    for code, (name, desc) in nodes.items():
        G.add_node(code, name=name, desc=desc)
    G.add_weighted_edges_from(edges)
    return G

# 绘制校园地图
def draw_campus_map_with_background(G, background_path):
    # 加载背景图片
    img = imread(background_path)

    # 节点布局
    pos = {
        "A": (1, 1),  # 信息学院 (最左下角)
        "B": (5, 1.5),  # 文宣楼
        "C": (2, 4.5),  # 国光
        "D": (5.5, 2),  # 德旺
        "E": (3.5, 2.5),  # 药学院
        "F": (5.5, 3),  # 航院
        "G": (7, 1.5),  # 小巨蛋
        "H": (9, 3),  # 一期操场
        "I": (9.5, 2.5),  # 东门 (最右边)
        "J": (4.5, 4.5),  # 思源
        "K": (5, 6),  # 新工科
    }

    # 调整背景范围
    fig, ax = plt.subplots(figsize=(16, 8))
    ax.imshow(img, extent=[0, 10, 0, 7])  # extent 决定图片范围

    # 绘制图的节点和边
    nx.draw(
        G, pos, ax=ax, with_labels=True, node_color="skyblue", node_size=1000,
        font_size=10, edge_color="gray"
    )
    labels = nx.get_edge_attributes(G, "weight")
    nx.draw_networkx_edge_labels(G, pos, edge_labels=labels, ax=ax, font_size=8)

    # 设置坐标范围和隐藏坐标轴
    ax.set_xlim(0, 10)
    ax.set_ylim(0, 7)
    ax.axis("off")

    return fig

# Tkinter界面
class CampusGuideApp:
    def __init__(self, root, background_path):
        self.root = root
        self.root.title("校园导游系统")
        self.background_path = background_path  # 存储背景路径

        # 检查背景图片是否存在
        if not os.path.exists(self.background_path):
            messagebox.showerror("错误", "背景图片路径无效")
            self.root.destroy()
            return

        self.campus_map = create_campus_map()
        self.fig = draw_campus_map_with_background(self.campus_map, self.background_path)
        self.create_widgets()

    def create_widgets(self):
        # 左侧功能区
        frame = tk.Frame(self.root)
        frame.pack(side=tk.LEFT, fill=tk.Y, padx=10, pady=10)

        # 查询景点信息
        tk.Label(frame, text="查询景点信息").pack(pady=5)
        self.info_combo = ttk.Combobox(frame, values=list(self.campus_map.nodes))
        self.info_combo.pack()
        tk.Button(frame, text="查询", command=self.query_info).pack(pady=5)

        # 查询最短路径
        tk.Label(frame, text="查询最短路径").pack(pady=5)
        self.start_combo = ttk.Combobox(frame, values=list(self.campus_map.nodes))
        self.start_combo.pack()
        self.end_combo = ttk.Combobox(frame, values=list(self.campus_map.nodes))
        self.end_combo.pack()
        tk.Button(frame, text="查询", command=self.query_shortest_path).pack(pady=5)

        # 信息展示框
        self.info_box = tk.Text(frame, height=30, width=40)
        self.info_box.pack(pady=10)

        # 查询多个景点的最佳路径
        tk.Label(frame, text="查询最佳访问路线").pack(pady=5)
        self.route_combo = tk.Entry(frame, width=30)
        self.route_combo.pack()
        tk.Button(frame, text="查询", command=self.query_best_route).pack(pady=5)

        # 绘制地图区域
        canvas_frame = tk.Frame(self.root)
        canvas_frame.pack(side=tk.RIGHT, expand=True, fill=tk.BOTH, padx=10, pady=10)
        canvas = FigureCanvasTkAgg(self.fig, master=canvas_frame)
        canvas.get_tk_widget().pack(fill=tk.BOTH, expand=True)
        canvas.draw()

    def query_info(self):
        node = self.info_combo.get()
        if not node:
            messagebox.showwarning("提示", "请选择一个景点")
            return
        data = self.campus_map.nodes[node]
        self.info_box.insert(tk.END, f"景点: {data['name']}\n简介: {data['desc']}\n\n")

    def query_shortest_path(self):
        start = self.start_combo.get()
        end = self.end_combo.get()
        if not start or not end:
            messagebox.showwarning("提示", "请选择起点和终点")
            return
        try:
            path = nx.shortest_path(self.campus_map, source=start, target=end, weight="weight")
            distance = nx.shortest_path_length(self.campus_map, source=start, target=end, weight="weight")
            self.info_box.insert(tk.END, f"最短路径: {' -> '.join(path)}\n总距离: {distance}\n\n")
        except nx.NetworkXNoPath:
            messagebox.showerror("错误", "两点之间没有可达路径")

    def query_best_route(self):
        waypoints = self.route_combo.get().split(",")
        if len(waypoints) < 2:
            messagebox.showwarning("提示", "请输入至少两个景点，并用逗号分隔")
            return

        try:
            for point in waypoints:
                if point.strip() not in self.campus_map.nodes:
                    messagebox.showerror("错误", f"无效的景点: {point.strip()}")
                    return

            best_path = None
            min_distance = float("inf")
            for perm in permutations(waypoints):
                distance = sum(
                    nx.shortest_path_length(self.campus_map, source=perm[i], target=perm[i + 1], weight="weight")
                    for i in range(len(perm) - 1)
                )
                if distance < min_distance:
                    min_distance = distance
                    best_path = perm

            self.info_box.insert(tk.END, f"最佳路径: {' -> '.join(best_path)}\n总距离: {min_distance:.2f}\n\n")

        except nx.NetworkXNoPath:
            messagebox.showerror("错误", "景点之间存在不可达的路径")

# 程序入口
if __name__ == "__main__":
    root = tk.Tk()
    background_path = "C:/Users/15379/Desktop/ds0101/01.png"
    app = CampusGuideApp(root, background_path)
    root.mainloop()
