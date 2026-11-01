using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    // ====================================================================== Łotrzyk A – Fałszywy świadek

    /// <summary>Wabik w miejscu łotrzyka: do limitu wrogów w zasięgu kieruje w niego swój następny zwykły atak (FeatureDecoy).</summary>
    /// <summary>Naciśnięcie Fałszywego świadka (dane dla mocy unikatu: moc tworzy własny wabik).</summary>
    public sealed class WitnessPress { public float seconds, range; public int maxDeceived; public FeatureDecoy decoy; }

    /// <summary>Sztuczka gotowa do wykonania (dane dla mocy unikatu).</summary>
    public sealed class TrickData { public StolenTrickFeature.TrickKind kind; public HitData hit; public float reach, radius, spread; public int count; public Element element; public bool heavy; }

    public sealed class FalseWitnessFeature : ClassFeatureController
    {
        FeatureDecoy decoy;
        public FeatureDecoy Decoy => decoy != null && decoy.Alive ? decoy : null;
        protected override void OnBind() { color = new Color(0.6f, 0.55f, 1f); }

        protected override void OnPress()
        {
            if (Decoy != null) Decoy.End();
            // Maska fałszywego króla: obiektem oszustwa staje się wskazany prawdziwy wróg.
            var king = new WitnessPress { seconds = T.decoySeconds, maxDeceived = T.decoyMaxDeceived, range = T.decoyRange };
            if (pc.UniquesFeatureEvent("witness.press", king) && king.decoy != null)
            {
                decoy = king.decoy;
                Telemetry.Add("wabik: oszukani wrogowie", decoy.DeceivedCount);
                Used(T.decoyCooldown, $"FAŁSZYWY KRÓL ({decoy.DeceivedCount})");
                return;
            }
            decoy = FeatureDecoy.Spawn(pc, pc.transform.position - pc.transform.forward * 0.3f, T.decoySeconds, pc.Health.Max * T.decoyHealth, T.decoyMaxDeceived, T.decoyRange);
            Telemetry.Add("wabik: oszukani wrogowie", decoy.DeceivedCount);
            Used(T.decoyCooldown, decoy.DeceivedCount > 0 ? $"FAŁSZYWY ŚWIADEK ({decoy.DeceivedCount})" : "FAŁSZYWY ŚWIADEK – nikt nie patrzy");
        }

        public override void FloorStart() { base.FloorStart(); FeatureDecoy.ClearAll(); decoy = null; }
        public override void Death() { FeatureDecoy.ClearAll(); decoy = null; }
        public override ClassFeatureHud Hud => Decoy != null ? new ClassFeatureHud { state = ClassFeatureState.Active, label = $"wabik: {Decoy.DeceivedCount} wrogów", fill = Cooldown > 0 ? 1f - Cooldown / cooldownLength : 1f, color = color } : ReadyOrCooldown();
    }

    // ====================================================================== Łotrzyk B – Kradzież sztuczki

    /// <summary>
    /// Obserwacja wskazanego wroga: jego poprawnie wykonany zwykły atak (wręcz, pocisk, obszar) zostaje zapisany jako jeden profil
    /// sztuczki; kolejne G wykonuje go jako gracz – znormalizowany budżet (wielokrotność zwykłego ataku gracza), żywioł i efekty
    /// z ataku wroga. Ataki specjalne (fazy, przywołania, ściany, kanały) są niedostępne – komunikat to wyjaśnia.
    /// </summary>
    public sealed class StolenTrickFeature : ClassFeatureController
    {
        public enum TrickKind { None, Melee, Projectile, Area }
        EnemyBrain observed;
        float observe, trickLife;
        TrickKind trick;
        string trickName, trickSource;
        Element trickElement; DamageType trickType;
        List<StatusApplication> trickStatuses;
        float trickReach, trickRadius; int trickCount; float trickSpread; bool trickHeavy;
        public TrickKind Trick => trick;
        public bool Observing => observe > 0f && observed != null && !observed.IsDead;
        protected override bool AwaitingSecondPress => trick != TrickKind.None;
        protected override void OnBind() { color = new Color(0.9f, 0.75f, 0.4f); EnemyBrain.AttackActive += OnEnemyAttack; }
        public override void Dispose() { base.Dispose(); EnemyBrain.AttackActive -= OnEnemyAttack; }

        protected override void OnPress()
        {
            if (trick != TrickKind.None) { Execute(); return; }
            if (Observing) { Fail("obserwacja trwa", "Kradzież sztuczki: czekam na atak celu"); return; }
            var e = pc.FeatureTarget(T.stealRange);
            if (e == null) { Fail("brak celu", "Kradzież sztuczki: brak celu w zasięgu"); return; }
            observed = e; observe = T.stealObserve;
            HellFx.Shadow(e.transform, e.transform.position, false, T.stealObserve, 0.6f);
            CombatEvents.RaiseMessage($"Obserwacja: {e.DisplayName}", color);
        }

        void OnEnemyAttack(EnemyBrain e, EnemyAttackEntry entry)
        {
            if (!Observing || e != observed || entry == null || entry.attack == null) return;
            if (entry.special != EnemySpecial.None)
            {
                Telemetry.Reject("atak specjalny");
                CombatEvents.RaiseMessage($"Kradzież sztuczki: „{entry.attack.name}” to atak specjalny – nie do skradzenia", new Color(0.8f, 0.7f, 0.6f));
                return;
            }
            var a = entry.attack;
            trick = a.delivery == AttackDelivery.Projectile ? TrickKind.Projectile : a.delivery == AttackDelivery.AreaAroundSelf ? TrickKind.Area : TrickKind.Melee;
            trickName = a.name; trickSource = e.Def.id; trickElement = a.element; trickType = a.damageType; trickHeavy = a.heavy;
            trickStatuses = a.statuses != null && a.statuses.Count > 0 ? new List<StatusApplication>(a.statuses) : null;
            trickReach = Mathf.Clamp(a.reach, 1.6f, 3.6f); trickRadius = Mathf.Clamp(a.radius > 0 ? a.radius : 2.5f, 1.5f, 4f);
            trickCount = Mathf.Clamp(a.projectileCount, 1, 3); trickSpread = Mathf.Min(a.spreadAngle, 15f);
            trickLife = T.stealTrickLife;
            observe = 0f; observed = null;
            Telemetry.Add("kradzież: zapisane sztuczki");
            CombatEvents.RaiseMessage($"SZTUCZKA: {trickName}", color);
        }

        void Execute()
        {
            if (pc.Actions.Current != ActionType.None && pc.Actions.Current != ActionType.Block) { Fail("zajęty", "Kradzież sztuczki: dokończ akcję"); return; }
            float budget = pc.BasicDamage * T.stealDamage;
            bool magic = trickType == DamageType.Magic || trickElement != Element.None;
            var h = pc.FeatureHit(trickName, magic ? 0f : budget, magic ? budget : 0f, trickElement, pc.transform.position);
            h.featureEffect = false; h.statuses = trickStatuses; h.heavy = trickHeavy; h.poiseDamage = trickHeavy ? 45f : 25f;
            Vector3 aim = pc.FeatureAim();
            var tr = new HitTracker();
            // Rękawice zawodowego oszusta: sztuczka zostaje pułapką (ten sam profil nie istnieje też w magazynie).
            if (pc.UniquesFeatureEvent("trick.execute", new TrickData { kind = trick, hit = h, reach = trickReach, radius = trickRadius, spread = trickSpread, count = trickCount, element = trickElement, heavy = trickHeavy }))
            {
                pc.Stamina.Drain(15f, pc.Config.balance.staminaRegenDelay);
                Telemetry.Add("kradzież: pułapki " + trick);
                trick = TrickKind.None;
                Used(T.stealCooldown, "SZTUCZKA → PUŁAPKA");
                return;
            }
            switch (trick)
            {
                case TrickKind.Melee:
                    HitQuery.Sphere(pc.transform.position + Vector3.up + aim * trickReach * 0.6f, trickReach * 0.7f, Faction.Player, tr, r => HitQuery.Apply(pc, r, h, true));
                    pc.PlayFeatureSwing(trickHeavy);
                    HellFx.Cone(pc.transform.position + Vector3.up * 1.1f, aim, trickElement != Element.None ? HellFx.For(trickElement) : HellFxElement.Physical, trickReach, 110f);
                    break;
                case TrickKind.Projectile:
                {
                    var e = pc.FeatureTarget(20f);
                    Vector3 from = pc.AimPoint + Vector3.up * 0.3f + aim * 0.8f;
                    for (int i = 0; i < trickCount; i++)
                    {
                        float ang = trickCount == 1 ? 0f : Mathf.Lerp(-trickSpread, trickSpread, i / (float)(trickCount - 1));
                        var hi = h; hi.physical /= trickCount; hi.magic /= trickCount;
                        Projectile.Spawn(from, Quaternion.Euler(0, ang, 0) * (e != null ? e.AimPoint - from : aim), 18f, hi, Faction.Player, pc, Names.ElementColor(trickElement), 0.25f, 2.5f, i == 0 ? e : null, e != null && i == 0 ? 60f : 0f);
                    }
                    break;
                }
                default:
                    HitQuery.Sphere(pc.transform.position + Vector3.up, trickRadius, Faction.Player, tr, r => HitQuery.Apply(pc, r, h, false));
                    HellFx.Shockwave(pc.transform.position, trickElement != Element.None ? HellFx.For(trickElement) : HellFxElement.Enemy, Names.ElementColor(trickElement), trickRadius, true);
                    break;
            }
            pc.Stamina.Drain(15f, pc.Config.balance.staminaRegenDelay);
            Telemetry.Add("kradzież: wykonane " + trick);
            Telemetry.Add("kradzież: źródło " + trickSource);
            trick = TrickKind.None;
            Used(T.stealCooldown, null);
            Sfx.Play(SoundId.Whiff, pc.AimPoint, 0.8f);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (observe > 0f) { observe -= dt; if (observed == null || observed.IsDead) observe = 0f; }
            if (trick != TrickKind.None) { trickLife -= dt; if (trickLife <= 0f) { trick = TrickKind.None; Telemetry.Add("kradzież: przepadłe sztuczki"); } }
        }

        public override void FloorStart() { base.FloorStart(); trick = TrickKind.None; observe = 0f; observed = null; }
        public override void Death() { trick = TrickKind.None; observe = 0f; observed = null; }
        public override ClassFeatureHud Hud => trick != TrickKind.None ? Timed(ClassFeatureState.Stored, $"sztuczka: {trickName} ({trickLife:0} s)", trickLife, T.stealTrickLife)
            : Observing ? Timed(ClassFeatureState.Target, $"obserwacja: {observed.DisplayName}", observe, T.stealObserve) : ReadyOrCooldown();
    }

    // ====================================================================== Łucznik A – Gwoździe łowcy

    /// <summary>
    /// W oknie podstawowy pocisk (łuk, różdżka) zostawia w trafionym grot z odłożonym budżetem (część trafienia; kontakt bez zmian).
    /// Limity: na cel, na aktywację, a pierwszy cel strzału dostaje pełny grot, kolejne (przebicie, rykoszet) – połowę. Drugie G
    /// wyrywa wszystkie groty w zasięgu: rany, krwawienie i postawa; wyrwanie nie osadza nowych grotów. Groty, których nie wyrwano,
    /// gasną po czasie od końca okna. Śmierć celu usuwa jego magazyn.
    /// </summary>
    public sealed class HuntersNailsFeature : ClassFeatureController
    {
        sealed class Store { public int count; public float budget; }
        readonly Dictionary<EnemyBrain, Store> nails = new Dictionary<EnemyBrain, Store>();
        readonly Dictionary<int, int> perShot = new Dictionary<int, int>();
        float active, expire;
        int total;
        public int NailCount => nails.Where(kv => kv.Key != null && !kv.Key.IsDead).Sum(kv => kv.Value.count);
        public float NailBudget => nails.Where(kv => kv.Key != null && !kv.Key.IsDead).Sum(kv => kv.Value.budget);
        public int NailsOn(EnemyBrain e) => nails.TryGetValue(e, out var s) ? s.count : 0;
        protected override bool AwaitingSecondPress => NailCount > 0;
        protected override void OnBind() { color = new Color(0.9f, 0.8f, 0.6f); }

        protected override void OnPress()
        {
            if (NailCount > 0) { Rip(); return; }
            if (pc.Build.weapon.light.delivery != AttackDelivery.Projectile) { Fail("brak łuku", "Gwoździe łowcy: potrzebny łuk albo różdżka"); return; }
            active = T.nailsSeconds; total = 0; perShot.Clear();
            Used(T.nailsCooldown, "GWOŹDZIE ŁOWCY");
        }

        public override void BasicShot(ref HitData hit, int index, int count) { if (active > 0f) hit.nailShare = T.nailsShare; }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (hit.nailShare <= 0f || hit.featureEffect || !(target is EnemyBrain e) || e.IsDead) return;
            if (!(result.outcome == HitOutcome.Hit || result.outcome == HitOutcome.GuardBroken)) return;
            if (total >= T.nailsPerActivation) { Telemetry.Add("groty: limit aktywacji"); return; }
            if (!nails.TryGetValue(e, out var s)) nails[e] = s = new Store();
            if (s.count >= T.nailsPerTarget) { Telemetry.Add("groty: limit celu"); return; }
            perShot.TryGetValue(hit.originId, out int n);
            perShot[hit.originId] = n + 1;
            float share = n == 0 ? 1f : 0.5f; // przebicie/rykoszet: dzielony budżet strzału
            s.count++; s.budget += hit.Total * hit.nailShare * share; total++;
            expire = T.nailsExpire;
            Telemetry.Add("groty: osadzone");
            Telemetry.Add("groty: budżet", hit.Total * hit.nailShare * share);
        }

        /// <summary>Groty na celach (dla mocy unikatu: Rękawice tkacza cięciw).</summary>
        public List<KeyValuePair<EnemyBrain, float>> NailBudgets() => nails.Where(kv => kv.Key != null && !kv.Key.IsDead).Select(kv => new KeyValuePair<EnemyBrain, float>(kv.Key, kv.Value.budget)).ToList();
        public bool Embedding => active > 0f;

        void Rip()
        {
            float rip = T.nailsRip;
            if (pc.UniquesFeatureEvent("nails.rip", ref rip, this)) { Telemetry.Add("groty: linie"); nails.Clear(); return; }
            int ripped = 0; float dealt = 0f;
            foreach (var kv in nails.ToList())
            {
                var e = kv.Key; var s = kv.Value;
                if (e == null || e.IsDead || HuWorld.Flat(e.transform.position, pc.transform.position) > T.nailsRange) continue;
                var h = pc.FeatureHit("Wyrwanie grotów", s.budget * T.nailsRip, 0f, Element.None, e.transform.position);
                h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Bleed, Mathf.Clamp(s.count, 1, 3)) };
                h.poiseDamage = 12f * s.count;
                h.nailShare = 0f;
                HitQuery.Apply(pc, e, h, false);
                FxLibrary.Blood(e.AimPoint, (pc.transform.position - e.transform.position).normalized, 1f + s.count * 0.3f);
                ripped += s.count; dealt += s.budget * T.nailsRip;
                nails.Remove(e);
            }
            if (ripped == 0) { Fail("brak celu", "Gwoździe łowcy: groty poza zasięgiem"); return; }
            Telemetry.Add("groty: wyrwane", ripped);
            Telemetry.Add("groty: obrażenia wyrwania", dealt);
            CombatEvents.RaiseMessage($"WYRWANIE: {ripped} grotów", color);
            Sfx.Play(SoundId.HitFleshHeavy, pc.AimPoint, 0.8f);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (active > 0f) active -= dt;
            foreach (var k in nails.Keys.Where(k => k == null || k.IsDead).ToList()) nails.Remove(k); // śmierć celu sprząta magazyn
            if (active <= 0f && nails.Count > 0) { expire -= dt; if (expire <= 0f) { Telemetry.Add("groty: wygasłe", NailCount); nails.Clear(); } }
        }

        public override void FloorStart() { base.FloorStart(); active = 0f; nails.Clear(); perShot.Clear(); }
        public override void Death() { active = 0f; nails.Clear(); }
        public override ClassFeatureHud Hud => active > 0f ? Timed(ClassFeatureState.Active, $"osadzanie {active:0.0} s · groty {NailCount}", active, T.nailsSeconds)
            : NailCount > 0 ? new ClassFeatureHud { state = ClassFeatureState.SecondPhase, label = $"wyrwij: {NailCount} grotów ({NailBudget:0})", fill = expire / Mathf.Max(0.1f, T.nailsExpire), color = color }
            : ReadyOrCooldown();
    }

    // ====================================================================== Łucznik B – Stanowisko strzeleckie

    /// <summary>
    /// Podstawowy strzał gracza wylatuje ze stanowiska (do zasięgu łącza), w realny cel albo w punkt przed graczem; właściciel,
    /// koszt i animacja – gracza. Sokole oko liczy odległość od punktu wylotu. Za daleko – zwykły strzał z ręki i komunikat.
    /// </summary>
    public sealed class FiringPositionFeature : ClassFeatureController
    {
        FiringPost post;
        bool fromPost; Vector3 lastOrigin;
        float warnCooldown;
        public FiringPost Post => post != null && post.Alive ? post : null;
        protected override void OnBind() { color = new Color(0.95f, 0.8f, 0.45f); }

        protected override void OnPress()
        {
            if (pc.Build.weapon.light.delivery != AttackDelivery.Projectile) { Fail("brak łuku", "Stanowisko: potrzebny łuk albo różdżka"); return; }
            Vector3 aim = pc.FeatureAim();
            Vector3 at = HuWorld.Clamp(pc.transform.position, pc.transform.position + aim * Mathf.Min(T.postPlaceRange, 6f), 0.6f);
            if (!Physics.Raycast(at + Vector3.up * 1.5f, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore)) { Fail("brak miejsca", "Stanowisko: brak podłoża"); return; }
            if (Post != null) Post.End();
            post = FiringPost.Spawn(at, Quaternion.LookRotation(aim), T.postSeconds);
            Used(T.postCooldown, "STANOWISKO STRZELECKIE");
        }

        public override void ShotOrigin(ref Vector3 origin, ref Vector3 aim, ref IHitReceiver homing)
        {
            fromPost = false;
            var p = Post;
            if (p == null) return;
            if (pc.UniquesFeatureEvent("post.shot", p)) return; // Buty wygnanego strzelca: mocny strzał z ręki, potem zamiana
            if (HuWorld.Flat(p.transform.position, pc.transform.position) > T.postLinkRange)
            {
                Telemetry.Add("stanowisko: poza zasięgiem");
                if (warnCooldown <= 0f) { CombatEvents.RaiseMessage("Stanowisko poza zasięgiem – strzał z ręki", new Color(0.8f, 0.75f, 0.6f)); warnCooldown = 2f; }
                return;
            }
            origin = p.Muzzle;
            Vector3 targetPoint = homing != null && !homing.IsDead ? homing.AimPoint : pc.transform.position + pc.transform.forward * 14f + Vector3.up * 1.2f;
            aim = targetPoint - origin;
            p.Flash(aim);
            p.Shots++;
            fromPost = true; lastOrigin = origin;
            Telemetry.Add("stanowisko: strzały z punktu");
        }

        public override void BasicShot(ref HitData hit, int index, int count)
        {
            if (!fromPost) return;
            hit.launchPoint = lastOrigin; hit.hasLaunchPoint = true;
        }

        public override void Tick(float dt) { base.Tick(dt); if (warnCooldown > 0f) warnCooldown -= dt; }
        public override void FloorStart() { base.FloorStart(); if (Post != null) Post.End(); post = null; }
        public override void Death() { if (Post != null) Post.End(); post = null; }
        public override ClassFeatureHud Hud => Post != null ? Timed(ClassFeatureState.Active, $"stanowisko {Post.Remaining:0.0} s" + (HuWorld.Flat(Post.transform.position, pc.transform.position) > T.postLinkRange ? " (poza zasięgiem)" : ""), Post.Remaining, T.postSeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Zabójca A – Przygwożdżony cień

    /// <summary>Cień ofiary w jej aktualnym miejscu: drugi, nieruchomy punkt odbierania trafień (PinnedShadow, alias celu).</summary>
    public sealed class PinnedShadowFeature : ClassFeatureController
    {
        PinnedShadow shadow;
        public PinnedShadow Shadow => shadow != null && shadow.Alive ? shadow : null;
        protected override void OnBind() { color = new Color(0.55f, 0.25f, 0.65f); }

        protected override void OnPress()
        {
            var e = pc.FeatureTarget(T.shadowRange);
            if (e == null) { Fail("brak celu", "Przygwożdżony cień: brak celu w zasięgu"); return; }
            PinnedShadow.ClearAll();
            shadow = PinnedShadow.Spawn(e, T.shadowSeconds, T.shadowLink);
            Used(T.shadowCooldown, "PRZYGWOŻDŻONY CIEŃ");
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            var s = Shadow;
            if (s == null || !ReferenceEquals(target, s.Victim) || hit.isStatusTick) return;
            // Trafienie z dala od ciała, a przy cieniu – przyszło przez cień.
            Vector3 src = hit.sourcePosition;
            if (HuWorld.Flat(src, s.transform.position) + 0.5f < HuWorld.Flat(src, s.Victim.transform.position) && HuWorld.Flat(s.Victim.transform.position, s.transform.position) > 1.2f)
            { s.CountForward(); Telemetry.Add("cień: trafienia przez cień"); }
        }

        public override void FloorStart() { base.FloorStart(); PinnedShadow.ClearAll(); shadow = null; }
        public override void Death() { PinnedShadow.ClearAll(); shadow = null; }
        public override ClassFeatureHud Hud => Shadow != null ? new ClassFeatureHud { state = ClassFeatureState.Target, label = $"cień: {Shadow.Victim.DisplayName}", fill = Cooldown > 0 ? 1f - Cooldown / cooldownLength : 1f, color = color } : ReadyOrCooldown();
    }

    // ====================================================================== Zabójca B – Otwarta tętnica

    /// <summary>Znacznik: cel spełnia próg egzekucji we wspólnym predykacie (<see cref="EnemyBrain.ExecuteQualified"/>), bez zmiany PŻ i faz.</summary>
    public sealed class OpenArteryFeature : ClassFeatureController
    {
        EnemyBrain target;
        public EnemyBrain Target => target != null && !target.IsDead && target.ArteryTime > 0f ? target : null;
        protected override void OnBind() { color = new Color(0.9f, 0.15f, 0.2f); EnemyBrain.ArteryExecute += OnExecute; }
        public override void Dispose() { base.Dispose(); EnemyBrain.ArteryExecute -= OnExecute; }
        void OnExecute(EnemyBrain e) { Telemetry.Add("tętnica: egzekucje poza progiem"); }

        protected override void OnPress()
        {
            var e = pc.FeatureTarget(T.arteryRange);
            if (e == null) { Fail("brak celu", "Otwarta tętnica: brak celu w zasięgu"); return; }
            if (Target != null) Target.ArteryTime = 0f;
            target = e;
            e.ArteryTime = T.arterySeconds;
            FxLibrary.Blood(e.AimPoint, Vector3.up, 1.5f);
            HellFx.Ambient(e.transform, Vector3.up * e.Def.scale, HellFxElement.Enemy, 0.35f * e.Def.scale, T.arterySeconds);
            Used(T.arteryCooldown, "OTWARTA TĘTNICA");
        }

        public override void FloorStart() { base.FloorStart(); target = null; }
        public override void Death() { if (Target != null) Target.ArteryTime = 0f; target = null; }
        public override ClassFeatureHud Hud => Target != null ? Timed(ClassFeatureState.Target, $"tętnica: {Target.DisplayName} {Target.ArteryTime:0.0} s", Target.ArteryTime, T.arterySeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Łucznik arkanów A – Strzała zaklęcia

    /// <summary>
    /// G → przygotowanie; przycisk slotu wybiera czar (tylko ofensywny skill HU; bez aur, leczenia, barier i przywołań) – bez rzutu.
    /// Następny podstawowy strzał (pierwszy pocisk salwy) płaci przy wypuszczeniu pełną cenę czaru i jego odnowienie, po ponownej
    /// walidacji (łuk zastępuje wyłącznie fokus). Ładunek wyzwala się raz, na pierwszym trafieniu, w jego miejscu, z mocą czaru
    /// bez drugiego mnożnika broni (wzorzec fokusu ×1). Pudło (ściana, koniec lotu) – ładunek przepada bez zwrotu.
    /// </summary>
    /// <summary>Uwolnienie czaru ze strzały (dane dla mocy unikatu).</summary>
    public sealed class SpellArrowRelease { public HuCast cast; public Vector3 from, at; public EnemyBrain target; }

    public sealed class SpellArrowFeature : ClassFeatureController
    {
        float preparing;
        SpellRuntime armed;
        struct Pending { public SpellRuntime spell; public float time; public Vector3 from; }
        readonly Dictionary<int, Pending> pending = new Dictionary<int, Pending>();
        static int tokens;
        public bool Preparing => preparing > 0f;
        public SpellRuntime Armed => armed;
        protected override bool AwaitingSecondPress => Preparing || armed != null;
        protected override void OnBind() { color = new Color(0.55f, 0.6f, 1f); }

        public static bool Compatible(SpellRuntime sk, out string why)
        {
            why = null;
            if (sk == null) { why = "pusty slot"; return false; }
            var d = sk.Def;
            if (!d.IsSpell) { why = $"{d.displayName} to technika, nie czar"; return false; }
            if (sk.instance.seals == null) { why = $"{d.displayName} nie mieści się w strzale"; return false; }
            switch (d.kind)
            {
                case SpellKind.Aura: case SpellKind.Heal: case SpellKind.Barrier: case SpellKind.Summon: case SpellKind.FrostArmor:
                case SpellKind.WeaponBuff: case SpellKind.Warcry: case SpellKind.BloodPact: case SpellKind.Overload: case SpellKind.Undying: case SpellKind.Dash:
                    why = $"{d.displayName}: tylko czar ofensywny"; return false;
            }
            return true;
        }

        bool Legal(SpellRuntime sk, out string why)
        {
            if (!Compatible(sk, out why)) return false;
            if (!sk.instance.Ready) { why = $"{sk.Def.displayName}: odnowienie"; return false; }
            if (pc.Mana.Current + 0.01f < sk.manaCost) { why = "za mało many"; return false; }
            var tags = Run.CurrentTags(pc.Build);
            if (!WeaponFamilies.Meets(sk.Def.requiredTags & ~BuildTag.Focus, tags)) { why = Names.Requirement(sk.Def.requiredTags); return false; }
            if (!sk.requirementsMet && pc.Config.balance.spellRequirementsAreHard) { why = "za mało Inteligencji"; return false; }
            return true;
        }

        protected override void OnPress()
        {
            if (Preparing) { preparing = 0f; CombatEvents.RaiseMessage("Strzała zaklęcia: anulowano", Color.gray); return; }
            if (armed != null) { armed = null; CombatEvents.RaiseMessage("Strzała zaklęcia: rozbrojona (bez kosztu)", Color.gray); return; }
            var w = pc.Build.weapon.light;
            if (w.delivery != AttackDelivery.Projectile || w.damageType != DamageType.Physical) { Fail("brak łuku", "Strzała zaklęcia: potrzebny łuk"); return; }
            bool any = false;
            for (int i = 0; i < RunState.SkillSlotCount; i++) if (Compatible(pc.Skill(i), out _)) any = true;
            if (!any) { Fail("brak czaru", "Strzała zaklęcia: brak czaru ofensywnego w slotach"); return; }
            preparing = T.spellArrowPrepare;
            CombatEvents.RaiseMessage("Strzała zaklęcia: wybierz czar przyciskiem slotu", color);
        }

        public override bool InterceptSkillRequest(int slot)
        {
            if (!Preparing) return false;
            var sk = pc.Skill(slot);
            if (!Legal(sk, out var why)) { Fail("niedozwolony czar", "Strzała zaklęcia: " + why); return true; }
            armed = sk; preparing = 0f;
            Telemetry.Add("strzała zaklęcia: wybrany " + sk.Def.id);
            CombatEvents.RaiseMessage($"Strzała zaklęcia: {sk.Def.displayName}", color);
            HellFx.Gather(pc.transform, HellSkillFx.Family(sk.Def), 0.6f);
            return true;
        }

        public override void BasicShot(ref HitData hit, int index, int count)
        {
            if (armed == null || index != 0) return;
            var sk = armed;
            armed = null;
            if (!Legal(sk, out var why)) { Fail("odmowa przy strzale", "Strzała zaklęcia: " + why + " – zwykła strzała"); return; }
            pc.Mana.TrySpend(sk.manaCost);
            sk.instance.TryUse();
            int token = ++tokens;
            hit.spellArrowToken = token;
            pending[token] = new Pending { spell = Normalized(sk), time = 4f, from = hit.sourcePosition };
            Telemetry.Add("strzała zaklęcia: opłacone", 1f);
            Used(0f);
            Telemetry.Add("strzała zaklęcia: mana", sk.manaCost);
        }

        /// <summary>Moc czaru bez mnożnika broni (łuk nie jest drugim fokusem): wzorzec ×1.</summary>
        static SpellRuntime Normalized(SpellRuntime sk)
        {
            float w = Mathf.Max(0.01f, sk.weaponScale);
            return new SpellRuntime { instance = sk.instance, manaCost = sk.manaCost, staminaCost = sk.staminaCost, power = sk.power / w, requirementsMet = sk.requirementsMet,
                equipmentMet = true, huScale = sk.huScale / w, weaponScale = 1f };
        }

        public override void HitResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (hit.spellArrowToken == 0 || !pending.TryGetValue(hit.spellArrowToken, out var p)) return;
            if (result.outcome == HitOutcome.Ignored || result.outcome == HitOutcome.Dodged) return;
            pending.Remove(hit.spellArrowToken); // dokładnie jedno uwolnienie
            Vector3 at = HuWorld.Ground(target.Transform.position);
            Vector3 dir = target.Transform.position - pc.transform.position; dir.y = 0f;
            dir = dir.sqrMagnitude > 1e-4f ? dir.normalized : pc.transform.forward;
            var cast = new HuCast(pc, p.spell, at, dir, at, 1f);
            // Łuk siódmej pieczęci / Amulet niedokończonej inkantacji: pas wzdłuż toru albo runa w miejscu trafienia.
            if (pc.UniquesFeatureEvent("spellarrow.release", new SpellArrowRelease { cast = cast, from = p.from, at = at, target = target as EnemyBrain })) { Telemetry.Add("strzała zaklęcia: uwolnienia"); return; }
            HuSkillCaster.Fire(cast, dir, p.spell.instance.seals.Shape.Parameters.GetF(HuParams.Range, 8f));
            Telemetry.Add("strzała zaklęcia: uwolnienia");
            HellFx.Shockwave(at, HellSkillFx.Family(p.spell.Def), p.spell.Def.color, 1.2f, false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (preparing > 0f) preparing -= dt;
            if (pending.Count == 0) return;
            foreach (var k in pending.Keys.ToList())
            {
                var p = pending[k]; p.time -= dt;
                if (p.time <= 0f) { pending.Remove(k); Telemetry.Add("strzała zaklęcia: pudła"); } else pending[k] = p;
            }
        }

        public override void FloorStart() { base.FloorStart(); preparing = 0f; armed = null; pending.Clear(); }
        public override void Death() { preparing = 0f; armed = null; pending.Clear(); }
        public override ClassFeatureHud Hud => Preparing ? Timed(ClassFeatureState.Preparing, "wybierz czar: przycisk slotu", preparing, T.spellArrowPrepare)
            : armed != null ? new ClassFeatureHud { state = ClassFeatureState.Stored, label = $"strzała: {armed.Def.displayName}", fill = 1f, color = armed.Def.color }
            : ReadyOrCooldown();
    }

    // ====================================================================== Łucznik arkanów B – Strzała odpowiedzi

    /// <summary>
    /// Ładunki na kilka ręcznych strzałów (salwa dzieli jeden ładunek). Przy kontakcie żywioł wybiera stan ofiary, w ustalonej
    /// kolejności: krwawienie → błyskawica, chłód → ogień, podpalenie → ziemia; bez warunku – zwykła strzała. Składnik fizyczny staje
    /// się magicznym w wybranym żywiole, premia do żywiołu liczona raz, potem zwykła odporność i reakcje celu. Rozstrzygane na końcu
    /// konwersji (po Przemianie w parę). Przebijający pocisk odpowiada każdej ofierze osobno.
    /// </summary>
    public sealed class AnsweringArrowFeature : ClassFeatureController
    {
        int charges; float life;
        public int Charges => charges;
        /// <summary>Koniec okna ładunków (czas gry) – odłożony pocisk nie przedłuża uprawnienia.</summary>
        public float WindowEnds => pc.Clock + (charges > 0 ? life : 0f);
        protected override void OnBind() { color = new Color(0.8f, 0.6f, 1f); }

        protected override void OnPress()
        {
            var w = pc.Build.weapon.light;
            if (w.delivery != AttackDelivery.Projectile) { Fail("brak łuku", "Strzała odpowiedzi: potrzebny łuk"); return; }
            charges = T.answerShots; life = T.answerSeconds;
            Used(T.answerCooldown, $"STRZAŁA ODPOWIEDZI ×{charges}");
        }

        public override void BasicShot(ref HitData hit, int index, int count)
        {
            if (charges <= 0) return;
            hit.answerCharge = true;
            if (index == count - 1) charges--; // jeden ręczny strzał = jeden ładunek, także dla salwy
        }

        /// <summary>Kolejność odpowiedzi – wspólna dla gry, opisu i testów.</summary>
        public static Element Answer(StatusEffects s)
        {
            if (s.BleedStacks > 0) return Element.Lightning;
            if (s.ChillStacks > 0 || s.Frozen) return Element.Fire;
            if (s.Burning) return Element.Earth;
            return Element.None;
        }

        public override void OutgoingAgainst(ref HitData hit, EnemyBrain target)
        {
            if (!hit.answerCharge || target == null) return;
            var el = Answer(target.Status);
            Telemetry.Add("odpowiedź: " + (el == Element.None ? "brak warunku" : Names.Element(el)));
            if (el == Element.None) return;
            float moved = hit.physical;
            hit.physical = 0f;
            if (hit.split.active) hit.AddMagic(el, moved); // podział wielu żywiołów (affiksy) – dokładny składnik
            else
            {
                if (hit.magic > 0f && hit.element != Element.None && hit.element != el) { hit.element2 = hit.element; hit.element2Share = hit.magic / (hit.magic + moved); }
                hit.magic += moved;
                hit.element = el;
            }
            hit.magic *= 1f + pc.Build.stats[Names.ElementDamageStat(el)] / 100f;
            HellFx.Impact(target.AimPoint, HellFx.For(el), Names.ElementColor(el), 0.9f);
        }

        public override void Tick(float dt) { base.Tick(dt); if (charges > 0) { life -= dt; if (life <= 0f) charges = 0; } }
        public override void FloorStart() { base.FloorStart(); charges = 0; }
        public override void Death() { charges = 0; }
        public override ClassFeatureHud Hud => charges > 0 ? Timed(ClassFeatureState.Stored, $"odpowiedź ×{charges} ({life:0} s)", life, T.answerSeconds) : ReadyOrCooldown();
    }
}
