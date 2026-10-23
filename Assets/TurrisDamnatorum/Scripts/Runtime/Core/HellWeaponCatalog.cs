using System;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>Dane nowych wariantów P1. Dwa rodzaje ataku, skalowania i garda pozostają z szablonu Turris.</summary>
    public static class HellWeaponCatalog
    {
        [Serializable] class Catalog { public Row[] weapons; }
        [Serializable] class Row
        {
            public string Id, targetId, sha256;
            public int Handedness, AttackKind, RequiredStrength, RequiredDexterity, RequiredIntelligence;
            public float DamagePhysical, DamageFire, DamageWater, DamageAir, DamageEarth, StaminaCost, Load;
            public float WindupSeconds = 0.12f, RecoverySeconds = 0.1f, Range = 48, AttacksPerSecond = 1, ProjectileSpeed = 420, ExplosionRadius;
            public string[] OnHitEffects = Array.Empty<string>();
        }
        static Row[] rows;
        public static bool Apply(ItemDefinition item)
        {
            rows = rows ?? JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("HellCatalog/weapons").text).weapons;
            var row = rows.FirstOrDefault(r => r.targetId == item.id);
            if (row == null && (item.id == "hu_staff_witch" || item.id == "hu_staff_summoner")) row = rows.First(r => r.Id == "weapon.staff");
            if (row == null) return false;
            item.huSourceId = row.Id; item.huSourceHash = row.sha256;
            item.weight = row.Load; item.twoHanded = row.Handedness == 1;
            item.requirements.Clear();
            void Require(AttributeType attribute, int amount) { if (amount > 0) item.requirements.Add(new AttributeRequirement { attribute = attribute, value = amount }); }
            Require(AttributeType.Strength, row.RequiredStrength); Require(AttributeType.Dexterity, row.RequiredDexterity); Require(AttributeType.Intelligence, row.RequiredIntelligence);
            var light = item.weapon.light;
            HellEnemyCatalog.SetDamage(light, row.DamagePhysical, row.DamageFire, row.DamageWater, row.DamageAir, row.DamageEarth);
            light.windup = row.WindupSeconds; light.active = 0.12f;
            light.recovery = Mathf.Max(row.RecoverySeconds, 1 / Mathf.Max(0.1f, row.AttacksPerSecond) - light.windup - light.active);
            light.reach = row.Range / 24f; light.staminaCost = row.StaminaCost;
            light.statuses = HellEnemyCatalog.Effects(row.OnHitEffects);
            if (row.AttackKind == 1)
            {
                light.delivery = AttackDelivery.Projectile; light.projectileSpeed = row.ProjectileSpeed / 24f;
                light.explosionRadius = row.ExplosionRadius / 24f; light.animation = AttackAnim.Cast;
                item.tags |= BuildTag.Magic;
            }
            var heavy = item.weapon.heavy;
            heavy.baseDamage = light.baseDamage * 1.7f; heavy.element = light.element; heavy.elementShare = light.elementShare;
            heavy.damageType = light.damageType; heavy.delivery = light.delivery; heavy.projectileSpeed = light.projectileSpeed;
            heavy.explosionRadius = light.explosionRadius; heavy.statuses = HellEnemyCatalog.Effects(row.OnHitEffects);
            heavy.staminaCost = light.staminaCost * 1.3f;
            if (row.AttackKind == 1) heavy.animation = AttackAnim.Cast;
            return true;
        }
    }
}
