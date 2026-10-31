"""Paxos 节点：同一进程内同时承担 Proposer / Acceptor / Learner；listener 只学习不投票。

三种提案机制共用同一套"多实例日志 + Acceptor + Learner"，只是 Proposer 侧流程不同：

  basic  每条命令对一个实例完整执行 Prepare/Promise + Accept/Accepted，任何节点都可提案
  multi  心跳选出稳定 Leader，Leader 用一次范围 Prepare 覆盖之后所有实例，之后只做 Phase 2
  fast   协调者（Leader）做范围 Prepare 后下发 Any，Proposer 直接把值投给 Acceptor，
         快速多数派（fast quorum）接受同一值即选定；冲突时由协调者用经典轮恢复

未调用任何现成的 Paxos/Raft 库。
"""
import argparse
import asyncio
import collections
import json
import os
import random
import sys
import time
import zlib

from . import wire
from .net import Transport, tune_event_loop
from .storage import WAL, REC_ACCEPT, REC_PROMISE_GLOBAL, REC_PROMISE_SLOT
from .wire import (ACCEPT, ACCEPTED, CLIENT_REQ, CLIENT_RESP, CTRL, CTRL_RESP, DECIDED_MARK,
                   FAST_PROPOSE, FORWARD, F_ERR, F_FAST, F_PHASE2, F_RANGE, HEARTBEAT, LEARN,
                   LEARN_REQ, NACK, NOOP, PING, PONG, PREPARE, PROMISE, Msg, ballot_node,
                   ballot_round, is_fast, mk_ballot, pack)

now = time.perf_counter


class Collector:
    """收集某一轮的应答，凑够多数派即成功；被拒到不可能凑够时失败。"""

    def __init__(self, need, n, loop):
        self.need = need
        self.n = n
        self.got = {}
        self.nacks = set()
        self.fut = loop.create_future()

    def ack(self, src, data=None):
        self.got[src] = data
        if len(self.got) >= self.need and not self.fut.done():
            self.fut.set_result(True)

    def reject(self, src):
        self.nacks.add(src)
        if len(self.nacks) > self.n - self.need and not self.fut.done():
            self.fut.set_result(False)

    def abort(self):
        if not self.fut.done():
            self.fut.set_result(False)


class FastSlot:
    """协调者对一个快速轮实例的观察：各 Acceptor 在该轮接受了什么值。"""
    __slots__ = ("reports", "t0", "t_probe", "recovering")

    def __init__(self, t):
        self.reports = {}
        self.t0 = t
        self.t_probe = t
        self.recovering = False


class Node:
    def __init__(self, cfg, loop):
        self.cfg = cfg
        self.loop = loop
        self.id = cfg.id
        self.mode = cfg.mode
        self.voters = sorted(cfg.peers)
        self.listeners = sorted(cfg.listeners)
        self.is_voter = self.id in cfg.peers
        self.n = len(self.voters)
        self.qc = self.n // 2 + 1                       # 经典多数派
        self.qf = (2 * self.n - self.qc) // 2 + 1       # 快速多数派：满足 qc + 2*qf > 2n 的最小值
        self.others = [p for p in self.voters + self.listeners if p != self.id]

        addrs = dict(cfg.peers)
        addrs.update(cfg.listeners)
        self.tp = Transport(self.id, addrs, self.handle, loop)
        self.tp.set_faults(delay_ms=cfg.delay_ms, loss=cfg.loss)
        self.wal = WAL(os.path.join(cfg.data_dir, "acceptor-%d.wal" % self.id),
                       fsync=cfg.fsync, loop=loop)

        # ---- Acceptor（需持久化）----
        self.pg = 0                     # promised_id（范围承诺，对所有实例生效）
        self.max_slot_promised = 0
        self.acc = {}                   # slot -> [promised_id, accepted_id, accepted_value]
        self.any_ballot = 0             # 当前允许直投的快速轮（易失，靠心跳重建）
        self.any_from = 0
        self.touch = {}                 # slot -> 最近一次他人在该实例上活动的时间

        # ---- Learner ----
        self.decided = {}               # slot -> value
        self.prefix = 0                 # 第一个未决实例（之前的全部已决并已按序应用）
        self.max_seen = -1
        self.cmd_slot = {}              # 命令ID -> 被选定的实例（去重）
        self.kv = {}
        self.last_progress = now()

        # ---- Proposer ----
        self.round = 0
        self.boot = int(time.time()) & 0xFFFFFFFF
        self.local_seq = 0
        self.waiters = {}               # (kind, slot, ballot) -> Collector
        self.active_col = {}            # slot -> 我在该实例上正等待的 Collector
        self.slot_events = {}           # slot -> Event
        self.my_active = set()          # 我正在提案的实例
        self.client_pending = {}        # 命令ID -> (conn, 客户端序号)
        self.fwd_pending = {}           # multi：命令ID -> [cmd, 上次发送时间]
        self.fast_mine = {}             # fast：slot -> [ballot, value, {acceptor: value}]
        self.any_event = asyncio.Event()

        # ---- Leader / 协调者 ----
        self.leader = None
        self.leader_ballot = 0
        self.is_leader = False
        self.my_ballot = 0
        self.next_slot = 0
        self.inflight = {}              # slot -> [value, ballot, acks, 发送时间]
        self.batch_q = collections.deque()
        self.batch_t = 0.0
        self.queued_ids = set()
        self.linger_armed = False
        self.fast_slots = {}            # slot -> FastSlot
        self.any_from_mine = 0
        self.last_hb = now()
        self.el_deadline = self._rand_deadline()
        self.campaigning = False

        self.stats = collections.Counter()
        self.pings = {}
        self.handlers = {
            PREPARE: self.on_prepare, PROMISE: self.on_promise, NACK: self.on_nack,
            ACCEPT: self.on_accept, ACCEPTED: self.on_accepted, LEARN: self.on_learn,
            HEARTBEAT: self.on_heartbeat, FAST_PROPOSE: self.on_fast_propose,
            FORWARD: self.on_forward, LEARN_REQ: self.on_learn_req,
            CLIENT_REQ: self.on_client_req, CTRL: self.on_ctrl,
            PING: self.on_ping, PONG: self.on_pong,
        }

    # ------------------------------------------------------------------ 启动/恢复
    async def start(self):
        recs = self.wal.replay()
        for rtype, slot, ballot, value in recs:
            if rtype == REC_PROMISE_GLOBAL:
                self.pg = max(self.pg, ballot)
            elif rtype == REC_PROMISE_SLOT:
                st = self._st(slot)
                st[0] = max(st[0], ballot)
            elif rtype == REC_ACCEPT:
                st = self._st(slot)
                st[0] = max(st[0], ballot)
                if ballot >= st[1]:
                    st[1] = ballot
                    st[2] = value
            self._see_ballot(ballot)
            if rtype != REC_PROMISE_GLOBAL and ballot > self.max_slot_promised:
                self.max_slot_promised = ballot
        if self.acc:
            self.max_seen = max(self.acc)
        self.stats["wal_recovered"] = len(recs)
        await self.tp.start(self.cfg.bind)
        self.loop.create_task(self._tick_loop())
        log("node %d up mode=%s voters=%s qc=%d qf=%d fsync=%s recovered=%d promised=%s"
            % (self.id, self.mode, self.voters, self.qc, self.qf, self.cfg.fsync, len(recs),
               wire.ballot_str(self.pg)))

    def _rand_deadline(self):
        return self.cfg.election_ms / 1000.0 * (1.0 + random.random())

    def _st(self, slot):
        st = self.acc.get(slot)
        if st is None:
            st = self.acc[slot] = [0, 0, b""]
        return st

    def _see_ballot(self, b):
        if b and b != DECIDED_MARK:
            r = ballot_round(b)
            if r > self.round:
                self.round = r

    def _see_slot(self, slot):
        if slot > self.max_seen:
            self.max_seen = slot

    # ------------------------------------------------------------------ 收发
    def handle(self, m, conn=None):
        self.stats["rx"] += 1
        h = self.handlers.get(m.type)
        if h is not None:
            h(m, conn)

    def send(self, peer, mtype, slot=0, ballot=0, aux=0, payload=b"", flags=0):
        if peer == self.id:
            # 本机角色之间直接投递，不走网络
            self.loop.call_soon(self.handle, Msg(mtype, self.id, flags, slot, ballot, aux, payload))
            return
        self.stats["tx"] += 1
        self.stats["tx_" + wire.TYPE_NAMES[mtype]] += 1
        self.stats["tx_bytes"] += wire.HEADER_SIZE + len(payload)
        self.tp.send(peer, pack(mtype, self.id, slot, ballot, aux, payload, flags))

    def bcast(self, peers, mtype, **kw):
        for p in peers:
            self.send(p, mtype, **kw)

    # ================================================================== Acceptor
    def on_prepare(self, m, conn):
        if not self.is_voter:
            return
        b = m.ballot
        self._see_ballot(b)
        src = m.src
        if m.flags & F_RANGE:
            cur = max(self.pg, self.max_slot_promised)
            if b > cur or b == self.pg:
                frm = m.slot

                def reply():
                    ents = []
                    for s, st in self.acc.items():
                        if s >= frm and st[1] and s not in self.decided:
                            ents.append((s, st[1], st[2]))
                    for s, v in self.decided.items():
                        if s >= frm:
                            ents.append((s, DECIDED_MARK, v))
                    self.send(src, PROMISE, slot=frm, ballot=b, flags=F_RANGE,
                              payload=wire.enc_entries(ents))
                if b > self.pg:
                    self.pg = b
                    self.wal.append(REC_PROMISE_GLOBAL, 0, b, b"", reply)   # 先落盘 promised_id 再回复
                else:
                    self.wal.barrier(reply)
            else:
                self.send(src, NACK, slot=m.slot, ballot=b, aux=cur, flags=F_RANGE)
            return
        s = m.slot
        self._see_slot(s)
        if s in self.decided:
            self.send(src, LEARN, slot=s, payload=self.decided[s])
            return
        st = self._st(s)
        eff = max(self.pg, st[0])
        if b > eff or b == st[0]:
            if src != self.id:
                self.touch[s] = now()

            def reply():
                ents = [(s, st[1], st[2])] if st[1] else []
                self.send(src, PROMISE, slot=s, ballot=b, payload=wire.enc_entries(ents))
            if b > st[0]:
                st[0] = b
                if b > self.max_slot_promised:
                    self.max_slot_promised = b
                self.wal.append(REC_PROMISE_SLOT, s, b, b"", reply)
            else:
                self.wal.barrier(reply)
        else:
            self.send(src, NACK, slot=s, ballot=b, aux=eff)

    def on_accept(self, m, conn):
        if not self.is_voter:
            return
        s, b, src = m.slot, m.ballot, m.src
        self._see_ballot(b)
        self._see_slot(s)
        if s in self.decided:
            self.send(src, LEARN, slot=s, payload=self.decided[s])
            return
        if is_fast(b):
            return                      # 快速轮里协调者不会下发具体值（见 _campaign）
        st = self._st(s)
        eff = max(self.pg, st[0])
        if b >= eff:
            if src != self.id:
                self.touch[s] = now()

            def reply():
                self.send(src, ACCEPTED, slot=s, ballot=b)
            if st[1] == b:
                self.wal.barrier(reply)             # 重传：幂等
            else:
                st[0] = b
                st[1] = b
                st[2] = m.payload
                if b > self.max_slot_promised:
                    self.max_slot_promised = b
                self.wal.append(REC_ACCEPT, s, b, m.payload, reply)  # 先落盘再回复
        else:
            self.send(src, NACK, slot=s, ballot=b, aux=eff, flags=F_PHASE2)

    def on_fast_propose(self, m, conn):
        """Fast Paxos：Proposer 绕过协调者直接投值。每个快速轮每个实例只接受第一个到达的值。"""
        if not self.is_voter:
            return
        s, b, src = m.slot, m.ballot, m.src
        self._see_slot(s)
        if s in self.decided:
            self.send(src, LEARN, slot=s, payload=self.decided[s])
            return
        if b != self.any_ballot or s < self.any_from:
            self.send(src, NACK, slot=s, ballot=b, aux=max(self.any_ballot, self.pg), flags=F_FAST)
            return
        st = self._st(s)
        eff = max(self.pg, st[0])
        coord = ballot_node(b)
        if eff <= b and st[1] < b:
            st[0] = b
            st[1] = b
            st[2] = m.payload
            if b > self.max_slot_promised:
                self.max_slot_promised = b
            value = m.payload

            def reply():
                self.send(src, ACCEPTED, slot=s, ballot=b, payload=value, flags=F_FAST)
                if coord != src:
                    self.send(coord, ACCEPTED, slot=s, ballot=b, payload=value, flags=F_FAST)
            self.wal.append(REC_ACCEPT, s, b, value, reply)
        elif st[1] == b:
            value = st[2]                # 本轮已接受过（可能是别人的值）：如实报告

            def reply():
                self.send(src, ACCEPTED, slot=s, ballot=b, payload=value, flags=F_FAST)
                if coord != src:
                    self.send(coord, ACCEPTED, slot=s, ballot=b, payload=value, flags=F_FAST)
            self.wal.barrier(reply)
        else:
            self.send(src, NACK, slot=s, ballot=b, aux=eff, flags=F_FAST)

    # ================================================================== Learner
    def learn(self, slot, value, announce=False):
        old = self.decided.get(slot)
        if old is not None:
            if old != value:
                self.stats["safety_violations"] += 1
                log("!!! SAFETY VIOLATION node=%d slot=%d" % (self.id, slot))
            return False
        self.decided[slot] = value
        self.last_progress = now()
        if slot > self.max_seen:
            self.max_seen = slot
        while self.prefix in self.decided:
            self._apply(self.decided[self.prefix])
            self.prefix += 1
        self.stats["decided"] += 1
        for origin, seq, _data in wire.dec_batch(value):
            cid = (origin, seq)
            if cid in self.cmd_slot:
                self.stats["dup_commits"] += 1
                continue
            self.cmd_slot[cid] = slot
            self.queued_ids.discard(cid)
            if origin == self.id:
                self.fwd_pending.pop(cid, None)
                p = self.client_pending.pop(cid, None)
                if p is not None:
                    p[0].send(pack(CLIENT_RESP, self.id, slot=slot, aux=p[1]))
        ev = self.slot_events.pop(slot, None)
        if ev is not None:
            ev.set()
        col = self.active_col.get(slot)
        if col is not None:
            col.abort()
        self.touch.pop(slot, None)
        self.fast_slots.pop(slot, None)
        if self.inflight.pop(slot, None) is not None and self.is_leader:
            self._maybe_flush()
        if announce:
            self.bcast(self.others, LEARN, slot=slot, payload=value)
        return True

    def _apply(self, value):
        """按实例顺序应用到状态机（演示用 KV：SET key value）。"""
        for _o, _s, data in wire.dec_batch(value):
            if data[:4] == b"SET ":
                parts = data.split(b" ", 2)
                if len(parts) == 3:
                    self.kv[parts[1]] = parts[2]
        self.stats["applied"] += 1

    def on_learn(self, m, conn):
        self.learn(m.slot, m.payload)

    def on_learn_req(self, m, conn):
        s = m.slot
        cnt = 0
        while cnt < 1000:
            v = self.decided.get(s)
            if v is None:
                break
            self.send(m.src, LEARN, slot=s, payload=v)
            s += 1
            cnt += 1

    # ================================================================== 应答路由
    def on_promise(self, m, conn):
        if m.flags & F_RANGE:
            w = self.waiters.get(("R", 0, m.ballot))
        else:
            w = self.waiters.get(("P", m.slot, m.ballot))
        if w is not None:
            w.ack(m.src, wire.dec_entries(m.payload))

    def on_accepted(self, m, conn):
        if m.flags & F_FAST:
            self._on_fast_accepted(m)
            return
        w = self.waiters.get(("A", m.slot, m.ballot))
        if w is not None:
            w.ack(m.src)
            return
        inf = self.inflight.get(m.slot)
        if inf is not None and inf[1] == m.ballot:
            inf[2].add(m.src)
            if len(inf[2]) >= self.qc:               # 多数派确认
                self.learn(m.slot, inf[0], announce=True)

    def on_nack(self, m, conn):
        self._see_ballot(m.aux)
        self.stats["nack_rx"] += 1
        if m.flags & F_RANGE:
            w = self.waiters.get(("R", 0, m.ballot))
        elif m.flags & F_PHASE2:
            w = self.waiters.get(("A", m.slot, m.ballot))
        else:
            w = self.waiters.get(("P", m.slot, m.ballot))
        if w is not None:
            w.reject(m.src)
        if self.is_leader and (m.ballot | 1) == (self.my_ballot | 1) and m.aux > m.ballot \
                and not (m.flags & F_FAST):
            self._step_down("nack %s" % wire.ballot_str(m.aux))

    # ================================================================== 客户端入口
    def on_client_req(self, m, conn):
        if not self.is_voter:
            conn.send(pack(CLIENT_RESP, self.id, aux=m.aux, flags=F_ERR))
            return
        self.local_seq += 1
        seq = (self.boot << 32) | self.local_seq
        cid = (self.id, seq)
        self.client_pending[cid] = (conn, m.aux)
        cmd = (self.id, seq, m.payload)
        self.stats["client_reqs"] += 1
        if self.mode == "basic":
            self.loop.create_task(self._basic_propose(cmd))
        elif self.mode == "fast":
            self.loop.create_task(self._fast_propose(cmd))
        else:
            self.fwd_pending[cid] = [cmd, now()]
            self._multi_submit(cmd)

    # ================================================================== Basic Paxos
    def _alloc_slot(self, lo=0):
        """选一个实例来提案：最低的、未决的、自己没在用的；默认还会避开别人刚动过的实例。"""
        s = max(self.prefix, lo)
        t = now()
        hole = self.cfg.retry_ms / 1000.0 * 4
        contend = self.cfg.contend
        while True:
            if s in self.decided or s in self.my_active:
                s += 1
                continue
            if not contend:
                tt = self.touch.get(s)
                if tt is not None and t - tt < hole:
                    s += 1
                    continue
            break
        self.my_active.add(s)
        self._see_slot(s)
        return s

    async def _round(self, kind, slot, ballot, mtype, payload=b""):
        if slot in self.decided:
            return None
        col = Collector(self.qc, self.n, self.loop)
        key = (kind, slot, ballot)
        self.waiters[key] = col
        self.active_col[slot] = col
        self.bcast(self.voters, mtype, slot=slot, ballot=ballot, payload=payload)
        try:
            ok = await asyncio.wait_for(col.fut, self.cfg.retry_ms / 1000.0)   # 超时重试
        except asyncio.TimeoutError:
            ok = False
            self.stats["timeouts"] += 1
        self.waiters.pop(key, None)
        if self.active_col.get(slot) is col:
            del self.active_col[slot]
        return col if ok else None

    async def _backoff(self, attempt):
        base = self.cfg.backoff_ms
        if base <= 0:
            await asyncio.sleep(0)
        else:
            await asyncio.sleep(random.uniform(0, base * (1 << min(attempt, 6))) / 1000.0)

    async def _basic_propose(self, cmd):
        cid = (cmd[0], cmd[1])
        value = wire.enc_batch([cmd])
        attempt = 0
        while cid not in self.cmd_slot:
            slot = self._alloc_slot()
            while slot not in self.decided:
                self.round += 1                                   # 提案编号单调递增
                b = mk_ballot(self.round, self.id)
                self.stats["basic_rounds"] += 1
                col = await self._round("P", slot, b, PREPARE)    # Phase 1
                if col is None:
                    if slot in self.decided:
                        break
                    self.stats["basic_conflicts"] += 1
                    attempt += 1
                    await self._backoff(attempt)
                    continue
                v, hb = value, 0
                for ents in col.got.values():
                    for _s, ab, av in ents:
                        if ab > hb:                               # 必须沿用已接受的最高编号的值
                            hb, v = ab, av
                col = await self._round("A", slot, b, ACCEPT, v)  # Phase 2
                if col is None:
                    if slot in self.decided:
                        break
                    self.stats["basic_conflicts"] += 1
                    attempt += 1
                    await self._backoff(attempt)
                    continue
                self.learn(slot, v, announce=True)
            self.my_active.discard(slot)
            if cid not in self.cmd_slot:
                self.stats["slot_lost"] += 1      # 该实例被别人的值占了，换一个实例重提

    # ================================================================== Multi-Paxos
    def _multi_submit(self, cmd):
        if self.is_leader:
            self._leader_enqueue(cmd)
        elif self.leader is not None:
            self.send(self.leader, FORWARD, payload=wire.enc_batch([cmd]))

    def on_forward(self, m, conn):
        if not self.is_leader:
            return
        for cmd in wire.dec_batch(m.payload):
            self._leader_enqueue(cmd)

    def _leader_enqueue(self, cmd):
        cid = (cmd[0], cmd[1])
        s = self.cmd_slot.get(cid)
        if s is not None:
            if cmd[0] != self.id:
                self.send(cmd[0], LEARN, slot=s, payload=self.decided[s])
            return
        if cid in self.queued_ids:
            return
        self.queued_ids.add(cid)
        if not self.batch_q:
            self.batch_t = now()
        self.batch_q.append(cmd)
        self._maybe_flush()

    def _maybe_flush(self):
        cfg = self.cfg
        q = self.batch_q
        while q and len(self.inflight) < cfg.window:
            if len(q) < cfg.batch and cfg.linger_ms > 0:
                wait = cfg.linger_ms / 1000.0 - (now() - self.batch_t)
                if wait > 0:
                    if not self.linger_armed:
                        self.linger_armed = True
                        self.loop.call_later(wait, self._linger_fire)
                    return
            cmds = [q.popleft() for _ in range(min(cfg.batch, len(q)))]   # 批处理：多条命令共用一个实例
            self.batch_t = now()
            s = self.next_slot
            while s in self.decided or s in self.inflight:
                s += 1
            self.stats["batches"] += 1
            self.stats["batched_cmds"] += len(cmds)
            self._propose_slot(s, wire.enc_batch(cmds), self.my_ballot | 1)

    def _linger_fire(self):
        self.linger_armed = False
        if self.is_leader:
            self._maybe_flush()

    def _propose_slot(self, slot, value, ballot):
        """Leader 的 Phase 2：稳定 Leader 下跳过 Prepare，直接 Accept。"""
        self.inflight[slot] = [value, ballot, set(), now()]
        if slot >= self.next_slot:
            self.next_slot = slot + 1
        self._see_slot(slot)
        self.bcast(self.voters, ACCEPT, slot=slot, ballot=ballot, payload=value)

    # ------------------------------------------------------------------ 选主 / 心跳
    def on_heartbeat(self, m, conn):
        b = m.ballot
        self._see_ballot(b)
        if b < self.leader_ballot:
            self.send(m.src, NACK, ballot=b, aux=self.leader_ballot)     # 让过期 Leader 退位
            return
        if self.is_leader and m.src != self.id and b > self.my_ballot:
            self._step_down("higher leader %d" % m.src)
        if self.is_voter and b < self.pg and m.src != self.id:
            # 我已向更高编号承诺过（某次没成功的选举留下的），这个 Leader 的提案我都会拒绝。
            # 必须告诉它：Multi 模式下它迟早会从 Accept 的 NACK 得知，但 Fast 模式的直投
            # 不经过它，不通知的话受影响的 Proposer 会永远等不到 Any。
            self.send(m.src, NACK, ballot=b, aux=self.pg)
        changed = self.leader != m.src
        self.leader = m.src
        self.leader_ballot = b
        self.last_hb = now()
        if m.slot - 1 > self.max_seen:
            self.max_seen = m.slot - 1
        if m.flags & F_FAST and b >= self.pg and self.is_voter:
            if self.any_ballot != b:
                self.any_ballot = b
                self.any_from = m.aux
                self.any_event.set()
        if changed:
            self.el_deadline = self._rand_deadline()
            if self.mode == "multi" and not self.is_leader:
                t = now()
                for ent in self.fwd_pending.values():
                    ent[1] = t
                    self.send(self.leader, FORWARD, payload=wire.enc_batch([ent[0]]))

    def _send_heartbeat(self):
        fl = F_FAST if self.mode == "fast" else 0
        self.bcast(self.voters + self.listeners, HEARTBEAT, ballot=self.my_ballot,
                   slot=self.prefix, aux=self.any_from_mine if fl else 0, flags=fl)

    def _step_down(self, why):
        if not self.is_leader:
            return
        log("node %d steps down (%s)" % (self.id, why))
        self.is_leader = False
        self.stats["step_downs"] += 1
        self.inflight.clear()
        self.batch_q.clear()
        self.queued_ids.clear()
        self.fast_slots.clear()
        self.last_hb = now()
        self.el_deadline = self._rand_deadline()
        if self.leader == self.id:
            self.leader = None

    def _pick(self, reports, nq):
        """Phase 1 的选值规则。经典轮：取最高编号的值；
        快速轮：若某值 v 仍可能已被某个快速多数派选定（nq - cnt(v) <= n - qf）则必须取 v。"""
        k = max(ab for ab, _ in reports)
        cnt = collections.Counter(v for ab, v in reports if ab == k)
        return _top(cnt)[0]   # 经典轮只会有一个值；快速轮若存在"必选值"，它必然是唯一的最多者

    async def _campaign(self):
        """范围 Prepare：一次 Phase 1 覆盖 >= prefix 的全部实例。"""
        self.campaigning = True
        try:
            self.stats["elections"] += 1
            self.round += 1
            b = mk_ballot(self.round, self.id, classic=(self.mode != "fast"))
            frm = self.prefix
            col = Collector(self.qc, self.n, self.loop)
            key = ("R", 0, b)
            self.waiters[key] = col
            t0 = now()
            self.bcast(self.voters, PREPARE, slot=frm, ballot=b, flags=F_RANGE)
            try:
                ok = await asyncio.wait_for(col.fut, self.cfg.retry_ms / 1000.0)
            except asyncio.TimeoutError:
                ok = False
            self.waiters.pop(key, None)
            self.last_hb = now()
            self.el_deadline = self._rand_deadline()
            if not ok or self.leader_ballot > b:
                return
            reports = collections.defaultdict(list)
            for ents in col.got.values():
                for s, ab, v in ents:
                    if ab == DECIDED_MARK:
                        self.learn(s, v)
                    else:
                        reports[s].append((ab, v))
            nq = len(col.got)
            maxs = max(max(reports, default=frm - 1), self.max_seen)
            self.is_leader = True
            self.my_ballot = b
            self.leader = self.id
            self.leader_ballot = b
            self.next_slot = max(frm, maxs + 1)
            self.any_from_mine = maxs + 1
            cb = b | 1      # 经典轮编号；对快速轮 b 而言是紧邻其后的 (round, node, classic)
            # 快速模式下，< any_from 的实例在快速轮 b 中不会有任何 Acceptor 接受值，
            # 因此对 b 安全的值对 b|1 同样安全，可以直接用 b|1 发 Accept。
            for s in range(frm, maxs + 1):
                if s in self.decided:
                    continue
                rep = reports.get(s)
                self._propose_slot(s, self._pick(rep, nq) if rep else NOOP, cb)
            self.stats["elected"] += 1
            log("node %d becomes leader ballot=%s from=%d recovered_slots=%d phase1=%.1fms"
                % (self.id, wire.ballot_str(b), frm, maxs + 1 - frm, (now() - t0) * 1000))
            self._send_heartbeat()
            if self.mode == "multi":
                for ent in list(self.fwd_pending.values()):
                    self._leader_enqueue(ent[0])
        finally:
            self.campaigning = False

    # ================================================================== Fast Paxos
    def _alloc_slot_fast(self):
        s = max(self.prefix, self.any_from)
        acc = self.acc
        while True:
            if s in self.decided or s in self.my_active:
                s += 1
                continue
            st = acc.get(s)
            if st is not None and st[1]:
                s += 1                   # 本机 Acceptor 已看到别人占了这个实例
                continue
            break
        self.my_active.add(s)
        self._see_slot(s)
        return s

    async def _fast_propose(self, cmd):
        cid = (cmd[0], cmd[1])
        value = wire.enc_batch([cmd])
        rt = self.cfg.retry_ms / 1000.0
        attempt = 0
        while cid not in self.cmd_slot:
            if not self.any_ballot or self.leader is None:
                self.any_event.clear()
                try:
                    await asyncio.wait_for(self.any_event.wait(), rt)
                except asyncio.TimeoutError:
                    pass
                continue
            b = self.any_ballot
            s = self._alloc_slot_fast()
            ev = self.slot_events[s] = asyncio.Event()
            self.fast_mine[s] = [b, value, {}]
            self.stats["fast_proposals"] += 1
            tries = 0
            while s not in self.decided and tries < 3:
                if self.any_ballot == b:
                    self.bcast(self.voters, FAST_PROPOSE, slot=s, ballot=b, payload=value)
                try:
                    await asyncio.wait_for(ev.wait(), rt)
                except asyncio.TimeoutError:
                    tries += 1
                    self.stats["timeouts"] += 1
            self.fast_mine.pop(s, None)
            self.slot_events.pop(s, None)
            self.my_active.discard(s)
            if cid not in self.cmd_slot:
                self.stats["slot_lost"] += 1      # 冲突落败（或超时），随机退避后换实例重投
                attempt += 1
                await self._backoff(attempt)

    def _on_fast_accepted(self, m):
        s = m.slot
        if s in self.decided:
            return
        mine = self.fast_mine.get(s)
        if mine is not None and mine[0] == m.ballot:
            mine[2][m.src] = m.payload
            if sum(1 for v in mine[2].values() if v == mine[1]) >= self.qf:
                # 快速多数派接受了我的值：2 个消息延迟即学到
                self.stats["fast_learned"] += 1
                self.learn(s, mine[1], announce=self.is_leader)
                return
        if self.is_leader and m.ballot == self.my_ballot:
            fs = self.fast_slots.get(s)
            if fs is None:
                fs = self.fast_slots[s] = FastSlot(now())
            if fs.recovering:
                return
            fs.reports[m.src] = m.payload
            self._fast_eval(s, fs)

    def _fast_eval(self, s, fs):
        cnt = collections.Counter(fs.reports.values())
        v, c = _top(cnt)
        if c >= self.qf:
            self.learn(s, v, announce=True)
            return
        nrep = len(fs.reports)
        if nrep < self.qc:
            return
        impossible = c + (self.n - nrep) < self.qf
        if impossible or nrep >= self.tp.alive(self.voters) \
                or now() - fs.t0 > self.cfg.fast_wait_ms / 1000.0:
            self._fast_recover(s, fs, v, len(cnt) > 1)

    def _fast_recover(self, s, fs, v, collided):
        """Coordinated recovery（Lamport, Fast Paxos §3.2）。

        Q = 已上报的 Acceptor（|Q| >= qc）。它们在快速轮 b 的投票是终局的（每轮只投一次），
        又因为已参与 b 而不会再给更低的轮投票，所以这些 2b 报文可直接当作 b|1 轮的 1b 用，
        省掉一轮 Prepare。b|1 紧邻 b，中间没有别的编号，不存在被遗漏的轮次。
        选值：若 |Q| - cnt(v) <= n - qf，则 v 可能已被选定，必须取 v（由 qc + 2qf > 2n 保证唯一，
        且一定是出现次数最多的值）；否则任何值都安全，这里取出现最多的。"""
        fs.recovering = True
        self.stats["fast_collisions" if collided else "fast_slowpath"] += 1
        self._propose_slot(s, v, self.my_ballot | 1)

    # ================================================================== 定时任务
    async def _tick_loop(self):
        cfg = self.cfg
        hb = cfg.hb_ms / 1000.0
        rt = cfg.retry_ms / 1000.0
        tick = min(hb, rt / 2, 0.05)
        next_hb = 0.0
        last_catchup = 0.0
        while True:
            await asyncio.sleep(tick)
            t = now()
            if self.is_leader:
                if t >= next_hb:
                    next_hb = t + hb
                    self._send_heartbeat()
                for s, inf in list(self.inflight.items()):        # Accept 超时重传
                    if t - inf[3] > rt:
                        inf[3] = t
                        self.stats["retransmits"] += 1
                        for p in self.voters:
                            if p not in inf[2]:
                                self.send(p, ACCEPT, slot=s, ballot=inf[1], payload=inf[0])
                if self.mode == "fast":
                    self._fast_tick(t, rt)
            elif self.is_voter and self.mode != "basic" and not self.campaigning \
                    and t - self.last_hb > self.el_deadline:
                self.loop.create_task(self._campaign())           # 心跳超时 -> 发起选举
            if self.mode == "multi" and self.fwd_pending:
                for ent in list(self.fwd_pending.values()):       # 转发超时重试
                    if t - ent[1] > rt:
                        ent[1] = t
                        self.stats["retransmits"] += 1
                        self._multi_submit(ent[0])
            # 追赶：发现自己落后（丢了 LEARN、刚重启、listener）就向别人要
            gap = self.max_seen - self.prefix + 1
            if gap > 0 and ((t - self.last_progress > rt and t - last_catchup > rt)
                            or (gap > 100 and t - last_catchup > 0.05)):
                last_catchup = t
                if self.leader is not None and self.leader != self.id:
                    peer = self.leader
                else:
                    cands = [p for p in self.voters if p != self.id]
                    peer = random.choice(cands) if cands else None
                if peer is not None:
                    self.send(peer, LEARN_REQ, slot=self.prefix)

    def _fast_tick(self, t, rt):
        fw = self.cfg.fast_wait_ms / 1000.0
        # 补洞：有更高实例在活动、而较低实例长期无人问津时，由协调者投 NOOP
        hi = min(self.max_seen, self.prefix + 256)
        for s in range(max(self.prefix, self.any_from_mine), hi + 1):
            if s not in self.decided and s not in self.fast_slots and s not in self.inflight:
                self.fast_slots[s] = FastSlot(t)
        for s, fs in list(self.fast_slots.items()):
            if fs.recovering:
                continue
            if len(fs.reports) >= self.qc:
                if t - fs.t0 > fw:
                    self._fast_eval(s, fs)
            elif t - fs.t_probe > rt:
                fs.t_probe = t
                self.stats["fast_probes"] += 1
                for p in self.voters:
                    if p not in fs.reports:
                        self.send(p, FAST_PROPOSE, slot=s, ballot=self.my_ballot, payload=NOOP)

    # ================================================================== 控制面
    def on_ping(self, m, conn):
        self.send(m.src, PONG, aux=m.aux)

    def on_pong(self, m, conn):
        fut = self.pings.pop(m.aux, None)
        if fut is not None and not fut.done():
            fut.set_result(now())

    async def _ping(self, peer, count):
        out = []
        for i in range(count):
            key = random.getrandbits(60)
            fut = self.pings[key] = self.loop.create_future()
            t0 = now()
            self.send(peer, PING, aux=key)
            try:
                t1 = await asyncio.wait_for(fut, 2.0)
                out.append((t1 - t0) * 1000.0)
            except asyncio.TimeoutError:
                self.pings.pop(key, None)
            await asyncio.sleep(0.005)
        return out

    def on_ctrl(self, m, conn):
        req = json.loads(m.payload.decode())
        op = req.get("op")
        if op == "ping":
            async def run():
                r = await self._ping(req["peer"], req.get("count", 20))
                conn.send(pack(CTRL_RESP, self.id, aux=m.aux, payload=json.dumps({"rtt_ms": r}).encode()))
            self.loop.create_task(run())
            return
        if op == "net":
            self.tp.set_faults(delay_ms=req.get("delay_ms"), jitter_ms=req.get("jitter_ms"),
                               loss=req.get("loss"), block=req.get("block"))
            resp = {"ok": True}
        elif op == "stats":
            resp = dict(self.stats)
            resp.update(id=self.id, mode=self.mode, is_leader=self.is_leader, leader=self.leader,
                        ballot=wire.ballot_str(self.leader_ballot), prefix=self.prefix,
                        ndecided=len(self.decided), max_seen=self.max_seen,
                        fsync_count=self.wal.fsync_count, fsync_time=self.wal.fsync_time,
                        wal_records=self.wal.records, wal_bytes=self.wal.bytes,
                        dropped=sum(l.dropped for l in self.tp.links.values()),
                        pending=len(self.client_pending), qc=self.qc, qf=self.qf,
                        promised=wire.ballot_str(self.pg), any_ballot=wire.ballot_str(self.any_ballot),
                        pg_num=self.pg, lb_num=self.leader_ballot,
                        any_from=self.any_from, inflight=len(self.inflight),
                        fast_slots=len(self.fast_slots))
        elif op == "dump":
            frm = req.get("from", 0)
            resp = {"slots": [[s, zlib.crc32(v)] for s, v in sorted(self.decided.items()) if s >= frm]}
        elif op == "get":
            v = self.kv.get(req["key"].encode())
            resp = {"value": v.decode() if v is not None else None, "prefix": self.prefix}
        else:
            resp = {"error": "unknown op"}
        conn.send(pack(CTRL_RESP, self.id, aux=m.aux, payload=json.dumps(resp).encode()))


def _top(cnt):
    """出现次数最多的值；并列时按 CRC 取，避免总是先到的（协调者本地的）值获胜而饿死其他 Proposer。"""
    best = max(cnt.values())
    return min((v for v, c in cnt.items() if c == best), key=zlib.crc32), best


def log(msg):
    sys.stderr.write("%.3f %s\n" % (time.time(), msg))
    sys.stderr.flush()


def parse_addrs(s):
    out = {}
    for part in filter(None, (s or "").split(",")):
        nid, addr = part.split("=")
        host, port = addr.rsplit(":", 1)
        out[int(nid)] = (host, int(port))
    return out


def build_parser():
    ap = argparse.ArgumentParser(description="Paxos node")
    ap.add_argument("--id", type=int, required=True)
    ap.add_argument("--peers", required=True, help="投票节点: 1=host:port,2=host:port,...")
    ap.add_argument("--listeners", default="", help="只学习不投票的节点: 9=host:port")
    ap.add_argument("--bind", default=None, help="监听地址（默认取 peers 中自己的 host）")
    ap.add_argument("--mode", choices=["basic", "multi", "fast"], default="multi")
    ap.add_argument("--data-dir", default="data")
    ap.add_argument("--fsync", type=int, default=1)
    ap.add_argument("--hb-ms", type=float, default=50)
    ap.add_argument("--election-ms", type=float, default=300)
    ap.add_argument("--retry-ms", type=float, default=200)
    ap.add_argument("--backoff-ms", type=float, default=5)
    ap.add_argument("--fast-wait-ms", type=float, default=30)
    ap.add_argument("--batch", type=int, default=1)
    ap.add_argument("--linger-ms", type=float, default=0)
    ap.add_argument("--window", type=int, default=512)
    ap.add_argument("--contend", type=int, default=0, help="basic: 所有 Proposer 抢同一实例（活锁实验）")
    ap.add_argument("--delay-ms", type=float, default=0, help="初始单向延迟")
    ap.add_argument("--loss", type=float, default=0)
    return ap


def main(argv=None):
    cfg = build_parser().parse_args(argv)
    cfg.peers = parse_addrs(cfg.peers)
    cfg.listeners = parse_addrs(cfg.listeners)
    cfg.fsync = bool(cfg.fsync)
    if sys.platform == "win32":
        asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    tune_event_loop(loop)
    node = Node(cfg, loop)
    loop.run_until_complete(node.start())
    try:
        loop.run_forever()
    except KeyboardInterrupt:
        pass


if __name__ == "__main__":
    main()
