using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Fałszywy świadek (Łotrzyk A): widoczny wabik z własną tożsamością celu i kolizją. Oszukani wrogowie kierują swój następny
    /// zwykły atak w niego; po ataku, rozbiciu albo wygaśnięciu wracają do gracza. Nie daje zabójstw, dusz, kradzieży życia
    /// ani efektów trupów – nie jest wrogiem i nie ma łupu.
    /// </summary>
    public class FeatureDecoy : MonoBehaviour, IHitReceiver, ICharacterAnimSource
    {
        public static readonly List<FeatureDecoy> Active = new List<FeatureDecoy>();

        PlayerCombat owner;
        float life, health, maxHealth;
        int maxDeceived, spent;
        readonly List<EnemyBrain> deceived = new List<EnemyBrain>();
        bool dead;
        GameObject body;

        public bool Alive => !dead && this != null && life > 0f && health > 0f;
        public int Redirected => spent;
        public int DeceivedCount => deceived.Count;
        public float Health => health;

        public Faction Faction => Faction.Player;
        public bool IsDead => !Alive;
        public Transform Transform => transform;
        public Vector3 AimPoint => transform.position + Vector3.up;

        /// <summary>Wabik doczepiony do celu (fałszywy król, śmiertelna chimera): trafienia przekazywane funkcją, bez własnego ciała.</summary>
        Transform follow; System.Func<HitData, float> onHit;

        public static FeatureDecoy SpawnOn(PlayerCombat owner, Transform follow, float seconds, int maxDeceived, float range, System.Func<HitData, float> onHit, EnemyBrain exclude = null)
        {
            var go = new GameObject("FeatureDecoyBound");
            go.transform.SetPositionAndRotation(follow.position, follow.rotation);
            var d = go.AddComponent<FeatureDecoy>();
            d.owner = owner; d.life = seconds; d.health = d.maxHealth = 1e9f; d.maxDeceived = maxDeceived;
            d.follow = follow; d.onHit = onHit;
            var col = go.AddComponent<CapsuleCollider>();
            col.center = Vector3.up * 0.9f; col.height = 1.8f; col.radius = 0.55f; col.isTrigger = false;
            // Wabik nakłada się na ciało celu: bez tego fizyka wypycha kontroler celu, a wabik za nim podąża (cel ucieka przez arenę).
            foreach (var body in follow.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(col, body);
            Active.Add(d);
            d.Deceive(range, exclude);
            return d;
        }

        /// <summary>Oszukuje (ponownie) wrogów w zasięgu do limitu – najbliżsi najpierw.</summary>
        public void Deceive(float range, EnemyBrain exclude = null)
        {
            Vector3 at = transform.position;
            var near = new List<EnemyBrain>();
            foreach (var e in EnemyBrain.All) if (e != null && !e.IsDead && e != exclude && !deceived.Contains(e) && HuWorld.Flat(e.transform.position, at) <= range) near.Add(e);
            near.Sort((a, b) => HuWorld.Flat(a.transform.position, at).CompareTo(HuWorld.Flat(b.transform.position, at)));
            foreach (var e in near)
            {
                if (deceived.Count >= maxDeceived) break;
                if (e.TryDeceive(this)) deceived.Add(e);
            }
        }

        public static FeatureDecoy Spawn(PlayerCombat owner, Vector3 at, float seconds, float health, int maxDeceived, float range)
        {
            var go = new GameObject("FeatureDecoy");
            go.transform.SetPositionAndRotation(at, owner.transform.rotation);
            var d = go.AddComponent<FeatureDecoy>();
            d.owner = owner; d.life = seconds; d.health = d.maxHealth = Mathf.Max(1f, health); d.maxDeceived = maxDeceived;
            var col = go.AddComponent<CapsuleCollider>();
            col.center = Vector3.up * 0.9f; col.height = 1.8f; col.radius = 0.4f;
            if (Application.isPlaying)
            {
                // Wygląd: cień łotrzyka (ciemna sylwetka z poświatą) – odróżnialny od gracza i od wrogów.
                d.body = new GameObject("DecoyBody");
                d.body.transform.SetParent(go.transform, false);
                var cv = d.body.AddComponent<CharacterVisual>();
                cv.BuildProcedural(new RigLook { style = RigStyle.Knight, head = HeadGear.Hood, body = BodyGear.Rags, weapon = WeaponModel.Dagger }, 1f);
                cv.Init(d);
                cv.SetTint(new Color(0.35f, 0.3f, 0.6f), 0.7f);
                HellFx.Shadow(go.transform, at, true, seconds);
                HellFx.Ambient(go.transform, Vector3.up * 0.9f, HellFxElement.Soul, 0.45f, seconds);
            }
            Active.Add(d);
            // Wrogowie w zasięgu (najbliżsi najpierw) wybierają wabik na następny dozwolony atak – do limitu.
            var near = new List<EnemyBrain>();
            foreach (var e in EnemyBrain.All) if (e != null && !e.IsDead && HuWorld.Flat(e.transform.position, at) <= range) near.Add(e);
            near.Sort((a, b) => HuWorld.Flat(a.transform.position, at).CompareTo(HuWorld.Flat(b.transform.position, at)));
            foreach (var e in near)
            {
                if (d.deceived.Count >= maxDeceived) break;
                if (e.TryDeceive(d)) d.deceived.Add(e);
            }
            return d;
        }

        /// <summary>Wróg oddał przejęty atak – zwolniony.</summary>
        public void AttackSpent(EnemyBrain e)
        {
            spent++;
            deceived.Remove(e);
            owner?.FeatureTelemetry.Add("wabik: przekierowane ataki");
            if (deceived.Count == 0 && spent >= maxDeceived && follow == null) End();
        }

        void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (dead) return;
            if (follow != null) { if (follow == null || !follow.gameObject.activeInHierarchy) { End(); return; } transform.position = follow.position; }
            life -= dt;
            if (life <= 0f || owner == null || owner.IsDead) End();
        }

        public HitResult ReceiveHit(HitData hit)
        {
            if (!Alive) return new HitResult { outcome = HitOutcome.Ignored };
            if (onHit != null) { float taken = onHit(hit); return new HitResult { outcome = HitOutcome.Hit, healthDamage = taken }; }
            float dmg = hit.Total;
            health -= dmg;
            CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.4f, "WABIK", new Color(0.7f, 0.65f, 1f));
            if (health <= 0f) End();
            return new HitResult { outcome = HitOutcome.Hit, healthDamage = dmg };
        }

        /// <summary>Koniec wabika: wszyscy oszukani wracają do gracza (bez referencji do usuniętego obiektu).</summary>
        public void End()
        {
            if (dead) return;
            dead = true;
            foreach (var e in deceived) if (e != null) e.ReleaseDecoy(this);
            deceived.Clear();
            Active.Remove(this);
            if (Application.isPlaying) FxLibrary.Flash(AimPoint, new Color(0.6f, 0.55f, 1f), 1f);
            Util.DestroySafe(gameObject);
        }

        void OnDestroy()
        {
            if (!dead) { dead = true; foreach (var e in deceived) if (e != null) e.ReleaseDecoy(this); }
            Active.Remove(this);
        }

        public static void ClearAll() { foreach (var d in Active.ToArray()) if (d != null) d.End(); Active.Clear(); }

        /// <summary>Wabik stoi w gardzie (bez akcji).</summary>
        public CharacterAnimState GetAnimState() => new CharacterAnimState { action = AnimAction.Block, phase = ActionPhase.Active };

        public void OnAttackParried(float poiseDamage) { }
        public void OnHitResolved(IHitReceiver target, in HitData hit, in HitResult result) { }
    }
}
