using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Buduje areny i postacie z prymitywów (placeholdery, brak zależności od assetów).</summary>
    public static class WorldBuilder
    {
        public static GameObject BuildArena(ArenaDefinition a, Transform parent)
        {
            var root = new GameObject("Arena_" + a.id);
            root.transform.SetParent(parent, false);

            float extent = a.size + 2f;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.localPosition = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(extent * 2f, 1f, extent * 2f);
            VisualFx.SetColor(floor, a.floorColor);
            floor.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            // Znaczniki podłogi, żeby było widać ruch.
            for (int i = 0; i < 12; i++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Util.DestroySafe(tile.GetComponent<Collider>());
                tile.name = "FloorMark";
                tile.transform.SetParent(root.transform, false);
                float ang = i * 30f * Mathf.Deg2Rad;
                tile.transform.localPosition = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * a.size * 0.55f + Vector3.up * 0.005f;
                tile.transform.localScale = new Vector3(1.2f, 0.01f, 1.2f);
                tile.transform.localRotation = Quaternion.Euler(0, i * 30f + 45f, 0);
                VisualFx.SetColor(tile, a.floorColor * 1.35f);
            }

            const float wallHeight = 4f;
            if (a.circular)
            {
                int segments = Mathf.Max(16, Mathf.RoundToInt(a.size * 2.2f));
                float segLen = 2f * Mathf.PI * a.size / segments + 0.4f;
                for (int i = 0; i < segments; i++)
                {
                    float ang = i * Mathf.PI * 2f / segments;
                    var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    w.name = "Wall";
                    w.transform.SetParent(root.transform, false);
                    w.transform.localPosition = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * (a.size + 0.5f) + Vector3.up * wallHeight * 0.5f;
                    w.transform.localRotation = Quaternion.Euler(0, -ang * Mathf.Rad2Deg + 90f, 0);
                    w.transform.localScale = new Vector3(segLen, wallHeight, 1f);
                    VisualFx.SetColor(w, a.wallColor * (i % 2 == 0 ? 1f : 0.9f));
                }
            }
            else
            {
                float h = a.size * 0.5f;
                Vector3[] pos = { new Vector3(0, 0, h + 0.5f), new Vector3(0, 0, -h - 0.5f), new Vector3(h + 0.5f, 0, 0), new Vector3(-h - 0.5f, 0, 0) };
                Vector3[] scl = { new Vector3(a.size + 2, wallHeight, 1), new Vector3(a.size + 2, wallHeight, 1), new Vector3(1, wallHeight, a.size + 2), new Vector3(1, wallHeight, a.size + 2) };
                for (int i = 0; i < 4; i++)
                {
                    var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    w.name = "Wall";
                    w.transform.SetParent(root.transform, false);
                    w.transform.localPosition = pos[i] + Vector3.up * wallHeight * 0.5f;
                    w.transform.localScale = scl[i];
                    VisualFx.SetColor(w, a.wallColor);
                }
            }

            foreach (var p in a.pillars)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = "Pillar";
                pillar.transform.SetParent(root.transform, false);
                pillar.transform.localPosition = p + Vector3.up * 2.5f;
                pillar.transform.localScale = new Vector3(1.3f, 5f, 1.3f);
                VisualFx.SetColor(pillar, a.wallColor * 1.1f);
            }
            return root;
        }

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
