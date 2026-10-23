"""Import tekstur i receptur cząsteczek HU; bez uruchamiania Godota ani generatora treści Unity.

python tools/art/import_hu_vfx.py --source "C:/GIT/Hell Unleashed"
Zachowuje istniejące GUID-y. Nie kopiuje plików .import Godota.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import uuid

ROOT = Path(__file__).resolve().parents[2]
ASSETS = ROOT / "Assets/TurrisDamnatorum"


def meta(path, folder=False, texture=False):
    target = Path(str(path) + ".meta")
    if target.exists():
        return
    guid = uuid.uuid5(uuid.NAMESPACE_URL, "turris/" + path.relative_to(ROOT).as_posix()).hex
    text = f"fileFormatVersion: 2\nguid: {guid}\n"
    if folder:
        text += "folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
    elif texture:
        text += """TextureImporter:
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 1
    sRGBTexture: 1
    borderMipMap: 0
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  textureType: 0
  textureShape: 1
  alphaSource: 1
  alphaIsTransparency: 1
  textureCompression: 0
"""
    else:
        text += "DefaultImporter:\n  externalObjects: {}\n"
    target.write_text(text, encoding="utf-8")


def extract_method(source, signature):
    start = source.index(signature)
    opening = source.index("{", start)
    depth = 1
    end = opening + 1
    while depth:
        depth += (source[end] == "{") - (source[end] == "}")
        end += 1
    return source[start:end]


def recipes(source):
    parts = [extract_method(source, signature) for signature in (
        "private static Layer[] LayersFor", "private static Layer[] IceLayers", "private static Layer[] FireLayers",
        "public static ElementParticles HandFlame")]
    code = "\n\n".join(parts)
    code = code.replace("private static Layer[] LayersFor", "internal static HellParticleLayer[] LayersFor")
    code = re.sub(r"\bLayer\b", "HellParticleLayer", code)
    code = code.replace("FxElement", "HellFxElement").replace("Vector3.Zero", "Vector3.zero")
    code = code.replace("Vector2I", "Vector2Int").replace("MeshOverride: RockChunk()", "Rock: true")
    code = code.replace("public static ElementParticles HandFlame", "internal static HellParticleLayer[] HandFlame")
    code = code.replace("return new ElementParticles(layers, oneShot: false);", "return layers;")
    code = code.replace("Math.Max", "Mathf.Max")
    # Komentarze receptur zachowujemy jako cytat kodu źródłowego, wraz z parametrami autora.
    return """// Wygenerowane przez tools/art/import_hu_vfx.py z World/ElementFx.cs (Hell Unleashed).
// Parametry warstw ze źródła; adapter silnika: HellParticles.cs. Nie edytuj ręcznie.
using UnityEngine;
namespace Turris
{
internal static class HellParticleRecipes
{
    private static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);
    private static readonly Color[] White = { C(1,1,1,0), C(1,1,1,1), C(1,1,1,0.9f), C(1,1,1,0) };
""" + code + "\n}\n}\n"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, default=Path("C:/GIT/Hell Unleashed"))
    parser.add_argument("--verify", action="store_true", help="Tylko porównuje kopie i receptury; niczego nie zapisuje.")
    args = parser.parse_args()
    game = args.source / "src/Game"
    source = game / "Scripts/World/ElementFx.cs"
    code = recipes(source.read_text(encoding="utf-8-sig"))  # Walidacja przed zapisem.
    dest = ASSETS / "Resources/HellVfx"
    if args.verify:
        manifest = json.loads((dest / "migration_manifest.json").read_text(encoding="utf-8"))
        if hashlib.sha256(source.read_bytes()).hexdigest() != manifest["recipe_sha256"]:
            raise SystemExit("Źródłowy ElementFx.cs zmienił się od importu.")
        for entry in manifest["textures"]:
            for path in (game / "Assets/Textures/Vfx" / entry["file"], dest / entry["file"]):
                if hashlib.sha256(path.read_bytes()).hexdigest() != entry["sha256"]:
                    raise SystemExit(f"Różnica SHA-256: {path}")
        if code != (ASSETS / "Scripts/Runtime/Fx/HellParticleRecipes.cs").read_text(encoding="utf-8"):
            raise SystemExit("Receptury różnią się od bieżącego źródła HU.")
        print(f"OK: {len(manifest['textures'])} tekstur identycznych ze źródłem i manifestem; receptury zgodne.")
        return
    dest.mkdir(parents=True, exist_ok=True)
    meta(dest.parent, folder=True)
    meta(dest, folder=True)
    entries = []
    for path in sorted((game / "Assets/Textures/Vfx").glob("*.png")):
        target = dest / path.name
        shutil.copyfile(path, target)
        meta(target, texture=True)
        entries.append({"file": path.name, "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})
    shutil.copyfile(game / "Assets/Textures/Vfx/generated_manifest.json", dest / "generated_manifest.json")
    # Szum jest próbkowany w przestrzeni świata i wymaga powtarzania, bez korekcji sRGB.
    noise_meta = Path(str(dest / "noise_tile.png") + ".meta")
    noise_meta.write_text(noise_meta.read_text().replace("sRGBTexture: 1", "sRGBTexture: 0")
                          .replace("alphaIsTransparency: 1", "alphaIsTransparency: 0")
                          .replace("wrapU: 1", "wrapU: 0").replace("wrapV: 1", "wrapV: 0"), encoding="utf-8")
    (dest / "migration_manifest.json").write_text(json.dumps({
        "source": "Hell Unleashed/src/Game/Assets/Textures/Vfx", "date": "2026-10-22",
        "recipe_sha256": hashlib.sha256(source.read_bytes()).hexdigest(), "textures": entries
    }, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (ASSETS / "Scripts/Runtime/Fx/HellParticleRecipes.cs").write_text(code, encoding="utf-8")
    print(f"Import: {len(entries)} tekstur; receptury Fire/Water/Air/Earth/Enemy/Soul/Ash/Ice/Physical.")


if __name__ == "__main__":
    main()
