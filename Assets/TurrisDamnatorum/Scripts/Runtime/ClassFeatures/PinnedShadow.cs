using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Przygwożdżony cień (Zabójca A): nieruchomy drugi punkt odbierania trafień jednej ofiary. Kolizja cienia jest aliasem
    /// ofiary (<see cref="IHitAlias"/>) – w jednym ataku ciało i cień liczą się jako jeden cel, a trafienie przechodzi przez
    /// zwykłą obronę, premie i nietykalność ofiary raz. Cień nie ma życia, AI, łupu, cierni ani efektów śmierci.
    /// </summary>
    public class PinnedShadow : MonoBehaviour, IHitReceiver, IHitAlias, ICharacterAnimSource
    {
        public static readonly List<PinnedShadow> Active = new List<PinnedShadow>();

        EnemyBrain victim;
        float life, link;
        bool ended;
        GameObject decal;
        LineRenderer tether;

        public EnemyBrain Victim => victim;
        /// <summary>Trafienia przekazane przez cień (telemetria).</summary>
        public int Forwarded { get; private set; }
        public bool Alive => !ended && victim != null && !victim.IsDead && life > 0f
                             && HuWorld.Flat(victim.transform.position, transform.position) <= link;

        public IHitReceiver AliasOf => Alive ? victim : null;
        public Faction Faction => Faction.Enemy;
        public bool IsDead => !Alive;
        public Transform Transform => transform;
        public Vector3 AimPoint => transform.position + Vector3.up * (victim != null ? victim.Def.scale : 1f);

        public static PinnedShadow Spawn(EnemyBrain victim, float seconds, float linkRange)
        {
            var go = new GameObject("PinnedShadow");
            go.transform.SetPositionAndRotation(HuWorld.Ground(victim.transform.position), victim.transform.rotation);
            var s = go.AddComponent<PinnedShadow>();
            s.victim = victim; s.life = seconds; s.link = linkRange;
            float sc = victim.Def.scale;
            var col = go.AddComponent<CapsuleCollider>();
            col.center = Vector3.up * 0.9f * sc; col.height = 1.8f * sc; col.radius = 0.45f * sc;
            if (Application.isPlaying)
            {
                s.decal = HellFx.Shadow(go.transform, go.transform.position, true, seconds)?.gameObject;
                // Sylwetka cienia: ciemna, fioletowa postać w miejscu przygwożdżenia + krąg – czytelny drugi punkt trafień.
                var body = new GameObject("ShadowBody");
                body.transform.SetParent(go.transform, false);
                var cv = body.AddComponent<CharacterVisual>();
                cv.BuildProcedural(new RigLook { style = RigStyle.Ghoul, head = HeadGear.Hood, body = BodyGear.Rags, hunch = 15f, weapon = WeaponModel.None, glowingEyes = true, eyes = new Color(0.8f, 0.3f, 1f) }, sc);
                cv.Init(s);
                cv.SetTint(new Color(0.12f, 0.02f, 0.18f), 0.85f);
                FxLibrary.Decal(go.transform.position, FxMaterials.Ring, new Color(0.7f, 0.25f, 0.9f, 0.9f), 1.6f * sc, 1.6f * sc, seconds, -20f);
                s.tether = go.AddComponent<LineRenderer>();
                s.tether.material = FxMaterials.Additive;
                s.tether.widthMultiplier = 0.07f;
                s.tether.startColor = new Color(0.85f, 0.35f, 1f, 0.9f); s.tether.endColor = new Color(0.6f, 0.2f, 0.85f, 0.7f);
                s.tether.positionCount = 2;
                s.tether.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            Active.Add(s);
            return s;
        }

        void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (ended) return;
            life -= dt;
            if (!Alive) { End(); return; }
            if (tether != null)
            {
                // Czytelny związek z ofiarą: nić od cienia do ciała.
                tether.SetPosition(0, transform.position + Vector3.up * 0.1f);
                tether.SetPosition(1, victim.AimPoint);
            }
        }

        /// <summary>Bezpośrednie trafienie w kolizję cienia (gdy ktoś przekaże je tu, a nie przez alias) – przekazanie do ofiary.</summary>
        public HitResult ReceiveHit(HitData hit)
        {
            if (!Alive) return new HitResult { outcome = HitOutcome.Ignored };
            Forwarded++;
            return victim.ReceiveHit(hit);
        }

        /// <summary>Trafienie doszło do ofiary przez cień (HitQuery rozstrzyga alias) – licznik.</summary>
        public void CountForward() => Forwarded++;

        public void End()
        {
            if (ended) return;
            ended = true;
            Active.Remove(this);
            Util.DestroySafe(gameObject);
        }

        void OnDestroy() { Active.Remove(this); }

        public static void ClearAll() { foreach (var s in Active.ToArray()) if (s != null) s.End(); Active.Clear(); }

        /// <summary>Kolizja w zasięgu którejś z aktywnych cieni (do liczenia przekazań).</summary>
        public static PinnedShadow Near(EnemyBrain victim, Vector3 point, float radius)
        {
            foreach (var s in Active) if (s != null && s.victim == victim && HuWorld.Flat(s.transform.position, point) <= radius) return s;
            return null;
        }

        /// <summary>Cień zamarł w pozie ofiary z chwili przygwożdżenia (bez własnej akcji).</summary>
        public CharacterAnimState GetAnimState() => new CharacterAnimState { action = AnimAction.Flinch, phase = ActionPhase.Active, phaseProgress = 0.5f };

        public void OnAttackParried(float poiseDamage) { }
        public void OnHitResolved(IHitReceiver target, in HitData hit, in HitResult result) { }
    }
}
