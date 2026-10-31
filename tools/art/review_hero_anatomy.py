"""Podsumowanie surowych pomiarów; progi to sygnały do oceny, nie diagnoza anatomiczna."""
from pathlib import Path
import argparse
import csv
import json
import math
import statistics
from collections import defaultdict
from PIL import Image, ImageOps, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "TestResults/hero-anatomy"
DRAW_SHEETS = True


def percentile(values, q=95):
    values = sorted(v for v in values if math.isfinite(v))
    if not values:
        return None
    x = (len(values) - 1) * q / 100
    lo = int(x)
    hi = min(lo + 1, len(values) - 1)
    return values[lo] * (hi - x) + values[hi] * (x - lo) if hi != lo else values[lo]


def angular(a, b):
    return (a - b + 180) % 360 - 180


def mean_angle(values):
    values=[math.radians(x) for x in values if math.isfinite(x)]
    return math.degrees(math.atan2(sum(math.sin(x) for x in values),sum(math.cos(x) for x in values))) if values else None


def stats(values):
    values = [v for v in values if math.isfinite(v)]
    return dict(n=len(values), p95=percentile(values), max=max(values) if values else None)


def rows(path):
    with path.open(encoding="utf-8-sig", newline="") as stream:
        for row in csv.DictReader(stream):
            stage, side = row.pop("stage"), row.pop("side")
            yield stage, side, {key: float(value) for key, value in row.items()}


def summarize(path):
    final = defaultdict(list)
    stages = defaultdict(list)
    for stage, side, row in rows(path):
        if stage == "final":
            final[side].append(row)
        elif stage in {"00-retarget", "02-foot-yaw", "05-leg-ik", "08-armored-feet", "08a-anatomical-feet", "10-anchor"}:
            stages[stage + "-" + side].append(row)
    result = dict(path=str(path.parent.relative_to(OUT)).replace("\\", "/"), sides={}, stages={})
    bins = {}
    for side, samples in final.items():
        min_y = percentile([x["world_y"] for x in samples], 5)
        low = [x for x in samples if x["world_y"] < min_y + .035 and abs(x["pitch"]) < 40]
        support = [x for x in low if abs(x["pitch"]) < 15 and math.isfinite(x["sole_min"]) and x["sole_max"] - x["sole_min"] < .045]
        # Przy pionowej stopie yaw traci sens: nie zgłaszaj takiego rzutu jako przeskoku animacji.
        jumps = [abs(angular(b["yaw"], a["yaw"])) for a, b in zip(samples, samples[1:]) if abs(a["pitch"]) < 60 and abs(b["pitch"]) < 60]
        slips = []
        window = []
        support_frames = {x["frame"] for x in support}
        for x in samples:
            if x["frame"] not in support_frames:
                if len(window) >= 3:
                    slips.append(max(math.hypot(y["sole_x"] - window[0]["sole_x"], y["sole_z"] - window[0]["sole_z"]) for y in window))
                window = []
            else:
                window.append(x)
        if len(window) >= 3:
            slips.append(max(math.hypot(y["sole_x"] - window[0]["sole_x"], y["sole_z"] - window[0]["sole_z"]) for y in window))
        valid_twist = [x for x in samples if x["knee_bend"] > 60 and math.isfinite(x["knee_twist"])]
        phase_bins = defaultdict(list)
        for x in samples:
            phase_bins[int(x["phase"] * 32) % 32].append(x)
        bins[side] = {i: dict(bend=statistics.mean(x["knee_bend"] for x in s), twist=mean_angle([x["knee_twist"] for x in s])) for i, s in phase_bins.items()}
        worst = max(valid_twist, key=lambda x: abs(x["knee_twist"]), default=None)
        result["sides"][side] = dict(frames=len(samples), support_yaw=stats([abs(x["rest_delta"]) for x in low]), support_window_slip_m=stats(slips),
            sole_penetration_m=stats([max(0, -x["sole_min"]) for x in support]), sole_hover_m=stats([max(0, x["sole_min"]) for x in support]),
            swing_twist=stats([abs(x["knee_twist"]) for x in valid_twist]), yaw_jump_deg=stats(jumps),
            worst_twist_phase=worst["phase"] if worst else None,
            invalid_knee_plane=sum(not math.isfinite(x["knee_twist"]) for x in samples),
            ankle_height_m=dict(min=min(x["world_y"] for x in samples), max=max(x["world_y"] for x in samples)),
            torso_pitch=stats([abs(x["torso_pitch"]) for x in samples]))
    # Zestawienie po wyrównaniu do szczytu zgięcia każdej nogi; bez porównania przypadkowych faz.
    difference = []
    if bins.get("L") and bins.get("R"):
        lpeak = max(bins["L"], key=lambda b: bins["L"][b]["bend"])
        rpeak = max(bins["R"], key=lambda b: bins["R"][b]["bend"])
        for i, left in bins["L"].items():
            right = bins["R"].get((i + rpeak - lpeak) % 32)
            if right and left["bend"] > 60 and right["bend"] > 60 and left["twist"] is not None and right["twist"] is not None:
                difference.append(abs(angular(left["twist"], -right["twist"])))
        result["phase_alignment_bins"] = (rpeak - lpeak) % 32
    result["mirrored_swing_asymmetry_deg"] = stats(difference)
    for key, samples in stages.items():
        result["stages"][key] = dict(twist=stats([abs(x["knee_twist"]) for x in samples if x["knee_bend"] > 60]), yaw=stats([abs(x["rest_delta"]) for x in samples]))
    return result


def sheet(paths, target, title, columns=4, width=300):
    if not paths or not DRAW_SHEETS:
        return
    height = width + 35
    out = Image.new("RGB", (columns * width, ((len(paths) + columns - 1) // columns) * height + 35), "#20242d")
    draw = ImageDraw.Draw(out)
    draw.text((10, 10), title, fill="white")
    for i, path in enumerate(paths):
        try:
            picture = ImageOps.contain(Image.open(path).convert("RGB"), (width, width))
        except OSError:
            continue
        x, y = i % columns * width, i // columns * height + 35
        out.paste(picture, (x, y))
        label = str(path.relative_to(target.parent)) if path.is_relative_to(target.parent) else path.name
        draw.text((x + 3, y + width + 4), label[-46:], fill="white")
    out.save(target, quality=90)


def contact_metrics(path):
    """Rzeczywiste wierzchołki BakeMesh. Ślizg śledzi ten sam punkt, nie środek stopy."""
    frames=defaultdict(list)
    tracks=defaultdict(list)
    with path.open(encoding="utf-8-sig",newline="") as stream:
        for raw in csv.DictReader(stream):
            side=raw["side"]
            row={key:float(value) for key,value in raw.items() if key!="side"}
            frames[side,row["frame"]].append(row)
            tracks[side,row["marker"]].append(row)
    result={}
    for side in ("L","R"):
        samples=[]
        for (s,frame),points in frames.items():
            if s!=side:
                continue
            valid=[p for p in points if math.isfinite(p["surface_y"])]
            if valid:
                samples.append(dict(frame=frame,ankle=valid[0]["ankle_y"],gap=min(p["world_y"]-p["surface_y"] for p in valid),phase=valid[0]["phase"]))
        if not samples:
            result[side]={"frames":0,"status":"brak danych"}
            continue
        low_ankle=percentile([p["ankle"] for p in samples],5)
        low=[p for p in samples if p["ankle"]<low_ankle+.035]
        slips=[]
        for (s,marker),track in tracks.items():
            if s!=side:
                continue
            window=[]
            def end():
                if len(window)>=3 and window[-1]["time"]-window[0]["time"]>=.05:
                    slips.append(max(math.hypot(p["world_x"]-window[0]["world_x"],p["world_z"]-window[0]["world_z"]) for p in window))
            for index,p in enumerate(track):
                previous=track[max(0,index-1)]
                dt=p["time"]-previous["time"]
                vertical=(p["world_y"]-previous["world_y"])/dt if dt>0 else 1
                if abs(p["world_y"]-p["surface_y"])<=.012 and abs(vertical)<.04:
                    window.append(p)
                else:
                    end()
                    window=[]
            end()
        worst=min(samples,key=lambda p:p["gap"])
        result[side]=dict(frames=len(samples),penetration_m=stats([max(0,-p["gap"]) for p in samples]),low_ankle_hover_m=stats([max(0,p["gap"]) for p in low]),
            contact_point_slip_m=stats(slips),penetration_over_2cm=sum(p["gap"]<-.02 for p in samples),worst_phase=worst["phase"],
            note="Brak okna stabilnego podparcia oznacza brak wyniku ślizgu, nigdy 0 cm.")
    (path.parent/"contact-metrics.json").write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding="utf-8")
    return result


def main():
    global DRAW_SHEETS, OUT
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument("--class-id")
    parser.add_argument("--sheets-only", action="store_true")
    parser.add_argument("--metrics-only", action="store_true")
    parser.add_argument("--data-root", type=Path)
    args = parser.parse_args()
    if args.data_root:OUT=args.data_root.resolve()
    DRAW_SHEETS=not args.metrics_only
    all_results = []
    folders = sorted(OUT.glob(args.class_id or "class_*"))
    for cls in folders:
        if not cls.is_dir():
            continue
        for visual in cls.iterdir():
            if not visual.is_dir():
                continue
            manifests = []
            for manifest in sorted(visual.glob("*/manifest.json")):
                profile = manifest.parent
                manifests.append(json.loads(manifest.read_text(encoding="utf-8-sig")))
                if not args.sheets_only:
                    for frames in sorted(profile.glob("*/frames.csv")):
                        if frames.parent.name.startswith("actions") or not (frames.parent / "sampling.json").exists():
                            continue
                        result = summarize(frames)
                        result["sampling"] = json.loads((frames.parent / "sampling.json").read_text())
                        all_results.append(result)
                        (frames.parent / "metrics.json").write_text(json.dumps(result, indent=2, ensure_ascii=False, allow_nan=False), encoding="utf-8")
                if profile.name.startswith("native"):
                    sheet(sorted((profile/"run-0-60fps").glob("p*-feet.png")), profile/"feet-review.jpg", cls.name+" / "+profile.name)
            pictures = []
            for case in sorted(visual.glob("native-*/run-0-60fps")):
                for phase in range(4):
                    pictures.extend(p for p in [case/f"p{phase}-front-feet.png",case/f"p{phase}-left-feet.png",case/f"p{phase}-back-feet.png",case/f"p{phase}-right-feet.png"] if p.exists())
            sheet(pictures, visual/"feet-review.jpg", cls.name)
            sheet(sorted(visual.glob("native-*/run-0-60fps/p*-front.png")), visual/"full-review.jpg", cls.name)
            sheet(sorted(visual.glob("arena-*/*.png")), visual/"arena-review.jpg", cls.name)
            (visual/"coverage.json").write_text(json.dumps(manifests, indent=2, ensure_ascii=False), encoding="utf-8")
            for diagnostic in visual.glob("diagnostic-*"):
                for case in diagnostic.glob("run-*-*fps"):
                    if (case/"frames.csv").exists() and (case/"sampling.json").exists() and not args.sheets_only:
                        result=summarize(case/"frames.csv")
                        result["sampling"]=json.loads((case/"sampling.json").read_text())
                        (case/"metrics.json").write_text(json.dumps(result,indent=2,ensure_ascii=False,allow_nan=False),encoding="utf-8")
                    sheet(sorted(case.glob("p*-isolated-*.png")),case/"isolated-review.jpg",cls.name+" / "+diagnostic.name)
                    for view in ("front","side"):
                        sequence=sorted(case.glob("sequence-"+view+"-*.png"))
                        if sequence and not args.metrics_only:
                            frames=[ImageOps.contain(Image.open(p).convert("RGB"),(384,384)) for p in sequence]
                            for speed,duration in (("normal",33 if "30fps" in case.name else 17),("slow",133 if "30fps" in case.name else 67)):
                                frames[0].save(case/("sequence-"+view+"-"+speed+".gif"),save_all=True,append_images=frames[1:],duration=duration,loop=0)
            for markers in [visual/"baked-contact/markers.csv",*visual.glob("native-arena-*/markers.csv")]:
                if markers.exists() and not args.sheets_only:
                    contact_metrics(markers)
        print(cls.name, "gotowe", flush=True)
    if all_results:
        name = "metrics-" + args.class_id if args.class_id else "metrics-all"
        (OUT / (name + ".json")).write_text(json.dumps(all_results, indent=2, ensure_ascii=False), encoding="utf-8")
        print("Przypadki:", len(all_results), flush=True)
        with (OUT/"coverage.csv").open("w",encoding="utf-8-sig",newline="") as stream:
            writer=csv.writer(stream)
            writer.writerow(["classId","visual","profile","weapon","offhand","walkSpeed","runSpeed","clips"])
            for manifest in sorted(OUT.glob("class_*/*/*/manifest.json")):
                d=json.loads(manifest.read_text(encoding="utf-8-sig"))
                writer.writerow([d[k] for k in ("classId","visual","profile","weapon","offhand","walkSpeed","runSpeed")]+[" | ".join(d["clips"])])


if __name__ == "__main__":
    main()
