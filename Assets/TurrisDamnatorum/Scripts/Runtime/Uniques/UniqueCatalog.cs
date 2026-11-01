using System.Collections.Generic;
using System.Linq;

namespace Turris
{
    /// <summary>
    /// Katalog 75 natywnych unikatów Turris (po pięć na każdą z 15 klas). Źródło prawdy dla rejestru przedmiotów, łupu, opisów,
    /// mocy (<see cref="UniquePowerFactory"/>), botów i testów. Karty nigdy nie trafiają do zwykłej puli sklepu ani nagród.
    /// </summary>
    public static partial class UniqueCatalog
    {
        static List<UniqueCard> all;
        static Dictionary<string, UniqueCard> byId;

        public static IReadOnlyList<UniqueCard> All { get { Build(); return all; } }

        public static UniqueCard Get(string id) { Build(); return id != null && byId.TryGetValue(id, out var c) ? c : null; }

        public static IEnumerable<UniqueCard> ForClass(string classId) { Build(); return all.Where(c => c.classId == classId); }

        // Id cech (te same co w ClassFeatureCatalog).
        public const string Guard = "class_feature.warrior.guard", Arsenal = "class_feature.warrior.arsenal";
        public const string Absolution = "class_feature.paladin.absolution", Judgement = "class_feature.paladin.judgement";
        public const string RedLine = "class_feature.berserker.redline", FurySpend = "class_feature.berserker.fury_spend";
        public const string Mismatched = "class_feature.dual.mismatched_pair", Dance = "class_feature.dual.unbroken_dance";
        public const string Debt = "class_feature.mage.arcane_debt", Cloak = "class_feature.mage.mana_cloak";
        public const string Furnace = "class_feature.fire.living_furnace", GreatFire = "class_feature.fire.great_fire";
        public const string Frost = "class_feature.water.enduring_frost", Current = "class_feature.water.deep_current";
        public const string Eye = "class_feature.air.silent_eye", Fronts = "class_feature.air.colliding_fronts";
        public const string Bastion = "class_feature.earth.bastion", Shell = "class_feature.earth.shattering_shell";
        public const string Incarnation = "class_feature.summoner.incarnation", Chimera = "class_feature.summoner.chimera";
        public const string Arcana = "class_feature.battlemage.weapon_arcana", TwoBeats = "class_feature.battlemage.two_beats";
        public const string Witness = "class_feature.rogue.false_witness", Trick = "class_feature.rogue.stolen_trick";
        public const string Nails = "class_feature.archer.hunters_nails", Post = "class_feature.archer.firing_position";
        public const string Shadow = "class_feature.assassin.pinned_shadow", Artery = "class_feature.assassin.open_artery";
        public const string SpellArrow = "class_feature.arcane_archer.spell_arrow", Answer = "class_feature.arcane_archer.answering_arrow";

        static UniqueCard C(string code, string id, string cls, string name, ItemKind kind, string baseItem, string feature, int minFloor,
            string condition, string promise, string rule, string cost, string core, string developed, string tradeoff, params object[] kv)
        {
            var c = new UniqueCard
            {
                code = code, id = "item.unique.turris." + id, classId = cls, name = name, kind = kind, baseItemId = baseItem, requiredFeature = feature,
                minFloor = minFloor, condition = condition, promise = promise, rule = rule, cost = cost, coreBuild = core, developedBuild = developed,
                tradeoff = tradeoff, isNew = code.EndsWith("4") || code.EndsWith("5"), bossOnly = kind == ItemKind.Weapon,
                basePrice = kind == ItemKind.Weapon ? 70f : 55f,
            };
            for (int i = 0; i + 1 < kv.Length; i += 2) c.param[(string)kv[i]] = System.Convert.ToSingle(kv[i + 1], System.Globalization.CultureInfo.InvariantCulture);
            all.Add(c);
            return c;
        }

        static UniqueCard S(this UniqueCard c, StatType t, float v, bool pct = false) { c.stats.Add(new StatModifier(t, v, pct ? ModifierMode.Percent : ModifierMode.Flat)); return c; }
        static UniqueCard Sk(this UniqueCard c, params string[] ids) { c.skills.AddRange(ids); return c; }

        static void Build()
        {
            if (all != null) return;
            all = new List<UniqueCard>();
            Warriors();
            Mages();
            Rogues();
            byId = all.ToDictionary(c => c.id);
        }

        // ================================================================== Wojownik, Paladyn, Berserker, Rębajło

        static void Warriors()
        {
            const string K = "class_knight", P = "class_hu_paladin", B = "class_hu_berserker", R = "class_hu_dual";

            C("W01", "warrior.last_pavise", K, "Pawęż ostatniego zakonnika", ItemKind.Shield, "hu_shield", Guard, 1,
                "Walka w gardzie (wybrana cecha Wojownika) i ręczny blok w jej oknie.",
                "W oknie Walki w gardzie blok płaci maną zamiast wytrzymałością.",
                "Podczas okna Walki w gardzie koszt ręcznego bloku pobierany jest najpierw z many: 1 punkt wytrzymałości = {mana_per_stamina} many. Wytrzymałość zostaje dla ataków i uników.",
                "Mana konkuruje z czarami; gdy many brakuje, resztę pokrywa wytrzymałość (bez podwójnej opłaty). Kąt bloku i ciosy nieblokowalne bez zmian.",
                "Wojownik z Walką w gardzie, hu_sword + ta tarcza, Inteligencja 10.",
                "Różdżka hu_wand_fire w głównej ręce + pawęż; pierścień many i Lodowy odłamek hu_frost_shard na dystans.",
                "Blok w oknie gardy zjada manę – po serii ciosów czar jest niedostępny, a cios w plecy dalej trafia.",
                "mana_per_stamina", 0.6f)
                .S(StatType.MaxMana, 25).S(StatType.MagicDefense, 15).S(StatType.ManaRegen, 1.5f);

            C("W02", "warrior.two_oaths", K, "Pas dwóch przysiąg", ItemKind.Belt, null, Arsenal, 2,
                "Arsenał weterana (cecha Wojownika) i dwa użyte zestawy broni.",
                "Pierwszy celny cios po zmianie zestawu niesie ślad poprzedniej broni.",
                "Pierwszy celny ręczny atak po legalnej zmianie zestawu wywołuje jeden słabszy ślad uderzenia poprzedniej broni ({echo_share} jej lekkiego ciosu) z jej żywiołem i efektami.",
                "Ślad uzbraja się dopiero, gdy poprzednim zestawem rzeczywiście trafiono; spam zmiany nic nie ładuje. Poprzednia broń daje zapisany pakiet, nie swoje pasywne premie.",
                "Wojownik z Arsenałem: hu_dagger w zestawie I, hu_warhammer w zestawie II.",
                "Ognisty miecz hu_sword_fire + mroźny topór, okrzyk hu_warcry i Rozpłatanie hu_cleave jako wykończenie.",
                "Trzeba trafić obiema brońmi – zmiana w złym momencie zostawia bez śladu i bez osłony.",
                "echo_share", 0.45f)
                .S(StatType.EquipLoad, 8).S(StatType.MaxStamina, 15).S(StatType.PhysicalDamage, 8);

            C("W03", "warrior.bellringer", K, "Hełm dzwonnika", ItemKind.Head, null, null, 1,
                "Legalna riposta bronią (parowanie i przełamana postawa wroga).",
                "Riposta staje się falą przed postacią.",
                "Fizyczna riposta uderza falą w stożku {wave_arc}° na {wave_range m} przed postacią: każdy wróg dostaje {wave_share} pełnych obrażeń riposty (premia riposty wzmacnia ten sam budżet).",
                "Pojedynczy cel dostaje mniej niż zwykła riposta; fala nie przechodzi przez ściany.",
                "Dowolna klasa z bronią zdolną do parowania, np. Wojownik z hu_sword i hu_shield.",
                "Pierścień riposty ring_riposte, amulet parowania amulet_parry, Rozpłatanie hu_cleave na resztę tłumu.",
                "Mniejsze skupione obrażenia na bossie – riposta w hordzie, nie w pojedynku.",
                "wave_share", 0.55f, "wave_range", 5f, "wave_arc", 80f)
                .S(StatType.RiposteDamage, 25).S(StatType.ParryWindow, 0.04f).S(StatType.PhysicalDefense, 20);

            C("W04", "warrior.left_watch", K, "Karwasze pozostawionej warty", ItemKind.Hands, null, null, 3,
                "Ręczny blok albo parowanie oraz technika bronią ze slotu (skill HU bez doskoku).",
                "Technika po obronie rodzi się w punkcie warty.",
                "Udany ręczny blok lub parowanie zapisuje punkt warty na {watch_seconds s}. Następna ręcznie użyta technika bronią rodzi się w tym punkcie (kierunek do celu albo przód postaci).",
                "Jedna warta naraz, normalny koszt i odnowienie techniki; technika nie działa przy ciele. Blok bierny tarczy niczego nie uzbraja.",
                "Wojownik z tarczą hu_shield i Rozpłataniem hu_cleave.",
                "Młynek hu_whirlwind zostawiony przy bramie, Fala przełamania hu_breaker_wave z punktu bloku; Walka w gardzie.",
                "Oddalenie od punktu warty – technika nie chroni miejsca, w którym stoisz.",
                "watch_seconds", 5f, "watch_range", 14f)
                .S(StatType.MaxStamina, 15).S(StatType.StaminaRegen, 8).S(StatType.PhysicalDefense, 20);

            C("W05", "warrior.empty_front", K, "Medalion pustego frontu", ItemKind.Amulet, null, null, 4,
                "Szarża hu_charge z jej normalnym wymaganiem broni.",
                "Szarża staje się kontrolowanym odwrotem, który rani ścigających.",
                "Szarża biegnie od celu, po odwróconej linii celowania; broń i tarcza osłaniają przód poruszającego się taranu – wrogowie na trasie dostają trafienie szarży (każdy raz).",
                "Brak końcowego wybuchu szarży, normalny koszt i teren; bez dodatkowej niewrażliwości.",
                "Wojownik z Szarżą hu_charge i bronią dwuręczną hu_warhammer.",
                "Tarcza hu_shield (szarża z tarczą powala), mocny cios ładowany i zwabianie wrogów w linię.",
                "Zamiana doskoku w ofensywę na cofnięcie – nie zamyka dystansu do łucznika.",
                "dash_share", 1f)
                .Sk("hu_charge").S(StatType.MaxStamina, 20).S(StatType.StaminaRegen, 10).S(StatType.EquipLoad, 6);

            // ---- Paladyn
            C("P01", "paladin.hungry_reliquary", P, "Relikwiarz głodnego świętego", ItemKind.Amulet, null, Absolution, 2,
                "Rozgrzeszenie (cecha Paladyna) z zebraną Łaską.",
                "Wydana Łaska zasila kolejne ciosy zamiast fali.",
                "Wydanie Łaski nie tworzy fali: dzieli ją na {charges} ładunków ważnych {charge_seconds s}. Każdy kolejny ręczny cios bronią lub podstawowy pocisk dostaje swój ładunek ({conversion} Łaski na obrażenia świętego ognia).",
                "Pudło zużywa ładunek przypisany do ataku; salwa dzieli jeden ładunek na pociski. Uwolnienie nie leczy i nie tworzy nowej Łaski.",
                "Paladyn z Rozgrzeszeniem, hu_morningstar + hu_shield, Aura odnowy hu_aura_renewal.",
                "Paladyn-łucznik z hu_bow, Aura zapału hu_aura_zeal i nadleczenie z flaszki.",
                "Łaska nie leczy tłumu falą – trzeba trafiać, a pudło przepala ładunki.",
                "charges", 4, "charge_seconds", 8f, "conversion", 1.2f)
                .S(StatType.HealthRegen, 2).S(StatType.FlaskPotency, 10).S(StatType.MaxHealth, 25);

            C("P02", "paladin.divided_litany", P, "Mitra rozdwojonej litanii", ItemKind.Head, null, Judgement, 3,
                "Sąd przysięgi (cecha Paladyna) i co najmniej dwie aktywne aury.",
                "Każdą odwróconą aurę można rzucić na innego wroga.",
                "Sąd przysięgi przenosi pierwszą aktywną aurę na wskazanego wroga, a każde kolejne naciśnięcie w ciągu {assign_seconds s} – następną aurę na nowy cel. Każda aura wraca do Paladyna po swoim czasie albo po śmierci swojego celu.",
                "Rozdzielone aury nie skupiają osłabień na jednym celu; ta sama rezerwacja i czas, bez trzeciej aury.",
                "Paladyn z Sądem przysięgi, Aura męstwa hu_aura_might + Aura świętego ognia hu_aura_holy_fire.",
                "Dwie elity, sługi i walka na dystans z hu_bow; Aura czystości hu_aura_purity.",
                "Dwa cele zamiast jednego – żaden nie jest osłabiony w pełni.",
                "assign_seconds", 2.5f)
                .S(StatType.MaxMana, 25).S(StatType.ManaRegen, 1.5f).S(StatType.MagicDefense, 10);

            C("P03", "paladin.empty_monastery", P, "Habit pustego klasztoru", ItemKind.Body, null, null, 3,
                "Własne aktywne aury wspierające i własne sługi.",
                "Aury wspierające obejmują własne sługi.",
                "Aura zapału przyspiesza ataki sług, a Aura męstwa zwiększa ich obrażenia – w {servant_share} siły aury. Sługa ze zdrowiem (śmiertelna chimera) korzysta też z leczenia Aury odnowy.",
                "Właściciel dostaje tylko {owner_share} premii swoich aur. Jedna aura nie mnoży się przez liczbę sług; sługa bez zdrowia nie korzysta z ochrony.",
                "Paladyn z Aurą zapału hu_aura_zeal i grymuarem przywołań hu_grimoire_summoning (chochliki).",
                "Przywoływacz z Aurą męstwa hu_aura_might, Sforą hu_summon_hounds i Sercem szóstego żebra.",
                "Słabsze własne wsparcie – Paladyn sam bije i broni się gorzej.",
                "servant_share", 0.7f, "owner_share", 0.6f)
                .S(StatType.MaxMana, 30).S(StatType.ManaRegen, 1.5f).S(StatType.PhysicalDefense, 25);

            C("P04", "paladin.penitent_bells", P, "Karwasze dzwonów pokutnych", ItemKind.Hands, null, null, 4,
                "Aktywna Aura świętego ognia hu_aura_holy_fire i ręczne parowanie.",
                "Aura nie parzy na bieżąco – udane parowanie uwalnia jej zgromadzony ogień.",
                "Aura świętego ognia przestaje tykać. Jej budżet narasta z czasem działania (do {cap_seconds s} tyknięć); udane parowanie uwalnia go jedną falą ze środka aury o promieniu {radius m}.",
                "Brak biernego DPS; liczba wrogów nie przyspiesza ładowania. Puste parowanie (bez ładunku) niczego nie uwalnia.",
                "Paladyn z hu_shield, Aura świętego ognia hu_aura_holy_fire, parowanie.",
                "Sąd przysięgi: fala wychodzi z oznaczonej ofiary; Hełm dzwonnika i pierścień riposty.",
                "Bez udanej obrony aura nic nie robi.",
                "cap_seconds", 6f, "radius", 4f, "tick_dps_share", 1f)
                .Sk("hu_aura_holy_fire").S(StatType.ParryWindow, 0.05f).S(StatType.FireDamage, 15).S(StatType.HealthRegen, 1.5f);

            C("P05", "paladin.long_penance", P, "Pastorał długiej pokuty", ItemKind.Weapon, "hu_morningstar", null, 4,
                "Techniki bronią wręcz oraz ręczne leczenie (czar leczenia albo flaszka życia).",
                "Część obrażeń technik odkłada się jako wina, którą rozlicza leczenie.",
                "{guilt_share} obrażeń własnych technik bronią zamiast trafić od razu odkłada się jako wina na ofierze (do {guilt_cap} × lekkiego ciosu). Opłacony ręczny czar leczenia albo wypicie flaszki życia rozlicza całą winę wrogów w promieniu {range m}.",
                "Opóźniona ofensywa i zużyta flaszka/czar. Wyrok nie leczy, nie odkłada nowej winy i nie daje Łaski; jeden wyrok na rozliczenie mimo leczenia obszarowego.",
                "Paladyn z tym pastorałem, hu_shield i Odtrąceniem hu_repel (technika dowolnej broni wręcz).",
                "Piekielna szarża hu_infernal_charge + Aura odnowy hu_aura_renewal; Relikwiarz głodnego świętego.",
                "Bez leczenia wina przepada po {guilt_seconds s}.",
                "guilt_share", 0.35f, "guilt_cap", 3f, "range", 14f, "guilt_seconds", 12f)
                .S(StatType.FlaskPotency, 15).S(StatType.MaxMana, 20).S(StatType.HealthRegen, 1f);

            // ---- Berserker
            C("B01", "berserker.hungry_scars", B, "Kolczuga głodnych blizn", ItemKind.Body, null, RedLine, 2,
                "Aktywny trans Czerwonej linii (cecha Berserkera).",
                "Cios zredukowany przez Żelazną skórę oddaje krwawy odwet.",
                "Rzeczywisty wrogi cios wręcz zredukowany przez Żelazną skórę zadaje napastnikowi odwet {retaliate_share} normalnego maksimum życia (× procent redukcji skóry), najwyżej raz na {retaliate_cooldown s}.",
                "Własne podstawowe ataki ×{basic_mult}. Koszt życia, rezerwacja i tyknięcia nie wywołują odwetu; skóra i śmierć bez zmian.",
                "Berserker z Czerwoną linią i hu_greataxe.",
                "Okrzyk hu_warcry, pierścień życia i topór kroczącej rzezi.",
                "Słabsze ciosy własne – trzeba przyjmować uderzenia, żeby zabijać.",
                "retaliate_share", 0.12f, "retaliate_cooldown", 0.6f, "basic_mult", 0.85f)
                .S(StatType.MaxHealth, 40).S(StatType.HealthRegen, 1.5f).S(StatType.PhysicalDefense, 20);

            C("B02", "berserker.angry_spirits", B, "Różaniec gniewnych duchów", ItemKind.Amulet, null, FurySpend, 3,
                "Spalenie furii (cecha Berserkera).",
                "Żetony gniewu stają się widmami wojowników.",
                "Każdy powstały żeton zamiast finansować skill staje się widmem na {spirit_seconds s} (najwyżej {max_spirits}), które co {spirit_interval s} uderza zapisanym profilem aktualnej broni ({spirit_share} lekkiego ciosu).",
                "Zużyty szał i brak darmowych opłat skilli. Widma nie kopiują legend, nie tworzą żetonów i nie leczą.",
                "Berserker ze Spaleniem furii i hu_greataxe.",
                "Aura męstwa hu_aura_might (Habit), Okrzyk hu_warcry, ciężka broń hu_warhammer.",
                "Bez darmowych skilli – trzeba płacić wytrzymałością.",
                "spirit_seconds", 6f, "max_spirits", 3, "spirit_interval", 1.1f, "spirit_share", 0.4f)
                .S(StatType.MaxStamina, 20).S(StatType.PhysicalDamage, 10).S(StatType.MaxHealth, 20);

            C("B03", "berserker.walking_slaughter", B, "Topór kroczącej rzezi", ItemKind.Weapon, "hu_greataxe", null, 4,
                "Własne krwawienia na wrogach.",
                "Rany krwawią, gdy ty krążysz wokół ofiary.",
                "Krwawienie od tego właściciela płynie szybciej, gdy właściciel porusza się w promieniu {range m} od ofiary (×{moving_mult}); ruch ofiary nie przyspiesza już ran. Gdy stoisz, rany płyną ×{standing_mult}.",
                "Obrót kamery, teleport i drgania nie liczą się jako ruch (limit {max_step m} na klatkę). Rana ma skończoną sumę.",
                "Berserker z tym toporem i Krwawym cięciem skill_rend albo Rozpłataniem hu_cleave.",
                "Uniki i atak po uniku, buty ruchu, Okrzyk hu_warcry.",
                "Stanie w miejscu spowalnia rany – trzeba ryzykować ruch przy wrogu.",
                "range", 4f, "moving_mult", 2.2f, "standing_mult", 0.5f, "max_step", 0.6f)
                .S(StatType.BleedDamage, 20).S(StatType.MoveSpeed, 5);

            C("B04", "berserker.chained_butcher", B, "Karwasze przykutego rzeźnika", ItemKind.Hands, null, null, 4,
                "Ciężki rzut skill_throw (rzut prawdziwą bronią) z jego wymaganiami.",
                "Rzucona broń wbija się w ofiarę, a mocny cios pięścią przyciąga cię do niej.",
                "Rzucona broń zatrzymuje się na pierwszej ofierze (do {anchor_seconds s}). Opłacony mocny atak pięścią przyciąga cię do wbitej broni ({pull_speed m}/s, kolizje); po dojściu broń wraca do ręki i kończy sekwencję uderzeniem {finish_share} ciężkiego ciosu.",
                "Broń niedostępna do bloku i powrotu; ruch bez niewrażliwości. Śmierć ofiary zwraca broń bez kończącego trafienia.",
                "Berserker z hu_greataxe i Ciężkim rzutem skill_throw.",
                "Pięści i Okrzyk hu_warcry, Topór kroczącej rzezi jako rzucana broń.",
                "Bez broni w ręce – walka pięściami do odzyskania oręża.",
                "anchor_seconds", 6f, "pull_speed", 14f, "finish_share", 0.8f, "max_pull", 14f)
                .Sk("skill_throw").S(StatType.MaxStamina, 15).S(StatType.PhysicalDamage, 10);

            C("B05", "berserker.held_rage", B, "Pierścień nierozładowanego szału", ItemKind.Ring, null, null, 4,
                "Legalny szał i ładowany mocny atak.",
                "Szał nie przyspiesza – ładowany cios odkłada kolejne ciężkie cięcia.",
                "Szał nie daje premii szybkości. Podczas ładowania mocnego ataku co {cut_charge s} odkładasz kolejne cięcie (do {max_cuts}); puszczenie wyzwala serię cięć po {cut_share} ciężkiego ciosu.",
                "Każde cięcie zużywa {rage_per_cut s} szału i koszt ataku w wytrzymałości. Tych sekund nie wyda potem Spalenie furii.",
                "Berserker z szałem i hu_greataxe.",
                "Czerwona linia, Kolczuga głodnych blizn i Topór kroczącej rzezi.",
                "Powolne okno ładowania przy niskim życiu zamiast szybkich ciosów.",
                "cut_charge", 0.45f, "max_cuts", 3, "cut_share", 0.55f, "rage_per_cut", 1.2f)
                .S(StatType.PhysicalDamage, 12).S(StatType.MaxStamina, 15).S(StatType.MaxHealth, 15);

            // ---- Rębajło
            C("R01", "dual.double_secret", R, "Rękawice podwójnego sekretu", ItemKind.Hands, null, Mismatched, 2,
                "Niezgodna para (cecha Rębajły).",
                "Różdżka w lewej ręce – splot to cięcie i krótki wystrzał.",
                "Pozwala założyć jednoręczną różdżkę do drugiej ręki. W oknie Niezgodnej pary splot to cięcie główną bronią i krótki wystrzał różdżki ({shot_share} jej ciosu) za {shot_mana} many.",
                "Lewa ręka nie daje drugiego ostrza ani premii dwóch broni. Zdjęcie rękawic odsyła różdżkę z drugiej ręki do plecaka.",
                "Rębajło z Niezgodną parą, hu_sword + hu_wand_fire w drugiej ręce.",
                "Fizyczna rana z hu_sabre + różdżka mrozu hu_wand_water; Pas żonglera ostrzami pokazuje warunek nieaktywny.",
                "Mana na każdy splot i brak drugiego ostrza.",
                "shot_share", 0.6f, "shot_mana", 4f)
                .S(StatType.MaxMana, 20).S(StatType.AttackSpeed, 5).S(StatType.SpellPower, 10);

            C("R02", "dual.unfinished_step", R, "Buty niedokończonego kroku", ItemKind.Feet, null, Dance, 2,
                "Taniec bez przerwy (cecha Rębajły).",
                "Unik z podtrzymanym atakiem zostawia odłożone cięcie.",
                "Unik przenoszący atak odkłada {trace_share} cięcia w miejscu startu (najwyżej {max_traces} ślady, {trace_seconds s}). Następny mocny atak uwalnia wszystkie ślady.",
                "Przeniesiony cios jest o tę część słabszy; ten sam unik nie odnawia śladu. Brak nowych klatek niewrażliwości.",
                "Rębajło z Tańcem bez przerwy, hu_sword + hu_dagger.",
                "Pułapki z odłożonych cięć i Młynek hu_whirlwind; Amulet zbłąkanej klingi.",
                "Słabszy bieżący cios – zysk przychodzi dopiero przy mocnym ataku.",
                "trace_share", 0.4f, "max_traces", 2, "trace_seconds", 4f, "trace_radius", 1.8f)
                .S(StatType.MoveSpeed, 6).S(StatType.MaxStamina, 15).S(StatType.PhysicalDamage, 6);

            C("R03", "dual.blade_juggler", R, "Pas żonglera ostrzami", ItemKind.Belt, null, null, 3,
                "Jednoręczna broń wręcz w drugiej ręce.",
                "Mocny atak posyła lewą broń na orbitę.",
                "Mocny atak wysyła broń z lewej ręki na orbitę o promieniu {orbit_radius m} na {orbit_seconds s}; co {tick s} rani wrogów na orbicie ({orbit_share} jej lekkiego ciosu). Prawa ręka walczy normalnie.",
                "Lewa nie atakuje w ręce i nie daje premii dwóch broni; broń wraca raz. Różdżka nie staje się ostrzem do rzutu.",
                "Rębajło z hu_sword + hu_dagger.",
                "Krwawiący sztylet hu_dagger_kris, Taniec bez przerwy, Amulet zbłąkanej klingi.",
                "Bez lewego ostrza w ręce – mniej lekkich ciosów na zmianę.",
                "orbit_radius", 2.2f, "orbit_seconds", 4f, "tick", 0.4f, "orbit_share", 0.5f)
                .S(StatType.AttackSpeed, 6).S(StatType.MaxStamina, 15).S(StatType.BleedDamage, 10);

            C("R04", "dual.reverse_wounds", R, "Szabla wspak pisanych ran", ItemKind.Weapon, "hu_sabre", null, 4,
                "Cięcie krzyżowe hu_cross_cut i dwie legalne bronie.",
                "Cięcie krzyżowe otwiera, a drugi cios zadaje przeciwna ręka.",
                "Cięcie krzyżowe zadaje tylko pierwsze cięcie ({open_share} mocy). Drugie czeka {answer_seconds s} na następne ręczne trafienie lewą ręką i wtedy uderza z profilem tej ręki ({answer_share} jej lekkiego ciosu).",
                "Okno wygasa bez zwrotu kosztu i pierwszego etapu; drugi pakiet Niezgodnej pary nie jest nowym trafieniem.",
                "Rębajło z tą szablą, hu_dagger w lewej ręce i Cięciem krzyżowym hu_cross_cut.",
                "Lekka ręka otwiera, wolniejsza zamyka; dwa żywioły (hu_sword_fire w lewej).",
                "Bez trafienia drugą ręką połowa cięcia przepada.",
                "open_share", 0.6f, "answer_seconds", 3f, "answer_share", 1.1f)
                .Sk("hu_cross_cut").S(StatType.BleedDamage, 15).S(StatType.AttackSpeed, 6);

            C("R05", "dual.wandering_blade", R, "Amulet zbłąkanej klingi", ItemKind.Amulet, null, null, 4,
                "Młynek hu_whirlwind i broń wręcz w głównej ręce.",
                "Młynek zostawia główną broń wirującą w miejscu.",
                "Młynek nie obraca postaci: główna broń wiruje w punkcie rozpoczęcia przez {vortex_seconds s} (co {tick s} trafienie {spin_share} lekkiego ciosu). Następny opłacony mocny atak przywołuje wir po ziemi do właściciela.",
                "Główna broń jest w tym czasie niedostępna – lewa walczy sama, bez niej pięści. Czas i suma trafień nie odnawiają się przy przyciąganiu.",
                "Rębajło albo Wojownik z hu_sword i Młynkiem hu_whirlwind.",
                "Pas żonglera ostrzami (druga broń w osobnym wirze), zwabianie przez Sidła hu_snare.",
                "Bez głównej broni w dłoni do czasu przywołania.",
                "vortex_seconds", 4f, "tick", 0.35f, "spin_share", 0.7f, "radius", 2f, "recall_speed", 12f)
                .Sk("hu_whirlwind").S(StatType.MaxStamina, 20).S(StatType.PhysicalDamage, 8);
        }
    }
}
