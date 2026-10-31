"""Podgląd napraw animacji w przeglądarce: odtwarzanie „przed / po” z arkuszy BerserkerAnimeReviewTests.

Arkusz testu: N kolumn (kolejne chwile ruchu) × 3 rzędy (bok, kamera gry zza pleców, przód), kafel 180×240.
index.txt podaje klatki gry (60 kl./s) każdej kolumny – odtwarzanie zachowuje czas gry (z regulacją tempa).
Użycie:
  python review_viewer.py <wyjście.html> "<tytuł>" <katalog_przed> <katalog_po> <sekwencja> [<sekwencja> ...]
Katalogi zawierają <sekwencja>.png, <sekwencja>.csv i index.txt (np. TestResults/berserker-anim-review/v2-light/hu_greataxe).
Ścieżki obrazów w HTML są względne wobec pliku wyjściowego.
"""
import csv, html, json, math, os, re, sys


def read_index(d):
    shots = {}
    p = os.path.join(d, "index.txt")
    if os.path.exists(p):
        for line in open(p, encoding="utf-8"):
            m = re.match(r"(\S+): (\d+) klatek .*zrzuty: ([\d ]+)", line.strip())
            if m:
                shots[m.group(1)] = [int(x) for x in m.group(3).split()]
    return shots


def metrics(path):
    """Kilka liczb z CSV: tor ostrza (azymut, wysokość) w kolumnach zrzutów, prędkość czubka, skok pozy, klip."""
    rows = list(csv.reader(open(path, encoding="utf-8")))[1:]
    V = lambda r, i: [float(x) for x in r[i:i + 3]]
    tipv = 0.0; jump = 0.0; clips = []
    for a, b in zip(rows, rows[1:]):
        tipv = max(tipv, math.dist(V(a, 29), V(b, 29)) * 60)
        ra, rb = V(a, 8), V(b, 8)
        for i in (11, 14, 23, 26, 32, 35):
            pa = [V(a, i)[k] - ra[k] for k in range(3)]; pb = [V(b, i)[k] - rb[k] for k in range(3)]
            jump = max(jump, math.dist(pa, pb) * 60)
    for r in rows:
        if r[7] and (not clips or clips[-1] != r[7]):
            clips.append(r[7])
    path_pts = []
    for r in rows:
        tip, root = V(r, 29), V(r, 8)
        t = [tip[k] - root[k] for k in range(3)]
        path_pts.append([round(math.degrees(math.atan2(t[0], t[2])), 1), round(tip[1], 3), r[5]])
    return {"tipv": round(tipv, 1), "jump": round(jump, 1), "clips": clips, "path": path_pts, "frames": len(rows)}


def main():
    out, title, before, after, seqs = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5:]
    od = os.path.dirname(os.path.abspath(out))
    ib, ia = read_index(before), read_index(after)
    data = []
    for s in seqs:
        item = {"name": s}
        for key, d, idx in (("before", before, ib), ("after", after, ia)):
            png = os.path.join(d, s + ".png")
            if not os.path.exists(png):
                item[key] = None; continue
            from PIL import Image
            w, h = Image.open(png).size
            item[key] = {"img": os.path.relpath(png, od).replace("\\", "/"), "cols": w // 180, "shots": idx.get(s, []),
                         "m": metrics(os.path.join(d, s + ".csv"))}
        data.append(item)
    page = TEMPLATE.replace("__TITLE__", html.escape(title)).replace("__DATA__", json.dumps(data, ensure_ascii=False))
    open(out, "w", encoding="utf-8").write(page)
    print("zapisano", out)


TEMPLATE = r"""<!DOCTYPE html>
<html lang="pl"><head><meta charset="utf-8"><title>__TITLE__</title>
<style>
 body{background:#1b1c20;color:#e8e6e1;font:14px system-ui,Segoe UI,sans-serif;margin:0;padding:18px 24px}
 h1{font-size:20px;margin:0 0 4px} .sub{color:#a7a39a;margin-bottom:14px}
 .bar{position:sticky;top:0;background:#1b1c20;padding:8px 0;z-index:5;display:flex;gap:14px;align-items:center;flex-wrap:wrap;border-bottom:1px solid #333}
 button{background:#3a3d46;color:#fff;border:0;border-radius:4px;padding:6px 12px;cursor:pointer} button.on{background:#a8442c}
 .seq{margin:22px 0;padding-bottom:12px;border-bottom:1px solid #2c2d33}
 .seq h2{font-size:16px;margin:0 0 6px}
 .pair{display:flex;gap:28px;flex-wrap:wrap}
 .side h3{font-size:13px;margin:4px 0;color:#c9c4b8} .side.after h3{color:#f0a37f}
 .views{display:flex;gap:4px} .tile{width:180px;height:240px;background-repeat:no-repeat;border-radius:3px}
 .lbl{display:flex;gap:4px;font-size:11px;color:#8d897f} .lbl span{width:180px;text-align:center}
 table{border-collapse:collapse;font-size:12px;margin-top:6px} td,th{padding:2px 8px;border-bottom:1px solid #2c2d33;text-align:left}
 .clip{color:#a7a39a;font-size:12px} input[type=range]{width:320px}
 canvas{background:#24252b;border-radius:3px;margin-top:6px}
</style></head><body>
<h1>__TITLE__</h1>
<div class="sub">Kafle z gry (Unity, berserker anime). Każda sekwencja: bok · kamera gry zza pleców · przód. Odtwarzanie w czasie gry (60 kl./s), „przed” i „po” zsynchronizowane. Wykres: tor czubka broni (azymut względem przodu postaci → wysokość), kolor = faza.</div>
<div class="bar"><button id="play" class="on">❚❚ pauza</button>
 tempo <button data-s="0.25">0,25×</button><button data-s="0.5">0,5×</button><button data-s="1" class="on">1×</button>
 <span>czas <input id="t" type="range" min="0" max="1000" value="0"></span><span id="tl"></span>
 <label><input type="checkbox" id="loop" checked> pętla</label></div>
<div id="list"></div>
<script>
const DATA = __DATA__;
const W=180,H=240; let speed=1, playing=true, t=0, last=performance.now();
const maxT = Math.max(...DATA.flatMap(d=>[d.before,d.after].filter(Boolean).map(x=>x.m.frames)));
const list=document.getElementById('list');
function block(side,label,cls){const div=document.createElement('div');div.className='side '+cls;
 if(!side){div.innerHTML='<h3>'+label+' – brak</h3>';return div;}
 div.innerHTML='<h3>'+label+'</h3><div class="views">'+[0,1,2].map(r=>`<div class="tile" data-row="${r}" style="background-image:url('${side.img}')"></div>`).join('')+'</div>'+
 '<div class="lbl"><span>bok</span><span>kamera gry</span><span>przód</span></div>'+
 `<div class="clip">klip: ${side.m.clips.join(' → ')||'(ruch)'} · czubek max ${side.m.tipv} m/s · skok pozy max ${side.m.jump} m/s</div>`;
 const c=document.createElement('canvas');c.width=548;c.height=120;div.appendChild(c);side.canvas=c;return div;}
DATA.forEach(d=>{const s=document.createElement('div');s.className='seq';s.innerHTML='<h2>'+d.name+'</h2>';
 const p=document.createElement('div');p.className='pair';p.appendChild(block(d.before,'PRZED','before'));p.appendChild(block(d.after,'PO','after'));
 s.appendChild(p);list.appendChild(s);d.el=s;});
const PH={Startup:'#6c8ebf',Active:'#e0533a',Recovery:'#8fb36b',None:'#888'};
function drawPath(side,f){if(!side)return;const c=side.canvas,g=c.getContext('2d');g.clearRect(0,0,c.width,c.height);
 const X=a=>(a+180)/360*c.width, Y=h=>c.height-6-(h/2.6)*(c.height-12);
 g.strokeStyle='#3a3b42';[ -90,0,90].forEach(a=>{g.beginPath();g.moveTo(X(a),0);g.lineTo(X(a),c.height);g.stroke();});
 g.fillStyle='#777';g.font='10px sans-serif';g.fillText('lewo −90°',X(-90)+3,10);g.fillText('przód 0°',X(0)+3,10);g.fillText('prawo +90°',X(90)+3,10);
 const P=side.m.path;for(let i=1;i<P.length;i++){if(Math.abs(P[i][0]-P[i-1][0])>180)continue;g.strokeStyle=PH[P[i][2]]||'#888';g.lineWidth=2;
  g.beginPath();g.moveTo(X(P[i-1][0]),Y(P[i-1][1]));g.lineTo(X(P[i][0]),Y(P[i][1]));g.stroke();}
 const k=Math.min(P.length-1,Math.floor(f));if(k>=0){g.fillStyle='#fff';g.beginPath();g.arc(X(P[k][0]),Y(P[k][1]),4,0,7);g.fill();}}
function show(side,el,f){if(!side)return;const s=side.shots;let i=0;while(i+1<s.length&&s[i+1]<=f)i++;
 el.querySelectorAll('.tile').forEach(tile=>{const r=+tile.dataset.row;tile.style.backgroundPosition=`${-i*W}px ${-r*H}px`;});drawPath(side,f);}
function render(){DATA.forEach(d=>{const ps=d.el.querySelectorAll('.side');show(d.before,ps[0],t);show(d.after,ps[1],t);});
 document.getElementById('t').value=Math.round(t/maxT*1000);document.getElementById('tl').textContent=`klatka ${Math.floor(t)} / ${maxT} (${(t/60).toFixed(2)} s)`;}
function tick(now){const dt=(now-last)/1000;last=now;if(playing){t+=dt*60*speed;if(t>=maxT){t=document.getElementById('loop').checked?0:maxT-1;}}render();requestAnimationFrame(tick);}
document.getElementById('play').onclick=e=>{playing=!playing;e.target.textContent=playing?'❚❚ pauza':'▶ odtwarzaj';e.target.classList.toggle('on',playing);};
document.querySelectorAll('[data-s]').forEach(b=>b.onclick=()=>{speed=+b.dataset.s;document.querySelectorAll('[data-s]').forEach(x=>x.classList.toggle('on',x===b));});
document.getElementById('t').oninput=e=>{t=e.target.value/1000*maxT;playing=false;const p=document.getElementById('play');p.textContent='▶ odtwarzaj';p.classList.remove('on');};
requestAnimationFrame(tick);
</script></body></html>"""

if __name__ == "__main__":
    main()
