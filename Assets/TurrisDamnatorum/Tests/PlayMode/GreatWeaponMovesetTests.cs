#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Zestaw ruchów ciężkiej broni dwuręcznej (topór, młot) wzorowany na „ciężkim toporze” souls-like:
    /// klatki ciosów z boku i zza pleców do porównania z nagraniem wzorca.
    /// </summary>
    public class GreatWeaponMovesetTests
    {
        const string Visuals = "Assets/TurrisDamnatorum/Content/Visuals/";
        const string Weapons = "Assets/TurrisDamnatorum/Models/Weapons/";
        const string Mixamo = "Assets/TurrisDamnatorum/Models/Animations/Mixamo/";
        readonly List<Object> cleanup = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            Time.captureFramerate = 0;
        }

        RigLook Greataxe()
        {
            var it = ScriptableObject.CreateInstance<ItemDefinition>();
            it.id = "greataxe"; it.kind = ItemKind.Weapon;
            it.model = AssetDatabase.LoadAssetAtPath<GameObject>(Weapons + "greataxe/greataxe.fbx");
            it.modelScale = 1f;
            cleanup.Add(it);
            return new RigLook { weapon = WeaponModel.GreatAxe, weaponItem = it };
        }

        static AnimationClip Load(string path) =>
            AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));

        Camera Stage()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(80, 1, 80);
            ground.GetComponent<Renderer>().material.color = new Color(0.42f, 0.40f, 0.38f);
            cleanup.Add(ground);
            var lightGo = new GameObject("L"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(40, -45, 0); cleanup.Add(lightGo);
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.52f);
            var camGo = new GameObject("Cam"); var cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.backgroundColor = new Color(0.55f, 0.42f, 0.36f); cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 34;
            return cam;
        }

        /// <summary>
        /// Pełny zamach: po rąbnięciu (klip autorski) ostrze topora jest przy ziemi przed postacią, a biodra niżej (przysiad);
        /// po cięciu topór jest po drugiej stronie ciała. Zwykłe cięcie (miecz dwuręczny) zostaje bez dociągnięcia.
        /// </summary>
        [UnityTest]
        public IEnumerator GreatSwing_ChopReachesTheGround_CleaveCarriesThrough()
        {
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Visuals + "visual_hero_warrior_berserker.asset");
            var go = new GameObject("H"); cleanup.Add(go);
            var v = go.AddComponent<CharacterVisual>();
            var src = new StubAnimSource();
            v.BuildModel(def, 1f, Greataxe()); v.Init(src);
            var swing = ((ClipAnimationDriver)v.Driver).GreatSwing;
            var axe = Util.FindDeep(go.transform, "WeaponSocket").GetComponentsInChildren<Renderer>();
            Assert.IsNotEmpty(axe, "model topora");
            float Lowest() { float y = float.MaxValue; foreach (var r in axe) y = Mathf.Min(y, r.bounds.min.y); return y; }
            float Side() { float x = 0f; foreach (var r in axe) x += go.transform.InverseTransformPoint(r.bounds.center).x; return x / axe.Length; }
            Time.captureFramerate = 60;
            IEnumerator Hold(AttackAnim a, ActionPhase phase, float q)
            {
                src.state = new CharacterAnimState { action = AnimAction.Attack, attack = a, phase = phase, phaseProgress = q,
                    windupSeconds = 0.9f, activeSeconds = 0.17f, actionDuration = 1.7f };
                for (int i = 0; i < 20; i++) yield return null;
            }
            for (int i = 0; i < 40; i++) yield return null;
            // Postawa topora (uwaga użytkownika 2026-10-31: „ręce z toporem jakby w tułowiu, a powinny być przed nim; topór
            // nienaturalnie uniesiony i przylepiony”, potem wzór z Elden Ring): niska garda – trzonek prawie poziomo przed
            // pasem, głowica po prawej, dłonie na wysokości bioder i przed tułowiem.
            var socket = Util.FindDeep(go.transform, "WeaponSocket");
            Vector3 grip = go.transform.InverseTransformPoint(socket.position), aim = go.transform.InverseTransformDirection(socket.up);
            Debug.Log($"DETAIL postawa: udział {swing.GuardWeight:F2}, trzonek w górę {aim.y:F2}, w przód {aim.z:F2}, w prawo {aim.x:F2}, chwyt przód {grip.z:F2} m, wys. {grip.y:F2} m, bok {grip.x:F2} m");
            Assert.Greater(swing.GuardWeight, 0.9f, "postawa topora w spoczynku");
            Assert.Less(Mathf.Abs(aim.y), 0.3f, "trzonek prawie poziomo");
            Assert.Greater(aim.x, 0.8f, "głowica po prawej");
            Assert.That(grip.y, Is.InRange(0.85f, 1.1f), "dłonie na wysokości bioder");
            Assert.Greater(grip.z, 0.25f, "chwyt przed tułowiem");

            yield return Hold(AttackAnim.Overhead, ActionPhase.Recovery, 0.15f);
            float plain = Lowest();
            Assert.Less(swing.Current.z, 0.01f, "zwykły cios bez dociągnięcia");

            var hipsBone = v.Model.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Hips);
            float hipsPlain = hipsBone.position.y;
            yield return Hold(AttackAnim.Chop, ActionPhase.Recovery, 0.15f);
            // Rąbnięcie to klip autorski (ArtSource/AnimationRepair/heavy-chop): przysiad i skłon są w nagraniu, nie w GreatSwingPose.
            float squat = hipsPlain - hipsBone.position.y;
            Debug.Log($"DETAIL rąbnięcie: najniżej {Lowest():F2} m (bez dociągnięcia {plain:F2}), biodra niżej o {squat:F2} m");
            Assert.Less(Lowest(), 0.2f, "ostrze przy ziemi");
            Assert.Greater(Lowest(), -0.05f, "ostrze nie wchodzi w podłogę");
            Assert.Greater(squat, 0.12f, "przysiad");

            yield return Hold(AttackAnim.Cleave, ActionPhase.Recovery, 0.15f);
            float left = Side();
            yield return Hold(AttackAnim.CleaveBack, ActionPhase.Recovery, 0.15f);
            float right = Side();
            Debug.Log($"DETAIL cięcia: topór po lewej {left:F2} m, po prawej {right:F2} m");
            Assert.Less(left, -0.2f, "pierwsze cięcie kończy się po lewej");
            Assert.Greater(right, 0.2f, "drugie cięcie kończy się po prawej");

            // Koniec akcji – poza wraca do postawy topora (wyprostowane plecy, lekko ugięte kolana).
            src.state = default;
            for (int i = 0; i < 40; i++) yield return null;
            var guard = new Vector3(GreatSwingPose.GuardLean, 0f, GreatSwingPose.GuardDrop);
            Assert.Less((swing.Current - guard).magnitude, 0.5f, "po ciosie wraca do postawy");
        }

        /// <summary>Kandydaci na postawę bojową topora: każdy klip jako spoczynek zestawu dwuręcznego – przód 3/4, bok i z góry kamery gry.</summary>
        [UnityTest]
        public IEnumerator IdleCandidates_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR."); yield break; }
            string gs = Mixamo + "Great Sword Pack/great sword ";
            var names = new[]
            {
                gs + "idle.fbx", gs + "idle (2).fbx", gs + "idle (3).fbx", gs + "idle (4).fbx", gs + "idle (5).fbx",
                gs + "crouching.fbx", gs + "blocking (3).fbx", Mixamo + "Pro Sword and Shield Pack/sword and shield idle (4).fbx",
            };
            var cam = Stage();
            var baseDef = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Visuals + "visual_hero_warrior_berserker.asset");
            string only = System.Environment.GetEnvironmentVariable("TURRIS_IDLE_ONLY");
            if (!string.IsNullOrEmpty(only)) names = only.Split('|').Select(n => n.StartsWith("Assets/") ? n : Mixamo + n).ToArray();
            // TURRIS_IDLE_LEGS=1 – zbliżenie na nogi: przód, bok, tył.
            bool legs = System.Environment.GetEnvironmentVariable("TURRIS_IDLE_LEGS") == "1";
            int W = string.IsNullOrEmpty(only) ? 220 : 420, H = string.IsNullOrEmpty(only) ? 280 : 540;
            var sheet = new Texture2D(W * names.Length, H * 3, TextureFormat.RGB24, false);
            var rt = new RenderTexture(W, H, 24);
            for (int n = 0; n < names.Length; n++)
            {
                var clip = Load(names[n]);
                if (clip == null) { Debug.LogWarning("brak " + names[n]); continue; }
                var set = Object.Instantiate(baseDef.twoHandSet); cleanup.Add(set);
                set.idle = clip; set.greatWeaponIdle = clip;
                var def = Object.Instantiate(baseDef); def.twoHandSet = set; cleanup.Add(def);
                var go = new GameObject("H"); cleanup.Add(go);
                var v = go.AddComponent<CharacterVisual>();
                v.BuildModel(def, 1f, Greataxe()); v.Init(new StubAnimSource());
                Time.captureFramerate = 60;
                for (int i = 0; i < 50; i++) yield return null;
                for (int k = 0; k < 3; k++)
                {
                    var off = k == 0 ? new Vector3(1.6f, 1.3f, 3.2f) : k == 1 ? new Vector3(3.6f, 1.1f, 0f) : new Vector3(0f, 7f, -5.5f);
                    float look = 0.95f;
                    if (legs) { off = k == 0 ? new Vector3(0.2f, 0.7f, 2.9f) : k == 1 ? new Vector3(2.9f, 0.6f, 0f) : new Vector3(-0.3f, 0.9f, -2.9f); look = 0.5f; }
                    cam.transform.position = off;
                    cam.transform.LookAt(Vector3.up * look);
                    cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                    sheet.ReadPixels(new Rect(0, 0, W, H), n * W, (2 - k) * H);
                    RenderTexture.active = null; cam.targetTexture = null;
                }
                Debug.Log($"DETAIL idle {n}: {names[n]} | {LegReport(go.GetComponentInChildren<Animator>(), go.transform)}");
                go.SetActive(false);
            }
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(dir, "idle_candidates.png"), sheet.EncodeToPNG());
            Object.Destroy(sheet); Object.Destroy(rt);
        }

        /// <summary>Nogi: kostka i palce nad ziemią (m), skręt stopy od przodu postaci i kolana od kierunku stopy (°).</summary>
        static string LegReport(Animator an, Transform root)
        {
            string r = "";
            foreach (bool left in new[] { true, false })
            {
                Transform B(HumanBodyBones l, HumanBodyBones rr) => an.GetBoneTransform(left ? l : rr);
                var hip = B(HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg).position;
                var knee = B(HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg).position;
                var ankle = B(HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot).position;
                var toe = B(HumanBodyBones.LeftToes, HumanBodyBones.RightToes).position;
                Vector3 f = Vector3.ProjectOnPlane(toe - ankle, Vector3.up), axis = (ankle - hip).normalized;
                float yaw = Vector3.SignedAngle(root.forward, f, Vector3.up);
                float kneeTwist = Vector3.SignedAngle(Vector3.ProjectOnPlane(f, axis), Vector3.ProjectOnPlane(knee - hip, axis), axis);
                float pitchFoot = Vector3.SignedAngle(f, toe - ankle, Vector3.Cross(Vector3.up, f));
                var al = root.InverseTransformPoint(ankle);
                r += $"{(left ? "L" : "P")}: x {al.x:F2} z {al.z:F2} kostka {ankle.y - root.position.y:F3} palce {toe.y - root.position.y:F3} nachylenie {pitchFoot:F0} skręt {yaw:F0} kolano {kneeTwist:F0}; ";
            }
            // Najniższy wierzchołek buta (siatka po skórowaniu) – stopa stoi na podłodze, nie wisi i nie wchodzi w nią.
            var baked = new Mesh();
            float lowL = float.MaxValue, lowR = float.MaxValue;
            var fl = an.GetBoneTransform(HumanBodyBones.LeftFoot).position; var fr = an.GetBoneTransform(HumanBodyBones.RightFoot).position;
            foreach (var skin in an.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skin.BakeMesh(baked);
                foreach (var v in baked.vertices)
                {
                    var p = skin.transform.TransformPoint(v);
                    if (p.y - root.position.y > 0.25f) continue;
                    if ((p - fl).sqrMagnitude < (p - fr).sqrMagnitude) lowL = Mathf.Min(lowL, p.y - root.position.y); else lowR = Mathf.Min(lowR, p.y - root.position.y);
                }
            }
            Object.Destroy(baked);
            var hl = an.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position; var hr = an.GetBoneTransform(HumanBodyBones.RightUpperLeg).position;
            float hipsYaw = Vector3.SignedAngle(root.right, Vector3.ProjectOnPlane(hr - hl, Vector3.up), Vector3.up);
            return r + $"spód buta L {lowL:F3} P {lowR:F3}; miednica {hipsYaw:F0}°";
        }

        /// <summary>Postawa topora w ruchu: spoczynek, chód 1,4 m/s, bieg 4 m/s – po 5 klatek z boku (góra) i z przodu (dół).</summary>
        [UnityTest]
        public IEnumerator GuardInMotion_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR."); yield break; }
            var cam = Stage();
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Visuals + "visual_hero_warrior_berserker.asset");
            var go = new GameObject("H"); cleanup.Add(go);
            var v = go.AddComponent<CharacterVisual>();
            v.BuildModel(def, 1f, Greataxe()); v.Init(new StubAnimSource());
            Time.captureFramerate = 60;
            const int W = 200, H = 240, N = 5;
            float[] speeds = { 0f, 1.4f, 4f };
            var sheet = new Texture2D(W * N * speeds.Length, H * 2, TextureFormat.RGB24, false);
            var rt = new RenderTexture(W, H, 24);
            for (int m = 0; m < speeds.Length; m++)
            {
                for (int i = 0; i < 60; i++) { go.transform.position += Vector3.forward * speeds[m] / 60f; yield return null; }
                for (int shot = 0; shot < N; shot++)
                {
                    for (int i = 0; i < 8; i++) { go.transform.position += Vector3.forward * speeds[m] / 60f; yield return null; }
                    Vector3 p = go.transform.position;
                    for (int k = 0; k < 2; k++)
                    {
                        cam.transform.position = p + (k == 0 ? new Vector3(3.6f, 1.1f, 0f) : new Vector3(1.2f, 1.6f, 3.4f));
                        cam.transform.LookAt(p + Vector3.up * 0.95f);
                        cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                        sheet.ReadPixels(new Rect(0, 0, W, H), (m * N + shot) * W, (1 - k) * H);
                        RenderTexture.active = null; cam.targetTexture = null;
                    }
                }
            }
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(dir, "guard_motion.png"), sheet.EncodeToPNG());
            Object.Destroy(sheet); Object.Destroy(rt);
        }

        /// <summary>Kandydaci z paczek Mixamo – każdy klip jako „cios” w zestawie dwuręcznym, 10 klatek, bok i zza pleców.</summary>
        [UnityTest]
        public IEnumerator CandidateClips_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR."); yield break; }
            string gs = Mixamo + "Great Sword Pack/great sword ";
            var names = new[]
            {
                gs + "slash.fbx", gs + "slash (2).fbx", gs + "slash (3).fbx", gs + "slash (4).fbx", gs + "slash (5).fbx",
                gs + "attack.fbx", gs + "high spin attack.fbx", gs + "jump attack.fbx", gs + "slide attack.fbx",
                gs + "kick.fbx", gs + "power up.fbx",
                Mixamo + "Loose/Standing Melee Attack Downward.fbx", Mixamo + "Loose/Standing Melee Attack 360 High.fbx",
                Mixamo + "Loose/Standing Melee Combo Attack Ver. 3.fbx",
            };
            var cam = Stage();
            var baseDef = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Visuals + "visual_hero_warrior_berserker.asset");
            const int W = 200, H = 240, N = 10;
            foreach (var path in names)
            {
                var clip = Load(path);
                if (clip == null) { Debug.LogWarning("brak " + path); continue; }
                var set = Object.Instantiate(baseDef.twoHandSet); cleanup.Add(set);
                set.actions = set.actions.Where(a => a.action != AnimAction.Attack).ToList();
                set.actions.Add(new ActionClip { action = AnimAction.Attack, attack = AttackAnim.SlashRight, clip = clip, windupEnd = 0.45f, activeEnd = 0.6f });
                var def = Object.Instantiate(baseDef); def.twoHandSet = set; cleanup.Add(def);
                var sheet = new Texture2D(W * N, H * 2, TextureFormat.RGB24, false);
                yield return Play(def, cam, AttackAnim.SlashRight, 0.45f * clip.length, 0.15f * clip.length, 0.4f * clip.length, sheet, W, H, N);
                File.WriteAllBytes(Path.Combine(dir, "cand_" + Path.GetFileNameWithoutExtension(path).Replace(' ', '_') + ".png"), sheet.EncodeToPNG());
                Debug.Log($"DETAIL clip {path}: {clip.length:F2} s, {clip.frameRate} fps");
                Object.Destroy(sheet);
            }
        }

        /// <summary>Szybkość prawej dłoni względem bioder klatka po klatce (30 kl./s) – do wyznaczenia klatek zamachu i trafienia.</summary>
        [UnityTest]
        public IEnumerator ClipHandSpeed_WhenRequested()
        {
            if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR"))) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR."); yield break; }
            string gs = Mixamo + "Great Sword Pack/great sword ";
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Visuals + "visual_hero_warrior_berserker.asset");
            var raw = Object.Instantiate(def.modelPrefab); cleanup.Add(raw);
            var anim = raw.GetComponentInChildren<Animator>();
            var hand = anim.GetBoneTransform(HumanBodyBones.RightHand); var hips = anim.GetBoneTransform(HumanBodyBones.Hips);
            foreach (var path in new[] { gs + "slash (2).fbx", gs + "slash (3).fbx", gs + "high spin attack.fbx", Mixamo + "Loose/Standing Melee Attack 360 High.fbx", Mixamo + "Loose/Standing Melee Combo Attack Ver. 3.fbx" })
            {
                var clip = Load(path);
                var speeds = new List<string>();
                Vector3 last = Vector3.zero;
                int n = Mathf.RoundToInt(clip.length * 30f);
                for (int f = 0; f <= n; f++)
                {
                    clip.SampleAnimation(raw, f / 30f);
                    Vector3 rel = hand.position - hips.position;
                    if (f > 0) speeds.Add(((rel - last).magnitude * 30f).ToString("F1"));
                    last = rel;
                }
                Debug.Log($"DETAIL speed {System.IO.Path.GetFileName(path)}: " + string.Join(" ", speeds));
            }
            yield return null;
        }

        /// <summary>Cały zestaw ruchów z czasami topora dwuręcznego (jak w grze): każdy ruch – 10 klatek z boku (góra) i zza pleców (dół).</summary>
        [UnityTest]
        public IEnumerator Moveset_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR."); yield break; }
            var cam = Stage();
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Visuals + "visual_hero_warrior_berserker.asset");
            var axe = AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/TurrisDamnatorum/Content/Items/hu_greataxe.asset");
            Assert.IsNotNull(axe, "hu_greataxe");
            var L = axe.weapon.light; var Hv = axe.weapon.heavy;
            // Tempo gry: akcje ×1,1, odzyskanie ×0,8 (PlayerCombat.ScaleStartup/ScaleRecovery), +10% szybkości broni dwuręcznej.
            float S(float x) => x / 1.1f / 1.1f; float R(float x) => x * 0.8f / 1.1f / 1.1f;
            var moves = new (string name, AttackAnim anim, AttackDefinition a, float windMul, float hold, float carry)[]
            {
                ("light1", AttackAnim.Cleave, L, 1f, 0f, 0f),
                ("light2", AttackAnim.CleaveBack, L, 1f, 0f, 0f),
                ("heavy1", AttackAnim.Chop, Hv, 1f, 0f, 0f),
                ("heavy2", AttackAnim.ChopFollow, Hv, 1f, 0f, 0f),
                ("charged", AttackAnim.Chop, Hv, 1f, PlayerCombat.ChargeMax, 0f),
                ("run_light", AttackAnim.RunningSweep, L, 1f, 0f, PlayerCombat.RunningLightCarry),
                ("run_heavy", AttackAnim.LeapChop, Hv, 1f, 0f, PlayerCombat.RunningHeavyCarry),
                ("dodge_chop", AttackAnim.DodgeChop, L, PlayerCombat.DodgeChopWindup, 0f, 0f),
            };
            const int W = 200, H = 240, N = 10;
            var all = new Texture2D(W * N, H * 2 * moves.Length, TextureFormat.RGB24, false);
            for (int m = 0; m < moves.Length; m++)
            {
                var mv = moves[m];
                var sheet = new Texture2D(W * N, H * 2, TextureFormat.RGB24, false);
                yield return Play(def, cam, mv.anim, S(mv.a.windup) * mv.windMul, S(mv.a.active), R(mv.a.recovery), sheet, W, H, N, mv.hold, mv.carry);
                File.WriteAllBytes(Path.Combine(dir, "move_" + mv.name + ".png"), sheet.EncodeToPNG());
                all.SetPixels(0, (moves.Length - 1 - m) * H * 2, W * N, H * 2, sheet.GetPixels());
                Debug.Log($"DETAIL move {mv.name}: zamach {S(mv.a.windup) * mv.windMul:F2} s, trafienie {S(mv.a.active):F2} s, powrót {R(mv.a.recovery):F2} s, ładowanie {mv.hold:F2} s");
                Object.Destroy(sheet);
            }
            all.Apply();
            File.WriteAllBytes(Path.Combine(dir, "moveset_all.png"), all.EncodeToPNG());
            Object.Destroy(all);
        }

        /// <summary>
        /// Gra postać przez cały cios (Startup→Active→Recovery, jak ActionController) i zapisuje N klatek z boku (góra) i zza pleców (dół).
        /// hold – ładowanie: zamach stoi w punkcie PlayerCombat.ChargeHoldAt; carry – rozpęd z biegu (m/s, wygasa do końca trafienia).
        /// </summary>
        IEnumerator Play(CharacterVisualDefinition def, Camera cam, AttackAnim attack, float wind, float act, float rec,
            Texture2D sheet, int W, int H, int N, float hold = 0f, float carry = 0f)
        {
            var go = new GameObject("H"); cleanup.Add(go);
            var v = go.AddComponent<CharacterVisual>();
            var src = new StubAnimSource();
            v.BuildModel(def, 1f, Greataxe()); v.Init(src);
            Time.captureFramerate = 60;
            for (int i = 0; i < 30; i++) yield return null;
            var rt = new RenderTexture(W, H, 24);
            float total = wind + act + rec + hold, holdAt = wind * PlayerCombat.ChargeHoldAt;
            int frames = Mathf.Max(N, Mathf.RoundToInt(total * 60f));
            int shot = 0;
            for (int f = 0; f <= frames; f++)
            {
                float time = f / 60f;
                float e = time < holdAt ? time : time < holdAt + hold ? holdAt : time - hold; // czas akcji (stoi w ładowaniu)
                var s = new CharacterAnimState { action = AnimAction.Attack, attack = attack, actionTime = e, actionDuration = wind + act + rec,
                    windupSeconds = wind, activeSeconds = act };
                if (e < wind) { s.phase = ActionPhase.Startup; s.phaseProgress = e / wind; }
                else if (e < wind + act) { s.phase = ActionPhase.Active; s.phaseProgress = (e - wind) / act; }
                else { s.phase = ActionPhase.Recovery; s.phaseProgress = Mathf.Clamp01((e - wind - act) / rec); }
                src.state = s;
                yield return null;
                float adv = v.Driver is ClipAnimationDriver d ? d.ConsumeAdvance() : 0f;
                if (carry > 0f && e < wind + act) adv += carry * (1f - e / (wind + act)) / 60f;
                go.transform.position += go.transform.forward * adv;
                if (f < frames * shot / (N - 1)) continue;
                Vector3 p = go.transform.position;
                for (int k = 0; k < 2; k++)
                {
                    cam.transform.position = p + (k == 0 ? new Vector3(3.6f, 1.1f, 0.6f) : new Vector3(-1.2f, 2.0f, -3.6f));
                    cam.transform.LookAt(p + Vector3.up * 0.9f);
                    cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                    sheet.ReadPixels(new Rect(0, 0, W, H), shot * W, (1 - k) * H);
                    RenderTexture.active = null; cam.targetTexture = null;
                }
                shot++;
                if (shot >= N) break;
            }
            sheet.Apply();
            Object.Destroy(rt);
            go.SetActive(false);
        }
    }
}
#endif
