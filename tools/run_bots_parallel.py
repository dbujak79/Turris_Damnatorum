"""
Równoległe serie botów na kopiach projektu (ten sam projekt nie może być otwarty w dwóch Unity naraz).

Każda kopia (domyślnie C:/GIT/Gry/Turris_Damnatorum_kopie/kopiaN) to lustro Assets, Packages i ProjectSettings
(robocopy /MIR przed każdym uruchomieniem – kopiuje tylko zmiany) oraz jednorazowo skopiowany Library (bez niego pierwszy
import trwa długo; potem Unity sam doimportowuje zmienione pliki). Zadania (filtry testów) trafiają do wolnych kopii po kolei.
Wyniki serii (JSON, raporty, powtórki) zapisują się do wspólnego katalogu --out w repozytorium – pliki mają w nazwie klasę
i osobowość, więc kopie sobie nie przeszkadzają. Wyzwania i profile botów mają unikalne nazwy w persistentDataPath.

Przykłady:
  python tools/run_bots_parallel.py --preset mag-zywioly --out docs/raporty_botow/mag-zywioly-v1 --runs 10
  python tools/run_bots_parallel.py --out docs/raporty_botow/x Turris.Tests.Bots.MageBotPlaytests.Mage_Tester Turris.Tests.Bots.WarriorBotPlaytests.Warrior_Tester
  python tools/run_bots_parallel.py --sync-only --copies 4        (tylko przygotowanie kopii)

Uwaga: przed pierwszą synchronizacją (kopiowanie Library) projekt główny nie powinien być otwarty w edytorze ani w teście.
"""
import argparse
import os
import queue
import subprocess
import sys
import threading
import time
import xml.etree.ElementTree as ET

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
UNITY = os.environ.get("UNITY_EXE", r"C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe")
DEFAULT_COPIES = os.path.join(os.path.dirname(ROOT), "Turris_Damnatorum_kopie")

BOTS = "Turris.Tests.Bots."
PRESETS = {
    "mag-zywioly": [BOTS + "MageElementBotPlaytests." + e + "_" + p
                    for e in ("Fire", "Water", "Air", "Earth") for p in ("SoulsSlasher", "Tester")],
    "mag-powietrze": [BOTS + "MageElementBotPlaytests.Air_" + p for p in ("SoulsSlasher", "Tester")],
    "wojownik-specjalizacje": [BOTS + "WarriorSpecBotPlaytests." + c + "_" + p
                               for c in ("Berserker", "Paladin", "Dual") for p in ("SoulsSlasher", "Tester")],
    "mag": [BOTS + "MageBotPlaytests.Mage_SoulsSlasher", BOTS + "MageBotPlaytests.Mage_Tester"],
    "wojownik": [BOTS + "WarriorBotPlaytests.Warrior_SoulsSlasher", BOTS + "WarriorBotPlaytests.Warrior_Tester"],
}

MIRRORED = ("Assets", "Packages", "ProjectSettings")


def robocopy(src, dst, mirror=True):
    args = ["robocopy", src, dst, "/E", "/NFL", "/NDL", "/NJH", "/NJS", "/NP", "/R:1", "/W:1", "/MT:16"]
    if mirror:
        args[3] = "/MIR"
    rc = subprocess.call(args, stdout=subprocess.DEVNULL)
    if rc >= 8:  # robocopy: 0–7 = sukces (z różnymi rodzajami zmian)
        raise RuntimeError(f"robocopy {src} -> {dst} zakończył się kodem {rc}")


def sync_copy(copy_dir):
    os.makedirs(copy_dir, exist_ok=True)
    for d in MIRRORED:
        robocopy(os.path.join(ROOT, d), os.path.join(copy_dir, d))
    lib = os.path.join(copy_dir, "Library")
    if not os.path.isdir(lib):
        print(f"[{os.path.basename(copy_dir)}] pierwsze kopiowanie Library (kilka GB)…", flush=True)
        robocopy(os.path.join(ROOT, "Library"), lib, mirror=False)
    # Pozostałości poprzedniego uruchomienia blokowałyby start Unity.
    lock = os.path.join(copy_dir, "Temp", "UnityLockfile")
    if os.path.exists(lock):
        os.remove(lock)


def run_job(copy_dir, test, args, results_dir):
    name = test.split(".")[-2] + "." + test.split(".")[-1]
    xml = os.path.join(results_dir, name + ".xml")
    log = os.path.join(results_dir, name + ".log")
    env = dict(os.environ)
    env["TURRIS_BOT_DIR"] = os.path.abspath(args.out)
    env["TURRIS_BOT_RUNS"] = str(args.runs)
    if args.realtime:
        env["TURRIS_BOT_REALTIME"] = "1"
    cmd = [UNITY, "-batchmode", "-nographics", "-projectPath", copy_dir, "-runTests", "-testPlatform", "PlayMode",
           "-testResults", xml, "-logFile", log, "-testFilter", test]
    t0 = time.time()
    print(f"[{os.path.basename(copy_dir)}] start: {name}", flush=True)
    rc = subprocess.call(cmd, env=env)
    minutes = (time.time() - t0) / 60
    result = "brak wyników"
    if os.path.exists(xml):
        r = ET.parse(xml).getroot()
        result = f"{r.get('result')} (zaliczone {r.get('passed')}, niezaliczone {r.get('failed')}, pominięte {r.get('skipped')})"
    print(f"[{os.path.basename(copy_dir)}] koniec: {name} – {result}, {minutes:.1f} min, kod {rc}", flush=True)
    return name, result, minutes


def main():
    ap = argparse.ArgumentParser(description="Równoległe serie botów na kopiach projektu.")
    ap.add_argument("tests", nargs="*", help="pełne nazwy testów (filtry -testFilter)")
    ap.add_argument("--preset", choices=sorted(PRESETS), help="gotowy zestaw testów")
    ap.add_argument("--out", help="katalog wyników serii (TURRIS_BOT_DIR)")
    ap.add_argument("--runs", type=int, default=20, help="podejść na serię (TURRIS_BOT_RUNS)")
    ap.add_argument("--copies", type=int, default=4, help="najwięcej kopii naraz")
    ap.add_argument("--first-copy", type=int, default=1, help="numer pierwszej kopii (np. 3, gdy 1–2 są zajęte)")
    ap.add_argument("--copies-root", default=DEFAULT_COPIES, help="katalog z kopiami projektu")
    ap.add_argument("--realtime", action="store_true", help="normalny czas (TURRIS_BOT_REALTIME=1)")
    ap.add_argument("--sync-only", action="store_true", help="tylko przygotuj/zaktualizuj kopie")
    args = ap.parse_args()

    tests = list(args.tests) + (PRESETS[args.preset] if args.preset else [])
    n = max(1, min(args.copies, len(tests))) if tests and not args.sync_only else args.copies
    copies = [os.path.join(args.copies_root, f"kopia{i + args.first_copy}") for i in range(n)]
    for c in copies:
        print(f"Synchronizacja {c}…", flush=True)
        sync_copy(c)
    if args.sync_only:
        print("Kopie gotowe.")
        return 0
    if not tests or not args.out:
        ap.error("podaj testy (albo --preset) i --out")

    os.makedirs(args.out, exist_ok=True)
    results_dir = os.path.abspath(os.path.join(args.out, "_unity"))  # Unity liczy ścieżki względem projektu (kopii)
    os.makedirs(results_dir, exist_ok=True)
    jobs = queue.Queue()
    for t in tests:
        jobs.put(t)
    done, lock = [], threading.Lock()

    def worker(copy_dir):
        while True:
            try:
                t = jobs.get_nowait()
            except queue.Empty:
                return
            res = run_job(copy_dir, t, args, results_dir)
            with lock:
                done.append(res)

    t0 = time.time()
    threads = [threading.Thread(target=worker, args=(c,)) for c in copies]
    for th in threads:
        th.start()
        time.sleep(20)  # rozłożenie startów (import i kompilacja obciążają dysk)
    for th in threads:
        th.join()
    print(f"\nWszystkie zadania: {(time.time() - t0) / 60:.1f} min")
    for name, result, minutes in sorted(done):
        print(f"  {name}: {result}, {minutes:.1f} min")
    return 0 if all(r[1].startswith("Passed") for r in done) else 1


if __name__ == "__main__":
    sys.exit(main())
