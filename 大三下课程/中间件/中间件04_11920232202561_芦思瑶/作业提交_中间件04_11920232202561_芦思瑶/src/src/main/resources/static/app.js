(function () {
  const apiBase = () => {
    const val = (document.getElementById("apiBase").value || "").trim();
    return (val || window.location.origin).replace(/\/$/, "");
  };
  const logEl = document.getElementById("log");
  const statusDot = document.getElementById("statusDot");
  const btnConnect = document.getElementById("btnConnect");
  const btnSend = document.getElementById("btnSend");
  const usernameInput = document.getElementById("username");
  const connHint = document.getElementById("connHint");
  const sentOut = document.getElementById("sentOut");

  let stompClient = null;

  function setConnUi(connected) {
    statusDot.classList.toggle("on", connected);
    btnConnect.textContent = connected ? "断开" : "连接";
    btnConnect.classList.toggle("off", connected);
    btnSend.disabled = !connected;
    connHint.classList.toggle("ok", connected);
    connHint.textContent = connected ? "已连接" : "未连接，点击「连接」后加入会话";
  }
  setConnUi(false);

  function initialChar(name) {
    const s = (name || "?").trim();
    return s.length ? s[0].toUpperCase() : "?";
  }

  function appendMessage(body) {
    if (!body.original && !body.translation) return;
    const t = new Date(body.serverTime || Date.now());
    const timeStr = t.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
    const row = document.createElement("div");
    row.className = "msg";

    const av = document.createElement("div");
    av.className = "avatar";
    av.textContent = initialChar(body.username);

    const card = document.createElement("div");
    card.className = "card";
    const meta = document.createElement("div");
    meta.className = "meta";
    const nm = document.createElement("span");
    nm.className = "nm";
    nm.textContent = body.username || "";
    const tm = document.createElement("span");
    tm.textContent = timeStr;
    meta.appendChild(nm);
    meta.appendChild(tm);

    const bubble = document.createElement("div");
    bubble.className = "bubble";
    bubble.textContent = body.original || "";

    card.appendChild(meta);
    card.appendChild(bubble);
    if (body.translation) {
      const tr = document.createElement("div");
      tr.className = "trans";
      tr.textContent = body.translation;
      card.appendChild(tr);
    }
    row.appendChild(av);
    row.appendChild(card);
    logEl.appendChild(row);
    logEl.scrollTop = logEl.scrollHeight;
  }

  btnConnect.addEventListener("click", function () {
    if (stompClient && stompClient.connected) {
      stompClient.disconnect(function () {});
      stompClient = null;
      setConnUi(false);
      return;
    }
    const socket = new SockJS(apiBase() + "/ws");
    stompClient = Stomp.over(socket);
    stompClient.debug = null;
    stompClient.connect(
      {},
      function () {
        setConnUi(true);
        stompClient.subscribe("/topic/messages", function (message) {
          appendMessage(JSON.parse(message.body));
        });
      },
      function () {
        setConnUi(false);
      }
    );
  });

  function sendMsg() {
    if (!stompClient || !stompClient.connected) {
      connHint.classList.remove("ok");
      connHint.textContent = "未连接，请先点击「连接」";
      return;
    }
    const text = document.getElementById("msg").value.trim();
    if (!text) return;
    stompClient.send(
      "/app/chat.send",
      { "content-type": "application/json;charset=UTF-8" },
      JSON.stringify({
        username: usernameInput.value.trim() || "匿名",
        text: text,
        targetLang: document.getElementById("targetLang").value
      })
    );
    document.getElementById("msg").value = "";
  }

  btnSend.addEventListener("click", sendMsg);
  document.getElementById("msg").addEventListener("keydown", function (e) {
    if (e.isComposing || e.keyCode === 229) return;
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendMsg();
    }
  });

  function clearSentOut() {
    while (sentOut.firstChild) sentOut.removeChild(sentOut.firstChild);
  }

  function renderSentimentResult(raw) {
    clearSentOut();
    let text = (raw || "").trim();
    if (text.startsWith("```")) {
      text = text.replace(/^```(?:json)?\s*/i, "").replace(/\s*```\s*$/m, "").trim();
    }
    let j;
    try {
      j = JSON.parse(text);
    } catch (_) {
      sentOut.textContent = raw;
      return;
    }
    const s = (j.sentiment || "").toString();
    const pill = document.createElement("span");
    pill.className = "sent-pill " + (s.includes("积极") ? "pos" : s.includes("消极") ? "neg" : "neu");
    pill.textContent = s || "中性";
    sentOut.appendChild(pill);

    if (j.reason) {
      const p = document.createElement("p");
      p.textContent = "说明：" + j.reason;
      sentOut.appendChild(p);
    }
    if (Array.isArray(j.suggestions) && j.suggestions.length) {
      const ul = document.createElement("ul");
      j.suggestions.forEach(function (item) {
        const li = document.createElement("li");
        li.textContent = item;
        li.style.cursor = "pointer";
        li.onclick = function () {
          const ta = document.getElementById("msg");
          ta.value = item;
          ta.focus();
        };
        ul.appendChild(li);
      });
      sentOut.appendChild(ul);
    }
  }

  async function postJson(path, body) {
    const r = await fetch(apiBase() + path, {
      method: "POST",
      headers: { "Content-Type": "application/json;charset=UTF-8" },
      body: JSON.stringify(body)
    });
    const rawText = await r.text();
    let data = {};
    try {
      data = rawText ? JSON.parse(rawText) : {};
    } catch (_) {
      data = { error: rawText || r.statusText };
    }
    if (!r.ok) {
      return { ok: false, error: data.error || "HTTP " + r.status };
    }
    return data;
  }

  document.getElementById("btnSent").addEventListener("click", async function () {
    const btn = this;
    const t = document.getElementById("sentIn").value.trim();
    if (!t) return;
    btn.disabled = true;
    clearSentOut();
    try {
      const d = await postJson("/api/sentiment", { text: t });
      if (d.ok) {
        renderSentimentResult(d.result);
      } else {
        sentOut.textContent = d.error || "失败";
      }
    } catch (e) {
      sentOut.textContent = String(e.message || e);
    }
    btn.disabled = false;
  });
})();
