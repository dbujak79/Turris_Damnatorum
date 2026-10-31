using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Stanowisko strzeleckie (Łucznik B): widoczny punkt wylotu podstawowych strzałów gracza. Nie celuje, nie strzela samo,
    /// nie przejmuje wrogów i nie ma kolizji – to wyłącznie geometryczne źródło pocisku (właścicielem zostaje gracz).
    /// </summary>
    public class FiringPost : MonoBehaviour
    {
        float life;
        Transform muzzle;
        public float Remaining => life;
        public bool Alive => this != null && life > 0f;
        /// <summary>Punkt wylotu strzały (wysokość łuku).</summary>
        public Vector3 Muzzle => muzzle != null ? muzzle.position : transform.position + Vector3.up * 1.3f;
        public int Shots { get; set; }

        public static FiringPost Spawn(Vector3 at, Quaternion rot, float seconds)
        {
            var go = new GameObject("FiringPost");
            go.transform.SetPositionAndRotation(HuWorld.Ground(at), rot);
            var p = go.AddComponent<FiringPost>();
            p.life = seconds;
            // Pal z poprzeczką i kołczanem – czytelne miejsce wystrzału, bez kolizji.
            PartBuilder.Add(go.transform, ProcMesh.Box(), Surface.Wood, new Color(0.42f, 0.3f, 0.18f), new Vector3(0f, 0.65f, 0f), new Vector3(0.12f, 1.3f, 0.12f));
            PartBuilder.Add(go.transform, ProcMesh.Box(), Surface.Wood, new Color(0.38f, 0.27f, 0.16f), new Vector3(0f, 1.2f, 0f), new Vector3(0.6f, 0.08f, 0.08f));
            PartBuilder.Add(go.transform, ProcMesh.Box(), Surface.Cloth, new Color(0.6f, 0.15f, 0.12f), new Vector3(0.18f, 0.9f, 0f), new Vector3(0.14f, 0.4f, 0.14f));
            p.muzzle = new GameObject("Muzzle").transform;
            p.muzzle.SetParent(go.transform, false);
            p.muzzle.localPosition = new Vector3(0f, 1.3f, 0.25f);
            foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c);
            if (Application.isPlaying)
            {
                FxLibrary.Dust(go.transform.position, 0.6f);
                FxLibrary.Decal(go.transform.position, FxMaterials.Ring, new Color(0.9f, 0.75f, 0.4f, 0.8f), 1.2f, 1.2f, seconds, 15f);
            }
            return p;
        }

        void Update() { life -= Time.deltaTime; if (life <= 0f) End(); }

        public void Flash(Vector3 dir)
        {
            if (muzzle != null && dir.sqrMagnitude > 1e-4f) muzzle.rotation = Quaternion.LookRotation(dir);
            if (Application.isPlaying) FxLibrary.Flash(Muzzle, new Color(1f, 0.9f, 0.6f), 0.6f);
        }

        public void End()
        {
            life = 0f;
            Util.DestroySafe(gameObject);
        }
    }
}
