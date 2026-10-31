"""自定义 TCP 报文格式（二进制定长头 + 变长负载）。

    0      2     3     4      6       8          16         24         32      36     40
    +------+-----+-----+------+-------+----------+----------+----------+-------+------+
    | 'PX' | ver | typ | src  | flags |   slot   |  ballot  |   aux    |  len  | crc  |
    +------+-----+-----+------+-------+----------+----------+----------+-------+------+
      2s     B     B     H       H         Q          Q          Q         I      I

所有字段网络字节序。crc 为负载的 CRC32。slot/ballot/aux 的含义随报文类型变化，见各类型注释。
"""
import struct
import zlib

MAGIC = b"PX"
VERSION = 1
HEADER = struct.Struct("!2sBBHHQQQII")
HEADER_SIZE = HEADER.size  # 40
MAX_PAYLOAD = 256 * 1024 * 1024

# ---- 报文类型 ----
HELLO = 1          # 建连握手            aux=角色(0 节点,1 客户端)
PREPARE = 2        # Phase 1a            slot, ballot; flags&F_RANGE 表示对 >=slot 的全部实例生效
PROMISE = 3        # Phase 1b            slot, ballot; payload=已接受条目列表
NACK = 4           # 拒绝                slot, ballot=被拒编号, aux=已承诺的更高编号
ACCEPT = 5         # Phase 2a            slot, ballot; payload=value
ACCEPTED = 6       # Phase 2b            slot, ballot; 快速轮携带 payload=value
LEARN = 7          # 通知已选定          slot; payload=value
HEARTBEAT = 8      # Leader 心跳/租约    ballot, slot=leader 已决前缀, aux=any_from(Fast)
FAST_PROPOSE = 9   # Fast Paxos 直投     slot, ballot(快速轮); payload=value
FORWARD = 10       # 转发客户端命令      payload=batch
LEARN_REQ = 11     # 追赶请求            slot=起始实例
CLIENT_REQ = 12    # 客户端请求          aux=客户端序号; payload=data
CLIENT_RESP = 13   # 客户端响应          aux=客户端序号; slot=被选定的实例; flags=状态
CTRL = 14          # 控制面（故障注入/统计） payload=JSON
CTRL_RESP = 15
PING = 16          # 链路 RTT 探测       aux=发送时间戳(ns)
PONG = 17

TYPE_NAMES = {
    HELLO: "HELLO", PREPARE: "PREPARE", PROMISE: "PROMISE", NACK: "NACK",
    ACCEPT: "ACCEPT", ACCEPTED: "ACCEPTED", LEARN: "LEARN", HEARTBEAT: "HEARTBEAT",
    FAST_PROPOSE: "FAST_PROPOSE", FORWARD: "FORWARD", LEARN_REQ: "LEARN_REQ",
    CLIENT_REQ: "CLIENT_REQ", CLIENT_RESP: "CLIENT_RESP", CTRL: "CTRL",
    CTRL_RESP: "CTRL_RESP", PING: "PING", PONG: "PONG",
}

# ---- flags ----
F_RANGE = 0x01     # PREPARE/PROMISE：范围 Prepare（Multi-Paxos / Fast Paxos 的 Leader 选举）
F_FAST = 0x02      # ACCEPTED/HEARTBEAT：快速轮
F_PHASE2 = 0x04    # NACK：拒绝的是 Accept
F_ERR = 0x08       # CLIENT_RESP：失败

DECIDED_MARK = (1 << 64) - 1   # PROMISE 条目里用它当 ballot，表示该实例在本 Acceptor 上已知被选定


def pack(mtype, src, slot=0, ballot=0, aux=0, payload=b"", flags=0):
    return HEADER.pack(MAGIC, VERSION, mtype, src, flags, slot, ballot, aux,
                       len(payload), zlib.crc32(payload)) + payload


class Msg:
    __slots__ = ("type", "src", "flags", "slot", "ballot", "aux", "payload")

    def __init__(self, mtype, src, flags, slot, ballot, aux, payload):
        self.type = mtype
        self.src = src
        self.flags = flags
        self.slot = slot
        self.ballot = ballot
        self.aux = aux
        self.payload = payload

    def __repr__(self):
        return "<%s src=%d slot=%d ballot=%s aux=%d len=%d>" % (
            TYPE_NAMES.get(self.type, self.type), self.src, self.slot,
            ballot_str(self.ballot), self.aux, len(self.payload))


class FrameError(Exception):
    pass


class FrameParser:
    """流式拆包：TCP 是字节流，需要自己按头部长度切帧。"""

    def __init__(self):
        self.buf = bytearray()

    def feed(self, data):
        self.buf += data
        out = []
        buf = self.buf
        pos = 0
        n = len(buf)
        while n - pos >= HEADER_SIZE:
            magic, ver, mtype, src, flags, slot, ballot, aux, length, crc = \
                HEADER.unpack_from(buf, pos)
            if magic != MAGIC or ver != VERSION or length > MAX_PAYLOAD:
                raise FrameError("bad frame header")
            end = pos + HEADER_SIZE + length
            if n < end:
                break
            payload = bytes(buf[pos + HEADER_SIZE:end])
            if zlib.crc32(payload) != crc:
                raise FrameError("payload crc mismatch")
            out.append(Msg(mtype, src, flags, slot, ballot, aux, payload))
            pos = end
        if pos:
            del buf[:pos]
        return out


# ---- 提案编号 ----
# ballot = round<<16 | node<<1 | classic
# 按 (round, node, classic) 字典序全序且全局唯一。同一 (round, node) 下快速轮(classic=0)
# 与经典轮(classic=1)相邻，中间不存在任何其他编号——Fast Paxos 的 coordinated recovery
# 依赖这一点（见 node.py 中 _fast_recover 的说明）。

def mk_ballot(rnd, node, classic=True):
    return (rnd << 16) | (node << 1) | (1 if classic else 0)


def ballot_round(b):
    return b >> 16


def ballot_node(b):
    return (b >> 1) & 0x7FFF


def is_fast(b):
    return b != 0 and (b & 1) == 0


def ballot_str(b):
    if b == 0:
        return "-"
    if b == DECIDED_MARK:
        return "DECIDED"
    return "%d.%d%s" % (ballot_round(b), ballot_node(b), "f" if is_fast(b) else "")


# ---- value 编码：一个实例的值是一批命令（批处理优化），空批即 NOOP ----
_CMD = struct.Struct("!HQI")
_CNT = struct.Struct("!H")
NOOP = _CNT.pack(0)


def enc_batch(cmds):
    """cmds: [(origin_node, seq, data_bytes), ...]"""
    parts = [_CNT.pack(len(cmds))]
    for origin, seq, data in cmds:
        parts.append(_CMD.pack(origin, seq, len(data)))
        parts.append(data)
    return b"".join(parts)


def dec_batch(value):
    (cnt,) = _CNT.unpack_from(value, 0)
    pos = _CNT.size
    out = []
    for _ in range(cnt):
        origin, seq, ln = _CMD.unpack_from(value, pos)
        pos += _CMD.size
        out.append((origin, seq, value[pos:pos + ln]))
        pos += ln
    return out


# ---- PROMISE 负载：[(slot, accepted_ballot, accepted_value)] ----
_ENT = struct.Struct("!QQI")
_ECNT = struct.Struct("!I")


def enc_entries(entries):
    parts = [_ECNT.pack(len(entries))]
    for slot, ballot, value in entries:
        parts.append(_ENT.pack(slot, ballot, len(value)))
        parts.append(value)
    return b"".join(parts)


def dec_entries(payload):
    if not payload:
        return []
    (cnt,) = _ECNT.unpack_from(payload, 0)
    pos = _ECNT.size
    out = []
    for _ in range(cnt):
        slot, ballot, ln = _ENT.unpack_from(payload, pos)
        pos += _ENT.size
        out.append((slot, ballot, payload[pos:pos + ln]))
        pos += ln
    return out
