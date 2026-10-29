using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Bronie z Hell Unleashed w Turris. Dwa rodzaje ataku, serie, garda i parowanie pochodzą z szablonu Turris; z HU
    /// przychodzą wymagania, ciężar, efekty, tempo i siła – przeliczone zasadą DPS:
    ///
    /// <b>DPS broni w Turris stoi w tej samej proporcji do miecza, co w HU</b> (obrażenia × ataki na sekundę), a tempo
    /// ciosu względem szablonu rośnie lub maleje jak ataki na sekundę względem broni wzorcowej szablonu. Szabla jest więc
    /// szybsza od miecza, różdżka ziemi wolniejsza od różdżki ognia, a broń dwuręczna bije ~1,25× mocniej niż miecz
    /// (jak w HU), a nie ~1,45×. Cechy (Siła, Zręczność…) skalują dalej według szablonu – zasada liczy bazę.
    /// </summary>
    public static class HellWeaponCatalog
    {
        [Serializable] class Catalog { public Row[] weapons; }
        [Serializable] public class Row
        {
            public string Id, targetId, sha256, displayName, description, ClassId;
            public int Handedness, AttackKind, RequiredStrength, RequiredDexterity, RequiredIntelligence, MinWave = 1, Rarity, Power, BasePierce;
            public float DamagePhysical, DamageFire, DamageWater, DamageAir, DamageEarth, StaminaCost, Load, PowerValue;
            public float WindupSeconds = 0.12f, RecoverySeconds = 0.1f, Range = 48, AttacksPerSecond = 1, ProjectileSpeed = 420, ExplosionRadius;
            public bool AvailableInShop = true;
            /// <summary>Ekonomia HU (etap 6): cena bazowa, wzrost ceny poziomu i ostatni poziom broni.</summary>
            public float BasePrice = 25f, PriceGrowth = 1.6f;
            public int MaxLevel = 5;
            public string[] OnHitEffects = Array.Empty<string>(), Modifiers = Array.Empty<string>(), Tags = Array.Empty<string>();
            public float TotalDamage => DamagePhysical + DamageFire + DamageWater + DamageAir + DamageEarth;
            public float HuDps => TotalDamage * AttacksPerSecond;
        }

        static Row[] rows;
        public static IReadOnlyList<Row> Rows => rows ?? (rows = JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("HellCatalog/weapons").text).weapons);
        public static Row Find(string huId) => Rows.FirstOrDefault(r => r.Id == huId);
        public static Row ForTarget(string turrisId) => Rows.FirstOrDefault(r => r.targetId == turrisId);

        /// <summary>Broń wzorcowa: miecz HU = miecz Turris (lekki cios 40 przy tempie szablonu miecza).</summary>
        public const string ReferenceWeapon = "weapon.sword";
        public const float ReferenceLight = 40f;

        // Tempo Turris (jak w PlayerCombat): akcje ×1,1, odzyskanie ×0,8.
        const float Speed = 1.1f, RecoveryScale = 0.8f;

        /// <summary>Czas serii lekkich ciosów (z końcowym odzyskaniem) przy danym tempie – model z audytu balansu.</summary>
        public static float SeriesTime(AttackDefinition a, int combo) =>
            combo * (a.windup + a.active) / Speed + (combo - 1) * a.cancelAfter / Speed * RecoveryScale + a.recovery / Speed * RecoveryScale;

        /// <summary>Bazowy DPS lekkich ciosów broni Turris (bez cech, bez premii).</summary>
        public static float TurrisDps(WeaponData w) => w.light.baseDamage * Mathf.Max(1, w.lightComboLength) / Mathf.Max(0.05f, SeriesTime(w.light, Mathf.Max(1, w.lightComboLength)));

        /// <summary>DPS miecza wzorcowego przy szablonie miecza Turris.</summary>
        static float ReferenceTurrisDps(WeaponData swordTemplate)
        {
            var a = swordTemplate.light;
            int n = Mathf.Max(1, swordTemplate.lightComboLength);
            return ReferenceLight * n / SeriesTime(a, n);
        }

        /// <summary>
        /// Stosuje dane HU do broni zbudowanej na szablonie: tempo (×aps wzorca szablonu / aps broni), siłę wg zasady DPS,
        /// żywioł, wymagania, ciężar, efekty trafienia, przebicie i poziom pojawiania się w ofertach.
        /// </summary>
        public static bool Apply(ItemDefinition item, Row row, Row templateReference, WeaponData swordTemplate)
        {
            if (row == null || item.weapon == null || item.weapon.light == null) return false;
            item.huSourceId = row.Id; item.huSourceHash = row.sha256;
            item.weight = row.Load; item.twoHanded = row.Handedness == 1;
            item.requirements.Clear();
            void Require(AttributeType attribute, int amount) { if (amount > 0) item.requirements.Add(new AttributeRequirement { attribute = attribute, value = amount }); }
            Require(AttributeType.Strength, row.RequiredStrength); Require(AttributeType.Dexterity, row.RequiredDexterity); Require(AttributeType.Intelligence, row.RequiredIntelligence);

            var w = item.weapon;
            float tempo = templateReference != null && row.AttacksPerSecond > 0 ? templateReference.AttacksPerSecond / row.AttacksPerSecond : 1f;
            float heavyRatio = w.light.baseDamage > 0 ? w.heavy.baseDamage / w.light.baseDamage : 1.7f;
            foreach (var a in new[] { w.light, w.heavy })
            {
                a.windup *= tempo; a.active *= tempo; a.recovery *= tempo; a.cancelAfter *= tempo;
            }
            var reference = Find(ReferenceWeapon);
            float targetDps = ReferenceTurrisDps(swordTemplate) * row.HuDps / Mathf.Max(0.01f, reference.HuDps);
            int combo = Mathf.Max(1, w.lightComboLength);
            float light = targetDps * SeriesTime(w.light, combo) / combo;
            w.light.baseDamage = Mathf.Round(light * 10f) / 10f;
            w.heavy.baseDamage = Mathf.Round(light * heavyRatio * 10f) / 10f;

            float total = Mathf.Max(0.001f, row.TotalDamage);
            float elemental = total - row.DamagePhysical;
            if (elemental > 0f && w.light.damageType == DamageType.Physical)
            {
                float max = Mathf.Max(row.DamageFire, row.DamageWater, row.DamageAir, row.DamageEarth);
                var el = max == row.DamageFire ? Element.Fire : max == row.DamageWater ? Element.Frost : max == row.DamageAir ? Element.Lightning : Element.Earth;
                foreach (var a in new[] { w.light, w.heavy }) { a.element = el; a.elementShare = elemental / total; }
            }
            var effects = HellEnemyCatalog.Effects(row.OnHitEffects);
            if (effects.Count > 0)
            {
                w.light.statuses = new List<StatusApplication>();
                w.heavy.statuses = new List<StatusApplication>();
                foreach (var e in effects)
                {
                    w.light.statuses.Add(new StatusApplication(e.kind, e.stacks, 0.5f));
                    w.heavy.statuses.Add(new StatusApplication(e.kind, e.stacks));
                }
            }
            if (row.AttackKind == 1 && w.light.delivery == AttackDelivery.Melee)
            {
                // Kostur HU strzela pociskiem z wybuchem (odległości HU w pikselach /24).
                foreach (var a in new[] { w.light, w.heavy })
                {
                    a.delivery = AttackDelivery.Projectile; a.projectileSpeed = row.ProjectileSpeed / 24f;
                    a.explosionRadius = row.ExplosionRadius / 24f; a.animation = AttackAnim.Cast;
                    if (elemental > 0f) a.damageType = DamageType.Magic;
                }
                item.tags |= BuildTag.Magic;
            }
            if (row.BasePierce > 0) item.effects.Add(new PassiveEffect(PassiveEffectType.ProjectilePierce, row.BasePierce));
            item.minFloor = DefaultContent.MinFloorFor(row.MinWave);
            return true;
        }

        /// <summary>Premie unikatów HU → statystyki Turris (zdrowie ×2, obrażenia płaskie ×2,5, ułamki → %).</summary>
        public static void ApplyUnique(ItemDefinition item, Row row)
        {
            foreach (var m in row.Modifiers)
            {
                var parts = m.Split('|');
                if (parts.Length != 3 || !float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v)) continue;
                switch (parts[0])
                {
                    case "PhysicalDamage": item.modifiers.Add(new StatModifier(StatType.PhysicalDamage, v * 100f)); break;
                    case "AttackSpeed": item.modifiers.Add(new StatModifier(StatType.AttackSpeed, v * 100f)); break;
                    case "MaxHealth": item.modifiers.Add(new StatModifier(StatType.MaxHealth, v * 2f)); break;
                    case "Armour": item.modifiers.Add(new StatModifier(StatType.PhysicalDefense, v * 6f)); break;
                    case "PhysicalResistance": item.modifiers.Add(new StatModifier(StatType.PhysicalDefense, v * 100f)); break;
                    case "FireResistance": item.modifiers.Add(new StatModifier(StatType.FireResist, v * 100f)); break;
                    case "FireDamage": item.modifiers.Add(new StatModifier(StatType.FireDamage, v * 100f)); break;
                    case "AirDamage": item.modifiers.Add(new StatModifier(StatType.LightningDamage, v * 100f)); break;
                    case "AirPenetration": item.modifiers.Add(new StatModifier(StatType.LightningDamage, v * 100f)); break; // brak przebicia odporności w Turris
                    case "BleedDamage": item.modifiers.Add(new StatModifier(StatType.BleedDamage, v * 100f)); break;
                    case "ExecuteDamage": item.modifiers.Add(new StatModifier(StatType.ExecuteDamage, v * 100f)); break;
                    case "MoveSpeed": item.modifiers.Add(new StatModifier(StatType.MoveSpeed, v * 100f)); break;
                    case "LifeOnKill": item.effects.Add(new PassiveEffect(PassiveEffectType.HealOnKill, v * 2f)); break;
                    case "Evasion": item.modifiers.Add(new StatModifier(StatType.Dexterity, Mathf.Round(v * 100f / 1.5f))); break; // uchylenie Turris rośnie ze Zręcznością (1,5%/pkt)
                    case "ProjectilePierce": item.effects.Add(new PassiveEffect(PassiveEffectType.ProjectilePierce, v)); break;
                }
            }
            switch (row.Power)
            {
                case 1: item.effects.Add(new PassiveEffect(PassiveEffectType.CheatDeath, row.PowerValue)); break;
                case 2: item.effects.Add(new PassiveEffect(PassiveEffectType.CorpseExplosion, row.PowerValue)); break;
                case 3: item.effects.Add(new PassiveEffect(PassiveEffectType.EmberAura, row.PowerValue * PlayerCombat.HuDamageScale)); break;
                case 4: item.effects.Add(new PassiveEffect(PassiveEffectType.Thorns, row.PowerValue * PlayerCombat.HuDamageScale)); break;
                case 5: item.effects.Add(new PassiveEffect(PassiveEffectType.WeaponChainLightning, row.PowerValue)); break;
                case 6: item.effects.Add(new PassiveEffect(PassiveEffectType.FrostWard, row.PowerValue)); break;
                case 7: item.effects.Add(new PassiveEffect(PassiveEffectType.LowHealthDamage, row.PowerValue)); break;
                case 8: item.effects.Add(new PassiveEffect(PassiveEffectType.SoulHarvest, row.PowerValue)); break;
            }
            item.offerWeight = 0.35f; // unikaty rzadziej w nagrodach (w HU tylko z łupów)
        }
    }
}
