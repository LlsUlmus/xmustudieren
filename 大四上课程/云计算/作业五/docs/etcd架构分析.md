# 开源项目架构分析：etcd —— 一个类 Chubby 的协调服务

## 2.1 选型与源码版本

Chubby 是 Google 内部的粗粒度锁服务：用 Paxos 把一个小型文件系统复制到 5 个副本上，对外提供**锁、会话（租约）、事件通知和小文件存储**。开源世界里与它定位最接近的是 ZooKeeper、etcd 和 Consul。本报告选择 etcd，理由是：

1. 功能上与 Chubby 一一对应：KV 存储 ↔ 小文件，Lease ↔ 会话，Watch ↔ 事件，`concurrency.Mutex` ↔ 锁。
2. 共识层（`go.etcd.io/raft`）是一个独立、无 I/O 的库，代码边界清晰，便于和自己实现的 Paxos 逐点对照。
3. Raft 与 Multi-Paxos 是同一类协议（稳定 Leader + 多数派日志复制），对照价值高。

源码通过 `git clone --depth 1` 拉取，分析基于以下提交（本节引用的文件路径与行号均对应这两个提交）：

| 仓库 | 提交 | 提交日期 | 说明 |
|---|---|---|---|
| `etcd-io/etcd` | `76d58e32b30d` | 2026-10-05 | 服务端、客户端、存储（`server/go.mod` 依赖 `go.etcd.io/raft/v3 v3.7.0`、`go.etcd.io/bbolt v1.5.0`） |
| `etcd-io/raft` | `1c0011d2c6b7` | 2026-09-22 | Raft 共识库 |

代码规模（不含测试，`wc -l`）：

| 模块 | 路径 | 行数 |
|---|---|---|
| Raft 库 | `raft/` | 11,626 |
| 服务端核心 | `etcd/server/etcdserver/` | 25,771 |
| MVCC 存储 | `etcd/server/storage/mvcc/` | 3,970 |
| WAL | `etcd/server/storage/wal/` | 2,579 |
| 租约 | `etcd/server/lease/` | 1,762 |
| 客户端并发原语 | `etcd/client/v3/concurrency/` | 1,063 |

## 2.2 总体架构

etcd 是一个典型的**复制状态机**：所有写请求先被编码成日志条目，经 Raft 在多数派上持久化（commit），再由每个节点按相同顺序应用（apply）到本地的 MVCC 存储。

```
              gRPC (KV / Watch / Lease / Txn)            节点间 HTTP 流 (rafthttp)
  client ───────────────►┌──────────────────────────┐◄──────────────────────► 其他节点
                         │ api/v3rpc   请求校验/配额  │
                         ├──────────────────────────┤
                         │ EtcdServer (v3_server.go) │  Propose ─┐      ┌─ 等待 apply 结果
                         │   wait.Wait 按请求ID挂起    │          ▼      │
                         ├──────────────────────────┤   ┌───────────────────┐
                         │ raftNode (etcdserver/raft.go)│◄─►│ go.etcd.io/raft    │ 纯状态机，无I/O
                         │   Ready 循环：落盘→发送→提交 │   │ Step()/Ready()/Advance()
                         ├───────────┬──────────────┤   └───────────────────┘
                         │ WAL(fsync) │ Snapshot      │   持久化：日志条目 + HardState(term,vote,commit)
                         ├───────────┴──────────────┤
                         │ apply/uber_applier → txn  │   按 index 顺序应用已提交条目
                         ├──────────────────────────┤
                         │ mvcc: treeIndex + bbolt    │   多版本 KV；watchableStore 产生事件
                         │ lease.Lessor               │   租约到期由 Leader 发起 Revoke 提案
                         └──────────────────────────┘
```

分层要点：

- **`raft` 库不做任何 I/O**。它只是一个状态机：外部通过 `Step(msg)` 喂入消息和时钟 tick，通过 `Ready()` 取出“需要落盘的条目、需要发送的消息、已提交可应用的条目”。网络和磁盘都由使用者（etcdserver）负责。这使得协议逻辑可以被确定性地测试。
- **etcdserver** 负责把库“接上”真实世界：WAL 落盘、`rafthttp` 网络传输（`stream.go` 长连接流传日志追加，`pipeline.go` 传大消息，`snapshot_sender.go` 传快照）、以及 apply。
- **存储层**分两级：WAL 保存“尚未/刚刚达成共识的日志”，bbolt（B+ 树）保存“已应用的状态”。两者之间用 consistent index 保证崩溃后不重复应用。

## 2.3 共识层关键代码（`raft/raft.go`）

### 2.3.1 选举：`campaign()`、`Step()` 中的投票规则

Follower 在 `tickElection()`（raft.go:850）里累加 `electionElapsed`，超过随机化的选举超时后给自己投递一条 `MsgHup`，进入 `hup()` → `campaign()`（raft.go:1025）。

```go
// raft.go:1025  campaign()（节选）
if t == campaignPreElection {
    r.becomePreCandidate()
    voteMsg = pb.MsgPreVote
    term = r.Term + 1          // PreVote 不真正自增 term
} else {
    r.becomeCandidate()
    voteMsg = pb.MsgVote
    term = r.Term
}
...
for _, id := range ids {
    if id == r.id {
        // 给自己的那一票也要等 vote 落盘后才算数
        r.send(&pb.Message{To: new(id), Term: new(term), Type: voteRespMsgType(voteMsg).Enum()})
        continue
    }
    last := r.raftLog.lastEntryID()
    r.send(&pb.Message{To: new(id), Term: new(term), Type: voteMsg.Enum(),
                       Index: new(last.index), LogTerm: new(last.term), Context: ctx})
}
```

投票方的判定在 `Step()`（raft.go:1089 起）里：

```go
// raft.go 约 1180 行
canVote := r.Vote == m.GetFrom() ||                       // 重复投给同一个人
    (r.Vote == None && r.lead == None) ||                 // 本 term 还没投过、也不知道有 Leader
    (m.GetType() == pb.MsgPreVote && m.GetTerm() > r.Term)
if canVote && r.raftLog.isUpToDate(candLastID) { ...grant... }

// log.go:442
func (l *raftLog) isUpToDate(their entryID) bool {
    our := l.lastEntryID()
    return their.term > our.term || their.term == our.term && their.index >= our.index
}
```

这里有三处值得与 Paxos 对照的设计：

1. **“日志最新者才能当选”**（`isUpToDate`）。Raft 把“新 Leader 必须拥有全部已提交条目”的责任放在选举上，因此 Leader 当选后**不需要从别人那里拉取日志**。Paxos（包括本作业自己的实现）则相反：任何节点都能当 Leader，但 Phase 1 的 Promise 必须把各 Acceptor 已接受的值带回来，由新 Leader 重新提案补齐。前者选举受限但恢复简单，后者选举自由但 Phase 1 报文可能很大。
2. **PreVote**（`embed/config.go:544` 默认开启）。被分区隔离的节点会不断超时、不断自增 term，网络恢复后会用高 term 打断正常 Leader。PreVote 先用 `term+1` 试探，拿不到多数派就不真正自增 term。
3. **Leader 租约保护**（raft.go 约 1100 行）：`inLease := r.checkQuorum && r.lead != None && r.electionElapsed < r.electionTimeout`，在最近一个选举超时内听到过 Leader 心跳的节点会**直接忽略**别人的拉票请求。etcd 固定开启 `CheckQuorum`（`etcdserver/bootstrap.go:572`）。

这三处对应的问题，在第 5 章自己的 Paxos 实验中都能直接观察到（例如高丢包下的“误选举”导致 Leader 抖动）。

Leader 当选后在 `becomeLeader()`（raft.go:933）里立刻追加一条**空条目**（raft.go:961-962 `emptyEnt`）。原因是 Raft 的 Leader 不能直接提交前任 term 的条目，只能通过提交一条本 term 的条目间接提交它们（`log.go:455 maybeCommit` 中的 `l.matchTerm(at)` 检查）。这与 Paxos 新 Leader 用更高编号重新提案旧值是同一个安全性问题的两种解法。

### 2.3.2 日志复制与提交：`stepLeader()`、`handleAppendEntries()`、`CommittedIndex()`

- **提案**：`stepLeader` 的 `case pb.MsgProp`（raft.go:1294）调用 `appendEntry()`（1349）把条目追加到本地未持久化日志，然后 `bcastAppend()`（1352）向所有 Follower 发 `MsgApp`。Follower 收到客户端提案则转发给 Leader（`stepFollower` 的 `MsgProp`，raft.go:1720）。
- **Follower 接收**：`handleAppendEntries()`（raft.go:1791）调用 `raftLog.maybeAppend`，按 `(prevIndex, prevTerm)` 做一致性检查；不匹配时返回 `Reject` 并带上 `RejectHint`/`LogTerm`，让 Leader 一次跳过整个冲突 term（`findConflictByTerm`），而不是逐条回退。
- **提交**：Leader 收到 `MsgAppResp`（raft.go:1384）后更新该 Follower 的 `Match`，调用 `maybeCommit()`（raft.go:775）。提交点的计算在 `quorum/majority.go:120`：

```go
// quorum/majority.go:120  CommittedIndex()（节选）
// 把每个投票者已确认的 index 排序，取从大到小第 n/2+1 个，
// 即“被多数派确认的最大 index”。
slices.Sort(srt)
pos := n - (n/2 + 1)
return Index(srt[pos])
```

这一行就是“多数派确认”的全部实现。与 Paxos 的差别在于：Paxos 的每个实例独立计票（本作业 `node.py` 的 `inflight[slot].acks`），而 Raft 因为日志连续，只需对每个 Follower 记一个 `Match` 水位，一次排序就得到整段日志的提交点——这是 Raft 流水线和批处理天然高效的原因。

### 2.3.3 心跳与 CheckQuorum：`tickHeartbeat()`（raft.go:862）

Leader 每个心跳周期发 `MsgBeat`；每过一个选举超时触发一次 `MsgCheckQuorum`（raft.go:1281），如果 `!r.trk.QuorumActive()`（最近一个周期内没有联系上多数派）就**主动退位**。这避免了被分区到少数派一侧的旧 Leader 长期自认为是 Leader。etcd 默认 `TickMs=100`、`ElectionMs=1000`（`embed/config.go`），即心跳 100ms、选举超时 1s（随机化到 1–2s）。

### 2.3.4 线性一致读：ReadIndex

读请求不写日志。`etcdserver/read/read.go` 的读循环通过 `raft.ReadIndex()`（read.go:236）向 Leader 要一个“当前提交点”，Leader 用一轮心跳确认自己仍是 Leader（`ReadOnlySafe`，raft.go:63），然后读线程等待 `appliedIndex >= confirmedIndex`（read.go 约 129 行 `ApplyWait(confirmedIndex)`）再读本地状态。另有 `ReadOnlyLeaseBased`（raft.go:69）直接信任租约、省掉这轮心跳，但依赖时钟漂移有界，且必须开启 CheckQuorum（raft.go:336）。Chubby 的读同样依赖 master 租约。

## 2.4 服务端关键代码（`etcd/server/etcdserver/`）

### 2.4.1 Ready 循环与持久化顺序：`raftNode.start()`（raft.go:174）

这是 etcd 中最关键的一段代码，决定了“什么必须先落盘、什么可以并行”。

```go
// etcdserver/raft.go:185 起（节选、简化）
case rd := <-r.Ready():
    ...
    r.applyc <- ap                                   // ① 已提交条目交给 apply 协程
    if islead {
        r.transport.Send(r.processMessages(rd.Messages))   // ② Leader：先发消息（与自己落盘并行）
    }
    r.storage.Save(rd.HardState, rd.Entries)         // ③ 写 WAL（必要时 fsync）
    r.raftStorage.Append(rd.Entries)                 // ④ 追加到内存日志
    if !islead {
        msgs := r.processMessages(rd.Messages)
        ...
        r.transport.Send(msgs)                       // ⑤ Follower：落盘之后才能回 MsgAppResp/投票
    }
    r.Advance()
```

- Follower 必须**先落盘再回复**（⑤ 在 ③ 之后），因为它的回复就是对 Leader 的承诺——与 Paxos Acceptor“先持久化 promised_id / accepted_id / accepted_value 再回复”完全同构。本作业 `node.py` 的 `on_prepare`/`on_accept` 把回复放进 `wal.append(..., callback)` 的回调里，就是这一规则。
- Leader 则可以**先发后写**（②在③之前，raft.go:236-242 的注释引用了 Raft 论文 10.2.1），因为 Leader 自己的那一票只有在本地落盘完成后才计入多数派，发送 `MsgApp` 本身不构成承诺。这把 Leader 的磁盘延迟从关键路径上的“串行”变成了“并行”。

何时必须 fsync 由 `raft.MustSync()` 决定（rawnode.go:191）：

```go
return entsnum != 0 || st.GetVote() != prevst.GetVote() || st.GetTerm() != prevst.GetTerm()
```

即：**有新日志条目、投票对象变了、term 变了**——恰好对应 Paxos 的 accepted_value、promised_id。仅 commit 水位变化不需要 fsync（丢了可以重新算出来）。`wal.Save()`（storage/wal/wal.go:1000）据此决定是否调用 `w.sync()`。

### 2.4.2 一次写请求的完整路径

以 `Put` 为例（v3_server.go:295）：

1. `Put()` → `raftRequest()`（v3_server.go:1013）→ `processInternalRaftRequestOnce()`。
2. 背压：若 `committedIndex - appliedIndex` 超过阈值，直接返回 `ErrTooManyRequests`（v3_server.go:1064）。这是防止 apply 落后时内存无限增长的保护。
3. 用 `reqIDGen.Next()` 生成请求 ID（1068），序列化（1094），`s.w.Register(id)` 注册一个等待通道（1107），然后 `s.r.Propose(cctx, data)`（1114）把数据交给 Raft。
4. 条目经 2.3.2 的流程被多数派持久化并提交，出现在某次 `Ready.CommittedEntries` 中，经 `applyc` 送到 `EtcdServer.run()`（server.go:751）→ `applyAll()`（978）→ `applyEntries()`（1180）→ `apply()`（1887）→ `applyEntryNormal()`（1940）。
5. `applyEntryNormal` 调 `apply.Apply(..., s.uberApply, ...)` 执行事务，随后 `s.w.Trigger(id, ar)` 唤醒第 3 步挂起的 gRPC 处理协程，返回客户端。
6. 应用前先 `consistIndex.SetConsistentApplyingIndex(index, term)`（server.go:1908），该 index 与数据在同一个 bbolt 事务里提交，崩溃重启后据此跳过已应用的条目，实现幂等。

本作业的实现与之对应：`client_pending[命令ID]` ↔ `wait.Wait`；`cmd_slot` 去重 ↔ consistent index；`learn()` 里按 `prefix` 顺序 `_apply()` ↔ `applyEntries`。

### 2.4.3 MVCC 存储与 Watch（`storage/mvcc/`）

- 每次写事务产生一个全局单调递增的 `Revision{Main, Sub}`（revision.go:35）。数据以 revision 为键存入 bbolt；内存里的 B 树索引 `keyIndex`（key_index.go:73）记录每个用户键的各代（generation）revision 列表，支持“读取某个历史版本”和“从某个 revision 开始 watch”。
- bbolt 的写入是**批量提交**的：`backend.go:36-37` 的 `defaultBatchLimit = 10000`、`defaultBatchInterval = 100ms`。也就是说 etcd 对状态机存储并不逐条 fsync——持久性由 WAL 保证，bbolt 只是 WAL 的物化视图。这和本作业“WAL 组提交”是同一种思路：把多次落盘合并成一次 fsync。
- `watchableStore`（watchable_store.go:56）把 watcher 分成三组：`synced`（已追上最新 revision，写入时由 `notify()` 直接推送，watchable_store.go:470）、`unsynced`（从历史 revision 开始，由后台 `syncWatchersLoop()` 追赶，224 行）、`victims`（接收端通道满了被阻塞的批次，由 `syncVictimsLoop()` 重试，259 行）。慢消费者不会阻塞写路径。

Chubby 的事件通知是“尽力而为 + 客户端缓存失效”，etcd 的 Watch 则基于 revision 保证**不丢、不重、有序**，这是 MVCC 带来的能力。

### 2.4.4 租约与分布式锁——Chubby 的核心功能

**租约（`lease/lessor.go`）**：`lessor` 维护 `leaseMap`、到期通知堆 `leaseExpiredNotifier` 和 `itemMap`（键 → 租约）。关键设计是**只有 Leader 上的 lessor 是 primary**（`isPrimary()`，lessor.go:263；`demotec` 非空表示 primary）：

- 到期检测只在 Leader 上运行（`runLoop()` → `revokeExpiredLeases()`，lessor.go:620/640），到期的租约通过 `expiredC` 交给 `EtcdServer.run()`（server.go:842），后者发起一条 **LeaseRevoke 提案**走 Raft。也就是说“租约过期”这个事实本身也是经过共识的日志条目，所有副本在同一个日志位置删除附着在租约上的键。
- Leader 切换时，新 Leader 调 `Promote(extend)`（lessor.go:480）把所有租约的到期时间**延长一个选举超时**，旧 Leader 调 `Demote()`（534）。延长的原因是新 Leader 不知道旧 Leader 最后一次续约发生在什么时候，只能保守处理——这与 Chubby 论文中 master 故障切换时客户端进入 grace period、新 master 重建会话的做法是同一思想。
- `Renew`（续约，lessor.go:396）不走 Raft，只在 Leader 内存中更新，因此续约很便宜；代价是需要 `Checkpoint`（367）机制定期把剩余 TTL 写入日志，避免 Leader 切换后长租约被“重置”。

**会话（`client/v3/concurrency/session.go`）**：`NewSession` 先 `client.Grant()` 申请一个租约（默认 `defaultSessionTTL = 60` 秒），再 `client.KeepAlive()` 后台续约。客户端崩溃 → 停止续约 → 租约到期 → 其持有的所有键被删除。

**锁（`client/v3/concurrency/mutex.go`）**：

```go
// mutex.go:111  tryAcquire()
m.myKey = fmt.Sprintf("%s%x", m.pfx, s.Lease())              // 锁前缀 + 自己的租约ID
cmp := v3.Compare(v3.CreateRevision(m.myKey), "=", 0)        // 我的键还不存在？
put := v3.OpPut(m.myKey, "", v3.WithLease(s.Lease()))        // 则创建，并绑定到我的租约
getOwner := v3.OpGet(m.pfx, v3.WithFirstCreate()...)         // 同时取该前缀下 CreateRevision 最小的键
resp, err := client.Txn(ctx).If(cmp).Then(put, getOwner).Else(get, getOwner).Commit()
m.myRev = resp.Header.Revision
```

```go
// mutex.go:75  Lock()
if len(ownerKey) == 0 || ownerKey[0].CreateRevision == m.myRev { return nil }  // 我就是最早的 → 拿到锁
werr := waitDeletes(ctx, client, m.pfx, m.myRev-1)   // 否则只等“排在我前面的那些键”被删除
```

整个锁建立在三个原语之上：**事务（比较并交换）+ 全局有序的 CreateRevision + 绑定租约的键**。排队顺序由共识日志决定（公平），持有者崩溃由租约到期自动释放，等待者只 watch 自己的前驱（避免惊群）。`Mutex.IsOwner()`（mutex.go:152）返回的比较条件可以附加到后续事务里，起到 Chubby 论文中 **sequencer（锁序号）** 的作用：即使锁因租约过期被别人拿走，旧持有者的延迟写也会被拒绝。

## 2.5 etcd 与 Chubby、与本作业 Paxos 实现的对照

| 维度 | Chubby（论文） | etcd（源码） | 本作业的 Paxos 原型 |
|---|---|---|---|
| 共识协议 | Multi-Paxos | Raft（`go.etcd.io/raft`） | Basic / Multi / Fast Paxos 三种可切换 |
| Leader 产生 | master 租约 + Paxos 选主 | 随机超时选举，日志最新者当选，PreVote + CheckQuorum | 心跳超时后做范围 Prepare，任何节点可当选 |
| 新 Leader 如何补齐日志 | Phase 1 收集已接受值 | 不需要（选举已保证），提交空条目 | Promise 带回已接受值，按编号最高者重提，空洞填 NOOP |
| 持久化规则 | 先写日志再回复 | `MustSync`：条目/term/vote 变化必须 fsync；Follower 先落盘后回复，Leader 先发后写 | Acceptor 先写 WAL（promised/accepted）再回复；组提交 |
| 提交判定 | 多数派 Accepted | `CommittedIndex`：Match 水位排序取第 n/2+1 个 | 每实例独立计 Accepted，≥ n/2+1 即 Learn |
| 读 | master 本地读（靠租约） | ReadIndex（一轮心跳）或 LeaseBased | 未实现线性读（只读已应用状态） |
| 会话/租约 | Session + KeepAlive，故障切换有 grace period | Lease，Leader 为 primary，切换时 `Promote(extend)` | 仅 Leader 心跳（选主用），无客户端租约 |
| 锁 | Acquire/Release + sequencer | 事务 + CreateRevision + Lease；`IsOwner()` 作 fencing | 无（状态机是演示用 KV） |
| 事件 | 尽力通知 + 缓存失效 | 基于 revision 的 Watch，三组 watcher | Listener 角色被动学习已选定的值 |
| 成员变更 | 替换故障副本 | Joint Consensus（`confchange/`）、Learner | 静态成员 |
| 日志压缩 | 快照 | Snapshot + WAL 释放 + MVCC compaction | 未实现（日志只增不减） |

从这张表可以看到自己实现的原型与工业系统之间的差距主要在三处：**成员变更、日志压缩/快照、线性一致读与客户端会话**。这些都不属于共识算法本身，却占了 etcd 服务端代码的大部分（25,771 行 vs. Raft 库 11,626 行），也正是 *Paxos Made Live* 一文强调的“从算法到系统”的工程鸿沟。

反过来，通读 etcd 源码也直接影响了本作业的几个实现决策：

1. **先落盘再回复**的回调结构（2.4.1）；
2. **组提交**：一次 fsync 覆盖事件循环一轮内积累的所有记录（对应 etcd 把一个 Ready 里的多条 Entry 一次 `Save`）；
3. **Leader 遇到更高编号立即退位**、过期 Leader 的心跳会被回以 NACK（对应 Raft 的 term 比较）；
4. **追赶机制**：落后的节点根据心跳携带的已决前缀主动请求补齐（对应 Raft 的 `MsgApp` 拒绝 + `RejectHint` 回退，只是方向相反）；
5. 实验中观察到的“高丢包 → 心跳连续丢失 → 误选举 → 吞吐抖动”，恰好解释了 etcd 为什么要有 PreVote 和 CheckQuorum 的租约保护，而本原型没有这两项机制。
