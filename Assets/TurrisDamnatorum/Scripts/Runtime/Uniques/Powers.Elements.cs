using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    // ====================================================================== H01 Korona zatopionego króla

    /// <summary>Niezmienny szron obejmuje wrogów w kręgu wokół postaci; kto wyjdzie z kręgu, traci znacznik.</summary>
    public sealed class DrownedKingPower : UniquePower
    {
        float until = -1f;
        readonly HashSet<EnemyBrain> marked = new HashSet<EnemyBrain>();
        HellParticles fx;
        public bool CircleActive => until > pc.Clock;
        public int Marked => marked.Count;

        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "frost.press") return false;
            until = pc.Clock + amount;
            if (fx != null) fx.StopAndRelease();
            fx = FxLibrary.Enabled ? HellFx.Ambient(pc.transform, Vector3.up * 0.2f, HellFxElement.Ice, P("radius") * 0.8f, amount) : null;
            Count("kręgi");
            return true;
        }

        public override void Tick(float dt)
        {
            if (!CircleActive) { if (marked.Count > 0) Release(); return; }
            var t = pc.FeatureTuning;
            float r = P("radius");
            foreach (var e in EnemyBrain.All)
            {
                if (e == null || e.IsDead) continue;
                bool inside = HuWorld.Flat(e.transform.position, pc.transform.position) <= r;
                if (inside && !marked.Contains(e)) { e.Status.MarkFrost(until - pc.Clock, t.frostReactionInterval, t.frostMudBonus); marked.Add(e); Count("oznaczenia"); }
                else if (!inside && marked.Contains(e)) { e.Status.ClearFrostMark(); marked.Remove(e); Count("wyjścia z kręgu"); }
            }
            marked.RemoveWhere(e => e == null || e.IsDead);
        }

        void Release()
        {
            foreach (var e in marked) if (e != null && !e.IsDead) e.Status.ClearFrostMark();
            marked.Clear();
            if (fx != null) { fx.StopAndRelease(); fx = null; }
        }

        public override void Death() { until = -1f; Release(); }
        public override string Hud => CircleActive ? $"krąg szronu {until - pc.Clock:0.0} s · {marked.Count}" : null;
    }

    // ====================================================================== H02 Płaszcz wędrującej rzeki

    /// <summary>Pole przesuwane Prądem głębin rozwija się w smugę segmentów wzdłuż drogi; tyknięcie dzieli się między segmenty (suma bez zmian).</summary>
    public sealed class WanderingRiverPower : UniquePower
    {
        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "current.glide" || !(data is CurrentGlide g) || g.zone == null) return false;
            var z = g.zone;
            Vector3 a = z.transform.position, b = g.to;
            float dist = HuWorld.Flat(a, b);
            int n = Mathf.Clamp(Mathf.CeilToInt(dist / Mathf.Max(0.3f, P("segment_step"))) + 1, 1, PI("max_segments"));
            var hit = z.ZoneHit;
            hit.physical /= n; hit.magic /= n; // jeden budżet na całą smugę
            float life = z.LocalRemaining, interval = z.LocalInterval, scale = z.TimeScale; // ten sam profil – bez ponownej kompresji
            var color = Names.ElementColor(hit.element);
            for (int i = 0; i < n; i++)
            {
                Vector3 p = n == 1 ? b : Vector3.Lerp(a, b, i / (float)(n - 1));
                DamageZone.Spawn(HuWorld.Ground(p), P("segment_radius"), life, interval, hit, Faction.Player, pc, color, scale);
            }
            z.Expire();
            Count("smugi"); Count("segmenty", n);
            return true;
        }
    }

    // ====================================================================== H03 Grymuar niemego lodowca

    /// <summary>Nova wody dzieli budżet na kokony na sługach; następny atak sługi uwalnia porcję na ofierze. Bez sług – zwykła nova.</summary>
    public sealed class SilentGlacierPower : UniquePower
    {
        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || cast.Def.id != "hu_nova_water") return false;
            var carriers = Servant.Active.Where(s => s != null && s.Master == pc && HuWorld.Flat(s.transform.position, pc.transform.position) <= P("range")).ToList();
            if (carriers.Count == 0) { Fail("brak sług", "Niemy lodowiec: brak sług – zwykła nova"); return false; }
            var share = cast.Hit(1f / carriers.Count);
            share.uniqueEffect = true; share.featureEffect = true; share.attackName = "Lodowy kokon";
            foreach (var s in carriers)
            {
                s.SetCocoon(share, P("cocoon_seconds"), P("burst_radius"));
                if (FxLibrary.Enabled) HellFx.Ambient(s.transform, Vector3.up * 0.8f, HellFxElement.Ice, 0.5f, P("cocoon_seconds"));
            }
            Count("kokony", carriers.Count);
            return true;
        }
    }

    // ====================================================================== H04 Obrączka wiecznego odpływu

    /// <summary>Fala przypływu zbiera własne miny z trasy i odkłada je na końcu drogi (uzbrojenie i termin bez zmian); bez odrzutu wrogów.</summary>
    public sealed class EternalEbbPower : UniquePower
    {
        public override bool SuppressKnockback(HuCast cast) => cast != null && cast.Def.id == "hu_tidal_wave";

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || cast.Def.id != "hu_tidal_wave") return false;
            float reach = cast.P.Has("wave_range") ? cast.Length("wave_range") : cast.Length("range", 6f);
            float speed = cast.Length("speed", 10f), width = P("pick_width");
            Vector3 o = cast.origin, dir = cast.direction;
            var carried = new List<IUniqueMine>();
            float t = 0f;
            HuWorld.Add(new UniqueRuntime(dt =>
            {
                if (pc == null || pc.IsDead) return false;
                t += dt;
                float d = Mathf.Min(reach, speed * t);
                Vector3 front = HuWorld.Clamp(o, o + dir * d);
                if (carried.Count < PI("max_carried"))
                    foreach (var m in UniqueMines.Live(pc))
                    {
                        if (carried.Count >= PI("max_carried") || carried.Contains(m)) continue;
                        if (HuWorld.Flat(m.AnchorPosition, front) <= width) { carried.Add(m); Count("miny zebrane"); }
                    }
                foreach (var m in carried) if (m.AnchorLive) m.MoveTo(front);
                return d < reach;
            }));
            return false; // fala płynie normalnie (bez odrzutu)
        }
    }

    // ====================================================================== H05 Różdżka słonego lustra

    /// <summary>Lodowy odłamek bez trafienia zostawia lustro; następny własny pocisk w lustrze odbija się do najbliższego wroga i chłodnieje.</summary>
    public sealed class SaltMirrorPower : UniquePower
    {
        sealed class Mirror { public Vector3 at; public float until; public GameObject mark; }
        readonly List<Mirror> mirrors = new List<Mirror>();
        readonly HashSet<Object> bounced = new HashSet<Object>();
        public int Mirrors => mirrors.Count;
        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.MainHand) != Item ? "uśpiona: różdżka musi być w głównej ręce" : NeedSkill("hu_frost_shard");
        protected override void OnBind() { HuShot.Ended += OnShotEnded; }
        public override void Dispose() { HuShot.Ended -= OnShotEnded; base.Dispose(); }

        void OnShotEnded(HuShot shot, Vector3 at, bool struck)
        {
            if (!Active || struck || shot == null || shot.SourceCast == null || shot.SourceCast.caster != pc || shot.SourceCast.Def.id != "hu_frost_shard" || bounced.Contains(shot)) return;
            while (mirrors.Count >= PI("max_mirrors")) Remove(0);
            var m = new Mirror { at = HuWorld.Ground(at), until = pc.Clock + P("mirror_seconds") };
            if (FxLibrary.Enabled) m.mark = VisualFx.GroundMarker(m.at, P("mirror_radius"), new Color(0.75f, 0.95f, 1f), HellFxElement.Ice);
            mirrors.Add(m);
            Count("lustra");
        }

        void Remove(int i) { if (mirrors[i].mark != null) Object.Destroy(mirrors[i].mark); mirrors.RemoveAt(i); }

        public override void Tick(float dt)
        {
            for (int i = mirrors.Count - 1; i >= 0; i--) if (pc.Clock > mirrors[i].until) { Count("lustra wygasłe"); Remove(i); }
            if (mirrors.Count == 0) return;
            float r = P("mirror_radius");
            for (int i = mirrors.Count - 1; i >= 0; i--)
            {
                var m = mirrors[i];
                Object used = null; Vector3 pos = default;
                foreach (var s in HuShot.Live) if (s != null && s.IsActive && s.SourceCast != null && s.SourceCast.caster == pc && !bounced.Contains(s) && HuWorld.Flat(s.transform.position, m.at) <= r) { used = s; pos = s.transform.position; break; }
                if (used == null)
                    foreach (var p in Projectile.Active) if (p != null && p.Side == Faction.Player && ReferenceEquals(p.Owner, pc) && !bounced.Contains(p) && HuWorld.Flat(p.transform.position, m.at) <= r) { used = p; pos = p.transform.position; break; }
                if (used == null) continue;
                var prey = HuWorld.Nearest(m.at, 14f);
                Vector3 dir = prey != null ? HuWorld.FlatDir(pos, prey.transform.position) : -(used is HuShot hs ? hs.Velocity : ((Projectile)used).Velocity);
                if (used is HuShot shot) { shot.Redirect(dir, 1f); shot.ConvertShare(Element.Frost, P("frost_share")); }
                else { var pr = (Projectile)used; pr.Redirect(dir, 1f); pr.ConvertShare(Element.Frost, P("frost_share")); }
                bounced.Add(used); // jedno odbicie na pocisk – bez pętli luster
                IceCrystals.Ring(m.at, 0.8f, 4, 0.4f);
                Remove(i);
                Count("odbicia");
            }
            if (bounced.Count > 64) bounced.RemoveWhere(o => o == null);
        }

        public override void Death() { for (int i = mirrors.Count - 1; i >= 0; i--) Remove(i); bounced.Clear(); }
        public override string Hud => mirrors.Count > 0 ? $"lustra: {mirrors.Count}" : null;
    }

    // ====================================================================== A01 Flakon ostatniego oddechu

    /// <summary>Magazyn Oka ciszy wylatuje jako jeden przebijający piorun z kontrolowaną sumą budżetów.</summary>
    public sealed class LastBreathPower : UniquePower
    {
        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "eye.release" || amount <= 0f) return false;
            Vector3 aim = pc.FeatureAim();
            var e = pc.FeatureTarget(25f);
            Vector3 from = pc.AimPoint + Vector3.up * 0.3f + aim * 0.8f;
            Vector3 dir = e != null ? e.AimPoint - from : aim;
            float dmg = amount * P("sum_share");
            var h = PowerHit("Ostatni oddech", 0f, dmg, Element.Lightning, from);
            h.isProjectile = true;
            float speed = 45f;
            Projectile.Spawn(from, dir, speed, h, Faction.Player, pc, new Color(0.8f, 0.85f, 1f), P("width") * 0.5f, P("length") / speed, null, 0f, false,
                new ProjectileOptions { pierce = 99 });
            LightningBolt.Strike(from, from + dir.normalized * P("length"), new Color(0.85f, 0.9f, 1f));
            CombatEvents.RaiseMessage($"OSTATNI ODDECH ({data})", new Color(0.8f, 0.85f, 1f));
            Count("pioruny"); Count("budżet piorunów", dmg);
            return true;
        }
    }

    // ====================================================================== A02 Rękawice burzowego kowala

    /// <summary>Odrzucony wróg wpadający we własny wolny pocisk zużywa go i uwalnia jeden wybuch (bez łańcucha).</summary>
    public sealed class StormSmithPower : UniquePower
    {
        public override void Tick(float dt)
        {
            if (!Active) return;
            foreach (var e in EnemyBrain.All.ToArray())
            {
                if (e == null || e.IsDead || !e.Displaced) continue;
                foreach (var s in HuShot.Live.ToArray())
                {
                    if (s == null || !s.IsActive || s.SourceCast == null || s.SourceCast.caster != pc || s.Velocity.magnitude > 12f) continue;
                    if (Vector3.Distance(s.transform.position, e.AimPoint) > P("contact") + 0.3f * e.Def.scale) continue;
                    var h = s.ShotHit;
                    h.physical *= P("burst_share"); h.magic *= P("burst_share");
                    h.knockback = 0f; h.isReaction = true; h.uniqueEffect = true; h.featureEffect = true; h.attackName = "Zderzenie w kuźni";
                    Vector3 at = s.transform.position;
                    s.Kill(); // pocisk poświęcony – jego budżet wydaje wybuch, nie trafienie
                    int n = HuWorld.Explode(pc, at, P("burst_radius"), h, new Color(0.75f, 0.85f, 1f));
                    LightningBolt.Strike(at + Vector3.up * 2f, at, new Color(0.75f, 0.85f, 1f));
                    Count("zderzenia"); Count("trafieni", n);
                    break;
                }
            }
        }
    }

    // ====================================================================== A03 Peleryna bezwietrznej nocy

    /// <summary>Krok wichru nie przesuwa postaci – kieruje i rozpędza własne pociski w obszarze (bez odświeżenia trafień i czasu).</summary>
    public sealed class WindlessNightPower : UniquePower
    {
        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || cast.Def.id != "hu_wind_step") return false;
            Vector3 c = pc.transform.position;
            var e = pc.HuAimTarget(20f, 60f, c, aim);
            int n = 0;
            foreach (var s in HuShot.Live.ToArray())
                if (s != null && s.IsActive && s.SourceCast != null && s.SourceCast.caster == pc && HuWorld.Flat(s.transform.position, c) <= P("radius"))
                { s.Redirect(e != null ? HuWorld.FlatDir(s.transform.position, e.transform.position) : aim, P("speed_mult")); n++; }
            foreach (var p in Projectile.Active.ToArray())
                if (p != null && p.Side == Faction.Player && ReferenceEquals(p.Owner, pc) && HuWorld.Flat(p.transform.position, c) <= P("radius"))
                { p.Redirect(e != null ? HuWorld.FlatDir(p.transform.position, e.transform.position) : aim, P("speed_mult")); n++; }
            HellFx.Shockwave(c, HellFxElement.Air, new Color(0.8f, 0.85f, 1f), P("radius"), false);
            Sfx.Play(SoundId.Whiff, pc.AimPoint, 0.8f, 0.8f);
            Count("podmuchy"); Count("pokierowane pociski", n);
            return true;
        }
    }

    // ====================================================================== A04 Różdżka pustych węzłów

    /// <summary>Łańcuch potępionych może skoczyć przez własny pocisk (węzeł, nie cel) do dalszego wroga.</summary>
    public sealed class EmptyNodesPower : UniquePower
    {
        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.MainHand) != Item ? "uśpiona: różdżka musi być w głównej ręce" : NeedSkill("hu_damned_chain");
        public override int ChainNodes(HuCast cast) => cast != null && cast.Def.id == "hu_damned_chain" ? PI("max_nodes") : 0;

        public override Vector3? ChainNode(Vector3 from, float jump, HashSet<object> used)
        {
            object best = null; Vector3 at = default; float bd = float.MaxValue;
            foreach (var s in HuShot.Live)
            {
                if (s == null || !s.IsActive || s.SourceCast == null || s.SourceCast.caster != pc || used.Contains(s)) continue;
                float d = HuWorld.Flat(s.transform.position, from);
                if (d <= jump && d < bd) { bd = d; best = s; at = s.transform.position; }
            }
            foreach (var p in Projectile.Active)
            {
                if (p == null || p.Side != Faction.Player || !ReferenceEquals(p.Owner, pc) || used.Contains(p)) continue;
                float d = HuWorld.Flat(p.transform.position, from);
                if (d <= jump && d < bd) { bd = d; best = p; at = p.transform.position; }
            }
            if (best == null) return null;
            used.Add(best);
            Count("węzły");
            return at;
        }
    }

    // ====================================================================== A05 Buty ostatniego podmuchu

    /// <summary>Nova powietrza nie rozpycha wrogów – po wyzwoleniu z trafieniem odrzuca właściciela od głównego celu.</summary>
    public sealed class LastGustPower : UniquePower
    {
        public override bool SuppressKnockback(HuCast cast) => cast != null && cast.Def.id == "hu_nova_air";

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || cast.Def.id != "hu_nova_air") return false;
            float telegraph = cast.P.GetF("telegraph", 0.3f), radius = cast.Area("radius", 3f);
            HuWorld.After(telegraph + 0.02f, () =>
            {
                if (pc == null || pc.IsDead) return;
                var main = pc.LockTarget != null && !pc.LockTarget.IsDead ? pc.LockTarget : HuWorld.Nearest(pc.transform.position, radius);
                if (main == null || HuWorld.Flat(main.transform.position, pc.transform.position) > radius + 0.5f) { Count("pudła (bez odskoku)"); return; }
                Vector3 away = HuWorld.FlatDir(main.transform.position, pc.transform.position);
                pc.UniquePushSelf(away, P("distance"), P("seconds"));
                FxLibrary.Dust(pc.transform.position, 1f);
                Count("odskoki");
            });
            return false; // nova wybucha normalnie (bez odrzutu)
        }
    }

    // ====================================================================== E01 Buty ruchomego bastionu

    /// <summary>Bastion: dwie krótsze boczne płyty prowadzone z postacią (kolizje, trwałość, zniszczalne).</summary>
    public sealed class MovingBastionPower : UniquePower
    {
        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "bastion.press") return false;
            var t = pc.FeatureTuning;
            EarthWall.ClearAll();
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 off = new Vector3(side * P("side_offset"), 0f, 0f);
                var rot = Quaternion.LookRotation(pc.transform.forward) * Quaternion.Euler(0f, 90f, 0f);
                var w = EarthWall.Spawn(pc, pc.transform.position + pc.transform.rotation * off, rot, t, P("plate_length"), P("seconds"), 2);
                w.Follow(pc.transform, off);
            }
            Count("płyty");
            return true;
        }
    }

    // ====================================================================== E02 Pas grobowego moździerza

    /// <summary>Płyta Kruszącej skorupy spada z nieba w punkt (zapowiedź, opóźnienie) zamiast lecieć z postaci.</summary>
    public sealed class GraveMortarPower : UniquePower
    {
        public override bool FeatureEvent(string evt, ref float amount, object data)
        {
            if (evt != "shell.plate") return false;
            var e = pc.FeatureTarget(P("range") + 4f);
            Vector3 at = e != null ? e.transform.position : pc.transform.position + pc.FeatureAim() * P("range");
            at = HuWorld.Clamp(pc.transform.position, at);
            float budget = amount, r = P("radius");
            var mark = VisualFx.GroundMarker(at, r, new Color(0.7f, 0.55f, 0.35f), HellFxElement.Earth);
            HuWorld.Add(new HuTelegraphRuntime(mark, P("delay")));
            HuWorld.After(P("delay"), () =>
            {
                if (pc == null || pc.IsDead) return;
                var h = PowerHit("Grobowa płyta", 0f, budget, Element.Earth, at);
                h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Stagger) };
                h.poiseDamage = 40f; h.heavy = true;
                int n = HuWorld.Explode(pc, at, r, h, new Color(0.7f, 0.55f, 0.35f));
                HellFx.SlamDust(at, r);
                Count("płyty z nieba"); Count("trafieni", n);
            });
            return true;
        }
    }

    // ====================================================================== E03 Młot niewykopanego grobu

    /// <summary>Kamienne kolce czekają pod ziemią (znaki); mocny atak młotem wyzwala wszystkie znaki w zasięgu.</summary>
    public sealed class UndugGravePower : UniquePower
    {
        sealed class Mark { public HuCast cast; public float until; public GameObject fx; }
        readonly List<Mark> marks = new List<Mark>();
        public int Marks => marks.Count;
        protected override string EquipmentDormant() => Run.equipment.Get(EquipSlot.MainHand) != Item ? "uśpiona: młot musi być w głównej ręce" : NeedSkill("hu_stone_spikes");

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || cast.Def.id != "hu_stone_spikes") return false;
            while (marks.Count >= PI("max_marks")) Remove(0);
            var m = new Mark { cast = cast.Variant(cast.origin, cast.direction, cast.target, 1f), until = pc.Clock + P("mark_seconds") };
            if (FxLibrary.Enabled)
            {
                Vector3 a = cast.origin + cast.direction * 0.6f;
                var d = FxLibrary.Decal(HuWorld.Ground(a + cast.direction * cast.Length("length", 8f) * 0.5f), HellVfxMaterials.Ground(HellFxElement.Earth, false), new Color(0.6f, 0.45f, 0.3f), cast.Area("width", 1.4f), cast.Length("length", 8f), P("mark_seconds"));
                if (d != null) { d.transform.rotation = Quaternion.LookRotation(cast.direction) * d.transform.rotation; m.fx = d.gameObject; }
            }
            marks.Add(m);
            Count("znaki");
            return true;
        }

        public override void AttackActive(ActionType type, AttackDefinition atk, int origin)
        {
            if (type != ActionType.HeavyAttack || marks.Count == 0 || Run.equipment.Get(EquipSlot.MainHand) != Item) return;
            int n = 0;
            for (int i = marks.Count - 1; i >= 0; i--)
            {
                var m = marks[i];
                if (HuWorld.Flat(m.cast.origin, pc.transform.position) > P("range")) continue;
                pc.UniqueFireHuCast(m.cast, m.cast.direction, 0f, origin);
                Remove(i); n++;
            }
            if (n > 0) { HellFx.SlamDust(pc.transform.position, 1.5f); Count("wyzwolenia"); Count("linie", n); }
        }

        void Remove(int i) { if (marks[i].fx != null) Object.Destroy(marks[i].fx); marks.RemoveAt(i); }
        public override void Tick(float dt) { for (int i = marks.Count - 1; i >= 0; i--) if (pc.Clock > marks[i].until) { Count("znaki wygasłe"); Remove(i); } }
        public override void Death() { for (int i = marks.Count - 1; i >= 0; i--) Remove(i); }
        public override string Hud => marks.Count > 0 ? $"znaki grobu: {marks.Count}" : null;
    }

    // ====================================================================== E04 Napierśnik żywego kamieniołomu

    /// <summary>Osłona z płyt: płyta odłamana wrogim ciosem zostawia odłamek; Kamienne kolce zbierają odłamki i poszerzają linię (bez podwójnego trafienia).</summary>
    public sealed class LivingQuarryPower : UniquePower
    {
        float capacity;
        readonly List<(Vector3 at, GameObject fx)> shards = new List<(Vector3, GameObject)>();
        public int Shards => shards.Count;
        protected override string EquipmentDormant() => SlotSkill("hu_stone_ward") == null ? NeedSkill("hu_stone_ward") : NeedSkill("hu_stone_spikes");

        public override void Tick(float dt)
        {
            if (pc.BarrierAmount <= 0.01f) capacity = 0f;
            else if (pc.BarrierAmount > capacity) capacity = pc.BarrierAmount; // nowa osłona = nowy podział na płyty
        }

        public override void BarrierAbsorbed(float before, float after, in HitData hit)
        {
            if (capacity <= 0f) return;
            float plate = capacity / Mathf.Max(1, PI("plates"));
            int crossed = Mathf.FloorToInt((capacity - after) / plate + 1e-4f) - Mathf.FloorToInt((capacity - before) / plate + 1e-4f);
            for (int i = 0; i < crossed && shards.Count < PI("max_shards"); i++)
            {
                Vector3 at = pc.transform.position + Random.insideUnitSphere.normalized * 0.8f; at.y = pc.transform.position.y;
                GameObject fx = FxLibrary.Enabled ? VisualFx.GroundMarker(HuWorld.Ground(at), 0.5f, new Color(0.6f, 0.5f, 0.35f), HellFxElement.Earth) : null;
                shards.Add((at, fx));
                Count("odłamki");
            }
        }

        public override bool OverrideHuCast(HuCast cast, Vector3 aim, float range)
        {
            if (!CastKinds.Manual(cast) || cast.Def.id != "hu_stone_spikes" || shards.Count == 0) return false;
            Vector3 a = cast.origin + cast.direction * 0.6f, b = a + cast.direction * cast.Length("length", 8f);
            float half = cast.Area("width", 1.4f) * 0.5f;
            int got = 0;
            for (int i = shards.Count - 1; i >= 0; i--)
                if (UniqueGeo.SegmentDistance(a, b, shards[i].at) <= P("pick_radius")) { if (shards[i].fx != null) Object.Destroy(shards[i].fx); shards.RemoveAt(i); got++; }
            if (got == 0) return false;
            float extra = got * P("width_per_shard");
            var hit = cast.Hit();
            HuWorld.After(cast.P.GetF("delay_seconds", 0.45f), () =>
            {
                if (pc == null || pc.IsDead) return;
                // Tylko poszerzone pokrycie: ofiary w zwykłej linii dostają cios raz (z samego skilla).
                int n = HuWorld.HitBand(pc, a, b, half + extra, hit, new HitTracker(), r => UniqueGeo.SegmentDistance(a, b, r.Transform.position) > half + 0.4f);
                Count("trafienia poszerzenia", n);
            });
            Count("zebrane odłamki", got);
            return false;
        }

        public override void Death() { foreach (var s in shards) if (s.fx != null) Object.Destroy(s.fx); shards.Clear(); capacity = 0f; }
        public override string Hud => shards.Count > 0 ? $"odłamki: {shards.Count}" : null;
    }

    // ====================================================================== E05 Amulet rany pod stopami

    /// <summary>Własne pęknięcie pancerza zostaje polem na ziemi; wrogowie w polu mają warstwy pęknięcia, po wyjściu je tracą.</summary>
    public sealed class WoundUnderfootPower : UniquePower
    {
        sealed class Field { public Vector3 at; public float until; public GameObject fx; }
        readonly List<Field> fields = new List<Field>();
        readonly Dictionary<EnemyBrain, int> granted = new Dictionary<EnemyBrain, int>();
        public int Fields => fields.Count;

        public override void OutgoingAgainst(ref HitData hit, EnemyBrain target)
        {
            if (target == null || hit.isStatusTick) return;
            bool sunder = hit.statuses != null && hit.statuses.Exists(s => s.kind == StatusKind.Sunder);
            bool heavyBasic = hit.basicAttack && hit.fromMeleeWeapon && pc.Actions.Current == ActionType.HeavyAttack && !hit.featureEffect;
            if (!sunder && !heavyBasic) return;
            if (sunder) hit.statuses = hit.statuses.Where(s => s.kind != StatusKind.Sunder).ToList(); // pęknięcie idzie w ziemię, nie w ciało
            while (fields.Count >= PI("max_fields")) Remove(0);
            var f = new Field { at = HuWorld.Ground(target.transform.position), until = pc.Clock + P("field_seconds") };
            if (FxLibrary.Enabled) f.fx = VisualFx.GroundMarker(f.at, P("radius"), new Color(0.7f, 0.5f, 0.3f), HellFxElement.Earth);
            fields.Add(f);
            Count("pola");
        }

        void Remove(int i) { if (fields[i].fx != null) Object.Destroy(fields[i].fx); fields.RemoveAt(i); }

        public override void Tick(float dt)
        {
            for (int i = fields.Count - 1; i >= 0; i--) if (pc.Clock > fields[i].until) Remove(i);
            int stacks = Mathf.Min(PI("stacks"), pc.Config.balance.sunderMaxStacks);
            float magic = pc.UniqueSunderMagic;
            foreach (var e in EnemyBrain.All.ToArray())
            {
                if (e == null || e.IsDead) continue;
                bool inside = fields.Exists(f => HuWorld.Flat(f.at, e.transform.position) <= P("radius"));
                granted.TryGetValue(e, out int g);
                if (inside)
                {
                    // Warstwy pola utrzymywane do limitu (pola się nie sumują, inne źródła zostają).
                    int added = e.Status.EnsureSunder(stacks, 0.6f, magic);
                    if (added > 0) granted[e] = g + added;
                }
                else if (g > 0) { e.Status.ReduceSunder(g); granted.Remove(e); Count("wyjścia z pola"); }
            }
        }

        public override void Death() { for (int i = fields.Count - 1; i >= 0; i--) Remove(i); granted.Clear(); }
        public override string Hud => fields.Count > 0 ? $"rany w ziemi: {fields.Count}" : null;
    }
}
