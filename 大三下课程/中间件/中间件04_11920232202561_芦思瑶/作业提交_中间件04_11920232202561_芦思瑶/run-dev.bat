@echo off
chcp 65001 >nul
cd /d %~dp0
set SERVER_PORT=8081
set RABBITMQ_HOST=capybara.lmq.cloudamqp.com
set RABBITMQ_PORT=5671
set RABBITMQ_USER=ifkaguci
set RABBITMQ_PASSWORD=C4JdSHgQ4qETlHsBhwIn1ozNc_nD8KSG
set RABBITMQ_VHOST=ifkaguci
set RABBITMQ_SSL=true
set WEBHOOK_AUTH_DISABLED=true

mvn spring-boot:run
pause
