using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turris
{
    public class RewardPools
    {
        public readonly List<ItemDefinition> items = new List<ItemDefinition>();
        public readonly List<BoonDefinition> boons = new List<BoonDefinition>();
        public readonly List<SpellDefinition> spells = new List<SpellDefinition>();
    }

    public class RewardOption
    {
        public RewardKind kind;
        public ItemInstance item;
        public BoonDefinition boon;
        public SpellDefinition spell;
        public SpellInstance spellToUpgrade;

        public string Title
        {
            get
            {
                switch (kind)
                {
                    case RewardKind.Item: return item.Name;
                    case RewardKind.Boon: return boon.displayName;
                    case RewardKind.LearnSpell: return spell.displayName;
                    default: return $"Ulepszenie: {spellToUpgrade.definition.displayName} → +{spellToUpgrade.level + 1}";
                }
            }
        }

        public string Category
        {
            get
            {
                switch (kind)
                {
                    case RewardKind.Item: return "PRZEDMIOT";
                    case RewardKind.Boon: return "WZMOCNIENIE";
                    case RewardKind.LearnSpell: return $"UMIEJĘTNOŚĆ ({Names.SkillCategory(spell.category).ToUpperInvariant()}) – NA TO PODEJŚCIE";
                    default: return "ULEPSZENIE UMIEJĘTNOŚCI";
                }
            }
        }

        public void Apply(RunState run)
        {
            switch (kind)
            {
                case RewardKind.Item: run.AddItem(item, true); break;
                case RewardKind.Boon: run.AddBoon(boon); break;
                case RewardKind.LearnSpell: run.LearnSpell(spell); break;
                case RewardKind.UpgradeSpell:
                    if (!spellToUpgrade.IsMaxLevel) spellToUpgrade.level++;
                    break;
            }
        }
    }

    /// <summary>
    /// Losuje trzy nagrody: przedmiot, wzmocnienie i umiejętność (nowa na to podejście lub ulepszenie).
    /// Umiejętności wymagające wyposażenia, którego postać nie ma (np. tarczy), nie są proponowane.
    /// Waga opcji rośnie, gdy pasują do tagów AKTUALNEGO buildu (wyposażenie, znane czary) i pustych slotów.
    /// Klasa startowa nie jest brana pod uwagę. Z prawdopodobieństwem rewardExplorationChance wybór jest
    /// całkowicie losowy – to zostawia szansę na zmianę kierunku rozwoju.
    /// </summary>
    public static class RewardGenerator
    {
        public static List<RewardOption> Generate(RunState run, BuildSnapshot snap, RewardPools pools, GameConfig cfg, Random rng)
        {
            var b = cfg.balance;
            var tags = run.CurrentTags(snap);
            var result = new List<RewardOption>();

            // 1. Przedmiot
            var itemDef = PickWeighted(pools.items, rng, b.rewardExplorationChance, i =>
            {
                float w = 1f + 2f * Overlap(i.tags, tags);
                if (run.equipment.Get(i.DefaultSlot) == null && !(i.kind == ItemKind.Ring && run.equipment.Get(EquipSlot.Ring2) != null)) w += 1.5f;
                return w;
            });
            if (itemDef != null)
            {
                int diffBonus = run.difficulty != null ? run.difficulty.itemQualityBonus : 0;
                int level = run.floorIndex / 2 + diffBonus + (rng.NextDouble() < 0.3 ? 1 : 0);
                level = Math.Min(level, b.maxItemLevel);
                result.Add(new RewardOption { kind = RewardKind.Item, item = new ItemInstance(itemDef, level) });
            }

            // 2. Wzmocnienie
            var boonCandidates = pools.boons.Where(x => run.BoonStacks(x) < x.maxStacks).ToList();
            var boon = PickWeighted(boonCandidates, rng, b.rewardExplorationChance, x => 1f + 2f * Overlap(x.tags, tags));
            if (boon != null) result.Add(new RewardOption { kind = RewardKind.Boon, boon = boon });

            // 3. Umiejętność: ulepszenie znanej lub nauka nowej
            var upgradable = run.knownSpells.Where(s => !s.IsMaxLevel).ToList();
            var unknown = pools.spells.Where(s => run.FindSpell(s) == null && (s.requiredTags & tags) == s.requiredTags).ToList();
            bool learn = unknown.Count > 0 && (upgradable.Count == 0 || rng.NextDouble() < b.learnNewSpellChance);
            if (learn)
            {
                var s = PickWeighted(unknown, rng, b.rewardExplorationChance, x => 1f + Overlap(x.tags, tags));
                result.Add(new RewardOption { kind = RewardKind.LearnSpell, spell = s });
            }
            else if (upgradable.Count > 0)
            {
                // Preferuj umiejętności w slotach (faktycznie używane).
                var attuned = upgradable.Where(s => run.SlotOf(s) >= 0).ToList();
                var from = attuned.Count > 0 && rng.NextDouble() < 0.75 ? attuned : upgradable;
                result.Add(new RewardOption { kind = RewardKind.UpgradeSpell, spellToUpgrade = from[rng.Next(from.Count)] });
            }
            else
            {
                // Brak opcji czaru – drugie, inne wzmocnienie.
                var other = boonCandidates.Where(x => x != boon).ToList();
                var extra = PickWeighted(other, rng, 1f, x => 1f);
                if (extra != null) result.Add(new RewardOption { kind = RewardKind.Boon, boon = extra });
            }
            return result;
        }

        static float Overlap(BuildTag a, BuildTag b)
        {
            int v = (int)(a & b), n = 0;
            while (v != 0) { n += v & 1; v >>= 1; }
            return n;
        }

        static T PickWeighted<T>(IList<T> list, Random rng, float explorationChance, Func<T, float> weight) where T : class
        {
            if (list == null || list.Count == 0) return null;
            if (rng.NextDouble() < explorationChance) return list[rng.Next(list.Count)];
            float total = 0;
            foreach (var x in list) total += Math.Max(0.01f, weight(x));
            double roll = rng.NextDouble() * total;
            foreach (var x in list)
            {
                roll -= Math.Max(0.01f, weight(x));
                if (roll <= 0) return x;
            }
            return list[list.Count - 1];
        }
    }

    /// <summary>Opisy przedmiotów, czarów i wzmocnień do UI.</summary>
    public static class Describe
    {
        public static string Item(ItemInstance inst, StatSheet stats, BalanceConfig b)
        {
            var d = inst.definition;
            var sb = new StringBuilder();
            sb.Append(KindName(d)).Append(d.twoHanded ? " (dwuręczna)" : "").Append($", waga {d.weight:0.#}\n");
            if (d.IsWeapon)
            {
                var w = d.weapon;
                sb.Append($"Lekki: {w.light.baseDamage * inst.WeaponLevelFactor:0} {Names.Damage(w.light.damageType)}, ciężki: {w.heavy.baseDamage * inst.WeaponLevelFactor:0}\n");
                sb.Append($"Skalowanie: S {w.strengthScaling:0.#} / Z {w.dexterityScaling:0.#} / I {w.intelligenceScaling:0.#}\n");
                if (w.canBlock) sb.Append($"Blok bronią: {w.guard.physicalReduction * 100:0}% fiz. / {w.guard.magicReduction * 100:0}% mag., stabilność ×{w.guard.stabilityMultiplier:0.##}\n");
                if (w.canParry) sb.Append($"Parowanie: okno {w.parry.activeWindow:0.00}s\n");
            }
            if (d.IsShield)
            {
                var s = d.shield;
                sb.Append($"Blok: {s.guard.physicalReduction * 100:0}% fiz. / {s.guard.magicReduction * 100:0}% mag., stabilność ×{s.guard.stabilityMultiplier:0.##}, kąt {s.guard.blockAngle:0}°\n");
                sb.Append(s.canParry ? $"Parowanie: okno {s.parry.activeWindow:0.00}s\n" : "Brak parowania\n");
            }
            foreach (var m in inst.Modifiers) sb.Append(m).Append('\n');
            foreach (var e in d.effects) sb.Append(e).Append('\n');
            if (d.requirements.Count > 0)
            {
                sb.Append("Wymaga: ");
                sb.Append(string.Join(", ", d.requirements.Select(r => $"{Names.Attribute(r.attribute)} {r.value}")));
                if (stats != null && !StatCalculator.RequirementsMet(d.requirements, stats))
                    sb.Append($"  [niespełnione – skuteczność {b.unmetRequirementEffectiveness * 100:0}%]");
                sb.Append('\n');
            }
            if (!string.IsNullOrEmpty(d.description)) sb.Append(d.description);
            return sb.ToString().TrimEnd();
        }

        public static string Spell(SpellDefinition s, int level, StatSheet stats, BalanceConfig b)
        {
            var sb = new StringBuilder();
            float cost = s.CostFactor(level);
            string charges = s.MaxCharges(level) > 1 ? $", ładunki {s.MaxCharges(level)}" : "";
            if (s.IsSpell) sb.Append($"Czar · mana {s.manaCost * cost:0}, rzucanie {s.castTime:0.00}s, odnowienie {s.CooldownAt(level):0.#}s{charges}\n");
            else sb.Append($"Technika · wytrzymałość {s.staminaCost * cost:0}, odnowienie {s.CooldownAt(level):0.#}s{charges}\n");
            float mult = s.weaponMultiplier * (1f + s.powerPerLevel * level) * 100f;
            switch (s.kind)
            {
                case SpellKind.Projectile: sb.Append($"Pocisk: {s.attack.baseDamage:0} obrażeń magicznych"); if (s.attack.projectileCount > 1) sb.Append($" ×{s.attack.projectileCount}"); sb.Append('\n'); break;
                case SpellKind.Nova: sb.Append($"Fala wokół postaci (promień {s.attack.radius:0.#} m): {s.attack.baseDamage:0} obrażeń, postawa {s.attack.poiseDamage:0}\n"); break;
                case SpellKind.Heal: sb.Append($"Leczy {s.amount:0} przez {s.duration:0.#}s (skaluje z Inteligencją)\n"); break;
                case SpellKind.WeaponBuff: sb.Append($"Broń zadaje +{s.amount:0} obrażeń magicznych przez {s.duration:0}s\n"); break;
                case SpellKind.Barrier: sb.Append($"Osłona pochłania {s.amount:0} obrażeń przez {s.duration:0}s (skaluje z Inteligencją)\n"); break;
                case SpellKind.Cleave: sb.Append($"Łuk {s.arcAngle:0}° przed sobą, zasięg {s.attack.reach:0.#} m: {mult:0}% lekkiego ataku broni\n"); break;
                case SpellKind.ShieldBash: sb.Append($"Uderzenie tarczą: {mult:0}% lekkiego ataku, postawa {s.attack.poiseDamage:0}\n"); break;
                case SpellKind.Charge: sb.Append($"Szarża na {s.attack.reach:0.#} m: {mult:0}% lekkiego ataku każdemu na drodze\n"); break;
                case SpellKind.Whirlwind: sb.Append($"Młynek przez {s.duration:0.#}s (promień {s.attack.radius:0.#} m): {mult:0}% lekkiego ataku co {s.tickInterval:0.##}s\n"); break;
                case SpellKind.Quake: sb.Append($"Uderzenie w ziemię (promień {s.attack.radius:0.#} m): {mult:0}% lekkiego ataku, postawa {s.attack.poiseDamage:0}\n"); break;
            }
            if (s.requiredTags != BuildTag.None) sb.Append(char.ToUpper(Names.Requirement(s.requiredTags)[0]) + Names.Requirement(s.requiredTags).Substring(1)).Append('\n');
            if (s.requirements.Count > 0)
            {
                sb.Append("Wymaga: " + string.Join(", ", s.requirements.Select(r => $"{Names.Attribute(r.attribute)} {r.value}")));
                if (stats != null && !StatCalculator.RequirementsMet(s.requirements, stats))
                    sb.Append($"  [niespełnione – moc {b.unmetRequirementEffectiveness * 100:0}%]");
                sb.Append('\n');
            }
            if (!string.IsNullOrEmpty(s.description)) sb.Append(s.description);
            return sb.ToString().TrimEnd();
        }

        public static string Boon(BoonDefinition d, int currentStacks)
        {
            var sb = new StringBuilder();
            foreach (var m in d.modifiers) sb.Append(m).Append('\n');
            foreach (var e in d.effects) sb.Append(e).Append('\n');
            if (d.extraHealthFlasks > 0) sb.Append($"+{d.extraHealthFlasks} ładunek flaszki życia\n");
            if (d.extraManaFlasks > 0) sb.Append($"+{d.extraManaFlasks} ładunek flaszki many\n");
            sb.Append($"Stosy: {currentStacks}/{d.maxStacks}\n");
            if (!string.IsNullOrEmpty(d.description)) sb.Append(d.description);
            return sb.ToString().TrimEnd();
        }

        public static string Reward(RewardOption o, RunState run, StatSheet stats, BalanceConfig b)
        {
            switch (o.kind)
            {
                case RewardKind.Item: return Item(o.item, stats, b);
                case RewardKind.Boon: return Boon(o.boon, run.BoonStacks(o.boon));
                case RewardKind.LearnSpell:
                    return Spell(o.spell, 0, stats, b) + "\n<color=#e8c070>Trafia do kolekcji na to podejście (pierwszy wolny slot). Na stałe – odblokowanie za popiół.</color>";
                default:
                {
                    var s = o.spellToUpgrade;
                    var d = s.definition;
                    string extra = d.extraChargeAtLevel > 0 && s.level + 1 == d.extraChargeAtLevel ? ", <b>+1 ładunek</b>" : "";
                    return $"Moc +{d.powerPerLevel * 100:0}%, koszt −{d.costReductionPerLevel * 100:0}%, odnowienie −{d.cooldownReductionPerLevel * 100:0}%{extra}\n" + Spell(d, s.level + 1, stats, b);
                }
            }
        }

        public static string KindName(ItemDefinition d)
        {
            switch (d.kind)
            {
                case ItemKind.Weapon: return "Broń";
                case ItemKind.Shield: return "Tarcza";
                case ItemKind.Head: return "Hełm";
                case ItemKind.Body: return "Zbroja";
                case ItemKind.Hands: return "Rękawice";
                case ItemKind.Belt: return "Pas";
                case ItemKind.Feet: return "Buty";
                case ItemKind.Ring: return "Pierścień";
                default: return "Amulet";
            }
        }
    }
}
