using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Tryb pomiaru skilli i pieczęci (port <c>--bench</c> z HU): jeden rzut w nieśmiertelne manekiny, łączne obrażenia
    /// w oknie jednego cyklu odnowienia (6–20 s) (trafienia, strefy, echa, sługi, podpalenie i krwawienie – z CombatEvents.Impact), koszt i odnowienie.
    /// Zapis do TURRIS_BENCH_DIR/bench.csv; bez tej zmiennej test jest pomijany (pomiar trwa do 90 minut).
    /// Ograniczenia jak w HU: manekiny nie robią uników, nie liczy się mobilność ani obrona; kształt gromady faworyzuje
    /// skille dookoła bohatera, kolos – pojedyncze uderzenia.
    /// </summary>
    public class HuBenchPlayTests
    {
        const float Window = 6f, MaxWindow = 20f, Speed = 2f, DummyHealth = 1e7f;
        static readonly string[] SupportSkills =
        {
            "hu_infernal_orb", "hu_nova_fire", "hu_cleave", "hu_volley", "hu_tidal_wave", "hu_damned_chain", "hu_sulfur_bomb", "hu_stone_spikes", "hu_whirlwind", "hu_ember_ring",
        };

        string profileFile;
        GameRoot root;
        float dealt;

        [SetUp] public void SetUp() { profileFile = $"turris_bench_{System.Guid.NewGuid():N}.json"; GameRoot.ProfileFileOverride = profileFile; CombatEvents.Impact += Count; }

        /// <summary>Wszystko, co trafia manekiny (na poligonie innych źródeł nie ma): trafienia, sługi, strefy i tyknięcia stanów.</summary>
        void Count(IHitReceiver target, HitData hit, HitResult result) { if (target is EnemyBrain) dealt += result.healthDamage; }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            CombatEvents.Impact -= Count;
            GameRoot.ProfileFileOverride = null;
            string path = Path.Combine(Application.persistentDataPath, profileFile);
            if (File.Exists(path)) File.Delete(path);
        }

        [UnityTearDown]
        public IEnumerator UnloadMain()
        {
            HuWorld.Clear();
            var main = SceneManager.GetSceneByName("Main");
            if (!main.IsValid() || !main.isLoaded) yield break;
            var empty = SceneManager.CreateScene("AfterTest_" + System.Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(main);
        }

        IEnumerator Targets(SkillRange.TargetSet set)
        {
            root.Range.PlaceTargets(set);
            yield return null; yield return null;
            foreach (var e in root.Enemies) if (e != null) Neutralize(e);
            Heal();
        }

        /// <summary>
        /// Manekin bez odporności i słabości (chochlik bierze ogień ×0,25 i mróz ×1,6, osiłek ma 25% pancerza): klon
        /// definicji tylko dla tego ciała, asset zostaje nietknięty. Inaczej pomiar porównuje żywioły zamiast skilli.
        /// </summary>
        static void Neutralize(EnemyBrain e)
        {
            var clone = Object.Instantiate(e.Def);
            clone.fireMultiplier = clone.frostMultiplier = clone.lightningMultiplier = clone.earthMultiplier = clone.bleedMultiplier = 1f;
            clone.physicalResist = clone.magicResist = 0f;
            typeof(EnemyBrain).GetProperty(nameof(EnemyBrain.Def)).GetSetMethod(true).Invoke(e, new object[] { clone });
        }

        void Heal()
        {
            foreach (var e in root.Enemies)
            {
                if (e == null || e.IsDead) continue;
                e.Health.SetMax(DummyHealth, false); e.Health.SetCurrent(DummyHealth);
                e.Status.Clear();
            }
        }

        static void ClearWorld()
        {
            HuWorld.Clear(); Servant.DismissAll();
            foreach (var z in DamageZone.Active.ToArray()) if (z != null) Object.Destroy(z.gameObject);
            foreach (var p in Projectile.Active.ToArray()) if (p != null) Object.Destroy(p.gameObject);
            foreach (var s in DelayedStrike.Active.ToArray()) if (s != null) Object.Destroy(s.gameObject);
            foreach (var s in Object.FindObjectsByType<HuShot>()) Object.Destroy(s.gameObject);
            foreach (var a in Object.FindObjectsByType<AuraEffect>()) a.Stop();
            foreach (var e in Object.FindObjectsByType<EnemyBrain>()) e.Status.Clear();
        }

        void Face()
        {
            var pc = root.Player;
            var alive = root.Enemies.Where(e => e != null && !e.IsDead).ToList();
            Vector3 center = alive.Aggregate(Vector3.zero, (a, e) => a + e.transform.position) / Mathf.Max(1, alive.Count);
            var cc = pc.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            Vector3 from = center - Vector3.forward * (alive.Count > 1 ? 2.5f : 2.2f);
            pc.transform.position = new Vector3(from.x, pc.transform.position.y, from.z);
            pc.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            if (cc != null) cc.enabled = true;
            pc.LockTarget = alive.OrderBy(e => Vector3.Distance(e.transform.position, pc.transform.position)).FirstOrDefault();
            pc.Health.SetCurrent(pc.Health.Max); pc.Mana.SetCurrent(pc.Mana.Max); pc.Stamina.SetCurrent(pc.Stamina.Max);
        }

        SpellInstance Equip(SpellDefinition def, HuSeal seal)
        {
            var run = root.Run;
            var inst = run.FindSpell(def) ?? run.LearnSpell(def);
            run.AssignSlot(inst, 0);
            inst.level = def.maxLevel;
            foreach (var s in inst.seals.RemoveAll()) { }
            if (seal != null && !inst.seals.TryInsert(seal, out _)) return null;
            run.RefreshBonds();
            root.Player.RefreshBuild();
            inst.ResetCooldown();
            return inst;
        }

        IEnumerator Measure(SpellDefinition def, HuSeal seal, string kind, string targets, StringBuilder csv)
        {
            var inst = Equip(def, seal);
            if (inst == null) yield break;
            var shape = inst.seals.Shape;
            ClearWorld();
            float wait = 0f;
            while (!root.Player.Actions.IsIdle && wait < 5f) { wait += Time.unscaledDeltaTime; yield return null; }
            yield return null;
            Heal();
            Face();
            dealt = 0f;
            bool aura = shape.HasComponent("Aura");
            float start = 0f;
            while (root.Player.Actions.Current != ActionType.Cast && !aura && start < 1.5f) { root.Player.RequestSkill(0); start += Time.unscaledDeltaTime; yield return null; }
            if (aura) { root.Player.RequestSkill(0); yield return null; }
            float t = 0f;
            // Okno = jeden cykl odnowienia (6–20 s): sługi i długie strefy działają do końca odnowienia – krótsze okno je zaniżało.
            float window = aura ? Window : Mathf.Clamp(inst.Cooldown, Window, MaxWindow);
            while (t < window) { t += Time.deltaTime; yield return null; }
            float dmg = dealt;
            var sk = root.Player.Skill(0);
            float cd = inst.Cooldown, cost = sk.Def.IsSpell ? sk.manaCost : sk.staminaCost;
            float life = shape.Parameters.GetF(HuParams.LifeCost);
            string Num(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);
            csv.AppendLine(string.Join(";", kind, def.id, seal?.id ?? "-", targets, Num(dmg), Num(cd), inst.MaxCharges, Num(aura ? dmg / window : dmg / Mathf.Max(0.6f, cd)), Num(sk.huScale * sk.power), sk.equipmentMet ? 1 : 0,
                sk.Def.IsSpell ? "mana" : "wytrz", Num(cost), Num(life), shape.Element, string.Join("+", shape.Components), Num(window)));
            if (aura) root.Player.RequestSkill(0);
        }

        [UnityTest, Timeout(5400000)]
        public IEnumerator Bench_SkillsFormsAndSupports_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_BENCH_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Pomiar na żądanie: ustaw TURRIS_BENCH_DIR."); yield break; }
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            root = Object.FindAnyObjectByType<GameRoot>();
            root.StartSkillRange(root.config.classes.First(c => c.id == "class_mage"));
            yield return null;
            Time.timeScale = Speed;
            Directory.CreateDirectory(dir);
            var csv = new StringBuilder("rodzaj;skill;pieczec;cele;obrazenia_cyklu;odnowienie;ladunki;na_sekunde_cyklu;skala;sprzet;zasob;koszt;koszt_zycia;zywiol;komponenty;okno\n");
            void Save() => File.WriteAllText(Path.Combine(dir, "bench.csv"), csv.ToString(), new UTF8Encoding(true)); // po każdej sekcji – przerwany pomiar zostawia dane

            var defs = root.config.baseSpellPool.Concat(root.config.classes.Select(c => c.signatureSkill)).Concat(root.config.classes.SelectMany(c => c.startingSkillChoices))
                .Where(d => d != null && d.HuData != null).Distinct().OrderBy(d => d.id).ToList();
            foreach (var target in new[] { SkillRange.TargetSet.Pack, SkillRange.TargetSet.Colossus })
            {
                yield return Targets(target);
                string tname = target == SkillRange.TargetSet.Pack ? "gromada" : "kolos";
                foreach (var def in defs)
                {
                    yield return Measure(def, null, "bez", tname, csv);
                    foreach (var form in HuCatalog.Seals.Where(s => s.IsForm && s.skillId == def.HuData.id))
                        yield return Measure(def, form, "forma", tname, csv);
                }
                Save();
            }
            yield return Targets(SkillRange.TargetSet.Pack);
            foreach (var id in SupportSkills)
            {
                var def = defs.FirstOrDefault(d => d.id == id);
                if (def == null) continue;
                foreach (var sup in HuCatalog.Seals.Where(s => s.kind == SealKind.Support))
                    yield return Measure(def, sup, sup.lootOnly ? "demoniczna" : "wsparcie", "gromada", csv);
                Save();
            }
            Debug.Log("[bench] zapisano " + Path.Combine(dir, "bench.csv"));
        }
    }
}
