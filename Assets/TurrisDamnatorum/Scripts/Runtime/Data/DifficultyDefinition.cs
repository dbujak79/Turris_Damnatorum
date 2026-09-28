using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    [CreateAssetMenu(menuName = "Turris/Difficulty", fileName = "Difficulty")]
    public class DifficultyDefinition : ContentDefinition
    {
        public int tier;
        public float enemyHealthMultiplier = 1f;
        public float enemyDamageMultiplier = 1f;
        [Tooltip("Mnożnik tempa ataków przeciwników (wyżej = krótsze przerwy).")]
        public float enemyAggressionMultiplier = 1f;
        public bool elites;
        public int flaskChargeDelta;
        [Tooltip("Życie i flaszki odnawiają się tylko w kapliczkach, nie po każdym piętrze.")]
        public bool restoreOnlyAtRest;
        public bool bossExtraBehavior;
        public float ashMultiplier = 1f;
        public float soulMultiplier = 1f;
        [Tooltip("Premia do jakości przedmiotów w nagrodach.")]
        public int itemQualityBonus;

        public List<string> DescribeModifiers()
        {
            var list = new List<string>();
            if (enemyHealthMultiplier != 1f) list.Add($"Życie wrogów ×{enemyHealthMultiplier:0.##}");
            if (enemyDamageMultiplier != 1f) list.Add($"Obrażenia wrogów ×{enemyDamageMultiplier:0.##}");
            if (enemyAggressionMultiplier != 1f) list.Add($"Wrogowie atakują częściej (×{enemyAggressionMultiplier:0.##})");
            if (elites) list.Add("Elity: wzmocnieni przeciwnicy z dodatkowymi atakami");
            if (flaskChargeDelta != 0) list.Add($"Ładunki flaszek {flaskChargeDelta:+#;-#}");
            if (restoreOnlyAtRest) list.Add("Życie i flaszki odnawiają się tylko w kapliczkach");
            if (bossExtraBehavior) list.Add("Boss: wcześniejsza druga faza i dodatkowe ataki");
            return list;
        }

        public List<string> DescribeRewards()
        {
            var list = new List<string>();
            list.Add($"Popiół ×{ashMultiplier:0.##}");
            if (soulMultiplier != 1f) list.Add($"Dusze ×{soulMultiplier:0.##}");
            if (itemQualityBonus > 0) list.Add($"Jakość przedmiotów +{itemQualityBonus}");
            return list;
        }
    }
}
