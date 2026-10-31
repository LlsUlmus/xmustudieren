/* 绿源零碳产业园 能源互联网监测平台 —— 前端可视化 */
const $ = (s) => document.querySelector(s);
const $$ = (s) => [...document.querySelectorAll(s)];
const COLOR = { load: '#3fd0ff', pv: '#36e2a0', grid: '#ffc43d', ess: '#b07cff', red: '#ff4d6d', orange: '#ff8a3d', muted: '#7f9bbb' };
const PALETTE = ['#3fd0ff', '#36e2a0', '#ffc43d', '#b07cff', '#ff8a3d', '#ff6fb5', '#5b8cff', '#9be15d', '#ff4d6d'];
const STATE_TXT = { normal: '正常', standby: '待机', alarm: '告警', offline: '中断' };
const ESS_TXT = { charge: '充电中', discharge: '放电中', idle: '静置' };
const TYPE_TXT = { overload: '功率超限', jump: '功率突变', offline: '数据中断', pue_high: 'PUE超限', pv_low: '光伏异常', soc_low: 'SOC过低' };

let S = null;               // 最新快照
let view = 'board';
let rtMode = 'park', pieMode = 'dev', alarmFilter = 'all';
let detailId = null;
const charts = {};

const fmt = (v, n = 1) => (v === null || v === undefined || isNaN(v)) ? '--' : Number(v).toLocaleString('zh-CN', { maximumFractionDigits: n, minimumFractionDigits: n });
const chg = (v) => v === null || v === undefined ? '' : `<span class="${v >= 0 ? 'up' : 'down'}">${v >= 0 ? '▲' : '▼'}${Math.abs(v).toFixed(1)}%</span>`;

function chart(id) {
  if (!charts[id]) charts[id] = echarts.init(document.getElementById(id), null, { renderer: 'canvas' });
  return charts[id];
}
const axisStyle = {
  axisLine: { lineStyle: { color: 'rgba(127,155,187,.4)' } }, axisLabel: { color: COLOR.muted, fontSize: 10 },
  splitLine: { lineStyle: { color: 'rgba(127,155,187,.12)' } },
};
const baseOpt = (extra = {}) => Object.assign({
  backgroundColor: 'transparent', animationDuration: 300, animationDurationUpdate: 300,
  textStyle: { color: '#d8e9ff' },
  tooltip: { trigger: 'axis', backgroundColor: 'rgba(6,20,36,.95)', borderColor: '#3fd0ff', textStyle: { color: '#d8e9ff' } },
  legend: { top: 0, textStyle: { color: '#d8e9ff', fontSize: 11 }, itemWidth: 12, itemHeight: 8 },
  grid: { left: 46, right: 16, top: 30, bottom: 24 },
}, extra);
const hours = [...Array(24).keys()].map(h => `${String(h).padStart(2, '0')}时`);

/* ============================ 数据连接 ============================ */
function connect() {
  const es = new EventSource('/api/stream');
  es.onopen = () => { $('#conn').className = 'conn on'; $('#conn').textContent = '● 实时连接'; };
  es.onerror = () => { $('#conn').className = 'conn off'; $('#conn').textContent = '● 连接中断，重连中'; };
  es.onmessage = (e) => {
    const d = JSON.parse(e.data);
    if (!d.ready) return;
    S = d; render();
  };
}

function render() {
  renderHeader();
  if (view === 'board') renderBoard();
  if (view === 'analysis') renderAnalysis();
  if (view === 'alarm') renderAlarmCenter();
  if (detailId) renderDetail();
}

function renderHeader() {
  $('#simTime').textContent = S.sim_time;
  const p = S.period;
  $('#periodTag').innerHTML = `${p.name}电价 ${p.price.toFixed(2)} 元/kWh`;
  $('#periodTag').style.cssText = `background:${p.color}33;border:1px solid ${p.color};color:${p.color}`;
  if (!S.sim_online) { $('#conn').className = 'conn off'; $('#conn').textContent = '● 模拟端离线'; }
  else { $('#conn').className = 'conn on'; $('#conn').textContent = '● 实时连接'; }
  const n = S.alarms.active.length;
  $('#alarmBadge').textContent = n; $('#alarmBadge').classList.toggle('hide', n === 0);
  const severe = S.alarms.active.filter(a => !a.acked);
  const bn = $('#alarmBanner');
  if (severe.length) {
    bn.classList.remove('hide');
    bn.innerHTML = `⚠ 当前 ${S.alarms.active.length} 条告警未恢复：` + severe.slice(0, 3).map(a => `【${a.level}】${a.name} ${a.msg}`).join('　｜　');
  } else bn.classList.add('hide');
}

/* ============================ 实时看板 ============================ */
function renderBoard() {
  const k = S.kpi, e = S.stats.energy;
  const tiles = [
    ['实时总负荷', fmt(k.load), 'kW', `负荷率 ${fmt(k.load_rate)}%`, COLOR.load],
    ['光伏实时出力', fmt(k.pv), 'kW', `今日发电 ${fmt(e.pv, 0)} kWh`, COLOR.pv],
    ['电网交互功率', fmt(k.grid), 'kW', k.grid >= 0 ? '购电' : '上网', COLOR.grid],
    ['储能功率', fmt(k.ess), 'kW', `SOC ${fmt(k.soc)}% · ${ESS_TXT[S.devices.find(d => d.id === 'ess').status]}`, COLOR.ess],
    ['今日用电量', fmt(e.load, 0), 'kWh', `较昨日同期 ${chg(e.load_change)}`, '#fff'],
    ['绿电占比', fmt(e.green_ratio), '%', `光伏自用率 ${fmt(e.pv_self_use)}%`, COLOR.pv],
    ['今日电费', fmt(e.cost, 0), '元', `光储节省 ${fmt(e.saving, 0)} 元`, COLOR.grid],
    ['今日碳排放', fmt(e.carbon / 1000, 2), 'tCO₂', `光伏减排 ${fmt(e.carbon_avoided / 1000, 2)} t`, '#ff9fb1'],
  ];
  $('#kpis').innerHTML = tiles.map(t => `<div class="kpi"><div class="t">${t[0]}</div><div class="v" style="color:${t[4]}">${t[1]}<small>${t[2]}</small></div><div class="sub">${t[3]}</div></div>`).join('');

  gauge('gLoad', '总负荷 kW', k.load, k.total_rated, [[0.6, '#36e2a0'], [0.8, '#ffc43d'], [1, '#ff4d6d']], 0);
  gauge('gPue', '数据中心 PUE', k.pue, 2.0, [[0.6, '#36e2a0'], [0.8, '#ffc43d'], [1, '#ff4d6d']], 2, 1.0);
  gauge('gSoc', '储能 SOC %', k.soc, 100, [[0.15, '#ff4d6d'], [0.4, '#ffc43d'], [1, '#36e2a0']], 1);

  // 电价时段迷你图
  const per = S.stats.periods;
  chart('cPeriodMini').setOption(baseOpt({
    grid: { left: 50, right: 50, top: 30, bottom: 24 },
    xAxis: { type: 'category', data: per.map(p => `${p.name}(${p.price})`), ...axisStyle },
    yAxis: [{ type: 'value', name: 'kWh', nameTextStyle: { color: COLOR.muted }, ...axisStyle }, { type: 'value', name: '元', nameTextStyle: { color: COLOR.muted }, ...axisStyle, splitLine: { show: false } }],
    series: [
      { name: '用电量', type: 'bar', barWidth: 22, data: per.map(p => ({ value: p.load, itemStyle: { color: p.color } })) },
      { name: '电网电费', type: 'line', yAxisIndex: 1, data: per.map(p => p.cost), itemStyle: { color: '#fff' }, symbolSize: 7, label: { show: true, color: '#fff', fontSize: 10 } },
    ],
  }));

  renderRealtime();
  renderDevices();
  renderFlow();
  renderPie();
  renderRank();
  renderAlarmList($('#alarmList'), S.alarms.recent.slice(0, 30));
  $('#alarmCount').textContent = `未恢复 ${S.alarms.active.length} / 未确认 ${S.alarms.unacked}`;
}

function gauge(id, name, v, max, colors, digits, min = 0) {
  chart(id).setOption({
    series: [{
      type: 'gauge', min, max, radius: '92%', center: ['50%', '58%'], startAngle: 210, endAngle: -30, splitNumber: 4,
      axisLine: { lineStyle: { width: 9, color: colors } },
      pointer: { width: 4, length: '58%', itemStyle: { color: '#d8e9ff' } },
      axisTick: { distance: -9, length: 4, lineStyle: { color: '#fff', width: 1 } },
      splitLine: { distance: -9, length: 9, lineStyle: { color: '#fff', width: 1 } },
      axisLabel: { color: COLOR.muted, distance: 12, fontSize: 9, formatter: (x) => digits >= 2 ? x.toFixed(1) : Math.round(x) },
      title: { offsetCenter: [0, '78%'], color: COLOR.muted, fontSize: 11 },
      detail: { valueAnimation: true, offsetCenter: [0, '40%'], fontSize: 17, color: '#fff', formatter: (x) => x.toFixed(digits) },
      data: [{ value: v ?? 0, name }],
    }],
  });
}

function periodAreas(ts) {
  // 在实时曲线背景上标注电价时段
  const hp = S.config.hour_period, tf = S.config.tariff, areas = [];
  let start = 0;
  for (let i = 1; i <= ts.length; i++) {
    const a = hp[parseInt(ts[start])], b = i < ts.length ? hp[parseInt(ts[i])] : null;
    if (a !== b) {
      if (a === 'sharp' || a === 'peak' || a === 'valley')
        areas.push([{ xAxis: ts[start], itemStyle: { color: tf[a].color + (a === 'valley' ? '14' : '1c') }, label: { show: true, formatter: tf[a].name, color: tf[a].color, fontSize: 10, position: 'insideTop' } }, { xAxis: ts[i - 1] }]);
      start = i;
    }
  }
  return areas;
}

function renderRealtime() {
  const r = S.realtime;
  let series;
  if (rtMode === 'park') {
    series = [
      { name: '园区总负荷', type: 'line', data: r.load, smooth: true, showSymbol: false, lineStyle: { width: 2 }, itemStyle: { color: COLOR.load },
        areaStyle: { color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{ offset: 0, color: 'rgba(63,208,255,.35)' }, { offset: 1, color: 'rgba(63,208,255,0)' }]) },
        markArea: { silent: true, data: periodAreas(r.ts) } },
      { name: '光伏出力', type: 'line', data: r.pv, smooth: true, showSymbol: false, itemStyle: { color: COLOR.pv },
        areaStyle: { color: 'rgba(54,226,160,.18)' } },
      { name: '电网功率', type: 'line', data: r.grid, smooth: true, showSymbol: false, itemStyle: { color: COLOR.grid }, lineStyle: { type: 'dashed' } },
      { name: '储能功率(+放/-充)', type: 'bar', data: r.ess, itemStyle: { color: COLOR.ess, opacity: .6 }, barWidth: '60%' },
    ];
  } else {
    series = S.devices.filter(d => d.kind === 'load').map((d, i) => ({
      name: d.name, type: 'line', data: r.dev[d.id], smooth: true, showSymbol: false, connectNulls: false,
      itemStyle: { color: PALETTE[i] }, lineStyle: { width: 1.6 },
    }));
  }
  chart('cRealtime').setOption(baseOpt({
    grid: { left: 50, right: 16, top: 34, bottom: 46 },
    xAxis: { type: 'category', data: r.ts, boundaryGap: false, ...axisStyle },
    yAxis: { type: 'value', name: 'kW', nameTextStyle: { color: COLOR.muted }, ...axisStyle },
    dataZoom: [{ type: 'inside' }, { type: 'slider', height: 14, bottom: 6, borderColor: 'transparent', textStyle: { color: COLOR.muted } }],
    series,
  }), { replaceMerge: ['series'] });
}

function renderDevices() {
  $('#devices').innerHTML = S.devices.map(d => {
    let sub = `今日 ${fmt(d.today_kwh, 0)} kWh`;
    let unit = 'kW';
    if (d.kind === 'pv') sub = `辐照 ${fmt(d.extra.irradiance, 0)} W/m² · 今日 ${fmt(d.today_kwh, 0)} kWh`;
    if (d.kind === 'ess') sub = `SOC ${fmt((d.extra.soc || 0) * 100)}% · ${ESS_TXT[d.status]}`;
    const rate = Math.min(100, d.kind === 'ess' ? (d.extra.soc || 0) * 100 : d.rate);
    const th = d.threshold ? ` · 阈值 ${d.threshold}` : '';
    return `<div class="dev ${d.state}" data-id="${d.id}">
      <div class="n"><i class="s ${d.state}"></i>${d.name}<span class="g">${d.group}</span></div>
      <div class="p">${d.state === 'offline' ? '<span style="color:#6c7a8c">-- </span>' : fmt(d.power)}<small> ${unit} · ${STATE_TXT[d.state]}</small></div>
      <div class="m"><span>${sub}</span><span>${d.kind === 'ess' ? '' : '负荷率 ' + fmt(d.rate, 0) + '%'}</span></div>
      <div class="bar"><i style="width:${rate}%"></i></div>
      <div class="msg">${d.alarm_msgs.join('；') || `<span style="color:#7f9bbb">额定 ${d.rated} kW${th}</span>`}</div>
    </div>`;
  }).join('');
}

function renderFlow() {
  const k = S.kpi, nodes = [], links = [], dv = S.devices;
  const add = (name, color) => { if (!nodes.find(n => n.name === name)) nodes.push({ name, itemStyle: { color } }); };
  const BUS = '10kV 母线';
  add(BUS, '#5b8cff');
  if (k.grid > 0.5) { add('电网购电', COLOR.grid); links.push({ source: '电网购电', target: BUS, value: k.grid }); }
  if (k.pv > 0.5) { add('屋顶光伏', COLOR.pv); links.push({ source: '屋顶光伏', target: BUS, value: k.pv }); }
  if (k.ess > 0.5) { add('储能放电', COLOR.ess); links.push({ source: '储能放电', target: BUS, value: k.ess }); }
  if (k.ess < -0.5) { add('储能充电', COLOR.ess); links.push({ source: BUS, target: '储能充电', value: -k.ess }); }
  if (k.grid < -0.5) { add('余电上网', COLOR.grid); links.push({ source: BUS, target: '余电上网', value: -k.grid }); }
  const groups = {};
  dv.filter(d => d.kind === 'load').forEach((d, i) => {
    const p = Math.max(d.power || 0, 0.01);
    groups[d.group] = (groups[d.group] || 0) + p;
    add(d.name, d.state === 'alarm' ? COLOR.red : d.state === 'offline' ? '#6c7a8c' : PALETTE[i]);
    links.push({ source: d.group, target: d.name, value: p });
  });
  Object.entries(groups).forEach(([g, v]) => { add(g, '#3fd0ff'); links.push({ source: BUS, target: g, value: v }); });
  chart('cFlow').setOption({
    tooltip: { trigger: 'item', formatter: (p) => p.dataType === 'edge' ? `${p.data.source} → ${p.data.target}<br/>${p.data.value.toFixed(1)} kW` : p.name },
    series: [{ type: 'sankey', left: 10, right: 120, top: 10, bottom: 10, nodeWidth: 12, nodeGap: 8, layoutIterations: 0,
      emphasis: { focus: 'adjacency' }, data: nodes, links,
      label: { color: '#d8e9ff', fontSize: 11, formatter: (p) => p.name },
      lineStyle: { color: 'gradient', opacity: 0.35, curveness: 0.5 } }],
  });
}

function renderPie() {
  const st = S.stats;
  const data = pieMode === 'dev' ? st.ranking.map(x => ({ name: x.name, value: x.kwh })) : st.group_share.map(x => ({ name: x.name, value: x.kwh }));
  chart('cPie').setOption({
    color: PALETTE,
    tooltip: { trigger: 'item', formatter: '{b}<br/>{c} kWh（{d}%）' },
    legend: { orient: 'vertical', right: 0, top: 'middle', textStyle: { color: '#d8e9ff', fontSize: 10 }, itemWidth: 10, itemHeight: 8 },
    series: [{ type: 'pie', radius: ['42%', '70%'], center: ['34%', '52%'], roseType: false, itemStyle: { borderColor: '#06121f', borderWidth: 2 },
      label: { formatter: '{d}%', color: '#d8e9ff', fontSize: 10 }, data }],
    graphic: [{ type: 'text', left: '27%', top: '45%', style: { text: `${fmt(st.energy.load, 0)}\nkWh`, fill: '#fff', font: 'bold 13px Consolas', textAlign: 'center' } }],
  });
}

function renderRank() {
  const r = [...S.stats.ranking].reverse();
  chart('cRank').setOption(baseOpt({
    tooltip: { trigger: 'axis', formatter: (p) => { const x = r[p[0].dataIndex]; return `${x.name}<br/>今日 ${x.kwh} kWh（${x.pct}%）<br/>昨日同期 ${x.yday} kWh<br/>变化 ${x.change ?? '--'}%<br/>电费估算 ${x.cost} 元`; } },
    legend: { show: false }, grid: { left: 100, right: 70, top: 6, bottom: 6 },
    xAxis: { type: 'value', show: false },
    yAxis: { type: 'category', data: r.map(x => x.name), ...axisStyle, axisLabel: { color: '#d8e9ff', fontSize: 11 } },
    series: [{ type: 'bar', barWidth: 12, data: r.map((x, i) => ({ value: x.kwh, itemStyle: { borderRadius: [0, 6, 6, 0], color: new echarts.graphic.LinearGradient(0, 0, 1, 0, [{ offset: 0, color: 'rgba(63,208,255,.2)' }, { offset: 1, color: i >= r.length - 3 ? '#ffc43d' : '#3fd0ff' }]) } })),
      label: { show: true, position: 'right', color: '#d8e9ff', fontSize: 10, formatter: (p) => `${fmt(p.value, 0)} ${r[p.dataIndex].change === null ? '' : (r[p.dataIndex].change >= 0 ? '↑' : '↓') + Math.abs(r[p.dataIndex].change).toFixed(0) + '%'}` } }],
  }));
}

function renderAlarmList(el, list) {
  if (!list.length) { el.innerHTML = '<li class="empty" style="display:block">暂无告警，系统运行正常 ✓</li>'; return; }
  el.innerHTML = list.map(a => `<li class="${a.active ? '' : 'done'}">
    <span class="lv ${a.level}">${a.level}</span>
    <div><b>${a.name}</b> · ${TYPE_TXT[a.type] || a.type}<div>${a.msg}</div>
      <div class="meta">${a.start_ts.slice(5)} 发生${a.end_ts && a.end_ts !== a.start_ts ? ' · ' + a.end_ts.slice(11) + ' 恢复' : (a.active ? ' · 持续中' : '')}</div></div>
    ${a.acked ? '<span class="meta">已确认</span>' : `<button class="ack" data-ack="${a.id}">确认</button>`}
  </li>`).join('');
}

/* ============================ 统计分析 ============================ */
function renderAnalysis() {
  const st = S.stats, h = st.hourly, e = st.energy;
  // ① 分时能耗 今日 vs 昨日
  chart('aHourly').setOption(baseOpt({
    grid: { left: 50, right: 50, top: 34, bottom: 24 },
    xAxis: { type: 'category', data: hours, ...axisStyle },
    yAxis: [{ type: 'value', name: 'kWh', nameTextStyle: { color: COLOR.muted }, ...axisStyle }, { type: 'value', name: '元', nameTextStyle: { color: COLOR.muted }, ...axisStyle, splitLine: { show: false } }],
    series: [
      { name: '今日用电', type: 'bar', data: h.today_load, itemStyle: { color: COLOR.load }, barGap: '10%',
        markPoint: { data: [{ type: 'max', name: '峰值' }, { type: 'min', name: '谷值' }], label: { fontSize: 9 }, itemStyle: { color: '#ff8a3d' } } },
      { name: '昨日用电', type: 'bar', data: h.yday_load, itemStyle: { color: 'rgba(127,155,187,.45)' } },
      { name: '今日光伏', type: 'line', data: h.today_pv, smooth: true, itemStyle: { color: COLOR.pv } },
      { name: '今日电网购电', type: 'line', data: h.today_grid, smooth: true, itemStyle: { color: COLOR.grid }, lineStyle: { type: 'dashed' } },
      { name: '今日电费', type: 'line', yAxisIndex: 1, data: h.today_cost, smooth: true, itemStyle: { color: '#ff6fb5' } },
    ],
  }));

  const cmp = [
    ['用电量 (kWh)', e.load, e.yday_load_same, e.load_change],
    ['昨日全天用电', e.yday_load, null, null],
    ['今日电费 (元)', e.cost, e.yday_cost, null],
    ['今日碳排 (kg)', e.carbon, e.yday_carbon, null],
    ['今日光伏 (kWh)', e.pv, e.yday_pv, null],
    ['数据中心 PUE', st.dc.pue_avg, null, null],
  ];
  $('#cmpBox').innerHTML = cmp.map(c => `<div class="kpi"><div class="t">${c[0]}</div><div class="v">${fmt(c[1], c[0].includes('PUE') ? 3 : 0)}</div>
    <div class="sub">${c[2] !== null ? (c[0].startsWith('用电量') ? '昨日同期 ' : '昨日全天 ') + fmt(c[2], 0) : ''} ${chg(c[3])}</div></div>`).join('') +
    `<div class="kpi" style="grid-column:span 2"><div class="t">峰值负荷小时</div><div class="v" style="font-size:16px">${st.peak.peak_hour ?? '--'}:00 - ${(st.peak.peak_hour ?? 0) + 1}:00（${S.config.tariff[S.config.hour_period[st.peak.peak_hour ?? 0]].name}时段）</div>
    <div class="sub">建议将可调负荷转移至谷段，提升储能峰时放电</div></div>`;

  // ② 区域堆叠
  chart('aGroup').setOption(baseOpt({
    color: PALETTE,
    xAxis: { type: 'category', data: hours, ...axisStyle },
    yAxis: { type: 'value', name: 'kWh', nameTextStyle: { color: COLOR.muted }, ...axisStyle },
    series: Object.entries(h.group).map(([g, d]) => ({ name: g, type: 'bar', stack: 'g', data: d, barWidth: '60%' })),
  }));

  // ③ 峰谷平
  const per = st.periods;
  chart('aPeriod').setOption({
    tooltip: { trigger: 'item' },
    legend: { bottom: 0, textStyle: { color: '#d8e9ff', fontSize: 10 }, itemWidth: 10, itemHeight: 8 },
    title: [{ text: '用电量', left: '22%', top: 4, textAlign: 'center', textStyle: { color: COLOR.muted, fontSize: 11 } }, { text: '电网电费', left: '72%', top: 4, textAlign: 'center', textStyle: { color: COLOR.muted, fontSize: 11 } }],
    series: [
      { type: 'pie', radius: ['30%', '58%'], center: ['22%', '50%'], data: per.map(p => ({ name: p.name, value: p.load, itemStyle: { color: p.color } })), label: { formatter: '{b}\n{d}%', color: '#d8e9ff', fontSize: 10 }, tooltip: { formatter: '{b} 用电 {c} kWh ({d}%)' } },
      { type: 'pie', radius: ['30%', '58%'], center: ['72%', '50%'], data: per.map(p => ({ name: p.name, value: p.cost, itemStyle: { color: p.color } })), label: { formatter: '{b}\n{c}元', color: '#d8e9ff', fontSize: 10 }, tooltip: { formatter: '{b} 电费 {c} 元 ({d}%)' } },
    ],
  });

  const pk = st.peak, row = (n, a, b) => `<tr><td>${n}</td><td>${a ?? '--'}</td><td>${b ?? '--'}</td></tr>`;
  const T = pk.today || {}, Y = pk.yday || {};
  $('#peakTbl').innerHTML = `<tr><th>指标</th><th>今日</th><th>昨日</th></tr>` +
    row('最大负荷 (kW)', T.max && `${T.max} @${T.max_ts}`, Y.max && `${Y.max} @${Y.max_ts}`) +
    row('最小负荷 (kW)', T.min && `${T.min} @${T.min_ts}`, Y.min && `${Y.min} @${Y.min_ts}`) +
    row('平均负荷 (kW)', T.avg, Y.avg) + row('峰谷差 (kW)', T.diff, Y.diff) +
    row('峰谷差率 (%)', T.diff_rate, Y.diff_rate) + row('日负荷率 (%)', T.factor, Y.factor) +
    row('谷段用电占比 (%)', fmt(per.find(p => p.key === 'valley').load / (e.load || 1) * 100), '') +
    row('平均购电单价 (元/kWh)', e.avg_price, '') +
    row('储能 充/放 (kWh)', `${e.ess_chg} / ${e.ess_dis}`, '');

  // ④ 电费估算
  const rk = [...st.ranking].sort((a, b) => a.cost - b.cost);
  chart('aCost').setOption(baseOpt({
    legend: { show: false }, grid: { left: 100, right: 60, top: 26, bottom: 10 },
    title: { text: `按电价估算合计 ${fmt(e.cost_tariff, 0)} 元 · 实际购电 ${fmt(e.cost, 0)} 元 · 光储节省 ${fmt(e.saving, 0)} 元`, textStyle: { color: COLOR.muted, fontSize: 11, fontWeight: 'normal' }, left: 0, top: 0 },
    xAxis: { type: 'value', show: false },
    yAxis: { type: 'category', data: rk.map(x => x.name), ...axisStyle, axisLabel: { color: '#d8e9ff', fontSize: 11 } },
    series: [{ type: 'bar', data: rk.map(x => x.cost), barWidth: 12, itemStyle: { color: '#ff8a3d', borderRadius: [0, 6, 6, 0] }, label: { show: true, position: 'right', color: '#d8e9ff', formatter: '{c} 元', fontSize: 10 } }],
  }));

  // ⑤ 光伏 vs 用电
  chart('aPv').setOption(baseOpt({
    title: { text: `今日光伏 ${fmt(e.pv, 0)} kWh · 自用率 ${fmt(e.pv_self_use)}% · 绿电占比 ${fmt(e.green_ratio)}%`, textStyle: { color: COLOR.muted, fontSize: 11, fontWeight: 'normal' }, left: 0, top: 0 },
    legend: { top: 18, textStyle: { color: '#d8e9ff', fontSize: 10 } }, grid: { left: 46, right: 12, top: 46, bottom: 24 },
    xAxis: { type: 'category', data: hours, ...axisStyle },
    yAxis: { type: 'value', name: 'kWh', ...axisStyle },
    series: [
      { name: '用电量', type: 'line', data: h.today_load, smooth: true, itemStyle: { color: COLOR.load }, areaStyle: { color: 'rgba(63,208,255,.12)' } },
      { name: '光伏发电', type: 'line', data: h.today_pv, smooth: true, itemStyle: { color: COLOR.pv }, areaStyle: { color: 'rgba(54,226,160,.3)' } },
      { name: '昨日光伏', type: 'line', data: h.yday_pv, smooth: true, itemStyle: { color: 'rgba(54,226,160,.4)' }, lineStyle: { type: 'dotted' }, showSymbol: false },
    ],
  }));

  // ⑥ 碳排放
  let acc = 0; const cum = h.today_carbon.map(v => v === null ? null : (acc += v, +acc.toFixed(1)));
  chart('aCarbon').setOption(baseOpt({
    title: { text: `今日排放 ${fmt(e.carbon / 1000, 2)} t · 光伏减排 ${fmt(e.carbon_avoided / 1000, 2)} t · 昨日 ${fmt(e.yday_carbon / 1000, 2)} t`, textStyle: { color: COLOR.muted, fontSize: 11, fontWeight: 'normal' }, left: 0, top: 0 },
    legend: { top: 18, textStyle: { color: '#d8e9ff', fontSize: 10 } }, grid: { left: 46, right: 46, top: 46, bottom: 24 },
    xAxis: { type: 'category', data: hours, ...axisStyle },
    yAxis: [{ type: 'value', name: 'kg/h', ...axisStyle }, { type: 'value', name: '累计kg', ...axisStyle, splitLine: { show: false } }],
    series: [
      { name: '逐时碳排', type: 'bar', data: h.today_carbon, itemStyle: { color: '#ff9fb1' }, barWidth: '55%' },
      { name: '累计碳排', type: 'line', yAxisIndex: 1, data: cum, smooth: true, itemStyle: { color: '#ffc43d' } },
    ],
  }));

  // ⑦ PUE
  const r = S.realtime;
  chart('aPue').setOption(baseOpt({
    title: { text: `今日平均 PUE ${fmt(st.dc.pue_avg, 3)} · 最高 ${fmt(st.dc.pue_max, 3)} · 制冷能耗占比 ${fmt(st.dc.cool_ratio)}%`, textStyle: { color: COLOR.muted, fontSize: 11, fontWeight: 'normal' }, left: 0, top: 0 },
    legend: { top: 18, textStyle: { color: '#d8e9ff', fontSize: 10 } }, grid: { left: 46, right: 46, top: 46, bottom: 24 },
    xAxis: { type: 'category', data: r.ts, boundaryGap: false, ...axisStyle },
    yAxis: [{ type: 'value', name: 'kW', ...axisStyle }, { type: 'value', name: 'PUE', min: 1, max: 2, ...axisStyle, splitLine: { show: false } }],
    series: [
      { name: 'IT机柜', type: 'line', stack: 'dc', data: r.dev.dc_it, showSymbol: false, areaStyle: {}, itemStyle: { color: '#5b8cff' } },
      { name: '制冷', type: 'line', stack: 'dc', data: r.dev.dc_cool, showSymbol: false, areaStyle: {}, itemStyle: { color: '#3fd0ff' } },
      { name: 'PUE', type: 'line', yAxisIndex: 1, data: r.pue, showSymbol: false, itemStyle: { color: '#ffc43d' },
        markLine: { symbol: 'none', data: [{ yAxis: S.config.pue_limit, name: '告警线' }], lineStyle: { color: COLOR.red }, label: { formatter: '告警线 {c}', color: COLOR.red, fontSize: 10 } } },
    ],
  }));
}

/* ============================ 告警中心 ============================ */
function renderAlarmCenter() {
  const all = S.alarms.recent;
  const cnt = (f) => all.filter(f).length;
  $('#alarmKpi').innerHTML = [
    ['告警总数', S.alarms.total, '#fff'], ['未恢复', S.alarms.active.length, COLOR.red],
    ['未确认', S.alarms.unacked, COLOR.orange], ['严重', cnt(a => a.level === '严重'), COLOR.red],
  ].map(x => `<div class="kpi"><div class="t">${x[0]}</div><div class="v" style="color:${x[2]}">${x[1]}</div></div>`).join('');
  const byType = {};
  all.forEach(a => byType[TYPE_TXT[a.type]] = (byType[TYPE_TXT[a.type]] || 0) + 1);
  chart('aAlarmPie').setOption({
    color: ['#ff4d6d', '#ff8a3d', '#ffc43d', '#b07cff', '#3fd0ff', '#36e2a0'],
    tooltip: { trigger: 'item' },
    series: [{ type: 'pie', radius: ['35%', '65%'], data: Object.entries(byType).map(([name, value]) => ({ name, value })), label: { color: '#d8e9ff', formatter: '{b}\n{c}次' } }],
  });
  $('#ruleTbl').innerHTML = `<tr><th>规则</th><th>判定条件</th><th>级别</th></tr>
    <tr><td>功率超限</td><td>实时功率 &gt; 设定阈值（&gt;额定为严重）</td><td>警告/严重</td></tr>
    <tr><td>功率突变</td><td>相邻周期变化 &gt; 60% 且 &gt; 20% 额定</td><td>提示</td></tr>
    <tr><td>数据中断</td><td>超过 4 秒未收到监测数据</td><td>严重</td></tr>
    <tr><td>PUE 超限</td><td>数据中心 PUE &gt; ${S.config.pue_limit}</td><td>警告</td></tr>
    <tr><td>光伏异常</td><td>辐照 &gt; 200 W/m² 且出力 &lt; 理论 35%</td><td>警告</td></tr>
    <tr><td>SOC 过低</td><td>储能 SOC &lt; 10%</td><td>提示</td></tr>`;
  let list = all;
  if (alarmFilter === 'active') list = all.filter(a => a.active);
  if (alarmFilter === 'unacked') list = all.filter(a => !a.acked);
  $('#alarmTbl').innerHTML = `<tr><th>#</th><th>级别</th><th>对象</th><th>类型</th><th>描述</th><th>发生(仿真)</th><th>恢复</th><th>状态</th><th>操作</th></tr>` +
    (list.length ? list.map(a => `<tr class="${a.active ? 'on' : ''}"><td>${a.id}</td><td><span class="lv ${a.level}">${a.level}</span></td><td>${a.name}</td><td>${TYPE_TXT[a.type]}</td>
    <td>${a.msg}</td><td>${a.start_ts.slice(5)}</td><td>${a.active ? '-' : a.end_ts.slice(11)}</td><td>${a.active ? '<span class="up">持续中</span>' : '<span class="down">已恢复</span>'}</td>
    <td>${a.acked ? '已确认' : `<button class="ack btn mini" data-ack="${a.id}">确认</button>`}</td></tr>`).join('') : '<tr><td colspan="9" class="empty">暂无记录</td></tr>');
}

/* ============================ 设备详情 ============================ */
function openDetail(id) { detailId = id; $('#detailMask').classList.remove('hide'); renderDetail(true); }
function renderDetail(first = false) {
  const d = S.devices.find(x => x.id === detailId), r = S.realtime;
  if (first) { chart('dChart').clear(); chart('dHour').clear(); }
  $('#dTitle').innerHTML = `<i class="s ${d.state}"></i> ${d.name} <small style="color:#7f9bbb;font-size:12px">${d.group} · ${STATE_TXT[d.state]}</small>`;
  const info = [['实时功率', fmt(d.power) + ' kW'], ['额定功率', d.rated + ' kW'], ['负荷率', fmt(d.rate) + '%'],
    [d.kind === 'pv' ? '今日发电' : d.kind === 'ess' ? '今日放电' : '今日用电', fmt(d.today_kwh, 0) + ' kWh']];
  if (d.kind === 'load') { const rk = S.stats.ranking.find(x => x.id === d.id); info.push(['今日电费估算', fmt(rk.cost, 0) + ' 元'], ['较昨日同期', chg(rk.change) || '--']); }
  if (d.kind === 'pv') info.push(['辐照度', fmt(d.extra.irradiance, 0) + ' W/m²'], ['理论出力', fmt(d.extra.expected) + ' kW']);
  if (d.kind === 'ess') info.push(['SOC', fmt(d.extra.soc * 100) + '%'], ['运行状态', ESS_TXT[d.status]]);
  $('#dInfo').innerHTML = info.map(x => `<div class="kpi"><div class="t">${x[0]}</div><div class="v" style="font-size:17px">${x[1]}</div></div>`).join('');
  const ml = d.threshold ? { symbol: 'none', data: [{ yAxis: d.threshold }], lineStyle: { color: COLOR.red, type: 'dashed' }, label: { formatter: '阈值 {c}', color: COLOR.red, fontSize: 10 } } : undefined;
  chart('dChart').setOption(baseOpt({
    xAxis: { type: 'category', data: r.ts, boundaryGap: false, ...axisStyle },
    yAxis: { type: 'value', name: 'kW', ...axisStyle },
    series: [{ name: '实时功率', type: 'line', data: r.dev[d.id], smooth: true, showSymbol: false, itemStyle: { color: COLOR.load },
      areaStyle: { color: 'rgba(63,208,255,.2)' }, markLine: ml }].concat(d.kind === 'ess' ? [{ name: 'SOC%', type: 'line', data: r.soc, showSymbol: false, itemStyle: { color: COLOR.pv } }] : []),
  }));
  // 分时用电（从分组/排名推不出单设备逐时，使用实时序列按小时聚合）
  const hk = [], hv = [];
  r.ts.forEach((t, i) => { const v = r.dev[d.id][i]; if (v === null) return; const k = t.slice(0, 2);
    if (hk[hk.length - 1] !== k) { hk.push(k); hv.push(0); } hv[hv.length - 1] += Math.abs(v) * 5 / 60; });
  chart('dHour').setOption(baseOpt({
    legend: { show: false }, title: { text: '近 12 小时逐时电量 kWh', textStyle: { color: COLOR.muted, fontSize: 11, fontWeight: 'normal' } },
    xAxis: { type: 'category', data: hk.map(k => k + '时'), ...axisStyle },
    yAxis: { type: 'value', ...axisStyle },
    series: [{ type: 'bar', data: hv.map(v => +v.toFixed(1)), itemStyle: { color: (p) => S.config.tariff[S.config.hour_period[parseInt(hk[p.dataIndex])]].color } }],
  }));
  if (first) {
    const faults = d.kind === 'pv' ? [['pv_fault', '光伏故障'], ['offline', '数据中断']] : d.kind === 'ess' ? [['offline', '数据中断']] :
      d.id === 'dc_cool' ? [['cooling', '制冷故障'], ['overload', '功率超限'], ['offline', '数据中断']] : [['overload', '功率超限'], ['surge', '功率突变'], ['offline', '数据中断']];
    $('#dCtl').innerHTML = (d.threshold ? `<span>功率告警阈值</span><input id="thInput" type="number" value="${d.threshold}"><span>kW</span><button class="btn mini" id="thSave" style="margin-left:0">保存阈值</button><span style="flex:1"></span>` : '<span style="flex:1"></span>') +
      `<span>模拟异常：</span>` + faults.map(f => `<button class="btn ghost mini" style="margin-left:0" data-fault="${f[0]}">${f[1]}</button>`).join('');
  }
  renderAlarmList($('#dAlarms'), S.alarms.recent.filter(a => a.device === d.id).slice(0, 10));
}

/* ============================ 交互 ============================ */
async function post(url, body) {
  const r = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body || {}) });
  return r.json();
}
function toast(msg) { const t = $('#toast'); t.textContent = msg; t.classList.remove('hide'); clearTimeout(t._h); t._h = setTimeout(() => t.classList.add('hide'), 2500); }
async function inject(device, type, ticks) {
  await post('/api/fault', { device, type, ticks });
  toast(type === 'clear' ? '已下发：清除全部异常' : `已下发：${S.devices.find(d => d.id === device).name} - ${$(`#fType option[value=${type}]`).textContent}`);
}

$$('.tab').forEach(b => b.onclick = () => {
  $$('.tab').forEach(x => x.classList.toggle('active', x === b));
  view = b.dataset.view;
  $$('.view').forEach(v => v.classList.toggle('hide', v.id !== 'view-' + view));
  if (S) render();
  Object.values(charts).forEach(c => c.resize());
});
function seg(id, cb) { $(id).onclick = (e) => { const b = e.target.closest('button'); if (!b) return; $$(id + ' button').forEach(x => x.classList.toggle('on', x === b)); cb(b.dataset.k); if (S) render(); }; }
seg('#rtSeg', k => rtMode = k);
seg('#pieSeg', k => pieMode = k);
seg('#alarmFilter', k => alarmFilter = k);

$('#devices').onclick = (e) => { const c = e.target.closest('.dev'); if (c) openDetail(c.dataset.id); };
document.addEventListener('click', async (e) => {
  const a = e.target.closest('[data-ack]');
  if (a) { await post('/api/alarm/ack', { id: +a.dataset.ack }); }
  const f = e.target.closest('[data-fault]');
  if (f) { await inject(detailId, f.dataset.fault, 8); f.textContent = '✓ 已下发'; }
  if (e.target.id === 'thSave') { await post('/api/threshold', { device: detailId, threshold: +$('#thInput').value }); e.target.textContent = '✓ 已保存'; }
  const q = e.target.closest('[data-q]');
  if (q) { const [d, t, n] = q.dataset.q.split(','); await inject(d, t, +n); }
});
$('#ackAll').onclick = () => post('/api/alarm/ack', {});
$('[data-close]').onclick = () => { detailId = null; $('#detailMask').classList.add('hide'); };
$('#detailMask').onclick = (e) => { if (e.target.id === 'detailMask') { detailId = null; e.target.classList.add('hide'); } };
$('#btnConsole').onclick = () => $('#console').classList.toggle('hide');
$('[data-close-console]').onclick = () => $('#console').classList.add('hide');
$('#fGo').onclick = () => inject($('#fDev').value, $('#fType').value, +$('#fTicks').value);
window.addEventListener('resize', () => Object.values(charts).forEach(c => c.resize()));

fetch('/api/snapshot').then(r => r.json()).then(d => {
  if (d.ready) { S = d; render(); }
  if (d.devices) $('#fDev').innerHTML = d.devices.map(x => `<option value="${x.id}">${x.name}</option>`).join('');
  else $('#fDev').innerHTML = ['ws_a', 'ws_b', 'air', 'dc_it', 'dc_cool', 'office', 'ev', 'pv', 'ess'].map(x => `<option>${x}</option>`).join('');
});
connect();
