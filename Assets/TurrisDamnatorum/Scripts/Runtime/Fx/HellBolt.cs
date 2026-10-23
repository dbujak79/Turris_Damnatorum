using UnityEngine;

namespace Turris
{
    /// <summary>Jądro czaru i smuga HU; odczepiona smuga kończy życie w świecie, bez skoku do nowego pocisku.</summary>
    public sealed class HellBolt : MonoBehaviour
    {
        HellParticles trail;
        GameObject body;
        Vector3 previous;
        bool released;
        Camera view;

        public void Init(HellParticles particles, Element element, Color color, float radius)
        {
            trail = particles; previous = transform.position; view = Camera.main;
            body = new GameObject("HU_BoltBody"); body.transform.SetParent(transform, false);
            if (element == Element.Earth)
            {
                var rock = PartBuilder.Add(body.transform, ProcMesh.Sphere(), Surface.Stone, new Color(0.24f,0.2f,0.16f), Vector3.zero, Vector3.one * radius * 1.45f);
                rock.GetComponent<Renderer>().sharedMaterial = HellVfxMaterials.Rock;
            }
            else
            {
                string sprite = element == Element.Fire ? "fireball_sheet" : element == Element.Lightning ? "plasma_orb" : "glow";
                FxLibrary.LoopEmit(body.transform, Vector3.zero, new FxLibrary.Emit
                {
                    sprite = sprite, flipbook = element == Element.Fire ? 8 : element == Element.Lightning ? 4 : 0,
                    rate = 8, life = new Vector2(0.18f, 0.22f), size = Vector2.one * radius * 3.6f,
                    colorA = element == Element.Fire || element == Element.Lightning ? Color.white : color,
                    shape = ParticleSystemShapeType.Sphere, radius = 0.01f, localSpace = true, sizeGrow = 1.1f
                });
            }
        }

        void LateUpdate()
        {
            if (released) return;
            if (view == null) view = Camera.main;
            if (Time.deltaTime > 0 && trail != null) trail.SetStream((transform.position - previous) / Time.deltaTime, view);
            previous = transform.position;
            if (body != null) body.transform.Rotate(110f * Time.deltaTime, 75f * Time.deltaTime, 0);
        }

        public void Release()
        {
            if (released) return;
            released = true; transform.SetParent(null, true);
            if (body != null) Destroy(body);
            if (trail != null) trail.StopAndRelease();
            Destroy(gameObject, 0.05f);
        }
    }
}
