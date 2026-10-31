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
        readonly bool anatomicalGrip;
        readonly Vector3 leftGripPosition;
        readonly Quaternion leftGripRotation, rightGripRotation;
        readonly float span;
        readonly Transform nock;
        readonly Vector3 nockRest;
        float gripWeight, guardWeight;
        /// <summary>Odstęp dłoni na trzonku topora i młota (m, od gniazda prawej dłoni do lewej). Trzonek topora kończy się
        /// 33,6 cm za gniazdem – przy 34 cm lewa pięść obejmowała sam koniec i w połowie powietrze.</summary>
        public const float HeavyGripSpan = 0.28f;
        /// <summary>Rozstaw dłoni w niskiej gardzie topora: lewa przy końcu trzonka, prawa wyżej (broń przesunięta w dłoni
        /// o <see cref="GreatSwingPose.GuardSlide"/>), jak na wzorcu użytkownika z Elden Ring.</summary>
        public const float GuardGripSpan = 0.56f;
        /// <summary>Udział niskiej gardy (0–1) – ustawia CharacterVisual z GreatSwingPose.</summary>
        public float GuardGrip { get; set; }
        readonly Quaternion restL = Quaternion.identity;
        public float GripError { get; private set; }
        public float BowDraw { get; private set; }
        public HellEquipmentPose(Animator animator, Transform root, RigLook look, WeaponAnimSet set, GameObject gear,
            bool anatomicalGrip = false, Transform leftSocket = null)
        {
            this.root = root; weapon = look.weapon; wand = look.weaponItem != null && look.weaponItem.id.Contains("wand");
            twoHanded = AnimResolve.GripFor(weapon) == GripStyle.TwoHand || (weapon == WeaponModel.Staff && look.weaponItem != null && look.weaponItem.twoHanded);
            span = set != null ? set.gripSpan : 0.2f;
            // Topór i młot trzyma się szeroko (dłonie rozsunięte na długim trzonku), nie pięścią przy pięści jak rękojeść miecza.
            if (AnimResolve.IsHeavyTwoHand(weapon)) span = Mathf.Max(span, HeavyGripSpan);
            if (animator.isHuman)
            {
                upperL = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm); lowerL = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm); handL = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                upperR = animator.GetBoneTransform(HumanBodyBones.RightUpperArm); lowerR = animator.GetBoneTransform(HumanBodyBones.RightLowerArm); handR = animator.GetBoneTransform(HumanBodyBones.RightHand);
            }
            if (lowerL != null && handL != null) restL = BindRelative(animator, lowerL, handL);
            if (weapon == WeaponModel.Bow && gear != null) nock = Util.FindDeep(gear.transform, "nock");
            if (nock != null) nockRest = nock.localPosition;
            this.anatomicalGrip = anatomicalGrip && handL != null && handR != null && leftSocket != null && gear != null;
            if (this.anatomicalGrip)
            {
                leftGripPosition = handL.InverseTransformPoint(leftSocket.position);
                leftGripRotation = Quaternion.Inverse(handL.rotation) * leftSocket.rotation;
                rightGripRotation = Quaternion.Inverse(handR.rotation) * gear.transform.parent.rotation;
            }
        }
        public void Apply(in CharacterAnimState state, Transform socket, float dt)
        {
            if (handL == null || handR == null || socket == null) return;
            bool free = state.action == AnimAction.Death || state.action == AnimAction.Dodge || state.action == AnimAction.Flinch || state.action == AnimAction.Cast || state.action == AnimAction.Drink;
            gripWeight = Mathf.MoveTowards(gripWeight, twoHanded && !free ? 1f : 0f, dt * 10);
            if (anatomicalGrip && !free)
            {
                var direction = socket.rotation;
                handR.rotation = direction * Quaternion.Inverse(rightGripRotation);
                socket.rotation = direction;
            }
            if (gripWeight > 0)
            {
                float spanNow = Mathf.Lerp(span, GuardGripSpan, GuardGrip);
                var target = socket.position - socket.up * spanNow;
                if (anatomicalGrip) handL.rotation = Quaternion.Slerp(handL.rotation,
                    socket.rotation * Quaternion.Inverse(leftGripRotation), gripWeight);
                // Garda: dłoń może obrócić się wokół trzonka (chwyt i tak go obejmuje) – wybieramy ułożenie najbliższe
                // naturalnemu nadgarstkowi; wcześniej lewy nadgarstek był zgięty o 137° i skręcony o ~155° (siatka dłoni się zapadała).
                if (GuardGrip > 0.002f) NaturalWrist(lowerL, handL, restL, socket.up, GuardGrip);
                // Retarget klipu na krótsze ramiona może zostawić drugi chwyt poza zasięgiem.
                // Cofamy wtedy także dłoń prowadzącą: broń pozostaje między obiema dłońmi, kości nie są rozciągane.
                for (int i = 0; i < 2; i++)
                {
                    float reach = Vector3.Distance(upperL.position, lowerL.position) + Vector3.Distance(lowerL.position, handL.position) - 0.005f;
                    var wristTarget = anatomicalGrip ? target - handL.TransformVector(leftGripPosition) : target;
                    var delta = wristTarget - upperL.position;
                    if (delta.magnitude <= reach) break;
                    var correction = -delta.normalized * (delta.magnitude - reach + 0.008f);
                    Solve(upperR, lowerR, handR, handR.position + correction, root.right - root.up, gripWeight);
                    target = socket.position - socket.up * spanNow;
                }
                Solve(upperL, lowerL, handL, anatomicalGrip ? target - handL.TransformVector(leftGripPosition) : target,
                    LeftPole(), gripWeight);
                if (GuardGrip > 0.002f)
                {
                    // po IK przedramię ma nowy kierunek – jeszcze raz dobór obrotu wokół trzonka i domknięcie chwytu
                    NaturalWrist(lowerL, handL, restL, socket.up, GuardGrip);
                    Solve(upperL, lowerL, handL, anatomicalGrip ? target - handL.TransformVector(leftGripPosition) : target,
                        LeftPole(), gripWeight);
                }
                GripError = Vector3.Distance(anatomicalGrip ? handL.TransformPoint(leftGripPosition) : handL.position, target);
            }
            guardWeight = Mathf.MoveTowards(guardWeight, state.hasShield && (state.action == AnimAction.Block || state.action == AnimAction.Parry) ? 1 : 0, dt * 12);
            if (guardWeight > 0) Solve(upperL, lowerL, handL, root.TransformPoint(new Vector3(-0.28f, 1.15f, 0.42f)), -root.right - root.up, guardWeight * 0.8f);
            if (state.action == AnimAction.None && (weapon == WeaponModel.Dagger || wand))
            {
                var target = root.TransformPoint(wand ? new Vector3(0.35f, 1.02f, 0.35f) : new Vector3(0.32f, 0.94f, 0.23f));
                Solve(upperR, lowerR, handR, target, root.right - root.up, 0.8f);
                var blade = root.TransformDirection(wand ? new Vector3(0.1f, 0.65f, 0.75f) : new Vector3(0.25f, -0.35f, 0.9f)).normalized;
                var desired = Quaternion.LookRotation(Vector3.ProjectOnPlane(root.up, blade).normalized, blade);
                if (anatomicalGrip) handR.rotation = desired * Quaternion.Inverse(rightGripRotation);
                socket.rotation = desired;
            }
            if (nock != null)
            {
                bool drawing = (state.action == AnimAction.Attack || state.action == AnimAction.Cast) && state.attack == AttackAnim.BowDraw;
                BowDraw = drawing && state.phase == ActionPhase.Startup ? Mathf.SmoothStep(0, 1, state.phaseProgress) : 0;
                nock.localPosition = nockRest;
                if (BowDraw > 0) { nock.position -= root.forward * (0.32f * BowDraw); Solve(upperR, lowerR, handR, nock.position, root.right + root.up * 0.2f, BowDraw); }
            }
        }
        /// <summary>Kierunek łokcia lewej ręki: w bok i w dół; w niskiej gardzie w dół i do tyłu (ręce opuszczone wzdłuż ciała, jak na wzorcu).</summary>
        Vector3 LeftPole() => root.TransformDirection(Vector3.Slerp(new Vector3(-0.9f, -0.8f, -0.2f), new Vector3(-0.35f, -0.75f, -0.55f), GuardGrip));

        /// <summary>Obrót dłoni względem przedramienia w pozie wiązania (z bindposes siatki; bez siatki – bieżąca poza).</summary>
        public static Quaternion BindRelative(Animator animator, Transform parent, Transform child)
        {
            foreach (var smr in animator.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                int ip = System.Array.IndexOf(smr.bones, parent), ic = System.Array.IndexOf(smr.bones, child);
                var bp = smr.sharedMesh != null ? smr.sharedMesh.bindposes : null;
                if (ip < 0 || ic < 0 || bp == null || ip >= bp.Length || ic >= bp.Length) continue;
                return Quaternion.Inverse(bp[ip].inverse.rotation) * bp[ic].inverse.rotation;
            }
            return Quaternion.Inverse(parent.rotation) * child.rotation;
        }

        /// <summary>Kąt składowej „skrętu” obrotu q wokół osi (°, −180…180).</summary>
        public static float TwistAngle(Quaternion q, Vector3 axis)
        {
            axis.Normalize();
            var p = Vector3.Project(new Vector3(q.x, q.y, q.z), axis);
            var t = new Quaternion(p.x, p.y, p.z, q.w);
            float n = Mathf.Sqrt(t.x * t.x + t.y * t.y + t.z * t.z + t.w * t.w);
            if (n < 1e-6f) return 0f;
            t = new Quaternion(t.x / n, t.y / n, t.z / n, t.w / n);
            t.ToAngleAxis(out float a, out var ax);
            if (Vector3.Dot(ax, axis) < 0) a = -a;
            if (a > 180f) a -= 360f; else if (a < -180f) a += 360f;
            return a;
        }

        /// <summary>
        /// Chwyt trzonka z naturalnym nadgarstkiem: dłoń obraca się wokół trzonka (oś <paramref name="shaft"/>) do ułożenia
        /// najbliższego pozie wiązania względem przedramienia, a resztę skrętu (do ±80°) przejmuje przedramię (pronacja/supinacja).
        /// Kierunek trzonka w dłoni się nie zmienia; obrót broni wokół trzonka ustawia osobno CharacterVisual.
        /// </summary>
        public static void NaturalWrist(Transform fore, Transform hand, Quaternion rest, Vector3 shaft, float weight)
        {
            if (fore == null || hand == null || weight <= 0f) return;
            var neutral = fore.rotation * rest;
            float roll = TwistAngle(neutral * Quaternion.Inverse(hand.rotation), shaft);
            var h = Quaternion.AngleAxis(roll * weight, shaft) * hand.rotation;
            Vector3 axis = fore.rotation * Vector3.up; // kość przedramienia: +Y wzdłuż kości
            float twist = TwistAngle(h * Quaternion.Inverse(neutral), axis);
            float share = Mathf.Clamp(twist * 0.7f, -80f, 80f) * weight;
            fore.rotation = Quaternion.AngleAxis(share, axis) * fore.rotation;
            hand.rotation = h;
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
