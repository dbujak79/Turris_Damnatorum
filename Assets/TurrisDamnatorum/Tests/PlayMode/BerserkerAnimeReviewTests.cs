#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>
    /// Przegląd (opt-in, TURRIS_SHOT_DIR) wszystkich ruchów berserkera anime z rzeczywistym toporem i młotem:
    /// ruch, ataki z czasami gry, ładowanie, serie, przerwania unikiem, unik, blok, parowanie, trafienie, śmierć, skille.
    /// Każda sekwencja: arkusz klatek (bok / kamera gry zza pleców / przód) i CSV pozycji kości, dłoni i ostrza co klatkę.
    /// Test nie ocenia jakości – zbiera materiał do przeglądu i pomiarów (stan wyjściowy przed naprawą animacji).
    /// </summary>
    public class BerserkerAnimeReviewTests
    {
        const string Content = "Assets/TurrisDamnatorum/Content/";
        const float Dt = 1f / 60f;
        readonly List<Object> cleanup = new List<Object>();

        class Source : ICharacterAnimSource { public CharacterAnimState state; public CharacterAnimState GetAnimState() => state; }
        struct F { public CharacterAnimState s; public Vector3 move; public string tag; }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            yield return null;
        }

        static string N(float x) => x.ToString("0.#####", CultureInfo.InvariantCulture);
        static string V(Vector3 v) => N(v.x) + "," + N(v.y) + "," + N(v.z);

        // Tempo gry jak w GreatWeaponMovesetTests.Moveset: akcje ×1,1, odzyskanie ×0,8, +10% szybkości broni dwuręcznej.
        static float S(float x) => x / 1.1f / 1.1f;
        static float R(float x) => x * 0.8f / 1.1f / 1.1f;

        static List<F> Attack(AttackAnim anim, float wind, float act, float rec, float hold = 0f, float carry = 0f, float cutAt = -1f)
        {
            var list = new List<F>();
            float total = wind + act + rec + hold, holdAt = wind * PlayerCombat.ChargeHoldAt;
            int frames = Mathf.RoundToInt(total / Dt);
            for (int f = 0; f <= frames; f++)
            {
                float time = f * Dt;
                if (cutAt >= 0f && time > cutAt) break;
                float e = time < holdAt ? time : time < holdAt + hold ? holdAt : time - hold;
                var s = new CharacterAnimState { action = AnimAction.Attack, attack = anim, actionTime = e, actionDuration = wind + act + rec, windupSeconds = wind, activeSeconds = act };
                if (hold > 0f && time >= holdAt && time < holdAt + hold) s.charge = Mathf.Max(0.05f, (time - holdAt) / PlayerCombat.ChargeMax);   // jak PlayerCombat w ładowaniu
                if (e < wind) { s.phase = ActionPhase.Startup; s.phaseProgress = e / wind; }
                else if (e < wind + act) { s.phase = ActionPhase.Active; s.phaseProgress = (e - wind) / act; }
                else { s.phase = ActionPhase.Recovery; s.phaseProgress = Mathf.Clamp01((e - wind - act) / rec); }
                Vector3 move = carry > 0f && e < wind + act ? Vector3.forward * carry * (1f - e / (wind + act)) * Dt : Vector3.zero;
                list.Add(new F { s = s, move = move, tag = s.phase.ToString() });
            }
            return list;
        }

        /// <summary>Unik jak w grze: PlayerController przesuwa postać przez rollDuration z profilem prędkości 1−u².</summary>
        /// <summary>Szarża barkiem (berserker): ten sam ruch co unik, dystans ×0,805 (BuildSnapshot), wariant klipu z kierunku.</summary>
        static List<F> Charge(Vector3 dir)
        {
            var l = Dodge(dir, distance: 4.62f * 0.805f);
            var probe = new GameObject("kierunek"); var a = AnimResolve.ChargeFor(probe.transform, dir); Object.DestroyImmediate(probe);
            for (int i = 0; i < l.Count; i++) { var f = l[i]; f.s.action = a; f.tag = a.ToString(); l[i] = f; }
            return l;
        }

        static List<F> Dodge(Vector3 dir, float total = 0.82f, float roll = 0.6f, float distance = 4.62f)
        {
            var list = new List<F>();
            int frames = Mathf.RoundToInt(total / Dt);
            for (int f = 0; f <= frames; f++)
            {
                float t = f * Dt, u = Mathf.Clamp01(t / roll);
                float v = t < roll ? distance * 1.5f * (1f - u * u) / roll : 0f;
                var s = new CharacterAnimState { action = AnimAction.Dodge, phase = ActionPhase.Active, phaseProgress = t / total, actionTime = t, actionDuration = total, rollDuration = roll, dodgeDirection = dir };
                list.Add(new F { s = s, move = dir * v * Dt, tag = "Dodge" });
            }
            return list;
        }

        static List<F> Simple(AnimAction a, float total, Func<float, CharacterAnimState, CharacterAnimState> fill = null)
        {
            var list = new List<F>();
            int frames = Mathf.RoundToInt(total / Dt);
            for (int f = 0; f <= frames; f++)
            {
                float t = f * Dt;
                var s = new CharacterAnimState { action = a, actionTime = t, actionDuration = total, phase = ActionPhase.Active, phaseProgress = t / total };
                if (fill != null) s = fill(t, s);
                list.Add(new F { s = s, move = Vector3.zero, tag = a.ToString() });
            }
            return list;
        }

        static List<F> Move(Vector3 localVelocity, float seconds)
        {
            var list = new List<F>();
            int frames = Mathf.RoundToInt(seconds / Dt);
            for (int f = 0; f < frames; f++) list.Add(new F { s = default, move = localVelocity * Dt, tag = localVelocity.sqrMagnitude > 0 ? "Move" : "Idle" });
            return list;
        }

        static List<F> Cat(params List<F>[] parts) { var l = new List<F>(); foreach (var p in parts) l.AddRange(p); return l; }

        static List<F> Phased(AnimAction a, float wind, float act, float rec, Func<CharacterAnimState, CharacterAnimState> fill = null)
        {
            var list = new List<F>();
            float total = wind + act + rec;
            int frames = Mathf.RoundToInt(total / Dt);
            for (int f = 0; f <= frames; f++)
            {
                float t = f * Dt;
                var s = new CharacterAnimState { action = a, actionTime = t, actionDuration = total };
                if (t < wind) { s.phase = ActionPhase.Startup; s.phaseProgress = t / wind; }
                else if (t < wind + act) { s.phase = ActionPhase.Active; s.phaseProgress = (t - wind) / act; }
                else { s.phase = ActionPhase.Recovery; s.phaseProgress = Mathf.Clamp01((t - wind - act) / rec); }
                if (fill != null) s = fill(s);
                list.Add(new F { s = s, move = Vector3.zero, tag = s.phase.ToString() });
            }
            return list;
        }

        Camera cam;

        void Studio()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(200, 1, 200);
            ground.GetComponent<Renderer>().material.color = new Color(0.42f, 0.40f, 0.38f);
            cleanup.Add(ground);
            // Siatka metrowa na ziemi – ślizg stóp widać względem podłoża.
            for (int i = -40; i <= 40; i++)
                for (int k = 0; k < 2; k++)
                {
                    var line = GameObject.CreatePrimitive(PrimitiveType.Cube); cleanup.Add(line);
                    Object.Destroy(line.GetComponent<Collider>());
                    line.transform.position = k == 0 ? new Vector3(i, 0.001f, 0) : new Vector3(0, 0.001f, i);
                    line.transform.localScale = k == 0 ? new Vector3(0.015f, 0.002f, 80) : new Vector3(80, 0.002f, 0.015f);
                    line.GetComponent<Renderer>().material.color = new Color(0.30f, 0.29f, 0.28f);
                }
            var lightGo = new GameObject("L"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.25f;
            lightGo.transform.rotation = Quaternion.Euler(40, -45, 0); cleanup.Add(lightGo);
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.52f);
            var camGo = new GameObject("Cam"); cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.backgroundColor = new Color(0.55f, 0.42f, 0.36f); cam.clearFlags = CameraClearFlags.SolidColor; cam.enabled = false;
        }

        /// <summary>0 – bok (prawa strona postaci), 1 – kamera gry (CameraRig: pivot 1,65 m, 4,6 m za plecami, pochylenie 12°), 2 – przód.</summary>
        void Aim(int view, Vector3 p)
        {
            if (view == 1)
            {
                cam.fieldOfView = 55f;
                Vector3 pivot = p + Vector3.up * 1.65f;
                var rot = Quaternion.Euler(12f, 0f, 0f);
                cam.transform.SetPositionAndRotation(pivot + rot * new Vector3(0f, 0f, -4.6f), rot);
                return;
            }
            cam.fieldOfView = 34f;
            cam.transform.position = p + (view == 0 ? new Vector3(4.2f, 1.1f, 0.4f) : new Vector3(0.6f, 1.2f, 4.2f));
            cam.transform.LookAt(p + Vector3.up * 0.9f);
        }

        IEnumerator Run(CharacterVisualDefinition def, RigLook look, string dir, string name, List<F> frames, StringBuilder index)
        {
            var go = new GameObject("Berserker " + name); cleanup.Add(go);
            var v = go.AddComponent<CharacterVisual>(); v.enabled = false;
            var src = new Source();
            v.BuildModel(def, 1f, look); v.Init(src);
            var drv = v.Driver as ClipAnimationDriver;
            Assert.IsNotNull(drv, "ClipAnimationDriver");
            var anim = v.Model.GetComponentInChildren<Animator>();
            Transform B(HumanBodyBones b) => anim.GetBoneTransform(b);
            var hips = B(HumanBodyBones.Hips); var lh = B(HumanBodyBones.LeftHand); var rh = B(HumanBodyBones.RightHand);
            var lf = B(HumanBodyBones.LeftFoot); var rf = B(HumanBodyBones.RightFoot); var lt = B(HumanBodyBones.LeftToes); var rt = B(HumanBodyBones.RightToes);
            var head = B(HumanBodyBones.Head);
            var all = v.Model.GetComponentsInChildren<Transform>(true);
            var slotL = all.FirstOrDefault(t => t.name == "HandSlot.L"); var slotR = all.FirstOrDefault(t => t.name == "HandSlot.R");

            // Kierunek głowicy broni w przestrzeni gniazda (oś +Y gniazda – trzonek): wierzchołki z ostatniej ćwiartki długości,
            // główna oś rozrzutu w płaszczyźnie XZ gniazda, zwrot ku dalszemu krańcowi. Do kalibracji orientacji broni w Blenderze.
            Transform sock = drv.GreatSwing != null ? drv.GreatSwing.Weapon : null;
            Vector3 bladeLocal = Vector3.zero;
            if (sock != null)
            {
                var pts = new List<Vector3>();
                foreach (var mf in sock.GetComponentsInChildren<MeshFilter>())
                {
                    if (mf.sharedMesh == null) continue;
                    using (var data = UnityEditor.MeshUtility.AcquireReadOnlyMeshData(mf.sharedMesh))
                    using (var verts = new Unity.Collections.NativeArray<Vector3>(mf.sharedMesh.vertexCount, Unity.Collections.Allocator.Temp))
                    {
                        data[0].GetVertices(verts);
                        foreach (var q in verts) pts.Add(sock.InverseTransformPoint(mf.transform.TransformPoint(q)));
                    }
                }
                if (pts.Count > 0)
                {
                    float maxY = pts.Max(q => q.y);
                    var headPts = pts.Where(q => q.y > maxY * 0.75f).ToList();
                    float mx = headPts.Average(q => q.x), mz = headPts.Average(q => q.z), cxx = 0, cxz = 0, czz = 0;
                    foreach (var q in headPts) { float x = q.x - mx, z = q.z - mz; cxx += x * x; cxz += x * z; czz += z * z; }
                    float ang = 0.5f * Mathf.Atan2(2 * cxz, cxx - czz);
                    bladeLocal = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
                    float far = headPts.Max(q => Vector3.Dot(q, bladeLocal)), near = -headPts.Min(q => Vector3.Dot(q, bladeLocal));
                    if (near > far) bladeLocal = -bladeLocal;
                    Debug.Log($"DETAIL głowica {name}: {headPts.Count}/{pts.Count} wierzch., oś {bladeLocal}, zasięg +{Mathf.Max(far, near):F3} / −{Mathf.Min(far, near):F3} m, długość {maxY:F3} m");
                }
                else Debug.Log("DETAIL głowica: siatka broni nieczytelna (isReadable=false)");
            }
            for (int i = 0; i < 60; i++) { src.state = default; v.Tick(Dt); go.transform.position += go.transform.forward * drv.ConsumeAdvance(); }
            go.transform.position = Vector3.zero; go.transform.rotation = Quaternion.identity;
            yield return null;

            const int W = 180, H = 240;
            int Ncols = int.TryParse(Environment.GetEnvironmentVariable("TURRIS_REVIEW_COLS"), out int nc) ? Mathf.Clamp(nc, 2, 40) : 12;
            var sheet = new Texture2D(W * Ncols, H * 3, TextureFormat.RGB24, false);
            var rtex = new RenderTexture(W, H, 24);
            var csv = new StringBuilder("frame,time,tag,action,attack,phase,progress,clip,root,,,hips,,,head,,,lhand,,,rhand,,,slotL,,,slotR,,,tip,,,lfoot,,,rfoot,,,ltoe,,,rtoe,,,blade,,,sockUp,,,weaponLowest,dropped\n");
            int shot = 0;
            var shotFrames = new List<int>();
            for (int f = 0; f < frames.Count; f++)
            {
                var fr = frames[f];
                src.state = fr.s;
                go.transform.position += go.transform.TransformDirection(fr.move);
                v.Tick(Dt);
                go.transform.position += go.transform.forward * drv.ConsumeAdvance();
                var clip = drv.CurrentActionClip != null && drv.CurrentActionClip.clip != null ? drv.CurrentActionClip.clip.name : "";
                csv.Append(f).Append(',').Append(N(f * Dt)).Append(',').Append(fr.tag).Append(',').Append(fr.s.action).Append(',').Append(fr.s.attack).Append(',')
                   .Append(fr.s.phase).Append(',').Append(N(fr.s.phaseProgress)).Append(',').Append(clip.Replace(',', ' ')).Append(',')
                   .Append(V(go.transform.position)).Append(',').Append(V(hips.position)).Append(',').Append(V(head.position)).Append(',')
                   .Append(V(lh.position)).Append(',').Append(V(rh.position)).Append(',')
                   .Append(V(slotL != null ? slotL.position : lh.position)).Append(',').Append(V(slotR != null ? slotR.position : rh.position)).Append(',')
                   .Append(V(v.WeaponTip)).Append(',').Append(V(lf.position)).Append(',').Append(V(rf.position)).Append(',')
                   .Append(V(lt != null ? lt.position : lf.position)).Append(',').Append(V(rt != null ? rt.position : rf.position)).Append(',')
                   .Append(V(sock != null ? sock.TransformDirection(bladeLocal) : Vector3.zero)).Append(',').Append(V(sock != null ? sock.up : Vector3.zero)).Append(',')
                   .Append(N(WeaponLowest(v))).Append(',').Append(v.WeaponDropped ? 1 : 0).Append('\n');
                if (shot >= Ncols || f < (frames.Count - 1) * shot / (Ncols - 1)) continue;
                yield return null;
                Vector3 p = go.transform.position;
                for (int k = 0; k < 3; k++)
                {
                    Aim(k, k == 1 ? p : new Vector3(p.x, 0f, p.z));
                    cam.targetTexture = rtex; cam.Render(); RenderTexture.active = rtex;
                    sheet.ReadPixels(new Rect(0, 0, W, H), shot * W, (2 - k) * H);
                    RenderTexture.active = null; cam.targetTexture = null;
                }
                shotFrames.Add(f);
                shot++;
            }
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(dir, name + ".png"), sheet.EncodeToPNG());
            File.WriteAllText(Path.Combine(dir, name + ".csv"), csv.ToString());
            index.AppendLine(name + ": " + frames.Count + " klatek (" + N(frames.Count * Dt) + " s), zrzuty: " + string.Join(" ", shotFrames));
            Object.Destroy(sheet); Object.Destroy(rtex);
            Object.Destroy(go);
            yield return null;
        }

        /// <summary>Najniższy punkt modelu broni (m) – także po wypadnięciu z dłoni przy śmierci (renderery poza siatką postaci).</summary>
        static float WeaponLowest(CharacterVisual v)
        {
            float y = float.MaxValue;
            var skin = v.Model != null ? v.Model.transform : null;
            foreach (var r in v.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled || r is SkinnedMeshRenderer || r is LineRenderer || r is TrailRenderer || r is ParticleSystemRenderer) continue;
                y = Mathf.Min(y, r.bounds.min.y);
            }
            return y == float.MaxValue ? float.NaN : y;
        }

        [UnityTest, Timeout(3600000)]
        public IEnumerator Berserker_PrzegladWszystkichRuchow_WhenRequested()
        {
            string root = Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(root)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR."); yield break; }
            string only = Environment.GetEnvironmentVariable("TURRIS_REVIEW_ONLY");
            Studio();
            var cls = AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content + "Classes/class_hu_berserker.asset");
            Assert.IsNotNull(cls); Assert.IsNotNull(cls.Visual);
            Assert.AreEqual("visual_berserker_anime", cls.Visual.name, "Aktywny wygląd berserkera");
            // TURRIS_REVIEW_AUTHORED="Cleave,CleaveBack" – na czas testu klipy tych ciosów jako autorskie (bez profilu GreatSwingPose),
            // żeby obejrzeć surowe nagranie z toporem. Asset nie jest zapisywany; flaga wraca w finally.
            var restore = new List<(ActionClip clip, bool value)>();
            string authored = Environment.GetEnvironmentVariable("TURRIS_REVIEW_AUTHORED");
            if (!string.IsNullOrEmpty(authored))
                foreach (var name in authored.Split(','))
                    if (Enum.TryParse<AttackAnim>(name.Trim(), out var a))
                        foreach (var set in new[] { cls.Visual.twoHandSet })
                            if (set != null) foreach (var ac in set.actions) if (ac.action == AnimAction.Attack && ac.attack == a) { restore.Add((ac, ac.authoredSwing)); ac.authoredSwing = true; }
            try
            {

            for (int kit = 0; kit < cls.loadouts.Count; kit++)
            {
                var k = cls.loadouts[kit];
                var look = new RigLook { weapon = AnimResolve.ForItem(k.weapon), weaponItem = k.weapon, offhandItem = k.offHand, shield = AnimResolve.ForShield(k.offHand) };
                string wname = k.weapon != null ? k.weapon.id : "kit" + kit;
                string dir = Path.Combine(root, wname); Directory.CreateDirectory(dir);
                var L = k.weapon.weapon.light; var Hv = k.weapon.weapon.heavy;
                var probe = new GameObject("probe"); var pv = probe.AddComponent<CharacterVisual>(); pv.enabled = false; pv.BuildModel(cls.Visual, 1f, look); pv.Init(new Source());
                float walk = pv.ModelSet != null ? pv.ModelSet.walkSpeed : cls.Visual.walkSpeed, run = pv.ModelSet != null ? pv.ModelSet.runSpeed : cls.Visual.runSpeed;
                Object.Destroy(probe); yield return null;

                List<F> Lt(AttackAnim a, float cut = -1f, float carry = 0f) => Attack(a, S(L.windup), S(L.active), R(L.recovery), 0f, carry, cut);
                List<F> Hy(AttackAnim a, float hold = 0f, float cut = -1f, float carry = 0f, float windMul = 1f) => Attack(a, S(Hv.windup) * windMul, S(Hv.active), R(Hv.recovery), hold, carry, cut);
                float lWindAct = S(L.windup) + S(L.active), hWind = S(Hv.windup);
                float lTotal = S(L.windup) + S(L.active) + R(L.recovery), hTotal = hWind + S(Hv.active) + R(Hv.recovery);
                Vector3 back = Vector3.back, left = Vector3.left, right = Vector3.right, fwd = Vector3.forward;

                var seqs = new List<(string name, List<F> frames)>
                {
                    ("loco_idle", Move(Vector3.zero, 2.5f)),
                    ("loco_walk_fwd", Move(fwd * walk, 2f)),
                    ("loco_run_fwd", Move(fwd * run, 2f)),
                    ("loco_run_back", Move(back * run, 2f)),
                    ("loco_run_right", Move(right * run, 2f)),
                    ("loco_start_stop", Cat(Move(Vector3.zero, 0.4f), Move(fwd * run, 1.2f), Move(Vector3.zero, 1.0f))),
                    ("atk_light1", Lt(AttackAnim.Cleave)),
                    ("atk_light2", Lt(AttackAnim.CleaveBack)),
                    ("atk_combo_light", Cat(Lt(AttackAnim.Cleave, lWindAct + R(L.recovery) * 0.45f), Lt(AttackAnim.CleaveBack), Lt(AttackAnim.Cleave))),
                    ("atk_heavy1", Hy(AttackAnim.Chop)),
                    ("atk_heavy2", Hy(AttackAnim.ChopFollow)),
                    ("atk_combo_heavy", Cat(Hy(AttackAnim.Chop, cut: hTotal * 0.75f), Hy(AttackAnim.ChopFollow))),
                    ("atk_charged_max", Hy(AttackAnim.Chop, PlayerCombat.ChargeMax)),
                    ("atk_run_light", Cat(Move(fwd * run, 0.8f), Lt(AttackAnim.RunningSweep, carry: PlayerCombat.RunningLightCarry))),
                    ("atk_run_heavy", Cat(Move(fwd * run, 0.8f), Hy(AttackAnim.LeapChop, carry: PlayerCombat.RunningHeavyCarry))),
                    ("atk_dodge_chop", Cat(Dodge(fwd), Attack(AttackAnim.DodgeChop, S(L.windup) * PlayerCombat.DodgeChopWindup, S(L.active), R(L.recovery)))),
                    ("skill_slash", Lt(AttackAnim.SlashRight)),
                    ("skill_thrust", Lt(AttackAnim.Thrust)),
                    ("skill_burst", Lt(AttackAnim.Burst)),
                    ("charge_fwd", Cat(Charge(fwd), Move(Vector3.zero, 0.4f))),
                    ("charge_back", Cat(Charge(back), Move(Vector3.zero, 0.4f))),
                    ("charge_left", Cat(Charge(left), Move(Vector3.zero, 0.4f))),
                    ("charge_right", Cat(Charge(right), Move(Vector3.zero, 0.4f))),
                    ("cancel_light_active_charge_left", Cat(Lt(AttackAnim.Cleave, S(L.windup) + S(L.active) * 0.5f), Charge(left), Move(Vector3.zero, 0.3f))),
                    ("cancel_charge_hold_charge_back", Cat(Hy(AttackAnim.Chop, PlayerCombat.ChargeMax, cut: hWind * PlayerCombat.ChargeHoldAt + 0.4f), Charge(back), Move(Vector3.zero, 0.3f))),
                    ("cancel_heavy_recovery_charge_fwd", Cat(Hy(AttackAnim.Chop, cut: hWind + S(Hv.active) + R(Hv.recovery) * 0.3f), Charge(fwd), Move(Vector3.zero, 0.3f))),
                    ("dodge_fwd", Cat(Dodge(fwd), Move(Vector3.zero, 0.4f))),
                    ("dodge_back", Cat(Dodge(back), Move(Vector3.zero, 0.4f))),
                    ("dodge_left", Cat(Dodge(left), Move(Vector3.zero, 0.4f))),
                    ("dodge_right", Cat(Dodge(right), Move(Vector3.zero, 0.4f))),
                    ("cancel_light_active_dodge_left", Cat(Lt(AttackAnim.Cleave, S(L.windup) + S(L.active) * 0.5f), Dodge(left), Move(Vector3.zero, 0.3f))),
                    ("cancel_heavy_windup_dodge_back", Cat(Hy(AttackAnim.Chop, cut: hWind * 0.5f), Dodge(back), Move(Vector3.zero, 0.3f))),
                    ("cancel_charge_hold_dodge_right", Cat(Hy(AttackAnim.Chop, PlayerCombat.ChargeMax, cut: hWind * PlayerCombat.ChargeHoldAt + 0.4f), Dodge(right), Move(Vector3.zero, 0.3f))),
                    ("cancel_heavy_recovery_dodge_fwd", Cat(Hy(AttackAnim.Chop, cut: hWind + S(Hv.active) + R(Hv.recovery) * 0.3f), Dodge(fwd), Move(Vector3.zero, 0.3f))),
                    ("block", Simple(AnimAction.Block, 1.5f, (t, s) => { s.weaponGuard = true; return s; })),
                    ("parry", Phased(AnimAction.Parry, 0.12f, 0.2f, 0.35f, s => { s.weaponGuard = true; return s; })),
                    ("flinch", Simple(AnimAction.Flinch, 0.35f, (t, s) => { s.hitDirection = back; s.hitWeight = 0.6f; return s; })),
                    ("death", Simple(AnimAction.Death, 3f, (t, s) => { s.hitDirection = back; s.hitWeight = 1f; return s; })),
                    // hitDirection – od napastnika do postaci: back = cios z przodu, fwd = cios z tyłu; siła jak w PlayerCombat (0,35 / 0,7)
                    ("hit_front_light", Cat(Simple(AnimAction.Flinch, 0.35f, (t, s) => { s.hitDirection = back; s.hitWeight = 0.35f; return s; }), Move(Vector3.zero, 0.4f))),
                    ("hit_front_heavy", Cat(Simple(AnimAction.Flinch, 0.35f, (t, s) => { s.hitDirection = back; s.hitWeight = 0.7f; return s; }), Move(Vector3.zero, 0.4f))),
                    ("hit_back_light", Cat(Simple(AnimAction.Flinch, 0.35f, (t, s) => { s.hitDirection = fwd; s.hitWeight = 0.35f; return s; }), Move(Vector3.zero, 0.4f))),
                    ("hit_back_heavy", Cat(Simple(AnimAction.Flinch, 0.35f, (t, s) => { s.hitDirection = fwd; s.hitWeight = 0.7f; return s; }), Move(Vector3.zero, 0.4f))),
                    ("death_front", Simple(AnimAction.Death, 3f, (t, s) => { s.hitDirection = back; s.hitWeight = 0.7f; return s; })),
                    ("death_back", Simple(AnimAction.Death, 3f, (t, s) => { s.hitDirection = fwd; s.hitWeight = 0.7f; return s; })),
                };
                var index = new StringBuilder();
                index.AppendLine($"Wygląd {cls.Visual.name}, broń {wname}, chód {N(walk)} m/s, bieg {N(run)} m/s");
                index.AppendLine($"Lekki: zamach {N(S(L.windup))} trafienie {N(S(L.active))} powrót {N(R(L.recovery))}; ciężki: {N(hWind)} {N(S(Hv.active))} {N(R(Hv.recovery))}");
                foreach (var sq in seqs)
                {
                    if (!string.IsNullOrEmpty(only) && !only.Split(',').Any(o => sq.name.Contains(o.Trim()))) continue;
                    // Młot: tylko ataki i przerwania (ruch, unik, blok – wspólne z toporem).
                    if (kit > 0 && !(sq.name.StartsWith("atk_") || sq.name.StartsWith("skill_") || sq.name.StartsWith("cancel_") || sq.name.StartsWith("charge_") || sq.name == "loco_idle" || sq.name == "loco_run_fwd")) continue;
                    yield return Run(cls.Visual, look, dir, sq.name, sq.frames, index);
                }
                File.WriteAllText(Path.Combine(dir, "index.txt"), index.ToString());
                Debug.Log("DETAIL review " + wname + "\n" + index);
            }
            }
            finally { foreach (var r in restore) r.clip.authoredSwing = r.value; }
        }
    }
}
#endif
