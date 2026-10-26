using System.Linq;
using UnityEngine;

namespace Turris
{
    public static partial class DefaultContent
    {
        /// <summary>
        /// Pozostałe skille z Hell Unleashed (etap 4): definicje Turris generowane z katalogu HU. Działanie, poziomy, odnowienia
        /// i pieczęcie pochodzą z parametrów HU (<see cref="HuSkillCaster"/>); asset dokłada koszt rzutu, animację, barwę i opis.
        /// Obrażenia w opisie i w <c>attack.baseDamage</c> to poziom I × 2,5 (skala Turris).
        /// </summary>
        static void AddHellSkills(GameConfig cfg)
        {
            foreach (var hu in HuCatalog.Skills.Values.OrderBy(s => s.id))
            {
                if (string.IsNullOrEmpty(hu.targetId) || bundle.byId.ContainsKey(hu.targetId)) continue;
                var s = FromHu(hu);
                if (hu.shop && !hu.IsSignature) cfg.baseSpellPool.Add(s);
            }
        }

        /// <summary>Rodzaj Turris (animacja, opisy, ikony) z komponentów HU – zachowanie i tak należy do komponentów.</summary>
        static SpellKind KindFor(HuSkillData hu)
        {
            if (hu.HasComponent("Aura")) return SpellKind.Aura;
            if (hu.HasComponent("Summon")) return SpellKind.Summon;
            if (hu.HasComponent("Spin")) return SpellKind.Whirlwind;
            if (hu.HasComponent("Dash")) return SpellKind.Dash;
            if (hu.HasComponent("Barrier")) return SpellKind.Barrier;
            if (hu.HasComponent("Shove")) return SpellKind.Warcry;
            if (hu.HasComponent("Chain")) return SpellKind.Chain;
            if (hu.HasComponent("Cone")) return SpellKind.Cone;
            if (hu.HasComponent("Wave")) return SpellKind.Wave;
            if (hu.HasComponent("Line")) return SpellKind.Quake;
            if (hu.HasComponent("Nova")) return SpellKind.Nova;
            if (hu.HasComponent("Projectile")) return SpellKind.Projectile;
            if (hu.HasComponent("Barrage") || hu.HasComponent("Fall")) return SpellKind.Meteor;
            return SpellKind.Zone;
        }

        /// <summary>Pierwsza fala HU, w której skill może trafić do sklepu → piętro Turris (30 fal HU ≈ 6 pięter).</summary>
        public static int MinFloorFor(int minWave) => minWave >= 16 ? 3 : minWave >= 7 ? 1 : 0;

        static SpellDefinition FromHu(HuSkillData hu)
        {
            bool spell = hu.tags.Contains("spell");
            var p1 = hu.At(1);
            string levels = (string.IsNullOrEmpty(hu.l2) ? "" : "\n" + hu.l2) + (string.IsNullOrEmpty(hu.l3) ? "" : "\n" + hu.l3);
            var s = Make<SpellDefinition>(hu.targetId, hu.name, hu.desc + levels, spell ? BuildTag.Magic : BuildTag.Melee);
            s.kind = KindFor(hu);
            s.category = spell ? SkillCategory.Spell : SkillCategory.Technique;
            s.flatDamage = true;
            float cd = p1.GetF(HuParams.Cooldown, 5f);
            s.cooldown = cd; s.cooldownReductionPerLevel = 0f; s.charges = Mathf.Max(1, Mathf.RoundToInt(p1.GetF(HuParams.Charges, 1f)));
            // Koszt jak w HU: własny „mana=”/„stamina=” albo domyślny rosnący z odnowieniem.
            s.manaCost = spell ? p1.GetF("mana", 8f + 2f * cd) : 0f;
            s.staminaCost = spell ? 0f : p1.GetF("stamina", 10f + cd);
            s.intelligenceScaling = spell ? 1f : 0f;
            s.castTime = s.kind == SpellKind.Aura ? 0.05f : 0.25f; s.recovery = 0.35f; s.cancelAfter = 0.2f;
            s.maxLevel = Mathf.Max(0, hu.MaxLevel - 1); s.powerPerLevel = 0f; s.costReductionPerLevel = 0f;
            float dmg = (float)p1.TotalDamage * PlayerCombat.HuDamageScale;
            var el = HuCast.ToTurris(hu.element);
            s.attack = Atk(hu.name, dmg, 0.25f, 0.1f, 0.35f, p1.GetF(HuParams.Range, p1.GetF("distance", p1.GetF("length", 0f))),
                p1.GetF("radius", p1.GetF("explosion_radius", p1.GetF("zone_radius", p1.GetF("strike_radius", 0f)))), 0, 0, 25, 20,
                type: spell ? DamageType.Magic : DamageType.Physical, delivery: AttackDelivery.AreaAroundSelf);
            s.attack.parryable = false;
            s.attack.element = el;
            s.attack.elementShare = el == Element.None ? 0f : (float)((p1.TotalDamage - p1.Damage(HuElement.Physical)) / System.Math.Max(1e-6, p1.TotalDamage));
            if (!spell && hu.tags.Contains("projectile") && hu.element == HuElement.Physical && hu.classId == "rogue") s.attack.animation = AttackAnim.BowDraw;
            s.amount = dmg;
            s.color = HuCast.ColorFor(hu.element, hu.HasComponent("Summon") ? new Color(0.45f, 1f, 0.65f) : new Color(0.85f, 0.85f, 0.9f));
            if (p1.On("ice")) s.color = new Color(0.6f, 0.9f, 1f);
            s.servant = (ServantKind)Mathf.Clamp(Mathf.RoundToInt(p1.GetF("servant")), 0, 2);
            s.minFloor = MinFloorFor(hu.minWave);
            return s;
        }
    }
}
