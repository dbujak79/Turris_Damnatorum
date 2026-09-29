using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Widoczność efektów na postaci: płomienie przy podpaleniu, szron przy chłodzie i zamrożeniu,
    /// iskry przy porażeniu, krople krwi przy krwawieniu. Obiekty są dziećmi postaci i znikają razem z efektem.
    /// </summary>
    public class StatusFx
    {
        GameObject fire;
        ParticleSystem flames, frost, shock;
        float bleedTimer;

        public void Update(StatusEffects s, Transform owner, float scale, float dt)
        {
            if (!FxLibrary.Enabled || owner == null) return;
            Vector3 chest = owner.position + Vector3.up * 1.1f * scale;

            // Podpalenie: ogień przy piersi + wyraźne języki płomieni na całej sylwetce.
            if (s.Burning && fire == null)
            {
                fire = FxLibrary.Fire(chest, 1.3f * scale, owner);
                flames = FxLibrary.Aura(owner, Vector3.up * 0.9f * scale, Names.StatusColor(StatusKind.Burn), 70f, 0.4f * scale, 0.3f);
            }
            else if (!s.Burning && fire != null) { Object.Destroy(fire); fire = null; Stop(ref flames); }

            bool cold = s.ChillStacks > 0 || s.Frozen;
            if (cold && frost == null) frost = FxLibrary.Aura(owner, Vector3.up * 1.0f * scale, Names.StatusColor(StatusKind.Chill), 55f, 0.6f * scale, 0.16f);
            else if (!cold) Stop(ref frost);

            if (s.Shocked && shock == null) shock = FxLibrary.Aura(owner, Vector3.up * 1.2f * scale, Names.StatusColor(StatusKind.Shock), 22f, 0.5f * scale, 0.05f);
            else if (!s.Shocked) Stop(ref shock);

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
            if (s.ChillStacks > 0) { c = Names.StatusColor(StatusKind.Chill); amount = 0.12f * s.ChillStacks; return true; }
            if (s.Burning) { c = Names.StatusColor(StatusKind.Burn); amount = 0.12f + 0.06f * Mathf.Sin(Time.time * 14f); return true; }
            c = Color.white; amount = 0f;
            return false;
        }

        public void Clear()
        {
            if (fire != null) Object.Destroy(fire);
            fire = null;
            Stop(ref flames);
            Stop(ref frost);
            Stop(ref shock);
        }

        static void Stop(ref ParticleSystem ps)
        {
            if (ps == null) return;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Object.Destroy(ps.gameObject, 1f);
            ps = null;
        }
    }
}
