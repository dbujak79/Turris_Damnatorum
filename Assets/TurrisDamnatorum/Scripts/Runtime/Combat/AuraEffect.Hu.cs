using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Aura z Hell Unleashed (z formami – port <c>AuraForms</c>/<c>AuraEmitter</c>): premie ze statystyk
    /// (<c>aura.flat.*</c>, <c>aura.pct.*</c>) i impulsy, które ranią, leczą albo oczyszczają.
    /// Forma może postawić aurę jako sztandar (działa tylko w jego kręgu, mocniej), liczyć demony w zasięgu albo
    /// zamienić premię na impuls.
    /// </summary>
    public partial class AuraEffect
    {
        const float CountSeconds = 0.3f, HealPerEnemyCap = 10;

        PlayerCombat hero;
        HuCast huCast;
        float power = 1f, countTimer, pulseTimer;
        Vector3? anchor;
        GameObject banner;
        int enemiesInside;

        /// <summary>Siła premii w tej chwili (forma: sztandar, tłum); 1 dla zwykłej aury.</summary>
        public float Strength { get; private set; } = 1f;
        public bool IsHu => huCast != null;
        public HuParams HuParameters => huCast != null ? huCast.P : null;

        public static AuraEffect SpawnHu(PlayerCombat hero, SpellRuntime sk, float power)
        {
            var cast = new HuCast(hero, sk, hero.transform.position, hero.transform.forward, hero.transform.position, power);
            var p = cast.P;
            float radius = p.GetF("radius", 3.5f) * Mathf.Sqrt(power);
            bool isBanner = p.On("aura_banner");
            var go = new GameObject("Aura_" + sk.Def.id);
            if (!isBanner) go.transform.SetParent(hero.transform, false);
            else go.transform.position = HuWorld.Ground(hero.transform.position);
            var a = go.AddComponent<AuraEffect>();
            a.Skill = sk.Def; a.owner = hero; a.hero = hero; a.huCast = cast; a.power = power; a.Radius = radius; a.color = cast.color;
            a.interval = Mathf.Max(0.2f, p.GetF("pulse_interval", 0f)); a.timer = a.interval;
            if (isBanner) { a.anchor = go.transform.position; a.banner = BuildBanner(go.transform.position, cast.color); }
            if (FxLibrary.Enabled)
            {
                a.particles = HellFx.Ambient(go.transform, Vector3.up * 0.15f, cast.family, radius * 0.9f);
                a.ring = FxLibrary.Decal(go.transform.position, FxMaterials.Ring, cast.color * 0.8f, radius * 2f, radius * 2f, 0f, 25f);
                if (a.ring != null) a.ring.Follow(go.transform);
            }
            a.Recount();
            return a;
        }

        Vector3 Center => anchor ?? transform.position;
        /// <summary>Środek działania aury (sztandar, bohater albo cel Sądu przysięgi).</summary>
        public Vector3 CenterPoint => Center;
        public float PulseEvery => huCast != null ? huCast.P.GetF("pulse_interval") : 0f;

        /// <summary>Premie aury HU dla statystyk bohatera (× moc Pobożności × siła formy).</summary>
        public IEnumerable<StatModifier> HuModifiers()
        {
            if (huCast == null) yield break;
            float k = power * Strength;
            if (k <= 0f) yield break;
            foreach (var kv in huCast.P.Values)
            {
                bool flat = kv.Key.StartsWith(HuParams.AuraFlatPrefix);
                if (!flat && !kv.Key.StartsWith(HuParams.AuraPercentPrefix)) continue;
                string stat = kv.Key.Substring((flat ? HuParams.AuraFlatPrefix : HuParams.AuraPercentPrefix).Length);
                if (MapStat(stat, (float)kv.Value, out var type, out float value)) yield return new StatModifier(type, value * k, ModifierMode.Flat);
            }
        }

        /// <summary>Statystyki HU → Turris (pancerz ×6 obrony, ułamki → %, zdrowie ×2 – jak przy klasach z HU).</summary>
        public static bool MapStat(string stat, float v, out StatType type, out float value)
        {
            value = v * 100f; type = StatType.GlobalDamage;
            switch (stat)
            {
                case "Armour": type = StatType.PhysicalDefense; value = v * 6f; return true;
                case "DamageReduction": type = StatType.DamageReduction; return true;
                case "GlobalDamage": type = StatType.GlobalDamage; return true;
                case "AttackSpeed": type = StatType.AttackSpeed; return true;
                case "MoveSpeed": type = StatType.MoveSpeed; return true;
                case "FireResistance": type = StatType.FireResist; return true;
                case "WaterResistance": type = StatType.FrostResist; return true;
                case "AirResistance": type = StatType.LightningResist; return true;
                case "EarthResistance": type = StatType.EarthResist; return true;
                case "HealthRegen": type = StatType.HealthRegen; value = v * 2f; return true;
                default: return false;
            }
        }

        void Recount()
        {
            var p = huCast.P;
            bool counts = p.On("aura_per_enemy") || p.On("heal_per_enemy");
            enemiesInside = 0;
            if (counts) foreach (var e in EnemyBrain.All) if (e != null && !e.IsDead && HuWorld.Flat(e.transform.position, Center) <= Radius) enemiesInside++;
            float s = p.GetF("aura_strength", 1f);
            if (anchor.HasValue) s *= HuWorld.Flat(hero.transform.position, Center) <= Radius ? p.GetF("banner_power", 1.5f) : 0f;
            float perEnemy = p.GetF("aura_per_enemy");
            if (perEnemy > 0f) s *= Mathf.Min(p.GetF("aura_per_enemy_cap", 3f), perEnemy * enemiesInside);
            if (Mathf.Abs(s - Strength) > 0.001f) { Strength = s; hero.AuraStrengthChanged(); }
        }

        void UpdateHu(float dt)
        {
            if (hero == null || hero.IsDead) return;
            countTimer -= dt;
            if (countTimer <= 0f) { countTimer = CountSeconds; Recount(); }
            var p = huCast.P;
            float every = p.GetF("pulse_interval");
            if (every <= 0f) return;
            pulseTimer += dt;
            if (pulseTimer < every) return;
            pulseTimer -= every;
            HuPulse();
        }

        /// <summary>Jeden impuls aury HU: obrażenia w kręgu, zdrowie dla bohatera, zdjęcie efektów.</summary>
        void HuPulse()
        {
            var p = huCast.P;
            float pulsePower = anchor.HasValue ? p.GetF("banner_power", 1.5f) : 1f;
            if (p.TotalDamage > 0)
            {
                var hit = huCast.Hit(pulsePower);
                hit.knockback = 0f; hit.knockdown = 0f;
                if (hero.UniquesAuraPulse(this, hit)) { }
                else if (HuWorld.HitCircle(hero, Center, Radius, hit) > 0 && FxLibrary.Enabled) HellFx.Shockwave(Center, huCast.family, huCast.color, Radius);
            }
            int enemies = p.On("heal_per_enemy") ? CountInside() : enemiesInside;
            float heal = (p.GetF("heal_pulse") + p.GetF("heal_per_enemy") * Mathf.Min(HealPerEnemyCap, enemies)) * power;
            if (heal > 0f) { hero.Health.Restore(hero.Health.Max * heal); FxLibrary.Heal(hero.transform, huCast.color, 0.4f); }
            if (p.On("cleanse_pulse") && hero.Status.Any) { hero.Status.Clear(); HellFx.Burst(hero.transform.position, HellFxElement.Water, 1.4f); }
        }

        int CountInside()
        {
            int n = 0;
            foreach (var e in EnemyBrain.All) if (e != null && !e.IsDead && HuWorld.Flat(e.transform.position, Center) <= Radius) n++;
            return n;
        }

        static GameObject BuildBanner(Vector3 at, Color color)
        {
            var go = new GameObject("AuraBanner");
            go.transform.position = at;
            PartBuilder.Add(go.transform, ProcMesh.Tube(0.7f), Surface.Wood, new Color(0.12f, 0.09f, 0.07f), new Vector3(0, 1.3f, 0), new Vector3(0.1f, 1.3f, 0.1f));
            PartBuilder.Add(go.transform, ProcMesh.Box(), Surface.Cloth, color * 0.65f, new Vector3(0.38f, 2.05f, 0), new Vector3(0.7f, 0.9f, 0.03f));
            PartBuilder.BakeAll(go.transform);
            if (FxLibrary.Enabled) FxLibrary.PointLight(at + Vector3.up * 2.7f, color, 1.4f, 4f, 600f, go.transform);
            return go;
        }
    }
}
