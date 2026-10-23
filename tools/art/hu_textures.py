"""Tekstury bohatera Hell Unleashed dla shadera Standard (Unity, wbudowany potok).

glTF pakuje metaliczność i chropowatość jako G = roughness, B = metallic; Standard chce
R = metallic, A = smoothness (1 − roughness). Albedo i normalna są kopiowane bez zmian (4K).
Użycie: python tools/art/hu_textures.py <folder_źródłowy> <nazwa> <folder_docelowy>
"""
import os
import shutil
import sys

from PIL import Image


def main():
    src, name, dst = sys.argv[1:4]
    os.makedirs(dst, exist_ok=True)
    albedo = os.path.join(src, name + "_Image_0.jpg")
    mr = os.path.join(src, name + "_Image_1.jpg")
    normal = os.path.join(src, name + "_normal.png")
    shutil.copyfile(albedo, os.path.join(dst, name + "_albedo.jpg"))
    shutil.copyfile(normal, os.path.join(dst, name + "_normal.png"))
    img = Image.open(mr).convert("RGB")
    _, g, b = img.split()
    smooth = g.point(lambda v: 255 - v)
    zero = Image.new("L", img.size, 0)
    Image.merge("RGBA", (b, zero, zero, smooth)).save(os.path.join(dst, name + "_metallic.png"))
    print("tekstury", name)


main()
