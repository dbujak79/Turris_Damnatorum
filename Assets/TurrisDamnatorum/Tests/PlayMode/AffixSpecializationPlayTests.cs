using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Affiksy specjalizacyjne w działającym silniku (rozdz. 10.2): rzeczywisty cios z dodanym ogniem przeciw odpornemu
    /// i podatnemu demonowi, postawa ciężkiej broni, czas przygotowania czaru i migawka obrażeń sług.
    /// </summary>
    public class AffixSpecializationPlayTests
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

        PlayerCombat Hero(string classId)
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
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(classId), difficulty = bundle.config.difficulties[0] };
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(RunFactory.Create(plan, bundle.config, 7), bundle.config);
            p.GetComponent<PlayerVisuals>().Refresh();
            return pc;
        }

        EnemyBrain Dummy(PlayerCombat pc, float fireMult = 1f, float maxPoise = 100000f)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = "Manekin"; def.maxHealth = 5000; def.maxPoise = maxPoise; def.scale = 1f; def.moveSpeed = 0f; def.strafeChance = 0f;
            def.fireMultiplier = fireMult;
            def.guardChance = 0f; def.evadeChance = 0f; // bez losowej gardy i uników – pomiar deterministyczny
            cleanup.Add(def);
            var pos = new Vector3(0, 0.05f, 1.6f);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(-pos.normalized);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            return e;
        }

        void Equip(PlayerCombat pc, string id, EquipSlot slot)
        {
            var it = new ItemInstance(bundle.Get<ItemDefinition>(id));
            pc.Run.inventory.Add(it);
            pc.Run.EquipFromInventory(it, slot);
            pc.RefreshBuild();
        }

        ItemInstance Ring(PlayerCombat pc, EquipSlot slot, params StatModifier[] mods)
        {
            var it = new ItemInstance(bundle.Get<ItemDefinition>("hu_item_ring_iron")) { rarity = ItemRarity.Rare, affixVersion = HellEconomy.AffixVersion };
            it.affixes.AddRange(mods);
            pc.Run.equipment.Equip(it, slot, null);
            pc.RefreshBuild();
            return it;
        }

        /// <summary>Jeden pierwszy lekki cios w manekina; zwraca utracone zdrowie i postawę.</summary>
        readonly List<(HitData hit, HitResult res)> impacts = new List<(HitData, HitResult)>();
        void OnImpact(IHitReceiver t, HitData h, HitResult r) { if (h.attacker is PlayerCombat) impacts.Add((h, r)); }

        IEnumerator Swing(PlayerCombat pc, EnemyBrain e, float[] result)
        {
            // Ten sam punkt startu za każdym razem (wypad ciosu przesuwa bohatera).
            pc.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            yield return new WaitForSeconds(0.1f);
            impacts.Clear();
            CombatEvents.Impact += OnImpact;
            float h0 = e.Health.Current, p0 = e.Poise.Current;
            pc.Request(ActionType.LightAttack);
            float minH = h0, minP = p0, end = Time.time + 1.3f;
            while (Time.time < end) // minimum w oknie – regeneracja postawy nie zaciera pomiaru
            {
                yield return null;
                minH = Mathf.Min(minH, e.Health.Current); minP = Mathf.Min(minP, e.Poise.Current);
            }
            CombatEvents.Impact -= OnImpact;
            result[0] = h0 - minH;
            result[1] = p0 - minP;
            Object.Destroy(e.gameObject);
            yield return new WaitForSeconds(0.8f); // koniec serii – następny cios znów pierwszy
        }

        [UnityTest]
        public IEnumerator P01_PhysicalSwordWithAddedFire_ResistantAndWeakDemon_OneHitEach()
        {
            var pc = Hero("legacy_knight");
            Equip(pc, "hu_sword", EquipSlot.MainHand);
            Ring(pc, EquipSlot.Ring1, new StatModifier(StatType.AddedWeaponFire, 50f));
            var b = pc.Build;
            // Każdy cios osobno: przechwycone trafienie (po PrepareOutgoing) i wynik na tym samym demonie. Porównania między
            // różnymi ciosami nie są wiarygodne – istniejąca ścieżka ciosu daje kilka–kilkanaście % rozrzutu (pomiar kontrolny w raporcie).
            foreach (float mult in new[] { 0.5f, 1.5f })
            {
                impacts.Clear();
                yield return SwingCapture(pc, mult);
                Assert.AreEqual(1, impacts.Count, "jeden cios = jedno zdarzenie trafienia");
                var (h, r) = impacts[0];
                float common = h.physical / b.WeaponDamage(b.weapon.light); // wspólne mnożniki tego ciosu
                float fire = h.MagicOf(Element.Fire);
                Assert.Greater(fire, 0f, "rzeczywisty składnik ognia");
                Assert.AreEqual(b.AddedElement(b.weapon.light, Element.Fire) * (1f + b.stats[StatType.FireDamage] / 100f) * common, fire, 0.05f,
                    "ogień = B × 50% × premia ognia × wspólne mnożniki; premia fizyczna go nie zwiększa");
                Assert.AreEqual(h.physical + fire * mult + (h.magic - fire), r.healthDamage, 0.05f, "składnik ognia według odporności demona (×" + mult + ")");
                Assert.AreEqual(HitOutcome.Hit, r.outcome);
            }
        }

        IEnumerator SwingCapture(PlayerCombat pc, float fireMult)
        {
            pc.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var e = Dummy(pc, fireMult);
            yield return new WaitForSeconds(0.1f);
            CombatEvents.Impact += OnImpact;
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(1.3f);
            CombatEvents.Impact -= OnImpact;
            Object.Destroy(e.gameObject);
            yield return new WaitForSeconds(0.8f);
        }

        [UnityTest]
        public IEnumerator P06_HeavyWeaponPoiseBonus_RealSwing_SwordUnaffected()
        {
            var pc = Hero("legacy_knight");
            Equip(pc, "hu_greataxe", EquipSlot.MainHand);
            float PoiseRatio() { var h = impacts[0].hit; return h.poiseDamage / pc.Build.weapon.light.poiseDamage; }
            impacts.Clear(); yield return SwingCapture(pc, 1f);
            Assert.AreEqual(1, impacts.Count);
            float r0 = PoiseRatio();
            Assert.Greater(r0, 0f, "ciężki cios odbiera postawę");
            var ring = Ring(pc, EquipSlot.Ring1, new StatModifier(StatType.HeavyPoiseDamage, 25f));
            impacts.Clear(); yield return SwingCapture(pc, 1f);
            Assert.AreEqual(1, impacts.Count, "jedno trafienie");
            Assert.AreEqual(r0 * 1.25f, PoiseRatio(), 0.01f, "postawa ×1,25 (mnożnik ruchu zachowany)");

            Equip(pc, "hu_sword", EquipSlot.MainHand);
            pc.Run.equipment.Unequip(EquipSlot.Ring1); pc.RefreshBuild();
            impacts.Clear(); yield return SwingCapture(pc, 1f);
            float s0 = PoiseRatio();
            pc.Run.equipment.Equip(ring, EquipSlot.Ring1, null); pc.RefreshBuild();
            impacts.Clear(); yield return SwingCapture(pc, 1f);
            Assert.AreEqual(s0, PoiseRatio(), 0.01f, "miecz – bez premii postawy");
        }

        [UnityTest]
        public IEnumerator P07_CastSpeed_ShortensStartupOnly_ReleaseAtNewTime()
        {
            var pc = Hero("class_mage");
            int slot = -1;
            for (int i = 0; i < pc.Build.skills.Length; i++)
                if (pc.Build.skills[i] != null && pc.Build.skills[i].Def.IsSpell && pc.Build.skills[i].Def.castTime > 0.1f && pc.Build.skills[i].equipmentMet) { slot = i; break; }
            if (slot < 0) { Assert.Inconclusive("brak czaru z przygotowaniem w slotach maga"); yield break; }
            var def = pc.Build.skills[slot].Def;
            pc.Mana.Restore(9999f);

            float t0, startup0;
            yield return new WaitForSeconds(0.2f);
            pc.RequestSkill(slot);
            yield return null;
            Assert.AreEqual(ActionType.Cast, pc.Actions.Current, "czar ruszył");
            t0 = Time.time;
            while (pc.Actions.Current == ActionType.Cast && pc.Actions.Phase == ActionPhase.Startup && Time.time - t0 < 3f) yield return null;
            startup0 = Time.time - t0;
            yield return new WaitForSeconds(Mathf.Max(def.cooldown, 0.5f) + 1.5f);

            Ring(pc, EquipSlot.Ring1, new StatModifier(StatType.CastSpeed, 50f));
            pc.Mana.Restore(9999f);
            Assert.AreEqual(pc.ScaleStartup(def.castTime) / 1.5f, pc.EffectiveCastTime(def), 1e-4f);
            pc.RequestSkill(slot);
            yield return null;
            Assert.AreEqual(ActionType.Cast, pc.Actions.Current, "czar ruszył drugi raz");
            float t1 = Time.time;
            while (pc.Actions.Current == ActionType.Cast && pc.Actions.Phase == ActionPhase.Startup && Time.time - t1 < 3f) yield return null;
            float startup1 = Time.time - t1;
            Assert.AreEqual(startup0 / 1.5f, startup1, 0.05f, "wyzwolenie po krótszym przygotowaniu");
        }

        [UnityTest]
        public IEnumerator P10_MinionDamage_SnapshotAtSummon_ChangeAppliesToNextSummon()
        {
            var pc = Hero("class_knight");
            var ring = Ring(pc, EquipSlot.Ring1, new StatModifier(StatType.MinionDamage, 30f));
            var hit = new HitData { physical = 10f, magic = 4f, element = Element.Fire };
            var first = Servant.Spawn(pc, ServantKind.Hound, 30f, 1f, () => hit, Color.green);
            Assert.AreEqual(1.3f, first.MinionMultiplier, 1e-4f);
            pc.Run.equipment.Unequip(EquipSlot.Ring1); pc.RefreshBuild();
            var second = Servant.Spawn(pc, ServantKind.Hound, 30f, 1f, () => hit, Color.green);
            Assert.AreEqual(1f, second.MinionMultiplier, 1e-4f, "nowe przyzwanie bez premii");
            Assert.AreEqual(1.3f, first.MinionMultiplier, 1e-4f, "istniejący sługa zachowuje migawkę");
            // Składniki chimery niosą tę samą funkcję trafienia – bez ponownego mnożenia.
            var parts = first.ToParts();
            Assert.AreEqual(10f, first.SnapshotHit().physical, 1e-3f, "Wcielenie: atak gracza bez premii sług");
            Assert.AreEqual(13f, parts[0].makeHit().physical, 1e-3f);
            Assert.AreEqual(5.2f, parts[0].makeHit().magic, 1e-3f);
            pc.Run.equipment.Equip(ring, EquipSlot.Ring1, null); pc.RefreshBuild();
            Assert.AreEqual(13f, parts[0].makeHit().physical, 1e-3f, "założenie premii nie zmienia istniejącej migawki");
            Assert.AreEqual(10f, hit.physical, "oryginalne trafienie bez zmian");
            yield return null;
        }
    }
}
