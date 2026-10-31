using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Cechy klasowe w działającym silniku (rozdz. 16.3–16.4): prawdziwy gracz, koszty, fizyka, pociski, sługi i wrogowie z AI.
    /// Każdy test sprawdza minimalny przypadek rozstrzygający z macierzy szczególnych regresji.
    /// </summary>
    public class ClassFeaturePlayTests
    {
        readonly List<Object> cleanup = new List<Object>();
        DefaultContent.Bundle bundle;
        ClassFeatureTuning T => bundle.config.classFeatures;

        [SetUp] public void SetUp() { Time.timeScale = 1f; bundle = TestContent.Create(); EnemyBrain.DebugNoEvasion = true; }

        [TearDown]
        public void TearDown()
        {
            Servant.DismissAll(); FeatureDecoy.ClearAll(); PinnedShadow.ClearAll(); EarthWall.ClearAll();
            foreach (var p in Projectile.Active.ToArray()) if (p != null) Object.Destroy(p.gameObject);
            foreach (var z in DamageZone.Active.ToArray()) if (z != null) Object.Destroy(z.gameObject);
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            EnemyBrain.DebugNoEvasion = false;
        }

        PlayerCombat Hero(string classId, string featureId = null, int loadout = 0, string opening = null)
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
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(classId), difficulty = bundle.config.difficulties[0], loadoutIndex = loadout, classFeatureId = featureId };
            if (opening != null) plan.startingSkill = bundle.Get<SpellDefinition>(opening);
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(RunFactory.Create(plan, bundle.config, 7), bundle.config);
            pc.DebugNoPassiveBlock = true;
            p.GetComponent<PlayerVisuals>().Refresh();
            return pc;
        }

        EnemyBrain Enemy(PlayerCombat pc, Vector3 pos, bool attacks = false, bool boss = false, float hp = 5000f, float speed = 0f)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = boss ? "Boss" : "Manekin"; def.maxHealth = hp; def.maxPoise = 100000; def.scale = 1f; def.moveSpeed = speed; def.strafeChance = 0f;
            def.isBoss = boss; def.preferredRange = 1.4f; def.attackInterval = new Vector2(0.2f, 0.3f);
            if (attacks) def.attacks.Add(new EnemyAttackEntry { attack = new AttackDefinition { name = "Pazur", baseDamage = 20f, reach = 2f, radius = 0.9f, windup = 0.3f, lunge = 0f }, maxRange = 3f });
            cleanup.Add(def);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(pc.transform.position - pos);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            return e;
        }

        int Slot(PlayerCombat pc, string id) => pc.Run.SlotOf(pc.Run.FindSpell(bundle.Get<SpellDefinition>(id)));
        static void Press(PlayerCombat pc) => pc.RequestSkill(PlayerCombat.SignatureSlot);

        // ================================================================== Łotrzyk A – wabik przejmuje atak AI

        [UnityTest]
        public IEnumerator FalseWitness_AiAttacksDecoy_ThenReturnsToPlayer()
        {
            var pc = Hero("class_rogue");
            var e = Enemy(pc, new Vector3(0, 0.05f, 2.5f), attacks: true, speed: 3f);
            pc.DebugInvulnerable = true;
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            var f = (FalseWitnessFeature)pc.Feature;
            Assert.IsNotNull(f.Decoy);
            Assert.IsTrue(e.Deceived, "AI wybrało wabik");
            pc.GetComponent<CharacterController>().enabled = false;
            pc.transform.position = new Vector3(6f, 0f, -4f);
            pc.GetComponent<CharacterController>().enabled = true;
            float t = 0f;
            while (t < 4f && pc.FeatureTelemetry.Get("wabik: przekierowane ataki") < 1f) { t += Time.deltaTime; yield return null; }
            Assert.GreaterOrEqual(pc.FeatureTelemetry.Get("wabik: przekierowane ataki"), 1f, "atak poszedł w wabik");
            Assert.IsFalse(e.Deceived, "po ataku wróg wraca do gracza");
        }

        // ================================================================== Zabójca A – cień i ciało jedną tożsamością

        [UnityTest]
        public IEnumerator PinnedShadow_ForwardsHit_AndBodyPlusShadowCountOnce()
        {
            var pc = Hero("class_hu_assassin");
            var e = Enemy(pc, new Vector3(0, 0.05f, 3f));
            yield return new WaitForSeconds(0.05f);
            pc.LockTarget = e;
            Press(pc);
            var shadow = ((PinnedShadowFeature)pc.Feature).Shadow;
            Assert.IsNotNull(shadow);
            Vector3 at = shadow.transform.position;
            e.transform.position = new Vector3(4f, 0.05f, 3f); // ofiara odchodzi, cień zostaje
            yield return new WaitForFixedUpdate(); yield return null;
            var h = pc.FeatureHitForTest(100f);
            float hp = e.Health.Current;
            HitQuery.Sphere(at + Vector3.up, 0.8f, Faction.Player, new HitTracker(), r => HitQuery.Apply(pc, r, h, false));
            float one = hp - e.Health.Current;
            Assert.Greater(one, 0f, "cios w cień trafia ofiarę");
            e.transform.position = at + new Vector3(1.2f, 0f, 0f);
            yield return new WaitForFixedUpdate(); yield return null;
            hp = e.Health.Current;
            var tr = new HitTracker();
            HitQuery.Sphere(at + Vector3.up, 3f, Faction.Player, tr, r => HitQuery.Apply(pc, r, h, false));
            Assert.AreEqual(1, tr.Count, "ciało i cień – jeden cel");
            Assert.AreEqual(one, hp - e.Health.Current, one * 0.05f + 0.5f, "jedna porcja obrażeń");
        }

        // ================================================================== Zabójca B – egzekucja ponad progiem bez zmiany PŻ

        [UnityTest]
        public IEnumerator OpenArtery_ExecuteBonusAboveThreshold_RealHealthUntouched()
        {
            var pc = Hero("class_hu_assassin", "class_feature.assassin.open_artery");
            var e = Enemy(pc, new Vector3(0, 0.05f, 3f), hp: 100000f);
            yield return new WaitForSeconds(0.05f);
            var h = pc.FeatureHitForTest(100f);
            Assert.Greater(h.executeBonus, 0f, "premia egzekucji Zabójcy");
            float hp = e.Health.Current;
            HitQuery.Apply(pc, e, h, false);
            float plain = hp - e.Health.Current;
            pc.LockTarget = e;
            float before = e.Health.Current;
            Press(pc);
            Assert.AreEqual(before, e.Health.Current, "znacznik nie zmienia PŻ");
            Assert.IsTrue(e.ExecuteQualified);
            hp = e.Health.Current;
            HitQuery.Apply(pc, e, h, false);
            float marked = hp - e.Health.Current;
            Assert.AreEqual(plain * (1f + h.executeBonus), marked, plain * 0.03f + 0.5f, "egzekucja ponad progiem");
            Assert.AreEqual(0, e.PhaseIndex, "fazy widzą prawdziwe życie");
            Assert.GreaterOrEqual(pc.FeatureTelemetry.Get("tętnica: egzekucje poza progiem"), 1f);
        }

        // ================================================================== Mag powietrza A – przechwycenie i salwa

        [UnityTest]
        public IEnumerator SilentEye_CapturesEnemyProjectile_AndReleasesItOnce()
        {
            var pc = Hero("class_hu_mage_air");
            var target = Enemy(pc, new Vector3(0, 0.05f, 6f));
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            float hp = pc.Health.Current;
            var bolt = new HitData { physical = 40, blockable = true, dodgeable = true, mods = StatusModifiers.None, sourcePosition = new Vector3(4f, 1f, 0f) };
            var p = Projectile.Spawn(new Vector3(4.5f, 1f, 0f), Vector3.left, 6f, bolt, Faction.Enemy, null, Color.red, 0.3f, 3f);
            float t = 0f;
            while (t < 1.5f && p != null) { t += Time.deltaTime; yield return null; }
            var eye = (SilentEyeFeature)pc.Feature;
            Assert.IsTrue(p == null, "pocisk zniknął ze świata");
            Assert.AreEqual(1, eye.Count, "jeden w magazynie");
            Assert.AreEqual(hp, pc.Health.Current, 0.01f, "nie trafił gracza");
            pc.LockTarget = target;
            Press(pc);
            Assert.AreEqual(0, eye.Count);
            Assert.AreEqual(1f, pc.FeatureTelemetry.Get("powietrze: wypuszczone pociski"));
            t = 0f; float th = target.Health.Current;
            while (t < 1.5f && target.Health.Current >= th) { t += Time.deltaTime; yield return null; }
            Assert.Less(target.Health.Current, th, "salwa trafia");
        }

        // ================================================================== Mag ziemi A – ściana blokuje pocisk

        [UnityTest]
        public IEnumerator Bastion_RealWallBlocksEnemyProjectile()
        {
            var pc = Hero("class_hu_mage_earth");
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            var wall = ((BastionFeature)pc.Feature).Wall;
            Assert.IsNotNull(wall, "ściana stoi");
            yield return new WaitForFixedUpdate(); yield return null;
            float hp = pc.Health.Current;
            var bolt = new HitData { physical = 40, blockable = true, dodgeable = true, mods = StatusModifiers.None };
            var p = Projectile.Spawn(pc.transform.position + Vector3.forward * 8f + Vector3.up, Vector3.back, 12f, bolt, Faction.Enemy, null, Color.red, 0.2f, 3f);
            yield return new WaitForSeconds(1.2f);
            Assert.IsTrue(p == null);
            Assert.AreEqual(hp, pc.Health.Current, 0.01f, "pocisk rozbił się na ścianie");
            Assert.IsTrue(wall.Blocks(pc.transform.position, pc.transform.position + Vector3.forward * 8f), "droga AI przecina ścianę");
        }

        // ================================================================== Mag wody B – pole zmienia miejsce bez odnowienia

        [UnityTest]
        public IEnumerator DeepCurrent_MovesOwnZone_KeepsRemainingTime()
        {
            var pc = Hero("class_hu_mage_water", "class_feature.water.deep_current");
            var e = Enemy(pc, new Vector3(3f, 0.05f, 6f));
            yield return new WaitForSeconds(0.05f);
            var h = pc.FeatureHitForTest(5f);
            var z = DamageZone.Spawn(new Vector3(-2f, 0f, 2f), 2f, 5f, 0.5f, h, Faction.Player, pc, Color.cyan);
            yield return new WaitForSeconds(1f);
            float rem = z.Remaining;
            float mana = pc.Mana.Current;
            pc.LockTarget = e;
            Press(pc);
            Assert.Less(pc.Mana.Current, mana, "opłata za pole");
            yield return new WaitForSeconds(T.currentMoveSeconds + 0.1f);
            Assert.Less(Vector3.Distance(z.transform.position, e.transform.position), 1.5f, "pole na celu");
            Assert.Less(z.Remaining, rem, "czas biegnie dalej – bez odnowienia");
            Assert.AreEqual(1, z.Moves);
        }

        // ================================================================== Paladyn B – odwrócona aura i powrót

        [UnityTest]
        public IEnumerator Judgement_InvertsDefiance_OnTarget_AndReturnsWithoutDoubleBuff()
        {
            var pc = Hero("class_hu_paladin", "class_feature.paladin.judgement", 0, "hu_aura_defiance");
            var e = Enemy(pc, new Vector3(0, 0.05f, 4f));
            yield return new WaitForSeconds(0.05f);
            float baseDef = pc.Build.stats[StatType.PhysicalDefense];
            pc.RequestSkill(Slot(pc, "hu_aura_defiance"));
            float auraDef = pc.Build.stats[StatType.PhysicalDefense];
            Assert.Greater(auraDef, baseDef);
            pc.LockTarget = e;
            Press(pc);
            Assert.IsNotNull(e.Judgement);
            Assert.Greater(e.Judgement.armourCut, 0f, "Niezłomność obniża obronę wroga");
            Assert.AreEqual(baseDef, pc.Build.stats[StatType.PhysicalDefense], 0.01f, "paladyn traci premię aury");
            yield return new WaitForSeconds(T.judgementSeconds + 0.3f);
            Assert.IsNull(e.Judgement, "koniec sądu");
            Assert.AreEqual(auraDef, pc.Build.stats[StatType.PhysicalDefense], 0.01f, "aura wraca raz");
            Assert.AreEqual(1, pc.Auras.Count);
        }

        // ================================================================== Przywoływacz A/B

        [UnityTest]
        public IEnumerator Incarnation_ConsumesServant_AttacksAsForm_ReturnsModel()
        {
            var pc = Hero("class_hu_summoner");
            var e = Enemy(pc, new Vector3(0, 0.05f, 2.5f));
            yield return new WaitForSeconds(0.05f);
            pc.RequestSkill(Slot(pc, "hu_summon_imps"));
            yield return new WaitForSeconds(0.8f);
            int servants = Servant.Active.Count;
            Assert.AreEqual(2, servants);
            Press(pc);
            var f = (IncarnationFeature)pc.Feature;
            Assert.IsTrue(f.InForm);
            Assert.AreEqual(servants - 1, Servant.Active.Count, "sługa zużyty");
            pc.LockTarget = e;
            float hp = e.Health.Current;
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(1, f.FormAttacks);
            Assert.Less(e.Health.Current, hp, "ostrzał chochlika gracza");
            var r = pc.ReceiveHit(new HitData { physical = 30, blockable = true, dodgeable = true, sourcePosition = pc.transform.position + Vector3.forward * 2, mods = StatusModifiers.None, attacker = e });
            Assert.Greater(r.healthDamage, 0f, "życie należy do gracza – brak nietykalności sługi");
            Press(pc);
            Assert.IsFalse(f.InForm);
            Assert.IsTrue(pc.GetComponentsInChildren<Renderer>().Any(x => x.enabled && !(x is ParticleSystemRenderer)), "model wraca");
        }

        [UnityTest]
        public IEnumerator Chimera_MergesTwoImps_ReservesSlots_SoulBondCountsOne()
        {
            var pc = Hero("class_hu_summoner", "class_feature.summoner.chimera");
            yield return new WaitForSeconds(0.05f);
            pc.RequestSkill(Slot(pc, "hu_summon_imps"));
            yield return new WaitForSeconds(0.8f);
            Assert.AreEqual(2, Servant.Active.Count);
            float rem = Servant.Active.Max(s => s.Remaining);
            Press(pc);
            Assert.IsTrue(((ChimeraFeature)pc.Feature).Previewing);
            Press(pc);
            Assert.AreEqual(1, Servant.Active.Count, "jedna istota");
            var ch = Servant.Active[0];
            Assert.IsTrue(ch.IsChimera);
            Assert.AreEqual(2, ch.Parts.Count);
            Assert.AreEqual(2, Servant.SlotsUsed, "zajmuje miejsca składników");
            Assert.LessOrEqual(ch.Remaining, rem + 0.01f, "bez odnowienia czasu");
        }

        // ================================================================== Mag powietrza B – zderzenie i naprężenie

        [UnityTest]
        public IEnumerator CollidingFronts_WallCollisionDischarges_BossGetsStrainOnly()
        {
            var pc = Hero("class_hu_mage_air", "class_feature.air.colliding_fronts");
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0, 1, 5.2f); wall.transform.localScale = new Vector3(6, 2, 0.5f);
            cleanup.Add(wall);
            var e = Enemy(pc, new Vector3(0, 0.05f, 3.6f));
            var boss = Enemy(pc, new Vector3(3f, 0.05f, 3f), boss: true);
            yield return new WaitForSeconds(0.1f);
            Press(pc);
            var h = pc.FeatureHitForTest(10f); h.featureEffect = false; h.knockback = 3f; h.originId = 991;
            pc.FeatureOutgoingForTest(ref h);
            Assert.Greater(h.frontsBudget, 0f);
            HitQuery.Apply(pc, e, h, false);
            yield return new WaitForSeconds(0.5f);
            var f = (CollidingFrontsFeature)pc.Feature;
            Assert.AreEqual(1, f.Discharges, "zderzenie ze ścianą");
            h.originId = 992;
            Vector3 bossPos = boss.transform.position;
            HitQuery.Apply(pc, boss, h, false);
            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(1, f.Strains, "odporny boss – naprężenie");
        }

        // ================================================================== Łucznik A/B

        [UnityTest]
        public IEnumerator HuntersNails_EmbedOnBasicShots_RipDealsStoredBudget()
        {
            var pc = Hero("class_hu_archer");
            var e = Enemy(pc, new Vector3(0, 0.05f, 6f), hp: 100000f);
            pc.LockTarget = e;
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            for (int i = 0; i < 2; i++) { pc.Request(ActionType.LightAttack); yield return new WaitForSeconds(1.0f); }
            var f = (HuntersNailsFeature)pc.Feature;
            Assert.GreaterOrEqual(f.NailsOn(e), 1, "groty osadzone");
            float budget = f.NailBudget, hp = e.Health.Current;
            Press(pc);
            Assert.AreEqual(0, f.NailCount);
            Assert.Greater(hp - e.Health.Current, budget * 0.5f, "wyrwanie zadaje rany");
        }

        [UnityTest]
        public IEnumerator FiringPosition_ShotLeavesFromPost_OwnedByPlayer()
        {
            var pc = Hero("class_hu_archer", "class_feature.archer.firing_position");
            var e = Enemy(pc, new Vector3(4f, 0.05f, 8f), hp: 100000f);
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            var post = ((FiringPositionFeature)pc.Feature).Post;
            Assert.IsNotNull(post);
            pc.LockTarget = e;
            pc.Request(ActionType.LightAttack);
            float t = 0f;
            while (t < 1.5f && post.Shots == 0) { t += Time.deltaTime; yield return null; }
            Assert.AreEqual(1, post.Shots, "jeden strzał z punktu");
            var shots = Projectile.Active.Where(p => p != null && p.Side == Faction.Player).ToList();
            Assert.AreEqual(1, shots.Count, "bez drugiej, niewidzialnej kopii");
            Assert.IsTrue(shots[0].Hit.hasLaunchPoint);
            Assert.Less(Vector3.Distance(shots[0].Hit.launchPoint, post.Muzzle), 0.5f);
            Assert.AreSame(pc, shots[0].Owner);
        }

        // ================================================================== Łucznik arkanów A/B

        [UnityTest]
        public IEnumerator SpellArrow_PaysOnce_ReleasesSpellAtHit()
        {
            var pc = Hero("class_hu_arcane_archer");
            var e = Enemy(pc, new Vector3(0, 0.05f, 7f), hp: 100000f);
            pc.LockTarget = e;
            var tear = pc.Run.FindSpell(bundle.Get<SpellDefinition>("hu_glacial_tear"));
            pc.Run.AssignSlot(tear, 1); pc.RefreshBuild();
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            pc.RequestSkill(1);
            float mana = pc.Mana.Current;
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(0.8f);
            Assert.AreEqual(mana - pc.Skill(1).manaCost, pc.Mana.Current, 1f, "pełna cena czaru raz");
            Assert.IsFalse(pc.Skill(1).instance.Ready, "odnowienie czaru");
            yield return new WaitForSeconds(2.2f);
            Assert.AreEqual(1f, pc.FeatureTelemetry.Get("strzała zaklęcia: uwolnienia"), "jedno uwolnienie");
            Assert.IsTrue(e.Status.Soaked, "Łza lodowca w trafionym celu");
        }

        [UnityTest]
        public IEnumerator AnsweringArrow_BleedingTargetGetsLightning()
        {
            var pc = Hero("class_hu_arcane_archer", "class_feature.arcane_archer.answering_arrow");
            var e = Enemy(pc, new Vector3(0, 0.05f, 6f), hp: 100000f);
            pc.LockTarget = e;
            yield return new WaitForSeconds(0.05f);
            e.ApplyStatus(StatusKind.Bleed, 200f, 0f);
            Press(pc);
            Assert.AreEqual(T.answerShots, ((AnsweringArrowFeature)pc.Feature).Charges);
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(T.answerShots - 1, ((AnsweringArrowFeature)pc.Feature).Charges, "jeden ładunek na strzał");
            Assert.GreaterOrEqual(pc.FeatureTelemetry.Get("odpowiedź: " + Names.Element(Element.Lightning)), 1f, "błyskawica na krwawiącym");
        }

        // ================================================================== Mag ognia A/B

        [UnityTest]
        public IEnumerator LivingFurnace_OnlyRealLostLifeMakesEmber_ImmunityGivesNone()
        {
            var pc = Hero("class_hu_mage_fire");
            yield return new WaitForSeconds(0.05f);
            float hp = pc.Health.Current;
            Press(pc);
            yield return new WaitForSeconds(2f);
            var f = (LivingFurnaceFeature)pc.Feature;
            float lost = hp - pc.Health.Current;
            Assert.Greater(lost, 0f, "płomień kosztuje życie");
            Assert.AreEqual(pc.FeatureTelemetry.Get("ogień: koszt własnych PŻ"), f.Ember, 0.5f, "Żar = faktycznie utracone PŻ");

            var immune = Hero("class_hu_mage_fire");
            immune.Status.BurnImmune = true;
            Press(immune);
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(0f, ((LivingFurnaceFeature)immune.Feature).Ember, "odporność – brak paliwa");
        }

        [UnityTest]
        public IEnumerator GreatFire_TwoSourcesMakeTwoFamilies_OnRealEnemy()
        {
            var pc = Hero("class_hu_mage_fire", "class_feature.fire.great_fire");
            var e = Enemy(pc, new Vector3(0, 0.05f, 3f), hp: 100000f);
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            var basic = pc.FeatureHitForTest(0f, 50f, Element.Fire); basic.featureEffect = false; basic.basicAttack = true;
            basic.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Burn) };
            pc.FeatureOutgoingLateForTest(ref basic);
            var skill = basic; skill.basicAttack = false; skill.attackName = "Nova ognia"; pc.FeatureOutgoingLateForTest(ref skill);
            HitQuery.Apply(pc, e, basic, false);
            HitQuery.Apply(pc, e, basic, false);
            Assert.AreEqual(1, e.Status.BurnFamilyCount, "to samo źródło – jedna rodzina");
            HitQuery.Apply(pc, e, skill, false);
            Assert.AreEqual(2, e.Status.BurnFamilyCount, "drugie źródło – druga rodzina");
            yield return new WaitForSeconds(T.greatFireSeconds + 0.3f);
            Assert.LessOrEqual(e.Status.BurnFamilyCount, 1, "po oknie – najsilniejsza");
        }

        // ================================================================== Rębajło A/B

        [UnityTest]
        public IEnumerator MismatchedPair_OneSwingDeliversTwoPackets()
        {
            var pc = Hero("class_hu_dual");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.4f), hp: 100000f);
            yield return new WaitForSeconds(0.05f);
            int impacts = 0;
            System.Action<IHitReceiver, HitData, HitResult> count = (t, h, r) => { if (ReferenceEquals(t, e)) impacts++; };
            CombatEvents.Impact += count;
            Press(pc);
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(0.9f);
            CombatEvents.Impact -= count;
            Assert.AreEqual(2, impacts, "główna i druga ręka – dwa źródła");
            Assert.AreEqual(1f, pc.FeatureTelemetry.Get("para: wspólne uderzenia"));
        }

        [UnityTest]
        public IEnumerator UnbrokenDance_SwingContinuesThroughDodge()
        {
            var pc = Hero("class_hu_dual", "class_feature.dual.unbroken_dance");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.5f), hp: 100000f);
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            pc.Request(ActionType.LightAttack);
            yield return null;
            pc.MoveIntent = Vector3.right * 0.2f + Vector3.forward;
            pc.Request(ActionType.Dodge);
            Assert.AreEqual(ActionType.Dodge, pc.Actions.Current);
            float hp = e.Health.Current;
            yield return new WaitForSeconds(0.8f);
            Assert.AreEqual(1f, pc.FeatureTelemetry.Get("taniec: przeniesione ciosy"));
            Assert.Less(e.Health.Current, hp, "to samo cięcie trafia w uniku");
        }

        // ================================================================== Łotrzyk B – kradzież normalnego ataku

        [UnityTest]
        public IEnumerator StolenTrick_CopiesEnemyMelee_ExecutesOnceAsPlayer()
        {
            var pc = Hero("class_rogue", "class_feature.rogue.stolen_trick");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.8f), attacks: true);
            pc.DebugInvulnerable = true;
            pc.LockTarget = e;
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            var f = (StolenTrickFeature)pc.Feature;
            Assert.IsTrue(f.Observing);
            float t = 0f;
            while (t < 4f && f.Trick == StolenTrickFeature.TrickKind.None) { t += Time.deltaTime; yield return null; }
            Assert.AreEqual(StolenTrickFeature.TrickKind.Melee, f.Trick, "zapisany profil wręcz");
            float hp = e.Health.Current;
            Press(pc);
            Assert.AreEqual(StolenTrickFeature.TrickKind.None, f.Trick, "jedno wydanie");
            Assert.Less(e.Health.Current, hp);
        }
            // ================================================================== Mag wody A, Mag ziemi B, Mag bitewny B

        [UnityTest]
        public IEnumerator EnduringFrost_MarksRealTarget_FireReactsRepeatedly_NoFreeze()
        {
            var pc = Hero("class_hu_mage_water");
            var e = Enemy(pc, new Vector3(0, 0.05f, 4f), boss: true, hp: 100000f);
            pc.LockTarget = e;
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            Assert.IsTrue(e.Status.FrostMarked);
            for (int i = 0; i < 3; i++)
            {
                var h = pc.FeatureHitForTest(0f, 40f, Element.Fire); h.featureEffect = false;
                HitQuery.Apply(pc, e, h, false);
                yield return new WaitForSeconds(T.frostReactionInterval + 0.05f);
            }
            Assert.GreaterOrEqual(e.Status.FrostMarkReactions, 3, "szok termiczny za każdym razem");
            Assert.IsFalse(e.Status.Frozen, "bez zamrożenia bossa");
            Assert.IsFalse(e.Status.Stunned);
        }

        [UnityTest]
        public IEnumerator ShatteringShell_HeavyAttackLaunchesPlate()
        {
            var pc = Hero("class_hu_mage_earth", "class_feature.earth.shattering_shell");
            var e = Enemy(pc, new Vector3(0, 0.05f, 6f), hp: 100000f);
            pc.LockTarget = e;
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            var f = (ShatteringShellFeature)pc.Feature;
            Assert.AreEqual(T.shellMaxPlates, f.Plates);
            float hp = e.Health.Current;
            pc.Request(ActionType.HeavyAttack);
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual(T.shellMaxPlates - 1, f.Plates, "jedna płyta na mocny atak");
            Assert.Less(e.Health.Current, hp);
            Assert.AreEqual(1f, pc.FeatureTelemetry.Get("ziemia: wydane płyty"));
        }

        [UnityTest]
        public IEnumerator TwoBeats_WeaponAndSpellBothLand_IndependentCosts()
        {
            var pc = Hero("class_hu_battlemage", "class_feature.battlemage.two_beats");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.6f), hp: 100000f);
            pc.LockTarget = e;
            yield return new WaitForSeconds(0.05f);
            Press(pc);
            float st = pc.Stamina.Current, mana = pc.Mana.Current;
            pc.Request(ActionType.LightAttack);
            int slot = Slot(pc, "hu_nova_fire");
            pc.RequestSkill(slot);
            Assert.AreEqual(ActionType.LightAttack, pc.Actions.Current, "broń prowadzi ciało");
            Assert.Less(pc.Mana.Current, mana, "czar płaci maną");
            Assert.Less(pc.Stamina.Current, st, "broń płaci wytrzymałością");
            float hp = e.Health.Current;
            yield return new WaitForSeconds(1.2f);
            Assert.Less(e.Health.Current, hp);
            Assert.AreEqual(1f, pc.FeatureTelemetry.Get("dwa takty: czary w torze"));
            Assert.IsFalse(((TwoBeatsFeature)pc.Feature).LaneBusy, "czar wyzwolony");
        }
    }
}
