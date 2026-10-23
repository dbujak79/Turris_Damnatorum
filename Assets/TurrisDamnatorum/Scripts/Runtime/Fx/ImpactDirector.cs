using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Odczucie ciosu – bez ruszania kamerą. Na każde bezpośrednie trafienie (<see cref="CombatEvents.Impact"/>) dobiera:
    ///  • hitstop lokalny – trafiony i atakujący bronią zastygają na ułamek sekundy (reszta gry toczy się dalej),
    ///  • reakcję ciała – odchylenie tułowia i głowy w kierunku ciosu, drżenie w czasie hitstopu, biały błysk,
    ///  • odrzut – wróg (i bohater) przesuwa się od ciosu po hitstopie,
    ///  • warstwy dźwięku – miażdżenie broni obuchowej, brzęk pancerza, cios kończący,
    ///  • wibracje pada – osobno dla zadanego i otrzymanego ciosu, bloku, parowania i przełamania gardy.
    /// Ciężar ciosu (0,1–1,2) wynika z danych trafienia: obrażenia postawy, ciężki atak, riposta.
    /// </summary>
    public class ImpactDirector : MonoBehaviour
    {
        static readonly BalanceConfig Fallback = new BalanceConfig();
        GameRoot root;

        void Awake() => root = GetComponent<GameRoot>();
        void OnEnable() => CombatEvents.Impact += OnImpact;
        void OnDisable() { CombatEvents.Impact -= OnImpact; Rumble.Stop(); }

        void Update() => Rumble.Tick(Time.unscaledDeltaTime, Time.timeScale <= 0f);

        BalanceConfig B => root != null && root.config != null ? root.config.balance : Fallback;

        /// <summary>Ciężar ciosu: lekki ~0,35, ciężki ~0,8–1, riposta 1,2. Strefy i czary bez postawy – słabiej.</summary>
        public static float Weight(in HitData h)
        {
            if (h.isRiposte) return 1.2f;
            float w = 0.3f + Mathf.Clamp01(h.poiseDamage / 80f) * 0.4f + (h.heavy ? 0.3f : 0f);
            if (h.physical <= 0f && h.poiseDamage <= 0f) w *= 0.4f;
            if (h.isReaction) w *= 0.6f;
            return Mathf.Clamp(w, 0.1f, 1.2f);
        }

        public static bool IsBlunt(WeaponModel w) =>
            w == WeaponModel.Mace || w == WeaponModel.Hammer || w == WeaponModel.None || w == WeaponModel.Staff;

        public static bool IsArmored(RigStyle s) => s == RigStyle.Knight || s == RigStyle.Warden || s == RigStyle.Castellan;

        void OnImpact(IHitReceiver target, HitData hit, HitResult r)
        {
            if (hit.isStatusTick || target == null) return;
            var b = B;
            var tc = target as Component;
            var tv = tc != null ? tc.GetComponent<CharacterVisual>() : null;
            var attacker = hit.attacker as IHitReceiver;
            var ac = hit.attacker as Component;
            var av = ac != null ? ac.GetComponent<CharacterVisual>() : null;

            Vector3 dir = target.Transform.position - hit.sourcePosition; dir.y = 0f;
            if (dir.sqrMagnitude < 1e-4f) dir = -target.Transform.forward;
            dir.Normalize();

            float w = Weight(hit);
            float react = Mathf.Max(0f, b.hitReactStrength);
            bool weaponHit = hit.weapon != WeaponModel.Auto;
            bool playerTarget = target is PlayerCombat;
            bool playerAttacker = hit.attacker is PlayerCombat;
            var enemy = target as EnemyBrain;
            var player = target as PlayerCombat;
            Vector3 aim = target.AimPoint;

            switch (r.outcome)
            {
                case HitOutcome.Hit:
                {
                    if (r.healthDamage <= 0.5f)
                    {
                        // W pełni pochłonięte przez osłonę: tylko lekkie pchnięcie.
                        if (tv != null) tv.Impact(dir, 0.15f * react, 0.15f);
                        break;
                    }
                    bool killed = target.IsDead;
                    float stop = hit.isRiposte ? b.hitstopRiposte : (hit.heavy || killed || w >= 0.75f) ? b.hitstopHeavy : b.hitstopLight;
                    if (weaponHit || hit.isRiposte || hit.heavy || w >= 0.5f) Stop(target, stop);
                    if (weaponHit) Stop(attacker, stop * 0.8f);
                    if (tv != null) tv.Impact(dir, w * react, playerTarget ? 0.35f : 0.65f);
                    if (weaponHit && av != null) av.AddWeaponBlood(0.1f + 0.1f * Mathf.Clamp01(w));

                    if (enemy != null && !killed) enemy.Knockback(dir, Mathf.Lerp(b.knockbackLight, b.knockbackHeavy, Mathf.InverseLerp(0.3f, 1f, w)));
                    if (player != null && !killed) player.Knockback(dir, b.playerKnockback * (hit.heavy ? 1.5f : 1f));

                    // Warstwy dźwięku nad podstawowym trafieniem (CombatFxDirector): obuch, pancerz, cios kończący.
                    if (!playerTarget)
                    {
                        if (weaponHit && IsBlunt(hit.weapon)) Sfx.Play(SoundId.HitBlunt, aim, 0.5f + 0.5f * Mathf.Clamp01(w), hit.weapon == WeaponModel.None ? 1.2f : 1f);
                        if (enemy != null && IsArmored(enemy.Style) && (weaponHit || hit.physical > 0f)) Sfx.Play(SoundId.HitArmor, aim, 0.45f + 0.25f * Mathf.Clamp01(w));
                    }
                    if (killed && !playerTarget) Sfx.Play(SoundId.KillBlow, aim, 0.6f + 0.4f * Mathf.Clamp01(w), enemy != null && enemy.Def.isBoss ? 0.75f : 1f);

                    if (playerAttacker)
                    {
                        Rumble.Play(0.12f + 0.35f * w, 0.2f + 0.3f * w, 0.06f + 0.08f * w);
                        if (killed) Rumble.Play(0.45f, 0.25f, 0.2f);
                    }
                    if (playerTarget) Rumble.Play(0.55f + 0.3f * w, 0.35f, 0.18f + 0.1f * w);
                    break;
                }
                case HitOutcome.Blocked:
                    if (tv != null) tv.Impact(dir, (hit.heavy ? 0.45f : 0.25f) * react, 0.1f);
                    Stop(target, b.hitstopBlock);
                    if (weaponHit) Stop(attacker, b.hitstopBlock);
                    if (player != null) player.Knockback(dir, b.playerKnockback * (hit.heavy ? 0.8f : 0.5f));
                    if (enemy != null) enemy.Knockback(dir, b.knockbackLight * 0.5f);
                    if (playerTarget) Rumble.Play(0.3f + (hit.heavy ? 0.3f : 0f), 0.55f, 0.1f);
                    if (playerAttacker) Rumble.Play(0.12f, 0.45f, 0.07f);
                    break;
                case HitOutcome.GuardBroken:
                    if (tv != null) tv.Impact(dir, 1f * react, 0.4f);
                    Stop(target, b.hitstopHeavy);
                    if (weaponHit) Stop(attacker, b.hitstopHeavy);
                    if (player != null) player.Knockback(dir, b.playerKnockback * 1.5f);
                    if (enemy != null) enemy.Knockback(dir, b.knockbackHeavy);
                    if (playerTarget) Rumble.Play(0.9f, 0.6f, 0.35f);
                    if (playerAttacker) Rumble.Play(0.4f, 0.5f, 0.15f);
                    break;
                case HitOutcome.Parried:
                    // Obrońca ledwie drgnie, napastnika odbija do tyłu (w stronę, z której przyszedł).
                    if (tv != null) tv.Impact(dir, 0.2f * react, 0.1f);
                    if (av != null) av.Impact(-dir, 0.9f * react, 0.3f);
                    Stop(target, b.hitstopBlock * 1.5f);
                    Stop(attacker, b.hitstopBlock * 1.5f);
                    if (playerTarget) Rumble.Play(0.2f, 0.9f, 0.12f);
                    break;
            }
        }

        static void Stop(IHitReceiver who, float seconds)
        {
            if (seconds > 0f && who is IHitStop s && !(who is PlayerCombat p && p.IsDead)) s.HitStop(seconds);
        }
    }
}
