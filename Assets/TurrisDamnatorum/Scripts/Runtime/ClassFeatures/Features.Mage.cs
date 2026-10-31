using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    // ====================================================================== Mag A – Dług arkanów

    /// <summary>
    /// Dług trzymany osobno od widocznej many (<see cref="RunState.manaDebt"/>): nie zeruje go setter, przeliczenie buildu,
    /// regeneracja ani zapis. W oknie brak many pokrywa dług do twardego limitu (bez częściowej opłaty). Każde odnowienie many
    /// (regeneracja, trafienia, zabójstwa, parowanie, flaszki, odnowienie piętra) najpierw spłaca dług – filtr puli many.
    /// Po oknie dług blokuje czary aż do spłaty.
    /// </summary>
    public sealed class ArcaneDebtFeature : ClassFeatureController
    {
        float active;
        public bool Active => active > 0f;
        public float Debt => Run != null ? Run.manaDebt : 0f;
        public float Limit => pc.Mana.Max * T.debtLimit;
        protected override void OnBind() { color = new Color(0.65f, 0.45f, 1f); pc.Mana.RestoreFilter = Repay; }

        float Repay(float amount)
        {
            if (Run == null || Run.manaDebt <= 0f) return amount;
            float pay = Mathf.Min(Run.manaDebt, amount);
            Run.manaDebt -= pay;
            Telemetry.Add("dług: spłacono", pay);
            if (Run.manaDebt <= 0.001f) { Run.manaDebt = 0f; if (!Active) CombatEvents.RaiseMessage("Dług spłacony", color); }
            return amount - pay;
        }

        protected override void OnPress()
        {
            if (Active) { Fail("już działa", "Dług arkanów już trwa"); return; }
            active = T.debtSeconds;
            Used(T.debtCooldown, "DŁUG ARKANÓW");
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Soul, 0.5f, T.debtSeconds);
        }

        public override string BlockCast(SpellRuntime skill, int slot)
        {
            if (skill == null || !skill.Def.IsSpell || Active || Debt <= 0f) return null;
            Telemetry.Reject("dług blokuje czary");
            return $"Dług arkanów: najpierw spłać {Debt:0} many";
        }

        public override bool TryPay(SpellRuntime skill, int slot, out CastReceipt receipt)
        {
            receipt = default;
            if (!Active || skill == null || !skill.Def.IsSpell || skill.manaCost <= 0f) return false;
            float cost = skill.manaCost;
            if (pc.Mana.Current + 0.01f >= cost) return false; // zwykła płatność
            float need = cost - pc.Mana.Current;
            if (Debt + need > Limit + 0.01f)
            {
                receipt.active = true; // odmowa bez częściowej opłaty
                Fail("limit długu", $"Dług arkanów: limit {Limit:0} many");
                return false;
            }
            float mana = pc.Mana.Current;
            pc.Mana.Drain(mana);
            Run.manaDebt += need;
            Telemetry.Add("dług: zaciągnięto", need);
            Telemetry.Max("dług: największy", Run.manaDebt);
            receipt = new CastReceipt { mana = mana, debt = need, active = true };
            return true;
        }

        public override void Refund(in CastReceipt receipt)
        {
            // Zwrot najwyżej tego, co pobrano: najpierw znika dług tego rzutu, potem wraca pobrana mana (filtr spłaca starszy dług).
            Run.manaDebt = Mathf.Max(0f, Run.manaDebt - receipt.debt);
            if (receipt.mana > 0f) pc.Mana.Restore(receipt.mana);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (active > 0f) { active -= dt; if (active <= 0f && Debt > 0f) CombatEvents.RaiseMessage($"Dług {Debt:0} – czary zablokowane do spłaty", color); }
            else if (Debt > 0f) Telemetry.Add("dług: czas blokady", dt);
        }

        public override void FloorStart() { base.FloorStart(); active = 0f; } // dług zostaje: rozlicza go odnowienie piętra (RestoreAll)
        public override void Death() { active = 0f; }
        public override void Dispose() { base.Dispose(); if (pc != null && pc.Mana != null && pc.Mana.RestoreFilter == (System.Func<float, float>)Repay) pc.Mana.RestoreFilter = null; }
        public override ClassFeatureHud Hud => Active ? Timed(ClassFeatureState.Active, $"dług {Debt:0}/{Limit:0} · {active:0.0} s", active, T.debtSeconds)
            : Debt > 0f ? new ClassFeatureHud { state = ClassFeatureState.Blocked, label = $"dług {Debt:0} – spłać", fill = 1f - Debt / Mathf.Max(1f, Limit), color = new Color(1f, 0.4f, 0.6f) }
            : ReadyOrCooldown();
    }

    // ====================================================================== Mag B – Płaszcz many

    /// <summary>
    /// Płaszcz przejmuje udział obrażeń po pełnej mitygacji i Arkanowej osłonie, a przed barierą: płaci maną po kursie, a gdy many
    /// brak – osłania tylko opłaconą część. Nie tworzy many i nie pożycza. Koszty życia nie są obrażeniami.
    /// </summary>
    public sealed class ManaCloakFeature : ClassFeatureController
    {
        float active;
        public bool Active => active > 0f;
        protected override void OnBind() { color = new Color(0.4f, 0.6f, 1f); }

        protected override void OnPress()
        {
            if (Active) { Fail("już działa", "Płaszcz many już trwa"); return; }
            active = T.cloakSeconds;
            Used(T.cloakCooldown, "PŁASZCZ MANY");
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Water, 0.6f, T.cloakSeconds);
        }

        public override float AbsorbIncoming(float damage, bool statusTick)
        {
            if (!Active || damage <= 0f) return damage;
            float want = damage * Mathf.Clamp01(T.cloakShare);
            float manaNeeded = want * T.cloakManaPerHealth;
            float paid = Mathf.Min(manaNeeded, pc.Mana.Current);
            float covered = paid / Mathf.Max(0.01f, T.cloakManaPerHealth);
            pc.Mana.Drain(paid);
            Telemetry.Add("płaszcz: mana na ochronę", paid);
            Telemetry.Add("płaszcz: osłonięte PŻ", covered);
            if (covered < want - 0.01f) Telemetry.Add("płaszcz: przepuszczone przy braku many", want - covered);
            return damage - covered;
        }

        public override void Tick(float dt) { base.Tick(dt); if (active > 0f) active -= dt; }
        public override void FloorStart() { base.FloorStart(); active = 0f; }
        public override void Death() { active = 0f; }
        public override ClassFeatureHud Hud => Active ? Timed(ClassFeatureState.Active, $"płaszcz {active:0.0} s", active, T.cloakSeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Mag ognia A – Żywy piec

    /// <summary>
    /// Własny płomień (koszt z normalnego maks. życia, po odporności na ogień i obronie) – Żar powstaje tylko z rzeczywiście utraconych
    /// PŻ od tego płomienia. Następny ręczny atak albo czar ofensywny wydaje Żar jako jeden budżet: dodatkowa porcja ognia na
    /// trafienie (pocisk salwy – część 1/N, cios/obszar – dwie porcje po połowie). Porcja to efekt cechy (bez kradzieży, bez Żaru).
    /// </summary>
    public sealed class LivingFurnaceFeature : ClassFeatureController
    {
        float active, emberLife;
        public float Ember { get; private set; }
        public float EmberCap => pc.Health.Max * T.furnaceEmberCap;
        HellParticles flame;
        int chargeOrigin, chargeCount = 1; float chargeBudget, chargeLeft, chargeTime;
        public bool Burning => active > 0f;
        protected override void OnBind() { color = new Color(1f, 0.45f, 0.1f); }

        protected override void OnPress()
        {
            if (Burning) { Fail("już działa", "Żywy piec już płonie"); return; }
            active = T.furnaceSeconds;
            Used(T.furnaceCooldown, pc.Status.BurnImmune ? "ŻYWY PIEC (odporność na ogień – brak paliwa)" : "ŻYWY PIEC");
            flame = HellFx.Ambient(pc.transform, Vector3.up * 0.9f, HellFxElement.Fire, 0.55f, T.furnaceSeconds);
            Sfx.Play(SoundId.FireCast, pc.AimPoint, 0.8f, 0.8f);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (active > 0f)
            {
                active -= dt;
                float lost = pc.SelfFire(pc.Health.Max * T.furnaceBurnPerSecond * dt);
                Telemetry.Add("ogień: koszt własnych PŻ", lost);
                float add = Mathf.Min(lost, Mathf.Max(0f, EmberCap - Ember));
                Ember += add;
                Telemetry.Add("ogień: Żar", add);
                if (lost > 0f) emberLife = T.furnaceEmberLife;
                if (active <= 0f) EndFlame();
            }
            else if (Ember > 0f) { emberLife -= dt; if (emberLife <= 0f) { Telemetry.Add("ogień: Żar zmarnowany", Ember); Ember = 0f; } }
            if (chargeLeft > 0f) { chargeTime -= dt; if (chargeTime <= 0f) chargeLeft = 0f; }
        }

        void EndFlame() { active = 0f; if (flame != null) { flame.StopAndRelease(); flame = null; } }

        void Arm(int origin, int count)
        {
            if (Ember <= 0f) return;
            chargeOrigin = origin; chargeCount = Mathf.Max(1, count);
            chargeBudget = chargeLeft = Ember * T.furnaceEmberToDamage;
            chargeTime = 1.5f;
            Telemetry.Add("ogień: wydany Żar", Ember);
            Ember = 0f;
        }

        public override void AttackStarted(ActionType type, AttackDefinition atk, int origin) => Arm(origin, atk != null && atk.delivery == AttackDelivery.Projectile ? Mathf.Max(1, atk.projectileCount) : 1);
        public override void BasicShot(ref HitData hit, int index, int count) { if (hit.originId == 0 || hit.originId == chargeOrigin) chargeCount = Mathf.Max(1, count); }
        public override void CastFired(SpellRuntime skill, int origin)
        {
            if (skill == null || !skill.Def.IsSpell || skill.Def.IsAura || skill.Def.kind == SpellKind.Summon || skill.Def.kind == SpellKind.Barrier || skill.Def.kind == SpellKind.Heal) return;
            Arm(origin, 1);
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (chargeLeft <= 0f || hit.originId != chargeOrigin || hit.featureEffect || hit.isStatusTick || hit.isGroundHazard || hit.isReaction) return;
            if (!(result.outcome == HitOutcome.Hit || result.outcome == HitOutcome.GuardBroken) || target.IsDead && result.healthDamage <= 0f) return;
            float share = hit.isProjectile ? 1f / chargeCount : 0.5f;
            float take = Mathf.Min(chargeLeft, chargeBudget * share);
            if (take <= 0.01f) return;
            chargeLeft -= take;
            var h = pc.FeatureHit("Żar", 0f, take, Element.Fire, pc.transform.position);
            h.isReaction = true; h.knockback = 0f;
            h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Burn) };
            if (!target.IsDead) HitQuery.Apply(pc, target, h, false);
            HellFx.Impact(target.AimPoint, HellFxElement.Fire, color, 0.8f);
            Telemetry.Add("ogień: obrażenia Żaru", take);
        }

        public override void FloorStart() { base.FloorStart(); EndFlame(); Ember = 0f; chargeLeft = 0f; }
        public override void Death() { EndFlame(); Ember = 0f; chargeLeft = 0f; }
        public override ClassFeatureHud Hud => Burning ? Timed(ClassFeatureState.Active, $"płomień {active:0.0} s · Żar {Ember:0}", active, T.furnaceSeconds)
            : Ember > 0f ? new ClassFeatureHud { state = ClassFeatureState.Stored, label = $"Żar {Ember:0}/{EmberCap:0} ({emberLife:0} s)", fill = Ember / Mathf.Max(1f, EmberCap), color = color }
            : ReadyOrCooldown();
    }

    // ====================================================================== Mag ognia B – Wielki pożar

    /// <summary>
    /// W oknie podpalenia gracza niosą klucz rodziny = źródło: grupa zwykłych ataków albo konkretny skill (nazwa definicji).
    /// Kopie, strefy i odpryski dziedziczą trafienie, więc i rodzinę. Każda rodzina ma swój czas i regułę mocniejszego odświeżenia.
    /// Po oknie każdy płonący wróg zachowuje tylko najsilniejszą rodzinę (bez przedłużenia).
    /// </summary>
    public sealed class GreatFireFeature : ClassFeatureController
    {
        float active;
        public bool Active => active > 0f;
        public const int WeaponFamily = 1;
        protected override void OnBind() { color = new Color(1f, 0.55f, 0.15f); }

        protected override void OnPress()
        {
            if (Active) { Fail("już działa", "Wielki pożar już trwa"); return; }
            active = T.greatFireSeconds;
            Used(T.greatFireCooldown, "WIELKI POŻAR");
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Fire, 0.5f, T.greatFireSeconds);
        }

        public static int FamilyOf(in HitData hit) => hit.basicAttack ? WeaponFamily : string.IsNullOrEmpty(hit.attackName) ? 2 : 3 + (hit.attackName.GetHashCode() & 0x3fffffff);

        public override void OutgoingLate(ref HitData hit)
        {
            if (!Active || hit.isStatusTick) return;
            hit.mods.burnFamily = FamilyOf(hit);
            hit.mods.burnFamilies = Mathf.Max(1, T.greatFireFamilies);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (active <= 0f) return;
            active -= dt;
            foreach (var e in EnemyBrain.All) if (e != null && !e.IsDead) Telemetry.Max("ogień: rodziny na jednym celu", e.Status.BurnFamilyCount);
            if (active <= 0f) Collapse();
        }

        void Collapse()
        {
            active = 0f;
            foreach (var e in EnemyBrain.All) if (e != null) e.Status.CollapseBurnFamilies();
        }

        public override void FloorStart() { base.FloorStart(); active = 0f; }
        public override void Death() { Collapse(); }
        public override ClassFeatureHud Hud => Active ? Timed(ClassFeatureState.Active, $"pożar {active:0.0} s", active, T.greatFireSeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Mag wody A – Niezmienny szron

    /// <summary>
    /// Znacznik Szronu na jednym legalnym celu: osobny warunek reakcji w <see cref="StatusEffects"/> (bez spowolnienia i kontroli),
    /// reakcje nie zużywają chłodu, wspólny odstęp rozliczenia na cel. Prawdziwe zamrożenie, ogłuszenie i odporności – bez zmian.
    /// </summary>
    public sealed class EnduringFrostFeature : ClassFeatureController
    {
        EnemyBrain target;
        int reactionsAtStart;
        HellParticles fx;
        public EnemyBrain Target => target != null && !target.IsDead && target.Status.FrostMarked ? target : null;
        protected override void OnBind() { color = new Color(0.6f, 0.9f, 1f); }

        protected override void OnPress()
        {
            var e = pc.FeatureTarget(T.frostRange);
            if (e == null) { Fail("brak celu", "Niezmienny szron: brak celu w zasięgu"); return; }
            Clear();
            target = e;
            reactionsAtStart = e.Status.FrostMarkReactions;
            e.Status.MarkFrost(T.frostSeconds, T.frostReactionInterval, T.frostMudBonus);
            fx = HellFx.Ambient(e.transform, Vector3.up * e.Def.scale, HellFxElement.Ice, 0.5f * e.Def.scale, T.frostSeconds);
            Used(T.frostCooldown, "NIEZMIENNY SZRON");
            Sfx.Play(SoundId.FrostCast, e.AimPoint, 0.8f);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (target == null) return;
            if (target.IsDead || !target.Status.FrostMarked) { Telemetry.Add("woda: reakcje Szronu", target.Status.FrostMarkReactions - reactionsAtStart); target = null; }
        }

        void Clear()
        {
            if (target != null && !target.IsDead) { Telemetry.Add("woda: reakcje Szronu", target.Status.FrostMarkReactions - reactionsAtStart); target.Status.ClearFrostMark(); }
            if (fx != null) { fx.StopAndRelease(); fx = null; }
            target = null;
        }

        public override void FloorStart() { base.FloorStart(); Clear(); }
        public override void Death() { Clear(); }
        public override ClassFeatureHud Hud => Target != null ? Timed(ClassFeatureState.Target, $"szron: {Target.DisplayName} {Target.Status.FrostMarkRemaining:0.0} s", Target.Status.FrostMarkRemaining, T.frostSeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Mag wody B – Prąd głębin

    /// <summary>
    /// Przesuwa do kilku własnych stref (DamageZone gracza) do punktu celowania – płynnie, z zachowaniem pozostałego czasu, rytmu
    /// tyknięć, pochodzenia i statusów; bez nowego rzutu i bez wybuchu początkowego. Opłata za faktycznie przesunięte pola; brak
    /// pola – bez opłaty. Ruch zatrzymuje się przed geometrią areny (bez przenikania przez ściany).
    /// </summary>
    public sealed class DeepCurrentFeature : ClassFeatureController
    {
        protected override void OnBind() { color = new Color(0.3f, 0.6f, 1f); }

        public List<DamageZone> OwnZones() => DamageZone.Active.Where(z => z != null && z.Relocatable && ReferenceEquals(z.Owner, pc) && z.Faction == Faction.Player
            && z.Remaining > 0.1f && HuWorld.Flat(z.transform.position, pc.transform.position) <= T.currentRange).ToList();

        protected override void OnPress()
        {
            var zones = OwnZones();
            if (zones.Count == 0) { Fail("brak pola", "Prąd głębin: brak własnego pola w zasięgu (np. Krąg żaru, Sidła)"); return; }
            var e = pc.FeatureTarget(T.currentRange);
            Vector3 point = e != null ? e.transform.position : pc.transform.position + pc.FeatureAim() * 6f;
            zones.Sort((a, b) => HuWorld.Flat(a.transform.position, point).CompareTo(HuWorld.Flat(b.transform.position, point)));
            int n = Mathf.Min(T.currentMaxZones, zones.Count);
            float cost = n * T.currentManaPerZone;
            if (pc.Mana.Current + 0.01f < cost) { Fail("brak many", $"Prąd głębin: potrzeba {cost:0} many"); return; }
            pc.Mana.TrySpend(cost);
            Vector3 side = Vector3.Cross(Vector3.up, pc.FeatureAim());
            for (int i = 0; i < n; i++)
            {
                var z = zones[i];
                Vector3 to = point + side * (i == 0 ? 0f : z.Radius * 1.1f * (i % 2 == 1 ? 1f : -1f));
                to = HuWorld.Clamp(z.transform.position, to, z.Radius * 0.5f);
                z.GlideTo(to, T.currentMoveSeconds);
                FxLibrary.Lightning(z.transform.position + Vector3.up * 0.3f, to + Vector3.up * 0.3f, new Color(0.4f, 0.7f, 1f));
            }
            Telemetry.Add("woda: przesunięte pola", n);
            Used(T.currentCooldown, $"PRĄD GŁĘBIN ({n})");
            Sfx.Play(SoundId.FrostCast, pc.AimPoint, 0.6f, 0.8f);
        }

        public override ClassFeatureHud Hud
        {
            get
            {
                if (Cooldown > 0f) return ReadyOrCooldown();
                int n = OwnZones().Count;
                return n == 0 ? new ClassFeatureHud { state = ClassFeatureState.Blocked, label = "brak własnego pola", color = color * 0.6f }
                    : new ClassFeatureHud { state = ClassFeatureState.Ready, label = $"pola w zasięgu: {n}", fill = 1f, color = color };
            }
        }
    }

    // ====================================================================== Mag powietrza A – Oko ciszy

    /// <summary>
    /// Krąg wokół maga przechwytuje zwykłe pociski wrogów (bez wybuchowych/obszarowych) i własne podstawowe strzały – do pojemności.
    /// Przechwycony pocisk znika ze świata (bez trafienia i niewidzialnej kolizji). Ponowne G albo koniec okna wypuszcza salwę
    /// gracza: własne pociski z zapłaconym trafieniem, wrogie – z budżetem gracza (najwyżej obrażenia oryginału).
    /// </summary>
    public sealed class SilentEyeFeature : ClassFeatureController
    {
        struct Stored { public HitData hit; public bool own; public Color color; public float speed; public bool arrow; }
        readonly List<Stored> stored = new List<Stored>();
        float open;
        HellParticles fx;
        public int Count => stored.Count;
        public bool Open => open > 0f;
        protected override bool AwaitingSecondPress => Open;
        protected override void OnBind() { color = new Color(0.75f, 0.85f, 1f); }

        protected override void OnPress()
        {
            if (Open) { Release(); return; }
            open = T.eyeSeconds;
            stored.Clear();
            fx = HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Air, T.eyeRadius * 0.8f, T.eyeSeconds);
            Used(T.eyeCooldown, "OKO CISZY");
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (!Open) return;
            open -= dt;
            if (stored.Count < T.eyeCapacity)
                foreach (var p in Projectile.Active.ToArray())
                {
                    if (stored.Count >= T.eyeCapacity) break;
                    if (p == null || p.Side != Faction.Enemy || !p.Capturable) continue;
                    if (Vector3.Distance(p.transform.position, pc.AimPoint) > T.eyeRadius) continue;
                    var h = p.Hit;
                    float budget = Mathf.Min(h.Total, pc.BasicDamage * T.eyeEnemyBudget);
                    var mine = pc.FeatureHit("Oddany pocisk", h.element == Element.None ? budget : 0f, h.element == Element.None ? 0f : budget, h.element, pc.transform.position);
                    stored.Add(new Stored { hit = mine, own = false, color = new Color(0.8f, 0.85f, 1f), speed = T.eyeReleaseSpeed, arrow = p.IsArrow });
                    p.Capture();
                    Telemetry.Add("powietrze: przechwycone wrogie pociski");
                    FxLibrary.Flash(pc.AimPoint + (p.transform.position - pc.AimPoint) * 0.5f, color, 0.5f);
                }
            if (open <= 0f) Release();
        }

        public override bool CaptureOwnShot(in HitData hit, Vector3 dir, float speed, Color c)
        {
            if (!Open || stored.Count >= T.eyeCapacity) return false;
            stored.Add(new Stored { hit = hit, own = true, color = c, speed = Mathf.Max(speed, T.eyeReleaseSpeed), arrow = hit.element == Element.None });
            Telemetry.Add("powietrze: własne pociski w oku");
            return true;
        }

        void Release()
        {
            open = 0f;
            if (fx != null) { fx.StopAndRelease(); fx = null; }
            if (stored.Count == 0 || pc.IsDead) { stored.Clear(); return; }
            Vector3 aim = pc.FeatureAim();
            var e = pc.FeatureTarget(25f);
            Vector3 from = pc.AimPoint + Vector3.up * 0.3f + aim * 0.8f;
            for (int i = 0; i < stored.Count; i++)
            {
                var s = stored[i];
                float ang = stored.Count == 1 ? 0f : Mathf.Lerp(-12f, 12f, i / (float)(stored.Count - 1));
                Vector3 dir = Quaternion.Euler(0, ang, 0) * (e != null ? (e.AimPoint - from) : aim);
                var h = s.hit; h.sourcePosition = from;
                Projectile.Spawn(from, dir, s.speed, h, Faction.Player, pc, s.color, 0.2f, 2.5f, e, e != null ? 70f : 0f, s.arrow);
            }
            Telemetry.Add("powietrze: wypuszczone pociski", stored.Count);
            CombatEvents.RaiseMessage($"SALWA OKA ({stored.Count})", color);
            Sfx.Play(SoundId.ArcaneBolt, pc.AimPoint, 0.9f, 1.2f);
            stored.Clear();
        }

        public override void FloorStart() { base.FloorStart(); open = 0f; stored.Clear(); if (fx != null) { fx.StopAndRelease(); fx = null; } }
        public override void Death() { open = 0f; stored.Clear(); if (fx != null) { fx.StopAndRelease(); fx = null; } }
        public override ClassFeatureHud Hud => Open ? Timed(ClassFeatureState.SecondPhase, $"oko {stored.Count}/{T.eyeCapacity} · {open:0.0} s", open, T.eyeSeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Mag powietrza B – Zderzenie frontów

    /// <summary>
    /// W oknie trafienia gracza z odrzutem niosą budżet wyładowania. Wróg reaguje na rzeczywistą kolizję w trakcie przesunięcia
    /// (ściana albo inne ciało – CollisionFlags), raz na atak i cel, z limitem zderzeń na atak. Cel odporny na przesuwanie
    /// (boss, pancerz w zamachu) dostaje od razu ograniczone naprężenie – jego odporność zostaje.
    /// </summary>
    public sealed class CollidingFrontsFeature : ClassFeatureController
    {
        float active;
        public bool Active => active > 0f;
        public int Discharges { get; private set; }
        public int Strains { get; private set; }
        protected override void OnBind()
        {
            color = new Color(0.7f, 0.8f, 1f);
            EnemyBrain.FrontsDischarge += OnDischarge;
        }

        void OnDischarge(EnemyBrain e, float dmg, bool strain)
        {
            if (strain) { Strains++; Telemetry.Add("powietrze: naprężenia"); }
            else { Discharges++; Telemetry.Add("powietrze: zderzenia"); }
        }

        protected override void OnPress()
        {
            if (Active) { Fail("już działa", "Zderzenie frontów już trwa"); return; }
            active = T.frontsSeconds;
            Used(T.frontsCooldown, "ZDERZENIE FRONTÓW");
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Air, 0.5f, T.frontsSeconds);
        }

        public override void Outgoing(ref HitData hit)
        {
            if (!Active || hit.featureEffect || hit.isReaction || hit.isStatusTick || hit.isGroundHazard) return;
            hit.frontsBudget = pc.BasicDamage * T.frontsDischarge;
        }

        public override void Tick(float dt) { base.Tick(dt); if (active > 0f) active -= dt; }
        public override void FloorStart() { base.FloorStart(); active = 0f; EnemyBrain.ResetFeatureStatics(); }
        public override void Death() { active = 0f; }
        public override void Dispose() { base.Dispose(); EnemyBrain.FrontsDischarge -= OnDischarge; }
        public override ClassFeatureHud Hud => Active ? Timed(ClassFeatureState.Active, $"fronty {active:0.0} s", active, T.frontsSeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Mag ziemi A – Bastion ziemi

    /// <summary>
    /// Ściana w poprzek celowania, po walidacji miejsca (granice, aktorzy, portale). Mocne trafienie gracza albo czar ziemi przy
    /// ścianie wywołuje pęknięcie wzdłuż niej (jeden budżet z tego trafienia), po którym ściana znika.
    /// </summary>
    public sealed class BastionFeature : ClassFeatureController
    {
        public EarthWall Wall => EarthWall.Active.Count > 0 ? EarthWall.Active[0] : null;
        protected override void OnBind() { color = new Color(0.75f, 0.6f, 0.4f); }

        protected override void OnPress()
        {
            Vector3 aim = pc.FeatureAim();
            Vector3 center = pc.transform.position + aim * T.wallDistance;
            var rot = Quaternion.LookRotation(aim);
            string why = EarthWall.Validate(center, rot, T.wallLength, T.wallHeight, T.wallThickness);
            if (why != null) { Fail("brak miejsca", "Bastion ziemi: " + why.Substring(0, 1).ToLower() + why.Substring(1)); return; }
            EarthWall.Spawn(pc, center, rot, T);
            Telemetry.Add("ziemia: ściany");
            Used(T.wallCooldown, "BASTION ZIEMI");
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            var w = Wall;
            if (w == null || hit.featureEffect || hit.isStatusTick || hit.isGroundHazard) return;
            if (!(result.outcome == HitOutcome.Hit || result.outcome == HitOutcome.GuardBroken || result.outcome == HitOutcome.Blocked)) return;
            if (!(hit.heavy || hit.Has(Element.Earth))) return;
            w.TryCrack(hit, target.Transform.position);
        }

        public override void FloorStart() { base.FloorStart(); EarthWall.ClearAll(); }
        public override void Death() { EarthWall.ClearAll(); }
        public override ClassFeatureHud Hud => Wall != null ? Timed(ClassFeatureState.Active, $"ściana {Wall.Remaining:0.0} s · {Wall.Durability:0}", Wall.Remaining, T.wallSeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Mag ziemi B – Krusząca skorupa

    /// <summary>
    /// Zużywa w jednej transakcji niewydaną Arkanową osłonę i barierę, dzieląc ich sumę na płyty (limit; nadmiar nie jest magazynem).
    /// Osłona zaczyna odrastać od zwykłego opóźnienia. Każdy mocny atak (cios albo strzał) wypuszcza płytę w cel – efekt cechy
    /// ze skalowaniem żywiołu ziemi raz w potoku, bez barier i kradzieży życia. Zmiana wyposażenia nie zmienia zaksięgowanych płyt.
    /// </summary>
    public sealed class ShatteringShellFeature : ClassFeatureController
    {
        readonly List<float> plates = new List<float>();
        float life;
        public int Plates => plates.Count;
        public float PlateBudget => plates.Sum();
        protected override void OnBind() { color = new Color(0.7f, 0.55f, 0.35f); }

        protected override void OnPress()
        {
            float ward = pc.Ward, barrier = pc.BarrierAmount;
            if (ward + barrier < 1f) { Fail("brak osłony", "Krusząca skorupa: brak osłony do zużycia"); return; }
            int free = T.shellMaxPlates - plates.Count;
            if (free <= 0) { Fail("pełny magazyn", "Krusząca skorupa: wszystkie płyty gotowe"); return; }
            float total = pc.ConsumeWard() + pc.ConsumeBarrier();
            for (int i = 0; i < free; i++) plates.Add(total / free);
            life = T.shellPlateLife;
            Telemetry.Add("ziemia: zużyta ochrona", total);
            Used(T.shellCooldown, $"SKORUPA: {free} płyty");
            HellFx.Shockwave(pc.transform.position, HellFxElement.Earth, color, 1.6f, true);
            Sfx.Play(SoundId.Shatter, pc.AimPoint, 0.7f, 0.9f);
        }

        public override void AttackActive(ActionType type, AttackDefinition atk, int origin)
        {
            if (type != ActionType.HeavyAttack || plates.Count == 0) return;
            float budget = plates[0];
            plates.RemoveAt(0);
            Vector3 aim = pc.FeatureAim();
            var e = pc.FeatureTarget(18f);
            var h = pc.FeatureHit("Kamienna płyta", 0f, budget * T.shellPlateFactor, Element.Earth, pc.transform.position);
            h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Stagger) };
            h.poiseDamage = 40f; h.heavy = true;
            Vector3 from = pc.AimPoint + aim * 0.9f;
            pc.FeatureProjectile(from, e != null ? e.AimPoint - from : aim, 20f, h, new Color(0.55f, 0.45f, 0.35f), 0.35f, e);
            Telemetry.Add("ziemia: wydane płyty");
            Telemetry.Add("ziemia: budżet płyt", budget);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (plates.Count > 0) { life -= dt; if (life <= 0f) { Telemetry.Add("ziemia: płyty zmarnowane", plates.Count); plates.Clear(); } }
        }

        public override void FloorStart() { base.FloorStart(); plates.Clear(); }
        public override void Death() { plates.Clear(); }
        public override ClassFeatureHud Hud => plates.Count > 0 ? Timed(ClassFeatureState.Stored, $"płyty {plates.Count} ({PlateBudget:0})", life, T.shellPlateLife)
            : pc.Ward + pc.BarrierAmount >= 1f ? ReadyOrCooldown($"osłona {pc.Ward + pc.BarrierAmount:0}") : new ClassFeatureHud { state = ClassFeatureState.Blocked, label = "brak osłony", color = color * 0.6f };
    }
}
