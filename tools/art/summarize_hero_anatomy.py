"""Utrwala macierz audytu w dokumentacji dopiero po sprawdzeniu kompletności danych."""
import csv
import json
import shutil
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
DATA=ROOT/"TestResults/hero-anatomy"
DOC=ROOT/"docs/RAPORT_WERYFIKACJI_ANATOMII_BOHATEROW.md"
QA=ROOT/"docs/qa/hero-anatomy-2026-10-29"


def main():
    tests={name:ET.parse(DATA/name).getroot() for name in ("audit-live.xml","followup-live.xml","contacts-live.xml","berserker-series-live.xml")}
    assert all(x.attrib["result"]=="Passed" and x.attrib["failed"]=="0" for x in tests.values())
    assert sum(int(x.attrib["passed"]) for x in tests.values())==7
    metrics=json.loads((DATA/"metrics-all.json").read_text(encoding="utf-8"))
    assert len(metrics)==2106
    manifests=[json.loads(f.read_text(encoding="utf-8-sig")) for f in DATA.glob("class_*/*/*/manifest.json")]
    assert len(manifests)==117 and sum(x["profile"].startswith("native-") for x in manifests)==27
    assert len(list(DATA.glob("class_*/*/baked-contact/sampling.json")))==15
    assert len(list(DATA.glob("class_*/*/diagnostic-*/reference.json")))==18
    assert all(json.loads(f.read_text())["wraps"]>=4 for f in DATA.glob("class_*/*/baked-contact/sampling.json"))
    protected=json.loads((DATA/"protected-source-check.json").read_text())
    assert len(protected)==8 and all(protected.values())
    native=[]
    for states in sorted(DATA.glob("class_*/*/native-arena-*/states.csv")):
        rows=list(csv.DictReader(states.open(encoding="utf-8-sig")))
        starts=sum(r["request"]=="combo" and r["action"]=="Attack" and r["phase"]=="Startup" and (i==0 or rows[i-1]["phase"]!="Startup" or rows[i-1]["action"]!="Attack") for i,r in enumerate(rows))
        assert starts>=3,(states,starts)
        native.append(dict(classId=states.parent.parent.parent.name,profile=states.parent.name,frames=len(rows),comboStarts=starts,
            observed={request:sorted({r["action"] for r in rows if r["request"]==request}) for request in sorted({r["request"] for r in rows})}))
    assert len(native)==27
    (DATA/"native-actions-summary.json").write_text(json.dumps(native,indent=2,ensure_ascii=False),encoding="utf-8")
    QA.mkdir(parents=True,exist_ok=True)
    names={x["id"]:x["name"] for x in json.loads((ROOT/"ArtSource/GothicAnimePoC/Heroes/classes.json").read_text(encoding="utf-8-sig"))}
    names.update(class_mage="Mag (anime)",class_hu_paladin="Paladyn")
    selected=[m for m in metrics if "/native-0/run-0-60fps" in m["path"]]
    assert len(selected)==15
    table=["| Postać / aktywny wygląd | Zestawy startowe + profile | N faz | Asymetria P95 / max | Zapadanie L/P, max | Ocena |",
           "|---|---:|---:|---:|---:|---|"]
    findings=[]
    for m in selected:
        class_id,visual,*_=m["path"].split("/")
        profiles=[p for p in manifests if p["classId"]==class_id]
        n=sum(p["profile"].startswith("native-") for p in profiles)
        contact=json.loads((DATA/class_id/visual/"baked-contact/contact-metrics.json").read_text(encoding="utf-8"))
        asym=m["mirrored_swing_asymmetry_deg"]
        assert asym["n"]>0
        status="podejrzenie"
        if class_id=="class_hu_berserker":status="błąd odtworzony"
        if class_id=="class_knight":status="podejrzenie; tylko odczyt"
        if class_id=="class_hu_paladin":status="skręt poprawny w tej próbie; podejrzenie kontaktu"
        if class_id=="class_hu_summoner":status="podejrzenie po kościach; brak pełnej oceny buta pod szatą"
        source=DATA/class_id/visual/"diagnostic-active/run-0-60fps/isolated-review.jpg"
        assert source.exists(),source
        shutil.copyfile(source,QA/(class_id+".jpg"))
        penetration=[contact[s]["penetration_m"]["max"]*100 for s in ("L","R")]
        table.append(f"| [{names[class_id]}](qa/hero-anatomy-2026-10-29/{class_id}.jpg) · `{visual}` | {n} + 6 | {asym['n']} | {asym['p95']:.1f}° / {asym['max']:.1f}° | {penetration[0]:.1f} / {penetration[1]:.1f} cm | {status} |")
        findings.append(dict(classId=class_id,visual=visual,nativeProfiles=n,additionalProfiles=6,status=status,asymmetry=asym,bakedContact=contact))
    text=DOC.read_text(encoding="utf-8")
    start="<!-- TABELA_WYNIKOW -->";end="<!-- KONIEC_TABELI_WYNIKOW -->"
    content=start+"\n"+"\n".join(table)+"\n"+end
    if end in text:
        prefix,rest=text.split(start,1);_,suffix=rest.split(end,1);text=prefix+content+suffix
    else:
        assert start in text
        text=text.replace(start,content)
    DOC.write_text(text,encoding="utf-8")
    summary=dict(classes=15,nativeProfiles=27,additionalProfiles=90,profiles=117,cases=2106,
                 motionFrames=sum(m["sides"]["L"]["frames"] for m in metrics),nativeActionFrames=sum(n["frames"] for n in native),
                 sourceComparisons=18,testExecutions=7,testFailures=0,protectedSources=protected,findings=findings)
    (DATA/"audit-summary.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding="utf-8")
    shutil.copyfile(DATA/"coverage.csv",QA/"coverage.csv")
    shutil.copyfile(DATA/"audit-summary.json",QA/"audit-summary.json")
    for src,dest in ((DATA/"berserker-paladin.jpg",QA/"berserker-paladin.jpg"),
                     (DATA/"class_hu_berserker/visual_berserker_anime/diagnostic-old-rig-same-sets/run-0-60fps/isolated-review.jpg",QA/"berserker-old-rig.jpg"),
                     (DATA/"class_hu_berserker/visual_berserker_anime/diagnostic-active/run-0-60fps/sequence-front-normal.gif",QA/"berserker-run-normal.gif"),
                     (DATA/"class_hu_berserker/visual_berserker_anime/diagnostic-active/run-0-60fps/sequence-front-slow.gif",QA/"berserker-run-slow.gif")):
        shutil.copyfile(src,dest)
    print(json.dumps({k:v for k,v in summary.items() if k not in ("findings","protectedSources")},ensure_ascii=False))


if __name__=="__main__":main()
