# 作业五：Chubby 类开源项目分析 + Paxos 协议重实现与性能临界点实验

## 目录结构

```
paxos/                 Paxos 原型（纯 Python 标准库，未使用任何 Paxos/Raft 库）
  wire.py              TCP 自定义二进制报文：40 字节定长头 + 负载 + CRC32；提案编号编码
  storage.py           Acceptor 的 WAL：promised_id / accepted_id / accepted_value 落盘，组提交，可开关 fsync
  net.py               TCP 传输（断线重连）+ 应用层故障注入（延迟 / 抖动 / 丢包 / 分区）
  node.py              节点：Proposer + Acceptor + Learner（+ Listener），Basic / Multi / Fast 三种提案机制
  client.py            客户端库、命令行、负载发生器（闭环 / 开环）
experiments/
  harness.py           单机多进程集群编排：启动 / kill / 重启 / 注入故障 / 采集统计 / 核对 Learner 一致性
  run.py               全部实验（11 组）
  plots.py             由 data/*.csv 画图
  smoke.py             冒烟测试
docker/                Dockerfile + docker-compose.yml（4 投票节点 + 1 Listener，固定 IP）
scripts/               启动脚本（本机 / 虚拟机）、tc netem 注入脚本
data/                  原始数据：每组实验一个 CSV（每次运行一行）+ raw/ 下逐请求延迟记录
docs/etcd架构分析.md    开源项目（etcd）架构与关键代码分析
report/实验报告.docx    实验报告（另有同内容的 .pdf；实验报告.md 为源稿，build_report.py 为生成脚本）
```

## 环境要求

- Python ≥ 3.8。节点与客户端**只依赖标准库**；画图/生成报告需要 `matplotlib pandas python-docx`。
- 本报告的数据在 Windows 11 + Python 3.10 上以“单机多进程 + 回环 TCP + 应用层故障注入”方式测得。
- Docker / 虚拟机部署文件已提供（见下），协议代码与平台无关。

## 快速开始（单机多进程）

```bash
# Linux / macOS / Git Bash
./scripts/start_local.sh 3 multi 0        # 3 个投票节点 + 1 个 Listener(9)，模式 multi，fsync 关
```

```powershell
# Windows PowerShell
.\scripts\start_local.ps1 -N 3 -Mode multi -Fsync 0
```

```bash
PEERS=1=127.0.0.1:7001,2=127.0.0.1:7002,3=127.0.0.1:7003
python -m paxos.client --peers $PEERS status                 # 谁是 Leader、各节点已决前缀
python -m paxos.client --peers $PEERS put hello world        # 提交一条命令
python -m paxos.client --peers $PEERS --node 3 get hello     # 从节点 3 的状态机读取
python -m paxos.client --peers $PEERS net --rtt-ms 10 --loss 0.01   # 运行时注入 RTT=10ms、丢包 1%
python -m paxos.client --peers $PEERS bench --clients 8 --duration 5
```

手工启动单个节点：

```bash
python -m paxos.node --id 1 --peers 1=127.0.0.1:7001,2=127.0.0.1:7002,3=127.0.0.1:7003 \
       --listeners 9=127.0.0.1:7009 --mode fast --fsync 1 --data-dir data
```

主要参数（`python -m paxos.node -h`）：

| 参数 | 含义 | 默认 |
|---|---|---|
| `--mode basic\|multi\|fast` | 提案机制 | multi |
| `--fsync 0\|1` | Acceptor 回复前是否 fsync | 1 |
| `--hb-ms` / `--election-ms` | Leader 心跳间隔 / 选举超时基数 T（实际在 [T,2T) 随机） | 50 / 300 |
| `--retry-ms` | 各阶段超时重试时间 | 200 |
| `--backoff-ms` | 冲突后随机指数退避的基数（0 = 不退避，用于活锁实验） | 5 |
| `--batch` / `--linger-ms` / `--window` | 批大小 / 攒批等待 / Leader 流水线窗口 | 1 / 0 / 512 |
| `--fast-wait-ms` | Fast Paxos 协调者等待迟到投票的时间 | 30 |
| `--contend 1` | Basic：所有 Proposer 抢同一最低空闲实例 | 0 |

## Docker 部署（固定 IP + tc netem）

```bash
# 4 个投票节点 172.28.0.11-14 + Listener 172.28.0.21，容器内端口 7000，宿主机 7001-7004/7009
MODE=multi FSYNC=1 docker compose -f docker/docker-compose.yml up -d --build

# 3 节点
PEERS=1=172.28.0.11:7000,2=172.28.0.12:7000,3=172.28.0.13:7000 \
  docker compose -f docker/docker-compose.yml up -d --build n1 n2 n3 l1

# 内核层注入：每个容器单向延迟 5ms（RTT≈10ms）+ 丢包 1%
for n in n1 n2 n3 n4; do docker compose -f docker/docker-compose.yml exec $n sh /app/scripts/netem.sh set 5 1; done

# 故障
docker kill docker-n2-1          # kill Acceptor（SIGKILL）
docker start docker-n2-1         # 重启，从 WAL 恢复
docker compose -f docker/docker-compose.yml exec n1 sh /app/scripts/netem.sh block 172.28.0.12   # 分区

# 从宿主机压测
python -m paxos.client --peers 1=127.0.0.1:7001,2=127.0.0.1:7002,3=127.0.0.1:7003,4=127.0.0.1:7004 bench
```

> 说明：编写本作业的机器上没有 Docker / WSL，因此 **Docker 文件未经实际运行验证**（compose 文件已通过 YAML 语法检查）。报告中的全部数据来自单机多进程方式。两种注入方式的差别见报告 4.2 节。

## 虚拟机部署

每台 Ubuntu 虚拟机固定 IP，放行端口后执行（修改脚本内 `PEERS` 为实际 IP）：

```bash
sudo ufw allow 7000/tcp
PEERS=1=192.168.56.11:7000,2=192.168.56.12:7000,3=192.168.56.13:7000 ./scripts/start_node.sh 1 multi 1
sudo ./scripts/netem.sh set 5 1      # 可选：注入延迟/丢包
```

## 复现实验

```bash
python experiments/smoke.py                 # 冒烟：三种模式 × 3/4 节点
python experiments/run.py --list            # 列出实验组
python experiments/run.py --exp baseline    # 跑一组；--exp all 跑全部（顺序执行约 1.5 小时；可用不同的 --port-base 分组并行）
python experiments/plots.py                 # 生成 report/figures/*.png
python report/build_report.py               # 由 report/实验报告.md + data/*.csv 生成 实验报告.docx
```

| 实验组 | 内容 | 输出 |
|---|---|---|
| `baseline` | 轻载下 3 种机制 × 3/4/5 节点 | `data/baseline.csv` |
| `concurrency` | 并发提案数 1–64 | `data/concurrency.csv` |
| `rtt` | RTT 0/1/10/50/100ms | `data/rtt.csv` |
| `loss` | RTT × 丢包率（0–40%）网格 | `data/loss.csv` |
| `fsync` / `batch` | fsync 开关、批处理 | `data/fsync.csv` `data/batch.csv` |
| `livelock` | Basic Paxos 活锁（竞争者数 × 退避窗口） | `data/livelock.csv` |
| `fastconflict` | Fast Paxos 冲突率 vs 到达率，对比 Multi | `data/fastconflict.csv` |
| `faults` | kill Acceptor / kill Leader / 分区 / 超过容错上限 | `data/faults.csv` + 时间线 |
| `recovery` | Leader 恢复时间 vs 选举超时 | `data/recovery.csv` |
| `chaos` | 随机故障下的安全性检查 | `data/chaos.csv` |

CSV 字段：`throughput`（req/s）、`p50_ms/p95_ms/p99_ms`、`msgs_per_op`（每条命令的节点间报文数，含心跳）、`conflict_rate`、`rounds_per_op`、`recovery_ms`（最长不可用窗口）、`diverged`（不同 Learner 学到不同值的实例数）、`safety_violations`（节点自检到的冲突学习次数）等。`data/raw/<组>/<运行>.csv` 为逐请求记录 `t_start_s,latency_ms,ok,node`（除故障类实验外每次运行随机保留至多 2000 条）。

## 现场演示与截图

```powershell
powershell -ExecutionPolicy Bypass -File scripts\demo\run_all.ps1
```

依次在 9 个真实控制台窗口中执行：启动集群、读写、查看 WAL、杀 Leader、超过容错上限、注入网络故障、三种机制冒烟测试、4 节点 Fast Paxos、Basic 活锁，并把每个窗口截图保存到 `report/screenshots/`（报告第二、四、五章使用；截图会自动裁去标题栏、主机名与本机路径）。仅支持 Windows；Python 路径用环境变量 `PAXOS_PYTHON` 指定。`python scripts/wal_dump.py <wal文件>` 可单独查看 Acceptor 的持久化内容。

## 实现要点

- **报文**：`'PX' | ver | type | src | flags | slot | ballot | aux | len | crc32` + 负载，共 17 种报文（Prepare/Promise/Accept/Accepted/Nack/Learn/Heartbeat/FastPropose/Forward/LearnReq/…）。
- **提案编号**：`round<<16 | node<<1 | classic`，全局唯一、全序、单调递增。
- **持久化**：Acceptor 在回复 Promise/Accepted 之前把记录追加到 WAL 并（可选）fsync；启动时重放 WAL 恢复，尾部写坏的记录按 CRC 截断。
- **多数派**：经典多数派 `qc = ⌊n/2⌋+1`；Fast Paxos 快速多数派 `qf` 取满足 `qc + 2·qf > 2n` 的最小值（n=3→3，n=4→3，n=5→4）。
- **Multi-Paxos**：心跳选主，范围 Prepare 一次覆盖后续全部实例，之后只做 Phase 2；流水线 + 批处理；Accept 超时重传；落后节点主动追赶。
- **Fast Paxos**：协调者下发 Any 后各 Proposer 直投；冲突时协调者做 coordinated recovery（把快速轮的 2b 当作下一经典轮的 1b，省一轮 Prepare）。
- **安全性自检**：Learner 对同一实例学到不同值时计数并报警；实验脚本在每次运行结束后比对所有节点（含 Listener）的全部实例。

> 若某次运行报 `cluster not ready`，多半是端口被本机其他程序占用，换一个 `--port-base` 重跑即可。
