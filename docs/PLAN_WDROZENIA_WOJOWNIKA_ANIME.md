# Gotyckie anime — wdrożenie modelu jako paladyna

Aktualizacja: 2026-10-26. Branch: `codex/paladin-gothic-anime`.
Pierwotny plan z 23 października dotyczył wojownika. Aktualne zlecenie użytkownika zastępuje ten wybór:
**nowy model trafia w miejsce paladyna; wojownik pozostaje bez zmian.**

**Stan: model jest wdrożony i sprawdzony w edytorze Unity.**
[Pełny raport i pliki](../ArtSource/GothicAnimePoC/Paladin/README.md).

## Wykonane

- [x] Utworzyć branch z aktualnego `main`, zachowując istniejące pliki i prace.
- [x] Użyć lokalnego `warrior-v2-grip.blend` ze zwiniętymi palcami. Nie zlecać dalszych operacji Meshy.
- [x] Skopiować istniejący szkielet wojownika z plików Turris, dopasować stawy do nowej siatki.
  Źródłowy FBX, stary wygląd wojownika i Hell Unleashed pozostawić bez zmian.
- [x] Nadać wagi całej siatce, zachować sztywność dłoni i stalowych butów, wypiec pozę T.
  Metoda: jawne regiony anatomiczne i płynne przejścia przy stawach; bone heat nie znalazł rozwiązania na siatce generatora.
- [x] Wyeksportować nowy FBX do `Models/PoC/PaladinAnime/`, zachowując plik roboczy w `ArtSource/GothicAnimePoC/Paladin/`.
- [x] Utworzyć i sprawdzić własny awatar Humanoid: 26 kości, gniazda `HandSlot.R/L`, brak nieważonych wierzchołków.
- [x] Przygotować mapę koloru 2K i materiał URP zgodny z obecną obsługą powierzchni i sygnałów trafienia.
- [x] Zastąpić materiał PBR osobnym `Turris/AnimeMatte`: matowy pancerz, pasma światła rozproszonego, cienie i światła Forward+.
- [x] Zweryfikować anatomię całego cyklu biegu/sprintu: pionowa poza wiązania nóg, pełne mapowanie Humanoid,
  kolana w płaszczyźnie kroku, węższe prowadzenie stóp i poprawne kostki.
- [x] Poprawić naturalność po ocenie użytkownika: tor biegu przy osi ruchu (podpora maks. 6,28 cm od środka),
  swobodniejsza praca kolan i butów oraz wagi brzucha/klatki na wszystkich trzech kościach kręgosłupa.
  Widoczna praca siatki brzucha względem klatki 9,23° / 7,46° w obu zestawach.
- [x] Utworzyć osobny `visual_paladin_anime.asset`, kopiując przypisania klipów i zestawów broni z obecnego paladyna.
  Ustawić `scale=1`, `footLength=1`, `rigidBootToes=true`; nie przenosić korekty długości stóp starego wojownika.
- [x] Przypisać wyłącznie `class_hu_paladin.lookOverride`. Zachować `class_hu_paladin.visual` jako poprzedni wygląd.
  Nie zmieniać `class_knight`, broni, cech, skilli ani parametrów klas.
  Korekta anatomiczna w runtime jest opcjonalna i włączona wyłącznie dla nowego paladyna (`sagittalRunFeet`);
  dotychczasowe modele, w tym wojownik, korzystają z poprzedniego prowadzenia animacji.
- [x] Sprawdzić start prawdziwego podejścia paladynem, istniejące animacje, chwyt i sztywność butów.
- [x] Poprawić serię lekkich ciosów: osobny zestaw jednoręczny paladyna, drugi cios bez głębokiego przysiadu,
  wycinek 18–24 kończący się przed unoszeniem dłoni i płynny powrót do gardy. Zachować tempo cięcia także z korbaczem.
- [x] Przenieść ruch łańcucha i kuli korbacza z Hell Unleashed: grawitacja, bezwładność, deformacja siatki,
  zachowana długość ogniw; próby obu korbaczy, pauzy i teleportu. Hell Unleashed bez zmian.
- [x] Obejrzeć galerię Unity, w tym widok na pierwszej arenie z normalnej kamery gry.
- [x] Wygładzić przejście pierwszego ciosu w drugi: zachowany ruch wybrzmienia, wejście przez zapowiedź,
  płynny start krótkiego wycinka. Zweryfikować trzy momenty rozpoczęcia i rzeczywistą serię w scenie gry.
- [x] Przywrócić paletę koncepcji: przygaszona ciepła stal, złote obrzeża, cieplejsze cienie i ograniczenie wybielania
  w mocnym świetle areny, przy zachowanej matowości. Zachować istniejącą teksturę i UV.
- [x] Porównać sumy kontrolne plików wojownika i zaktualizować dokumentację.

## Wynik sprawdzenia

| Sprawdzenie | Wynik |
|---|---|
| Unity | 6000.6.3f1, URP TurrisURP, Linear, Windows64 |
| Siatka | 45 832 trójkąty, 39 456 wierzchołków po imporcie Unity i korekcie wag tułowia |
| Materiał ciała | Jeden materiał, kolor 2048×2048 |
| Pełne EditMode | 196 zaliczonych, 0 błędów, 1 pominięty eksport dźwięków |
| Celowane PlayMode | 15/15: start i bieg w grze, galeria, oba zestawy, materiał i paleta, serie i granica ciosów, tempo i łańcuch korbacza, regresje |
| Drugi lekki cios | Klatki 18–24, wczesny powrót do gardy, tempo 1,00× również z korbaczem |
| Korbacz | Swobodna kula na 14 ogniwach, grawitacja i bezwładność, deformacja istniejącej siatki |
| Bieg/sprint | 5,2 i 8,06 m/s, 360 klatek/zestaw; tor podparcia maks. 6,28 cm od osi, kierunek buta do 4° |
| Anatomia | Kontrolowana praca kolan do 2,29 cm od płaszczyzny kroku, bez przeprostu; długości ud i goleni stałe |
| Tułów | Wagi na Spine/Spine1/Spine2; siatka brzucha pracuje względem klatki 9,23° / 7,46° |
| Matowość | Zmiana widoku: różnica koloru 0; kontrola PBR 0,06275; moknięcie też matowe |
| Buty | Odchylenie próbek względem kości około 0,001 mm |
| Dłonie i gniazda | Stabilne względem kości, zmiany około 0,001 mm |
| Wojownik | 16 plików identycznych bajtowo, dotychczasowa definicja i model |
| Koszt Meshy tej integracji | 0 kredytów |

Dane i zrzuty: `TestResults/paladin-anime/`. Testy:
`PaladinAnimeTests` (EditMode), `PaladinAnimePlayTests` (PlayMode).
Galeria obejmuje spoczynek, bieg, blok, cięcia, unik, gest czaru i śmierci, tył oraz widok z kamery gry.
Cykl biegu sprawdzono dodatkowo z przodu i boku, dla broni jedno- i dwuręcznej.
Nie uruchamiano pełnego PlayMode ani buildu gracza; wyniki dotyczą edytora i podanych scenariuszy.

Przed zrzutem ręcznie próbkowanej animacji trzeba poczekać na aktualizację skinningu.
W przeciwnym razie `Camera.Render` może pokazać poprzednią siatkę i bieżące położenie broni.
Poprawiono test galerii; nie było potrzeby zmieniać gniazd ani współdzielonych modeli broni.

## Powtórzenie i przywrócenie wyglądu

[Skrypt rigowania](../tools/art/rig_anime_paladin.py) uruchamia Blender w tle i zapisuje wyłącznie pliki PoC.
[Importer Unity](../Assets/TurrisDamnatorum/Scripts/Editor/PaladinAnimeImport.cs):
**Turris → PoC → Włącz model anime dla paladyna**, albo `Turris.EditorTools.PaladinAnimeImport.Run()` przez MCP.
Nie używać `RunBatchForce` ani hurtowego nadpisywania klas.

Poprzedni wygląd paladyna przywraca wyczyszczenie pola `lookOverride` w `class_hu_paladin`.
Importer HU zapisujący `visual` nie kasuje wyboru PoC. Pełne odtworzenie klasy z wartości domyślnych może go nadpisać.
Commit/push wyłącznie na prośbę użytkownika.

## Dalsza ocena

- Ocenić paladyna podczas zwykłej gry, szczególnie naramienniki i tabard w skrajnych pozach oraz stal przy pochodniach.
- Dalsze ręczne wykończenie siatki oraz opcjonalny obrys anime wykonać po ocenie matowego materiału w grze.
- Pozostałe assety: [szerszy zakres PoC](POC_GOTYCKIE_ANIME.md). Ich generacji ani podmiany nie wykonywano w tej integracji.
- Wojownika pozostawić przy obecnym wyglądzie; nie wracać do pierwotnego pomysłu przypisania mu tego modelu.
