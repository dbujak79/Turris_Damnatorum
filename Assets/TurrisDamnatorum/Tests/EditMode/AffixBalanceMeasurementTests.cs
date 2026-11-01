using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>
    /// Krótkie porównanie balansu affiksów specjalizacyjnych (rozdz. 10.3): stałe seedy, prawdziwe obliczenia gry
    /// (BuildCalculator, DamageResolver, AffixMath, HellEconomy). Wynik: TestResults/affiksy-specjalizacyjne/pomiary.md.
    /// Asercje pilnują tylko blokerów (wielokrotne naliczenie, martwy affiks, darmowy blok), nie docelowego DPS.
    /// </summary>
    public class AffixBalanceMeasurementTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        static StatModifier M(StatType s, float v) => new StatModifier(s, v);

        [Test]
        public void Measurements_WriteReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Pomiary affiksów specjalizacyjnych (EditMode, deterministyczne)\n");
            sb.AppendLine("Wartości: brak premii / typowy affiks (T2, jakość 0) / maksymalny legalny zestaw po jakości 2 (×1,5).\n");

            // ---- Blok tarczą
            var run = fx.NewRun("class_knight");
            var shield = new ItemInstance(fx.Get<ItemDefinition>("hu_shield"));
            Assert.IsTrue(run.equipment.Equip(shield, EquipSlot.OffHand, null));
            sb.AppendLine("## Blok tarczą (Tarcza HU, rycerz)\n");
            sb.AppendLine("| Wariant | Premia | Koszt bloku (obciążenie 20 / 40) | Bloki z pełnej wytrzymałości (40) |");
            sb.AppendLine("|---|---|---|---|");
            float prevCost = float.MaxValue;
            foreach (var (name, v, q) in new[] { ("brak", 0f, 0), ("T2 typowy", 14.5f, 0), ("T3 maks. × jakość 2", 20f, 2) })
            {
                shield.affixes.Clear(); shield.huQuality = q; shield.affixVersion = HellEconomy.AffixVersion;
                if (v > 0) shield.affixes.Add(M(StatType.ShieldBlockCost, v));
                var b = BuildCalculator.Compute(run, fx.Cfg);
                float c20 = b.BlockCost(20f), c40 = b.BlockCost(40f);
                int blocks = c40 > 0 ? Mathf.FloorToInt(b.MaxStamina / c40) : int.MaxValue;
                sb.AppendLine($"| {name} | −{b.shieldBlockAffix:0.#}% | {c20:0.0} / {c40:0.0} | {blocks} (wytrz. {b.MaxStamina:0}) |");
                Assert.Greater(c40, 0f, "blok nigdy darmowy z samego affiksu");
                Assert.LessOrEqual(c40, prevCost);
                prevCost = c40;
            }

            // ---- Szybkość rzucania
            var spell = fx.bundle.all.OfType<SpellDefinition>().Where(s => s.IsSpell && s.castTime > 0.1f).OrderBy(s => s.id).First();
            float t0 = spell.castTime;
            sb.AppendLine($"\n## Szybkość rzucania ({spell.displayName}, przygotowanie {t0:0.000} s przed tempem gracza)\n");
            sb.AppendLine("| Wariant | Premia | Przygotowanie | Rzuty przygotowania w 10 s (bez recovery i odnowienia) |");
            sb.AppendLine("|---|---|---|---|");
            foreach (var (name, c) in new[] { ("brak", 0f), ("T2 typowy", 9f), ("fokus T3 + grymuar T3, jakość 2", 36f) })
            {
                float t = AffixMath.CastTime(t0, c);
                sb.AppendLine($"| {name} | +{c:0.#}% | {t:0.000} s | {10f / t:0.0} |");
            }

            // ---- Postawa ciężkiej broni
            var boss = fx.bundle.all.OfType<EnemyDefinition>().Where(e => e.isBoss).OrderByDescending(e => e.maxPoise).FirstOrDefault();
            var axe = fx.Get<ItemDefinition>("hu_greataxe").weapon;
            sb.AppendLine($"\n## Postawa ciężkiej broni (Topór HU: lekki {axe.light.poiseDamage:0}, ciężki {axe.heavy.poiseDamage:0} postawy)\n");
            if (boss != null)
            {
                sb.AppendLine($"Boss: {boss.displayName}, postawa {boss.maxPoise:0}, regeneracja {boss.poiseRegen:0}/s po {boss.poiseRegenDelay:0.#} s.\n");
                sb.AppendLine("| Wariant | Premia | Ciężkie ciosy do przełamania | Lekkie ciosy do przełamania |");
                sb.AppendLine("|---|---|---|---|");
                foreach (var (name, p) in new[] { ("brak", 0f), ("T2 typowy", 17f), ("T3 maks. × jakość 2", 37.5f) })
                {
                    float m = AffixMath.PoiseMultiplier(p);
                    sb.AppendLine($"| {name} | +{p:0.#}% | {Mathf.CeilToInt(boss.maxPoise / (axe.heavy.poiseDamage * m))} | {Mathf.CeilToInt(boss.maxPoise / (axe.light.poiseDamage * m))} |");
                }
            }

            // ---- Sługi
            sb.AppendLine("\n## Obrażenia sług (migawka przy przyzwaniu)\n");
            sb.AppendLine("| Wariant | Premia | Mnożnik DPS nowego sługi |");
            sb.AppendLine("|---|---|---|");
            foreach (var (name, p) in new[] { ("brak", 0f), ("T2 typowy", 22f), ("hełm T3 + grymuar T3, jakość 2", 90f) })
                sb.AppendLine($"| {name} | +{p:0.#}% | ×{AffixMath.MinionMultiplier(p):0.00} |");

            // ---- Dodane żywioły i przebicie
            var sword = new ItemInstance(fx.Get<ItemDefinition>("hu_sword"));
            var r2 = fx.NewRun("class_knight");
            r2.inventory.Add(sword); r2.EquipFromInventory(sword, EquipSlot.MainHand);
            var bb = BuildCalculator.Compute(r2, fx.Cfg);
            float B = bb.WeaponBase(bb.weapon.light);
            sb.AppendLine($"\n## Dodane żywioły (miecz HU, B lekkiego ciosu = {B:0.0})\n");
            sb.AppendLine("| Wariant | Suma dodanego ognia | Składnik ognia przed premią | Udział w ciosie (bez premii) |");
            sb.AppendLine("|---|---|---|---|");
            foreach (var (name, a) in new[] { ("brak", 0f), ("T2 typowy (jeden slot)", 32.5f), ("T3 × jakość 2 na 5 slotach (broń, rękawice, 2 pierścienie, amulet)", 525f) })
            {
                float add = AffixMath.AddedElement(B, a);
                sb.AppendLine($"| {name} | +{a:0.#}% | {add:0.0} | {add / (bb.WeaponDamage(bb.weapon.light) + add) * 100f:0}% |");
            }
            sb.AppendLine("\n| Przebicie ognia | Odporność 50% | Odporność 25% | Podatność +25% |");
            sb.AppendLine("|---|---|---|---|");
            foreach (var (name, p) in new[] { ("brak", 0f), ("T2 typowy", 0.07f), ("broń + grymuar + amulet T3, jakość 2", 0.45f) })
                sb.AppendLine($"| {name} ({p * 100:0.#} p.p.) | ×{AffixMath.ResistMultiplier(0.5f, p):0.00} | ×{AffixMath.ResistMultiplier(0.75f, p):0.00} | ×{AffixMath.ResistMultiplier(1.25f, p):0.00} |");

            // ---- Dostępność grup w próbce losowań
            sb.AppendLine("\n## Dostępność nowych grup (500 losowań na komórkę, rzadkość jak w sklepie; to częstość w próbce, nie gwarancja dropu)\n");
            var items = new[] { ("Topór HU", "hu_greataxe"), ("Kostur HU", "hu_staff"), ("Tarcza HU", "hu_shield"), ("Hełm ciężki", "hu_item_head_heavy_t1"), ("Grymuar przywołań", "hu_grimoire_summoning"), ("Amulet", "hu_item_amulet_bone") };
            sb.AppendLine("| Przedmiot | Piętro | " + string.Join(" | ", new[] { "przebicie (dowolne)", "postawa", "sługi", "blok", "rzucanie" }) + " |");
            sb.AppendLine("|---|---|---|---|---|---|---|");
            foreach (var (label, id) in items)
                foreach (int floor in new[] { 2, 10, 20 })
                {
                    var rng = new System.Random(1000 + floor);
                    var counts = new int[5];
                    for (int i = 0; i < 500; i++)
                    {
                        var it = HellEconomy.Roll(fx.Get<ItemDefinition>(id), LootSource.Shop, floor, rng);
                        if (it.affixIds.Any(x => x.Contains("_pen."))) counts[0]++;
                        if (it.affixIds.Any(x => x.Contains("poise_damage"))) counts[1]++;
                        if (it.affixIds.Any(x => x.Contains("minion_damage"))) counts[2]++;
                        if (it.affixIds.Any(x => x.Contains("shield_block_cost"))) counts[3]++;
                        if (it.affixIds.Any(x => x.Contains("cast_speed"))) counts[4]++;
                    }
                    sb.AppendLine($"| {label} | {floor} | " + string.Join(" | ", counts.Select(c => $"{c / 5f:0.#}%")) + " |");
                }

            string dir = Path.Combine(Application.dataPath, "..", "TestResults", "affiksy-specjalizacyjne");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "pomiary.md"), sb.ToString(), new UTF8Encoding(false));
            Debug.Log(sb.ToString());
        }
    }
}
