using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Widoczność efektów na postaci: płomienie przy podpaleniu, szron przy chłodzie i zamrożeniu,
    /// iskry przy porażeniu, krople krwi przy krwawieniu. Obiekty są dziećmi postaci i znikają razem z efektem.
    /// </summary>
    public class StatusFx
    {
        HellParticles fire, frost, shock, sunder;
        ParticleSystem stun;
        float bleedTimer;

        public void Update(StatusEffects s, Transform owner, float scale, float dt)
        {
            if (!FxLibrary.Enabled || owner == null) return;
            Vector3 chest = owner.position + Vector3.up * 1.1f * scale;

            // Podpalenie: ogień przy piersi + wyraźne języki płomieni na całej sylwetce.
            if (s.Burning && fire == null)
            {
                fire = HellFx.Flame(owner, Vector3.up * 0.9f * scale, 0.65f * scale);
            }
            else if (!s.Burning) Stop(ref fire);

            bool cold = s.ChillStacks > 0 || s.Frozen;
            if (cold && frost == null) frost = HellFx.Ambient(owner, Vector3.up * 0.7f * scale, HellFxElement.Ice, 0.5f * scale);
            else if (!cold) Stop(ref frost);

            if (s.Shocked && shock == null) shock = HellFx.ElectricAura(owner, Vector3.up * scale);
            else if (!s.Shocked) Stop(ref shock);

            // Ogłuszenie: wirujące iskry nad głową; zachwianie i pęknięty pancerz – drobny pył kamienny.
            if (s.Stunned && stun == null) stun = FxLibrary.Aura(owner, Vector3.up * 2.0f * scale, Names.StatusColor(StatusKind.Stun), 30f, 0.35f * scale, 0.05f);
            else if (!s.Stunned) Stop(ref stun);
            bool cracked = s.StaggerStacks > 0 || s.SunderStacks > 0;
            if (cracked && sunder == null) { sunder = HellFx.Ambient(owner, Vector3.up * 0.9f * scale, HellFxElement.Earth, 0.4f * scale); sunder.SetAmount(0.3f); sunder.SetScale(0.4f); }
            else if (!cracked) Stop(ref sunder);

            bleedTimer -= dt;
            if (s.BleedStacks > 0 && bleedTimer <= 0)
            {
                bleedTimer = Mathf.Lerp(0.6f, 0.25f, s.BleedStacks / 5f);
                FxLibrary.Blood(chest, Vector3.down + Random.insideUnitSphere * 0.3f, 0.25f + 0.08f * s.BleedStacks);
            }
        }

        /// <summary>Zabarwienie postaci (zamrożenie – lodowy błękit, chłód – lekki, podpalenie – pomarańcz).</summary>
        public static bool Tint(StatusEffects s, out Color c, out float amount)
        {
            if (s.Frozen) { c = new Color(0.6f, 0.9f, 1f); amount = 0.6f; return true; }
            if (s.Stunned) { c = Names.StatusColor(StatusKind.Stun); amount = 0.3f + 0.15f * Mathf.Sin(Time.time * 18f); return true; }
            if (s.ChillStacks > 0) { c = Names.StatusColor(StatusKind.Chill); amount = 0.12f * s.ChillStacks; return true; }
            if (s.Burning) { c = Names.StatusColor(StatusKind.Burn); amount = 0.12f + 0.06f * Mathf.Sin(Time.time * 14f); return true; }
            c = Color.white; amount = 0f;
            return false;
        }

        public void Clear()
        {
            Stop(ref fire);
            Stop(ref frost);
            Stop(ref shock);
            Stop(ref stun);
            Stop(ref sunder);
        }

        static void Stop(ref ParticleSystem ps)
        {
            if (ps == null) return;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Object.Destroy(ps.gameObject, 1f);
            ps = null;
        }

        static void Stop(ref HellParticles fx)
        {
            if (fx != null) fx.StopAndRelease();
            fx = null;
        }
    }
}
