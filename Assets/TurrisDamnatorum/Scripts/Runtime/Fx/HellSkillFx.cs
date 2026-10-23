using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Oprawa przeniesionych skilli HU. Woda i powietrze zachowują własne cząsteczki,
    /// mimo że mechanika Turris nazywa te obrażenia mrozem i błyskawicą.
    /// </summary>
    public static class HellSkillFx
    {
        public static HellFxElement Family(SpellDefinition skill)
        {
            if (skill == null) return HellFxElement.Physical;
            switch (skill.id)
            {
                case "hu_maelstrom": case "hu_nova_water": return HellFxElement.Water;
                case "hu_wind_slash": case "hu_nova_air": case "hu_tempest": case "hu_storm_cleave": return HellFxElement.Air;
                case "hu_overload": case "hu_legion": return HellFxElement.Soul;
                case "hu_aura_defiance": return HellFxElement.Earth;
                default: return HellFx.For(skill.attack.element);
            }
        }

        public static void Cast(SpellDefinition skill, Vector3 at, Vector3 forward)
        {
            if (!FxLibrary.Enabled || skill == null || !skill.IsSpell) return;
            // Krótki błysk dostawy; trwające fazy mają właściciela (pocisk, aura, strefa).
            var family = Family(skill);
            FxLibrary.Particles(at, new FxLibrary.Emit
            {
                sprite = "glow", burst = 1, life = new Vector2(0.16f,0.22f), size = new Vector2(0.5f,0.65f),
                colorA = skill.color, sizeGrow = 1.2f
            });
            if (family == HellFxElement.Air) HellFx.Crackle(at + forward * 0.3f);
        }

        public static void Shockwave(SpellDefinition skill, Vector3 at, float radius, bool dust = false)
        {
            if (skill == null || !FxLibrary.Enabled) return;
            HellFx.Shockwave(at, Family(skill), skill.color, radius, dust);
        }

        public static FxDecal Telegraph(SpellDefinition skill, Transform caster)
        {
            if (!FxLibrary.Enabled) return null;
            var decal = FxLibrary.Decal(caster.position, HellVfxMaterials.Ground(Family(skill), true), skill.color,
                skill.attack.radius * 2, skill.attack.radius * 2, skill.telegraph + 0.05f);
            decal.Progress = 0;
            var timer = decal.gameObject.AddComponent<HellTelegraph>(); timer.seconds = skill.telegraph;
            if (skill.followCaster) decal.Follow(caster);
            return decal;
        }
    }
}
