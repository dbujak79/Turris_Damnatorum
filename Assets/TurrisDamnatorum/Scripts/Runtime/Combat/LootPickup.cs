using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Łup na ziemi (Hell Unleashed <c>LootDrop</c>): przedmiot, pieczęć albo dar bossa leży, aż bohater podejdzie na 1,6 m –
    /// wtedy zbiera go sam. Nie znika z czasem; to, co zostało na ziemi, zbiera koniec piętra.
    /// </summary>
    public class LootPickup : MonoBehaviour
    {
        public static readonly List<LootPickup> Active = new List<LootPickup>();
        public const float PickupRadius = 1.6f;

        Action onCollect;
        PlayerCombat hero;
        Transform gem;
        float phase;
        public string Label { get; private set; }
        public bool Collected { get; private set; }

        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        /// <summary>Kładzie łup na ziemi (lekko odsunięty, by kilka łupów się nie nakładało).</summary>
        public static LootPickup Spawn(PlayerCombat hero, Vector3 at, Color color, string label, Action onCollect, bool large = false)
        {
            if (!Application.isPlaying) { onCollect?.Invoke(); return null; }
            Vector3 jitter = UnityEngine.Random.insideUnitSphere * 0.6f; jitter.y = 0f;
            Vector3 pos = at + jitter;
            if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out var gh, 6f, ~0, QueryTriggerInteraction.Ignore)) pos = gh.point;
            var go = new GameObject("Loot " + label);
            go.transform.position = pos;
            var p = go.AddComponent<LootPickup>();
            p.onCollect = onCollect; p.hero = hero; p.Label = label; p.phase = UnityEngine.Random.value * 6f;
            p.gem = new GameObject("Gem").transform;
            p.gem.SetParent(go.transform, false);
            float s = large ? 0.42f : 0.28f;
            PartBuilder.Add(p.gem, ProcMesh.Box(), Surface.Metal, color, Vector3.zero, new Vector3(s, s, s), new Vector3(45f, 0f, 45f));
            PartBuilder.BakeAll(p.gem);
            var light = go.AddComponent<Light>();
            light.type = LightType.Point; light.color = color; light.range = large ? 4f : 2.5f; light.intensity = large ? 2.5f : 1.5f;
            light.shadows = LightShadows.None;
            HellFx.Burst(pos, HellFxElement.Soul, large ? 1.2f : 0.7f);
            return p;
        }

        void Update()
        {
            phase += Time.deltaTime;
            if (gem != null)
            {
                gem.localPosition = Vector3.up * (0.55f + Mathf.Sin(phase * 2.2f) * 0.1f);
                gem.localRotation = Quaternion.Euler(0f, phase * 90f, 0f);
            }
            if (hero == null || hero.IsDead) return;
            Vector3 d = hero.transform.position - transform.position; d.y = 0f;
            if (d.sqrMagnitude <= PickupRadius * PickupRadius) Collect();
        }

        /// <summary>Zbiera łup (raz) i usuwa go z ziemi.</summary>
        public void Collect()
        {
            if (Collected) return;
            Collected = true;
            onCollect?.Invoke();
            Sfx.Ui(SoundId.SoulGain);
            Destroy(gameObject);
        }

        /// <summary>Koniec piętra: wszystko, co leży, trafia do bohatera.</summary>
        public static void CollectAll()
        {
            foreach (var p in Active.ToArray()) if (p != null) p.Collect();
        }

        /// <summary>Sprzątanie świata bez zbierania (śmierć, wyjście do menu).</summary>
        public static void DiscardAll()
        {
            foreach (var p in Active.ToArray()) if (p != null) { p.Collected = true; Destroy(p.gameObject); }
            Active.Clear();
        }
    }
}
