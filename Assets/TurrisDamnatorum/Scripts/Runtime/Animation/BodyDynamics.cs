using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Ruch wtórny modelu Humanoid nakładany po klipach (tak jak w <see cref="ProceduralHumanoidAnimator"/>):
    ///  • reakcja na trafienie – tułów ustępuje w kierunku ciosu i wraca z małym przestrzałem, głowa dogania go z opóźnieniem
    ///    (smagnięcie), boczne trafienie skręca i przechyla tułów;
    ///  • pochylenie ruchu – do przodu przy ruszaniu, do tyłu przy hamowaniu, do środka zakrętu.
    /// Zgięcie jest rozłożone na kręgi (lędźwie, klatka, górna klatka) jak w ludzkim kręgosłupie, a jego zakres
    /// ograniczony, więc żadne połączenie nie wygina się nienaturalnie. Biodra i nogi zostają z klipu – stopy nie odjeżdżają.
    /// </summary>
    public class BodyDynamics
    {
        // Tułów: lekko niedotłumiony (odchylenie i powrót z małym przestrzałem); głowa – wolniejsza, dogania tułów.
        const float ReactK = 170f, ReactC = 15f, HeadK = 95f, HeadC = 9f;
        const float LeanK = 90f, LeanC = 13f;
        // Największe kąty (°) – granice ruchomości odcinka piersiowo-lędźwiowego przy reakcji, z zapasem względem klipu.
        const float MaxPitch = 30f, MaxYaw = 24f, MaxRoll = 14f, MaxHead = 22f;

        readonly Transform character;
        readonly Transform[] spine;
        readonly float[] share;
        readonly Transform neck, head;

        Vector3 react, reactVel, headReact, headVel;
        Vector3 lean, leanVel, prevLocalVel;
        float prevYaw;
        bool hasPrev;

        /// <summary>Bieżące wychylenie reakcji na trafienie (przestrzeń postaci) – dla testów.</summary>
        public Vector3 ReactOffset => react;
        /// <summary>Pochylenie ruchu: x – przechył w zakręcie, z – pochylenie przód/tył (dla testów).</summary>
        public Vector3 Lean => lean;

        public BodyDynamics(Animator animator, Transform character)
        {
            this.character = character;
            var s = animator.GetBoneTransform(HumanBodyBones.Spine);
            var c = animator.GetBoneTransform(HumanBodyBones.Chest);
            var u = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            // Lędźwie biorą mniej niż odcinek piersiowy (większa ruchomość skrętu i zgięcia wyżej).
            if (u != null) { spine = new[] { s, c, u }; share = new[] { 0.3f, 0.35f, 0.35f }; }
            else { spine = new[] { s, c }; share = new[] { 0.4f, 0.6f }; }
            neck = animator.GetBoneTransform(HumanBodyBones.Neck);
            head = animator.GetBoneTransform(HumanBodyBones.Head);
        }

        public void AddImpact(Vector3 localDir, float strength)
        {
            localDir.y = 0f;
            if (localDir.sqrMagnitude < 1e-4f) localDir = Vector3.back;
            // Prędkość początkowa tak dobrana, by szczyt wychylenia ≈ strength.
            reactVel += localDir.normalized * (strength * 24f);
        }

        public void Apply(in CharacterAnimState s, Vector3 worldVelocity, float dt)
        {
            if (character == null || dt <= 0f) return;
            Simulate(s, worldVelocity, dt);
            bool lying = s.action == AnimAction.Death;
            float leanK = lying || AnimResolve.IsDodge(s.action) ? 0f : 1f;

            float pitch = Mathf.Clamp(react.z * 26f + lean.z * 7f * leanK, -MaxPitch, MaxPitch);
            float yaw = Mathf.Clamp(-react.x * 18f, -MaxYaw, MaxYaw);
            float roll = Mathf.Clamp(-react.x * 10f - lean.x * 6f * leanK, -MaxRoll, MaxRoll);
            if (lying) { pitch *= 0.3f; yaw *= 0.3f; roll *= 0.3f; }
            if (Mathf.Abs(pitch) + Mathf.Abs(yaw) + Mathf.Abs(roll) > 0.01f)
            {
                Vector3 right = character.right, up = character.up, fwd = character.forward;
                for (int i = 0; i < spine.Length; i++)
                {
                    if (spine[i] == null) continue;
                    float f = share[i];
                    var q = Quaternion.AngleAxis(yaw * f, up) * Quaternion.AngleAxis(pitch * f, right) * Quaternion.AngleAxis(roll * f, fwd);
                    spine[i].rotation = q * spine[i].rotation;
                }
            }
            // Głowa: bezwładność – najpierw zostaje w miejscu (względem tułowia zgina się przeciwnie), potem smaga w stronę ciosu.
            Vector3 lag = headReact - react;
            float hp = Mathf.Clamp(lag.z * 20f + headReact.z * 8f, -MaxHead, MaxHead);
            float hr = Mathf.Clamp(-lag.x * 14f, -MaxHead * 0.6f, MaxHead * 0.6f);
            if (Mathf.Abs(hp) + Mathf.Abs(hr) > 0.01f)
            {
                var q = Quaternion.AngleAxis(hp, character.right) * Quaternion.AngleAxis(hr, character.forward);
                if (neck != null)
                {
                    var half = Quaternion.Slerp(Quaternion.identity, q, 0.4f);
                    neck.rotation = half * neck.rotation;
                    if (head != null) head.rotation = Quaternion.Slerp(Quaternion.identity, q, 0.6f) * head.rotation;
                }
                else if (head != null) head.rotation = q * head.rotation;
            }
        }

        void Simulate(in CharacterAnimState s, Vector3 worldVelocity, float dt)
        {
            Vector3 localVel = Quaternion.Inverse(character.rotation) * worldVelocity;
            localVel.y = 0f;
            float yaw = character.eulerAngles.y;
            Vector3 target = Vector3.zero;
            bool calm = s.action == AnimAction.None || s.action == AnimAction.Block || s.action == AnimAction.Cast || s.action == AnimAction.Drink;
            if (hasPrev && !AnimResolve.IsDodge(s.action) && s.action != AnimAction.Death)
            {
                Vector3 acc = (localVel - prevLocalVel) / dt;
                float yawRate = Mathf.DeltaAngle(prevYaw, yaw) / dt;
                float k = calm ? 1f : 0.35f;
                float speedN = Mathf.Clamp01(localVel.magnitude / 4.6f);
                target.z = Mathf.Clamp(acc.z * 0.03f, -1f, 1f) * k;
                target.x = Mathf.Clamp(yawRate / 400f * speedN, -1f, 1f) * k;
            }
            prevLocalVel = localVel;
            prevYaw = yaw;
            hasPrev = true;

            // Kilka mniejszych kroków – stabilnie także przy niskim FPS.
            int steps = Mathf.Clamp(Mathf.CeilToInt(dt / 0.008f), 1, 8);
            float h = dt / steps;
            for (int i = 0; i < steps; i++)
            {
                reactVel += (-ReactK * react - ReactC * reactVel) * h;
                react += reactVel * h;
                headVel += (HeadK * (react - headReact) - HeadC * headVel) * h;
                headReact += headVel * h;
                leanVel += (LeanK * (target - lean) - LeanC * leanVel) * h;
                lean += leanVel * h;
            }
            if (react.sqrMagnitude < 1e-6f && reactVel.sqrMagnitude < 1e-5f && headReact.sqrMagnitude < 1e-6f && headVel.sqrMagnitude < 1e-5f)
                react = reactVel = headReact = headVel = Vector3.zero;
        }
    }
}
