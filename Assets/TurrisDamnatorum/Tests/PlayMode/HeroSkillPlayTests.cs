using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Skille i cechy bohaterów z Hell Unleashed w działającym silniku (fizyka, pociski, sługi, aury).</summary>
    public class HeroSkillPlayTests
    {
        readonly List<Object> cleanup = new List<Object>();
        DefaultContent.Bundle bundle;

        [SetUp] public void SetUp() { Time.timeScale = 1f; bundle = TestContent.Create(); }

        [TearDown]
        public void TearDown()
        {
            Servant.DismissAll();
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
        }

        PlayerCombat Hero(string classId, int loadout = 0, string opening = null)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(80, 1, 80);
            cleanup.Add(floor);
            var camGo = new GameObject("TestCam");
            camGo.AddComponent<Camera>();
            var rig = camGo.AddComponent<CameraRig>();
            cleanup.Add(camGo);
            var p = WorldBuilder.CreatePlayer();
            cleanup.Add(p);
            var ctrl = p.GetComponent<PlayerController>();
            ctrl.cameraRig = rig; ctrl.InputEnabled = false;
            rig.target = p.transform; rig.player = ctrl;
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(classId), difficulty = bundle.config.difficulties[0], loadoutIndex = loadout };
            if (opening != null) plan.startingSkill = bundle.Get<SpellDefinition>(opening);
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(RunFactory.Create(plan, bundle.config, 7), bundle.config);
            p.GetComponent<PlayerVisuals>().Refresh();
            return pc;
        }

        EnemyBrain Dummy(PlayerCombat pc, Vector3 pos)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = "Manekin"; def.maxHealth = 5000; def.maxPoise = 100000; def.scale = 1f; def.moveSpeed = 0f; def.strafeChance = 0f;
            cleanup.Add(def);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(-pos.normalized);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            return e;
        }

        int Slot(PlayerCombat pc, string id) => pc.Run.SlotOf(pc.Run.FindSpell(bundle.Get<SpellDefinition>(id)));

        [UnityTest]
        public IEnumerator CopiedConeSkills_UseAirAndPhysicalParticles_InsteadOfFrost()
        {
            var mage = Hero("class_hu_battlemage");
            yield return new WaitForSeconds(0.1f);
            mage.RequestSkill(PlayerCombat.SignatureSlot);
            yield return new WaitForSeconds(0.4f);
            var air = Object.FindObjectsByType<HellParticles>().FirstOrDefault(f => f.name == "HU_Cone_Air");
            Assert.IsNotNull(air,"Cięcie burzy wywołuje cząsteczki powietrza");
            Assert.IsFalse(air.GetComponentsInChildren<ParticleSystem>().Any(p => p.name == "ice_shards"));
            Object.Destroy(mage.gameObject);
            var warrior = Hero("class_knight",0,"hu_cleave");
            yield return new WaitForSeconds(0.1f);
            warrior.RequestSkill(Slot(warrior,"hu_cleave"));
            yield return new WaitForSeconds(0.4f);
            var physical = Object.FindObjectsByType<HellParticles>().FirstOrDefault(f => f.name == "HU_Cone_Physical");
            Assert.IsNotNull(physical,"Rozpłatanie nie używa FrostCone");
            Assert.IsFalse(physical.GetComponentsInChildren<ParticleSystem>().Any(p => p.name == "frost_mist"));
        }

        [UnityTest]
        public IEnumerator Cleave_KnocksDownEnemies_ButNotBosses()
        {
            var warrior = Hero("class_knight", 0, "hu_cleave");
            var demon = Dummy(warrior, new Vector3(-0.6f, 0.05f, 1.8f));
            var boss = Dummy(warrior, new Vector3(0.6f, 0.05f, 1.8f));
            boss.Def.isBoss = true;
            yield return new WaitForSeconds(1.1f); // koniec uśpienia po pojawieniu się
            warrior.transform.rotation = Quaternion.identity;
            int slot = Slot(warrior, "hu_cleave");
            Assert.AreEqual(5f, warrior.Skill(slot).instance.Cooldown, 0.01f, "odnowienie Rozpłatania 5 s (balans po serii 5)");
            warrior.RequestSkill(slot);
            float t = 0f;
            while (demon.Health.Current >= 5000f && t < 1.5f) { t += Time.deltaTime; yield return null; }
            Assert.Less(demon.Health.Current, 5000f, "Rozpłatanie trafia");
            Assert.Less(boss.Health.Current, 5000f, "boss też dostaje obrażenia");
            Assert.IsTrue(demon.Status.Stunned, "trafiony demon leży (powalenie)");
            Assert.AreEqual(EnemyBrain.State.Staggered, demon.CurrentState);
            Assert.IsFalse(boss.Status.Stunned, "bossa powalenie nie dotyczy");
        }

        [UnityTest]
        public IEnumerator Maelstrom_PulsesUseWaterParticles_WithFrostDamage()
        {
            var mage=Hero("class_hu_mage_water");
            var enemy=Dummy(mage,new Vector3(0,0.05f,3));
            yield return new WaitForSeconds(0.1f);
            mage.RequestSkill(PlayerCombat.SignatureSlot);
            yield return new WaitForSeconds(0.7f);
            Assert.IsTrue(Object.FindObjectsByType<HellParticles>().Any(f => f.name == "HU_Burst_Water"));
            Assert.Greater(enemy.Status.ChillStacks,0,"Zachowana mechanika chłodu Turris");
        }

        [UnityTest]
        public IEnumerator WindSlash_WavePassesThroughLineOfEnemies()
        {
            var pc = Hero("class_knight", 0, "hu_wind_slash");
            var near = Dummy(pc, new Vector3(0, 0.05f, 3f));
            var far = Dummy(pc, new Vector3(0, 0.05f, 7f));
            yield return new WaitForSeconds(0.1f);
            pc.RequestSkill(Slot(pc, "hu_wind_slash"));
            yield return new WaitForSeconds(1.5f);
            Assert.Less(near.Health.Current, 5000f, "Fala trafia pierwszego…");
            Assert.Less(far.Health.Current, 5000f, "…i przenika do następnego");
        }

        [UnityTest]
        public IEnumerator Legion_SummonsHounds_ThatBiteEnemies()
        {
            var pc = Hero("class_hu_summoner");
            var e = Dummy(pc, new Vector3(0, 0.05f, 5f));
            yield return new WaitForSeconds(0.1f);
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(2, Servant.Active.Count(s => s.Kind == ServantKind.Hound), "Dwa widmowe ogary");
            yield return new WaitForSeconds(3f);
            Assert.Less(e.Health.Current, 5000f, "Ogary gryzą wroga");
        }

        [UnityTest]
        public IEnumerator BladeFan_RingHitsEnemiesAllAround()
        {
            var pc = Hero("class_rogue");
            var front = Dummy(pc, new Vector3(0, 0.05f, 3f));
            var back = Dummy(pc, new Vector3(0, 0.05f, -3f));
            var side = Dummy(pc, new Vector3(2.5f, 0.05f, 0));
            yield return new WaitForSeconds(0.1f);
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            yield return new WaitForSeconds(1.2f);
            Assert.Less(front.Health.Current, 5000f);
            Assert.Less(back.Health.Current, 5000f, "Pierścień ostrzy trafia też za plecami");
            Assert.Less(side.Health.Current, 5000f);
        }

        [UnityTest]
        public IEnumerator PiercingShot_VisitsAllHitsWithinOneLargeFrame()
        {
            var pc = Hero("class_rogue");
            var near = Dummy(pc,new Vector3(0,0.05f,4));
            var far = Dummy(pc,new Vector3(0,0.05f,7));
            var projectile = Projectile.Spawn(new Vector3(0,1.1f,1),Vector3.forward,24,
                new HitData { physical=40,blockable=true,dodgeable=true },Faction.Player,pc,Color.white,
                0.12f,3,null,0,true,new ProjectileOptions { pierce=4 });
            cleanup.Add(projectile.gameObject);
            projectile.enabled = false;
            yield return null;
            Physics.SyncTransforms();
            projectile.Step(0.4f); // 9,6 m w jednej klatce, obaj wrogowie na tym samym odcinku.
            Assert.Less(near.Health.Current,5000);
            Assert.Less(far.Health.Current,5000,"Przebicie nie pomija drugiego celu w długiej klatce");
        }

        [UnityTest]
        public IEnumerator PiercingShot_GoesThroughSeveralEnemies()
        {
            var pc = Hero("class_rogue", 0, "hu_piercing_shot");
            var a = Dummy(pc, new Vector3(0, 0.05f, 4f));
            var b = Dummy(pc, new Vector3(0, 0.05f, 7f));
            yield return new WaitForSeconds(0.1f);
            pc.RequestSkill(Slot(pc, "hu_piercing_shot"));
            yield return new WaitForSeconds(1.2f);
            Assert.Less(a.Health.Current, 5000f);
            Assert.Less(b.Health.Current, 5000f, "Strzała przebija pierwszego wroga");
        }

        [UnityTest]
        public IEnumerator HolyFireAura_BurnsEnemiesNearby_UntilSwitchedOff()
        {
            var pc = Hero("class_hu_paladin", 0, "hu_aura_holy_fire");
            var e = Dummy(pc, new Vector3(0, 0.05f, 2f));
            yield return new WaitForSeconds(0.1f);
            int slot = Slot(pc, "hu_aura_holy_fire");
            pc.RequestSkill(slot);
            Assert.AreEqual(1, pc.Auras.Count);
            yield return new WaitForSeconds(2.6f);
            float hp = e.Health.Current;
            Assert.Less(hp, 5000f, "Aura parzy co sekundę");
            pc.RequestSkill(slot);
            Assert.AreEqual(0, pc.Auras.Count, "Ponowne naciśnięcie gasi aurę");
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual(hp, e.Health.Current, 0.5f, "Zgaszona aura nie rani");
        }

        [UnityTest]
        public IEnumerator DeathMark_DashesToEnemy_AndBleedsIt()
        {
            var pc = Hero("class_hu_assassin");
            var e = Dummy(pc, new Vector3(0, 0.05f, 5f));
            yield return new WaitForSeconds(0.1f);
            Vector3 start = pc.transform.position;
            pc.LockTarget = e;
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            yield return new WaitForSeconds(1f);
            Assert.Greater(Vector3.Distance(start, pc.transform.position), 2f, "Doskok do wroga");
            Assert.Less(e.Health.Current, 5000f);
            Assert.Greater(e.Status.BleedStacks, 0, "Znak śmierci nakłada krwawienie");
        }

        [UnityTest]
        public IEnumerator ShoulderCharge_DodgeHitsEnemyOnTheWay()
        {
            var pc = Hero("class_knight");
            var e = Dummy(pc, new Vector3(0, 0.05f, 1.6f));
            yield return new WaitForSeconds(0.1f);
            pc.MoveIntent = Vector3.forward;
            pc.Request(ActionType.Dodge);
            yield return new WaitForSeconds(0.6f);
            Assert.Less(e.Health.Current, 5000f, "Szarża barkiem rani");
            Assert.Greater(e.Status.StaggerStacks + (e.Status.Stunned ? 1 : 0), 0, "…i zachwiewa");
        }

        [UnityTest]
        public IEnumerator ElementalArrow_ExplodesAndLeavesFlame()
        {
            var pc = Hero("class_hu_arcane_archer");
            var target = Dummy(pc, new Vector3(0, 0.05f, 6f));
            var neighbour = Dummy(pc, new Vector3(1.5f, 0.05f, 6.5f));
            yield return new WaitForSeconds(0.1f);
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            yield return new WaitForSeconds(1.2f);
            Assert.Less(target.Health.Current, 5000f);
            Assert.Less(neighbour.Health.Current, 5000f, "Wybuch rani sąsiada");
            Assert.IsTrue(DamageZone.Active.Count > 0, "Zostaje płomień");
        }
    }
}
