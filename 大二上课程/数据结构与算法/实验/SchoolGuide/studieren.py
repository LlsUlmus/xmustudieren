import tkinter as tk
from tkinter import messagebox
import heapq

class CampusGuideSystem:
    def __init__(self):
        # 景点信息和图数据（可替换为您的学校地图）
        self.places = {
            0: {"name": "校门", "description": "学校的正门入口"},
            1: {"name": "图书馆", "description": "提供丰富的图书资源"},
            2: {"name": "教学楼A", "description": "主要用于理科教学"},
            3: {"name": "教学楼B", "description": "主要用于文科教学"},
            4: {"name": "食堂", "description": "提供师生餐饮服务"},
            5: {"name": "操场", "description": "体育活动和锻炼的场所"},
            6: {"name": "实验楼", "description": "实验课程教学"},
            7: {"name": "宿舍区", "description": "学生住宿区域"},
            8: {"name": "礼堂", "description": "大型活动举办地"},
            9: {"name": "行政楼", "description": "学校管理部门所在地"},
        }

        # 图的邻接矩阵（无向图）
        self.graph = [
            [0, 2, 0, 0, 0, 0, 0, 3, 0, 0],
            [2, 0, 4, 0, 0, 0, 0, 0, 0, 0],
            [0, 4, 0, 5, 0, 0, 6, 0, 0, 0],
            [0, 0, 5, 0, 2, 0, 0, 0, 0, 0],
            [0, 0, 0, 2, 0, 3, 0, 0, 0, 0],
            [0, 0, 0, 0, 3, 0, 0, 0, 0, 7],
            [0, 0, 6, 0, 0, 0, 0, 4, 0, 0],
            [3, 0, 0, 0, 0, 0, 4, 0, 5, 0],
            [0, 0, 0, 0, 0, 0, 0, 5, 0, 6],
            [0, 0, 0, 0, 0, 7, 0, 0, 6, 0],
        ]

    def get_place_info(self, place_id):
        return self.places.get(place_id, None)

    def dijkstra(self, start, end):
        n = len(self.graph)
        distances = [float('inf')] * n
        distances[start] = 0
        priority_queue = [(0, start)]
        prev = [-1] * n

        while priority_queue:
            current_distance, current_node = heapq.heappop(priority_queue)

            if current_distance > distances[current_node]:
                continue

            for neighbor in range(n):
                if self.graph[current_node][neighbor] > 0:
                    distance = current_distance + self.graph[current_node][neighbor]

                    if distance < distances[neighbor]:
                        distances[neighbor] = distance
                        prev[neighbor] = current_node
                        heapq.heappush(priority_queue, (distance, neighbor))

        # 构建路径
        path = []
        at = end
        while at != -1:
            path.append(at)
            at = prev[at]
        path.reverse()

        return path if distances[end] != float('inf') else None, distances[end]

    def tsp(self, places):
        from itertools import permutations
        best_path = None
        min_distance = float('inf')

        for perm in permutations(places):
            current_distance = 0
            valid = True

            for i in range(len(perm) - 1):
                _, dist = self.dijkstra(perm[i], perm[i + 1])
                if dist == float('inf'):
                    valid = False
                    break
                current_distance += dist

            if valid and current_distance < min_distance:
                min_distance = current_distance
                best_path = perm

        return best_path, min_distance

class CampusGuideApp:
    def __init__(self, root):
        self.system = CampusGuideSystem()
        self.root = root
        self.root.title("校园导游咨询系统")

        self.create_widgets()

    def create_widgets(self):
        # 景点信息查询
        tk.Label(self.root, text="景点编号").grid(row=0, column=0)
        self.place_id_entry = tk.Entry(self.root)
        self.place_id_entry.grid(row=0, column=1)
        tk.Button(self.root, text="查询景点信息", command=self.query_place_info).grid(row=0, column=2)

        # 最短路径查询
        tk.Label(self.root, text="起点").grid(row=1, column=0)
        self.start_entry = tk.Entry(self.root)
        self.start_entry.grid(row=1, column=1)

        tk.Label(self.root, text="终点").grid(row=1, column=2)
        self.end_entry = tk.Entry(self.root)
        self.end_entry.grid(row=1, column=3)

        tk.Button(self.root, text="查询最短路径", command=self.query_shortest_path).grid(row=1, column=4)

        # 最佳路径查询
        tk.Label(self.root, text="景点序列 (用逗号分隔)").grid(row=2, column=0)
        self.places_entry = tk.Entry(self.root)
        self.places_entry.grid(row=2, column=1, columnspan=3)
        tk.Button(self.root, text="查询最佳路径", command=self.query_best_route).grid(row=2, column=4)

    def query_place_info(self):
        place_id = self.place_id_entry.get()
        if not place_id.isdigit():
            messagebox.showerror("错误", "请输入有效的景点编号！")
            return

        info = self.system.get_place_info(int(place_id))
        if info:
            messagebox.showinfo("景点信息", f"名称: {info['name']}\n简介: {info['description']}")
        else:
            messagebox.showerror("错误", "景点不存在！")

    def query_shortest_path(self):
        start = self.start_entry.get()
        end = self.end_entry.get()

        if not (start.isdigit() and end.isdigit()):
            messagebox.showerror("错误", "请输入有效的起点和终点编号！")
            return

        path, distance = self.system.dijkstra(int(start), int(end))
        if path:
            messagebox.showinfo("最短路径", f"路径: {' -> '.join(map(str, path))}\n距离: {distance}")
        else:
            messagebox.showerror("错误", "两点之间不存在路径！")

    def query_best_route(self):
        places = self.places_entry.get()
        try:
            places_list = list(map(int, places.split(',')))
        except ValueError:
            messagebox.showerror("错误", "请输入有效的景点编号序列！")
            return

        path, distance = self.system.tsp(places_list)
        if path:
            messagebox.showinfo("最佳路径", f"路径: {' -> '.join(map(str, path))}\n距离: {distance}")
        else:
            messagebox.showerror("错误", "无法找到最佳路径！")

if __name__ == "__main__":
    root = tk.Tk()
    app = CampusGuideApp(root)
    root.mainloop()
