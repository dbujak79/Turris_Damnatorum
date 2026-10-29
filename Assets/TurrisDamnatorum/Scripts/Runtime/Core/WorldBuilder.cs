using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Buduje areny i postacie z prymitywów (placeholdery, brak zależności od assetów).</summary>
    public static class WorldBuilder
    {
        public static GameObject BuildArena(ArenaDefinition a, Transform parent) =>
            a.gothicSet!=null ? GothicArenaBuilder.Build(a,parent) : ArenaBuilder.Build(a,parent);

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

        /// <summary>
        /// Miejsce portalu demonów: losowe, bliżej ścian niż środka areny, co najmniej <paramref name="minDistance"/> od bohatera
        /// i z dala od kolumn. Gdy arena jest za ciasna – najdalszy z kandydatów.
        /// </summary>
        public static Vector3 PortalPoint(ArenaDefinition a, Vector3 player, float minDistance, System.Random rng)
        {
            Vector3 best = Vector3.zero;
            float bestDist = -1f;
            for (int i = 0; i < 24; i++)
            {
                float u = (float)rng.NextDouble(), v = (float)rng.NextDouble();
                Vector3 p;
                if (a.circular)
                {
                    float ang = u * Mathf.PI * 2f;
                    float r = Mathf.Lerp(0.45f, 0.82f, Mathf.Sqrt(v)) * a.size;
                    p = new Vector3(Mathf.Cos(ang) * r, 0.1f, Mathf.Sin(ang) * r);
                }
                else
                {
                    float half = a.size * 0.5f - 1.6f;
                    p = new Vector3((u * 2f - 1f) * half, 0.1f, (v * 2f - 1f) * half);
                }
                bool blocked = false;
                foreach (var c in a.pillars)
                    if (new Vector2(p.x - c.x, p.z - c.z).sqrMagnitude < 2.2f * 2.2f) { blocked = true; break; }
                if (blocked) continue;
                float d = new Vector2(p.x - player.x, p.z - player.z).magnitude;
                if (d >= minDistance) return p;
                if (d > bestDist) { bestDist = d; best = p; }
            }
            return best;
        }

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

            BuildEnemyLook(go.AddComponent<CharacterVisual>(), def);
            return go.AddComponent<EnemyBrain>();
        }

        /// <summary>Sam wygląd wroga (bez AI i fizyki) – podgląd w bestiariuszu.</summary>
        public static CharacterVisual CreateEnemyVisual(EnemyDefinition def, Vector3 pos, Transform parent)
        {
            var go = new GameObject("Look_" + def.id);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var v = go.AddComponent<CharacterVisual>();
            BuildEnemyLook(v, def);
            return v;
        }

        static void BuildEnemyLook(CharacterVisual visual, EnemyDefinition def)
        {
            float s = def.scale;
            // Każdy egzemplarz nieco inny (odcienie, sylwetka, hełm) – ten sam archetyp i broń.
            var look = RigLook.Vary(RigLook.ForEnemy(def), Random.Range(int.MinValue, int.MaxValue));
            if (def.visual != null && def.visual.modelPrefab != null && def.visual.demon)
                visual.BuildModel(def.visual, s, new RigLook { weapon = WeaponModel.None, shield = ShieldModel.None, boss = def.isBoss });
            else if (def.visual != null && def.visual.modelPrefab != null)
                visual.BuildModel(def.visual, s, look.weapon, look.weaponColor, look.shield, look.shieldColor);
            else
                visual.BuildProcedural(look, s);
        }
    }
}
