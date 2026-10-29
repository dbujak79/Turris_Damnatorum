using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Wieża jako wierny port 30 fal Hell Unleashed: piętro N = fala N. Zwykłe piętra to portale z czasem, budżetem,
    /// odstępem i wielkością grup, składem (wagi demonów) i mnożnikami życia/obrażeń/prędkości/dusz fali; co trzecie
    /// piętro to boss fali (pojedynek), a po nim kapliczka. Ostatnie, 30. piętro (Arcydemon) kończy podejście.
    /// Dane kopiuje <c>tools/art/import_hu_waves.py</c> do <c>Resources/HellCatalog/waves.json</c>.
    /// </summary>
    public static class HellTower
    {
        [Serializable] class Catalog { public Row[] waves; }

        [Serializable]
        public class Row
        {
            public int Index;
            public string sha256, BossEnemyId;
            public bool IsBossWave;
            public float DurationSeconds = 75f, SpawnBudget, GroupIntervalSeconds = 3f;
            public int MinGroupSize = 1, MaxGroupSize = 2;
            public string[] Composition = Array.Empty<string>();
            public float HealthMultiplier = 1f, DamageMultiplier = 1f, SpeedMultiplier = 1f, SoulRewardMultiplier = 1f;
        }

        static Row[] rows;
        public static IReadOnlyList<Row> Rows => rows ?? (rows = JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("HellCatalog/waves").text).waves);

        /// <summary>
        /// Łagodzenie życia zwykłych fal z HU (<c>NormalWaveHealthScale</c>): ×1 do fali 10, potem liniowo do ×0,75 na fali 29.
        /// Późne piętra sprawdzają mieszankę demonów bardziej niż ich wytrzymałość. Bossowie mają własne liczby.
        /// </summary>
        public static float NormalWaveHealthScale(int wave) => 1f - 0.25f * Mathf.Clamp01((wave - 10) / 19f);

        /// <summary>
        /// Turris ma mniej demonów niż Hell Unleashed (założenie projektu): budżet portali każdej fali ×0,8, czyli ok. 20% mniej wrogów.
        /// </summary>
        public const float MonsterCountScale = 0.8f;

        /// <summary>
        /// Tempo wychodzenia demonów z portali względem HU: 0,5 = o połowę wolniej (obserwacja gracza – potwory wychodziły za szybko).
        /// Odstęp grup i czas otwarcia portali rosną razem, więc liczba demonów zostaje (portal kończy się po czasie albo budżecie).
        /// </summary>
        public const float SpawnTempoScale = 0.5f;

        /// <summary>
        /// Łagodzenie pięter IV–VI (raport botów, seria 9): bohater Turris rośnie wolniej niż w HU (brak sklepu z perkami i poziomami
        /// broni co falę), więc fale 4–6 dostają życie ×0,85 i obrażenia ×0,9 – przejście pierwszych sześciu pięter jak w dawnej wieży.
        /// </summary>
        public const float EarlyHealthEase = 0.85f, EarlyDamageEase = 0.9f;

        /// <summary>
        /// HU balans 2.0 (<c>EliteSettings</c>): złota elita co 28–35 wypuszczonych demonów (było 20–30) i dopiero od fali 2 –
        /// pierwsze piętro uczy samej hordy. Licznik biegnie przez cały run.
        /// </summary>
        public static readonly Vector2Int EliteEvery = new Vector2Int(28, 35);
        public const int EliteFirstWave = 2;
        public static bool IsEarlyEased(int wave) => wave >= 4 && wave <= 6;

        /// <summary>
        /// Korekty Turris: o tylu wrogów mniej na fali, na której boty trafiały na ścianę (ginęło wyraźnie ponad połowę).
        /// Zdejmowani są najpierw najsłabsi – budżet portali maleje o koszt najsłabszego demona składu (najniższy koszt
        /// wystawienia, przy remisie najmniej życia), więc skład i silni wrogowie zostają jak w HU.
        /// </summary>
        public static readonly Dictionary<int, int> FewerEnemies = new Dictionary<int, int>
        {
            { 7, 8 }, // seria 11–12: piętro VII przechodzi 4/17 i 1/20 gracza (ok. 37 wrogów, piętro V ok. 30)
        };

        /// <summary>Najsłabszy demon składu piętra: najniższy koszt wystawienia, przy remisie najmniej życia.</summary>
        public static EnemyDefinition Weakest(PortalSpawning p) =>
            p.composition.Where(c => c.enemy != null).Select(c => c.enemy).OrderBy(e => e.spawnCost).ThenBy(e => e.maxHealth).FirstOrDefault();

        /// <summary>Nazwy aktów (po trzy piętra, boss na końcu aktu).</summary>
        static readonly string[] Acts =
        {
            "Wrota Otchłani", "Psiarnia Piekieł", "Zatopione Krużganki", "Rzeźnia", "Kamienne Trzewia",
            "Wieża Burz", "Kostnica", "Chór Cieni", "Kuźnia Piekieł", "Tron Arcydemona",
        };

        static readonly string[] Roman =
        {
            "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII", "XIII", "XIV", "XV",
            "XVI", "XVII", "XVIII", "XIX", "XX", "XXI", "XXII", "XXIII", "XXIV", "XXV", "XXVI", "XXVII", "XXVIII", "XXIX", "XXX",
        };

        /// <summary>
        /// Buduje 30 pięter. <paramref name="enemy"/> zwraca definicję Turris dla id HU (np. <c>enemy.imp</c>);
        /// areny: zwykłe piętra na zmianę dziedziniec i krypta, bossowie na dziedzińcu, finał na szczycie.
        /// </summary>
        public static List<FloorDefinition> Build(Func<string, EnemyDefinition> enemy, ArenaDefinition courtyard, ArenaDefinition crypt, ArenaDefinition summit)
        {
            var floors = new List<FloorDefinition>();
            foreach (var w in Rows)
            {
                int i = w.Index - 1;
                bool last = w.Index == Rows.Count;
                var f = new FloorDefinition
                {
                    statScale = w.HealthMultiplier * (w.IsBossWave ? 1f : NormalWaveHealthScale(w.Index)) * (IsEarlyEased(w.Index) ? EarlyHealthEase : 1f),
                    damageScale = w.DamageMultiplier * (IsEarlyEased(w.Index) ? EarlyDamageEase : 1f),
                    speedScale = w.SpeedMultiplier,
                    soulScale = w.SoulRewardMultiplier,
                    ashReward = last ? 50 : 3 + i, // rośnie z piętrem (farmienie niskich pięter się nie opłaca)
                };
                if (w.IsBossWave)
                {
                    var boss = enemy(w.BossEnemyId);
                    f.name = $"{Roman[i]}. {(boss != null ? boss.displayName : Acts[i / 3])}";
                    f.arena = last ? summit : courtyard;
                    if (boss != null) f.enemies.Add(boss);
                    f.isDuel = true;
                    f.isBoss = last;
                    f.restAfter = !last;
                    f.portals.enabled = false;
                }
                else
                {
                    f.name = $"{Roman[i]}. {Acts[i / 3]}";
                    f.arena = i % 2 == 0 ? courtyard : crypt;
                    f.isDuel = false;
                    f.portals = new PortalSpawning
                    {
                        enabled = true, duration = w.DurationSeconds / SpawnTempoScale, budget = w.SpawnBudget * MonsterCountScale,
                        groupInterval = w.GroupIntervalSeconds / SpawnTempoScale,
                        minGroup = w.MinGroupSize, maxGroup = w.MaxGroupSize,
                    };
                    foreach (var entry in w.Composition)
                    {
                        var parts = entry.Split('|');
                        var def = enemy(parts[0]);
                        if (def == null || parts.Length < 2) continue;
                        f.portals.composition.Add(new PortalEntry { enemy = def, weight = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture) });
                    }
                    var weakest = Weakest(f.portals);
                    if (weakest != null && FewerEnemies.TryGetValue(w.Index, out int fewer))
                        f.portals.budget = Mathf.Max(weakest.spawnCost, f.portals.budget - fewer * weakest.spawnCost);
                }
                floors.Add(f);
            }
            if (floors.Count > 0) floors[0].name = "I. Wrota Otchłani";
            if (floors.Count > 1)
            {
                // Korekta Turris (raport botów, seria 1): piętro II zabijało w 18/20 śmierci – waga kultysty 4,5 → 3,7
                // (mniejszy udział kultystów; budżet już ×0,8 jak wszędzie).
                var ii = floors[1];
                ii.name = "II. Kult Bramy";
                foreach (var c in ii.portals.composition) if (c.enemy != null && c.enemy.id == "enemy_hu_cultist") c.weight = 3.7f;
            }
            return floors;
        }
    }
}
