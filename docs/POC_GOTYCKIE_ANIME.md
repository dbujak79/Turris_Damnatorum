# Gotyckie anime — zakres PoC

Data: 2026-10-23. Projekt docelowy: `C:/GIT/Gry/Turris_Damnatorum`.
Referencja: [gotyckie anime](concepts/2026-10-23/01-gotyckie_anime.png).

## Cel i stan

Przygotować mały, spójny zestaw rzeczywistych assetów 3D i VFX do oceny w Unity 6000.6.3f1 / URP 17.6.
**Model PoC został wdrożony 2026-10-26 jako wygląd paladyna. Wojownik pozostaje bez zmian.**
Hell Unleashed pozostaje wyłącznie do odczytu. PoC powstanie osobno, bez nadpisania bieżącej treści Turris.

**Wykonanie:** [stan planu integracji](PLAN_WDROZENIA_WOJOWNIKA_ANIME.md),
[model rigowany i raport](../ArtSource/GothicAnimePoC/Paladin/README.md).
Model korzysta z dopasowanej kopii szkieletu wojownika i obecnych animacji paladyna; oryginały są zachowane.
Stały chwyt dłoni wybrany przez użytkownika, 1,8 m, 45 832 trójkąty, mapa 2K, własny awatar Humanoid.
Po korekcie anatomii biegu i matowego materiału: EditMode 191/0/1, celowane PlayMode 7/7, obejrzane zrzuty Unity.
Bieg/sprint 5,2/8,06 m/s: wąski tor stóp przy osi ruchu (podpora maks. 6,28 cm od środka),
kontrolowana praca kolan i butów, artykulacja tułowia dzięki wagom na trzech kościach kręgosłupa.
Materiał `Turris/AnimeMatte` bez refleksów.
Dodatkowy koszt Meshy: 0.

### Aktualne ograniczenie próby — tylko wojownik

Użytkownik podał 159 kredytów i zlecił najpierw **jednego wojownika do oceny**.
Chochlik i pozostałe assety pozostają w planie, bez uruchamiania kolejnych generacji.
Przygotowano [referencję wojownika](concepts/2026-10-23/meshy/warrior-reference-v1.png)
przez wbudowany image_gen (0 kredytów Meshy) oraz
[parametry jednego zadania](concepts/2026-10-23/meshy/warrior-job-v1.json).
Wariant: Meshy 7.1, standardowa geometria, tekstura 2K, bez płatnego rigowania i animacji.
Potwierdzony koszt **30 kredytów API**: saldo przed zadaniem 159, po pobraniu 129.
Wysłano jedno zadanie `01a16a0a-a6e5-77cc-81be-c97226baf934`, zakończone `SUCCEEDED`.
Nie utożsamiać salda API automatycznie z pulą serwisu WWW.
Nie ponawiać utworzenia zadania po niejednoznacznym błędzie lub utracie odpowiedzi.

Meshy MCP 0.6.5 zainstalowano lokalnie w `C:/Users/Dawid/.codex/mcp/meshy` i zarejestrowano
w konfiguracji klienta `C:/Users/Dawid/.codex/config.toml`; test startu potwierdził 28 narzędzi.
Użytkownik uzupełnił `MESHY_API_KEY` lokalnie. Połączenie i saldo API sprawdzone; klucza nie zapisano w repozytorium ani rozmowie.

### Wynik pierwszej próby

Pliki źródłowe: [ArtSource/GothicAnimePoC/Warrior](../ArtSource/GothicAnimePoC/Warrior/README.md).

- Oryginalne eksporty `warrior-v1.glb` i `warrior-v1.fbx`, tekstura koloru 2048×2048; jeden materiał i jedna siatka.
- GLB: **45 832 trójkąty**, 39 204 wierzchołki po podziale na potrzeby eksportu.
  FBX: 23 291 wielokątów, w tym 22 582 czworokąty; **45 889 trójkątów** po triangulacji.
  Parametr docelowy 20 000 nie jest twardym limitem wyniku. Dokładne dane: `audit.json` obok modelu.
- Brak szkieletu, wag i animacji. Ramiona rozłożone do późniejszego dopasowania do rigu; nie sprawdzono deformacji w ruchu.
- W osobnym procesie Blender 5.2.2 zaimportowano oba formaty, potwierdzono wczytanie tekstur i obejrzano rendery
  [przodu](../ArtSource/GothicAnimePoC/Warrior/warrior-v1-front.png),
  [ujęcia ¾](../ArtSource/GothicAnimePoC/Warrior/warrior-v1-three-quarter.png) oraz
  [tyłu](../ArtSource/GothicAnimePoC/Warrior/warrior-v1-back.png).
- `warrior-v1-preview.blend` zawiera spakowaną teksturę i oświetlenie studyjne. Tylko scena podglądu ma ustawione 1,8 m wysokości;
  oryginały GLB/FBX zachowano bez zmian. Skrypt odtworzenia: `tools/art/preview_meshy_warrior.py`.
- Ocena: czytelna sylwetka i zgodna paleta; dłonie, okolice szyi i łączenia płyt wymagają ręcznej kontroli/czyszczenia.
  Generator powtórzył klamrę pasa także z tyłu. Uproszczenie siatki i praca nad deformacją sztywnych płyt należą do integracji.
- Nie uruchamiano Unity ani testów gry: ta próba przygotowuje źródła i podgląd statyczny, bez zmiany assetów rozgrywki.
  Nie wykonano płatnego rigowania, animacji, kolejnych prób ani dodatkowego przetwarzania przez Meshy.

## Najpilniejsze assety

| Kolejność | Asset | Zakres i baza |
|---|---|---|
| 1 | Model bohatera | Gotowy PoC przypisany do paladyna jako `visual_paladin_anime`; `visual_hero_warrior` bez zmian |
| 2 | Miecz i tarcza | `hu_sword`, `hu_shield`; proste sylwetki, osobne modele, zgodne punkty chwytu i długość ostrza |
| 3 | Podstawowa walka | Smuga lekkiego/ciężkiego ciosu, trafienie, blok/parowanie, krótki pył uniku; prezentacja podpięta do istniejących zdarzeń |
| 4 | Skille startowe wojownika | `hu_cleave` (Rozpłatanie), `hu_wind_slash` (Cięcie wichru), `hu_warcry` (Okrzyk bojowy); bez zmian kosztów, zasięgów i obrażeń |
| 5 | Startowa arena | Dziedziniec pierwszego piętra „I. Wrota Otchłani”: posadzka i sigil, ściana, filar, łuk, baner, pochodnia i portal jako moduły |
| 6 | Startowi przeciwnicy | Chochlik: jeden model bazowy i cztery warianty materiałów/VFX — ogień, woda/mróz, powietrze/błyskawice, ziemia; zachować rig i zachowania |

Skille ustalono na podstawie zapisanej definicji `class_knight`: dwa skille do wyboru na start i osobny skill klasy.
Pierwsze piętro korzysta z chochlika i jego wariantów; kultysta, robak i boss należą do późniejszych pięter.

## Podział narzędzi i kosztów

- Meshy: aktualnie **jedna generacja testowa wojownika**. Bazowy chochlik dopiero po ocenie wyniku i nowym zleceniu.
  Pojedynczy obiekt z rozłożonymi ramionami, bez otoczenia i wyposażenia. Jedna generacja wykonana za 30 kredytów.
- Blender: modelowanie miecza/tarczy i modułów areny, czyszczenie siatek, poprawne rozmiary, UV oraz dopasowanie do istniejącego szkieletu.
- Unity: wspólne materiały toon, selektywne kontury, światło, VFX i prefabrykaty PoC.
- ElevenLabs: opcjonalny późniejszy etap dźwiękowy. Nie jest potrzebne do oceny wizualnego PoC.

Generacja 3D dostarcza bazową siatkę; integracja obejmuje osobno kontrolę topologii, materiałów, rigu,
wag, deformacji i zgodności z istniejącymi animacjami. Nie uznawać surowego wyniku generatora za gotowy asset do gry.

## Warunki kontynuacji przez Meshy

Osobny MCP Meshy jest zainstalowany, a lokalny klucz użytkownika działa. Do tej próby użyto klienta stdio
z zainstalowanym oficjalnym serwerem; po ponownym wczytaniu konfiguracji klienta narzędzia mogą być dostępne bezpośrednio.
Połączenie MCP z żywym Blenderem było niedostępne; audyt i podgląd wykonano lokalnym Blenderem w osobnym procesie w tle.
Kluczy API nie wklejać do rozmowy ani repozytorium.
Limit jednej próby wojownika został wykorzystany; dalsze płatne generacje wymagają nowego zlecenia użytkownika.

## Odbiór PoC

- Osobny katalog assetów i scena demonstracyjna; zachowana działająca gra i pozostałe prace użytkownika.
- Wojownik: spoczynek, bieg, unik, blok, lekki i ciężki cios; bez deformacji stalowych części.
- Czytelne cztery odmiany chochlika i trzy skille przy normalnej kamerze rozgrywki.
- Porównawcze zrzuty z Unity, ujęcie modeli i sekwencje VFX; kontrola brakujących materiałów i błędów shaderów.
- Testy odpowiednie do zmian przez MCP w istniejącym edytorze; nie przerywać testów uruchomionych przez inne zadanie.
- Raportować oddzielnie przygotowane assety, integrację, rzeczywiście wykonane sprawdzenia i pozostałe ograniczenia.
