using System.Collections.Generic;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>Wspólne przygotowanie danych testowych na bazie domyślnej treści.</summary>
    public class Fixture
    {
        public readonly DefaultContent.Bundle bundle;
        public GameConfig Cfg => bundle.config;
        readonly List<GameObject> spawned = new List<GameObject>();

        public Fixture() { bundle = DefaultContent.Create(); }

        public T Get<T>(string id) where T : ContentDefinition => bundle.Get<T>(id);

        public LoadoutPlan Plan(string classId, int tier = 0)
        {
            return new LoadoutPlan { classDef = Get<ClassDefinition>(classId), difficulty = Cfg.difficulties[tier] };
        }

        public RunState NewRun(string classId, int tier = 0) => RunFactory.Create(Plan(classId, tier), Cfg, 1234);

        public PlayerCombat NewCombatant(RunState run)
        {
            var go = new GameObject("TestPlayer");
            spawned.Add(go);
            var pc = go.AddComponent<PlayerCombat>();
            pc.Init(run, Cfg);
            return pc;
        }

        /// <summary>Trafienie od przodu (atakujący 2 m przed postacią na +Z).</summary>
        public static HitData FrontHit(float physical = 50, float magic = 0, float guardLoad = 20, bool blockable = true, bool parryable = true, bool dodgeable = true)
        {
            return new HitData
            {
                physical = physical, magic = magic, guardLoad = guardLoad, poiseDamage = 10,
                blockable = blockable, parryable = parryable, dodgeable = dodgeable,
                sourcePosition = new Vector3(0, 0, 2),
            };
        }

        public static HitData BackHit(float physical = 50)
        {
            var h = FrontHit(physical);
            h.sourcePosition = new Vector3(0, 0, -2);
            return h;
        }

        public void Cleanup()
        {
            foreach (var go in spawned) if (go != null) Object.DestroyImmediate(go);
            foreach (var so in bundle.all) if (so != null) Object.DestroyImmediate(so);
        }

        /// <summary>Symuluje upływ czasu w krokach 1/60 s.</summary>
        public static void Advance(PlayerCombat pc, float seconds)
        {
            const float dt = 1f / 60f;
            for (float t = 0; t < seconds - 1e-4f; t += dt) pc.Tick(dt);
        }
    }
}
