"""Kopiuje ekonomię bohatera z Hell Unleashed (etap 6): przedmioty, affixy i perki do Resources/HellCatalog.

Wyjście: items.json (71 zbroi, biżuterii i unikatów), affixes.json (131 affixów w 46 grupach), upgrades.json (46 perków).
Nazwy po polsku z tabel napisów HU. Źródło tylko do odczytu; pozostałych plików katalogu nie dotyka.
Uruchomienie: python tools/art/import_hu_economy.py
"""
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from import_hu_catalog import ROOT, SOURCE, OUT, read_resource, sha, strings_table  # noqa: E402

KINDS = {0: 'Head', 1: 'Body', 2: 'Hands', 3: 'Feet', 4: 'Amulet', 5: 'Ring'}


def rows(folder, names, fields):
    out = []
    for p in sorted((SOURCE / 'src/Game/Content' / folder).glob('*.tres')):
        f = read_resource(p)
        row = {'id': f['Id'], 'file': p.stem, 'sha256': sha(p), 'name': names.get(f.get('NameKey', ''), f['Id'])}
        if 'DescriptionKey' in f:
            row['desc'] = names.get(f['DescriptionKey'], '')
        for k, default in fields.items():
            row[k] = f.get(k, default)
        out.append(row)
    return out


def main():
    names = strings_table()
    items = rows('Items', names, {'Kind': 0, 'Rarity': 0, 'Modifiers': [], 'Tags': [], 'Power': 0, 'PowerValue': 0.0, 'ClassId': '',
                                  'BasePrice': 40.0, 'MinWave': 1, 'InShop': True, 'Weight': 1.0, 'Load': 0.0})
    for r in items:
        r['KindName'] = KINDS.get(r['Kind'], '?')
    affixes = rows('Affixes', names, {'Kind': 0, 'Group': '', 'Tier': 1, 'MinWave': 1, 'Weight': 1.0, 'Categories': [], 'RequiredTags': [], 'Ranges': []})
    upgrades = rows('Upgrades', names, {'Scope': 0, 'AttackTags': [], 'Modifiers': [], 'ModifierLevelTotals': [], 'Conversion': '', 'OnHitEffects': [],
                                        'RequiresAny': [], 'MaxLevel': 5, 'BasePrice': 20.0, 'PriceGrowth': 1.5, 'Weight': 1.0, 'ClassId': '',
                                        'RequiredSlot': 0, 'RequiredTags': [], 'ForbiddenTags': []})
    for name, data in [('items', items), ('affixes', affixes), ('upgrades', upgrades)]:
        (OUT / f'{name}.json').write_text(json.dumps({name: data}, ensure_ascii=False, indent=1) + '\n', encoding='utf-8')
        print(f'{len(data)} {name} -> {(OUT / (name + ".json")).relative_to(ROOT)}')


if __name__ == '__main__':
    main()
