using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Decyzje botów wobec perków specjalizacyjnych (prawdziwa scena Main, bez serii): ocena w sklepie zależna od buildu,
    /// wartość działającego perka w ocenie wyposażenia (tarcza nie wypiera Pojedynkowicza), wybór ciosu i celu przy wyłomie.
    /// </summary>
    public class NativePerkBotTests
    {
        /// <summary>Bot wojownika z dostępem do chronionych decyzji (tylko test).</summary>
        class ProbeBot : WarriorBot
        {
            public float Score(BoonDefinition b) => PerkScore(b);
            public float Gear() => GearScore();
            public bool Helps(ItemInstance i) => GearWouldHelp(i);
        }

        [UnityTest]
        public IEnumerator Bot_ScoresNativePerksByBuild_KeepsDuelistOverShield()
        {
            GameRoot.ProfileFileOverride = $"turris_perk_bot_{System.Guid.NewGuid():N}.json";
            LogAssert.ignoreFailingMessages = true;
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            Assert.IsNotNull(root);
            var bot = root.Player.gameObject.AddComponent<ProbeBot>();
            bot.Init(root, PersonaProfile.For(BotPersona.SoulsSlasher), 11);
            var plan = new LoadoutPlan { difficulty = root.config.difficulties[0] };
            root.Meta.SelectClass(plan, root.config.classes.First(c => c.id == "class_knight"));
            Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);
            yield return null;
            var run = root.Run;
            var main = run.equipment.Get(EquipSlot.MainHand);
            Assume.That(main != null && NativePerkCatalog.OneHandedMelee(main.definition), "rycerz zaczyna z bronią jednoręczną");
            var shield = run.equipment.Get(EquipSlot.OffHand);
            if (shield != null) Assert.IsTrue(run.UnequipToInventory(EquipSlot.OffHand));
            root.Player.RefreshBuild();

            BoonDefinition P(string id) => NativePerkCatalog.Get(root.config, id);
            float duel = bot.Score(P(NativePerkCatalog.Duelist)), pack = bot.Score(P(NativePerkCatalog.Pack)), manif = bot.Score(P(NativePerkCatalog.Manifestation));
            TestContext.WriteLine($"ocena: pojedynkowicz={duel} sfora={pack} manifestacja={manif} wyłom={bot.Score(P(NativePerkCatalog.Breach))}");
            Assert.GreaterOrEqual(duel, 2.5f, "miecz i pusta ręka – bot kupi Pojedynkowicza");
            Assert.AreEqual(0f, pack, "bez przywołań – Sfora nic nie da");
            Assert.AreEqual(0f, manif, "bez strefy czaru – Manifestacja nic nie da");

            run.AddBoon(P(NativePerkCatalog.Duelist));
            root.Player.RefreshBuild();
            Assert.AreEqual(0f, bot.Score(P(NativePerkCatalog.Duelist)), "jeden poziom – drugi raz nie kupuje");
            if (shield != null)
            {
                float before = bot.Gear();
                bool helps = bot.Helps(shield);
                TestContext.WriteLine($"ocena buildu {before:0.0}; tarcza pomogłaby: {helps}");
                Assert.IsFalse(helps, "tarcza do pustej ręki uśpiłaby Pojedynkowicza – bot jej nie zakłada");
                Assert.IsNull(run.equipment.Get(EquipSlot.OffHand), "próba nie zostawia tarczy w ręce");
                Assert.AreEqual(before, bot.Gear(), 0.01f, "stan po próbie bez zmian");
            }
            Object.Destroy(bot);
            string path = System.IO.Path.Combine(Application.persistentDataPath, GameRoot.ProfileFileOverride);
            GameRoot.ProfileFileOverride = null;
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            // Scena Main z działającym podejściem nie może zostać na kolejne testy (wrogowie, fale i gracz wpływają na ich wyniki).
            var mainScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(SceneManager.CreateScene("PoTescieBota"));
            yield return SceneManager.UnloadSceneAsync(mainScene);
        }
    }
}
