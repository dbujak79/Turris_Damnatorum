# Prompt wdrożeniowy: cztery perki specjalizacyjne Turris Damnatorum

Dokument przygotowany 2026-11-01. Jest samodzielnym zleceniem dla agenta wdrażającego. Samo utworzenie tego pliku nie oznacza wdrożenia mechanik. Wszystkie nowe liczby to hipotezy startowe do sprawdzenia w grze.

## 1. Zadanie i granice zakresu

Wdróż kompletnie cztery kupowane za dusze perki: **Pojedynkowicz, Wykorzystaj wyłom, Różnorodna sfora, Gwałtowna manifestacja**. Mają otwierać różne sposoby gry i działać dla każdej klasy spełniającej warunki. Dostarcz mechanikę, konfigurację, rzeczywiste oferty sklepu, zapis/odczyt, polskie opisy, analizę buildu i dowody testowe.

Perki mają po jednym poziomie. Nie dodawaj nowego drzewka pasywnego, przycisku, waluty, slotu ani mechanizmu odblokowań za popiół. Nie zmieniaj cech klasowych w ogólne perki. Nie wdrażaj w tym zadaniu Strzału z przyłożenia, Krwawego pośpiechu, przenoszenia amuletu na sługi, Krwawych zaklęć ani pozostałych pomysłów z analizy PoE2.

Nie traktuj planowanego pakietu affiksów jako automatycznie ukończonej zależności. W aktualnym checkoutcie są już jego pliki i zmiany potoku obrażeń; sprawdź ich faktyczny stan i zintegruj się z nim. Nie cofaj równoległych zmian, nie zakładaj ponownie ograniczenia trafienia do dwóch żywiołów.

## 2. Przygotowanie i zasady projektu

1. Przeczytaj `docs/PROGRESS.md`, następnie `README.md`, aktualne `AGENTS.md`, ten prompt oraz raporty cech i unikatów. Użyj skilla `hack-and-slash-design`; do zmian wyglądu umiejętności dobierz `skill-visuals`, a do operowania Unity właściwe instrukcje dla faktycznego środowiska.
2. Sprawdź `git status` i bieżący kod przed zmianami. W repozytorium trwają inne prace, m.in. nad affiksami, unikatami i animacją. Zachowaj je. Nie używaj reset/checkout do czyszczenia cudzych zmian.
3. Projekt, UI, komentarze i dokumentacja są po polsku. Unity projektu: **6000.6.3f1**, Windows; sprawdź wersję projektu przed uruchomieniem.
4. Nie uruchamiaj `TurrisSetup.RunBatchForce`. Rejestracja nowej treści musi działać z istniejącym `GameConfig` i z `DefaultContent`, bez nadpisywania istniejących assetów.
5. Nie wykonuj commit/push. Nie wznawiaj wstrzymanych wielogodzinnych serii botów; ten prompt autoryzuje implementację i krótkie, ukierunkowane scenariusze weryfikacyjne.
6. Testy przez `tools/run_tests.py` wymagają zamkniętego edytora — poproś użytkownika o zamknięcie dopiero przed ich uruchomieniem. Jeśli dostępne MCP pozwala wykonać właściwe testy w otwartym edytorze, można użyć tej drogi i zapisać wyniki.
7. Nie zastępuj istniejących reguł własnymi pod pretekstem integracji. Konieczną poprawkę regresji oddziel w raporcie od nowych perków. Historia znanych błędów w PROGRESS nie zastępuje ustalenia obecnego wyniku bazowego.

## 3. Źródła prawdy i miejsca integracji

Ścieżki w tym dokumencie są względem `C:/GIT/Gry/Turris_Damnatorum`. Ich zawartość może zmienić się przed rozpoczęciem wdrożenia; poniżej opisano stan zastany, nie nakaz odtwarzania starszego kodu.

| Obszar | Sprawdź w repozytorium |
|---|---|
| Definicja perka i treść domyślna | `Assets/TurrisDamnatorum/Scripts/Runtime/Data/BoonDefinition.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Core/DefaultContent.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Core/DefaultContent.HellEconomy.cs` |
| Obecne 46 perków HU i ich uśpienie | `Assets/TurrisDamnatorum/Resources/HellCatalog/upgrades.json`, `Assets/TurrisDamnatorum/Scripts/Runtime/Run/PerkMechanics.cs` |
| Sklep, cena, zakup i analiza | `Assets/TurrisDamnatorum/Scripts/Runtime/Run/FloorShop.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Run/LoadoutAnalysis.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/UI/GameUI.Economy.cs` |
| Stan, agregacja i zapis | `Assets/TurrisDamnatorum/Scripts/Runtime/Run/RunState.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Run/RunSnapshot.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Meta/MetaService.cs` |
| Rodzaje broni, statystyki i akcje | `Assets/TurrisDamnatorum/Scripts/Runtime/Stats/WeaponFamilies.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Stats/BuildSnapshot.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Player/PlayerCombat.cs` |
| Rozliczanie trafień i postawy | `Assets/TurrisDamnatorum/Scripts/Runtime/Combat/HitSystem.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Combat/DamageResolver.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Enemies/EnemyBrain.cs` |
| Słudzy, chimery, Wcielenie i premie | `Assets/TurrisDamnatorum/Scripts/Runtime/Combat/Servant.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/ClassFeatures/Features.Summoner.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Player/PlayerCombat.Hero.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Player/PlayerCombat.Affixes.cs` |
| Strefy Turris i HU | `Assets/TurrisDamnatorum/Scripts/Runtime/Combat/DamageZone.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Seals/HuSkillCaster.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Seals/HuRuntimes.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Seals/HuWorld.cs` |
| Dane skilli i definicja magii | `Assets/TurrisDamnatorum/Resources/HellCatalog/skills.json`, `Assets/TurrisDamnatorum/Scripts/Runtime/Data/SpellDefinition.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Core/DefaultContent.HellSkills.cs` |
| Konfiguracja i pochodzenie obrażeń | `Assets/TurrisDamnatorum/Scripts/Runtime/Data/GameConfig.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Data/Enums.cs`, `Assets/TurrisDamnatorum/Scripts/Runtime/Data/CombatData.cs` |

Istotne ograniczenia aktualnego kodu:

- `FloorShop.Generate` filtruje perki przez niepuste `PerkRow`, a ta metoda rozpoznaje tylko prefiks `hu_perk_`. Samo dopisanie `BoonDefinition` nie daje działających ofert.
- `PerkMechanics.Active(BoonDefinition, RunState)` dla treści spoza HU zwraca obecnie aktywność bez sprawdzania nowych warunków. `RunState.AllModifiers/AllEffects`, sklep i porównanie wyposażenia potrzebują zgodnej kwalifikacji.
- `StartAttack` używa `Build.attackSpeed + MomentumBonus`, a wspólne `ScaleStartup/ScaleRecovery` obsługują również inne akcje. Globalne dodanie szybkości może niechcący wpłynąć na skille lub wycenę DPS broni dla technik.
- Zwykłe wyczerpanie postawy w `EnemyBrain.ReceiveHit` może kończyć się stanem `Staggered` i natychmiastowym uzupełnieniem postawy. Parowanie ma osobną ścieżkę `OnAttackParried`, a riposta `PerformRiposte → ReceiveRiposte` omija zwykłe `HitQuery.Apply`.
- Słudzy ustawiają `hit.attacker = master`, więc sam właściciel trafienia nie odróżnia własnego ataku gracza od ataku sługi. `Servant.Deliver` jest współdzielone także z Wcieleniem gracza.
- Premia obrażeń sług z affiksów jest migawką przy przyzwaniu; istnieją też dynamiczne premie `ServantDamageMult`. Różnorodna sfora ma być osobną, dynamiczną premią bez zapisywania jej w migawce.
- `DamageZone` tyka od czasu 0, ma minimalny interwał 0,1 s i obsługuje duży krok czasu pętlą. `HuSkillCaster.SpawnZone` liczy obrażenia tyknięcia z `share × tick`; zmiana samego `tick` przed tym obliczeniem może skasować zamierzone zwiększenie intensywności.
- `RunSnapshot.Index` musi znać natywne definicje także podczas odczytu. Rejestracja dopiero przy otwarciu sklepu jest za późna.

## 4. Wspólny kontrakt danych, dostępności i ekonomii

Dodaj mały, natywny katalog perków Turris, np. `NativePerkCatalog` z definicjami `BoonDefinition`, metadanymi oferty i czystymi regułami kwalifikacji. Możesz wybrać inną spójną nazwę. Nie podszywaj nowych perków pod HU i nie edytuj zewnętrznego repozytorium Hell Unleashed.

| Stałe id | Nazwa | Poziomy | Parametr działania | Cena bazowa przed istniejącym przeliczeniem HU | Waga | Od piętra |
|---|---|---:|---|---:|---:|---:|
| `perk.turris.duelist` | Pojedynkowicz | 1 | +15% szybkości kwalifikowanych ataków | 28 | 0,70 | 1 |
| `perk.turris.exploit_breach` | Wykorzystaj wyłom | 1 | +25% obrażeń jednego trafienia, okno 3 s | 30 | 0,80 | 3 |
| `perk.turris.diverse_pack` | Różnorodna sfora | 1 | +10% za inny aktywny rodzaj, maks. +20% | 32 | 0,65 | 1 |
| `perk.turris.violent_manifestation` | Gwałtowna manifestacja | 1 | czas i odstępy tyknięć ×0,5 | 30 | 0,65 | 1 |

„Od piętra” oznacza numer piętra oferty: `run.floorsCleared + 1`. Nie mieszaj tego z indeksem od zera. Wszystkie parametry działania, ceny, progi i wagi mają jedno edytowalne źródło, np. serializowane strojenie w `GameConfig` z bezpiecznymi domyślnymi wartościami dla istniejącego assetu. Liczby w UI i testach czytaj z tej samej konfiguracji.

- Zachowaj cztery oferty perków i dotychczasowe losowanie bez powtórzeń. Dodaj kwalifikujące się natywne perki do wspólnej puli z HU; nie gwarantuj ich w każdym sklepie i nie twórz dodatkowej darmowej oferty.
- Cena korzysta z tego samego `PriceScale` i inflacji co HU, dokładnie raz. Przy `PriceScale = 2,5`, inflacji 1 i pierwszym zakupie ceny wynoszą odpowiednio 70, 75, 80 i 75 dusz. Poziom maksymalny 1 wyklucza dalsze zakupy.
- Nowe perki są neutralne klasowo: brak kary wagi za obcą klasę. Warunki dotyczą buildu, nie nazwy profesji. Sklep ma odrzucać perk, którego gracz nie może wykorzystać w aktualnym wyposażeniu/skillach.
- Odróżnij **możliwość wykorzystania** od **chwilowej premii**. Sfora może pojawić się przy jednym działającym skillu przywołań, nawet gdy w sklepie nie ma sług; jej chwilowa premia może wynosić 0%. Wyłom jest gotowy do wykorzystania bez istniejącego znacznika na przeciwniku.
- Pojedynkowicz kwalifikuje się przy zgodnych rękach. Sfora przy co najmniej jednym obecnie wyposażonym, legalnym skillu przywołań, również z grymuaru. Manifestacja przy co najmniej jednym legalnym skillu mającym kwalifikowaną strefę na aktualnym poziomie i z aktualnymi pieczęciami. Wyłom przy legalnej ręcznej ofensywie z dodatnią postawą albo możliwości parowania prowadzącego do przełamania.
- Ponownie sprawdź kwalifikację i limit poziomu podczas zakupu, nie tylko przy generowaniu oferty. Nie pobieraj dusz za nieudany zakup; udany zakup jest atomowy. Nie zmieniaj istniejącej polityki starych ofert HU poza koniecznym wspólnym interfejsem.
- Rejestruj definicje idempotentnie, przed budową pul i indeksem zapisu, zarówno dla konfiguracji assetowej, jak i domyślnej. Bez duplikatów, globalnego stanu przeciekającego między konfiguracjami/testami i wielokrotnego dopisywania modyfikatorów.
- Nie dodawaj nowych perków automatycznie do talentów startowych, odblokowań profilu ani nagród/łupu. Jeśli nowy wpis enuma jest potrzebny, dopisz go na końcu i zachowaj stare wartości numeryczne.

## 5. Pojedynkowicz — dokładny kontrakt

**Decyzja:** rezygnuję z drugiej ręki na rzecz szybszego rytmu walki jednym orężem. Ceną jest utrata tarczy, grymuaru lub dodatkowej broni; perk nie daje osobnej premii obronnej.

Warunek: rzeczywista broń jednoręczna do walki wręcz w głównej ręce i pusty slot drugiej ręki. Nie wystarcza ukryty model przedmiotu. Nie kwalifikują się łuki, różdżki, kostury, pięści, broń dwuręczna ani zestawy dwóch ostrzy reprezentowane jednym przedmiotem. Uwzględnij jednoręczne buławy/topory/korbacze, jeśli istnieją — sam brak `BuildTag.HeavyWeapon` nie oznacza braku broni wręcz. Oprzyj filtr na rzeczywistych danych broni i udokumentowanych rodzinach, nie polskiej nazwie.

1. Bonus dotyczy ręcznych podstawowych lekkich i mocnych ataków tą bronią, w tym wariantów z biegu/po uniku. Nie dotyczy riposty, technik ze slotów, czarów, parowania, uniku, flaszek, czasu ładowania cechy ani Wcielenia.
2. Dodaj `0,15` do istniejącej sumy szybkości używanej przez kwalifikowany atak, przed dotychczasowymi ograniczeniami. Przykład: `Build.attackSpeed = 1,20`, `MomentumBonus = 0,10` → `1,45`. Nie mnożysz tej sumy przez `1,15`.
3. Stosuj otrzymane tempo w tym samym miejscu co dotychczasowe tempo ataku do windup/active/recovery/cancelAfter. Nie dodawaj bonusu drugi raz przez `StatType.AttackSpeed` lub wspólne skalowanie akcji.
4. Koszt wytrzymałości jednego ataku, obrażenia na trafienie, zasięg, postawa i odnowienia pozostają bez zmian. Więcej ciosów na sekundę oznacza też większe zużycie wytrzymałości na sekundę.
5. Przeliczaj warunek po zmianie wyposażenia i Arsenału. Wyrzucona/orbitująca broń drugiej ręki nadal zajmuje slot i nie uruchamia perka. Wymuszona transformacja ataku w pięści/czar nie dostaje premii tylko dlatego, że bazowy przedmiot jest mieczem.
6. Nie zmieniaj czasu już rozpoczętej akcji po zmianie buildu. Następna akcja bierze aktualny warunek. Synchronizacja animacji wynika z zegara akcji, bez globalnego `Time.timeScale`.
7. Analiza sklepu ma pokazywać wzrost częstotliwości kwalifikowanych ataków. Nie zwiększaj przez ten perk wzorcowego DPS broni używanego do obrażeń technik (`WeaponFamilies.SkillScale`).

## 6. Wykorzystaj wyłom — zdarzenie przełamania i jednorazowa nagroda

**Decyzja:** najpierw doprowadzam do rzeczywistego przełamania, potem wybieram trafienie wykorzystujące krótkie okno. Ceną jest przygotowanie, limit czasu i rezygnacja z innych wydatków za dusze; przeciwnik może odsunąć się albo wymusić obronę.

### Powstanie okna

- Twórz znacznik dla pary **właściciel perka + konkretny żywy przeciwnik**, gdy jego własne bezpośrednie trafienie ręcznym podstawowym atakiem, techniką lub ofensywnym czarem faktycznie przełamie postawę. Uwzględnij zwykłe przełamanie kończące się `Staggered`, nie tylko stan `PoiseBroken`.
- Uwzględnij też udane ręczne parowanie tego gracza, które rzeczywiście doprowadzi do przełamania postawy napastnika. Obecna sygnatura `OnAttackParried(float)` nie niesie obrońcy — dodaj minimalne, zgodne przekazanie pochodzenia albo jawne zdarzenie po rozstrzygnięciu parowania. Nie zgaduj właściciela po globalnym graczu.
- Samo wyzerowanie puli pod hiperzbroją bez faktycznego przełamania nie tworzy okna. Sam `HitOutcome.GuardBroken`, zwykły flinch, zamrożenie, powalenie lub trzy warstwy statusu zachwiania też go nie tworzą.
- Słudzy, ich komendy, tyknięcia stref/statusów, reakcje, odbite pociski, kopie i wtórne trafienia cech/unikatów nie tworzą okna. To, że ich `attacker` wskazuje gracza, nie wystarcza do kwalifikacji.
- Trafienie zabijające cel nie pozostawia znacznika. Zdarzenie emituj raz na rzeczywiste przełamanie, przed/po resecie postawy w jednoznacznie udokumentowanym miejscu. Nie wykrywaj go w kolejnej klatce przez sprawdzanie aktualnej wartości puli.

### Wykorzystanie i matematyka

1. Okno trwa 3 s zegara walki; pauza go nie zużywa. Zachowaj tożsamość źródłowej ręcznej akcji (`originId` lub równoważny identyfikator).
2. Pierwsze kolejne własne, bezpośrednie trafienie nowej ręcznej akcji w ten cel otrzymuje mnożnik `1,25` całego własnego budżetu obrażeń bezpośrednich. Może być podstawowym atakiem, techniką, ofensywnym czarem lub prawidłową ripostą. Późniejsze segmenty tej samej akcji, która otworzyła okno, nie wykorzystują premii.
3. Mnożnik stosuj raz do części fizycznej, nieprzypisanej magii i wszystkich żywiołów, przed obroną celu. Nie mnożysz postawy, kosztu gardy, siły statusów ani osobno obrażeń reakcji/efektów wtórnych. Zachowaj dotychczasową zasadę wyprowadzania statusów z trafienia, bez dodatkowego mnożnika statusu z perka.
4. Przykład przed obroną: 80 fizycznych + 20 ognia → 100 fizycznych + 25 ognia. Nadal jest to jedno trafienie. Przy liniowej redukcji 20% daje 100 obrażeń życia zamiast 80; inne warstwy obrony zachowują własne reguły.
5. Zużyj okno dopiero po trafieniu z dodatnimi obrażeniami życia. Pudło, parowanie, pełny blok i `Ignored` go nie zużywają. Próba przyznania premii i potwierdzenie jej zużycia muszą być odporne na zagnieżdżone reakcje i kilka kolizji celu.
6. Znacznik jest per cel. Jedno AoE może wykorzystać po jednym znaczniku na kilku wcześniej przygotowanych celach, ale ten sam cel nie dostaje kilku premii przez dodatkowe hitboxy, cień/alias lub pociski tej samej akcji.
7. Nie dodawaj obrażeń jako osobnego trafienia po fakcie: ominęłoby to obronę, powielało zdarzenia i komplikowało zabójstwa. Nie zapisuj globalnego wzmocnienia na pocisku; kwalifikacja zależy od trafionego celu i aktualnego okna.
8. Integruj zwykłą ripostę w jej rzeczywistej ścieżce. Przy unikatowej zamianie riposty na czar/falę premia może dotyczyć jednego zastępującego skutku głównego w oznaczony cel, nigdy dodatkowej fizycznej riposty obok niego. Zachowaj normalną manę, gotowość i fallback bez zasobów. Udokumentuj klasyfikację tych zastępstw i sprawdź ją testem.
9. Nowe przełamanie zastępuje okno na danym celu, bez kumulowania ładunków. Śmierć celu/gracza, usunięcie perka, koniec piętra i restart czyszczą stan. Znaczniki są stanem krótkiej walki, nie nowym trwałym zasobem runu.

Nie wydłużaj CC, nie zmieniaj regeneracji postawy i nie rób z bossa celu stale podatnego na ripostę. Sprawdź połączenie z affiksem postawy ciężkiej broni, Krwawą ripostą i istniejącymi mocami ripost.

## 7. Różnorodna sfora — premia dynamiczna, migawki pozostają migawkami

**Decyzja:** utrzymuję różne role sług kosztem slotów skilli, many i inwestycji w jedną specjalizację. Premia nie zwiększa liczby sług, ich czasu życia, zdrowia, obrony, zasięgu ani szybkości.

- Dla danego właściciela policz zbiór aktywnych rodzajów: chochlik, ogar, golem. Zwykły sługa wnosi swój rodzaj; chimera wnosi rodzaje aktualnie działających wkładów. Powtarzające się wkłady nie powiększają zbioru. Nie licz sług innego właściciela, pustych/usuniętych wkładów ani Wcielenia gracza.
- Śmiertelna chimera wnosi działające wkłady zgodnie z jej regułami życia; nie usuwaj ich tylko dlatego, że zwykły licznik czasu jest zamrożony. Zachowaj dotychczasowe reguły śmiertelności i limit sześciu miejsc.
- `K = liczba różnych rodzajów`, `bonus = 0,10 × max(0, K − 1)`. Dla K=0/1 bonus=0, dla K=2 bonus=10%, dla K=3 bonus=20%. Mnożnik `1 + bonus` jest jedną osobną warstwą, stosowaną dokładnie raz.
- Odczytuj K przy wykonaniu ataku sługi i zachowaj ten wynik w budżecie wyemitowanego ataku. Pocisk chochlika w locie nie zmienia obrażeń po zniknięciu golema. Następny atak uwzględnia już nowy skład.
- Przykład: migawka ataku 100, affiks sług +30%, aktualna sfora trzech rodzajów → `100 × 1,30 × 1,20 = 156` przed pozostałymi istniejącymi warstwami. Po zniknięciu jednego rodzaju następny atak: `100 × 1,30 × 1,10 = 143`. Zmiana sprzętu nie przelicza wstecz migawki affiksu.
- Zastosuj warstwę do zwykłych ataków, komend i ataków każdego wkładu chimery. Nie wkładaj jej do `makeHit`, `baseMakeHit`, `SnapshotHit`, `ToParts`, składanego budżetu chimery ani stałej statystyki, która przetrwa utratę rodzaju.
- Nie skaluj drugi raz przy scaleniu, rozdzieleniu, przywołaniu następcy, komendzie lub emisji pocisku. `Servant.Deliver` obsługuje też Wcielenie — ogólna poprawka w tej metodzie nie może przyznać premii graczowi w formie.
- Podstawowe trafienie sługi może przekazać swój normalny budżet statusom według istniejących reguł; nie dodawaj osobnego mnożnika do tyknięć statusów ani wtórnych mocy unikatów. Przechowana nova kokonu nie dostaje premii sfory tylko dlatego, że uwalnia ją sługa.
- Dynamiczną premię uwzględnij po warstwach migawki i przed rozdzieleniem budżetu ataku na trafiane cele. Kopiuj dane trafienia bez współdzielonej mutacji tablic/składników; respektuj aktualne `MagicSplit` i pomocnicze skalowanie obrażeń.
- Ogranicz zbędne alokacje: dla trzech rodzajów wystarczy maska bitowa. Aktualizacja przy zmianie składu lub odczyt małej listy są dopuszczalne, ale poprawność właściciela i cyklu życia jest obowiązkowa.

## 8. Gwałtowna manifestacja — kompresja czasu z zachowaniem zdarzeń

**Decyzja:** oddaję długie zabezpieczenie miejsca za szybsze wykorzystanie strefy w krótkim oknie. Mana, odnowienie, promień, obrażenia wejściowej eksplozji i liczba stref z jednego rzutu pozostają takie same.

### Kwalifikacja

Kwalifikują się naziemne, okresowo zadające obrażenia strefy ofensywnych czarów ręcznie inicjowanych przez gracza. Sprawdzaj rzeczywistą semantykę czaru (`IsSpell` i dane dostarczenia), po uwzględnieniu poziomu/pieczęci. Sam `SpellKind.Zone` ani obecność komponentu `Zone` nie wystarcza: tą ścieżką przechodzą też techniki, Sidła i Deszcz strzał.

W audycie obowiązkowo rozstrzygnij i opisz w raporcie:

| Źródło | Decyzja bazowa |
|---|---|
| `spell_burningground` — Płonąca ziemia Turris | kwalifikuje się |
| `hu_ember_ring` — Krąg żaru HU | kwalifikuje się |
| strefa novy ognia HU z poziomu III | kwalifikuje się wyłącznie strefa, bez przyspieszania fali novy |
| strefa Meteoru | kwalifikuje się, jeśli wykonywany jest legalny ręczny czar; opóźnienie i obrażenia uderzenia bez zmian |
| `hu_arrow_rain`, `hu_snare`, `hu_thorn_scatter`, `hu_slag_spray` | nie kwalifikują się jako techniki broni |
| Nawałnica / Strzała żywiołów w starych definicjach sygnatur | nie przywracaj ich do puli; jeśli dostępne w legalnym trybie jako ręczny czar, zastosuj tę samą regułę do właściwej strefy |
| aury, wiry podążające za bohaterem, promienie, kanały, ściany, miny, totemy, pułapki, przywołania | poza zakresem |
| pozostałości podstawowych pocisków, wtórne efekty cech i unikatów, czary wyzwolone automatycznie | poza zakresem |

Poziom lub pieczęć dodająca kwalifikowaną strefę do ręcznego czaru ma uruchomić perk. Kopia takiego czaru dziedziczy już przekształcony profil strefy dokładnie raz; nie staje się dodatkowym ręcznym rzutem. Jeśli cecha/unikat magazynuje lub zastępuje strefę, zachowaj pochodzenie i zużyj ten sam budżet, bez tworzenia dwóch pełnych efektów. Prąd głębin może przesunąć kwalifikowaną strefę bez ponownego przyspieszania i odświeżania jej czasu.

### Matematyka i zegar

**W tym pakiecie kompresuj harmonogram, zamiast podwajać obrażenia pojedynczego tyknięcia.** To doprecyzowanie poprzedniej propozycji „dwa razy większe obrażenia na sekundę”: zachowuje liczbę trafień, ich jednostkową siłę i budżet przy nieparzystej liczbie tyknięć.

- Niech bazowy harmonogram po dotychczasowych bonusach trwa D sekund, z tyknięciami w chwilach `t_i` i budżetami `H_i`. Nowy harmonogram: czas `D/2`, tyknięcia `t_i/2`, identyczne `H_i`. Każde bazowe tyknięcie ma dokładnie jeden odpowiednik.
- Dla interwału stałego: `interval' = interval/2`, `duration' = duration/2`, `damagePerTick' = damagePerTick`. Nie mnożysz jednocześnie obrażeń tyknięcia przez 2. Przykład: 8 trafień po 10 w 4 s przy interwale 0,5 s → 8 trafień po 10 w 2 s przy interwale 0,25 s; nominalnie 20 → 40 obrażeń/s i 80 → 80 łącznych obrażeń.
- Zachowaj semantykę trafienia początkowego i wykluczenia końca przedziału z istniejącego harmonogramu. Przykład D=2,5 s, interval=0,35 s: chwile 0…2,45 dają 8 trafień; po kompresji 0…1,225 w czasie 1,25 s też dają 8, nie 7 ani 9.
- W HU najpierw ustal bazowe `H_i` z dotychczasowego `share × bazowy tick`, następnie kompresuj czas. Nie przeliczaj `H_i` na nowo z połowy interwału.
- Obecne minimum 0,1 s nie może obciąć kwalifikowanego wyniku 0,05 s ani zgubić połowy zdarzeń. Możesz zastosować lokalny zegar strefy ×2 albo osobny, jawnie oznaczony przekształcony harmonogram. Nie obniżaj globalnie minimum wszystkim strefom i nie zmieniaj globalnej skali czasu.
- `Step(dt)` musi dawać ten sam wynik dla małych i dużych kroków oraz na granicach zakończenia. Nie uzależniaj liczby trafień od FPS. Odróżniaj czas lokalny od sekund świata w publicznych `Remaining`, `Interval`, `RemainingBudget`, przesuwaniu, telemetrii i opisach.
- Stan perka/profil strefy pobieraj raz przy ręcznym rzucie i przekaż do opóźnionych skutków. Nie mutuj współdzielonego `SpellDefinition`, danych HU ani profilu innych graczy. Przesunięcie, klonowanie lub odtworzenie przechowanego efektu nie kompresuje ponownie już przekształconego profilu.
- Czas VFX/SFX, opady wizualne i zwolnienie miejsca w `HuWorld.LiveZones` muszą odpowiadać rzeczywistemu skróconemu istnieniu strefy. Sprzątanie po wcześniejszym zniszczeniu/śmierci właściciela nie może zwolnić licznika drugi raz.
- Zachowaj siłę pojedynczej aplikacji statusu i łączną liczbę aplikacji w pełnej strefie. Nie skracaj czasu statusów ani reakcji. Szybsze nawarstwienie może zmienić wynik po statusach/obronie — to skutek do pomiaru, nie dowód zachowania końcowego DPS w każdym buildzie.
- W szczególności zachowanie 80 surowych obrażeń nie gwarantuje 80 obrażeń po regeneracji, statusach i reakcjach. Raportuj osobno surowy budżet, obrażenia życia i obrażenia efektów wtórnych.

## 9. Zapis, UI i informacja dla gracza

### Trwałość

- Zapisuj posiadanie czterech perków przez stabilne id i liczbę poziomów w istniejących `boons`. Rejestr ma być gotowy przed `RunSnapshot.Index/Restore`, także bez wcześniejszego sklepu w tej sesji.
- Stary zapis bez nowych perków ładuje się bez zmiany statystyk, wyposażenia, skilli, pieczęci, długu many, cechy klasowej i drugiego zestawu Arsenału. Nie resetuj profilu, nie losuj niczego ponownie i nie przemianowuj istniejących id.
- Uśpiony perk pozostaje kupiony. Wczytanie nie przyznaje go drugi raz i nie naprawia warunków przez przymusowe zdejmowanie wyposażenia.
- Krótkotrwałe okna Wyłomu znikają przy końcu walki/piętra; nie dodawaj nowego zapisu aktywnej symulacji, jeśli gra dotąd nie zapisuje sług i stref w trakcie walki. Jeśli taki zapis już istnieje w aktualnej gałęzi, respektuj go i zachowaj znacznik jednokrotnej kompresji.
- Brakujące pola nowej konfiguracji dostają poprawne wartości domyślne. Waliduj parametry: brak NaN/Infinity, dodatni czas okna i kompresja; zachowaj stare enumy.

### UI

- Karta sklepu: nazwa, cena, poziom 1/1, dokładna korzyść, warunek i koszt alternatywny. Osobno powód uśpienia lub brak chwilowej premii. Opis musi wynikać z parametrów używanych w walce.
- Pojedynkowicz: „+15% szybkości podstawowych ataków wręcz. Wymaga jednoręcznej broni i pustej drugiej ręki.” Ostrzegaj przy zakładaniu przedmiotu drugiej ręki i porównuj faktyczną częstotliwość ataków.
- Wyłom: „Po przełamaniu postawy następne trafienie nową ręczną akcją w ten cel zadaje +25% obrażeń. Okno: 3 s.” Małe oznaczenie na właściwym celu i czytelne wygaśnięcie; nie zasłaniaj zapowiedzi ataków.
- Sfora: pokaż aktywne rodzaje oraz aktualną premię 0/10/20%. Jednorodna sfora nie jest błędem ani utratą zakupu. Nie pokazuj premii za samą liczbę modeli.
- Manifestacja: pokazuj efektywny czas i intensywność kwalifikowanej strefy, z informacją o zachowanym budżecie jej trafień. Nie pokazuj +100% do całych obrażeń Meteoru albo czasu działania wszystkich skilli.
- Analiza przed/po zmianie wyposażenia ma używać tego samego kwalifikatora co runtime, bez chwilowego zakładania przedmiotu na żywej postaci i bez tworzenia sług. `PerkUseless` musi rozpoznawać nowe zasady; planowana możliwość zastosowania nie oznacza gwarantowanego uptime.
- Zachowaj nawigację padem, fokus i transakcję zakupu. Sprawdź 720p/1080p oraz pomoc HUD. Nie przebudowuj całego interfejsu.
- Oznaczenia stanu mają być oszczędne i zgodne ze stylem projektu. Jeśli dodasz ikony, uwzględnij licencję/pochodzenie i rzeczywiste wyświetlanie. Nowy model/animacja broni nie jest potrzebny do tego zakresu.

## 10. Minimalna architektura i kolejność wdrożenia

1. Ustal wynik bazowy, audytuj ścieżki czterech mechanik i kwalifikację stref. Sporządź w raporcie krótką listę miejsc faktycznie zmienianych.
2. Dodaj katalog/strojenie i rejestrację. Rozszerz wspólną obsługę ofert, aktywności, cen i indeksu zapisu. Zachowaj 46 perków HU oraz istniejące zwykłe wzmocnienia i talenty; bez globalnego odblokowania ich wszystkich w sklepie.
3. Dodaj czyste funkcje matematyki/kwalifikacji oraz mały kontroler stanu nowych perków na właściciela, jeśli potrzebny. Nie twórz czterech rozproszonych wyjątków zależnych od nazw UI.
4. Wdróż Pojedynkowicza i Sforę w miejscach wykonania ich akcji. Potem jawne zdarzenie rzeczywistego przełamania i zastosowanie Wyłomu w zwykłych trafieniach oraz ripostach.
5. Wdróż profil kompresji czasu w kwalifikowanych strefach i jego dziedziczenie. Nie stosuj modyfikacji globalnie w każdym `DamageZone.Spawn`, bo obejmuje też wrogów, techniki i efekty wtórne.
6. Dodaj UI, analizę i telemetrię. Zliczaj m.in. ataki kwalifikowane, utworzone/zużyte/wygasłe okna, premię sfory w chwili emisji ataku, bazową/efektywną długość i liczbę tyknięć strefy. Nie loguj każdego trafienia bez potrzeby w normalnej grze.
7. Uruchom testy z rozdziału 11, krótkie porównania z rozdziału 12 i obejrzyj zrzuty. Dopiero po weryfikacji aktualizuj dokumentację jako „wdrożone”.

Nowe typy/pliki muszą mieć potrzebne Unity `.meta`. Nie rozbudowuj ogólnego frameworka proców lub zapisów bardziej, niż wymaga spójne pochodzenie tych czterech mechanik.

## 11. Testy obowiązkowe

To scenariusze zachowania, a nie wymóg dokładnie tylu metod testowych. Możesz łączyć przypadki w parametryzowane testy. Nie zastępuj testów prawdziwych ścieżek wyłącznie testem funkcji `×1,25`.

### EditMode

| Id | Scenariusz i oczekiwany dowód |
|---|---|
| E01 | Katalog: cztery unikalne stabilne id, jeden poziom, poprawne domyślne parametry, idempotentna rejestracja dla dwóch niezależnych konfiguracji. |
| E02 | Pula sklepu: nowe i HU współistnieją; cztery oferty bez duplikatów, stały seed daje ten sam wynik, kupione 1/1 znikają, inne zwykłe talenty nie wpadają niechcący do puli. |
| E03 | Cena/progi: 70/75/80/75 bez inflacji, inflacja raz, Wyłom od piętra 3, brak kary obcej klasy; nieudana transakcja nie pobiera dusz. |
| E04 | Pojedynkowicz: macierz legalnych jednoręcznych broni kontra dwuręczne, łuki, fokusy, pięści, pary ostrzy; każda zajęta druga ręka wyłącza bonus. |
| E05 | Tempo: 1,20 + 0,10 + 0,15 = 1,45; brak podwójnego naliczenia, wzrostu obrażeń technik i wpływu na inne akcje. |
| E06 | Wyłom: faktyczne przełamanie przy zwykłym trafieniu i parowaniu emituje jeden znacznik właściwego właściciela; hiperzbroja, sam GuardBroken i statusy go nie udają. |
| E07 | Wyłom: nowa akcja, właściwy cel, granica 3 s, brak zużycia przez akcję otwierającą, pudło/blok/parowanie; wielokrotne hitboxy i alias celu nie powielają premii. |
| E08 | Wyłom: wszystkie składniki obrażeń ×1,25 raz, bez premii postawy/statusu; rozdzielone cele AoE, wtórne reakcje i oznaczenie sług zachowują kontrakt. |
| E09 | Sfora: K=0/1/2/3 daje 0/0/10/20%; duplikaty i obcy właściciel bez wpływu, aktywne wkłady chimery liczą się poprawnie. |
| E10 | Sfora: przykłady 156/143, migawka affiksu bez zmian, skopiowany/przetrzymany budżet nie zawiera powielonej premii; Wcielenie jej nie dostaje. |
| E11 | Strefy: jawna tabela kwalifikacji, poziom III novy i pieczęć dodająca strefę; techniki/aura/słudzy/wrogowie wykluczeni. |
| E12 | Harmonogram: 4/0,5 → 2/0,25 i 8 tych samych trafień; 2,5/0,35 → 1,25/0,175 i 8 trafień; minimalny interwał, małe/duże kroki i granice czasu. |
| E13 | HU: `share × bazowy tick` zachowane; skopiowanie/przesunięcie/opóźniona emisja nie kompresują drugi raz; brak mutacji danych innego rzutu. |
| E14 | Zapis: stary snapshot, zapis czterech perków i ponowny odczyt przed sklepem, uśpione perki, powtórna rejestracja, zachowane dane Arsenału/cechy/długu/ekwipunku. |
| E15 | Analiza/UI: te same parametry i kwalifikacja co runtime, porównanie drugiej ręki, 0% sfory jako stan poprawny, efektywny czas wyłącznie właściwej części skilla. |
| E16 | Cykl życia: śmierć/restart/zmiana piętra usuwa okna i obiekty; brak ujemnych/podwójnie zwalnianych liczników stref oraz stanu przeciekającego między testami. |

### PlayMode — rzeczywiste ścieżki gry

| Id | Scenariusz i oczekiwany dowód |
|---|---|
| P01 | Istniejący `GameConfig` bez regeneracji: sklep rzeczywiście oferuje i sprzedaje cztery perki w odpowiednich buildach; zapis/odczyt po zakupie. |
| P02 | Miecz/sztylet jednoręczny: zmierzony czas serii i koszt wytrzymałości; założenie tarczy/grymuaru wyłącza bonus; czar, technika, unik, parowanie i riposta zachowują swoje czasy. |
| P03 | Pojedynkowicz + Rozpęd oraz zmiana Arsenału: poprawne tempo i animacja po każdej zmianie, bez narastania statystyk. |
| P04 | Rzeczywisty cios przełamujący postawę, następnie mocny atak/czar: premia tylko w nowej akcji, jeden raz, na właściwym celu; elita/boss zachowują reguły CC. |
| P05 | Parowanie → riposta, a osobno unikat zastępujący ripostę: jedno wykorzystanie Wyłomu, poprawne koszty i fallback, bez dodatkowej fizycznej riposty lub obejścia obrony. |
| P06 | Sługa/strefa/reakcja z `attacker = gracz` nie tworzą i nie zużywają Wyłomu; trafienie wieloelementowe i alias/cień nie powielają premii. |
| P07 | Chochliki → ogar → golem: realne obrażenia zwykłych ataków i komend rosną zgodnie z K; usunięcie rodzaju zmienia następny atak, nie pocisk w locie. |
| P08 | Złożenie/rozdzielenie/wygaśnięcie wkładu chimery, śmiertelna chimera i Wcielenie: brak powielania sfory, zachowane migawki i budżety mocy unikatów. |
| P09 | Płonąca ziemia, Krąg żaru i strefa novy III: ten sam zestaw/siła tyknięć w połowie czasu; HU, kopia, przesunięcie i opóźniona strefa; technika i strefa wroga bez zmiany. |
| P10 | Klawiatura/pad, 720p/1080p: zakup, uśpienie, porównanie, oznaczenie celu, skład sfory i koniec VFX strefy zgodny z mechaniką; restart/śmierć nie zostawia stanów ani obiektów. |

Uruchom też odpowiednie istniejące regresje: `HuPerkTests`, `HuPortTests`, cechy klasowe, unikaty i affiksy specjalizacyjne, jeśli obecne w gałęzi. Nie kasuj asercji, żeby dopasować wynik. Awarię bazową potwierdź na aktualnym stanie i podaj jako osobny wynik, nie jako zaliczony test.

Po zamknięciu edytora, w PowerShell; najpierw testy filtrowane, potem wymagany zestaw regresji:

```powershell
$env:TURRIS_TEST_RESULT_DIR = 'TestResults/perki-specjalizacyjne/editmode'
python tools/run_tests.py EditMode
$env:TURRIS_TEST_RESULT_DIR = 'TestResults/perki-specjalizacyjne/playmode'
python tools/run_tests.py PlayMode window
Remove-Item Env:TURRIS_TEST_RESULT_DIR
```

Kontrola grafiki, bez nadpisywania wyników innych prac:

```powershell
$env:TURRIS_TEST_RESULT_DIR = 'TestResults/perki-specjalizacyjne/gfx'
$env:TURRIS_SHOT_DIR = 'TestResults/perki-specjalizacyjne/zrzuty'
python tools/run_tests.py PlayMode gfx
Remove-Item Env:TURRIS_SHOT_DIR
Remove-Item Env:TURRIS_TEST_RESULT_DIR
```

Jeśli zmienne miały wcześniej wartości, zachowaj je i przywróć zamiast usuwać. Zrzuty **otwórz i obejrzyj**. Dla tempa i stref dołącz krótki ciąg klatek z czasami/metrykami, nie wyłącznie pojedynczy efekt końcowy. IMGUI i nawigację padem sprawdzaj w trybie okienkowym, bo `gfx` z batchmode nie dowodzi poprawności menu.

## 12. Krótka walidacja balansu i dostępności

Nie ogłaszaj zbalansowania na podstawie samych testów funkcji. Porównaj na tych samych seedach i wyposażeniu: bez perka / z danym perkiem / legalna silna synergia. Wydatek dusz jest częścią porównania: pokaż też alternatywny zakup za zbliżoną kwotę, np. istniejący perk szybkości lub obrażeń.

| Perk | Minimalny rdzeń buildu | Synergia do sprawdzenia | Główna słabość |
|---|---|---|---|
| Pojedynkowicz | dowolna legalna jednoręczna broń wręcz, pusta druga ręka | Rozpęd + Szybkie ręce | brak ochrony/utility drugiej ręki, zużycie wytrzymałości |
| Wykorzystaj wyłom | broń z postawą lub parowanie, akcja kończąca | affiks postawy + ciężki cios; osobno Krwawa riposta | przygotowanie, krótki czas, ryzyko przerwania |
| Różnorodna sfora | dwa dostępne rodzaje przywołań | trzy rodzaje + affiks sług; osobno Chimera i Dyrygent | sloty, mana, znikanie wkładów |
| Gwałtowna manifestacja | Krąg żaru lub Płonąca ziemia | nova III + pieczęć kopiująca; osobno Prąd głębin | krótsza kontrola miejsca, precyzja rzutu, statusy |

Potwierdź, że rdzenie można legalnie zdobyć bez konkretnego rzadkiego unikatu. Ustal i opisz koszt/realny moment uzyskania drugiego rodzaju sług. Zmierz na grupie zwykłych wrogów, elicie, bossie, pod presją defensywną i przy niskiej manie/wytrzymałości:

- czas zabicia i obrażenia według źródła, przeżywalność, zużycie zasobów;
- rzeczywistą częstotliwość ataków i rozjazd animacji z trafieniem;
- liczbę przełamań oraz utworzonych/zużytych/wygasłych okien Wyłomu; udział ripost;
- aktywne rodzaje sfory, czas utrzymania 10/20%, obrażenia sług i koszt ich odnawiania;
- liczbę tyknięć, surowy budżet strefy, czas życia, pokrycie pola, obrażenia statusów i reakcji;
- częstość ofert i faktyczne ceny na kilku stałych seedach; brak wypierania całej dotychczasowej puli.

Matematyczne kryteria odbioru są ścisłe: brak podwójnych premii i zachowana liczba/budżet tyknięć. Kryteria siły buildów są hipotezami do oceny — nie ustanawiaj arbitralnego wspólnego limitu DPS wszystkim czterem perkom. Jeśli występuje dominacja lub pętla, opisz scenariusz i zaproponuj najmniejszą korektę parametru; nie zmieniaj po cichu klas, bossów i starych perków.

## 13. Wynik pracy i kryteria odbioru

Wdrożenie jest zakończone dopiero, gdy:

1. Wszystkie cztery perki są kupowane w prawdziwym sklepie, działają na istniejącym `GameConfig`, mają poprawne warunki, ceny i limit poziomu oraz przeżywają zapis/odczyt bez wcześniejszego wejścia do sklepu.
2. Pojedynkowicz przyspiesza dokładnie wskazane ataki, Wyłom reaguje na rzeczywiste przełamanie i działa również w ripostach, Sfora liczy rodzaje dynamicznie bez naruszania migawek, Manifestacja kompresuje harmonogram raz z zachowaniem jego zdarzeń.
3. Stare HU, talenty, cechy klasowe, unikaty, affiksy, pauza, pad i dane zapisu nie mają nieuzasadnionych zmian. Raport wskazuje rzeczywiste wyniki regresji i wszelkie ograniczenia.
4. Obrazy/timing potwierdzają zgodność UI i efektów z mechaniką; zrzuty zostały obejrzane, a wynik nie opiera się wyłącznie na kompilacji.
5. Powstaje `docs/RAPORT_PERKI_SPECJALIZACYJNE.md` z tabelą stanu czterech perków, mapą kodu, zasadami interakcji, wynikami E/P, pomiarami, ścieżkami dowodów i listą nierozwiązanych problemów. Oddziel „zaimplementowane”, „przetestowane” i „zbalansowane”.
6. Aktualizujesz odpowiednie fragmenty `README.md` i `docs/PROGRESS.md`. Dodaj zwięzły katalog zasad/cen do raportu lub osobnego pliku, zamiast udawać, że nowe wpisy należą do wygenerowanych danych HU.

W odpowiedzi końcowej podaj konkretnie: co działa, co i jak sprawdzono, gdzie są raport/zrzuty oraz co wymaga dalszej pracy. Nie nazywaj ukończoną mechaniki, dla której istnieją tylko definicje lub atrapy testowe.

## 14. Inspiracje i oddzielenie ich od projektu Turris

Poniższe strony były materiałem inspiracyjnym do analizy, nie źródłem cen, progów ani parametrów Turris. Nie kopiuj całego drzewka ani wymagań systemów, których nasza gra nie ma. Przy ponownym porównaniu sprawdź aktualne opisy źródeł.

- [PoE2 Vault — katalog pasywów](https://www.poe2vault.com/passives).
- [Dance with Death — specjalizacja jednej ręki](https://poe2db.tw/us/Dance_with_Death).
- [Crushing Impacts — nagroda za silne ogłuszenie](https://www.poe2vault.com/passives/crushing-impacts-59372).
- [At your Command — różnorodność przywołań](https://www.poe2vault.com/passives/at-your-command).
- [Wildsurge Incantation — intensywność kosztem czasu](https://poe2db.tw/us/Wildsurge_Incantation).

Autorskie adaptacje Turris obejmują m.in. jednorazowe okno po przełamaniu, dynamiczne +10% za inny rodzaj sługi oraz kompresję czasu zachowującą jednostkowe trafienia. Nie przedstawiaj ich jako dosłownych mechanik Path of Exile 2.
