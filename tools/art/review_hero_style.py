"""Plansze diagnostyczne z prawdziwych zrzutów Unity; nie retuszuje renderów ani tekstur gry."""
from pathlib import Path
import json
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
DIRECTORY = ROOT / "TestResults/hero-style-v2"


def main():
    report = {}
    compact = Image.new("RGB", (1200, 956), (23, 26, 33))
    compact_draw = ImageDraw.Draw(compact)
    for hero_row, key in enumerate(("mage", "berserker")):
        views = ("neutral-front", "arena-front", "arena-face", "game")
        sheet = Image.new("RGB", (1200, len(views) * 478), (23, 26, 33))
        draw = ImageDraw.Draw(sheet)
        for row, view in enumerate(views):
            pair = []
            for column, state in enumerate(("before", "after")):
                path = DIRECTORY / f"{key}-{view}-{state}.png"
                with Image.open(path) as source:
                    image = source.convert("RGB")
                    pair.append(np.asarray(image).astype(np.float32) / 255)
                    image.thumbnail((600, 450), Image.Resampling.LANCZOS)
                    sheet.paste(image, (column * 600, row * 478 + 28))
                    if view == "arena-front":
                        compact.paste(image, (column * 600, hero_row * 478 + 28))
                label = "PRZED" if state == "before" else "PO KOREKCIE"
                draw.text((column * 600 + 12, row * 478 + 8), f"{key} / {view} / {label}", fill=(233, 222, 200))
                if view == "arena-front":
                    compact_draw.text((column * 600 + 12, hero_row * 478 + 8), f"{key} / {label}", fill=(233, 222, 200))
            before, after = pair
            delta = np.abs(after - before)
            magenta = (after[:, :, 0] > .86) & (after[:, :, 1] < .16) & (after[:, :, 2] > .86)
            report[f"{key}-{view}"] = {
                "mean_absolute_rgb_difference": float(delta.mean()),
                "changed_pixel_fraction_over_8_levels": float((delta.max(axis=2) > 8 / 255).mean()),
                "magenta_pixel_fraction": float(magenta.mean()),
                "source_size": [after.shape[1], after.shape[0]],
            }
        sheet.save(DIRECTORY / f"{key}-comparison.jpg", quality=95)
    compact.save(DIRECTORY / "comparison-arena.jpg", quality=95)
    (DIRECTORY / "image-review.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
