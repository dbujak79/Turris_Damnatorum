# Berserker i mag anime — wdrożenie modeli

2026-10-27. Nowe modele przypisane do istniejących klas, bez wymiany ich mechanik, broni i klipów.

| Klasa | Nowy wygląd | Baza szkieletu | Geometria źródłowa |
|---|---|---|---|
| Berserker `class_hu_berserker` | `visual_berserker_anime` | `hero_warrior_berserker.fbx`, 26 kości | 1 270 871 wierzchołków, 2 543 857 trójkątów |
| Mag `class_mage` | `visual_mage_anime` | `mage_witch.fbx`, 26 kości | 1 802 038 wierzchołków, 3 619 624 trójkąty |

Wojownik `class_knight`, paladyn i pozostałe specjalizacje nie zostały podmienione w tej pracy.
Przypisania zrealizowano wyłącznie przez `lookOverride` i Unity API. Oryginalne definicje wyglądu,
modele oraz źródłowe FBX/Blender z Meshy pozostają zachowane. Hell Unleashed bez zmian.

## Rig i chwyt

Od 2026-10-28 nowy mag ma [osobną korektę postawy](../ArtSource/GothicAnimePoC/Mage/posture-v1/README.md):
łagodniejsze pochylenie i mniej obniżone biodra, przy zachowanych stopach oraz pozach czarów i ataków.
Flaga `balancedMageStance` jest włączona tylko dla nowego maga.

- Kopie nazw/hierarchii starych szkieletów, dopasowanie stawów i wypieczenie pozy A do T.
  Własne awatary Humanoid; obecne animacje i zestawy broni skopiowane referencjami do nowych definicji.
- Pełna geometria bez redukcji. Unity rozdziela dodatkowe wierzchołki na granicach UV/normalnych;
  liczba wierzchołków importu może być większa od liczby źródłowej, bez wzrostu liczby trójkątów.
- Maksymalnie cztery wpływy; sztywne chwyty i buty. Oba modele mają dopasowane wagi anatomiczne;
  długa szata jest ciągłym okryciem wiązanym do bioder, nie osobnymi nogawkami.
- Palce zwinięte w siatce, kciuki domykane; osobne gniazda w środku uchwytów. Broń jest nadal osobnym obiektem.
  Nowa opcja `anatomicalGripSlots` uwzględnia odległość od nadgarstka do środka dłoni oraz jej orientację
  przy IK drugiego chwytu. Domyślnie wyłączona; włączona tylko w tych dwóch nowych definicjach.
- Średnica istniejących uchwytów, grubość rękawic i skrajne pozy mogą wymagać dalszego ręcznego wykończenia
  w dużych zbliżeniach. Nie dodawano płatnego rigowania Meshy ani osobnych kości paliczków.

Stały wymóg dla przyszłych postaci zapisano w `AGENTS.md` oraz
[specyfikacji stylu 1.1](STYL_GRAFICZNY_GOTYCKIE_ANIME.md): palce obejmują broń, kciuk domyka chwyt,
kontrola podczas spoczynku, biegu i ciosów. Ręka wykonująca czar lub obsługująca cięciwę może mieć właściwą pozę zadaniową.

## Materiały i zachowane źródła

Pierwotne prywatne materiały `Turris/AnimeCreatureMatte` zachowano. Od 2026-10-28 oba modele korzystają
z [korekty v2](../ArtSource/GothicAnimePoC/StyleCorrections/README.md): osobny `Turris/AnimeHeroPainterly`,
selektywna ochrona detalu, maska matowej stali oraz granatowo-fioletowe warstwy maga. Oryginalne mapy
koloru 4K pozostają identyczne bajtowo. Brak połysku PBR i odbić środowiska; materiały innych postaci bez zmian.

- [Źródło berserkera](../ArtSource/GothicAnimePoC/Heroes/Berserker/meshy-v1/README.md)
  i [rig](../ArtSource/GothicAnimePoC/Heroes/Berserker/rigged/rig-report.json).
- [Źródło maga](../ArtSource/GothicAnimePoC/Mage/meshy-v1/README.md)
  i [rig](../ArtSource/GothicAnimePoC/Mage/rigged/rig-report.json).
- [ZIP koncepcji](../ArtSource/Archives/Turris_Damnatorum_concept_arty_bohaterow_2026-10-27_v1.zip) pozostaje oryginalną migawką,
  bez nadpisania. Nowsza reguła chwytu jest w bieżącej specyfikacji projektu.

## Odtworzenie i przywracanie

`tools/art/rig_anime_berserker.py` przygotowuje berserkera, a argument `--mage` przygotowuje maga.
Skrypt działa w Blenderze i zapisuje nowe kopie do `Models/PoC/BerserkerAnime` oraz `Models/PoC/MageAnime`.
`BerserkerAnimeImport.RunBothBatch` importuje oba modele przez Unity API. Nie uruchamia `TurrisSetup.RunBatchForce`.

Menu Unity:

- **Turris → PoC → Berserker anime → Przywróć oryginalny model**.
- **Turris → PoC → Mag anime → Przywróć oryginalny model**.

Odpowiadające polecenia „Włącz nowy model” ponownie włączają wyglądy anime.
Materiały v2 można osobno wyłączyć: **Turris → PoC → Korekta stylu bohaterów → Przywróć materiały sprzed korekty**.
Modele obecnie nie mają LOD ani pomiaru wydajności w buildzie gracza. Zachowana pełna geometria
jest ciężka; sprawdzenie wydajności i siatki zoptymalizowanej do gry to osobny etap.

## Weryfikacja

Testy `BerserkerAnimePlayTests`: rzeczywisty start z toporem i młotem, bieg, seria ciosów,
awatar, stare zestawy klipów, wagi, geometria w ośmiu pozach i błąd drugiego chwytu poniżej 2,5 cm.
Testy `MageAnimePlayTests`: prawdziwa scena i czar z grymuaru, różdżka i kostur, sześć póz,
stabilność siatki i chwytu. Galeria w `TestResults/berserker-anime` i `TestResults/mage-anime`.
Pomiar krawędzi porównuje dwie próbki `BakeMesh` w tych samych jednostkach FBX.

Wyniki końcowe i aktualny stan kontroli zapisano w [PROGRESS.md](PROGRESS.md).
Końcowo **PlayMode: 19 zaliczonych, 0 błędów, 5 pominiętych opcjonalnych galerii**
(własne galerie obu nowych modeli zapisano niezależnie). Wybrane testy EditMode: **35/35**.
Galerie `review-final.jpg` obejrzane. P99 lokalnego rozciągania krawędzi maga w sprawdzanych pozach
pozostaje poniżej 2,6× względem ustabilizowanej pozy idle; pojedyncze mocniej pracujące drobne krawędzie
wymagają dalszego ręcznego dopracowania zbliżeń. To kontrola deformacji, nie benchmark wydajności.
Praca bez dodatkowych operacji/kredytów Meshy, commita i pusha.
