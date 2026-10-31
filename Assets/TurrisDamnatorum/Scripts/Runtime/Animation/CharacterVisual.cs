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
        Transform weaponSocket, shieldSocket, shieldGrip, leftHand, leftElbow, rightHand, rightElbow;
        GameObject weaponObj, shieldObj;
        Vector3 weaponRestPos; Quaternion weaponRestRot = Quaternion.identity;
        WeaponModel modelWeapon;
        CharacterVisualDefinition modelDef;
        HellBodySurface bodySurface;
        HellEquipmentPose equipmentPose;
        HellFlailChain mainChain, offhandChain;
        public float GripError => equipmentPose != null ? equipmentPose.GripError : 0;
        public float BowDraw => equipmentPose != null ? equipmentPose.BowDraw : 0;

        public void Init(ICharacterAnimSource src)
        {
            source = src;
            var trail = GetComponent<SwingTrail>();
            if (trail == null) trail = gameObject.AddComponent<SwingTrail>();
            trail.Init(this, src);
            trail.enabled = !noTrail;
        }

        bool noTrail;

        /// <summary>Ile trwa upadek po śmierci (s), zanim ciało zacznie zapadać się pod podłogę.</summary>
        public float DeathFallTime => Driver is DemonAnimationDriver d ? Mathf.Max(1.6f, d.DeathLength) : 1.6f;

        /// <summary>Prawa dłoń (dla efektów rzucania czarów).</summary>
        public Transform RightHand
        {
            get
            {
                if (Rig != null) return Rig[Bone.HandR];
                var anim = Model != null ? Model.GetComponentInChildren<Animator>() : null;
                if (anim != null && anim.isHuman) return anim.GetBoneTransform(HumanBodyBones.RightHand);
                return Model != null ? Util.FindDeep(Model.transform, "hand.R") : null; // demony Hell Unleashed (Generic)
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

        public void BuildModel(CharacterVisualDefinition def, float scale, WeaponModel weapon, Color weaponColor, ShieldModel shield, Color shieldColor) =>
            BuildModel(def, scale, new RigLook { weapon = weapon, weaponColor = weaponColor, shield = shield, shieldColor = shieldColor });

        /// <summary>Model FBX z animacjami; broń i przedmiot drugiej ręki z modeli 3D przedmiotów (albo proceduralne).</summary>
        public void BuildModel(CharacterVisualDefinition def, float scale, RigLook look)
        {
            Clear();
            modelDef = def;
            Model = Instantiate(def.modelPrefab, transform);
            Model.name = "Model";
            Model.transform.localPosition = def.positionOffset;
            Model.transform.localRotation = Quaternion.Euler(def.rotationOffset);
            Model.transform.localScale = Vector3.one * def.scale * scale;
            foreach (var c in Model.GetComponentsInChildren<Collider>()) Util.DestroySafe(c);
            bodySurface = new HellBodySurface(Model);
            modelTint.Collect(Model.GetComponentsInChildren<Renderer>());

            var animator = Model.GetComponentInChildren<Animator>();
            if (animator == null) animator = Model.AddComponent<Animator>();
            animator.runtimeAnimatorController = null;
            FootMesh.Shorten(Model, animator, def.footLength);
            if (def.demon)
            {
                // Demon z Hell Unleashed: własne klipy, pazury i czary w animacji – bez broni w dłoni i smugi ostrza.
                ModelSet = null;
                modelGrip = GripStyle.OneHand;
                Driver = new DemonAnimationDriver(animator, def, Model.transform, look.boss);
                HellDemonFx.Attach(Model.transform, def.name);
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var trail = GetComponent<SwingTrail>();
                if (trail != null) trail.enabled = false;
                noTrail = true;
                return;
            }
            ModelSet = def.SetFor(look.weapon);
            bool greatWeapon = AnimResolve.IsHeavyTwoHand(look.weapon);
            modelGrip = AnimResolve.GripFor(look.weapon);
            uprightWeight = 1f;
            Driver = new ClipAnimationDriver(animator, def, transform, ModelSet, greatWeapon);
            // Graf ręczny (Tick) nakłada pozę od razu, także gdy postaci nie widać (testy, postać poza kadrem).
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            if (def.attachGear)
            {
                // Gniazda ustawiane w pozie spoczynkowej (T-pose, zaraz po utworzeniu – animacja jeszcze nie ruszyła):
                // ostrze (+Y broni) do przodu, góra broni (+Z) do góry – jak trzyma miecz nagranie Mixamo.
                // Łuk trzyma lewa ręka, cięciwę naciąga prawa.
                bool bow = look.weapon == WeaponModel.Bow;
                var bladeForward = Quaternion.LookRotation(Vector3.up, Vector3.forward);
                // Łuk w T-pose: ramiona pionowo (+Y modelu w górę), brzusiec do przodu, cięciwa (−Z) do łucznika.
                var bowGrip = Quaternion.identity;
                var right = MakeSlot(animator, def.rightSlot, HumanBodyBones.RightHand, bladeForward, "WeaponSocket", def.weaponPosition, def.weaponRotation);
                var left = MakeSlot(animator, def.leftSlot, HumanBodyBones.LeftHand, bow ? bowGrip : bladeForward, "OffhandSocket", def.shieldPosition, def.shieldRotation);
                weaponSocket = bow ? left : right;
                shieldSocket = bow ? right : left;
                weaponSocketRest = weaponSocket.localRotation;
                if (Driver is ClipAnimationDriver clipDriver && clipDriver.GreatSwing != null) clipDriver.GreatSwing.Weapon = weaponSocket;
                if (animator.isHuman)
                {
                    rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    rightElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                    if (rightElbow == null) rightHand = null;
                }
                // Tarcza na przedramieniu (osobny węzeł – grymuar i sztylet zostają w dłoni).
                shieldGrip = null;
                if (!bow && ModelSet != null && ModelSet.hasShieldGrip)
                {
                    shieldGrip = new GameObject("ShieldGrip").transform;
                    shieldGrip.SetParent(left, false);
                    shieldGrip.localPosition = ModelSet.shieldGripPosition;
                    shieldGrip.localRotation = Quaternion.Euler(ModelSet.shieldGripEuler);
                }
                SetModelGear(look);
            }
        }

        /// <summary>Zestaw animacji broni użyty przez model FBX (null = klipy samego modelu).</summary>
        public WeaponAnimSet ModelSet { get; private set; }
        Transform MakeSlot(Animator animator, string slotName, HumanBodyBones bone, Quaternion restInCharacter, string name, Vector3 legacyPos, Vector3 legacyRot)
        {
            Transform parent = string.IsNullOrEmpty(slotName) ? null : Util.FindDeep(animator.transform, slotName);
            var t = new GameObject(name).transform;
            if (parent != null)
            {
                t.SetParent(parent, false);
                t.rotation = transform.rotation * restInCharacter;
                return t;
            }
            // Model bez gniazd (np. z Mixamo): ręczne przesunięcie względem kości dłoni.
            parent = animator.isHuman ? animator.GetBoneTransform(bone) : null;
            t.SetParent(parent != null ? parent : animator.transform, false);
            t.localPosition = legacyPos;
            t.localRotation = Quaternion.Euler(legacyRot);
            return t;
        }

        void SetModelGear(RigLook look)
        {
            if (weaponObj != null) Util.DestroyNow(weaponObj);
            if (shieldObj != null) Util.DestroyNow(shieldObj);
            modelWeapon = look.weapon;
            weaponObj = GearObject(weaponSocket, look.weaponItem, () => GearBuilder.Weapon(null, look.weapon == WeaponModel.Claws ? WeaponModel.None : look.weapon, look.weaponColor), out weaponLength);
            if (weaponObj != null) { weaponRestPos = weaponObj.transform.localPosition; weaponRestRot = weaponObj.transform.localRotation; }
            var off = look.offhandItem;
            var shieldAt = shieldGrip != null ? shieldGrip : shieldSocket;
            if (off != null && off.model != null) shieldObj = GearObject(off.kind == ItemKind.Shield ? shieldAt : shieldSocket, off, null, out _);
            else if (look.shield != ShieldModel.None) shieldObj = GearObject(shieldAt, null, () => GearBuilder.Shield(null, look.shield, look.shieldColor), out _);
            // Noże bliźniacze: drugi nóż w lewej dłoni (jak u postaci proceduralnej).
            else if (look.weapon == WeaponModel.Knives) shieldObj = GearObject(shieldSocket, look.weaponItem, () => GearBuilder.Weapon(null, WeaponModel.Knives, look.weaponColor), out _);
            gearTint.Clear();
            equipmentPose = new HellEquipmentPose(Model.GetComponentInChildren<Animator>(), transform, look, ModelSet, weaponObj,
                modelDef != null && modelDef.anatomicalGripSlots, shieldSocket);
            mainChain = HellFlailChain.Find(weaponObj);
            offhandChain = HellFlailChain.Find(shieldObj);
            if (weaponObj != null) gearTint.Collect(weaponObj.GetComponentsInChildren<Renderer>());
        }

        float weaponLength = 0.9f;

        /// <summary>Model przedmiotu w gnieździe: z pliku przedmiotu, a bez niego – proceduralny (ostrze z +Z obrócone na +Y).</summary>
        GameObject GearObject(Transform socket, ItemDefinition item, System.Func<GameObject> procedural, out float length)
        {
            length = 0.9f;
            if (socket == null) return null;
            GameObject go;
            if (item != null && item.model != null)
            {
                go = Instantiate(item.model, socket, false);
                go.name = item.id;
                foreach (var c in go.GetComponentsInChildren<Collider>()) Util.DestroySafe(c);
                foreach (var a in go.GetComponentsInChildren<Animator>()) Util.DestroySafe(a);
                go.transform.localScale *= item.modelScale;
                go.transform.localPosition = Vector3.down * item.modelGripShift;
                float top = 0f;
                foreach (var r in go.GetComponentsInChildren<Renderer>())
                {
                    var b = r.bounds;
                    top = Mathf.Max(top, socket.InverseTransformPoint(b.center + socket.up * b.extents.magnitude).y);
                }
                if (top > 0.2f) length = top * 0.9f;
                return go;
            }
            if (procedural == null) return null;
            go = procedural();
            if (go == null) return null;
            go.transform.SetParent(socket, false);
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            return go;
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
            SetModelGear(new RigLook { weapon = weapon, weaponColor = weaponColor, shield = shield, shieldColor = shieldColor });
        }

        void Clear()
        {
            Driver?.Dispose();
            Driver = null;
            if (Rig != null) Util.DestroyNow(Rig.gameObject);
            if (Model != null) Util.DestroyNow(Model);
            Rig = null; Model = null;
            modelTint.Clear(); gearTint.Clear();
            weaponSocket = shieldSocket = shieldGrip = leftHand = leftElbow = rightHand = rightElbow = null;
            weaponObj = shieldObj = null;
            ModelSet = null;
            bodySurface = null;
            equipmentPose = null;
            mainChain = offhandChain = null;
        }

        void OnDestroy() => Driver?.Dispose();

        void LateUpdate()
        {
            Tick(Time.deltaTime);
            // Krew na broni powoli zasycha i znika (~30 s).
            if (blood > 0f)
            {
                blood = Mathf.Max(0f, blood - Time.deltaTime / 30f);
                if (Mathf.Abs(blood - bloodShown) > 0.02f || blood <= 0f) { bloodShown = blood; if (Rig != null) Rig.SetWeaponBlood(blood); }
            }
            // Błysk trafienia wygasa niezależnie od tego, kto i kiedy ustawia zabarwienie.
            if (flash > 0f || flashShown)
            {
                flash = Mathf.Max(0f, flash - Time.deltaTime / FlashTime);
                ApplyTint();
                flashShown = flash > 0f;
            }
        }

        // ------------------------------------------------------------------ Reakcja na trafienie

        const float FlashTime = 0.1f;
        float flash;
        bool flashShown;
        float hold, holdTotal;
        Vector3 jitterDir;
        float jitterAmp;

        float blood, bloodShown;

        /// <summary>Ile krwi jest na broni (0–1).</summary>
        public float WeaponBlood => blood;

        /// <summary>Cios bronią, który doszedł: ostrze brudzi się krwią (kolejne ciosy – mocniej).</summary>
        public void AddWeaponBlood(float amount)
        {
            blood = Mathf.Clamp01(blood + amount);
            bloodShown = blood;
            if (Rig != null) Rig.SetWeaponBlood(blood);
        }

        /// <summary>Ile jeszcze trwa zatrzymanie (hitstop) – poza stoi.</summary>
        public float HoldRemaining => hold;

        /// <summary>
        /// Trafienie: impuls ciała w kierunku ciosu (świat), krótki biały błysk i – przy hitstopie – drżenie trafionego.
        /// Siła ≈ 0..1,5 (lekki cios ~0,4, ciężki ~0,8, riposta ~1,2).
        /// </summary>
        public void Impact(Vector3 worldDir, float strength, float flashAmount = 0.6f)
        {
            worldDir.y = 0f;
            if (worldDir.sqrMagnitude < 1e-4f) worldDir = -transform.forward;
            worldDir.Normalize();
            if (Driver is IImpactReactive r) r.AddImpact(Quaternion.Inverse(transform.rotation) * worldDir, strength);
            flash = Mathf.Max(flash, Mathf.Clamp01(flashAmount));
            jitterDir = worldDir;
            jitterAmp = Mathf.Clamp(strength, 0f, 1.5f) * 0.035f;
            ApplyTint();
        }

        /// <summary>Hitstop: animacja stoi przez podany czas (licznik nie sumuje się – bierze dłuższy).</summary>
        public void Hold(float seconds)
        {
            if (seconds <= hold) return;
            hold = seconds;
            holdTotal = seconds;
        }

        /// <summary>Krok animacji (publiczny na potrzeby testów).</summary>
        public void Tick(float dt)
        {
            if (Driver == null || source == null || dt <= 0f) return;
            var shakeRoot = Rig != null ? Rig.VisualRoot : Model != null ? Model.transform : null;
            if (hold > 0f)
            {
                // Zatrzymana poza drży wzdłuż kierunku ciosu, coraz słabiej – sama postać, nie kamera.
                hold -= dt;
                float k = holdTotal > 0 ? Mathf.Clamp01(hold / holdTotal) : 0f;
                if (shakeRoot != null && jitterAmp > 0f)
                    shakeRoot.localPosition = BaseOffset(shakeRoot) + transform.InverseTransformDirection(jitterDir) * (Mathf.Sin(hold * 190f) * jitterAmp * k);
                if (hold > 0f) { lastPos = transform.position; UpdateChains(dt); return; }
                hold = 0f;
            }
            if (shakeRoot != null && jitterAmp > 0f) { shakeRoot.localPosition = BaseOffset(shakeRoot); jitterAmp = 0f; }
            Vector3 pos = transform.position;
            if (hasLastPos)
            {
                Vector3 v = (pos - lastPos) / dt;
                v.y = 0;
                velocity = Vector3.Lerp(velocity, v, 1f - Mathf.Exp(-12f * dt));
            }
            lastPos = pos;
            hasLastPos = true;
            var state = source.GetAnimState();
            if (bodySurface != null)
            {
                var enemy = source as EnemyBrain;
                var player = source as PlayerCombat;
                var status = enemy != null ? enemy.Status : player != null ? player.Status : null;
                bool shadow = player != null && player.Run != null && player.Run.startingClass.dodgeStyle == DodgeStyle.Vanish && state.action == AnimAction.Dodge;
                bodySurface.Set(shadow ? 1 : 0, status != null && status.ChillStacks > 0 ? 0.8f : 0, status != null && status.Burning ? 1 : 0, enemy != null && enemy.IsElite ? 1 : 0);
            }
            Driver.Tick(state, velocity, dt);
            if (Model != null) { PostPose(state, dt); GuardGrip(); equipmentPose?.Apply(state, weaponSocket, dt); CarryShield(); DropWeaponOnDeath(state, dt); }
            UpdateChains(dt);
        }

        /// <summary>
        /// Niska garda topora: broń przesuwa się w prawej dłoni ku końcowi trzonka (prawa dłoń wyżej, lewa przy końcu)
        /// i obraca wokół trzonka ostrzem w dół – dłonie zostają w naturalnym chwycie (GreatSwingPose.GuardGrip/GuardSpin).
        /// </summary>
        void GuardGrip()
        {
            var swing = (Driver as ClipAnimationDriver)?.GreatSwing;
            float g = swing != null ? swing.GuardGrip : 0f;
            if (equipmentPose != null) equipmentPose.GuardGrip = g;
            if (weaponObj == null || weaponDropped || weaponObj.transform.parent != weaponSocket) return;
            float spin = swing != null ? swing.GuardSpin : 0f;
            weaponObj.transform.localPosition = weaponRestPos + Vector3.down * (GreatSwingPose.GuardSlide * g);
            weaponObj.transform.localRotation = Quaternion.AngleAxis(spin, Vector3.up) * weaponRestRot;
        }

        // ------------------------------------------------------------------ Broń wypada przy śmierci

        /// <summary>Czas spadania broni na ziemię (s) i najpóźniejsza chwila wypuszczenia od początku śmierci.</summary>
        public const float WeaponDropFall = 0.28f, WeaponDropLatest = 1.8f;
        bool weaponDropped;
        float dropT;
        Transform dropParent;
        Vector3 dropLocalPos, dropFromPos, dropToPos;
        Quaternion dropLocalRot, dropFromRot, dropToRot;
        /// <summary>Broń leży na ziemi po śmierci (dla testów).</summary>
        public bool WeaponDropped => weaponDropped;

        /// <summary>
        /// Śmierć: broń wypada z dłoni, gdy dotyka podłogi albo dłoń jest nisko (najpóźniej po WeaponDropLatest s), i w krótkim
        /// spadku kładzie się płasko na ziemi (głowica płasko, trzonek poziomo) – bez wchodzenia w podłogę. Bez fizyki.
        /// Korbacze (łańcuch) zostają w dłoni. Po wyjściu ze śmierci (wskrzeszenie) broń wraca do gniazda.
        /// </summary>
        void DropWeaponOnDeath(in CharacterAnimState s, float dt)
        {
            if (s.action != AnimAction.Death)
            {
                if (weaponDropped && weaponObj != null && dropParent != null)
                {
                    weaponObj.transform.SetParent(dropParent, false);
                    weaponObj.transform.localPosition = dropLocalPos; weaponObj.transform.localRotation = dropLocalRot;
                }
                weaponDropped = false;
                return;
            }
            if (weaponObj == null || weaponSocket == null || mainChain != null) return;
            float floor = transform.position.y;
            if (!weaponDropped)
            {
                bool touches = LowestPoint(weaponObj) < floor + 0.03f || weaponSocket.position.y - floor < 0.45f;
                if (!(s.actionTime >= 0.4f && touches) && s.actionTime < WeaponDropLatest) return;
                var t = weaponObj.transform;
                dropParent = t.parent; dropLocalPos = t.localPosition; dropLocalRot = t.localRotation;
                dropFromPos = t.position; dropFromRot = t.rotation;
                // płasko: trzonek (oś +Y gniazda) poziomo, płaszczyzna głowicy (oś Z gniazda) pionowo w górę
                Vector3 shaft = Vector3.ProjectOnPlane(weaponSocket.up, Vector3.up);
                if (shaft.sqrMagnitude < 1e-4f) shaft = transform.forward;
                var socketRot = Quaternion.LookRotation(weaponSocket.forward, weaponSocket.up);
                dropToRot = Quaternion.LookRotation(Vector3.up, shaft.normalized) * Quaternion.Inverse(socketRot) * dropFromRot;
                t.SetParent(transform, true);
                t.rotation = dropToRot;
                dropToPos = dropFromPos + Vector3.up * (floor + 0.005f - LowestPoint(weaponObj));
                t.SetPositionAndRotation(dropFromPos, dropFromRot);
                weaponDropped = true; dropT = 0f;
            }
            dropT = Mathf.Min(WeaponDropFall, dropT + dt);
            float u = dropT / WeaponDropFall, k = u * u;          // spadanie z przyspieszeniem
            weaponObj.transform.SetPositionAndRotation(Vector3.Lerp(dropFromPos, dropToPos, k), Quaternion.Slerp(dropFromRot, dropToRot, Mathf.Sqrt(u)));
        }

        static float LowestPoint(GameObject go)
        {
            float y = float.MaxValue;
            foreach (var r in go.GetComponentsInChildren<Renderer>()) if (r.enabled) y = Mathf.Min(y, r.bounds.min.y);
            return y;
        }

        void UpdateChains(float dt)
        {
            mainChain?.Update(dt,transform.position.y);
            offhandChain?.Update(dt,transform.position.y);
        }

        // ------------------------------------------------------------------ Poprawki pozy modelu FBX

        GripStyle modelGrip;
        float uprightWeight;
        Quaternion weaponSocketRest = Quaternion.identity; // chwyt z dłoni (lokalnie) – punkt wyjścia poprawek, by się nie kumulowały

        /// <summary>
        /// Po klipie: kostur bez własnych nagrań (jak w Hell Unleashed – StaffRest) stoi pionowo przy boku, gdy postać
        /// nic nie robi albo idzie; miecz jednoręczny przy swobodnie opuszczonej ręce (spoczynek bez broni w nagraniu)
        /// opada sztychem ku ziemi, zamiast sterczeć poziomo. W czasie czaru i ataku broń zostaje w dłoni tak, jak prowadzi ją klip.
        /// </summary>
        void PostPose(in CharacterAnimState s, float dt)
        {
            CarryShield();
            if (weaponSocket == null) return;
            weaponSocket.localRotation = weaponSocketRest;
            bool rest = s.action == AnimAction.None || s.action == AnimAction.Block || s.action == AnimAction.Drink;
            if (modelGrip == GripStyle.Staff)
            {
                uprightWeight = Mathf.MoveTowards(uprightWeight, rest ? 1f : 0f, dt * 6f);
                if (uprightWeight <= 0f) return;
                // Kierunek drzewca: lekko do przodu od pionu (az 4°, el 82° w osiach postaci).
                Vector3 blade = transform.rotation * new Vector3(Mathf.Sin(4f * Mathf.Deg2Rad) * Mathf.Cos(82f * Mathf.Deg2Rad), Mathf.Sin(82f * Mathf.Deg2Rad), Mathf.Cos(4f * Mathf.Deg2Rad) * Mathf.Cos(82f * Mathf.Deg2Rad));
                Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, blade).normalized;
                var upright = Quaternion.LookRotation(fwd, blade);
                weaponSocket.rotation = Quaternion.Slerp(weaponSocket.rotation, upright, uprightWeight);
                return;
            }
            if (modelGrip == GripStyle.OneHand && ModelSet != null && ModelSet.useModelIdle)
            {
                // Postawa bojowa jak w Hell Unleashed (SwingStyles.Stance): ostrze 32° w prawo od przodu i 34° w górę –
                // miecz uniesiony przed barkiem, nie zwisa. W ataku i innych akcjach broń prowadzi klip.
                bool stance = s.action == AnimAction.None;
                uprightWeight = Mathf.MoveTowards(uprightWeight, stance ? 1f : 0f, dt * (stance ? 3f : 8f));
                if (uprightWeight <= 0f) return;
                float az = 32f * Mathf.Deg2Rad, el = 34f * Mathf.Deg2Rad;
                Vector3 blade = transform.rotation * new Vector3(Mathf.Sin(az) * Mathf.Cos(el), Mathf.Sin(el), Mathf.Cos(az) * Mathf.Cos(el));
                Vector3 top = Vector3.ProjectOnPlane(transform.up, blade);
                if (top.sqrMagnitude < 1e-4f) top = transform.forward;
                weaponSocket.rotation = Quaternion.Slerp(weaponSocket.rotation, Quaternion.LookRotation(top.normalized, blade), Smooth01(uprightWeight));
                return;
            }
            if (modelGrip != GripStyle.OneHand || rightHand == null) return;
            // Ręka zwisa wzdłuż ciała → ostrze w dół i lekko do przodu (jak luźno trzymany miecz).
            Vector3 forearm = (rightHand.position - rightElbow.position).normalized;
            float hang = rest ? Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 0.92f, Vector3.Dot(forearm, -transform.up))) : 0f;
            uprightWeight = Mathf.MoveTowards(uprightWeight, hang, dt * 6f);
            if (uprightWeight <= 0f) return;
            Vector3 down = (transform.forward * 0.62f - transform.up * 0.78f + transform.right * 0.1f).normalized;
            Vector3 edge = Vector3.ProjectOnPlane(transform.up, down).sqrMagnitude > 1e-4f ? Vector3.Cross(transform.right, down) : transform.forward;
            weaponSocket.rotation = Quaternion.Slerp(weaponSocket.rotation, Quaternion.LookRotation(edge, down), uprightWeight);
        }

        /// <summary>
        /// Tarcza przypięta do przedramienia (jak na pasach): liczona w każdej klatce z kierunku przedramienia, a nie z dłoni –
        /// zgięcie nadgarstka w klipie jej nie przechyla. Oś pionowa tarczy jest prostopadła do przedramienia i możliwie
        /// pionowa, więc przy przedramieniu równoległym do podłoża tarcza stoi dokładnie w pionie; lico patrzy na zewnątrz
        /// ciała (przed siebie w gardzie, w bok przy boku). Przy ręce opuszczonej wzdłuż ciała tarcza płynnie przechodzi
        /// do zawieszenia wzdłuż ręki (pionowo przy boku), zamiast sterczeć poziomo.
        /// </summary>
        void CarryShield()
        {
            if (shieldGrip == null || ModelSet == null) return;
            if (leftHand == null)
            {
                var anim = Model.GetComponentInChildren<Animator>();
                if (anim == null || !anim.isHuman) return;
                leftHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
                leftElbow = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                if (leftHand == null || leftElbow == null) { leftHand = null; return; }
            }
            Vector3 up = transform.up;
            Vector3 forearm = leftHand.position - leftElbow.position;
            float length = forearm.magnitude;
            if (length < 1e-4f) return;
            Vector3 f = forearm / length;
            // Góra tarczy: pion rzutowany na płaszczyznę prostopadłą do przedramienia (przy ręce zwisającej – wzdłuż ręki w górę).
            Vector3 strapUp = Vector3.ProjectOnPlane(up, f);
            strapUp = strapUp.sqrMagnitude > 1e-4f ? strapUp.normalized : Vector3.ProjectOnPlane(transform.forward, f).normalized;
            float hang = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 0.9f, Vector3.Dot(f, -up)));
            Vector3 top = Vector3.Slerp(strapUp, -f, hang).normalized;
            // Lico na zewnątrz: od osi ciała w stronę przedramienia.
            Vector3 mid = leftElbow.position + forearm * 0.5f;
            Vector3 axis = transform.position + up * Vector3.Dot(mid - transform.position, up);
            Vector3 outward = Vector3.ProjectOnPlane(mid - axis, top);
            Vector3 face = Vector3.Cross(f, top);
            if (face.sqrMagnitude < 1e-4f) face = outward;
            face = Vector3.ProjectOnPlane(face, top).normalized;
            if (Vector3.Dot(face, outward) < 0f) face = -face;
            // Model tarczy: +Z – góra, +Y – lico. Środek na zewnętrznej stronie przedramienia.
            float scale = Model.transform.lossyScale.x;
            shieldGrip.SetPositionAndRotation(mid + face * (0.07f * scale), Quaternion.LookRotation(top, face));
        }

        static float Smooth01(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

        Vector3 BaseOffset(Transform root) => Rig != null ? Vector3.zero : (modelDef != null ? modelDef.positionOffset : Vector3.zero);

        // ------------------------------------------------------------------ Kolory

        Color tintColor = Color.white;
        float tintAmount;

        public void SetTint(Color c, float amount)
        {
            tintColor = c;
            tintAmount = amount;
            ApplyTint();
        }

        /// <summary>Zabarwienie zamówione przez właściciela, rozjaśnione błyskiem trafienia.</summary>
        void ApplyTint()
        {
            Color c = tintColor;
            float amount = tintAmount;
            if (flash > 0f)
            {
                float f = flash * flash;
                c = Color.Lerp(amount > 0f ? c : Color.white, new Color(1f, 0.97f, 0.9f), f);
                amount = Mathf.Max(amount, 0.75f * f);
            }
            if (Rig != null) { Rig.SetTint(c, amount); return; }
            modelTint.SetTint(c, amount);
        }

        /// <summary>Pokazuje/ukrywa broń w dłoni (np. na czas ciężkiego rzutu).</summary>
        public void SetWeaponVisible(bool visible)
        {
            var socket = Rig != null ? Rig.WeaponSocket : weaponSocket;
            if (socket == null) return;
            foreach (Transform child in socket) child.gameObject.SetActive(visible);
        }

        public void SetWeaponGlow(Color c, float intensity)
        {
            if (Rig != null) { Rig.SetWeaponGlow(c, intensity); return; }
            // Model bez broni (np. pazury w animacji) – delikatna poświata całej postaci.
            if (gearTint.Count == 0) { if (intensity > 0) SetTint(c, intensity * 0.35f); return; }
            gearTint.SetGlow(c, intensity);
        }

        public Vector3 WeaponTip => mainChain != null ? mainChain.HeadTip : Rig != null ? Rig.WeaponTip : (weaponSocket != null ? weaponSocket.position + weaponSocket.up * weaponLength : transform.position + Vector3.up);
    }
}
