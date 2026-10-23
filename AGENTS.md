# Turris Damnatorum – wskazówki dla asystenta

- Język projektu, komentarzy, UI i dokumentacji: **polski**.
- Najpierw przeczytaj `docs/PROGRESS.md` (stan, historia, pułapki, plan), potem `README.md` (architektura, sterowanie).
- Unity 6000.6.3f1 (`C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe`). Kod w `Assets/TurrisDamnatorum/Scripts`, testy w `Assets/TurrisDamnatorum/Tests`.
- Testy/kompilacja: `python tools/run_tests.py EditMode` oraz `python tools/run_tests.py PlayMode window` – **edytor musi być zamknięty** (poproś użytkownika).
- Zmiany wyglądu weryfikuj zrzutami: `TURRIS_SHOT_DIR=<katalog> python tools/run_tests.py PlayMode gfx`, obejrzyj PNG przed ogłoszeniem wyniku.
- Nie uruchamiaj `TurrisSetup.RunBatchForce` bez zgody – nadpisuje assety treści.
- Commit/push tylko na prośbę użytkownika.
- Po istotnej pracy zaktualizuj `docs/PROGRESS.md` (stan, wyniki testów, metryki, następne kroki).
