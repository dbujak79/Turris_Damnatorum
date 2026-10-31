# Turris Damnatorum — specyfikacja stylu gotyckiego anime

Wersja 1.2, 2026-10-28. Dokument do przygotowywania przyszłych promptów, oceniania koncepcji i przenoszenia ich do assetów 3D oraz Unity.

**Kierunek: dojrzałe gotyckie anime fantasy, z malarskim detalem, rzeźbiarską formą i matowym cieniowaniem.** Świat ma być mroczny i bogaty w szczegóły, a postacie oraz przebieg walki czytelne z kamery gry. Charakter referencji trzeba zachować także w działającym modelu.

## 1. Referencje i ich znaczenie

| Referencja | Co określa | Czego nie należy z niej przenosić automatycznie |
|---|---|---|
| [Docelowy dziedziniec](../ArtSource/GothicAnimePoC/Arena/reference-courtyard.png) | Wspólny wygląd świata: gotyk, kamień, noc, światło, materiały, szczegółowość i relacja postaci do areny | Układu areny, nocy i księżyca do każdego kolejnego assetu lub lokacji |
| [Paladyn — wzór wyglądu](../ArtSource/GothicAnimePoC/Paladin/reference-palette.png) | Jasna matowa zbroja, ostre ale eleganckie podziały płyt, przygaszone złoto, burgund, granatowe podbicie, sposób rysowania i cieniowania | Zbroi, krzyży i czerwonego płaszcza do maga, demonów lub każdej nowej klasy |
| [Mag — koncepcja](../ArtSource/GothicAnimePoC/Mage/mage-gothic-anime-v1.png) | Zachowawcza adaptacja istniejącej postaci, malarskie tkaniny, ciemne warstwy, włosy, skóra i drobne okucia | Kapelusza, księgi i kostura do innych postaci |
| [Mag i berserker — korekta wykończenia](../ArtSource/GothicAnimePoC/StyleCorrections/mage-berserker-correction-v1.png) | Zatwierdzony kierunek korekty: wyraźne granatowo-fioletowe warstwy maga; większe malarskie płaszczyzny skóry, zbroi, włosów i futra berserkera; wspólne światło z dziedzińcem | Pozowania, wyposażenia i twarzy do innych klas; pomarańczowego oświetlenia jako stałego koloru krawędzi tekstury |
| [Ogień](../ArtSource/GothicAnimePoC/FirstArenaEnemies/imp-fire-gothic-anime-v1.png), [woda/lód](../ArtSource/GothicAnimePoC/FirstArenaEnemies/imp-water-gothic-anime-v1.png) | Organiczne demony, detal rogów/pazurów/błon, rozróżnienie żywiołów | Milionowej liczby trójkątów jako wymogu stylu; skrzydeł do ognistego chochlika |
| [Wiatr](../ArtSource/GothicAnimePoC/FirstArenaEnemies/imp-air-gothic-anime-v1.png), [ziemia](../ArtSource/GothicAnimePoC/FirstArenaEnemies/imp-earth-gothic-anime-v1.png) | Roboczy kierunek kolejnych odmian tej rodziny | Założenia, że te modele zostały już wykonane lub wdrożone |

Koncepcje są wzorcem artystycznym. Zrzuty bieżącej gry i surowe rendery Meshy służą do porównywania postępu; nie zastępują wzorca. Zbyt ciemne sylwetki, połysk, zniekształcone stopy lub niedopracowane deformacje widoczne w PoC nie stają się przez to cechami stylu.

W promptach trzeba jawnie opisać rolę każdego obrazka: **tożsamość i konstrukcja assetu**, **styl rysowania**, **paleta** albo **kompozycja**. Referencja stylu nie upoważnia do kopiowania wyposażenia innej postaci.

## 2. Zasady nadrzędne

1. **Zachowuj tożsamość istniejącego assetu.** Przed przeróbką obejrzyj model używany w grze: przód, tył, sylwetkę i wyposażenie. Wypisz elementy, które muszą pozostać. Sama nazwa klasy lub gatunku nie jest wystarczającą referencją.
2. **Nie upraszczaj projektu wizualnego.** Zachowaj warstwy stroju, profile kamienia, podziały zbroi, anatomię, pazury i ornament tam, gdzie występują w referencji. Nie zastępuj szczegółowego projektu gładkimi bryłami ani przypadkowym szumem tekstury.
3. **Forma ma być czytelna przed drobnym detalem.** Najpierw sylwetka i duże płaszczyzny, następnie konstrukcja i warstwy, na końcu zużycie. Detale nie mogą rozbijać postaci na nieczytelną plamę.
4. **Styl anime nie oznacza zmiany wszystkich proporcji.** Bohaterowie mają dojrzały wygląd i wiarygodną anatomię. Ostrość rysunku, kontrola koloru i cienia dają stylizację; nie są potrzebne wielkie oczy, ogromne głowy ani groteskowo powiększona broń.
5. **Mrok nie może usuwać informacji.** W cieniu nadal widać głowę, ręce, podparcie stóp, kierunek ciosu oraz główne warstwy materiału.
6. **Wysoka jakość nie jest liczbą polygonów.** Pełne źródło zachowujemy. Wersja do gry musi zachować wygląd i być sprawdzona w kamerze oraz animacji. Parametry generatora, LOD i budżet siatki ustala się osobno; sam styl nie nakazuje miliona trójkątów ani automatycznej redukcji.

## 3. Język formy, rysunek i detal

### Sylwetka i proporcje

- Naturalne, dorosłe proporcje postaci ludzkich; istniejący model ma pierwszeństwo przed ogólnym wzorcem anime.
- Sylwetki wyraziste, lekko wydłużone i eleganckie tam, gdzie wynika to z referencji. Ciężar zbroi, długość szaty i masa broni muszą być wiarygodne.
- Ostrołuki, szpice hełmów, wydłużone podziały płyt i tkaniny wspierają gotycki charakter. Nie dokładaj kolców na każdej powierzchni.
- Dłonie, stopy, rogi, ogony i skrzydła muszą mieć logiczne połączenia. Kontakt stopy z podłożem i chwyt przedmiotu są częścią jakości modelu.
- Nie poszerzaj automatycznie barków, nóg, stóp, skrzydeł ani broni. Zachowaj skalę i funkcję assetu w walce.

### Kontur i płaszczyzny

- Precyzyjny, oszczędny, ciemny kontur. Mocniejszy na obrysie sylwetki i w głębokich połączeniach; delikatniejszy wewnątrz formy.
- Płaszczyzny mają być rzeźbiarskie i świadomie zaprojektowane. Płytę zbroi, policzek, fałdę i krawędź kamienia opisuje kształt oraz kolor cienia.
- Duże obszary światła i cienia z niewielką liczbą czytelnych pasm; przejścia miejscami miękkie. Unikaj zarówno fotograficznego rozdrobnienia, jak i płaskiego, dwukolorowego plakatu.
- Kontur nie może wyglądać jak gruby jednolity marker. Realizacja w 3D nie wymaga obrysowania shaderem każdego trójkąta.

### Hierarchia szczegółów

| Poziom | Przykłady | Wymaganie |
|---|---|---|
| Duża forma | Sylwetka, bryła pancerza, płaszcz, łuk, rozpiętość skrzydeł | Musi być zrozumiała z kamery gry |
| Konstrukcja | Zachodzące płyty, szwy, pasy, fałdy, klińce, żebra błony | Wyjaśnia sposób zbudowania obiektu |
| Mały detal | Rysy, strzępienia, ukruszenia, łuski, pory, patyna | Podkreśla materiał; nie zagłusza dwóch poprzednich poziomów |

Powierzchnie mają mieć miejsca spokojne i miejsca bogate w detal. Jednolita gęstość rys, pęknięć lub łusek na całym obiekcie jest niepożądana.

## 4. Materiały i tekstury

| Materiał | Wymagany wygląd | Niedopuszczalny kierunek |
|---|---|---|
| Zbroja i stal | Matowa lub wizualnie bardzo szorstka, szerokie malowane płaszczyzny, widoczne krawędzie i grubość płyt | Chrom, lustrzane odbicia, mokry plastik, ceramiczny kombinezon, wąskie ruchome refleksy jak na aucie |
| Okucia i złoto | Stare, przygaszone, oszczędne; podkreślają konstrukcję | Jaskrawe żółte złoto, błyszczące obwódki na każdej części, biżuteria dodana bez podstawy |
| Tkanina | Matowa, ciężar i kierunek fałd zgodne z krojem; czytelne warstwy, zamierzone przetarcia i strzępienia | Satynowy połysk, guma, przypadkowo poszarpany każdy brzeg, strój bez grubości i warstw |
| Skóra wyprawiona | Przygaszone brązy/oliwka, ślady zgięć przy paskach i rękawicach | Winyl, jaskrawa pomarańcz, fotograficzna faktura dominująca nad formą |
| Kamień | Matowy, duże spokojne plamy koloru, profile, fazy, wiarygodne ubytki i szczeliny | Gładki beton bez detalu, plastik, bruk z setek jednakowych kostek, hałaśliwy skan fotograficzny |
| Skóra demona i keratyna | Organiczne mięśnie, naturalne grzbiety i rogi, kontrolowany detal, kościane pazury | Futurystyczny pancerz, metalowa skóra, szklisty cały korpus, losowe neonowe obwody |
| Włosy | Spójne, modelowalne pasma i większe kosmyki z malowanym cieniem | Fotorealistyczne pojedyncze włoski lub przesadne kolce fryzury bez związku z postacią |
| Drewno | Kierunek włókien zgodny z formą, matowe, zużyte przy chwycie i okuciach | Plastik, przesadnie kontrastowe słoje na całej powierzchni |

Tekstura koloru powinna zawierać pigment, malarskie zróżnicowanie i lokalny detal. Nie wypiekaj w nią mocnego światła studyjnego, odbić otoczenia, dużej czarnej plamy cienia ani białych refleksów, które potem zostaną oświetlone ponownie w Unity.

Materiał ma być rozpoznawalny w neutralnym świetle oraz na arenie. Zmiana na mokry lub nałożenie statusu nie może przypadkiem przywracać lustrzanego połysku. Żar, oczy i efekty trafienia są emisją ograniczoną do właściwych fragmentów.

## 5. Paleta i światło

Dominują barwy przygaszone. Żywszy kolor kieruje uwagę na broń, oczy demona, efekt skilla albo istotny punkt lokacji. Nie rozświetlaj całego stroju lub ciała, aby wyróżnić klasę.

Poniższe HEX to **orientacyjna paleta autorska do opisu promptów**, a nie kolory pobrane pomiarem z referencji ani wartości do bezpośredniego wpisania w shader Linear. Obraz referencyjny ma pierwszeństwo.

| Rola | Przykładowy zakres |
|---|---|
| Jasna matowa zbroja / kość | ciepła kość słoniowa `#C8BEAC`, cień kamiennoszary `#777B83` |
| Stare złoto / mosiądz | `#A88C56`, ciemne okucia `#62503A` |
| Tkanina paladyna | burgund `#752B32`, głębokie podbicie `#202A3B` |
| Mag | atrament `#20212B`, granat/fiolet `#303147`, oliwkowa skóra `#555044`, włosy `#D4D0C4` |
| Kamień | ciepły szary/beż `#918674`, chłodny cień `#4B5365` |
| Noc i tło | granat `#182B4D`, daleki błękit `#365782` |
| Pochodnia / żar | bursztyn `#F0AA4F`, mały jasny rdzeń `#FFE6AD` |

### Oświetlenie pierwszego dziedzińca

- Chłodna noc i księżycowe wypełnienie, miejscowe ciepło pochodni, subtelne ciepłe odbicia od posadzki.
- Oddzielone plany: podłoga i przeciwnicy, architektura dziedzińca, odległy zamek i niebo.
- Chłodne, barwne cienie. Czerń przede wszystkim w szczelinach, wnętrzu hełmu i głębokich stykach, nie jako cała przednia powierzchnia postaci.
- Jasna zbroja zachowuje kolor i podział płyt. Nie może zamienić się w białą wypaloną plamę z czarnymi kończynami.
- Bloom pozostaje lokalny przy płomieniu, portalu i magii. Nie zasłania anatomii ani kierunku ataku.
- Mgła i tło tworzą głębię; nie zlewają murów, zamku i postaci w jedną warstwę.

Nocny profil jest wymaganiem tego dziedzińca. Inna lokacja może mieć inne światło, zachowując matowe materiały, paletę cieni, sposób rysowania i czytelność.

## 6. Wymagania dla kategorii assetów

### Paladyn i inni opancerzeni bohaterowie

- Paladyn: jasna zbroja o odcieniu kości słoniowej/przygaszonej stali, wąskie stare złoto, granatowe miękkie części i burgundowa tkanina.
- Płyty nakładają się logicznie; widoczne są pas, miejsca zgięcia, podbicie i odstępy między segmentami.
- Hełm ma średniowieczną konstrukcję i ciemne wnętrze wizury. Brak świecącego elektronicznego wizjera.
- Stalowy but zachowuje twardą bryłę. Palce nie opadają jak miękka skarpetka; nogi nie skręcają się nienaturalnie.
- Chwyt broni dopasowany do konkretnej rękojeści. Przy stałym chwycie palce są zwinięte; dłoń nie zaciska się przez ostrze ani okucie.
- Wojownik w bieżącym projekcie pozostaje bez zmian do osobnego zlecenia. Praca nad paladynem nie jest zgodą na podmianę wojownika.

### Mag

Adaptacja zachowawcza: istniejący model jest lubiany przez użytkownika. Zachowaj wysoki zagięty kapelusz z szerokim nieregularnym rondem, jasne długie włosy, twarz w cieniu ronda, poszarpaną granatowo-fioletową warstwową szatę, narzutkę/kaptur, oliwkowe rękawice, pasy i księgę na łańcuchu.

Kostur pozostaje rozpoznawalny: ciemne skręcone drewno, metalowa klatka/korona głowicy i mały bursztynowy motyw ognia. W przygotowaniu siatki postaci kostur jest osobnym assetem. Nie dodawaj ciężkiej zbroi, nowych wielkich naramienników, nadmiaru haftów, klejnotów lub neonowych run.

### Potwory pierwszej areny

Wspólna rodzina ma rogaty łeb, ostre uszy, pazury, ogon i agresywny organiczny charakter małego przeciwnika walczącego wręcz. Nie zamieniaj chochlika w wielkiego bossa, smoka ani uroczą maskotkę. Zachowaj różnice istniejących sylwetek.

| Wariant | Niezmienniki sylwetki | Paleta i detal |
|---|---|---|
| Ogień | **Bez skrzydeł**, żylasty pochylony korpus, długi ogon | Spalony brąz i przygaszona czerwień, węglowe rogi, kościane pazury, drobny pomarańczowy żar |
| Woda/lód | **Dwa skrzydła**, rogi, ogon, długie przedramiona | Petrol/granat, jasne rogi, lokalny szron i kryształy, mały chłodny akcent oczu |
| Wiatr | Dwa skrzydła, ogon, smukła budowa | Łupkowa szarość, chłodny fiolet, delikatne żyłki i postrzępienia błon |
| Ziemia | Dwa skrzydła, ogon, mocniejsze naturalne grzbiety | Ochra, umbra i ciemny kamień, matowa keratyna; błony nadal elastyczne |

Skrzydła są osobnymi kończynami przy łopatkach, z czytelną nasadą, palcami i błoną. Dłonie przed skrzydłami nie mogą zostać z nimi połączone. Naturalnych grzbietów i łusek nie przedstawiaj jako metalowych płyt kombinezonu.

Elita ognistego chochlika korzysta z tej samej rodziny i istniejących sygnałów wzmocnienia. Nie wymyślaj jej dodatkowych głów, kończyn lub osobnego gatunku bez takiego zadania.

### Architektura i arena

- Gotycki kamienny dziedziniec: ostrołuki, smukłe piony, profilowane kolumny, mury z wiarygodnych bloków, gzymsy i podstawy.
- Duże nieregularne płyty posadzki, fazowane i miejscami ukruszone; szczeliny mają kierunek i głębokość. W centrum intarsjowany sigil z obręczami i promieniami.
- Czerwone ciężkie sztandary z oszczędnym symbolem, kute kosze pochodni, złamane elementy i lokalne skupiska gruzu.
- Odległy zamek i most tworzą skalę oraz głębię. Tło nie konkuruje detalem ani jasnością z walką.
- Nie pomijaj profili, grubości, klińców i żłobień pod pretekstem stylizacji. Nie zastępuj całej architektury płaskim obrazkiem.
- Detal posadzki nie może tworzyć nieczytelnego pola wysokich przeszkód pod stopami. Widoczna nierówność i kolizja muszą wspierać naturalny ruch.

### Broń i wyposażenie

**Stały wymóg użytkownika dla wszystkich obecnych i przyszłych modeli:** każda dłoń trzymająca broń
ma zaciskać palce na rękojeści lub drzewcu. Cztery palce obejmują uchwyt, a kciuk domyka chwyt;
broń nie może wisieć przy otwartej dłoni ani przechodzić przez palce. Dotyczy obu rąk przy broni
dwuręcznej i każdej broni osobno przy dwóch klingach. Średnicę uchwytu i punkt gniazda trzeba
dopasować do wnętrza dłoni. Zachowanie ma być widoczne w spoczynku, biegu i całej serii ciosów.
Dłoń wolna przy rzucaniu czaru albo obsłudze cięciwy może przyjąć właściwy dla czynności układ;
nie jest to wyjątek pozwalający na otwartą dłoń trzymającą rękojeść.

Ten wymóg wpisuj do przyszłych promptów i weryfikacji modeli. Sama referencja z otwartą dłonią
lub brak kości palców w starym rigu nie zwalniają z przygotowania zamkniętego chwytu w siatce
albo animacji. Wyposażenie nadal pozostaje osobnym assetem i nie wolno zespawać go z dłonią.

Średniowieczna konstrukcja i proporcje odpowiadające chwytowi oraz animacji. Ostrze, jelec, rękojeść i głowica są rozdzielone konstrukcyjnie. Krawędź może mieć malowany jasny akcent, ale nie chromowane odbicie otoczenia. Korbacz wymaga osobnej kuli i łańcucha zdolnego do ruchu; nie wolno scalać ich w sztywny pręt. Tarcza, kostur i broń wymienna pozostają oddzielne od ciała.

### Skille i VFX

- Kształt efektu ma wspierać gest i kierunek działania: łuk cięcia, punkt trafienia, zasięg fali, krótki impuls zaklęcia.
- Płomienie, iskry, woda i magia mają malowane, kierunkowe formy oraz oszczędne smugi. Nie stosuj wszędzie tej samej rozmytej świecącej kuli.
- Najsilniejszy akcent pojawia się w chwili trafienia lub kulminacji, potem szybko ustępuje miejsca czytelności postaci.
- Obrys niebezpiecznego obszaru i zapowiedź ciosu muszą pozostać widoczne. Barwy sygnałów mechaniki wynikają z istniejących zasad gry; nie wolno ich zmieniać tylko dla zgodności z paletą stroju.
- Efekty osadzone na kości poruszają się z właściwą dłonią, bronią lub ogonem. Aura nie zasłania całej sylwetki.
- Referencja do modelowania jest bez dymu, pyłu i zasłaniającej magii. Prezentacja skilla może je zawierać w osobnej warstwie.

## 7. Format wyniku trzeba wskazać w promptach

| Cel | Wymagana prezentacja |
|---|---|
| Koncepcja kierunku | Pełna sylwetka lub scena, docelowe materiały i światło; można dodać spójny tył i detal |
| Pojedyncze wejście postaci do Meshy | Dokładnie jedna kompletna postać, neutralne szare tło, widoczne wszystkie kończyny, bez ramek, detali w narożnikach i wyposażenia zasłaniającego ciało |
| Model pod istniejący rig | Poza T lub dopasowana poza A, określone osie/przód, skala, rozdzielone kończyny; najpierw sprawdzenie źródłowego szkieletu |
| Pojedynczy moduł otoczenia | Jeden obiekt, pełny obrys, neutralne światło, czytelna podstawa i grubość; docelowy wymiar/punkt łączenia podany osobno |
| Tekstura | Określona mapa: kolor, normalne, szorstkość itd.; dla powierzchni powtarzalnych jawne wymaganie bezszwowości |
| Wdrożenie w Unity | Oceniane zarówno w świetle neutralnym, jak i w oświetleniu areny, z normalnej kamery gry i w ruchu |

Nie wysyłaj do generacji pojedynczego modelu planszy z trzema postaciami, zbliżeniem głowy i podpisami. Plansza koncepcyjna i obraz wejściowy 3D to różne wyniki pracy. Wybór pozy A/T musi uwzględniać rozdzielenie rąk, skrzydeł i tkanin; sama poza T nie gwarantuje zgodności z animacjami.

## 8. Stały blok stylu do kopiowania

Poniższy blok jest wspólną bazą. Trzeba dopisać konkretny temat, niezmienniki i format; sam blok nie opisuje kompletnego assetu.

```text
Styl gry Turris Damnatorum: dojrzałe gotyckie anime fantasy, bogata ilustracja
koncepcyjna przeznaczona do wiernego odtworzenia w 3D. Wyrazista sylwetka,
wiarygodna anatomia i konstrukcja, rzeźbiarskie duże formy, precyzyjny oszczędny
kontur, kontrolowane malowane płaszczyzny światła i cienia. Zachowaj wszystkie
istotne warstwy, profile, podziały i detale referencji. Duża forma ma pierwszeństwo
przed małym detalem, ale nie wolno zastępować szczegółowego projektu prostymi bryłami.

Materiały matowe: przygaszona stal i kość słoniowa, stare złoto, ciężkie tkaniny,
zużyta skóra, malowany kamień i organiczna keratyna — dobierz je do tematu.
Przygaszone barwy, chłodne barwne cienie, małe ciepłe akcenty ognia lub magii.
W ciemnych partiach zachowaj czytelne warstwy i anatomię. Emisja lokalna i oszczędna.

Bez chromu, mokrego plastiku, futurystycznych kombinezonów, neonowych obwodów,
fotograficznego szumu, chibi, karykaturalnej anatomii, przesadnej broni i grubego
jednolitego obrysu. Bez wypalonej białej zbroi i sylwetki zalanej czernią.
Jakość wykonania i szczegółowość zgodna z dostarczoną koncepcją gotyckiego anime.
```

## 9. Szablony pełnych promptów

### A. Adaptacja istniejącej postaci lub potwora

```text
Zadanie: przygotuj [koncepcję / pojedynczą referencję do modelu 3D] dla [NAZWA],
istniejącego assetu gry Turris Damnatorum.

Referencja 1: rzeczywisty obecny model — określa tożsamość, anatomię, sylwetkę,
wyposażenie i proporcje. Referencja 2: [PLIK] — wyłącznie styl rysowania i cieniowania.
Referencja 3: [PLIK, JEŚLI POTRZEBNY] — [DOKŁADNA ROLA].

Zachowaj: [LISTA KONKRETNYCH CECH; np. kształt hełmu, liczba skrzydeł, kapelusz,
księga przy pasie, budowa ogona, układ płyt i długość szaty].
Zmiana obejmuje: [ZAKRES]. Nie dodawaj nowego wyposażenia ani ornamentu poza zakresem.
Paleta: [KOLORY I MATERIAŁY TEGO ASSETU]. Skala/funkcja: [NP. MAŁY SZYBKI PRZECIWNIK].

[WKLEJ STAŁY BLOK STYLU Z SEKCJI 8]

Wynik: [FORMAT]. [DLA POJEDYNCZEJ REFERENCJI 3D: dokładnie jedna postać,
cała sylwetka i wszystkie rogi, palce, skrzydła, ogon oraz stopy wewnątrz kadru;
neutralne jasnoszare tło, równomierne światło, otwarta poza T/A dopasowana do rigu,
rozdzielone kończyny, dłonie widoczne przed skrzydłami. Bez dodatkowych widoków,
podpisów i scenografii. Broń wymienna osobno.]

Chwyt dłoni: każda dłoń trzymająca [BROŃ] zaciska cztery palce wokół [RĘKOJEŚCI/DRZEWCA],
kciuk domyka chwyt. Dopasuj średnicę uchwytu i środek gniazda do dłoni. Bez otwartych palców
wokół trzymanej broni; dla ręki wolnej wskaż konkretną czynność. Broń pozostaje osobnym assetem.
Bez dymu, rozbłysków i efektów zasłaniających konstrukcję. Nie upraszczaj projektu.
```

### B. Arena lub scena

```text
Zadanie: koncepcja lokacji [NAZWA] do gry Turris Damnatorum, zgodna z dołączoną
referencją gotyckiego dziedzińca. Referencja kompozycji: [PLIK].
Zachowaj: [PLAN, PUNKTY ORIENTACYJNE, SKALA, DROGI RUCHU].
Architektura: [KONKRETNE MODUŁY I PROFILE]. Posadzka: [KONSTRUKCJA I MOTYW].
Stan zachowania: [GDZIE RUINY, UKRUSZENIA I GRUZ; GDZIE POWIERZCHNIE SPOKOJNE].
Pora dnia i światło: [PROFIL; DLA PIERWSZEGO DZIEDZIŃCA CHŁODNA NOC + POCHODNIE].

[WKLEJ STAŁY BLOK STYLU Z SEKCJI 8]

Kadr z wysokości i perspektywy zbliżonej do kamery trzecioosobowej gry.
Czytelne trzy plany głębi, miejsce na walkę, logiczne grubości i połączenia brył.
Nie pomijaj profili kolumn, klińców, faz, intarsji i zniszczeń obecnych w referencji.
Detal ma wspierać kompozycję i ruch. Bez UI, napisów, znaków wodnych i przypadkowych
nowoczesnych elementów. Nie zastępuj architektury dekoracyjną płaską planszą.
```

### C. Pojedynczy moduł lub przedmiot

```text
Jeden asset do Turris Damnatorum: [NAZWA], funkcja [FUNKCJA], wymiar [WYMIAR],
punkt oparcia/łączenia/chwytu [OPIS]. Referencje: [TOŻSAMOŚĆ] i [STYL].
Zachowaj konstrukcję: [ELEMENTY, GRUBOŚCI, PROFILE, PODZIAŁ NA CZĘŚCI].
Materiały i zużycie: [KONKRET].
[WKLEJ STAŁY BLOK STYLU Z SEKCJI 8]
Pełny obiekt w kadrze, neutralne tło i światło, bez scenografii, podpisów, płomieni
i cieni zasłaniających formę. Elementy ruchome i wymienne mają pozostać rozdzielone.
```

### D. Tekstura lub mapa materiału

```text
Mapa [KOLORU / NORMALNYCH / SZORSTKOŚCI] dla [POWIERZCHNIA] w stylu gotyckiego anime
Turris Damnatorum. [BEZSZWOWA / KONKRETNY ATLAS UV].
Paleta: [KOLORY]. Konstrukcja i skala detalu: [OPIS].
Dla mapy koloru: malarski pigment, spokojne duże plamy, oszczędny detal materiału;
bez kierunkowego światła studyjnego, białych refleksów, perspektywy i dużych
wypieczonych cieni. Nie umieszczaj gotowego obiektu 3D na obrazku tekstury.
Wynik ma być mapą wskazanego typu, nie ilustracją prezentującą materiał.
```

### E. Koncepcja skilla / VFX

```text
Efekt [NAZWA SKILLA] w stylu gotyckiego anime Turris Damnatorum.
Mechanika i czas: [ZAPOWIEDŹ / TRAFIENIE / WYGASZENIE, ZASIĘG, KIERUNEK].
Punkt emisji: [DŁOŃ / BROŃ / PODŁOŻE], żywioł i paleta [OPIS].
Główna forma efektu: [ŁUK / SMUGA / FALA / POCISK / IMPULS].
Malowane kierunkowe kształty, czytelny akcent kulminacji, oszczędny bloom i cząstki.
Zachowaj widoczność postaci, punktu trafienia i telegraphów. Kolory sygnałów walki
pozostają zgodne z istniejącą mechaniką. [WIDOK KLUCZOWEJ KLATKI / KOLEJNE FAZY].
Nie dodawaj wielkiej świetlistej kuli lub dymu zamiast konkretnego kształtu działania.
```

W każdej gotowej wersji zastąp pola w nawiasach. Nie wysyłaj do generatora nieuzupełnionego szablonu ani wszystkich kategorii naraz. Dla jednego assetu wypisz tylko materiały i cechy, które rzeczywiście go dotyczą.

## 10. Kontrola wyniku przed uznaniem za zgodny

- [ ] Widać ten sam asset i jego cechy rozpoznawcze; zmiany nie wykraczają poza zadanie.
- [ ] Zachowane liczba kończyn i skrzydeł, wyposażenie, warstwy oraz istotne profile referencji.
- [ ] Sylwetka pozostaje czytelna po zmniejszeniu obrazu do miniatury; w grze sprawdzona z rzeczywistej kamery.
- [ ] Matowa stal, tkanina, skóra i kamień są rozróżnialne; nie wyglądają jak ten sam plastik.
- [ ] Cienie pokazują formę, jasne płyty nie są wypalone, ciemna postać nie jest czarną plamą.
- [ ] Drobny detal jest podporządkowany formie; brak fotograficznego szumu i przypadkowego ornamentu.
- [ ] Dłonie, nadgarstki, kolana i stopy mają wiarygodny układ; broń jest prawidłowo trzymana.
- [ ] Dla referencji do 3D: jedna pełna postać/obiekt, brak zasłaniających efektów i połączonych części.
- [ ] Dla modelu: obejrzane przód, tył, boki, ¾ i detale; poprawne tekstury, skala, wagi i połączenia.
- [ ] Dla animowanego modelu: ocenione chód/bieg, sekwencja ataków, powrót, śmierć oraz ruch tkanin, ogona i skrzydeł.
- [ ] Dla wdrożenia: obejrzany rzeczywisty render Unity w świetle areny; statyczna ilustracja nie zastępuje tej oceny.

Ocena techniczna „model ma UV, szkielet i przechodzące testy” nie oznacza automatycznie zgodności artystycznej. Jeśli wynik odbiega od koncepcji, nazwij różnicę i popraw właściwy etap: projekt, siatkę, teksturę, materiał, światło albo animację.

## 11. Zasady pracy nad istniejącym projektem

- Hell Unleashed pozostaje bez zmian. Korzystamy z kopii danych i modeli w Turris.
- Stare assety i pełne źródła nowych generacji zachowujemy, a podmiany mają umożliwiać powrót.
- W pierwszej kolejności wykorzystujemy istniejące szkielety i animacje, dopasowując siatkę oraz wagi. Nie zakładaj potrzeby płatnego rigowania od zera.
- Wspólny styl nie jest zgodą na podmianę wszystkich postaci, zmianę parametrów skilli, hitboxów lub zasad walki.
- Surowy model z generatora, koncepcja, rigowana siatka i asset gotowy do licznej grupy przeciwników to osobne etapy. Zapisuj stan rzeczywisty.
- Parametry i koszty narzędzi mają własne manifesty zadań. Nie zapisujemy bieżących cen, salda ani wersji modeli jako trwałych wymagań estetycznych.

Historia wdrożeń i aktualny stan techniczny: [PROGRESS.md](PROGRESS.md). Zakres pierwszego PoC: [POC_GOTYCKIE_ANIME.md](POC_GOTYCKIE_ANIME.md). W razie rozbieżności historycznego opisu PoC z tą specyfikacją stosuj niniejszy dokument do kierunku wizualnego, a bieżący `PROGRESS` do stanu wykonania.
