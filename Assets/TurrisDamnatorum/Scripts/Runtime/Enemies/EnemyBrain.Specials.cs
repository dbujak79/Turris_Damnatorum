using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Turris
{
    /// <summary>
    /// Ataki specjalne demonów z Hell Unleashed (<see cref="EnemySpecial"/>): wgryzienie czerwia, szarża po zapowiedzianej
    /// linii, kręgi ognia, kule zostawiające płonącą ziemię, skok z uderzeniem w krąg i przywołanie sług.
    /// Liczby (zasięgi, promienie, czasy) pochodzą z definicji ataku – tak jak specjały HU są danymi, a nie kodem pod demona.
    /// </summary>
    public partial class EnemyBrain
    {
        readonly List<GameObject> specialMarkers = new List<GameObject>();
        Vector3 leapTarget;
        bool leapLocked, chargeExploded;
        Vector3 chargeLast;
        float chargeTrail;
        readonly HitTracker specialTracker = new HitTracker();

        // Uczepienie
        bool latched;
        EnemyAttackEntry latchEntry;
        float latchTimer, latchTick, latchStart, latchSlow;
        Vector3 latchOffset;

        const float LatchSlow = 0.2f;

        static Color FireColor => Names.ElementColor(Element.Fire);

        /// <summary>Atak specjalny w toku (dla testów i interfejsu).</summary>
        public EnemySpecial CurrentSpecial => CurrentState == State.Attacking && current != null ? current.special : EnemySpecial.None;

        void BeginSpecial(EnemyAttackEntry e)
        {
            ClearSpecialMarkers();
            leapLocked = false;
            chargeExploded = false;
            specialTracker.Reset();
            BeginHell(e);
            if (e.special == EnemySpecial.Latch || e.special == EnemySpecial.Leap || e.special == EnemySpecial.Charge)
                lungeSpeed = 0f; // ruch prowadzi sam atak specjalny
            var c = TelegraphColors.For(e.attack.Telegraph); c.a = 1f;
            switch (e.special)
            {
                case EnemySpecial.Charge:
                    // Zapowiedziana linia szarży: kręgi wzdłuż drogi (podążają za obrotem do końca namierzania).
                    int n = Mathf.Max(2, Mathf.CeilToInt(e.specialCount / 1.6f));
                    for (int i = 0; i < n; i++) specialMarkers.Add(VisualFx.GroundMarker(transform.position, e.attack.radius, c));
                    break;
                case EnemySpecial.Leap:
                    specialMarkers.Add(VisualFx.GroundMarker(player.transform.position, e.specialRadius, c));
                    break;
            }
        }

        /// <summary>Ruch i znaczniki w czasie zapowiedzi.</summary>
        Vector3 SpecialWindup(AttackDefinition a, float speed)
        {
            float progress = Mathf.Clamp01(attackT / Mathf.Max(0.01f, a.windup));
            if (current.huAbility != null && !string.IsNullOrEmpty(current.huAbility.name)) foreach (var m in specialMarkers) SetProgress(m, progress);
            switch (current.special)
            {
                case EnemySpecial.Charge:
                {
                    Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
                    for (int i = 0; i < specialMarkers.Count; i++)
                    {
                        if (specialMarkers[i] == null) continue;
                        float d = (i + 0.5f) * current.specialCount / specialMarkers.Count;
                        specialMarkers[i].transform.position = transform.position + fwd * d + Vector3.up * 0.03f;
                        SetProgress(specialMarkers[i], progress);
                    }
                    return Vector3.zero;
                }
                case EnemySpecial.Leap:
                {
                    // Cel skoku zamyka się w połowie zapowiedzi; potem demon leci łukiem na to miejsce.
                    if (!leapLocked)
                    {
                        leapTarget = player.transform.position;
                        if (progress >= 0.55f) leapLocked = true;
                        if (specialMarkers.Count > 0 && specialMarkers[0] != null) specialMarkers[0].transform.position = leapTarget + Vector3.up * 0.03f;
                    }
                    if (specialMarkers.Count > 0 && specialMarkers[0] != null) SetProgress(specialMarkers[0], progress);
                    if (!leapLocked) return Vector3.zero;
                    return Toward(leapTarget, a.windup - attackT, 0.4f, 30f);
                }
            }
            return Vector3.zero;
        }

        /// <summary>Ruch w fazie aktywnej (skok czerwia, szarża) – prędkość w m/s.</summary>
        Vector3 SpecialActive(AttackDefinition a, float speed, float dt)
        {
            if (current.huAbility != null && current.huAbility.name == "Whirlwind") return transform.forward * (current.huAbility.Get("speed", 70) / 24f);
            switch (current.special)
            {
                case EnemySpecial.Latch:
                    return Toward(player.transform.position, a.windup + a.active - attackT, 0.5f, 22f);
                case EnemySpecial.Charge:
                {
                    Vector3 pos = transform.position;
                    // Szarża: trafia raz po drodze; za sobą zostawia pas płonącej ziemi.
                    HitQuery.Capsule(chargeLast + Vector3.up, pos + Vector3.up + transform.forward * 0.6f * Def.scale, a.radius, Faction.Enemy, specialTracker, r =>
                    {
                        var hit = HitData.FromAttack(a, a.baseDamage * DamageMult, transform.position, this);
                        HitQuery.Apply(this, r, hit, true);
                    });
                    // Sparowana szarża kończy atak już w trakcie trafienia (OnAttackParried zeruje current) – dalej nie ma czego prowadzić.
                    if (current == null || CurrentState != State.Attacking) return Vector3.zero;
                    chargeTrail += Vector3.Distance(Flat(chargeLast), Flat(pos));
                    chargeLast = pos;
                    if (chargeTrail >= 2f && current.specialDuration > 0f)
                    {
                        chargeTrail = 0f;
                        LeaveFire(pos, a.radius, current.specialDuration, current.specialShare);
                    }
                    // Ściana przerywa szarżę – wybuch tam, gdzie demon się zatrzymał.
                    if (attackT > a.windup + 0.05f && (cc.collisionFlags & CollisionFlags.Sides) != 0) attackT = a.windup + a.active;
                    return transform.forward * (current.specialCount / Mathf.Max(0.05f, a.active));
                }
            }
            return Vector3.zero;
        }

        void EndSpecialActive()
        {
            if (current != null && current.special == EnemySpecial.Charge && !chargeExploded && attackPhase != ActionPhase.Startup)
            {
                chargeExploded = true;
                var a = current.attack;
                Burst(transform.position, current.specialRadius, HitData.FromAttack(a, a.baseDamage * DamageMult, transform.position, this));
            }
            ClearSpecialMarkers();
        }

        /// <summary>Chwila ciosu ataku specjalnego. Zwraca true, gdy atak specjalny zastąpił zwykłe działanie ataku.</summary>
        bool SpecialStrike(AttackDefinition a, float dmg)
        {
            if (HellStrike(a, dmg)) return true;
            var e = current;
            switch (e.special)
            {
                case EnemySpecial.Charge:
                    chargeLast = transform.position;
                    chargeTrail = 0f;
                    ClearSpecialMarkers();
                    Sfx.Play(SoundId.SwingHeavy, AimPoint, 1f, 0.7f);
                    FxLibrary.Flash(AimPoint, FireColor, 1.6f);
                    return true;

                case EnemySpecial.Leap:
                    ClearSpecialMarkers();
                    Burst(transform.position, e.specialRadius, HitData.FromAttack(a, dmg, transform.position, this));
                    return true;

                case EnemySpecial.FireZones:
                {
                    int count = Mathf.Max(1, Mathf.RoundToInt(e.specialCount));
                    float delay = e.specialDuration > 0f ? e.specialDuration : 1.1f;
                    var color = a.element != Element.None ? Names.ElementColor(a.element) : TelegraphColors.For(a.Telegraph);
                    for (int i = 0; i < count; i++)
                    {
                        // Pierwszy krąg tam, gdzie stoi bohater, reszta wokół – ucieczka wymaga uniku albo biegu.
                        Vector3 at = player.transform.position;
                        if (i > 0) at += Quaternion.Euler(0, i * 360f / (count - 1) + Random.Range(-25f, 25f), 0) * Vector3.forward * Random.Range(2.4f, 4.4f);
                        at.y = transform.position.y;
                        DelayedStrike.Spawn(at, e.specialRadius, delay, HitData.FromAttack(a, dmg, at, this), Faction.Enemy, this, color);
                    }
                    Sfx.Play(SoundId.CastCharge, AimPoint);
                    return true;
                }

                case EnemySpecial.Orbs:
                {
                    int count = Mathf.Max(1, Mathf.RoundToInt(e.specialCount));
                    Vector3 origin = transform.position + Vector3.up * 1.3f * Def.scale + transform.forward * 0.8f * Def.scale;
                    Vector3 aim = player.AimPoint - origin; aim.y = 0f;
                    var opts = new ProjectileOptions
                    {
                        fullExplosion = true, zoneRadius = e.specialRadius * 0.8f, zoneDuration = e.specialDuration, zoneInterval = 0.5f, zoneShare = e.specialShare,
                    };
                    for (int i = 0; i < count; i++)
                    {
                        float ang = count == 1 ? 0f : Mathf.Lerp(-a.spreadAngle, a.spreadAngle, i / (float)(count - 1));
                        var hit = HitData.FromAttack(a, dmg, origin, this);
                        hit.explosionRadius = e.specialRadius;
                        Projectile.Spawn(origin, Quaternion.Euler(0, ang, 0) * aim.normalized, a.projectileSpeed, hit, Faction.Enemy, this,
                            FireColor, 0.45f, 6f, null, 0f, false, opts);
                    }
                    Sfx.Play(SoundId.FireExplosion, AimPoint, 0.5f, 1.4f);
                    return true;
                }

                case EnemySpecial.Summon:
                {
                    if (e.summon == null) return true;
                    int alive = 0;
                    foreach (var m in All) if (m != null && m.Owner == this && m.Def == e.summon && !m.IsDead) alive++;
                    int count = Mathf.Min(Mathf.Max(1, Mathf.RoundToInt(e.specialCount)), Mathf.Max(0, e.summonMax - alive));
                    if (Def.huArchetype == 7) count = Mathf.Min(count, Mathf.Max(0, e.summonMax * 3 - hellRaisedTotal));
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 at = transform.position + Quaternion.Euler(0, i * 360f / Mathf.Max(1, count) + Random.Range(-20f, 20f), 0) * Vector3.forward * (1.6f * Def.scale);
                        FxLibrary.Flash(at + Vector3.up, new Color(0.7f, 0.3f, 1f), 1.2f);
                        SummonRequested?.Invoke(this, e.summon, at);
                        if (Def.huArchetype == 7) hellRaisedTotal++;
                    }
                    Sfx.Play(SoundId.BossRoar, AimPoint, 0.7f, 1.2f);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Wybuch w kręgu: trafia wszystkich w promieniu, fala na ziemi, wstrząs.</summary>
        void Burst(Vector3 at, float radius, HitData hit)
        {
            var tr = new HitTracker();
            HitQuery.Sphere(at + Vector3.up, radius, Faction.Enemy, tr, r => HitQuery.Apply(this, r, hit, false));
            var c = hit.element != Element.None ? Names.ElementColor(hit.element) : TelegraphColors.For(current != null ? current.attack.Telegraph : TelegraphKind.Heavy);
            FxLibrary.Shockwave(at, c, radius);
            FxLibrary.Dust(at, radius);
            Sfx.Play(hit.element == Element.Fire ? SoundId.FireExplosion : SoundId.Quake, at);
        }

        void LeaveFire(Vector3 at, float radius, float duration, float share)
        {
            var a = current.attack;
            var tick = HitData.FromAttack(a, a.baseDamage * DamageMult * Mathf.Max(0.05f, share), at, this);
            tick.poiseDamage = 0f; tick.guardLoad = 0f; tick.blockable = false; tick.parryable = false;
            DamageZone.Spawn(at, radius, duration, 0.5f, tick, Faction.Enemy, this, FireColor);
        }

        /// <summary>Prędkość, z jaką trzeba lecieć, by dotrzeć do celu w pozostałym czasie (zatrzymanie tuż przed nim).</summary>
        Vector3 Toward(Vector3 target, float timeLeft, float stop, float maxSpeed)
        {
            Vector3 d = target - transform.position; d.y = 0f;
            float dist = d.magnitude - stop * Def.scale;
            if (dist <= 0.02f) return Vector3.zero;
            return d.normalized * Mathf.Min(dist / Mathf.Max(0.05f, timeLeft), maxSpeed);
        }

        static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

        static void SetProgress(GameObject marker, float p)
        {
            var decal = marker != null ? marker.GetComponent<FxDecal>() : null;
            if (decal != null) decal.Progress = p;
        }

        void ClearSpecialMarkers()
        {
            foreach (var m in specialMarkers) if (m != null) Destroy(m);
            specialMarkers.Clear();
        }

        // ------------------------------------------------------------------ Uczepienie (czerw)

        /// <summary>
        /// Wgryzienie: czerw jedzie na bohaterze (najwyżej trzy naraz), spowalnia go i co sekundę gryzie (krwawienie).
        /// Strząsa go unik; po czasie trwania puszcza sam.
        /// </summary>
        void BeginLatch(EnemyAttackEntry e)
        {
            if (latched || LatchedCount >= MaxLatched || player == null || player.IsDead) return;
            latched = true;
            LatchedCount++;
            latchEntry = e;
            current = null;
            ClearMarker();
            latchTimer = e.specialDuration > 0f ? e.specialDuration : 6f;
            latchTick = 1f;
            latchStart = Time.time;
            latchSlow = LatchSlow;
            player.Status.ExternalSlow += latchSlow;
            float side = Random.value < 0.5f ? -1f : 1f;
            latchOffset = new Vector3(side * Random.Range(0.18f, 0.32f), Random.Range(0.75f, 1.05f), -Random.Range(0.15f, 0.3f));
            cc.enabled = false;
            CurrentState = State.Latched;
            MarkState(0f);
            CombatEvents.RaiseWorldText(player.AimPoint + Vector3.up * 0.6f, "WGRYZIONY", Names.StatusColor(StatusKind.Bleed));
            Sfx.Play(SoundId.HitFleshHeavy, player.AimPoint, 0.8f, 1.3f);
        }

        void TickLatch(float dt)
        {
            if (player == null || player.IsDead) { ReleaseLatch(false); EnterMoving(); return; }
            var pt = player.transform;
            transform.position = pt.TransformPoint(latchOffset);
            Vector3 look = pt.position + Vector3.up * latchOffset.y - transform.position; look.y = 0f;
            if (look.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(look);

            // Unik strząsa czerwia (każdy unik zaczęty po uczepieniu).
            if (player.TimeSinceDodge < Time.time - latchStart)
            {
                ReleaseLatch(true);
                CombatEvents.RaiseWorldText(AimPoint + Vector3.up * 0.4f, "STRZĄŚNIĘTY", new Color(1f, 0.9f, 0.5f));
                CurrentState = State.Staggered;
                stateTimer = 0.9f;
                MarkState(0.9f);
                return;
            }

            latchTimer -= dt;
            latchTick -= dt;
            if (latchTick <= 0f)
            {
                latchTick += 1f;
                var a = latchEntry.attack;
                var bite = HitData.FromAttack(a, a.baseDamage * DamageMult * Mathf.Max(0.05f, latchEntry.specialShare), transform.position, this);
                bite.poiseDamage = 0f; bite.guardLoad = 0f;
                bite.blockable = false; bite.parryable = false; bite.dodgeable = false;
                bite.attackName = "Wgryzienie";
                HitQuery.Apply(this, player, bite, false);
            }
            if (latchTimer <= 0f) { ReleaseLatch(false); EnterMoving(); }
        }

        /// <summary>Puszcza bohatera: ląduje obok niego (strząśnięty – odrzucony dalej). Stan ustawia wywołujący.</summary>
        void ReleaseLatch(bool shaken)
        {
            if (!latched) return;
            latched = false;
            LatchedCount = Mathf.Max(0, LatchedCount - 1);
            if (player != null)
            {
                player.Status.ExternalSlow = Mathf.Max(0f, player.Status.ExternalSlow - latchSlow);
                Vector3 away = transform.position - player.transform.position; away.y = 0f;
                if (away.sqrMagnitude < 1e-4f) away = -player.transform.forward;
                Vector3 p = player.transform.position + away.normalized * (shaken ? 1.6f : 0.9f);
                p.y = player.transform.position.y + 0.05f;
                transform.position = p;
                if (shaken) FxLibrary.Dust(p, 0.8f);
            }
            if (cc != null) cc.enabled = CurrentState != State.Dead;
        }

        /// <summary>Tylko testy: zeruje licznik uczepionych (po zniszczeniu sceny).</summary>
        public static void ResetLatchCount() => LatchedCount = 0;
    }
}
