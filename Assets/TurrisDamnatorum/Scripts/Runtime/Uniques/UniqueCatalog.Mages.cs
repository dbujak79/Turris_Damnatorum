namespace Turris
{
    public static partial class UniqueCatalog
    {
        /// <summary>Kompatybilne ręczne umiejętności typu nova (komponent Nova).</summary>
        public static readonly string[] Novas = { "hu_nova_fire", "hu_nova_water", "hu_nova_air", "hu_nova_earth" };

        // ================================================================== Magowie i przywoływacz

        static void Mages()
        {
            const string M = "class_mage", F = "class_hu_mage_fire", H = "class_hu_mage_water", A = "class_hu_mage_air", E = "class_hu_mage_earth",
                S = "class_hu_summoner", T = "class_hu_battlemage";

            // ---- Mag
            C("M01", "mage.moneylenders_note", M, "Weksel lichwiarza", ItemKind.Ring, null, Debt, 2,
                "Dług arkanów (cecha Maga).",
                "Ciosy bronią spłacają dług arkanów.",
                "Podstawowe ręczne trafienia bronią (także różdżką) spłacają dług: {repay_ratio} zadanych obrażeń, najwyżej {repay_cap} many na jedną akcję. Zwykła regeneracja many nie spłaca długu.",
                "Podczas długu regeneracja nie tworzy osobnego zapasu many; flaszka many spłaca normalnie (polityka cechy).",
                "Mag z Długiem arkanów, hu_sword + hu_grimoire_fire.",
                "Kosztowna magia przeplatana ostrzałem hu_wand_air; Szata zamkniętego źródła pokazuje konflikt cechy.",
                "Regeneracja przestaje pomagać – trzeba wejść w zwarcie albo strzelać.",
                "repay_ratio", 0.25f, "repay_cap", 6f)
                .S(StatType.MaxMana, 20).S(StatType.PhysicalDamage, 8);

            C("M02", "mage.sealed_source", M, "Szata zamkniętego źródła", ItemKind.Body, null, Cloak, 2,
                "Płaszcz many (cecha Maga).",
                "Mana wydana na ochronę zostawia runy dla następnej novy.",
                "Każde {rune_mana} many wydane Płaszczem many na pochłonięcie ciosu zostawia runę (najwyżej {max_runes}). Następna ręczna nova wydaje wszystkie runy na dodatkowe wyładowanie ({rune_share} trafienia novy za runę).",
                "Brak zwrotu many; koszt czaru, samouszkodzenie i rezerwacja nie tworzą run. Jedna konsumpcja na rzut.",
                "Mag z Płaszczem many i grymuarem hu_grimoire_fire (Nova ognia).",
                "Grymuar uśpionych kręgów – runy zapisane w jednej minie; Aura niezłomności.",
                "Runy powstają tylko z przyjętych ciosów.",
                "rune_mana", 12f, "max_runes", 4, "rune_share", 0.18f)
                .S(StatType.MaxMana, 30).S(StatType.MagicDefense, 15).S(StatType.MaxHealth, 20);

            C("M03", "mage.sleeping_circles", M, "Grymuar uśpionych kręgów", ItemKind.OffHand, "hu_grimoire_fire", null, 3,
                "Ręczna nova (Nova ognia, wody, powietrza, ziemi).",
                "Nova staje się miną w wybranym punkcie.",
                "Nova zostaje umieszczona we wskazanym punkcie jako mina: uzbraja się po {arm_seconds s}, wybucha (cała nova z formami) po wejściu wroga w {trigger_radius m}. Najwyżej {max_mines} miny na {mine_seconds s}.",
                "Pełny koszt i odnowienie, brak wybuchu przy sobie; wygaśnięcie nie daje darmowego wybuchu.",
                "Mag z tym grymuarem (daje Novę ognia) i drugą novą hu_nova_water.",
                "Dług arkanów finansujący pułapki, Obrączka wiecznego odpływu i Przekaźnik.",
                "Brak ratunkowej novy przy sobie.",
                "arm_seconds", 0.8f, "trigger_radius", 1.7f, "max_mines", 3, "mine_seconds", 14f, "place_range", 9f)
                .Sk(Novas).S(StatType.MaxMana, 20).S(StatType.SpellPower, 12);

            C("M04", "mage.bound_spells", M, "Rękawice związanych zaklęć", ItemKind.Hands, null, null, 4,
                "Trzy różne ręczne ofensywne czary.",
                "Dwa czary zostawiają runy, trzeci uwalnia moc wzdłuż linii między nimi.",
                "Dwa różne ręczne czary w odstępie do {link_seconds s} zostawiają punkty runiczne (każdy wstrzymuje {hold_share} swojej mocy). Trzeci, inny czar domyka połączenie: odcinek między punktami rani za wstrzymany budżet obu.",
                "Punkty żyją {link_seconds s}; zła kolejność traci przygotowanie. Brak trzeciej darmowej kopii czarów.",
                "Mag z Lodowym odłamkiem hu_frost_shard, Piekielną kulą hu_infernal_orb i Łańcuchem hu_damned_chain.",
                "Grymuar uśpionych kręgów i narożniki areny.",
                "Pierwsze dwa czary są słabsze, a zła kolejność marnuje część mocy.",
                "link_seconds", 4f, "hold_share", 0.25f, "line_width", 1.2f, "line_mult", 1.4f)
                .S(StatType.MaxMana, 25).S(StatType.SpellPower, 10);

            C("M05", "mage.late_presence", M, "Buty spóźnionej obecności", ItemKind.Feet, null, null, 3,
                "Własny unik i następny ofensywny czar ręczny.",
                "Czar po uniku wychodzi z miejsca, które opuściłeś.",
                "Po uniku na {trace_seconds s} zostaje ślad źródła w miejscu startu. Następny ręczny czar ofensywny wychodzi ze śladu, a nie z aktualnej pozycji.",
                "Jedno źródło, bez kopii i bez dodatkowej niewrażliwości; nova wybuchnie w poprzednim miejscu.",
                "Mag z Lodowym odłamkiem hu_frost_shard.",
                "Grymuar uśpionych kręgów, zmiana kąta pocisków, Nova ognia hu_nova_fire jako pułapka w tyle.",
                "Nova ratunkowa nie wybucha przy tobie.",
                "trace_seconds", 2.5f)
                .S(StatType.MoveSpeed, 6).S(StatType.MaxMana, 15).S(StatType.StaminaRegen, 8);

            // ---- Mag ognia
            C("F01", "fire.pyre_devourer", F, "Miech pożeracza stosów", ItemKind.Amulet, null, null, 3,
                "Piekielna kula hu_infernal_orb i własne strefy ognia.",
                "Kula pożera własne pola ognia i niesie ich moc do wybuchu.",
                "Lecąca Piekielna kula pożera napotkane własne strefy ognia (do {max_fields}): pozostały czas pola × {field_share} jego budżetu dodaje się do końcowego wybuchu.",
                "Zjedzone pole znika. Cudzych pól i tego samego pola dwa razy kula nie zje.",
                "Mag ognia z hu_infernal_orb i Kręgiem żaru hu_ember_ring.",
                "Ściana ognia, Deszcz siarki hu_brimstone_rain jako przygotowanie pól; Wielki pożar.",
                "Teren przestaje palić – zostaje jeden mocny cios.",
                "max_fields", 3, "field_share", 0.6f, "eat_radius", 0.6f)
                .Sk("hu_infernal_orb").S(StatType.FireDamage, 15).S(StatType.MaxMana, 20);

            C("F02", "fire.ash_puppets", F, "Mankiety popielnych marionetek", ItemKind.Hands, null, GreatFire, 3,
                "Wielki pożar (cecha Maga ognia).",
                "Pożar zabitej ofiary wędruje do nowego celu.",
                "Gdy ofiara z własnymi rodzinami podpalenia ginie, staje się płomieniem (najwyżej {max_puppets}), który w {seek_range m} szuka nowego celu i przenosi pozostałe rany.",
                "Podpalenia krótsze ×{burn_mult}; rana nie odnawia się do pełna, a przeniesiona rodzina zachowuje źródło. Pusty budżet nie tworzy marionetki.",
                "Mag ognia z Wielkim pożarem i Novą ognia hu_nova_fire.",
                "Ściana i Piekielna kula jako źródła trzech rodzin; Szata odroczonego pogorzeliska.",
                "Krótsze palenie pojedynczego celu.",
                "max_puppets", 3, "seek_range", 8f, "burn_mult", 0.8f, "puppet_speed", 6f)
                .S(StatType.FireDamage, 12).S(StatType.MaxMana, 20);

            C("F03", "fire.borrowed_sun", F, "Młot pożyczonego słońca", ItemKind.Weapon, "hu_morningstar", Furnace, 4,
                "Żywy piec (cecha Maga ognia) i zebrany Żar.",
                "Mocny cios zamienia Żar w palenisko na ofierze.",
                "Mocny cios zużywa cały Żar i tworzy palenisko na ofierze ({ember_mult} × Żar jako budżet). Kolejne ręczne trafienia bronią w tę ofiarę wypłacają po {portion} budżetu iskrami ognia.",
                "Żar jest oddany tej sekwencji; czary nie wypłacają iskier, inne cele nie kopiują zapasu.",
                "Mag ognia z Żywym piecem, tym młotem i hu_grimoire_fire.",
                "Życie i regeneracja, Okrzyk hu_warcry, Szata odroczonego pogorzeliska.",
                "Walka w zwarciu z płonącym sobą.",
                "ember_mult", 1.6f, "portion", 0.25f, "hearth_seconds", 10f)
                .S(StatType.FireDamage, 12).S(StatType.MaxHealth, 25);

            C("F04", "fire.deferred_wildfire", F, "Szata odroczonego pogorzeliska", ItemKind.Body, null, null, 4,
                "Własne podpalenia przeciwników.",
                "Podpalenia czekają na mocny cios, który ściska je w słup ognia.",
                "Nowe własne podpalenia wrogów nie tykają, tylko gromadzą budżet (do {cap_seconds s} palenia). Ręczny mocny atak w ofiarę ściska zgromadzony budżet w słup ognia o promieniu {radius m}.",
                "Opóźnienie i wymóg mocnego trafienia; budżet wygasa po {cap_seconds s}. Własne podpalenie Żywego pieca działa normalnie.",
                "Mag ognia z hu_morningstar i Novą ognia hu_nova_fire.",
                "Wielki pożar (trzy rodziny), różdżka hu_wand_fire przygotowująca, Młot pożyczonego słońca.",
                "Brak biernego palenia – bez mocnego ciosu nic się nie dzieje.",
                "cap_seconds", 6f, "radius", 1.6f, "pillar_mult", 1.15f)
                .S(StatType.FireDamage, 15).S(StatType.MaxHealth, 25).S(StatType.PhysicalDamage, 6);

            C("F05", "fire.sky_furnace", F, "Diadem pieca pod niebem", ItemKind.Head, null, null, 4,
                "Krąg żaru hu_ember_ring i podstawowy ostrzał.",
                "Strzał oddany z własnego Kręgu żaru spada z nieba.",
                "Podstawowy strzał oddany z wnętrza własnego Kręgu żaru staje się spadającym uderzeniem w celowany punkt (opóźnienie {delay s}, promień {radius m}, zasięg {range m}) zamiast przelotu.",
                "Utrata przebicia i rykoszetu; zastępuje zwykłe zaklinanie strzału. Salwa dzieli jeden budżet.",
                "Mag ognia z hu_wand_fire i Kręgiem żaru hu_ember_ring.",
                "Stanowisko artylerii; Miech pożeracza stosów może zjeść krąg.",
                "Bezruch w kręgu i opóźnione trafienie w ruchliwy cel.",
                "delay", 0.6f, "radius", 1.5f, "range", 12f)
                .Sk("hu_ember_ring").S(StatType.FireDamage, 12).S(StatType.MaxMana, 20);

            // ---- Mag wody
            C("H01", "water.drowned_king", H, "Korona zatopionego króla", ItemKind.Head, null, Frost, 2,
                "Niezmienny szron (cecha Maga wody).",
                "Szron działa w kręgu wokół ciebie.",
                "Szron obejmuje wszystkich wrogów w kręgu {radius m} wokół postaci zamiast wskazanego celu; wróg, który wyjdzie z kręgu, traci znacznik.",
                "Brak oznaczania z daleka. Immunitety na kontrolę bez zmian.",
                "Mag wody z Niezmiennym szronem, hu_sword i hu_grimoire_water.",
                "Ogień i reakcje (Para), Nova wody hu_nova_water.",
                "Trzeba trzymać wrogów blisko siebie.",
                "radius", 3.2f)
                .S(StatType.MaxHealth, 30).S(StatType.FrostDamage, 12).S(StatType.PhysicalDefense, 15);

            C("H02", "water.wandering_river", H, "Płaszcz wędrującej rzeki", ItemKind.Body, null, Current, 3,
                "Prąd głębin (cecha Maga wody).",
                "Prowadzone pola rozwijają się w smugę za tobą.",
                "Prowadzone pole Prądu głębin rozwija się w wąską smugę wzdłuż przebytej drogi: co {segment_step m} powstaje segment o promieniu {segment_radius m}, a czasowy budżet pola dzieli się między segmenty (najwyżej {max_segments}).",
                "Dłuższa ścieżka rozcieńcza moc; pełne pole i pełna smuga nie istnieją naraz.",
                "Mag wody z Prądem głębin i Falą przypływu hu_tidal_wave.",
                "Ogień, mróz i burza w jednej rzece; buty ruchu.",
                "Skupiona moc w jednym miejscu przepada.",
                "segment_step", 1.6f, "segment_radius", 1.1f, "max_segments", 8)
                .S(StatType.MoveSpeed, 6).S(StatType.MaxMana, 25).S(StatType.FrostDamage, 8);

            C("H03", "water.silent_glacier", H, "Grymuar niemego lodowca", ItemKind.OffHand, "hu_grimoire_water", null, 3,
                "Nova wody hu_nova_water oraz własne sługi.",
                "Nova zakłada sługom lodowe kokony-amunicję.",
                "Nova wody nie wybucha przy tobie: dzieli swój budżet na kokony na sługach w {range m} (każdy {cocoon_seconds s}). Następny atak każdego sługi uwalnia jego porcję na ofierze (promień {burst_radius m}).",
                "Jeden budżet na wszystkie kokony; kokon nie jest barierą, a śmierć sługi go marnuje.",
                "Przywoływacz z tym grymuarem (Nova wody) i Sforą hu_summon_hounds.",
                "Golem hu_summon_golem, reakcje Para i Roztrzaskanie.",
                "Bez sług nova jest pusta.",
                "range", 10f, "cocoon_seconds", 6f, "burst_radius", 1.6f)
                .Sk("hu_nova_water").S(StatType.FrostDamage, 12).S(StatType.MaxMana, 20);

            C("H04", "water.eternal_ebb", H, "Obrączka wiecznego odpływu", ItemKind.Ring, null, null, 4,
                "Fala przypływu hu_tidal_wave i własne miny.",
                "Fala zbiera twoje miny i przenosi je na swój koniec.",
                "Fala przypływu zbiera własne miny na trasie (najwyżej {max_carried}) i odkłada je na końcu swojej drogi, zamiast odpychać wrogów.",
                "Brak odrzutu; mina zachowuje uzbrojenie, termin i budżet (bez odnowienia).",
                "Mag wody z Falą przypływu i Grymuarem uśpionych kręgów.",
                "Sidła hu_snare (miny Łotrzyka) i Przekaźnik.",
                "Fala nie chroni przed szarżą.",
                "max_carried", 3, "pick_width", 1.6f)
                .Sk("hu_tidal_wave").S(StatType.FrostDamage, 10).S(StatType.MaxMana, 15);

            C("H05", "water.salt_mirror", H, "Różdżka słonego lustra", ItemKind.Weapon, "hu_wand_water", null, 4,
                "Lodowy odłamek hu_frost_shard i własne pociski.",
                "Odłamek w ziemi zostawia lustro, od którego odbijają się twoje pociski.",
                "Lodowy odłamek, który doleci do końca bez trafienia, zostawia lodowe lustro (najwyżej {max_mirrors}, {mirror_seconds s}). Następny własny pocisk przechodzący przez lustro odbija się w stronę najbliższego wroga, a {frost_share} jego obrażeń staje się mrozem.",
                "Odłamek wydany na przygotowanie; lustro jednorazowe, wrogie pociski go nie używają.",
                "Mag wody z tą różdżką i Lodowym odłamkiem.",
                "Piekielna kula przez lustro – zimna wersja kuli; ostrzał zza narożnika.",
                "Odłamek przygotowujący nie rani.",
                "max_mirrors", 2, "mirror_seconds", 8f, "frost_share", 0.5f, "mirror_radius", 0.9f)
                .Sk("hu_frost_shard").S(StatType.FrostDamage, 12).S(StatType.SpellPower, 8);

            // ---- Mag powietrza
            C("A01", "air.last_breath", A, "Flakon ostatniego oddechu", ItemKind.Amulet, null, Eye, 2,
                "Oko ciszy (cecha Maga powietrza).",
                "Zgromadzone pociski wylatują jako jeden przebijający piorun.",
                "Wypuszczenie magazynu Oka ciszy tworzy jeden przebijający piorun o sumie {sum_share} budżetów pocisków (przebija wszystkich na linii {length m}).",
                "Pudło marnuje całość; brak szerokiego wachlarza.",
                "Mag powietrza z Okiem ciszy i hu_wand_air.",
                "Własna amunicja z Łańcucha i Nova powietrza; snajper.",
                "Jeden tor zamiast wachlarza.",
                "sum_share", 0.9f, "length", 18f, "width", 0.8f)
                .S(StatType.LightningDamage, 15).S(StatType.MaxMana, 20);

            C("A02", "air.storm_smith", A, "Rękawice burzowego kowala", ItemKind.Hands, null, Fronts, 3,
                "Zderzenie frontów (cecha Maga powietrza).",
                "Odrzucony wróg rozbija się o twój pocisk.",
                "Własny wolny pocisk jest punktem zderzenia: odrzucony wróg, który w niego wpadnie ({contact m}), zużywa pocisk i uwalnia wybuch ({burst_share} trafienia pocisku, promień {burst_radius m}).",
                "Poświęcony pocisk; jeden wybuch na pocisk, bez łańcucha.",
                "Mag powietrza ze Zderzeniem frontów i Piekielną kulą hu_infernal_orb (wolny pocisk).",
                "Nova powietrza hu_nova_air jako podmuch, Peleryna bezwietrznej nocy.",
                "Trzeba ustawić ruch wroga i pocisku.",
                "contact", 0.9f, "burst_share", 1.2f, "burst_radius", 2f)
                .S(StatType.LightningDamage, 12).S(StatType.MaxStamina, 15);

            C("A03", "air.windless_night", A, "Peleryna bezwietrznej nocy", ItemKind.Body, null, null, 3,
                "Krok wichru hu_wind_step.",
                "Krok wichru kieruje twoje pociski zamiast ciebie.",
                "Krok wichru nie przesuwa postaci: wszystkie własne pociski w promieniu {radius m} skręcają w kierunku celowania i przyspieszają ×{speed_mult}.",
                "Utrata mobilności skilla; pocisk nie odświeża trafień ani czasu życia.",
                "Mag powietrza z Krokiem wichru i hu_wand_air.",
                "Oko cyklonu, wolne kule, Buty ostatniego podmuchu jako ucieczka.",
                "Bez skoku ucieczki.",
                "radius", 7f, "speed_mult", 1.35f)
                .Sk("hu_wind_step").S(StatType.MaxMana, 25).S(StatType.MoveSpeed, 5).S(StatType.SpellPower, 8);

            C("A04", "air.empty_nodes", A, "Różdżka pustych węzłów", ItemKind.Weapon, "hu_wand_air", null, 4,
                "Łańcuch potępionych hu_damned_chain i własne pociski w świecie.",
                "Łańcuch skacze przez twoje pociski do dalszych wrogów.",
                "Gdy z ciała nie ma dalszego wroga w zasięgu skoku, łańcuch może zużyć skok na własny pocisk w świecie (węzeł) i skoczyć z niego dalej (najwyżej {max_nodes} węzły na rzut).",
                "Węzeł nie jest celem i zużywa skok; pociski w magazynie Oka nie są węzłami.",
                "Mag powietrza z tą różdżką i Łańcuchem.",
                "Sieć z Oka cyklonu i Kroku wichru; odizolowany boss.",
                "Skoki na węzły to mniej trafień.",
                "max_nodes", 2)
                .Sk("hu_damned_chain").S(StatType.LightningDamage, 12).S(StatType.MaxMana, 15);

            C("A05", "air.last_gust", A, "Buty ostatniego podmuchu", ItemKind.Feet, null, null, 4,
                "Nova powietrza hu_nova_air.",
                "Nova odrzuca ciebie zamiast wrogów.",
                "Nova powietrza nie rozpycha wrogów: po wyzwoleniu odrzuca właściciela o {distance m} od głównego celu (albo do tyłu).",
                "Normalny koszt i kolizja, bez niewrażliwości; liczba trafionych nie zmienia dystansu.",
                "Mag powietrza z Novą powietrza (grymuar hu_grimoire_air).",
                "Strzał → nova → odskok; Peleryna bezwietrznej nocy.",
                "Wrogowie zostają przy celu, nie są odepchnięci.",
                "distance", 4.5f, "seconds", 0.25f)
                .Sk("hu_nova_air").S(StatType.MaxStamina, 15).S(StatType.MoveSpeed, 6).S(StatType.LightningDamage, 8);

            // ---- Mag ziemi
            C("E01", "earth.moving_bastion", E, "Buty ruchomego bastionu", ItemKind.Feet, null, Bastion, 2,
                "Bastion ziemi (cecha Maga ziemi).",
                "Zamiast jednej ściany dwie płyty idą z tobą po bokach.",
                "Bastion tworzy dwie krótsze płyty ({plate_length m}) po bokach postaci, prowadzone z nią przez {seconds s}.",
                "Odsłonięty przód i tył; płyty mają kolizje, trwałość i bossowie je niszczą.",
                "Mag ziemi z Bastionem i hu_bow albo hu_morningstar.",
                "Szturm między osłonami, Kamienna osłona hu_stone_ward.",
                "Bez zamknięcia przejścia.",
                "plate_length", 2.2f, "seconds", 6f, "side_offset", 1.4f)
                .S(StatType.PhysicalDefense, 30).S(StatType.MaxStamina, 15).S(StatType.MoveSpeed, 4);

            C("E02", "earth.grave_mortar", E, "Pas grobowego moździerza", ItemKind.Belt, null, Shell, 3,
                "Krusząca skorupa (cecha Maga ziemi).",
                "Mocny atak spuszcza płytę z nieba zamiast strzelać nią z ciebie.",
                "Mocny atak wyznacza punkt (namierzony wróg albo {range m} przed tobą): płyta spada po {delay s} w promieniu {radius m} za pełen budżet płyty.",
                "Ten sam zapas płyt, widoczna zapowiedź – ofiara może uciec.",
                "Mag ziemi z Kruszącą skorupą i Kamienną osłoną hu_stone_ward.",
                "Łuk hu_bow i artyleria bariery; Napierśnik żywego kamieniołomu.",
                "Opóźnienie przeciw ruchliwym celom.",
                "range", 7f, "delay", 0.7f, "radius", 1.8f)
                .S(StatType.MaxHealth, 25).S(StatType.EarthDamage, 12).S(StatType.MaxMana, 15);

            C("E03", "earth.undug_grave", E, "Młot niewykopanego grobu", ItemKind.Weapon, "hu_morningstar", null, 4,
                "Kamienne kolce hu_stone_spikes.",
                "Kolce czekają pod ziemią na mocny cios młota.",
                "Kamienne kolce nie uderzają od razu – zostają zapisane pod ziemią (najwyżej {max_marks}, {mark_seconds s}). Mocny atak młotem wyzwala wszystkie zapisane linie w zasięgu {range m}.",
                "Pełny koszt przygotowania, brak natychmiastowego ciosu; nie daje prawa do technik broni dwuręcznej.",
                "Mag ziemi z tym młotem i hu_grimoire_earth.",
                "Saper z Kamienną osłoną; Amulet rany pod stopami.",
                "Bez mocnego ciosu kolce przepadają.",
                "max_marks", 3, "mark_seconds", 10f, "range", 14f)
                .Sk("hu_stone_spikes").S(StatType.EarthDamage, 12).S(StatType.MaxMana, 15);

            C("E04", "earth.living_quarry", E, "Napierśnik żywego kamieniołomu", ItemKind.Body, null, null, 4,
                "Kamienna osłona hu_stone_ward i Kamienne kolce hu_stone_spikes.",
                "Odłamane płyty osłony stają się materiałem dla kolców.",
                "Osłona składa się z {plates} płyt tej samej łącznej ochrony. Płyta odłamana wrogim trafieniem zostawia odłamek (do {max_shards}); Kamienne kolce zbierają odłamki w promieniu {pick_radius m} i poszerzają swoją linię o {width_per_shard m} za odłamek.",
                "Odłamki nie są barierą; większe pokrycie nie mnoży trafienia na tej samej ofierze. Zużycie osłony Kruszącą skorupą nie tworzy odłamków.",
                "Mag ziemi z Kamienną osłoną i Kamiennymi kolcami.",
                "Krusząca skorupa, Pas grobowego moździerza, hu_morningstar.",
                "Trzeba przyjmować ciosy w osłonę.",
                "plates", 4, "max_shards", 4, "pick_radius", 3f, "width_per_shard", 0.5f)
                .S(StatType.MaxHealth, 30).S(StatType.PhysicalDefense, 25).S(StatType.EarthDamage, 6);

            C("E05", "earth.wound_underfoot", E, "Amulet rany pod stopami", ItemKind.Amulet, null, null, 3,
                "Własne źródła Pęknięcia pancerza (amulet daje skromne źródło na mocnym ataku).",
                "Pęknięcie pancerza zostaje w ziemi zamiast na ciele.",
                "Własne Pęknięcie pancerza zostawia pole o promieniu {radius m} na {field_seconds s} (najwyżej {max_fields}); wrogowie w polu mają {stacks} warstwy pęknięcia, po wyjściu je tracą. Mocny atak bronią nakłada pęknięcie.",
                "Nakładające się pola nie przekraczają normalnego limitu warstw.",
                "Mag ziemi z hu_morningstar i Novą ziemi hu_nova_earth.",
                "Przywołania i bronie walczące w polu; Zbroja runożercy (magiczne pole).",
                "Ofiara, która wyjdzie z pola, jest zdrowa.",
                "radius", 2.2f, "field_seconds", 5f, "max_fields", 3, "stacks", 3)
                .S(StatType.EarthDamage, 12).S(StatType.PhysicalDamage, 8).S(StatType.MaxHealth, 15);

            // ---- Przywoływacz
            C("S01", "summoner.third_hunger", S, "Maska trzeciego głodu", ItemKind.Head, null, Incarnation, 3,
                "Wcielenie (cecha Przywoływacza) i drugi sługa innego rodzaju.",
                "Forma zjada drugiego sługę i bierze jego zdolność do mocnego ataku.",
                "W formie Wcielenia mocny atak zużywa najbliższego własnego sługę innego rodzaju niż forma i zastępuje mocny atak jego zdolnością (chochlik – ognisty pocisk, ogar – rozdarcie z krwawieniem, golem – zachwianie) na resztę formy.",
                "Dwa realnie zużyte przywołania, bez sumowania czasów; nie tworzy autonomicznej chimery.",
                "Przywoływacz z Wcieleniem, Sforą i Przyzwaniem chochlików.",
                "Golem hu_summon_golem jako forma tanku z ognistym mocnym atakiem.",
                "Mniejsza armia za lepszą formę.",
                "ability_share", 1.1f)
                .S(StatType.MaxHealth, 25).S(StatType.MaxStamina, 15);

            C("S02", "summoner.sixth_rib", S, "Serce szóstego żebra", ItemKind.Amulet, null, Chimera, 3,
                "Chimera legionu (cecha Przywoływacza).",
                "Chimera dostaje prawdziwe życie i staje się celem wrogów.",
                "Chimera ma prawdziwe zdrowie ({hp_per_second} PŻ za każdą sekundę wkładów) i przyciąga wrogów; zamiast znikać po czasie, żyje dopóki ma zdrowie (najdłużej {max_seconds s}). Leczenie (aury, czar) może ją podtrzymać.",
                "Może zginąć; ponowne scalenie nie odnawia zdrowia do pełna. Bez niewrażliwości bezżyciowego sługi.",
                "Przywoływacz z Chimerą legionu i Aurą odnowy.",
                "Habit pustego klasztoru, leczenie hu_aura_renewal, Grymuar otwartych pysków.",
                "Chimera ginie od skupionego ognia – trzeba ją chronić.",
                "hp_per_second", 14f, "max_seconds", 30f, "taunt_range", 9f)
                .S(StatType.MaxHealth, 25).S(StatType.HealthRegen, 1.5f);

            C("S03", "summoner.bone_conductor", S, "Rękawice kościanego dyrygenta", ItemKind.Hands, null, null, 3,
                "Własne samodzielne sługi.",
                "Sługi atakują na twoje ciosy.",
                "Sługi nie atakują same: każde ręczne trafienie właściciela wydaje komendę ataku kolejnemu słudze (najszybciej co {min_interval_share} jego zwykłego odstępu).",
                "Przerwa w atakach zatrzymuje armię; jedna salwa nie tworzy nieograniczonej kolejki.",
                "Przywoływacz z hu_dagger i Sforą hu_summon_hounds.",
                "Szybka broń wręcz, Wcielenie dowodzące resztą sług.",
                "Bez twoich trafień sługi stoją.",
                "min_interval_share", 0.45f)
                .S(StatType.AttackSpeed, 8).S(StatType.MaxStamina, 15);

            C("S04", "summoner.open_maws", S, "Grymuar otwartych pysków", ItemKind.OffHand, "hu_grimoire_summoning", null, 4,
                "Własny sługa i ręczny czar pociskowy.",
                "Twoje pociski czarów wylatują z pysków sług.",
                "Ręczny czar pociskowy wychodzi z kolejnego własnego sługi (rotacja przez armię), w kierunku celu. Jeden rzut – jeden pocisk zgodny z definicją.",
                "Każdy rzut zabiera słudze {servant_cost s} czasu życia (śmiertelnej chimerze {chimera_hp} PŻ).",
                "Przywoływacz z tym grymuarem (chochliki) i Lodowym odłamkiem hu_frost_shard.",
                "Serce szóstego żebra – chimera jako działo; Sfora z boku.",
                "Sługi szybciej znikają.",
                "servant_cost", 1.5f, "chimera_hp", 25f)
                .S(StatType.MaxMana, 20).S(StatType.SpellPower, 10);

            C("S05", "summoner.inherited_beast", S, "Pas dziedziczonej bestii", ItemKind.Belt, null, null, 3,
                "Naturalnie kończące się własne przywołanie.",
                "Sługa, który dożył końca, przekazuje cechę następnemu przywołaniu innego rodzaju.",
                "Naturalny koniec sługi zostawia ślad dziedziczenia na {trace_seconds s}. Następne opłacone przywołanie innego rodzaju dostaje właściwość poprzednika: chochlik – podpalenie, ogar – krwawienie, golem – zachwianie.",
                "Jeden ślad (nowy zastępuje stary), bez dodatkowego ciała i czasu. Wcielenie, scalenie i śmierć nie są naturalnym końcem.",
                "Przywoływacz z chochlikami i Sforą.",
                "Rotacja rodzajów i żywiołów, golem z ogniem.",
                "Trzeba pozwolić słudze dożyć końca.",
                "trace_seconds", 8f)
                .S(StatType.MaxMana, 20).S(StatType.ManaRegen, 1.5f);

            // ---- Mag bitewny
            C("T01", "battlemage.runic_wrestler", T, "Bandaże runicznego zapaśnika", ItemKind.Hands, null, Arcana, 3,
                "Arkana oręża (cecha Maga bitewnego) i puste dłonie.",
                "Pięści stają się orężem-fokusem, a czary – uderzeniami.",
                "Pięści zadają {fist_mult} × więcej obrażeń. Puste dłonie są legalnym orężem Arkanów oręża. Ofensywne czary rzucane w zwarciu przyjmują postać uderzenia pięścią: pocisk – wybuchający prosty ({punch_reach m}), nova – uderzenie obrotowe wokół, reszta działa normalnie.",
                "Bez trzymanych przedmiotów; prawdziwe koszty czarów. Z bronią lub grymuarem moc jest uśpiona.",
                "Mag bitewny z Arkanami oręża, bez broni, Lodowy odłamek hu_frost_shard i Nova ognia hu_nova_fire.",
                "Okrzyk hu_warcry i pierścienie siły.",
                "Zwarcie bez broni i bez tarczy.",
                "punch_reach", 2.4f, "punch_radius", 1.4f, "fist_mult", 2f)
                .S(StatType.Strength, 3).S(StatType.MaxMana, 20).S(StatType.SpellPower, 8);

            C("T02", "battlemage.unfinished_rune", T, "Miecz niedokończonej runy", ItemKind.Weapon, "hu_sword", TwoBeats, 4,
                "Dwa takty (cecha Maga bitewnego).",
                "Czar z toru zostawia pieczęć, a mocny atak tnie do niej.",
                "Czar rzucony torem Dwóch taktów zostawia jeden punkt trafienia ({point_seconds s}). Następny mocny atak tnie odcinek od ciebie do pieczęci (najwyżej {max_length m}) zamiast zwykłego łuku.",
                "Jeden zapamiętany punkt; czar wielocelowy nie daje wielu linii.",
                "Mag bitewny z Dwoma taktami, tym mieczem i hu_grimoire_fire.",
                "Krzyżujące się linie z Lodowym odłamkiem; Bandaże pokazują konflikt pustych dłoni.",
                "Mocny atak w ustalonym kierunku.",
                "point_seconds", 4f, "max_length", 9f, "width", 1.2f, "line_mult", 1.2f)
                .S(StatType.MaxMana, 20).S(StatType.SpellPower, 8);

            C("T03", "battlemage.rune_eater", T, "Zbroja runożercy", ItemKind.Body, null, null, 3,
                "Własne Pęknięcie pancerza.",
                "Pęknięcie pancerza osłabia odporności magiczne zamiast fizycznej.",
                "Własne pęknięcie pancerza obniża odporność na żywioły (ogień, mróz, błyskawice, ziemia) o {magic_per_stack} za warstwę zamiast odporności fizycznej.",
                "Fizyczna część ofensywy traci osłabienie; granice odporności i czas statusu bez zmian.",
                "Mag bitewny z młotem hu_warhammer (pęknięcie) i czarami.",
                "Amulet rany pod stopami (pole magiczne), Cięcie burzy.",
                "Ciosy bronią nie korzystają z pęknięcia.",
                "magic_per_stack", 0.05f)
                .S(StatType.MaxHealth, 30).S(StatType.SpellPower, 8).S(StatType.PhysicalDefense, 20);

            C("T04", "battlemage.parrying_rune", T, "Obrączka parującej runy", ItemKind.Ring, null, null, 4,
                "Udane ręczne parowanie i czar w wybranym slocie.",
                "Riposta zamienia się w skonfigurowany czar.",
                "Następna riposta po parowaniu zostaje zastąpiona czarem ze slotu {slot_label} (wybór w ekwipunku) rzuconym w ofiarę z premią riposty ×{riposte_bonus_share}.",
                "Normalna mana i gotowość; bez many/gotowości – zwykła riposta. Brak fizycznej riposty obok czaru.",
                "Mag bitewny z hu_sword, grymuarem hu_grimoire_water (czary wymagają fokusu) i Lodowym odłamkiem.",
                "Pierścień riposty, grymuar, Hełm dzwonnika pokazuje warunek.",
                "Mana i odnowienie czaru zamiast darmowej riposty.",
                "slot", 0, "slot_label", 1, "riposte_bonus_share", 0.5f)
                .S(StatType.RiposteDamage, 20).S(StatType.MaxMana, 20).S(StatType.ParryWindow, 0.04f);

            C("T05", "battlemage.broken_wand", T, "Grymuar złamanej różdżki", ItemKind.OffHand, "hu_grimoire_air", null, 4,
                "Jednoręczna różdżka w głównej ręce.",
                "Lekki atak różdżki tnie przywołanym ostrzem.",
                "Lekki atak różdżki staje się cięciem przywołanym ostrzem jej żywiołu (zasięg {reach m}, {blade_mult} obrażeń). Mocny atak pozostaje ostrzałem.",
                "Utrata szybkiego dystansu; druga ręka zajęta. Nie działa z kosturem.",
                "Mag bitewny z hu_wand_fire i tym grymuarem.",
                "Arkany oręża, odzysk many za trafienie wręcz.",
                "Walka w zwarciu różdżką.",
                "reach", 2.2f, "blade_mult", 1.15f, "radius", 0.9f)
                .S(StatType.SpellPower, 10).S(StatType.MaxMana, 20).S(StatType.MaxStamina, 10);
        }
    }
}
