#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>Nowa siatka paladyna: prawdziwy start gry oraz geometria w istniejących animacjach.</summary>
    public class PaladinAnimePlayTests
    {
        const string Content = "Assets/TurrisDamnatorum/Content/";
        const string VisualPath = Content + "Visuals/visual_paladin_anime.asset";
        readonly List<Object> cleanup = new List<Object>();
        string profile;
        Gamepad testPad;

        [SetUp] public void Setup()
        {
            profile = "turris_anime_test_" + System.Guid.NewGuid().ToString("N") + ".json";
            GameRoot.ProfileFileOverride = profile;
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var obj in cleanup) if (obj != null) Object.Destroy(obj);
            cleanup.Clear();
            if (testPad != null) InputSystem.RemoveDevice(testPad);
            testPad = null;
            Time.captureFramerate = 0;
            GameRoot.ProfileFileOverride = null;
            string path = Path.Combine(Application.persistentDataPath, profile);
            if (File.Exists(path)) File.Delete(path);
            var main = SceneManager.GetSceneByName("Main");
            if (main.IsValid() && main.isLoaded)
            {
                var empty = SceneManager.CreateScene("AfterAnimeTest");
                SceneManager.SetActiveScene(empty);
                yield return SceneManager.UnloadSceneAsync(main);
            }
            yield return null;
        }

        [UnityTest] public IEnumerator Paladin_StartsRunWithNewLook_WarriorKeepsOriginal()
        {
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            var paladin = root.config.classes.First(c => c.id == "class_hu_paladin");
            var warrior = root.config.classes.First(c => c.id == "class_knight");
            var definition = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            Assert.AreSame(definition, paladin.Visual);
            Assert.AreEqual("visual_hero_warrior", warrior.Visual.name);
            Assert.IsNull(warrior.lookOverride, "Nie podmieniamy wyglądu wojownika");
            Assert.IsTrue(root.TryStartRun(new LoadoutPlan { classDef = paladin, difficulty = root.config.difficulties[0] }, out var reason), reason);
            yield return null; yield return null;
            var visual = root.Player.GetComponent<CharacterVisual>();
            Assert.IsTrue(visual.UsesModel);
            Assert.IsNotNull(Util.FindDeep(visual.Model.transform, "HandSlot.R"));
            var animator = visual.Model.GetComponentInChildren<Animator>();
            Assert.IsTrue(animator.isHuman && animator.avatar.isValid);
            Assert.AreEqual(definition.modelPrefab.GetComponentInChildren<Animator>().avatar, animator.avatar);
            Assert.IsNotNull(visual.RightHand);
            Assert.IsNotNull(visual.ModelSet);
            foreach (var enemy in root.Enemies) if (enemy != null) enemy.gameObject.SetActive(false);
            for (int i = 0; i < 30; i++) yield return null;
            var dir = OutputDirectory();
            if (dir != null)
            {
                var camera = Camera.main;
                Assert.IsNotNull(camera);
                Shot(camera, Path.Combine(dir, "paladin-game.png"));
            }
            // Ten sam model w normalnej pętli gry, z ruchem sterowanym wirtualnym padem.
            testPad = InputSystem.AddDevice<Gamepad>("PaladinAnimeGamepad");
            Time.captureFramerate = 60;
            var feet = new[] { animator.GetBoneTransform(HumanBodyBones.LeftFoot), animator.GetBoneTransform(HumanBodyBones.RightFoot) };
            var lastFeet = feet.Select(foot => foot.position).ToArray();
            var plantedSteps = new List<float>();
            for (int frame = 0; frame < 96; frame++)
            {
                InputSystem.QueueStateEvent(testPad, new GamepadState { leftStick = Vector2.up });
                yield return null;
                for (int side = 0; side < 2; side++)
                {
                    Vector3 delta = feet[side].position - lastFeet[side]; delta.y = 0;
                    if (frame > 24 && feet[side].position.y - root.Player.transform.position.y < 0.16f)
                        plantedSteps.Add(delta.magnitude);
                    lastFeet[side] = feet[side].position;
                }
            }
            Assert.Greater(root.Player.GetComponent<CharacterController>().velocity.magnitude, 3f, "Paladyn rzeczywiście biegnie w scenie gry");
            Assert.IsNotEmpty(plantedSteps, "Rzeczywisty bieg zawiera fazy podparcia");
            plantedSteps.Sort();
            float plantedP95 = plantedSteps[(int)((plantedSteps.Count - 1) * 0.95f)];
            Debug.Log($"Paladyn w grze: przesunięcie stopy podporowej na klatkę p95 {plantedP95 * 100:F2} cm");
            Assert.Less(plantedP95, 0.05f, "Korekta anatomiczna nie wprowadza ślizgania stopy po ziemi");
            if (dir != null) Shot(Camera.main, Path.Combine(dir, "paladin-game-run.png"));
            InputSystem.QueueStateEvent(testPad, new GamepadState());
            for (int i = 0; i < 45; i++) yield return null;
            var pc = root.Player;
            var pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
            float standing = pelvis.position.y - pc.transform.position.y, comboLow = standing;
            var requested = new bool[3]; requested[0] = true;
            bool thirdHit = false;
            var liveRates = new List<float>();
            var liveDriver = (ClipAnimationDriver)visual.Driver;
            var previousState = pc.GetAnimState();
            var liveHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var liveChest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            Vector3 previousHand = pc.transform.InverseTransformPoint(liveHand.position);
            Quaternion previousChest = liveChest.rotation;
            float livePreparationStep = 0, livePreparationTurn = 0;
            float firstLead=float.NegativeInfinity,secondLead=float.NegativeInfinity;
            bool previousActive = false;
            float previousVisualTime = liveDriver.CurrentActionTime;
            float previousActionElapsed = previousState.actionTime;
            pc.Request(ActionType.LightAttack);
            for (int frame = 0; frame < 420; frame++)
            {
                foreach (var enemy in root.Enemies) if (enemy != null) enemy.gameObject.SetActive(false);
                yield return null;
                Vector3 handNow = pc.transform.InverseTransformPoint(liveHand.position);
                if (previousState.action == AnimAction.Attack && previousState.attack == AttackAnim.SlashLeft && previousState.phase == ActionPhase.Startup)
                {
                    livePreparationStep = Mathf.Max(livePreparationStep,Vector3.Distance(handNow,previousHand));
                    livePreparationTurn = Mathf.Max(livePreparationTurn,Quaternion.Angle(liveChest.rotation,previousChest));
                }
                previousHand = handNow; previousChest = liveChest.rotation;
                bool leftActive = previousState.action == AnimAction.Attack && previousState.attack == AttackAnim.SlashLeft && previousState.phase == ActionPhase.Active;
                if (leftActive && previousActive && liveDriver.CurrentActionClip != null
                    && liveDriver.CurrentActionTime < liveDriver.CurrentActionClip.clip.length-0.0001f
                    && previousState.actionTime > previousActionElapsed+0.00001f)
                    liveRates.Add((liveDriver.CurrentActionTime-previousVisualTime)/(previousState.actionTime-previousActionElapsed));
                previousActive = leftActive; previousVisualTime = liveDriver.CurrentActionTime;
                previousActionElapsed = previousState.actionTime; previousState = pc.GetAnimState();
                if (pc.Actions.Current == ActionType.LightAttack)
                {
                    if(previousState.phase==ActionPhase.Recovery && previousState.phaseProgress>.15f)
                    {
                        float lead=Vector3.Dot(feet[1].position-feet[0].position,pc.transform.forward);
                        if(pc.Actions.ComboIndex==0) firstLead=Mathf.Max(firstLead,lead);
                        if(pc.Actions.ComboIndex==1) secondLead=Mathf.Max(secondLead,-lead);
                    }
                    comboLow = Mathf.Min(comboLow,pelvis.position.y-pc.transform.position.y);
                    int next = pc.Actions.ComboIndex+1;
                    if (pc.Actions.ComboIndex == 2) thirdHit = true;
                    if (next < 3 && !requested[next] && pc.Actions.PastCancelPoint)
                    {
                        requested[next] = true; pc.Request(ActionType.LightAttack);
                    }
                }
                else if (thirdHit) break;
            }
            Debug.Log($"Paladyn w grze, seria korbacza: biodra {standing:F3} → min {comboLow:F3} m");
            Assert.IsTrue(thirdHit,"Rzeczywisty system walki wykonał trzy ciosy serii");
            Assert.Greater(firstLead,.08f,"W grze pierwszy cios kończy się prawą nogą z przodu");
            Assert.Greater(secondLead,.08f,"W grze drugi cios kończy się lewą nogą z przodu");
            Assert.Greater(comboLow,standing-0.18f,"Seria w scenie gry nie przechodzi w przysiad");
            Assert.IsNotEmpty(liveRates,"Zmierzono tempo drugiego ciosu w prawdziwej walce");
            Debug.Log($"Paladyn korbacz w grze: tempo drugiego ciosu {liveRates.Average():F2}x nagrania");
            Assert.Greater(liveRates.Min(),0.95f,"Drugi cios korbacza w grze nie zwalnia względem nagrania");
            Debug.Log($"Korbacz w Main: przygotowanie drugiego ciosu, dłoń max {livePreparationStep*100:F2} cm/kl., barki {livePreparationTurn:F2}°/kl.");
            Assert.Less(livePreparationStep,0.13f,"Przygotowanie drugiego ciosu w grze nie szarpie dłonią");
            // Main obejmuje całą zapowiedź, w tym początek cięcia i dodatkową pracę BodyDynamics.
            // Osobna próba przejścia (pierwsze 0,3 s) ma ciaśniejszą granicę 10°/kl.
            Assert.Less(livePreparationTurn,12f,"Przygotowanie drugiego ciosu w grze nie przeskakuje w barkach");
        }

        static string OutputDirectory()
        {
            const string dir = "TestResults/paladin-anime";
            return Directory.Exists(dir) ? dir : null;
        }

        [UnityTest] public IEnumerator Flail_HeadHangsAndMovesIndependentlyDuringSwing()
        {
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            var cls = AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content+"Classes/class_hu_paladin.asset");
            var kit = cls.loadouts[0];
            var go = new GameObject("Paladyn_LuznyKorbacz"); cleanup.Add(go);
            var visual = go.AddComponent<CharacterVisual>(); visual.enabled = false;
            var source = new StubAnimSource();
            visual.BuildModel(def,1,new RigLook { weapon = AnimResolve.ForItem(kit.weapon),weaponItem = kit.weapon,
                shield = ShieldModel.Heater,offhandItem = kit.offHand }); visual.Init(source);
            var gear = Util.FindDeep(visual.Model.transform,kit.weapon.id);
            Assert.IsNotNull(gear);
            var anchor = Util.FindDeep(gear,"chain0"); var head = Util.FindDeep(gear,"head");
            Assert.IsNotNull(anchor); Assert.IsNotNull(head);
            for (int i = 0; i < 240; i++) visual.Tick(1f/60);
            Assert.Less(Vector3.Angle(head.position-anchor.position,Vector3.down),12f);
            Vector3 hanging = gear.InverseTransformPoint(head.position);
            float motion = 0;
            var cameraGo = new GameObject("KameraKorbacza"); cleanup.Add(cameraGo);
            var camera = cameraGo.AddComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.13f,0.15f,0.18f); camera.fieldOfView = 35;
            camera.transform.position = new Vector3(2.3f,1.5f,3.3f); camera.transform.LookAt(Vector3.up*0.95f);
            var lightGo = new GameObject("SwiatloKorbacza"); cleanup.Add(lightGo);
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.4f;
            lightGo.transform.rotation = Quaternion.Euler(40,-35,0);
            var sheet = new Texture2D(1400,1000,TextureFormat.RGB24,false); cleanup.Add(sheet);
            for (int frame = 0; frame < 96; frame++)
            {
                source.state = frame < 18 ? new CharacterAnimState { action=AnimAction.Attack,attack=AttackAnim.SlashRight,phase=ActionPhase.Startup,phaseProgress=frame/18f }
                    : frame < 26 ? new CharacterAnimState { action=AnimAction.Attack,attack=AttackAnim.SlashRight,phase=ActionPhase.Active,phaseProgress=(frame-18)/8f }
                    : frame < 44 ? new CharacterAnimState { action=AnimAction.Attack,attack=AttackAnim.SlashRight,phase=ActionPhase.Recovery,phaseProgress=(frame-26)/18f }
                    : default;
                visual.Tick(1f/60);
                motion = Mathf.Max(motion,Vector3.Distance(hanging,gear.InverseTransformPoint(head.position)));
                Assert.Less(Vector3.Distance(head.position,visual.WeaponTip),0.17f,"Czubek broni podąża za kulą");
                if (frame%12 == 0 && OutputDirectory()!=null)
                {
                    yield return null; yield return null;
                    CycleTile(camera,sheet,frame/12);
                }
            }
            Vector3 paused = head.position; visual.Tick(0);
            Assert.Less(Vector3.Distance(paused,head.position),0.00001f);
            Assert.Greater(motion,0.15f,"Kula zmienia położenie względem trzonka, także podczas wyhamowania");
            Debug.Log($"Korbacz: zakres niezależnego ruchu kuli {motion:F3} m");
            if (OutputDirectory()!=null)
            {
                sheet.Apply(); File.WriteAllBytes(Path.Combine(OutputDirectory(),"flail-chain-cycle.png"),sheet.EncodeToPNG());
            }
        }

        [UnityTest] public IEnumerator OneHandCombo_DoesNotDropIntoCrouch() => StandingCombo(WeaponModel.Sword, "combo-onehand");
        [UnityTest] public IEnumerator TwoHandCombo_DoesNotDropIntoCrouch() => StandingCombo(WeaponModel.GreatSword, "combo-twohand");

        IEnumerator StandingCombo(WeaponModel weapon, string prefix)
        {
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            var go = new GameObject("PaladynAnime_Seria"); cleanup.Add(go);
            var visual = go.AddComponent<CharacterVisual>(); visual.enabled = false;
            var source = new StubAnimSource();
            visual.BuildModel(def, 1, new RigLook { weapon = weapon, shield = weapon == WeaponModel.Sword ? ShieldModel.Heater : ShieldModel.None });
            visual.Init(source);
            var animator = visual.Model.GetComponentInChildren<Animator>();
            var driver = (ClipAnimationDriver)visual.Driver;
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            var feet = new[] { animator.GetBoneTransform(HumanBodyBones.LeftFoot), animator.GetBoneTransform(HumanBodyBones.RightFoot) };
            for (int i = 0; i < 60; i++) visual.Tick(1f/60);
            float reference = hips.position.y - go.transform.position.y, lowest = reference, worstJump = 0, retreat = 0;
            float previousHeight = reference;
            float lateHandAboveShoulder = -10f;
            float strikeHandHeight = -10f;
            var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var shoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            float standingShoulderHeight = shoulder.position.y-go.transform.position.y;
            var previousFeet = feet.Select(f => f.position).ToArray();
            var previousGround = feet.Select(f => f.position.y - go.transform.position.y < 0.16f).ToArray();
            var cameraGo = new GameObject("KameraSerii"); cleanup.Add(cameraGo);
            var camera = cameraGo.AddComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.13f,0.15f,0.18f); camera.fieldOfView = 35;
            var lightGo = new GameObject("SwiatloSerii"); cleanup.Add(lightGo);
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.4f;
            lightGo.transform.rotation = Quaternion.Euler(40,-35,0);
            var sheet = new Texture2D(1400,1000,TextureFormat.RGB24,false); cleanup.Add(sheet);
            var report = new System.Text.StringBuilder("klatka\tcios\tfaza\tpostep\tbiodra\tLz\tLy\tPz\tPy\tdlonY\tbarkY\tczasKlipu\n");
            int frame = 0, tile = 0;
            var cutRates = new List<float>();
            float previousClipTime = 0;
            var attacks = new[] { AttackAnim.SlashRight, AttackAnim.SlashLeft, AttackAnim.Thrust, AttackAnim.SlashLeft, AttackAnim.SlashLeft };
            for (int hit = 0; hit < attacks.Length; hit++)
                for (int phase = 0; phase < 3; phase++)
                {
                    int count = phase == 0 ? 18 : phase == 1 ? 8 : hit == attacks.Length - 1 ? 18 : 8;
                    for (int i = 1; i <= count; i++)
                    {
                        source.state = new CharacterAnimState { action = AnimAction.Attack, attack = attacks[hit], phase = (ActionPhase)phase,
                            phaseProgress = i / (float)(phase == 2 ? 18 : count), windupSeconds = 18f/60,activeSeconds = 8f/60,actionDuration = 44f/60 };
                        visual.Tick(1f/60);
                        if (attacks[hit] == AttackAnim.SlashLeft && phase == 1 && i > 1
                            && driver.CurrentActionTime < driver.CurrentActionClip.clip.length-0.0001f)
                            cutRates.Add((driver.CurrentActionTime - previousClipTime)*60f);
                        previousClipTime = driver.CurrentActionTime;
                        if (attacks[hit] == AttackAnim.SlashLeft && phase > 0)
                            strikeHandHeight = Mathf.Max(strikeHandHeight,hand.position.y-go.transform.position.y);
                        if (attacks[hit] == AttackAnim.SlashLeft && phase == 2 && source.state.phaseProgress >= 0.8f)
                            lateHandAboveShoulder = Mathf.Max(lateHandAboveShoulder,hand.position.y-shoulder.position.y);
                        go.transform.position += go.transform.forward * driver.ConsumeAdvance();
                        float height = hips.position.y - go.transform.position.y;
                        lowest = Mathf.Min(lowest,height); worstJump = Mathf.Max(worstJump,Mathf.Abs(height-previousHeight));
                        previousHeight = height;
                        for (int side = 0; side < 2; side++)
                        {
                            bool grounded = feet[side].position.y - go.transform.position.y < 0.16f;
                            if (grounded && previousGround[side]) retreat = Mathf.Max(retreat,previousFeet[side].z-feet[side].position.z);
                            previousFeet[side] = feet[side].position; previousGround[side] = grounded;
                        }
                        report.AppendLine($"{frame}\t{attacks[hit]}\t{(ActionPhase)phase}\t{source.state.phaseProgress:F3}\t{height:F4}\t{feet[0].position.z:F4}\t{feet[0].position.y-go.transform.position.y:F4}\t{feet[1].position.z:F4}\t{feet[1].position.y-go.transform.position.y:F4}\t{hand.position.y-go.transform.position.y:F4}\t{shoulder.position.y-go.transform.position.y:F4}\t{driver.CurrentActionTime:F4}");
                        frame++;
                        if (OutputDirectory()!=null && frame % 16 == 0 && tile < 8)
                        {
                            yield return null; yield return null;
                            camera.transform.position = go.transform.position + new Vector3(2.6f,1.5f,4);
                            camera.transform.LookAt(go.transform.position + Vector3.up*0.95f);
                            CycleTile(camera,sheet,tile++);
                        }
                    }
                }
            if (OutputDirectory()!=null)
            {
                File.WriteAllText(Path.Combine(OutputDirectory(),prefix+".tsv"),report.ToString());
                sheet.Apply(); File.WriteAllBytes(Path.Combine(OutputDirectory(),prefix+".png"),sheet.EncodeToPNG());
            }
            Debug.Log($"Paladyn {weapon} seria: biodra {reference:F3} → min {lowest:F3} m, spadek {(reference-lowest)*100:F1} cm, zmiana na klatkę max {worstJump*100:F1} cm, cofnięcie podparcia max {retreat*100:F1} cm");
            Assert.Greater(lowest,reference-0.18f,"Lekka seria nie przechodzi do głębokiego przysiadu");
            Assert.Less(worstJump,0.06f,"Biodra nie przeskakują między ciosami");
            Assert.Less(retreat,0.05f,"Przycięty cios nie wprowadza cofnięcia stopy podporowej");
            Debug.Log($"Paladyn {weapon}: tempo odcinka trafienia drugiego ciosu {cutRates.Average():F2}x nagrania");
            if (weapon == WeaponModel.Sword)
            {
                Assert.Greater(cutRates.Min(),0.95f,"Krótki wycinek cięcia nie może być rozciągany do zwolnionego tempa w fazie trafienia");
                Assert.Less(lateHandAboveShoulder,0.05f,"Pod koniec drugiego ciosu dłoń wraca do gardy, nie zostaje uniesiona nad barkiem");
                Debug.Log($"Drugi cios: dłoń max {strikeHandHeight:F3} m, bark w gardzie {standingShoulderHeight:F3} m");
                Assert.Less(strikeHandHeight,standingShoulderHeight+0.05f,"Drugi cios kończy się przed uniesieniem dłoni powyżej wysokości barku w gardzie");
            }
        }

        [UnityTest] public IEnumerator SwordCombo_AlternatesLeadFoot() => AlternatingSteps(WeaponModel.Sword,"sword");
        [UnityTest] public IEnumerator FlailCombo_AlternatesLeadFoot() => AlternatingSteps(WeaponModel.Mace,"flail");

        IEnumerator AlternatingSteps(WeaponModel weapon,string prefix)
        {
            var def=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            var go=new GameObject("Paladyn_KrokiSerii"); cleanup.Add(go);
            var visual=go.AddComponent<CharacterVisual>(); visual.enabled=false;
            var source=new StubAnimSource();
            var kit=AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content+"Classes/class_hu_paladin.asset").loadouts[0];
            visual.BuildModel(def,1,new RigLook { weapon=weapon,weaponItem=prefix=="flail"?kit.weapon:null,shield=ShieldModel.Heater }); visual.Init(source);
            var driver=(ClipAnimationDriver)visual.Driver; var animator=visual.Model.GetComponentInChildren<Animator>();
            var feet=new[]{animator.GetBoneTransform(HumanBodyBones.LeftFoot),animator.GetBoneTransform(HumanBodyBones.RightFoot)};
            const float dt=1f/60;
            for(int i=0;i<60;i++) visual.Tick(dt);
            var cameraGo=new GameObject("KameraKrokowSerii"); cleanup.Add(cameraGo); var camera=cameraGo.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.13f,.15f,.18f); camera.fieldOfView=35;
            var lightGo=new GameObject("SwiatloKrokowSerii"); cleanup.Add(lightGo); var light=lightGo.AddComponent<Light>();
            light.type=LightType.Directional; light.intensity=1.4f; lightGo.transform.rotation=Quaternion.Euler(40,-35,0);
            var sheet=new Texture2D(1400,1000,TextureFormat.RGB24,false); cleanup.Add(sheet);
            var sideSheet=new Texture2D(1400,1000,TextureFormat.RGB24,false); cleanup.Add(sideSheet);
            var report=new System.Text.StringBuilder("cios\tfaza\tpostep\tLz\tLy\tPz\tPy\tkrokCiala\n");
            var start=feet.Select(f=>f.position).ToArray(); float supportDrift=0,supportLift=0,worstFrame=0,highestLift=0;
            Assert.Greater(start[0].z-start[1].z,.08f,"Pozycja wyjściowa ma lewą nogę z przodu");
            var previous=feet.Select(f=>f.position).ToArray(); var leads=new float[2]; int tile=0;
            for(int hit=0;hit<2;hit++)
            {
                int support=hit==0?0:1; var planted=feet[support].position;
                for(int phase=0;phase<3;phase++)
                {
                    int count=phase==0?18:phase==1?8:hit==0?8:18;
                    for(int i=1;i<=count;i++)
                    {
                        // Tak jak PlayerController: najpierw zużywa przesunięcie poprzedniej klatki, potem ewaluacja pozy.
                        go.transform.position+=go.transform.forward*driver.ConsumeAdvance();
                        source.state=new CharacterAnimState { action=AnimAction.Attack,attack=hit==0?AttackAnim.SlashRight:AttackAnim.SlashLeft,
                            phase=(ActionPhase)phase,phaseProgress=i/(float)(phase==2?18:count),windupSeconds=18*dt,activeSeconds=8*dt,actionDuration=44*dt };
                        visual.Tick(dt);
                        var offset=feet[support].position-planted; offset.y=0; supportDrift=Mathf.Max(supportDrift,offset.magnitude);
                        supportLift=Mathf.Max(supportLift,Mathf.Abs(feet[support].position.y-planted.y));
                        for(int side=0;side<2;side++)
                        {
                            worstFrame=Mathf.Max(worstFrame,Vector3.Distance(previous[side],feet[side].position));
                            previous[side]=feet[side].position;
                            highestLift=Mathf.Max(highestLift,feet[side].position.y-start[side].y);
                        }
                        report.AppendLine($"{hit}\t{phase}\t{source.state.phaseProgress:F3}\t{feet[0].position.z:F4}\t{feet[0].position.y:F4}\t{feet[1].position.z:F4}\t{feet[1].position.y:F4}\t{go.transform.position.z:F4}");
                        if((phase==0 && (i==6 || i==15)) || (phase==1 && i==6) || (phase==2 && i==count))
                        {
                            yield return null; yield return null;
                            camera.transform.position=go.transform.position+new Vector3(2.7f,1.5f,4); camera.transform.LookAt(go.transform.position+Vector3.up*.95f);
                            CycleTile(camera,sheet,tile);
                            camera.transform.position=go.transform.position+new Vector3(4,1.4f,0); camera.transform.LookAt(go.transform.position+Vector3.up*.95f);
                            CycleTile(camera,sideSheet,tile++);
                        }
                    }
                }
                leads[hit]=feet[1-hit].position.z-feet[hit].position.z;
            }
            string dir="TestResults/paladin-attack-steps"; Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,prefix+".tsv"),report.ToString()); sheet.Apply(); File.WriteAllBytes(Path.Combine(dir,prefix+".png"),sheet.EncodeToPNG());
            sideSheet.Apply(); File.WriteAllBytes(Path.Combine(dir,prefix+"-side.png"),sideSheet.EncodeToPNG());
            Debug.Log($"Kroki {weapon}: przód prawa/lewa {leads[0]:F3}/{leads[1]:F3} m, podpora {supportDrift:F4} m, klatka {worstFrame:F4} m, podniesienie {highestLift:F3} m, ciało {go.transform.position.z:F3} m");
            File.WriteAllText(Path.Combine(dir,prefix+"-metrics.txt"),System.FormattableString.Invariant($"leadRight={leads[0]:F6}\nleadLeft={leads[1]:F6}\nsupportDrift={supportDrift:F6}\nsupportLift={supportLift:F6}\nmaxFrame={worstFrame:F6}\nlift={highestLift:F6}\nadvance={go.transform.position.z:F6}\n"));
            Assert.That(leads[0],Is.InRange(.682f,.722f),"Po pierwszym ciosie prawa stopa prowadzi w dłuższym kroku");
            Assert.That(leads[1],Is.InRange(.682f,.722f),"Po drugim ciosie lewa stopa prowadzi w dłuższym kroku");
            Assert.Less(supportDrift,.025f,"Stopa podporowa pozostaje w miejscu podczas ciosu");
            Assert.Less(supportLift,.005f,"Większy wykrok nie odrywa stopy podporowej od podłoża");
            Assert.Less(worstFrame,.13f,"Pełny krok nadal przechodzi płynnie między klatkami przy 60 kl./s");
            Assert.That(highestLift,Is.InRange(.164f,.176f),"Odrobinę wyższe przeniesienie buta: około 17 cm");
            Assert.That(go.transform.position.z,Is.InRange(1.394f,1.414f),"Krok wydłużony o kolejne 30%: dwa ciosy przesuwają sylwetkę o 140,4 cm");
            source.state=default;
            for(int i=0;i<30;i++) { go.transform.position+=go.transform.forward*driver.ConsumeAdvance(); visual.Tick(dt); }
            Assert.Greater(feet[0].position.z-feet[1].position.z,.08f,"Powrót rąk do gardy nie cofa wypracowanej lewej nogi");
            go.transform.position+=Vector3.forward*5;
            visual.Tick(dt);
            var hips=animator.GetBoneTransform(HumanBodyBones.Hips);
            Assert.Less(Mathf.Abs(hips.position.z-go.transform.position.z),.35f,"Teleport zwalnia podparcie ze starego miejsca świata");
        }

        [UnityTest] public IEnumerator FlailCombo_SecondHitHasContinuousPreparation()
        {
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            var cls = AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content+"Classes/class_hu_paladin.asset");
            var kit = cls.loadouts[0];
            const float dt = 1f/60;
            float startup = kit.weapon.weapon.light.windup/1.1f, active = kit.weapon.weapon.light.active/1.1f;
            float recovery = kit.weapon.weapon.light.recovery*0.8f/1.1f;
            var cameraGo = new GameObject("KameraPrzejsciaCiosow"); cleanup.Add(cameraGo);
            var camera = cameraGo.AddComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.13f,0.15f,0.18f); camera.fieldOfView = 35;
            var lightGo = new GameObject("SwiatloPrzejsciaCiosow"); cleanup.Add(lightGo);
            lightGo.transform.rotation = Quaternion.Euler(40,-35,0);
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.4f;
            var sheet = new Texture2D(1400,1000,TextureFormat.RGB24,false); cleanup.Add(sheet);
            var report = new System.Text.StringBuilder("przerwanie\tklatka\tdlon_cm\tzmiana_predkosci_cm\tbarki_stopnie\tbiodra_cm\n");
            float worstStep = 0, worstChange = 0, worstChest = 0, worstHip = 0;
            int tile = 0;
            foreach (float cancel in new[] { 0.25f,0.55f,0.9f })
            {
                var go = new GameObject("Paladyn_PierwszyDrugiCios"); cleanup.Add(go);
                var visual = go.AddComponent<CharacterVisual>(); visual.enabled = false;
                var source = new StubAnimSource();
                visual.BuildModel(def,1,new RigLook { weapon=AnimResolve.ForItem(kit.weapon),weaponItem=kit.weapon,
                    shield=ShieldModel.Heater,offhandItem=kit.offHand }); visual.Init(source);
                var driver = (ClipAnimationDriver)visual.Driver;
                var animator = visual.Model.GetComponentInChildren<Animator>();
                var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                var chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
                var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                for (int i=0;i<60;i++) visual.Tick(dt);
                Vector3 lastHand=go.transform.InverseTransformPoint(hand.position), lastDelta=Vector3.zero;
                Quaternion lastChest=chest.rotation;
                float lastHip=hips.position.y-go.transform.position.y;
                for (int hit=0;hit<2;hit++)
                    for (float time=dt;time<startup+active+recovery*(hit==0?cancel:1);time+=dt)
                    {
                        var phase=time<startup?ActionPhase.Startup:time<startup+active?ActionPhase.Active:ActionPhase.Recovery;
                        float q=phase==ActionPhase.Startup?time/startup:phase==ActionPhase.Active?(time-startup)/active:(time-startup-active)/recovery;
                        source.state=new CharacterAnimState { action=AnimAction.Attack,attack=hit==0?AttackAnim.SlashRight:AttackAnim.SlashLeft,
                            phase=phase,phaseProgress=q,actionTime=time,actionDuration=startup+active+recovery,windupSeconds=startup,activeSeconds=active };
                        visual.Tick(dt);
                        Vector3 h=go.transform.InverseTransformPoint(hand.position), delta=h-lastHand;
                        float hip=hips.position.y-go.transform.position.y;
                        if (hit==1 && time<0.3f)
                        {
                            float change=(delta-lastDelta).magnitude;
                            worstStep=Mathf.Max(worstStep,delta.magnitude); worstChange=Mathf.Max(worstChange,change);
                            worstChest=Mathf.Max(worstChest,Quaternion.Angle(chest.rotation,lastChest));
                            worstHip=Mathf.Max(worstHip,Mathf.Abs(hip-lastHip));
                            report.AppendLine($"{cancel:F2}\t{time/dt:F0}\t{delta.magnitude*100:F3}\t{change*100:F3}\t{Quaternion.Angle(chest.rotation,lastChest):F3}\t{Mathf.Abs(hip-lastHip)*100:F3}");
                        }
                        lastHand=h; lastDelta=delta; lastChest=chest.rotation; lastHip=hip;
                        go.transform.position+=go.transform.forward*driver.ConsumeAdvance();
                        if (cancel==0.55f && hit==1 && time<0.28f && Mathf.RoundToInt(time/dt)%2==0 && tile<8 && OutputDirectory()!=null)
                        {
                            yield return null; yield return null;
                            camera.transform.position=go.transform.position+new Vector3(2.6f,1.5f,4);
                            camera.transform.LookAt(go.transform.position+Vector3.up*0.95f);
                            CycleTile(camera,sheet,tile++);
                        }
                    }
                Object.Destroy(go); yield return null;
            }
            if (OutputDirectory()!=null)
            {
                File.WriteAllText(Path.Combine(OutputDirectory(),"combo-transition.tsv"),report.ToString());
                sheet.Apply(); File.WriteAllBytes(Path.Combine(OutputDirectory(),"combo-transition.png"),sheet.EncodeToPNG());
            }
            Debug.Log($"Przejście korbacza: dłoń max {worstStep*100:F2} cm/kl., zmiana kroku {worstChange*100:F2} cm/kl., barki {worstChest:F2}°/kl., biodra {worstHip*100:F2} cm/kl.");
            Assert.Less(worstStep,0.13f,"Przygotowanie drugiego ciosu nie szarpie dłonią jak aktywne uderzenie");
            Assert.Less(worstChange,0.04f,"Zmiana kroku dłoni w przygotowaniu jest mniejsza od szerokości chwytu");
            Assert.Less(worstChest,10f,"Barki nie przeskakują w przygotowaniu drugiego ciosu");
            Assert.Less(worstHip,0.04f,"Przejście nie podrzuca bioder");
        }

        [UnityTest] public IEnumerator MatteArmor_HasNoMovingGlint_AndReceivesAdditionalLights()
        {
            var original = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath)
                .modelPrefab.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
            var material = new Material(original); cleanup.Add(material);
            material.SetTexture("_BaseMap", Texture2D.whiteTexture);
            material.SetColor("_BaseColor", new Color(0.35f, 0.35f, 0.35f));
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad); cleanup.Add(quad);
            quad.layer = 30; quad.GetComponent<Renderer>().sharedMaterial = material;
            var cameraGo = new GameObject("KameraTestuMatowegoMaterialu"); cleanup.Add(cameraGo);
            var camera = cameraGo.AddComponent<Camera>(); camera.cullingMask = 1 << 30;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            camera.orthographic = true; camera.orthographicSize = 0.5f;
            var lightGo = new GameObject("SwiatloTestuMatowegoMaterialu"); cleanup.Add(lightGo);
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1;
            light.cullingMask = 1 << 30; lightGo.transform.rotation = Quaternion.Euler(0, 30, 0);
            bool fog = RenderSettings.fog;
            var ambient = RenderSettings.ambientLight;
            var ambientMode = RenderSettings.ambientMode;
            RenderSettings.fog = false;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;
            try
            {
                yield return null; yield return null;
                Color Read(float x)
                {
                    camera.transform.position = new Vector3(x, 0, -3); camera.transform.LookAt(Vector3.zero);
                    var previous = RenderTexture.active;
                    var target = RenderTexture.GetTemporary(64, 64, 24);
                    var texture = new Texture2D(64, 64, TextureFormat.RGB24, false);
                    try
                    {
                        camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                        texture.ReadPixels(new Rect(0, 0, 64, 64), 0, 0); texture.Apply();
                        return texture.GetPixel(32, 32);
                    }
                    finally
                    {
                        camera.targetTexture = null; RenderTexture.active = previous;
                        RenderTexture.ReleaseTemporary(target); Object.Destroy(texture);
                    }
                }
                float Difference(Color a, Color b) => Mathf.Max(Mathf.Abs(a.r-b.r), Mathf.Abs(a.g-b.g), Mathf.Abs(a.b-b.b));
                Read(-1.732f); yield return null; yield return null;
                Color left = Read(-1.732f), right = Read(1.732f);
                Assert.Greater(left.grayscale, 0.05f, "Próbka rzeczywiście jest oświetlona");
                Assert.Less(Difference(left, right), 0.015f, "Ruch kamery nie przesuwa refleksu po zbroi");
                var block = new MaterialPropertyBlock(); block.SetVector("_HellSurface", new Vector4(0,1,0,0));
                quad.GetComponent<Renderer>().SetPropertyBlock(block);
                Assert.Less(Difference(Read(-1.732f), Read(1.732f)), 0.015f, "Moknięcie też nie przywraca połysku");
                quad.GetComponent<Renderer>().SetPropertyBlock(null);
                // Kontrola dodatnia: ta sama próba wykrywa połysk zwykłego materiału PBR.
                var glossy = new Material(Shader.Find("Universal Render Pipeline/Lit")); cleanup.Add(glossy);
                glossy.SetFloat("_Metallic", 1); glossy.SetFloat("_Smoothness", 0.85f);
                quad.GetComponent<Renderer>().sharedMaterial = glossy;
                Read(-1.732f); yield return null; yield return null;
                float pbrDifference = Difference(Read(-1.732f), Read(1.732f));
                Assert.Greater(pbrDifference, 0.05f, "Kontrola dodatnia wykrywa ruchomy refleks PBR");
                quad.GetComponent<Renderer>().sharedMaterial = material;
                light.type = LightType.Point; light.color = Color.red; light.range = 4; light.intensity = 2;
                lightGo.transform.position = new Vector3(0,0,-1);
                yield return null;
                Color additional = Read(0);
                Assert.Greater(additional.r - additional.g, 0.05f, "Matowy materiał odbiera światło punktowe w Forward+");
                Debug.Log($"Matowy materiał: różnica widoków {Difference(left,right):F5}, kontrola PBR {pbrDifference:F5}, światło punktowe {additional}");
                // Paleta koncepcji: przygaszona stal, złote okucia i zachowany burgund, czytelne także w cieniu.
                light.type = LightType.Directional; light.color = Color.white; light.intensity = 1;
                lightGo.transform.rotation = Quaternion.Euler(0,30,0);
                material.SetColor("_BaseColor",Color.white);
                var pigment = new Texture2D(2,2,TextureFormat.RGB24,false); cleanup.Add(pigment);
                material.SetTexture("_BaseMap",pigment);
                Color SamplePigment(Color color)
                {
                    pigment.SetPixels(new[] { color,color,color,color }); pigment.Apply();
                    return Read(0);
                }
                Color steel = SamplePigment(new Color(0.85f,0.85f,0.85f));
                Color trim = SamplePigment(new Color(0.80f,0.75f,0.63f));
                Color cloth = SamplePigment(new Color(0.48f,0.07f,0.10f));
                lightGo.transform.rotation = Quaternion.Euler(0,180,0);
                Color shade = SamplePigment(new Color(0.85f,0.85f,0.85f));
                Assert.Greater(steel.r-steel.b,0.025f,"Stal ma ciepłą szarość zamiast zimnej bieli");
                Assert.Less(steel.grayscale,0.9f,"Płyty nie wypalają się do białego plastiku");
                Assert.Greater(trim.r-trim.b,steel.r-steel.b+0.06f,"Okucia są wyraźniej złote od stalowych płyt");
                Assert.Greater(cloth.r,cloth.g*3,"Burgund nie dostaje jednolitego złotego filtra");
                Assert.Greater(cloth.r,cloth.b*2);
                Assert.Less(shade.grayscale,steel.grayscale*0.65f,"Cień zachowuje czytelny podział bryły");
                Assert.Less(Mathf.Abs(shade.r-shade.b),0.10f,"Cień jest neutralny lub delikatnie chłodny, bez nasyconego błękitu");
                Assert.Greater(shade.grayscale,steel.grayscale*.35f,"W cieniu pozostaje czytelny kolor płyty");
                lightGo.transform.rotation=Quaternion.Euler(0,30,0); light.intensity=8;
                Color strong=SamplePigment(new Color(.98f,.98f,.98f));
                Assert.Less(strong.grayscale,.82f,"Nawet mocne światło nie zamienia płyt w białą emalię");
                Debug.Log($"Paleta: stal {steel}, złoto {trim}, tkanina {cloth}, cień {shade}, światło 8× {strong}");
            }
            finally
            {
                RenderSettings.fog = fog; RenderSettings.ambientLight = ambient; RenderSettings.ambientMode = ambientMode;
            }
        }

        [UnityTest] public IEnumerator RunCycle_FeetFollowKnees_WithoutOutwardTwist() => RunCycle(WeaponModel.Sword, "run-feet");
        [UnityTest] public IEnumerator TwoHandRun_FeetFollowKnees_WithoutOutwardTwist() => RunCycle(WeaponModel.GreatSword, "run-twohand");

        IEnumerator RunCycle(WeaponModel weapon, string filePrefix)
        {
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            var go = new GameObject("PaladynAnime_Bieg"); cleanup.Add(go);
            var visual = go.AddComponent<CharacterVisual>(); visual.enabled = false;
            var source = new StubAnimSource();
            visual.BuildModel(def, 1, new RigLook { weapon = weapon, shield = weapon == WeaponModel.Sword ? ShieldModel.Heater : ShieldModel.None });
            visual.Init(source);
            var animator = visual.Model.GetComponentInChildren<Animator>();
            var driver = (ClipAnimationDriver)visual.Driver;
            var feet = new[] { HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot }.Select(animator.GetBoneTransform).ToArray();
            var toes = new[] { HumanBodyBones.LeftToes, HumanBodyBones.RightToes }.Select(animator.GetBoneTransform).ToArray();
            Assert.IsTrue(toes.All(toe => toe != null), "Obie kości palców stóp muszą być zmapowane dla korekt biegu");
            var knees = new[] { HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg }.Select(animator.GetBoneTransform).ToArray();
            var hips = new[] { HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg }.Select(animator.GetBoneTransform).ToArray();
            var shoulders = new[] { HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder }.Select(animator.GetBoneTransform).ToArray();
            var pelvisYaw = new List<float>();
            var chestYaw = new List<float>();
            var torsoTwist = new List<float>();
            var footTrack = new List<float>();
            var hipHeight = new List<float>();
            var chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            var skin = visual.Model.GetComponentsInChildren<SkinnedMeshRenderer>().First(s => s.bones.Contains(chest));
            var baked = new Mesh(); cleanup.Add(baked); skin.BakeMesh(baked);
            var restVertices = baked.vertices;
            int Landmark(float x) => Enumerable.Range(0, restVertices.Length).OrderBy(i =>
                (skin.transform.TransformPoint(restVertices[i]) - go.transform.position - new Vector3(x, 1.22f, 0.12f)).sqrMagnitude).First();
            int abdomenLeft = Landmark(-0.06f), abdomenRight = Landmark(0.06f);
            var abdomenDirections = new List<Vector3>();
            var upperLength = Enumerable.Range(0, 2).Select(side => Vector3.Distance(hips[side].position, knees[side].position)).ToArray();
            var lowerLength = Enumerable.Range(0, 2).Select(side => Vector3.Distance(knees[side].position, feet[side].position)).ToArray();
            var soleRight = new Vector3[2];
            for (int side = 0; side < 2; side++) soleRight[side] = feet[side].InverseTransformDirection(go.transform.right);
            var planeErrors = new[] { new List<float>(), new List<float>() };
            var stanceYaws = new[] { new List<float>(), new List<float>() };
            float widest = 0, worstKneePlane = 0, worstLength = 0, reverseKnee = 0;
            var report = new System.Text.StringBuilder("predkosc\tklatka\tstrona\tyaw\tskret_wzgledem_goleni\twysokosc\tpoprawka\todchylenie_buta\tbok_od_biodra\tkolano_poza_plaszczyzna\ttor_stopy_od_srodka\tobrot_miednicy\tobrot_barkow\n");
            var cameraGo = new GameObject("KameraCykluBiegu"); cleanup.Add(cameraGo);
            var camera = cameraGo.AddComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.13f, 0.15f, 0.18f); camera.fieldOfView = 35;
            var lightGo = new GameObject("SwiatloCykluBiegu"); cleanup.Add(lightGo);
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.4f;
            lightGo.transform.rotation = Quaternion.Euler(40, -35, 0);
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.58f);
            string dir = OutputDirectory();
            var frontSheet = new Texture2D(1400,1000,TextureFormat.RGB24,false); cleanup.Add(frontSheet);
            var sideSheet = new Texture2D(1400,1000,TextureFormat.RGB24,false); cleanup.Add(sideSheet);
            foreach (float speed in new[] { 5.2f, 8.06f })
            {
                for (int frame = 0; frame < 240; frame++)
                {
                    go.transform.position += Vector3.forward * (speed / 60);
                    visual.Tick(1f / 60);
                    if (frame < 60) continue;
                    Vector3 pelvisAxis = go.transform.InverseTransformDirection(hips[1].position - hips[0].position);
                    Vector3 shoulderAxis = go.transform.InverseTransformDirection(shoulders[1].position - shoulders[0].position);
                    float py = Mathf.Atan2(-pelvisAxis.z, pelvisAxis.x) * Mathf.Rad2Deg;
                    float cy = Mathf.Atan2(-shoulderAxis.z, shoulderAxis.x) * Mathf.Rad2Deg;
                    pelvisYaw.Add(py); chestYaw.Add(cy); torsoTwist.Add(Mathf.DeltaAngle(py, cy));
                    hipHeight.Add((hips[0].position.y + hips[1].position.y) * 0.5f - go.transform.position.y);
                    if (frame % 4 == 0)
                    {
                        skin.BakeMesh(baked);
                        Vector3 abdomenAxis = skin.transform.TransformDirection(baked.vertices[abdomenRight] - baked.vertices[abdomenLeft]);
                        abdomenDirections.Add(chest.InverseTransformDirection(abdomenAxis).normalized);
                    }
                    for (int side = 0; side < 2; side++)
                    {
                        Vector3 forward = toes[side].position - feet[side].position;
                        var axis = (knees[side].position - feet[side].position).normalized;
                        float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
                        Vector3 hinge = Vector3.Cross(knees[side].position - hips[side].position, feet[side].position - knees[side].position).normalized;
                        float twist = Vector3.SignedAngle(hinge, feet[side].TransformDirection(soleRight[side]), axis);
                        float height = feet[side].position.y - go.transform.position.y;
                        float planeError = Vector3.Angle(go.transform.right, feet[side].TransformDirection(soleRight[side]));
                        planeErrors[side].Add(planeError);
                        Vector3 hipToFoot = feet[side].position - hips[side].position;
                        Vector3 kneeOffset = knees[side].position - hips[side].position;
                        float lateral = Mathf.Abs(Vector3.Dot(hipToFoot, go.transform.right));
                        float track = go.transform.InverseTransformPoint(feet[side].position).x;
                        if (height < 0.20f) footTrack.Add(Mathf.Abs(track));
                        float kneePlane = Mathf.Abs(Vector3.Dot(kneeOffset, Vector3.Cross(go.transform.forward, hipToFoot).normalized));
                        widest = Mathf.Max(widest, lateral);
                        worstKneePlane = Mathf.Max(worstKneePlane, kneePlane);
                        reverseKnee = Mathf.Min(reverseKnee, Vector3.Dot(kneeOffset, Vector3.ProjectOnPlane(go.transform.forward, hipToFoot.normalized).normalized));
                        worstLength = Mathf.Max(worstLength, Mathf.Abs(kneeOffset.magnitude - upperLength[side]), Mathf.Abs(Vector3.Distance(knees[side].position, feet[side].position) - lowerLength[side]));
                        if (height < 0.20f && new Vector2(forward.x, forward.z).magnitude > forward.magnitude * 0.7f) stanceYaws[side].Add(Mathf.Abs(yaw));
                        report.AppendLine($"{speed:F2}\t{frame}\t{side}\t{yaw:F2}\t{twist:F2}\t{height:F3}\t{(side == 0 ? driver.FootYawFix.x : driver.FootYawFix.y):F2}\t{planeError:F2}\t{lateral:F4}\t{kneePlane:F4}\t{track:F4}\t{py:F2}\t{cy:F2}");
                    }
                    if (dir != null && speed == 5.2f && frame >= 120 && frame < 152 && frame % 4 == 0)
                    {
                        yield return null; yield return null;
                        camera.transform.position = go.transform.position + new Vector3(0, 0.65f, 3.6f);
                        camera.transform.LookAt(go.transform.position + Vector3.up * 0.65f);
                        Shot(camera, Path.Combine(dir, $"{filePrefix}-{frame:D3}.png"));
                        CycleTile(camera, frontSheet, (frame - 120) / 4);
                        camera.transform.position = go.transform.position + new Vector3(3.6f, 0.65f, 0);
                        camera.transform.LookAt(go.transform.position + Vector3.up * 0.65f);
                        Shot(camera, Path.Combine(dir, $"{filePrefix}-side-{frame:D3}.png"));
                        CycleTile(camera, sideSheet, (frame - 120) / 4);
                    }
                }
            }
            if (dir != null)
            {
                File.WriteAllText(Path.Combine(dir, filePrefix + ".tsv"), report.ToString());
                frontSheet.Apply(); sideSheet.Apply();
                File.WriteAllBytes(Path.Combine(dir, filePrefix + "-cycle.png"), frontSheet.EncodeToPNG());
                File.WriteAllBytes(Path.Combine(dir, filePrefix + "-cycle-side.png"), sideSheet.EncodeToPNG());
            }
            Debug.Log($"Paladyn {weapon}: stopa od biodra max {widest * 100:F2} cm; kolano poza płaszczyzną max {worstKneePlane * 100:F2} cm; błąd długości {worstLength * 1000:F3} mm");
            Debug.Log($"Paladyn {weapon}: tor stopy od środka max {footTrack.Max() * 100:F2} cm; miednica zakres {pelvisYaw.Max()-pelvisYaw.Min():F2}°; barki {chestYaw.Max()-chestYaw.Min():F2}°; skręt tułowia {torsoTwist.Max()-torsoTwist.Min():F2}°; pion {100*(hipHeight.Max()-hipHeight.Min()):F2} cm");
            float visibleTorsoMotion = abdomenDirections.Max(d => Vector3.Angle(abdomenDirections[0],d));
            Debug.Log($"Paladyn {weapon}: praca siatki brzucha względem klatki {visibleTorsoMotion:F2}°");
            Assert.Greater(visibleTorsoMotion, 2f, "Widoczny tułów pracuje w pasie, zamiast być jednym sztywnym blokiem");
            Assert.Less(footTrack.Max(), 0.08f, "W biegu stopy lądują bliżej osi ruchu niż szerokość bioder");
            Assert.Less(widest, 0.13f, "Przywiedzenie nogi do osi ruchu mieści się w wąskim torze biegu");
            Assert.Less(worstKneePlane, 0.025f, "Kolano prowadzi w płaszczyźnie kroku");
            Assert.GreaterOrEqual(reverseKnee, -0.005f, "Brak przeprostu kolana");
            Assert.Less(worstLength, 0.001f, "IK zachowuje długości ud i goleni");
            for (int side = 0; side < 2; side++)
            {
                planeErrors[side].Sort();
                float p95 = planeErrors[side][Mathf.FloorToInt((planeErrors[side].Count - 1) * 0.95f)];
                Assert.IsNotEmpty(stanceYaws[side], "Cykl zawiera płaskie podparcie obu stóp");
                Debug.Log($"Paladyn bieg {(side == 0 ? "L" : "P")}: boczne odchylenie buta p95 {p95:F2}°, max {planeErrors[side].Max():F2}°, yaw podparcia max {stanceYaws[side].Max():F2}°");
                Assert.Less(p95, 15, "But zgina się w płaszczyźnie kroku, bez wykręcania na boki");
                Assert.Less(stanceYaws[side].Max(), 35, "Stopa w podporze nie ucieka na zewnątrz");
            }
        }

        static void CycleTile(Camera camera, Texture2D sheet, int tile)
        {
            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(350,500,24);
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                sheet.ReadPixels(new Rect(0,0,350,500), tile % 4 * 350, (1 - tile / 4) * 500);
            }
            finally
            {
                camera.targetTexture = null; RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
            }
        }

        [UnityTest] public IEnumerator ExistingAnimations_KeepBootsRigid_AndRenderGallery()
        {
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            Assert.IsNotNull(def);
            var go = new GameObject("PaladynAnime_Test"); cleanup.Add(go);
            var visual = go.AddComponent<CharacterVisual>(); visual.enabled = false;
            var source = new StubAnimSource();
            var cls = AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content + "Classes/class_hu_paladin.asset");
            var kit = cls.loadouts[0];
            var look = new RigLook { weapon = WeaponModel.Sword, weaponItem = kit.weapon, shield = ShieldModel.Heater, offhandItem = kit.offHand };
            visual.BuildModel(def, 1f, look); visual.Init(source);
            var animator = visual.Model.GetComponentInChildren<Animator>();
            Assert.IsTrue(animator.isHuman);
            var leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            var skin = visual.Model.GetComponentsInChildren<SkinnedMeshRenderer>().First(renderer => renderer.bones.Contains(leftFoot));
            var mesh = new Mesh(); cleanup.Add(mesh);
            var vertices = skin.sharedMesh.vertices;
            var weights = skin.sharedMesh.boneWeights;
            var samples = new List<(int index, Transform foot, Vector3 point)>();
            skin.BakeMesh(mesh);
            foreach (var foot in new[] { animator.GetBoneTransform(HumanBodyBones.LeftFoot), animator.GetBoneTransform(HumanBodyBones.RightFoot) })
            {
                int bone = System.Array.IndexOf(skin.bones, foot);
                for (int i = 0; i < vertices.Length; i++)
                    if (weights[i].boneIndex0 == bone && weights[i].weight0 > 0.999f && samples.Count(s => s.foot == foot) < 30)
                        samples.Add((i, foot, foot.InverseTransformPoint(skin.transform.TransformPoint(mesh.vertices[i]))));
            }
            Assert.GreaterOrEqual(samples.Count, 40, "Próbki obu sztywnych butów");
            var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var slot = Util.FindDeep(visual.Model.transform, def.rightSlot);
            var handSamples = new List<(int index, Vector3 point)>();
            int handBone = System.Array.IndexOf(skin.bones, hand);
            for (int i = 0; i < vertices.Length; i++)
                if (weights[i].boneIndex0 == handBone && weights[i].weight0 > 0.999f && handSamples.Count < 60)
                    handSamples.Add((i, hand.InverseTransformPoint(skin.transform.TransformPoint(mesh.vertices[i]))));
            Vector3 slotRest = hand.InverseTransformPoint(slot.position);
            var gripReport = new System.Text.StringBuilder("poza\tprzesuniecie_skory_mm\tprzesuniecie_gniazda_mm\tgniazdo_w_dlon\n");
            var camGo = new GameObject("KameraAnime"); cleanup.Add(camGo);
            var camera = camGo.AddComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.13f, 0.15f, 0.18f); camera.fieldOfView = 35;
            var lightGo = new GameObject("ŚwiatłoAnime"); cleanup.Add(lightGo);
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.4f;
            lightGo.transform.rotation = Quaternion.Euler(40, -35, 0);
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.58f);
            float worst = 0;
            var states = new[] {
                ("idle", default(CharacterAnimState)),
                ("run", default(CharacterAnimState)),
                ("block", new CharacterAnimState { action = AnimAction.Block, hasShield = true }),
                ("slash", new CharacterAnimState { action = AnimAction.Attack, attack = AttackAnim.SlashRight, phase = ActionPhase.Active, phaseProgress = 0.5f }),
                ("heavy", new CharacterAnimState { action = AnimAction.Attack, attack = AttackAnim.Overhead, phase = ActionPhase.Active, phaseProgress = 0.5f }),
                ("dodge", new CharacterAnimState { action = AnimAction.Dodge, actionTime = 0.3f }),
                ("cast", new CharacterAnimState { action = AnimAction.Cast, phase = ActionPhase.Active, phaseProgress = 0.5f }),
                ("death", new CharacterAnimState { action = AnimAction.Death, actionTime = 1f })
            };
            string dir = OutputDirectory();
            foreach (var (name, state) in states)
            {
                source.state = state;
                for (int i = 0; i < 60; i++)
                {
                    if (name == "run") go.transform.position += Vector3.forward * (5.2f / 60);
                    visual.Tick(1f / 60);
                }
                // Transform broni zmienia się od razu, ale skinning renderera aktualizuje się raz na klatkę.
                // Zrzut w tej samej klatce pokazywał siatkę z poprzedniej pozy i pozornie odsuniętą broń.
                yield return null; yield return null;
                skin.BakeMesh(mesh);
                var posed = mesh.vertices;
                float handShift = handSamples.Max(sample => Vector3.Distance(sample.point, hand.InverseTransformPoint(skin.transform.TransformPoint(posed[sample.index]))));
                float slotShift = Vector3.Distance(slotRest, hand.InverseTransformPoint(slot.position));
                gripReport.AppendLine($"{name}\t{handShift * 1000:F3}\t{slotShift * 1000:F3}\t{hand.InverseTransformPoint(slot.position)}");
                Assert.Less(handShift, 0.003f, name + ": chwyt nie odsuwa się od kości dłoni");
                Assert.Less(slotShift, 0.003f, name + ": gniazdo pozostaje w chwycie");
                foreach (var sample in samples)
                    worst = Mathf.Max(worst, Vector3.Distance(sample.point, sample.foot.InverseTransformPoint(skin.transform.TransformPoint(posed[sample.index]))));
                Assert.Less(skin.bounds.size.magnitude, 5f, name + ": prawidłowa skala siatki");
                Assert.IsFalse(float.IsNaN(visual.WeaponTip.x));
                if (dir != null)
                {
                    camera.transform.position = go.transform.position + new Vector3(2.6f, 1.6f, 4f);
                    camera.transform.LookAt(go.transform.position + new Vector3(0, 0.95f, 0));
                    Shot(camera, Path.Combine(dir, "paladin-" + name + ".png"));
                    if (name == "idle")
                    {
                        camera.transform.position = go.transform.position + new Vector3(-2.4f, 1.6f, -3.5f);
                        camera.transform.LookAt(go.transform.position + new Vector3(0, 0.95f, 0));
                        Shot(camera, Path.Combine(dir, "paladin-back.png"));
                    }
                }
                yield return null;
            }
            Debug.Log($"Paladyn anime: maks. ruch punktu buta względem kości {worst * 1000:F3} mm");
            if (dir != null) File.WriteAllText(Path.Combine(dir, "grip.tsv"), gripReport.ToString());
            Assert.Less(worst, 0.003f, "Stalowe buty zachowują kształt we wszystkich próbach");
        }

        static void Shot(Camera camera, string file)
        {
            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(1400, 1000, 24);
            var texture = new Texture2D(1400, 1000, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 1400, 1000), 0, 0); texture.Apply();
                File.WriteAllBytes(file, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null; RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target); Object.Destroy(texture);
            }
        }
    }
}
#endif
