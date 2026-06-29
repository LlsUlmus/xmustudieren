package midware.frontend;

import jakarta.servlet.http.HttpServlet;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import java.io.IOException;

public class IndexServlet extends HttpServlet {
    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws IOException {
        resp.setCharacterEncoding("UTF-8");
        resp.setContentType("text/html;charset=UTF-8");

        // 轻量美化前端：发布 + SSE 订阅（多开浏览器标签即可验证 topic 一对多）
        String html = """
                <!doctype html>
                <html lang="zh-CN">
                <head>
                  <meta charset="utf-8"/>
                  <meta name="viewport" content="width=device-width, initial-scale=1"/>
                  <title>中间件 Topic/存储转发演示</title>
                  <style>
                    :root {
                      --bg1: #eef4ff;
                      --bg2: #f7fbff;
                      --card: #ffffff;
                      --border: #dbe7ff;
                      --primary: #2f6bff;
                      --primary-dark: #2455cf;
                      --text: #1e2a3a;
                      --muted: #6c7a90;
                      --ok: #1f9d55;
                      --warn: #f59e0b;
                    }
                    * { box-sizing: border-box; }
                    body {
                      margin: 0;
                      min-height: 100vh;
                      font-family: "Segoe UI", Arial, sans-serif;
                      color: var(--text);
                      background: linear-gradient(135deg, var(--bg1), var(--bg2));
                      display: flex;
                      justify-content: center;
                      align-items: flex-start;
                      padding: 26px;
                    }
                    .card {
                      width: min(920px, 100%);
                      background: var(--card);
                      border: 1px solid var(--border);
                      border-radius: 16px;
                      box-shadow: 0 14px 35px rgba(47, 107, 255, 0.12);
                      overflow: hidden;
                    }
                    .header {
                      padding: 18px 22px;
                      background: linear-gradient(120deg, #2f6bff, #59a0ff);
                      color: #fff;
                    }
                    .header h2 {
                      margin: 0 0 6px;
                      font-size: 22px;
                      font-weight: 700;
                    }
                    .header p {
                      margin: 0;
                      opacity: 0.95;
                      font-size: 14px;
                    }
                    .meta {
                      margin-top: 10px;
                      display: flex;
                      gap: 8px;
                      flex-wrap: wrap;
                    }
                    .pill {
                      background: rgba(255,255,255,.18);
                      color: #fff;
                      border: 1px solid rgba(255,255,255,.35);
                      border-radius: 999px;
                      font-size: 12px;
                      padding: 4px 10px;
                    }
                    .content { padding: 20px 22px 22px; }
                    .grid {
                      display: grid;
                      grid-template-columns: 1fr 1fr;
                      gap: 12px;
                    }
                    .field label {
                      display: block;
                      margin-bottom: 6px;
                      color: var(--muted);
                      font-size: 13px;
                    }
                    input, textarea {
                      width: 100%;
                      border: 1px solid #d7e3ff;
                      border-radius: 10px;
                      font-size: 14px;
                      padding: 10px 12px;
                      outline: none;
                      transition: border-color .2s, box-shadow .2s;
                      background: #fff;
                    }
                    input:focus, textarea:focus {
                      border-color: #6c95ff;
                      box-shadow: 0 0 0 3px rgba(108,149,255,.18);
                    }
                    textarea {
                      min-height: 90px;
                      resize: vertical;
                      margin-top: 10px;
                    }
                    .btn-row {
                      margin-top: 12px;
                      display: flex;
                      gap: 10px;
                      flex-wrap: wrap;
                    }
                    button {
                      border: 0;
                      border-radius: 10px;
                      padding: 9px 16px;
                      font-size: 14px;
                      cursor: pointer;
                      transition: transform .06s, opacity .2s, background .2s;
                    }
                    button:active { transform: translateY(1px); }
                    .btn-primary { background: var(--primary); color: #fff; }
                    .btn-primary:hover { background: var(--primary-dark); }
                    .btn-ghost { background: #eef3ff; color: #355087; }
                    .btn-ghost:hover { background: #e1ebff; }
                    .status {
                      margin-top: 12px;
                      display: inline-flex;
                      align-items: center;
                      gap: 8px;
                      color: var(--muted);
                      font-size: 13px;
                    }
                    .dot {
                      width: 8px; height: 8px; border-radius: 50%;
                      background: #9aa9c3;
                    }
                    .dot.ok { background: var(--ok); box-shadow: 0 0 0 4px rgba(31,157,85,.16); }
                    .dot.warn { background: var(--warn); box-shadow: 0 0 0 4px rgba(245,158,11,.16); }
                    #log {
                      margin-top: 14px;
                      height: 320px;
                      overflow: auto;
                      border: 1px solid #dbe6ff;
                      border-radius: 12px;
                      background: #f8fbff;
                      padding: 12px;
                      font-family: Consolas, "Courier New", monospace;
                      font-size: 13px;
                      line-height: 1.5;
                    }
                    .toolbar {
                      margin-top: 10px;
                      display: flex;
                      justify-content: space-between;
                      align-items: center;
                      gap: 8px;
                      flex-wrap: wrap;
                    }
                    .counter {
                      color: var(--muted);
                      font-size: 12px;
                    }
                    @media (max-width: 720px) {
                      .grid { grid-template-columns: 1fr; }
                    }
                  </style>
                </head>
                <body>
                  <div class="card">
                    <div class="header">
                      <h2>中间件技术实验：Topic + 存储转发</h2>
                      <p>Tomcat 前端 + Socket Broker</p>
                      <div class="meta">
                        <span class="pill">发布/订阅</span>
                        <span class="pill">一对多 Topic</span>
                        <span class="pill">离线存储转发</span>
                      </div>
                    </div>
                    <div class="content">
                      <div class="grid">
                        <div class="field">
                          <label>Topic</label>
                          <input id="topic" value="news"/>
                        </div>
                        <div class="field">
                          <label>消息内容</label>
                          <input id="quickMsg" value="第一条消息"/>
                        </div>
                      </div>
                      <textarea id="message">第一条消息</textarea>
                      <div class="btn-row">
                        <button class="btn-primary" onclick="publish()">发布消息</button>
                        <button class="btn-primary" onclick="subscribe()">开始订阅</button>
                        <button class="btn-ghost" onclick="clearLog()">清空日志</button>
                      </div>
                      <div class="status">
                        <span id="stateDot" class="dot"></span>
                        <span id="stateText">未订阅</span>
                      </div>
                      <div class="toolbar">
                        <div class="counter">日志条数：<span id="logCount">0</span></div>
                      </div>
                      <div id="log"></div>
                    </div>
                  </div>

                  <script>
                    let es = null;
                    let hadError = false;
                    const stateText = () => document.getElementById('stateText');
                    const stateDot = () => document.getElementById('stateDot');
                    const topicInput = () => document.getElementById('topic');
                    const msgInput = () => document.getElementById('message');
                    const quickMsg = () => document.getElementById('quickMsg');

                    quickMsg().addEventListener('input', (e) => {
                      msgInput().value = e.target.value;
                    });
                    msgInput().addEventListener('input', (e) => {
                      quickMsg().value = e.target.value;
                    });

                    function setState(text, ok) {
                      stateText().textContent = text;
                      stateDot().className = ok ? 'dot ok' : 'dot warn';
                    }

                    function log(s) {
                      const el = document.getElementById('log');
                      el.textContent = el.textContent + s + '\\n';
                      el.scrollTop = el.scrollHeight;
                      const count = el.textContent.trim() ? el.textContent.trim().split('\\n').length : 0;
                      document.getElementById('logCount').textContent = String(count);
                    }
                    async function publish() {
                      const topic = topicInput().value.trim();
                      const message = msgInput().value.trim();
                      if (!topic || !message) { alert('topic 和 message 不能为空'); return; }
                      const body = new URLSearchParams({ topic, message }).toString();
                      const res = await fetch('/api/publish', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' },
                        body
                      });
                      const txt = await res.text();
                      log('[publish] ' + txt);
                    }
                    function subscribe() {
                      const topic = topicInput().value.trim();
                      if (!topic) { alert('topic 不能为空'); return; }
                      if (es) { es.close(); es = null; }
                      log('[subscribe] topic=' + topic);
                      setState('正在订阅 ' + topic + ' ...', false);
                      es = new EventSource('/api/events?topic=' + encodeURIComponent(topic));
                      es.onmessage = (ev) => {
                        hadError = false;
                        setState('已订阅：' + topic, true);
                        log(ev.data);
                      };
                      es.onerror = () => {
                        setState('连接异常，等待重连', false);
                        if (!hadError) {
                          log('[events] 连接错误/断开（自动重连中）');
                          hadError = true;
                        }
                        // 不强制 close：浏览器可能自动重连
                      };
                    }
                    function clearLog() {
                      document.getElementById('log').textContent = '';
                      document.getElementById('logCount').textContent = '0';
                    }
                    setState('未订阅', false);
                  </script>
                </body>
                </html>
                """;

        resp.getWriter().write(html);
    }
}

