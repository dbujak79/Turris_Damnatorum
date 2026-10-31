#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests
{
    public class HellMigrationPlayTests
    {
        DefaultContent.Bundle bundle;
        readonly List<Object> cleanup = new List<Object>();
        Gamepad pad; Keyboard keyboard;
        InputSettings.BackgroundBehavior background;
        InputSettings.EditorInputBehaviorInPlayMode editorInput;
        [SetUp] public void Setup()
        {
            bundle = DefaultContent.Create(); Time.timeScale = 1;
            background = InputSystem.settings.backgroundBehavior; editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale = 1;
            if (pad != null) InputSystem.RemoveDevice(pad);
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = background; InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
            foreach (var item in cleanup) if (item != null) Object.Destroy(item);
            foreach (var effect in Object.FindObjectsByType<HellEnemyChannel>()) Object.Destroy(effect.gameObject);
            foreach (var effect in Projectile.Active.ToArray()) Object.Destroy(effect.gameObject);
            foreach (var effect in WaveEffect.Active.ToArray()) Object.Destroy(effect.gameObject);
            foreach (var effect in DamageZone.Active.ToArray()) Object.Destroy(effect.gameObject);
            foreach (var effect in DelayedStrike.Active.ToArray()) Object.Destroy(effect.gameObject);
            foreach (var effect in Object.FindObjectsByType<HellCocoon>()) Object.Destroy(effect.gameObject);
            foreach (var effect in HellWebPatch.Active.ToArray()) Object.Destroy(effect.gameObject);
            yield return null;
            foreach (var item in bundle.all.Concat(bundle.legacy)) if (item != null) Object.Destroy(item);
        }
        void Floor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.transform.position = Vector3.down * 0.5f;
            floor.transform.localScale = new Vector3(100, 1, 100); cleanup.Add(floor);
        }
        PlayerCombat Player(ClassDefinition cls = null, int loadout = 0)
        {
            var go = new GameObject("P0_Player"); cleanup.Add(go);
            var cc = go.AddComponent<CharacterController>(); cc.height = 1.8f; cc.radius = 0.35f; cc.center = Vector3.up * 0.9f;
            var p = go.AddComponent<PlayerCombat>(); p.enabled = false;
            var plan = new LoadoutPlan { classDef = cls ?? bundle.config.classes[0], difficulty = bundle.config.difficulties[0], loadoutIndex = loadout };
            p.Init(RunFactory.Create(plan, bundle.config, 19), bundle.config); p.Health.SetMax(1000000, true);
            p.transform.position = new Vector3(0, 0, 1.4f); return p;
        }
        EnemyBrain Enemy(EnemyDefinition def, PlayerCombat player)
        {
            var go = new GameObject("P1_" + def.id); cleanup.Add(go);
            var cc = go.AddComponent<CharacterController>(); cc.height = 1.8f; cc.radius = 0.35f; cc.center = Vector3.up * 0.9f;
            var e = go.AddComponent<EnemyBrain>(); e.Setup(def, player, 1, null, false, bundle.config.balance); e.enabled = false;
            return e;
        }
        [UnityTest]
        public IEnumerator EveryImportedAbility_Releases_AndParryCancelsBeforeHit()
        {
            Floor(); var player = Player(); int released = 0;
            var coverage = new List<string> { "enemy\tattack\tdelivery\tdamage\tsource_sha256" };
            foreach (var def in bundle.config.enemyCatalog.Where(d => !string.IsNullOrEmpty(d.huSourceId)))
            for (int index = 0; index < def.attacks.Count; index++)
            {
                var attack = def.attacks[index];
                if (attack.huAbility == null || string.IsNullOrEmpty(attack.huAbility.name)) continue;
                var enemy = Enemy(def, player);
                player.Actions.Reset(); player.Status.Clear(); player.RestoreAll();
                Physics.SyncTransforms();
                enemy.DebugStartAttack(index);
                enemy.OnAttackParried(999999);
                float before = player.Health.Current;
                enemy.Step(attack.attack.windup + 0.01f);
                Assert.AreEqual(before, player.Health.Current, def.id + ": anulowanie " + attack.attack.name);
                enemy.DebugStartAttack(index);
                enemy.Step(enemy.AttackWindupDuration + 0.01f);
                Assert.IsTrue(enemy.IsDead || enemy.AttackPhase == ActionPhase.Active, def.id + ": nie rozpoczęto " + attack.attack.name);
                if (!enemy.IsDead) enemy.Step(0.01f);
                foreach (var channel in Object.FindObjectsByType<HellEnemyChannel>()) channel.Step(6);
                foreach (var wave in WaveEffect.Active.ToArray()) wave.Step(2);
                foreach (var projectile in Projectile.Active.ToArray()) projectile.Step(2);
                foreach (var cocoon in Object.FindObjectsByType<HellCocoon>()) cocoon.Step(2);
                foreach (var strike in DelayedStrike.Active.ToArray()) strike.Step(6);
                if (attack.attack.baseDamage > 0 && attack.huAbility.name != "StoneWard")
                    Assert.Less(player.Health.Current, before, def.id + ": brak obrażeń " + attack.attack.name);
                coverage.Add(def.id + "\t" + attack.attack.name + "\t" + attack.huAbility.name + "\t" + (before - player.Health.Current).ToString(System.Globalization.CultureInfo.InvariantCulture) + "\t" + def.huSourceHash);
                enemy.DebugKill();
                Object.Destroy(enemy.gameObject);
                released++;
                yield return null;
            }
            Assert.Greater(released, 100, "test ma przejść rzeczywisty katalog, w tym kroki kombinacji");
            File.WriteAllLines(Path.Combine(Application.dataPath, "../TestResults/hu-native-coverage.tsv"), coverage);
        }
        [UnityTest]
        public IEnumerator SummonLimit_MasterDeath_AndBroodHaveNoOrphans()
        {
            Floor(); var player = Player(); var children = new List<EnemyBrain>();
            void Spawn(EnemyBrain master, EnemyDefinition definition, Vector3 at) { var child = Enemy(definition, player); child.Owner = master; children.Add(child); }
            EnemyBrain.SummonRequested += Spawn;
            try
            {
                var def = bundle.Get<EnemyDefinition>("enemy_hu_boss_bonelord"); var boss = Enemy(def, player);
                int summon = def.attacks.FindIndex(a => a.huAbility?.name == "Summon" && a.summon.id == "enemy_minion");
                for (int i = 0; i < 3; i++) { boss.DebugStartAttack(summon); boss.Step(boss.AttackWindupDuration + 0.01f); }
                Assert.AreEqual(14, children.Count); Assert.IsTrue(children.All(c => c.Owner == boss));
                boss.DebugKill(); Assert.IsTrue(children.All(c => c.IsDead));
                children.Clear(); var brood = Enemy(bundle.Get<EnemyDefinition>("enemy_hu_eggsac"), player); brood.Step(6.1f);
                Assert.IsTrue(brood.IsDead); Assert.AreEqual(3, children.Count); Assert.IsTrue(children.All(c => c.Owner == null && !c.IsDead));
                children.Clear(); var rotter = Enemy(bundle.Get<EnemyDefinition>("enemy_hu_rotter"), player); rotter.DebugKill();
                Assert.AreEqual(2, children.Count); Assert.IsTrue(children.All(c => c.Def.huSplitCount == 0 && !c.IsDead));
                Assert.AreEqual(rotter.Def.maxHealth * 0.4f, children[0].Health.Max, 0.1f);
                children.Clear(); var necro = Enemy(bundle.Get<EnemyDefinition>("enemy_hu_necromancer"), player);
                int raising = necro.Def.attacks.FindIndex(a => a.special == EnemySpecial.Summon);
                for (int i = 0; i < 10; i++)
                {
                    necro.DebugStartAttack(raising); necro.Step(necro.AttackWindupDuration + 0.01f);
                    foreach (var child in children) if (!child.IsDead) child.DebugKill();
                }
                Assert.AreEqual(12, children.Count, "nekromanta: 3 × limit 4 w całym życiu, bez nieskończonego przywoływania");
            }
            finally { EnemyBrain.SummonRequested -= Spawn; }
            yield return null;
        }
        [UnityTest]
        public IEnumerator Zone_LargeFrameDoesNotLoseTicks()
        {
            Floor(); var p = Player(); var e = Enemy(bundle.Get<EnemyDefinition>("enemy_hu_hound"), p); Physics.SyncTransforms();
            var hit = new HitData { physical = 50, attacker = e, blockable = false, parryable = false, dodgeable = false };
            float initial = p.Health.Current;
            var small = DamageZone.Spawn(p.transform.position, 2, 2, 0.5f, hit, Faction.Enemy, e, Color.white);
            small.Step(0.01f); float oneTick = initial - p.Health.Current; Object.DestroyImmediate(small.gameObject);
            p.RestoreAll(); p.Actions.Reset();
            var large = DamageZone.Spawn(p.transform.position, 2, 2, 0.5f, hit, Faction.Enemy, e, Color.white);
            large.Step(2);
            Assert.AreEqual(oneTick * 4, initial - p.Health.Current, 0.2f, "t=0, 0.5, 1, 1.5; bez tyknięcia po końcu życia");
            yield return null;
        }
        [UnityTest]
        public IEnumerator BloodRite_SelectsOneAlly_OnCompletion_AndDirectHitBreaksChannel()
        {
            Floor(); var p = Player(); var priestDef = bundle.Get<EnemyDefinition>("enemy_hu_sacrificer");
            var priest = Enemy(priestDef, p); var hound = Enemy(bundle.Get<EnemyDefinition>("enemy_hu_hound"), p);
            var brute = Enemy(bundle.Get<EnemyDefinition>("enemy_hu_brute"), p);
            int rite = priestDef.attacks.FindIndex(a => a.huAbility?.name == "BloodRite");
            priest.DebugStartAttack(rite); priest.Step(priest.AttackWindupDuration + 0.01f);
            var channel = Object.FindAnyObjectByType<HellEnemyChannel>(); Assert.IsNotNull(channel);
            channel.Step(1); Assert.AreEqual(0, brute.FrenzyRemaining); Assert.AreEqual(0, hound.FrenzyRemaining);
            channel.Step(2); Assert.Greater(brute.FrenzyRemaining, 0); Assert.AreEqual(0, hound.FrenzyRemaining); Assert.AreEqual(0, priest.FrenzyRemaining);
            yield return null;
            priest.DebugStartAttack(rite); priest.Step(priest.AttackWindupDuration + 0.01f);
            channel = Object.FindAnyObjectByType<HellEnemyChannel>(); Assert.IsNotNull(channel);
            priest.ReceiveHit(new HitData { physical = 1, sourcePosition = Vector3.forward });
            channel.Step(3); Assert.AreEqual(0, hound.FrenzyRemaining); Assert.AreEqual(EnemyBrain.State.Staggered, priest.CurrentState);
            yield return null;
        }
        [UnityTest]
        public IEnumerator Cocoon_RootsWithoutStun_BlocksDodge_AndWebOnlySlows()
        {
            Floor(); var p = Player(); var e = Enemy(bundle.Get<EnemyDefinition>("enemy_hu_weaver"), p);
            var hit = new HitData { physical = 5, attacker = e, dodgeable = true };
            var cocoon = HellCocoon.Spawn(e, p, hit, 10, 8, 1.2f, 2, 5); cocoon.Step(0.2f);
            Assert.IsTrue(p.Status.Rooted); Assert.IsFalse(p.Status.Incapacitated);
            // Krótkie drgnięcie od samego trafienia kończy się przed unieruchomieniem.
            p.Tick(0.2f); p.Tick(0.4f);
            p.Request(ActionType.Dodge); Assert.AreNotEqual(ActionType.Dodge, p.Actions.Current);
            p.Request(ActionType.LightAttack); Assert.AreEqual(ActionType.LightAttack, p.Actions.Current);
            p.Status.Clear(); p.Actions.Reset(); p.RestoreAll();
            var patch = HellWebPatch.Spawn(e, p, p.transform.position, 2, 5); float hp = p.Health.Current;
            patch.Step(0); Assert.IsFalse(p.Status.Webbed);
            patch.Step(0.1f); Assert.IsTrue(p.Status.Webbed); Assert.AreEqual(hp, p.Health.Current);
            Assert.AreEqual(0.5f, p.Status.MoveMultiplier(bundle.config.balance));
            yield return null;
            // Przejście przez kokon w oknie uniku nie kończy lotu i nie unieruchamia.
            p.Status.Clear(); p.Actions.Reset(); p.Request(ActionType.Dodge); p.Actions.Tick(p.Build.dodge.invulnStart + 0.001f);
            cocoon = HellCocoon.Spawn(e, p, hit, 10, 8, 1.2f, 2, 5); cocoon.Step(0.1f);
            Assert.IsFalse(p.Status.Rooted); cocoon.Step(1);
            Assert.AreEqual(2, HellWebPatch.Active.Count);
            e.DebugKill(); yield return null; yield return null;
            Assert.AreEqual(0, HellWebPatch.Active.Count);
        }
        [UnityTest]
        public IEnumerator EnemyTrial_DoesNotChangeProfile_AndEndsInBestiary()
        {
            var profile = "turris_p1_trial_" + Guid.NewGuid().ToString("N") + ".json";
            var go = new GameObject("P1_TrialGame"); go.SetActive(false); cleanup.Add(go);
            var existingSun = Object.FindAnyObjectByType<Light>();
            var root = go.AddComponent<GameRoot>(); root.config = bundle.config; root.profileFileName = profile; go.SetActive(true);
            yield return null;
            cleanup.Add(root.Player.gameObject); cleanup.Add(root.CameraRig.gameObject); cleanup.Add(GameObject.Find("World"));
            if (existingSun == null) { var sun = Object.FindAnyObjectByType<Light>(); if (sun != null) cleanup.Add(sun.gameObject); }
            string before = JsonUtility.ToJson(root.Meta.Profile);
            root.StartEnemyTrial(bundle.Get<EnemyDefinition>("enemy_hu_boss_titan"));
            Assert.IsTrue(root.IsEnemyTrial); Assert.AreEqual(GameScreen.Playing, root.Screen);
            root.DebugKillAllEnemies(); root.DebugCompleteFloorNow();
            Assert.AreEqual(GameScreen.Bestiary, root.Screen); Assert.AreEqual(before, JsonUtility.ToJson(root.Meta.Profile));
            var file = Path.Combine(Application.persistentDataPath, profile); if (File.Exists(file)) File.Delete(file);
            yield return null;
        }
        [UnityTest]
        public IEnumerator HostileWave_HitsPlayer_AndPauseCannotReleaseNewZone()
        {
            Floor(); var player = Player(); var enemy = Enemy(bundle.Get<EnemyDefinition>("enemy_hu_hound"), player);
            Physics.SyncTransforms();
            var hit = new HitData { physical = 50, attacker = enemy, blockable = false, parryable = false, dodgeable = false };
            var wave = WaveEffect.Spawn(enemy, Vector3.zero, Vector3.forward, 10, 10, 2, 0, hit, Color.white);
            float hp = player.Health.Current; wave.Step(0); Assert.AreEqual(hp, player.Health.Current);
            wave.Step(0.3f); Assert.Less(player.Health.Current, hp, "fala wroga musi używać frakcji właściciela");
            Time.timeScale = 0; hp = player.Health.Current;
            DamageZone.Spawn(player.transform.position, 3, 3, 0.2f, hit, Faction.Enemy, enemy, Color.white);
            yield return null; yield return null;
            Assert.AreEqual(hp, player.Health.Current, "strefa utworzona podczas pauzy nie trafia");
            Time.timeScale = 1; enemy.DebugKill(); yield return null; yield return null;
            Assert.AreEqual(0, DamageZone.Active.Count); Assert.AreEqual(0, WaveEffect.Active.Count);
        }
        [UnityTest]
        public IEnumerator StoneWard_BreaksIntoDaze_AndChannelsStopOnDeath()
        {
            Floor(); var p = Player(); var def = bundle.Get<EnemyDefinition>("enemy_hu_boss_titan"); var e = Enemy(def, p);
            int ward = def.attacks.FindIndex(a => a.huAbility != null && a.huAbility.name == "StoneWard");
            e.DebugStartAttack(ward); e.Step(2);
            Assert.Greater(e.WardRemaining, 0);
            float health = e.Health.Current; float capacity = e.WardRemaining;
            e.ReceiveHit(new HitData { physical = capacity / (1 - def.physicalResist) + 0.001f, sourcePosition = Vector3.forward, blockable = false, parryable = false, dodgeable = false });
            Assert.AreEqual(health, e.Health.Current, 0.1f); Assert.AreEqual(0, e.WardRemaining, 0.01f); Assert.AreEqual(EnemyBrain.State.Staggered, e.CurrentState);
            int rocks = def.attacks.FindIndex(a => a.huAbility != null && a.huAbility.name == "Rockfall");
            e.DebugStartAttack(rocks); e.Step(3);
            Assert.Greater(Object.FindObjectsByType<HellEnemyChannel>().Length, 0);
            e.DebugKill(); yield return null; yield return null;
            Assert.AreEqual(0, Object.FindObjectsByType<HellEnemyChannel>().Length); Assert.AreEqual(0, DelayedStrike.Active.Count);
        }
        [UnityTest]
        public IEnumerator AllClassesAndStartingLoadouts_AcceptKeyboardAndGamepadSignature()
        {
            pad = InputSystem.AddDevice<Gamepad>("P0_Pad"); keyboard = InputSystem.AddDevice<Keyboard>("P0_Keyboard");
            foreach (var cls in bundle.config.classes)
            for (int index = 0; index < cls.loadouts.Count; index++)
            {
                var player = Player(cls, index);
                var input = player.gameObject.AddComponent<PlayerInputReader>();
                int requested = -1; input.SkillPressed += slot => { requested = slot; player.RequestSkill(slot); };
                input.SetGameplayEnabled(true); yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G)); yield return null; yield return null;
                Assert.AreEqual(PlayerCombat.SignatureSlot, requested, cls.id + ": klawiatura");
                Assert.AreEqual(1, player.FeatureTelemetry.presses, cls.id + ": cecha klasowa z klawiatury");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                player.Actions.Reset(); player.OnFloorStart(); player.RestoreAll(); requested = -1;
                InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.Select)); yield return null; yield return null;
                Assert.AreEqual(PlayerCombat.SignatureSlot, requested, cls.id + ": pad");
                Assert.AreEqual(2, player.FeatureTelemetry.presses, cls.id + ": cecha klasowa z pada");
                InputSystem.QueueStateEvent(pad, new GamepadState()); yield return null;
                Object.Destroy(player.gameObject); yield return null;
            }
        }
        [UnityTest]
        public IEnumerator AllNewModels_HaveClips_AndWeaponAssetsHaveModels()
        {
            foreach (var row in HellEnemyCatalog.Rows)
            {
                var def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>($"Assets/TurrisDamnatorum/Content/Enemies/{row.targetId}.asset");
                Assert.IsNotNull(def, row.Id); Assert.IsNotNull(def.visual, row.Id); Assert.IsNotNull(def.visual.idle, row.Id);
                Assert.IsNotNull(def.visual.Find(AnimAction.Death, AttackAnim.Auto), row.Id);
                var visual = WorldBuilder.CreateEnemyVisual(def, Vector3.zero, null); cleanup.Add(visual.gameObject);
                visual.Init(new IdleAnimSource()); visual.Tick(1f / 30);
                Assert.IsNotNull(visual.Model); Assert.Greater(visual.Model.GetComponentsInChildren<SkinnedMeshRenderer>().Length, 0, row.Id);
                Object.Destroy(visual.gameObject); yield return null;
            }
            foreach (var id in new[] { "hu_flail2h", "hu_morningstar2h", "hu_staff", "hu_staff_summoner", "hu_staff_witch", "hu_greataxe_bearded", "hu_warhammer_siege" })
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<ItemDefinition>($"Assets/TurrisDamnatorum/Content/Items/{id}.asset").model, id);
        }
    }
}
#endif
