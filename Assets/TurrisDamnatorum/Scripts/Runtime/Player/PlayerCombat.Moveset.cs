using UnityEngine;

namespace Turris
{
    /// <summary>Rodzaj ciosu z zestawu ruchów ciężkiej broni dwuręcznej.</summary>
    public enum GreatMove { Normal, Running, AfterDodge, Charged }

    /// <summary>
    /// Zestaw ruchów ciężkiej broni dwuręcznej (topór, młot) wzorowany na ciężkim toporze z gier souls-like:
    /// <list type="bullet">
    /// <item>lekkie – szerokie poziome cięcia na przemian (z prawej, z lewej);</item>
    /// <item>ciężkie – rąbnięcia z góry na przemian: z wykrokiem, potem zza pleców (seria ciężkich);</item>
    /// <item>przytrzymany ciężki – ładowanie z bronią uniesioną nad głową, mocniejszy cios i fala uderzeniowa;</item>
    /// <item>z biegu: lekki – niskie cięcie z rozpędu, ciężki – skok z rąbnięciem i falą;</item>
    /// <item>tuż po uniku (przewrót, odskok) – szybkie rąbnięcie z góry.</item>
    /// </list>
    /// </summary>
    public partial class PlayerCombat
    {
        /// <summary>Bieg skończony najwyżej tyle sekund temu – cios liczy się jako atak z biegu.</summary>
        public const float RunAttackWindow = 0.2f;
        /// <summary>Cios w trakcie końcówki uniku albo tyle sekund po nim – atak po uniku.</summary>
        public const float DodgeAttackWindow = 0.35f;
        /// <summary>Ładowanie: zatrzymanie zamachu w tym punkcie (broń nad głową), najdłużej ChargeMax s; od ChargeMin – cios naładowany.</summary>
        public const float ChargeHoldAt = 0.8f, ChargeMax = 0.8f, ChargeMin = 0.25f;
        /// <summary>Pełne naładowanie: +60% obrażeń i +50% obrażeń postawy.</summary>
        public const float ChargeDamageBonus = 0.6f, ChargePoiseBonus = 0.5f;
        public const float RunningLightDamage = 1.1f, RunningHeavyDamage = 1.2f, RunningHeavyPoise = 1.3f;
        /// <summary>Rąbnięcie po uniku jest szybsze (krótszy zamach).</summary>
        public const float DodgeChopWindup = 0.8f;
        /// <summary>Rozpęd z biegu przenoszony na początek ciosu (m/s, wygasa do końca fazy aktywnej).</summary>
        public const float RunningLightCarry = 3.5f, RunningHeavyCarry = 4.5f;
        /// <summary>Fala uderzeniowa ciosu naładowanego i skoku z biegu: część obrażeń ciosu w promieniu (m) wokół miejsca uderzenia.</summary>
        public const float ShockwaveShare = 0.5f, ShockwaveRadius = 2.4f;

        /// <summary>Przycisk ciężkiego ataku trzymany (PlayerController; boty nie ładują).</summary>
        public bool HeavyHeld { get; set; }
        public GreatMove CurrentMove { get; private set; }
        /// <summary>Czas ładowania bieżącego ciężkiego ciosu (s).</summary>
        public float ChargeTime { get; private set; }
        /// <summary>Rozpęd ciosu z biegu (m/s) – PlayerController dokłada go do kroku z klipu.</summary>
        public float AttackCarry { get; private set; }
        /// <summary>Atak, który właśnie trwa (z wariantem z biegu/po uniku), albo null.</summary>
        public AttackDefinition CurrentAttack => currentAttack;

        float lastSprintTime = -10f;
        bool sprinting, chargeReleased;
        float moveDamage = 1f, movePoise = 1f;
        readonly HitTracker shockTracker = new HitTracker();

        /// <summary>Ustawiane przez PlayerController.</summary>
        public bool Sprinting
        {
            get => sprinting;
            set { sprinting = value; if (value) lastSprintTime = clock; }
        }

        /// <summary>Zestaw ruchów działa z toporem lub młotem dwuręcznym (bez drugiej broni i nie w czasie rzutu).</summary>
        public bool GreatWeaponMoveset => Build != null && Build.offWeapon == null && AnimResolve.IsHeavyTwoHand(HeldWeaponModel);

        /// <summary>Wariant ciosu z zestawu ruchów (z biegu, po uniku) – kopia definicji, bo zmienia animację i siłę.</summary>
        AttackDefinition ChooseMove(ActionType type, AttackDefinition atk)
        {
            CurrentMove = GreatMove.Normal;
            AttackCarry = 0f; ChargeTime = 0f; chargeReleased = false;
            moveDamage = movePoise = 1f;
            if (!GreatWeaponMoveset || atk.delivery != AttackDelivery.Melee) return atk;
            bool chaining = Actions.Current == type;
            if (!chaining && clock - lastSprintTime < RunAttackWindow && Actions.Current != ActionType.Dodge)
            {
                var r = atk.Clone();
                CurrentMove = GreatMove.Running;
                if (type == ActionType.LightAttack) { r.animation = AttackAnim.RunningSweep; moveDamage = RunningLightDamage; AttackCarry = RunningLightCarry; }
                else { r.animation = AttackAnim.LeapChop; moveDamage = RunningHeavyDamage; movePoise = RunningHeavyPoise; AttackCarry = RunningHeavyCarry; }
                return r;
            }
            if (type == ActionType.LightAttack && (Actions.Current == ActionType.Dodge || TimeSinceDodge < DodgeAttackWindow))
            {
                var d = atk.Clone();
                CurrentMove = GreatMove.AfterDodge;
                d.animation = AttackAnim.DodgeChop;
                d.windup *= DodgeChopWindup;
                return d;
            }
            return atk;
        }

        /// <summary>
        /// Ładowanie ciężkiego ciosu: póki przycisk trzymany, zamach stoi w punkcie ChargeHoldAt (broń nad głową).
        /// Zwraca true, gdy w tej klatce akcja ma stać (pomija tyknięcie akcji).
        /// </summary>
        bool ChargeHolding(float dt)
        {
            if (Actions.Current != ActionType.HeavyAttack || Actions.Phase != ActionPhase.Startup) return false;
            if (CurrentMove != GreatMove.Normal && CurrentMove != GreatMove.Charged) return false;
            if (!GreatWeaponMoveset) return false;
            if (!HeavyHeld) chargeReleased = true;
            if (chargeReleased || ChargeTime >= ChargeMax || Actions.PhaseProgress < ChargeHoldAt) return false;
            ChargeTime = Mathf.Min(ChargeMax, ChargeTime + dt);
            if (ChargeTime >= ChargeMin)
            {
                CurrentMove = GreatMove.Charged;
                float f = ChargeTime / ChargeMax;
                moveDamage = 1f + ChargeDamageBonus * f;
                movePoise = 1f + ChargePoiseBonus * f;
            }
            return true;
        }

        /// <summary>Mnożniki ciosu z zestawu ruchów (z biegu, naładowany).</summary>
        void ApplyMove(ref HitData hit)
        {
            hit.physical *= moveDamage; hit.magic *= moveDamage;
            hit.poiseDamage *= movePoise;
        }

        /// <summary>Fala uderzeniowa w miejscu uderzenia ciosu naładowanego i skoku z biegu – trafia tych, których ostrze ominęło.</summary>
        void MoveShockwave()
        {
            bool leap = CurrentMove == GreatMove.Running && Actions.Current == ActionType.HeavyAttack;
            if (CurrentMove != GreatMove.Charged && !leap) return;
            var atk = currentAttack;
            if (atk == null) return;
            Vector3 at = transform.position + transform.forward * Mathf.Min(atk.reach, 2f);
            float radius = ShockwaveRadius * AreaFactor;
            shockTracker.Reset();
            HitQuery.Sphere(at + Vector3.up * 0.5f, radius, Faction.Player, shockTracker, target =>
            {
                if (tracker.Contains(target)) return;
                var hit = HitData.FromAttack(atk, Build.WeaponDamage(atk) * ShockwaveShare, at, this);
                hit.weapon = HeldWeaponModel;
                ApplyMove(ref hit);
                PrepareOutgoing(ref hit);
                HitQuery.Apply(this, target, hit, false);
            });
            HellFx.SlamDust(at, radius);
            FxLibrary.Shockwave(at, new Color(0.85f, 0.75f, 0.6f), radius, false);
            Sfx.Play(SoundId.Quake, at, 0.8f, 1.15f);
        }
    }
}
