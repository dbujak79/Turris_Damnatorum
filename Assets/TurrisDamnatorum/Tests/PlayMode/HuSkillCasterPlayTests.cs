using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Skille Hell Unleashed wykonywane z komponentów (<see cref="HuSkillCaster"/>) w prawdziwej scenie poligonu: każdy skill
    /// na najwyższym poziomie i każda jego forma rzucona w gromadę manekinów zadaje obrażenia, bez wyjątków i bez śladów
    /// po wyczyszczeniu. Osobno: kopie mają wspólny budżet, wyzwalacz uniku odpala skill, aura rezerwuje zasób.
    /// </summary>
    public class HuSkillCasterPlayTests
    {
        string profileFile;
        GameRoot root;
        float dealt;
        readonly List<string> report = new List<string>();

        [SetUp] public void SetUp() { profileFile = $"turris_hu_test_{System.Guid.NewGuid():N}.json"; GameRoot.ProfileFileOverride = profileFile; CombatEvents.Impact += Count; }

        [TearDown]
        public void TearDown()
        {
            CombatEvents.Impact -= Count;
            GameRoot.ProfileFileOverride = null;
            string path = Path.Combine(Application.persistentDataPath, profileFile);
            if (File.Exists(path)) File.Delete(path);
        }

        [UnityTearDown]
        public IEnumerator UnloadMain()
        {
            Time.timeScale = 1f;
            HuWorld.Clear();
            var main = SceneManager.GetSceneByName("Main");
            if (!main.IsValid() || !main.isLoaded) yield break;
            var empty = SceneManager.CreateScene("AfterTest_" + System.Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(main);
        }

        void Count(IHitReceiver target, HitData hit, HitResult result)
        {
            if (target is EnemyBrain && hit.attacker is PlayerCombat && !hit.isStatusTick) dealt += result.healthDamage;
        }

        IEnumerator StartRange(string classId)
        {
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            root = Object.FindAnyObjectByType<GameRoot>();
            root.StartSkillRange(root.config.classes.First(c => c.id == classId));
            yield return null;
            root.Range.PlaceTargets(SkillRange.TargetSet.Pack);
            yield return null;
        }

        /// <summary>Bohater 2,5 m przed środkiem gromady, twarzą do niej, z pełnymi zasobami.</summary>
        void Face()
        {
            var pc = root.Player;
            var alive = root.Enemies.Where(e => e != null && !e.IsDead).ToList();
            Vector3 center = alive.Aggregate(Vector3.zero, (a, e) => a + e.transform.position) / Mathf.Max(1, alive.Count);
            Vector3 from = root.Range != null ? center - Vector3.forward * 2.5f : pc.transform.position;
            var cc = pc.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            pc.transform.position = new Vector3(from.x, pc.transform.position.y, from.z);
            pc.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            if (cc != null) cc.enabled = true;
            // Pomiary porównują rzuty wzdłuż tego samego kierunku – w środek gromady. Pomoc w celowaniu skierowałaby rzut
            // w najbliższego wroga (inna liczba trafionych), a namierzanie i tak trzyma kontroler, nie pc.LockTarget.
            root.Controller.AimAssistEnabled = false;
            root.Controller.LockOn(null);
            pc.Health.SetCurrent(pc.Health.Max); pc.Mana.SetCurrent(pc.Mana.Max); pc.Stamina.SetCurrent(pc.Stamina.Max);
        }

        SpellInstance Equip(SpellDefinition def, params HuSeal[] seals)
        {
            var run = root.Run;
            // Skill wymaga rodzaju broni – broń wzorcowa rodzaju (siła ×1), żeby pomiary nie zależały od broni klasy.
            TestWeapons.EnsureFor(run, def, id => root.config.baseItemPool.Concat(root.config.classes.SelectMany(c => c.loadouts).SelectMany(l => new[] { l.weapon, l.offHand }))
                .FirstOrDefault(i => i != null && i.id == id));
            var inst = run.FindSpell(def) ?? run.LearnSpell(def);
            run.AssignSlot(inst, 0);
            inst.level = def.maxLevel;
            foreach (var s in inst.seals.RemoveAll()) { }
            foreach (var s in seals) Assert.IsTrue(inst.seals.TryInsert(s, out _), $"{s.id} → {def.id}: {inst.seals.Fit(s)}");
            run.RefreshBonds();
            root.Player.RefreshBuild();
            inst.ResetCooldown();
            return inst;
        }

        /// <summary>Usuwa wszystko, co zostawiły poprzednie rzuty (strefy, pociski, uderzenia, sługi).</summary>
        static void ClearWorld()
        {
            HuWorld.Clear(); Servant.DismissAll();
            foreach (var z in DamageZone.Active.ToArray()) if (z != null) Object.Destroy(z.gameObject);
            foreach (var p in Projectile.Active.ToArray()) if (p != null) Object.Destroy(p.gameObject);
            foreach (var s in DelayedStrike.Active.ToArray()) if (s != null) Object.Destroy(s.gameObject);
            foreach (var s in Object.FindObjectsByType<HuShot>()) Object.Destroy(s.gameObject);
        }

        IEnumerator CastAndMeasure(SpellDefinition def, string label, float wait)
        {
            ClearWorld();
            float idle = 0f;
            while (!root.Player.Actions.IsIdle && idle < 5f) { idle += Time.deltaTime; yield return null; }
            yield return new WaitForSeconds(0.25f);
            // Manekiny po poprzednim rzucie wstają po 1,6 s – rzut w pustą arenę nic by nie sprawdził.
            float stand = 0f;
            while (root.Enemies.Any(e => e == null || e.IsDead) && stand < 4f) { stand += Time.deltaTime; yield return null; }
            foreach (var e in root.Enemies) if (e != null) e.Status.Clear(); // podpalenie z poprzedniego rzutu nie dobija celów
            Face();
            dealt = 0f;
            float start = 0f;
            while (root.Player.Actions.Current != ActionType.Cast && start < 1.5f) { root.Player.RequestSkill(0); start += Time.deltaTime; yield return null; }
            float t = 0f;
            var trace = new System.Text.StringBuilder();
            Vector3 p0 = root.Player.transform.position;
            while (t < wait && dealt <= 0f)
            {
                if (trace.Length < 900) { Vector3 d = root.Player.transform.position - p0; d.y = 0; trace.Append($"[{t:0.00} {root.Player.Actions.Current}/{root.Player.Actions.Phase} d={d.magnitude:0.0} hu={HuWorld.Count} al={root.Enemies.Count(e => e != null && !e.IsDead)}] "); }
                t += Time.deltaTime; yield return null;
            }
            report.Add($"{label}: {dealt:0} w {t:0.0} s" + (dealt <= 0f ? " ŚLAD: start=" + start.ToString("0.00") + " " + trace : ""));
        }

        static bool Damaging(HuSkillShape shape) =>
            shape.Parameters.TotalDamage > 0 && !shape.HasComponent("Barrier") && !shape.HasComponent("Aura") && !shape.HasComponent("Overload");

        [UnityTest, Timeout(900000)]
        public IEnumerator EveryHuSkillAndForm_DealsDamage_WithoutErrors()
        {
            yield return StartRange("class_mage");
            Time.timeScale = 3f; // ok. 300 rzutów – czas gry ×3 (fizyka w FixedUpdate liczy te same kroki), test 3× krótszy
            var defs = root.config.classes.SelectMany(c => new[] { c.signatureSkill }).Concat(root.config.baseSpellPool)
                .Where(d => d != null && d.HuData != null).Distinct().ToList();
            foreach (var hu in HuCatalog.Skills.Values)
            {
                var d = root.config.baseSpellPool.Concat(root.config.classes.Select(c => c.signatureSkill)).Concat(root.config.classes.SelectMany(c => c.startingSkillChoices))
                    .FirstOrDefault(x => x != null && x.HuData == hu);
                if (d != null && !defs.Contains(d)) defs.Add(d);
            }
            var failures = new List<string>();
            foreach (var def in defs)
            {
                var forms = new HuSeal[] { null }.Concat(HuCatalog.Seals.Where(s => s.IsForm && s.skillId == def.HuData.id)).ToList();
                foreach (var form in forms)
                {
                    var inst = Equip(def, form == null ? new HuSeal[0] : new[] { form });
                    var shape = inst.seals.Shape;
                    if (!Damaging(shape)) continue;
                    bool summon = shape.HasComponent("Summon");
                    yield return CastAndMeasure(def, def.id + (form != null ? " + " + form.id : ""), summon ? 6f : 4f);
                    if (dealt <= 0f) failures.Add(def.id + (form != null ? " + " + form.id : ""));
                }
            }
            Debug.Log("[HuSkillCaster] " + string.Join("\n", report));
            CollectionAssert.IsEmpty(failures, "bez obrażeń: " + string.Join(", ", failures));
            HuWorld.Clear();
            yield return null;
            Assert.AreEqual(0, HuWorld.Count, "czyszczenie kończy wszystkie runtime'y");
        }

        /// <summary>
        /// Galeria wybranych skilli i form w trakcie działania (kamera gry → PNG). Zapis do TURRIS_SHOT_DIR,
        /// tylko na żądanie (TURRIS_SHOT_DIR); inaczej test jest pomijany.
        /// </summary>
        [UnityTest, Timeout(300000)]
        public IEnumerator Gallery_SkillsAndForms_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Galeria na żądanie: ustaw TURRIS_SHOT_DIR."); yield break; }
            Directory.CreateDirectory(dir);
            yield return StartRange("class_mage");
            var shots = new (string skill, string form, float at)[]
            {
                ("hu_infernal_orb", null, 0.35f), ("hu_infernal_orb", "seal.form.orb.sun", 0.8f), ("hu_infernal_orb", "seal.form.orb.comet", 0.7f),
                ("hu_brimstone_rain", null, 0.9f), ("hu_cyclone_eye", null, 0.8f), ("hu_damned_chain", null, 0.15f), ("hu_damned_chain", "seal.form.chain.ball", 0.8f),
                ("hu_stone_spikes", null, 0.55f), ("hu_tidal_wave", null, 0.35f), ("hu_glacial_tear", "seal.form.tear.hail", 0.9f),
                ("hu_ember_ring", null, 0.6f), ("hu_volley", "seal.form.volley.ring", 0.15f), ("hu_arrow_rain", null, 0.8f),
                ("hu_whirlwind", "seal.form.whirl.tornado", 0.9f), ("hu_storm_verdict", null, 0.7f), ("hu_slag_spray", "seal.form.slag.geyser", 0.2f),
                ("spell_firering", null, 1.2f), ("spell_firering", "seal.form.wall.wave", 1.4f), ("spell_firering", "seal.form.wall.fortress", 1.2f),
            };
            var cam = Camera.main;
            foreach (var shot in shots)
            {
                var def = root.config.baseSpellPool.FirstOrDefault(d => d.id == shot.skill);
                if (def == null) continue;
                Equip(def, shot.form == null ? new HuSeal[0] : new[] { HuCatalog.Seal(shot.form) });
                ClearWorld();
                float idle = 0f;
                while (!root.Player.Actions.IsIdle && idle < 5f) { idle += Time.deltaTime; yield return null; }
                yield return new WaitForSeconds(0.3f);
                Face();
                float start = 0f;
                while (root.Player.Actions.Current != ActionType.Cast && start < 1.5f) { root.Player.RequestSkill(0); start += Time.deltaTime; yield return null; }
                yield return new WaitForSeconds(0.25f + shot.at);
                var rt = new RenderTexture(1600, 900, 24);
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); tex.Apply();
                cam.targetTexture = null; RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(dir, $"{shot.skill}{(shot.form != null ? "__" + shot.form.Replace("seal.form.", "") : "")}.png"), tex.EncodeToPNG());
                Object.Destroy(tex); rt.Release(); Object.Destroy(rt);
            }
        }

        [UnityTest]
        public IEnumerator EchoAndFan_ShareOneDamageBudget()
        {
            yield return StartRange("class_mage");
            var orb = root.config.baseSpellPool.First(d => d.id == "hu_infernal_orb");
            Equip(orb);
            yield return CastAndMeasure(orb, "kula", 3f);
            yield return new WaitForSeconds(1f);
            float plain = 0; dealt = 0; Face(); root.Player.RequestSkill(0); yield return new WaitForSeconds(2.5f); plain = dealt;
            Equip(orb, HuCatalog.Seal("seal.demon.veles_crown"), HuCatalog.Seal("seal.echo"));
            dealt = 0; Face(); root.Player.RequestSkill(0); yield return new WaitForSeconds(2.5f);
            Assert.Greater(plain, 0f);
            // Korona Welesa (4 × 40%) i echo (65%) przekraczają budżet 1,5× – kopie maleją, łącznie najwyżej 2,5× rzutu.
            Assert.LessOrEqual(dealt, plain * 2.5f * 1.25f, $"kopie {dealt:0} przy rzucie {plain:0}");
        }

        [UnityTest]
        public IEnumerator Dash_CoversItsDistance_NotMore()
        {
            yield return StartRange("class_mage");
            root.Range.PlaceTargets(SkillRange.TargetSet.None);
            yield return null;
            var step = root.config.baseSpellPool.First(d => d.id == "hu_wind_step");
            var inst = Equip(step);
            float distance = inst.seals.Shape.Parameters.GetF("distance");
            root.Player.LockTarget = null;
            Vector3 p0 = root.Player.transform.position;
            float t = 0;
            while (root.Player.Actions.Current != ActionType.Cast && t < 1.5f) { root.Player.RequestSkill(0); t += Time.deltaTime; yield return null; }
            var log = new System.Text.StringBuilder();
            t = 0; while (root.Player.Actions.Current == ActionType.Cast && t < 3f)
            {
                Vector3 dd = root.Player.transform.position - p0; dd.y = 0;
                log.Append($"[{t:0.00} {root.Player.Actions.Phase} m={root.Player.SkillMotion.magnitude:0.0} d={dd.magnitude:0.0} dt={Time.deltaTime:0.000}] ");
                t += Time.deltaTime; yield return null;
            }
            yield return null;
            Vector3 d = root.Player.transform.position - p0; d.y = 0;
            Assert.AreEqual(distance, d.magnitude, distance * 0.15f, $"doskok {d.magnitude:0.0} m zamiast {distance:0.0} m: {log}");
        }

        [UnityTest]
        public IEnumerator DodgeTrigger_CastsTheSkill()
        {
            yield return StartRange("class_mage");
            var orb = root.config.baseSpellPool.First(d => d.id == "hu_infernal_orb");
            var inst = Equip(orb, HuCatalog.Seal("seal.trigger.dodge"));
            Face();
            dealt = 0f;
            root.Player.Request(ActionType.Dodge);
            float t = 0; while (t < 2.5f && dealt <= 0f) { t += Time.deltaTime; yield return null; }
            Assert.Greater(dealt, 0f, "Odruch: kula poleciała sama przy uniku");
        }

        [UnityTest]
        public IEnumerator Aura_ReservesStamina_AndPaysBackWhenOff()
        {
            yield return StartRange("class_knight");
            var might = root.config.baseSpellPool.First(d => d.id == "hu_aura_might");
            Equip(might);
            float max = root.Player.Stamina.Max;
            root.Player.RequestSkill(0);
            yield return null; yield return null;
            Assert.AreEqual(max * (1f - PlayerCombat.AuraStaminaReserve / 100f), root.Player.Stamina.Max, 0.5f, "aura rezerwuje 20% wytrzymałości");
            Assert.Greater(root.Player.Build.stats[StatType.GlobalDamage], 0f, "Aura męstwa podnosi obrażenia");
            yield return new WaitForSeconds(1.2f);
            root.Player.RequestSkill(0);
            yield return null; yield return null;
            Assert.AreEqual(max, root.Player.Stamina.Max, 0.5f);
        }
    }
}
