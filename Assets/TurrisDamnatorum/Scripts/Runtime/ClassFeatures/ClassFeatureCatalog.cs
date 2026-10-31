using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Katalog 30 cech klasowych (po dwie na każdą z 15 klas). Źródło prawdy dla wyboru w przygotowaniu, zapisu, botów i testów.
    /// Klasy rozpoznawane są po id (te same co w assetach). Cecha nigdy nie trafia do sklepu, nagród ani zwykłych skilli.
    /// </summary>
    public static class ClassFeatureCatalog
    {
        static List<ClassFeatureDefinition> all;
        static Dictionary<string, ClassFeatureDefinition> byId;

        public static IReadOnlyList<ClassFeatureDefinition> All { get { Build(); return all; } }

        /// <summary>Definicja po id; null dla nieznanego.</summary>
        public static ClassFeatureDefinition Get(string id) { Build(); return id != null && byId.TryGetValue(id, out var d) ? d : null; }

        /// <summary>Oba warianty klasy (A, B); pusta lista dla klasy spoza katalogu.</summary>
        public static List<ClassFeatureDefinition> For(ClassDefinition cls) => For(cls != null ? cls.id : null);

        public static List<ClassFeatureDefinition> For(string classId)
        {
            Build();
            return all.Where(d => d.classId == classId).OrderBy(d => d.variant).ToList();
        }

        /// <summary>Bezpieczny wariant domyślny klasy (A).</summary>
        public static ClassFeatureDefinition Default(ClassDefinition cls) => For(cls).FirstOrDefault();

        /// <summary>
        /// Rozstrzyga zapamiętany/zapisany wybór: id musi należeć do tej klasy. Obce, nieznane albo puste id → wariant A
        /// (z ostrzeżeniem w <paramref name="warning"/>, gdy id było niepuste).
        /// </summary>
        public static ClassFeatureDefinition Resolve(ClassDefinition cls, string id, out string warning)
        {
            warning = null;
            var own = For(cls);
            if (own.Count == 0) return null;
            var d = own.FirstOrDefault(x => x.id == id);
            if (d != null) return d;
            if (!string.IsNullOrEmpty(id))
            {
                var foreign = Get(id);
                warning = foreign != null
                    ? $"Cecha {foreign.name} należy do innej klasy ({foreign.classId}) – wybrano {own[0].name}."
                    : $"Nieznana cecha klasowa „{id}” – wybrano {own[0].name}.";
            }
            return own[0];
        }

        public static string ServantName(ServantKind k) => k == ServantKind.Imp ? "chochlik" : k == ServantKind.Hound ? "ogar" : "golem";

        public static bool Owns(ClassDefinition cls, string featureId) => cls != null && Get(featureId)?.classId == cls.id;

        // ------------------------------------------------------------------ Budowa katalogu

        static string N(float v) => v.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');
        static string P(float v) => ClassFeatureTuning.Pct(v).ToString("0", CultureInfo.InvariantCulture) + "%";

        static ClassFeatureDefinition D(string id, string cls, ClassFeatureVariant v, string name, ClassFeatureActivation act,
            Func<ClassFeatureTuning, string> rule, Func<ClassFeatureTuning, string> benefit, Func<ClassFeatureTuning, string> cost,
            Func<ClassFeatureTuning, string> usage, string example, string core, string developed, string passive, string reset, params string[] skills)
        {
            var d = new ClassFeatureDefinition
            {
                id = id, classId = cls, variant = v, name = name, activation = act, rule = rule, benefit = benefit, cost = cost, usage = usage,
                exampleBuild = example, coreBuild = core, developedBuild = developed, passiveHint = passive, floorReset = reset,
            };
            d.startingSkillIds.AddRange(skills);
            return d;
        }

        static void Build()
        {
            if (all != null) return;
            const ClassFeatureVariant A = ClassFeatureVariant.A, B = ClassFeatureVariant.B;
            const ClassFeatureActivation W = ClassFeatureActivation.Window, T2 = ClassFeatureActivation.TwoPhase, I = ClassFeatureActivation.Instant;
            all = new List<ClassFeatureDefinition>
            {
                // ================================================================ Wojownik
                D("class_feature.warrior.guard", "class_knight", A, "Walka w gardzie", W,
                    t => "Tylko Wojownik trzyma prawdziwą gardę w trakcie własnego zwykłego ataku i techniki (także podstawowego strzału różdżki lub łuku z gardą).",
                    t => "Blok z przodu działa podczas ciosu – atak i obrona w tej samej chwili.",
                    t => $"Przez {N(t.guardSeconds)} s wytrzymałość nie odrasta; atak, blok i unik pożerają tę samą pulę. Przełamanie gardy przerywa cios, plecy są odsłonięte. Odnowienie {N(t.guardCooldown)} s.",
                    t => $"{{G}}: {N(t.guardSeconds)} s okna. Trzymaj blok i atakuj – garda nie obejmuje czarów ze slotów.",
                    "Tarcza + sztylet: szybkie pchnięcia zza tarczy.", "Miecz i tarcza ze startu.", "Tarcza + różdżka (Różdżka ognia) i perk Lekka tarcza.",
                    "Tarcza i broń z wysokim blokiem stają się bronią ofensywną.", "Okno i odnowienie od zera."),
                D("class_feature.warrior.arsenal", "class_knight", B, "Arsenał weterana", I,
                    t => "Tylko Wojownik nosi dwa przygotowane zestawy główna/druga ręka i przełącza je jednym przyciskiem.",
                    t => "Seria może zacząć się sztyletem i skończyć toporem – etap serii zostaje.",
                    t => $"Oba zestawy obciążają postać. Zmiana tylko poza zamachem i nie częściej niż co {N(t.arsenalSwapInterval)} s; premie daje wyłącznie trzymany zestaw.",
                    t => "{G}: zamiana zestawów (w spoczynku albo w wybrzmieniu ciosu). Drugi zestaw wybierasz w przygotowaniu i w ekwipunku.",
                    "Sztylet + topór dwuręczny.", "Miecz i tarcza + miecz dwuręczny (oba zestawy startowe).", "Łuk + miecz/tarcza i perk Rozpęd.",
                    "Każda broń z plecaka może czekać w drugim zestawie zamiast zalegać.", "Zestawy zostają; zegar zmiany od zera."),

                // ================================================================ Paladyn
                D("class_feature.paladin.absolution", "class_hu_paladin", A, "Rozgrzeszenie", I,
                    t => "Tylko Paladyn zamienia nadleczenie (leczenie ponad pełne życie, po limitach źródła) na magazyn Łaski.",
                    t => $"{{G}} wydaje całą Łaskę na falę osądu ({N(t.graceToDamage)} obrażeń za punkt, promień {N(t.graceWaveRadius)} m).",
                    t => $"Najpierw leczenie uzupełnia życie – Łaska powstaje dopiero z nadwyżki. Pojemność {P(t.graceCapacity)} maks. życia; flaszki liczą się za {P(t.graceFromFlask)}. Łaska znika na nowym piętrze.",
                    t => "Lecz się przy pełnym życiu, potem {G} – fala wokół Paladyna.",
                    "Regeneracja + Aura odnowy.", "Korbacz i tarcza; regeneracja klasy (+1/s).", "Kradzież życia + szybkie trafienia, Aura odnowy (hu_aura_renewal).",
                    "Regeneracja i kradzież życia przestają się marnować przy pełnym życiu.", "Łaska zeruje się na nowym piętrze."),
                D("class_feature.paladin.judgement", "class_hu_paladin", B, "Sąd przysięgi", W,
                    t => "Tylko Paladyn odwraca własne aktywne aury i przenosi ich środek na jednego oznaczonego wroga.",
                    t => $"Niezłomność −{P(t.judgementDefianceArmour)} odporności fizycznej, Czystość −{P(t.judgementPurityResist)} odporności żywiołów, Zapał ×{N(t.judgementZealSlow)} tempa, Męstwo ×{N(t.judgementMightWeaken)} obrażeń wroga, Odnowa {N(t.judgementRenewalPerRegen)}× regeneracji jako ubytek, Święty ogień pali wokół celu.",
                    t => $"Przez {N(t.judgementSeconds)} s tracisz premie przeniesionych aur, a ich rezerwacja many trwa. Odnowienie {N(t.judgementCooldown)} s.",
                    t => $"{{G}}: cel z namierzenia/pomocy w celowaniu (do {N(t.judgementRange)} m). Wymaga aktywnej aury.",
                    "Paladyn z łukiem.", "Jedna aura startowa (Aura świętego ognia albo niezłomności).", "Dwie aury (Pobożność) + przywołania.",
                    "Każda aura ma drugi, ofensywny sens.", "Sąd kończy się; aury wracają."),

                // ================================================================ Berserker
                D("class_feature.berserker.redline", "class_hu_berserker", A, "Czerwona linia: Żelazna skóra", W,
                    t => $"Tylko Berserker rezerwuje życie do {P(t.redlineCap)} normalnego maksimum, a Żelazna skóra chroni tym mocniej, im mniej ma życia.",
                    t => $"Redukcja = {N(t.redlineMaxReduction)} × (1 − PŻ / normalne maks.): przy 35% życia 52%, przy 10% 72%. Ryk odpycha, zachwiewa i daje furię.",
                    t => $"Wejście obniża PŻ do linii (bez leczenia, gdy jesteś niżej). Leczenie najwyżej do linii, nadwyżka przepada. Śmierć nadal możliwa. Odnowienie {N(t.redlineCooldown)} s.",
                    t => $"{{G}}: trans na {N(t.redlineSeconds)} s.",
                    "Regeneracja + walka przy niskim życiu.", "Topór dwuręczny; Żądza krwi.", "Szybka broń + kradzież życia + efekty dla rannej postaci (Pakt krwi).",
                    "Niskie życie przestaje być wyłącznie ryzykiem.", "Trans i odnowienie od zera."),
                D("class_feature.berserker.fury_spend", "class_hu_berserker", B, "Spalenie furii", I,
                    t => "Tylko Berserker przerywa szał i zamienia jego pozostałe sekundy na opłaty za zwykłe umiejętności.",
                    t => $"Żeton Gniewu za każde pełne {N(t.furySecondsPerToken)} s szału (najwyżej {t.furyMaxTokens}); żeton opłaca manę albo wytrzymałość następnego skilla.",
                    t => $"Szał od razu się kończy (bez szybkości i ochrony). Żetony gasną po {N(t.furyTokenLife)} s; nie opłacają życia ani rezerwacji, nie omijają odnowień i wymagań broni.",
                    t => "{G} w szale.",
                    "Miecz + grymuar: furia płaci za czary.", "Topór dwuręczny + Rozpłatanie.", "Broń ciężka z kosztownymi technikami (Upadek kata, Młynek).",
                    "Kosztowne skille i czary stają się dostępne mimo małej puli.", "Żetony znikają."),

                // ================================================================ Rębajło
                D("class_feature.dual.mismatched_pair", "class_hu_dual", A, "Niezgodna para", W,
                    t => "Tylko Rębajło łączy w jednym uderzeniu dwie trzymane bronie jednoręczne z ich własnymi żywiołami i efektami.",
                    t => $"Każda broń wnosi {P(t.pairMainShare)} własnych obrażeń i swoje efekty – kolejność: główna ręka, potem druga.",
                    t => $"Tempo to średnia obu broni; ogólne moce wyposażenia działają raz na uderzenie. Wymaga dwóch broni jednoręcznych. Odnowienie {N(t.pairCooldown)} s.",
                    t => $"{{G}}: {N(t.pairSeconds)} s wspólnych uderzeń zwykłym atakiem.",
                    "Szybka i wolniejsza broń; dwa różne żywioły.", "Dwa miecze ze startu.", "Miecz wody + miecz ognia (szok termiczny w jednym ciosie).",
                    "Druga broń przestaje być tylko szybkością – wnosi własny żywioł.", "Okno i odnowienie od zera."),
                D("class_feature.dual.unbroken_dance", "class_hu_dual", B, "Taniec bez przerwy", W,
                    t => "Tylko Rębajło robi unik bez kasowania rozpoczętego ataku – cięcie trwa dalej w nowym miejscu.",
                    t => "Unik w trakcie zamachu nie marnuje ciosu: ta sama oś czasu i lista trafionych.",
                    t => $"Pełny koszt ataku i uniku; brak dodatkowej niewrażliwości. Cięcie może spudłować przez ruch. Odnowienie {N(t.danceCooldown)} s.",
                    t => $"{{G}}: {N(t.danceSeconds)} s okna; atakuj i rób unik w trakcie zamachu.",
                    "Krótkie ostrza z krwawieniem.", "Dwa miecze ze startu.", "Wolniejsza technika obszarowa (Rozpłatanie) w uniku.",
                    "Wolne bronie stają się bezpieczne w ruchu.", "Okno i przeniesiony cios od zera."),

                // ================================================================ Mag
                D("class_feature.mage.arcane_debt", "class_mage", A, "Dług arkanów", W,
                    t => "Tylko Mag finansuje ręczne czary ujemnym saldem many.",
                    t => $"W oknie brakującą manę pokrywa dług do {P(t.debtLimit)} maks. many.",
                    t => "Po oknie dług blokuje czary aż do spłaty; regeneracja, mana z trafień, zabójstw, parowania i flaszki najpierw spłacają dług. Przejście piętra nie kasuje długu.",
                    t => $"{{G}}: {N(t.debtSeconds)} s okna. Odnowienie {N(t.debtCooldown)} s.",
                    "Duży zapas many + seria drogich czarów.", "Różdżka i grymuar ze startu.", "Regeneracja many + mana za zabójstwa – szybsza spłata.",
                    "Maksymalna mana staje się limitem kredytu.", "Dług zostaje (spłaca go odnowienie piętra na trudnościach, które odnawiają manę)."),
                D("class_feature.mage.mana_cloak", "class_mage", B, "Płaszcz many", W,
                    t => "Tylko Mag przeznacza manę na przyjmowanie części obrażeń.",
                    t => $"Płaszcz przejmuje {P(t.cloakShare)} obrażeń po redukcjach i Arkanowej osłonie, a przed barierą.",
                    t => $"{N(t.cloakManaPerHealth)} many za 1 PŻ ochrony; przy braku many reszta trafia w życie. Odnowienie {N(t.cloakCooldown)} s.",
                    t => $"{{G}}: {N(t.cloakSeconds)} s ochrony.",
                    "Różdżka + tarcza + odzysk many po bloku.", "Różdżka i grymuar ze startu.", "Duża pula many + rzadsze mocne czary (Piekielna kula).",
                    "Mana staje się drugim paskiem życia.", "Okno od zera."),

                // ================================================================ Mag ognia
                D("class_feature.fire.living_furnace", "class_hu_mage_fire", A, "Żywy piec", W,
                    t => "Tylko Mag ognia podpala siebie i zamienia życie utracone od tego płomienia na Żar.",
                    t => $"Następny ręczny atak albo czar ofensywny wydaje Żar: {N(t.furnaceEmberToDamage)} obrażeń ognia za punkt, jeden budżet na cały atak.",
                    t => $"Płomień zabiera {P(t.furnaceBurnPerSecond)} normalnego maks. życia na sekundę (po odporności na ogień i osłonie) – może zabić. Odporność na podpalenie zeruje paliwo. Żar: pojemność {P(t.furnaceEmberCap)} maks. życia, gaśnie po {N(t.furnaceEmberLife)} s.",
                    t => $"{{G}}: {N(t.furnaceSeconds)} s płomienia. Odnowienie {N(t.furnaceCooldown)} s.",
                    "Wysokie życie + regeneracja + broń wręcz.", "Różdżka ognia + Grymuar ognia.", "Przedmioty wzmacniające płonącą postać + regeneracja.",
                    "Życie i odporność na ogień stają się zasobem ofensywnym.", "Płomień i Żar znikają."),
                D("class_feature.fire.great_fire", "class_hu_mage_fire", B, "Wielki pożar", W,
                    t => $"Tylko Mag ognia utrzymuje na jednym wrogu do {t.greatFireFamilies} niezależne podpalenia z różnych źródeł.",
                    t => "Źródło to skill albo grupa zwykłych ataków; kopie, strefy i odpryski należą do źródła.",
                    t => $"Po oknie zostaje tylko najsilniejsza rodzina (bez przedłużenia). Odporność na podpalenie działa. Odnowienie {N(t.greatFireCooldown)} s.",
                    t => $"{{G}}: {N(t.greatFireSeconds)} s okna.",
                    "Trzy różne źródła ognia.", "Różdżka ognia + Nova ognia z grymuaru (dwie rodziny od startu).", "Płonąca broń + dwa czary (Krąg żaru, Piekielna kula).",
                    "Różne źródła ognia przestają się nadpisywać.", "Rodziny wygasają."),

                // ================================================================ Mag wody
                D("class_feature.water.enduring_frost", "class_hu_mage_water", A, "Niezmienny szron", W,
                    t => "Tylko Mag wody utrzymuje warunek chłodu dla reakcji bez zużywania go przez te reakcje.",
                    t => $"Oznaczony cel spełnia warunek Szronu: szok termiczny i błoto działają wielokrotnie (najczęściej co {N(t.frostReactionInterval)} s).",
                    t => "Szron nie zamraża ani nie ogłusza: kontrola, jej czas i odporność bossów zostają normalne. Bez prawdziwego chłodu błoto daje premię do obrażeń zamiast ogłuszenia.",
                    t => $"{{G}}: znacznik na {N(t.frostSeconds)} s (do {N(t.frostRange)} m). Odnowienie {N(t.frostCooldown)} s.",
                    "Woda + ogień.", "Różdżka wody + Piekielna kula (hu_infernal_orb) do wyboru na start.", "Broń obuchowa + efekty chłodu + ogień.",
                    "Ogień i ziemia stają się wzmacniaczem maga wody.", "Znaczniki znikają."),
                D("class_feature.water.deep_current", "class_hu_mage_water", B, "Prąd głębin", I,
                    t => "Tylko Mag wody przesuwa własne istniejące pola bez nowych rzutów i bez odnawiania ich czasu.",
                    t => $"Do {t.currentMaxZones} własnych stref w zasięgu {N(t.currentRange)} m płynie do celu.",
                    t => $"{N(t.currentManaPerZone)} many za każde przesunięte pole; bez pola lub miejsca – bez opłaty. Pole zachowuje czas, tyknięcia i limity.",
                    t => "{G}: pola płyną do namierzonego wroga albo przed maga.",
                    "Przesuwana płonąca ziemia.", "Krąg żaru (hu_ember_ring) do wyboru na start.", "Kilka pól (Sidła, Krąg żaru) łączonych w przejściu.",
                    "Strefy przestają być statyczne.", "Brak zasobów do resetu."),

                // ================================================================ Mag powietrza
                D("class_feature.air.silent_eye", "class_hu_mage_air", A, "Oko ciszy", T2,
                    t => "Tylko Mag powietrza przechwytuje, przechowuje i ręcznie wypuszcza pociski.",
                    t => $"Do {t.eyeCapacity} pocisków wroga albo własnych podstawowych wraca salwą w kierunku celowania.",
                    t => "Pełny magazyn nie chroni. Bez promieni, fal ziemi i ataków arenowych. Wrogi pocisk ma budżet gracza.",
                    t => $"{{G}}: otwiera oko na {N(t.eyeSeconds)} s; ponowne {{G}} albo koniec okna – salwa. Odnowienie {N(t.eyeCooldown)} s.",
                    "Odpowiedź na ostrzał.", "Różdżka powietrza: własne strzały ładują oko.", "Przygotowana salwa własnej różdżki + Rozszczepienie.",
                    "Wrogi ostrzał staje się amunicją.", "Magazyn znika."),
                D("class_feature.air.colliding_fronts", "class_hu_mage_air", B, "Zderzenie frontów", W,
                    t => "Tylko Mag powietrza zamienia własny odrzut w zderzenie wroga ze ścianą albo innym wrogiem.",
                    t => $"Zderzenie wywołuje wyładowanie ({P(t.frontsDischarge)} obrażeń zwykłego ataku, promień {N(t.frontsRadius)} m).",
                    t => $"Odporny na odrzut cel dostaje tylko naprężenie ({P(t.frontsStrain)} wyładowania). Najwyżej {t.frontsMaxPerSource} zderzenia na atak. Odnowienie {N(t.frontsCooldown)} s.",
                    t => $"{{G}}: {N(t.frontsSeconds)} s okna.",
                    "Ciężka broń z odrzutem.", "Różdżka powietrza (odrzut) + Nova powietrza.", "Czary powietrza i walka przy ścianach + perk odrzutu.",
                    "Odrzut przestaje tylko odpychać – zadaje obrażenia.", "Okno od zera."),

                // ================================================================ Mag ziemi
                D("class_feature.earth.bastion", "class_hu_mage_earth", A, "Bastion ziemi", I,
                    t => "Tylko Mag ziemi stawia prawdziwą ścianę blokującą ruch i zwykłe pociski.",
                    t => $"Ściana {N(t.wallLength)} m na {N(t.wallSeconds)} s; mocny cios albo czar ziemi przy niej wywołuje pęknięcie wzdłuż ściany ({P(t.wallCrackShare)} trafienia).",
                    t => $"Wrogowie obchodzą ją albo burzą (bossy {N(t.wallBossDamage)}× szybciej); blokuje też twoje pociski. Odnowienie {N(t.wallCooldown)} s.",
                    t => "{G}: ściana przed magiem w poprzek celowania. Mocny cios albo czar ziemi przy ścianie – pęknięcie.",
                    "Łuk i przygotowanie stanowiska.", "Różdżka ziemi + Nova ziemi.", "Młot i rozbijanie własnej fortyfikacji.",
                    "Teren staje się bronią.", "Ściany znikają."),
                D("class_feature.earth.shattering_shell", "class_hu_mage_earth", B, "Krusząca skorupa", I,
                    t => "Tylko Mag ziemi poświęca pozostałą własną ochronę na kamienne płyty.",
                    t => $"Arkanowa osłona i bariery stają się do {t.shellMaxPlates} płyt (jeden budżet dzielony równo).",
                    t => $"Ochrona znika od razu i odrasta normalnym tempem. Płyty wypuszczają mocne ataki, gasną po {N(t.shellPlateLife)} s. Odnowienie {N(t.shellCooldown)} s.",
                    t => "{G}: zbiera ochronę; mocny atak wypuszcza płytę.",
                    "Życie i rodzima osłona.", "Różdżka ziemi; Arkanowa osłona maga.", "Bariery (Kamienna osłona) + mocne ataki dystansowe.",
                    "Osłona i bariery stają się amunicją.", "Płyty znikają."),

                // ================================================================ Przywoływacz
                D("class_feature.summoner.incarnation", "class_hu_summoner", A, "Wcielenie", I,
                    t => "Tylko Przywoływacz zużywa własnego sługę i walczy jego sposobem.",
                    t => "Chochlik – ostrzał ognia, ogar – szybkie krwawiące ugryzienia, golem – ciężkie uderzenie wokół.",
                    t => $"Sługa znika z armii i Więzi dusz. Forma trwa do {N(t.incarnationSeconds)} s (nie dłużej niż sługa). Życie i obrona są twoje; w formie bez czarów, bloku i parowania. Odnowienie {N(t.incarnationCooldown)} s.",
                    t => "{G}: wciela najbliższego sługę przed tobą; ponowne {G} kończy formę.",
                    "Opancerzony golem.", "Chochliki z grymuaru przywołań.", "Szybki ogar (Sfora, hu_summon_hounds) z ranami i mobilnością.",
                    "Przywołania stają się zestawem ataków.", "Forma kończy się na piętrze."),
                D("class_feature.summoner.chimera", "class_hu_summoner", B, "Chimera legionu", T2,
                    t => "Tylko Przywoływacz łączy kilku własnych sług w jedną istotę z ich zdolnościami.",
                    t => $"Do {t.chimeraMaxParts} składniki: golem – korpus i uderzenie, chochlik – ostrzał, ogar – krwawe natarcie.",
                    t => "Chimera zajmuje miejsca składników, a Więź dusz liczy ją raz. Każdy wkład gaśnie we własnym czasie.",
                    t => $"{{G}}: podgląd składników ({N(t.chimeraPreview)} s), ponowne {{G}} – scalenie. Odnowienie {N(t.chimeraCooldown)} s.",
                    "Jeden wspierający towarzysz.", "Dwa chochliki z grymuaru → skupiona chimera.", "Trzy rodzaje przywołań (chochliki, Sfora, Kościany golem).",
                    "Mała armia staje się jedną bestią.", "Chimera odchodzi z innymi sługami."),

                // ================================================================ Mag bitewny
                D("class_feature.battlemage.weapon_arcana", "class_hu_battlemage", A, "Arkana oręża", W,
                    t => "Tylko Mag bitewny traktuje własną broń wręcz jako fokus, a jej główny atrybut – jako Inteligencję czarów.",
                    t => "Czary bez fokusu działają w oknie; wymaganie i skalowanie Inteligencji przechodzi na Siłę albo Zręczność broni.",
                    t => $"Koszt many, odnowienia i czas rzutu bez zmian; moc fokusu broni znormalizowana. Odnowienie {N(t.arcanaCooldown)} s.",
                    t => $"{{G}}: {N(t.arcanaSeconds)} s okna.",
                    "Topór dwuręczny + Piekielna kula.", "Piekielna kula (hu_infernal_orb) do wyboru na start.", "Dwie bronie + nova.",
                    "Siła staje się statystyką czarów.", "Okno od zera."),
                D("class_feature.battlemage.two_beats", "class_hu_battlemage", B, "Dwa takty", W,
                    t => "Tylko Mag bitewny prowadzi jednocześnie akcję bronią i ręczny czar.",
                    t => "W oknie czar ze slotu leci osobnym torem – ciało zostaje wolne dla ataków.",
                    t => $"Broń płaci wytrzymałością, czar maną; obie akcje mają odnowienia. Jeden czar naraz, wymagania fokusu zostają. Odnowienie {N(t.beatsCooldown)} s.",
                    t => $"{{G}}: {N(t.beatsSeconds)} s okna.",
                    "Miecz + grymuar i krótkie czary.", "Miecz żywiołu + grymuar z novą.", "Wolniejsze uderzenia + magia obszarowa.",
                    "Krótkie czary stają się częścią serii ciosów.", "Okno od zera."),

                // ================================================================ Łotrzyk
                D("class_feature.rogue.false_witness", "class_rogue", A, "Fałszywy świadek", I,
                    t => "Tylko Łotrzyk zostawia wabik, który przejmuje następny celowany atak wroga.",
                    t => $"Do {t.decoyMaxDeceived} wrogów w {N(t.decoyRange)} m wybiera wabik na swój następny zwykły atak.",
                    t => $"Wabik żyje {N(t.decoySeconds)} s albo do rozbicia. Rozpoczęte ciosy zostają; boss oddaje najwyżej jeden atak. Odnowienie {N(t.decoyCooldown)} s.",
                    t => "{G}: wabik w miejscu łotrzyka.",
                    "Sztylet i zasadzka.", "Sztylety + nóż do rzucania.", "Bomby i pułapki (Bomba siarkowa, Sidła).",
                    "Zasadzka zyskuje okno bez pogoni wrogów.", "Wabiki znikają."),
                D("class_feature.rogue.stolen_trick", "class_rogue", B, "Kradzież sztuczki", T2,
                    t => "Tylko Łotrzyk podgląda atak wskazanego wroga i raz wykonuje jego dopuszczone zachowanie.",
                    t => $"Wręcz, pocisk albo obszar wroga staje się twoim ciosem ({N(t.stealDamage)}× zwykłego ataku, żywioł wroga).",
                    t => $"Obserwacja nie chroni przed ciosem. Jedna sztuczka naraz, gaśnie po {N(t.stealTrickLife)} s. Ataki specjalne bossów są niedostępne. Odnowienie {N(t.stealCooldown)} s.",
                    t => $"{{G}}: obserwacja celu ({N(t.stealObserve)} s); po jego ataku ponowne {{G}} – kradziona sztuczka.",
                    "Ciężka broń + pożyczony dystans.", "Łuk albo sztylety ze startu.", "Sztylet + pożyczony obszar/status.",
                    "Repertuar wrogów staje się arsenałem.", "Sztuczka przepada."),

                // ================================================================ Łucznik
                D("class_feature.archer.hunters_nails", "class_hu_archer", A, "Gwoździe łowcy", T2,
                    t => "Tylko Łucznik osadza groty w ofiarach, by wyrwać je później.",
                    t => $"Podstawowy pocisk odkłada {P(t.nailsShare)} swojego trafienia jako grot; ponowne {{G}} wyrywa wszystkie ({N(t.nailsRip)}× budżetu, krwawienie, postawa).",
                    t => $"Najwyżej {t.nailsPerTarget} grotów na cel i {t.nailsPerActivation} na aktywację; salwa dzieli budżet. Nie wyrwane groty gasną {N(t.nailsExpire)} s po oknie. Odnowienie {N(t.nailsCooldown)} s.",
                    t => $"{{G}}: {N(t.nailsSeconds)} s osadzania; {{G}} – wyrwanie w {N(t.nailsRange)} m.",
                    "Szybki łuk i wykończenie.", "Łuk myśliwski.", "Różdżka i wiele celów (Rozszczepienie, Przebicie).",
                    "Szybkie trafienia stają się odroczonym wykończeniem.", "Groty znikają."),
                D("class_feature.archer.firing_position", "class_hu_archer", B, "Stanowisko strzeleckie", I,
                    t => "Tylko Łucznik oddziela punkt wylotu własnego podstawowego strzału od siebie.",
                    t => $"Przez {N(t.postSeconds)} s strzały wychodzą ze stanowiska (do {N(t.postLinkRange)} m od łucznika); Sokole oko liczy od stanowiska.",
                    t => "Nadal strzelasz ręcznie i płacisz wytrzymałość; stanowisko nie celuje samo i nie przejmuje wrogów. Za daleko – zwykły strzał.",
                    t => $"{{G}}: stanowisko w zasięgu {N(t.postPlaceRange)} m. Odnowienie {N(t.postCooldown)} s.",
                    "Boczny ostrzał i pułapki.", "Łuk myśliwski.", "Przebicie przez linię wrogów (Strzał przeszywający).",
                    "Pozycja przestaje być tylko twoim miejscem.", "Stanowisko znika."),

                // ================================================================ Zabójca
                D("class_feature.assassin.pinned_shadow", "class_hu_assassin", A, "Przygwożdżony cień", I,
                    t => "Tylko Zabójca przygważdża cień wroga – nieruchomy drugi punkt odbierania trafień.",
                    t => "Cios w cień trafia ofiarę; ciało i cień w jednym ataku liczą się raz.",
                    t => $"Cień trwa {N(t.shadowSeconds)} s i zrywa się dalej niż {N(t.shadowLink)} m od ofiary. Nie ma życia ani łupu. Odnowienie {N(t.shadowCooldown)} s.",
                    t => $"{{G}}: cień celu do {N(t.shadowRange)} m.",
                    "Pułapki na ruchliwego wroga.", "Sztylet i noże.", "Wolna broń na nieruchomy cel (Sidła).",
                    "Ruchliwe cele stają się celem dla wolnej broni i pułapek.", "Cienie znikają."),
                D("class_feature.assassin.open_artery", "class_hu_assassin", B, "Otwarta tętnica", I,
                    t => "Tylko Zabójca sprawia, że cel spełnia próg niskiego życia dla premii egzekucji.",
                    t => "Egzekucja (ExecuteDamage Zabójcy +30%, ciosy z egzekucją) działa na pełnym życiu celu.",
                    t => $"Znacznik {N(t.arterySeconds)} s; prawdziwe PŻ, fazy bossa i obrona bez zmian. Odnowienie {N(t.arteryCooldown)} s.",
                    t => $"{{G}}: znacznik na cel do {N(t.arteryRange)} m.",
                    "Wolna broń i egzekucje.", "Sztylet; Obrażenia egzekucji klasy.", "Wysoki burst na elity i początek walki z bossem.",
                    "Premie egzekucji działają od pierwszego ciosu.", "Znacznik znika."),

                // ================================================================ Łucznik arkanów
                D("class_feature.arcane_archer.spell_arrow", "class_hu_arcane_archer", A, "Strzała zaklęcia", T2,
                    t => "Tylko Łucznik arkanów zamyka własny czar ofensywny w następnej strzale – łuk zastępuje fokus tylko w tym użyciu.",
                    t => "Czar wyzwala się raz w miejscu pierwszego trafienia.",
                    t => "Strzała płaci swój koszt, a przy wypuszczeniu pełną cenę czaru i jego odnowienie; pudło niczego nie zwraca.",
                    t => $"{{G}}: przygotowanie ({N(t.spellArrowPrepare)} s), potem przycisk slotu wybiera czar; następny strzał go niesie.",
                    "Łza lodowca na dystans.", "Łza lodowca (hu_glacial_tear) do wyboru na start.", "Nova wyzwolona w trafionym celu.",
                    "Czary bez fokusu stają się ładunkami strzał.", "Przygotowanie znika."),
                D("class_feature.arcane_archer.answering_arrow", "class_hu_arcane_archer", B, "Strzała odpowiedzi", W,
                    t => "Tylko Łucznik arkanów dobiera żywioł strzały w chwili trafienia według stanu ofiary.",
                    t => "Krwawienie → błyskawica (przewodzenie), chłód → ogień (szok termiczny), podpalenie → ziemia (wybuch żaru); bez warunku – zwykła strzała.",
                    t => $"{t.answerShots} ręczne strzały w {N(t.answerSeconds)} s; salwa dzieli jeden ładunek. Odnowienie {N(t.answerCooldown)} s.",
                    t => "{G}: ładunki odpowiedzi.",
                    "Przygotowanie chłodu i odpowiedź ogniem.", "Sidła (hu_snare: chłód i krwawienie) do wyboru na start.", "Fizyczny łuk z krwawieniem i przewodzeniem.",
                    "Statusy przygotowane przez kogokolwiek stają się reakcjami.", "Ładunki znikają."),
            };
            // Jawne skille startowe wariantów (zwykłe skille, dostępne innym klasom w sklepie).
            Get0("class_feature.water.enduring_frost").startingSkillIds.Add("hu_infernal_orb");
            Get0("class_feature.water.deep_current").startingSkillIds.Add("hu_ember_ring");
            Get0("class_feature.summoner.incarnation").startingSkillIds.Add("hu_summon_hounds");
            Get0("class_feature.summoner.chimera").startingSkillIds.Add("hu_summon_hounds");
            Get0("class_feature.battlemage.weapon_arcana").startingSkillIds.Add("hu_infernal_orb");
            Get0("class_feature.arcane_archer.spell_arrow").startingSkillIds.Add("hu_glacial_tear");
            Get0("class_feature.arcane_archer.answering_arrow").startingSkillIds.Add("hu_snare");
            byId = all.ToDictionary(d => d.id);
        }

        static ClassFeatureDefinition Get0(string id) => all.First(d => d.id == id);
    }
}
