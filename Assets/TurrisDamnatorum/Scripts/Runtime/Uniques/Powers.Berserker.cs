using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    // ====================================================================== B01 Kolczuga głodnych blizn

    /// <summary>
    /// W transie Czerwonej linii prawdziwy wrogi cios wręcz zredukowany przez Żelazną skórę oddaje napastnikowi krwawy odwet
    /// (normalne maks. PŻ × udział × procent redukcji), z odstępem. Własne podstawowe ataki słabsze. Koszt życia/tyknięcia – bez odwetu.
    /// </summary>
    public sealed class HungryScarsPower : UniquePower
    {
        float ready;

        public override void Outgoing(ref HitData hit)
        {
            if (!hit.basicAttack || hit.featureEffect) return;
            float k = P("basic_mult");
            hit.physical *= k; hit.magic *= k;
        }

        public override void Struck(in HitData hit, in HitResult result, float featureMultiplier)
        {
            float reduction = 1f - featureMultiplier;
            if (reduction <= 0.001f || hit.isProjectile || hit.isStatusTick || hit.isReaction || !(hit.attacker is EnemyBrain e) || e.IsDead) return;
            if (pc.Clock < ready) { Count("odwet w odnowieniu"); return; }
            ready = pc.Clock + P("retaliate_cooldown");
            float dmg = pc.Health.Max * P("retaliate_share") * reduction;
            var h = PowerHit("Krwawy odwet", dmg, 0f, Element.None, pc.transform.position);
            h.blockable = false; h.parryable = false;
            Apply(e, h, true);
            FxLibrary.Blood(e.AimPoint, (e.transform.position - pc.transform.position).normalized, 1.5f);
            Count("odwety"); Count("obrażenia odwetu", dmg);
        }
    }

    // ====================================================================== B02 Różaniec gniewnych duchów

    /// <summary>Żetony Spalenia furii stają się widmami wojowników atakującymi zapisanym profilem broni (bez opłat skilli, bez legend).</summary>
    public sealed class AngrySpiritsPower : UniquePower
    {
        sealed class Spirit { public GameObject go; public float life, timer; public HitData hit; public Vector3 pos; }
        readonly List<Spirit> spirits = new List<Spirit>();
        public int Alive => spirits.Count;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "fury.tokens" || amount < 1f) return false;
            int room = Mathf.Max(0, PI("max_spirits") - spirits.Count);
            int n = Mathf.Min(room, Mathf.RoundToInt(amount));
            var hit = pc.UniqueWeaponHit(pc.Build.weapon.light, P("spirit_share"), false); // jeden zapisany profil broni
            hit.attackName = "Gniewne widmo";
            for (int i = 0; i < n; i++)
            {
                float a = i * 120f + Random.Range(-20f, 20f);
                Vector3 pos = pc.transform.position + Quaternion.Euler(0, a, 0) * Vector3.forward * 1.5f;
                var s = new Spirit { pos = pos, life = P("spirit_seconds"), timer = 0.3f * i, hit = hit };
                if (Application.isPlaying)
                {
                    s.go = new GameObject("GniewneWidmo");
                    s.go.transform.position = pos;
                    var cv = s.go.AddComponent<CharacterVisual>();
                    cv.BuildProcedural(new RigLook { style = RigStyle.Knight, head = HeadGear.Skull, body = BodyGear.Rags, weapon = AnimResolve.ForItem(pc.Build.mainHand.definition) }, 0.95f);
                    cv.Init(s.go.AddComponent<UniqueAnimSource>());
                    cv.SetTint(new Color(1f, 0.3f, 0.15f), 0.65f);
                    HellFx.Ambient(s.go.transform, Vector3.up * 0.9f, HellFxElement.Enemy, 0.4f, s.life);
                }
                spirits.Add(s);
            }
            if (amount > n) Count("żetony bez miejsca na widmo", amount - n);
            Count("widma", n);
            return true;
        }

        public override void Tick(float dt)
        {
            for (int i = spirits.Count - 1; i >= 0; i--)
            {
                var s = spirits[i];
                s.life -= dt; s.timer -= dt;
                if (s.life <= 0f || pc.IsDead) { Kill(i); continue; }
                var prey = HuWorld.Nearest(s.pos, 9f);
                if (prey != null)
                {
                    Vector3 to = prey.transform.position - s.pos; to.y = 0f;
                    float d = to.magnitude;
                    if (d > 1.6f) s.pos += to / d * Mathf.Min(d - 1.6f, 7f * dt);
                    else if (s.timer <= 0f)
                    {
                        s.timer = P("spirit_interval");
                        var h = s.hit; h.sourcePosition = s.pos;
                        Apply(prey, h, true);
                        FxLibrary.Sparks(prey.AimPoint, to.normalized, new Color(1f, 0.35f, 0.2f));
                        Count("ciosy widm");
                    }
                    if (s.go != null && d > 0.1f) s.go.transform.rotation = Quaternion.LookRotation(to);
                }
                if (s.go != null) s.go.transform.position = HuWorld.Ground(s.pos);
            }
        }

        void Kill(int i) { if (spirits[i].go != null) Object.Destroy(spirits[i].go); spirits.RemoveAt(i); }
        public override void Death() { for (int i = spirits.Count - 1; i >= 0; i--) Kill(i); }
        public override string Hud => spirits.Count > 0 ? $"widma: {spirits.Count}" : null;
    }

    // ====================================================================== B03 Topór kroczącej rzezi

    /// <summary>
    /// Rany płyną w tempie ruchu właściciela wokół ofiary (w zasięgu), nie ofiary. Suma rany bez zmian. Teleport i drgania
    /// (krok ponad limit na klatkę) się nie liczą.
    /// </summary>
    public sealed class WalkingSlaughterPower : UniquePower
    {
        Vector3 last; bool hasLast; float smoothed;
        public float OwnerSpeed => smoothed;

        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.MainHand) != Item ? "uśpiona: topór musi być w głównej ręce" : null;

        protected override void OnBind() { EnemyBrain.BleedRateProvider = Rate; }

        float Rate(EnemyBrain e)
        {
            if (pc == null || !Active || e == null) return -1f;
            bool near = HuWorld.Flat(e.transform.position, pc.transform.position) <= P("range");
            return near && smoothed > 1.2f ? P("moving_mult") : P("standing_mult");
        }

        public override void Tick(float dt)
        {
            if (dt <= 0f) return;
            Vector3 now = pc.transform.position;
            if (!hasLast) { last = now; hasLast = true; return; }
            float step = HuWorld.Flat(now, last);
            last = now;
            if (step > P("max_step")) { Count("skoki pominięte"); step = 0f; } // teleport/przeskok
            float speed = step / dt;
            smoothed = Mathf.Lerp(smoothed, speed, 1f - Mathf.Exp(-dt / 0.25f));
        }

        public override void Dispose() { if (EnemyBrain.BleedRateProvider == (System.Func<EnemyBrain, float>)Rate) EnemyBrain.BleedRateProvider = null; base.Dispose(); }
        public override void FloorStart() { hasLast = false; smoothed = 0f; EnemyBrain.BleedRateProvider = Rate; }
    }

    // ====================================================================== B04 Karwasze przykutego rzeźnika

    /// <summary>
    /// Ciężki rzut: broń zatrzymuje się na pierwszej ofierze. Opłacony mocny atak pięścią przyciąga gracza do wbitej broni;
    /// po dojściu broń wraca do ręki i kończy sekwencję ciężkim ciosem. Śmierć ofiary – zwrot bez kończącego trafienia.
    /// </summary>
    public sealed class ChainedButcherPower : UniquePower
    {
        ThrownWeapon anchored;
        bool pulling;
        public bool Anchored => anchored != null && anchored.Anchored != null;
        protected override string EquipmentDormant() => NeedSkill("skill_throw");

        public override void CastFired(SpellRuntime sk, int origin)
        {
            if (!IsSkill(sk, "skill_throw")) return;
            foreach (var tw in ThrownWeapon.Active)
                if (tw != null && tw.Owner == pc && !tw.Returning) { tw.AnchorOnHit = true; tw.AnchorSeconds = P("anchor_seconds"); anchored = tw; Count("rzuty z łańcuchem"); }
        }

        public override bool InterceptAttack(ActionType type)
        {
            if (type != ActionType.HeavyAttack || pulling || anchored == null || anchored.Anchored == null || !pc.WeaponThrown) return false;
            var victim = anchored.Anchored;
            float dist = HuWorld.Flat(victim.transform.position, pc.transform.position);
            if (dist > P("max_pull")) { Fail("za daleko", "Łańcuch: broń za daleko"); return false; }
            var fist = pc.Config.unarmed != null ? pc.Config.unarmed.weapon.heavy : pc.Build.weapon.heavy;
            if (pc.Stamina.Current <= 0f) { Fail("brak wytrzymałości", "Łańcuch: brak wytrzymałości"); return true; }
            pc.Stamina.Drain(fist.staminaCost, pc.Config.balance.staminaRegenDelay); // mocny atak pięścią – zwykły koszt
            Vector3 dir = HuWorld.FlatDir(pc.transform.position, victim.transform.position);
            float travel = Mathf.Max(0f, dist - 1.3f);
            float speed = P("pull_speed");
            pc.UniquePushSelf(dir, travel, travel / speed);
            pulling = true;
            float t = 0f, limit = travel / speed + 0.35f;
            var tw = anchored;
            HuWorld.Add(new UniqueRuntime(dt =>
            {
                t += dt;
                if (pc == null || pc.IsDead || tw == null) { pulling = false; return false; }
                var v = tw.Anchored;
                if (v == null || v.IsDead) { pulling = false; return false; } // broń już wraca sama
                if (HuWorld.Flat(v.transform.position, pc.transform.position) <= 1.8f)
                {
                    tw.Catch();
                    var h = pc.UniqueWeaponHit(pc.Build.weapon.heavy, P("finish_share"), false);
                    h.attackName = Card.name; h.heavy = true; h.featureEffect = false;
                    Apply(v, h, true);
                    pc.PlayFeatureSwing(true);
                    FxLibrary.Blood(v.AimPoint, dir, 2f);
                    Count("dociągnięcia");
                    pulling = false; return false;
                }
                if (t > limit) { Count("dociągnięcia przerwane"); pulling = false; return false; }
                return true;
            }));
            FxLibrary.Lightning(pc.AimPoint, victim.AimPoint, new Color(0.7f, 0.6f, 0.5f));
            Sfx.Play(SoundId.Chains, pc.AimPoint);
            return true;
        }

        public override void Death() { pulling = false; anchored = null; }
        public override string Hud => Anchored ? "broń wbita – mocny atak: przyciągnięcie" : null;
    }

    // ====================================================================== B05 Pierścień nierozładowanego szału

    /// <summary>
    /// Szał bez premii szybkości. Ładowany mocny atak odkłada kolejne cięcia (każde zużywa sekundy szału i koszt ataku); puszczenie
    /// wyzwala serię. Przerwanie przepala opłacone cięcia.
    /// </summary>
    public sealed class HeldRagePower : UniquePower
    {
        int cuts; AttackDefinition atk;
        public int Cuts => cuts;
        public override bool SuppressesRageSpeed => true;

        public override void ChargeTick(float chargeSeconds, float dt)
        {
            if (!pc.Raging) return;
            int want = Mathf.Min(PI("max_cuts"), Mathf.FloorToInt(chargeSeconds / Mathf.Max(0.1f, P("cut_charge"))));
            var heavy = pc.CurrentAttackDef ?? pc.Build.weapon.heavy;
            while (cuts < want)
            {
                if (pc.Stamina.Current <= 0f) { Fail("brak wytrzymałości", null); return; }
                if (!pc.ConsumeRage(P("rage_per_cut"))) { Fail("brak szału", null); return; }
                pc.Stamina.Drain(heavy.staminaCost, pc.Config.balance.staminaRegenDelay);
                cuts++; atk = heavy;
                CombatEvents.RaiseWorldText(pc.AimPoint + Vector3.up * 1.1f, $"+{cuts}", new Color(1f, 0.3f, 0.15f));
                Count("odłożone cięcia"); Count("sekundy szału w cięciach", P("rage_per_cut"));
            }
        }

        public override void AttackActive(ActionType type, AttackDefinition a, int origin)
        {
            if (type != ActionType.HeavyAttack || cuts <= 0) return;
            int n = cuts; cuts = 0;
            var attack = atk ?? a;
            for (int i = 1; i <= n; i++)
            {
                HuWorld.After(0.16f * i, () =>
                {
                    if (pc == null || pc.IsDead) return;
                    var h = pc.UniqueWeaponHit(attack, P("cut_share"), false);
                    h.attackName = "Seria szału"; h.heavy = true;
                    Vector3 o = pc.transform.position, fwd = pc.transform.forward;
                    foreach (var e in EnemyBrain.All.ToArray())
                        if (e != null && !e.IsDead && UniqueGeo.InCone(o, fwd, e.transform.position, attack.reach + 0.6f, 140f)) Apply(e, h, true);
                    HellFx.Cone(o + Vector3.up * 1.1f, fwd, HellFxElement.Enemy, attack.reach + 0.6f, 140f);
                    pc.PlayFeatureSwing(true);
                });
            }
            Count("serie"); Count("cięcia wyzwolone", n);
        }

        public override void ForcedInterrupt() { if (cuts > 0) { Count("cięcia przepalone (przerwanie)", cuts); cuts = 0; } }
        public override void Death() { cuts = 0; }
        public override string Hud => cuts > 0 ? $"cięcia szału: {cuts}" : null;
    }
}
