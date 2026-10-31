using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Wpływy cech klasowych gracza na wroga – jawne, ograniczone, liczone zegarem gry:
    /// wabik (Fałszywy świadek) zmienia cel następnego dozwolonego ataku; znacznik tętnicy spełnia próg egzekucji bez zmiany PŻ;
    /// Sąd przysięgi nakłada odwrócone aury; Zderzenie frontów wyładowuje się przy rzeczywistej kolizji odrzucanego ciała;
    /// ściany Bastionu blokują drogę i są atakowane przy zetknięciu.
    /// </summary>
    public partial class EnemyBrain
    {
        /// <summary>Wróg zaczyna zapowiedź ataku (Kradzież sztuczki obserwuje rzeczywisty atak).</summary>
        public static event Action<EnemyBrain, EnemyAttackEntry> AttackStarted;
        /// <summary>Atak wroga wszedł w fazę aktywną (wypuszczenie pocisku, uderzenie obszarowe).</summary>
        public static event Action<EnemyBrain, EnemyAttackEntry> AttackActive;

        // ------------------------------------------------------------------ Wabik

        FeatureDecoy decoy;
        bool attackOnDecoy;
        /// <summary>Wróg wybrał wabik na swój następny dozwolony atak.</summary>
        public bool Deceived => decoy != null && decoy.Alive;
        public FeatureDecoy CurrentDecoy => Deceived ? decoy : null;
        /// <summary>Atak w toku celuje w wabik (już rozpoczęty – zachowuje cel do końca).</summary>
        public bool AttackingDecoy => attackOnDecoy;

        Vector3 TargetPos => Deceived || (attackOnDecoy && decoy != null) ? decoy.transform.position : player.transform.position;
        Vector3 TargetAim => Deceived || (attackOnDecoy && decoy != null) ? decoy.AimPoint : player.AimPoint;

        /// <summary>
        /// Przynęta przejmuje następny atak – tylko wróg, który nie jest w trakcie ataku (rozpoczęte ciosy zachowują cel).
        /// Bossy: jeden dopuszczony atak; specjalne, fazowe i arenowe ataki idą normalnie (wybierają gracza we własnej logice).
        /// </summary>
        public bool TryDeceive(FeatureDecoy d)
        {
            if (IsDead || d == null || Deceived || CurrentState == State.Attacking || CurrentState == State.Latched || TrainingDummy) return false;
            decoy = d;
            return true;
        }

        /// <summary>Wabik zniknął – wróg wraca do gracza (bez wiszącej referencji).</summary>
        public void ReleaseDecoy(FeatureDecoy d)
        {
            if (decoy != d) return;
            decoy = null;
            attackOnDecoy = false;
        }

        void FeatureAttackStarted(EnemyAttackEntry entry)
        {
            attackOnDecoy = Deceived && entry != null && entry.special == EnemySpecial.None;
            AttackStarted?.Invoke(this, entry);
        }

        /// <summary>Koniec (albo przerwanie) ataku: wykorzystany wabik zwalnia wroga.</summary>
        void FeatureAttackEnded()
        {
            if (attackOnDecoy && decoy != null) { decoy.AttackSpent(this); decoy = null; }
            attackOnDecoy = false;
            if (decoy != null && !decoy.Alive) decoy = null;
        }

        // ------------------------------------------------------------------ Otwarta tętnica

        /// <summary>Znacznik tętnicy (s) – cel spełnia próg egzekucji. Prawdziwe PŻ i progi faz bez zmian.</summary>
        public float ArteryTime { get; set; }
        bool RealExecuteThreshold => Health.Fraction < 0.3f || CurrentState == State.PoiseBroken;
        /// <summary>Jeden wspólny predykat kwalifikacji do premii egzekucji (rzeczywisty próg albo znacznik – nigdy dwa naliczenia).</summary>
        public bool ExecuteQualified => RealExecuteThreshold || ArteryTime > 0f;
        /// <summary>Egzekucja tylko dzięki znacznikowi (telemetria: premia poza zwykłym progiem).</summary>
        public static event Action<EnemyBrain> ArteryExecute;

        // ------------------------------------------------------------------ Sąd przysięgi

        /// <summary>Odwrócone aury paladyna na tym wrogu (null – brak).</summary>
        public JudgementEffect Judgement { get; set; }
        float FeatureDamageMult => Judgement != null && Judgement.Active ? Judgement.damageMult : 1f;
        float FeatureSpeedMult => Judgement != null && Judgement.Active ? Judgement.speedMult : 1f;
        float FeatureArmourCut => Judgement != null && Judgement.Active ? Judgement.armourCut : 0f;
        float FeatureResistCut => Judgement != null && Judgement.Active ? Judgement.resistCut : 0f;

        // ------------------------------------------------------------------ Zderzenie frontów

        float frontsBudget, frontsTime;
        int frontsOrigin, frontsDischargedOrigin;
        PlayerCombat frontsOwner;
        static readonly Dictionary<int, int> frontsPerOrigin = new Dictionary<int, int>();
        /// <summary>Zderzenie: (wróg, obrażenia, czy naprężenie odpornego celu) – telemetria.</summary>
        public static event Action<EnemyBrain, float, bool> FrontsDischarge;
        public static int FrontsLimited { get; private set; }

        /// <summary>Odrzut z oknem Zderzenia frontów: oznacz ruch; cel odporny na przesuwanie dostaje ograniczone naprężenie od razu.</summary>
        void FeatureKnockback(in HitData hit)
        {
            if (hit.frontsBudget <= 0f || !(hit.attacker is PlayerCombat pc)) return;
            bool resistant = Def.isBoss || (CurrentState == State.Attacking && current != null && current.hyperArmor);
            if (resistant)
            {
                if (frontsDischargedOrigin == hit.originId) return;
                frontsDischargedOrigin = hit.originId;
                float strain = hit.frontsBudget * pc.FeatureTuning.frontsStrain;
                var h = pc.FeatureHit("Naprężenie frontów", 0f, strain, Element.Lightning, transform.position);
                h.isReaction = true; h.knockback = 0f; h.poiseDamage *= 0.5f;
                HitQuery.Apply(pc, this, h, false);
                FrontsDischarge?.Invoke(this, strain, true);
                return;
            }
            frontsBudget = hit.frontsBudget; frontsOrigin = hit.originId; frontsOwner = pc;
            frontsTime = 0.35f;
        }

        void CheckFrontsCollision(float dt)
        {
            if (frontsTime <= 0f) return;
            frontsTime -= dt;
            if (frontsOwner == null || frontsOwner.IsDead || displacementTime <= 0f) return;
            if ((cc.collisionFlags & CollisionFlags.Sides) == 0) return;
            if (frontsDischargedOrigin == frontsOrigin) return; // jedno wyładowanie na atak i cel
            frontsPerOrigin.TryGetValue(frontsOrigin, out int n);
            var t = frontsOwner.FeatureTuning;
            if (n >= t.frontsMaxPerSource) { FrontsLimited++; frontsTime = 0f; return; }
            frontsPerOrigin[frontsOrigin] = n + 1;
            if (frontsPerOrigin.Count > 64) frontsPerOrigin.Clear();
            frontsDischargedOrigin = frontsOrigin;
            frontsTime = 0f;
            Vector3 at = transform.position + Vector3.up * 0.8f + displacement.normalized * 0.5f * Def.scale;
            var h = frontsOwner.FeatureHit("Zderzenie frontów", 0f, frontsBudget, Element.Lightning, at);
            h.isReaction = true; h.knockback = 0f; // bez ponownego odrzutu i bez rekurencji
            var tr = new HitTracker();
            var owner = frontsOwner;
            HitQuery.Sphere(at, t.frontsRadius, Faction.Player, tr, r => HitQuery.Apply(owner, r, h, false));
            LightningBolt.Strike(at + Vector3.up * 1.5f, at, new Color(0.75f, 0.85f, 1f));
            Sfx.Play(SoundId.LightningZap, at, 0.8f);
            FrontsDischarge?.Invoke(this, frontsBudget, false);
        }

        public static void ResetFeatureStatics() { frontsPerOrigin.Clear(); FrontsLimited = 0; }

        // ------------------------------------------------------------------ Tyknięcie

        void FeatureTick(float dt)
        {
            if (ArteryTime > 0f) ArteryTime -= dt;
            if (Judgement != null)
            {
                Judgement.Tick(this, dt);
                if (!Judgement.Active) Judgement = null;
            }
            CheckFrontsCollision(dt);
            if (CurrentState == State.Moving || CurrentState == State.Attacking) EarthWall.Touch(this, dt);
        }

        void RaiseArteryExecute() => ArteryExecute?.Invoke(this);
    }

    /// <summary>Odwrócone aury paladyna na jednym wrogu (Sąd przysięgi) – liczone z rzeczywiście aktywnych aur.</summary>
    public sealed class JudgementEffect
    {
        public PlayerCombat owner;
        public float remaining;
        public float armourCut, resistCut, speedMult = 1f, damageMult = 1f, renewalDps;
        public readonly List<string> auraIds = new List<string>();
        float tick;
        public bool Active => remaining > 0f && owner != null && !owner.IsDead;

        public void Tick(EnemyBrain e, float dt)
        {
            remaining -= dt;
            if (renewalDps <= 0f || !Active) return;
            tick += dt;
            if (tick < 0.5f) return;
            // Odwrócona Odnowa: stały ubytek z siły leczenia aury (nie procent maks. życia bossa), jako tyknięcie.
            var h = new HitData { magic = renewalDps * tick, element = Element.None, isStatusTick = true, isGroundHazard = true, attacker = owner,
                attackName = "Odwrócona odnowa", mods = StatusModifiers.None, featureEffect = true, sourcePosition = e.transform.position };
            tick = 0f;
            HitQuery.Apply(owner, e, h, false);
        }
    }
}
