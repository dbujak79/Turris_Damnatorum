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
            float dps = b.WeaponDamage(b.weapon.light) * b.attackSpeed;
            return dps + 0.25f * s[StatType.MaxHealth] + 0.4f * (s[StatType.PhysicalDefense] + s[StatType.MagicDefense])
                   + 0.3f * s[StatType.SpellPower] + 0.2f * s[StatType.MaxStamina] + EffectScore(b);
        }

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
            if (old == null) return run.equipment.CanEquip(item, slot, out _);
            if (item.definition.IsShield && !old.definition.IsShield) return false;
            float before = GearScore();
            run.inventory.Add(item);
            bool ok = run.EquipFromInventory(item, slot);
            float after = ok ? GearScore() : before;
            if (ok) run.EquipFromInventory(old, slot);
            run.inventory.Remove(item);
            return after > before * 1.01f;
        }
    }
}
