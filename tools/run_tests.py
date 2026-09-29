"""Uruchamia testy Unity projektu Turris Damnatorum i wypisuje czytelne podsumowanie.

Użycie:
    python tools/run_tests.py EditMode [batch|gfx|window] [filtr]
    python tools/run_tests.py PlayMode window
    python tools/run_tests.py PlayMode gfx Turris.Tests.AnimationPlayTests

Tryby:
    batch  (domyślny) – -batchmode -nographics; najszybszy. OnGUI nie działa → testy pada są pomijane.
    gfx    – -batchmode z grafiką; działają zrzuty z kamery (galerie), OnGUI nadal nie.
    window – bez -batchmode (otwiera się okno edytora); jedyny tryb, w którym przechodzą testy menu padem.

Wymagania: projekt NIE może być otwarty w edytorze Unity (blokada projektu).
Ścieżkę do Unity można podać zmienną UNITY_EXE. Galerie zrzutów włącza zmienna TURRIS_SHOT_DIR.
Wyniki: TestResults/<platforma>.xml i TestResults/<platforma>.log.
"""
import os
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
UNITY = os.environ.get("UNITY_EXE", r"C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe")


def main():
    try:
        sys.stdout.reconfigure(errors="replace")
    except Exception:
        pass
    if len(sys.argv) < 2 or sys.argv[1] not in ("EditMode", "PlayMode"):
        print(__doc__)
        return 2
    platform = sys.argv[1]
    mode = sys.argv[2] if len(sys.argv) > 2 else "batch"
    test_filter = sys.argv[3] if len(sys.argv) > 3 else None

    out_dir = os.path.join(ROOT, "TestResults")
    os.makedirs(out_dir, exist_ok=True)
    xml_path = os.path.join(out_dir, platform + ".xml")
    log_path = os.path.join(out_dir, platform + ".log")
    for f in (xml_path, log_path):
        if os.path.exists(f):
            os.remove(f)

    cmd = [UNITY, "-projectPath", ROOT, "-runTests", "-testPlatform", platform,
           "-testResults", xml_path, "-logFile", log_path]
    if mode in ("batch", "gfx"):
        cmd.insert(1, "-batchmode")
    if mode == "batch":
        cmd.insert(2, "-nographics")
    if test_filter:
        cmd += ["-testFilter", test_filter]

    rc = subprocess.call(cmd)
    print("Kod wyjścia Unity:", rc)

    log = open(log_path, encoding="utf-8", errors="replace").read() if os.path.exists(log_path) else ""
    issues = sorted(set(l.strip() for l in log.splitlines() if "error CS" in l or "warning CS" in l))
    for line in issues[:30]:
        print(line)
    if "another Unity instance" in log or ("Aborting" in log and not os.path.exists(xml_path)):
        print("Uwaga: czy projekt nie jest otwarty w edytorze?")
    if not os.path.exists(xml_path):
        print("Brak wyników testów (sprawdź log:", log_path, ")")
        return 1

    root = ET.parse(xml_path).getroot()
    print(f"Wynik: {root.get('result')}  razem {root.get('total')}  zaliczone {root.get('passed')}  "
          f"niezaliczone {root.get('failed')}  pominięte {root.get('skipped')}")
    for tc in root.iter("test-case"):
        result = tc.get("result")
        tag = {"Passed": "  ok     ", "Skipped": "  pomin. "}.get(result, "  BŁĄD   ")
        print(tag + tc.get("name"))
        if result not in ("Passed",):
            msg = tc.find(".//message")
            if msg is not None and msg.text:
                print("         " + msg.text.strip().splitlines()[0][:300])
    for line in log.splitlines():
        if line.startswith("DETAIL ") or line.startswith("ARENA "):
            print(line)
    return 0 if root.get("failed") in ("0", None) else 1


if __name__ == "__main__":
    sys.exit(main())
