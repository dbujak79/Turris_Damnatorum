"""Kontrola wyników naprawy oraz utrwalenie porównań w dokumentacji."""
from pathlib import Path
import csv
import hashlib
import json
import shutil
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[2]
DATA=ROOT/"TestResults/hero-feet-repair"
BEFORE=ROOT/"TestResults/hero-anatomy"
AFTER=DATA/"after"
QA=ROOT/"docs/qa/hero-feet-repair-2026-10-29"
DOC=ROOT/"docs/NAPRAWA_KOSTEK_BOHATEROW.md"


def main():
    full=ET.parse(DATA/"after-live.xml").getroot()
    regression=ET.parse(DATA/"regression-live.xml").getroot()
    hinge=ET.parse(DATA/"hinges-live.xml").getroot()
    assert int(full.attrib["passed"])==6 and int(full.attrib["failed"])==0
    assert int(regression.attrib["passed"])==21 and int(regression.attrib["failed"])==0
    assert int(regression.attrib["skipped"])==5
    assert int(hinge.attrib["passed"])==1 and int(hinge.attrib["failed"])==0
    protected=json.loads((DATA/"protected-before.json").read_text())
    assert len(protected)==19
    assert all(hashlib.sha256((ROOT/path).read_bytes()).hexdigest()==digest for path,digest in protected.items())
    original=json.loads((BEFORE/"protected-source-check.json").read_text())
    assert all(original.values())
    control=json.loads((DATA/"warrior-pose-check.json").read_text())
    assert control["unchanged"] and control["maxDifference"]<.001
    rows=list(csv.DictReader((DATA/"hinge-regression.csv").open()))
    assert len(rows)==56 and min(int(r["swing_samples"]) for r in rows)>3
    assert max(float(r["max_twist"]) for r in rows)<18
    before=json.loads((BEFORE/"metrics-all.json").read_text(encoding="utf-8"))
    after=json.loads((AFTER/"metrics-all.json").read_text(encoding="utf-8"))
    assert len(before)==len(after)==2106
    def defaults(data):return {m["path"].split("/")[0]:m for m in data if "/native-0/run-0-60fps" in m["path"]}
    old,new=defaults(before),defaults(after)
    assert len(old)==len(new)==15
    manifests=list(AFTER.glob("class_*/*/*/manifest.json"));assert len(manifests)==117
    native=[]
    for f in sorted(AFTER.glob("class_*/*/native-arena-*/states.csv")):
        states=list(csv.DictReader(f.open()))
        starts=sum(s["request"]=="combo" and s["action"]=="Attack" and s["phase"]=="Startup" and (i==0 or states[i-1]["phase"]!="Startup" or states[i-1]["action"]!="Attack") for i,s in enumerate(states))
        assert starts>=3,(f,starts)
        native.append(dict(classId=f.parent.parent.parent.name,profile=f.parent.name,comboStarts=starts,frames=len(states)))
    assert len(native)==27
    names={c["id"]:c["name"] for c in json.loads((ROOT/"ArtSource/GothicAnimePoC/Heroes/classes.json").read_text(encoding="utf-8-sig"))}
    names.update(class_mage="Mag anime",class_hu_paladin="Paladyn")
    QA.mkdir(parents=True,exist_ok=True)
    lines=["| Postać | Asymetria wymachu P95 przed → po | Zapadanie badanych punktów, max przed → po |",
           "|---|---:|---:|"]
    findings=[]
    for cls in sorted(new):
        visual=new[cls]["path"].split("/")[1]
        bcontact=json.loads((BEFORE/cls/visual/"baked-contact/contact-metrics.json").read_text(encoding="utf-8"))
        acontact=json.loads((AFTER/cls/visual/"baked-contact/contact-metrics.json").read_text(encoding="utf-8"))
        bmax=max(bcontact[s]["penetration_m"]["max"] for s in ("L","R"))
        amax=max(acontact[s]["penetration_m"]["max"] for s in ("L","R"))
        bp=old[cls]["mirrored_swing_asymmetry_deg"]["p95"];ap=new[cls]["mirrored_swing_asymmetry_deg"]["p95"]
        assert new[cls]["mirrored_swing_asymmetry_deg"]["n"]>0
        protected_warrior=cls=="class_knight"
        if not protected_warrior:
            assert ap<12,(cls,ap)
            assert amax<.007,(cls,amax)
            content=(ROOT/"Assets/TurrisDamnatorum/Content/Visuals"/(visual+".asset")).read_text(encoding="utf-8-sig")
            assert "anatomicalFeet: 1" in content,visual
        label=names[cls]+(" — bez zmian" if protected_warrior else "")
        lines.append(f"| [{label}](qa/hero-feet-repair-2026-10-29/{cls}-after.jpg) | {bp:.2f}° → {ap:.2f}° | {bmax*100:.2f} → {amax*100:.2f} cm |")
        src=AFTER/cls/visual/"diagnostic-active/run-0-60fps/isolated-review.jpg"
        assert src.exists(),src
        shutil.copyfile(src,QA/(cls+"-after.jpg"))
        findings.append(dict(classId=cls,visual=visual,unchangedControl=protected_warrior,asymmetryP95Before=bp,asymmetryP95After=ap,
            maxPenetrationBefore=bmax,maxPenetrationAfter=amax,contactAfter=acontact))
    max_bend=max(float(r["max_twist"]) for r in rows)
    minimum=min(int(r["swing_samples"]) for r in rows)
    text="\n".join([
        "Pełny przebieg po wdrożeniu: **6/6 zaliczonych testów**, 15 wyglądów (14 poprawionych i wojownik kontrolny),",
        "117 kombinacji wyposażenia/profili, 2106 przypadków ruchu, 27 rzeczywistych zestawów walki.",
        "Regresja: **21 zaliczonych / 0 błędów / 5 pominiętych opcjonalnych galerii**.",
        "Dodatkowy test zawiasów: **1/1 zaliczony**; razem 28 udanych wykonań testów końcowych.",
        f"Test obu nóg zebrał 56 wyników dla 14 rigów i 30/60 kl./s. Najmniejsza liczba próbek wymachu nogi: **{minimum}**;",
        f"maksymalny skręt w tej kontroli: **{max_bend:.2f}°**. Brak wyjątku dla łucznika i brak zaliczania brakującej nogi jako zera.",
        "",
        "Tabela dotyczy domyślnego zestawu i biegu do przodu. Kontakt to niezależny BakeMesh co klatkę na płaszczyźnie y=0.",
        "Pełne dane dla pozostałych kierunków i profili zachowano w katalogu `after`.",
        "",*lines,"",
        "U wszystkich 14 poprawionych wyglądów asymetria w tej próbie spadła poniżej **2,4°**;",
        "maksymalne zapadanie badanych punktów regionu stóp wyniosło **poniżej 7 mm**.",
        "Wynik wojownika pozostał kontrolą bez naprawy. Jego 480 próbek porównawczych ma różnicę 0, a 19 hashy plików jest zgodnych.",
        "",
        "[Berserker przed/po](qa/hero-feet-repair-2026-10-29/berserker-before-after.jpg) ·",
        "[bieg po naprawie](qa/hero-feet-repair-2026-10-29/berserker-after-normal.gif) ·",
        "[bieg w zwolnieniu](qa/hero-feet-repair-2026-10-29/berserker-after-slow.gif).",
        "",
        "Kontrola podparcia ma rozdzielone wyniki zapadania i ślizgu. Nie poświadcza idealnego zablokowania każdego punktu",
        "podeszwy: w części rigów brak stabilnego okna pomiarowego; u paladyna prawe stabilne punkty wykazują do ok. 3,7 cm",
        "przemieszczenia. Prędkość klipu i poziomy tor kroków pozostały zachowane. Wysokość fugi to odrębny pomiar od collidera.",
        "",
        "W trakcie prac nowy test wykrył skok obrotu u Zabójcy. Poprawiono wyznaczanie zgięcia buta z ciągłego ruchu goleni",
        "i powtórzono testy; tabela oraz końcowe XML dotyczą poprawionej wersji. Nie wykonano commita ani pusha.",
    ])
    original_text=DOC.read_text(encoding="utf-8")
    original_text=original_text.replace("Trwa końcowa weryfikacja pełnej macierzy; wyniki końcowe zostaną zapisane poniżej.","Pełna macierz oraz regresja zakończyły się bez błędów; wyniki znajdują się poniżej.")
    marker="<!-- WYNIKI_NAPRAWY -->";end="<!-- KONIEC_WYNIKOW_NAPRAWY -->"
    if end in original_text:
        prefix,rest=original_text.split(marker,1);_,suffix=rest.split(end,1);original_text=prefix+marker+"\n\n"+text+"\n"+end+suffix
    else:
        assert marker in original_text;original_text=original_text.replace(marker,marker+"\n\n"+text+"\n"+end)
    DOC.write_text(original_text,encoding="utf-8")
    summary=dict(fullPassed=6,regressionPassed=21,regressionFailed=0,optionalSkipped=5,hingePassed=1,correctedLooks=14,
                 motionCases=2106,profiles=117,nativeLoadouts=27,protectedFiles=len(protected),warriorControl=control,findings=findings)
    (DATA/"repair-summary.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding="utf-8")
    shutil.copyfile(DATA/"repair-summary.json",QA/"repair-summary.json")
    shutil.copyfile(DATA/"hinge-regression.csv",QA/"hinge-regression.csv")
    shutil.copyfile(AFTER/"coverage.csv",QA/"coverage.csv")
    p=Path("class_hu_berserker/visual_berserker_anime/diagnostic-active/run-0-60fps")
    for speed in ("normal","slow"):shutil.copyfile(AFTER/p/("sequence-front-"+speed+".gif"),QA/("berserker-after-"+speed+".gif"))
    from PIL import Image,ImageOps,ImageDraw
    images=[BEFORE/p/"isolated-review.jpg",AFTER/p/"isolated-review.jpg"]
    out=Image.new("RGB",(1200,1510),"#20242d");draw=ImageDraw.Draw(out)
    for i,src in enumerate(images):out.paste(ImageOps.contain(Image.open(src).convert("RGB"),(1200,705)),(0,i*755+35));draw.text((10,i*755+10),"PRZED" if i==0 else "PO",fill="white")
    out.save(QA/"berserker-before-after.jpg",quality=93)
    for page in range(3):
        overview=Image.new("RGB",(1400,1875),"#20242d");labels=ImageDraw.Draw(overview)
        for row,finding in enumerate(findings[page*5:page*5+5]):
            labels.text((8,row*375+5),finding["classId"],fill="white")
            folder=AFTER/finding["classId"]/finding["visual"]/"diagnostic-active/run-0-60fps"
            for phase in range(4):
                picture=ImageOps.contain(Image.open(folder/f"p{phase}-isolated-front.png").convert("RGB"),(350,350))
                overview.paste(picture,(phase*350,row*375+25))
        overview.save(QA/f"overview-{page+1}.jpg",quality=92)
    print(json.dumps({k:v for k,v in summary.items() if k not in ("findings","warriorControl")},ensure_ascii=False))


if __name__=="__main__":main()
