using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    [Serializable]
    public class EnemyAttackEntry
    {
        public AttackDefinition attack = new AttackDefinition();
        public float minRange = 0f;
        public float maxRange = 3f;
        public float weight = 1f;
        public float cooldown = 0f;
        [Tooltip("Faza bossa od której atak jest dostępny (0 = zawsze).")]
        public int minPhase = 0;
        [Tooltip("Tylko dla elit (wyższa trudność).")]
        public bool eliteOnly;
        [Tooltip("Tylko gdy trudność włącza dodatkowe zachowanie bossa.")]
        public bool bossExtraOnly;
        [Tooltip("Indeks ataku, który może nastąpić bezpośrednio po tym (-1 = brak).")]
        public int followUp = -1;
        [Range(0, 1)] public float followUpChance = 0.5f;
        [Tooltip("Odporność na przerwanie podczas ataku.")]
        public bool hyperArmor;
        [Tooltip("Po ataku przeciwnik odskakuje o tyle metrów.")]
        public float retreatAfter;
    }

    [Serializable]
    public class EnemyPhase
    {
        [Range(0, 1)] public float healthThreshold = 0.5f;
        public float speedMultiplier = 1.2f;
        public float aggressionMultiplier = 1.3f;
        public string announcement = "Wróg wpada w szał!";
    }

    /// <summary>Kolejna fala wrogów na piętrze (wchodzi, gdy poprzednia zginie).</summary>
    [Serializable]
    public class EnemyWave
    {
        public List<EnemyDefinition> enemies = new List<EnemyDefinition>();
    }

    [Serializable]
    public class FloorDefinition
    {
        public string name = "Piętro";
        public ArenaDefinition arena;
        public List<EnemyDefinition> enemies = new List<EnemyDefinition>();
        [Tooltip("Kolejne fale po pierwszej (enemies). Piętro kończy się po pokonaniu ostatniej.")]
        public List<EnemyWave> extraWaves = new List<EnemyWave>();
        [Tooltip("Umiarkowane skalowanie życia i obrażeń przeciwników.")]
        public float statScale = 1f;
        public bool isDuel = true;
        public bool isBoss;
        [Tooltip("Po piętrze gracz trafia do kapliczki odpoczynku.")]
        public bool restAfter;
        [Tooltip("Popiół (waluta trwała) za ukończenie piętra, przed mnożnikiem trudności.")]
        public int ashReward = 5;

        public int WaveCount => 1 + (extraWaves?.Count(w => w != null && w.enemies.Count > 0) ?? 0);
    }
}
