"""Tekstury demona Hell Unleashed wyjęte wprost z .glb (te, których używa materiał modelu w grze).

glTF pakuje metaliczność i chropowatość jako G = roughness, B = metallic; Standard chce
R = metallic, A = smoothness (1 − roughness). Albedo, normalna i emisja są zapisywane bez zmian.
Użycie: python tools/art/hu_demon_textures.py <plik.glb> <nazwa> <folder_docelowy>
"""
import io
import json
import os
import struct
import sys

from PIL import Image


def read_glb(path):
    data = open(path, "rb").read()
    length = struct.unpack("<I", data[12:16])[0]
    doc = json.loads(data[20:20 + length])
    off = 20 + length
    blen = struct.unpack("<I", data[off:off + 4])[0]
    return doc, data[off + 8:off + 8 + blen]


def image_bytes(doc, binary, tex_index):
    img = doc["images"][doc["textures"][tex_index]["source"]]
    view = doc["bufferViews"][img["bufferView"]]
    start = view.get("byteOffset", 0)
    return binary[start:start + view["byteLength"]], img.get("mimeType", "image/png")


def main():
    glb, name, dst = sys.argv[1:4]
    os.makedirs(dst, exist_ok=True)
    doc, binary = read_glb(glb)
    mat = doc["materials"][0]
    pbr = mat.get("pbrMetallicRoughness", {})

    def save(tex, suffix):
        raw, mime = image_bytes(doc, binary, tex["index"])
        ext = ".jpg" if "jpeg" in mime else ".png"
        out = os.path.join(dst, name + suffix + ext)
        open(out, "wb").write(raw)
        return out

    if "baseColorTexture" in pbr:
        save(pbr["baseColorTexture"], "_albedo")
    if "normalTexture" in mat:
        save(mat["normalTexture"], "_normal")
    if "emissiveTexture" in mat:
        save(mat["emissiveTexture"], "_emission")
    if "metallicRoughnessTexture" in pbr:
        raw, _ = image_bytes(doc, binary, pbr["metallicRoughnessTexture"]["index"])
        img = Image.open(io.BytesIO(raw)).convert("RGB")
        _, g, b = img.split()
        smooth = g.point(lambda v: 255 - v)
        zero = Image.new("L", img.size, 0)
        Image.merge("RGBA", (b, zero, zero, smooth)).save(os.path.join(dst, name + "_metallic.png"))
    print("tekstury", name, "emisja" if "emissiveTexture" in mat else "")


main()
