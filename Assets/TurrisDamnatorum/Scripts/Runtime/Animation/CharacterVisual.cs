using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Wygląd i animacja postaci. Źródłem stanu jest logika walki (<see cref="ICharacterAnimSource"/>),
    /// a sterownikiem – proceduralny humanoid albo model FBX z klipami (<see cref="CharacterVisualDefinition"/>).
    /// Kod walki nie wie, który wariant jest używany.
    /// </summary>
    public class CharacterVisual : MonoBehaviour
    {
        public HumanoidRig Rig { get; private set; }
        public GameObject Model { get; private set; }
        public ICharacterAnimationDriver Driver { get; private set; }
        public bool UsesModel => Model != null;

        ICharacterAnimSource source;
        Vector3 lastPos;
        Vector3 velocity;
        bool hasLastPos;

        // Tryb FBX
        readonly TintSet modelTint = new TintSet();
        readonly TintSet gearTint = new TintSet();
        Transform weaponSocket, shieldSocket;
        GameObject weaponObj, shieldObj;
        WeaponModel modelWeapon;

        public void Init(ICharacterAnimSource src)
        {
            source = src;
            var trail = GetComponent<SwingTrail>();
            if (trail == null) trail = gameObject.AddComponent<SwingTrail>();
            trail.Init(this, src);
        }

        /// <summary>Prawa dłoń (dla efektów rzucania czarów).</summary>
        public Transform RightHand
        {
            get
            {
                if (Rig != null) return Rig[Bone.HandR];
                var anim = Model != null ? Model.GetComponentInChildren<Animator>() : null;
                return anim != null && anim.isHuman ? anim.GetBoneTransform(HumanBodyBones.RightHand) : null;
            }
        }

        public void SetTrailColor(Color c)
        {
            var trail = GetComponent<SwingTrail>();
            if (trail != null) trail.color = c;
        }

        public void BuildProcedural(RigLook look, float scale)
        {
            Clear();
            Rig = HumanoidRig.Create(transform, look, scale);
            Driver = new ProceduralHumanoidAnimator(Rig, transform);
        }

        public void BuildModel(CharacterVisualDefinition def, float scale, WeaponModel weapon, Color weaponColor, ShieldModel shield, Color shieldColor)
        {
            Clear();
            Model = Instantiate(def.modelPrefab, transform);
            Model.name = "Model";
            Model.transform.localPosition = def.positionOffset;
            Model.transform.localRotation = Quaternion.Euler(def.rotationOffset);
            Model.transform.localScale = Vector3.one * def.scale * scale;
            foreach (var c in Model.GetComponentsInChildren<Collider>()) Util.DestroySafe(c);
            modelTint.Collect(Model.GetComponentsInChildren<Renderer>());

            var animator = Model.GetComponentInChildren<Animator>();
            if (animator == null) animator = Model.AddComponent<Animator>();
            animator.runtimeAnimatorController = null;
            Driver = new ClipAnimationDriver(animator, def, transform);

            if (def.attachGear)
            {
                weaponSocket = MakeSocket(animator, def.weaponBone, def.weaponBonePath, def.weaponPosition, def.weaponRotation, "WeaponSocket");
                shieldSocket = MakeSocket(animator, def.shieldBone, def.shieldBonePath, def.shieldPosition, def.shieldRotation, "ShieldSocket");
                SetGear(weapon, weaponColor, shield, shieldColor);
            }
        }

        Transform MakeSocket(Animator animator, HumanBodyBones bone, string path, Vector3 pos, Vector3 rot, string name)
        {
            Transform parent = null;
            if (!string.IsNullOrEmpty(path)) parent = animator.transform.Find(path);
            if (parent == null && animator.isHuman) parent = animator.GetBoneTransform(bone);
            if (parent == null) parent = animator.transform;
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = pos;
            t.localRotation = Quaternion.Euler(rot);
            return t;
        }

        public void SetGear(WeaponModel weapon, Color weaponColor, ShieldModel shield, Color shieldColor)
        {
            if (Rig != null)
            {
                Rig.SetWeapon(weapon, weaponColor);
                Rig.SetShield(shield, shieldColor);
                (Driver as ProceduralHumanoidAnimator)?.Refresh();
                return;
            }
            if (weaponSocket == null) return;
            if (weaponObj != null) Util.DestroyNow(weaponObj);
            if (shieldObj != null) Util.DestroyNow(shieldObj);
            modelWeapon = weapon;
            weaponObj = GearBuilder.Weapon(weaponSocket, weapon == WeaponModel.Claws ? WeaponModel.None : weapon, weaponColor);
            shieldObj = GearBuilder.Shield(shieldSocket, shield, shieldColor);
            gearTint.Clear();
            gearTint.Collect(weaponObj.GetComponentsInChildren<Renderer>());
        }

        void Clear()
        {
            Driver?.Dispose();
            Driver = null;
            if (Rig != null) Util.DestroyNow(Rig.gameObject);
            if (Model != null) Util.DestroyNow(Model);
            Rig = null; Model = null;
            modelTint.Clear(); gearTint.Clear();
            weaponSocket = shieldSocket = null;
        }

        void OnDestroy() => Driver?.Dispose();

        void LateUpdate() => Tick(Time.deltaTime);

        /// <summary>Krok animacji (publiczny na potrzeby testów).</summary>
        public void Tick(float dt)
        {
            if (Driver == null || source == null || dt <= 0f) return;
            Vector3 pos = transform.position;
            if (hasLastPos)
            {
                Vector3 v = (pos - lastPos) / dt;
                v.y = 0;
                velocity = Vector3.Lerp(velocity, v, 1f - Mathf.Exp(-12f * dt));
            }
            lastPos = pos;
            hasLastPos = true;
            Driver.Tick(source.GetAnimState(), velocity, dt);
        }

        // ------------------------------------------------------------------ Kolory

        public void SetTint(Color c, float amount)
        {
            if (Rig != null) { Rig.SetTint(c, amount); return; }
            modelTint.SetTint(c, amount);
        }

        public void SetWeaponGlow(Color c, float intensity)
        {
            if (Rig != null) { Rig.SetWeaponGlow(c, intensity); return; }
            // Model bez broni (np. pazury w animacji) – delikatna poświata całej postaci.
            if (gearTint.Count == 0) { if (intensity > 0) SetTint(c, intensity * 0.35f); return; }
            gearTint.SetGlow(c, intensity);
        }

        public Vector3 WeaponTip => Rig != null ? Rig.WeaponTip : (weaponSocket != null ? weaponSocket.position + weaponSocket.forward * 0.9f : transform.position + Vector3.up);
    }
}
