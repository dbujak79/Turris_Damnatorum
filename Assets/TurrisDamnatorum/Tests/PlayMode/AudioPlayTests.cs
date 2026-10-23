using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Dźwięk w działającej grze: synteza w tle, zdarzenia walki i przebiegu, muzyka, pauza, głośność w profilu.</summary>
    public class AudioPlayTests
    {
        readonly List<Object> cleanup = new List<Object>();
        string profileFile;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
            profileFile = $"turris_audio_test_{System.Guid.NewGuid():N}.json";
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            string path = Path.Combine(Application.persistentDataPath, profileFile);
            if (File.Exists(path)) File.Delete(path);
            Cursor.lockState = CursorLockMode.None;
            AudioListener.pause = false;
        }

        GameRoot CreateGame(out DefaultContent.Bundle bundle)
        {
            bundle = TestContent.Create();
            var go = new GameObject("TestGame");
            go.SetActive(false);
            var root = go.AddComponent<GameRoot>();
            root.config = bundle.config;
            root.profileFileName = profileFile;
            go.SetActive(true);
            cleanup.Add(go);
            cleanup.Add(root.Player.gameObject);
            cleanup.Add(GameObject.Find("World"));
            cleanup.Add(root.CameraRig.gameObject);
            var sun = Object.FindAnyObjectByType<Light>();
            if (sun != null) cleanup.Add(sun.gameObject);
            return root;
        }

        static IEnumerator WaitFor(System.Func<bool> cond, float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!cond() && Time.realtimeSinceStartup < end) yield return null;
        }

        static int Count(GameRoot root, SoundId id) => root.Audio.PlayCounts[(int)id];

        [UnityTest]
        public IEnumerator Sounds_FollowCombatAndFlow_MusicSwitches_VolumeIsSaved()
        {
            var root = CreateGame(out var bundle);
            Assert.AreSame(root.Audio, AudioService.Instance);
            yield return WaitFor(() => AudioService.SfxReady, 60f);
            Assert.IsTrue(AudioService.SfxReady, "Efekty zsyntezowane w tle");
            Assert.IsNotNull(AudioService.Clip(SoundId.Parry), "Klip parowania istnieje");
            Assert.AreEqual(SoundInfo.For(SoundId.HitFlesh).variants, 4);
            yield return WaitFor(() => AudioService.MusicReady, 120f);
            Assert.IsTrue(AudioService.MusicReady, "Muzyka zsyntezowana w tle");
            yield return WaitFor(() => root.Audio.CurrentMusic == MusicId.Menu, 2f);
            Assert.AreEqual(MusicId.Menu, root.Audio.CurrentMusic, "Muzyka menu");
            Assert.AreEqual(MusicId.AmbSummit, root.Audio.CurrentAmbience, "Tło szczytu za menu");

            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("legacy_knight"), difficulty = bundle.config.difficulties[0] };
            Assert.IsTrue(root.TryStartRun(plan, out var reason), reason);
            Assert.AreEqual(1, Count(root, SoundId.FloorStart), "Dzwon na wejście na piętro");
            yield return null;

            // Cios gracza: świst w fazie aktywnej.
            int swings = Count(root, SoundId.SwingLight) + Count(root, SoundId.SwingHeavy);
            root.Player.Request(ActionType.LightAttack);
            yield return WaitFor(() => Count(root, SoundId.SwingLight) + Count(root, SoundId.SwingHeavy) > swings, 2f);
            Assert.Greater(Count(root, SoundId.SwingLight) + Count(root, SoundId.SwingHeavy), swings, "Świst broni");

            // Unik.
            yield return WaitFor(() => root.Player.Actions.IsIdle, 2f);
            int dodges = Count(root, SoundId.Dodge);
            root.Player.Request(ActionType.Dodge);
            yield return null;
            Assert.Greater(Count(root, SoundId.Dodge), dodges, "Szelest uniku");

            // Pauza wstrzymuje efekty gry (muzyka gra dalej, bo ignoruje pauzę słuchacza).
            root.TogglePause();
            yield return null;
            Assert.IsTrue(AudioListener.pause, "Pauza wstrzymuje efekty");
            root.TogglePause();
            yield return null;
            Assert.IsFalse(AudioListener.pause);

            // Muzyka walki, gdy żyją wrogowie.
            yield return WaitFor(() => root.Audio.CurrentMusic == MusicId.Combat, 2f);
            Assert.AreEqual(MusicId.Combat, root.Audio.CurrentMusic, "Muzyka walki na piętrze z wrogami");
            Assert.AreEqual(MusicId.AmbCourtyard, root.Audio.CurrentAmbience, "Tło dziedzińca");

            // Śmierć wrogów: dźwięk śmierci i wchłoniętej duszy.
            int deaths = Count(root, SoundId.DeathArmored) + Count(root, SoundId.DeathGhoul) + Count(root, SoundId.DeathBones);
            int souls = Count(root, SoundId.SoulGain);
            root.DebugKillAllEnemies();
            Assert.Greater(Count(root, SoundId.DeathArmored) + Count(root, SoundId.DeathGhoul) + Count(root, SoundId.DeathBones), deaths, "Dźwięk śmierci wroga");
            Assert.Greater(Count(root, SoundId.SoulGain), souls, "Dusza wchłonięta");

            // Głośność: zmiana trafia do słuchacza i do profilu na dysku.
            root.ChangeVolume(1, -0.3f);
            root.ChangeVolume(0, -0.2f);
            Assert.AreEqual(0.6f, AudioListener.volume, 0.001f, "Głośność ogólna → słuchacz");
            var reloaded = new MetaService(root.config, new FileProfileStorage(root.ProfilePath));
            Assert.AreEqual(0.4f, reloaded.Profile.musicVolume, 0.001f, "Głośność muzyki w profilu");
        }
    }
}
