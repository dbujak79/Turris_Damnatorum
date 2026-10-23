using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Wprowadzenie sprawdzonych archetypów na późniejszych piętrach, z zachowaniem liczby fal i przeciwników.</summary>
    public static class HellEncounters
    {
        public static void ExtendTower(GameConfig cfg, Dictionary<string, ContentDefinition> content)
        {
            if (cfg.tower == null || cfg.tower.floors.Count < 6) return;
            void Replace(List<EnemyDefinition> list, int index, string expected, string next)
            {
                if (index < list.Count && list[index] != null && list[index].id == expected && content.TryGetValue(next, out var value)) list[index] = (EnemyDefinition)value;
            }
            var iv = cfg.tower.floors[3];
            Replace(iv.enemies, 0, "enemy_ghoul", "enemy_hu_hound");
            Replace(iv.enemies, 1, "enemy_ghoul", "enemy_hu_brute");
            if (iv.extraWaves.Count > 1)
            {
                Replace(iv.extraWaves[0].enemies, 2, "enemy_ghoul", "enemy_hu_frostcaster");
                Replace(iv.extraWaves[1].enemies, 1, "enemy_heretic", "enemy_hu_necromancer");
            }
            var v = cfg.tower.floors[4];
            if (v.extraWaves.Count > 1)
            {
                Replace(v.extraWaves[0].enemies, 0, "enemy_ghoul", "enemy_hu_legionary");
                Replace(v.extraWaves[0].enemies, 1, "enemy_ghoul", "enemy_hu_harpy");
                Replace(v.extraWaves[1].enemies, 0, "enemy_heretic", "enemy_hu_wraith");
                Replace(v.extraWaves[1].enemies, 3, "enemy_ghoul", "enemy_hu_weaver");
            }
        }
    }
}
