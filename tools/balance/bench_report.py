import csv, sys, statistics as st
from collections import defaultdict

rows = list(csv.DictReader(open(sys.argv[1], encoding='utf-8-sig'), delimiter=';'))
F = lambda r, k: float(r[k])
print('wierszy', len(rows))

base = {(r['skill'], r['cele']): r for r in rows if r['rodzaj'] == 'bez'}
print('\n== SKILLE BEZ PIECZĘCI (gromada | kolos) – dps cyklu, dmg/skala, koszt, cd ==')
skills = sorted({r['skill'] for r in rows if r['rodzaj'] == 'bez'})
tab = []
for s in skills:
    g, k = base.get((s, 'gromada')), base.get((s, 'kolos'))
    if not g: continue
    tab.append((s, F(g, 'na_sekunde_cyklu'), F(k, 'na_sekunde_cyklu') if k else 0, F(g, 'obrazenia_cyklu' if 'obrazenia_cyklu' in g else 'obrazenia_6s'), F(g, 'skala'), F(g, 'odnowienie'), g['zasob'], F(g, 'koszt'), g['sprzet'], g['komponenty']))
tab.sort(key=lambda t: -t[1])
for t in tab:
    print(f"{t[0]:28} g={t[1]:8.1f} k={t[2]:8.1f} dmg={t[3]:8.0f} skala={t[4]:5.2f} cd={t[5]:5.2f} {t[6]}={t[7]:5.1f} sprz={t[8]} {t[9]}")
dm = [t[1] for t in tab if t[1] > 0]
print('mediana gromada', st.median(dm), 'kolos', st.median([t[2] for t in tab if t[2] > 0]))

print('\n== FORMY: mnożnik względem skilla bez pieczęci ==')
for tg in ('gromada', 'kolos'):
    res = []
    for r in rows:
        if r['rodzaj'] != 'forma' or r['cele'] != tg: continue
        b = base.get((r['skill'], tg))
        if not b or F(b, 'na_sekunde_cyklu') <= 0: continue
        res.append((F(r, 'na_sekunde_cyklu') / F(b, 'na_sekunde_cyklu'), r['skill'], r['pieczec'], F(r, 'odnowienie'), F(b, 'odnowienie'), F(r, 'koszt'), F(b, 'koszt')))
    res.sort()
    print(f'-- {tg}: n={len(res)} mediana={st.median([x[0] for x in res]):.2f}')
    for x in res[:12] + [None] + res[-12:]:
        if x is None: print('   ...'); continue
        print(f"   x{x[0]:5.2f} {x[1]:26} {x[2]:30} cd {x[4]:.1f}->{x[3]:.1f} koszt {x[6]:.0f}->{x[5]:.0f}")

print('\n== WSPARCIA: mediana mnożnika po skillach testowych (gromada) ==')
sup = defaultdict(list)
for r in rows:
    if r['rodzaj'] not in ('wsparcie', 'demoniczna'): continue
    b = base.get((r['skill'], 'gromada'))
    if not b or F(b, 'na_sekunde_cyklu') <= 0: continue
    sup[(r['pieczec'], r['rodzaj'])].append((F(r, 'na_sekunde_cyklu') / F(b, 'na_sekunde_cyklu'), r['skill'], F(r, 'koszt') / max(0.01, F(b, 'koszt')), F(r, 'koszt_zycia')))
out = []
for (s, kind), v in sup.items():
    m = [x[0] for x in v]
    out.append((st.median(m), min(m), max(m), s, kind, len(v), st.median([x[2] for x in v]), max(x[3] for x in v), max(v)[1], min(v)[1]))
out.sort(key=lambda t: -t[0])
for t in out:
    print(f"med x{t[0]:5.2f} min {t[1]:5.2f} max {t[2]:5.2f} ({t[8]}) koszt x{t[6]:.2f} życie {t[7]:.0f} {t[3]:28} {t[4]} n={t[5]}")
