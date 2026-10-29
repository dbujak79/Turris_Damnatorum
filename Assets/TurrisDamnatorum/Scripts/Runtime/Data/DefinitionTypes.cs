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

        [Header("Atak specjalny (Hell Unleashed)")]
        public EnemySpecial special;
        [Tooltip("Liczba (stref, kul, sług), zasięg szarży (m) – zależnie od rodzaju.")]
        public float specialCount;
        [Tooltip("Promień stref / wybuchu (m).")]
        public float specialRadius;
        [Tooltip("Czas (s): uczepienia, płonącej ziemi.")]
        public float specialDuration;
        [Tooltip("Część obrażeń na tyknięcie (uczepienie, płonąca ziemia).")]
        public float specialShare;
        [Tooltip("Przywoływany sługa i ile może ich żyć naraz.")]
        public EnemyDefinition summon;
        public int summonMax;
        public HellEnemyAbility huAbility;
        [Tooltip("Jednorazowe wejście w fazę (1..N), poza rotacją ataków.")]
        public int huOnPhase;
        public bool huEnragedOnly;
    }

    /// <summary>
    /// Ataki specjalne demonów z Hell Unleashed. Wartości zapisane w assetach – nowe tylko na końcu.
    /// Latch – skok i wgryzienie (spowalnia, krwawienie co sekundę, strząsa unik); Charge – szarża po zapowiedzianej linii
    /// z płonącym śladem i wybuchem na końcu; FireZones – zapowiedziane kręgi ognia wokół bohatera; Orbs – wolne kule
    /// wybuchające płonącą ziemią; Summon – przywołanie sług; Leap – skok na miejsce bohatera z uderzeniem w krąg.
    /// </summary>
    public enum EnemySpecial { None, Latch, Charge, FireZones, Orbs, Summon, Leap }

    [Serializable]
    public class EnemyPhase
    {
        [Range(0, 1)] public float healthThreshold = 0.5f;
        public float speedMultiplier = 1.2f;
        public float aggressionMultiplier = 1.3f;
        public string announcement = "Wróg wpada w szał!";
        [Tooltip("Atak (indeks) wykonywany od razu po wejściu w fazę, np. przywołanie sług (-1 = brak).")]
        public int openingAttack = -1;
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
        [Tooltip("Skalowanie życia przeciwników (i obrażeń, gdy damageScale < 0).")]
        public float statScale = 1f;
        [Tooltip("Skalowanie obrażeń przeciwników (fala HU: DamageMultiplier). Ujemne – jak statScale.")]
        public float damageScale = -1f;
        [Tooltip("Skalowanie prędkości ruchu przeciwników (fala HU: SpeedMultiplier).")]
        public float speedScale = 1f;
        [Tooltip("Mnożnik dusz za demony (fala HU: SoulRewardMultiplier).")]
        public float soulScale = 1f;

        public float DamageScale => damageScale >= 0f ? damageScale : statScale;
        public bool isDuel = true;
        public bool isBoss;
        [Tooltip("Po piętrze gracz trafia do kapliczki odpoczynku.")]
        public bool restAfter;
        [Tooltip("Popiół (waluta trwała) za ukończenie piętra, przed mnożnikiem trudności.")]
        public int ashReward = 5;

        [Tooltip("Hell Unleashed: wrogowie wychodzą grupami z portali przez cały czas piętra (zamiast fal po kolei).")]
        public PortalSpawning portals = new PortalSpawning();

        public int WaveCount => 1 + (extraWaves?.Count(w => w != null && w.enemies.Count > 0) ?? 0);
    }

    /// <summary>Jeden rodzaj demona w składzie piętra z portalami (waga losowania).</summary>
    [Serializable]
    public class PortalEntry
    {
        public EnemyDefinition enemy;
        public float weight = 1f;
    }

    /// <summary>
    /// Fala z Hell Unleashed: przez <see cref="duration"/> sekund co <see cref="groupInterval"/> otwiera się portal
    /// w losowym miejscu areny (nigdy tuż przy bohaterze) i wychodzi z niego grupa demonów. Wypuszczanie kończy się,
    /// gdy minie czas albo skończy się budżet (suma kosztów demonów); piętro kończy się po zabiciu wszystkich.
    /// </summary>
    [Serializable]
    public class PortalSpawning
    {
        public bool enabled;
        public float duration = 75f;
        public float budget = 30f;
        public float groupInterval = 3f;
        public int minGroup = 1, maxGroup = 2;
        [Tooltip("Najwięcej żywych demonów naraz – ponad to portal czeka.")]
        public int maxAlive = 30;
        [Tooltip("Najmniejsza odległość portalu od bohatera (m).")]
        public float minDistance = 7f;
        [Tooltip("Złota elita co tyle wypuszczonych demonów (losowo z przedziału; licznik biegnie przez cały run).")]
        public Vector2Int eliteEvery = new Vector2Int(20, 30);
        public List<PortalEntry> composition = new List<PortalEntry>();
    }
}
