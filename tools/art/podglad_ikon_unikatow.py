"""Rysuje plansze kontrolne 120/64/32 px, zachowując źródłowe PNG bez zmian."""

from __future__ import annotations

import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "ArtSource/Unikaty/ProsteIkony_v1"
OUT = ROOT / "TestResults/unique-icons-v1"


def main() -> None:
    manifest = json.loads((ART / "manifest.json").read_text(encoding="utf-8"))
    font_path = Path("C:/Windows/Fonts/segoeui.ttf")
    font = ImageFont.truetype(str(font_path), 14)
    small = ImageFont.truetype(str(font_path), 11)
    OUT.mkdir(parents=True, exist_ok=True)
    items = manifest["przedmioty"]
    for part, start in enumerate(range(0, len(items), 15), 1):
        board = Image.new("RGB", (1440, 740), "#14151a")
        draw = ImageDraw.Draw(board)
        draw.text((18, 12), f"Turris — ikony unikatów {part}/5 — 120 / 64 / 32 px", font=font, fill="#e4d1a5")
        for index, item in enumerate(items[start:start + 15]):
            x, y = 18 + (index % 5) * 284, 45 + (index // 5) * 224
            draw.rectangle((x, y, x + 272, y + 212), fill="#22232a", outline="#45413a")
            title = item["kod"] + " " + item["nazwa"]
            if len(title) > 32:
                words = title.split()
                first = ""
                while words and len(first + " " + words[0]) <= 32:
                    first += (" " if first else "") + words.pop(0)
                lines = [first, " ".join(words)]
            else:
                lines = [title]
            for line_no, line in enumerate(lines):
                draw.text((x + 8, y + 7 + 17 * line_no), line, font=font, fill="#e4d1a5")
            path = ART / item["plik"]
            if path.is_file():
                with Image.open(path) as im:
                    source = im.convert("RGBA")
                    cursor = x + 8
                    for size in (120, 64, 32):
                        preview = source.resize((size, size), Image.Resampling.LANCZOS)
                        board.paste(preview, (cursor, y + 52 + (120 - size) // 2), preview)
                        cursor += size + 8
            else:
                draw.text((x + 10, y + 100), "Do wygenerowania", font=font, fill="#bd826d")
            draw.text((x + 8, y + 187), item["klasa"] + " | 120 / 64 / 32 px", font=small, fill="#bbb")
        board.save(OUT / f"kontrola_{part:02d}.png")
    print("Zapisano 5 plansz kontrolnych; źródłowe ikony bez zmian.")


if __name__ == "__main__":
    main()
