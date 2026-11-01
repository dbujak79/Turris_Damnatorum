"""Generuje docs/AFFIKSY_PRZEDMIOTOW.md (pełny katalog) i docs/AFFIKSY_SKROT.md (lista skrócona) z affixes.json.

Przeliczniki jak w grze: HellEconomy.MapAffix / MapEffect / RollAffixes (Assets/TurrisDamnatorum/Scripts/Runtime/Run/HellEconomy.cs).
Po zmianie affixes.json, nakładki tools/data/affixes_turris.json albo przeliczników uruchom ponownie:
    python tools/art/affix_docs.py
"""
import collections
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / 'Assets/TurrisDamnatorum/Resources/HellCatalog/affixes.json'
FULL = ROOT / 'docs/AFFIKSY_PRZEDMIOTOW.md'
SHORT = ROOT / 'docs/AFFIKSY_SKROT.md'


def conv(stat, op):
    """(etykieta, mnożnik HU → Turris, jednostka) – zgodnie z HellEconomy.MapAffix/MapEffect."""
    pct = op.startswith('Percent')
    m = {
        'MaxHealth': ('Maks. życie', 100, '%') if pct else ('Maks. życie', 2, ''),
        'MaxStamina': ('Maks. wytrzymałość', 1, ''),
        'MaxMana': ('Maks. mana', 1, ''),
        'HealthRegen': ('Regeneracja życia', 2, '/s'),
        'ManaRegen': ('Regeneracja many', 1, '/s'),
        'StaminaRegen': ('Regeneracja wytrzymałości', 100, '%') if pct else ('Regeneracja wytrzymałości', 1, ''),
        'Armour': ('Pancerz (obrona fizyczna)', 6, ''),
        'PhysicalResistance': ('Obrona fizyczna', 100, ''),
        'DamageReduction': ('Redukcja obrażeń', 100, '%'),
        'FireResistance': ('Odporność na ogień', 100, '%'),
        'WaterResistance': ('Odporność na mróz (woda)', 100, '%'),
        'AirResistance': ('Odporność na błyskawice (powietrze)', 100, '%'),
        'EarthResistance': ('Odporność na ziemię', 100, '%'),
        'PhysicalDamage': ('Obrażenia fizyczne', 100, '%') if pct else ('Obrażenia broni (dodane fizyczne)', 5, '%'),
        'FireDamage': ('Obrażenia od ognia', 100, '%') if pct else ('Dodany ogień (% bazy broni B)', 3.5, '%'),
        'WaterDamage': ('Obrażenia od mrozu', 100, '%') if pct else ('Dodany mróz (% bazy broni B)', 3.5, '%'),
        'AirDamage': ('Obrażenia od błyskawic', 100, '%') if pct else ('Dodane błyskawice (% bazy broni B)', 3.5, '%'),
        'EarthDamage': ('Obrażenia od ziemi', 100, '%') if pct else ('Dodana ziemia (% bazy broni B)', 3.5, '%'),
        'GlobalDamage': ('Wszystkie obrażenia', 100, '%'),
        'SpellDamage': ('Moc czarów', 100, '%'),
        'AttackSpeed': ('Szybkość ataku', 100, '%'),
        'MoveSpeed': ('Szybkość ruchu', 100, '%'),
        'SkillCooldownReduction': ('Skrócenie odnowienia umiejętności', 100, '%'),
        'LifeLeech': ('Kradzież życia', 100, '%'),
        'ExecuteDamage': ('Obrażenia egzekucji (wróg z niskim życiem)', 100, '%'),
        'BleedDamage': ('Obrażenia krwawienia', 100, '%'),
        'EquipLoad': ('Udźwig', 1, ''),
        'AreaSize': ('Obszar działania', 100, '%'),
        'Evasion': ('Uchylenie', 100, '%'),
        'PassiveBlockChance': ('Szansa na bierny blok', 100, '%'),
        'Knockback': ('Odrzut ciosów wręcz', 1, ''),
        'MeleeRange': ('Zasięg ataków wręcz', 100, '%'),
        'PhysicalPenetration': ('Przebicie obrony fizycznej', 100, '%'),
        'ProjectilePierce': ('Przebicie pocisków (liczba celów)', 1, ''),
        'ProjectileSpeed': ('Szybkość pocisków', 100, '%'),
        'LifeOnKill': ('Życie za zabójstwo', 2, ''),
        'ManaOnKill': ('Mana za zabójstwo', 1, ''),
        'SoulGain': ('Zdobywane dusze', 100, '%'),
        'Accuracy': ('Celność', 100, '%'),
        # Pakiet specjalizacyjny 2026-11-01
        'FirePenetration': ('Przebicie odporności na ogień', 100, ' p.p.'),
        'WaterPenetration': ('Przebicie odporności na mróz', 100, ' p.p.'),
        'AirPenetration': ('Przebicie odporności na błyskawice', 100, ' p.p.'),
        'EarthPenetration': ('Przebicie odporności na ziemię', 100, ' p.p.'),
        'HeavyPoiseDamage': ('Obrażenia postawy ciężkiej broni', 100, '%'),
        'MinionDamage': ('Obrażenia sług (nowe przyzwania)', 100, '%'),
        'ShieldBlockCost': ('Koszt aktywnego bloku tarczą', -100, '%'),
        'CastSpeed': ('Szybkość rzucania czarów', 100, '%'),
    }
    return m[stat]


def rnd(v, mx, op, decimals):
    if decimals is not None and decimals >= 0:
        return round(v, decimals)
    if abs(mx) >= 2 and not op.startswith('Percent'):
        return round(v)
    return round(v * 1000) / 1000


def fmt(x):
    return f'{x:.1f}'.rstrip('0').rstrip('.').replace('.', ',')


def fmtw(x):
    return f'{x:.2f}'.rstrip('0').rstrip('.').replace('.', ',')


def sign(x):
    return '+' if x >= 0 else '−'


def values(a):
    out = []
    for r in a['Ranges']:
        stat, op, lo, hi = r.split('|')
        lo, hi = float(lo), float(hi)
        lab, k, unit = conv(stat, op)
        dec = a.get('Decimals', -1)
        out.append((lab, rnd(lo, hi, op, dec) * k, rnd(hi, hi, op, dec) * k, unit))
    return out


def rng_text(lo, hi, unit):
    a, b = sign(lo) + fmt(abs(lo)) + unit, sign(hi) + fmt(abs(hi)) + unit
    return a if lo == hi else f'{a} – {b}'


CAT = {'Weapon': 'broń', 'Offhand': 'druga ręka', 'Head': 'hełm', 'Body': 'zbroja', 'Hands': 'rękawice',
       'Feet': 'buty', 'Amulet': 'amulet', 'Ring': 'pierścień'}
TAG = {'shield': 'tarcza', 'medium': 'zbroja średnia', 'light': 'zbroja lekka', 'spell': 'broń czarów',
       'heavy': 'zbroja ciężka', 'projectile': 'broń dystansowa', 'robe': 'szata', 'melee': 'broń wręcz'}
FILTER = {'penetration': 'broń, grymuar albo amulet (nie tarcza, nóż, pierścień, zbroja)',
          'heavy_melee': 'ciężka broń wręcz (WeaponFamilies; nie łuk, nie kostur)',
          'minion': 'hełm albo grymuar przywołań',
          'shield': 'tylko tarcza',
          'caster': 'broń-fokus (różdżka, kostur) albo grymuar'}


def full(affixes):
    L = ['# Affiksy przedmiotów losowych – przedrostki i przyrostki\n',
         f'Lista wygenerowana (`python tools/art/affix_docs.py`) z `Assets/TurrisDamnatorum/Resources/HellCatalog/affixes.json`: '
         f'{sum(1 for a in affixes if a.get("source") != "turris")} affiksów z Hell Unleashed i '
         f'{sum(1 for a in affixes if a.get("source") == "turris")} z nakładki Turris (`tools/data/affixes_turris.json`, '
         'pakiet specjalizacyjny 2026-11-01). Wartości przeliczone jak w grze (`Run/HellEconomy.cs`: `MapAffix`, `MapEffect`, `RollAffixes`) – '
         'to, co dostaje bohater w Turris **przed jakością**.\n',
         '## Zasady losowania\n',
         '- **Rzadkość → liczba affiksów:** zwykły 0, magiczny 1 przedrostek + 1 przyrostek, rzadki 2 + 2, legendarny 3 + 3 (od piętra 5), unikat – bez losowych affiksów.',
         '- **Grupa raz na przedmiot:** np. nie wypadną dwa affiksy szybkości ataku. Waga grupy = najwyższa waga jej odblokowanych tierów (waga względna w puli przedmiotu, nie procent dropu).',
         '- **Tier:** tylko tiery z progiem piętra ≤ bieżące piętro (piętro = fala HU); tier losowany z wagą równą numerowi (wyższe częściej).',
         '- **Wartość:** min + (max − min) · q, q losowe 0–1; przedmiot legendarny losuje q dwa razy i bierze lepsze.',
         '- **Jakość HU:** wszystkie premie affiksów × (1 + 0,25 · jakość), raz, przy zakładaniu (elita 0–1, boss 1–2 → do ×1,5). Poziom broni zwiększa bazę broni, nie procent affiksu.',
         '- **Slot, tagi i filtr:** affiks musi pasować do slotu (kolumna „Sloty”); dawne tagi HU działają jak „którykolwiek”; '
         'nowe grupy mają wąski filtr przedmiotu (kolumna „Wymaga”) – opisuje przedmiot, nie klasę gracza.',
         '- **Przeliczniki HU → Turris:** życie ×2, pancerz ×6, ułamki → %, dodane obrażenia fizyczne → +5%, dodane żywioły → +3,5% bazy broni za punkt. '
         'Wartości płaskie z max ≥ 2 (dane HU) są zaokrąglane do całości przed przeliczeniem; nowe rekordy mają zadeklarowaną precyzję (3 miejsca w jednostkach HU).',
         '- **Dodane żywioły (`fire_add`, `water_add`, `air_add`, `earth_add`):** od pakietu 2026-11-01 to **rzeczywisty składnik danego żywiołu**: '
         'dodatek = B × suma dodanych % / 100, gdzie B to obrażenia tego ataku bronią bez premii fizycznej. Działa na zwykłe ciosy, podstawowe strzały '
         'łuku/różdżki i techniki (Turris i HU), nie na czary, sługi i efekty wtórne. Premia fizyczna go nie zwiększa, premia tego żywiołu – raz; odporność '
         'i przebicie liczone osobno dla każdego żywiołu; sam dodatek nie nakłada podpalenia, chłodu ani porażenia. Stare egzemplarze (sprzed pakietu) zachowują '
         'zapisaną premię fizyczną – w opisie „Premia obrażeń broni ze starego zapisu”.',
         '- **Znane (bez zmian w tym pakiecie):** zaokrąglanie płaskich wartości HU spłaszcza zakresy – Trolla (`regen` T3) daje zawsze +4/s, Kontemplacji (`mana_regen` T2) tylko +1/s albo +2/s.',
         '- Przedrostki z HU nie mają polskich nazw (w danych `name` = id) – kolumna „Nazwa” pokazuje wtedy „—”. Przyrostki HU i wszystkie rekordy Turris mają nazwy.\n']
    for kind, title in ((0, 'Przedrostki'), (1, 'Przyrostki')):
        rows = [a for a in affixes if a['Kind'] == kind]
        groups = collections.OrderedDict()
        for a in sorted(rows, key=lambda a: (a['Group'], a['Tier'], a['id'])):
            groups.setdefault(a['Group'], []).append(a)
        L.append(f'## {title} ({len(rows)} affiksów, {len(groups)} grup)\n')
        L.append('| Grupa | Tier | Nazwa | Statystyka | Zakres | Od piętra | Waga | Sloty | Wymaga |')
        L.append('|---|---|---|---|---|---|---|---|---|')
        for g, tiers in groups.items():
            for a in tiers:
                vs = values(a)
                stat = '<br>'.join(v[0] for v in vs)
                val = '<br>'.join(rng_text(v[1], v[2], v[3]) for v in vs)
                name = a['name'] if a['name'] != a['id'] else '—'
                slots = ', '.join(CAT.get(c, c) for c in a['Categories'])
                req = FILTER.get(a.get('Filter') or '', '') or ', '.join(TAG.get(t, t) for t in a['RequiredTags']) or '—'
                mark = ' *(Turris)*' if a.get('source') == 'turris' else ''
                L.append(f"| `{g}`{mark} | T{a['Tier']} | {name} | {stat} | {val} | {a['MinWave']} | {fmtw(a['Weight'])} | {slots} | {req} |")
        L.append('')
    L.append('## Działanie nowych grup (pakiet 2026-11-01)\n')
    L.append('- **Przebicie żywiołu** (`fire_pen`, `water_pen`, `air_pen`, `earth_pen`): odejmuje punkty procentowe od odporności celu na ten żywioł '
             '(50% → 40% przy 10 p.p.), tylko dla składnika tego żywiołu, nie schodzi poniżej 0, nie zmienia podatności. Bezpośrednie trafienia gracza, '
             'także czary; nie tyknięcia statusów i nie sługi.')
    L.append('- **Kruszący** (`poise_damage`): mnoży obrażenia postawy zwykłych ciosów i technik wręcz wykonanych ciężką bronią; bez riposty, fali ciosu '
             'ładowanego, czarów, sług i efektów wtórnych. Zdrowie bez zmian.')
    L.append('- **Władczy** (`minion_damage`): obrażenia sług przyzwanych **po** założeniu (migawka przy przyzwaniu; istniejący sługa się nie zmienia). '
             'Chimera dziedziczy migawki składników raz; Wcielenie gracza nie korzysta.')
    L.append('- **Muru tarcz** (`shield_block_cost`): koszt aktywnego bloku założoną tarczą × (1 − suma, najwyżej 50%), raz, po starych perkach; '
             'bez wpływu na blok bierny, okno parowania i ciosy nieblokowalne.')
    L.append('- **Inkantacji** (`cast_speed`): przygotowanie ręcznie rzucanego czaru / (1 + suma); bez recovery, odnowienia, kanału, technik, flaszek, '
             'parowania i uniku. Animacja przygotowania idzie fazą akcji, więc wyzwala się razem z rozgrywką.\n')
    return '\n'.join(L)


def short(affixes):
    NAMES = {'all_res': 'Wszystkie odporności', 'armour': 'Pancerz', 'health': 'Maks. życie (płaskie)', 'health_pct': 'Maks. życie (%)',
             'phys_add': 'Dodane obrażenia fizyczne (jako % obrażeń broni)',
             'fire_add': 'Dodany ogień (rzeczywisty składnik, % bazy broni)',
             'water_add': 'Dodany mróz (rzeczywisty składnik, % bazy broni)',
             'air_add': 'Dodane błyskawice (rzeczywisty składnik, % bazy broni)',
             'earth_add': 'Dodana ziemia (rzeczywisty składnik, % bazy broni)',
             'shield_block_cost': 'Tańszy aktywny blok tarczą (limit 50%)',
             'minion_damage': 'Obrażenia sług (przy przyzwaniu)',
             'poise_damage': 'Obrażenia postawy ciężkiej broni',
             'cast_speed': 'Szybkość rzucania czarów'}
    EXTRA = {'spell_pct': ' (broń czarów do +24%; zbroja, szata, biżuteria do +16%)'}

    def line(grp, tiers):
        vs = [values(a) for a in tiers]
        lab, _, _, unit = vs[0][0]
        lo = min(v[0][1] for v in vs)
        hi = max(v[0][2] for v in vs)
        if lo < 0:  # koszt bloku: od najmniejszej do największej redukcji
            lo, hi = max(v[0][1] for v in vs), min(v[0][2] for v in vs)
        mark = ' *(nowe)*' if tiers[0].get('source') == 'turris' else ''
        return f'- {NAMES.get(grp, lab)}: **{rng_text(lo, hi, unit)}**{EXTRA.get(grp, "")}{mark}'

    L = ['# Affiksy przedmiotów – lista skrócona\n',
         'Jedna linia na grupę: od minimum najniższego tieru do maksimum najwyższego (przed jakością HU, która mnoży do ×1,5). '
         'Pełne dane z tierami, piętrami, slotami i filtrami: [AFFIKSY_PRZEDMIOTOW.md](AFFIKSY_PRZEDMIOTOW.md). '
         'Wygenerowano: `python tools/art/affix_docs.py`.\n',
         '**Dodane żywioły** dodają od pakietu 2026-11-01 rzeczywisty składnik żywiołu (B × % / 100, z własną premią i odpornością); '
         'przedmioty z wcześniejszych zapisów zachowują swoją premię fizyczną („Premia obrażeń broni ze starego zapisu”).\n']
    for kind, title in ((0, 'Przedrostki'), (1, 'Przyrostki')):
        groups = collections.defaultdict(list)
        for a in affixes:
            if a['Kind'] == kind:
                groups[a['Group']].append(a)
        lines = sorted((line(k, t) for k, t in groups.items()), key=str.lower)
        L.append(f'## {title} ({len(lines)})\n')
        L += lines
        L.append('')
    return '\n'.join(L)


def main():
    affixes = json.loads(SRC.read_text(encoding='utf-8'))['affixes']
    FULL.write_text(full(affixes), encoding='utf-8')
    SHORT.write_text(short(affixes), encoding='utf-8')
    print(f'{len(affixes)} affiksów -> {FULL.relative_to(ROOT)}, {SHORT.relative_to(ROOT)}')


if __name__ == '__main__':
    main()
