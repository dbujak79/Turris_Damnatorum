"""Odbudowuje katalog źródłowych ikon unikatów; nie zmienia obrazów."""

from __future__ import annotations

import argparse
import hashlib
import html
import json
import re
from collections import Counter
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "ArtSource/Unikaty/ProsteIkony_v1"


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--wymagaj-kompletu", action="store_true")
    args = parser.parse_args()
    plan = json.loads((ART / "plan_grafik.json").read_text(encoding="utf-8"))["przedmioty"]
    if len(plan) != 75 or any(len({item[key] for item in plan}) != 75 for key in ("kod", "id", "plik")):
        raise SystemExit("Plan musi zawierać 75 różnych kodów, id i nazw plików.")
    class_counts = Counter(item["id_klasy"] for item in plan)
    if len(class_counts) != 15 or set(class_counts.values()) != {5}:
        raise SystemExit("Plan musi zawierać 15 klas po 5 unikatów.")
    definitions = (ROOT / "docs/PROMPT_WDROZENIE_75_UNIKATOW_KLASOWYCH.md").read_text(encoding="utf-8")
    if any(item["id"] not in definitions or f"### {item['kod']}" not in definitions for item in plan):
        raise SystemExit("Karta lub id nie występuje w specyfikacji unikatów.")
    old = json.loads((ART / "manifest.json").read_text(encoding="utf-8"))
    old_by_code = {item["kod"]: item for item in old["przedmioty"]}
    records = []
    prompts = []
    errors = []
    for card in plan:
        item = {key: value for key, value in card.items() if key not in {"prompt", "moc", "status"}}
        path = ART / card["plik"]
        item["status"] = "zapisany" if path.is_file() else "do_wygenerowania"
        if path.is_file():
            with Image.open(path) as im:
                item["szerokosc_px"], item["wysokosc_px"] = im.size
                item["tryb"] = im.mode
                item["przezroczystosc"] = "A" in im.getbands() and im.getchannel("A").getextrema() == (0, 255)
                if not item["przezroczystosc"]:
                    errors.append(f"{card['kod']}: brak pełnej przezroczystości RGBA")
                if "A" in im.getbands():
                    item["obrys_alfa_16"] = im.getchannel("A").point(lambda x: 255 if x >= 16 else 0).getbbox()
            item["sha256"] = hashlib.sha256(path.read_bytes()).hexdigest()
        elif args.wymagaj_kompletu:
            errors.append(f"{card['kod']}: brak pliku")
        record_path = ART / "rejestr_generowania" / f"{card['kod']}.json"
        if record_path.is_file():
            record = json.loads(record_path.read_text(encoding="utf-8"))
            if record.get("id") != card["id"] or record.get("plik") != card["plik"]:
                errors.append(f"{card['kod']}: niezgodność rejestru generowania z planem")
            item["rejestr_generowania"] = str(record_path.relative_to(ART)).replace("\\", "/")
            prompts.append((card, record["prompt"]))
        elif card.get("prompt"):
            prompts.append((card, card["prompt"]))
        else:
            item["prompt_pierwotny"] = "PROMPTY.md"
            if card["kod"] not in old_by_code:
                errors.append(f"{card['kod']}: brak pierwotnego wpisu")
        records.append(item)
    count = sum(item["status"] == "zapisany" for item in records)
    manifest = {
        "wersja_schematu": 2,
        "data": "2026-10-31",
        "projekt": "Turris Damnatorum",
        "status": "źródłowe ikony 2D; brak integracji z UI",
        "liczba_grafik": count,
        "liczba_unikatow_w_projekcie": 75,
        "dyspozycja_autora": old["dyspozycja_autora"],
        "styl": "małe, proste, uproszczone malarskie dark fantasy",
        "narzedzie": "wbudowane image_gen",
        "docelowy_rozmiar_wyswietlania_px": [32, 64],
        "eksporty_male_wykonane": False,
        "sprawdzenie_w_ui_wykonane": False,
        "definicje_przedmiotow": "../../../docs/PROMPT_WDROZENIE_75_UNIKATOW_KLASOWYCH.md",
        "prompty": ["PROMPTY.md", "PROMPTY_KONTYNUACJA.md"],
        "przedmioty": records,
    }
    (ART / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    prompt_text = "# Prompty kontynuacji ikon unikatów\n\nWbudowane image_gen, osobne wywołanie na przedmiot, transparent_background=true.\n"
    for card, prompt in prompts:
        prompt_text += f"\n## {card['kod']} — {card['nazwa']}\n\n```text\n{prompt}\n```\n"
    (ART / "PROMPTY_KONTYNUACJA.md").write_text(prompt_text, encoding="utf-8")
    groups = []
    for start in range(0, len(records), 15):
        cells = []
        for item in records[start:start + 15]:
            title = html.escape(f"{item['kod']} — {item['nazwa']}")
            file = html.escape(item["plik"], quote=True)
            if item["status"] == "zapisany":
                pictures = f'<div class="scale"><img src="{file}" width="120" height="120"><img src="{file}" width="64" height="64"><img src="{file}" width="32" height="32"></div>'
            else:
                pictures = '<div class="scale missing">Do wygenerowania</div>'
            cells.append(f'<article><h2>{title}</h2>{pictures}<p>120 / 64 / 32 px · {html.escape(item["klasa"])}</p></article>')
        groups.append('<section data-part="' + str(start // 15) + '">' + "".join(cells) + '</section>')
    page = '''<!doctype html><html lang="pl"><meta charset="utf-8"><title>Unikaty — kontrola ikon</title>
<style>*{box-sizing:border-box}body{margin:0;padding:20px;background:#14151a;color:#ddd;font:13px system-ui}header{display:flex;justify-content:space-between;align-items:center;margin-bottom:12px}h1{font-size:20px;margin:0}a{color:#d0b884}section{display:grid;grid-template-columns:repeat(5,1fr);gap:10px;margin-bottom:25px}article{background:#22232a;border:1px solid #45413a;padding:10px;border-radius:3px;height:210px;overflow:hidden}h2{font-size:13px;height:32px;margin:0 0 8px;color:#e4d1a5}.scale{height:120px;display:flex;align-items:center;gap:8px;background:#17181d}img{object-fit:contain;flex:none}p{font-size:11px;color:#aaa;margin:8px 0 0}.missing{color:#bd826d;justify-content:center}body.light .scale{background:#c4bdb0}nav{display:flex;gap:12px}</style>
<header><h1>Proste ikony unikatów — kontrola skali</h1><nav><a href="?part=0">1</a><a href="?part=1">2</a><a href="?part=2">3</a><a href="?part=3">4</a><a href="?part=4">5</a><a href="?">Wszystkie</a><a href="#" id="light">Zmień tło</a></nav></header>'''
    page += "".join(groups)
    page += '''<script>const p=new URLSearchParams(location.search).get('part');if(p!==null)document.querySelectorAll('section').forEach(s=>s.hidden=s.dataset.part!==p);document.getElementById('light').onclick=e=>{e.preventDefault();document.body.classList.toggle('light')};</script></html>'''
    page = page.replace("section{display:grid", "section[hidden]{display:none}section{display:grid")
    (ART / "galeria_kontrolna.html").write_text(page, encoding="utf-8")
    readme_path = ART / "README.md"
    readme = readme_path.read_text(encoding="utf-8")
    readme = re.sub(r"Zestaw obejmuje \*\*.*?75 unikatów\*\*[^\n]*", f"Zestaw obejmuje **{count} z planowanych 75 unikatów**. Aktualną listę wszystkich kart i stan plików zawiera manifest.json.", readme)
    readme = readme.replace("Wszystkie trzy pliki: PNG, RGBA, **1254 × 1254 px**, kanał alfa obejmuje wartości 0–255.", "Źródła: PNG RGBA z kanałem alfa 0–255; rozmiar każdego pliku oraz suma SHA-256 są zapisane w manifest.json.")
    if "## Kontynuacja katalogu" not in readme:
        readme += "\n## Kontynuacja katalogu\n\nManifest zawiera wszystkie 75 kart, a status zapisany oznacza obecność PNG. Powyższa tabela wymienia trzy pierwotne próbki. Pełna lista jest w manifest.json i plan_grafik.json. Faktyczne prompty kontynuacji: PROMPTY_KONTYNUACJA.md oraz rejestr_generowania/KOD.json. Lokalna galeria: galeria_kontrolna.html (120 / 64 / 32 px).\n\nOdbudowa katalogu: python tools/art/katalog_ikon_unikatow.py. Sprawdzenie kompletu: to samo polecenie z --wymagaj-kompletu. Plansze kontrolne, bez modyfikowania źródeł: python tools/art/podglad_ikon_unikatow.py; wynik w TestResults/unique-icons-v1/kontrola_01.png do kontrola_05.png. Są to plansze przeglądowe, nie eksporty ikon do gry.\n"
    readme_path.write_text(readme, encoding="utf-8")
    summary = {"zapisane": count, "planowane": len(plan), "klasy": dict(Counter(item["klasa"] for item in records if item["status"] == "zapisany")), "bledy": errors}
    print(json.dumps(summary, ensure_ascii=True))
    if errors:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
