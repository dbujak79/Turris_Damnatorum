using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    // ====================================================================== R01 Rękawice podwójnego sekretu

    /// <summary>
    /// Różdżka w lewej ręce (wyjątek slotu w RunState). W oknie Niezgodnej pary splot to cięcie główną bronią i krótki wystrzał
    /// różdżki za manę. Bez many – samo cięcie (komunikat), lewa ręka nie daje drugiego ostrza.
    /// </summary>
    public sealed class DoubleSecretPower : UniquePower
    {
        protected override string EquipmentDormant() => pc.Build.offWand == null ? "różdżka w drugiej ręce jeszcze niezałożona" : null;
        // Rękawice działają (pozwalają założyć różdżkę) zawsze; splot – tylko z Niezgodną parą.
        public override string Dormant() => base.Dormant();

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "pair.wand" || !(data is IHitReceiver target) || pc.Build.offWand == null) return false;
            float mana = P("shot_mana");
            if (pc.Mana.Current + 0.01f < mana) { Fail("brak many", "Splot: za mało many na wystrzał"); return true; }
            pc.Mana.TrySpend(mana);
            var wand = pc.Build.offWand.definition;
            var h = pc.UniqueWeaponHit(wand.weapon.light, P("shot_share"), false);
            h.attackName = "Splot – wystrzał"; h.fromMeleeWeapon = false; h.isProjectile = true;
            Vector3 from = pc.AimPoint + pc.transform.forward * 0.4f;
            pc.FeatureProjectile(from, target.AimPoint - from, wand.weapon.light.projectileSpeed, h, Names.ElementColor(wand.weapon.light.element), 0.16f, target);
            Count("wystrzały splotu"); Count("mana splotu", mana);
            return true;
        }
    }

    // ====================================================================== R02 Buty niedokończonego kroku

    /// <summary>Unik z przeniesionym ciosem (Taniec) zostawia ślad części cięcia w miejscu startu; mocny atak uwalnia wszystkie ślady.</summary>
    public sealed class UnfinishedStepPower : UniquePower
    {
        sealed class Trace { public Vector3 at; public AttackDefinition atk; public bool offhand; public int origin; public float until; public GameObject mark; }
        readonly List<Trace> traces = new List<Trace>();
        public int Traces => traces.Count;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "dance.carried" || !(data is DanceCarry c)) return false;
            float share = P("trace_share");
            amount *= 1f - share; // bieżący cios słabszy o odłożoną część
            while (traces.Count >= PI("max_traces")) Remove(0);
            var t = new Trace { at = pc.transform.position, atk = c.attack, offhand = c.offhand, origin = c.origin, until = pc.Clock + P("trace_seconds") };
            if (FxLibrary.Enabled) { var d = FxLibrary.Decal(HuWorld.Ground(t.at), HellVfxMaterials.Ground(HellFxElement.Physical, false), new Color(0.9f, 0.35f, 0.35f), 1.4f, 1.4f, P("trace_seconds")); t.mark = d != null ? d.gameObject : null; }
            traces.Add(t);
            Count("ślady");
            return false; // Taniec przenosi cios dalej (zmniejszony)
        }

        public override void AttackActive(ActionType type, AttackDefinition atk, int origin)
        {
            if (type != ActionType.HeavyAttack || traces.Count == 0) return;
            foreach (var t in traces)
            {
                var h = pc.UniqueWeaponHit(t.atk, P("trace_share"), t.offhand);
                h.attackName = "Niedokończony krok"; h.originId = t.origin;
                int n = HuWorld.HitCircle(pc, t.at, P("trace_radius"), h);
                HellFx.Burst(t.at + Vector3.up * 0.6f, HellFxElement.Physical, P("trace_radius"));
                Count("trafienia śladów", n);
            }
            Count("uwolnienia");
            for (int i = traces.Count - 1; i >= 0; i--) Remove(i);
        }

        void Remove(int i) { if (traces[i].mark != null) Object.Destroy(traces[i].mark); traces.RemoveAt(i); }

        public override void Tick(float dt)
        {
            for (int i = traces.Count - 1; i >= 0; i--) if (pc.Clock > traces[i].until) { Count("ślady wygasłe"); Remove(i); }
        }

        public override void Death() { for (int i = traces.Count - 1; i >= 0; i--) Remove(i); }
        public override string Hud => traces.Count > 0 ? $"ślady kroku: {traces.Count}" : null;
    }

    // ====================================================================== R03 Pas żonglera ostrzami

    /// <summary>Mocny atak wysyła lewą broń na orbitę: rani wrogów na okręgu, w tym czasie nie atakuje w ręce. Wraca raz.</summary>
    public sealed class BladeJugglerPower : UniquePower
    {
        UniqueRuntime orbit;
        public bool Orbiting => orbit != null && !orbit.Done;
        public override bool HoldsOffhand => Orbiting;

        protected override string EquipmentDormant()
        {
            var off = Run.equipment.Get(EquipSlot.OffHand);
            if (off == null || !off.definition.IsOneHandedMelee) return off != null && off.definition.IsWeapon ? "nieaktywny: różdżka nie jest ostrzem do rzutu" : "uśpiona: brak jednoręcznej broni wręcz w drugiej ręce";
            return null;
        }

        public override void AttackActive(ActionType type, AttackDefinition atk, int origin)
        {
            if (type != ActionType.HeavyAttack || Orbiting) return;
            var off = Run.equipment.Get(EquipSlot.OffHand);
            if (off == null || !off.definition.IsOneHandedMelee) return;
            var hit = pc.UniqueWeaponHit(off.definition.weapon.light, P("orbit_share"), true);
            hit.attackName = "Orbita ostrza"; hit.weapon = AnimResolve.ForItem(off.definition);
            float t = 0f, seconds = P("orbit_seconds"), radius = P("orbit_radius"), tick = P("tick");
            var last = new Dictionary<EnemyBrain, float>();
            GameObject blade = null;
            if (Application.isPlaying)
            {
                blade = new GameObject("OrbitaOstrza");
                var w = GearBuilder.Weapon(blade.transform, AnimResolve.ForItem(off.definition), off.definition.color);
                if (w != null) w.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Projectile.AddTrail(blade, new Color(0.9f, 0.85f, 0.8f), 0.25f, 0.25f);
            }
            orbit = new UniqueRuntime(dt =>
            {
                if (pc == null || pc.IsDead) return false;
                t += dt;
                float ang = t * 300f * Mathf.Deg2Rad;
                Vector3 pos = pc.transform.position + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * radius + Vector3.up;
                if (blade != null) { blade.transform.position = pos; blade.transform.Rotate(Vector3.up, 900f * dt, Space.World); }
                foreach (var e in EnemyBrain.All.ToArray())
                {
                    if (e == null || e.IsDead || Vector3.Distance(e.AimPoint, pos) > 1f) continue;
                    if (last.TryGetValue(e, out float w2) && t - w2 < tick) continue;
                    last[e] = t;
                    var h = hit; h.sourcePosition = pos;
                    Apply(e, h, true);
                }
                return t < seconds;
            }, () => { if (blade != null) Object.Destroy(blade); });
            HuWorld.Add(orbit);
            Count("orbity");
        }

        public override void Death() { orbit?.Kill(); orbit = null; }
        public override string Hud => Orbiting ? "lewe ostrze na orbicie" : null;
    }

    // ====================================================================== R04 Szabla wspak pisanych ran

    /// <summary>Cięcie krzyżowe: pierwszy etap teraz, drugi czeka na następne ręczne trafienie lewą ręką (z jej profilem).</summary>
    public sealed class ReverseWoundsPower : UniquePower
    {
        float until = -1f; float power = 1f;
        public bool Waiting => until > pc.Clock;

        protected override string EquipmentDormant()
        {
            if (Run.equipment.Get(EquipSlot.MainHand) != Item && Run.equipment.Get(EquipSlot.OffHand) != Item) return "uśpiona: szabla musi być w ręce";
            if (pc.Build.offWeapon == null) return "uśpiona: potrzebne dwie bronie wręcz";
            return NeedSkill("hu_cross_cut");
        }

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (cast.copy || cast.relayed || !IsSkill(cast.spell, "hu_cross_cut")) return false;
            var open = cast.Variant(cast.origin, cast.direction, cast.target, P("open_share"));
            pc.UniqueFireHuCast(open, aim, range, pc.ActionOrigin);
            pc.HuSwing(true);
            until = pc.Clock + P("answer_seconds"); power = cast.power;
            Count("otwarcia");
            return true;
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (!Waiting || !hit.basicAttack || hit.featureEffect || !pc.AttackIsOffhand) return;
            if (result.outcome != HitOutcome.Hit && result.outcome != HitOutcome.GuardBroken) return;
            if (target == null || target.IsDead || pc.Build.offWeapon == null) return;
            until = -1f;
            var off = pc.Build.offWeapon;
            var h = pc.UniqueWeaponHit(off.definition.weapon.light, P("answer_share") * power, true);
            h.attackName = "Odpowiedź wspak"; h.weapon = AnimResolve.ForItem(off.definition);
            Apply(target, h, true);
            FxLibrary.Blood(target.AimPoint, pc.transform.right, 1.6f);
            CombatEvents.RaiseWorldText(target.AimPoint + Vector3.up * 0.5f, "WSPAK", new Color(0.95f, 0.4f, 0.4f));
            Count("domknięcia");
        }

        public override void Tick(float dt) { if (until > 0f && pc.Clock > until) { until = -1f; Count("okna wygasłe"); } }
        public override void Death() { until = -1f; }
        public override string Hud => Waiting ? $"odpowiedź lewą {until - pc.Clock:0.0} s" : null;
    }

    // ====================================================================== R05 Amulet zbłąkanej klingi

    /// <summary>
    /// Młynek zostawia główną broń wirującą w punkcie startu (broń poza ręką – lewa albo pięści walczą dalej). Opłacony mocny atak
    /// przywołuje wir po ziemi do właściciela; czas i trafienia się nie odnawiają.
    /// </summary>
    public sealed class WanderingBladePower : UniquePower
    {
        UniqueRuntime vortex;
        Vector3 at; bool recalling; float left;
        public bool Spinning => vortex != null && !vortex.Done;
        public override bool OffhandFightsAlone => Spinning;
        public Vector3 VortexPoint => at;

        protected override string EquipmentDormant()
        {
            var main = Run.equipment.Get(EquipSlot.MainHand);
            if (main == null || !main.definition.IsWeapon || main.definition.weapon.light.delivery != AttackDelivery.Melee) return "uśpiona: brak broni wręcz w głównej ręce";
            return NeedSkill("hu_whirlwind");
        }

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (cast.copy || cast.relayed || !IsSkill(cast.spell, "hu_whirlwind") || Spinning || pc.WeaponThrown) return false;
            var main = Run.equipment.Get(EquipSlot.MainHand);
            var hit = pc.UniqueWeaponHit(main.definition.weapon.light, P("spin_share"), false);
            hit.attackName = "Zbłąkana klinga";
            at = cast.origin; left = P("vortex_seconds"); recalling = false;
            pc.ForceWeaponThrown(true);
            float tick = P("tick"), radius = P("radius"), speed = P("recall_speed");
            var last = new Dictionary<EnemyBrain, float>();
            float t = 0f;
            GameObject blade = null;
            if (Application.isPlaying)
            {
                blade = new GameObject("ZblakanaKlinga");
                var w = GearBuilder.Weapon(blade.transform, AnimResolve.ForItem(main.definition), main.definition.color);
                if (w != null) w.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                HellFx.Ambient(blade.transform, Vector3.zero, HellFxElement.Physical, radius * 0.6f, left);
            }
            vortex = new UniqueRuntime(dt =>
            {
                if (pc == null || pc.IsDead) return false;
                t += dt; left -= dt;
                if (recalling)
                {
                    Vector3 to = pc.transform.position - at; to.y = 0f;
                    float d = to.magnitude;
                    if (d < 1f) { Count("powroty"); return false; }
                    at += to / d * Mathf.Min(d, speed * dt);
                }
                if (blade != null) { blade.transform.position = at + Vector3.up; blade.transform.Rotate(Vector3.up, 1080f * dt, Space.World); }
                foreach (var e in UniqueGeo.EnemiesIn(at, radius))
                {
                    if (last.TryGetValue(e, out float w2) && t - w2 < tick) continue;
                    last[e] = t;
                    var h = hit; h.sourcePosition = at;
                    Apply(e, h, true);
                }
                return left > 0f;
            }, () =>
            {
                if (blade != null) Object.Destroy(blade);
                if (pc != null) pc.ForceWeaponThrown(false);
            });
            HuWorld.Add(vortex);
            Count("wiry");
            return true;
        }

        public override bool InterceptAttack(ActionType type)
        {
            if (type != ActionType.HeavyAttack || !Spinning || recalling) return false;
            var fist = pc.Config.unarmed != null ? pc.Config.unarmed.weapon.heavy : pc.Build.weapon.heavy;
            if (pc.Stamina.Current <= 0f) { Fail("brak wytrzymałości", "Przywołanie: brak wytrzymałości"); return true; }
            pc.Stamina.Drain(fist.staminaCost, pc.Config.balance.staminaRegenDelay);
            recalling = true;
            Sfx.Play(SoundId.SwingHeavy, pc.AimPoint, 0.7f, 1.2f);
            Count("przywołania");
            return true;
        }

        public override void Death() { vortex?.Kill(); vortex = null; if (pc != null && pc.WeaponThrown) pc.ForceWeaponThrown(false); }
        public override string Hud => Spinning ? $"klinga wiruje {left:0.0} s" : null;
    }
}
