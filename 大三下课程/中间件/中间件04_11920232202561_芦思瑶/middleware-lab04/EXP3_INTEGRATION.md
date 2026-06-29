# 实验三接入说明（不改实验三源文件）

本项目已新增实验三桥接器，默认关闭；开启后会订阅实验三 `spring-chat` 的 STOMP 主题并自动写入实验四消息链路（RabbitMQ -> `chat_events`）。
此外，实验四已内置聊天 WebSocket 能力（`/ws`、`/app/chat.send`、`/topic/messages`）和演示页 `index.html`，可单独提交运行。

## 1. 启动实验三（原项目不改动）

确保实验三 `spring-chat` 运行在默认端口 `8080`，并可提供 `/ws` SockJS 端点、`/topic/messages` 主题。

## 2. 启动实验四（开启桥接）

如果实验三占用 8080，实验四建议改到 8081。

```powershell
cd "E:\大三下提交\中间件\中间件04_11920232202561_芦思瑶\middleware-lab04"

$env:SERVER_PORT="8081"
$env:RABBITMQ_HOST="capybara.lmq.cloudamqp.com"
$env:RABBITMQ_PORT="5671"
$env:RABBITMQ_USER="ifkaguci"
$env:RABBITMQ_PASSWORD="你的当前密码"
$env:RABBITMQ_VHOST="ifkaguci"
$env:RABBITMQ_SSL="true"
$env:WEBHOOK_AUTH_DISABLED="true"

$env:EXP3_BRIDGE_ENABLED="true"
$env:EXP3_WS_URL="ws://localhost:8080/ws"
$env:EXP3_TOPIC="/topic/messages"

mvn spring-boot:run
```

## 3. 验证

1. 在实验三前端发送聊天消息。  
2. 查询实验四数据库 `chat_events`，应出现 `source=exp3-stomp`、`intent=chat_message` 的新记录。

```sql
select id, source, intent, status, created_at from chat_events order by created_at desc;
```

## 4. 单独提交实验四（不依赖实验三）

直接启动实验四，访问：

- `http://localhost:8080/index.html`（或你自定义端口）

通过页面发送聊天消息后，查询 `chat_events` 可看到 `source=exp4-stomp` 的记录。

