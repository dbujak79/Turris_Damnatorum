using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Kamera trzecioosobowa zza pleców (bez Cinemachine – patrz README: decyzja projektowa).
    /// Swobodny obrót myszą/drążkiem, a przy namierzaniu kamera płynnie ustawia się tak, by gracz i cel byli w kadrze.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        public Transform target;
        public PlayerController player;
        public PlayerInputReader input;

        public float distance = 4.6f;
        public float height = 1.65f;
        public float shoulderOffset = 0.6f;
        public float minPitch = -30f, maxPitch = 65f;
        public float lockPitch = 17f;
        public float lockTurnSpeed = 8f;
        public float followSharpness = 18f;
        public float collisionRadius = 0.25f;

        float yaw, pitch = 12f;
        Vector3 pivotSmoothed;
        Camera cam;

        public Camera Camera => cam;
        public Vector3 FlatForward { get { var f = transform.forward; f.y = 0; return f.sqrMagnitude < 0.001f ? Vector3.forward : f.normalized; } }
        public Vector3 FlatRight { get { var r = transform.right; r.y = 0; return r.sqrMagnitude < 0.001f ? Vector3.right : r.normalized; } }

        void Awake() { cam = GetComponent<Camera>(); }

        public void SnapBehindTarget()
        {
            if (target == null) return;
            yaw = target.eulerAngles.y;
            pitch = 12f;
            pivotSmoothed = target.position + Vector3.up * height;
            LateUpdate();
        }

        public void RecenterBehind()
        {
            if (target != null) yaw = target.eulerAngles.y;
        }

        void LateUpdate()
        {
            if (target == null) return;
            float dt = Time.unscaledDeltaTime;
            Vector3 pivot = target.position + Vector3.up * height;
            pivotSmoothed = Vector3.Lerp(pivotSmoothed, pivot, 1f - Mathf.Exp(-followSharpness * dt));

            var lockTarget = player != null ? player.LockTarget : null;
            bool inputOn = player == null || player.InputEnabled;
            if (lockTarget != null)
            {
                Vector3 to = lockTarget.transform.position - target.position; to.y = 0;
                if (to.sqrMagnitude > 0.01f)
                {
                    float targetYaw = Quaternion.LookRotation(to).eulerAngles.y;
                    yaw = Mathf.LerpAngle(yaw, targetYaw, 1f - Mathf.Exp(-lockTurnSpeed * dt));
                }
                float d = to.magnitude;
                float desiredPitch = Mathf.Lerp(lockPitch + 8f, lockPitch, Mathf.Clamp01(d / 8f));
                pitch = Mathf.Lerp(pitch, desiredPitch, 1f - Mathf.Exp(-4f * dt));
            }
            else if (input != null && inputOn && Time.timeScale > 0)
            {
                Vector2 look = input.LookDelta;
                yaw += look.x;
                pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);
            }

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
            Vector3 offset = rot * new Vector3(shoulderOffset, 0, -distance);
            Vector3 desired = pivotSmoothed + offset;

            // Kolizja kamery z geometrią areny (ignoruje postacie).
            Vector3 dir = desired - pivotSmoothed;
            float len = dir.magnitude;
            if (Physics.SphereCast(pivotSmoothed, collisionRadius, dir / len, out var hit, len, ~0, QueryTriggerInteraction.Ignore)
                && hit.collider.GetComponentInParent<IHitReceiver>() == null)
                desired = pivotSmoothed + dir / len * Mathf.Max(0.5f, hit.distance);

            transform.position = desired;
            Vector3 lookAt = pivotSmoothed;
            if (lockTarget != null) lookAt = Vector3.Lerp(pivotSmoothed, lockTarget.AimPoint, 0.35f);
            // Patrzymy równolegle do osi pivot→kamera przesuniętej o ramię, by postać była lekko z boku kadru.
            lookAt += rot * new Vector3(shoulderOffset, 0, 0);
            transform.rotation = Quaternion.LookRotation(lookAt - transform.position, Vector3.up);
        }
    }
}
