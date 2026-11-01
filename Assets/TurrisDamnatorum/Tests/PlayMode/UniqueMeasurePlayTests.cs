using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>
    /// Pomiar kandydatów na dominację wśród unikatów (docs/AUDYT_BUILDCRAFT.md, etap 2): ten sam build i ta sama polityka
    /// z mocą i bez mocy (<see cref="PlayerCombat.DebugNoUniquePowers"/> – statystyki unikatu zostają), 20 s walki w dwóch
    /// kontekstach: grupa czterech wrogów o zwykłej postawie i jeden twardy cel (postawa bossa). Miarą jest życie stracone przez
    /// wrogów (wszystkie źródła: ciosy, statusy, sługi, moce). Wrogowie nie atakują – kontekst obronny nie jest tu mierzony.
    /// Uruchomienie na żądanie: TURRIS_MEASURE=&lt;katalog&gt; (bez zmiennej test jest pomijany).
    /// </summary>
    public class UniqueMeasurePlayTests
    {
        const float FightSeconds = 20f;
        readonly List<Object> cleanup = new List<Object>();
        DefaultContent.Bundle bundle;

        [SetUp] public void SetUp() { Time.timeScale = 1f; bundle = TestContent.Create(); EnemyBrain.DebugNoEvasion = true; UniqueRegistry.ResetForTests(); }

        [TearDown]
        public void TearDown() { Clean(); EnemyBrain.DebugNoEvasion = false; EnemyBrain.BleedRateProvider = null; }

        void Clean()
        {
            Servant.DismissAll(); FeatureDecoy.ClearAll(); PinnedShadow.ClearAll(); EarthWall.ClearAll(); HuWorld.Clear();
            foreach (var p in Projectile.Active.ToArray()) if (p != null) Object.Destroy(p.gameObject);
            foreach (var z in DamageZone.Active.ToArray()) if (z != null) Object.Destroy(z.gameObject);
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
        }

        /// <summary>Kandydat: karta, wyposażenie, skille w slotach, polityka (t, pc, z mocą?) wołana co klatkę.</summary>
        class Case
        {
            public string code, main, off; public string[] skills;
            public bool startInRage;
            public bool generic; // rdzeń z testu dymnego, wróg atakuje, bohater nieśmiertelny, ale przyjmuje ciosy
            public string baselineMain; // broń wariantu bez mocy, gdy moc zastępuje broń (T01: pięści kontra miecz)
            public Action<PlayerCombat, bool, Dictionary<string, float>> policy;
        }

        static void Light(PlayerCombat pc) { if (pc.Actions.IsIdle) pc.Request(ActionType.LightAttack); }

        static readonly Case[] Cases =
        {
            // B02: z mocą – Spalenie furii w szale (widma); bez mocy – szał zostaje (premia szybkości). Każdy wariant w swojej najlepszej polityce.
            new Case { code = "B02", main = "hu_greataxe", skills = new string[0], startInRage = true,
                policy = (pc, on, m) => { if (on && pc.Raging && pc.Actions.IsIdle) pc.RequestSkill(PlayerCombat.SignatureSlot); else Light(pc); } },
            // R03: co trzeci cios mocny (orbita drugiej broni).
            new Case { code = "R03", main = "hu_sword", off = "hu_dagger", skills = new string[0],
                policy = (pc, on, m) => { if (!pc.Actions.IsIdle) return; m.TryGetValue("n", out var n); m["n"] = n + 1; pc.Request(((int)n) % 3 == 2 ? ActionType.HeavyAttack : ActionType.LightAttack); } },
            // R05: Młynek, gdy gotowy, inaczej lekkie.
            new Case { code = "R05", main = "hu_sword", off = "hu_dagger", skills = new[] { "hu_whirlwind" },
                policy = (pc, on, m) => { if (pc.Actions.IsIdle && pc.SkillUsable(0)) pc.RequestSkill(0); else Light(pc); } },
            // B05: ładowany mocny atak (1,4 s), w szale.
            new Case { code = "B05", main = "hu_greataxe", skills = new string[0], startInRage = true,
                policy = (pc, on, m) =>
                {
                    m.TryGetValue("hold", out var hold);
                    if (hold > 0f) { hold -= Time.deltaTime; m["hold"] = hold; pc.HeavyHeld = hold > 0f; return; }
                    if (pc.Actions.IsIdle) { pc.HeavyHeld = true; pc.Request(ActionType.HeavyAttack); m["hold"] = 1.4f; }
                } },
            // S03: Sfora, gdy gotowa, inaczej lekkie sztyletem (komendy sług z trafień właściciela).
            new Case { code = "S03", main = "hu_dagger", off = "hu_grimoire_summoning", skills = new[] { "hu_summon_hounds" },
                policy = (pc, on, m) => { if (pc.Actions.IsIdle && pc.SkillUsable(0)) pc.RequestSkill(0); else Light(pc); } },
            // Wyniki wystające ponad szum ogólnej rotacji.
            new Case { code = "P02", main = "hu_morningstar", off = "hu_shield", skills = new[] { "hu_aura_might", "hu_aura_holy_fire" }, policy = Rotation, generic = true },
            new Case { code = "F04", main = "hu_morningstar", skills = new[] { "hu_nova_fire" }, policy = Rotation, generic = true },
            // T01 pod moc: Arkana włączone raz, czary z karty (prosty, obrót) i lekkie między nimi; miecz wyposażony w te same czary.
            new Case { code = "T01", main = "", baselineMain = "hu_sword", skills = new[] { "hu_frost_shard", "hu_nova_fire" }, generic = true,
                policy = (pc, on, m) =>
                {
                    if (!pc.Actions.IsIdle) return;
                    if (!m.ContainsKey("armed")) { m["armed"] = 1f; pc.RequestSkill(PlayerCombat.SignatureSlot); return; }
                    for (int i = 0; i < 2; i++) if (pc.SkillUsable(i)) { pc.RequestSkill(i); if (!pc.Actions.IsIdle) return; }
                    Light(pc);
                } },
        };

        static IEnumerable<string> Codes => Cases.Select(c => c.code);

        [UnityTest, Timeout(30 * 60 * 1000)]
        public IEnumerator Candidates_PowerOnVsOff_GroupAndHardTarget()
        {
            string dir = Environment.GetEnvironmentVariable("TURRIS_MEASURE");
            if (string.IsNullOrEmpty(dir)) Assert.Ignore("Ustaw TURRIS_MEASURE=<katalog>, aby zmierzyć kandydatów na dominację.");
            Directory.CreateDirectory(dir);
            var sb = new StringBuilder();
            sb.AppendLine("| Kod | Unikat | Kontekst | Bez mocy | Z mocą | Zysk mocy |");
            sb.AppendLine("|---|---|---|---:|---:|---:|");
            string only = Environment.GetEnvironmentVariable("TURRIS_MEASURE_ONLY");
            int.TryParse(Environment.GetEnvironmentVariable("TURRIS_MEASURE_REPEATS"), out int repeats);
            repeats = Mathf.Max(1, repeats);
            if (repeats > 1) { Time.captureFramerate = 60; LogAssert.ignoreFailingMessages = true; }
            foreach (var c in Cases.Where(x => string.IsNullOrEmpty(only) || only.Split(',').Contains(x.code)))
                foreach (bool group in new[] { true, false })
                {
                    var offs = new List<float>(); var ons = new List<float>();
                    for (int r = 0; r < repeats; r++)
                    {
                        yield return Fight(c, false, group, d => offs.Add(d));
                        yield return Fight(c, true, group, d => ons.Add(d));
                    }
                    float off = offs.Average(), on = ons.Average();
                    var card = UniqueCatalog.All.First(x => x.code == c.code);
                    string gain = off > 0 ? $"×{on / off:0.00}" : "–";
                    if (repeats > 1) gain += $" (bez mocy {offs.Min():0}–{offs.Max():0}, z mocą {ons.Min():0}–{ons.Max():0})";
                    string line = $"| {c.code} | {card.name} | {(group ? "grupa 4 (postawa 35)" : "twardy cel (postawa 150)")} | {off:0} | {on:0} | {gain} |";
                    sb.AppendLine(line);
                    TestContext.WriteLine(line);
                    Assert.Greater(off + on, 0f, c.code + ": walka zadała obrażenia");
                }
            Time.captureFramerate = 0; LogAssert.ignoreFailingMessages = false;
            File.WriteAllText(Path.Combine(dir, string.IsNullOrEmpty(only) ? "pomiar_unikatow.md" : $"pomiar_{only.Replace(',', '_')}.md"), sb.ToString());
        }

        /// <summary>Ogólna rotacja: cecha co 6 s, gotowe skille, co trzeci cios mocny (ta sama dla każdej mocy).</summary>
        static void Rotation(PlayerCombat pc, bool on, Dictionary<string, float> m)
        {
            m.TryGetValue("t", out var t); t += Time.deltaTime; m["t"] = t;
            if (!pc.Actions.IsIdle) return;
            m.TryGetValue("press", out var press);
            if (t >= press) { m["press"] = t + 6f; pc.RequestSkill(PlayerCombat.SignatureSlot); if (!pc.Actions.IsIdle) return; }
            for (int i = 0; i < RunState.SkillSlotCount; i++)
            {
                var sk = pc.Skill(i);
                if (sk != null && !sk.Def.IsAura && pc.SkillUsable(i)) { pc.RequestSkill(i); if (!pc.Actions.IsIdle) return; }
            }
            m.TryGetValue("n", out var n); m["n"] = n + 1;
            pc.Request(((int)n) % 3 == 2 ? ActionType.HeavyAttack : ActionType.LightAttack);
        }

        /// <summary>Wszystkie moce tą samą rotacją (przybliżenie): obok zysku liczba rodzajów zdarzeń mocy – 0 znaczy „warunek nie zaszedł”, nie „moc słaba”.</summary>
        [UnityTest, Timeout(6 * 60 * 60 * 1000)]
        public IEnumerator AllPowers_GenericRotation()
        {
            string dir = Environment.GetEnvironmentVariable("TURRIS_MEASURE_ALL");
            if (string.IsNullOrEmpty(dir)) Assert.Ignore("Ustaw TURRIS_MEASURE_ALL=<katalog>, aby zmierzyć wszystkie moce ogólną rotacją.");
            Directory.CreateDirectory(dir);
            Time.captureFramerate = 60;
            LogAssert.ignoreFailingMessages = true;
            var sb = new StringBuilder();
            sb.AppendLine("| Kod | Unikat | Grupa | Twardy cel | Zdarzenia mocy (grupa/twardy) |");
            sb.AppendLine("|---|---|---:|---:|---:|");
            try
            {
                foreach (var card in UniqueCatalog.All)
                {
                    UniqueSmokePlayTests.Builds.TryGetValue(card.code, out var b);
                    var c = new Case { code = card.code, main = b.main, off = b.off, skills = b.skills ?? new string[0], policy = Rotation, generic = true };
                    float gOff = 0, gOn = 0, hOff = 0, hOn = 0; int gEv = 0, hEv = 0;
                    yield return Fight(c, false, true, d => gOff = d);
                    yield return Fight(c, true, true, d => gOn = d); gEv = lastEvents;
                    yield return Fight(c, false, false, d => hOff = d);
                    yield return Fight(c, true, false, d => hOn = d); hEv = lastEvents;
                    sb.AppendLine($"| {card.code} | {card.name} | {(gOff > 0 ? $"×{gOn / gOff:0.00}" : "–")} | {(hOff > 0 ? $"×{hOn / hOff:0.00}" : "–")} | {gEv}/{hEv} |");
                    File.WriteAllText(Path.Combine(dir, "pomiar_wszystkich.md"), sb.ToString()); // na bieżąco – długi przebieg
                }
            }
            finally { Time.captureFramerate = 0; LogAssert.ignoreFailingMessages = false; }
        }

        int lastEvents;

        /// <summary>
        /// Droga fizyczna kontra żywiołowa na prawdziwych demonach z katalogu (audyt, etap 3): te same sloty T3 (średnie rzuty), prawdziwe
        /// odporności i pancerz, 6 s lekkich ciosów. Wrogowie stoją i nie atakują; statusy i reakcje poza zakresem (dodanie żywiołu ich nie nakłada).
        /// </summary>
        [UnityTest, Timeout(3 * 60 * 60 * 1000)]
        public IEnumerator Routes_PhysicalVsElemental_RealDemons()
        {
            string dir = Environment.GetEnvironmentVariable("TURRIS_MEASURE_ROUTES");
            if (string.IsNullOrEmpty(dir)) Assert.Ignore("Ustaw TURRIS_MEASURE_ROUTES=<katalog>, aby porównać drogi obrażeń.");
            Directory.CreateDirectory(dir);
            Time.captureFramerate = 60;
            LogAssert.ignoreFailingMessages = true;
            var config = bundle.config;
            StatModifier M(StatType s, float v) => new StatModifier(s, v);
            var routes = new (string name, StatModifier[] mods)[]
            {
                ("fizyczna 6", new[] { M(StatType.PhysicalDamage, 271f) }),
                ("ogień 6", new[] { M(StatType.AddedWeaponFire, 161f), M(StatType.FireDamage, 45f) }),
                ("mróz 6", new[] { M(StatType.AddedWeaponFrost, 161f), M(StatType.FrostDamage, 45f) }),
                ("fizyczna pełna", new[] { M(StatType.PhysicalDamage, 332.5f) }),
                ("ogień pełna", new[] { M(StatType.AddedWeaponFire, 201.25f), M(StatType.FireDamage, 135f) }),
                ("mróz pełna", new[] { M(StatType.AddedWeaponFrost, 201.25f), M(StatType.FrostDamage, 135f) }),
            };
            var demons = config.enemyCatalog.Where(d => d != null && d.maxHealth > 0f).ToList();
            var sums = new Dictionary<string, float>();
            var sb = new StringBuilder();
            sb.AppendLine("| Demon | Odporność fiz. | Ogień × | Mróz × | " + string.Join(" | ", routes.Select(r => r.name)) + " |");
            sb.AppendLine("|---|---:|---:|---:|" + string.Concat(routes.Select(_ => "---:|")));
            try
            {
                foreach (var demon in demons)
                {
                    var cells = new List<string>();
                    foreach (var route in routes)
                    {
                        float lost = 0f;
                        yield return RouteFight(demon, route.mods, d => lost = d);
                        cells.Add(lost.ToString("0"));
                        sums.TryGetValue(route.name, out var sum); sums[route.name] = sum + lost;
                    }
                    sb.AppendLine($"| {demon.displayName} | {demon.physicalResist:0.00} | {demon.fireMultiplier:0.00} | {demon.frostMultiplier:0.00} | {string.Join(" | ", cells)} |");
                    File.WriteAllText(Path.Combine(dir, "drogi_obrazen.md"), sb.ToString());
                }
                sb.AppendLine();
                sb.AppendLine($"Średnio na demona: " + string.Join(", ", routes.Select(r => $"{r.name} {sums[r.name] / demons.Count:0}")));
                File.WriteAllText(Path.Combine(dir, "drogi_obrazen.md"), sb.ToString());
            }
            finally { Time.captureFramerate = 0; LogAssert.ignoreFailingMessages = false; }
        }

        IEnumerator RouteFight(EnemyDefinition demon, StatModifier[] mods, Action<float> done)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0); floor.transform.localScale = new Vector3(80, 1, 80); cleanup.Add(floor);
            var camGo = new GameObject("TestCam"); camGo.AddComponent<Camera>(); var rig = camGo.AddComponent<CameraRig>(); cleanup.Add(camGo);
            var p = WorldBuilder.CreatePlayer(); cleanup.Add(p);
            var ctrl = p.GetComponent<PlayerController>(); ctrl.cameraRig = rig; ctrl.InputEnabled = false; rig.target = p.transform; rig.player = ctrl;
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("class_knight"), difficulty = bundle.config.difficulties[0] };
            var run = RunFactory.Create(plan, bundle.config, 7);
            run.attributes.strength = 30; run.attributes.dexterity = 30;
            run.equipment.Unequip(EquipSlot.MainHand); run.equipment.Unequip(EquipSlot.OffHand);
            var sword = new ItemInstance(bundle.Get<ItemDefinition>("hu_sword")); run.inventory.Add(sword); Assert.IsTrue(run.EquipFromInventory(sword, EquipSlot.MainHand));
            var boon = ScriptableObject.CreateInstance<BoonDefinition>(); cleanup.Add(boon);
            boon.id = boon.name = "test_droga"; boon.displayName = "Test"; boon.maxStacks = 1;
            boon.modifiers.AddRange(mods);
            run.AddBoon(boon);
            run.SyncGrantedSkills();
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(run, bundle.config);
            pc.DebugNoPassiveBlock = true; pc.DebugInvulnerable = true;
            var def = Object.Instantiate(demon); cleanup.Add(def);
            def.maxHealth = 1000000f; def.maxPoise = 1000000f; def.moveSpeed = 0f; def.strafeChance = 0f; def.guardChance = 0f; def.evadeChance = 0f; def.scale = 1f;
            def.attacks = new List<EnemyAttackEntry>(); def.huSplitCount = 0; def.huSummonCount = 0; def.huWakeRadius = 0f; def.huHatchSeconds = 0f; def.isBoss = false;
            var pos = new Vector3(0, 0.05f, 1.6f);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(pc.transform.position - pos);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            yield return new WaitForSeconds(1.2f);
            pc.LockTarget = e;
            float t = 0f, start = e.Health.Current;
            while (t < 6f && !e.IsDead)
            {
                pc.RestoreAll();
                if (pc.Actions.IsIdle) pc.Request(ActionType.LightAttack);
                t += Time.deltaTime; yield return null;
            }
            done(start - e.Health.Current);
            Clean();
            yield return null;
        }

        IEnumerator Fight(Case c, bool powerOn, bool group, Action<float> done)
        {
            var card = UniqueCatalog.All.First(x => x.code == c.code);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0); floor.transform.localScale = new Vector3(80, 1, 80); cleanup.Add(floor);
            var camGo = new GameObject("TestCam"); camGo.AddComponent<Camera>(); var rig = camGo.AddComponent<CameraRig>(); cleanup.Add(camGo);
            var p = WorldBuilder.CreatePlayer(); cleanup.Add(p);
            var ctrl = p.GetComponent<PlayerController>(); ctrl.cameraRig = rig; ctrl.InputEnabled = false; rig.target = p.transform; rig.player = ctrl;
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(card.classId), difficulty = bundle.config.difficulties[0], classFeatureId = card.requiredFeature };
            var run = RunFactory.Create(plan, bundle.config, 7);
            run.attributes.intelligence = 30; run.attributes.strength = 30; run.attributes.dexterity = 30;
            if (!c.generic || c.main == "") { run.equipment.Unequip(EquipSlot.MainHand); run.equipment.Unequip(EquipSlot.OffHand); }
            if (!powerOn && c.baselineMain != null)
            {
                var sword = new ItemInstance(bundle.Get<ItemDefinition>(c.baselineMain));
                run.inventory.Add(sword); Assert.IsTrue(run.EquipFromInventory(sword, EquipSlot.MainHand), c.baselineMain);
            }
            void Eq(string id, EquipSlot s) { if (string.IsNullOrEmpty(id)) return; var it = new ItemInstance(bundle.Get<ItemDefinition>(id)); run.inventory.Add(it); Assert.IsTrue(run.EquipFromInventory(it, s), id); }
            Eq(c.main, EquipSlot.MainHand); Eq(c.off, EquipSlot.OffHand);
            var item = HellEconomy.Roll(UniqueRegistry.Find(bundle.config, card.id), LootSource.Boss, 8, new System.Random(1));
            run.inventory.Add(item);
            Assert.IsTrue(run.EquipFromInventory(item, item.definition.DefaultSlot), c.code + ": unikat");
            if (c.code == "R04") Eq("hu_dagger", EquipSlot.OffHand);
            if (c.code == "R01") Eq("hu_wand_fire", EquipSlot.OffHand); // różdżka w lewej – dopiero z założonymi rękawicami
            run.SyncGrantedSkills();
            if (!c.generic) for (int i = 0; i < RunState.SkillSlotCount; i++) run.ClearSlot(i);
            var skills = c.skills.Concat(card.skills).Distinct().Take(RunState.SkillSlotCount).ToList();
            for (int i = 0; i < skills.Count; i++) { var d = bundle.Get<SpellDefinition>(skills[i]); run.AssignSlot(run.FindSpell(d) ?? run.LearnSpell(d, true), i); }
            var pc = p.GetComponent<PlayerCombat>();
            pc.DebugNoUniquePowers = !powerOn;
            pc.Init(run, bundle.config);
            pc.DebugNoPassiveBlock = true;
            if (c.generic) pc.Immortal = true; else pc.DebugInvulnerable = true;
            p.GetComponent<PlayerVisuals>().Refresh();
            Assert.AreEqual(powerOn, pc.Uniques.Get(card.id) != null, c.code + ": moc zgodnie z wariantem");
            if ((card.classId == "class_hu_summoner" && c.skills.Length == 0) || (c.generic && (card.classId == "class_hu_summoner" || c.code == "P03" || c.code == "H03")))
                Servant.Spawn(pc, ServantKind.Hound, 60f, 1f, () => pc.FeatureHitForTest(5f), Color.green);
            if (c.generic) for (int i = 0; i < RunState.SkillSlotCount; i++) if (pc.Skill(i) != null && pc.Skill(i).Def.IsAura) pc.RequestSkill(i);

            var enemies = new List<EnemyBrain>();
            if (group) foreach (var pos in new[] { new Vector3(0, 0.05f, 1.9f), new Vector3(1.3f, 0.05f, 2.2f), new Vector3(-1.3f, 0.05f, 2.2f), new Vector3(0, 0.05f, 3.1f) })
                    enemies.Add(Enemy(pc, pos, 6000f, 35f, c.generic && enemies.Count == 0));
            else enemies.Add(Enemy(pc, new Vector3(0, 0.05f, 1.9f), 200000f, 150f, c.generic));
            yield return new WaitForSeconds(1.2f); // koniec uśpienia wrogów
            if (c.startInRage) pc.AddFury(100f);
            var memo = new Dictionary<string, float>();
            float t = 0f;
            while (t < FightSeconds)
            {
                var target = enemies.Where(e => e != null && !e.IsDead).OrderBy(e => (e.transform.position - pc.transform.position).sqrMagnitude).FirstOrDefault();
                if (target == null) break;
                pc.LockTarget = target;
                Vector3 dir = target.transform.position - pc.transform.position; dir.y = 0;
                if (pc.Actions.IsIdle && dir.sqrMagnitude > 1e-4f) pc.transform.rotation = Quaternion.LookRotation(dir);
                c.policy(pc, powerOn, memo);
                t += Time.deltaTime;
                yield return null;
            }
            pc.HeavyHeld = false;
            lastEvents = powerOn ? pc.UniqueTelemetry.values.Count(kv => kv.Key.StartsWith(c.code + ":") && kv.Value > 0f) : 0;
            float lost = enemies.Sum(e => e == null ? 0f : e.Health.Max - Mathf.Max(0f, e.Health.Current));
            TestContext.WriteLine($"{c.code} {(powerOn ? "z mocą" : "bez mocy")} {(group ? "grupa" : "twardy")}: {lost:0} – " +
                string.Join("; ", pc.UniqueTelemetry.values.Where(kv => kv.Key.StartsWith(c.code)).Select(kv => $"{kv.Key}={kv.Value:0.#}")));
            done(lost);
            Clean();
            yield return null;
        }

        EnemyBrain Enemy(PlayerCombat pc, Vector3 pos, float hp, float poise, bool attacks = false)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = "Manekin"; def.maxHealth = hp; def.maxPoise = poise; def.scale = 1f; def.moveSpeed = 0f; def.strafeChance = 0f;
            def.preferredRange = 1.4f; def.attackInterval = new Vector2(0.6f, 0.9f);
            if (attacks) def.attacks.Add(new EnemyAttackEntry { attack = new AttackDefinition { name = "Pazur", baseDamage = 15f, reach = 2.2f, radius = 1f, windup = 0.35f, lunge = 0f }, maxRange = 3f });
            cleanup.Add(def);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(pc.transform.position - pos);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            return e;
        }
    }
}
