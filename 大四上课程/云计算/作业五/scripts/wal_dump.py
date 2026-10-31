"""查看 Acceptor 的 WAL：python scripts/wal_dump.py <wal文件> [最后N条]"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from paxos import wire  # noqa: E402
from paxos.storage import WAL, REC_ACCEPT, REC_PROMISE_GLOBAL, REC_PROMISE_SLOT  # noqa: E402

NAMES = {REC_PROMISE_GLOBAL: "PROMISE(range)", REC_PROMISE_SLOT: "PROMISE(slot)", REC_ACCEPT: "ACCEPT"}


def hexdump(path, n):
    with open(path, "rb") as f:
        data = f.read(n)
    for off in range(0, len(data), 16):
        row = data[off:off + 16]
        print("%08x  %-48s |%s|" % (off, " ".join("%02x" % b for b in row),
                                    "".join(chr(b) if 32 <= b < 127 else "." for b in row)))


def main():
    if sys.argv[1] == "--hex":
        return hexdump(sys.argv[2], int(sys.argv[3]) if len(sys.argv) > 3 else 96)
    path = sys.argv[1]
    last = int(sys.argv[2]) if len(sys.argv) > 2 else 12
    with open(path, "rb") as f:
        size = len(f.read())
    w = WAL(path, fsync=False)
    recs = w.replay()
    w.close()
    cnt = {}
    for r in recs:
        cnt[NAMES[r[0]]] = cnt.get(NAMES[r[0]], 0) + 1
    print("%s: %d bytes, %d records %s" % (os.path.basename(path), size, len(recs), cnt))
    print("%-6s %-15s %-6s %-12s %s" % ("#", "record", "slot", "ballot", "accepted_value (decoded command batch)"))
    idx = [i for i, r in enumerate(recs) if r[0] != REC_ACCEPT][:3]
    idx += [i for i in range(max(0, len(recs) - last), len(recs)) if i not in idx]
    prev = -1
    for i in sorted(idx):
        if i != prev + 1:
            print("...")
        prev = i
        rt, slot, ballot, value = recs[i]
        val = ""
        if rt == REC_ACCEPT:
            cmds = wire.dec_batch(value)
            val = "NOOP" if not cmds else "; ".join(
                "from node%d seq=%d %r" % (o, s & 0xFFFFFFFF, d[:30]) for o, s, d in cmds[:2])
        print("%-6d %-15s %-6s %-12s %s" % (i, NAMES[rt], "*" if rt == REC_PROMISE_GLOBAL else slot,
                                           wire.ballot_str(ballot), val))


if __name__ == "__main__":
    main()
