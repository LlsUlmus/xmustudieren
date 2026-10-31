#!/bin/sh
# 单机起一个本地集群（多进程 + 回环 TCP）。  ./scripts/start_local.sh [节点数=3] [mode=multi] [fsync=0]
N=${1:-3}; MODE=${2:-multi}; FSYNC=${3:-0}
PY=${PYTHON:-python3}
cd "$(dirname "$0")/.." || exit 1
mkdir -p run/local
PEERS=""
for i in $(seq 1 "$N"); do PEERS="$PEERS${PEERS:+,}$i=127.0.0.1:$((7000+i))"; done
for i in $(seq 1 "$N"); do
  $PY -m paxos.node --id "$i" --peers "$PEERS" --listeners 9=127.0.0.1:7009 --mode "$MODE" \
     --fsync "$FSYNC" --data-dir run/local/data 2>>"run/local/node-$i.log" &
  echo "$!" > "run/local/node-$i.pid"
done
$PY -m paxos.node --id 9 --peers "$PEERS" --listeners 9=127.0.0.1:7009 --mode "$MODE" \
   --data-dir run/local/data 2>>run/local/node-9.log &
echo "$!" > run/local/node-9.pid
echo "PEERS=$PEERS"
echo "查看状态: $PY -m paxos.client --peers $PEERS status"
echo "停止:     kill \$(cat run/local/*.pid)"
