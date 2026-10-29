using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Bot Maga (class_mage): różdżka i grymuar jednego z czterech żywiołów (grymuar daje novę), Arkanowa osłona, mrugnięcie
    /// zamiast przewrotu, skill klasy Przeciążenie (następny czar mocniejszy i szerszy).
    /// Walka na dystans z doświadczeń wojownika: trzyma 4,5–9 m od celu, strzela różdżką w pasie dystansu, odchodzi od tłumu
    /// w stronę środka areny, novą odpowiada na okrążenie, pije flaszkę many w spokojnej chwili. Sklep i przedmioty ocenia
    /// pod moc czarów i własny żywioł (premie fizyczne mu nic nie dają).
    /// </summary>
    public class MageBot : HeroBot
    {
        public const string ClassId = "class_mage";
        public const int SignatureSlot = 3;

        /// <summary>Pas dystansu do celu: bliżej – odwrót, dalej – podejście.</summary>
        const float KeepMin = 4.5f, KeepMax = 9f;
        /// <summary>Promień, w którym bot chce wracać ku środkowi areny zamiast cofać się pod ścianę.</summary>
        const float ArenaComfort = 9f;

        float nextShotAt;

        // Wabienie pod czar obszarowy: od kiedy trwa, do kiedy przerwa po nieudanej próbie, kiedy wolno kolejne mrugnięcie.
        float lureSince = -1f, lurePauseUntil, nextLureDodgeAt;
        /// <summary>Dlaczego czar obszarowy został wybrany – zapisywane do telemetrii dopiero po faktycznym rzucie.</summary>
        string areaReason;
        const float LureTimeout = 5f, LurePause = 3f;

        // Tester: żywioły po kolei; gracz: najpierw każdy, potem najlepszy (epsilon-zachłannie, jak wojownik).
        public override void PreparePlan(LoadoutPlan plan, int runIndex, IReadOnlyList<BotRunRecord> history)
        {
            var cls = plan.classDef;
            int combos = Mathf.Max(1, cls.loadouts.Count);
            int pick;
            if (P.persona == BotPersona.Tester || runIndex < combos) pick = runIndex % combos;
            else
            {
                var score = new float[combos];
                var n = new int[combos];
                foreach (var r in history)
                {
                    int c = cls.loadouts.FindIndex(x => LoadoutName(x) == r.loadout);
                    if (c < 0) continue;
                    score[c] += r.floorReached + (r.Victory ? 2f : 0f);
                    n[c]++;
                }
                pick = Enumerable.Range(0, combos).OrderByDescending(c => n[c] > 0 ? score[c] / n[c] : 99f).First();
                if (rng.NextDouble() < 0.15) pick = rng.Next(combos);
            }
            Root.Meta.SelectLoadout(plan, pick);
            if (cls.startingSkillChoices.Count > 0) Root.Meta.SelectStartingSkill(plan, cls.startingSkillChoices[0]);
        }

        // Gracz: Inteligencja (moc czarów, mana) i co trzeci punkt Wytrzymałość – mag ma mało życia. Tester rotuje wszystkie.
        protected override AttributeType NextAttribute(int i)
        {
            if (P.persona == BotPersona.Tester)
            {
                var all = new[] { AttributeType.Intelligence, AttributeType.Toughness, AttributeType.Dexterity, AttributeType.Strength };
                return all[i % all.Length];
            }
            return i % 3 == 2 ? AttributeType.Toughness : AttributeType.Intelligence;
        }

        Element WandElement => Pc.Build.weapon.light.element;

        // ------------------------------------------------------------------ Walka

        protected override void OnFight()
        {
            base.OnFight();
            TryManaFlask();
        }

        /// <summary>Flaszka many: mało many, nikt nie stoi blisko i żaden zauważony cios nie nadchodzi.</summary>
        void TryManaFlask()
        {
            if (Root.Run.manaFlasks <= 0 || Pc.Mana.Fraction > 0.2f || !Pc.Actions.IsIdle) return;
            if (EnemiesWithin(4f) > 0) return;
            if (swings.Any(s => now >= s.noticeAt && Threatens(s) && TimeToImpact(s) < 1f)) return;
            Pc.RequestFlask(true);
            Rec.R.actions.Add("flaszka many (próba)");
        }

        protected override bool TryUseSkills()
        {
            if (rng.NextDouble() > P.skillEagerness && P.persona != BotPersona.Tester) return false;
            float d = target != null ? FlatDist(target) : 99f;
            int close = EnemiesWithin(3.6f);
            float hp = Pc.Health.Fraction;
            bool tester = P.persona == BotPersona.Tester;

            // Przeciążenie: przed mocnym czarem – grupa w zasięgu albo boss, i jest mana na czar zaraz po nim.
            var sig = Pc.Skill(SignatureSlot);
            if (sig != null && sig.Def.kind == SpellKind.Overload && !Pc.OverloadReady)
            {
                bool spellReady = Enumerable.Range(0, RunState.SkillSlotCount).Any(s => Pc.Skill(s) != null && Pc.SkillUsable(s) && !Pc.Skill(s).Def.IsAura);
                bool worth = EnemiesWithin(6f) >= 3 || (target != null && target.Def.isBoss && d < 10f) || (tester && close >= 1);
                if (spellReady && worth && Pc.Mana.Fraction > 0.4f && UseSkill(SignatureSlot)) return true;
            }
            else if (sig != null && sig.Def.kind != SpellKind.Overload)
            {
                // Specjalizacja ze skillem klasy innego rodzaju (meteor, wir, nawałnica…) – zwykłe zasady.
                if (WantsSkill(sig.Def, d, close, hp) && UseSkill(SignatureSlot)) return true;
            }

            for (int slot = 0; slot < RunState.SkillSlotCount; slot++)
            {
                var sk = Pc.Skill(slot);
                if (sk == null || !Pc.SkillUsable(slot)) continue;
                areaReason = null;
                bool want = WantsSkill(sk.Def, d, close, hp);
                // Po Przeciążeniu: pierwszy czar w zasięgu, nawet gdy zwykle by poczekał (czar obszarowy – nadal tylko na grupę albo w ratunku).
                if (!want && Pc.OverloadReady && sk.Def.IsSpell && !sk.Def.IsAura && !SelfArea(sk.Def)) want = d <= Mathf.Max(sk.Def.attack.reach, sk.Def.attack.radius, 4f);
                if (want && UseSkill(slot))
                {
                    if (areaReason != null) { Rec.R.actions.Add(areaReason); if (areaReason.Contains("zwabieniu")) lureSince = -1f; }
                    return true;
                }
            }
            return false;
        }

        /// <summary>Czar obszarowy wokół maga (nova, trzęsienie, młynek) – ten wymaga zebrania wrogów.</summary>
        static bool SelfArea(SpellDefinition def) =>
            def.kind == SpellKind.Nova || def.kind == SpellKind.Quake || def.kind == SpellKind.Whirlwind;

        static float AreaRadius(SpellDefinition def) => Mathf.Max(def.attack.radius, def.attack.reach * 0.5f, 2f);

        /// <summary>Życie zależy od tej chwili: mało życia albo zauważony cios zaraz dojdzie przy połowie życia.</summary>
        bool LifeAtStake => Pc.Health.Fraction < 0.35f ||
            (Pc.Health.Fraction < 0.55f && swings.Any(s => now >= s.noticeAt && Threatens(s) && TimeToImpact(s) < 0.6f));

        /// <summary>Czar obszarowy gotowy, na arenie kilku wrogów, a pod czarem mniej niż dwóch – trzeba ich zwabić.</summary>
        SpellDefinition AreaToLureFor()
        {
            if (now < lurePauseUntil || alive.Count < 2) return null;
            for (int slot = 0; slot < RunState.SkillSlotCount; slot++)
            {
                var sk = Pc.Skill(slot);
                if (sk != null && SelfArea(sk.Def) && Pc.SkillUsable(slot) && EnemiesWithin(AreaRadius(sk.Def)) < 2) return sk.Def;
            }
            return null;
        }

        /// <summary>
        /// Czar obszarowy wokół siebie: co najmniej dwóch wrogów w promieniu. Na jednego – tylko gdy od tego zależy życie
        /// albo gdy na arenie nie ma nikogo więcej (nie ma kogo zwabić). Inaczej mag wabi (<see cref="Lure"/>).
        /// </summary>
        protected override bool WantsSkill(SpellDefinition def, float d, int close, float hp)
        {
            if (!SelfArea(def)) return base.WantsSkill(def, d, close, hp);
            int inside = EnemiesWithin(AreaRadius(def));
            if (inside >= 2) { areaReason = lureSince >= 0f ? "czar obszarowy po zwabieniu" : "czar obszarowy na grupę"; return true; }
            if (inside == 0) return false;
            if (LifeAtStake) { areaReason = "czar obszarowy na jednego (ratunek)"; return true; }
            if (alive.Count <= 1) { areaReason = "czar obszarowy na jedynego wroga"; return true; } // jedyny wróg na arenie – nie ma kogo wabić
            if (P.persona == BotPersona.Tester && rng.NextDouble() < 0.25) { areaReason = "czar obszarowy na jednego (test)"; return true; }
            return false;
        }

        /// <summary>
        /// Wabienie pod czar obszarowy: mag przestaje uciekać i pozwala wrogom podejść; gdy przy nim jest jeden, a reszta dalej –
        /// mrugnięcie od środka grupy (wszyscy gonią z jednej strony i schodzą się razem). Po 5 s bez skutku – zwykła walka na dystans.
        /// </summary>
        bool Lure()
        {
            var area = AreaToLureFor();
            if (area == null) { lureSince = -1f; return false; }
            if (lureSince < 0f) lureSince = now;
            if (now - lureSince > LureTimeout)
            {
                Rec.R.actions.Add("wabienie bez skutku");
                lureSince = -1f; lurePauseUntil = now + LurePause;
                return false;
            }
            float r = AreaRadius(area);
            float nearest = alive.Select(FlatDist).DefaultIfEmpty(99f).Min();
            int inside = EnemiesWithin(r);
            if (inside == 1 && nearest < 1.6f && now >= nextLureDodgeAt && Pc.Stamina.Fraction > 0.35f)
            {
                Vector3 away = AwayFrom(CrowdCenter(99f)); // od środka wszystkich wrogów na arenie
                Vector3 p = transform.position; p.y = 0f;
                if (p.magnitude > ArenaComfort) away = (away - p.normalized).normalized; // nie pod ścianę
                if (RequestDodge(away, "wabienie pod czar obszarowy")) { nextLureDodgeAt = now + 1.8f; return true; }
            }
            // Czekanie na podejście: lekkie krążenie, strzały w najbliższego.
            Vector3 to = DirTo(target.transform.position);
            MoveWorld = Strafe(to) * 0.35f;
            if (Pc.Stamina.Fraction > P.lowStamina && CanShoot()) Shoot();
            return true;
        }

        /// <summary>
        /// Walka na dystans: za blisko albo w tłumie – odwrót z odchyleniem w bok i ku środkowi areny; za daleko – podejście;
        /// w pasie 4,5–9 m – strzały różdżką z lekkim krążeniem. Wytrzymałość zostaje na mrugnięcie.
        /// </summary>
        protected override void Offense()
        {
            if (Pc.Build.weapon.light.delivery != AttackDelivery.Projectile) { base.Offense(); return; } // broń wręcz kupiona po drodze
            if (!LifeAtStake && Lure()) return;
            float d = FlatDist(target);
            Vector3 to = DirTo(target.transform.position);
            int crowd = EnemiesWithin(3.2f);
            float stam = Pc.Stamina.Fraction;

            if (d < KeepMin || crowd >= 2)
            {
                Vector3 back = -to * 0.85f + Strafe(to) * 0.5f;
                Vector3 p = transform.position; p.y = 0f;
                if (p.magnitude > ArenaComfort) back += -p.normalized * Mathf.Clamp01((p.magnitude - ArenaComfort) / 4f) * 1.2f;
                MoveWorld = back.normalized;
                SprintHeld = d < 2.5f && stam > 0.5f;
                // Przyparty (ktoś tuż obok, a odwrót nie idzie): strzał z bliska zamiast biernego cofania.
                if (d < 1.8f && stam > P.lowStamina && CanShoot()) Shoot();
                return;
            }
            if (d > KeepMax)
            {
                MoveWorld = to;
                SprintHeld = d > 13f && stam > 0.55f;
                return;
            }
            MoveWorld = Strafe(to) * 0.4f;
            if (stam < P.lowStamina) return;
            if (CanShoot()) Shoot();
        }

        bool CanShoot() => now >= nextShotAt && (Pc.Actions.IsIdle ||
            (Pc.Actions.Current == ActionType.LightAttack && Pc.Actions.Phase == ActionPhase.Recovery));

        void Shoot()
        {
            Pc.Request(ActionType.LightAttack);
            nextShotAt = now + 0.1f;
        }

        // ------------------------------------------------------------------ Sklep i przedmioty

        StatType WandElementStat => Names.ElementDamageStat(WandElement);

        static readonly PassiveEffectType[] WandMechanics =
        {
            PassiveEffectType.ProjectilePierce, PassiveEffectType.ProjectileRicochet, PassiveEffectType.ProjectileVolley,
            PassiveEffectType.ProjectileSplit, PassiveEffectType.ProjectileExplosion, PassiveEffectType.ProjectileResidue, PassiveEffectType.AreaSize,
        };

        PassiveEffectType OwnPenetration => WandElement == Element.Fire ? PassiveEffectType.PenetrationFire
            : WandElement == Element.Frost ? PassiveEffectType.PenetrationFrost
            : WandElement == Element.Lightning ? PassiveEffectType.PenetrationLightning
            : WandElement == Element.Earth ? PassiveEffectType.PenetrationEarth : PassiveEffectType.PenetrationPhysical;

        /// <summary>Gust maga: moc czarów, własny żywioł, mechanika pocisków różdżki i odnowienia; potem życie i obrona; mana.</summary>
        protected override float PerkScore(BoonDefinition b)
        {
            if (!PerkMechanics.Active(b, Root.Run)) return 0f; // uśpiony przy tym wyposażeniu – nic nie da
            float s = 1f;
            if (b.modifiers.Any(m => m.stat == StatType.SpellPower || m.stat == StatType.GlobalDamage || m.stat == WandElementStat
                                     || m.stat == StatType.CooldownReduction || m.stat == StatType.AttackSpeed)
                || b.effects.Any(e => WandMechanics.Contains(e.type) || e.type == OwnPenetration))
                s = 3f;
            else if (b.modifiers.Any(m => m.stat == StatType.MaxHealth || m.stat == StatType.DamageReduction || m.stat == StatType.PhysicalDefense)
                     || b.effects.Any(e => e.type == PassiveEffectType.Evasion))
                s = 2.5f;
            else if (b.modifiers.Any(m => m.stat == StatType.MaxMana || m.stat == StatType.ManaRegen))
                s = 2f;
            return s + Root.Run.BoonStacks(b) * 0.5f;
        }

        /// <summary>Wartość buildu maga: DPS różdżki z mocą czarów i premią żywiołu, życie, obrona, mana.</summary>
        protected override float GearScore()
        {
            var b = BuildCalculator.Compute(Root.Run, Root.config);
            var s = b.stats;
            float elem = 1f + (WandElement != Element.None ? s[WandElementStat] / 100f : 0f);
            float dps = b.WeaponDamage(b.weapon.light) * b.attackSpeed * elem * (1f + s[StatType.SpellPower] / 100f);
            return dps + 0.3f * s[StatType.MaxHealth] + 0.4f * (s[StatType.PhysicalDefense] + s[StatType.MagicDefense])
                   + 0.15f * s[StatType.MaxMana] + 2f * s[StatType.ManaRegen] + 0.1f * s[StatType.MaxStamina];
        }
    }
}
