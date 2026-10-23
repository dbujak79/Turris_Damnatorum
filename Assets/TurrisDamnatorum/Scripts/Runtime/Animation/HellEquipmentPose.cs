using UnityEngine;

namespace Turris
{
    /// <summary>Warstwa po Playables: chwyt HU na Humanoid, bez modyfikacji importowanych klipów.</summary>
    public sealed class HellEquipmentPose
    {
        readonly Transform root, upperL, lowerL, handL, upperR, lowerR, handR;
        readonly WeaponModel weapon;
        readonly bool wand;
        readonly bool twoHanded;
        readonly float span;
        readonly Transform nock;
        readonly Vector3 nockRest;
        float gripWeight, guardWeight;
        public float GripError { get; private set; }
        public float BowDraw { get; private set; }
        public HellEquipmentPose(Animator animator, Transform root, RigLook look, WeaponAnimSet set, GameObject gear)
        {
            this.root = root; weapon = look.weapon; wand = look.weaponItem != null && look.weaponItem.id.Contains("wand");
            twoHanded = AnimResolve.GripFor(weapon) == GripStyle.TwoHand || (weapon == WeaponModel.Staff && look.weaponItem != null && look.weaponItem.twoHanded);
            span = set != null ? set.gripSpan : 0.2f;
            if (animator.isHuman)
            {
                upperL = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm); lowerL = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm); handL = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                upperR = animator.GetBoneTransform(HumanBodyBones.RightUpperArm); lowerR = animator.GetBoneTransform(HumanBodyBones.RightLowerArm); handR = animator.GetBoneTransform(HumanBodyBones.RightHand);
            }
            if (weapon == WeaponModel.Bow && gear != null) nock = Util.FindDeep(gear.transform, "nock");
            if (nock != null) nockRest = nock.localPosition;
        }
        public void Apply(in CharacterAnimState state, Transform socket, float dt)
        {
            if (handL == null || handR == null || socket == null) return;
            bool free = state.action == AnimAction.Death || state.action == AnimAction.Dodge || state.action == AnimAction.Flinch || state.action == AnimAction.Cast || state.action == AnimAction.Drink;
            gripWeight = Mathf.MoveTowards(gripWeight, twoHanded && !free ? 1f : 0f, dt * 10);
            if (gripWeight > 0)
            {
                var target = socket.position - socket.up * span;
                // Retarget klipu na krótsze ramiona może zostawić drugi chwyt poza zasięgiem.
                // Cofamy wtedy także dłoń prowadzącą: broń pozostaje między obiema dłońmi, kości nie są rozciągane.
                for (int i = 0; i < 2; i++)
                {
                    float reach = Vector3.Distance(upperL.position, lowerL.position) + Vector3.Distance(lowerL.position, handL.position) - 0.005f;
                    var delta = target - upperL.position;
                    if (delta.magnitude <= reach) break;
                    var correction = -delta.normalized * (delta.magnitude - reach + 0.008f);
                    Solve(upperR, lowerR, handR, handR.position + correction, root.right - root.up, gripWeight);
                    target = socket.position - socket.up * span;
                }
                Solve(upperL, lowerL, handL, target, root.TransformDirection(new Vector3(-0.9f, -0.8f, -0.2f)), gripWeight);
                GripError = Vector3.Distance(handL.position, target);
            }
            guardWeight = Mathf.MoveTowards(guardWeight, state.hasShield && (state.action == AnimAction.Block || state.action == AnimAction.Parry) ? 1 : 0, dt * 12);
            if (guardWeight > 0) Solve(upperL, lowerL, handL, root.TransformPoint(new Vector3(-0.28f, 1.15f, 0.42f)), -root.right - root.up, guardWeight * 0.8f);
            if (state.action == AnimAction.None && (weapon == WeaponModel.Dagger || wand))
            {
                var target = root.TransformPoint(wand ? new Vector3(0.35f, 1.02f, 0.35f) : new Vector3(0.32f, 0.94f, 0.23f));
                Solve(upperR, lowerR, handR, target, root.right - root.up, 0.8f);
                var blade = root.TransformDirection(wand ? new Vector3(0.1f, 0.65f, 0.75f) : new Vector3(0.25f, -0.35f, 0.9f)).normalized;
                socket.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(root.up, blade).normalized, blade);
            }
            if (nock != null)
            {
                bool drawing = (state.action == AnimAction.Attack || state.action == AnimAction.Cast) && state.attack == AttackAnim.BowDraw;
                BowDraw = drawing && state.phase == ActionPhase.Startup ? Mathf.SmoothStep(0, 1, state.phaseProgress) : 0;
                nock.localPosition = nockRest;
                if (BowDraw > 0) { nock.position -= root.forward * (0.32f * BowDraw); Solve(upperR, lowerR, handR, nock.position, root.right + root.up * 0.2f, BowDraw); }
            }
        }
        /// <summary>Dwukościowe IK HU: długości kończyn są stałe, cel ograniczony do osiągalnego zasięgu.</summary>
        public static void Solve(Transform upper, Transform lower, Transform hand, Vector3 target, Vector3 pole, float weight)
        {
            if (upper == null || lower == null || hand == null || weight <= 0) return;
            var shoulder = upper.position;
            float a = Vector3.Distance(shoulder, lower.position), b = Vector3.Distance(lower.position, hand.position);
            var delta = target - shoulder;
            if (a < 0.001f || b < 0.001f || delta.sqrMagnitude < 1e-6f) return;
            float d = Mathf.Clamp(delta.magnitude, Mathf.Abs(a - b) + 0.001f, a + b - 0.001f);
            var n = delta.normalized; var side = Vector3.ProjectOnPlane(pole, n);
            if (side.sqrMagnitude < 1e-6f) side = Vector3.Cross(n, Vector3.forward);
            if (side.sqrMagnitude < 1e-6f) side = Vector3.Cross(n, Vector3.up);
            float cosine = Mathf.Clamp((a * a + d * d - b * b) / (2 * a * d), -1, 1);
            var elbow = shoulder + n * (a * cosine) + side.normalized * (a * Mathf.Sqrt(1 - cosine * cosine));
            var wristRotation = hand.rotation;
            upper.rotation = Quaternion.Slerp(upper.rotation, Quaternion.FromToRotation(lower.position - shoulder, elbow - shoulder) * upper.rotation, weight);
            lower.rotation = Quaternion.Slerp(lower.rotation, Quaternion.FromToRotation(hand.position - lower.position, shoulder + n * d - lower.position) * lower.rotation, weight);
            hand.rotation = wristRotation;
        }
    }
    /// <summary>Lustrzany zamach wolną ręką w przestrzeni mięśni Humanoid. Nogi i krok pozostają w klipie.</summary>
    public sealed class HellOffhandPose : System.IDisposable
    {
        readonly HumanPoseHandler handler;
        readonly int[] partner;
        HumanPose pose;
        float[] original;
        public HellOffhandPose(Animator animator)
        {
            if (!animator.isHuman || animator.avatar == null) return;
            handler = new HumanPoseHandler(animator.avatar, animator.transform);
            var names = HumanTrait.MuscleName; partner = new int[names.Length]; original = new float[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                string n = names[i]; bool arm = n.Contains("Arm") || n.Contains("Shoulder") || n.Contains("Forearm") || n.Contains("Hand") || n.Contains("Thumb") || n.Contains("Index") || n.Contains("Middle") || n.Contains("Ring") || n.Contains("Little");
                string other = n.StartsWith("Left") ? "Right" + n.Substring(4) : n.StartsWith("Right") ? "Left" + n.Substring(5) : "";
                partner[i] = arm && other.Length > 0 ? System.Array.IndexOf(names, other) : -1;
            }
        }
        public void Apply(bool offhand)
        {
            if (!offhand || handler == null) return;
            handler.GetHumanPose(ref pose); System.Array.Copy(pose.muscles, original, original.Length);
            for (int i = 0; i < partner.Length; i++) if (partner[i] >= 0) pose.muscles[i] = original[partner[i]];
            handler.SetHumanPose(ref pose);
        }
        public void Dispose() => handler?.Dispose();
    }
}
