using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Turris
{
    /// <summary>
    /// Poligon umiejętności (wzorem Hell Unleashed): arena bez fal, demony stoją jako manekiny, a panel listuje wszystkie
    /// umiejętności rodziny klasy – bazowej, jej specjalizacji i wspólne. Wybrana trafia do pierwszego slotu (Q), poprzednia
    /// do drugiego (E), na wybranym poziomie, bez kupowania. Bohater nie ginie, odnowienia i zasoby wracają od razu,
    /// manekiny wstają po chwili na swoim miejscu. Cele można ranić (krwawienie) i wychładzać, żeby było widać reakcje.
    /// „Pokaz” sam rzuca umiejętność z pierwszego slotu co kilka sekund.
    /// Klawiatura: [ ] – umiejętność, - = – poziom, T – cele, B – krwawienie, V – chłód, P – pokaz,
    /// przytrzymany Alt – kursor do klikania w panel (bohater i kamera stoją). Esc – pauza i wyjście.
    /// </summary>
    public class SkillRange
    {
        public enum TargetSet { Pack, Heavy, Colossus, None }

        public struct Entry { public string header; public SpellDefinition skill; }

        class Dummy
        {
            public EnemyDefinition def;
            public Vector3 spot;
            public float health;
            public EnemyBrain brain;
            public float standsUpAt = -1f;
        }

        const float StandUpSeconds = 1.6f, StatusEverySeconds = 1.5f, ShowEverySeconds = 2.4f;
        /// <summary>Cele stoją kilka kroków przed miejscem startu bohatera (ustalane w Begin).</summary>
        Vector3 centre;

        readonly GameRoot root;
        readonly List<Dummy> dummies = new List<Dummy>();
        float statusTimer, showTimer;

        public ClassDefinition Family { get; }
        public IReadOnlyList<ClassDefinition> Families { get; }
        public List<Entry> Entries { get; } = new List<Entry>();
        public int Selected { get; private set; } = -1;
        public int Level { get; private set; }
        public TargetSet Targets { get; private set; } = TargetSet.Pack;
        public bool Bleed { get; set; }
        public bool Chill { get; set; }
        public bool Show { get; private set; }
        /// <summary>Kursor wolny (przytrzymany Alt) – panel klikalny, bohater i kamera stoją.</summary>
        public bool PointerMode { get; private set; }
        public SpellDefinition SelectedSkill => Selected >= 0 && Selected < Entries.Count ? Entries[Selected].skill : null;

        public SkillRange(GameRoot root, ClassDefinition hero)
        {
            this.root = root;
            Family = hero != null ? hero.Family : root.config.classes.First(c => c.parent == null);
            Families = root.config.classes.Where(c => c.parent == null).ToList();
            BuildList();
        }

        // ------------------------------------------------------------------ Lista umiejętności

        /// <summary>Najpierw klasa bazowa, potem każda specjalizacja pod swoją nazwą, na końcu wspólne (ogólne i z nagród).</summary>
        void BuildList()
        {
            var cfg = root.config;
            var seen = new HashSet<SpellDefinition>();
            void Group(string header, IEnumerable<SpellDefinition> skills)
            {
                foreach (var s in skills.Where(s => s != null && seen.Add(s)).OrderBy(s => s.displayName))
                    Entries.Add(new Entry { header = header, skill = s });
            }
            IEnumerable<SpellDefinition> Own(ClassDefinition c)
            {
                if (c.signatureSkill != null) yield return c.signatureSkill;
                foreach (var s in c.startingSkillChoices) yield return s;
                foreach (var s in c.startingSpells) yield return s;
                foreach (var u in cfg.unlocks)
                    if (u.kind == UnlockKind.Spell && !u.IsGeneral && u.classes.Contains(c) && u.target is SpellDefinition sd) yield return sd;
            }
            Group(Family.displayName, Own(Family));
            foreach (var spec in cfg.classes.Where(c => c.parent == Family)) Group(spec.displayName, Own(spec));
            Group("Wspólne", cfg.unlocks.Where(u => u.kind == UnlockKind.Spell && u.IsGeneral).Select(u => u.target as SpellDefinition)
                .Concat(cfg.baseSpellPool));
        }

        // ------------------------------------------------------------------ Start i koniec

        public void Begin()
        {
            root.Player.Immortal = true;
            var start = root.Player.transform;
            centre = start.position + start.forward * 6f;
            PlaceTargets(TargetSet.Pack);
            Select(Entries.Count > 0 ? 0 : -1);
            CombatEvents.RaiseMessage("Poligon umiejętności", new Color(0.95f, 0.85f, 0.6f));
        }

        public void End()
        {
            SetPointerMode(false);
            if (root.Player != null) root.Player.Immortal = false;
            dummies.Clear();
        }

        // ------------------------------------------------------------------ Wybór

        public void Select(int index)
        {
            if (index < 0 || index >= Entries.Count) return;
            Selected = index;
            Level = Mathf.Min(Level, SelectedSkill.maxLevel);
            Equip();
        }

        public void Step(int delta)
        {
            if (Entries.Count == 0) return;
            Select(((Selected < 0 ? 0 : Selected) + delta + Entries.Count) % Entries.Count);
        }

        public void SetLevel(int level)
        {
            if (SelectedSkill == null) return;
            Level = Mathf.Clamp(level, 0, SelectedSkill.maxLevel);
            Equip();
        }

        /// <summary>Wybrana umiejętność do pierwszego slotu na wybranym poziomie; poprzednia przechodzi do drugiego.</summary>
        void Equip()
        {
            var run = root.Run;
            if (run == null || SelectedSkill == null) return;
            var previous = run.skillSlots[0];
            var inst = run.LearnSpell(SelectedSkill);
            inst.level = Level;
            inst.ResetCooldown();
            run.AssignSlot(inst, 0);
            if (previous != null && previous != inst && run.SlotOf(previous) < 0) run.AssignSlot(previous, 1);
            root.Player.RefreshBuild();
        }

        public SpellRuntime Equipped => root.Player != null ? root.Player.Skill(0) : null;

        // ------------------------------------------------------------------ Cele

        public void PlaceTargets(TargetSet set)
        {
            Targets = set;
            foreach (var d in dummies) if (d.brain != null) { root.RemoveRangeEnemy(d.brain); Object.Destroy(d.brain.gameObject); }
            dummies.Clear();
            var c = centre;
            switch (set)
            {
                case TargetSet.Pack:
                    dummies.Add(NewDummy("enemy_imp", c, 1f));
                    for (int i = 0; i < 6; i++)
                    {
                        float a = i * Mathf.PI * 2f / 6f;
                        dummies.Add(NewDummy(i % 3 == 2 ? "enemy_hu_hound" : "enemy_imp", c + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * 2.2f, 1f));
                    }
                    break;
                case TargetSet.Heavy:
                    for (int i = 0; i < 3; i++) dummies.Add(NewDummy("enemy_hu_brute", c + new Vector3((i - 1) * 2.6f, 0, Mathf.Abs(i - 1) * 0.9f), 3f));
                    break;
                case TargetSet.Colossus:
                    dummies.Add(NewDummy("enemy_hu_brute", c, 20f));
                    break;
            }
            dummies.RemoveAll(d => d.def == null);
            foreach (var d in dummies) StandUp(d);
        }

        public void NextTargets() => PlaceTargets((TargetSet)(((int)Targets + 1) % 4));

        Dummy NewDummy(string id, Vector3 spot, float health) =>
            new Dummy { def = root.config.enemyCatalog.FirstOrDefault(e => e != null && e.id == id), spot = spot, health = health };

        void StandUp(Dummy d)
        {
            d.brain = root.SpawnRangeDummy(d.def, d.spot, d.health);
            d.standsUpAt = -1f;
        }

        /// <summary>Manekin padł: ciało zostaje na chwilę, potem w tym samym miejscu staje nowy.</summary>
        public void OnDummyDied(EnemyBrain e)
        {
            var d = dummies.FirstOrDefault(x => x.brain == e);
            if (d != null) d.standsUpAt = Time.time + StandUpSeconds;
        }

        // ------------------------------------------------------------------ Klatka

        public void SetShow(bool on) { Show = on; showTimer = 0.6f; }

        public void Tick(float dt)
        {
            var player = root.Player;
            if (player == null || root.Run == null) return;
            HandleKeys();
            // Nic tu nie kończy sesji: zdrowie, mana i wytrzymałość pełne, każda umiejętność od razu gotowa.
            player.RestoreAll();
            foreach (var s in root.Run.skillSlots) s?.ResetCooldown();
            player.Skill(PlayerCombat.SignatureSlot)?.instance?.ResetCooldown();

            foreach (var d in dummies)
            {
                if (d.brain != null && !d.brain.IsDead) continue;
                if (d.standsUpAt < 0f) d.standsUpAt = Time.time + StandUpSeconds;
                else if (Time.time >= d.standsUpAt)
                {
                    if (d.brain != null) { root.RemoveRangeEnemy(d.brain); Object.Destroy(d.brain.gameObject, 2f); }
                    StandUp(d);
                }
            }

            statusTimer -= dt;
            if (statusTimer <= 0f && (Bleed || Chill))
            {
                statusTimer = StatusEverySeconds;
                var b = root.config.balance;
                foreach (var d in dummies)
                {
                    if (d.brain == null || d.brain.IsDead) continue;
                    if (Bleed) d.brain.Status.Apply(StatusKind.Bleed, 25f, 0f, b);
                    if (Chill) d.brain.Status.Apply(StatusKind.Chill, 0f, 25f, b);
                }
            }

            if (Show && !PointerMode)
            {
                showTimer -= dt;
                if (showTimer <= 0f && player.Actions.IsIdle)
                {
                    showTimer = ShowEverySeconds;
                    if (root.Controller.LockTarget == null && dummies.Any(d => d.brain != null && !d.brain.IsDead)) root.Controller.ToggleLock();
                    player.RequestSkill(0);
                }
            }
        }

        void HandleKeys()
        {
            var k = Keyboard.current;
            if (k == null) return;
            SetPointerMode(k.leftAltKey.isPressed || k.rightAltKey.isPressed);
            if (k.leftBracketKey.wasPressedThisFrame) Step(-1);
            if (k.rightBracketKey.wasPressedThisFrame) Step(1);
            if (k.minusKey.wasPressedThisFrame) SetLevel(Level - 1);
            if (k.equalsKey.wasPressedThisFrame) SetLevel(Level + 1);
            if (k.tKey.wasPressedThisFrame) NextTargets();
            if (k.bKey.wasPressedThisFrame) Bleed = !Bleed;
            if (k.vKey.wasPressedThisFrame) Chill = !Chill;
            if (k.pKey.wasPressedThisFrame) SetShow(!Show);
        }

        void SetPointerMode(bool on)
        {
            if (on == PointerMode) return;
            PointerMode = on;
            if (root.Controller != null) root.Controller.InputEnabled = !on && root.Screen == GameScreen.Playing;
            Cursor.lockState = on ? CursorLockMode.None : (root.Screen == GameScreen.Playing ? CursorLockMode.Locked : CursorLockMode.None);
            Cursor.visible = on || root.Screen != GameScreen.Playing;
        }
    }
}
