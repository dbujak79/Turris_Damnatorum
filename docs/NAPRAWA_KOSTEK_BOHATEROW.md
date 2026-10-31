# Naprawa kostek i podparcia bohaterów

Data: 2026-10-29. Naprawa po [audycie](RAPORT_WERYFIKACJI_ANATOMII_BOHATEROW.md).

**Korekta została wdrożona do 14 aktywnych wyglądów. Wojownik zachowuje dotychczasową animację.**
Pełna macierz oraz regresja zakończyły się bez błędów; wyniki znajdują się poniżej.

## Zmiana w grze

`CharacterVisualDefinition.anatomicalFeet` włącza `AnatomicalFootPose` dla konkretnego modelu.
Położenie kostek i rytm kroków pochodzą z obecnych klipów. Korekta odtwarza zawias kolana
w osiach danego rigu, ustawia stopę względem aktualnej goleni oraz zachowuje stałe długości
kości. Obrót buta w wymachu wynika z ruchu goleni. Palce zachowują długość śródstopia;
miękkie obuwie ma ograniczony zakres zgięcia, a stalowe noski pozostają sztywne.

Przy budowie postaci BakeMesh kalibruje obwód podeszwy. Podczas gry sprawdzane są jego punkty
i w razie potrzeby podnoszona jest kostka przez dwukościowe IK. W pętli animacji nie ma
BakeMesh ani kopiowania całej siatki. Model, tekstury, siatka i wagi źródłowe pozostają zachowane.

Wskazane modele pomijają dawną stałą poprawkę yaw mierzoną w podporze. Globalne parametry
`FootFixStanceGain` i `FootFixSwingGain` pozostają bez zmian. Dotychczasowy tor kroków,
animacje tułowia i chwyt broni są zachowane. Korekta wygasza się wraz z pełną warstwą akcji;
kroki serii i doczepienie broni nadal wykonują istniejące sterowniki.

Paladyn zachowuje wcześniejsze prowadzenie nóg podczas ruchu do przodu; nowa kontrola
uzupełnia podparcie. Wojownik ma wyłączoną nową flagę i nadal korzysta z poprzedniej ścieżki.

## Wdrożone wyglądy

Berserker, Rębajło, mag bitewny, Łotrzyk, łucznik, łucznik arkanów, Zabójca, mag ognia,
wody, powietrza i ziemi, Przywoływacz, bazowy mag anime oraz paladyn anime.
Flagi zapisano przez Unity API w ich aktywnych definicjach. Importery nowych modeli
berserkera/maga i paladyna zachowują włączenie korekty przy ponownym imporcie.

Hell Unleashed nie był modyfikowany. Modele i tekstury nie były generowane ponownie.

## Walidacja

- `RetargetFootRepairPlayTests.B_AktywneRigi_SkretICiaglosc`: 14 aktywnych rigów, obie nogi,
  bieg przy 30/60 kl./s; obecność rzeczywistych próbek wymachu, ograniczenie skrętu i kontrola
  skoku obrotu między klatkami. Brak próbek nogi powoduje błąd testu. Test działa również
  w zwykłej regresji PlayMode; porównanie diagnostyczne A wymaga uruchomienia z menu.
- Kontrola porównawcza na tymczasowych kopiach: berserker, mag, łucznik, paladyn i wojownik.
  Zrzuty i CSV zachowane w `TestResults/hero-feet-repair`.
- Kontrola hashy **19 plików wojownika i wspólnych zestawów animacji**.
- Pełny zapis po zmianie: `TestResults/hero-feet-repair/after`, osobno od pomiarów sprzed naprawy.
  Ruch i broń, arena, klatki ze szkieletem, wycinki nóg oraz niezależny BakeMesh podparcia.

<!-- WYNIKI_NAPRAWY -->

Pełny przebieg po wdrożeniu: **6/6 zaliczonych testów**, 15 wyglądów (14 poprawionych i wojownik kontrolny),
117 kombinacji wyposażenia/profili, 2106 przypadków ruchu, 27 rzeczywistych zestawów walki.
Regresja: **21 zaliczonych / 0 błędów / 5 pominiętych opcjonalnych galerii**.
Dodatkowy test zawiasów: **1/1 zaliczony**; razem 28 udanych wykonań testów końcowych.
Test obu nóg zebrał 56 wyników dla 14 rigów i 30/60 kl./s. Najmniejsza liczba próbek wymachu nogi: **29**;
maksymalny skręt w tej kontroli: **11.00°**. Brak wyjątku dla łucznika i brak zaliczania brakującej nogi jako zera.

Tabela dotyczy domyślnego zestawu i biegu do przodu. Kontakt to niezależny BakeMesh co klatkę na płaszczyźnie y=0.
Pełne dane dla pozostałych kierunków i profili zachowano w katalogu `after`.

| Postać | Asymetria wymachu P95 przed → po | Zapadanie badanych punktów, max przed → po |
|---|---:|---:|
| [Łucznik arkanów](qa/hero-feet-repair-2026-10-29/class_hu_arcane_archer-after.jpg) | 128.42° → 0.49° | 5.32 → 0.04 cm |
| [Łucznik](qa/hero-feet-repair-2026-10-29/class_hu_archer-after.jpg) | 113.06° → 0.00° | 4.79 → 0.00 cm |
| [Zabójca](qa/hero-feet-repair-2026-10-29/class_hu_assassin-after.jpg) | 149.87° → 2.37° | 4.56 → 0.15 cm |
| [Mag bitewny](qa/hero-feet-repair-2026-10-29/class_hu_battlemage-after.jpg) | 27.60° → 0.00° | 7.74 → 0.00 cm |
| [Berserker](qa/hero-feet-repair-2026-10-29/class_hu_berserker-after.jpg) | 35.66° → 0.01° | 10.18 → 0.43 cm |
| [Rębajło](qa/hero-feet-repair-2026-10-29/class_hu_dual-after.jpg) | 36.34° → 0.00° | 8.25 → 0.00 cm |
| [Mag powietrza](qa/hero-feet-repair-2026-10-29/class_hu_mage_air-after.jpg) | 56.96° → 0.00° | 7.38 → 0.00 cm |
| [Mag ziemi](qa/hero-feet-repair-2026-10-29/class_hu_mage_earth-after.jpg) | 104.02° → 0.00° | 8.92 → 0.46 cm |
| [Mag ognia](qa/hero-feet-repair-2026-10-29/class_hu_mage_fire-after.jpg) | 94.79° → 0.00° | 9.98 → 0.36 cm |
| [Mag wody](qa/hero-feet-repair-2026-10-29/class_hu_mage_water-after.jpg) | 139.85° → 0.00° | 5.23 → 0.09 cm |
| [Paladyn](qa/hero-feet-repair-2026-10-29/class_hu_paladin-after.jpg) | 2.25° → 2.25° | 2.67 → 0.00 cm |
| [Przywoływacz](qa/hero-feet-repair-2026-10-29/class_hu_summoner-after.jpg) | 44.19° → 1.44° | 6.13 → 0.63 cm |
| [Wojownik — bez zmian](qa/hero-feet-repair-2026-10-29/class_knight-after.jpg) | 116.14° → 116.14° | 6.71 → 6.71 cm |
| [Mag anime](qa/hero-feet-repair-2026-10-29/class_mage-after.jpg) | 49.72° → 0.01° | 12.35 → 0.69 cm |
| [Łotrzyk](qa/hero-feet-repair-2026-10-29/class_rogue-after.jpg) | 101.67° → 0.00° | 2.88 → 0.44 cm |

U wszystkich 14 poprawionych wyglądów asymetria w tej próbie spadła poniżej **2,4°**;
maksymalne zapadanie badanych punktów regionu stóp wyniosło **poniżej 7 mm**.
Wynik wojownika pozostał kontrolą bez naprawy. Jego 480 próbek porównawczych ma różnicę 0, a 19 hashy plików jest zgodnych.

[Berserker przed/po](qa/hero-feet-repair-2026-10-29/berserker-before-after.jpg) ·
[bieg po naprawie](qa/hero-feet-repair-2026-10-29/berserker-after-normal.gif) ·
[bieg w zwolnieniu](qa/hero-feet-repair-2026-10-29/berserker-after-slow.gif).

Kontrola podparcia ma rozdzielone wyniki zapadania i ślizgu. Nie poświadcza idealnego zablokowania każdego punktu
podeszwy: w części rigów brak stabilnego okna pomiarowego; u paladyna prawe stabilne punkty wykazują do ok. 3,7 cm
przemieszczenia. Prędkość klipu i poziomy tor kroków pozostały zachowane. Wysokość fugi to odrębny pomiar od collidera.

W trakcie prac nowy test wykrył skok obrotu u Zabójcy. Poprawiono wyznaczanie zgięcia buta z ciągłego ruchu goleni
i powtórzono testy; tabela oraz końcowe XML dotyczą poprawionej wersji. Nie wykonano commita ani pusha.
<!-- KONIEC_WYNIKOW_NAPRAWY -->

## Narzędzia

- `AnatomicalFootPose.cs`: korekta w runtime, tylko dla oznaczonych modeli.
- `HeroFeetRepair.cs`: menu **Turris → Naprawa stóp → Włącz korektę aktywnych bohaterów**.
- `RetargetFootRepairLiveRunner`: porównanie, zawiasy, pełna macierz i regresja w otwartym Unity.

Powrót do wcześniejszej animacji danego wyglądu: wyłączyć `anatomicalFeet` w jego definicji
przez Inspector/Unity API i odtworzyć postać, np. przez ponowne uruchomienie Play.
Nie wymaga przywracania modelu ani tekstur.
