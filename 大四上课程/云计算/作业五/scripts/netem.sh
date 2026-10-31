#!/bin/sh
# 在 Docker 容器 / 虚拟机内用 tc netem 注入内核层延迟与丢包。
#   netem.sh set <单向延迟ms> <丢包率%>     例：netem.sh set 5 1   => RTT≈10ms, 丢包 1%
#   netem.sh clear
#   netem.sh block <对端IP>  /  netem.sh unblock <对端IP>     模拟网络分区
# Docker 下： docker compose exec n1 sh /app/scripts/netem.sh set 5 1
DEV=${DEV:-eth0}
case "$1" in
  set)     tc qdisc replace dev "$DEV" root netem delay "${2}ms" loss "${3:-0}%" ;;
  clear)   tc qdisc del dev "$DEV" root 2>/dev/null; iptables -F ;;
  block)   iptables -A INPUT -s "$2" -j DROP; iptables -A OUTPUT -d "$2" -j DROP ;;
  unblock) iptables -D INPUT -s "$2" -j DROP; iptables -D OUTPUT -d "$2" -j DROP ;;
  *) echo "usage: $0 set <delay_ms> <loss_%> | clear | block <ip> | unblock <ip>"; exit 1 ;;
esac
