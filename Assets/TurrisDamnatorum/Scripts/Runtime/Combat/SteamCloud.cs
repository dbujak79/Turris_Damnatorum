using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Chmura pary (reakcja HU Steam: woda w płonący cel). Demon, który w niej zaczyna cios, chybia z szansą
    /// <see cref="BalanceConfig.steamMissChance"/> (<see cref="EnemyBrain"/>). Sama nie zadaje obrażeń.
    /// </summary>
    public class SteamCloud : MonoBehaviour
    {
        public static readonly List<SteamCloud> Active = new List<SteamCloud>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        float radius, life;
        GameObject visual;

        public float Radius => radius;

        public static SteamCloud Spawn(Vector3 center, float radius, float seconds)
        {
            var go = new GameObject("SteamCloud");
            go.transform.position = center;
            var c = go.AddComponent<SteamCloud>();
            c.radius = radius; c.life = seconds;
            c.visual = FxLibrary.ElementGround(center, radius, Element.Frost, new Color(0.88f, 0.92f, 0.95f, 0.6f), seconds, HellFxElement.Water);
            Sfx.Play(SoundId.FrostImpact, center, 0.7f, 1.3f);
            return c;
        }

        /// <summary>Punkt leży w którejś chmurze pary.</summary>
        public static bool Covers(Vector3 p)
        {
            foreach (var c in Active)
            {
                if (c == null) continue;
                Vector3 d = p - c.transform.position; d.y = 0f;
                if (d.sqrMagnitude <= c.radius * c.radius) return true;
            }
            return false;
        }

        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f) Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (visual != null) Destroy(visual);
        }
    }
}
