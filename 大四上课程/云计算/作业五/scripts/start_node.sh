#!/bin/sh
# 在 Ubuntu 虚拟机上启动一个节点（每台虚拟机执行一次，ID 不同）。
#   ./scripts/start_node.sh <节点ID> [mode] [fsync]
# 先按实际固定 IP 修改 PEERS / LISTENERS，并放行端口：sudo ufw allow 7000/tcp
ID=${1:?usage: start_node.sh <id> [basic|multi|fast] [fsync 0|1]}
MODE=${2:-multi}
FSYNC=${3:-1}
PEERS=${PEERS:-"1=192.168.56.11:7000,2=192.168.56.12:7000,3=192.168.56.13:7000"}
LISTENERS=${LISTENERS:-""}
cd "$(dirname "$0")/.." || exit 1
mkdir -p data logs
exec python3 -m paxos.node --id "$ID" --peers "$PEERS" ${LISTENERS:+--listeners "$LISTENERS"} \
  --bind 0.0.0.0 --mode "$MODE" --fsync "$FSYNC" --data-dir data 2>>"logs/node-$ID.log"
