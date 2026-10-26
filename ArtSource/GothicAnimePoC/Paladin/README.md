# Paladyn anime — wdrożenie modelu PoC

2026-10-26, branch `codex/paladin-gothic-anime`. Model zastępuje wygląd **paladyna**; wojownik pozostaje dotychczasowy.

![Paladyn na pierwszej arenie, kamera gry](../../../TestResults/paladin-anime/paladin-game.png)

## Pliki i powtórzenie importu

- [Model roboczy z wagami](paladin_anime_rigged.blend), [raport rigowania](rig-report.json).
- [Model Unity](../../../Assets/TurrisDamnatorum/Models/PoC/PaladinAnime/paladin_anime.fbx),
  [materiał](../../../Assets/TurrisDamnatorum/Models/PoC/PaladinAnime/paladin_anime.mat),
  [tekstura 2K](../../../Assets/TurrisDamnatorum/Models/PoC/PaladinAnime/paladin_anime_base_color.png).
- [Definicja wyglądu](../../../Assets/TurrisDamnatorum/Content/Visuals/visual_paladin_anime.asset).
- [Celowany importer](../../../Assets/TurrisDamnatorum/Scripts/Editor/PaladinAnimeImport.cs):
  **Turris → PoC → Włącz model anime dla paladyna**, albo `Turris.EditorTools.PaladinAnimeImport.Run()` przez Unity MCP.

Odtworzenie FBX z katalogu głównego projektu:

```powershell
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' -b --python tools/art/rig_anime_paladin.py
```

Następnie odświeżyć assety Unity i uruchomić powyższy importer. Skrypt Blendera zapisuje nowy FBX w katalogu PoC;
nie nadpisuje modeli źródłowych ani wojownika. Nie korzysta z Meshy i nie wydaje kredytów.

## Szkielet, wagi i wygląd

Źródło geometrii: [warrior-v2-grip.blend](../Warrior/warrior-v2-grip.blend), ze zwiniętymi palcami obu dłoni.
Szkielet skopiowano z istniejącego `Models/Heroes/hero_warrior/hero_warrior.fbx` w Turris, dopasowano położenie stawów
i wypieczono pozę T. Hierarchia ma 26 kości, w tym `HandSlot.R/L`; palce dłoni są ułożone w geometrii.
Bone heat nie znalazł rozwiązania na wielowarstwowej siatce generatora. Wagi nadano jawnie według regionów anatomicznych,
z płynnymi przejściami przy stawach. Tułów korzysta z Hips, Spine, Spine1 i Spine2; pierwotne przypięcie brzucha i klatki
wyłącznie do Spine2 dawało sztywną bryłę pomimo ruchu szkieletu. Dłonie i dolne części stalowych butów mają sztywne przypisanie do swoich kości.
Wszystkie wierzchołki są ważone, maksymalnie do czterech kości; kości gniazd nie deformują siatki.

Wysokość źródłowa 1,8 m, 45 832 trójkąty, jedna mapa koloru 2048×2048 i jeden materiał ciała.
Matowy shader `Turris/AnimeMatte` współpracuje z istniejącym `HellBodySurface` i sygnałami `TintSet`.
Ma pasma światła rozproszonego, cienie oraz światła dodatkowe Forward+, bez połysku i odbić otoczenia.
Moknięcie przyciemnia kolor i nie zmienia matowego charakteru zbroi.
Paleta materiału odnosi się do [wzoru użytkownika](reference-palette.png): ciepła, przygaszona stal,
stare złoto obrzeży i ciepłe cienie. Shader koryguje neutralne płyty oraz beżowe okucia osobno,
zachowując burgund i UV obecnej tekstury. Światło otoczenia jest ograniczone, a główne światło ma limit
jasności zapobiegający wypalaniu płyt do bieli w scenie areny. Nie dodano metalicznych refleksów.
Awatar Humanoid powstaje z nowego modelu; klipy i zestawy broni pochodzą z `visual_paladin`,
z wyjątkiem drugiego lekkiego cięcia opisanego poniżej.
Nowa definicja ma `scale=1`, `footLength=1`, `rigidBootToes=true`, `sagittalRunFeet=true`;
korekty długości starej stopy nie kopiowano.

## Poprawka anatomii biegu

Pierwsze wdrożenie miało sztywne buty, lecz wadliwe prowadzenie szkieletu. Automatyczny Humanoid pomijał palce stóp,
co wyłączało korekty stóp w sterowniku. Importer ma teraz jawne mapowanie 22 kości, w tym palców, barków i całego kręgosłupa.
Poza T została wyprostowana także w nogach, a zapamiętana poza szkieletu awatara jest aktualizowana z FBX.

Sam bieg Mixamo ma duży skręt lewej stopy i goleni. Jego cele IK odtwarzały wykręconą stopę pomimo zmiany krzywych mięśni.
Wspólne klipy pozostają oryginalne. `ClipAnimationDriver.AlignArmoredRunFeet` włącza korektę przez opcję definicji
**wyłącznie nowego paladyna**: wąski tor stóp przy osi ruchu (cel ±5,5 cm w biegu, ±8 cm w chodzie),
kontrolowana praca kolan do 3,6° względem płaszczyzny kroku i kierunek buta do 4°, stałe długości segmentów,
brak patologicznego skrętu kostki. Poprzednie ustawienie stóp dokładnie pod biodrami okazało się nadal za szerokie.
W podporze podeszwa jest płaska, a w wymachu but podąża za golenią z ograniczonym zgięciem kostki.
Kadencja i wykrok pochodzą z dotychczasowej animacji; cele są ograniczone zasięgiem nóg.
Korekta wygasa przy akcjach całego ciała; obrót bioder prowadzi bieg na skos. Osobne klipy cofania/biegu bokiem bez tej korekty.
Domyślna wartość opcji jest wyłączona — dotychczasowy wojownik i pozostałe modele zachowują poprzednie działanie.

Podmieniono tylko `class_hu_paladin.lookOverride`. Broń, tarcza, cechy, skille i parametry klasy pozostają obecne.
Oryginalny `class_hu_paladin.visual` wskazuje nadal stary model; wyczyszczenie `lookOverride` przywraca ten wygląd.
Importer HU zapisujący `visual` nie usuwa tego wyboru. Pełne odtworzenie definicji klasy może go nadpisać.

## Seria ataków i korbacz

Nowy paladyn ma własny `anim_paladin_anime_onehand.asset`. Drugi cios korzysta z lokalnej kopii
`sword and shield slash (3)`, klatki **18–24**, zamiast klipu z głębokim przysiadem.
Końcówka jest ucięta przed nadmiernym uniesieniem prawej dłoni. Po końcu wycinka warstwa akcji
płynnie wygasa do gardy, również gdy logiczne okno akcji jeszcze trwa. Obrażenia, koszty i czasy walki bez zmian.
Rzeczywiste czasy zapowiedzi i trafienia pozwalają odtwarzać krótkie cięcie w tempie **1,00×** także z wolniejszym korbaczem.
`preparationBlendShare=1` rozkłada przejście z pierwszego ciosu na zapowiedź drugiego. Poprzednik przez chwilę
kontynuuje ruch, stopniowo zwalniając; początek wycinka łagodnie nabiera prędkości. Usuwa to szybki skręt do
zamrożonej pozy przygotowania. Opcja dotyczy tylko tego cięcia. W trzech próbach granicy ciosów ruch dłoni
zmalał z 28,19 do 10,07 cm/kl., a obrót barków z 28,42 do 9,17°/kl. przy 60 kl./s.
Klipy źródłowe Mixamo, pozostałe ciosy i zestaw dwuręczny pozostają dotychczasowe.

`HellFlailChain` przenosi zachowanie korbacza z Hell Unleashed: 14 ogniw, symulacja Verlet w świecie,
grawitacja i bezwładność ciężkiej kuli. Symulacja działa po końcowej pozie dłoni, obraca istniejące kości broni
i aktualizuje końcówkę oręża. Kula zwisa, pozostaje w ruchu podczas zatrzymania animacji trafieniem i wygasa po zamachu.
Pauza zatrzymuje ją razem z grą. Próby obejmują oba korbacze, deformację siatki, długość ogniw, długą klatkę i teleport.
Port nie zapisuje niczego w Hell Unleashed ani w źródłowych modelach broni.

## Sprawdzenie w Unity

- Unity 6000.6.3f1, URP `TurrisURP`, Linear, Windows64. Praca przez MCP w istniejącym edytorze.
- Pełne EditMode: **196 zaliczonych, 0 błędów, 1 pominięty** (warunkowy eksport dźwięków).
- Celowane PlayMode: **15/15** — dziewięć prób paladyna, dwie korbacza i cztery regresje dotychczasowych modeli.
- Anatomia biegu 5,2 i 8,06 m/s, jedno- i dwuręczny zestaw: po 360 klatek pomiarowych, przednie i boczne ujęcia cyklu.
  Tor stopy podporowej maks. 6,28/6,16 cm od osi ruchu (przed poprawką 12,86/10,27 cm),
  kolano maks. 2,29/2,15 cm od płaszczyzny kroku, bez przeprostu; kierunek buta do 4°.
  Długości segmentów stałe (błąd numeryczny <0,01 mm). Dane: `run-feet.tsv`, `run-twohand.tsv` w katalogu wyników.
- Widoczna praca siatki brzucha względem klatki: 9,23° / 7,46°; pomiar obejmuje deformowaną siatkę, nie tylko obrót kości.
  Arkusze ośmiu faz: `run-feet-cycle.png`, `run-feet-cycle-side.png` oraz odpowiedniki `run-twohand-cycle*`.
- Bieg w prawdziwej scenie `Main`, wirtualny pad: przesunięcie stopy podporowej na klatkę p95 **2,01 cm** przy 60 kl./s.
- Próba materiału na oświetlonej powierzchni: obrót kamery nie zmienia koloru (różnica 0), także po zamoczeniu;
  ta sama próba wykrywa refleks PBR (0,06275). Materiał odbiera światło punktowe Forward+.
  Shader kompiluje się bez błędów w Direct3D12; przebiegi cienia, głębokości i normalnych są obecne.
- Galeria: spoczynek, bieg 5,2 m/s, blok, cięcie, mocny cios, unik, poza czaru i śmierci, tył, kamera pierwszej areny.
  Zrzuty w [TestResults/paladin-anime](../../../TestResults/paladin-anime/) obejrzano.
- Odchylenie próbek stalowego buta względem kości: około **0,001 mm**. Chwyt i gniazdo pozostają przy dłoni w całej próbie.
  Dane: [grip.tsv](../../../TestResults/paladin-anime/grip.tsv).
- Porównanie sum SHA256: **16 plików wojownika identycznych bajtowo**; klasa, definicja, FBX i towarzyszące pliki.
  Zapis w `TestResults/paladin-anime/warrior-before.json` i `warrior-after.json`.
- Aktualne wyniki MCP: `TestResults/paladin-anime/transition-palette-editmode.json` i `transition-palette-playmode.json`.
  Poprzednia wersja: `final-*.json`, pomiary szerokiego toru w `before-fluid-run/`.
  Nie uruchamiano pełnego PlayMode ani buildu gracza.

Pułapka galerii: po ręcznej zmianie pozy trzeba poczekać na aktualizację skinningu przed `Camera.Render`.
Wcześniejszy zrzut pokazywał siatkę z poprzedniego stanu i broń w nowym stanie. Poprawiono sekwencję testu;
kalibracja gniazd oraz modele broni pozostały poprawne.

Do dalszej oceny w graniu: zachodzenie naramienników i tabardu przy skrajnych pozach oraz kolor stali w oświetleniu areny.
Opcjonalny obrys anime, nowy oręż, arena i przeciwnicy należą do dalszych etapów PoC.
