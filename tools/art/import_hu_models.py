"""Eksport brakujących modeli P1 do Turris, bez nadpisywania istniejących FBX i bez zapisu w HU."""
import concurrent.futures
import hashlib
import json
import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path('C:/GIT/Hell Unleashed/src/Game/Assets/Models')
BLENDER = Path('C:/Program Files/Blender Foundation/Blender 5.2/blender.exe')
OUT = ROOT / 'Assets/TurrisDamnatorum/Models'
LOGS = ROOT / 'TestResults/hu-model-import'


def convert(job):
    category, source, name = job
    src = SOURCE / category / (source + '.glb')
    folder = OUT / category / name
    fbx = folder / (name + '.fbx')
    folder.mkdir(parents=True, exist_ok=True)
    if not fbx.exists():
        converter = 'convert_hu_demon.py' if category == 'Demons' else 'convert_hu_weapon.py'
        dest = fbx if category == 'Demons' else folder
        with (LOGS / (name + '.log')).open('w', encoding='utf-8') as log:
            subprocess.run([str(BLENDER), '-b', '--python', str(ROOT / 'tools/art' / converter), '--', str(src), str(dest)], stdout=log, stderr=subprocess.STDOUT, check=True, cwd=ROOT)
        if category == 'Weapons' and source != name:
            (folder / (source + '.fbx')).rename(fbx)
    subprocess.run([sys.executable, str(ROOT / 'tools/art/hu_demon_textures.py'), str(src), name, str(folder)], check=True, stdout=subprocess.DEVNULL)
    if category == 'Demons':
        beats = src.with_suffix('.beats.json')
        if beats.exists() and not (folder / (name + '.beats.json.txt')).exists():
            shutil.copyfile(beats, folder / (name + '.beats.json.txt'))
    print('Gotowe:', category, name, flush=True)
    return {'source': src.as_posix(), 'sha256': hashlib.sha256(src.read_bytes()).hexdigest(), 'target': fbx.relative_to(ROOT).as_posix()}


def main():
    LOGS.mkdir(parents=True, exist_ok=True)
    catalog = json.loads((ROOT / 'Assets/TurrisDamnatorum/Resources/HellCatalog/catalog.json').read_text(encoding='utf-8'))
    models = sorted({e['model'] for e in catalog['enemies']})
    jobs = [('Demons', n, n) for n in models if not (OUT / 'Demons' / n / (n + '.fbx')).exists()]
    jobs += [('Weapons', n, n) for n in ['flail_2h', 'morningstar_2h', 'mage_staff', 'mage_staff_summoner', 'mage_staff_witch']]
    # Aktualne topór i młot z HU dostają nowe foldery; wcześniejsze klipy i eksporty pozostają.
    jobs += [('Weapons', n, n + '_hu_p1') for n in ['greataxe', 'warhammer']]
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
        result = list(pool.map(convert, jobs))
    path = OUT / 'hu_p1_manifest.json'
    previous = json.loads(path.read_text(encoding='utf-8')) if path.exists() else []
    merged = {r['target']: r for r in previous + result}
    path.write_text(json.dumps(list(merged.values()), indent=2)+'\n', encoding='utf-8')


if __name__ == '__main__':
    main()
