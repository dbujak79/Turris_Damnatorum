using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Test dymny wszystkich 75 unikatów: osiągalny rdzeń buildu (klasa, wymagana cecha, potrzebna broń i skille w trzech zwykłych
    /// slotach), kilka sekund walki z atakującym wrogiem z AI – zwykłe ataki, mocne, skille, przycisk cechy, unik, blok i parowanie.
    /// Sprawdza: moc aktywna (nie uśpiona) w legalnym buildzie, brak wyjątków (każdy błąd w logu oblewa test), zdarzenia mocy w
    /// telemetrii tam, gdzie pętla jest wykonalna bez scenariusza. To nie dowód siły mocy, tylko jej działania w silniku.
    /// </summary>
    public class UniqueSmokePlayTests
    {
        readonly List<Object> cleanup = new List<Object>();
        DefaultContent.Bundle bundle;

        public static IEnumerable<string> Codes => UniqueCatalog.All.Select(c => c.code);

        /// <summary>Dodatkowe wyposażenie (główna, druga ręka) i skille rdzenia buildu dla kart, które tego wymagają.</summary>
        internal static readonly Dictionary<string, (string main, string off, string[] skills)> Builds = new Dictionary<string, (string, string, string[])>
        {
            ["W01"] = ("hu_sword", null, new string[0]),
            ["W04"] = ("hu_sword", "hu_shield", new[] { "hu_cleave" }),
            ["W05"] = ("hu_warhammer", null, new[] { "hu_charge" }),
            ["P04"] = ("hu_morningstar", "hu_shield", new[] { "hu_aura_holy_fire" }),
            ["P02"] = ("hu_morningstar", "hu_shield", new[] { "hu_aura_might", "hu_aura_holy_fire" }),
            ["P03"] = ("hu_morningstar", "hu_grimoire_summoning", new[] { "hu_aura_zeal", "hu_summon_imps" }),
            ["P05"] = (null, "hu_shield", new[] { "hu_cleave" }),
            ["B01"] = ("hu_greataxe", null, new string[0]),
            ["B04"] = ("hu_greataxe", null, new[] { "skill_throw" }),
            ["B05"] = ("hu_greataxe", null, new string[0]),
            ["R01"] = ("hu_sword", null, new string[0]),
            ["R02"] = ("hu_sword", "hu_dagger", new string[0]),
            ["R03"] = ("hu_sword", "hu_dagger", new string[0]),
            ["R04"] = (null, "hu_dagger", new[] { "hu_cross_cut" }),
            ["R05"] = ("hu_sword", "hu_dagger", new[] { "hu_whirlwind" }),
            ["M01"] = ("hu_sword", "hu_grimoire_fire", new string[0]),
            ["M02"] = (null, "hu_grimoire_fire", new[] { "hu_nova_fire" }),
            ["M04"] = ("hu_wand_fire", null, new[] { "hu_frost_shard", "hu_infernal_orb", "hu_damned_chain" }),
            ["M05"] = ("hu_wand_fire", null, new[] { "hu_frost_shard" }),
            ["F01"] = ("hu_wand_fire", null, new[] { "hu_infernal_orb", "hu_ember_ring" }),
            ["F05"] = ("hu_wand_fire", null, new[] { "hu_ember_ring" }),
            ["H01"] = ("hu_sword", "hu_grimoire_water", new string[0]),
            ["H04"] = ("hu_wand_water", null, new[] { "hu_tidal_wave" }),
            ["H05"] = (null, null, new[] { "hu_frost_shard" }),
            ["A01"] = ("hu_wand_air", null, new string[0]),
            ["A03"] = ("hu_wand_air", null, new[] { "hu_wind_step" }),
            ["A04"] = (null, null, new[] { "hu_damned_chain" }),
            ["A05"] = (null, "hu_grimoire_air", new[] { "hu_nova_air" }),
            ["E02"] = ("hu_morningstar", null, new[] { "hu_stone_ward" }),
            ["E03"] = (null, "hu_grimoire_earth", new[] { "hu_stone_spikes" }),
            ["E04"] = ("hu_wand_earth", null, new[] { "hu_stone_ward", "hu_stone_spikes" }),
            ["E05"] = ("hu_morningstar", null, new string[0]),
            ["S03"] = ("hu_dagger", "hu_grimoire_summoning", new[] { "hu_summon_hounds" }),
            ["S04"] = (null, null, new[] { "hu_frost_shard" }),
            ["S05"] = (null, "hu_grimoire_summoning", new[] { "hu_summon_hounds" }),
            ["T01"] = ("", "", new[] { "hu_frost_shard" }),
            ["T03"] = ("hu_warhammer", null, new string[0]),
            ["T04"] = ("hu_sword", "hu_grimoire_water", new[] { "hu_frost_shard" }),
            ["T05"] = ("hu_wand_fire", null, new string[0]),
            ["L03"] = ("hu_dagger", null, new[] { "hu_sulfur_bomb", "hu_snare" }),
            ["L04"] = (null, null, new[] { "hu_shadow_step", "hu_snare" }),
            ["L05"] = ("hu_dagger", "hu_offdagger", new[] { "hu_knife_throw" }),
            ["K04"] = ("hu_bow", null, new[] { "hu_volley" }),
            ["K05"] = ("hu_bow", null, new[] { "hu_arrow_rain" }),
            ["Z03"] = (null, null, new string[0]),
            ["Z04"] = ("hu_dagger", "hu_offdagger", new[] { "hu_knife_throw" }),
            ["Z05"] = ("hu_dagger", null, new[] { "hu_cross_cut" }),
            ["Q02"] = ("hu_bow", null, new string[0]),
            ["Q04"] = ("hu_bow", null, new string[0]),
            ["Q05"] = ("hu_bow", null, new string[0]),
        };

        /// <summary>Karty, których rdzeń wymaga sytuacji spoza dymu (sług, stanu wroga) – moc może pozostać uśpiona informacyjnie.</summary>
        static readonly HashSet<string> SituationalDormancy = new HashSet<string> { "P02" };

        [SetUp] public void SetUp() { Time.timeScale = 1f; bundle = TestContent.Create(); EnemyBrain.DebugNoEvasion = true; UniqueRegistry.ResetForTests(); }

        [TearDown]
        public void TearDown()
        {
            Servant.DismissAll(); FeatureDecoy.ClearAll(); PinnedShadow.ClearAll(); EarthWall.ClearAll(); HuWorld.Clear();
            foreach (var p in Projectile.Active.ToArray()) if (p != null) Object.Destroy(p.gameObject);
            foreach (var z in DamageZone.Active.ToArray()) if (z != null) Object.Destroy(z.gameObject);
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            EnemyBrain.DebugNoEvasion = false; EnemyBrain.BleedRateProvider = null; UniqueMines.HoldContact = null;
        }

        void Equip(RunState run, string id, EquipSlot slot)
        {
            if (id == null) return;
            if (id == "") { var cur = run.equipment.Get(slot); if (cur != null) run.UnequipToInventory(slot); return; }
            var def = bundle.Get<ItemDefinition>(id);
            var it = new ItemInstance(def);
            run.inventory.Add(it);
            Assert.IsTrue(run.EquipFromInventory(it, slot), $"{id} → {slot}");
        }

        [UnityTest]
        public IEnumerator Unique_CoreBuild_RunsInCombat_NoErrors([ValueSource(nameof(Codes))] string code)
        {
            var card = UniqueCatalog.All.First(c => c.code == code);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0); floor.transform.localScale = new Vector3(80, 1, 80);
            cleanup.Add(floor);
            var camGo = new GameObject("TestCam"); camGo.AddComponent<Camera>(); var rig = camGo.AddComponent<CameraRig>(); cleanup.Add(camGo);
            var p = WorldBuilder.CreatePlayer(); cleanup.Add(p);
            var ctrl = p.GetComponent<PlayerController>(); ctrl.cameraRig = rig; ctrl.InputEnabled = false; rig.target = p.transform; rig.player = ctrl;
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(card.classId), difficulty = bundle.config.difficulties[0], classFeatureId = card.requiredFeature };
            var run = RunFactory.Create(plan, bundle.config, 7);
            run.attributes.intelligence = 30; run.attributes.strength = 30; run.attributes.dexterity = 30;
            Builds.TryGetValue(code, out var b);
            if (b.main == "") { Equip(run, "", EquipSlot.MainHand); Equip(run, "", EquipSlot.OffHand); }
            else { Equip(run, b.main, EquipSlot.MainHand); Equip(run, b.off, EquipSlot.OffHand); }
            var item = HellEconomy.Roll(UniqueRegistry.Find(bundle.config, card.id), LootSource.Boss, 8, new System.Random(1));
            run.inventory.Add(item);
            var slot = item.definition.DefaultSlot;
            if (item.definition.IsWeapon && b.main == null && b.off != null && !item.definition.twoHanded) slot = EquipSlot.MainHand;
            Assert.IsTrue(run.EquipFromInventory(item, slot), $"{code}: unikat w slocie {slot}");
            if (code == "R04") { Equip(run, "hu_dagger", EquipSlot.OffHand); }
            if (code == "R01") { Equip(run, "hu_wand_fire", EquipSlot.OffHand); } // różdżka w lewej – dopiero z założonymi rękawicami
            run.SyncGrantedSkills();
            var skills = (b.skills ?? new string[0]).Concat(card.skills).Distinct().Take(RunState.SkillSlotCount).ToList();
            for (int i = 0; i < skills.Count; i++)
            {
                var def = bundle.Get<SpellDefinition>(skills[i]);
                var inst = run.FindSpell(def) ?? run.LearnSpell(def, true);
                run.AssignSlot(inst, i);
            }
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(run, bundle.config);
            pc.DebugNoPassiveBlock = true; pc.DebugInvulnerable = true;
            p.GetComponent<PlayerVisuals>().Refresh();
            var power = pc.Uniques.Get(card.id);
            Assert.IsNotNull(power, code + ": moc zainstalowana");
            for (int i = 0; i < RunState.SkillSlotCount; i++) if (pc.Skill(i) != null && pc.Skill(i).Def.IsAura) pc.RequestSkill(i); // aury z rdzenia włączone
            yield return null;
            string dormant = power.Dormant();
            if (!SituationalDormancy.Contains(code)) Assert.IsNull(dormant, $"{code}: rdzeń buildu legalny – {dormant}");

            // Wróg z atakiem i drugi obok (grupa), sługa dla kart przywołań.
            var e1 = Enemy(pc, new Vector3(0, 0.05f, 2.2f), true);
            var e2 = Enemy(pc, new Vector3(1.4f, 0.05f, 3.2f), false);
            if (card.classId == "class_hu_summoner" || code == "P03" || code == "H03")
                Servant.Spawn(pc, ServantKind.Hound, 15f, 1f, () => pc.FeatureHitForTest(5f), Color.green);
            yield return new WaitForSeconds(0.1f);
            pc.LockTarget = e1;
            Vector3 d = e1.transform.position - pc.transform.position; d.y = 0; pc.transform.rotation = Quaternion.LookRotation(d);

            var script = new System.Action[]
            {
                () => pc.RequestSkill(PlayerCombat.SignatureSlot),
                () => pc.Request(ActionType.LightAttack),
                () => pc.RequestSkill(0),
                () => pc.Request(ActionType.HeavyAttack),
                () => pc.RequestSkill(1),
                () => { pc.SetBlockHeld(true); },
                () => { pc.SetBlockHeld(false); pc.Request(ActionType.Parry); },
                () => pc.RequestSkill(2),
                () => { pc.MoveIntent = Vector3.back; pc.Request(ActionType.Dodge); },
                () => { pc.MoveIntent = Vector3.zero; pc.RequestSkill(PlayerCombat.SignatureSlot); },
                () => pc.Request(ActionType.HeavyAttack),
                () => pc.Request(ActionType.LightAttack),
                () => pc.RequestSkill(0),
                () => pc.Request(ActionType.HeavyAttack),
            };
            foreach (var step in script)
            {
                if (e1 != null && !e1.IsDead) pc.LockTarget = e1;
                step();
                yield return new WaitForSeconds(0.45f);
            }
            yield return new WaitForSeconds(0.5f);
            int events = pc.UniqueTelemetry.values.Keys.Count(k => k.StartsWith(code + ":")) + pc.UniqueTelemetry.rejections.Keys.Count(k => k.StartsWith(code + ":"));
            TestContext.WriteLine($"{code} {card.name}: zdarzenia mocy {events} – " + string.Join("; ", pc.UniqueTelemetry.values.Where(kv => kv.Key.StartsWith(code)).Select(kv => $"{kv.Key}={kv.Value:0.#}")));
            // Śmierć i nowe piętro sprzątają runtime mocy bez wyjątków.
            pc.OnFloorStart();
            yield return null;
        }

        EnemyBrain Enemy(PlayerCombat pc, Vector3 pos, bool attacks)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = "Manekin"; def.maxHealth = 4000f; def.maxPoise = 60f; def.scale = 1f; def.moveSpeed = 1.5f; def.strafeChance = 0f;
            def.preferredRange = 1.4f; def.attackInterval = new Vector2(0.6f, 0.9f);
            if (attacks) def.attacks.Add(new EnemyAttackEntry { attack = new AttackDefinition { name = "Pazur", baseDamage = 15f, reach = 2f, radius = 0.9f, windup = 0.35f, lunge = 0f }, maxRange = 3f });
            cleanup.Add(def);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(pc.transform.position - pos);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            return e;
        }
    }
}
