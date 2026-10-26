# Audyt skilli i pieczęci (pomiar na manekinach)

Stan: 2026-10-26, gałąź `feature/urp-vfx-migration`. Dane: `TestResults/hu-bench/bench.csv` (642 pomiary, manekiny bez
odporności), porównawczo `bench_manekiny_zywiolowe.csv` (manekiny z odpornościami gry). Test:
`Tests/PlayMode/HuBenchPlayTests.cs`, analiza: `python tools/balance/bench_report.py TestResults/hu-bench/bench.csv`.

## Wdrożone korekty (2026-10-26, `Seals/HuBalance.cs`, testy w `HuContentTests`)

| Ustalenie | Korekta | Pomiar przed → po |
|---|---|---|
| P1-a Aura świętego ognia | impuls 12/17/24 → 8/11/14; Sztandar ×1,5 → ×1,25 (opis „o jedną czwartą”) | 300/60 → **175/35** (kolos 1,24× mediany); Sztandar 615/90 → **306/44** |
| P1-b sługi | obrażenia: chochliki ×0,4 (5/6/8), golem ×0,45 (10+5 / 13+6), ogary ×0,6 (4/5/6); liczebność i czas życia bez zmian | kolos 170/155/106 (okno = odnowienie) → szac. ≈ 68/70/64 (test: ≤ 25 HU/s) |
| P2-b formy dominujące | +1,5 s odnowienia: Kula błyskawic, Bumerang (cięcie wiatru, strzała odłamkowa), Napalm; Lepka bomba odnowienie 4 → 5 s | szac. 1,65–2,2× → 1,3–1,5× bazy |
| P2-c formy martwe | Mina siarkowa 2 → 3 ładunki; Deszcz meteorów 60% → 80% na meteor | 0,5–0,6× → szac. 0,8× |

Bez zmian (decyzja): Deszcz strzał (późny skok mocy, obserwować), Huragan kroku wiatru (ratuje słaby skill ruchu),
wsparcia demoniczne (rzadkie), Legion (2,2× przy odnowieniu 28 s), formy użytkowe (Kuźnia, Odpychające oko) – do grania.
Pomiar z oknem = odnowienie (`bench_po_aurze.csv`) potwierdził sługi: chochliki 170/s, golem 155/s, ogary 106/s przy
medianie kolosa 28/s.

Oznaczenia: **[P]** pomiar, **[S]** szacunek z pomiaru, **[H]** hipoteza projektowa do sprawdzenia w grze.

## 1. Metoda i ograniczenia

- Każdy z 61 skilli HU na poziomie III i każda forma: jeden rzut w **gromadę** (6 celów w kręgu 2,2 m) i w **kolosa**
  (jeden cel). 51 wsparć na 10 skillach testowych (gromada). Liczone wszystkie obrażenia (`CombatEvents.Impact`: trafienia,
  sługi, strefy, tyknięcia). Wynik = obrażenia jednego rzutu / odnowienie („na sekundę cyklu”); aury – obrażenia / czas.
- Ograniczenia (ważne przy czytaniu liczb):
  1. **Okno 6 s** – sługi (do 18 s) i długie strefy są **zaniżone**. Test już poprawiony (okno = odnowienie, 6–20 s);
     potrzebny ponowny przebieg.
  2. Bohater testowy to **mag**: techniki fizyczne liczone bronią maga – porównuj je między sobą, nie z czarami.
  3. Manekiny stoją: nie liczy się mobilność, unik, obrona, zasięg ani ryzyko; **wyzwalacze** (unik, zabójstwo, obrażenia)
     nie mają okazji zadziałać, więc ich wynik < 1 nic nie mówi.
  4. Geometria: stożki zależą od ustawienia – **Rozpłatanie** dało 95 i 285 obrażeń w dwóch przebiegach (1 vs 3 cele),
     więc jego wyniki i mnożniki na nim są niewiarygodne.
  5. Koszt w zdrowiu (Ofiara krwi, Pakt) nie jest odejmowany w kolumnie kosztu many/wytrzymałości.

## 2. Obraz ogólny [P]

| | Gromada | Kolos |
|---|---:|---:|
| Mediana skilli zadających obrażenia (na s cyklu) | 96 | 27 |
| Stosunek najlepszy / mediana | 3,7× (Deszcz strzał) | 3,5× (chochliki) |

Skille obszarowe (Barrage, Rain, Nova, Wave) zbierają 3–8× więcej na gromadzie niż na kolosie – to zamierzony podział
ról. Problemem są skille **dobre w obu kontekstach naraz**, bez ceny w innym wymiarze.

## 3. Ustalenia

### P1-a. Aura świętego ognia – stałe obrażenia bez kosztu akcji [P]

- 300/s na gromadzie, **60/s na kolosie (2,2× mediany)**. Forma **Sztandar: 615 / 90** – najlepszy wynik w obu
  kontekstach spośród wszystkich skilli i form.
- Aura nie zajmuje czasu akcji: dokłada się do całej rotacji. Koszt: rezerwacja 20% wytrzymałości i walka w promieniu 3,5 m.
- Test pięciu kontekstów: gromada ✔✔, elita ✔, boss ✔ (stoi przy nim tak czy inaczej), zagrożenie obronne ✖ (wymusza
  bliskość – jedyna cena), niedobór zasobów ✔ (nic nie wydaje). Dominuje w 4 z 5.
- Opcje:
  - A. Obrażenia impulsu III 24 → 14 (≈ 35/s na cel, 1,3× mediany kolosa). Prosto, zachowuje fantazję „ognia wokół”.
  - B. Rezerwacja 20% → 35% wytrzymałości. Cena widoczna w walce (mniej uników/bloków), ale nie zmienia wyniku ofensywnego.
  - C. Limit 3 celów na impuls. Tnie tylko gromadę, kolos zostaje 2×.
  - **Rekomendacja do testu: A** (w `HuBalance.cs`: `skill.aura.holy_fire` poziom 3 `damage.fire=14`, poziom 2 `=11`,
    poziom 1 `=8`) i osobno Sztandar `banner_power` 1,5 → 1,25. **[H]** cel: Sztandar ≤ 1,5× mediany kolosa.

### P1-b. Sługi nadal najsilniejsze na pojedynczy cel [S – zaniżone oknem]

- W 6 s okna: chochliki 946 obrażeń kolosowi (**≈ 158/s**), golem 1014 (**≈ 169/s**), ogary 634 (≈ 106/s), Legion 676
  (≈ 113/s). Czas życia ≈ odnowienie (korekta P1-b z poprzedniego audytu), więc **na cykl to ≈ 4–6× mediany kolosa**.
- Manekiny nie atakują sług – w grze sługi giną i rozpraszają się, co obniża wynik. Mimo to skala odchyłki jest za duża,
  żeby ją wyjaśnić samym brakiem przeciwnika.
- Najpierw **ponowny pomiar z oknem = odnowienie** (test już poprawiony). Opcje po potwierdzeniu:
  - A. Obrażenia sług ×0,6 (najprostsze, jedno miejsce w `HuBalance`).
  - B. Czas życia ½ odnowienia (sługi jako „wybuch mocy”, nie stała obecność).
  - C. Sługi dzielą zasoby z bohaterem (np. utrzymanie many) – większa zmiana systemowa.
  - **Rekomendacja: A dla golema i chochlików**, B tylko jeśli w grze nadal są stałe. **[H]** cel: ≤ 2× mediany kolosa.

### P2-a. Deszcz strzał – mocny wszędzie [P]

- 359/s gromada (najlepszy), 51/s kolos (1,9× mediany). Dostępny od piętra 3 (`minWave` 16) – późny skok mocy jest
  zamierzony; wymaga celowania i stania celu w strefie (manekiny zawyżają).
- **Rekomendacja: obserwować**, bez korekty. Jeśli w grze dominuje: `zone_seconds` III 3,5 → 3,0.

### P2-b. Formy dominujące w obu kontekstach (≥ 1,6× bazy) [P]

Kula błyskawic (Łańcuch, 1,7/2,2), Wyrok pojedynczy (2,2/1,75), Napalm (2,0/2,05), Lepka bomba (1,65/1,65),
Bumerang cięcia wiatru (2,0/2,0), Huragan (kroku wiatru, 3,3/1,6). Forma ma zmieniać sposób gry, nie być darmowym
mnożnikiem. **Rekomendacja:** każdej dać cenę w innym wymiarze zamiast cięcia obrażeń – np. +odnowienie 1–2 s
(jak Słońce, Sopel), wolniejszy pocisk albo krótszy zasięg. Kandydaci do testu: Napalm i Bumerang (+1,5 s odnowienia).

### P2-c. Formy martwe w obu kontekstach (≤ 0,7× bazy) [P]

Odpychające oko cyklonu (0,24/0,65), Smoła sideł (0,50/0,54), Gwiazda kolców (0,69/0,60), Mina siarkowa (0,54/0,47).
Kuźnia pierścienia (0,02) – **prawdopodobnie użytkowa** (działa przez ciosy bronią, których test nie wykonuje) – do
sprawdzenia w grze, nie do korekty.
- Formy kontroli (odpychanie, smoła) mogą być słabsze w obrażeniach, jeśli dają kontrolę – ale wtedy kontrola musi być
  odczuwalna. **Rekomendacja:** w grze sprawdzić czas kontroli; Minę siarkową podnieść do ≥ 0,8× (np. +1 mina).

### P2-d. Wsparcia demoniczne [P]

Brama piekieł ×2,6, Korona Welesa ×2,3, Serce otchłani ×2,0 (kosztuje 5% zdrowia), Upadek piekieł ×1,7 – zwykłe
najlepsze: Wachlarz ×1,9, Kaskada ×1,7. Demoniczne są rzadkie (elity 25%, dar bossa), więc silniejszy efekt jest
uzasadniony. **Rekomendacja:** bez korekty, ale Brama piekieł i Korona razem na jednym skillu sprawdzić osobno
(suma mnożników ma limit +200%, kopie 1,5× – pomiar par nie był robiony).

### P3. Słabe skille [P]

Krzyżowe cięcie 23/s, Wachlarz ostrzy 19/s, Legion 24/s (odnowienie 28 s – patrz P1-b), Znak śmierci 28/s, Rzut toporem
42/s. Część to techniki fizyczne (liczone bronią maga) albo skille ruchu – **potrzebny pomiar per klasa** przed decyzją.

### Odporności wrogów (z przebiegu 1) [P]

Na ognistych chochlikach skille ognia tracą 57%, wody zyskują 45% – zamierzone (chochliki mają warianty żywiołów).
**[H]** Sprawdzić w grze, czy mag ognia nie trafia za często na piętra z samymi ognistymi chochlikami.

## 4. Pętle i bezpieczniki

- Wyzwalacze mają limit co 1,5 s i połowę kosztu; kopie ≤ 1,5× i ≤ 24 zdarzenia (z HU) – pomiar nie wykrył lawin.
- Znaleziony i naprawiony **błąd wielobieżności**: trafienie wywołujące kolejne trafienie (impuls aury → efekt
  „przy trafieniu”) psuło listę celów w `HitQuery` (wyjątek, utracone obrażenia). Test regresji w EditMode.

## 5. Plan walidacji

1. Ponowny pomiar z oknem = odnowienie (sługi, strefy) – potwierdza albo obala P1-b.
2. Pomiar per klasa (każda rodzina z własną bronią) – techniki fizyczne (P3).
3. Korekty P1-a (i P1-b po potwierdzeniu) w `HuBalance.cs` z testem w `HuContentTests`, potem ponowny pomiar.
4. Granie: Aura świętego ognia i sługi na piętrach 1–6 (czas zabijania bossa, liczba śmierci), formy kontroli.

Progi w tym dokumencie to hipotezy, nie prawa gatunku: skill może być lokalnie silniejszy, jeśli płaci w innym wymiarze.
