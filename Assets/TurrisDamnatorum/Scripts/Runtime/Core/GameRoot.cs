using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Turris
{
    public enum GameScreen { MainMenu, Unlocks, Loadout, Playing, Reward, Intermission, Equipment, Paused, Death, Victory }

    /// <summary>
    /// Punkt wejścia i przepływ gry: menu → przygotowanie → piętra → nagrody/kapliczka → śmierć lub zwycięstwo.
    /// Scena zawiera tylko ten komponent, kamerę i światło; arena i postacie powstają w czasie działania.
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        public GameConfig config;
        [Tooltip("Nazwa pliku profilu w Application.persistentDataPath.")]
        public string profileFileName = "turris_profile.json";
        /// <summary>Tylko testy: podmienia plik profilu, by nie dotykać zapisu gracza.</summary>
        public static string ProfileFileOverride;

        public MetaService Meta { get; private set; }
        public RunState Run { get; private set; }
        public LoadoutPlan Plan { get; private set; }
        public GameScreen Screen { get; private set; } = GameScreen.MainMenu;
        public GameScreen ScreenBeforeEquipment { get; private set; }
        public PlayerCombat Player { get; private set; }
        public PlayerController Controller { get; private set; }
        public CameraRig CameraRig { get; private set; }
        public List<RewardOption> Rewards { get; private set; } = new List<RewardOption>();
        public FloorDefinition CurrentFloor => Run != null && Run.floorIndex < config.tower.floors.Count ? config.tower.floors[Run.floorIndex] : null;
        public bool AtRestPoint { get; private set; }
        public string LastFloorSummary { get; private set; }
        public int LastAsh { get; private set; }
        public string ProfilePath { get; private set; }

        readonly List<EnemyBrain> enemies = new List<EnemyBrain>();
        Transform world;
        GameObject arena;
        Light sun;
        System.Random rng;
        float clearTimer = -1f;
        float deathTimer = -1f;

        void Awake()
        {
            if (config == null)
            {
                Debug.LogWarning("[Turris] Brak przypisanego GameConfig – używam domyślnej treści z kodu. Uruchom Turris → Setup, aby utworzyć assety.");
                config = DefaultContent.Create().config;
            }
            if (GetComponent<GameUI>() == null) gameObject.AddComponent<GameUI>();
            if (GetComponent<CombatFxDirector>() == null) gameObject.AddComponent<CombatFxDirector>();
            ProfilePath = Path.Combine(Application.persistentDataPath, ProfileFileOverride ?? profileFileName);
            Meta = new MetaService(config, new FileProfileStorage(ProfilePath));
            if (Meta.Warning != null) Debug.LogWarning("[Turris] " + Meta.Warning);

            world = new GameObject("World").transform;
            SetupSceneObjects();
            CombatEvents.Died += OnSomethingDied;
            ShowMainMenu();
        }

        void OnDestroy()
        {
            CombatEvents.Died -= OnSomethingDied;
            Time.timeScale = 1f;
        }

        void SetupSceneObjects()
        {
            sun = null;
            foreach (var l in FindObjectsByType<Light>(FindObjectsInactive.Exclude))
                if (l.type == LightType.Directional) { sun = l; break; }
            if (sun == null)
            {
                var l = new GameObject("Sun");
                sun = l.AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            sun.shadows = LightShadows.Soft;

            var cam = Camera.main;
            if (cam == null)
            {
                var c = new GameObject("Main Camera");
                c.tag = "MainCamera";
                cam = c.AddComponent<Camera>();
                c.AddComponent<AudioListener>();
            }
            cam.nearClipPlane = 0.1f;
            CameraRig = cam.GetComponent<CameraRig>();
            if (CameraRig == null) CameraRig = cam.gameObject.AddComponent<CameraRig>();

            var p = WorldBuilder.CreatePlayer();
            Player = p.GetComponent<PlayerCombat>();
            Controller = p.GetComponent<PlayerController>();
            Controller.cameraRig = CameraRig;
            Controller.PauseRequested += TogglePause;
            CameraRig.target = p.transform;
            CameraRig.player = Controller;
            CameraRig.input = p.GetComponent<PlayerInputReader>();
            Player.Died += OnPlayerDied;
            p.SetActive(false);

            QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 8);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 60f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.32f, 0.3f, 0.33f);
        }

        // ================================================================== Menu

        public void ShowMainMenu()
        {
            ClearWorld();
            Run = null;
            Player.gameObject.SetActive(false);
            SetScreen(GameScreen.MainMenu);
            BuildMenuBackdrop();
        }

        void BuildMenuBackdrop()
        {
            arena = WorldBuilder.BuildArena(config.tower.floors[config.tower.floors.Count - 1].arena, world);
            var cam = CameraRig.transform;
            CameraRig.enabled = false;
            cam.position = new Vector3(0, 6, -14);
            cam.rotation = Quaternion.Euler(18, 0, 0);
        }

        public void ShowUnlocks() => SetScreen(GameScreen.Unlocks);

        public void ShowLoadout()
        {
            Plan = Meta.LastPlan();
            SetScreen(GameScreen.Loadout);
        }

        public void SetScreen(GameScreen s)
        {
            Screen = s;
            bool playing = s == GameScreen.Playing;
            Controller.InputEnabled = playing;
            Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !playing;
            // W trakcie gry cały świat (gracz, wrogowie, pociski, efekty) biegnie w tempie balance.gameSpeed.
            float gameSpeed = config != null && config.balance.gameSpeed > 0f ? config.balance.gameSpeed : 1f;
            Time.timeScale = s == GameScreen.Paused || s == GameScreen.Equipment ? 0f : playing ? gameSpeed : 1f;
        }

        // ================================================================== Podejście

        public bool TryStartRun(LoadoutPlan plan, out string reason)
        {
            if (!Meta.ValidateLoadout(plan, out reason)) return false;
            StartRun(plan);
            return true;
        }

        public void StartRun(LoadoutPlan plan)
        {
            Plan = plan;
            Meta.RecordRunStart(plan);
            Meta.RememberSkillSlots(plan.skillSlots);
            int seed = System.Environment.TickCount;
            rng = new System.Random(seed);
            Run = RunFactory.Create(plan, config, seed);
            Player.gameObject.SetActive(true);
            Player.Init(Run, config);
            var vis = Player.GetComponent<PlayerVisuals>();
            vis.SetBodyColor(plan.classDef.color);
            vis.Refresh();
            CameraRig.enabled = true;
            LoadFloor(0);
        }

        /// <summary>Szybki restart po śmierci – ten sam zestaw startowy.</summary>
        public void RetrySameLoadout()
        {
            var plan = Plan ?? Meta.LastPlan();
            if (!Meta.ValidateLoadout(plan, out _)) plan = Meta.LastPlan();
            StartRun(plan);
        }

        void LoadFloor(int index)
        {
            ClearWorld();
            Run.floorIndex = index;
            var floor = config.tower.floors[index];
            arena = WorldBuilder.BuildArena(floor.arena, world);
            sun.color = floor.arena.lightColor;
            RenderSettings.fogColor = floor.arena.fogColor;
            if (CameraRig.Camera != null) { CameraRig.Camera.backgroundColor = floor.arena.fogColor; CameraRig.Camera.clearFlags = CameraClearFlags.SolidColor; }

            Controller.Teleport(WorldBuilder.PlayerSpawn(floor.arena), Quaternion.identity);
            Player.Actions.Reset();
            Player.OnFloorStart();
            CameraRig.SnapBehindTarget();

            var spawns = WorldBuilder.EnemySpawns(floor.arena, floor.enemies.Count);
            bool elite = Run.difficulty.elites && floor.isDuel && !floor.isBoss;
            for (int i = 0; i < floor.enemies.Count; i++)
            {
                var e = WorldBuilder.CreateEnemy(floor.enemies[i], spawns[i], world);
                e.transform.rotation = Quaternion.LookRotation(-spawns[i].normalized);
                e.Setup(floor.enemies[i], Player, floor.statScale, Run.difficulty, elite, config.balance);
                enemies.Add(e);
            }

            Meta.RecordReachedFloor(index);
            clearTimer = -1f;
            deathTimer = -1f;
            SetScreen(GameScreen.Playing);
            CombatEvents.RaiseMessage(floor.name, new Color(0.95f, 0.85f, 0.6f));
        }

        void ClearWorld()
        {
            foreach (var e in enemies) if (e != null) Destroy(e.gameObject);
            enemies.Clear();
            if (arena != null) Destroy(arena);
            foreach (var p in Projectile.Active.ToList()) Destroy(p.gameObject);
            foreach (var f in FxFade.Active.ToList()) Destroy(f.gameObject);
            arena = null;
        }

        void OnSomethingDied(IHitReceiver who)
        {
            if (Run == null || !(who is EnemyBrain e) || !enemies.Contains(e)) return;
            Run.kills++;
            Run.souls += Mathf.RoundToInt(e.Def.soulReward * Run.difficulty.soulMultiplier * (e.IsElite ? 1.5f : 1f));
            if (enemies.All(x => x == null || x.IsDead) && !Player.IsDead) clearTimer = 1.5f;
        }

        void Update()
        {
            if (Screen == GameScreen.Playing && clearTimer > 0)
            {
                clearTimer -= Time.deltaTime;
                if (clearTimer <= 0) CompleteFloor();
            }
            if (deathTimer > 0)
            {
                deathTimer -= Time.deltaTime;
                if (deathTimer <= 0) SetScreen(GameScreen.Death);
            }
            if (Screen == GameScreen.Death && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
                RetrySameLoadout();
            if (Screen == GameScreen.MainMenu && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.f9Key.wasPressedThisFrame)
                Meta.DebugAddAsh(100);
        }

        void CompleteFloor()
        {
            clearTimer = -1f;
            var floor = CurrentFloor;
            Run.floorsCleared++;
            LastAsh = Meta.AwardFloorClear(Run, Run.floorIndex);
            Run.healthFraction = Player.Health.Fraction;
            Run.manaFraction = Player.Mana.Fraction;

            if (floor.isBoss || Run.floorIndex >= config.tower.floors.Count - 1)
            {
                LastAsh += Meta.AwardVictory(Run);
                Run.IsFinished = true;
                Run.Victory = true;
                SetScreen(GameScreen.Victory);
                return;
            }

            // Odnowienie po piętrze (chyba że trudność pozwala na to tylko w kapliczkach).
            AtRestPoint = floor.restAfter;
            if (!Run.difficulty.restoreOnlyAtRest || AtRestPoint)
            {
                Run.RefillFlasks();
                Player.RestoreAll();
            }

            LastFloorSummary = $"{floor.name} ukończone. Popiół +{LastAsh}.";
            Rewards = RewardGenerator.Generate(Run, Player.Build, Meta.BuildRewardPools(), config, rng);
            SetScreen(GameScreen.Reward);
        }

        public void ChooseReward(int index)
        {
            if (Screen != GameScreen.Reward) return;
            if (Rewards.Count > 0)
            {
                if (index < 0 || index >= Rewards.Count) return;
                Rewards[index].Apply(Run);
            }
            Player.RefreshBuild();
            Rewards.Clear();
            SetScreen(GameScreen.Intermission);
        }

        public void NextFloor()
        {
            Run.healthFraction = Player.Health.Fraction;
            Run.manaFraction = Player.Mana.Fraction;
            LoadFloor(Run.floorIndex + 1);
        }

        public void OpenEquipment()
        {
            ScreenBeforeEquipment = Screen;
            SetScreen(GameScreen.Equipment);
        }

        public void CloseEquipment()
        {
            Meta.RememberSkillSlots(Run.skillSlots.Select(s => s?.definition).ToList());
            Player.RefreshBuild();
            SetScreen(ScreenBeforeEquipment);
        }

        // ------------------------------------------------------------------ Kapliczka

        public int WeaponUpgradeCost => config.balance.weaponUpgradeSoulCost * (1 + (Run.equipment.Get(EquipSlot.MainHand)?.level ?? 0));
        public int FlaskUpgradeCost => config.balance.flaskUpgradeSoulCost * (1 + Run.bonusHealthFlasks);

        public bool CanUpgradeWeapon(out string reason)
        {
            reason = null;
            var w = Run.equipment.Get(EquipSlot.MainHand);
            if (w == null) { reason = "Brak broni"; return false; }
            if (w.level >= config.balance.maxItemLevel) { reason = "Maksymalny poziom"; return false; }
            if (Run.souls < WeaponUpgradeCost) { reason = "Za mało dusz"; return false; }
            return true;
        }

        public void UpgradeWeapon()
        {
            if (!AtRestPoint || !CanUpgradeWeapon(out _)) return;
            Run.souls -= WeaponUpgradeCost;
            Run.equipment.Get(EquipSlot.MainHand).level++;
            Player.RefreshBuild();
        }

        public void BuyFlask()
        {
            if (!AtRestPoint || Run.bonusHealthFlasks >= 2 || Run.souls < FlaskUpgradeCost) return;
            Run.souls -= FlaskUpgradeCost;
            Run.bonusHealthFlasks++;
            Run.healthFlasks++;
        }

        // ------------------------------------------------------------------ Śmierć / pauza

        void OnPlayerDied()
        {
            if (Run == null || Run.IsFinished) return;
            Run.IsFinished = true;
            Meta.RecordDeath();
            deathTimer = 1.8f;
        }

        public void TogglePause()
        {
            if (Screen == GameScreen.Playing && !Player.IsDead) SetScreen(GameScreen.Paused);
            else if (Screen == GameScreen.Paused) SetScreen(GameScreen.Playing);
        }

        public void AbandonRun()
        {
            if (Run != null && !Run.IsFinished)
            {
                Run.IsFinished = true;
                Meta.RecordDeath();
            }
            ShowMainMenu();
        }

        // ------------------------------------------------------------------ Dostęp dla UI/testów

        public IReadOnlyList<EnemyBrain> Enemies => enemies;

        /// <summary>Tylko do testów: natychmiast kończy bieżące piętro.</summary>
        public void DebugKillAllEnemies()
        {
            foreach (var e in enemies.ToList()) if (e != null && !e.IsDead) e.DebugKill();
        }

        public void DebugCompleteFloorNow()
        {
            DebugKillAllEnemies();
            CompleteFloor();
        }
    }
}
