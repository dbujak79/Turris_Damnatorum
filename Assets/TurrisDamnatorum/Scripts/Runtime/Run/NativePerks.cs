using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Strojenie czterech natywnych perków specjalizacyjnych (docs/PROMPT_WDROZENIE_PERKOW_SPECJALIZACYJNYCH.md) – jedno źródło liczb
    /// dla walki, sklepu, opisów i testów. Pole w <see cref="GameConfig"/> z bezpiecznymi wartościami domyślnymi (stary asset bez pola
    /// dostaje te liczby; wartości spoza zakresu są poprawiane przez <see cref="Valid"/>).
    /// </summary>
    [Serializable]
    public class NativePerkTuning
    {
        [Header("Pojedynkowicz")]
        public float duelistSpeed = 0.15f;
        public float duelistPrice = 28f, duelistWeight = 0.7f; public int duelistFloor = 1;
        [Header("Wykorzystaj wyłom")]
        public float breachMultiplier = 1.5f, breachSeconds = 3f;
        [Tooltip("Okno otwiera tylko wróg z maksymalną postawą co najmniej tyle (po mnożniku elity) – twardy przeciwnik, nie zwykły.")]
        public float breachMinPoise = 50f;
        public float breachPrice = 30f, breachWeight = 0.8f; public int breachFloor = 3;
        [Header("Różnorodna sfora")]
        public float packPerKind = 0.15f;
        public float packPrice = 32f, packWeight = 0.65f;
        [Tooltip("Od piętra, na którym dostępny jest drugi rodzaj sług (Sfora, fala 7) – wcześniej perk dawałby 0%.")]
        public int packFloor = 7;
        [Header("Gwałtowna manifestacja")]
        [Tooltip("Mnożnik czasu i odstępów tyknięć kwalifikowanej strefy (0,5 = dwa razy szybciej, te same trafienia).")]
        public float manifestationTime = 0.5f;
        public float manifestationPrice = 30f, manifestationWeight = 0.65f; public int manifestationFloor = 1;
        [Header("Otwarta rana (rodzina Łotrzyka)")]
        public float woundPerStack = 0.04f, woundCap = 0.12f;
        public float woundPrice = 30f, woundWeight = 0.7f; public int woundFloor = 1;
        [Header("Długi oddech (Zasadzka)")]
        [Tooltip("Okno Zasadzki po uniku (s) i premia ciosu w plecy zamiast klasowych 1,5 s i +60%.")]
        public float breathWindow = 2.5f, breathBackBonus = 0.5f;
        public float breathPrice = 28f, breathWeight = 0.7f; public int breathFloor = 1;
        [Header("Lekki krok (rodzina Łotrzyka)")]
        [Tooltip("Mnożnik wytrzymałości pierwszego lekkiego ciosu po uniku.")]
        public float stepCost = 0.5f;
        public float stepPrice = 26f, stepWeight = 0.7f; public int stepFloor = 1;
        [Header("Daleki strzał (rodzina Łotrzyka, łuk)")]
        public float farBonus = 0.12f, farPenalty = 0.12f, farDistance = 8f, nearDistance = 4f;
        public float farPrice = 30f, farWeight = 0.7f; public int farFloor = 1;

        static float F(float v, float fallback, float min, float max) => float.IsNaN(v) || float.IsInfinity(v) ? fallback : Mathf.Clamp(v, min, max);

        /// <summary>Kopia z poprawionymi wartościami (brak NaN/Infinity, dodatni czas okna i kompresja).</summary>
        public NativePerkTuning Valid() => new NativePerkTuning
        {
            duelistSpeed = F(duelistSpeed, 0.15f, 0f, 2f), duelistPrice = F(duelistPrice, 28f, 1f, 1000f), duelistWeight = F(duelistWeight, 0.7f, 0f, 10f), duelistFloor = Mathf.Max(1, duelistFloor),
            breachMultiplier = F(breachMultiplier, 1.5f, 1f, 5f), breachMinPoise = F(breachMinPoise, 50f, 0f, 100000f), breachSeconds = F(breachSeconds, 3f, 0.1f, 30f), breachPrice = F(breachPrice, 30f, 1f, 1000f), breachWeight = F(breachWeight, 0.8f, 0f, 10f), breachFloor = Mathf.Max(1, breachFloor),
            packPerKind = F(packPerKind, 0.15f, 0f, 2f), packPrice = F(packPrice, 32f, 1f, 1000f), packWeight = F(packWeight, 0.65f, 0f, 10f), packFloor = Mathf.Max(1, packFloor),
            manifestationTime = F(manifestationTime, 0.5f, 0.05f, 1f), manifestationPrice = F(manifestationPrice, 30f, 1f, 1000f), manifestationWeight = F(manifestationWeight, 0.65f, 0f, 10f), manifestationFloor = Mathf.Max(1, manifestationFloor),
            woundPerStack = F(woundPerStack, 0.04f, 0f, 1f), woundCap = F(woundCap, 0.12f, 0f, 2f), woundPrice = F(woundPrice, 30f, 1f, 1000f), woundWeight = F(woundWeight, 0.7f, 0f, 10f), woundFloor = Mathf.Max(1, woundFloor),
            breathWindow = F(breathWindow, 2.5f, 0.1f, 30f), breathBackBonus = F(breathBackBonus, 0.5f, 0f, 5f), breathPrice = F(breathPrice, 28f, 1f, 1000f), breathWeight = F(breathWeight, 0.7f, 0f, 10f), breathFloor = Mathf.Max(1, breathFloor),
            stepCost = F(stepCost, 0.5f, 0f, 1f), stepPrice = F(stepPrice, 26f, 1f, 1000f), stepWeight = F(stepWeight, 0.7f, 0f, 10f), stepFloor = Mathf.Max(1, stepFloor),
            farBonus = F(farBonus, 0.12f, 0f, 2f), farPenalty = F(farPenalty, 0.12f, 0f, 0.9f), farDistance = F(farDistance, 8f, 0.1f, 100f), nearDistance = F(nearDistance, 4f, 0f, 100f), farPrice = F(farPrice, 30f, 1f, 1000f), farWeight = F(farWeight, 0.7f, 0f, 10f), farFloor = Mathf.Max(1, farFloor),
        };
    }

    /// <summary>
    /// Natywne perki Turris kupowane za dusze (po jednym poziomie): definicje <see cref="BoonDefinition"/> budowane w pamięci dla danej
    /// konfiguracji (bez assetów i bez podszywania się pod HU), kwalifikacja buildu, ceny i opisy. Ta sama kwalifikacja służy sklepowi,
    /// aktywności perka w statystykach, analizie i walce.
    /// </summary>
    public static class NativePerkCatalog
    {
        public const string Duelist = "perk.turris.duelist", Breach = "perk.turris.exploit_breach", Pack = "perk.turris.diverse_pack", Manifestation = "perk.turris.violent_manifestation";
        public const string OpenWound = "perk.turris.open_wound", LongBreath = "perk.turris.long_breath", LightStep = "perk.turris.light_step", FarShot = "perk.turris.far_shot";
        public static readonly string[] Ids = { Duelist, Breach, Pack, Manifestation, OpenWound, LongBreath, LightStep, FarShot };

        static readonly Dictionary<GameConfig, List<BoonDefinition>> built = new Dictionary<GameConfig, List<BoonDefinition>>();

        public static bool IsNative(BoonDefinition b) => b != null && Ids.Contains(b.id);

        public static NativePerkTuning Tuning(GameConfig cfg) => (cfg != null && cfg.nativePerks != null ? cfg.nativePerks : new NativePerkTuning()).Valid();

        static string N(float v) => v.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');
        static string Pc(float v) => (v * 100f).ToString("0.#", CultureInfo.InvariantCulture).Replace('.', ',') + "%";

        public static string Name(string id) =>
            id == Duelist ? "Pojedynkowicz" : id == Breach ? "Wykorzystaj wyłom" : id == Pack ? "Różnorodna sfora" : id == Manifestation ? "Gwałtowna manifestacja"
            : id == OpenWound ? "Otwarta rana" : id == LongBreath ? "Długi oddech" : id == LightStep ? "Lekki krok" : id == FarShot ? "Daleki strzał" : id;

        /// <summary>Dokładna korzyść (liczby ze strojenia).</summary>
        public static string Benefit(string id, NativePerkTuning t)
        {
            switch (id)
            {
                case Duelist: return $"+{Pc(t.duelistSpeed)} szybkości podstawowych ataków wręcz.";
                case Breach: return $"Po przełamaniu postawy twardego wroga (postawa od {N(t.breachMinPoise)}: elity, ciężcy, bossowie) następne trafienie nową ręczną akcją w ten cel zadaje +{Pc(t.breachMultiplier - 1f)} obrażeń. Okno: {N(t.breachSeconds)} s.";
                case Pack: return $"Sługi zadają +{Pc(t.packPerKind)} obrażeń za każdy inny aktywny rodzaj ponad pierwszy (najwyżej +{Pc(t.packPerKind * 2f)}).";
                case OpenWound: return $"Bezpośredni ręczny cios w krwawiący cel zadaje +{Pc(t.woundPerStack)} obrażeń za warstwę krwawienia (najwyżej +{Pc(t.woundCap)}).";
                case LongBreath: return $"Okno Zasadzki po uniku trwa {N(t.breathWindow)} s, ale cios w plecy daje +{Pc(t.breathBackBonus)} zamiast +60%.";
                case LightStep: return $"Pierwszy lekki cios wręcz po uniku kosztuje {Pc(t.stepCost)} wytrzymałości.";
                case FarShot: return $"Zwykły strzał z łuku: ponad {N(t.farDistance)} m +{Pc(t.farBonus)} obrażeń, poniżej {N(t.nearDistance)} m −{Pc(t.farPenalty)}.";
                case Manifestation: return $"Kwalifikowane strefy czarów trwają ×{N(t.manifestationTime)} krócej i tykają tyle samo razy z tą samą siłą (szybciej).";
            }
            return "";
        }

        public static string Condition(string id)
        {
            switch (id)
            {
                case Duelist: return "Wymaga jednoręcznej broni do walki wręcz i pustej drugiej ręki.";
                case Breach: return "Wymaga ręcznej ofensywy łamiącej postawę albo parowania.";
                case Pack: return "Wymaga legalnego skilla przywołań w slotach (także z grymuaru).";
                case OpenWound: return "Rodzina Łotrzyka; wymaga broni wręcz i źródła krwawienia (broń albo skill w slocie).";
                case LongBreath: return "Klasy z Zasadzką (Łotrzyk, Zabójca).";
                case LightStep: return "Rodzina Łotrzyka; wymaga broni do walki wręcz.";
                case FarShot: return "Rodzina Łotrzyka; wymaga łuku.";
                case Manifestation: return "Wymaga ręcznego czaru ze strefą zadającą obrażenia (Płonąca ziemia, Krąg żaru, strefa novy III, Meteor).";
            }
            return "";
        }

        public static string Tradeoff(string id)
        {
            switch (id)
            {
                case Duelist: return "Brak tarczy, grymuaru i drugiej broni; więcej wytrzymałości na sekundę.";
                case Breach: return "Trzeba najpierw przełamać postawę i zdążyć w krótkim oknie; zwykli wrogowie nie otwierają okna.";
                case Pack: return "Różne przywołania zajmują sloty i manę; jednorodna sfora nie dostaje premii.";
                case OpenWound: return "Trzeba najpierw nałożyć krwawienie; samo krwawienie nie rośnie.";
                case LongBreath: return "Słabszy cios w plecy za łatwiejsze otwarcie po uniku.";
                case LightStep: return "Działa raz na unik i tylko blisko wroga.";
                case FarShot: return "Kara w zwarciu; wymaga miejsca na dystans.";
                case Manifestation: return "Krótsza kontrola miejsca – strefa znika w połowie czasu.";
            }
            return "";
        }

        /// <summary>Definicje tej konfiguracji (idempotentnie, osobno dla każdej konfiguracji – brak stanu przeciekającego między nimi).</summary>
        public static IReadOnlyList<BoonDefinition> Boons(GameConfig cfg)
        {
            if (cfg == null) return new List<BoonDefinition>();
            if (built.TryGetValue(cfg, out var list) && list.All(b => b != null)) return list;
            var t = Tuning(cfg);
            list = new List<BoonDefinition>();
            foreach (var id in Ids)
            {
                var b = ScriptableObject.CreateInstance<BoonDefinition>();
                b.id = b.name = id; b.displayName = Name(id); b.maxStacks = 1;
                b.description = Benefit(id, t) + " " + Condition(id);
                list.Add(b);
            }
            built[cfg] = list;
            return list;
        }

        public static BoonDefinition Get(GameConfig cfg, string id) => Boons(cfg).FirstOrDefault(b => b.id == id);

        public static float BasePrice(string id, NativePerkTuning t) => id == Duelist ? t.duelistPrice : id == Breach ? t.breachPrice : id == Pack ? t.packPrice
            : id == OpenWound ? t.woundPrice : id == LongBreath ? t.breathPrice : id == LightStep ? t.stepPrice : id == FarShot ? t.farPrice : t.manifestationPrice;
        public static float Weight(string id, NativePerkTuning t) => id == Duelist ? t.duelistWeight : id == Breach ? t.breachWeight : id == Pack ? t.packWeight
            : id == OpenWound ? t.woundWeight : id == LongBreath ? t.breathWeight : id == LightStep ? t.stepWeight : id == FarShot ? t.farWeight : t.manifestationWeight;
        public static int MinFloor(string id, NativePerkTuning t) => id == Duelist ? t.duelistFloor : id == Breach ? t.breachFloor : id == Pack ? t.packFloor
            : id == OpenWound ? t.woundFloor : id == LongBreath ? t.breathFloor : id == LightStep ? t.stepFloor : id == FarShot ? t.farFloor : t.manifestationFloor;

        /// <summary>Cena: baza × skala HU × inflacja sklepu – dokładnie raz (jak perki HU).</summary>
        public static int Price(string id, NativePerkTuning t, RunState run, BalanceConfig b) =>
            Mathf.RoundToInt(BasePrice(id, t) * HellEconomy.PriceScale * SoulShop.Inflation(run, b));

        // ================================================================== Kwalifikacja (możliwość wykorzystania w buildzie)

        /// <summary>Broń jednoręczna do walki wręcz (bez pięści, łuków, różdżek, kosturów i par ostrzy jako jednego przedmiotu).</summary>
        public static bool OneHandedMelee(ItemDefinition d)
        {
            if (d == null || !d.IsWeapon || d.twoHanded || d.weapon?.light == null || d.weapon.light.delivery != AttackDelivery.Melee) return false;
            if (d.id == "weapon_fists") return false;
            var model = AnimResolve.ForItem(d);
            return model != WeaponModel.None && model != WeaponModel.Knives && model != WeaponModel.Claws && model != WeaponModel.Staff && model != WeaponModel.Bow;
        }

        public static bool DuelistHands(ItemDefinition main, ItemDefinition off) => OneHandedMelee(main) && off == null;

        static IEnumerable<SpellDefinition> SlotSkills(RunState run, ItemDefinition main, ItemDefinition off)
        {
            foreach (var s in run.skillSlots)
                if (s?.definition != null && WeaponFamilies.Usable(s.definition, main, off)) yield return s.definition;
        }

        static bool RogueFamily(RunState run) => FloorShop.Family(run.startingClass) == "rogue";
        static bool Melee(ItemDefinition d) => d != null && d.IsWeapon && d.weapon?.light != null && d.weapon.light.delivery == AttackDelivery.Melee;
        static bool Bow(ItemDefinition d) => d != null && d.IsWeapon && d.weapon?.light != null && d.weapon.light.delivery == AttackDelivery.Projectile && AnimResolve.ForItem(d) == WeaponModel.Bow;

        /// <summary>Broń albo skill w slocie nakłada krwawienie.</summary>
        static bool BleedSource(RunState run, ItemDefinition main, ItemDefinition off)
        {
            bool Bleeds(AttackDefinition a) => a?.statuses != null && a.statuses.Exists(x => x.kind == StatusKind.Bleed);
            foreach (var w in new[] { main, off })
                if (w != null && w.IsWeapon && w.weapon != null && (Bleeds(w.weapon.light) || Bleeds(w.weapon.heavy))) return true;
            foreach (var s in run.skillSlots)
            {
                if (s?.definition == null || !WeaponFamilies.Usable(s.definition, main, off)) continue;
                if (Bleeds(s.definition.attack) || (s.seals != null && s.seals.Shape.Parameters.On("effect.Bleed"))) return true;
            }
            return false;
        }

        static bool IsSummon(SpellInstance s) => s?.definition != null && (s.definition.kind == SpellKind.Summon || (s.seals != null && s.seals.Shape.HasComponent("Summon")));

        /// <summary>Skill ma kwalifikowaną strefę (ręczny czar ofensywny, strefa z obrażeniami na aktualnym poziomie i pieczęciach).</summary>
        public static bool HasQualifiedZone(SpellInstance s)
        {
            if (s?.definition == null || !s.definition.IsSpell || s.definition.IsAura) return false;
            if (s.seals != null)
            {
                var shape = s.seals.Shape;
                if (shape.HasComponent("Summon") || shape.HasComponent("Aura")) return false;
                if (shape.HasComponent("Zone") && shape.Parameters.GetF("zone_seconds") > 0f) return true;
                return shape.HasComponent("Nova") && shape.Parameters.GetF("zone_seconds") > 0f;
            }
            var d = s.definition;
            return (d.kind == SpellKind.Zone && d.duration > 0f) || (d.kind == SpellKind.Meteor && d.duration > 0f && d.tickInterval > 0f && d.attack.radius > 0f);
        }

        public static bool Usable(string id, RunState run) =>
            run != null && UsableWith(id, run, run.equipment.Get(EquipSlot.MainHand)?.definition, run.equipment.Get(EquipSlot.OffHand)?.definition);

        /// <summary>Kwalifikacja z podanymi rękami (analiza „po zmianie” bez zakładania przedmiotu na żywej postaci).</summary>
        public static bool UsableWith(string id, RunState run, ItemDefinition main, ItemDefinition off)
        {
            switch (id)
            {
                case Duelist: return DuelistHands(main, off);
                case OpenWound: return RogueFamily(run) && Melee(main) && BleedSource(run, main, off);
                case LongBreath: return run.startingClass != null && run.startingClass.trait == ClassTrait.Ambush;
                case LightStep: return RogueFamily(run) && Melee(main);
                case FarShot: return RogueFamily(run) && Bow(main);
                case Pack:
                    foreach (var s in run.skillSlots) if (IsSummon(s) && WeaponFamilies.Usable(s.definition, main, off)) return true;
                    return false;
                case Manifestation:
                    foreach (var s in run.skillSlots) if (HasQualifiedZone(s) && WeaponFamilies.Usable(s.definition, main, off)) return true;
                    return false;
                case Breach:
                    // Ręczna ofensywa z postawą: broń wręcz/dystansowa z dodatnią postawą albo parowanie (broń, tarcza) albo czar w slocie.
                    var w = main != null && main.IsWeapon ? main.weapon : null;
                    bool weaponPoise = w != null && (w.light.poiseDamage > 0f || w.heavy.poiseDamage > 0f);
                    bool parry = (w != null && w.canParry) || (off != null && off.IsShield && off.shield.canParry);
                    return weaponPoise || parry || SlotSkills(run, main, off).Any(d => d.attack != null && d.attack.poiseDamage > 0f);
            }
            return false;
        }

        /// <summary>Powód uśpienia (null – perk działa w tym buildzie).</summary>
        public static string Dormant(string id, RunState run) => Usable(id, run) ? null : "uśpiony: " + Condition(id).Replace("Wymaga", "wymaga");

        /// <summary>Tylko testy: odbudowa po zmianie strojenia.</summary>
        public static void ResetForTests() => built.Clear();
    }
}
