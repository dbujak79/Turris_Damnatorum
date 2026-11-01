using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Wszystko, czego potrzebuje jeden rzut skilla HU (port <c>SkillCast</c>): przerobiony kształt, miejsce i kierunek,
    /// punkt celowania, moc (przeciążenie, narastanie, udział kopii) i flagi kopii / przekaźnika / wyzwalacza.
    /// </summary>
    public sealed class HuCast
    {
        public readonly PlayerCombat caster;
        public readonly SpellRuntime spell;
        public readonly HuSkillShape shape;
        public readonly Vector3 origin, direction, target;
        public readonly float power;
        /// <summary>Odpalony z kotwicy (nova z przekaźnikiem) – zostaje tam, gdzie wybuchł.</summary>
        public readonly bool relayed;
        /// <summary>Kopia (echo, wachlarz, kaskada, słup) – nie przenosi bohatera.</summary>
        public readonly bool copy;
        /// <summary>Odpalony wyzwalaczem – to, co zabije, nie odpala kolejnego wyzwalacza.</summary>
        public readonly bool triggered;
        /// <summary>Mnożnik promieni: pieczęć obszaru (<c>area_scale</c>) i jedna trzecia dodatkowej mocy (przeciążenie poszerza).</summary>
        public readonly float area;
        public readonly Color color;
        public readonly HellFxElement family;
        /// <summary>Gwałtowna manifestacja: skala czasu stref tego rzutu, pobrana raz przy tworzeniu (kopie dziedziczą profil bez ponownej kompresji).</summary>
        public float zoneTimeScale = 1f;
        /// <summary>Strefy tego rzutu kwalifikują się do manifestacji (pomiar realizacji budżetu także bez perka).</summary>
        public bool zoneQualified;

        public HuCast(PlayerCombat caster, SpellRuntime spell, Vector3 origin, Vector3 direction, Vector3 target, float power = 1f,
            bool relayed = false, bool copy = false, bool triggered = false)
        {
            this.caster = caster; this.spell = spell;
            shape = spell.instance.seals.Shape;
            origin.y = caster != null ? caster.transform.position.y : origin.y;
            this.origin = origin;
            direction.y = 0; this.direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector3.forward;
            this.target = target;
            this.power = power; this.relayed = relayed; this.copy = copy; this.triggered = triggered;
            area = (1f + (power - 1f) / 3f) * Mathf.Max(0.1f, P.GetF("area_scale", 1f))
                   * (1f + (caster != null && caster.Build != null ? caster.Build.effects[PassiveEffectType.AreaSize] / 100f : 0f)); // Szeroki zasięg (perk HU)
            bool ice = P.On("ice");
            family = ice ? HellFxElement.Ice : FamilyFor(shape.Element, spell.Def);
            color = ice ? new Color(0.6f, 0.9f, 1f) : ColorFor(shape.Element, spell.Def.color);
            zoneTimeScale = caster != null ? caster.ManifestationScale(spell.Def, triggered) : 1f;
            zoneQualified = caster != null && caster.ManifestationQualifies(spell.Def, triggered);
        }

        public HuParams P => shape.Parameters;
        public HuSkillData Data => spell.instance.seals.data;
        public SpellDefinition Def => spell.Def;

        public float Area(string key, float fallback = 0f) => P.GetF(key, fallback) * area;
        public float Length(string key, float fallback = 0f) => P.GetF(key, fallback);

        /// <summary>Kopia z innego miejsca albo kierunku, z częścią mocy. Kopia nigdy nie robi tego, co tylko rzut bohatera (przeskok).</summary>
        public HuCast Variant(Vector3 o, Vector3 d, Vector3 t, float share, bool? relay = null) =>
            new HuCast(caster, spell, o, d, t, power * share, relay ?? relayed, true, triggered);

        /// <summary>Trafienie skilla z jego obrażeniami, premiami, efektami i odrzutem; <paramref name="share"/> – część mocy.</summary>
        public HitData Hit(float share = 1f, bool projectile = false, bool plain = false) => caster.HuHit(this, share, projectile, plain);

        /// <summary>
        /// Komponenty uderzenia (Explosion, Zone, Barrage, Infuse…) w punkcie – wołane przez komponent dostawy albo od razu.
        /// Z zapalnikiem najpierw znak na ziemi; z grawitacją punkt najpierw wciąga wszystko wokół.
        /// </summary>
        public void Impact(Vector3 point)
        {
            float fuse = P.GetF("fuse_seconds");
            if (fuse > 0f)
            {
                var mark = VisualFx.GroundMarker(HuWorld.Ground(point), Area("explosion_radius", 2f), color, family);
                HuWorld.Add(new HuTelegraphRuntime(mark, fuse));
                HuWorld.After(fuse, () => Collapse(point));
                return;
            }
            Collapse(point);
        }

        void Collapse(Vector3 point)
        {
            if (P.On("implode_radius")) { HuWorld.Add(new HuImplode(this, point, () => ImpactNow(point))); return; }
            ImpactNow(point);
        }

        void ImpactNow(Vector3 point)
        {
            if (caster == null || caster.IsDead) return;
            point = HuWorld.Ground(point);
            foreach (var name in shape.Components)
                if (HuSkillCaster.PhaseOf(name) == HuPhase.Impact) HuSkillCaster.RunComponent(name, this, point);

            // Zawierucha: wir w miejscu uderzenia wciąga ocalałych.
            float whirl = P.GetF("impact_vortex_seconds");
            if (whirl > 0f)
                HuWorld.Add(new HuVortex(this, point, Area("impact_vortex_radius", 2.6f), whirl, 0.4f, Length("impact_vortex_pull", 4f),
                    Hit(P.GetF("impact_vortex_share", 0.2f), plain: true), 0f, 0f));

            // Przeskok: bohater przenosi się tam, gdzie skill uderzył.
            if (P.On("blink_to_impact") && !copy && !relayed) caster.HuTeleport(point, color);
        }

        public static HellFxElement FamilyFor(HuElement e, SpellDefinition def)
        {
            switch (e)
            {
                case HuElement.Fire: return HellFxElement.Fire;
                case HuElement.Water: return HellFxElement.Water;
                case HuElement.Air: return HellFxElement.Air;
                case HuElement.Earth: return HellFxElement.Earth;
                default: return def != null ? HellSkillFx.Family(def) : HellFxElement.Physical;
            }
        }

        public static Color ColorFor(HuElement e, Color fallback)
        {
            switch (e)
            {
                case HuElement.Fire: return new Color(1f, 0.5f, 0.15f);
                case HuElement.Water: return new Color(0.55f, 0.85f, 1f);
                case HuElement.Air: return new Color(0.78f, 0.82f, 1f);
                case HuElement.Earth: return new Color(0.78f, 0.6f, 0.35f);
                default: return fallback;
            }
        }

        /// <summary>Żywioł HU w Turris: woda → mróz, powietrze → błyskawice.</summary>
        public static Element ToTurris(HuElement e)
        {
            switch (e)
            {
                case HuElement.Fire: return Element.Fire;
                case HuElement.Water: return Element.Frost;
                case HuElement.Air: return Element.Lightning;
                case HuElement.Earth: return Element.Earth;
                default: return Element.None;
            }
        }
    }
}
