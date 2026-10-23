"""Katalog i audyt HU. Źródło otwierane wyłącznie do odczytu; zapis tylko w Turris.

python tools/art/import_hu_catalog.py [--verify]
Audyt czyta również eksport fabryki Unity (HellMigrationImport.ExportAudit).
"""
import argparse
import hashlib
import json
import math
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path('C:/GIT/Hell Unleashed')
OUT = ROOT / 'Assets/TurrisDamnatorum/Resources/HellCatalog'
SKILLS = {
    **{f'skill.sub.{n}': f'hu_{n}' for n in 'arrow_storm blade_dance elemental_arrow judgement maelstrom death_mark legion meteor quake storm_cleave tempest undying'.split()},
    **{f'skill.warrior.{n}': f'hu_{n}' for n in 'cleave wind_slash warcry'.split()},
    **{f'skill.rogue.{n}': f'hu_{n}' for n in 'piercing_shot splinter_arrow cross_cut snare knife_throw blade_fan'.split()},
    **{f'skill.nova.{n}': f'hu_nova_{n}' for n in 'fire water air earth'.split()},
    'skill.aura.holy_fire': 'hu_aura_holy_fire', 'skill.aura.defiance': 'hu_aura_defiance',
    'skill.summon.imps': 'hu_summon_imps', 'skill.mage.overload': 'hu_overload',
}
SKILLS.pop('skill.nova.fire')
SKILLS['skill.explosion'] = 'hu_nova_fire'
WEAPONS = {f'weapon.{n}': f'hu_{n}' for n in 'sword greatsword greataxe warhammer flail shield bow dagger offdagger'.split()}
WEAPONS.update({f'weapon.{n.replace("_", ".")}': f'hu_{n}' for n in 'sword_broad sword_fire sword_water sword_air sword_earth wand_water wand_air wand_earth grimoire_water grimoire_air grimoire_earth grimoire_summoning'.split()})
WEAPONS.update({'weapon.wand': 'hu_wand_fire', 'weapon.grimoire': 'hu_grimoire_fire', 'weapon.dagger.poniard': 'hu_poniard'})
WEAPONS.update({f'weapon.{n.replace("_", ".")}': f'hu_{n}' for n in 'flail2h flail2h_iron morningstar2h morningstar2h_runic staff greataxe_bearded greataxe_bardiche warhammer_runic warhammer_siege'.split()})
ENEMIES = {'enemy.imp': 'enemy_imp', 'enemy.cultist': 'enemy_hu_cultist', 'enemy.worm': 'enemy_worm', 'enemy.minion': 'enemy_minion', 'enemy.boss': 'enemy_gatelord'}


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def value(text):
    text = text.strip()
    if text.startswith(('Packed', 'Array[')):
        inner = text[text.index('(') + 1:-1]
        return json.loads('[' + inner + ']')
    if text in ('true', 'false'):
        return text == 'true'
    try:
        return json.loads(text)
    except ValueError:
        return text


def read_resource(path):
    section = path.read_text(encoding='utf-8-sig').split('[resource]', 1)[1]
    return {k: value(v) for k, v in re.findall(r'^(\w+) = (.+)$', section, re.M) if k != 'script'}


def source_defaults(kind):
    path = SOURCE / f'src/Game/Scripts/Content/{kind}Definition.cs'
    text = path.read_text(encoding='utf-8-sig')
    return {name: value(v.rstrip('f')) for name, v in re.findall(r'public (?:float|int|bool) (\w+) \{ get; set; \} = ([\d.f-]+|true|false);', text)}


def parameters(entries):
    return [{'key': k, 'value': float(v)} for k, v in (s.split('=', 1) for s in entries)]


def ability(text):
    head, _, tail = text.partition('|')
    name, _, arg = head.partition(':')
    return {'name': name, 'argument': arg, 'parameters': parameters(tail.split(',')) if tail else [], 'raw': text}


def generate():
    strings = (SOURCE / 'src/Game/Scripts/Infrastructure/Strings.cs').read_text(encoding='utf-8-sig')
    names = dict(re.findall(r'\["([^"\n]+)"\] = "([^"\n]*)"', strings))
    model_text = (SOURCE / 'src/Game/Scripts/World/EnemyActor.cs').read_text(encoding='utf-8-sig').split('public static (string Path, float Height) For')[1]
    models = {k: (model, float(height)) for k, model, height in re.findall(r'"(enemy\.[^"]+)" => \(ModelLibrary.Demon\("([^"]+)"\), ([\d.]+)f\)', model_text)}
    models.update({'enemy.imp': ('imp', 1.3), 'enemy.brute': ('brute', 2.4), 'enemy.boss': ('gatelord', 3.8)})
    catalog = {'skills': [], 'weapons': [], 'enemies': []}
    files = {}
    for category, kind, mapping in [('Skills', 'Skill', SKILLS), ('Weapons', 'Weapon', WEAPONS), ('Enemies', 'Enemy', ENEMIES)]:
        defaults = source_defaults(kind)
        for p in sorted((SOURCE / 'src/Game/Content' / category).glob('*.tres')):
            fields = defaults | read_resource(p)
            sid = fields['Id']
            target = mapping.get(sid, '')
            if kind == 'Enemy' and not target:
                target = 'enemy_hu_' + sid.removeprefix('enemy.').replace('.', '_')
            row = {'id': sid, 'targetId': target, 'file': p.relative_to(SOURCE).as_posix(), 'sha256': sha(p),
                   'displayName': names.get(fields.get('NameKey'), sid), 'fields': fields}
            if kind == 'Skill':
                merged = {}
                row['levels'] = []
                for level in range(1, 4):
                    merged.update({e['key']: e['value'] for e in parameters(fields.get(f'Level{level}', []))})
                    row['levels'].append(parameters([f'{k}={v}' for k, v in merged.items()]))
            if kind == 'Enemy':
                row['model'], row['height'] = models[sid]
                row['abilities'] = [ability(s) for s in fields.get('BossAbilities', fields.get('Specials', []))]
            catalog[category.lower()].append(row)
            files[row['file']] = row['sha256']
    # Wszystkie źródła kodu, katalogu i użyte modele – kontrola niezmienności po migracji.
    for base in ['src/Game/Scripts', 'src/Game.Core', 'src/Game/Content', 'src/Game/Assets/Models']:
        for p in sorted((SOURCE / base).rglob('*')):
            if p.is_file():
                files[p.relative_to(SOURCE).as_posix()] = sha(p)
    return catalog, files


def yaml_assets():
    import yaml
    result = {}
    for p in (ROOT / 'Assets/TurrisDamnatorum/Content').rglob('*.asset'):
        raw = p.read_text(encoding='utf-8-sig')
        raw = re.sub(r'^%.*\n|^--- !u!.*\n', '', raw, flags=re.M)
        data = yaml.safe_load(raw)
        if isinstance(data, dict) and isinstance(data.get('MonoBehaviour'), dict):
            fields = data['MonoBehaviour']
            if fields.get('id'):
                result[fields['id']] = fields
    return result


def flat(data, prefix=''):
    out = {}
    for k, v in data.items():
        if k in ('m_ObjectHideFlags', 'm_CorrespondingSourceObject', 'm_PrefabInstance', 'm_PrefabAsset', 'm_GameObject', 'm_Enabled', 'm_EditorHideFlags', 'm_Script', 'm_Name', 'm_EditorClassIdentifier'):
            continue
        key = prefix + k
        if isinstance(v, dict):
            out.update(flat(v, key + '.'))
        else:
            out[key] = v
    return out


def audit(catalog):
    assets = yaml_assets()
    factory_path = ROOT / 'TestResults/hu-factory.json'
    factory = json.loads(factory_path.read_text(encoding='utf-8-sig')) if factory_path.exists() else {'entries': []}
    factory = {e['id']: json.loads(e['json']) for e in factory['entries']}
    report = ['# Audyt P0: katalog Hell Unleashed → Turris', '',
              'Generowane przez `tools/art/import_hu_catalog.py`. Dane źródła wraz z SHA i kompletem parametrów są w `Resources/HellCatalog/catalog.json`.', '',
              'Indeksy poziomów: HU 1–3, Turris +0–+2. `maxLevel=2` oznacza trzeci poziom. '
              'Turris zachowuje koszt akcji, ładunki, perki, skalowanie atrybutami, pancerz i reguły bloku; '
              'przyrost obrażeń/kosztów/odnowienia jest lokalny, nie jest kopią krzywych HU. '
              'Bronie mają dwa ataki, serie, postawę, gardę i parowanie zamiast pojedynczego ataku HU. '
              'Woda → mróz, powietrze → błyskawice; wygląd rozróżnia HellSkillFx. '
              'Mnożnik 2,5 dotyczy bazowych obrażeń przeniesionych skilli; bronie i przeciwnicy mają osobny balans.', '',
              '## Kontrola zapisanych assetów względem fabryki', '']
    drifts = []
    for group in ('skills', 'weapons'):
        report += [f'## {group} ({len(catalog[group])})', '', '| HU id | Turris id | Różnica / dane HU → asset Turris | Test |', '|---|---|---|---|']
        for r in catalog[group]:
            tid = r['targetId']
            a = assets.get(tid)
            if a is None:
                detail, test = 'Brak odpowiednika HU (nie utożsamiać z podobnym skillem/bronią Turris).', 'HellMigrationTests.Catalog_HasUniqueMappingsAndSourceHashes'
            elif group == 'skills':
                levels = [{p['key']: p['value'] for p in level} for level in r['levels']]
                keys = sorted(set().union(*(l.keys() for l in levels)))
                source = '; '.join(f'{k}=' + '/'.join(str(l[k]) if k in l else '—' for l in levels) for k in keys if any(k in l for l in levels))
                target = '; '.join(f'{k}={a.get(k)}' for k in ('manaCost', 'staminaCost', 'cooldown', 'castTime', 'recovery', 'telegraph', 'pulses', 'pierce', 'duration', 'tickInterval', 'zoneRadius', 'zoneDuration', 'maxLevel') if k in a)
                target += '; atak=' + json.dumps({k: v for k, v in a.get('attack', {}).items() if k in ('baseDamage', 'element', 'elementShare', 'reach', 'radius', 'projectileCount', 'projectileSpeed', 'statuses')}, ensure_ascii=False)
                detail = f'HU poziomy: {source}. Turris +0: {target}. Krzywa Turris: power={a.get("powerPerLevel")}, koszt={a.get("costReductionPerLevel")}, cooldown={a.get("cooldownReductionPerLevel")}; cechy={json.dumps(a.get("levelFeatures", []), ensure_ascii=False)}.'
                test = 'HeroSkillPlayTests / HellMigrationTests'
            else:
                f = r['fields']
                attack = a.get('weapon', {})
                detail = 'HU: ' + '; '.join(f'{k}={f[k]}' for k in ('Handedness', 'StaminaCost', 'Range', 'Windup', 'Recovery', 'AttacksPerSecond', 'OnHitEffects') if k in f)
                detail += '. Turris: ' + '; '.join(f'{side}={json.dumps({k:v for k,v in attack.get(side, {}).items() if k in ("baseDamage", "windup", "active", "recovery", "reach", "radius", "staminaCost", "statuses")}, ensure_ascii=False)}' for side in ('light', 'heavy'))
                test = 'HeroClassTests / HellMigrationTests'
            if a is not None and tid in factory:
                af, df = flat(a), flat(factory[tid])
                differences = {k: {'asset': af.get(k), 'factory': v} for k, v in df.items() if k in af and not equal(af[k], v) and not k.endswith(('fileID', 'guid', 'type', 'instanceID'))}
                if differences:
                    drifts.append({'id': tid, 'differences': differences})
                    detail += ' Rozbieżność fabryka/asset: ' + ', '.join(differences) + ' (pełne wartości w audit.json).'
            report.append(f'| `{r["id"]}` | `{tid or "—"}` | {detail.replace("|", "/")} | {test} |')
    report[report.index('## Kontrola zapisanych assetów względem fabryki')+2:report.index('## Kontrola zapisanych assetów względem fabryki')+2] = [f'Eksport fabryki: {"obecny" if factory else "jeszcze nie wykonany"}; rozbieżności: {len(drifts)}. Referencje Unity i modele importowane w edytorze są porównywane testami, nie jako numery fileID.', '']
    (ROOT / 'docs/AUDYT_HU_P0.md').write_text('\n'.join(report)+'\n', encoding='utf-8')
    (OUT / 'audit.json').write_text(json.dumps({'factoryPresent': bool(factory), 'drifts': drifts}, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print('Audyt:', len(catalog['skills']), 'skilli,', len(catalog['weapons']), 'broni; rozbieżności:', len(drifts))


def equal(a, b):
    if isinstance(a, (float, int)) and isinstance(b, (float, int)):
        return math.isclose(a, b, rel_tol=1e-6, abs_tol=1e-6)
    if isinstance(a, list) and isinstance(b, list):
        return len(a) == len(b) and all(equal(x, y) for x, y in zip(a, b))
    if isinstance(a, dict) and isinstance(b, dict):
        return a.keys() == b.keys() and all(equal(a[k], b[k]) for k in a)
    return a == b


def enemy_report(catalog):
    report = ['# Katalog demonów HU w Turris', '',
              'Generowane przez `tools/art/import_hu_catalog.py` z rzeczywistych `.tres` oraz mapowania `EnemyModels.For`.',
              'Wszystkie modele i klipy są istniejącymi zasobami Hell Unleashed przekonwertowanymi GLB → FBX. Nie tworzono nowych modeli.', '',
              'Wysokość podano w metrach; HP i parametry zdolności w jednostkach źródła. Adapter Unity przelicza odległości /24 i nowe HP/obrażenia ×2,5. '
              'Pięć wcześniejszych definicji zachowuje balans Turris. Szczegóły i ograniczenia: [raport P0/P1](RAPORT_MIGRACJI_P0_P1.md).', '',
              'Każda pozycja ma test importu modelu/klipów (`AllNewModels_HaveClips_AndWeaponAssetsHaveModels`). '
              'Nowe zdolności i kroki kombinacji obejmuje `EveryImportedAbility_Releases_AndParryCancelsBeforeHit`; poprzednie demony — `DemonPlayTests`. '
              'Galeria: `TestResults/hu-p1/p1_demons_*.png`; próba walki dostępna w bestiariuszu.', '',
              '| HU id → Turris id | Model / wysokość | HP HU | Fazy HP / zdolności | SHA-256 definicji |',
              '|---|---|---:|---|---|']
    for row in catalog['enemies']:
        f = row['fields']
        abilities = f.get('BossAbilities', []) + f.get('Specials', [])
        detail = 'Progi: ' + str(f.get('BossPhaseThresholds', [])) + '; ' + '; '.join(abilities)
        report.append(f'| `{row["id"]}` → `{row["targetId"]}` | `{row["model"]}` / {row["height"]} m | {f["MaxHealth"]} | {detail.replace("|", "/")} | `{row["sha256"]}` |')
    (ROOT / 'docs/KATALOG_DEMONOW_HU.md').write_text('\n'.join(report)+'\n', encoding='utf-8')


def main():
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument('--verify', action='store_true')
    args = parser.parse_args()
    if args.verify:
        manifest = json.loads((OUT / 'source_manifest.json').read_text(encoding='utf-8'))
        failures = [rel for rel, digest in manifest.items() if not (SOURCE / rel).is_file() or sha(SOURCE / rel) != digest]
        if failures:
            raise SystemExit('Zmienione źródła HU: ' + ', '.join(failures))
        print('HU bez zmian:', len(manifest), 'plików.'); return
    catalog, files = generate()
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / 'catalog.json').write_text(json.dumps(catalog, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    runtime = {'enemies': [r['fields'] | {k: r[k] for k in ('targetId', 'model', 'height', 'sha256', 'displayName')} for r in catalog['enemies']]}
    (OUT / 'enemies.json').write_text(json.dumps(runtime, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    (OUT / 'weapons.json').write_text(json.dumps({'weapons': [r['fields'] | {k: r[k] for k in ('targetId', 'sha256', 'displayName')} for r in catalog['weapons']]}, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    manifest = OUT / 'source_manifest.json'
    # Nigdy nie zastępuj punktu odniesienia po rozpoczęciu migracji.
    if not manifest.exists():
        manifest.write_text(json.dumps(files, indent=2)+'\n', encoding='utf-8')
    audit(catalog)
    enemy_report(catalog)


if __name__ == '__main__':
    main()
