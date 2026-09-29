using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Buduje areny i postacie z prymitywów (placeholdery, brak zależności od assetów).</summary>
    public static class WorldBuilder
    {
        public static GameObject BuildArena(ArenaDefinition a, Transform parent) => ArenaBuilder.Build(a, parent);

        public static float SpawnRadius(ArenaDefinition a) => a.circular ? a.size * 0.6f : a.size * 0.35f;

        public static List<Vector3> EnemySpawns(ArenaDefinition a, int count)
        {
            var list = new List<Vector3>();
            float r = SpawnRadius(a);
            for (int i = 0; i < count; i++)
            {
                float spread = count == 1 ? 0 : Mathf.Lerp(-35f, 35f, i / (float)(count - 1));
                list.Add(Quaternion.Euler(0, spread, 0) * new Vector3(0, 0.1f, r));
            }
            return list;
        }

        public static Vector3 PlayerSpawn(ArenaDefinition a) => new Vector3(0, 0.1f, -SpawnRadius(a));

        public static GameObject CreatePlayer()
        {
            var go = new GameObject("Player");
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.4f; cc.center = new Vector3(0, 0.9f, 0);
            cc.stepOffset = 0.3f; cc.skinWidth = 0.05f;
            cc.minMoveDistance = 0f; // przy wysokim FPS ruch na klatkę bywa < 1 mm

            var visual = go.AddComponent<CharacterVisual>();
            visual.BuildProcedural(new RigLook(), 1f);

            go.AddComponent<PlayerInputReader>();
            var combat = go.AddComponent<PlayerCombat>();
            visual.Init(combat);
            go.AddComponent<PlayerController>();
            go.AddComponent<PlayerVisuals>();
            return go;
        }

        public static EnemyBrain CreateEnemy(EnemyDefinition def, Vector3 pos, Transform parent)
        {
            float s = def.scale;
            var go = new GameObject("Enemy_" + def.id);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f * s; cc.radius = 0.42f * s; cc.center = new Vector3(0, 0.9f * s, 0);
            cc.stepOffset = 0.3f;
            cc.minMoveDistance = 0f;

            var visual = go.AddComponent<CharacterVisual>();
            var look = RigLook.ForEnemy(def);
            if (def.visual != null && def.visual.modelPrefab != null)
                visual.BuildModel(def.visual, s, look.weapon, look.weaponColor, look.shield, look.shieldColor);
            else
                visual.BuildProcedural(look, s);

            return go.AddComponent<EnemyBrain>();
        }
    }
}
