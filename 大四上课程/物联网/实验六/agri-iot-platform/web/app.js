/* 网页端：WebSocket 接收实时快照，RESTful API 下发控制/故障模拟 */
const S = { data: null, gh: localStorageGet('gh') || 'GH-1', imgMode: 'ann', ctrlGh: null, camTs: 0 };
const $ = (id) => document.getElementById(id);
const C = {
  green: '#2f9e5b', blue: '#2f7fd6', orange: '#e08a1e', red: '#d9443a', purple: '#8a5cd6', teal: '#169c9c', gray: '#8c96a0',
};
const LEVEL = { critical: '严重', warning: '警告', info: '提示', ok: '恢复' };
const FAULTS = {
  sensor_abnormal: ['传感器数据异常', '超量程跳变 / 数据卡死'],
  sensor_offline: ['传感器中断', '停止上报土壤数据'],
  valve_no_response: ['灌溉阀门不响应', '忽略开关阀指令'],
  pest_false_alarm: ['虫情监测误报', '诱捕计数异常暴增'],
  drone_image_missing: ['无人机图像缺失', '图传丢失 / 图像损坏'],
  camera_offline: ['摄像头离线', '停止推送视频帧'],
};
const SPECIES = [{ k: 'whitefly', name: '白粉虱', color: C.orange }, { k: 'aphid', name: '蚜虫', color: C.green }, { k: 'thrips', name: '蓟马', color: C.purple }];
const GRADE_COLOR = { 健康: C.green, 轻度: '#c7b21c', 中度: C.orange, 重度: C.red };

function localStorageGet(k) { try { return localStorage.getItem(k); } catch (e) { return null; } }
function localStorageSet(k, v) { try { localStorage.setItem(k, v); } catch (e) { /* ignore */ } }
const fmt = (v, d = 1) => (v == null || Number.isNaN(v) ? '—' : Number(v).toFixed(d));
const t2s = (t) => (t ? new Date(t * 1000).toTimeString().slice(0, 8) : '—');
const esc = (s) => String(s ?? '').replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));

async function api(path, body) {
  const r = await fetch(path, { method: body ? 'POST' : 'GET', headers: { 'Content-Type': 'application/json' }, body: body ? JSON.stringify(body) : undefined });
  if (!r.ok) { const j = await r.json().catch(() => ({})); toast('warning', j.detail || `请求失败 ${r.status}`); throw new Error(r.status); }
  return r.json();
}

/* ---------------- WebSocket ---------------- */
function connect() {
  const ws = new WebSocket(`${location.protocol === 'https:' ? 'wss' : 'ws'}://${location.host}/ws`);
  ws.onopen = () => { $('conn').className = 'pill conn-on'; $('conn').textContent = '● 实时连接'; };
  ws.onmessage = (ev) => {
    const m = JSON.parse(ev.data);
    if (m.type === 'snapshot') { S.data = m.data; (m.data.events || []).forEach((e) => toast(e.level, e.msg)); render(); }
  };
  ws.onclose = () => { $('conn').className = 'pill conn-off'; $('conn').textContent = '● 重连中…'; setTimeout(connect, 2000); };
  setInterval(() => { if (ws.readyState === 1) ws.send('ping'); }, 15000);
}

function toast(level, msg) {
  const el = document.createElement('div');
  el.className = `toast ${level}`;
  el.textContent = msg;
  $('toasts').prepend(el);
  setTimeout(() => el.classList.add('out'), 5200);
  setTimeout(() => el.remove(), 5800);
  while ($('toasts').children.length > 5) $('toasts').lastChild.remove();
}

/* ---------------- 渲染 ---------------- */
function render() {
  const d = S.data; if (!d) return;
  const g = d.greenhouses[S.gh];
  renderHeader(d); renderTabs(d); renderEnv(g); renderIrr(g, d); renderPest(g); renderDrone(g); renderCam(g);
  renderAlarms(d); renderLinkage(d); renderFaults(d); renderDevices(g);
}

function renderHeader(d) {
  const h = d.sim.hour;
  $('simClock').textContent = h == null ? '模拟时刻 --:--' : `模拟时刻 ${String(Math.floor(h)).padStart(2, '0')}:${String(Math.floor((h % 1) * 60)).padStart(2, '0')} ${d.sim.daylight > 0 ? '☀' : '☾'}`;
  $('simState').textContent = d.sim.online ? '模拟器在线' : '模拟器离线';
  $('simState').className = 'pill ' + (d.sim.online ? 'conn-on' : 'conn-off');
  $('msgCount').textContent = `MQTT 消息 ${d.msg_count}`;
}

function ghAlarmLevel(d, gid) {
  const a = d.alarms.filter((x) => x.gh === gid);
  if (a.some((x) => x.level === 'critical')) return 'critical';
  if (a.some((x) => x.level === 'warning')) return 'warning';
  return a.length ? 'info' : 'ok';
}

function renderTabs(d) {
  $('ghTabs').innerHTML = Object.values(d.greenhouses).map((g) => {
    const s = g.soil.latest, irr = g.irrigation.status, dr = g.drone.latest;
    const lvl = ghAlarmLevel(d, g.id); const cnt = d.alarms.filter((x) => x.gh === g.id).length;
    return `<button class="gh-tab ${g.id === S.gh ? 'active' : ''}" data-gh="${g.id}">
      <div class="t1"><span class="dot ${lvl}"></span><b>${g.cfg.name}</b><span class="crop">${g.cfg.crop} · ${g.cfg.stage}</span>${cnt ? `<span class="badge ${lvl}">${cnt}</span>` : ''}</div>
      <div class="t2">
        <span>湿度 <b class="${s && s.valid && s.soil_moisture < g.low ? 'low' : ''}">${s && s.valid ? fmt(s.soil_moisture) + '%' : '<em>异常</em>'}</b></span>
        <span>阀门 <b class="${irr && irr.valve === 'open' ? 'on' : ''}">${irr ? (irr.valve === 'open' ? '开' : '关') : '—'}</b></span>
        <span>病情 <b style="color:${dr && dr.result ? GRADE_COLOR[dr.result.grade] : 'inherit'}">${dr ? (dr.result ? dr.result.grade : '失败') : '—'}</b></span>
        <span>摄像 <b>${g.camera.online ? '在线' : '离线'}</b></span>
      </div></button>`;
  }).join('');
}

function kpi(label, val, unit, cls = '', sub = '') {
  return `<div class="kpi ${cls}"><div class="k-l">${label}</div><div class="k-v">${val}<small>${unit}</small></div>${sub ? `<div class="k-s">${sub}</div>` : ''}</div>`;
}

function renderEnv(g) {
  const s = g.soil.latest, an = g.soil.analysis;
  $('envDevice').textContent = `${g.cfg.devices.soil} · 上报 ${t2s(s && s.ts)}${s ? ` · 电量 ${s.battery}% · RSSI ${s.rssi}dBm` : ''}`;
  if (!s) { $('envKpis').innerHTML = '<div class="empty">等待传感器数据…</div>'; return; }
  const bad = !s.valid;
  const mCls = bad ? 'bad' : s.soil_moisture < g.low ? 'warn' : s.soil_moisture > g.high ? 'info' : 'good';
  $('envKpis').innerHTML = [
    kpi('土壤湿度', fmt(s.soil_moisture), '%', mCls, `目标 ${g.low}~${g.high}%`),
    kpi('土壤温度', fmt(s.soil_temp), '℃', bad && (s.soil_temp > 50 || s.soil_temp < -10) ? 'bad' : ''),
    kpi('土壤 EC', fmt(s.ec, 2), 'mS/cm'),
    kpi('空气温度', fmt(s.air_temp), '℃'),
    kpi('空气湿度', fmt(s.air_humidity), '%', s.air_humidity > 85 ? 'warn' : ''),
    kpi('光照', fmt(s.light / 1000, 1), 'klx'),
    kpi('CO₂', fmt(s.co2, 0), 'ppm'),
  ].join('');
  const ser = g.soil.series; const valid = ser.filter((x) => x.valid);
  Charts.line($('chartMoisture'), {
    height: 200, band: { y0: g.low, y1: g.high },
    series: [
      { name: '土壤湿度', color: C.blue, data: ser.map((x) => [x.ts, x.valid ? x.m : null]), area: true },
      { name: '移动平均(5)', color: C.orange, dash: '5 4', width: 1.6, data: valid.map((x, i) => [x.ts, g.soil.ma[i]]) },
    ],
    hlines: [{ y: g.low, color: C.red, label: `下限 ${g.low}%` }, { y: g.high, color: C.teal, label: `上限 ${g.high}%` }],
    points: ser.filter((x) => !x.valid).map((x) => ({ t: x.ts, color: C.red })),
    marks: g.irrigation.events.flatMap((e) => [{ t: e.start_ts, color: C.green }].concat(e.end_ts ? [{ t: e.end_ts, color: C.gray }] : [])),
    legendExtra: `<span><i style="background:${C.red}"></i>异常数据(已剔除)</span><span><i style="background:${C.green}"></i>开阀时刻</span>`,
  });
  Charts.line($('chartClimate'), {
    height: 170,
    series: [
      { name: '空气温度℃', color: C.red, data: valid.map((x) => [x.ts, x.at]) },
      { name: '土壤温度℃', color: C.orange, data: valid.map((x) => [x.ts, x.st]), dash: '4 3' },
      { name: '空气湿度%(右轴)', color: C.teal, axis: 'right', data: valid.map((x) => [x.ts, x.ah]) },
    ],
  });
  if (!an) { $('envAnalysis').innerHTML = '<div class="empty">有效数据不足，暂无分析</div>'; return; }
  $('envAnalysis').innerHTML = `
    <div class="an-grid">
      <div><span>湿度 均值/最小/最大</span><b>${an.soil_moisture.avg} / ${an.soil_moisture.min} / ${an.soil_moisture.max}%</b></div>
      <div><span>湿度标准差</span><b>${an.soil_moisture.std}</b></div>
      <div><span>趋势斜率（线性回归）</span><b class="${an.moisture_slope < 0 ? 'down' : 'up'}">${an.moisture_slope > 0 ? '+' : ''}${an.moisture_slope}%/min</b></div>
      <div><span>阈值内占比</span><b>${an.moisture_in_range}%</b></div>
      <div><span>气温 均值</span><b>${an.air_temp.avg}℃</b></div>
      <div><span>VPD 饱和水汽压差</span><b>${an.vpd} kPa</b></div>
      <div><span>病害风险指数</span><b class="${an.disease_risk > 50 ? 'down' : ''}">${an.disease_risk}%</b></div>
      <div><span>统计样本</span><b>${an.n} 条 / ${an.window_s}s</b></div>
    </div>
    <p class="concl">📈 ${esc(an.trend_text)}；VPD ${an.vpd} kPa：${esc(an.vpd_text)}。${s.valid ? '' : `<span class="bad-t">当前读数异常：${esc(s.problems.join('；'))}</span>`}</p>
    <p class="method">方法：最近 60 条有效数据求均值/极值/标准差；最近 20 条做最小二乘线性回归得到趋势斜率与 R²，并外推到达阈值时间；VPD 由 Tetens 公式计算；病害风险 = 高湿(RH&gt;85%)且 15~28℃ 的样本占比。超量程或连续 ${6} 次相同读数判为异常并剔除。</p>`;
}

function renderIrr(g, d) {
  const st = g.irrigation.status, an = g.irrigation.analysis;
  $('irrDevice').textContent = `${g.cfg.devices.valve} · 水泵 PUMP-01 · ${g.cfg.area_m2} m²`;
  const open = st && st.valve === 'open';
  const pend = g.irrigation.pending;
  $('irrState').innerHTML = st ? `
    <div class="valve ${open ? 'open' : ''} ${g.irrigation.cmd_fail ? 'fail' : ''}">
      <svg viewBox="0 0 120 60"><rect x="0" y="24" width="120" height="12" rx="3" class="pipe"/><path d="M45 12 L75 48 M75 12 L45 48" class="vx"/><rect x="52" y="2" width="16" height="10" class="vh"/>
      ${open ? '<circle cx="100" cy="30" r="3" class="flow f1"/><circle cx="85" cy="30" r="3" class="flow f2"/><circle cx="20" cy="30" r="3" class="flow f3"/>' : ''}</svg>
      <div class="v-t">${g.irrigation.cmd_fail ? '⚠ 控制失败' : open ? '阀门开启 · 灌溉中' : '阀门关闭'}</div>
    </div>
    <div class="irr-nums">
      <div><span>瞬时流量</span><b>${fmt(st.flow_lpm)} L/min</b></div>
      <div><span>管道压力</span><b>${fmt(st.pressure_kpa, 0)} kPa</b></div>
      <div><span>累计用水</span><b>${fmt(an.water_used_l)} L</b></div>
      <div><span>控制模式</span><b>${g.auto ? '自动联动' : '手动'}</b></div>
    </div>` : '<div class="empty">等待阀门状态…</div>';
  if (S.ctrlGh !== g.id || document.activeElement.tagName !== 'INPUT') {
    $('autoMode').checked = g.auto;
    if (document.activeElement !== $('thrLow')) $('thrLow').value = g.low;
    if (document.activeElement !== $('thrHigh')) $('thrHigh').value = g.high;
    S.ctrlGh = g.id;
  }
  $('irrPending').innerHTML = pend.length ? pend.map((c) => `<span class="spin"></span>指令 ${c.cmd_id}（${c.action === 'open' ? '开阀' : '关阀'}）等待设备应答${c.retries ? `，已重发 ${c.retries} 次` : ''}…`).join('<br>') : '';
  const ws = g.irrigation.water_series;
  Charts.line($('chartWater'), {
    height: 150,
    series: [
      { name: '累计用水 L', color: C.blue, data: ws.map((x) => [x.ts, x.total]), area: true },
      { name: '流量 L/min(右轴)', color: C.green, axis: 'right', data: ws.map((x) => [x.ts, x.flow]), yRight: [0, 25] },
    ],
    yRight: [0, 25],
  });
  const totalAll = Object.values(d.greenhouses).reduce((a, x) => a + x.irrigation.analysis.water_used_l, 0);
  $('irrAnalysis').innerHTML = `
    <div class="an-grid">
      <div><span>本棚累计用水</span><b>${an.water_used_l} L</b></div>
      <div><span>单位面积用水</span><b>${an.water_per_m2} L/m²</b></div>
      <div><span>灌溉次数</span><b>${an.events} 次</b></div>
      <div><span>平均时长 / 水量</span><b>${an.avg_duration_s}s / ${an.avg_liters}L</b></div>
      <div><span>灌溉效率</span><b>${an.efficiency == null ? '—' : an.efficiency + ' %/100L'}</b></div>
      <div><span>基地总用水 / 本棚占比</span><b>${fmt(totalAll, 0)} L / ${totalAll ? fmt((an.water_used_l / totalAll) * 100, 0) : 0}%</b></div>
    </div>
    <p class="method">方法：流量计累计读数差分得到用水量；以开阀→关阀为一次灌溉事件，统计次数、时长与水量；灌溉效率 = 灌溉前后土壤湿度提升量 / 用水量 ×100。时间按 1s=${120}s 加速折算。</p>`;
  const cmds = d.commands.filter((c) => c.gh === g.id).slice(0, 6);
  const stText = { done: '✔ 已执行', pending: '… 等待应答', retrying: '↻ 重发中', failed: '✖ 无响应' };
  $('cmdTable').innerHTML = `<thead><tr><th>时间</th><th>指令</th><th>来源</th><th>控制结果</th><th>时延</th></tr></thead><tbody>${cmds.map((c) => `
    <tr><td>${t2s(c.sent_ts)}</td><td>${c.action === 'open' ? '开阀' : '关阀'}${c.duration ? ` ${c.duration}s` : ''}</td><td>${{ auto: '自动联动', manual: '手动', 'auto-safety': '安全保护' }[c.source] || c.source}</td>
    <td class="st-${c.status}">${stText[c.status]}</td><td>${c.latency_ms != null ? c.latency_ms + 'ms' : '—'}</td></tr>`).join('') || '<tr><td colspan="5" class="muted">暂无控制指令</td></tr>'}</tbody>`;
}

function renderPest(g) {
  const an = g.pest.analysis;
  $('pestDevice').textContent = `${g.cfg.devices.pest} · 黄板+诱虫灯 · 每 5s 上报`;
  Charts.stacked($('chartPest'), {
    height: 180, keys: SPECIES, rows: g.pest.series,
    flagColors: { suspect: C.orange, pending: C.orange, confirmed: C.red, false_alarm: C.gray },
  });
  if (!an) { $('pestAnalysis').innerHTML = '<div class="empty">等待虫情数据…</div>'; return; }
  const rows = SPECIES.map((sp) => {
    const x = an.species[sp.k];
    const gr = x.growth == null ? '—' : `<b class="${x.growth > 0 ? 'down' : 'up'}">${x.growth > 0 ? '+' : ''}${x.growth}%</b>`;
    return `<tr><td><i class="sw" style="background:${sp.color}"></i>${sp.name}</td><td>${an.latest[sp.k]}</td><td>${x.recent}</td><td>${x.prev ?? '—'}</td><td>${gr}</td><td>${x.total}</td></tr>`;
  }).join('');
  const pend = g.pest.pending;
  $('pestAnalysis').innerHTML = `
    <table class="tbl"><thead><tr><th>虫种</th><th>最新</th><th>近30s</th><th>前30s</th><th>增长率</th><th>累计</th></tr></thead><tbody>${rows}</tbody></table>
    <div class="an-grid">
      <div><span>白粉虱基线（前12次均值）</span><b>${an.baseline}</b></div>
      <div><span>当前 z-score</span><b class="${an.zscore > 3 ? 'down' : ''}">${an.zscore}</b></div>
      <div><span>总数 3 点移动平均</span><b>${an.ma}</b></div>
      <div><span>异常判定</span><b class="${an.anomaly ? 'down' : 'up'}">${an.anomaly ? '异常' : '正常'}</b></div>
    </div>
    ${pend ? `<p class="concl warn-t">🚁 计数突增（${pend.count} 头，z=${pend.z}），已联动无人机任务 ${pend.mission_id} 核查中…</p>` : ''}
    <p class="method">方法：按 30s 窗口汇总各虫种数量并与上一窗口比较得到增长率；白粉虱以前 12 次计数（剔除已判误报点）为基线计算 z-score，z&gt;3 且超出基线 12 头判为异常，并自动派无人机拍照核查真伪。</p>`;
}

function renderDrone(g) {
  const L = g.drone.latest, stt = g.drone.stats;
  if (!L) { $('droneView').innerHTML = '<div class="empty tall">等待无人机巡检（每 25s 轮巡一个大棚）…</div>'; $('droneAnalysis').innerHTML = ''; $('droneThumbs').innerHTML = ''; return; }
  const reason = { routine: '例行巡检', manual: '手动派发', pest_verify: '虫情核查', retry: '故障重试' };
  if (L.status === 'ok') {
    const src = S.imgMode === 'ann' ? L.ann : L.raw;
    if (!$('droneImg') || $('droneImg').dataset.src !== src) {
      $('droneView').innerHTML = `<img id="droneImg" data-src="${src}" src="${src}" alt="无人机图像"><div class="img-tag">${S.imgMode === 'ann' ? '识别标注图' : '原始图像'} · ${reason[L.reason] || L.reason} · ${t2s(L.ts)}</div>`;
    }
  } else {
    $('droneView').innerHTML = `<div class="img-fail"><div class="big">⚠</div><div>任务 ${esc(L.mission_id)} 识别失败</div><div class="muted">${esc(L.error)}</div><div class="muted">系统将自动重新派发巡检</div></div>`;
  }
  const r = L.result;
  const dist = ['健康', '轻度', '中度', '重度'].map((k) => ({ label: k, v: stt.grade_dist[k] || 0, color: GRADE_COLOR[k] }));
  $('droneAnalysis').innerHTML = `
    ${r ? `<div class="result-line"><span class="grade" style="background:${GRADE_COLOR[r.grade]}">${r.grade}</span>
      病斑 <b>${r.lesion_count}</b> 处 · 白粉虱 <b>${r.whitefly_clusters}</b> 簇 · 病斑面积占比 <b>${r.lesion_ratio}%</b> · 冠层覆盖 <b>${r.leaf_cover}%</b> · 置信度 <b>${r.confidence}</b></div>` : ''}
    <div class="drone-stats">
      <div>${Charts.bars(dist)}</div>
      <div class="an-grid one">
        <div><span>任务数 / 成功率</span><b>${stt.missions} / ${stt.success_rate ?? '—'}%</b></div>
        <div><span>病斑数趋势</span>${Charts.spark(stt.lesion_trend, C.red)}</div>
        <div><span>白粉虱簇趋势</span>${Charts.spark(stt.fly_trend, C.orange)}</div>
      </div>
    </div>
    <p class="method">方法：HSV 颜色分割（绿色冠层/褐色病斑/白色虫体）+ 8 邻域连通域分析，按形状过滤噪声；评分 = 病斑数 + 0.8×白粉虱簇数，分为健康/轻度/中度/重度。无人机 ${L.result ? '' : ''}电量 ${L.battery ?? '—'}%。</p>`;
  $('droneThumbs').innerHTML = g.drone.history.map((h) => `
    <div class="thumb ${h.status !== 'ok' ? 'fail' : ''}" ${h.ann ? `data-img="${h.ann}"` : ''}>
      ${h.ann ? `<img src="${h.ann}" loading="lazy" alt="">` : '<div class="nf">✖</div>'}
      <span>${t2s(h.ts)} ${h.result ? h.result.grade : '失败'}</span></div>`).join('');
}

function renderCam(g) {
  const cam = g.camera;
  if (cam.last_ts && cam.last_ts !== S.camTs && S.camGh === g.id) {
    S.camTs = cam.last_ts;
    const im = new Image();
    im.onload = () => { $('camImg').src = im.src; };
    im.src = `/api/camera/${g.id}/latest.jpg?t=${cam.last_ts}`;
  } else if (S.camGh !== g.id) {
    S.camGh = g.id; S.camTs = 0; $('camImg').removeAttribute('src');
    if (cam.last_ts) $('camImg').src = `/api/camera/${g.id}/latest.jpg?t=${cam.last_ts}`;
  }
  $('camOverlay').classList.toggle('hidden', cam.online);
  $('camOverlay').innerHTML = cam.online ? '' : `<div>📷 ${g.cfg.devices.camera} 离线</div><small>最后画面 ${t2s(cam.last_ts)} · 等待恢复</small>`;
  $('camView').classList.toggle('offline', !cam.online);
  Charts.line($('chartCam'), {
    height: 110,
    series: [{ name: '变化率%', color: C.purple, data: cam.series.map((x) => [x.ts, x.change]), area: true },
      { name: '亮度(右轴)', color: C.gray, axis: 'right', dash: '3 3', width: 1.2, data: cam.series.map((x) => [x.ts, x.bright]) }],
    hlines: [{ y: 1.2, color: C.orange, label: '运动阈值' }],
    marks: cam.snapshots.filter((s) => s.kind === 'motion').map((s) => ({ t: s.ts, color: C.orange })),
  });
  const st = cam.stats;
  $('camAnalysis').innerHTML = `<div class="an-grid">
      <div><span>状态</span><b class="${cam.online ? 'up' : 'down'}">${cam.online ? '在线' : '离线'}</b></div>
      <div><span>在线率</span><b>${st.online_rate}%</b></div>
      <div><span>运动事件</span><b>${st.motion_events} 次</b></div>
      <div><span>近 1 分钟平均变化</span><b>${st.avg_change}%</b></div>
    </div>
    <p class="method">方法：帧下采样为 160×90 灰度图，逐像素差分并扣除中位数（抵消昼夜亮度漂移），变化像素占比 &gt;1.2% 视为运动并自动抓拍；在线率 = 实收帧数 / 应收帧数。</p>`;
  $('camSnaps').innerHTML = cam.snapshots.map((s) => `<div class="thumb" data-img="${s.img}"><img src="${s.img}" loading="lazy" alt=""><span>${t2s(s.ts)} ${s.kind === 'manual' ? '手动' : '运动 ' + s.change + '%'}</span></div>`).join('') || '<div class="muted">暂无抓拍记录</div>';
}

function alarmItem(a) {
  return `<div class="alarm ${a.status === 'active' ? a.level : 'recovered'}">
    <div class="a-h"><span class="lv">${a.status === 'active' ? LEVEL[a.level] : '已恢复'}</span><b>${a.gh} ${esc(a.title)}</b><span class="tm">${t2s(a.start_ts)}${a.end_ts ? ' → ' + t2s(a.end_ts) : ''}</span></div>
    <div class="a-m">${esc(a.msg)}</div>
    ${a.recover_msg ? `<div class="a-r">✔ ${esc(a.recover_msg)}</div>` : ''}</div>`;
}

function renderAlarms(d) {
  $('alarmCount').textContent = d.alarms.length;
  $('alarmCount').className = 'badge ' + (d.alarms.some((a) => a.level === 'critical') ? 'critical' : d.alarms.length ? 'warning' : 'ok');
  $('alarmActive').innerHTML = d.alarms.map(alarmItem).join('') || '<div class="all-ok">✔ 当前无活动告警，系统运行正常</div>';
  $('alarmHist').innerHTML = d.alarm_hist.filter((a) => a.status !== 'active').map(alarmItem).join('') || '<div class="muted">暂无记录</div>';
}

function renderLinkage(d) {
  $('linkLog').innerHTML = d.linkage.map((l) => `<div class="li"><span class="tm">${t2s(l.ts)}</span><span class="tag">${esc(l.rule)}</span>${esc(l.msg)}</div>`).join('') || '<div class="muted">暂无联动记录</div>';
}

function renderFaults(d) {
  $('faultGh').textContent = `作用于 ${S.gh}`;
  const active = (d.sim.faults || {})[S.gh] || [];
  const html = Object.entries(FAULTS).map(([k, [name, desc]]) => `
    <label class="fault ${active.includes(k) ? 'on' : ''}"><input type="checkbox" data-fault="${k}" ${active.includes(k) ? 'checked' : ''}>
      <span class="f-n">${name}</span><span class="f-d">${desc}</span></label>`).join('');
  if ($('faultPanel').dataset.sig !== S.gh + active.join()) { $('faultPanel').innerHTML = html; $('faultPanel').dataset.sig = S.gh + active.join(); }
}

function renderDevices(g) {
  const T = { online: '在线', offline: '离线', fault: '故障', unknown: '未知' };
  $('devList').innerHTML = g.devices.map((x) => `<div class="dev"><span class="dot ${x.status}"></span><b>${x.id}</b><span>${x.type}</span><em class="${x.status}">${T[x.status]}</em></div>`).join('');
}

/* ---------------- 交互 ---------------- */
document.addEventListener('click', async (e) => {
  const tab = e.target.closest('.gh-tab');
  if (tab) { S.gh = tab.dataset.gh; localStorageSet('gh', S.gh); render(); return; }
  const th = e.target.closest('[data-img]');
  if (th) { $('lbImg').src = th.dataset.img; $('lightbox').classList.remove('hidden'); return; }
  if (e.target.closest('#lightbox')) { $('lightbox').classList.add('hidden'); return; }
  const sc = e.target.closest('[data-scen]');
  if (sc) { await api('/api/scenario', { gh: S.gh, name: sc.dataset.scen }); return; }
});
document.addEventListener('change', async (e) => {
  if (e.target.dataset.fault) await api('/api/fault', { gh: S.gh, fault: e.target.dataset.fault, on: e.target.checked });
});
$('autoMode').onchange = () => api(`/api/irrigation/${S.gh}/mode`, { auto: $('autoMode').checked });
$('saveThr').onclick = () => api(`/api/irrigation/${S.gh}/mode`, { low: +$('thrLow').value, high: +$('thrHigh').value }).then(() => toast('ok', '阈值已保存'));
$('btnOpen').onclick = () => api(`/api/irrigation/${S.gh}/command`, { action: 'open' });
$('btnClose').onclick = () => api(`/api/irrigation/${S.gh}/command`, { action: 'close' });
$('btnOpen30').onclick = () => api(`/api/irrigation/${S.gh}/command`, { action: 'open', duration: 20 });
$('btnDrone').onclick = () => api(`/api/drone/${S.gh}/mission`, {});
$('btnSnap').onclick = () => api(`/api/camera/${S.gh}/snapshot`, {}).then(() => toast('ok', '抓拍成功'));
$('imgToggle').onclick = () => { S.imgMode = S.imgMode === 'ann' ? 'raw' : 'ann'; $('imgToggle').textContent = S.imgMode === 'ann' ? '看原图' : '看标注'; render(); };
window.addEventListener('resize', () => render());

connect();
