using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>Kopia danych HU z sumą źródła. JsonUtility czyta dane, nigdy pliki projektu Godota.</summary>
    public static class HellEnemyCatalog
    {
        public const int Version = 4;
        [Serializable] class Catalog { public HellEnemySource[] enemies; }
        static HellEnemySource[] rows;
        public static HellEnemySource[] Rows => rows ?? (rows = JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("HellCatalog/enemies").text).enemies);
        public static string Target(string source) => Rows.FirstOrDefault(r => r.Id == source)?.targetId;

        public static void Add(GameConfig cfg, IList<ScriptableObject> all)
        {
            var enemies = all.OfType<EnemyDefinition>().ToDictionary(e => e.id);
            foreach (var r in Rows)
            {
                if (enemies.ContainsKey(r.targetId)) { cfg.enemyCatalog.Add(enemies[r.targetId]); continue; } // zachowaj balans Turris
                var e = ScriptableObject.CreateInstance<EnemyDefinition>();
                e.id = e.name = r.targetId; e.displayName = r.displayName;
                e.description = "Demon z Hell Unleashed: " + r.Id;
                e.huSourceId = r.Id; e.huSourceHash = r.sha256; e.huArchetype = r.Archetype;
                e.huMigrationVersion = Version;
                e.huModel = r.model; e.scale = r.height / 1.8f;
                e.maxHealth = r.MaxHealth * 2.5f;
                e.maxPoise = r.Archetype == 3 ? 150f : r.Archetype == 2 ? 80f : 35f;
                e.moveSpeed = r.MoveSpeed / 24f;
                e.preferredRange = r.Archetype == 1 || r.Archetype == 6 || r.Archetype == 7 || r.Archetype == 10 || r.Archetype == 12 ? r.PreferredDistance / 24f : Mathf.Max(1f, r.AttackRange / 24f);
                e.retreatRange = e.preferredRange > 4 ? e.preferredRange * 0.6f : 0f;
                e.attackInterval = new Vector2(r.AttackCooldownSeconds, r.AttackCooldownSeconds);
                e.strafeChance = r.Archetype == 1 ? 0.35f : 0f;
                e.physicalResist = r.ResistPhysical;
                e.fireMultiplier = 1 - r.ResistFire; e.frostMultiplier = 1 - r.ResistWater;
                e.lightningMultiplier = 1 - r.ResistAir; e.earthMultiplier = 1 - r.ResistEarth;
                e.isBoss = r.Archetype == 3; e.isMinion = r.Id == "enemy.minion";
                e.soulReward = Mathf.RoundToInt(r.SoulReward); e.spawnCost = r.SpawnCost; e.packSize = r.PackSize;
                e.eliteHealthMultiplier = 7f; e.eliteDamageMultiplier = 1.6f;
                e.huCanBeElite = r.CanBeElite; e.huWakeRadius = r.WakeRadius / 24f; e.huHatchSeconds = r.HatchSeconds;
                e.huSplitCount = r.SplitCount; e.huSummonCount = r.SummonCount;
                e.archetype = new[] { "Szybki wręcz", "Strzelec", "Ciężki", "Boss", "Szarżujący", "Wybuchający", "Artyleria", "Przywoływacz", "Skrytobójca", "Tarczownik", "Wsparcie", "Posąg", "Zjawa", "Lęg" }[r.Archetype];
                e.huEnrageHealth = e.isBoss ? r.BossEnrageHealthFraction : 0; e.huEnrageTempo = e.isBoss ? r.BossEnrageTempoMultiplier : 1;
                e.weaponModel = WeaponModel.Claws; e.rigStyle = RigStyle.Ghoul;
                e.guard = new GuardData { blockAngle = r.GuardArcDegrees, physicalReduction = 1f, magicReduction = 0.7f, stabilityMultiplier = 1f };
                var basic = BaseAttack(r);
                var entry = new EnemyAttackEntry { attack = basic, maxRange = e.preferredRange + 2f };
                switch (r.Archetype)
                {
                    case 1: basic.delivery = AttackDelivery.Projectile; break;
                    case 2: basic.delivery = AttackDelivery.AreaAroundSelf; basic.parryable = false; basic.dodgeable = false; break;
                    case 4: entry.special = EnemySpecial.Charge; entry.specialCount = r.ChargeDistance / 24f; entry.specialRadius = r.AreaRadius / 24f; basic.active = r.ChargeDistance / r.ChargeSpeed; entry.maxRange = entry.specialCount; break;
                    case 5: entry.huAbility = HellEnemyAbility.Parse("Explode"); basic.delivery = AttackDelivery.AreaAroundSelf; entry.maxRange = basic.radius * 0.8f; break;
                    case 6: entry.huAbility = HellEnemyAbility.Parse("Zones|count=1,radius=" + r.AreaRadius.ToString(CultureInfo.InvariantCulture)); entry.maxRange = 25f; break;
                    case 7: entry.special = EnemySpecial.Summon; entry.specialCount = r.SummonCount; entry.summonMax = r.SummonMaxAlive; entry.maxRange = 25; basic.windup *= 2; entry.cooldown = r.AttackCooldownSeconds * 2.5f; break;
                    case 8: entry.huAbility = HellEnemyAbility.Parse("BlinkStrike"); entry.maxRange = 15; entry.cooldown = 4; break;
                    case 9: e.guardChance = 1f; break;
                    case 10: basic.delivery = AttackDelivery.Projectile; entry.maxRange = e.preferredRange * 1.2f; break;
                    case 12: entry.maxRange = basic.reach + 0.5f; break;
                    case 13: entry.weight = 0; e.moveSpeed = 0; break;
                }
                e.attacks.Add(entry);
                if (r.Archetype == 7 || r.Archetype == 12)
                {
                    var bolt = BaseAttack(r); bolt.delivery = AttackDelivery.Projectile;
                    if (r.Archetype == 12) bolt.windup *= 1.5f;
                    e.attacks.Add(new EnemyAttackEntry { attack = bolt, minRange = r.Archetype == 12 ? entry.maxRange : 0, maxRange = e.preferredRange * 1.3f });
                }
                if (r.Archetype == 3) e.attacks.Clear();
                foreach (var raw in r.BossAbilities.Concat(r.Specials)) AddAbility(e, HellEnemyAbility.Parse(raw));
                foreach (float threshold in r.BossPhaseThresholds)
                    e.phases.Add(new EnemyPhase { healthThreshold = threshold, speedMultiplier = 1f, aggressionMultiplier = 1f, announcement = e.displayName + ": kolejna faza" });
                all.Add(e); enemies.Add(e.id, e);
                cfg.enemyCatalog.Add(e);
            }
            foreach (var r in Rows)
            {
                var e = enemies[r.targetId];
                if (e.huSourceId != r.Id) continue;
                if (!string.IsNullOrEmpty(r.SummonEnemyId)) enemies.TryGetValue(Target(r.SummonEnemyId) ?? "", out e.huSummon);
                foreach (var a in e.attacks)
                {
                    var id = a.huAbility?.argument;
                    if (a.special == EnemySpecial.Summon) a.summon = e.huSummon;
                    if (id != null && id.StartsWith("enemy.")) enemies.TryGetValue(Target(id) ?? "", out a.summon);
                }
            }
            foreach (var e in enemies.Values.ToArray())
                if (e.huSplitCount > 0)
                {
                    var copy = UnityEngine.Object.Instantiate(e);
                    copy.id = copy.name = e.id + "_copy"; copy.displayName = e.displayName + " (odłamek)";
                    copy.maxHealth *= 0.4f; copy.scale *= 0.7f; copy.huSplitCount = 0; copy.soulReward = 0; copy.huCanBeElite = false;
                    e.huSummon = copy; all.Add(copy);
                }
        }

        static AttackDefinition BaseAttack(HellEnemySource r)
        {
            var a = new AttackDefinition { name = r.displayName, baseDamage = 2.5f * (r.DamagePhysical + r.DamageFire + r.DamageWater + r.DamageAir + r.DamageEarth),
                windup = r.TelegraphSeconds, active = 0.12f, recovery = r.RecoverySeconds, reach = r.AttackRange / 24f,
                radius = r.AreaRadius / 24f, damageType = DamageType.Physical, guardLoad = 24, poiseDamage = 20,
                projectileSpeed = r.ProjectileSpeed / 24f, projectileCount = r.ProjectilesPerAttack, spreadAngle = r.ProjectileSpreadDegrees,
                blockable = true, parryable = true, dodgeable = true, animation = AttackAnim.Auto };
            SetDamage(a, r.DamagePhysical, r.DamageFire, r.DamageWater, r.DamageAir, r.DamageEarth);
            a.statuses = Effects(r.OnHitEffects);
            return a;
        }

        public static void SetDamage(AttackDefinition a, float physical, float fire, float water, float air, float earth)
        {
            float total = physical + fire + water + air + earth;
            a.baseDamage = total * 2.5f;
            float max = Mathf.Max(fire, water, air, earth);
            a.element = max <= 0 ? Element.None : max == fire ? Element.Fire : max == water ? Element.Frost : max == air ? Element.Lightning : Element.Earth;
            a.elementShare = total > 0 ? (total - physical) / total : 0;
            a.damageType = DamageType.Physical;
        }

        public static List<StatusApplication> Effects(IEnumerable<string> source)
        {
            var result = new List<StatusApplication>();
            foreach (var effect in source)
            {
                var p = effect.Split('|');
                if (StatusFor(p[0], out var kind)) result.Add(new StatusApplication { kind = kind, stacks = p.Length > 1 ? int.Parse(p[1]) : 1, chance = 1f });
            }
            return result;
        }

        public static bool StatusFor(string name, out StatusKind kind)
        {
            switch (name) { case "Ignite": kind = StatusKind.Burn; return true; case "Soaked": case "Slow": kind = StatusKind.Chill; return true; case "Bleed": kind = StatusKind.Bleed; return true; case "Stagger": kind = StatusKind.Stagger; return true; case "Sunder": kind = StatusKind.Sunder; return true; }
            kind = default; return false;
        }

        static void AddAbility(EnemyDefinition e, HellEnemyAbility ability)
        {
            int phase = (int)ability.Get("phase");
            int onphase = (int)ability.Get("onphase");
            var a = new AttackDefinition { name = e.displayName + ": " + ability.name, windup = ability.Get("telegraph", 0.65f), active = 0.12f,
                recovery = ability.Get("rest", 0.6f), reach = ability.Get("range", 70) / 24f, radius = ability.Get("radius", 45) / 24f,
                projectileSpeed = ability.Get("speed", 190) / 24f, projectileCount = (int)ability.Get("count", 1), spreadAngle = ability.Get("spread", 60),
                blockable = true, parryable = false, dodgeable = true, guardLoad = 35, poiseDamage = 25, animation = AttackAnim.Special };
            SetDamage(a, ability.Get("damage.physical"), ability.Get("damage.fire"), ability.Get("damage.water"), ability.Get("damage.air"), ability.Get("damage.earth"));
            SetThreat(a, ability.name);
            foreach (var p in ability.parameters)
                if (p.key.StartsWith("effect.") && StatusFor(p.key.Substring(7), out var kind))
                    if (!a.statuses.Any(s => s.kind == kind)) a.statuses.Add(new StatusApplication { kind = kind, stacks = (int)p.value, chance = 1 });
            var entry = new EnemyAttackEntry { attack = a, huAbility = ability, minPhase = phase, huOnPhase = onphase,
                cooldown = ability.Get("every", ability.Get("cooldown", 3f)), minRange = ability.Get("near") / 24f,
                maxRange = ability.Get("reach", 600) / 24f, weight = onphase > 0 ? 0 : 1f };
            if (ability.name == "InfernalCharge") { entry.special = EnemySpecial.Charge; entry.specialCount = a.reach; entry.specialRadius = a.radius; entry.specialDuration = ability.Get("duration"); entry.specialShare = ability.Get("share"); a.active = a.reach / Mathf.Max(1f, a.projectileSpeed); }
            if (ability.name == "Latch") { entry.special = EnemySpecial.Latch; entry.specialDuration = ability.Get("duration", 6); entry.specialShare = ability.Get("share", 0.4f); }
            if (ability.name == "Summon") { entry.special = EnemySpecial.Summon; entry.specialCount = ability.Get("count", 2); entry.summonMax = (int)ability.Get("max", 6); }
            if (ability.name == "Whirlwind" || ability.name == "CycloneEye" || ability.name == "Petrify" || ability.name == "Tether" || ability.name == "BloodRite") a.active = ability.Get("duration", 3);
            if (ability.name == "Combo")
            {
                var steps = ability.argument.Split(',');
                for (int i = 0; i < steps.Length; i++)
                {
                    var part = steps[i].Split('@'); var timing = part.Length > 1 ? part[1].Split('x') : new[] { "0.7" };
                    var step = a.Clone(); step.windup = float.Parse(timing[0], CultureInfo.InvariantCulture);
                    step.baseDamage *= timing.Length > 1 ? float.Parse(timing[1], CultureInfo.InvariantCulture) : 1;
                    step.recovery = i == steps.Length - 1 ? ability.Get("rest", 1) : 0.12f;
                    var name = part[0].TrimStart('!');
                    var move = HellEnemyAbility.Parse("ComboStrike:" + name + "|arc=" + ability.Get("arc", 120).ToString(CultureInfo.InvariantCulture));
                    SetThreat(step, "ComboStrike", name);
                    step.animation = name == "thrust" ? AttackAnim.Thrust : name == "slam" ? AttackAnim.Slam : name == "leap" ? AttackAnim.Leap : AttackAnim.Special;
                    var s = new EnemyAttackEntry { attack = step, huAbility = move, minPhase = phase, huEnragedOnly = part[0].StartsWith("!"),
                        huOnPhase = i == 0 ? onphase : 0, weight = i == 0 ? entry.weight : 0, maxRange = entry.maxRange, minRange = entry.minRange,
                        cooldown = i == 0 ? entry.cooldown : 0, followUp = i < steps.Length - 1 ? e.attacks.Count + 1 : -1, followUpChance = 1 };
                    if (name == "leap") { s.special = EnemySpecial.Leap; s.specialRadius = a.radius; }
                    e.attacks.Add(s);
                }
            }
            else e.attacks.Add(entry);
        }
        public static void SetThreat(AttackDefinition attack, string name, string move = "")
        {
            attack.blockable = true; attack.parryable = true; attack.dodgeable = true; attack.heavy = false;
            if (name == "ComboStrike")
            {
                if (move == "slash" || move == "sweep") attack.heavy = true;
                else if (move == "thrust") attack.parryable = false;
                else if (move == "spin") { attack.parryable = false; attack.blockable = false; }
                else { attack.parryable = false; attack.dodgeable = false; }
            }
            else if (name == "Swing" || name == "ShieldBash") attack.heavy = true;
            else if (new[] { "Latch", "BloodRite", "Cocoon", "EggSac", "Petrify", "Volley", "Summon", "StoneWard" }.Contains(name)) { }
            else if (new[] { "Vomit", "Tether", "InfernalCharge", "TidalWave", "WindStep", "Whirlwind" }.Contains(name)) attack.parryable = false;
            else { attack.parryable = false; attack.dodgeable = false; }
        }
    }

    [Serializable] public class HellEnemyAbility
    {
        [Serializable] public class Parameter { public string key; public float value; }
        public string name, argument;
        public List<Parameter> parameters = new List<Parameter>();
        public float Get(string key, float fallback = 0) { var p = parameters.Find(x => x.key == key); return p != null ? p.value : fallback; }
        public static HellEnemyAbility Parse(string raw)
        {
            var p = raw.Split('|'); var head = p[0].Split(':');
            var a = new HellEnemyAbility { name = head[0], argument = head.Length > 1 ? head[1] : "" };
            if (p.Length > 1) foreach (var pair in p[1].Split(',')) { var kv = pair.Split('='); a.parameters.Add(new Parameter { key = kv[0], value = float.Parse(kv[1], CultureInfo.InvariantCulture) }); }
            return a;
        }
    }

    [Serializable] public class HellEnemySource
    {
        public string Id, targetId, model, sha256, displayName;
        public float height, MaxHealth = 30, MoveSpeed = 70, BodyRadius = 8, DamagePhysical, DamageFire, DamageWater, DamageAir, DamageEarth;
        public int Archetype;
        public float AttackRange = 22, AttackCooldownSeconds = 1.4f, TelegraphSeconds = 0.35f, RecoverySeconds = 0.3f, AreaRadius = 42;
        public float ProjectileSpeed = 160, ProjectileLifetimeSeconds = 3, ProjectileSpreadDegrees, PreferredDistance = 140, ChargeSpeed = 420, ChargeDistance = 200;
        public int ProjectilesPerAttack = 1, SummonCount = 2, SummonMaxAlive = 6, PackSize = 1, SplitCount;
        public string SummonEnemyId;
        public float SoulReward = 3, SpawnCost = 1, ResistPhysical, ResistFire, ResistWater, ResistAir, ResistEarth, GuardArcDegrees, WakeRadius, HatchSeconds;
        public bool CanBeElite = true;
        public string[] OnHitEffects = Array.Empty<string>(), BossAbilities = Array.Empty<string>(), Specials = Array.Empty<string>();
        public float[] BossPhaseThresholds = Array.Empty<float>();
        public float BossEnrageHealthFraction = 0.35f, BossEnrageTempoMultiplier = 1.45f;
    }
}
