using System.Linq;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Przedmioty bota: między piętrami przymierza zbroje, biżuterię i tarcze z plecaka i zostawia te, które poprawiają build
    /// (broń zostaje – od jej rodzaju zależą skille). Ocena: obrażenia na sekundę broni, życie i obrona z obliczonego buildu.
    /// </summary>
    public abstract partial class HeroBot
    {
        /// <summary>Wartość buildu: DPS lekkich ciosów, życie, obrona fizyczna i magiczna, moc czarów (proste wagi gracza H&amp;S).</summary>
        protected virtual float GearScore()
        {
            var b = BuildCalculator.Compute(Root.Run, Root.config);
            var s = b.stats;
            float dps = (b.WeaponDamage(b.weapon.light) + AddedElementDamage(b)) * b.attackSpeed;
            return dps + MinionScore(b) + 0.25f * s[StatType.MaxHealth] + 0.4f * (s[StatType.PhysicalDefense] + s[StatType.MagicDefense])
                   + 0.3f * s[StatType.SpellPower] + 0.2f * s[StatType.MaxStamina] + EffectScore(b) + UniqueScore(Root.Run, dps) + NativePerkGearScore(Root.Run);
        }

        /// <summary>Dodane żywioły lekkiego ciosu z premią danego żywiołu (obok obrażeń fizycznych broni).</summary>
        protected static float AddedElementDamage(BuildSnapshot b)
        {
            float bas = b.WeaponBase(b.weapon.light), sum = 0f;
            foreach (var e in new[] { Element.Fire, Element.Frost, Element.Lightning, Element.Earth })
                sum += AffixMath.AddedElement(bas, b.stats[Names.AddedWeaponStat(e).Value]) * (1f + b.stats[Names.ElementDamageStat(e)] / 100f);
            return sum;
        }

        /// <summary>Obrażenia sług mają wartość tylko z przywołaniem w slotach (punkt za procent, jak moc czarów).</summary>
        protected static float MinionScore(BuildSnapshot b) =>
            b.skills.Any(s => s != null && s.Def.kind == SpellKind.Summon) ? 0.3f * b.stats[StatType.MinionDamage] : 0f;

        /// <summary>
        /// Natywne unikaty: wartość ma tylko moc, która w tym wariancie działa (cecha właściciela, skill w slocie) – uśpiony unikat
        /// liczy się wyłącznie statystykami, więc statystycznie lepszy zwykły przedmiot wygrywa i uśpiony trafia do sprzedaży.
        /// </summary>
        protected static float UniqueScore(RunState run, float dps)
        {
            float v = 0f;
            foreach (var it in run.equipment.Items)
            {
                var card = UniqueRegistry.CardOf(it.definition);
                if (card == null || !UniqueRegistry.WouldBeActive(card, run)) continue;
                v += MeasuredPowerGain.TryGetValue(card.code, out var gain) ? dps * (gain - 1f) : UniquePowerValue;
            }
            return v;
        }

        /// <summary>Wartość działającej mocy bez pomiaru (w punktach oceny buildu – ok. kilku procent typowego buildu, nie automatyczna wygrana).</summary>
        public const float UniquePowerValue = 25f;

        /// <summary>
        /// Zmierzony zysk obrażeń z mocy (UniqueMeasurePlayTests, średnia grupy i twardego celu, 2026-11-01): wycena = DPS × (zysk − 1),
        /// więc moc szkodząca w prostej grze obniża ocenę. Pozostałe moce mają stałą wartość, dopóki nie zostaną zmierzone.
        /// </summary>
        static readonly System.Collections.Generic.Dictionary<string, float> MeasuredPowerGain = new System.Collections.Generic.Dictionary<string, float>
        {
            ["B02"] = 1.30f, ["R03"] = 1.07f, ["R05"] = 1.05f, ["B05"] = 1.05f, ["S03"] = 1.13f, ["P02"] = 0.92f, ["F04"] = 1.00f, ["T01"] = 0.87f,
        };

        /// <summary>
        /// Wartość efektów pasywnych z przedmiotów HU (uchylenie, blok bierny, moce unikatów, dusze, życie i mana za zabójstwo, przebicie, obszar) –
        /// bez niej bot uznałby lekką zbroję z uchyleniem albo unikat z mocą za bezużyteczne i je sprzedał.
        /// </summary>
        protected static float EffectScore(BuildSnapshot b)
        {
            var fx = b.effects;
            float powers = 0f;
            foreach (var t in new[] { PassiveEffectType.CheatDeath, PassiveEffectType.CorpseExplosion, PassiveEffectType.EmberAura, PassiveEffectType.Thorns,
                                      PassiveEffectType.WeaponChainLightning, PassiveEffectType.FrostWard, PassiveEffectType.LowHealthDamage, PassiveEffectType.SoulHarvest })
                if (fx[t] > 0f) powers += 10f;
            float pen = fx[PassiveEffectType.PenetrationPhysical] + fx[PassiveEffectType.PenetrationFire] + fx[PassiveEffectType.PenetrationFrost]
                        + fx[PassiveEffectType.PenetrationLightning] + fx[PassiveEffectType.PenetrationEarth];
            return 1.5f * b.evasionChance * 100f + 1f * b.passiveBlockChance * 100f + powers + 0.5f * pen
                   + 0.3f * fx[PassiveEffectType.AreaSize] + 0.2f * fx[PassiveEffectType.SoulGain]
                   + 1f * fx[PassiveEffectType.HealOnKill] + 0.5f * fx[PassiveEffectType.ManaOnKill];
        }

        /// <summary>
        /// Sprzedaż (HU, 35% wartości): po przymierzeniu wszystkiego w plecaku zostaje to, czego bot nie użyje – przedmioty gorsze od założonych
        /// i bezużyteczne dla niego (bronie i druga ręka, których nie zmienia). Dusze idą na sklep; plecak nie zapełnia się, więc łup nie przepada.
        /// </summary>
        protected void SellUnused()
        {
            var run = Root.Run;
            foreach (var item in run.inventory.ToList())
            {
                if (!run.SellItem(item)) continue;
                string why = IsGearCandidate(item.definition) ? "gorszy od założonego" : "bezużyteczny";
                Rec.R.purchases.Add($"sprzedany ({why}): {item.definition.displayName}");
            }
        }

        static bool IsGearCandidate(ItemDefinition d) => d != null && !d.IsWeapon && d.kind != ItemKind.OffHand;

        /// <summary>Przymierza każdy przedmiot z plecaka; zostaje, jeśli podnosi wartość buildu o ponad 1%.</summary>
        protected void ManageGear()
        {
            var run = Root.Run;
            bool changed = false;
            foreach (var item in run.inventory.Where(i => IsGearCandidate(i.definition)).ToList())
            {
                var slots = item.definition.kind == ItemKind.Ring ? new[] { EquipSlot.Ring1, EquipSlot.Ring2 } : new[] { item.definition.DefaultSlot };
                foreach (var slot in slots)
                {
                    if (!run.inventory.Contains(item) || !run.equipment.CanEquip(item, slot, out _)) continue;
                    var old = run.equipment.Get(slot);
                    // Tarcza zamiast drugiej broni/grymuaru zmieniłaby rodzaj broni – tylko tarcza za tarczę albo do pustej ręki.
                    if (item.definition.IsShield && old != null && !old.definition.IsShield) continue;
                    float before = GearScore();
                    if (!run.EquipFromInventory(item, slot)) continue;
                    if (GearScore() > before * 1.01f)
                    {
                        changed = true;
                        Rec.R.purchases.Add("założony przedmiot: " + item.definition.displayName);
                        break;
                    }
                    if (old != null) run.EquipFromInventory(old, slot); else run.UnequipToInventory(slot);
                }
            }
            if (changed) Pc.RefreshBuild();
        }

        /// <summary>Czy przedmiot z nagrody poprawiłby build (pusty slot albo lepszy od założonego) – próba na kopii stanu.</summary>
        protected bool GearWouldHelp(ItemInstance item)
        {
            if (item == null || !IsGearCandidate(item.definition)) return false;
            var run = Root.Run;
            var slot = item.definition.DefaultSlot;
            if (item.definition.kind == ItemKind.Ring && run.equipment.Get(EquipSlot.Ring1) != null) slot = EquipSlot.Ring2;
            var old = run.equipment.Get(slot);
            // Pusty slot: zwykle zysk – chyba że przedmiot uśpi natywny perk (tarcza a Pojedynkowicz), wtedy uczciwe porównanie.
            if (old == null && LoadoutAnalysis.NativePerksLosingEffect(run, item.definition).Count == 0) return run.equipment.CanEquip(item, slot, out _);
            if (old != null && item.definition.IsShield && !old.definition.IsShield) return false;
            float before = GearScore();
            run.inventory.Add(item);
            bool ok = run.EquipFromInventory(item, slot);
            float after = ok ? GearScore() : before;
            if (ok) { if (old != null) run.EquipFromInventory(old, slot); else run.UnequipToInventory(slot); }
            run.inventory.Remove(item);
            return after > before * 1.01f;
        }
    }
}
