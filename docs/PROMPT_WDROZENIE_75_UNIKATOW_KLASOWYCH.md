# Prompt wdrożeniowy: 75 unikatów otwierających nowe buildy

Data: 2026-10-31. Projekt: Turris Damnatorum.

Status: szczegółowy zakres przyszłego wdrożenia. Ten plik jest promptem dla wykonawcy, nie potwierdzeniem zaimplementowania przedmiotów.

## 1. Zadanie

Wdróż wszystkie 75 przedmiotów opisanych w tym dokumencie: po pięć dla każdej z 15 klas. Zachowaj pierwsze trzy pomysły każdej klasy zaakceptowane w rozmowie i dodaj dwa oznaczone jako NOWY. Zakres to 45 wcześniejszych oraz 30 nowych unikatów, wraz z rzeczywistymi mocami, zdobywaniem, ekwipunkiem, opisami, zapisem, oprawą, botami i testami.

Nie kończ na samych assetach, statystykach, ikonach, rejestrze mocy ani kilku najłatwiejszych przedmiotach. Wszystkie 75 ma rzeczywiście zmieniać walkę. Autor pozwala na odważne przemiany skilli, nowe sposoby dostarczania obrażeń i silne, celowe synergie.

Unikat ma otwierać nowy sposób gry lub istotne odgałęzienie istniejącego buildu. Samo „+30% obrażeń” nie zastępuje opisanej mocy. Zwykły przedmiot może być lepszy statystycznie; unikat wygrywa szczególną regułą.

Przygotowując ten prompt, nie uruchamiaj jego wdrożenia. Dopiero zadanie zlecające wykonanie niniejszego promptu obejmuje modyfikacje kodu gry.

## 2. Zależność od cech klasowych i granice decyzji

Przeczytaj docs/PROMPT_WDROZENIE_30_CECH_KLASOWYCH.md. To powiązana specyfikacja 30 wybieranych cech, a nie dowód, że są już w kodzie.

- Najpierw sprawdź ich rzeczywisty stan. Dokończ potrzebne zależności zgodnie z tamtym promptem lub wdrażaj cechę i jej przedmioty jako pełny, zgodny etap. Nie udawaj cechy samą nazwą i warunkiem zawsze zwracającym true.
- Gracz nadal wybiera jedną z dwóch cech klasy na start podejścia. Przedmiot nie przyznaje drugiej cechy i nie przenosi cudzego klasowego prawa.
- Bazowe mechaniki, np. Furia, Żądza krwi, Pobożność i Zasadzka, działają według specyfikacji klas.
- Powiązanie przedmiotu z klasą oznacza projektowany kierunek synergii. Zwykłe moce zmieniające wspólne skille mogą działać na innych klasach, jeżeli te legalnie używają odpowiedniej broni/skilla.
- Moce modyfikujące wyłączną cechę są aktywne tylko przy wybraniu właściwej cechy jej rzeczywistego właściciela. Sama możliwość założenia przedmiotu lub wyglądu danej klasy nie wystarcza.
- Nie wprowadzaj globalnego zakazu używania innych broni i skilli przez klasę.
- Zachowaj trzy zwykłe sloty. Przedmiot może przyznać zwykły skill do kolekcji, ale jego używanie nadal wymaga legalnego slotu, kosztu i wyposażenia.
- Nie przywracaj starych sygnatur jako darmowych dodatkowych skilli tylko dlatego, że przypominają efekt przedmiotu.
- Zasady Czerwonej linii i Żelaznej skóry Berserkera pozostają: mniej rzeczywistego życia zwiększa redukcję podczas transu; nie dodawaj stałego 80% obok nowej krzywej ani ukrytej nieśmiertelności.

Wartości liczbowe podane dalej są hipotezami startowymi. Możesz stroić liczby na podstawie pomiarów. Zmianę jakościową, usunięcie przedmiotu albo zastąpienie jego mocy samym bonusem statystycznym trzeba uzgodnić z autorem.

## 3. Kontekst repozytorium i bezpieczne wykonanie

Najpierw przeczytaj AGENTS.md, docs/PROGRESS.md, README.md oraz powiązany prompt cech. Użyj skilla hack-and-slash-design i jego materiałów o przedmiotach, synergiach, walce i testowaniu. Zachowaj aktualne instrukcje autora.

- Projekt: C:\GIT\Gry\Turris_Damnatorum.
- Unity 6000.6.3f1: C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe.
- Kod: Assets/TurrisDamnatorum/Scripts; testy: Assets/TurrisDamnatorum/Tests.
- UI, komentarze i dokumentacja po polsku. Id techniczne są stabilne i niezależne od nazwy wyświetlanej.
- Hell Unleashed pozostaje wyłącznie do odczytu. Nie zmieniaj jego kodu, zasobów ani katalogów źródłowych.
- Nie używaj TurrisSetup.RunBatchForce bez odrębnej zgody. Potrzebna jest celowana, idempotentna migracja treści.
- Nie wykonuj commita ani pusha bez prośby autora. Zachowaj cudze i wcześniejsze zmiany.
- CLI Unity uruchamiaj dopiero przy zamkniętym edytorze; poproś autora o jego zamknięcie, gdy jest to potrzebne. Nie zamykaj go samodzielnie.
- Jeżeli używasz działającego MCP, potwierdź dataPath i właściwą instancję, a nie kopię bota.
- Nowa broń musi mieć zamknięty chwyt palców i kciuka w spoczynku, biegu, uniku i atakach. Obie dłonie broni dwuręcznej są obowiązkowe.
- Zachowaj naprawy anatomii, stóp, chwytu i nowe zestawy ruchów topora/młota. Nie regeneruj wszystkich bohaterów.
- Zmiany wizualne wymagają obejrzanych zrzutów. Materiały muszą działać też w buildzie.

## 4. Fakty architektoniczne do sprawdzenia przed kodowaniem

Odszukaj aktualne odpowiedniki:

- Runtime/Data/ItemDefinition.cs: kind, twoHanded, weight, wymagania, modifiers, effects, grantedSkill oraz modele.
- Runtime/Stats/Equipment.cs: ItemInstance, uid, jakość, EquipmentSet, zwalnianie drugiej ręki i legalne sloty.
- Runtime/Run/HellEconomy.cs: ItemRarity, IsUnique, Roll, cena, jakość, afiksy i źródła łupu.
- Runtime/Core/DefaultContent.HellEconomy.cs i DefaultContent.HellWeapons.cs: generowanie istniejącej zawartości.
- Runtime/Run/FloorShop.cs, SoulShop.cs i rzeczywista ścieżka dropów w GameRoot: oferty, zakupy i zbieranie.
- Runtime/Combat/LootPickup.cs: łup w świecie oraz jego automatyczne zbieranie po piętrze.
- Runtime/Run/RunState.cs, RunSnapshot.cs, Meta/PlayerProfile.cs: ekwipunek, indeks definicji, wyzwania i zapis.
- Runtime/Player/PlayerCombat.Gear.cs i pozostałe części PlayerCombat: istniejące moce, koszty, leczenie i trafienia.
- Runtime/Stats/WeaponFamilies.cs i BuildSnapshot.cs: klasyfikacja oraz skalowanie nowych broni.
- Runtime/Seals/HuSkillShape.cs, HuCast.cs, HuSkillCaster.cs, HuWorld.cs i wykonawcy pocisków/stref: przemiany i pochodzenie.
- Runtime/Combat/StatusEffects.cs oraz Servant.cs: budżety ran i rzeczywiste zasady sług.
- Runtime/UI, Runtime/Animation i Tests/PlayMode/Bots: prezentacja, sterowanie, animacje, polityki i telemetria.

W stanie projektu z chwili przygotowania promptu IsUnique rozpoznaje przede wszystkim pochodzenie HU, a Roll potrafi obniżyć nieuznany „unikat” do zwykłej rzadkości. Nowe natywne przedmioty muszą być obsługiwane jawnie. Nie fałszuj huSourceId, aby ominąć ten problem.

FloorShop nie sprzedaje obecnie unikatów; wpadają one z łupów. Nie dodawaj ich przypadkowo do zwykłych ofert przez niepełną klasyfikację. Nie przywracaj usuniętego ekranu darmowej nagrody po piętrze.

## 5. Dane i trwałość

1. Dodaj natywny rejestr unikatów Turris, niezależny od generowanych katalogów HU. Każda karta poniżej ma osobne id definicji i jednoznaczny profil mocy.
2. Dane obejmują: rzadkość Unique, slot, bazę broni, wymagania, wagę, kierunek statystyk, stałą moc, parametry, zgodne skille/cechy, źródła łupu, próg piętra i opis.
3. Moc jest gwarantowana dla danej definicji. Losowanie dotyczy dozwolonych wartości statystyk/jakości, nie tego, czy w ogóle otrzymasz obiecaną przemianę.
4. Poziom jakości nie mnoży liczby kopii, pojemności rekurencji, dozwolonych torów akcji, liczby posiadanych cech i liczby aktywnych legend. Rozdziel parametry skalowalne od strukturalnych.
5. Nie udawaj wszystkich przemian jednym nieopisanym floatem PassiveEffect. Użyj typowanych danych i istniejącego wykonawcy skilli, z kontrolowanymi rozszerzeniami.
6. Przedmiot jest definicją współdzieloną; magazyny, ładunki, znaki i rozgrzane ciała należą do runtime'u konkretnego właściciela i egzemplarza.
7. Egzemplarz ma trwałe id również w arsenale i snapshotach. Odtwarzanie nie może tworzyć kolizji uid, klonów przedmiotów i niewłaściwego przypisania magazynu.
8. Indeks zapisów obejmuje wszystkie 75 definicji. Testuj plecak, oba pierścienie, drugą rękę, broń w locie i oba zestawy Wojownika.
9. Stare profile i przedmioty wczytują się bez utraty zawartości. Nowe pole ma bezpieczną wartość domyślną; nie zmieniaj kolejności serializowanych enumów.
10. Ten sam unikat założony dwa razy nie dubluje jego strukturalnej mocy. Statystyki egzemplarzy stosuj zgodnie z jawną regułą; wybierz jeden aktywny profil legendy, a nie dwie rozbieżne wersje przemiany.
11. Zdjęcie, sprzedaż i zmiana wariantu w nowym podejściu zamykają odpowiednie kontrolery. Już opłacony pocisk może dokończyć swój zapisany budżet, ale nie generuje nowych ładunków zdjętego przedmiotu.
12. Magazyny nie odnawiają się przy otwarciu ekwipunku. Snapshot nie zeruje długu, kosztów zdrowia i zaciągniętych zobowiązań.

Legalne sloty: MainHand, OffHand, Head, Body, Hands, Belt, Feet, Ring1/Ring2 i Amulet. Kołczan w tym katalogu jest przedmiotem pasa, nie nowym slotem. Grymuar zajmuje drugą rękę i nie działa równocześnie ze zwykłą bronią dwuręczną.

Różdżka w lewej ręce jest wyjątkiem konkretnego unikatu Rębajły, a nie globalną zmianą FitsSlot. Nowe młoty jednoręczne nie stają się automatycznie ciężką bronią w rozumieniu skilli wymagających dwuręcznego uzbrojenia.

## 6. Kontrakt przemian, budżetów i sterowania

### 6.1. Kolejność

Ustal jeden deterministyczny pipeline:

~~~text
bazowy skill/atak -> legalne wyposażenie i wybrana cecha
-> zgodne formy/pieczęcie -> strukturalna przemiana unikatu
-> budżet i koszt -> zgodne wsparcia/konwersje w ustalonym etapie
-> rzeczywiste wykonanie -> obrona -> wynik -> dozwolone zdarzenia wtórne
~~~

Nie traktuj powyższego jako polecenia przeniesienia istniejących obron w niepoprawne miejsce. Udokumentuj mapowanie do rzeczywistego pipeline gry. Każda konwersja i premia ma zostać naliczona raz.

Przemiany zmieniające ten sam aspekt mają jawną zgodność. Najpierw szukaj sensownej kompozycji; jeżeli dwa efekty są faktycznie rozłączne, pokaż konflikt w UI. Kolejność zakładania przedmiotów nie ustala wyniku.

### 6.2. Zasady budżetu

- Kopia, odłożony cios, pocisk, rana i strefa zachowują właściciela, źródło, identyfikator pierwotnej ręcznej akcji oraz etap naliczenia budżetu.
- Odróżniaj przeniesienie już istniejących obrażeń od nowej porcji. Mocne legalne combo jest pożądane; wielokrotne wydanie tej samej porcji jest błędem.
- Pobranie budżetu i utworzenie jego nowej postaci jest atomowe. Nie może pozostać jednocześnie na ofierze, w mine, w broni i w kolejce.
- Jeśli budżet był już po obronie pierwszej ofiary, nie naliczaj automatycznie tej samej obrony ponownie. Zasady na drugim celu muszą być jawne i testowane na różnych odpornościach.
- Przenoszone rany nie odzyskują pełnego pierwotnego czasu. Pole przesunięte, zwinięte lub rozłożone nie dostaje nowej początkowej eksplozji.
- Tyknięcia, reakcje i kopie nie stają się nowymi ręcznymi aktywacjami. Salwa może legalnie trafić wiele razy, ale nie ładuje wiele razy całego magazynu przeznaczonego na jeden strzał.
- Zachowaj legalne istniejące różnice między kontaktem i eksplozją. Nie „naprawiaj” łańcuchów przez wyłączenie wszystkich ciekawych dodatkowych trafień.
- Koszt życia, rezerwacja i samoodpalenie znacznika nie są trafieniem wroga. Nie uruchamiają Zemsty, cierni i furii z otrzymania ciosu.
- Efekt spłacający dług nie pożycza go ponownie. Wydanie osłony nie odtwarza tej osłony. Widmo nie tworzy kolejnego widma.
- Dla każdej mocy określ współczynnik kradzieży życia, dostęp do statusów, reakcji i dalszych wyzwalaczy. Domyślnie wtórny magazynowany budżet nie leczy ponownie.

### 6.3. Wejście gracza

Zachowaj mapowanie zwykłych akcji i G/View. Moce używają wskazanych w kartach ataków, skilli oraz istniejącego namierzania. Nie dodawaj obowiązkowego nowego klawisza na padzie dla każdego unikatu.

Wybór skilla do mocy i ustawienia kolejności wykonaj w ekwipunku. Czasowe konteksty, np. wybór czaru do strzały, nie mogą przypadkowo rzucać skilla dwa razy. Wszystkie wejścia respektują remapowanie.

Podwójne źródła akcji, np. Dwa takty, zachowują własne timery i koszty. Unikat nie resetuje ich przez zmianę reprezentacji.

### 6.4. Ograniczenia świata

Rzeczywiste kolizje, zasięg połączenia, fazy bossa, przerwania i śmierć pozostają skuteczne. Teleport nie daje dodatkowej niewrażliwości. Źródło geometryczne i właściciel trafienia są różnymi danymi.

Śmierć sprząta runtime bez nowych darmowych ataków. Pauza zatrzymuje zegary. Po piętrze usuń tymczasowe byty; zobowiązania zasobów rozlicz zgodnie z trudnością i promptem cech.

## 7. Statystyki i parametry startowe

W kartach podano kierunki statystyk, nie listę obowiązkowych maksymalnych premii. Dobierz umiarkowane pakiety, aby zwykły/rzadki przedmiot pozostawał konkurencją statystyczną.

Przed ukończeniem implementacji każda moc ma w konfiguracji konkretne: współczynniki, czasy, promienie, limity, koszty, tagi zgodności, zasady anulowania i poziomy jakości. Nie zostawiaj „części obrażeń” bez wartości w kodzie i tooltipie.

Hipotezy wspólne: pojedynczy znak zwykle 3–6 s; mały magazyn zwykle 1–3 jednostki; najwyżej kilka aktywnych dodatkowych bytów na właściciela. To punkty startowe do testów, nie jednakowy limit narzucony wszystkim mechanikom.

Legendarna moc może zmieniać szybkość i liczbę aktywacji, ale całkowity wynik porównuj przy tych samych seedach, kosztach i czasie walki. Nie strój balansu wyłącznie na manekinie.

## 8. Zdobywanie i ekonomia

- Zachowaj istniejące rozróżnienie: Unique z właściwych źródeł łupu, bez przypadkowej sprzedaży w zwykłej puli FloorShop.
- Dodaj natywne unikaty do prawdziwej ścieżki dropów, odpowiedniego indeksu oraz zbierania w świecie. Definicja istniejąca tylko w narzędziu deweloperskim nie jest ukończonym przedmiotem.
- Dla każdego id zapisz próg piętra, źródła elita/boss, wagę, powiązanie tematyczne oraz wartość. Sprawdź istniejącą ekonomię 30 pięter.
- Nie wpuszczaj 75 pozycji jako jednakowo rozcieńczonej listy bez analizy. Rozdziel źródła tematycznie, zastosuj umiarkowaną preferencję dla buildu i pozostaw możliwość zaskakujących znalezisk spoza klasy.
- Przy przedmiocie wymagającym konkretnej cechy dopuszczaj niepasujący drop jako świadomą rzadką możliwość, a nie większość zawartości. Tooltip wyjaśnia uśpienie mocy.
- Daj w katalogu informację, gdzie szukać przedmiotu. Dla odkrytych unikatów przygotuj wskazanie jednego poszukiwanego przedmiotu z jawną preferencją w odpowiedniej przyszłej puli. Nie zmieniaj przy tym po cichu szansy rzadkości Unique i nie dodawaj nowej waluty.
- Ustal parametry tej preferencji na podstawie symulacji; samo pięciokrotne zwiększenie wagi w olbrzymiej puli nie jest dowodem rozsądnej dostępności.
- Zachowaj zasadę: podstawowy build działa bez unikatu. Nie dawaj wszystkich 75 za darmo na starcie i nie uzależniaj działania cechy od rzadkiego dropu.
- Sprzedaż, odkup, zmiana piętra, śmierć, restart i przerzuty nie tworzą dodatniej pętli waluty lub ładunków.
- Ulepszanie statystyk unikatowej broni musi mieć jawną drogę zgodną z aktualnymi zasadami. Nie zostawiaj jej zbyt słabą w połowie wieży tylko dlatego, że stara ścieżka wyklucza Unique.

## 9. Format katalogu i status nowych pomysłów

Każda klasa ma pozycje 01–05. Pierwsze trzy zachowują zaakceptowane nazwy i główną moc. Pozycje 04–05 są nowymi propozycjami włączonymi do tego promptu na polecenie autora.

Oznaczenia warunku: A/B = właściwa wybrana cecha z promptu klas; „ogólny” = moc wspólnego skilla lub legalnej akcji, bez udzielania klasowej cechy.

Każda karta obejmuje id, slot, warunek, kierunek statystyk, właściwą przemianę, cenę/budżet, przykład buildu i test rozstrzygający. Do końcowego rejestru dodaj opisy dostępne graczowi i konkretne parametry.

## 10. Wojownik — class_knight

### W01 — Pawęż ostatniego zakonnika

- Id: item.unique.turris.warrior.last_pavise. Slot: tarcza/OffHand. Warunek: Walka w gardzie A.
- Staty: mana, obrona magiczna, umiarkowana stabilność. Nie maksymalizuj jednocześnie wszystkich odporności.
- Moc: podczas okna A koszt ręcznego bloku pobieraj najpierw z many. Wytrzymałość pozostaje dla ataków i uników.
- Cena: mana konkuruje z czarami. Jeżeli pokrywa tylko część kosztu, resztę pobierz z wytrzymałości, bez podwójnej opłaty i darmowego bloku za ułamek many.
- Build: różdżka + tarcza; miecz + Inteligencja + odzysk many po obronie. Zachowaj normalny kąt i ataki nieblokowalne.
- Test: frontalny blok w czasie własnego ataku, częściowa mana, cios w plecy i przełamanie gardy.

### W02 — Pas dwóch przysiąg

- Id: item.unique.turris.warrior.two_oaths. Slot: Belt. Warunek: Arsenał weterana B.
- Staty: udźwig, wytrzymałość, umiarkowane obrażenia fizyczne.
- Moc: pierwszy celny ręczny atak po legalnej zmianie zestawu wywołuje jeden słabszy ślad uderzenia poprzedniej broni, z jej żywiołem/statusami.
- Cena: przed ponownym uzbrojeniem śladu trzeba rzeczywiście użyć obu zestawów. Spam G nie ładuje mocy. Poprzednia broń daje zapisany pakiet, nie wszystkie swoje pasywne premie drugi raz.
- Build: sztylet -> młot, dwa żywioły, długi zasięg -> ciężkie wykończenie.
- Test: brak duplikowania śladów i ładunków przy zmianie, pudle, wczytaniu i przerwaniu.

### W03 — Hełm dzwonnika

- Id: item.unique.turris.warrior.bellringer. Slot: Head. Warunek: ogólna legalna riposta bronią.
- Staty: obrażenia riposty, okno parowania, umiarkowana obrona.
- Moc: fizyczna riposta zmienia się w falę przed postacią. Premia riposty wzmacnia ten sam budżet obszarowy.
- Cena: mniejsze skupione obrażenia na jednym celu; fala ma zasięg, przeszkody i rozpoznawalną zapowiedź.
- Build: parowanie w hordzie, miecz/tarcza, ciężkie wykończenia kilku wrogów.
- Test: jeden opłacony atak, wielu legalnych odbiorców, bez dodatkowej normalnej riposty pod spodem.

### W04 — Karwasze pozostawionej warty [NOWY]

- Id: item.unique.turris.warrior.left_watch. Slot: Hands. Warunek: ogólny świadomy blok lub parowanie.
- Staty: wytrzymałość, odzysk po obronie, obrona fizyczna.
- Moc: ręczna obrona zapisuje na kilka sekund punkt warty. Następna ręcznie użyta ofensywna technika rodzi się w tym punkcie zamiast przy Wojowniku.
- Cena: jedna warta, normalny koszt/odnowienie techniki, utrata lokalnej ofensywy przy własnym ciele. Blok bierny nie uzbraja punktu.
- Build: odejście po bloku i zdalne Rozpłatanie; Młynek pozostawiony przy bramie. Wybierz zgodne techniki, bez teleportowania samego gracza i bez autonomicznego powtarzania.
- Test: geometria, kierunek, wygaśnięcie i brak drugiego rzutu z postaci; A/B nie są nadawane przez karwasze.

### W05 — Medalion pustego frontu [NOWY]

- Id: item.unique.turris.warrior.empty_front. Slot: Amulet. Warunek: nowa Szarża hu_charge, z normalnym wymaganiem broni.
- Staty: wytrzymałość, regeneracja wytrzymałości, udźwig.
- Moc: Szarża staje się kontrolowanym odwrotem po linii celowania; broń/tarcza osłania przednią stronę poruszającego się taranu, raniąc ścigających na trasie.
- Cena: zamiana ofensywnego doskoku na cofnięcie; brak drugiego pełnego końcowego wybuchu, normalny koszt i ograniczenia terenu. Bez dodatkowej niewrażliwości.
- Build: dwuręczna broń z przygotowanym mocnym ciosem, taktyczna tarcza i zwabianie w linię.
- Test: kierunek rzeczywiście od celu, granica areny, kolejne ofiary trafione raz i brak działania ze skasowaną starą skill_charge.

## 11. Paladyn — class_hu_paladin

### P01 — Relikwiarz głodnego świętego

- Id: item.unique.turris.paladin.hungry_reliquary. Slot: Amulet. Warunek: Rozgrzeszenie A.
- Staty: regeneracja życia, siła leczenia, umiarkowane życie.
- Moc: wydana Łaska nie tworzy fali; zasila kilka kolejnych ręcznych trafień broni/podstawowych pocisków.
- Cena: jeden podzielony budżet, określona liczba ataków i termin ważności; pudło zużywa przypisany ładunek. Uwolnienie nie leczy i nie produkuje Łaski.
- Build: Paladyn-łucznik; szybka broń i nadleczenie.
- Test: salwa i eksplozja nie kopiują całej Łaski na każdy pocisk, a fala pierwotna nie uruchamia się równolegle.

### P02 — Mitra rozdwojonej litanii

- Id: item.unique.turris.paladin.divided_litany. Slot: Head. Warunek: Sąd przysięgi B.
- Staty: mana, regeneracja many, umiarkowany zasięg aur.
- Moc: każdą aktywną odwróconą aurę można przypisać innemu celowi; dostarcz czytelny wybór na padzie.
- Cena: rezygnacja ze skupienia obu osłabień na jednym celu, jeśli wybrano rozdział; ta sama rezerwacja i czas, bez nowej trzeciej aury.
- Build: kontrola dwóch elit, wspieranie sług i walka na dystans.
- Test: dwa niezależne cele, śmierć jednego i poprawny powrót właściwej aury bez resetu drugiej.

### P03 — Habit pustego klasztoru

- Id: item.unique.turris.paladin.empty_monastery. Slot: Body. Warunek: własne aktywne aury i sługi.
- Staty: mana, regeneracja, umiarkowana ochrona.
- Moc: wspierające aury obejmują własne sługi. Zapał i Męstwo rzeczywiście zmieniają ich ataki; obrona/leczenie mają sens tylko na sługach z prawdziwym zdrowiem.
- Cena: właściciel dostaje słabszą część wsparcia. Jedna aura nie mnoży się przez liczbę nośników. Sąd nie daje jednocześnie utraconego własnego buffa.
- Build: Paladyn z grymuarem i przywołaniami; leczenie śmiertelnej bestii.
- Test: faktyczne tempo/obrażenia sługi, brak sumowania kopii i poprawne uśpienie nieprzydatnej ochrony na normalnym słudze bez PŻ.

### P04 — Karwasze dzwonów pokutnych [NOWY]

- Id: item.unique.turris.paladin.penitent_bells. Slot: Hands. Warunek: Aura świętego ognia i ręczne parowanie.
- Staty: okno parowania, ogień, umiarkowana regeneracja.
- Moc: aura przestaje stale parzyć. Jej ograniczony budżet narasta z czasem aktywności; udane parowanie uwalnia go jedną falą ze środka aury.
- Cena: brak biernego DPS i konieczność udanej obrony. Czasowe magazynowanie ma limit; liczba wrogów nie przyspiesza ładowania.
- Build: parujący Paladyn; w Sądzie fala może wyjść z oznaczonej ofiary, według rzeczywistego środka aury.
- Test: jeden budżet czasu, puste parowania, Sąd i brak równoległego zwykłego tyknięcia.

### P05 — Pastorał długiej pokuty [NOWY]

- Id: item.unique.turris.paladin.long_penance. Slot: broń jednoręczna/buława. Warunek: kwalifikowana ofensywa wręcz i ręczne leczenie.
- Staty: umiarkowana baza broni, leczenie, mana.
- Moc: część obrażeń własnych technik bronią zamienia się w ograniczoną „winę” na ofierze. Opłacony ręczny czar leczenia albo użycie flaszki rozlicza zgromadzoną winę.
- Cena: opóźniona ofensywa, slot leczenia lub zużyta flaszka. Samo zadanie obrażeń rozliczenia nie leczy, nie odkłada nowej winy i nie generuje Łaski.
- Build: Paladyn-sędzia przeplatający ataki i leczenie. Przy pełnym PŻ płatna czynność może wyzwolić winę, ale nie tworzy fikcyjnego przywrócenia życia.
- Test: zachowanie sumy budżetu, brak pętli leczenie/Łaska/wina oraz jeden wyrok mimo AoE leczenia.

## 12. Berserker — class_hu_berserker

### B01 — Kolczuga głodnych blizn

- Id: item.unique.turris.berserker.hungry_scars. Slot: Body. Warunek: aktywna Czerwona linia A.
- Staty: normalne maksymalne życie, regeneracja, umiarkowana obrona.
- Moc: rzeczywisty wrogi cios wręcz redukowany przez Żelazną skórę wywołuje ograniczony krwawy odwet, skalowany normalnym maksimum PŻ właściciela.
- Cena: osłabione własne podstawowe ataki; ograniczona częstotliwość odwetu. Nie kopiuj surowych obrażeń bossa.
- Build: Berserker cierniowy. Zachowaj dynamiczną skórę, rezerwację i możliwość śmierci; własny koszt PŻ nie wywołuje odwetu.
- Test: prawdziwy cios, zmienny procent skóry, wiele ofiar i samouszkodzenie bez rekurencji.

### B02 — Różaniec gniewnych duchów

- Id: item.unique.turris.berserker.angry_spirits. Slot: Amulet. Warunek: Spalenie furii B.
- Staty: wytrzymałość, fizyczne obrażenia, umiarkowane życie.
- Moc: powstałe żetony stają się krótkotrwałymi widmami wojowników z kontrolowanym atakiem aktualnej broni, zamiast finansować skille.
- Cena: zużyty szał oraz brak darmowych opłat. Widma nie kopiują strukturalnych legend, nie tworzą nowych żetonów ani nie liczą się wielokrotnie w limicie przywołań.
- Build: plemienny dowódca z ciężką bronią i aurą; ręczna walka zapewnia kolejne okna szału.
- Test: maksymalna liczba, czas, jeden zapisany profil broni i brak pełnego efektu podstawowego Spalenia obok widm.

### B03 — Topór kroczącej rzezi

- Id: item.unique.turris.berserker.walking_slaughter. Slot: topór dwuręczny. Warunek: własne krwawienia.
- Staty: fizyczna baza, krwawienie, umiarkowana mobilność.
- Moc: rozliczenie ran zależy od ruchu właściciela wokół ofiary w ograniczonym zasięgu. Sam ruch przeciwnika nie jest już głównym przyspieszeniem.
- Cena: stanie spowalnia rany; obrót kamery, teleport i sztuczne kołysanie nie produkują dodatkowego budżetu. Rana ma skończoną pozostałą sumę.
- Build: mobilny Berserker, uniki i ataki po uniku; legalne użycie przez innego krwawiącego wojownika nie daje Żądzy krwi.
- Test: ruch po łuku, oddalenie, stanie, teleport i przejęcie już częściowo rozliczonej rany.

### B04 — Karwasze przykutego rzeźnika [NOWY]

- Id: item.unique.turris.berserker.chained_butcher. Slot: Hands. Warunek: rzeczywisty skill rzutu bronią, np. Ciężki rzut, z jego wymaganiami.
- Staty: wytrzymałość, obrażenia broni, umiarkowane obrażenia postawy.
- Moc: rzucona broń zatrzymuje się na pierwszej ofierze. Kolejny opłacony mocny atak pięścią przyciąga gracza ku wbitemu orężowi; po dojściu odzyskuje broń i kończy sekwencję.
- Cena: broń naprawdę niedostępna do powrotu; brak normalnego bloku nią. Ruch ma kolizję, zasięg i nie daje niewrażliwości.
- Build: rzucający Berserker przechodzący przez walkę pięściami do ciężkiego zwarcia. Na śmierci celu umożliw bezpieczny zwrot bez darmowego kończącego trafienia.
- Test: lot, zakotwiczenie, pięść, przeszkoda, martwa ofiara i brak klonu broni w arsenale.

### B05 — Pierścień nierozładowanego szału [NOWY]

- Id: item.unique.turris.berserker.held_rage. Slot: Ring. Warunek: legalny szał i ładowany mocny atak.
- Staty: obrażenia ciężkie, wytrzymałość, umiarkowane życie; bez dodatkowej szybkości szału.
- Moc: bonus tempa szału zostaje zastąpiony możliwością odkładania podczas ładowania do kilku kolejnych ciężkich cięć. Puszczenie ataku wyzwala przygotowaną serię.
- Cena: każde cięcie konsumuje rzeczywisty odcinek szału i koszt ataku. Tych samych sekund nie można potem wydać Spaleniem furii na żetony/widma.
- Build: powolny Berserker ładowany pod wielkie okno; ryzyko przygotowania przy niskim życiu zamiast szybkich lekkich ciosów.
- Test: pułap serii, przerwanie, koszt, utracony bonus tempa i rozliczenie równoczesnego G.

## 13. Rębajło — class_hu_dual

### R01 — Rękawice podwójnego sekretu

- Id: item.unique.turris.dual.double_secret. Slot: Hands. Warunek: Niezgodna para A.
- Staty: mana, szybkość, umiarkowana moc czarów.
- Moc: pozwól legalnie użyć jednoręcznej różdżki jako drugiej broni tylko w odpowiednim wyposażeniu i wariancie. Splot to cięcie oraz krótki wystrzał.
- Cena: pocisk płaci manę, lewa ręka nie daje drugiego ostrza. Budżet i tempo uwzględniają oba narzędzia; bez pełnej darmowej salwy na każdy pakiet cięcia.
- Build: miecz + różdżka, fizyczna rana + żywioł.
- Test: CanEquip i runtime zgodne, powrót do normalnych reguł po zdjęciu rękawic, brak ukrytej trzeciej ręki.

### R02 — Buty niedokończonego kroku

- Id: item.unique.turris.dual.unfinished_step. Slot: Feet. Warunek: Taniec bez przerwy B.
- Staty: ruch, wytrzymałość, umiarkowana skuteczność broni.
- Moc: unik z podtrzymanym atakiem odkłada część cięcia w miejscu startu. Następny mocny atak uwalnia przygotowany ślad.
- Cena: mniejszy bieżący budżet, mały limit śladów i czas ważności. Ten sam unik nie odnawia śladu tej samej fazy trafienia.
- Build: taniec tworzący pułapki z odłożonych cięć i świadome wyzwolenie.
- Test: geometria śladu, jeden root, pudło mocnego ataku, wygaśnięcie i brak nowych i-frame'ów.

### R03 — Pas żonglera ostrzami

- Id: item.unique.turris.dual.blade_juggler. Slot: Belt. Warunek: legalna lewa jednoręczna broń wręcz.
- Staty: szybkość, wytrzymałość, umiarkowane krwawienie.
- Moc: mocny atak wysyła lewą broń na ograniczoną orbitę, podczas której prawa ręka walczy normalnie.
- Cena: lewa nie atakuje w ręce i nie daje premii wymagających faktycznych dwóch broni. Broń wraca raz, bez kopii. Różdżka z R01 nie staje się automatycznie rzucanym ostrzem.
- Build: orbitujący sztylet i miecz; ruchliwe przecinanie tłumu.
- Test: animacja lewej ręki, statusy, globalne proci, orbitowanie i zakończenie po śmierci/zmianie piętra.

### R04 — Szabla wspak pisanych ran [NOWY]

- Id: item.unique.turris.dual.reverse_wounds. Slot: broń jednoręczna/szabla. Warunek: Cięcie krzyżowe i dwie legalne bronie.
- Staty: umiarkowana baza, krwawienie, szybkość.
- Moc: Cięcie krzyżowe staje się otwarciem i odpowiedzią. Drugi cios czeka na następne rzeczywiste ręczne trafienie przeciwną ręką, a jego profil korzysta z tej ręki.
- Cena: okno odpowiedzi wygasa; koszt skilla i pierwszy etap nie wracają po niewykonanym domknięciu. Drugi pakiet Niezgodnej pary w tym samym root nie udaje nowej akcji.
- Build: lekka ręka otwiera, wolniejsza zamyka; dwie odmienne rany/żywioły.
- Test: prawidłowa ręka, normalny koszt raz, kolejność statusów i brak wielokrotnego wyzwalania drugiego cięcia.

### R05 — Amulet zbłąkanej klingi [NOWY]

- Id: item.unique.turris.dual.wandering_blade. Slot: Amulet. Warunek: legalny Młynek i rzeczywista broń wręcz.
- Staty: wytrzymałość, fizyczne obrażenia, umiarkowany obszar.
- Moc: Młynek pozostawia główną broń wirującą w punkcie rozpoczęcia. Kolejny opłacony mocny atak przywołuje wir po ziemi w stronę właściciela.
- Cena: główna broń jest w tym czasie niedostępna; lewa walczy sama, a bez niej zostają pięści. Czas wiru i suma trafień nie odnawiają się przy przyciąganiu.
- Build: pułapka wirująca i zwabianie; dwa odrębne krążące ostrza z R03, jeżeli każde pochodzi z innej ręki.
- Test: własność obu broni, ograniczony czas, powrót po kolizji i brak rzucenia tego samego egzemplarza dwa razy.

## 14. Mag — class_mage

### M01 — Weksel lichwiarza

- Id: item.unique.turris.mage.moneylenders_note. Slot: Ring. Warunek: Dług arkanów A.
- Staty: mana, umiarkowana skuteczność podstawowej broni, odzysk po trafieniu.
- Moc: podstawowe ręczne trafienia bronią spłacają dług według kontrolowanego przelicznika. Dotyczy też legalnej różdżki, ale jeden strzał ma wspólny limit spłaty.
- Cena: zwykła regeneracja many nie spłaca zadłużenia. Podczas długu nie może ominąć go, tworząc osobny dodatni zapas.
- Build: miecz + grymuar i spłata w zwarciu; kosztowna magia przeplatana ostrzałem. Flaszki i inne źródła odzysku mają jawną politykę, zgodną z opisem.
- Test: częściowa spłata, pełna spłata, salwa, wybuch, brak trafienia i brak ujemnego długu zamienianego w darmową manę.

### M02 — Szata zamkniętego źródła

- Id: item.unique.turris.mage.sealed_source. Slot: Body. Warunek: Płaszcz many B.
- Staty: mana, obrona magiczna, umiarkowane życie.
- Moc: mana rzeczywiście wydana na ochronę zostawia ograniczone runy. Następna ręczna nova wydaje je na dodatkowe wyładowanie, wraz z normalnym rzutem.
- Cena: mana była faktycznie zużyta; brak refundu i duplikacji run. Koszt czaru, samouszkodzenie i rezerwacja nie udają opłaconej ochrony.
- Build: Mag-bastion i przygotowana odpowiedź obszarowa; współpraca z minami M03 wymaga podziału tej samej runicznej porcji.
- Test: źródło wydania many, limit, wiele trafień jednocześnie i jedna konsumpcja przy wielocelowej umiejętności typu nova.

### M03 — Grymuar uśpionych kręgów

- Id: item.unique.turris.mage.sleeping_circles. Slot: OffHand/fokus. Warunek: kompatybilne ręczne umiejętności typu nova.
- Staty: mana, moc czarów, umiarkowany promień.
- Moc: nova zostaje umieszczona w wybranym punkcie jako uzbrajana mina, wybuchająca po wejściu wroga. Zachowaj właściwy żywioł i zgodne formy.
- Cena: pełny koszt, odnowienie, limit min i czas uzbrojenia. Tracisz natychmiastowy wybuch przy sobie; koniec czasu nie daje darmowego powtórzenia całej mocy.
- Build: kilka żywiołów i kontrola ścieżek; Dług finansujący przygotowanie pułapek.
- Test: realne wykrycie przeciwnika, pierwotny impuls raz, Przekaźnik i brak kopii budżetu przy przenoszeniu.

### M04 — Rękawice związanych zaklęć [NOWY]

- Id: item.unique.turris.mage.bound_spells. Slot: Hands. Warunek: trzy ręczne ofensywne czary.
- Staty: mana, moc czarów, umiarkowany zasięg.
- Moc: dwa różne czary w krótkim odstępie zostawiają punkty runiczne. Trzeci ręczny czar domyka połączenie i uwalnia odłożoną moc wzdłuż odcinka między nimi.
- Cena: część pierwszych dwóch budżetów jest wstrzymana; punkty mają krótki termin, a niewłaściwa kolejność traci przygotowanie. Nie ma trzeciej darmowej kopii pełnych czarów.
- Build: architekt linii, min i narożników; mieszanie kosztownego i taniego zaklęcia.
- Test: kolejność definicji skilli, jeden root na etap, brak ładowania kopiami, sensowna geometria i zachowanie sumy budżetu.

### M05 — Buty spóźnionej obecności [NOWY]

- Id: item.unique.turris.mage.late_presence. Slot: Feet. Warunek: własny unik i następny zgodny czar ofensywny.
- Staty: ruch, mana, regeneracja wytrzymałości.
- Moc: po uniku zostaje krótki ślad źródła. Następny ręczny czar wychodzi ze śladu, a nie z aktualnej pozycji Maga.
- Cena: tylko jedno zastępcze źródło, brak drugiej kopii, termin ważności. Nova ratunkowa wybuchnie w poprzednim miejscu; trzeba świadomie zarządzać pozycją.
- Build: mobilny pułapkarz, zmiana kąta pocisku, odejście od wcześniej przygotowanego miejsca walki.
- Test: kierunek, przeszkody, celowanie padem, normalny koszt i brak dodatkowej niewrażliwości albo resetu czasu czaru.

## 15. Mag ognia — class_hu_mage_fire

### F01 — Miech pożeracza stosów

- Id: item.unique.turris.fire.pyre_devourer. Slot: Amulet. Warunek: Piekielna kula i własne strefy ognia.
- Staty: ogień, mana, umiarkowany promień wybuchu.
- Moc: kula pożera kompatybilne własne pola napotkane w locie, przenosząc kontrolowaną pozostałą moc do końcowego wybuchu.
- Cena: zjedzone pola tracą odpowiedni czas/budżet. Nie zjadaj tej samej strefy wielokrotnie ani cudzych pól. Krąg o funkcji zaklinania pocisków ma osobną zgodność.
- Build: przygotowane pola -> skupione uderzenie w elitę; Mag kontrolujący teren tylko do momentu finishera.
- Test: trzy pola, częściowo zużyte pole, Echo, kilka kul i brak równoczesnego aktywnego budżetu w polu oraz kuli.

### F02 — Mankiety popielnych marionetek

- Id: item.unique.turris.fire.ash_puppets. Slot: Hands. Warunek: Wielki pożar B.
- Staty: ogień, czas podpalenia jako umiarkowany kierunek, mana.
- Moc: własne rodziny podpalenia na zabitej ofierze stają się wędrującymi płomieniami przenoszącymi pozostałe rany do nowego celu.
- Cena: krótsze podpalenia, globalny limit marionetek, brak odnawiania rany do pełna. Przeniesiona rodzina zachowuje tożsamość źródła.
- Build: ogniowy quasi-przywoływacz z kilkoma źródłami; marionetki nie uzyskują automatycznie całego niezależnego DPS normalnego sługi.
- Test: śmierć w tej samej klatce, normalny limit rodzin na nowej ofierze, brak stworzenia kolejnych marionetek z pustego budżetu.

### F03 — Młot pożyczonego słońca

- Id: item.unique.turris.fire.borrowed_sun. Slot: młot jednoręczny. Warunek: Żywy piec A.
- Staty: umiarkowana baza, ogień, życie.
- Moc: mocny cios zużywa Żar, tworząc palenisko na ofierze. Następne ręczne trafienia bronią rozliczają je porcjami iskier.
- Cena: Żar jest oddany tej sekwencji; czary nie uruchamiają iskier, a kolejne cele nie kopiują całego zapasu. Nie naliczaj też domyślnego pełnego wydania Żaru tego samego ataku.
- Build: młot + grymuar; życie i regeneracja wspierające bliską kuźnię.
- Test: koszt faktycznych PŻ Żywego pieca, jedno palenisko, rozliczenie i legalne wyposażenie fokusu w drugiej ręce.

### F04 — Szata odroczonego pogorzeliska [NOWY]

- Id: item.unique.turris.fire.deferred_wildfire. Slot: Body. Warunek: własne podpalenia przeciwników.
- Staty: ogień, życie, umiarkowana moc ciężkiego uderzenia.
- Moc: nowe własne podpalenia wrogów nie tykają na bieżąco. Ręczny mocny atak ściska pozostały zgromadzony budżet w lokalny słup ognia.
- Cena: opóźnienie i wymóg mocnego trafienia; rana ma pojemność/czas i nie rośnie przez ponowne odświeżanie tej samej mocy. Własne podpalenie Żywego pieca nadal działa normalnie.
- Build: powolne uderzenia w trzy rodziny Wielkiego pożaru; różdżka przygotowująca i rozliczająca spalanie.
- Test: brak podwójnych tyknięć, trzy rodziny, zmiana ofiary, wygaśnięcie i brak zmiany samopodpalenia.

### F05 — Diadem pieca pod niebem [NOWY]

- Id: item.unique.turris.fire.sky_furnace. Slot: Head. Warunek: Krąg żaru i podstawowy ostrzał.
- Staty: ogień, mana, umiarkowany obszar.
- Moc: podstawowy strzał oddany z własnego kompatybilnego Kręgu żaru staje się małym spadającym uderzeniem w celowany punkt. Zastępuje to zwykły przelot pocisku i standardowy dodatek zaklinania tego strzału.
- Cena: opóźnienie, ograniczony zasięg, utrata normalnego przebicia/rykoczetu w tej formie. Jedna opłacona akcja nie tworzy wielu pełnych meteorów.
- Build: nieruchome stanowisko artylerii; synergia z własnym kręgiem kosztem mobilności i jego ewentualnego zjedzenia przez F01.
- Test: miejsce oddania strzału, wyjście z kręgu, telegraph, salwa dzieląca budżet i brak podwójnego ognistego zaklinania.

## 16. Mag wody — class_hu_mage_water

### H01 — Korona zatopionego króla

- Id: item.unique.turris.water.drowned_king. Slot: Head. Warunek: Niezmienny szron A.
- Staty: życie, mróz, umiarkowana obrona.
- Moc: Szron działa w małym kręgu wokół postaci zamiast na odległym wskazanym celu. Warunek obejmuje legalne ofiary tylko, gdy pozostają blisko.
- Cena: oddajesz bezpieczne oznaczanie z daleka; nie zachowuj pełnych znaczników na wszystkich celach po ich wyjściu z kręgu.
- Build: wodny wojownik z jednoręczną bronią/grymuarem, ogniem i reakcjami; nadal normalne immunitety na kontrolę.
- Test: wejście/wyjście, kilka celów, zamrożenie prawdziwe i znamię reakcji bez zatrzymania bossa.

### H02 — Płaszcz wędrującej rzeki

- Id: item.unique.turris.water.wandering_river. Slot: Body. Warunek: Prąd głębin B.
- Staty: ruch, mana, umiarkowane żywiołowe obrażenia.
- Moc: prowadzone pola rozwijają się w wąską smugę wzdłuż przebytej drogi, rozdzielając tam swój czasowy budżet.
- Cena: dłuższa ścieżka rozcieńcza skupioną moc. Nie zachowuj jednocześnie pełnego oryginalnego pola i pełnej smugi.
- Build: rysowanie rzek ognia, mrozu i burzy; planowanie toru poruszania się.
- Test: suma tyknięć, zatrzymanie, ruch 30/60 kl./s, krótka/długa ścieżka i liczba segmentów.

### H03 — Grymuar niemego lodowca

- Id: item.unique.turris.water.silent_glacier. Slot: OffHand/fokus. Warunek: Nova wody oraz własne sługi.
- Staty: mróz, mana, umiarkowane wsparcie przywołań.
- Moc: nova zakłada na sługach lodowe kokony-amunicję. Następny legalny atak każdego nośnika uwalnia jego porcję budżetu Nova wody na ofierze.
- Cena: podział jednego budżetu, brak wybuchu przy graczu, czas ważności. Kokon nie jest fikcyjną barierą zdrowia normalnego nieśmiertelnego sługi.
- Build: wodny przywoływacz z ogarami/golemem przygotowujący reakcje.
- Test: pełna armia nie tworzy sześciu pełnych kopii Nova wody; śmierć/wygaśnięcie nośnika nie oddaje gratis pełnego zaklęcia.

### H04 — Obrączka wiecznego odpływu [NOWY]

- Id: item.unique.turris.water.eternal_ebb. Slot: Ring. Warunek: Fala przypływu i własne miny.
- Staty: mróz, mana, umiarkowany zasięg.
- Moc: fala zbiera własne zgodne miny na trasie i przenosi je do swego końca, zamiast odpychać wrogów. Zachowują uzbrojenie, termin i opłacony budżet.
- Cena: utrata zwykłego odrzutu, limit przenoszonych min i respektowanie kolizji. Nie odnawiaj pułapki przy każdym odpływie.
- Build: Mag-saper z minami, także nowymi z M03; oddzielne prawo dla min nie przyznaje Prądu głębin.
- Test: mina już uzbrojona, częściowo wygasła, aktywująca się podczas transportu i niesiona przez dwie fale.

### H05 — Różdżka słonego lustra [NOWY]

- Id: item.unique.turris.water.salt_mirror. Slot: różdżka jednoręczna. Warunek: Lodowy odłamek i własne pociski.
- Staty: mróz, moc czarów, umiarkowana mana.
- Moc: odłamek trafiający w legalne podłoże może pozostawić cienkie lodowe lustro. Następny własny zgodny pocisk odbija się od niego, zmieniając kierunek i uzgodniony składnik na mróz.
- Cena: odłamek wydany na przygotowanie zamiast normalnego trafienia; lustro jednorazowe, z limitem i terminem. Brak rozszczepienia całego pełnego budżetu przez każde odbicie.
- Build: ostrzał zza narożnika, układanie tras i zimne wersje własnych pocisków.
- Test: normalna odbicia, kamera/pad, przebicie, wrogie pociski bez niezamierzonego przejęcia i brak pętli luster.

## 17. Mag powietrza — class_hu_mage_air

### A01 — Flakon ostatniego oddechu

- Id: item.unique.turris.air.last_breath. Slot: Amulet. Warunek: Oko ciszy A.
- Staty: błyskawica, mana, umiarkowana penetracja.
- Moc: zgromadzone pociski stają się jednym przebijającym piorunem o kontrolowanej sumie budżetu.
- Cena: utrata szerokiego wachlarza; pudło marnuje całość. Nie twórz pełnego obrażenia każdego pocisku na każdej składowej wiązki.
- Build: snajper z przechwyconej lub przygotowanej własnej amunicji.
- Test: jeden, częściowy i pełny magazyn, kilka celów oraz normalizacja pocisku bossa.

### A02 — Rękawice burzowego kowala

- Id: item.unique.turris.air.storm_smith. Slot: Hands. Warunek: Zderzenie frontów B.
- Staty: błyskawica, umiarkowany odrzut i wytrzymałość.
- Moc: zgodne wolne własne pociski są punktami zderzenia dla odrzucanych wrogów. Zderzenie zużywa dany pocisk i uwalnia jedną kontrolowaną odpowiedź.
- Cena: poświęcony pocisk, konieczność ustawienia ruchu i brak powtórnego wydania jego budżetu przy wybuchu.
- Build: magiczny bilard, podmuch kierujący demona w kulę błyskawic.
- Test: realny kontakt, dwaj wrogowie w tym samym momencie i brak kwadratowego łańcucha pocisk/zderzenie.

### A03 — Peleryna bezwietrznej nocy

- Id: item.unique.turris.air.windless_night. Slot: Body. Warunek: Krok wichru.
- Staty: mana, ruch, umiarkowana moc czarów.
- Moc: skill nie przesuwa postaci, lecz kieruje i rozpędza własne zgodne pociski w obszarze podmuchu.
- Cena: utrata mobilności skilla; pocisk zmienia tor, ale nie odświeża trafień, mocy i całego czasu życia.
- Build: sterowanie wolnymi kulami, przygotowanymi salwami i lotem powrotnym.
- Test: jeden pocisk wielokrotnie popychany, kontrola kierunku, koszty i niezmienione zasady uniku.

### A04 — Różdżka pustych węzłów [NOWY]

- Id: item.unique.turris.air.empty_nodes. Slot: różdżka jednoręczna. Warunek: Łańcuch potępionych i własne pociski w świecie.
- Staty: błyskawica, mana, umiarkowany zasięg.
- Moc: łańcuch może zużyć skok na własny pocisk jako węzeł drogi do dalszego wroga. Węzeł nie jest nowym celem do zadania obrażeń.
- Cena: normalny limit skoków i odwiedzonych celów; pociski schowane w magazynie Oka nie są aktywnymi węzłami przed wypuszczeniem.
- Build: sieć własnego ostrzału przed dalekim uderzeniem; rzadko rozmieszczone cele i walka z odizolowanym bossem.
- Test: cykl węzłów, wielokrotna próba tego samego bossa, śmierć pocisku w locie i brak nowego łańcucha z każdego skoku.

### A05 — Buty ostatniego podmuchu [NOWY]

- Id: item.unique.turris.air.last_gust. Slot: Feet. Warunek: Nova powietrza.
- Staty: wytrzymałość, ruch, umiarkowana błyskawica.
- Moc: zamiast normalnego rozpychania nowa odrzuca właściciela w kierunku przeciwnym do głównego celu, tworząc kontrolowany odskok po wyzwoleniu.
- Cena: zamiana kontroli wroga na ruch siebie; normalny koszt, kolizja i brak dodatkowej niewrażliwości. Liczba trafionych nie mnoży dystansu.
- Build: dystansowy tancerz, strzał/nowa/odskok; działa z samotnym odpornym na odrzut bossem po poprawnym trafieniu.
- Test: trafienie, pudło, ściana za graczem, wiele celów i niezmienione zasady pól obrażeń.

## 18. Mag ziemi — class_hu_mage_earth

### E01 — Buty ruchomego bastionu

- Id: item.unique.turris.earth.moving_bastion. Slot: Feet. Warunek: Bastion ziemi A.
- Staty: obrona, wytrzymałość, umiarkowany ruch.
- Moc: zamiast jednej ściany powstają dwie krótsze boczne płyty prowadzone z postacią.
- Cena: odsłonięty przód/tył i brak trwałego zamknięcia przejścia. Płyty mają właściwe kolizje, trwałość i zdolność zniszczenia przez bossa.
- Build: szturm z bronią lub łukiem między ruchomymi osłonami.
- Test: obrót, przejście przez ciasny teren, teleport, zniszczenie jednej płyty i brak wypychania AI poza arenę.

### E02 — Pas grobowego moździerza

- Id: item.unique.turris.earth.grave_mortar. Slot: Belt. Warunek: Krusząca skorupa B.
- Staty: życie, ziemia, umiarkowana mana.
- Moc: mocny atak wyznacza punkt opóźnionego spadnięcia płyty zamiast wystrzału z postaci.
- Cena: ten sam skończony zapas, widoczna zapowiedź i możliwość ucieczki ofiary. Pocisk zwykłego ataku nie kopiuje całego ładunku na odłamki.
- Build: Mag ziemi z łukiem i osłoną; artyleria bariery.
- Test: rzeczywisty marker, wydatek płyty raz, ruchliwy cel i brak normalnej płyty pod spodem.

### E03 — Młot niewykopanego grobu

- Id: item.unique.turris.earth.undug_grave. Slot: młot jednoręczny. Warunek: Kamienne kolce.
- Staty: baza broni, ziemia, umiarkowana mana.
- Moc: kolce pozostają zapisane pod ziemią; mocny atak młota wyzwala przygotowane pęknięcia w dozwolonym zasięgu.
- Cena: pełny koszt przygotowania i brak natychmiastowego pierwotnego ciosu kolców. Ograniczona liczba znaków oraz termin.
- Build: jednoręczny młot + grymuar; geomanta-saper. Nie dawaj automatycznie prawa do technik dwuręcznej ciężkiej broni.
- Test: legalny fokus, normalny mocny atak, kilka linii, braki znaków i brak drugiego pełnego wypłacenia przygotowania.

### E04 — Napierśnik żywego kamieniołomu [NOWY]

- Id: item.unique.turris.earth.living_quarry. Slot: Body. Warunek: Kamienna osłona i Kamienne kolce.
- Staty: życie, obrona, umiarkowana ziemia.
- Moc: osłona jest przedstawiona jako skończone płyty tej samej łącznej ochrony. Faktycznie odłamane wrogim trafieniem części zostawiają odłamki; kolce zbierają je, poszerzając pokrycie swej linii.
- Cena: odłamki nie stanowią ponownej bariery; większe pokrycie nie mnoży pełnego trafienia na tej samej ofierze.
- Build: wojownik kamienia przyjmujący uderzenie i przebudowujący teren. Zużycie osłony Kruszącą skorupą nie tworzy równocześnie darmowych odłamków.
- Test: suma ochrony, częściowa płyta, zebrany odłamek raz i zgodność z B.

### E05 — Amulet rany pod stopami [NOWY]

- Id: item.unique.turris.earth.wound_underfoot. Slot: Amulet. Warunek: kwalifikowane źródła Pęknięcia pancerza.
- Staty: ziemia, fizyczne obrażenia, umiarkowane życie; zapewnij skromne podstawowe źródło kwalifikacji na wskazanym ataku.
- Moc: własne pęknięcie pancerza zostaje w małym obszarze ziemi, a nie przy ciele. Ofiary stojące w nim odczuwają ograniczone osłabienie.
- Cena: cel wychodzący z obszaru traci to źródło. Nakładające się pola nie omijają normalnego limitu osłabienia.
- Build: fizyczny geomanta, bronie i przywołania walczące w przygotowanym polu.
- Test: wejście/wyjście, dwie ofiary, limit pól, boss i współpraca ze zwykłym pęknięciem innego źródła.

## 19. Przywoływacz — class_hu_summoner

### S01 — Maska trzeciego głodu

- Id: item.unique.turris.summoner.third_hunger. Slot: Head. Warunek: Wcielenie A.
- Staty: życie, wytrzymałość, umiarkowana moc przywołań.
- Moc: zużycie drugiego własnego sługi innego rodzaju zastępuje mocny atak formy jego zdolnością. Ciało golema może zyskać ognisty atak chochlika.
- Cena: dwa realnie zużyte przywołania, bez sumowania pełnych czasów. Nie nadaje autonomicznej Chimery B.
- Build: hybrydy form, tank z dystansowym mocnym atakiem albo szybki ogar z ciężkim zakończeniem.
- Test: wybór drugiego rodzaju, rzeczywiste dwa ubytki armii, termin formy, animacja i poprawne przywrócenie wyposażenia.

### S02 — Serce szóstego żebra

- Id: item.unique.turris.summoner.sixth_rib. Slot: Amulet. Warunek: Chimera legionu B.
- Staty: życie, regeneracja, umiarkowane wsparcie.
- Moc: chimera otrzymuje prawdziwe PŻ i staje się celem AI. Pozostały budżet wkładów zamienia się na skończone, konfigurowalne zdrowie/zdolności zamiast zwykłego zniknięcia wyłącznie po czasie.
- Cena: może zostać zabita; normalne leczenie i odbudowa wymagają zasobów. Nie zachowuj jednocześnie niewrażliwości bezżyciowego sługi.
- Build: opiekun jednej bestii, leczenie i aury. Dodaj legalny, czytelny wybór chimery jako odbiorcy zgodnego leczenia, bez nowego obowiązkowego klawisza.
- Test: prawdziwa śmierć, AI, leczenie, rezerwacja limitu sług i brak pełnego odnowienia PŻ przez ponowne scalenie.

### S03 — Rękawice kościanego dyrygenta

- Id: item.unique.turris.summoner.bone_conductor. Slot: Hands. Warunek: własne samodzielne sługi.
- Staty: szybkość podstawowa, wytrzymałość, umiarkowane wsparcie przywołań.
- Moc: sługi oddają swobodny rytm ataku; każde ręczne trafienie właściciela wydaje komendę kolejnemu legalnemu słudze.
- Cena: przerwa w atakowaniu spowalnia armię. Jedna salwa/eksplozja nie daje nieograniczonej kolejki, a tempo ma pułap zależny od legalnych ataków sług.
- Build: szybka broń wręcz i ręcznie prowadzona armia; Wcielenie może dowodzić pozostałymi sługami według normalnego pochodzenia akcji.
- Test: kolejka, cel, przerwanie, brak ruchu oraz zgodność pojedynczej chimery z aktywnymi częściami jej ataków.

### S04 — Grymuar otwartych pysków [NOWY]

- Id: item.unique.turris.summoner.open_maws. Slot: OffHand/fokus. Warunek: własny sługa i ręczny zgodny czar pociskowy.
- Staty: mana, moc czarów, umiarkowane wsparcie przywołań.
- Moc: kolejny ręczny pocisk czaru wychodzi z wybranego własnego sługi, a kolejne rzuty mogą rotować przez armię. Jeden rzut daje jeden pocisk/budżet zgodny z własną definicją, nie pełną kopię na każdego sługę.
- Cena: oprócz normalnej many wykorzystuje mały odcinek pozostałego wkładu sługi. Dla śmiertelnej chimery S02 koszt rozlicz w jej skończonym budżecie/PŻ, bez darmowego braku czasu.
- Build: mobilna artyleria, ogary dostarczające czar z boku i duża chimera jako kontrolowany punkt wyjścia.
- Test: frakcja, źródło geometryczne, przeszkody, zniknięcie sługi i pojedyncza opłata.

### S05 — Pas dziedziczonej bestii [NOWY]

- Id: item.unique.turris.summoner.inherited_beast. Slot: Belt. Warunek: naturalnie kończące się własne przywołanie.
- Staty: mana, regeneracja many, umiarkowane przywołania.
- Moc: naturalny koniec sługi zostawia jeden krótki ślad dziedziczenia. Następny opłacony summon innego rodzaju otrzymuje ograniczoną właściwość poprzednika: ogień chochlika, ranę ogara albo zachwianie golema.
- Cena: jeden dziedziczony profil/status, bez dodatkowego ciała, pełnego drugiego zestawu ataków lub czasu. Ślad zastępuje poprzedni.
- Build: rotacja rodzajów i żywiołów. Zużycie sługi Wcieleniem, scalenie, ręczne odwołanie i śmierć nie udają naturalnego końca dla darmowego dziedziczenia.
- Test: kolejność, wygaśnięcie śladu, ten sam rodzaj, B i brak dziedziczenia strukturalnych unikatów.

## 20. Mag bitewny — class_hu_battlemage

### T01 — Bandaże runicznego zapaśnika

- Id: item.unique.turris.battlemage.runic_wrestler. Slot: Hands. Warunek: Arkana oręża A i puste dłonie.
- Staty: Siła, mana, umiarkowana moc czarów.
- Moc: pięści stają się legalnym orężem-fokusem A. Zgodne ofensywne czary przyjmują postać uderzeń: wybuchający prosty, uderzenie obrotowe nowej itd.
- Cena: rezygnacja z obu trzymanych przedmiotów, zwarcie i prawdziwe koszty czarów. Moc bez pustych dłoni jest uśpiona, zamiast dawać niewidzialny drugi fokus.
- Build: magiczny pięściarz. Zdefiniuj mapowanie wszystkich kompatybilnych kształtów i odrzucenia, nie udawaj każdej umiejętności identycznym wybuchem.
- Test: walidacja wyposażenia, skalowanie pięści, fazy i chwyt po powrocie do broni.

### T02 — Miecz niedokończonej runy

- Id: item.unique.turris.battlemage.unfinished_rune. Slot: miecz jednoręczny. Warunek: Dwa takty B.
- Staty: baza miecza, mana, umiarkowana moc czarów.
- Moc: czar rzucony w oknie B zostawia jeden punkt trafienia. Następny mocny atak tnie odcinek do pieczęci zamiast normalnego łuku.
- Cena: ograniczony zasięg połączenia, jeden zapamiętany punkt, prawdziwy koszt i z góry określony kierunek. Jeden czar wielocelowy nie daje wielu pełnych linii.
- Build: szermierz geometrii; grymuar i krzyżujące się miejsca trafienia.
- Test: linia, przeszkoda, źródło, normalny atak po wygaśnięciu i niepodwojona podstawowa geometria.

### T03 — Zbroja runożercy

- Id: item.unique.turris.battlemage.rune_eater. Slot: Body. Warunek: własne Pęknięcie pancerza.
- Staty: życie, umiarkowana moc czarów, obrona.
- Moc: kwalifikowane własne pęknięcie osłabia odporności magiczne zamiast obrony fizycznej. Daj jawną listę typów i przeliczniki.
- Cena: fizyczna część ofensywy traci normalne osłabienie; respektuj granice odporności i czas statusu.
- Build: cięcia przygotowujące czary; hybryda naprawdę potrzebująca obu części.
- Test: kilka źródeł pęknięcia, boss, wygasanie i brak naliczenia jednocześnie pełnej fizycznej oraz magicznej wersji.

### T04 — Obrączka parującej runy [NOWY]

- Id: item.unique.turris.battlemage.parrying_rune. Slot: Ring. Warunek: udane ręczne parowanie i wybrany zgodny czar.
- Staty: obrażenia riposty, mana, okno parowania.
- Moc: następna legalna riposta zostaje zastąpiona skonfigurowanym czarem, korzystającym z kontrolowanej premii riposty.
- Cena: normalna mana, gotowość skilla i wymagania. Parowanie tylko uzbraja okazję; nie rzuca za darmo czaru. Bez czaru/opłaty wykonaj zwykłą ripostę według jawnej reguły.
- Build: parujący Mag z mieczem i grymuarem, inwestujący w ripostę jako oś magiczną.
- Test: raz zapłacony spell, jedno okno, normalne fazy bossa i brak fizycznej riposty równolegle.

### T05 — Grymuar złamanej różdżki [NOWY]

- Id: item.unique.turris.battlemage.broken_wand. Slot: OffHand/fokus. Warunek: jednoręczna różdżka.
- Staty: moc czarów, mana, umiarkowana wytrzymałość.
- Moc: lekki podstawowy atak różdżki staje się cięciem przywołanym ostrzem jej żywiołu. Mocny pozostaje normalnym ostrzałem.
- Cena: utrata szybkiego dystansu, zajęta druga ręka, realny zasięg i fazy melee. Nie wystrzeliwuj przy lekkim ataku niewidzialnego zwykłego pocisku.
- Build: Mag bitewny walczący magicznym ostrzem z własnej różdżki; legalne działanie Klingi zaklęć raz.
- Test: melee/projectile, przywracanie many zgodne z faktycznym trafieniem, powrót po zdjęciu i brak zgodności z dwuręcznym kosturem.

## 21. Łotrzyk — class_rogue

### L01 — Maska fałszywego króla

- Id: item.unique.turris.rogue.false_king. Slot: Head. Warunek: Fałszywy świadek A.
- Staty: ruch, życie, umiarkowana skuteczność zasadzki.
- Moc: prawdziwy wskazany przeciwnik może zostać obiektem oszustwa. Dopuszczone ataki oszukanych wrogów rzeczywiście mogą go zranić.
- Cena: limit celów/akcji oraz znormalizowany budżet wrogiej ofensywy. Nie kopiuj ogromnego mnożnika późnego bossa. Samotny boss korzysta z normalnego wabika, a nie z wymuszonego zabijania siebie.
- Build: prowokator i pole pełne pułapek.
- Test: rzeczywisty wybór AI, frakcje, fazy, aggro po końcu oraz prawidłowe nagrody bez wielokrotnych zabójstw.

### L02 — Rękawice zawodowego oszusta

- Id: item.unique.turris.rogue.professional_cheat. Slot: Hands. Warunek: Kradzież sztuczki B.
- Staty: wytrzymałość, umiarkowane umiejętności, ruch.
- Moc: jednorazowa skradziona akcja staje się uzbrajaną pułapką; kontakt ofiary wykonuje jej dopuszczony profil.
- Cena: czas, miejsce i sprowadzenie celu; brak natychmiastowej normalnej sztuczki. Ten sam profil nie istnieje równocześnie w magazynie i na ziemi.
- Build: saper używający narzędzi wrogów; różne profile mają czytelny kształt pułapki.
- Test: melee/pocisk/obszar, czas ważności, Przekaźnik i nielegalny profil bossowej fazy.

### L03 — Pas niewypałów

- Id: item.unique.turris.rogue.duds. Slot: Belt. Warunek: Bomba siarkowa, Sidła lub legalny cios w plecy.
- Staty: umiarkowane obrażenia umiejętności, wytrzymałość, ogień.
- Moc: bomba przykleja się do ofiary. Własne Sidła albo ręczny cios z pleców uruchamiają ładunek.
- Cena: brak natychmiastowej eksplozji, limit przyklejonych bomb i termin. Kolejna eksplozja nie jest automatycznie nowym ciosem z pleców.
- Build: wabik, przygotowanie i detonacja; ruchliwy Łotrzyk wybierający kolejność.
- Test: wejście w Sidła, zaskoczenie, śmierć nosiciela, faza bossa i brak łańcucha pełnych ładunków.

### L04 — Sztylet niedokończonej ucieczki [NOWY]

- Id: item.unique.turris.rogue.unfinished_escape. Slot: sztylet jednoręczny. Warunek: Krok cienia oraz wskazana kompatybilna bomba/mina w zwykłym slocie.
- Staty: baza, ruch, umiarkowane krwawienie.
- Moc: poprawny Krok cienia może zostawić w punkcie startu jedną opłaconą pułapkę ze skonfigurowanej umiejętności.
- Cena: oba koszty i odnowienia, brak darmowej kopii. Jeżeli drugi skill nie jest gotowy, Krok działa normalnie z czytelną informacją, bez połowicznego pobrania ceny pułapki.
- Build: doskok w ofiarę i przygotowany odwrót przez punkt startu; kombinacja z niewypałem ma jawną postać.
- Test: dwa źródła kosztu, przerwanie, legalne miejsce, normalny cel doskoku i brak spawnu przy odrzuconej akcji.

### L05 — Medalion sygnału z otchłani [NOWY]

- Id: item.unique.turris.rogue.abyss_signal. Slot: Amulet. Warunek: Rzut nożami i kompatybilne własne pułapki.
- Staty: wytrzymałość, umiarkowany zasięg, obrażenia technik.
- Moc: wybrane pułapki stają się zdalne. Ręczny nóż przecinający ich znacznik uruchamia przygotowany ładunek.
- Cena: zwykłe wejście wroga już nie detonuje tej wersji. Trzeba trafić sygnałem; pełny przygotowany budżet jest zużywany raz.
- Build: rzucający nożami saper i kontrolowane szeregi zapalników; nie wymaga nowej do detonacji.
- Test: geometria noża, kilka pułapek na trasie, Przekaźnik, brak odpalenia cudzych min oraz brak rekurencyjnego nowego noża z wybuchu.

## 22. Łucznik — class_hu_archer

### K01 — Rękawice tkacza cięciw

- Id: item.unique.turris.archer.bowstring_weaver. Slot: Hands. Warunek: Gwoździe łowcy A.
- Staty: szybkość, umiarkowany zasięg, wytrzymałość.
- Moc: groty łączą trafione cele liniami. Wyzwolenie rani przeciwników pomiędzy kotwicami; dozwolony ręczny grot w podłożu tworzy jeden punkt terenowy.
- Cena: utrata normalnego skupionego wyrwania, ograniczona liczba punktów/odcinków i jeden podzielony budżet.
- Build: sieć pułapek i przeciąganie przeciwników przez układ.
- Test: linia, przecięcie przez wiele colliderów jednej ofiary, śmierć kotwicy, powrót do podstawowego A po zdjęciu.

### K02 — Buty wygnanego strzelca

- Id: item.unique.turris.archer.exiled_shooter. Slot: Feet. Warunek: Stanowisko strzeleckie B.
- Staty: ruch, regeneracja wytrzymałości, umiarkowana obrona.
- Moc: kwalifikowany mocny strzał może zamienić pozycję gracza i stanowiska. Strzał ma jedno ustalone źródło sprzed zamiany; punkt pozostaje w starym miejscu właściciela.
- Cena: osobne odnowienie, legalne miejsca i brak dodatkowej niewrażliwości. Gdy zamiana niedostępna, nie blokuj zwykłego mocnego strzału.
- Build: dwa kąty ostrzału i kontrolowany odwrót.
- Test: koszt, kierunek, krawędź areny, zajęta pozycja, faza uniku i brak podwojenia wystrzału.

### K03 — Łuk grabarza

- Id: item.unique.turris.archer.gravediggers_bow. Slot: łuk dwuręczny. Warunek: podstawowe ataki tej broni.
- Staty: baza, przebicie, umiarkowana wytrzymałość.
- Moc: lekki jest pchnięciem strzałą w zwarciu, po którym strzała przelatuje przez ofiarę dalej. Mocny pozostaje tradycyjnym strzałem.
- Cena: utrata szybkiego podstawowego dystansu. Kontakt i dalszy lot mają jeden opisany budżet i rejestr, bez trafienia tej samej ofiary ponownie tuż po wylocie.
- Build: Łucznik w zwarciu, efektowe groty i przebijanie tylnego szeregu.
- Test: połączenie animacji z kontaktem, dalszy pocisk, wymóg dwóch rąk i zgodność Gwoździ tylko z kwalifikowanymi porcjami.

### K04 — Pas niedokończonej salwy [NOWY]

- Id: item.unique.turris.archer.unfinished_volley. Slot: Belt. Warunek: Salwa i ręczny mocny strzał.
- Staty: wytrzymałość, umiarkowane obrażenia technik, ruch.
- Moc: Salwa ustawia przygotowany rząd strzał w ziemi zamiast natychmiastowego wachlarza. Następny mocny strzał wskazuje kierunek ich jednorazowego wspólnego wystrzału.
- Cena: opłacona Salwa, czas uzbrojenia, limit przygotowań i osobny koszt mocnego strzału. Rząd nie strzela sam i nie zyskuje nowych strzał co klatkę.
- Build: stanowisko egzekucyjne, pułapki na szarżujących i świadome ustawienie linii.
- Test: oba wejścia, kierunek z pada, pierwotne źródło techniki zamiast sztucznego podstawowego strzału oraz wygaśnięcie.

### K05 — Kaptur odwróconego celownika [NOWY]

- Id: item.unique.turris.archer.reversed_sight. Slot: Head. Warunek: Deszcz strzał i podstawowy ostrzał.
- Staty: umiarkowany czas/obszar, wytrzymałość, szybkość.
- Moc: aktywny Deszcz podąża za ofiarą ostatniego kwalifikowanego podstawowego ręcznego strzału. Nowy wybrany cel przenosi ten sam deszcz.
- Cena: utrata stałego punktu kontroli, bez odnowienia czasu/tyknięcia początkowego. Salwa lub rykoszet wybiera tylko jednego jawnego głównego przewodnika, nie kopiuje deszczu.
- Build: prowadzenie artylerii między elitami; ruchliwa ofiara.
- Test: zmiana celu, śmierć przewodnika, nietykalność bossa i ten sam całkowity rytm deszczu.

## 23. Zabójca — class_hu_assassin

### Z01 — Całun własnego pogrzebu

- Id: item.unique.turris.assassin.own_funeral. Slot: Body. Warunek: Przygwożdżony cień A.
- Staty: życie, umiarkowana mobilność, fizyczna ofensywa.
- Moc: cień ofiary towarzyszy właścicielowi przy stopach. Legalne ataki we własnej bliskości mogą przez niego trafić połączoną ofiarę.
- Cena: ograniczony zasięg i rzeczywisty koszt własnych PŻ na pierwotny ręczny atak; nie kopiuj kosztu na każdy collider. Koszt nie jest ciosami przeciwnika i nie korzysta z redukcji.
- Build: wolna broń i zdalna walka przez niebezpieczne połączenie.
- Test: źródło/plecy zgodne z A, wspólny tracker ciała i cienia, cena oraz śmiertelne wyczerpanie PŻ.

### Z02 — Pierścień zwleczonego wyroku

- Id: item.unique.turris.assassin.deferred_verdict. Slot: Ring. Warunek: Otwarta tętnica B i bonus egzekucji.
- Staty: egzekucja, krwawienie, umiarkowana wytrzymałość.
- Moc: część kwalifikowanego bonusu egzekucji staje się odłożoną raną zamiast bezpośredniego zwiększenia trafienia. Rana może być rozliczona przewodzeniem.
- Cena: opóźnienie; zachowaj dokładny podział podstawy i dodatku. Nie naliczaj egzekucji drugi raz na ranie lub reakcji.
- Build: egzekucyjny DoT, sztylet + grymuar błyskawicy.
- Test: wróg ponad/pod progiem, statusowa odporność, konwersja i skończony budżet po reakcji.

### Z03 — Igła grobowej szwaczki

- Id: item.unique.turris.assassin.grave_needle. Slot: sztylet jednoręczny. Warunek: własna pozostała rana krwawienia.
- Staty: baza, krwawienie, umiarkowany zasięg umiejętności.
- Moc: mocny atak wyciąga część pozostałego krwawienia ofiary i zapisuje je jako pułapkę w ziemi. Kontakt przenosi ranę do nowego celu.
- Cena: rana rzeczywiście znika z poprzedniej ofiary w tej części. Nie jest klonowana, resetowana ani zwiększana przez samą zmianę nośnika.
- Build: Zabójca-saper i dalsze wykorzystanie mocnego przygotowania elity.
- Test: transfer atomowy, śmierć, odporność nowego celu, wspólna ofiara cienia i ciała oraz brak duplikacji przy minach.

### Z04 — Maska milczącego świadka [NOWY]

- Id: item.unique.turris.assassin.silent_witness. Slot: Head. Warunek: rzeczywiste ręczne trafienie w plecy i Rzut nożami.
- Staty: zasadzka/fizyczne obrażenia, wytrzymałość, umiarkowany zasięg.
- Moc: część dodatku wynikającego z trafienia w plecy zostaje zachowana jako jeden krótki ładunek. Następny Rzut nożami dostarcza go jako podzieloną porcję do odległych celów.
- Cena: słabsze początkowe zaskoczenie, termin ładunku i normalny koszt noży. Zachowany dodatek nie otrzymuje ponownie pełnej Zasadzki po dotarciu.
- Build: podejście za plecy -> przeniesienie nagrody w dystans, bez łuku.
- Test: zapis tylko bonusu, trafienie z przodu, salwa, wygaśnięcie i brak samopowielającej się zasadzki.

### Z05 — Rękawice karminowego węzła [NOWY]

- Id: item.unique.turris.assassin.crimson_knot. Slot: Hands. Warunek: Cięcie krzyżowe i własne krwawiące ofiary.
- Staty: krwawienie, szybkość, umiarkowane życie.
- Moc: różne ofiary legalnie trafione jednym Cięciem zostają krótko zszyte. Ręczne trafienie jednej może wydać część jej pozostałej rany na podzieloną odpowiedź u innych powiązanych.
- Cena: zużycie realnego budżetu pierwszej rany, mały limit węzła, zasięg i częstotliwość. Odpowiedź nie jest nowym ręcznym trafieniem uruchamiającym węzeł.
- Build: dystrybucja egzekucyjnego krwawienia w hordzie; układ z pułapkami.
- Test: jedno i wiele ciał, śmierć łącznika, różne odporności, rozliczenie budżetu i brak nieskończonego przewodzenia.

## 24. Łucznik arkanów — class_hu_arcane_archer

### Q01 — Łuk siódmej pieczęci

- Id: item.unique.turris.arcane_archer.seventh_seal. Slot: łuk dwuręczny. Warunek: Strzała zaklęcia A.
- Staty: baza, moc czarów, umiarkowany zasięg.
- Moc: obszar osadzonego czaru staje się pasem wzdłuż toru strzały zamiast jednym kręgiem trafienia.
- Cena: skończona moc rozkłada się na długości, niższe skupienie na jednym celu. Nie wypłacaj pełnego czaru na każdy metr.
- Build: korytarze mrozu, ognia i ziemi, ustawianie całej linii wrogów.
- Test: zapis drogi, kolizje, opóźniony payload, jeden cel przecinający kilka segmentów i brak pełnego punktowego wybuchu obok pasa.

### Q02 — Kołczan skradzionych żywiołów

- Id: item.unique.turris.arcane_archer.stolen_elements. Slot: Belt. Warunek: Strzała odpowiedzi B.
- Staty: umiarkowane żywioły, wytrzymałość, mana.
- Moc: część realnie powstałej reakcji jest odłożona do następnego ręcznego strzału zamiast trafić pierwotną ofiarę.
- Cena: pierwszy cel dostaje mniej, jest jeden ładunek z terminem, a pudło go marnuje. Reakcja z wydanego ładunku nie ładuje go ponownie.
- Build: przygotowanie na zwykłym wrogu i przeniesienie odpowiedzi w elitę.
- Test: atomowe pobranie części reakcji, kilka odłamków, odmienny cel i brak mnożenia przenoszonego bonusu.

### Q03 — Łuk heretyckiego chóru

- Id: item.unique.turris.arcane_archer.heretic_choir. Slot: łuk dwuręczny. Warunek: dodatkowe strzały i trzy zwykłe sloty.
- Staty: baza, umiarkowane wielożywiołowe premie, wytrzymałość.
- Moc: charakter dodatkowych strzał wynika z bazowych żywiołów wyposażonych umiejętności. Zapisz deterministyczne przypisanie; puste/fizyczne sloty mają jawne zachowanie.
- Cena: podział budżetu i rozproszenie inwestycji. Nie dodawaj pełnej kopii każdego elementu do każdego pocisku.
- Build: techniki różnych żywiołów i wielokolorowy ostrzał. Nie nadaje samodzielnie fokusu dla nielegalnie wyposażonych czarów.
- Test: kolejność slotów, zmiana wyposażenia, odpowiedź B i brak zależności od kolejności nakładania konwersji.

### Q04 — Amulet niedokończonej inkantacji [NOWY]

- Id: item.unique.turris.arcane_archer.unfinished_incantation. Slot: Amulet. Warunek: Strzała zaklęcia A.
- Staty: mana, moc czarów, umiarkowany czas przygotowania.
- Moc: dostarczony czar pozostaje krótką runą w miejscu trafienia. Następny legalny ręczny atak innym żywiołem w jej cel/obszar odpala zachowany czar z uzgodnioną przemianą na żywioł zapalnika.
- Cena: oba działania mają normalne opłaty; opóźnienie i możliwość wygaśnięcia bez zwrotu. Jeden payload jest w runie, a nie równocześnie w strzale i świecie.
- Build: dwustrzałowa kompozycja, techniki łuku jako legalne zapalniki bez dodatkowego wyjątku fokusu.
- Test: wybór innego żywiołu, własność runy, Q01 zachowujący dawny tor, nietrafienie i pojedyncze uwolnienie.

### Q05 — Rękawice kołczanu między światami [NOWY]

- Id: item.unique.turris.arcane_archer.between_worlds. Slot: Hands. Warunek: dodatkowe podstawowe pociski łuku.
- Staty: mana, umiarkowana szybkość i wytrzymałość.
- Moc: dodatkowe strzały zamiast natychmiastowego lotu trafiają do widocznego, ograniczonego kołczanu. Następny mocny strzał uwalnia wcześniej zgromadzone w nowym kierunku.
- Cena: odłożony DPS, termin, pojemność i brak ochronnej kolizji wiszących strzał. Zachowaj stare root/budżety/żywioły; to przechowanie, nie nowa pełna salwa.
- Build: kontrolowany burst, Chór Q03 i dobieranie miejsca uwolnienia; B rozstrzyga odpowiedź w rzeczywistym kontakcie według własnej reguły.
- Test: uwolnij starą kolejkę przed dodaniem nowych pocisków tego mocnego strzału; dzieci nie kopiują payloadu A ani ładunku Q02.

## 25. Obowiązkowa kompozycja i konflikty

Sprawdź końcowy graf wykonania po wszystkich modyfikatorach. Nie wystarczy osobno wykazać, że działa każdy przedmiot. Nie każda para da się założyć lub połączyć z jednym wybranym wariantem klasy.

Priorytet ogólny:

1. Najpierw walidacja realnego wyposażenia i właściciela cechy.
2. Potem zamiana typu akcji, np. fizyczna riposta na czar.
3. Następnie zgodna geometria/dostarczenie, np. nova na minę lub payload na pas.
4. Potem magazynowanie, transport i opóźnienie tego samego budżetu.
5. Na końcu właściwe rozliczenie obrażeń, reakcji i legalnych zdarzeń wtórnych.

To rozróżnienie semantyczne, nie polecenie przepisywania istniejącego pipeline w ciemno. Wynik ma być deterministyczny i niezależny od kolejności zakładania.

### 25.1. Konkretne wymagane kombinacje

| Kombinacja | Wymagany wynik |
| --- | --- |
| W01 + W04 z A | Blok w czasie ataku legalnie kosztuje manę i uzbraja jedną wartę; nie rzuca od razu darmowej techniki |
| W02 + W03 z B | Zmiana broni zachowuje właściwe źródło śladu i nową geometrię prawdziwej riposty; brak obu zestawów premii naraz |
| P01 + P05 z A | Leczenie może zasilać legalną Łaskę i rozliczyć istniejącą winę; rozliczenie samo nie leczy i nie tworzy nowej winy/Łaski |
| P02 + P04 z B | Ręczne parowanie uwalnia ograniczoną moc odpowiedniej przeniesionej aury, bez pełnego drugiego pulsu przy właścicielu |
| P03 na Przywoływaczu z S02 | Jedna dostępna temu właścicielowi aura wspiera śmiertelną chimerę; Habit nie przyznaje Pobożności i drugiej aury |
| B01 + B03 + B05 z A | Odwet, ruch ran i odkładany szał są różnymi budżetami; niski limit życia nie staje się 100% dla skóry |
| B02 + B05 z B | Sekundy oddane serii ciężkich cięć nie produkują też żetonów i widm |
| R01 + R03 z różdżką | R01 legalizuje drugą różdżkę, ale R03 nie udaje, że jest ona lewym ostrzem do rzutu; pokaż nieaktywny warunek |
| R03 + R05 | Dwie różne bronie mogą legalnie mieć odrębne role w locie, jeśli budżety i animacje na to pozwalają; jeden egzemplarz nie istnieje w dwóch wirach |
| R04 + Niezgodna para | Drugi pakiet tego samego splecionego ataku nie jest osobną ręczną odpowiedzią domykającą Cięcie |
| M02 + M03 z B | Runy zużyte przy przygotowaniu miny są zapisane w tym jednym payloadzie, a nie użyte też przez następne zaklęcie |
| M03 + H04 na Magu wody | Mina zmienia pozycję Falą, ale nie uzbraja się ponownie i nie odzyskuje czasu |
| M03 + M04 | Jeden rzut dający kilka zgodnych pól odkłada jeden kontrolowany etap połączenia, nie pełną sieć na każdy odłamek |
| F01 + F05 | Zjedzony Krąg nie zaklina dalszych strzałów; przekształcony strzał nie zachowuje jednocześnie pełnego dodatku i meteoru |
| F02 + F04 z B | Marionetka przenosi tę samą rodzinę i jej odłożony budżet; nie wypłaca go jako osobnego wybuchu i drugiego pełnego podpalenia |
| H02 + pola przestawiane B | Smuga nie zostawia jednocześnie pełnej strefy, nowych tyknięć i starego pełnego czasu |
| A02 + A04 z B | Węzły łańcucha i punkty zderzeń są odrębne; samo przeskoczenie pioruna przez pocisk nie udaje fizycznego zderzenia |
| A03 + A05 | Krok kontroluje pociski, nova przesuwa gracza; żadna część nie dodaje klatek niewrażliwości |
| E02 + E04 z B | Osłona zużyta na płyty nie zostawia też pełnych odłamków kamieniołomu |
| E03 + E05 | Odroczone kolce oraz osłabienie ziemi zachowują różne źródła, skończone życie i prawidłowy wpływ na fizyczny cel |
| S01 + S05 z A | Sługa zużyty do Wcielenia nie daje jednocześnie śladu naturalnego wygaśnięcia |
| S02 + S04 z B | Czar przez śmiertelną chimerę płaci normalną manę i odpowiadający wkład/PŻ nośnika; brak darmowej nieskończonej artylerii |
| S02 + P03 + S04 | Leczenie/aury mogą podtrzymywać bestię za realne zasoby, ale nie mnożą każdej premii przez liczbę jej dawnych składników |
| S04 + H03 | Oba są grymuarami drugiej ręki: nie twórz fikcyjnego buildu z dwoma jednocześnie |
| T04 + W03 | Riposta zamieniona na czar nie wykonuje także pełnej fizycznej fali hełmu; tooltip wyjaśnia warunek W03 |
| T03 + E05 | Dopuszczalna kompozycja: miejsce osłabienia jest na ziemi, a jego typ magiczny. Nie dodawaj równolegle pełnej wersji fizycznej |
| T01 + T05 | Bandaże wymagają pustych dłoni; założony grymuar wyłącza tę część. Nie reklamuj tej pary jako pełnego działania |
| L02 + L05 z B | Skradziona pułapka może być zdalnie odpalana nożem; wejście wroga nie detonuje jej równocześnie drugi raz |
| L03 + L04 | Przygotowany w punkcie startu nośnik zgodnej bomby może przyczepić ładunek do pierwszej ofiary; nie pomijaj następnego warunku detonacji niewypału |
| K01 + K04 z A | Strzały Salwy pozostają źródłem techniki, bez pełnych magazynów podstawowych Gwoździ na każdą przygotowaną strzałę |
| K02 + K04 z B | Mocny strzał ustala kierunek szeregu i własne źródło przed zamianą pozycji; rząd nie teleportuje się razem ze stanowiskiem |
| K03 + grymuar | Łuk zajmuje obie ręce; bez jawnej zatwierdzonej reguły nie działa z fokusem w drugiej |
| Z02 + Z03 + Z05 z B | Ta sama rana nie może być jednocześnie w ofierze, pułapce, węźle i reakcji. Każdy transfer ma atomowe pobranie |
| Z01 + Z04 z A | Bonus z pleców sprawdza prawdziwą relację właściciel/ofiara, a nie zawsze korzystną orientację własnego cienia |
| Q01 + Q03 | To dwa łuki; brak równoczesnego działania obu. Arsenał innej klasy nie przyznaje jej Strzały zaklęcia |
| Q01 + Q04 + Q05 z A | Jeden główny payload może przejść w runę i później pas dawnego toru. Strzały z kołczanu nie dziedziczą jego pełnej kopii |
| Q02 + Q03 + Q05 z B | Bazowy żywioł z Chóru, kolejka i odpowiedź mają jawną kolejność; dzieci zużywają tylko pierwotny wspólny zapas |

Dla Q05 określ twardy czas ważności: odłożenie pocisku nie przedłuża końca pierwotnego okna cechy B. Jeżeli jego szczególne uprawnienie już wygasło, pocisk zachowuje zwykły zapisany żywioł. Adaptacja, gdy dozwolona, sprawdza rzeczywisty stan ofiary przy trafieniu.

### 25.2. Istniejące pieczęcie i proci

Uwzględnij co najmniej: Echo, Kaskada, Rozszczepienie, Rozprysk, przebicie, rykoszet, Odruch, Riposta, Zemsta, Żniwiarz, Osłona, Ofiara krwi, Słup ofiarny, Przekaźnik, konwersje i więzi.

Przykłady:

- Echo nie tworzy nowego magazynu ręcznej akcji, ale może wykonać dozwoloną słabszą kopię zmienionego kształtu z dzielonymi uprawnieniami.
- Słup nie używa automatycznie mocy wymagającej ręcznego parowania, wyboru ofiary albo nowego kosztu własnego życia.
- Osłona z legalnego pierwotnego rzutu może powstać normalnie; potomne płyty, runy i odwet nie tworzą nieograniczonej nowej Osłony.
- Przekaźnik odpala skończony istniejący ładunek. Nie zwiększa pozostałego budżetu miny wyłącznie z powodu jej przeniesienia.
- Konwersja żywiołu musi odświeżyć zgodne statusy i premie. Nie pozostawiaj pełnych premii ognia na części, która faktycznie stała się mrozem.
- Wspólna moc „atak z uniku” nie zamienia jednego naciśnięcia w nowy niezależny ręczny rzut dla każdego potomnego pocisku.

Nie wyłączaj w ciemno wszystkich pieczęci na przemienionych skillach. Zgodność testuj na końcowym kształcie i pokaż powód rzeczywistego konfliktu.

## 26. UI, opisy i oprawa

1. Każdy z 75 przedmiotów ma unikalną nazwę po polsku, ikonę, jednozdaniową obietnicę mocy i pełny opis.
2. Tooltip rozdziela statystyki, legendarną regułę, koszt/ograniczenie i warunek. Wyjaśnia uśpienie oraz konflikt slotu lub skilla.
3. Liczby pobieraj z tej samej konfiguracji co runtime. Brak ręcznie wpisanych, rozjeżdżających się procentów.
4. Pokaż, co zmieniło się w wyposażonym skillu: kształt, żywioł, koszt, źródło, tryb ręczny/pułapka, gotowość i brak kompatybilności.
5. HUD pokazuje magazyny tylko wtedy, gdy są potrzebne: runy, zapas cięć, warta, winy, grot, ślad dziedziczenia, runa czaru i kołczan między światami.
6. Brak gotowości nie udaje udanej aktywacji. Podaj konkretną przyczynę po polsku bez pobrania zasobu za odrzuconą część.
7. Obsłuż mysz, klawiaturę, pad, remapowanie i powrót fokusu w ekwipunku oraz katalogu. Zmiana urządzenia aktualizuje podpowiedzi.
8. Nowe przedmioty w dłoniach wymagają modeli i prawdziwego chwytu. Wariant różdżki w lewej dłoni, pięści, łuk w zwarciu i broń w locie wymagają osobnego dowodu.
9. Pole, lustro, warta, rząd strzał i punkt źródła muszą zgadzać się z kolizją i kierunkiem działania.
10. Opóźniona moc ma czytelną zapowiedź, a zgromadzenie pełnej wartości rozpoznawalny stan. Nie zasłaniaj znaków wrogów efektami.
11. Dźwięki mają ograniczenie powtórzeń przy tłumie i salwach. Różne rodzaje przemian powinny być rozpoznawalne, bez pełnej głośności na każdą kopię.
12. Użyj zgodnej stylistyki gotyckiego anime i istniejących zasobów. Nie regeneruj wszystkich modeli ani nie rozpoczynaj odpłatnej generacji zewnętrznej jako domyślnego skrótu.
13. Materiały i mechaniki działają również w buildzie. Ograniczenie VFX nie może zmieniać obrażeń.

## 27. Boty i mierzenie efektu przedmiotów

### 27.1. Legalne buildy

Dla każdego unikatu przygotuj minimum:

- jeden osiągalny rdzeń z odpowiednią cechą, wyposażeniem i potrzebnymi skillami;
- jeden rozwinięty build z co najmniej dwoma dodatkowymi zgodnymi składnikami;
- przykład świadomego kosztu: co gracz oddaje i kiedy przeciwnik nadal mu zagraża.

Użyj rzeczywistych id zasobów, limitu trzech slotów, legalnego fokusu, dłoni i wymagań. Gdy przedmiot przyznaje podstawowy potrzebny skill, pokaż to jawnie w danych i UI. Nie wyposażaj do testu dwóch grymuarów lub dwóch łuków na raz.

### 27.2. Polityki botów

Bot ocenia, czy moc działa w jego wariancie, czy ma potrzebny skill i czy potrafi wykonać nową pętlę. Nie uznaje każdego Unique za automatyczną poprawę.

Dodaj obsługę: przygotowania i wyzwolenia, zmiany źródła, budowy sieci, kosztu zasobów, odmowy przy braku miejsca, ratowania śmiertelnej chimery i rzeczywistego dystansu po teleportacji.

Zachowaj bezpieczne picie. Wyników nie poprawiaj ukrytym ignorowaniem ceny życia, zwrotem długu lub odpornością na obrażenia.

Uwzględnij też sprzedaż niepasującego przedmiotu i zachowanie statystycznie lepszego zwykłego wyposażenia. Polityka ma rozpoznawać potencjał przyszłego buildu, ale nie zakładać pewnego dropu brakujących składników.

### 27.3. Telemetria

Rejestruj: klasę, cechę, id/uid/jakość przedmiotów, skill źródłowy, seed, próbę aktywacji, sukces, powód odmowy, koszt, przeniesiony/wydany/zmarnowany budżet oraz ograniczenie liczby zdarzeń.

Dla szczególnych grup zbieraj:

- obrażenia bezpośrednie, rany, reakcje i wyzwalacze oddzielnie;
- źródło geometryczne oraz odległość od faktycznego emitera;
- magazyny: wejście, wyjście, wygaśnięcie i pozostała suma;
- obronę: zasób wydany, realne obrażenia zatrzymane, realne PŻ utracone;
- miny/strefy: czas od przygotowania, liczba legalnych trafień i przeniesienia;
- sługi: właściwy wkład, czas/zdrowie, polecenia, dziedziczenie i śmierć;
- przyczynę końca sekwencji, np. brak drugiej ręki, wygaśnięcie szału, przerwanie lub nietrafiony zapalnik;
- koszt klatki, alokacje, liczby bytów i limity potomnych zdarzeń.

Porównuj dany build z kontrolą bez legendy, zachowując zbliżone zwykłe statystyki. Nie porównuj wyłącznie mocnego późnego unikatu z gołą postacią.

Wielogodzinne serie botów respektują najnowsze wstrzymanie w docs/PROGRESS.md. Ten prompt go nie odwołuje. Krótkie deterministyczne scenariusze zachowań i przygotowanie polityk są obowiązkowe. Przy dozwolonych seriach przestrzegaj ustalonego limitu kopii i pamięci.

## 28. Testy obowiązkowe

### 28.1. Dane i ekonomia

- 75 unikalnych id, pięć na każdą z 15 grup i 30 nowych oznaczonych kart.
- Wszystkie rzeczywiste definicje mają rzadkość Unique i są indeksowane w buildzie oraz snapshotach.
- Brak fałszywego huSourceId i zależności od edytorowego AssetDatabase w runtime.
- Roll nie obniża rodzimego unikatu do Legendary/Rare.
- Łup z właściwego źródła tworzy prawdziwy egzemplarz; kolekcja w świecie działa i nie duplikuje go na końcu piętra.
- W zwykłym sklepie nie pojawia się niezamierzony unikat. Wskazanie poszukiwanego przedmiotu nie omija progu ani źródła.
- Stała moc występuje w każdym dopuszczonym rollu. Poziom/jakość nie mnoży struktury bez zgody danych.
- Wartość, sprzedaż i ulepszanie są poprawne dla broni i akcesoriów bez katalogowego wiersza HU.
- Snapshot zachowuje jakość, statystyki, odblokowania oraz poprawne źródła bez kolizji uid.
- Stary profil i stary łup działają tak jak wcześniej.

### 28.2. Wyposażenie i runtime

- Przedmiot w plecaku nie działa. Zdjęty nie generuje nowych zdarzeń, a opłacony wcześniej budżet ma jedną opisaną ścieżkę zakończenia.
- Dwa pierścienie tej samej definicji nie podwajają legendy.
- Nie można korzystać z nielegalnego offhandu przy 2H, a wyjątek R01 jest lokalny i odwracalny.
- Broń w locie nie jest równocześnie bronią w dłoni. Zmiana arsenalu nie duplikuje jej.
- Obie przeciwne kolejności zakładania kompatybilnych mocy dają ten sam wynik.
- Wymagana cecha A/B jest sprawdzana w wykonaniu, nie tylko w tooltipie.
- Zerowy zasób, częściowy koszt, anulowanie, równoczesne trafienia, pauza, śmierć i przejście piętra mają prawidłowe rozliczenie.

### 28.3. Każda z 75 mocy

Każda karta ma test matematyczny/kontraktowy oraz test PlayMode pokazujący jej prawdziwe działanie. Testy mają wykrywać błędną mechanikę, a nie tylko odczytywać tę samą stałą z dwóch miejsc.

Obejmij pięć kontekstów:

| Kontekst | Pytanie |
| --- | --- |
| Grupa zwykłych wrogów | Czy pokrycie i liczba celów nie klonują magazynu albo pełnej reakcji? |
| Elita | Czy wykonanie nowej sekwencji daje realną przewagę i zachowuje ograniczenia? |
| Samotny/ruchliwy boss | Czy build ma zastosowanie, zachowuje fazy i nie potrzebuje fikcyjnych ofiar? |
| Zagrożenie defensywne | Czy nadal można otrzymać obrażenia i czy cena jest czytelna? |
| Mało zasobów / brak warunku | Czy odmowa jest poprawna, a postać może wrócić do legalnych podstawowych akcji? |

Cecha hordowa nie musi wygrywać pojedynku z bossem. Nie może jednak psuć podstawowego sterowania, być opisana fałszywie lub działać przez obchodzenie faz.

Dla przedmiotów geometrycznych sprawdź ścianę, ciasny teren, krawędź areny, ruchomy cel i brak legalnego miejsca. Dla AI sprawdź prawdziwy wybór celu. Dla przechowywanych ran i budżetów przeprowadź jednoczesny test dwóch prób konsumpcji.

Brak próbek oznacza brak potwierdzenia, nie wynik „0 błędów”. Test pominięty z powodu niewłączonej galerii nie potwierdza wyglądu.

### 28.4. Przypadki najbardziej ryzykowne

- leczenie -> Łaska -> atak -> leczenie;
- leczenie -> wina -> wyrok -> nowa wina;
- mana na ochronę -> runy -> nowa Osłona -> następne runy;
- zadłużony czar -> spłata -> podwójny zwrot;
- szał -> zapisane cięcia -> żetony -> widma z tych samych sekund;
- bariera -> płyta -> odłamki -> odtworzona pełna bariera;
- rana -> pułapka -> węzeł -> reakcja -> odnowiona rana;
- przechwycony/przechowywany pocisk -> kolejne przechwycenie -> pełny nowy payload;
- źródło po uniku -> warta -> stanowisko -> kilka pełnych miejsc tego samego ataku;
- równoległe działania -> nowy root na każdej klatce;
- naturalny koniec sługi -> dziedziczenie -> scalenie -> ponowny darmowy koniec;
- widmo -> kopiowana legenda -> nowe widmo;
- sieć piorunów odwiedzająca ten sam cel przez dowolną liczbę własnych węzłów;
- nadawanie cudzej cechy przez założenie wyglądu, przedmiotu lub wczytanie obcego id.

Ograniczaj konkretną pętlę, a nie cały legalny system synergii. Nie obniżaj wszystkich klas globalnie z powodu błędu jednego przedmiotu.

### 28.5. Regresje i pomiary

Uruchom odpowiednie istniejące testy ekwipunku, statystyk, skilli, pieczęci, klas, AI, ekonomii, zapisów i sterowania oraz pełne wymagane zestawy po integracji.

Zmierz realne DPS/obrażenia w oknie, czas czyszczenia, użyteczność, koszt, przeżywalność i trudność obsługi na ustalonych seedach. Dla źródeł opóźnionych porównuj cały cykl przygotowania, a nie tylko końcową eksplozję.

Zbierz alokacje, liczbę aktywnych bytów, koszt klatki i rozmiary kolejek. Sprawdź wiele celów i serię 100 aktywacji/zmian wyposażenia pod kątem wycieków, osieroconych obiektów i rosnącej liczby callbacków.

## 29. Weryfikacja wizualna i polecenia

CLI tylko przy zamkniętym edytorze:

~~~powershell
python tools/run_tests.py EditMode
python tools/run_tests.py PlayMode window
~~~

Galeria zgodnie z aktualnym runnerem:

~~~powershell
$env:TURRIS_SHOT_DIR = "TestResults/unique-items/shots"
python tools/run_tests.py PlayMode gfx
~~~

UI i pad sprawdź w window albo we właściwym działającym edytorze, z uwagi na OnGUI i WaitForEndOfFrame w batchmode. Ustaw oddzielny katalog wyników, aby nie nadpisywać innej pracy.

Obejrzyj dowody wszystkich 75 efektów: minimum czytelny obraz/sekwencja i odpowiadająca jej telemetria. Dla mocy niewidocznych w jednej klatce pokaż przed/po i właściwą sekwencję.

Osobno udowodnij:

- pięści Maga, różdżkę w lewej ręce i łuk w zwarciu;
- dwa narzędzia w locie, prawdziwy brak broni w dłoni i powrót;
- odwróconą Szarżę oraz zamianę pozycji bez dodatkowych i-frame'ów;
- stare i nowe źródło pocisku/czaru;
- skończone magazyny, transport min i zapisane rany;
- realną śmiertelną chimerę, wybór leczenia i zdrowie;
- prawdziwe przejęcie ataku wroga i przyjazny ogień po oszustwie;
- czytelne zapowiedzi oraz brak różowych/niewidzialnych materiałów w buildzie.

Nie deklaruj obejrzenia czegoś, czego nie otwarto. Sam świecący model, ikonka lub poprawna liczba w Inspectorze nie dowodzą działania mocy.

## 30. Etapy, dokumentacja i definicja ukończenia

1. Audyt stanu 30 cech i aktualnego łupu. Zapisz zależności oraz tabelę wszystkich 75 pozycji.
2. Natywne dane Unique, indeksowanie, zapis, klasyfikacja, rzeczywiste dropy i testy ekonomii.
3. Wspólny kontrakt przemian, etapów budżetu, opłat, zgodności i prezentacji.
4. Pełna pionowa ścieżka jednego przedmiotu z każdej głównej rodziny mechanik: przemiana ataku, magazyn, pole/mina, przywołanie, AI, przesunięcie źródła.
5. Wdrażanie kolejnych grup do kompletnych 75, bez zatrzymania na przykładach. Każda grupa obejmuje działanie, UI, zapis, boty i dowód.
6. Legalne kombinacje oraz celowane konflikty z rozdziału 25, 30 cechami i wspólnymi pieczęciami.
7. Oprawa, koszt klatki, regresje, krótkie testy zachowań botów i prawdziwe obrazy.
8. Aktualizacja README, docs/PROGRESS.md, katalogu przedmiotów i raportu implementacji.

W tabeli postępu prowadź oddzielnie: definicja, runtime, drop, opis/UI/pad, zapis, legalny build, test kontraktu, test silnika, oprawa i dowód. Sam asset nie daje statusu „gotowe”.

W raporcie dla każdej legendy umieść:

- finalne parametry i dwa osiągalne przykłady buildu z prawdziwymi id;
- co zmienia się w pętli gry i za co gracz płaci;
- zgodności/konflikty oraz pochodzenie przenoszonego budżetu;
- wyniki testów, seed, czas i ścieżki obrazów/logów;
- rozdzielenie faktu z kodu, pomiaru i hipotezy balansu;
- ocenę koncepcyjną 0–4 dla wyrazistości, nowych decyzji, synergii, czytelności, ceny, przeciwwag, progresji, odporności na nadużycia i utrzymania; nie zastępuj jej samą średnią DPS.

Wadliwa kolizja, nieosiągalny build, utrata zasobów lub nieskończona pętla są blokerami niezależnie od oceny pomysłu.

### Warunki odbioru

- Wszystkie 75 przedmiotów, w tym 30 nowych, ma prawdziwą odrębną moc, zachowane nazwy i legalny slot.
- Wszystkie mogą trafić do gracza przez właściwą ścieżkę gry, mają opisy, wartość, gwarantowaną moc i działający zapis.
- Baza 30 cech istnieje i jest poprawnie integrowana; brak darmowego drugiego wariantu lub starej trzeciej sygnatury.
- Każdy unikat ma sensowny build; nie wymaga niemożliwego wyposażenia lub nieosiągalnego skilla.
- Ulepszanie i jakość nie klonują mechaniki, a duplikaty nie mnożą strukturalnej legendy.
- Podane kompozycje działają albo pokazują uzasadniony konflikt; kolejność zakładania nie zmienia efektu.
- Gracz na klawiaturze i padzie rozumie przygotowanie, wyzwolenie i cenę.
- Budżety są skończone, opłaty poprawne, bossowie zachowują fazy, a śmierć nadal jest możliwa.
- Istnieją prawdziwe testy, pomiary i obejrzane dowody, z jawnymi nieuruchomionymi lub pominiętymi sprawdzeniami.
- Nie naruszono źródła Hell Unleashed, cudzych zmian i istniejących poprawionych animacji.

Jeśli praca zostanie przerwana albo zabraknie narzędzia, podaj dokładny stan pozycji i blokadę. Nie przedstawiaj częściowego wdrożenia jako pełnego. Przy rzeczywistej blokadzie wykonaj niezależną część, zachowując cel ukończenia całego katalogu.

Końcowa odpowiedź ma podawać konkretne działające mechaniki, rzeczywiste wyniki, miejsce raportu i pozostałe ograniczenia. Commit/push wyłącznie po odrębnej prośbie autora.
