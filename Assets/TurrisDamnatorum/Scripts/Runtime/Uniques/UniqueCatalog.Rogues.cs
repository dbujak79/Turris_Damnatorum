namespace Turris
{
    public static partial class UniqueCatalog
    {
        // ================================================================== Łotrzyk, Łucznik, Zabójca, Łucznik arkanów

        static void Rogues()
        {
            const string L = "class_rogue", K = "class_hu_archer", Z = "class_hu_assassin", Q = "class_hu_arcane_archer";

            // ---- Łotrzyk
            C("L01", "rogue.false_king", L, "Maska fałszywego króla", ItemKind.Head, null, Witness, 2,
                "Fałszywy świadek (cecha Łotrzyka).",
                "Oszukani wrogowie atakują wskazanego przeciwnika.",
                "Gdy Fałszywy świadek oszuka wrogów, wskazany prawdziwy przeciwnik staje się obiektem oszustwa: oszukani w {range m} atakują jego, a ich ciosy naprawdę go ranią (do {max_hits} ciosów, każdy najwyżej {hit_cap} × twój lekki cios).",
                "Znormalizowany budżet wrogiej ofensywy; samotny boss korzysta ze zwykłego wabika.",
                "Łotrzyk z Fałszywym świadkiem i hu_dagger.",
                "Pole pułapek: Sidła hu_snare i Bomba siarkowa hu_sulfur_bomb.",
                "Oszustwo kończy się, a wrogowie wracają do ciebie.",
                "range", 8f, "max_hits", 6, "hit_cap", 2.5f)
                .S(StatType.MoveSpeed, 6).S(StatType.MaxHealth, 25);

            C("L02", "rogue.professional_cheat", L, "Rękawice zawodowego oszusta", ItemKind.Hands, null, Trick, 3,
                "Kradzież sztuczki (cecha Łotrzyka).",
                "Skradziona sztuczka staje się pułapką.",
                "Wydanie skradzionej sztuczki kładzie ją jako pułapkę w miejscu celowania ({arm_seconds s} uzbrajania, {trap_seconds s}); kontakt ofiary w {trigger_radius m} wykonuje jej profil.",
                "Brak natychmiastowej sztuczki; ten sam profil nie istnieje w magazynie i na ziemi.",
                "Łotrzyk z Kradzieżą sztuczki i Rzutem nożami hu_knife_throw.",
                "Medalion sygnału z otchłani – zdalne odpalanie; Sidła.",
                "Trzeba sprowadzić cel na pułapkę.",
                "arm_seconds", 0.6f, "trap_seconds", 12f, "trigger_radius", 1.6f, "place_range", 8f)
                .S(StatType.MaxStamina, 20).S(StatType.MoveSpeed, 4);

            C("L03", "rogue.duds", L, "Pas niewypałów", ItemKind.Belt, null, null, 2,
                "Bomba siarkowa hu_sulfur_bomb oraz Sidła hu_snare albo cios w plecy.",
                "Bomba przykleja się do ofiary i czeka na Sidła lub cios w plecy.",
                "Bomba siarkowa przykleja się do pierwszej ofiary zamiast wybuchnąć (najwyżej {max_bombs}, {bomb_seconds s}). Własne Sidła na nosicielu albo ręczny cios w plecy odpalają ładunek.",
                "Brak natychmiastowego wybuchu; wybuch nie jest nowym ciosem w plecy, ładunki nie łańcuchują pełnych wybuchów.",
                "Łotrzyk z Bombą siarkową i Sidłami.",
                "Ruchliwy Łotrzyk wybierający kolejność; Sztylet niedokończonej ucieczki.",
                "Bomba bez zapalnika przepada.",
                "max_bombs", 3, "bomb_seconds", 8f)
                .Sk("hu_sulfur_bomb").S(StatType.MaxStamina, 15).S(StatType.FireDamage, 10);

            C("L04", "rogue.unfinished_escape", L, "Sztylet niedokończonej ucieczki", ItemKind.Weapon, "hu_dagger", null, 4,
                "Krok cienia hu_shadow_step oraz Sidła lub Bomba siarkowa w zwykłym slocie.",
                "Krok cienia zostawia w punkcie startu opłaconą pułapkę.",
                "Udany Krok cienia kładzie w punkcie startu pułapkę z pierwszego gotowego skilla pułapki w slotach (Sidła hu_snare albo Bomba siarkowa hu_sulfur_bomb), płacąc jego normalny koszt i odnowienie.",
                "Gdy pułapka nie jest gotowa, Krok działa normalnie (komunikat), bez częściowej opłaty.",
                "Łotrzyk z tym sztyletem, Krokiem cienia i Sidłami.",
                "Pas niewypałów – bomba przyklejona do pierwszej ofiary; Bomba siarkowa.",
                "Dwa koszty i dwa odnowienia.",
                "unused", 0)
                .Sk("hu_shadow_step").S(StatType.MoveSpeed, 6).S(StatType.BleedDamage, 10);

            C("L05", "rogue.abyss_signal", L, "Medalion sygnału z otchłani", ItemKind.Amulet, null, null, 4,
                "Rzut nożami hu_knife_throw i własne pułapki.",
                "Pułapki odpala nóż, nie wróg.",
                "Własne miny i pułapki nie wybuchają od wejścia wroga – ręczny nóż przelatujący w {signal_radius m} od znacznika odpala przygotowany ładunek (raz).",
                "Trzeba trafić sygnałem; wybuch nie tworzy nowych noży, cudze miny bez zmian.",
                "Łotrzyk z Rzutem nożami i Sidłami hu_snare z pieczęcią Wnyki (sama strefa Sideł nie jest pułapką).",
                "Szeregi zapalników, Rękawice zawodowego oszusta.",
                "Pułapka bez sygnału nie chroni.",
                "signal_radius", 1.2f)
                .Sk("hu_knife_throw").S(StatType.MaxStamina, 15).S(StatType.PhysicalDamage, 8);

            // ---- Łucznik
            C("K01", "archer.bowstring_weaver", K, "Rękawice tkacza cięciw", ItemKind.Hands, null, Nails, 2,
                "Gwoździe łowcy (cecha Łucznika).",
                "Groty łączą trafione cele liniami, które ranią przy wyrwaniu.",
                "Wyrwanie grotów nie wypłaca ich skupionej mocy: kolejne kotwice (do {max_anchors}) łączą odcinki, a każdy wróg przecinający odcinek dostaje {line_share} sumy grotów (jeden podzielony budżet).",
                "Utrata skupionego wyrwania; ograniczona liczba odcinków.",
                "Łucznik z Gwoździami łowcy i hu_bow.",
                "Sieć z Salwy hu_volley; Pas niedokończonej salwy.",
                "Pojedynczy boss dostaje mniej.",
                "max_anchors", 4, "line_share", 0.45f, "width", 0.9f)
                .S(StatType.AttackSpeed, 6).S(StatType.MaxStamina, 15);

            C("K02", "archer.exiled_shooter", K, "Buty wygnanego strzelca", ItemKind.Feet, null, Post, 3,
                "Stanowisko strzeleckie (cecha Łucznika).",
                "Mocny strzał zamienia cię miejscami ze stanowiskiem.",
                "Mocny strzał przy aktywnym stanowisku wylatuje z twojej pozycji, a potem zamieniasz się miejscami ze stanowiskiem (stanowisko zostaje w twoim starym miejscu); osobne odnowienie {swap_cooldown s}.",
                "Brak dodatkowej niewrażliwości; gdy zamiana niedostępna, mocny strzał działa normalnie.",
                "Łucznik ze Stanowiskiem i hu_bow.",
                "Dwa kąty ostrzału, Pas niedokończonej salwy.",
                "Teleport w niebezpieczne miejsce.",
                "swap_cooldown", 4f)
                .S(StatType.MoveSpeed, 6).S(StatType.StaminaRegen, 8).S(StatType.PhysicalDefense, 15);

            C("K03", "archer.gravediggers_bow", K, "Łuk grabarza", ItemKind.Weapon, "hu_bow", null, 4,
                "Podstawowe ataki tego łuku.",
                "Lekki atak to pchnięcie strzałą, która leci dalej przez ofiarę.",
                "Lekki atak jest pchnięciem strzałą w zwarciu ({reach m}); strzała przelatuje przez ofiarę dalej ({pass_share} obrażeń, bez powtórnego trafienia tej samej ofiary). Mocny atak to zwykły strzał.",
                "Utrata szybkiego dystansu; łuk zajmuje obie ręce (bez fokusu).",
                "Łucznik z tym łukiem.",
                "Gwoździe łowcy, przebijanie tylnego szeregu, Kaptur odwróconego celownika.",
                "Lekkie ataki tylko w zwarciu.",
                "reach", 2.2f, "pass_share", 0.6f)
                .S(StatType.MaxStamina, 15);

            C("K04", "archer.unfinished_volley", K, "Pas niedokończonej salwy", ItemKind.Belt, null, null, 4,
                "Salwa hu_volley i ręczny mocny strzał.",
                "Salwa ustawia rząd strzał w ziemi, a mocny strzał wskazuje kierunek.",
                "Salwa nie strzela od razu: wbija rząd strzał przed tobą ({arm_seconds s} uzbrajania, {row_seconds s}). Następny mocny strzał wystrzeliwuje cały rząd jeden raz w swoim kierunku.",
                "Opłacona Salwa i osobny koszt mocnego strzału; rząd nie strzela sam.",
                "Łucznik z Salwą i hu_bow.",
                "Stanowisko strzeleckie (źródło przed zamianą), pułapki na szarżujących.",
                "Opóźniona salwa.",
                "arm_seconds", 0.4f, "row_seconds", 10f)
                .Sk("hu_volley").S(StatType.MaxStamina, 15).S(StatType.MoveSpeed, 4);

            C("K05", "archer.reversed_sight", K, "Kaptur odwróconego celownika", ItemKind.Head, null, null, 4,
                "Deszcz strzał hu_arrow_rain i podstawowy ostrzał.",
                "Deszcz strzał podąża za ofiarą twojego strzału.",
                "Aktywny Deszcz strzał przesuwa się za ofiarą ostatniego podstawowego ręcznego strzału ({follow_speed m}/s); nowy trafiony cel przejmuje deszcz.",
                "Bez stałego punktu i bez odnowienia czasu; salwa wybiera jednego przewodnika.",
                "Łucznik z Deszczem strzał i hu_bow.",
                "Artyleria między elitami, Łuk grabarza.",
                "Deszcz ucieka z przygotowanego miejsca.",
                "follow_speed", 5f)
                .Sk("hu_arrow_rain").S(StatType.MaxStamina, 15).S(StatType.AttackSpeed, 5);

            // ---- Zabójca
            C("Z01", "assassin.own_funeral", Z, "Całun własnego pogrzebu", ItemKind.Body, null, Shadow, 2,
                "Przygwożdżony cień (cecha Zabójcy).",
                "Cień ofiary idzie przy twoich stopach.",
                "Przygwożdżony cień ofiary towarzyszy właścicielowi: ręczne ataki w {range m} wokół ciebie trafiają przez niego połączoną ofiarę. Każdy ręczny atak, który tak trafi, kosztuje {life_cost} maks. życia.",
                "Koszt nie jest ciosem wroga; nie korzysta z redukcji i może zabić.",
                "Zabójca z Przygwożdżonym cieniem i hu_greatsword.",
                "Wolna broń, walka z daleka przez cień.",
                "Życie płaci każdy atak.",
                "range", 2.6f, "life_cost", 0.02f)
                .S(StatType.MaxHealth, 30).S(StatType.MoveSpeed, 4).S(StatType.PhysicalDamage, 6);

            C("Z02", "assassin.deferred_verdict", Z, "Pierścień zwleczonego wyroku", ItemKind.Ring, null, Artery, 3,
                "Otwarta tętnica (cecha Zabójcy) i premia egzekucji.",
                "Premia egzekucji staje się raną na później.",
                "{wound_share} premii egzekucji trafienia nie zwiększa ciosu, tylko staje się raną (krwawienie z tym budżetem przez {wound_seconds s}); przewodzenie może ją rozliczyć.",
                "Opóźnienie; rana nie liczy egzekucji drugi raz.",
                "Zabójca z Otwartą tętnicą i hu_dagger.",
                "Grymuar błyskawicy (przewodzenie), Igła grobowej szwaczki.",
                "Cios egzekucji jest słabszy w chwili trafienia.",
                "wound_share", 0.5f, "wound_seconds", 4f)
                .S(StatType.ExecuteDamage, 15).S(StatType.BleedDamage, 12).S(StatType.MaxStamina, 10);

            C("Z03", "assassin.grave_needle", Z, "Igła grobowej szwaczki", ItemKind.Weapon, "hu_dagger", null, 4,
                "Własna pozostała rana krwawienia na ofierze.",
                "Mocny cios wyciąga krwawienie do pułapki w ziemi.",
                "Mocny atak wyciąga {pull_share} pozostałego krwawienia ofiary i zapisuje je jako pułapkę w ziemi (najwyżej {max_traps}, {trap_seconds s}). Kontakt innego wroga przenosi ranę na niego.",
                "Rana naprawdę znika z ofiary w tej części – nie jest klonowana ani odnawiana.",
                "Zabójca z tą igłą i Krwawym cięciem skill_rend.",
                "Cięcie krzyżowe, Rękawice karminowego węzła.",
                "Pierwsza ofiara krwawi słabiej.",
                "pull_share", 0.6f, "max_traps", 3, "trap_seconds", 10f, "trigger_radius", 1.3f)
                .S(StatType.BleedDamage, 15);

            C("Z04", "assassin.silent_witness", Z, "Maska milczącego świadka", ItemKind.Head, null, null, 4,
                "Ręczne trafienie w plecy i Rzut nożami hu_knife_throw.",
                "Premia za cios w plecy czeka na następny Rzut nożami.",
                "{store_share} premii trafienia w plecy zostaje zachowane jako ładunek na {charge_seconds s}. Następny Rzut nożami dzieli ładunek między noże.",
                "Słabsze zaskoczenie; zachowany dodatek nie dostaje drugiej Zasadzki.",
                "Zabójca z hu_dagger i Rzutem nożami (hu_offdagger).",
                "Podejście za plecy → dystans bez łuku.",
                "Bez rzutu ładunek przepada.",
                "store_share", 0.5f, "charge_seconds", 5f)
                .Sk("hu_knife_throw").S(StatType.PhysicalDamage, 10).S(StatType.MaxStamina, 15);

            C("Z05", "assassin.crimson_knot", Z, "Rękawice karminowego węzła", ItemKind.Hands, null, null, 4,
                "Cięcie krzyżowe hu_cross_cut i własne krwawiące ofiary.",
                "Ofiary jednego Cięcia zostają zszyte krwią.",
                "Różne ofiary trafione jednym Cięciem krzyżowym są zszyte na {knot_seconds s} (do {max_links}). Ręczne trafienie jednej wydaje {spend_share} jej pozostałej rany i rozdziela ją między pozostałe zszyte ofiary (najwyżej raz na {interval s}).",
                "Zużycie realnej rany pierwszej ofiary; odpowiedź nie uruchamia węzła ponownie.",
                "Zabójca z Cięciem krzyżowym i hu_dagger.",
                "Egzekucyjne krwawienie w hordzie, Igła grobowej szwaczki.",
                "Pierwsza ofiara traci ranę.",
                "knot_seconds", 6f, "max_links", 4, "spend_share", 0.4f, "interval", 0.5f)
                .Sk("hu_cross_cut").S(StatType.BleedDamage, 15).S(StatType.AttackSpeed, 5).S(StatType.MaxHealth, 15);

            // ---- Łucznik arkanów
            C("Q01", "arcane_archer.seventh_seal", Q, "Łuk siódmej pieczęci", ItemKind.Weapon, "hu_bow", SpellArrow, 4,
                "Strzała zaklęcia (cecha Łucznika arkanów).",
                "Osadzony czar rozkłada się pasem wzdłuż toru strzały.",
                "Czar osadzony w strzale nie wybucha kręgiem w punkcie trafienia: rozkłada się pasem od miejsca strzału do trafienia (segmenty co {step m}, szerokość {width m}), dzieląc budżet czaru na długość.",
                "Mniejsze skupienie na jednym celu; cel przecinający kilka segmentów dostaje trafienie raz.",
                "Łucznik arkanów ze Strzałą zaklęcia, tym łukiem i Novą ognia.",
                "Korytarze mrozu i ziemi, Amulet niedokończonej inkantacji.",
                "Pojedynczy cel dostaje mniej.",
                "step", 1.5f, "width", 1.3f)
                .S(StatType.SpellPower, 10);

            C("Q02", "arcane_archer.stolen_elements", Q, "Kołczan skradzionych żywiołów", ItemKind.Belt, null, Answer, 3,
                "Strzała odpowiedzi (cecha Łucznika arkanów).",
                "Część reakcji czeka na następny strzał.",
                "{store_share} obrażeń reakcji wywołanej przez Strzałę odpowiedzi nie trafia pierwotnej ofiary, tylko trafia do ładunku ({charge_seconds s}). Następny ręczny strzał niesie go do nowego celu.",
                "Jeden ładunek; pudło go marnuje, reakcja z ładunku nie ładuje go ponownie.",
                "Łucznik arkanów ze Strzałą odpowiedzi i hu_bow.",
                "Przygotowanie na zwykłym wrogu i przeniesienie w elitę.",
                "Pierwszy cel dostaje mniej.",
                "store_share", 0.5f, "charge_seconds", 5f)
                .S(StatType.MaxStamina, 15).S(StatType.MaxMana, 15).S(StatType.FireDamage, 5).S(StatType.FrostDamage, 5);

            C("Q03", "arcane_archer.heretic_choir", Q, "Łuk heretyckiego chóru", ItemKind.Weapon, "hu_bow", null, 4,
                "Dodatkowe strzały podstawowe i skille w trzech slotach.",
                "Dodatkowe strzały przyjmują żywioły twoich skilli.",
                "Każda dodatkowa strzała podstawowa (indeks 1, 2…) przyjmuje żywioł skilla w kolejnym slocie (1→2→3, w kółko); {convert_share} jej obrażeń staje się tym żywiołem. Slot pusty albo fizyczny – strzała zostaje fizyczna.",
                "Podział budżetu, bez pełnej kopii każdego żywiołu.",
                "Łucznik arkanów z tym łukiem, Rozszczepieniem i skillami trzech żywiołów.",
                "Strzała odpowiedzi; Rękawice kołczanu między światami.",
                "Rozproszona inwestycja w żywioły.",
                "convert_share", 0.7f)
                .S(StatType.FireDamage, 6).S(StatType.FrostDamage, 6).S(StatType.LightningDamage, 6).S(StatType.MaxStamina, 10);

            C("Q04", "arcane_archer.unfinished_incantation", Q, "Amulet niedokończonej inkantacji", ItemKind.Amulet, null, SpellArrow, 4,
                "Strzała zaklęcia (cecha Łucznika arkanów).",
                "Dostarczony czar zostaje runą, którą odpala atak innym żywiołem.",
                "Czar dostarczony strzałą zaklęcia zostaje runą w miejscu trafienia ({rune_seconds s}). Następny ręczny atak innym żywiołem trafiający w promieniu {radius m} odpala zachowany czar zmieniony na żywioł zapalnika.",
                "Oba działania płatne normalnie; runa może wygasnąć bez zwrotu.",
                "Łucznik arkanów ze Strzałą zaklęcia i hu_bow.",
                "Łuk siódmej pieczęci, techniki łuku jako zapalniki.",
                "Opóźnienie i ryzyko wygaśnięcia.",
                "rune_seconds", 5f, "radius", 2.5f)
                .S(StatType.MaxMana, 20).S(StatType.SpellPower, 10);

            C("Q05", "arcane_archer.between_worlds", Q, "Rękawice kołczanu między światami", ItemKind.Hands, null, null, 4,
                "Dodatkowe podstawowe pociski łuku (np. Rozszczepienie).",
                "Dodatkowe strzały trafiają do kołczanu, a mocny strzał je uwalnia.",
                "Dodatkowe strzały podstawowe nie lecą od razu: trafiają do widocznego kołczanu (do {capacity}, {hold_seconds s}). Następny mocny strzał najpierw uwalnia zgromadzone strzały w swoim kierunku.",
                "Odłożony DPS i termin; zachowują stary żywioł i budżet.",
                "Łucznik arkanów z hu_bow i perkiem Rozszczepienie.",
                "Łuk heretyckiego chóru, Strzała odpowiedzi.",
                "Mniej strzał w danej chwili.",
                "capacity", 6, "hold_seconds", 6f)
                .S(StatType.MaxMana, 15).S(StatType.AttackSpeed, 5).S(StatType.MaxStamina, 10);
        }
    }
}
