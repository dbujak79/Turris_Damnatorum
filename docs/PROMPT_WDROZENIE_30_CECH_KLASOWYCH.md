# Prompt wdrożeniowy: 30 wybieranych cech klasowych

Data ustaleń: 2026-10-30. Projekt: Turris Damnatorum.

Status tego dokumentu: zatwierdzony zakres do przyszłego wdrożenia, ze startowymi hipotezami balansu. Samo przygotowanie promptu nie oznacza zaimplementowania mechanik.

## 1. Zadanie i oczekiwany rezultat

Wdróż kompletny system wyboru jednej z dwóch wyjątkowych cech klasowych na początku podejścia dla każdej z 15 grywalnych klas. Łącznie wykonaj wszystkie 30 wariantów opisanych poniżej, wraz z działaniem w grze, wyborem w przygotowaniu, obsługą klawiatury i pada, zapisem, botami, opisami, oprawą i testami.

Autor zaakceptował cały katalog. Sześć wyróżnionych wcześniej pomysłów było propozycją kolejności prototypowania, a nie ograniczeniem zakresu. Nie zakończ zadania na sześciu wariantach, samym szkielecie systemu, ikonach, pozorowanych efektach ani kodzie działającym wyłącznie w testach.

Cel projektu: każda klasa ma dawać niedostępne innym klasom prawo do zmiany konkretnej reguły walki. Wybrana cecha ma uzasadniać nietypową kombinację broni, czarów, statystyk, statusów lub wyposażenia. Sam wzrost obrażeń nie zastępuje opisanej mechaniki.

Wszystkie 30 wariantów jest równorzędną częścią zadania. Pracuj etapami, ale utrzymuj jeden spójny plan do ukończenia całości. Nie wymagaj kolejnego zatwierdzania każdego wariantu lub rutynowych decyzji implementacyjnych. Istotne odstępstwo od zatwierdzonej mechaniki przedstaw autorowi, zamiast samowolnie usuwać trudny wariant.

## 2. Ustalenia autora, których trzeba przestrzegać

1. Gracz wybiera dokładnie jedną z dwóch cech swojej konkretnej klasy przed rozpoczęciem podejścia.
2. Oba warianty są dostępne od razu, bez popiołu, losowania i kosztu w budżecie przygotowania.
3. Wybór obowiązuje przez całe podejście. Nie zmienia się go w sklepie, przy zmianie broni ani po piętrze.
4. Każda cecha należy wyłącznie do swojej klasy. Zmiana wyglądu postaci nie daje cech wyglądu.
5. Zachowaj swobodę korzystania ze zwykłych broni i skilli według ich wymagań. Nie przywracaj globalnego przypisania wspólnych umiejętności do klas.
6. Wybrana cecha może zawierać część pasywną i aktywację. Obsługuje ją dotychczasowy przycisk klasy: domyślnie G / Select (View), z uwzględnieniem zmienionych przypisań.
7. Zachowaj trzy zwykłe sloty Q/E/R oraz A/X/Y. Wybór cechy nie zajmuje zwykłego slotu.
8. Istniejące fundamenty klas — Furia, Żądza krwi, Pobożność, Arkanowa osłona, Zasadzka, Dwie klingi, Klinga zaklęć, Sokole oko, Więź dusz — pozostają, chyba że konkretna karta jawnie zmienia ich działanie.
9. Nowy wariant zastępuje dotychczasową aktywną sygnaturę pod przyciskiem klasy. Stara sygnatura nie pozostaje automatycznie jako darmowa trzecia opcja ani umiejętność ogólna. Zachowaj stare definicje tam, gdzie wymaga tego migracja zapisów.
10. Czerwona linia Berserkera ma być połączona z Żelazną skórą: im mniej rzeczywistego życia, tym większa redukcja otrzymywanych obrażeń. To część wariantu A, a nie trzecia cecha.
11. Nie wprowadzaj gwarantowanego przeżycia śmiertelnego ciosu, nieśmiertelności przy uniku ani ukrytej ochrony przed śmiercią. Istniejące, niezależne moce przedmiotów zachowują swoje udokumentowane reguły.

Nazewnictwo w UI: nowy wybór nazywaj „Cecha klasowa”, a istniejącą bazową właściwość klasy „Mechanika klasy”, jeżeli potrzebne jest ich rozróżnienie. Nie myl nowego wyboru z punktami Siły, Zręczności, Inteligencji i Wytrzymałości.

## 3. Obowiązkowy kontekst projektu

Najpierw przeczytaj AGENTS.md, następnie docs/PROGRESS.md i README.md. Porównaj ten prompt z aktualnym kodem; późniejsze bezpośrednie decyzje autora mają pierwszeństwo. Zastosuj dostępny skill hack-and-slash-design i potrzebne materiały o walce, synergiach oraz testowaniu. Specjalistyczne skille Unity stosuj zgodnie z ich rzeczywistym zakresem i platformą.

- Projekt i wszystkie zapisy zmian: C:\GIT\Gry\Turris_Damnatorum.
- Język UI, komentarzy, raportów i dokumentacji: polski.
- Unity: 6000.6.3f1, C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe.
- Kod: Assets/TurrisDamnatorum/Scripts; testy: Assets/TurrisDamnatorum/Tests.
- Hell Unleashed jest wyłącznie źródłem do odczytu. Nie zapisuj do jego repozytorium.
- Nie uruchamiaj TurrisSetup.RunBatchForce bez osobnej zgody. Zastosuj celowaną, idempotentną migrację tylko właściwych pól i nowych assetów.
- Nie wykonuj commita ani pusha bez prośby autora.
- Zachowaj cudze i wcześniejsze niezatwierdzone zmiany. Sprawdź stan repozytorium przed edycją.
- Wszystkie modele trzymające broń muszą mieć zamknięty chwyt palców z kciukiem, również obiema dłońmi broni dwuręcznej. Obowiązuje docs/STYL_GRAFICZNY_GOTYCKIE_ANIME.md.
- Nie regeneruj istniejących bohaterów i animacji jako skrótu do rozwiązania problemu. Nie psuj poprawionych kostek, chwytań, retargetu i zestawu ruchów topora/młota.
- Testy CLI wymagają zamkniętego edytora: poproś autora o jego zamknięcie dopiero wtedy, gdy trzeba je uruchomić. Nie zamykaj jego edytora samodzielnie. Dopuszczalny istniejący workflow testów w otwartym edytorze wymaga potwierdzenia właściwego dataPath i instancji.
- Zrzuty trzeba obejrzeć. Sam fakt powstania PNG nie jest potwierdzeniem poprawnego wyglądu.

## 4. Miejsca integracji do sprawdzenia

Podane lokalizacje są mapą istniejącej architektury, a nie poleceniem umieszczenia całej implementacji w tych plikach:

- Runtime/Data/ClassDefinition.cs: obecne trait i signatureSkill.
- Runtime/Core/DefaultContent.Heroes.cs: 15 klas, startowe zestawy i sygnatury.
- Runtime/Core/DefaultContent.HellSkills.cs: budowanie ogólnych pul skilli.
- Runtime/Meta/MetaService.cs: LoadoutPlan, LastPlan i RunFactory.Create; odszukaj rzeczywiste deklaracje.
- Runtime/Meta/PlayerProfile.cs: zapamiętywanie przygotowania i zgodność profilu.
- Runtime/Run/RunState.cs, RunState.Seals.cs i RunSnapshot.cs: stan podejścia, indeks treści, zapis/odtwarzanie oraz wyzwania.
- Runtime/UI/GameUI.cs i powiązane pliki: przygotowanie podejścia, pomoc, HUD i nawigacja.
- Runtime/Player/PlayerCombat.cs, PlayerCombat.Hero.cs i pozostałe części PlayerCombat: akcje, furia, szał, osłona, leczenie, obrona, trafienia, wykonanie skilli.
- Runtime/Stats/BuildSnapshot.cs i WeaponFamilies.cs: przeliczenia broni, wyposażenie, wymagania skilli, rzeczywiste źródło ataku.
- Runtime/Seals/HuSkillCaster.cs, HuCast.cs, HuSkillShape.cs, HuShot.cs, HuWorld.cs, TurrisSkills.cs i HuBalance.cs: kompozycja, wykonanie, wyzwalacze i korekty danych.
- Runtime/Combat/StatusEffects.cs, Servant.cs i pozostałe rzeczywiste moduły trafień, pocisków, barier oraz stref.
- Runtime/Player/PlayerCombat.Gear.cs i istniejące mechaniki perków: leczenie, ciernie, wybuchy zwłok, reakcje i zabezpieczenia.
- Runtime/Animation: animacje, przenikanie i prowadzenie rzeczywistych faz akcji.
- Tests/EditMode/HeroClassTests.cs, ClassLoadoutTests.cs i testy statystyk/skilli/pieczęci.
- Tests/PlayMode/HeroSkillPlayTests.cs, HuSkillCasterPlayTests.cs, SealFlowPlayTests.cs, GamepadPlayTests.cs i Tests/PlayMode/Bots.

Nie ufaj wyłącznie opisom w README: część historycznych opisów bywa starsza od kodu. W szczególności sprawdź rzeczywiste limity obrony, liczenie odnowień, rezerwację aur, ograniczenia wyzwalaczy i zasady przywołań.

## 5. Model danych, trwałość i migracja

1. Wprowadź stabilną tożsamość wyboru cechy, np. classFeatureId. Nie utożsamiaj jej z indeksem na ekranie, nazwą wyświetlaną ani aktualnym wyglądem.
2. Zdefiniuj dane każdej cechy: id, klasa właściciela, nazwa, krótki i pełny opis, parametry, sposób aktywacji, dozwolone źródła, czytelne warunki użycia.
3. Rozdziel definicję od runtime'u. Bieżące liczniki, przechwycone pociski, cele i znaczniki należą do konkretnego podejścia/postaci, nie do współdzielonego ScriptableObject.
4. Klasy serializowane przez Unity umieszczaj w plikach zgodnych z nazwą klasy. Nie zmieniaj kolejności istniejących wartości enumów zapisanych w assetach.
5. Dodaj wybór do LoadoutPlan, RunFactory, RunState, profilu ostatniego wyboru dla poszczególnych klas, snapshotów, wyzwań, powtórek i telemetrii.
6. Zapamiętany wybór innej klasy nie może przejść do aktualnej. Nieprawidłowe lub obce id zastępuj bezpiecznym wariantem domyślnym, z diagnostyką.
7. Nowe podejście ma dokładnie jeden wariant. Zmiana ekranu, wyglądu i wyposażenia nie może uruchomić obu kontrolerów.
8. Nowe snapshoty zapisują jawnie wybraną cechę. Indeks treści musi obejmować oba warianty każdej klasy, także aktualnie niewybrany.
9. Stary zapis bez nowego pola mapuj na wariant A danej klasy. Dla starego Berserkera z hu_undying oznacza to Czerwoną linię połączoną z Żelazną skórą. Zachowaj życie, przedmioty, waluty, skille i postęp starego zapisu.
10. Stara sygnatura zapisana jako osobny wpis nie może przetrwać obok nowej cechy i dawać dodatkowego działania. Zwróć jej niezgodne pieczęcie do istniejącego systemu przechowywania zamiast kasować przedmioty.
11. Nie przenoś przypadkowo zgromadzonej Łaski, Żaru, grotów, przechwyconych pocisków i znaczników między piętrami. Dla każdej cechy jawnie określ reset.
12. Snapshot początku piętra odtwarza trwały wybór oraz potrzebne wyposażenie, w tym drugi zestaw Arsenału. Odtwarzanie śródwalkowego zapisu, jeżeli rzeczywiście istnieje, musi także zachowywać długi i aktywne zobowiązania; nie umożliwiaj ich kasowania przez zapis/wczytanie.
13. Nie pozwól wyjściem z walki skasować długu many lub kosztu zdrowia na trudności zachowującej zasoby. Najpierw rozlicz zobowiązanie zgodnie z polityką przejścia, dopiero potem zastosuj jawne odnawianie zasobów piętra.

### Id klas i stabilne id proponowanych cech

Zachowaj istniejące id klas. Poniższe nowe id mogą zostać użyte bezpośrednio; ewentualną zmianę konwencji wykonaj spójnie i udokumentuj:

| Klasa | Id klasy | Wariant A | Wariant B |
| --- | --- | --- | --- |
| Wojownik | class_knight | class_feature.warrior.guard | class_feature.warrior.arsenal |
| Paladyn | class_hu_paladin | class_feature.paladin.absolution | class_feature.paladin.judgement |
| Berserker | class_hu_berserker | class_feature.berserker.redline | class_feature.berserker.fury_spend |
| Rębajło | class_hu_dual | class_feature.dual.mismatched_pair | class_feature.dual.unbroken_dance |
| Mag | class_mage | class_feature.mage.arcane_debt | class_feature.mage.mana_cloak |
| Mag ognia | class_hu_mage_fire | class_feature.fire.living_furnace | class_feature.fire.great_fire |
| Mag wody | class_hu_mage_water | class_feature.water.enduring_frost | class_feature.water.deep_current |
| Mag powietrza | class_hu_mage_air | class_feature.air.silent_eye | class_feature.air.colliding_fronts |
| Mag ziemi | class_hu_mage_earth | class_feature.earth.bastion | class_feature.earth.shattering_shell |
| Przywoływacz | class_hu_summoner | class_feature.summoner.incarnation | class_feature.summoner.chimera |
| Mag bitewny | class_hu_battlemage | class_feature.battlemage.weapon_arcana | class_feature.battlemage.two_beats |
| Łotrzyk | class_rogue | class_feature.rogue.false_witness | class_feature.rogue.stolen_trick |
| Łucznik | class_hu_archer | class_feature.archer.hunters_nails | class_feature.archer.firing_position |
| Zabójca | class_hu_assassin | class_feature.assassin.pinned_shadow | class_feature.assassin.open_artery |
| Łucznik arkanów | class_hu_arcane_archer | class_feature.arcane_archer.spell_arrow | class_feature.arcane_archer.answering_arrow |

## 6. Wybór na początku podejścia i obsługa gracza

- Po wybraniu klasy pokaż dwie równorzędne karty cech, przed zatwierdzeniem startu.
- Karta zawiera nazwę, wyłączną regułę, korzyść, koszt/ryzyko, sposób użycia i przykład nietypowego buildu. Używaj liczb z właściwej konfiguracji, bez duplikacji ręcznie wpisanych wartości.
- Obie karty muszą działać myszą i padem. Fokus na karcie nie zmienia jeszcze zatwierdzonego wyboru; wybór oznacz osobnym czytelnym stanem.
- A/Enter wybiera, B/Wstecz cofa zgodnie z istniejącą nawigacją. Przywróć fokus po zmianie klasy i powrocie z innych paneli.
- Ekran podsumowania pokazuje klasę, wygląd, cechę, broń, skille i trudność. Wygląd jest niezależny od mechaniki.
- Restart po śmierci zachowuje wybraną cechę. Powrót do przygotowania pozwala ją zmienić przed następnym podejściem.
- Podpowiedź G/View aktualizuje się po remapowaniu i zmianie urządzenia. Nie dokładaj nowego obowiązkowego przycisku na padzie.
- Rozróżniaj na HUD: gotowość, przygotowanie, działanie, zgromadzony zasób, cel, drugą fazę aktywacji oraz odnowienie.
- G w stanie przygotowania może przejść w drugą fazę, a zwykłe Q/E/R mogą pełnić jawnie opisany wybór skilla. Nie może przy tym przypadkowo wywołać normalnego rzutu lub potwierdzenia menu.
- Niepowodzenie ma konkretny komunikat po polsku: brak gardy, brak drugiej broni, brak osłony, brak sługi, brak miejsca, niedozwolony czar, brak celu, pełny magazyn itp.
- Nie pobieraj zasobów i nie rozpoczynaj odnowienia, jeżeli aktywacja nie przeszła walidacji. Wyjątki po faktycznym rozpoczęciu rzutu/misji pocisku muszą być jawne.
- Części pasywne opisz także poza walką. Gracz ma rozumieć, dlaczego dana broń lub statystyka stała się interesująca.

## 7. Wspólny kontrakt walki i wyłączności

### 7.1. Jedno źródło prawdy

Reguły trafień, kosztów, statusów i timerów muszą wynikać z logiki gry, nie z obecności efektu VFX albo zdarzenia przypadkowego klipu. Współpracuj z istniejącą kolejką akcji i mechanikami trafień.

Rozszerz kontekst trafienia tylko o potrzebne informacje: właściciela, źródło geometryczne, początkowy atak/rzut, pochodzenie kopii, kategorię efektu, wspólny budżet i już trafione cele. Dostosuj nazwy do aktualnej architektury, zamiast budować drugi równoległy system obrażeń.

Stosuj lokalne kontrolery cech zamiast ogromnego zestawu nowych wyjątków we wszystkich częściach PlayerCombat. Wspólne operacje — rezerwacja zasobu, księgowanie kosztu, kontekst trafienia, wybór celu — powinny mieć jedno wykonanie.

### 7.2. Pochodzenie i zabezpieczenia łańcuchów

- Odróżnij ręczny atak/rzut, kopię, tyknięcie, reakcję, sługę, koszt życia i otrzymany cios.
- Echo, Kaskada, rozszczepienie, przebicie i rykoszet dziedziczą pochodzenie. Nie tworzą same z siebie nowych ręcznych aktywacji cechy.
- Jedno trafienie nie nalicza wielokrotnie tej samej premii podczas przechodzenia przez cień, ciało, osłonę i potomną eksplozję, jeżeli dana karta tego nie przewiduje.
- Zachowaj prawidłowe istniejące wielotrafienia: kontakt pocisku i jego przewidziany wybuch mogą pozostać oddzielnymi zdarzeniami. Nowe zabezpieczenia nie mają wyłączyć legalnych kombinacji ogólnych.
- Każdy magazyn ma pojemność, termin ważności i zasadę usuwania referencji do zmarłych lub zniszczonych obiektów.
- Każdy efekt potomny ma określone uprawnienia do kradzieży życia, statusów, reakcji, generowania zasobu i dalszych wyzwalaczy.
- Stosuj istniejące limity zdarzeń wtórnych i dodaj diagnostykę ich osiągania. Nie ukrywaj utraconej funkcji arbitralnym ucinaniem każdego ciekawego połączenia.

### 7.3. Wymagania i skalowanie

Tylko jawne wyjątki danej cechy mogą zastąpić fokus, atrybut, źródło broni lub zasób. Sprawdź je zarówno w UI przygotowania/ekwipunku, jak i w runtime. Samo obejście walidatora w przycisku to błąd.

Broń, moc czarów, żywioł, penetracja, poziom przedmiotu i cecha są naliczane zgodnie z jednym opisanym porządkiem. Zapisuj, czy przenoszony budżet jest przed czy po obronie; nie licz obrony dwa razy ani nie usuwaj jej niezamierzenie.

Koszt życia i obniżenie dostępnego limitu życia nie są atakiem wroga: nie generują furii za otrzymanie ciosu, Zemsty, cierni ani leczenia za obrażenia.

### 7.4. Wyłączność i pieczęcie

Nie dodawaj cech do zwykłego sklepu, wspólnych nagród, mocy przedmiotów ani skilli dostępnych wszystkim. Waliduj właściciela także przy wczytaniu zapisu i wykonywaniu aktywacji.

Domyślnie klasowa reguła — np. dług many, nadleczenie w Łaskę, wybór arsenału, inkarnacja — nie jest kopiowalnym komponentem zwykłego skilla. Wspólne pieczęcie nie mogą uruchamiać drugi raz całej transformacji/rezerwacji ani zmieniać właściciela.

Jeśli zachowujesz obsługę pieczęci sygnatur, utwórz jawną zgodność dla bezpiecznej części ofensywnej. Nie wyłączaj po cichu wszystkich dawnych pieczęci i więzi: pokaż niezgodność oraz prawidłowo zwróć przenoszone pieczęcie. Wybór zgodnych wzmocnień ma nadal mieć sens.

### 7.5. Pauza, anulowanie i reset

Używaj zegara gry. Pauza zatrzymuje okna, długi czasowe, pociski i magazyny. Opisz koszt anulowania przed i po faktycznym wyzwoleniu.

Śmierć usuwa byty i kontrolery runtime'u bez wykonywania nowej ofensywy. Koniec piętra usuwa cele, groty, ściany, wabiki i przenośne efekty; zasoby i zobowiązania rozliczaj według jawnego kontraktu. Zwykły reset odnowień na piętrze nie daje dodatkowego wariantu cechy.

## 8. Parametry balansu

Wszystkie parametry umieść w danych lub dedykowanej konfiguracji. Czasy i wartości podane w kartach są hipotezami startowymi. Możesz stroić liczby po pomiarach bez pytania o każdą korektę, ale zachowaj zaakceptowaną mechanikę, koszt alternatywny i jej wyłączność.

Przed wdrożeniem grupy zapisz roboczo: czas przygotowania, czas działania, koszt, odnowienie, pojemność, częstotliwość rozliczenia, zasady kopiowania, reakcje z atrybutami i zasady na bossie. Nie zostawiaj wartości nieokreślonych w finalnej implementacji.

Nie dawaj wszystkim cechom identycznego odnowienia i DPS. Porównuj ich faktyczne korzyści: bezpieczeństwo, kąt ataku, dostęp do narzędzi, przygotowanie, dług, mobilność, kontrolę i możliwości budowania.

## 9. Pełny katalog: Wojownik, Paladyn, Berserker i Rębajło

### 9.1. Wojownik A — Walka w gardzie

- Reguła wyłączna: utrzymywanie rzeczywistej gardy podczas własnego podstawowego ataku lub techniki. Uwzględnij podstawowy ostrzał z różdżki/łuku, jeżeli aktualny zestaw rzeczywiście zapewnia legalną gardę.
- G otwiera roboczo 6 s działania. Gracz nadal świadomie trzyma blok i kieruje gardą; aktywacja nie włącza automatycznego bloku ze wszystkich stron.
- Korzystaj z redukcji, kąta i kosztu aktualnej gardy. Dozwolone ataki wroga zachowują normalne znaczenie kolorów i flag blokowania.
- Podczas działania regeneracja wytrzymałości jest zatrzymana. Atak, blok i unik obciążają ten sam rzeczywisty zasób.
- Przełamanie gardy działa i przerywa okno ofensywne zgodnie z rzeczywistymi zasadami akcji. Cios od tyłu nadal trafia.
- Ochrona jest warstwą bloku. Nie dodawaj jej jako kolejnego ogólnego procentu redukcji.
- Zwykłe czary ze slotów nie zyskują automatycznie równoległej gardy, jeżeli nie wynika to z jawnej reguły tej karty; rozróżnij je od podstawowego strzału różdżki.
- Brak legalnej gardy daje informację i nie pobiera kosztu.
- Buildy do dowodu: tarcza + sztylet; tarcza + różdżka.
- Test kluczowy: atak i blok mają miejsce w tej samej osi czasu; wspólny zasób wyczerpuje się, garda zostaje przełamana, a plecy są odsłonięte.

### 9.2. Wojownik B — Arsenał weterana

- Reguła wyłączna: dwa przygotowane zestawy główna/druga ręka i szybka zmiana zestawu przez G.
- Dodaj czytelny wybór drugiego zestawu w przygotowaniu oraz jego zarządzanie w ekwipunku. Rdzeń wariantu musi działać już na starcie, z legalnymi podstawowymi broniami.
- Dany egzemplarz przedmiotu ma jeden identyfikator i nie może być równocześnie kopiowany do obu zestawów, plecaka i sklepu.
- Oba zestawy liczą się do obciążenia. Premie aktywnego wyposażenia dają aktualnie trzymane przedmioty; nie sumuj pasywnych premii obu zestawów.
- G zachowuje etap serii, pozwalając np. rozpocząć sztyletem i zakończyć ciężką bronią. Zdefiniuj legalne okno przełączenia i minimalny czas, aby spam G nie usuwał wszystkich powrotów ataku.
- Atak już wyzwolony zachowuje prawidłowo zapisane źródło, obrażenia i wymagania. Nie podmieniaj jego siły w połowie trafienia przez zmianę broni.
- Wspólne trzy sloty, zasoby, pieczęcie i odnowienia są zachowane. Umiejętności z przedmiotów synchronizują się bez odnawiania ładunków przy każdym przełączeniu.
- Zestaw 2H nie zachowuje tarczy ani grymuaru z poprzedniego zestawu. Blok, parowanie i fokus przeliczają się atomowo.
- Snapshot i restart zachowują oba zestawy, ich ulepszenia, afiksy i aktywny zestaw.
- Buildy: sztylet + topór; łuk + miecz/tarcza.
- Test: przełączanie 100 razy nie duplikuje przedmiotów, nie leczy, nie odnawia skilli i zachowuje poprawne chwyty.

### 9.3. Paladyn A — Rozgrzeszenie

- Reguła wyłączna: nadleczenie zamieniane na magazyn Łaski, następnie wydawane na ofensywę.
- Przeprowadź leczenie przez istniejący kontrakt limitów. Rozróżniaj legalnie dostępną ilość leczenia, faktyczne przywrócone PŻ i nadwyżkę.
- Łaska pochodzi z nadwyżki po zastosowaniu odpowiednich limitów źródła, nie z nieograniczonego teoretycznego leczenia przed ograniczeniami.
- Uwzględnij regenerację, aurę, kradzież życia, leczenie za zabójstwo i flaszki z jawnymi współczynnikami. Nie pobieraj dodatkowych flaszek automatycznie.
- G wydaje zgromadzoną Łaskę na rzeczywistą falę osądu wokół Paladyna. Wybierz ten jeden spójny sposób wykonania, zamiast nieopisanej losowej zamiany na następny cios.
- Robocza pojemność magazynu: 40% normalnego maksymalnego życia; ostateczny przelicznik Łaski na obrażenia ustal w danych i zmierz.
- Rozgrzeszenie nie kradnie życia, nie wytwarza Łaski i nie aktywuje ponownie własnego źródła leczenia. Zwykłe efekty mają jawny kontrakt zgodności.
- Łaska zeruje się na nowym piętrze. Brak Łaski oznacza brak wydania zasobu, a HUD pokazuje warunek.
- Buildy: regeneracja + Aura odnowy; kradzież życia + szybkie trafienia.
- Test: leczenie przy częściowym życiu najpierw uzupełnia PŻ; dopiero nadwyżka zasila magazyn. Horda nie obchodzi limitu leczenia i pojemności.

### 9.4. Paladyn B — Sąd przysięgi

- Reguła wyłączna: czasowe odwrócenie aktualnie aktywnych aur i przeniesienie ich środka na jednego oznaczonego przeciwnika.
- G wybiera cel według namierzenia/pomocy w celowaniu. Działają tylko rzeczywiście aktywne aury, maksymalnie legalna liczba Paladyna.
- Niezłomność obniża obronę/pancerz wrogów; Czystość ich odporności; Zapał tempo; Męstwo obrażenia; Odnowa zadaje okresowy ubytek według siły własnego leczenia; Święty ogień pali wokół celu.
- Zapisz dokładne przeliczniki dla każdej z sześciu aur. Nie licz odwróconej Odnowy jako procentu maksymalnego życia bossa.
- Paladyn traci własne korzyści przeniesionych aur. Ich rezerwacja zasobów nadal obowiązuje.
- Obniżki respektują aktualne granice statystyk i odporności. Dwa efekty tego samego rodzaju nie tworzą nieograniczonych ujemnych wartości.
- Przy śmierci celu lub zakończeniu działania aury wracają do właściciela, jeżeli nadal powinny być aktywne. Nie pobieraj po raz drugi kosztu włączenia.
- W czasie Sądu określ i obsłuż wyłączenie aury, utratę jej źródła oraz zmianę wyposażenia; brak zduplikowanego buffa.
- Buildy: Paladyn z łukiem; Paladyn z przywołaniami.
- Test: jedna aura działa od startu; druga poszerza możliwości. Każdy typ działa na dozwolonym bossie, a odporność na kontrolę nie jest usuwana.

### 9.5. Berserker A — Czerwona linia: Żelazna skóra

To najważniejsze doprecyzowanie autora: połącz rezerwację niskiego życia z Żelazną skórą, której redukcja rośnie, gdy życie maleje.

#### Działanie

- G uruchamia czasowy trans. Roboczo 6 s działania; startowe odnowienie przyjmij z obecnej Żelaznej skóry i dostrój je po pomiarach, nie zakładając automatycznie stałego działania.
- Podczas transu dostępne życie jest ograniczone do 35% normalnego maksimum. Jeśli jest go więcej, obniż bieżące PŻ do tego poziomu jako jawny koszt/rezerwację, bez wyzwalaczy otrzymania ciosu.
- Zdrowie niższe od limitu nie zostaje podniesione przez samą aktywację. Nie twórz darmowego leczenia.
- Regeneracja, flaszki i kradzież życia nadal działają, lecz uzupełniają najwyżej do czerwonej linii. Nadwyżka nie jest magazynowana do późniejszego zwrotu.
- Zachowaj czytelny ryk, odpychanie/zachwianie i wyzwolenie furii ze starej Żelaznej skóry, o ile jej aktualna implementacja je zawiera. Rozlicz je tylko raz.
- Po końcu transu znika ograniczenie dostępnego życia i specjalna redukcja, ale życie samo się nie uzupełnia. Nie przechowuj ukrytej nadwyżki z chwili wejścia, która wróci jako bezpłatne PŻ.

#### Dynamiczna redukcja obrażeń

Jako startową hipotezę zastosuj:

~~~text
H_normal = normalne maksymalne PŻ bez rezerwacji Czerwonej linii
h = clamp(PŻ_bieżące / H_normal, 0, 1)
R_skóry(h) = R_max * (1 - h)
R_max = 0,80
obrażenia_po_skórze = obrażenia_przed_skórą * (1 - R_skóry(h))
~~~

| Rzeczywiste życie względem normalnego maksimum | Redukcja Żelaznej skóry |
| --- | --- |
| 100% | 0% |
| 75% | 20% |
| 50% | 40% |
| 35% | 52% |
| 25% | 60% |
| 10% | 72% |
| Granica 0% | Granica 80%; śmierć nadal działa |

- To jeden dynamiczny mnożnik zastępujący starą stałą redukcję 80% w tym wariancie. Nie dodawaj starego 80% i nowej krzywej jako dwóch osobnych osłon.
- „Odporność” w tej karcie oznacza redukcję otrzymywanych obrażeń, nie dopisanie 80 punktów do wszystkich statystyk odporności żywiołowych.
- Przeliczaj ją na podstawie życia sprzed rozstrzygnięcia danego trafienia. Cios obniża życie i dopiero kolejne trafienie korzysta z mocniejszej ochrony. Nie rozwiązuj w pętli ochrony wyliczanej z obrażeń, które sama dopiero zmieniła.
- Żądza krwi, warunki „niskie życie”, flaszki procentowe, furia i wskaźniki odnoszą się do normalnego maksimum, zgodnie z właściwą regułą źródła. 35/100 nie może stać się „pełnym życiem” 35/35 dla premii klasowych.
- Specjalna redukcja działa podczas transu na dozwolone obrażenia bezpośrednie i okresowe; koszty życia i rezerwacja nie są nią obniżane.
- Zachowaj właściwą kolejność normalnej obrony, ogólnej redukcji, Czerwonej linii, furii i pancerza w ataku. Dokumentuj i testuj rzeczywistą implementację, zamiast sumować procenty.
- Zachowaj dotychczasową zasadę osłabienia biernego leczenia podczas Żelaznej skóry, o ile aktualny kod ją stosuje. Nie obejmuj przypadkiem regeneracji furii innym limitem niż dotychczas; każdą zmianę takiej reguły uzasadnij pomiarem.
- HUD pokazuje bieżące PŻ względem normalnego maksimum, zarezerwowany obszar, pozostały czas i aktualny procent redukcji.
- Buildy: regeneracja + walka przy niskim życiu; szybkie bronie + kradzież życia + efekty dla rannej postaci.

#### Obowiązkowy dowód

Testuj monotoniczność krzywej, podane punkty tabeli, życie tuż pod/nad limitem, leczenie, zmianę maksymalnego życia, równoczesne trafienia, tyknięcia, koszt Ofiary krwi, koniec działania oraz śmiertelny cios.

Pokaż obliczenie łączenia warstw. Przykładowo dla 35% PŻ, pełnej furii 20%, pancerza w ataku 50% i ogólnej redukcji 45%, jeżeli te wartości nadal występują w kodzie:

~~~text
(1 - 0,52) * (1 - 0,20) * (1 - 0,50) * (1 - 0,45) = 0,1056
~~~

To 10,56% budżetu obrażeń przed tymi czterema mnożnikami, jeszcze przed/po innych warstwach zależnie od rzeczywistej kolejności. Zmierz również obronę fizyczną i leczenie w praktyce. Nie przedstawiaj tego wyniku jako pełnych obrażeń surowych po całym pipeline.

Jeżeli kombinacja daje praktyczną nieśmiertelność, dostrój lokalną krzywą, czas, odnowienie albo koszt tej cechy. Zachowaj zasadę „mniej życia = mocniejsza ochrona” oraz realną możliwość śmierci. Nie osłabiaj wszystkich klas globalnym limitem jako skrótem.

### 9.6. Berserker B — Spalenie furii

- Reguła wyłączna: zamiana pozostałego szału na ograniczoną liczbę opłat za zwykłe umiejętności.
- G działa w szale: kończy go i przelicza pozostałe sekundy na Żetony Gniewu. Hipoteza: jeden żeton na każde pełne 2 s, maksymalnie trzy; dokładnie obsłuż moment wejścia/wyjścia z szału i granice klatek.
- Żeton zastępuje zwykły koszt many albo wytrzymałości następnej poprawnie użytej ręcznej umiejętności. Nie zastępuje życia, rezerwacji aur i kosztów szczególnych nieobjętych kartą.
- Odnowienie i wymagania wyposażenia nadal obowiązują. Żeton nie pozwala rzucić czaru bez fokusu.
- Nie wydawaj żetonu na akcję odrzuconą przed startem. Dla przerwanego poprawnego rzutu zastosuj jednoznaczne istniejące zasady zwrotu i przetestuj brak duplikacji.
- Żetony wygasają po roboczo 10 s i nie przechodzą między piętrami. Automatyczne kopie i wyzwalacze ich nie powielają.
- Przerwanie szału rzeczywiście usuwa jego szybkość i ochronę oraz prawidłowo przelicza część furii/leczenia. Nie zostawiaj pełnej furii ukrytej pod nowym zasobem.
- Wariant B nie otrzymuje rezerwacji życia i dynamicznej Żelaznej skóry wariantu A.
- Buildy: miecz + grymuar; broń ciężka z kosztownymi technikami.
- Test: żetony są warte faktycznie oddanego szału; spam przycisku i odnowienie nie tworzą darmowej waluty.

### 9.7. Rębajło A — Niezgodna para

- Reguła wyłączna: wspólne uderzenie dwiema legalnie trzymanymi broniami jednoręcznymi, łączące ich własne zachowania trafienia.
- G uruchamia krótkie okno. Każda broń wnosi kontrolowaną część własnych obrażeń i właściwości, zamiast kopiować wynik najsilniejszej ręki.
- Uwzględnij własny żywioł, status, odrzut i właściwości przedmiotu. Ogólne premie postaci i wspólne wyzwalacze nie są podwajane przez dwa pakiety.
- Wspólny atak ma deterministyczną kolejność pakietów, roboczo główna ręka, potem druga. Jest to widoczne i testowalne dla reakcji żywiołów.
- Tempo uwzględnia obie bronie i ich skuteczność. Sztylet nie może prowadzić pełnej mocy powolnej broni z własną nieograniczoną szybkością.
- Waliduj zasięg obu narzędzi, dostępność drugiej ręki i legalność wyposażenia. Nie łącz dwóch dwuręcznych broni.
- Zachowaj przydatność rzeczywistych kombinacji: zimne + ogniste ostrze, krwawienie + uderzenie dodatkowego żywiołu. Do karty startowej użyj potwierdzonych przedmiotów z gry.
- Utrata drugiej broni kończy lub blokuje efekt bez duplikacji ataku.
- Buildy: szybka i wolniejsza broń; dwa różne żywioły.
- Test: jeden wspólny atak ma oba źródła, ale nie uruchamia dwukrotnie każdej ogólnej mocy wyposażenia.

### 9.8. Rębajło B — Taniec bez przerwy

- Reguła wyłączna: unik przenosi rozpoczęty atak/technikę zamiast ją kasować.
- G otwiera czasowe okno. Po uniku kontynuuj właściwą oś czasu, fazę, celowanie i rejestr trafień wcześniej rozpoczętej ofensywy.
- Pełne koszty ataku i uniku pozostają. Nie dodawaj nowych klatek niewrażliwości ani odporności na ataki obszarowe.
- Atak może spudłować wskutek ruchu. Nie teleportuj hitboxa do przeciwnika i nie dawaj automatycznie pełnego kąta trafienia.
- Zachowaj legalne śledzenie celu w zamachu i normalne zakończenie fazy aktywnej.
- Drugi unik nie uruchamia od początku tego samego cięcia i nie odświeża listy trafionych celów.
- Działanie po wygaśnięciu okna i w przypadku przerwania/staggera jest jednoznaczne. Nie pozostawiaj ukrytej akcji wiszącej po śmierci.
- Animacja musi pokazać przenoszone cięcie bez nagłego skoku dłoni, broni i nóg.
- Buildy: krótkie ostrza z krwawieniem; wolniejsza technika obszarowa.
- Test: to samo rzeczywiste trafienie następuje podczas zmiany pozycji, ma właściwą cenę i może legalnie spudłować.

## 10. Pełny katalog: Mag i cztery specjalizacje żywiołów

### 10.1. Mag A — Dług arkanów

- Reguła wyłączna: czasowe finansowanie ręcznych czarów ujemnym saldem many.
- G otwiera roboczo 6 s okna; limit zadłużenia jest zależny od maksymalnej, faktycznie dostępnej many po rezerwacjach.
- Rozdziel widoczną manę i dług albo rozszerz istniejący zasób tak, aby ujemne saldo nie było automatycznie zerowane przez stary setter, RefreshBuild, regenerację i zapis.
- Czar musi mieć legalne wyposażenie, wymagania i gotowy ładunek. Dług zastępuje brakującą część many; zachowuje odnowienie i inne koszty.
- Nie twórz długu z teoretycznego kosztu skilla, którego Ofiara krwi lub inny legalny efekt już opłacił inaczej.
- Limit jest twardy: czar przekraczający go jest odrzucony bez częściowego pobrania ceny.
- Po końcu okna ujemne saldo blokuje kolejne czary do spłaty. Regeneracja, mana z trafień, zabójstw, parowania i flaszki najpierw spłacają dług; reszta zasila manę.
- Leczenie, otwarcie ekwipunku, zmiana maksymalnej many i nowy obiekt gracza nie zerują długu.
- Przy spadku maksymalnej many nie kasuj istniejącego zadłużenia. Nowe pożyczanie jest blokowane, dopóki saldo nie mieści się w nowym limicie.
- Koniec piętra nie może być darmowym anulowaniem zobowiązania na trudności zachowującej manę. Zastosuj kontrakt z rozdziału 5.
- HUD pokazuje liczbowo manę, dług i limit oraz moment blokady.
- Buildy: duży zapas many + seria drogich czarów; regeneracja/odzyskiwanie many + częstsze wychodzenie z długu.
- Test: wszystkie źródła odzysku spłacają to samo saldo, a anulowanie rzutu oddaje najwyżej rzeczywiście pobrany koszt.

### 10.2. Mag B — Płaszcz many

- Reguła wyłączna: czasowe przeznaczenie many na przyjmowanie części obrażeń.
- G uruchamia roboczo 6 s ochrony. Ustal w danych udział przekierowania i przelicznik kosztu, np. 60% kwalifikowanego obrażenia za 2 many na 1 PŻ ochrony jako hipotezę startową.
- Operuj na jasno wskazanym etapie rzeczywistego pipeline. Podaj kolejność względem rodzimej osłony, barier, redukcji, minimum ciosu i zdrowia.
- Jeśli mana starcza na część ochrony, zużyj tylko dostępną ilość, a nieosłonięta reszta trafia dalej. Nie kasuj pełnego ciosu za ostatni ułamek many.
- Płaszcz nie tworzy many i nie pożycza długu wariantu A. Jedna postać ma jeden wariant.
- Koszty życia i rezerwacje nie są obrażeniami osłanianymi przez Płaszcz.
- Prawidłowo obsłuż tyknięcia, jednoczesne trafienia, zmianę maksymalnej many i legalne efekty odzyskiwania many.
- Pokaż ilość many wydanej na ochronę oddzielnie od many wydanej na czary.
- Buildy: różdżka + tarcza + odzysk many po obronie; duża pula many + rzadsze mocne czary.
- Test: rzeczywiste PŻ i mana odpowiadają podziałowi obrażeń; sprawdź skrajny build z naturalną osłoną i barierą bez ukrytej pełnej odporności.

### 10.3. Mag ognia A — Żywy piec

- Reguła wyłączna: własne kontrolowane podpalenie zasila Żar, który można wydać ofensywnie.
- G uruchamia roboczo 6 s samopodpalenia. Początkowy koszt/siła płomienia wynika z normalnego maksymalnego życia i jest konfigurowalna.
- Żar powstaje tylko z faktycznych PŻ utraconych od tego konkretnego własnego płomienia, po właściwej obronie. Obrażenia wrogów, koszty życia i inne płomienie nie podszywają się pod jego źródło.
- Odporność na podpalenie lub osłonięcie tego obrażenia odpowiednio ogranicza/zeruje produkcję Żaru. Nie produkuj paliwa z obrażeń, które nie doszły do życia.
- Własny ogień ma spójną współpracę ze stanem „płonę” i istniejącymi przedmiotami. BurnImmune nie może jednocześnie usuwać ryzyka i zostawiać pełnego paliwa.
- Kolejny ręczny podstawowy atak albo ofensywny czar wydaje określony Żar na dodatkową porcję ognia. Jeden atak/rzut ma jeden wspólny budżet, także przy salwie i obszarze.
- Żar ma pojemność i czas ważności. Efekt z wydanego Żaru nie kradnie życia, nie produkuje własnego Żaru i nie wyzwala dodatkowych kopii aktywacji cechy.
- Własne podpalenie nie uruchamia Zemsty, cierni, furii za cios ani moce „ostatnia szansa” jako sposób finansowania paliwa. Ostateczny śmiertelny skutek kosztu musi być opisany; nie wprowadzaj domyślnej ochrony na 1 PŻ.
- Na śmierci i po końcu okna usuń płomień zgodnie z jego własnym źródłem, nie kasując przy okazji normalnego podpalenia wroga.
- Buildy: wysokie życie + regeneracja + broń wręcz; przedmioty wzmacniające płonącą postać.
- Test: identyczny koszt życia nie jest wielokrotnie przeliczany przez liczbę celów albo pocisków następnego ataku.

### 10.4. Mag ognia B — Wielki pożar

- Reguła wyłączna: kilka niezależnych podpaleń własnego Maga na tym samym przeciwniku, rozróżnionych po źródle.
- G otwiera roboczo 6 s okna, z maksymalnie trzema źródłami.
- Źródłem jest definicja ręcznie użytej ofensywnej umiejętności albo grupa podstawowych ataków bronią. Nowy rzut tej samej umiejętności nie tworzy nowej rodziny podpalenia.
- Kopie, totemy, tyknięcia strefy i pociski potomne dziedziczą źródło. Nie pozwól utworzyć trzech pełnych warstw przez trzy odpryski jednego przycisku.
- Każda rodzina zachowuje własny czas i regułę mocniejszego odświeżenia. Nie sumuj co klatkę pełnego budżetu podpalenia na nowo.
- Po wygaśnięciu okna dodatkowe rodziny wygasają albo są redukowane do normalnego, najsilniejszego podpalenia. Wybierz tę drugą zasadę jako startową: zachowaj pozostały czas i DPS najsilniejszej rodziny, bez przedłużania i dopisywania jej budżetu.
- BurnImmune i odporności nadal obowiązują. Zwykłe reakcje ziemi w płonącego mają jeden ustalony kontrakt, bez mnożenia reakcji przez liczbę rodzin.
- Podstawowe trafienie ognia i nova z grymuaru dają użyteczne dwie rodziny już we wczesnym buildzie.
- Buildy: trzy różne źródła ognia; płonąca broń + dwa czary.
- Test: nowa struktura podpaleń nie zmienia zasad innych 14 klas i nie zamienia zwykłego odświeżenia w darmowe dodawanie obrażeń.

### 10.5. Mag wody A — Niezmienny szron

- Reguła wyłączna: utrzymanie warunku chłodu dla reakcji bez jego zużycia przez te reakcje.
- G oznacza pojedynczy legalny cel na roboczo 6 s i zapewnia bazowy warunek Szronu. Reakcje korzystają z osobnego znacznika, nie z podmieniania pól zwykłego statusu co klatkę.
- Ogień może wykorzystywać szok termiczny wielokrotnie, mimo że zwykle zużywa chłód. Ziemia może korzystać z właściwego warunku reakcji.
- Warunek zamrożenia jest zachowywany tylko wtedy, gdy cel faktycznie został legalnie zamrożony. Sam Szron nie nadaje pełnego zamrożenia bossowi odpornemu na kontrolę.
- Rzeczywiste unieruchomienie, jego czas, odporność po kontroli i ograniczenia bossów pozostają normalne. Wirtualny warunek reakcji nie zatrzymuje AI.
- Wprowadź wspólną konfigurowalną częstotliwość rozliczenia specjalnej reakcji na cel, roboczo maksymalnie raz na sekundę. Rozdziel limit obrażeń reakcji od istniejącej odporności na kontrolę.
- Reakcje i tyknięcia nie wywołują ponownie tej samej reakcji. Zwykłe trafienie nadal ma prawidłowy żywioł i swoje premie.
- Po wygaśnięciu usuń tylko własny znacznik, zachowując niezależne prawdziwe statusy.
- Buildy: woda + ogień; broń obuchowa i efekt zamrożenia/chłodu.
- Test: na bossie działa przygotowanie obrażeń, ale nie powstaje nieskończone zamrożenie ani ogłuszenie.

### 10.6. Mag wody B — Prąd głębin

- Reguła wyłączna: przemieszczanie własnych istniejących stref bez tworzenia nowych rzutów i odnawiania ich budżetu.
- G wybiera do ograniczonej liczby własnych pól w zasięgu, roboczo do dwóch. Ustal przewidywalny wybór przez namierzenie punktu i celownik, z widocznym podświetleniem.
- Przesuwaj je do wybranego punktu albo prowadź za Magiem w jednym jasno określonym trybie sterowania; implementacja ma być jednakowa na padzie i myszy.
- Zachowaj pozostały czas, odstęp tyknięć, pochodzenie, statusy oraz limity trafień. Przesunięcie nie odpala ponownie początkowej eksplozji.
- Koszt zależy od faktycznie poruszanych pól według konfiguracji. Brak poprawnego miejsca nie pobiera opłaty.
- Dotyczy własnych kompatybilnych stref dowolnego żywiołu. Nie przejmuj pól wroga, aur, totemów, min, ścian ani stanowiska Łucznika przez przypadkowe dziedziczenie wspólnej klasy bazowej.
- Ruch respektuje granice areny, podłoże i istotne przeszkody. Żywe pole nie teleportuje obrażeń przez ściany niezgodnie z normalną regułą danego skilla.
- Wariant wymaga użytecznego narzędzia strefowego od początku. Zapewnij widoczny wybór istniejącego zwykłego skilla strefowego w przygotowaniu, dostępny także jako wspólny skill dla innych klas; nie uzależniaj podstawowej funkcji od losowego zakupu.
- Gracz może świadomie zrezygnować z kompatybilnego skilla po rozpoczęciu podejścia, ale UI ma wtedy wyjaśniać brak celu.
- Buildy: przesuwana płonąca ziemia; łączenie kilku pól w przejściu.
- Test: przesunięcie tej samej strefy wielokrotnie zachowuje pierwotny czas i sumę jej dozwolonych tyknięć.

### 10.7. Mag powietrza A — Oko ciszy

- Reguła wyłączna: czasowe przechwycenie, przechowanie i ręczne wypuszczenie pocisków.
- G otwiera krótki magazyn wokół Maga; ponowne G albo jawnie opisany koniec okna uwalnia salwę w kierunku celowania.
- Roboczo najwyżej cztery zwykłe pociski. Zdefiniuj jawną zgodność: przechwytowalne pociski, brak promieni, fal ziemi, telegrafów i ataków arenowych.
- Własne podstawowe pociski mogą być świadomie ładowane do magazynu. Dzięki temu cecha ma funkcję także przeciw bossowi bez ostrzału.
- Zatrzymanie pocisku wroga rzeczywiście usuwa jego aktywną kolizję ofensywną. Nie zostawiaj niewidzialnego zagrożenia w pierwotnym miejscu.
- Własne pociski zachowują opłacony koszt i wspólny budżet; ich okres przechowywania nie odnawia czasu życia bez limitu. Pocisk może zostać przechwycony najwyżej raz przez tę aktywację.
- Wrogie pociski przelicz na jawny budżet gracza zależny od konfiguracji/builda, nie kopiuj bez ograniczeń obrażeń i całej logiki bossowej.
- Zwolnione pociski zachowują poprawną frakcję i właściciela. Nie przechwytują się ponownie i nie ładują samego Oka.
- Pełny magazyn przestaje przechwytywać. Nie daje niewidocznej, nieograniczonej tarczy.
- Buildy: odpowiedź na ostrzał; przygotowana salwa własnej różdżki.
- Test: pocisk nie trafia dwukrotnie, nie istnieje w magazynie i świecie jednocześnie, a sama pojemność rzeczywiście ogranicza ochronę.

### 10.8. Mag powietrza B — Zderzenie frontów

- Reguła wyłączna: wykorzystanie własnego odrzutu do tworzenia ofensywnego zderzenia.
- Podczas czasowego okna oznaczaj odrzuty z legalnych własnych ataków, broni i skilli.
- Zderzenie przesuwanego wroga ze ścianą albo innym przeciwnikiem wywołuje wyładowanie o budżecie gracza. Reaguj na rzeczywisty ruch/kolizję, nie na sam fakt przekazania niezerowego knockback.
- Dla celu odpornego na przesuwanie zastosuj ograniczone naprężenie, liczone z próby odrzutu i odporności celu. To zapewnia zastosowanie na bossie, ale nie usuwa jego odporności.
- Ustal jeden limit wyładowania na źródłowy atak i cel oraz ograniczenie liczby zderzeń w tłumie. Spoczynkowa kolizja dwóch ciał nie tyka co klatkę.
- Wyładowanie nie odrzuca ponownie na potrzeby tej samej cechy i nie tworzy rekurencji. Nie licz obrażeń od maksymalnego życia wroga.
- Powiąż odrzut z rzeczywistymi danymi, również gdy perk działa tylko na zwykłe ataki. Nie rozszerzaj globalnie zakresu perka.
- Zderzenia czytelnie pokazują punkt i kierunek, ale efekt nie zasłania zapowiedzi wroga.
- Buildy: ciężka broń z odrzutem; czary powietrza i walka przy ścianach.
- Test: cel bez ruchu, normalny wróg, elita, odporny boss i kilkanaście ścierających się ciał dają ograniczone, zgodne wyniki.

### 10.9. Mag ziemi A — Bastion ziemi

- Reguła wyłączna: stworzenie rzeczywistej, krótkiej przeszkody wpływającej na poruszanie i lot zwykłych pocisków.
- G stawia ścianę w legalnym punkcie i orientacji wyznaczonej celowaniem. Przed zatwierdzeniem sprawdź miejsce, granice, innych aktorów i portale.
- Ściana ma czas życia, trwałość i ograniczoną długość/liczbę. Nie zamyka całej areny, wyjścia, spawnu lub bossa w nierozwiązywalnym obszarze.
- Wrogowie muszą potrafić ją obejść lub zniszczyć. Zaktualizuj faktyczny sposób prowadzenia AI i zasięg atakowania przeszkody, zamiast zakładać istnienie nieużywanego NavMesh.
- Bossowie niszczą ścianę znacznie skuteczniej; ich ataki arenowe zachowują właściwe reguły i zapowiedzi.
- Zwykłe własne pociski też mogą być blokowane. Jawnie wskaż, które przebicia/wybuchy są kompatybilne.
- Własne mocne uderzenie lub zaklęcie ziemi może pobudzić pęknięcie wzdłuż ściany. To nowy ofensywny użytek przeszkody, z jednym budżetem źródłowym.
- Pęknięcie nie wywołuje drugiego pęknięcia tej samej ściany z własnych obrażeń. Zniszczenie, wygaśnięcie i kolejny G nie duplikują końcowego efektu.
- Ściana ma widoczną geometrię i kolizję odpowiadającą sobie. Bez niewidzialnej blokady po jej usunięciu.
- Buildy: łuk i przygotowanie stanowiska; młot i rozbijanie własnej fortyfikacji.
- Test: droga AI, fala portali, boss, własny ostrzał, brak miejsca, zniszczenie podczas ataku i przejście piętra.

### 10.10. Mag ziemi B — Krusząca skorupa

- Reguła wyłączna: poświęcenie pozostałej własnej ochrony na kamienną amunicję.
- G zbiera aktualnie niewydaną rodzimą Arkanową osłonę i kompatybilne bariery. Te źródła zostają rzeczywiście zużyte/usunięte.
- Pojemność płyt i całkowity budżet mają limit. Roboczo do trzech płyt; cały pozyskany budżet rozdziela się pomiędzy nie, a nie kopiuje do każdej.
- Uwzględniaj faktycznie pozostałą pojemność ochrony. Nadmiar ponad limit nie jest ukrytym magazynem na później.
- Kolejne poprawnie wykonane mocne ataki wypuszczają płytę w kierunku/na cel ataku. Ustal regułę dla mocnego strzału i różdżki, zamiast zakładać wyłącznie miecz.
- Płyta korzysta z kontrolowanego skalowania ziemi i jednego przejścia przez obronę. Nie doliczaj drugi raz pełnego obrażenia broni, które już służyło za bazę innej części.
- Płyty nie aktywują Osłony i podobnych efektów tworzących następną barierę. Pierwotny ręczny atak nadal może mieć swoje zwykłe, legalne działanie.
- Zużycie rodzimej osłony uruchamia właściwy okres jej regeneracji. Spam G nie odzyskuje świeżo rozbitej ochrony natychmiast.
- Zmiana wyposażenia i maksymalnego życia nie zwiększa już zaksięgowanych płyt. Nowy budżet wymaga nowej rzeczywistej ochrony.
- Buildy: życie i rodzima osłona; bariery oraz mocne ataki dystansowe.
- Test: ochrona znika w tej samej transakcji, w której powstaje amunicja; suma wydanych płyt nie przekracza opisanego budżetu.

## 11. Pełny katalog: Przywoływacz i Mag bitewny

### 11.1. Przywoływacz A — Wcielenie

- Reguła wyłączna: zużycie własnego sługi i czasowe przejęcie jego sposobu podstawowej walki.
- G wybiera własnego sługę czytelnym celowaniem/podświetleniem; domyślne wskazanie ma być deterministyczne. Nie przejmuj sług wrogów lub innego źródła.
- Roboczo do 8 s formy, nie dłużej niż pozostały czas wykorzystanego sługi. Sługa rzeczywiście znika z niezależnej armii.
- Chochlik daje podstawowy ostrzał ogniem, ogar szybkie krwawiące uderzenia, golem ciężkie uderzenie obszarowe. Każda forma ma gotowy podstawowy i mocny atak oraz czytelne ograniczenia.
- Życie, obrona i otrzymywane trafienia należą do gracza. Nie przejmuj braku życia i nietykalności normalnych sług.
- Gracz traci udział zużytego sługi w Więzi dusz. Forma nie jest równocześnie dodatkowym niewidzialnym sługą podbijającym licznik.
- Ataki formy korzystają z jednego źródła skalowania przywołania, z jawną zgodnością wyposażenia, statusów i premii. Nie mnoż bronią, mocą czarów i mocą sługi tego samego składnika kolejno bez założenia.
- Zachowaj zwykłe skille tam, gdzie ich wymagania są legalne, albo jawnie określ blokadę wynikającą z formy. Nie zostawiaj ukrytej tarczy, broni i fokusu po schowaniu modelu.
- Zakończenie, śmierć, wyjście z piętra i wymuszone przerwanie odtwarzają prawidłowy model, rozmiar kolizji, kamerę i wyposażenie bez leczenia.
- Nie zamieniaj Wcielenia na zwykły procentowy buff i zieloną poświatę. Gracz musi naprawdę sterować innym zestawem ataków.
- Buildy: opancerzony golem; szybki ogar z ranami i mobilnością.
- Test: wszystkie trzy formy w spoczynku, biegu, ataku, uniku, trafieniu i śmierci; prawidłowy powrót do bohatera oraz brak nietykalności.

### 11.2. Przywoływacz B — Chimera legionu

- Reguła wyłączna: połączenie kilku własnych sług w jedną istotę o zdolnościach wynikających z rzeczywistych składników.
- G zaznacza składniki i zatwierdza scalenie, przy czytelnej informacji co powstanie. Unikaj dodatkowego obowiązkowego przycisku na padzie.
- Golem wnosi ciężki korpus/uderzenia, chochlik ostrzał, ogar krwawiące natarcie. Kombinacje tego samego rodzaju też muszą mieć użytkowy, skupiony wariant, aby startowe chochliki pozwalały użyć cechy.
- Scaleni słudzy znikają. Chimera nie działa obok niewidzialnych kopii ich AI.
- Zachowaj pochodzenie budżetów składników. Najbezpieczniej prowadzić ich pozostałe czasy niezależnie: po upływie wkładu wygasa odpowiednia część mocy, a nie cała armia dostaje czas najdłuższego przywołania.
- Chimera rezerwuje miejsca zużytych składników w limicie sług tak długo, jak trwa ich wkład. Nie daje ukrytego podwojenia limitu sześciu.
- Więź dusz liczy jedną faktycznie samodzielną chimerę, a nie jednocześnie wszystkie zjedzone ciała. To świadomy koszt koncentracji armii.
- Kolejne scalenie nie odnawia pozostałych czasów i nie kopiuje przeliczonych premii. Jawnie zdecyduj, czy rozwój istniejącej chimery jest dopuszczony; jeśli tak, rozliczaj tylko nowy rzeczywisty wkład.
- Skoncentrowana moc ma cenę: mniej niezależnych kierunków ataku, mniej jednostek i zależność od prowadzenia jednej bestii.
- Wykonaj widoczną, odróżnialną postać chimery dla kombinacji; wykorzystaj istniejące zasoby/moduły, bez końcowego efektu polegającego tylko na zmianie skali zwykłego golema.
- Buildy: jeden wspierający towarzysz; trzy rodzaje przywołań rozwijające wielofunkcyjną chimerę.
- Test: kombinacje składników, różne pozostałe czasy, globalny limit, zmiana piętra i następne scalenie bez klonowania budżetu.

### 11.3. Mag bitewny A — Arkana oręża

- Reguła wyłączna: czasowe traktowanie własnej broni wręcz jako pełnoprawnego fokusu i użycie jej głównego atrybutu dla określonego skalowania/wymagań czarów.
- G otwiera roboczo 6 s okna. Obejmuje legalnie wyposażoną broń wręcz, również dwuręczną; wyklucz łuki jako broń wręcz.
- Określ deterministycznie główny atrybut na podstawie skalowania broni, a nie najwyższej przypadkowej statystyki postaci.
- W aktywnym oknie zastępuj wymaganie fokusu, odpowiednie wymaganie Inteligencji i odpowiednie skalowanie jawnie opisanym atrybutem broni. Pozostałe wymagania skilla nadal obowiązują.
- Nie usuwaj kosztu many, odnowienia ani prawdziwego czasu rzutu.
- Dopasuj moc broni-fokusu do istniejącego wzorca skilli. Nie używaj całego DPS ciężkiej broni jako drugiego mnożnika czaru bez normalizacji.
- Klinga zaklęć nie może utworzyć pętli: moc czarów zwiększa broń, która ponownie zwiększa moc tego samego czaru.
- UI musi pokazywać, że konkretny czar jest warunkowo używalny w oknie, zamiast uniemożliwiać jego wyposażenie jako „brak fokusu”.
- Przerwany rzut i koniec okna rozliczają źródło raz. Zdefiniuj, czy rzut poprawnie rozpoczęty w oknie kończy się po jego wygaśnięciu; przyjmij, że tak, z zachowanym snapshotem warunków startu.
- Zachowaj rzeczywiste niskie zasoby postaci inwestującej w Siłę zamiast Inteligencji. Cecha nie przyznaje automatycznie dużej puli many.
- Buildy: topór dwuręczny + Piekielna kula; dwie bronie + nova.
- Test: identyczna broń na innej klasie nie staje się fokusem, a zmiana wyposażenia nie daje stale odblokowanej magii.

### 11.4. Mag bitewny B — Dwa takty

- Reguła wyłączna: prowadzenie jednej własnej akcji bronią i jednego ręcznego czaru równolegle.
- G otwiera czasowe okno. Obie osie akcji zachowują własne fazy, czasy, koszty, ładunki, hittrackery i celowanie.
- Broń płaci wytrzymałość, czar manę, oba zachowują odnowienia. Każda akcja powstaje z rzeczywistego wejścia gracza, bez automatycznego kopiowania wszystkich ataków.
- Nadal trzeba spełniać wymagania fokusu i atrybutów. Wariant B nie dostaje Arkanów oręża wariantu A.
- Zdefiniuj zasady pierwszeństwa orientacji, lokomocji i celowania, aby jeden tor nie obracał modelu skokowo w przeciwną stronę.
- Maksymalnie jeden atak/technika bronią oraz jeden czar naraz. Brak rekurencyjnego uruchamiania następnych torów.
- Ogłuszenie, śmierć i wymuszone przerwanie rozliczają oba tory raz. Zwykły unik pozostaje zgodny z normalnymi regułami tej klasy; nie przejmuj Tańca bez przerwy Rębajły.
- Koszt nieudanego drugiego wejścia nie może zabierać zasobu pierwszego ani podwójnie zwracać many.
- Zadbaj o czytelną animację obu działań. Nie wystarczy wywołać obrażenia drugiej akcji podczas animacji pokazującej coś innego.
- Buildy: miecz + grymuar i krótkie czary; wolniejsze uderzenia + magia obszarowa.
- Test: dwa rzeczywiste, opłacone działania mogą trafić w różnych chwilach, a przerwanie i anulowanie zachowują niezależne księgowanie.

## 12. Pełny katalog: Łotrzyk, Łucznik, Zabójca i Łucznik arkanów

### 12.1. Łotrzyk A — Fałszywy świadek

- Reguła wyłączna: wabik zmieniający wybór celu określonego następnego ataku przeciwnika.
- G zostawia widoczną przynętę w legalnej pozycji na krótki czas, z limitem liczby oszukanych wrogów i przejętych ataków.
- AI musi faktycznie wybrać wabik dla następnego dozwolonego działania. Nie wystarczy pokazać model i dodatkowo ukryć gracza.
- Już rozpoczęte ataki zachowują swój cel/reguły śledzenia; wabik nie anuluje darmowo kończącego się ciosu.
- Obejmuje tylko oznaczone zwykłe ataki celowane. Boss może oddać jeden dopuszczony atak, ale jego fazy, przywołania, ściany i ataki arenowe działają normalnie.
- Wabik ma własną tożsamość celu i geometrię. Nie generuje zabójstw, dusz, kradzieży życia ani efektów trupów.
- Zdefiniuj współpracę z reżyserem ataków. Jeden oszukany wróg nie tworzy nieograniczonego miejsca w budżecie kolejnych atakujących ani wiecznej blokady ich akcji.
- Po rozbiciu/wygaśnięciu przeciwnik wraca do legalnego celu. Nie pozostaje w pogoni za usuniętą referencją.
- Tylko wybrany Łotrzyk ma ten wabik. Słudzy Przywoływacza nadal nie przejmują aggro na mocy wspólnego komponentu.
- Buildy: sztylet i zasadzka; bomby oraz pułapki.
- Test: celowanie wręcz i dystansowe, rozpoczęty zamach, przejęcie jednego ataku bossa, śmierć wabika i powrót AI.

### 12.2. Łotrzyk B — Kradzież sztuczki

- Reguła wyłączna: obserwacja ataku wskazanego przeciwnika i jednorazowe wykonanie jego dopuszczonego zachowania jako gracz.
- Pierwsze G oznacza cel/otwiera krótkie okno obserwacji. Poprawnie wykonany przez niego zwykły atak daje jedną przechowywaną sztuczkę. Kolejne G ją wykonuje.
- Obserwacja nie neguje zagrożenia, nie anuluje ciosu i nie daje samodzielnie niewrażliwości.
- Zdefiniuj jawny katalog zgodności rzeczywistych ataków obecnych w grze: minimum użyteczne profile wręcz, pocisku i obszaru; pokryj wszystkich obecnych wrogów co najmniej jednym normalnym zachowaniem tam, gdzie mają taką akcję.
- Atak kopiuje zachowanie, nie cały EnemyBrain, fazę, statystyki bossa lub nieskończone wywoływanie demonów.
- Każda obsługiwana sztuczka ma znormalizowany budżet, koszt, zasięg, czas, status i informację po polsku. Jeżeli szczególnego ataku nie wolno ukraść, komunikat to wyjaśnia.
- Sztuczka jest przypisana do gracza, jego frakcji i skalowania. Nie używa obrażeń wynikających z dużego mnożnika późnej fali przeciwnika.
- Jeden zapisany atak, jedno wydanie, określony czas ważności; brak przechowywania między piętrami i brak podrzucania go innym klasom przez snapshot.
- Użyj odpowiedniej animacji gracza lub nowej czytelnej reprezentacji skradzionego narzędzia. Nie pokazuj cięcia mieczem przy niewidzialnym wielkim głazie.
- Buildy: ciężka broń + pożyczony dystans; sztylet + pożyczony obszar/status.
- Test: brak podwójnego zużycia, bezpieczna referencja po śmierci źródła, ruchliwy cel, boss i nieobsługiwany atak specjalny.

### 12.3. Łucznik A — Gwoździe łowcy

- Reguła wyłączna: trafienia podstawowych pocisków zakładają rzeczywisty magazyn grotów na ofierze, do późniejszego ręcznego wyrwania.
- Pierwsze G otwiera czas osadzania; ponowne G wyrywa zgromadzone groty w dozwolonym zasięgu. Po upływie okna zastosuj jedną opisaną politykę wyzwolenia/wygaśnięcia.
- Obejmuje podstawowe pociski broni, również różdżki; ofensywna magia ze slotu nie zostaje automatycznie kolejnym generatorem grotów.
- Rozdziel zwykłe obrażenie kontaktu i odłożony budżet grota. Gracz rozumie, jaka część mocy jest magazynowana, a co już zostało zadane.
- Magazyn ma limit na cel, na aktywację i na pierwotny ręczny strzał. Dodatkowe pociski nie kopiują pełnego budżetu osadzania.
- Przebicie i rykoszet mogą osadzać groty w wielu ofiarach, ale korzystają z określonego podziału wspólnego budżetu.
- Wyrwanie zadaje rzeczywiste rany i obciąża postawę, z jawnym skalowaniem, obroną i zasadami na bossie. Samo wyrwanie nie osadza kolejnych grotów.
- DoT, eksplozje i reakcje nie udają nowego podstawowego pocisku. Kradzież życia i statusy wyrwania mają ustalone współczynniki.
- Śmierć celu sprząta jego magazyn bez tworzenia darmowych grotów na innym wrogu. HUD informuje o wybranym celu i całkowitej liczbie/budżecie.
- Buildy: szybki łuk i wykończenie; różdżka i wiele celów.
- Test: legalny kontakt plus wyrwanie działają, a osiem pocisków jednego strzału nie daje ośmiu pełnych magazynów.

### 12.4. Łucznik B — Stanowisko strzeleckie

- Reguła wyłączna: oddzielenie właściciela ręcznego podstawowego ostrzału od jego geometrycznego punktu wyjścia.
- G stawia jeden punkt w legalnym miejscu na roboczo 8 s. W ograniczonym zasięgu podstawowy strzał gracza wychodzi ze stanowiska.
- Nadal trzeba faktycznie ręcznie zaatakować, zapłacić wytrzymałość i przejść odpowiednią akcję. Stanowisko nie jest autonomicznym totemem.
- Działa z legalnym łukiem/różdżką. Zachowaj dane aktualnej broni, ale nie twórz drugiego zestawu jej premii.
- Kamera i celowanie pozostają przy graczu. Trajektorię pocisku wylicz ze stanowiska do rzeczywistego celu/punktu i sprawdź przeszkody.
- Sokole oko i inne zależności od odległości źródła trafienia korzystają tu z punktu wystrzału. Właściciel nagród, statusów i zabójstw nadal jest graczem.
- Własna animacja gracza, napięcie cięciwy/wystrzał i efekt stanowiska muszą zgadzać się czasowo. Nie wypuszczaj drugiej, niewidzialnej kopii z postaci.
- Po przekroczeniu zasięgu zastosuj jawne wyłączenie łącza/powrót do zwykłego ostrzału z komunikatem. Brak teleportowania strzał przez arenę.
- Stanowisko nie przejmuje aggro jak Fałszywy świadek i nie rzuca czarów ze slotów bez wejścia gracza.
- Buildy: boczny ostrzał i pułapki; przebicie przez linię przeciwników.
- Test: rzeczywista kolizja, właściwa odległość, koszt i pojedynczy pocisk; powrót do lokalnego strzału po końcu działania.

### 12.5. Zabójca A — Przygwożdżony cień

- Reguła wyłączna: drugi, nieruchomy punkt odbierania trafień dla jednego oznaczonego przeciwnika.
- G tworzy na ziemi cień przy aktualnym położeniu legalnego celu na roboczo 6 s, w ograniczonym zasięgu połączenia.
- Uderzenie w cień przekazuje opisany atak właściwej ofierze. Ofiara nadal normalnie porusza się i walczy.
- Cień i ciało mają wspólną tożsamość trafionego celu w obrębie jednego ataku/tyknięcia. Obszar obejmujący oba zadaje jedną należną porcję obrażeń.
- Przekazanie nie oznacza drugiego przejścia przez bonusy i obronę. Nie twórz drugiego, niezależnego zabójstwa i nagród.
- Zasadzka i trafienie w plecy liczą legalną pozycję gracza względem właściwej ofiary, nie zawsze korzystny kierunek ozdobnego cienia.
- Nietykalność, faza, śmierć i zniknięcie bossa obejmują też cień. Przekroczenie limitu zasięgu ma jawną politykę; nie atakuj usuniętego lub nieobecnego wroga.
- Pułapki i legalne obszary mogą trafić cień. Nie pozwól jednemu identycznemu impulsowi odliczyć obu colliderów osobno.
- Cień nie ma własnego życia, AI, łupu, cierni i efektów śmierci.
- Buildy: pułapki na ruchliwego wroga; wolna broń wykorzystująca nieruchomy cel.
- Test: ruchliwy boss, ciało + cień w jednym AoE, reakcja, obrażenie okresowe, blok/odporność i faza nietykalności.

### 12.6. Zabójca B — Otwarta tętnica

- Reguła wyłączna: czasowe traktowanie oznaczonego celu jako spełniającego próg niskiego życia dla kwalifikowanych premii egzekucji.
- G oznacza jednego legalnego przeciwnika na krótkie okno, roboczo 4 s. Cel ma swoje prawdziwe PŻ, które nie są modyfikowane przez sam znacznik.
- Premie ExecuteDamage i perk Kat uwzględniają znacznik w jednym wspólnym predykacie kwalifikacji. Nie wpisuj mu fałszywego Health.Fraction w całym AI.
- Zwykłe progi faz bossa, rozpacz, animacja śmierci, łup i wszystkie inne warunki nadal widzą rzeczywiste życie.
- Nie omijaj obrony, faz ani nietykalności. Cecha sama nie wykonuje automatycznego zabójstwa.
- Jeśli cel rzeczywiście ma już mało życia, nie naliczaj dwukrotnie tej samej premii egzekucji.
- Rzeczywiste ograniczenia poszczególnych perków, np. tylko kwalifikowane ataki wręcz, nadal obowiązują.
- Wariant ma być użyteczny od startu dzięki istniejącej premii egzekucji Zabójcy. Wyposażenie pozwala rozwinąć tę oś.
- Buildy: wolna broń i Kat; wysoki burst na elity i początek walki z bossem.
- Test: ten sam przeciwnik przy 100%, 50% i poniżej progu ma poprawne obrażenia oraz nie uruchamia przedwcześnie faz AI.

### 12.7. Łucznik arkanów A — Strzała zaklęcia

- Reguła wyłączna: zamknięcie jednego własnego wyposażonego czaru ofensywnego w następnym podstawowym strzale, z lokalnym zastąpieniem wymogu fokusu przez łuk.
- G przechodzi do przygotowania, a Q/E/R lub A/X/Y wybiera czar z odpowiedniego slotu. W tym kontekście wybór nie rzuca go przypadkowo normalnym sposobem.
- Pokaż, które czary są dopuszczone. Przygotowana strzała nie jest sposobem na użycie cudzej cechy klasowej, aury, leczenia, bariery lub przywołania.
- Podstawowy strzał faktycznie wypuszczający ładunek jednorazowo płaci swoją cenę oraz pełną cenę czaru i uruchamia jego odnowienie. Do tego momentu rezerwuj/zwalniaj przygotowanie atomowo; wybór nie generuje darmowego ładunku.
- W momencie wystrzału sprawdź jeszcze raz legalność broni, czaru, atrybutów, many, ładunku i celu. Wyjątek dotyczy tylko fokusu w tym użyciu.
- Ładunek wyzwala się raz, na pierwszym legalnym trafieniu, z lokalną geometrią tego punktu. Kolejne przebicia i odpryski zachowują zwykłą strzałę bez pełnego następnego ładunku.
- Ustal regułę trafienia ściany i końca lotu. Czar ma swoją zapowiedź od momentu uwolnienia i normalne reguły obrażeń; nie pomijaj jego telegrafu, jeśli decyduje o uczciwości działania.
- Pudło po wypuszczeniu nie oddaje już many ani odnowienia. Anulowanie samego przygotowania przed strzałem nie traci niepobranego zasobu.
- Brak podwójnego skalowania broni łuku i fokusu. Określ jeden wzorzec dla dostarczonego czaru.
- Zapewnij w przygotowaniu startowy wybór zwykłego kompatybilnego czaru do jednego z trzech slotów. Wariant nie może zaczynać z samymi technikami łuku i przez cały run nie mieć czego osadzić.
- Buildy: Łza lodowca dostarczona na dystans; nova wyzwolona w trafionym celu.
- Test: wybór z pada, pełen koszt raz, nielegalny skill, anulowanie, pierwsze trafienie, rozszczepienie, echo i pudło.

### 12.8. Łucznik arkanów B — Strzała odpowiedzi

- Reguła wyłączna: dobór żywiołu podstawowego pocisku łuku przy kontakcie na podstawie rzeczywistego stanu kolejnej ofiary.
- G otwiera krótkie okno lub ograniczoną liczbę ręcznych strzałów; wybierz i zapisz jedną politykę, roboczo trzy strzały z terminem ważności.
- Bazowa deterministyczna kolejność odpowiedzi: krwawienie -> błyskawica/przewodzenie; chłód -> ogień/szok termiczny; podpalenie -> ziemia/wybuch żaru. Bez warunku pocisk zachowuje zwykły żywioł.
- Jeżeli kilka warunków zachodzi jednocześnie, UI wyjaśnia tę kolejność. Nie wybieraj losowo nowej reakcji co klatkę.
- Zmień właściwy składnik obrażeń pocisku według opisanej konwersji, zachowując budżet przed należnymi premiami wybranego żywiołu. Nie dopisuj całego dodatkowego pocisku każdego koloru.
- Premie, penetrację i odporność rozlicz dla rzeczywistego typu raz. Zdefiniuj kolejność z ogólnymi konwersjami, w tym Przemianą w parę, tak aby nie powstały zależne od kolejności pętle.
- Przebijający pocisk może udzielić innej odpowiedzi kolejnej ofierze. Każdy cel i pierwotny strzał zachowują swoje limity reakcji i trafień.
- Cecha nie nadaje automatycznie wszystkich statusów: korzysta z tych, które gracz i otoczenie legalnie przygotowali.
- Zapewnij widoczny startowy wybór wspólnego narzędzia nakładającego przydatny status, np. kompatybilnej Mroźnej strzały. Późniejsze wyposażenie może rozwinąć ranienie do krwi.
- Dodatkowe pociski dziedziczą zużycie pojedynczego ręcznego ładunku cechy, zamiast każdy otrzymać nowy pełny zapas trzech strzałów.
- VFX przy kontakcie pokazuje wybrany żywioł. Nie zostawiaj sprzecznego koloru całego trafienia.
- Buildy: przygotowanie chłodu i odpowiedź ogniem; fizyczny łuk z krwawieniem i przewodzeniem.
- Test: wielostatusowy cel, kilka ofiar w linii, pusta ofiara, odporności, konwersja, rozszczepienie i brak rekurencji reakcji.

## 13. Dostępność buildów i kompletność treści

Dla każdej z 30 cech przygotuj dwie konkretne konfiguracje możliwe do złożenia w tej grze:

1. Rdzeń dostępny na starcie lub z powszechnych, gwarantowanych narzędzi przygotowania.
2. Rozwinięty nietypowy build wykorzystujący co najmniej dwie dalsze synergie.

Używaj prawdziwych id przedmiotów, perków i skilli oraz ich rzeczywistych wymagań. Samo wpisanie „młot + czary” nie jest dowodem legalnego wyposażenia.

Nie narzucaj na sztywno jedynego zestawu. Zapewnij zgodny podstawowy wybór i wyjaśnij zależności. Potrzebne zwykłe narzędzia pozostają zwykłymi skillami dostępnymi także innym klasom; tylko klasowy sposób ich użycia jest wyłączny.

W szczególności:

- Arsenał ma legalny drugi zestaw od startu i umie go zapisać.
- Niezgodna para ma legalne dwie bronie, a ekran pokazuje, kiedy wymaganie nie jest spełnione.
- Sąd przysięgi działa już z jedną aurą; druga daje rozwinięcie, nie warunek podstawowy.
- Prąd głębin ma dostępny wybór zwykłej strefy.
- Krusząca skorupa potrafi użyć naturalnej osłony, bez obowiązkowego rzadkiego dropu.
- Wcielenie i Chimera mają dostęp do zwykłego startowego przywołania.
- Arkana oręża pozwalają wyposażyć zwykły czar do warunkowego użycia.
- Strzała zaklęcia ma wybór kompatybilnego czaru, a Strzała odpowiedzi — narzędzia do przygotowania statusu.
- Otwarta tętnica korzysta z bazowej premii egzekucji Zabójcy.
- Oko ciszy ma zastosowanie z własnymi pociskami przeciw celowi bez ostrzału.

Nie rozwiązuj braku narzędzia przez przyznawanie ukrytego czwartego zwykłego slotu lub losowych darmowych przedmiotów po starcie. Ewentualne rozszerzenie listy startowych opcji jest jawne, deterministyczne, objęte UI i testem.

## 14. Grafika, animacja i dźwięk

1. Utrzymaj gotyckie anime zgodne z istniejącą specyfikacją. Odróżnij cechy czytelną formą, a nie tylko zmianą koloru tego samego wybuchu.
2. Cecha ma pokazywać własne przygotowanie, działanie i zakończenie. Słabe i maksymalne naładowanie muszą być rozpoznawalne.
3. Nie zasłaniaj zapowiedzi wrogów i ich kolorów. Szczególnie sprawdź ściany, chimery, płaszcz, płomienie, równoległe czary i magazyn pocisków.
4. Czerwona linia wymaga widocznej rezerwacji paska PŻ, zmiennej ochrony i końca transu. Nie pokazuj stałego napisu „80%”, jeśli bieżąca krzywa daje 52%.
5. Wcielenie musi pokazać faktyczną formę i odpowiadający jej atak. Chimera musi wizualnie komunikować składniki.
6. Bastion ma zgodną widzialną i fizyczną geometrię. Cień Zabójcy ma czytelny związek z ofiarą. Stanowisko pokazuje faktyczne miejsce wystrzału.
7. Splecione bronie, unik z atakiem oraz Dwa takty nie mogą rozrywać chwytu, wykręcać nadgarstków i nagle skakać między pozami. Zweryfikuj także ruch nóg.
8. Nie zastępuj istniejących modeli bohaterów ani ich poprawionych stóp. Korzystaj z obecnych assetów i własnych, legalnych dodatków proceduralnych. Nie zlecaj odpłatnych generacji zewnętrznych w ramach rutynowego rozwiązania.
9. Dodaj spójny dźwięk przygotowania/gotowości/wyzwolenia tam, gdzie jest pomocny. Kopie i horda nie mogą emitować pełnej głośności tego samego dźwięku kilkadziesiąt razy na klatkę.
10. Materiały, shadery i zasoby muszą być dostępne również w buildzie gracza, bez zależności od wyłącznie edytorowych ścieżek.
11. Wspieraj niski i wysoki poziom detali. Ograniczenie VFX nie może usuwać hitboxów ani zmieniać wyników walki.

## 15. Boty i telemetria

### 15.1. Wybór i podejmowanie decyzji

- Rozszerz konfiguracje botów o jawny wybór cechy; domyślny wariant nie może oznaczać, że B pozostaje nieużywany.
- Rozpoznawaj koszt i stan cechy: przechowywany zasób, cel, drugi etap, dług, przełączanie broni, właściwy sługa, odpowiedni czar i zagrożenie po wygaśnięciu.
- Dostosuj ocenę ekwipunku/perków do wybranej cechy. Przykładowo regeneracja ma większą wartość dla Rozgrzeszenia, a bariery dla Kruszącej skorupy.
- Bot nie może zakładać legalności czarów tylko na podstawie klasy. Używaj tego samego API walidacji co gracz.
- Obsłuż podstawowe pętle wszystkich 30 wariantów w krótkich deterministycznych scenariuszach.
- Zachowaj wspólną zasadę picia tylko w bezpiecznej chwili. Nie poprawiaj wyników botów ukrytym leczeniem, darmowymi żetonami albo brakiem kosztu rezerwacji.
- W raporcie oddziel wynik mechaniki od jakości polityki bota. Nieudolne użycie nie jest automatycznie dowodem słabości zdolności.

### 15.2. Co mierzyć

Zapisuj id klasy, cechy, seed, build, czasy, liczbę prób i faktycznych użyć, przyczyny odrzucenia, koszt, czas działania, wytworzony/wydany/zmarnowany zasób i liczbę ograniczonych zdarzeń.

Dodaj metryki odpowiednie dla konkretnej mechaniki:

- garda: równoczesne ataki/bloki, zużyta wytrzymałość, przełamania;
- arsenał: zmiany, użycie obu zestawów, legalne kontynuacje serii;
- Łaska: leczenie, nadleczenie po limitach, magazyn, obrażenia wydania;
- Sąd: aktywne odwrócone aury, utracone własne buffy, czas na celu;
- Czerwona linia: rozkład PŻ, bieżąca redukcja, obrażenia przed/po warstwie, leczenie ograniczone limitem, zgony w transie i po jego końcu;
- Spalenie furii: oddane sekundy, żetony, opłacone umiejętności;
- dług: największe zadłużenie, źródła spłaty, czas blokady;
- płaszcz: mana na ochronę i czary, obrażenia przepuszczone przy braku many;
- ogień: faktyczny koszt własnych PŻ i Żar, liczba rzeczywistych rodzin podpaleń;
- woda: legalne reakcje i czas kontroli, przeniesienia pól bez dodania budżetu;
- powietrze: pochodzenie i liczba pocisków, zderzenia i naprężenia;
- ziemia: czas ściany, obejścia/zniszczenia, zużyta ochrona i wydane płyty;
- przywołania: składniki, czas wkładów, zarezerwowane miejsca, ataki form;
- Dwa takty: rzeczywisty czas nakładania akcji i koszty obu torów;
- wabik: faktycznie przekierowane ataki, powrót do celu;
- kradzież: id źródłowego ataku, jednorazowe wykonanie i odrzucenia;
- Łucznik: groty i wspólny budżet, rzeczywiste punkty wystrzału;
- Zabójca: trafienia przez cień, zablokowane duplikaty, bonus egzekucji poza zwykłym progiem;
- Łucznik arkanów: wybrany czar, opłata, dokładnie jedno uwolnienie, wybory żywiołu i legalne reakcje.

Pełne wielogodzinne serie botów wykonuj zgodnie z najnowszą dyspozycją autora w docs/PROGRESS.md. Ten prompt nie uchyla wcześniejszego wstrzymania takich serii. Krótkie testy zachowań i implementacja polityk nadal są częścią zadania. Przy wznowionych seriach stosuj ustalone ograniczenia pamięci, w szczególności najwyżej dwie kopie, jeżeli to nadal obowiązująca konfiguracja.

## 16. Testy: wspólna macierz i przypadki szczególne

### 16.1. Testy danych i wyboru

Obowiązkowe:

- dokładnie 15 obecnych klas, po dwa różne id wariantów, łącznie 30;
- każda cecha ma jednego prawidłowego właściciela;
- klasa bez specjalizacji także ma oba warianty; specjalizacje nie otrzymują dodatkowo wariantów rodzica;
- wybór, zmiana klasy, powrót, restart, wygląd, remapowanie i obsługa pada;
- RunFactory instaluje dokładnie jeden wariant i nie dodaje starej sygnatury obok niego;
- wybór A/B jest zachowany w profilu per klasa, nowym snapshocie, wyzwaniu i odtworzeniu;
- wczytanie starego profilu/snapshotu bez nowego pola oraz z dawną sygnaturą;
- obce, nieznane i zduplikowane id nie przyznają mocy innej klasy;
- pule sklepu, odblokowania, przedmioty i zwykłe skille nie rozdają cech;
- po celowanej migracji generator i assety mają zgodne definicje, bez resetowania niepowiązanej zawartości.

### 16.2. Matematyka i zasoby

Testuj prawdziwe własności mechanik, a nie jedynie zgodność stałych zapisanych w teście z tymi samymi stałymi konfiguracji.

- zero i częściowy zasób, maksymalne wartości, ułamkowy koszt oraz jednoczesne zdarzenia;
- pobranie/zwrot kosztu dokładnie raz, poprawne odnowienie, anulowanie przed i po wyzwoleniu;
- monotoniczność dynamicznej ochrony i pełna tabela Berserkera;
- zmiana maksymalnego życia/many, rezerwacji i wyposażenia podczas działania;
- podział wspólnego budżetu na wiele pocisków, celów, źródeł i sług;
- dokładnie jedno naliczenie skalowania i obrony na odpowiednim etapie;
- prawidłowe działanie z dodatnimi/zerowymi wartościami premii, odpornością i penetracją;
- brak pętli: leczenie -> Łaska -> leczenie, osłona -> płyty -> osłona, szał -> żetony -> darmowy szał, przechwycenie -> ponowne przechwycenie;
- granice zegara: koniec okna, ta sama klatka trafienia/śmierci, pauza i zmienny krok 30/60 kl./s.

### 16.3. Prawdziwy runtime

Dla każdej cechy wykonaj deterministyczny PlayMode z rzeczywistym graczem, kosztami, geometrią i przeciwnikiem. Brak próbek jest niezaliczonym pomiarem, nie wynikiem 0.

Uwzględnij pięć kontekstów:

| Kontekst | Wymagana obserwacja |
| --- | --- |
| Grupa zwykłych wrogów | Działanie obszarowe i ograniczenia liczby zdarzeń |
| Elita | Zachowanie odporności i rzeczywista nagroda za przygotowanie |
| Samotny boss | Przydatność bez zabójstw oraz brak naruszenia faz i nietykalności |
| Zagrożenie defensywne | Czytelny koszt, realna możliwość otrzymania obrażeń i śmierci |
| Mało zasobów / słaby build | Prawidłowa odmowa, plan powrotu do podstawowych akcji, brak martwego stanu |

Dla cech zależnych od położenia dodaj ruchliwego bossa, ścianę, krawędź areny i brak legalnego miejsca. Dla sterowania AI przetestuj realny wybór celu, zamiast sprawdzać samą obecność flagi.

### 16.4. Macierz szczególnych regresji — wszystkie 30

| Cecha | Minimalny test rozstrzygający |
| --- | --- |
| Walka w gardzie | Atak z przodu blokowany w czasie własnego ciosu; tył trafia; wytrzymałość może się skończyć |
| Arsenał weterana | Dwa zestawy zapisane i odtworzone; brak duplikacji oraz resetów po serii przełączeń |
| Rozgrzeszenie | Nadwyżka po limitach zasila Łaskę, a wydanie nie produkuje następnej Łaski |
| Sąd przysięgi | Wszystkie sześć aur ma prawdziwe odwrotne działanie i wraca bez podwójnego buffa |
| Czerwona linia | Krzywa sprzed ciosu, rezerwacja względem normalnego maksimum, leczenie i legalny zgon |
| Spalenie furii | Pozostały szał daje ograniczone żetony, płatność raz, bez obejścia wyposażenia |
| Niezgodna para | Dwa właściwe źródła trafienia i deterministyczna reakcja bez podwojenia ogólnych proców |
| Taniec bez przerwy | To samo cięcie trwa w uniku i może spudłować; brak nowych trafień z resetu trackera |
| Dług arkanów | Ujemne saldo, blokada, wszystkie źródła spłaty i zapis bez anulowania długu |
| Płaszcz many | Częściowa mana osłania tylko opłaconą część obrażenia |
| Żywy piec | Tylko faktycznie utracone własne PŻ dają Żar; immunitet nie daje darmowego paliwa |
| Wielki pożar | Różne źródła sumują się, a trzy kopie tego samego źródła pozostają jedną rodziną |
| Niezmienny szron | Kolejne legalne reakcje działają, ale kontrola i odporność bossa nie są omijane |
| Prąd głębin | Pole zmienia miejsce bez odnowienia czasu, tyknięcia początkowego i mocy |
| Oko ciszy | Przechwycony pocisk znika z zagrożeń i zostaje wypuszczony dokładnie raz |
| Zderzenie frontów | Rzeczywiste zderzenie, ograniczony tłum i naprężenie bez przesuwania odpornego bossa |
| Bastion ziemi | Rzeczywista blokada, obejście/zniszczenie przez AI i brak zamknięcia portali |
| Krusząca skorupa | Pozostała ochrona zostaje zużyta, a płyty dzielą jeden budżet i nie odnawiają osłony |
| Wcielenie | Trzy różne zestawy ataków, własne PŻ i pełny powrót modelu/ekwipunku |
| Chimera legionu | Prawdziwe składniki i czasy wkładów, rezerwacja miejsc, brak kopii armii |
| Arkana oręża | Warunkowy fokus/atrybut tylko właściciela, normalny koszt i brak podwójnego skalowania |
| Dwa takty | Dwie niezależne, rzeczywiste fazy i opłaty; poprawne przerwanie obu |
| Fałszywy świadek | AI faktycznie wykonuje następny dopuszczony atak w wabik i wraca do gracza |
| Kradzież sztuczki | Wzięty profil normalnego ataku działa raz jako gracz, bez statystyk i faz bossa |
| Gwoździe łowcy | Osadzanie i ręczne wyrwanie, ograniczony wspólny budżet salwy |
| Stanowisko strzeleckie | Pocisk naprawdę startuje z innego miejsca, ma poprawny dystans i tylko jedną kopię |
| Przygwożdżony cień | Ciało/cień współdzielą trafienie, a faza nietykalności chroni oba |
| Otwarta tętnica | Egzekucja działa ponad progiem bez zmiany prawdziwych PŻ i faz AI |
| Strzała zaklęcia | Wybór skilla z pada, opłata raz i jedno uwolnienie mimo dodatkowych pocisków |
| Strzała odpowiedzi | Jeden pocisk odpowiada różnym celom, z prawidłowym typem i budżetem reakcji |

### 16.5. Kombinacje i obciążenie

Zrób celowaną macierz interakcji, a nie nierealne wyliczenie wszystkich kombinacji 30 cech między sobą — jedna postać wybiera jedną.

Każdy wariant sprawdź z odpowiednimi istniejącymi składnikami: Echo, Kaskada, rozszczepienie, przebicie, rykoszet, Odruch, Riposta, Zemsta, Żniwiarz, Osłona, Ofiara krwi, Słup ofiarny, konwersje, kradzież życia, reakcje i moce przedmiotów. Niezgodność musi mieć jawną przyczynę.

Wspólne mechanizmy danych testuj parametrycznie, a najbardziej ryzykowne połączenia uruchom w prawdziwym silniku. Nie zastępuj pomiaru walki samą kontrolą liczby obiektów.

Zbierz koszt klatki, alokacje, maksymalną liczbę bytów, zajętość magazynów i liczbę zdarzeń wtórnych. Test tłumu musi wykryć wzrost kwadratowy przy zderzeniach oraz wybuch łańcuchów. Porównuj kontrolne scenariusze na tych samych seedach, a zaobserwowane ograniczenia opisuj liczbami.

### 16.6. Uruchamianie i dowody

Standardowe polecenia, dopiero przy zamkniętym edytorze:

~~~powershell
python tools/run_tests.py EditMode
python tools/run_tests.py PlayMode window
~~~

Do opt-in galerii, zgodnie z aktualnym runnerem:

~~~powershell
$env:TURRIS_SHOT_DIR = "TestResults/class-features/shots"
python tools/run_tests.py PlayMode gfx
~~~

Zrzuty interfejsu i nawigację pada wykonuj w trybie window lub właściwym działającym edytorze, ponieważ OnGUI i WaitForEndOfFrame mają znane ograniczenia w batchmode. Zmienną galerii usuń po użyciu z własnej sesji, jeżeli wpływałaby na dalsze testy.

Wyniki kieruj do wydzielonego TestResults/class-features lub równoważnego katalogu, bez nadpisywania cudzych raportów. Zachowaj XML/logi, parametry, seedy i rzeczywiste metryki.

Pokaż i obejrzyj:

- przygotowanie A/B, podsumowanie, fokus pada i dynamiczny HUD;
- co najmniej jeden dowód widocznego działania każdego z 30 wariantów;
- dodatkowe sekwencje dla Berserkera, chimer, Wcielenia, ściany, przenoszonych stref, cienia i obu działań równoległych;
- chwyt broni i poprawną lokomocję dla zmienianych zestawów/form;
- powrót do normalnego modelu i stanu po końcu efektu.

Dla cech mało widocznych w jednej klatce dołącz sekwencję/krótki zapis i telemetrię pokazującą przyczynę oraz skutek. Sam świecący materiał nie jest dowodem działania.

## 17. Kolejność wykonania i kontrola zakresu

1. Audyt aktualnego kodu, źródeł danych, zmian w repozytorium i starych zapisów. Zapisz konkretny plan oraz tabelę 30 wariantów ze stanem.
2. Dane, identyfikatory, wybór, profil, RunFactory, snapshot i wyłączność. Uruchom testy tej warstwy przed dużą migracją.
3. Wspólne kontrakty zasobów, pochodzenia trafień i runtime'u. Nie zmieniaj wyników klas, które jeszcze nie otrzymały nowych danych.
4. Wdróż kompletną pionową ścieżkę Czerwonej linii: wybór, ryk, rezerwacja, zmienna skóra, HUD, zapis, animacja i testy.
5. Wdrażaj pozostałe grupy cech w kontrolowanych porcjach. Każda porcja obejmuje rzeczywistą mechanikę, UI, politykę bota i dowód, a nie same definicje.
6. Uzupełnij najbardziej wymagające nowe reguły: arsenał, równoległe działania, przenoszenie pól, przechwytywanie, fizyczne ściany, formy, chimery, wabiki, skradzione ataki i cień.
7. Wykonaj celowaną migrację wszystkich 15 klas i nowych assetów. Zweryfikuj zgodność treści generowanej i zapisanej.
8. Sprawdź 30 startowych konfiguracji i ich rozwinięcia, wspólne pieczęcie, obronę oraz źródła obrażeń.
9. Dokończ oprawę, wszystkie testy, krótkie scenariusze botów, zrzuty i kontrolę w buildzie gracza, jeśli dostępny workflow pozwala go wykonać.
10. Zaktualizuj README, docs/PROGRESS.md i raport implementacji. Dopiero wtedy podsumuj ukończoną pracę.

Tabela postępu ma odróżniać: dane, runtime, wybór/zapis, sterowanie, oprawę, test matematyczny, test w grze, dowód obrazu i legalny build. Jeden checkbox „gotowe” bez tych dowodów nie wystarcza.

Jeśli pojawi się rzeczywista blokada, ukończ niezależne części i podaj dokładnie brakującą czynność. Brak dostępu do długiej serii botów albo chwilowy brak zamkniętego edytora nie usprawiedliwia pominięcia pozostałych mechanik. Jednocześnie nie deklaruj przejścia testu, którego nie udało się uruchomić.

## 18. Ostateczne kryteria odbioru

Zadanie jest ukończone dopiero wtedy, gdy:

- wszystkie 15 klas ma dwie prawdziwe, różne cechy i istnieje 30 działających wariantów;
- gracz wybiera jedną na starcie, bez kosztu, a wybór jest widoczny i trwały przez run;
- klawiatura, mysz, pad, remapowanie, ponowny start i zmiana wyglądu działają;
- nowy wybór i stare zapisy nie przyznają jednocześnie obu cech lub starej trzeciej sygnatury;
- specjalne reguły pozostają niedostępne innym klasom i poza zwykłym sklepem;
- zwykłe bronie, skille i pieczęcie zachowują swobodę oraz mają jawne kompatybilności;
- Berserker ma połączoną Czerwoną linię i dynamiczną Żelazną skórę, prawidłową rezerwację i rzeczywistą możliwość śmierci;
- każda cecha ma legalny rdzeń od początku i co najmniej jeden pokazany nietypowy rozwinięty build;
- cienie, ściany, magazyny, formy i stanowiska faktycznie zmieniają walkę, a nie tylko render;
- nie ma pętli darmowych zasobów, wielokrotnego liczenia obrony, klonowania ekwipunku ani nieskończonych wyzwalaczy;
- wszystkie wymagane testy mają prawdziwe wyniki, a nie pominięcie z powodu braku próbek;
- obrazy i sekwencje zostały obejrzane, chwyty i nogi zweryfikowane, a oprawa nie zasłania zagrożeń;
- zmiany są opisane wraz z pomiarami, ograniczeniami, aktualnymi parametrami i ścieżkami dowodów;
- pliki Hell Unleashed, niezwiązane assety oraz cudze zmiany pozostały nienaruszone; commit/push wykonano tylko po ewentualnej odrębnej prośbie.

W końcowej odpowiedzi podaj konkretnie: co działa, które testy i dowody to potwierdzają, gdzie jest raport i jakie realne ograniczenia pozostały. Nie nazywaj wdrożenia pełnym, jeżeli któregokolwiek z 30 wariantów nadal brakuje.
