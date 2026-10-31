"""Kopiuje 30 fal Hell Unleashed do Resources/HellCatalog/waves.json (źródło tylko do odczytu).

Wieża Turris to wierny port fal HU: piętro N = fala N (portale, bossowie co trzecie piętro). Pozostałe dane
katalogu (skille, pieczęcie, bronie, demony) kopiuje import_hu_catalog.py – ten skrypt ich nie dotyka.
Uruchomienie: python tools/art/import_hu_waves.py [ścieżka do Hell Unleashed]
"""
import hashlib
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(r'C:\GIT\Hell Unleashed')
OUT = ROOT / 'Assets/TurrisDamnatorum/Resources/HellCatalog/waves.json'


def field(text, key, cast=str, default=None):
    m = re.search(rf'^{key} = (.*)$', text, re.M)
    if not m:
        return default
    v = m.group(1).strip().strip('"')
    return cast(v) if cast is not bool else v == 'true'


def main():
    rows = []
    for p in sorted((SOURCE / 'src/Game/Content/Waves').glob('wave_*.tres')):
        t = p.read_text(encoding='utf-8-sig')
        rows.append({
            'Index': field(t, 'Index', int),
            'sha256': hashlib.sha256(p.read_bytes()).hexdigest(),
            'IsBossWave': field(t, 'IsBossWave', bool, False),
            'BossEnemyId': field(t, 'BossEnemyId', str, ''),
            'DurationSeconds': field(t, 'DurationSeconds', float, 75.0),
            'SpawnBudget': field(t, 'SpawnBudget', float, 0.0),
            'GroupIntervalSeconds': field(t, 'GroupIntervalSeconds', float, 3.0),
            'MinGroupSize': field(t, 'MinGroupSize', int, 1),
            'MaxGroupSize': field(t, 'MaxGroupSize', int, 2),
            'Composition': re.findall(r'"(enemy\.[^"|]+\|[\d.]+)"', t),
            'HealthMultiplier': field(t, 'HealthMultiplier', float, 1.0),
            'DamageMultiplier': field(t, 'DamageMultiplier', float, 1.0),
            'SpeedMultiplier': field(t, 'SpeedMultiplier', float, 1.0),
            'SoulRewardMultiplier': field(t, 'SoulRewardMultiplier', float, 1.0),
        })
    rows.sort(key=lambda r: r['Index'])
    assert [r['Index'] for r in rows] == list(range(1, 31)), [r['Index'] for r in rows]
    OUT.write_text(json.dumps({'waves': rows}, ensure_ascii=False, indent=1) + '\n', encoding='utf-8')
    print(f'{len(rows)} fal -> {OUT.relative_to(ROOT)}')


if __name__ == '__main__':
    main()
