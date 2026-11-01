"""Nakładka danych Turris na affiksy z Hell Unleashed: walidacja i deterministyczne scalanie.

Źródło nakładki: tools/data/affixes_turris.json (nowe rekordy Turris, polskie nazwy, jawne nadpisania).
- Importer HU (import_hu_economy.py) scala nakładkę po odczycie źródła, więc ponowny import jej nie usuwa.
- Bez źródła HU: `python tools/art/turris_affix_overlay.py` scala nakładkę z już zaimportowanym
  Resources/HellCatalog/affixes.json (rekordy oznaczone "source": "turris" są najpierw zdejmowane – scalenie idempotentne).
- `--check` tylko waliduje i porównuje wynik z plikiem (kod 1 przy różnicy); nic nie zapisuje.
Źródło HU pozostaje tylko do odczytu. Konflikt id z rekordem HU albo duplikat w nakładce przerywa scalanie.
"""
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OVERLAY = ROOT / 'tools/data/affixes_turris.json'
CATALOG = ROOT / 'Assets/TurrisDamnatorum/Resources/HellCatalog/affixes.json'

CATEGORIES = {'Weapon', 'Offhand', 'Head', 'Body', 'Hands', 'Feet', 'Amulet', 'Ring'}
FILTERS = {'', 'penetration', 'heavy_melee', 'minion', 'shield', 'caster'}
# Statystyki obsługiwane przez HellEconomy.MapAffix/MapEffect (nowe w pakiecie + używane w nakładce).
STATS = {'FirePenetration', 'WaterPenetration', 'AirPenetration', 'EarthPenetration',
         'HeavyPoiseDamage', 'MinionDamage', 'ShieldBlockCost', 'CastSpeed'}
OPS = {'Flat', 'PercentAdditive'}
FIELDS = ('id', 'name', 'Kind', 'Group', 'Tier', 'MinWave', 'Weight', 'Categories', 'RequiredTags', 'Ranges', 'Filter', 'Decimals')


class OverlayError(ValueError):
    pass


def validate(rec):
    rid = rec.get('id', '?')
    for f in FIELDS:
        if f not in rec:
            raise OverlayError(f'{rid}: brak pola {f}')
    if not rid.startswith('affix.turris.'):
        raise OverlayError(f'{rid}: id nakładki musi zaczynać się od affix.turris.')
    if rec['Kind'] not in (0, 1):
        raise OverlayError(f'{rid}: Kind musi być 0 (przedrostek) albo 1 (przyrostek)')
    if not isinstance(rec['Tier'], int) or rec['Tier'] < 1:
        raise OverlayError(f'{rid}: zły tier')
    if not isinstance(rec['MinWave'], int) or rec['MinWave'] < 1:
        raise OverlayError(f'{rid}: zły próg piętra')
    if not (0 < float(rec['Weight']) <= 5):
        raise OverlayError(f'{rid}: zła waga')
    if not rec['Group'] or not rec['name'] or rec['name'] == rid:
        raise OverlayError(f'{rid}: brak grupy albo polskiej nazwy')
    if not rec['Categories'] or any(c not in CATEGORIES for c in rec['Categories']):
        raise OverlayError(f'{rid}: zła kategoria {rec["Categories"]}')
    if rec['Filter'] not in FILTERS:
        raise OverlayError(f'{rid}: nieznany filtr {rec["Filter"]}')
    if not isinstance(rec['Decimals'], int) or not (-1 <= rec['Decimals'] <= 4):
        raise OverlayError(f'{rid}: zła precyzja')
    if not rec['Ranges']:
        raise OverlayError(f'{rid}: brak zakresu')
    for r in rec['Ranges']:
        p = r.split('|')
        if len(p) != 4 or p[0] not in STATS or p[1] not in OPS:
            raise OverlayError(f'{rid}: nieobsługiwany zakres {r}')
        lo, hi = float(p[2]), float(p[3])
        if not (0 < lo <= hi):
            raise OverlayError(f'{rid}: zakres {r} musi być dodatni i rosnący')


def merge(hu_affixes, overlay):
    """Rekordy HU (bez wcześniejszych rekordów Turris, w kolejności źródła) + nadpisania + nowe rekordy nakładki wg id."""
    base = [dict(a) for a in hu_affixes if a.get('source') != 'turris']
    ids = {}
    for a in base:
        if a['id'] in ids:
            raise OverlayError(f'{a["id"]}: duplikat w danych HU')
        ids[a['id']] = a
    for ov in overlay.get('overrides', []):
        rid = ov.get('id')
        if rid not in ids:
            raise OverlayError(f'{rid}: nadpisanie nieistniejącego rekordu')
        for k, v in ov.items():
            if k != 'id':
                ids[rid][k] = v
    new = []
    seen = set()
    for rec in overlay.get('affixes', []):
        validate(rec)
        if rec['id'] in ids:
            raise OverlayError(f'{rec["id"]}: konflikt id z danymi HU – nakładka nie nadpisuje po cichu')
        if rec['id'] in seen:
            raise OverlayError(f'{rec["id"]}: duplikat w nakładce')
        seen.add(rec['id'])
        out = {k: rec[k] for k in FIELDS}
        out['source'] = 'turris'
        new.append(out)
    groups = {}
    for r in new:
        groups.setdefault(r['Group'], set()).add(r['Tier'])
    for g, tiers in groups.items():
        if tiers != set(range(1, len(tiers) + 1)):
            raise OverlayError(f'{g}: tiery muszą być kolejne od 1 ({sorted(tiers)})')
        if any(a['Group'] == g for a in base):
            raise OverlayError(f'{g}: grupa koliduje z grupą HU')
    return base + sorted(new, key=lambda a: a['id'])


def load_overlay():
    return json.loads(OVERLAY.read_text(encoding='utf-8'))


def dump(affixes):
    return json.dumps({'affixes': affixes}, ensure_ascii=False, indent=1) + '\n'


def main(argv):
    current = json.loads(CATALOG.read_text(encoding='utf-8'))['affixes']
    merged = merge(current, load_overlay())
    text = dump(merged)
    if '--check' in argv:
        same = CATALOG.read_text(encoding='utf-8') == text  # read_text ujednolica końce linii – porównanie treści
        print('zgodne' if same else 'RÓŻNICA: uruchom bez --check')
        return 0 if same else 1
    CATALOG.write_text(text, encoding='utf-8')  # jak importer HU (końce linii systemu)
    print(f'{len(merged)} affiksów ({sum(1 for a in merged if a.get("source") == "turris")} z nakładki Turris) -> {CATALOG.relative_to(ROOT)}')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
