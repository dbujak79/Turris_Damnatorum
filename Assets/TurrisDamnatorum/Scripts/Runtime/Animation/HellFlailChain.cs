using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Port FlailChain z HU: lina Verlet w świecie, ciężka kula, wyłącznie obroty istniejących kości.</summary>
    public sealed class HellFlailChain
    {
        const int Iterations = 12;
        const float Gravity = 9.8f, Damping = 0.1f, HeadMass = 12f, MaxStep = 1f/120;
        readonly Transform model, head;
        readonly Transform[] bones;
        readonly Quaternion[] rest;
        readonly Vector3[] reach, points, previous;
        readonly float[] lengths;
        readonly float totalLength;
        Vector3 lastAnchor;
        float previousStep;
        bool started;

        HellFlailChain(Transform model, Transform[] bones, Transform head)
        {
            this.model = model; this.bones = bones; this.head = head;
            rest = new Quaternion[bones.Length]; reach = new Vector3[bones.Length]; lengths = new float[bones.Length];
            points = new Vector3[bones.Length+1]; previous = new Vector3[points.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                var next = i+1 < bones.Length ? bones[i+1] : head;
                rest[i] = bones[i].localRotation;
                reach[i] = next.localPosition;
                lengths[i] = Vector3.Distance(bones[i].position,next.position) / Scale;
                totalLength += lengths[i];
            }
            // Kula wychyla się poza obrys pozy eksportowej. Poszerzamy wyłącznie granice renderera instancji broni.
            foreach (var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Vector3 s = skin.transform.lossyScale;
                float radius = (Length+0.2f*Scale)/Mathf.Max(0.0001f,Mathf.Min(Mathf.Abs(s.x),Mathf.Abs(s.y),Mathf.Abs(s.z)));
                Vector3 centre = skin.transform.InverseTransformPoint(Anchor);
                Bounds bounds = skin.localBounds;
                bounds.Encapsulate(centre-Vector3.one*radius); bounds.Encapsulate(centre+Vector3.one*radius);
                skin.localBounds = bounds;
            }
        }
        float Scale => Mathf.Max(0.0001f,Mathf.Abs(model.lossyScale.x));
        public Vector3 Anchor => bones[0].position;
        public Vector3 HeadPosition => head.position;
        public Vector3 HeadTip
        {
            get
            {
                Vector3 direction = head.position - bones[bones.Length-1].position;
                return head.position + direction.normalized * (0.15f*Scale);
            }
        }
        public float Length => totalLength*Scale;

        public static HellFlailChain Find(GameObject model)
        {
            if (model == null) return null;
            var first = Util.FindDeep(model.transform,"chain0");
            var head = Util.FindDeep(model.transform,"head");
            if (first == null || head == null) return null;
            var chain = new List<Transform>();
            for (int i = 0; ; i++)
            {
                var bone = Util.FindDeep(model.transform,"chain"+i);
                if (bone == null) break;
                if (i > 0 && bone.parent != chain[i-1]) return null;
                chain.Add(bone);
            }
            if (chain.Count == 0 || head.parent != chain[chain.Count-1]) return null;
            // Kości poruszamy ręcznie poza Animatorem. Unity musi przeliczać siatkę i jej obrys także po wychyleniu
            // kuli poza obrys spoczynkowy, inaczej korbacz potrafi wizualnie zastygnąć przy brzegu kadru.
            foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>()) renderer.updateWhenOffscreen = true;
            return new HellFlailChain(model.transform,chain.ToArray(),head);
        }

        void Reset(Vector3 anchor)
        {
            for (int i = 0; i < points.Length; i++)
                points[i] = previous[i] = i < bones.Length ? bones[i].position : head.position;
            lastAnchor = anchor; previousStep = 0; started = true;
        }

        /// <summary>Wywoływane po pozie dłoni i broni; dt=0 zatrzymuje symulację, dłuższa klatka jest dzielona.</summary>
        public void Update(float dt, float ground = 0f)
        {
            if (model == null || bones[0] == null || head == null) return;
            Vector3 anchor = Anchor;
            if (!started || Vector3.Distance(anchor,lastAnchor) > Length*4f+0.5f) Reset(anchor);
            dt = Mathf.Clamp(dt,0f,0.05f);
            if (dt > 0)
            {
                int steps = Mathf.CeilToInt(dt/MaxStep);
                float step = dt/steps;
                for (int s = 1; s <= steps; s++) Simulate(Vector3.Lerp(lastAnchor,anchor,s/(float)steps),step,ground);
            }
            lastAnchor = anchor;
            Pose();
        }

        void Simulate(Vector3 anchor, float dt, float ground)
        {
            float keep = Mathf.Pow(Damping,dt);
            float scale = Scale;
            float ratio = previousStep > 0 ? dt/previousStep : 1f;
            Vector3 fall = Vector3.down*(Gravity*dt*dt);
            points[0] = previous[0] = anchor;
            for (int i = 1; i < points.Length; i++)
            {
                Vector3 velocity = (points[i]-previous[i])*(keep*ratio);
                previous[i] = points[i]; points[i] += velocity+fall;
                points[i].y = Mathf.Max(points[i].y,ground+0.04f);
            }
            previousStep = dt;
            for (int k = 0; k < Iterations; k++)
                for (int i = 0; i < lengths.Length; i++)
                {
                    Vector3 a = points[i], b = points[i+1], d = b-a;
                    float distance = d.magnitude;
                    if (distance < 0.00001f) continue;
                    float error = (distance-lengths[i]*scale)/distance;
                    if (i == 0) { points[1] -= d*error; continue; }
                    float share = 1f/(1f+(i+1 == lengths.Length ? 1f/HeadMass : 1f));
                    points[i] += d*(error*share);
                    points[i+1] -= d*(error*(1f-share));
                }
        }

        void Pose()
        {
            for (int i = 0; i < bones.Length; i++)
            {
                Quaternion worldRest = bones[i].parent.rotation*rest[i];
                Vector3 current = worldRest*reach[i], wanted = points[i+1]-points[i];
                if (current.sqrMagnitude < 1e-10f || wanted.sqrMagnitude < 1e-10f) return;
                bones[i].rotation = Quaternion.FromToRotation(current,wanted)*worldRest;
            }
        }
    }
}
