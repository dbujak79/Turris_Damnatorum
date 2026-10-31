"""Archiwum koncepcji bohaterów z kontrolą integralności i zachowaniem linków."""
import hashlib
import json
import zipfile
from pathlib import Path

root = Path(__file__).resolve().parents[2]
heroes = root / 'ArtSource/GothicAnimePoC/Heroes'
destination = root / 'ArtSource/Archives'
destination.mkdir(exist_ok=True)
archive = destination / 'Turris_Damnatorum_concept_arty_bohaterow_2026-10-27_v1.zip'
if archive.exists():
    raise FileExistsError('Zachowano istniejące archiwum; dla kolejnego użyj nowej wersji.')
files = [p for p in heroes.iterdir() if p.is_file() and p.suffix in ('.png','.json','.html','.md','.py')]
files += [root / p for p in (
    'ArtSource/GothicAnimePoC/Paladin/reference-palette.png',
    'ArtSource/GothicAnimePoC/Mage/mage-gothic-anime-v1.png',
    'ArtSource/GothicAnimePoC/Mage/concept-prompt.txt',
    'ArtSource/GothicAnimePoC/Arena/reference-courtyard.png',
    'docs/STYL_GRAFICZNY_GOTYCKIE_ANIME.md')]
assert len(list(heroes.glob('*-gothic-anime-v1.png'))) == 13
snapshot = {p.relative_to(root).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(set(files))}
with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6) as output:
    for path in sorted(set(files)):
        output.write(path,path.relative_to(root).as_posix())
    output.writestr('SUMY_SHA256.json',json.dumps(snapshot,ensure_ascii=False,indent=2))
    output.writestr('CZYTAJ.txt','Turris Damnatorum — koncepcje bohaterów, 2026-10-27.\n'
        '13 nowych plansz + istniejące koncepcje paladyna i maga.\n'
        'Wypakuj cały ZIP i otwórz ArtSource/GothicAnimePoC/Heroes/galeria.html.\n'
        'Dwa wcześniejsze projekty znajdują się w folderach Paladin i Mage.\n'
        'Dołączono podglądy źródłowe, prompty, manifest i specyfikację stylu.\n'
        'Bez modeli 3D. Plansza z wieloma widokami wymaga osobnej referencji do Meshy.\n')
with zipfile.ZipFile(archive) as output:
    assert output.testzip() is None
    for name,digest in snapshot.items():
        assert hashlib.sha256(output.read(name)).hexdigest() == digest, name
digest = hashlib.sha256(archive.read_bytes()).hexdigest()
archive.with_suffix('.zip.sha256').write_text(digest+'  '+archive.name+'\n',encoding='utf-8')
print(json.dumps(dict(archive=str(archive),bytes=archive.stat().st_size,files=len(snapshot)+2,
                      new_concepts=13,previous_concepts=2,sha256=digest,verified=True),ensure_ascii=False,indent=2))
