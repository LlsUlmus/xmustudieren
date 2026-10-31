/* 轻量 SVG 图表（无第三方依赖，离线可用） */
const Charts = (() => {
  const css = (v) => getComputedStyle(document.documentElement).getPropertyValue(v).trim();
  const hhmmss = (t) => new Date(t * 1000).toTimeString().slice(0, 8);

  function niceRange(min, max) {
    if (min === max) { min -= 1; max += 1; }
    const pad = (max - min) * 0.08;
    return [min >= 0 && min - pad < 0 ? 0 : min - pad, max + pad];
  }

  function frame(el, w, h) {
    const W = el.clientWidth || w;
    return { W, H: h, L: 40, R: 40, T: 10, B: 22 };
  }

  /**
   * opt: { height, series:[{data:[[t,v]], color, name, width, dash, axis:'left'|'right', area}],
   *        band:{y0,y1}, hlines:[{y,color,label}], marks:[{t,color}], yLeft:[min,max], yRight:[min,max] }
   */
  function line(el, opt) {
    const h = opt.height || 190;
    const f = frame(el, 600, h);
    const pts = opt.series.flatMap((s) => s.data);
    if (!pts.length) { el.innerHTML = `<div class="empty">等待数据…</div>`; return; }
    const ts = pts.map((p) => p[0]);
    const x0 = Math.min(...ts), x1 = Math.max(...ts, x0 + 1);
    const ranges = {};
    for (const axis of ['left', 'right']) {
      const vals = opt.series.filter((s) => (s.axis || 'left') === axis).flatMap((s) => s.data.map((p) => p[1])).filter((v) => v != null);
      if (axis === 'left' && opt.band) vals.push(opt.band.y0, opt.band.y1);
      const fixed = axis === 'left' ? opt.yLeft : opt.yRight;
      ranges[axis] = fixed || (vals.length ? niceRange(Math.min(...vals), Math.max(...vals)) : [0, 1]);
    }
    const X = (t) => f.L + ((t - x0) / (x1 - x0)) * (f.W - f.L - f.R);
    const Y = (v, axis = 'left') => { const [a, b] = ranges[axis]; return f.T + (1 - (v - a) / (b - a)) * (h - f.T - f.B); };
    const grid = css('--grid'), muted = css('--muted');
    let s = `<svg viewBox="0 0 ${f.W} ${h}" width="100%" height="${h}" role="img">`;
    if (opt.band) {
      s += `<rect x="${f.L}" y="${Y(opt.band.y1)}" width="${f.W - f.L - f.R}" height="${Y(opt.band.y0) - Y(opt.band.y1)}" fill="${css('--band')}"/>`;
    }
    for (let i = 0; i <= 4; i++) {
      const [a, b] = ranges.left; const v = a + ((b - a) * i) / 4; const y = Y(v);
      s += `<line x1="${f.L}" x2="${f.W - f.R}" y1="${y}" y2="${y}" stroke="${grid}"/>`;
      s += `<text x="${f.L - 6}" y="${y + 4}" text-anchor="end" class="ax">${v.toFixed(Math.abs(b - a) < 10 ? 1 : 0)}</text>`;
      if (opt.series.some((x) => x.axis === 'right')) {
        const [c, d] = ranges.right; const v2 = c + ((d - c) * i) / 4;
        s += `<text x="${f.W - f.R + 6}" y="${y + 4}" class="ax">${v2.toFixed(Math.abs(d - c) < 10 ? 1 : 0)}</text>`;
      }
    }
    for (let i = 0; i <= 3; i++) {
      const t = x0 + ((x1 - x0) * i) / 3;
      s += `<text x="${X(t)}" y="${h - 5}" text-anchor="${i === 0 ? 'start' : i === 3 ? 'end' : 'middle'}" class="ax">${hhmmss(t)}</text>`;
    }
    for (const m of opt.marks || []) {
      s += `<line x1="${X(m.t)}" x2="${X(m.t)}" y1="${f.T}" y2="${h - f.B}" stroke="${m.color}" stroke-width="1.5" stroke-dasharray="3 3" opacity=".8"/>`;
    }
    for (const hl of opt.hlines || []) {
      s += `<line x1="${f.L}" x2="${f.W - f.R}" y1="${Y(hl.y)}" y2="${Y(hl.y)}" stroke="${hl.color}" stroke-dasharray="5 4"/>`;
      if (hl.label) s += `<text x="${f.W - f.R - 4}" y="${Y(hl.y) - 4}" text-anchor="end" class="ax" fill="${hl.color}">${hl.label}</text>`;
    }
    for (const se of opt.series) {
      let d = '', pen = false;
      for (const [t, v] of se.data) {
        if (v == null) { pen = false; continue; }
        d += `${pen ? 'L' : 'M'}${X(t).toFixed(1)},${Y(v, se.axis).toFixed(1)}`; pen = true;
      }
      if (se.area && se.data.length) {
        const first = se.data.find((p) => p[1] != null), last = [...se.data].reverse().find((p) => p[1] != null);
        if (first) s += `<path d="${d}L${X(last[0])},${h - f.B}L${X(first[0])},${h - f.B}Z" fill="${se.color}" opacity=".12"/>`;
      }
      s += `<path d="${d}" fill="none" stroke="${se.color}" stroke-width="${se.width || 2}" ${se.dash ? `stroke-dasharray="${se.dash}"` : ''} stroke-linejoin="round" stroke-linecap="round"/>`;
    }
    for (const p of opt.points || []) {
      s += `<g transform="translate(${X(p.t)},${h - f.B - 6})"><path d="M-4,-4L4,4M4,-4L-4,4" stroke="${p.color}" stroke-width="2"/></g>`;
    }
    s += `</svg>`;
    const legend = opt.series.filter((x) => x.name).map((x) => `<span><i style="background:${x.color}"></i>${x.name}</span>`).join('') + (opt.legendExtra || '');
    el.innerHTML = s + `<div class="legend">${legend}</div>`;
  }

  /** 堆叠柱状图 opt: {height, keys:[{k,name,color}], rows:[{ts, k1..., flag}], flagColors} */
  function stacked(el, opt) {
    const h = opt.height || 180;
    const f = frame(el, 600, h);
    const rows = opt.rows;
    if (!rows.length) { el.innerHTML = `<div class="empty">等待数据…</div>`; return; }
    const tot = rows.map((r) => opt.keys.reduce((a, k) => a + (r[k.k] || 0), 0));
    const max = Math.max(10, ...tot) * 1.1;
    const bw = (f.W - f.L - f.R) / rows.length;
    const Y = (v) => f.T + (1 - v / max) * (h - f.T - f.B);
    let s = `<svg viewBox="0 0 ${f.W} ${h}" width="100%" height="${h}">`;
    for (let i = 0; i <= 4; i++) {
      const v = (max * i) / 4, y = Y(v);
      s += `<line x1="${f.L}" x2="${f.W - f.R}" y1="${y}" y2="${y}" stroke="${css('--grid')}"/><text x="${f.L - 6}" y="${y + 4}" text-anchor="end" class="ax">${v.toFixed(0)}</text>`;
    }
    rows.forEach((r, i) => {
      let acc = 0; const x = f.L + i * bw + 1.5;
      const fc = opt.flagColors[r.flag];
      for (const k of opt.keys) {
        const v = r[k.k] || 0; if (!v) continue;
        const y1 = Y(acc + v), y0 = Y(acc);
        s += `<rect x="${x}" y="${y1}" width="${Math.max(1, bw - 3)}" height="${y0 - y1}" fill="${r.flag === 'false_alarm' ? css('--muted-2') : k.color}"/>`;
        acc += v;
      }
      if (fc) s += `<rect x="${x - 1}" y="${Y(acc) - 1}" width="${Math.max(1, bw - 1)}" height="${Y(0) - Y(acc) + 2}" fill="none" stroke="${fc}" stroke-width="2"/>`;
    });
    [0, Math.floor(rows.length / 2), rows.length - 1].forEach((i, j) => {
      s += `<text x="${f.L + i * bw + bw / 2}" y="${h - 5}" text-anchor="${j === 0 ? 'start' : j === 2 ? 'end' : 'middle'}" class="ax">${hhmmss(rows[i].ts)}</text>`;
    });
    s += `</svg>`;
    el.innerHTML = s + `<div class="legend">${opt.keys.map((k) => `<span><i style="background:${k.color}"></i>${k.name}</span>`).join('')}</div>`;
  }

  /** 迷你折线 */
  function spark(values, color, w = 120, h = 30) {
    if (!values || values.length < 2) return `<span class="muted">—</span>`;
    const mx = Math.max(...values, 1), mn = Math.min(...values, 0);
    const X = (i) => (i / (values.length - 1)) * (w - 4) + 2;
    const Y = (v) => h - 3 - ((v - mn) / (mx - mn || 1)) * (h - 6);
    const d = values.map((v, i) => `${i ? 'L' : 'M'}${X(i).toFixed(1)},${Y(v).toFixed(1)}`).join('');
    return `<svg width="${w}" height="${h}" class="spark"><path d="${d}" fill="none" stroke="${color}" stroke-width="2"/><circle cx="${X(values.length - 1)}" cy="${Y(values[values.length - 1])}" r="3" fill="${color}"/></svg>`;
  }

  /** 水平条形分布 */
  function bars(items) {
    const max = Math.max(1, ...items.map((i) => i.v));
    return `<div class="hbars">${items.map((i) => `<div class="hbar"><span class="lbl">${i.label}</span><span class="track"><span style="width:${(i.v / max) * 100}%;background:${i.color}"></span></span><b>${i.v}</b></div>`).join('')}</div>`;
  }

  return { line, stacked, spark, bars, hhmmss };
})();
