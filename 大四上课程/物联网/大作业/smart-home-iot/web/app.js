// 智慧客厅前端：通过 WebSocket 与中枢实时通信，动态渲染场景、设备、传感器与交互过程
(() => {
  const $ = (s, el = document) => el.querySelector(s);
  const $$ = (s, el = document) => [...el.querySelectorAll(s)];
  let ws = null;
  let state = { env: {}, devices: {} };
  const history = [];
  const MAX_POINTS = 150;

  // ---------------- WebSocket ----------------
  function connect() {
    ws = new WebSocket(`${location.protocol === 'https:' ? 'wss' : 'ws'}://${location.host}/ws/ui`);
    ws.onopen = () => $('#st-hub').classList.add('ok');
    ws.onclose = () => {
      $('#st-hub').classList.remove('ok');
      $('#st-dev').classList.remove('ok');
      setTimeout(connect, 2000);
    };
    ws.onmessage = (e) => handle(JSON.parse(e.data));
  }
  const send = (obj) => ws && ws.readyState === 1 && ws.send(JSON.stringify(obj));

  function handle(m) {
    switch (m.type) {
      case 'init':
        history.length = 0;
        history.push(...m.history.slice(-MAX_POINTS));
        renderRules(m.rules);
        $('#log').innerHTML = '';
        m.events.forEach(addLog);
        applyState(m.state);
        break;
      case 'state':
        if (m.point) { history.push(m.point); if (history.length > MAX_POINTS) history.shift(); }
        applyState(m.state);
        break;
      case 'event':
        addLog(m.event);
        flashStage(m.event.stage);
        if (m.event.rule) {
          fireRule(m.event.rule);
          addMsg('sys', '⚙️ ' + m.event.text);
        }
        break;
      case 'dialog':
        addMsg('user', (m.source === 'voice' ? '🎤 ' : '') + m.user_text);
        addTrace(m.trace.slice(1));
        addMsg('bot', m.reply);
        speak(m.reply);
        break;
      case 'greet':
        showGreet(m.text);
        addMsg('sys', `👋 ${m.text}（已加载「${m.user}」的偏好：${m.profile.color_temp}K / ${m.profile.brightness}% / 空调 ${m.profile.ac_target}℃）`);
        speak(m.text);
        break;
      case 'ack':
        if (m.cmd) flashCard(m.cmd.device);
        break;
    }
  }

  // ---------------- 状态渲染 ----------------
  function applyState(s) {
    state = s;
    const env = s.env || {}, dev = s.devices || {};
    $('#st-dev').classList.toggle('ok', !!s.online);
    $('#st-clock').textContent = '🕒 ' + (env.time || '--:--');
    $('#st-user').textContent = '用户：' + s.user;
    $('#auto-toggle').checked = !!s.auto;
    if (!env.time) return;

    // 传感器
    $('#v-temp').textContent = env.temperature.toFixed(1);
    $('#v-hum').textContent = env.humidity.toFixed(0);
    $('#v-lux').textContent = env.illuminance.toFixed(0);
    $('#v-pir').textContent = env.presence ? '有人' : '无人';
    $('#v-pir-sub').textContent = env.presence ? env.person : `已空置 ${Math.min(env.vacant_minutes, 999).toFixed(0)} 分`;
    $('#v-out').textContent = env.outdoor_temp.toFixed(1);
    $('#t-temp').classList.toggle('alert', env.temperature > 28);
    $('#t-hum').classList.toggle('alert', env.humidity > 78);
    $('#t-pir').classList.toggle('on', env.presence);

    // 设备控件（正在拖动的控件不覆盖）
    for (const [name, d] of Object.entries(dev)) {
      const card = $('#card-' + name);
      if (!card) continue;
      card.classList.toggle('active', name === 'curtain' ? d.position > 0 : !!d.on);
      const sw = $(`input[type=checkbox][data-dev=${name}]`, card);
      if (sw) sw.checked = !!d.on;
      $$('input[type=range]', card).forEach((r) => {
        if (document.activeElement !== r && !r.dragging) r.value = d[r.dataset.key];
      });
      $$('.seg', card).forEach((seg) => $$('button', seg).forEach((b) =>
        b.classList.toggle('sel', String(d[seg.dataset.key]) === b.dataset.val)));
    }
    $$('[data-show]').forEach((el) => {
      const [n, k] = el.dataset.show.split('.');
      let v = dev[n] && dev[n][k];
      if (v === undefined) return;
      if (k === 'brightness' || k === 'position') v += '%';
      if (k === 'color_temp') v += 'K';
      if (k === 'target') v += '℃';
      el.textContent = v;
    });
    renderRoom(env, dev);
    drawChart();
  }

  function ctColor(k) {
    const t = Math.max(0, Math.min(1, (k - 2700) / 3800));
    const w = [255, 172, 88], c = [215, 232, 255];
    return `rgb(${w.map((x, i) => Math.round(x + (c[i] - x) * t)).join(',')})`;
  }

  function renderRoom(env, dev) {
    const h = parseInt(env.time) + parseInt(env.time.split(':')[1]) / 60;
    // 天空颜色随时间变化
    let sky = ['#0b1a3a', '#22355f'], sun = 0, sunY = 95, starOp = 1;
    if (h >= 6 && h < 7.5 || h >= 17.5 && h < 19) { sky = ['#f39c6b', '#ffd6a5']; sun = 0.9; sunY = 150; starOp = 0; }
    else if (h >= 7.5 && h < 17.5) { sky = ['#5aa9ff', '#bfe3ff']; sun = 1; sunY = 80; starOp = 0; }
    $('#sky1').setAttribute('stop-color', sky[0]);
    $('#sky2').setAttribute('stop-color', sky[1]);
    $('#sun').setAttribute('opacity', sun);
    $('#sun').setAttribute('cy', sunY);
    $('#stars').setAttribute('opacity', starOp);

    // 窗帘
    const closed = 1 - dev.curtain.position / 100;
    const w = 10 + 75 * closed;
    $('#curtain-l').setAttribute('width', w);
    $('#curtain-r').setAttribute('width', w);
    $('#curtain-r').setAttribute('x', 210 - w);

    // 灯光
    const L = dev.light, col = ctColor(L.color_temp);
    $('#glow-c').setAttribute('stop-color', col);
    $('#light-cone').style.opacity = L.on ? (0.25 + 0.75 * L.brightness / 100) : 0;
    $('#bulb').setAttribute('fill', L.on ? col : '#777');

    // 整体照度遮罩
    $('#dim').setAttribute('opacity', Math.max(0, 0.78 - Math.min(env.illuminance, 500) / 500 * 0.78).toFixed(2));

    // 风扇
    const fb = $('#fan-blades');
    fb.classList.toggle('spin', dev.fan.on);
    fb.style.setProperty('--fan-dur', [1.2, 0.6, 0.3][dev.fan.speed - 1] + 's');

    // 空调
    const ac = dev.ac, acCol = { cool: '#5ec8ff', heat: '#ff8a5c', dry: '#4fe0b6' }[ac.mode];
    $('#ac-wind').classList.toggle('on', ac.on);
    $('#ac-wind').setAttribute('stroke', acCol);
    $('#ac-led').setAttribute('fill', ac.on ? acCol : '#9aa3b0');
    $('#ac-text').textContent = ac.on ? `${{ cool: '制冷', heat: '制热', dry: '除湿' }[ac.mode]} ${ac.target}℃` : 'OFF';

    // 音箱 & 人
    $('#notes').classList.toggle('on', dev.speaker.on);
    $('#person').setAttribute('opacity', env.presence ? 1 : 0);
    $('#person-name').textContent = env.person || '';
    $('#hud-text').textContent = `🌡 ${env.temperature.toFixed(1)}℃  💧 ${env.humidity.toFixed(0)}%  ☀ ${env.illuminance.toFixed(0)} lux`;
  }

  // ---------------- 曲线 ----------------
  function drawChart() {
    const cv = $('#chart'), dpr = window.devicePixelRatio || 1;
    const W = cv.clientWidth, H = 190;
    if (cv.width !== W * dpr) { cv.width = W * dpr; cv.height = H * dpr; }
    const g = cv.getContext('2d');
    g.setTransform(dpr, 0, 0, dpr, 0, 0);
    g.clearRect(0, 0, W, H);
    const pad = { l: 34, r: 40, t: 10, b: 22 };
    const pw = W - pad.l - pad.r, ph = H - pad.t - pad.b;
    const temps = history.map((p) => p.temperature), luxs = history.map((p) => p.illuminance);
    const tMin = Math.floor(Math.min(22, ...temps)), tMax = Math.ceil(Math.max(32, ...temps));
    const lMax = Math.max(500, ...luxs);
    g.strokeStyle = 'rgba(255,255,255,.07)'; g.fillStyle = '#8a93ad'; g.font = '11px sans-serif';
    for (let i = 0; i <= 4; i++) {
      const y = pad.t + ph * i / 4;
      g.beginPath(); g.moveTo(pad.l, y); g.lineTo(W - pad.r, y); g.stroke();
      g.textAlign = 'right'; g.fillText((tMax - (tMax - tMin) * i / 4).toFixed(0), pad.l - 6, y + 4);
      g.textAlign = 'left'; g.fillText((lMax - lMax * i / 4).toFixed(0), W - pad.r + 6, y + 4);
    }
    const x = (i) => pad.l + pw * i / (MAX_POINTS - 1);
    const line = (arr, lo, hi, color, fill) => {
      if (arr.length < 2) return;
      g.beginPath();
      arr.forEach((v, i) => { const y = pad.t + ph * (1 - (v - lo) / (hi - lo)); i ? g.lineTo(x(i), y) : g.moveTo(x(i), y); });
      g.strokeStyle = color; g.lineWidth = 2; g.stroke();
      if (fill) { g.lineTo(x(arr.length - 1), pad.t + ph); g.lineTo(x(0), pad.t + ph); g.fillStyle = fill; g.fill(); }
    };
    line(luxs, 0, lMax, '#ffd166', 'rgba(255,209,102,.08)');
    line(temps, tMin, tMax, '#ff7b72');
    g.fillStyle = '#8a93ad'; g.textAlign = 'center';
    const ticks = history.length > 40 ? [0, Math.floor(history.length / 2), history.length - 1] : [0, history.length - 1];
    if (history.length > 15) ticks.forEach((i) => g.fillText(history[i].t, x(i), H - 6));
  }

  // ---------------- 规则 / 日志 / 对话 ----------------
  function renderRules(rules) {
    $('#rules').innerHTML = rules.map((r) =>
      `<li data-rule="${r.id}"><span class="rid">${r.id}</span><div><b>${r.name}</b><br>${r.description}</div></li>`).join('');
  }
  function fireRule(id) {
    const li = $(`#rules li[data-rule=${id}]`);
    if (!li) return;
    li.classList.add('fired');
    setTimeout(() => li.classList.remove('fired'), 3000);
  }
  function addLog(ev) {
    const li = document.createElement('li');
    if (ev.level === 'error') li.className = 'error';
    li.innerHTML = `<time>${ev.sim_time}</time><span class="tag ${ev.stage}">${ev.stage}</span><span></span>`;
    li.lastChild.textContent = ev.text;
    const log = $('#log');
    log.appendChild(li);
    while (log.children.length > 150) log.firstChild.remove();
    log.scrollTop = log.scrollHeight;
  }
  function addMsg(kind, text) {
    const d = document.createElement('div');
    d.className = 'msg ' + kind;
    d.textContent = text;
    const chat = $('#chat');
    chat.appendChild(d);
    chat.scrollTop = chat.scrollHeight;
  }
  function addTrace(lines) {
    if (!lines.length) return;
    const d = document.createElement('div');
    d.className = 'trace';
    lines.forEach((l) => { const x = document.createElement('div'); x.textContent = l; d.appendChild(x); });
    $('#chat').appendChild(d);
  }
  const stageTimers = {};
  function flashStage(stage) {
    const el = $(`.stage[data-stage="${stage}"]`);
    if (!el) return;
    el.classList.add('on');
    clearTimeout(stageTimers[stage]);
    stageTimers[stage] = setTimeout(() => el.classList.remove('on'), 700);
  }
  function flashCard(dev) {
    const c = $('#card-' + dev);
    if (!c) return;
    c.classList.add('flash');
    setTimeout(() => c.classList.remove('flash'), 800);
  }
  let greetTimer;
  function showGreet(text) {
    const g = $('#greet');
    g.textContent = text;
    g.classList.add('show');
    clearTimeout(greetTimer);
    greetTimer = setTimeout(() => g.classList.remove('show'), 3500);
  }
  function speak(text) {
    if (!$('#tts').checked || !window.speechSynthesis) return;
    const u = new SpeechSynthesisUtterance(text);
    u.lang = 'zh-CN';
    speechSynthesis.cancel();
    speechSynthesis.speak(u);
  }

  // ---------------- 用户输入 ----------------
  function submitText(text, source = 'text') {
    text = text.trim();
    if (text) send({ type: 'text', text, source });
  }
  $('#chat-form').addEventListener('submit', (e) => {
    e.preventDefault();
    submitText($('#chat-input').value);
    $('#chat-input').value = '';
  });
  $$('#quick .chip').forEach((b) => b.addEventListener('click', () => submitText(b.textContent)));

  // 设备卡片控件
  $$('.dev input[type=checkbox]').forEach((cb) => cb.addEventListener('change', () =>
    send({ type: 'control', device: cb.dataset.dev, params: { on: cb.checked } })));
  $$('.dev input[type=range]').forEach((r) => {
    r.addEventListener('pointerdown', () => (r.dragging = true));
    r.addEventListener('change', () => {
      r.dragging = false;
      const params = { [r.dataset.key]: +r.value };
      if (r.dataset.dev !== 'curtain') params.on = true;
      send({ type: 'control', device: r.dataset.dev, params });
    });
  });
  $$('.dev .seg').forEach((seg) => $$('button', seg).forEach((b) => b.addEventListener('click', () => {
    const v = seg.dataset.key === 'speed' ? +b.dataset.val : b.dataset.val;
    send({ type: 'control', device: seg.dataset.dev, params: { [seg.dataset.key]: v, on: true } });
  })));
  $$('#card-ac [data-step]').forEach((b) => b.addEventListener('click', () =>
    send({ type: 'control', device: 'ac', params: { target: state.devices.ac.target + +b.dataset.step, on: true } })));
  $('#next-track').addEventListener('click', () => send({ type: 'control', device: 'speaker', params: { next: true, on: true } }));

  // 仿真控制
  $('#sim-enter').addEventListener('click', () =>
    send({ type: 'sim', action: 'presence', value: { present: true, person: $('#sim-person').value } }));
  $('#sim-leave').addEventListener('click', () => send({ type: 'sim', action: 'presence', value: { present: false } }));
  $$('[data-time]').forEach((b) => b.addEventListener('click', () => send({ type: 'sim', action: 'time', value: +b.dataset.time })));
  $$('[data-out]').forEach((b) => b.addEventListener('click', () => send({ type: 'sim', action: 'outdoor', value: +b.dataset.out })));
  $('#sim-speed').addEventListener('change', (e) => send({ type: 'sim', action: 'speed', value: +e.target.value }));
  $('#auto-toggle').addEventListener('change', (e) => send({ type: 'auto', enabled: e.target.checked }));

  // 语音识别（浏览器 Web Speech API，Chrome / Edge 支持）
  const SR = window.SpeechRecognition || window.webkitSpeechRecognition;
  if (SR) {
    const rec = new SR();
    rec.lang = 'zh-CN';
    rec.interimResults = true;
    let finalText = '';
    rec.onresult = (e) => {
      finalText = [...e.results].map((r) => r[0].transcript).join('');
      $('#chat-input').value = finalText;
    };
    rec.onend = () => {
      $('#mic').classList.remove('rec');
      if (finalText) submitText(finalText, 'voice');
      $('#chat-input').value = '';
      finalText = '';
    };
    rec.onerror = (e) => { $('#voice-tip').textContent = '语音识别出错：' + e.error; };
    $('#mic').addEventListener('click', () => {
      if ($('#mic').classList.contains('rec')) return rec.stop();
      $('#mic').classList.add('rec');
      $('#voice-tip').textContent = '正在聆听，说完自动发送…';
      rec.start();
    });
    $('#voice-tip').textContent = '支持 🎤 语音输入';
  } else {
    $('#mic').disabled = true;
    $('#voice-tip').textContent = '当前浏览器不支持语音识别，请使用文字';
  }

  window.addEventListener('resize', drawChart);
  // 演示/测试用：暴露少量接口
  window.SmartHome = { send, submitText, get state() { return state; } };
  connect();
})();
