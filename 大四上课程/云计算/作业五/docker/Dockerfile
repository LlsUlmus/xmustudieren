# Paxos 节点镜像：纯 Python 标准库实现，无第三方依赖
FROM python:3.11-slim
# iproute2 提供 tc（netem 延迟/丢包注入），iptables 用于模拟网络分区
RUN apt-get update && apt-get install -y --no-install-recommends iproute2 iptables iputils-ping \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY paxos /app/paxos
COPY scripts /app/scripts
ENV PYTHONUNBUFFERED=1
VOLUME /data
EXPOSE 7000
ENTRYPOINT ["python", "-m", "paxos.node"]
