using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Turris
{
    public enum GameScreen { MainMenu, Unlocks, Loadout, Playing, Reward, Intermission, Equipment, Paused, Death, Victory, Controls, Bestiary }

    /// <summary>
    /// Punkt wejścia i przepływ gry: menu → przygotowanie → piętra → nagrody/kapliczka → śmierć lub zwycięstwo.
    /// Scena zawiera tylko ten komponent, kamerę i światło; arena i postacie powstają w czasie działania.
    /// </summary>
    public partial class GameRoot : MonoBehaviour
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
        FloorDefinition trialFloor;
        public bool IsEnemyTrial => trialFloor != null;
        public FloorDefinition CurrentFloor => Run != null ? trialFloor ?? (Run.floorIndex < config.tower.floors.Count ? config.tower.floors[Run.floorIndex] : null) : null;
        public bool AtRestPoint { get; private set; }
        /// <summary>Oferta sklepu dusz: losowa umiejętność na to podejście (null = brak/kupiona).</summary>
        public SpellDefinition ShopOffer { get; private set; }
        /// <summary>Nagrody można przerzucić raz na piętro.</summary>
        public bool RerollUsed { get; private set; }
        public string LastFloorSummary { get; private set; }
        public int LastAsh { get; private set; }
        public string ProfilePath { get; private set; }
        public AudioService Audio { get; private set; }
        /// <summary>Między falami: kolejna fala zaraz wejdzie (muzyka walki gra dalej).</summary>
        public bool WaveIncoming => nextWaveTimer > 0;

        readonly List<EnemyBrain> enemies = new List<EnemyBrain>();
        Transform world;
        GameObject arena;
        Light sun;
        System.Random rng;
        float clearTimer = -1f;
        float nextWaveTimer = -1f;
        /// <summary>Bieżąca fala na piętrze (0 = pierwsza).</summary>
        public int WaveIndex { get; private set; }
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
            if (GetComponent<ImpactDirector>() == null) gameObject.AddComponent<ImpactDirector>();
            ProfilePath = Path.Combine(Application.persistentDataPath, ProfileFileOverride ?? profileFileName);
            Meta = new MetaService(config, new FileProfileStorage(ProfilePath));
            if (Meta.Warning != null) Debug.LogWarning("[Turris] " + Meta.Warning);
            Audio = GetComponent<AudioService>();
            if (Audio == null) Audio = gameObject.AddComponent<AudioService>();
            Audio.Bind(this);
            Audio.SetVolumes(Meta.Profile.masterVolume, Meta.Profile.musicVolume, Meta.Profile.sfxVolume);
            Rumble.Strength = Meta.Profile.rumbleStrength;

            world = new GameObject("World").transform;
            SetupSceneObjects();
            CombatEvents.Died += OnSomethingDied;
            EnemyBrain.SummonRequested += OnSummonRequested;
            ShowMainMenu();
        }

        void OnDestroy()
        {
            CombatEvents.Died -= OnSomethingDied;
            EnemyBrain.SummonRequested -= OnSummonRequested;
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
            sun.intensity = RenderingSetup.SunIntensity;

            var cam = Camera.main;
            if (cam == null)
            {
                var c = new GameObject("Main Camera");
                c.tag = "MainCamera";
                cam = c.AddComponent<Camera>();
                c.AddComponent<AudioListener>();
            }
            cam.nearClipPlane = 0.1f;
            RenderingSetup.Prepare(cam);
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
            Input = CameraRig.input;
            Input.EnsureMap();
            Input.LoadOverrides(Meta.Profile.bindingOverrides);
            Player.Died += OnPlayerDied;
            p.SetActive(false);

            QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 8);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 60f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.32f, 0.3f, 0.33f) * RenderingSetup.AmbientScale;
        }

        // ================================================================== Menu

        public void ShowMainMenu()
        {
            Range?.End();
            Range = null;
            trialFloor = null;
            ClearWorld();
            Run = null;
            Player.gameObject.SetActive(false);
            SetScreen(GameScreen.MainMenu);
            BuildMenuBackdrop();
        }

        void BuildMenuBackdrop()
        {
            var backdrop = config.tower.floors[config.tower.floors.Count - 1].arena;
            arena = WorldBuilder.BuildArena(backdrop, world);
            Audio.SetArena(ArenaBuilder.StyleOf(backdrop));
            var cam = CameraRig.transform;
            CameraRig.enabled = false;
            cam.position = new Vector3(0, 6, -14);
            cam.rotation = Quaternion.Euler(18, 0, 0);
        }

        public void ShowUnlocks() => SetScreen(GameScreen.Unlocks);
        public void ShowBestiary() => SetScreen(GameScreen.Bestiary);
        public void StartEnemyTrial(EnemyDefinition enemy)
        {
            if (enemy == null) return;
            var plan = Meta.LastPlan();
            plan.trialEnemy = enemy;
            StartRun(plan);
        }

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
            Time.timeScale = s == GameScreen.Paused || s == GameScreen.Equipment || (s == GameScreen.Controls && Run != null) ? 0f : playing ? gameSpeed : 1f;
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
            trialFloor = plan.trialEnemy != null ? new FloorDefinition { name = "Próba: " + plan.trialEnemy.displayName, arena = config.tower.floors[1].arena, enemies = new List<EnemyDefinition> { plan.trialEnemy }, isDuel = true } : pendingRangeFloor;
            if (!IsEnemyTrial) { Meta.RecordRunStart(plan); Meta.RememberSkillSlots(plan.skillSlots); }
            int seed = System.Environment.TickCount;
            rng = new System.Random(seed);
            Run = RunFactory.Create(plan, config, seed);
            eliteCountdown = -1;
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
            var floor = CurrentFloor;
            arena = WorldBuilder.BuildArena(floor.arena, world);
            Audio.SetArena(ArenaBuilder.StyleOf(floor.arena));
            Sfx.Ui(floor.isBoss ? SoundId.BossIntro : SoundId.FloorStart);
            sun.color = floor.arena.lightColor;
            RenderSettings.fogColor = floor.arena.fogColor;
            if (CameraRig.Camera != null) { CameraRig.Camera.backgroundColor = floor.arena.fogColor; CameraRig.Camera.clearFlags = CameraClearFlags.SolidColor; }

            Controller.Teleport(WorldBuilder.PlayerSpawn(floor.arena), Quaternion.identity);
            Player.Actions.Reset();
            Player.OnFloorStart();
            CameraRig.SnapBehindTarget();

            WaveIndex = 0;
            nextWaveTimer = -1f;
            SpawnWave(floor, floor.enemies, Run.difficulty.elites && floor.isDuel && !floor.isBoss, false);
            StartPortals(floor);

            if (!IsEnemyTrial) Meta.RecordReachedFloor(index);
            clearTimer = -1f;
            deathTimer = -1f;
            SetScreen(GameScreen.Playing);
            CombatEvents.RaiseMessage(floor.name, new Color(0.95f, 0.85f, 0.6f));
        }

        /// <summary>
        /// Wprowadza falę wrogów. Kolejne fale wchodzą po przeciwnej stronie areny niż stoi gracz,
        /// żeby nie pojawiały się mu pod nosem.
        /// </summary>
        void SpawnWave(FloorDefinition floor, List<EnemyDefinition> defs, bool elite, bool oppositePlayer)
        {
            // Fala bez bossa: wrogowie wychodzą z portali po kolei, w różnych miejscach (GameRoot.Portals).
            if (!defs.Any(d => d != null && d.isBoss)) { QueueWave(defs, elite); return; }
            var spawns = WorldBuilder.EnemySpawns(floor.arena, defs.Count);
            if (oppositePlayer && Player != null)
            {
                Vector3 pp = Player.transform.position; pp.y = 0;
                float playerAngle = pp.sqrMagnitude > 0.01f ? Mathf.Atan2(pp.x, pp.z) * Mathf.Rad2Deg : 180f;
                var rot = Quaternion.Euler(0, playerAngle + 180f, 0);
                for (int i = 0; i < spawns.Count; i++) spawns[i] = rot * spawns[i];
            }
            for (int i = 0; i < defs.Count; i++)
            {
                // Wariant żywiołu (np. płonący ghul) zamiast zwykłego wroga – z szansą z definicji.
                var def = defs[i];
                if (def == null) continue;
                if (def.variants != null && def.variants.Count > 0 && rng.NextDouble() < def.variantChance)
                    def = def.variants[rng.Next(def.variants.Count)] ?? def;
                if (!IsEnemyTrial) Meta.RecordSeen(def);
                var e = WorldBuilder.CreateEnemy(def, spawns[i], world);
                e.transform.rotation = Quaternion.LookRotation(-new Vector3(spawns[i].x, 0, spawns[i].z).normalized);
                e.Setup(def, Player, floor.statScale, Run.difficulty, elite, config.balance);
                enemies.Add(e);
            }
            if (oppositePlayer) foreach (var e in enemies) if (e != null && !e.IsDead) FxLibrary.Dust(e.transform.position, 1.5f);
        }

        void ClearWorld()
        {
            foreach (var e in enemies) if (e != null) Destroy(e.gameObject);
            enemies.Clear();
            StopPortals();
            if (arena != null) Destroy(arena);
            foreach (var p in Projectile.Active.ToList()) Destroy(p.gameObject);
            foreach (var z in DamageZone.Active.ToList()) Destroy(z.gameObject);
            foreach (var wave in WaveEffect.Active.ToArray()) Destroy(wave.gameObject);
            foreach (var d in DelayedStrike.Active.ToList()) Destroy(d.gameObject);
            foreach (var st in StormEffect.Active.ToList()) Destroy(st.gameObject);
            foreach (var tw in ThrownWeapon.Active.ToList()) Destroy(tw.gameObject);
            foreach (var f in FxFade.Active.ToList()) Destroy(f.gameObject);
            arena = null;
        }

        void OnSomethingDied(IHitReceiver who)
        {
            if (Run == null || !(who is EnemyBrain e) || !enemies.Contains(e)) return;
            if (Range != null) { Range.OnDummyDied(e); return; }
            Run.kills++;
            if (!IsEnemyTrial) Meta.RecordKill(e.Def);
            Run.souls += Mathf.RoundToInt(e.Def.soulReward * Run.difficulty.soulMultiplier * (e.IsElite ? 1.5f : 1f));
            Sfx.Ui(SoundId.SoulGain);
            SealDrop(e);
            if (enemies.All(x => x == null || x.IsDead) && !Player.IsDead && !PortalsRunning)
            {
                var floor = CurrentFloor;
                if (floor != null && WaveIndex + 1 < floor.WaveCount)
                {
                    nextWaveTimer = 2f;
                    Sfx.Ui(SoundId.WaveHorn);
                    CombatEvents.RaiseMessage($"Fala {WaveIndex + 2}/{floor.WaveCount} nadchodzi…", new Color(1f, 0.75f, 0.4f));
                }
                else clearTimer = 1.5f;
            }
        }

        void Update()
        {
            if (Screen == GameScreen.Playing && Run != null) TickPortals(Time.deltaTime);
            if (Screen == GameScreen.Playing && Range != null) Range.Tick(Time.deltaTime);
            if (Screen == GameScreen.Playing && nextWaveTimer > 0)
            {
                nextWaveTimer -= Time.deltaTime;
                if (nextWaveTimer <= 0) StartNextWave();
            }
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

        void StartNextWave()
        {
            nextWaveTimer = -1f;
            var floor = CurrentFloor;
            var waves = floor.extraWaves.Where(w => w != null && w.enemies.Count > 0).ToList();
            if (WaveIndex >= waves.Count) { clearTimer = 1.5f; return; }
            WaveIndex++;
            SpawnWave(floor, waves[WaveIndex - 1].enemies, false, true);
            CombatEvents.RaiseMessage($"Fala {WaveIndex + 1}/{floor.WaveCount}", new Color(1f, 0.6f, 0.3f));
        }

        void CompleteFloor()
        {
            if (IsEnemyTrial) { Sfx.Ui(SoundId.Victory); ShowMainMenu(); ShowBestiary(); return; }
            clearTimer = -1f;
            nextWaveTimer = -1f;
            var floor = CurrentFloor;
            Run.floorsCleared++;
            Run.attributePoints += config.balance.attributePointsPerFloor;
            LastAsh = Meta.AwardFloorClear(Run, Run.floorIndex);
            Run.healthFraction = Player.Health.Fraction;
            Run.manaFraction = Player.Mana.Fraction;

            if (floor.isBoss || Run.floorIndex >= config.tower.floors.Count - 1)
            {
                LastAsh += Meta.AwardVictory(Run);
                Run.IsFinished = true;
                Run.Victory = true;
                Sfx.Ui(SoundId.Victory);
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

            Sfx.Ui(SoundId.FloorCleared);
            LastFloorSummary = $"{floor.name} ukończone. Popiół +{LastAsh}.";
            Rewards = RewardGenerator.Generate(Run, Player.Build, Meta.BuildRewardPools(), config, rng);
            RerollUsed = false;
            ShopOffer = SoulShop.PickOffer(Run, Player.Build, Meta.BuildRewardPools(), rng, config.balance);
            PruneGifts();
            RollSealOffers();
            SetScreen(GameScreen.Reward);
        }

        public void ChooseReward(int index)
        {
            if (Screen != GameScreen.Reward) return;
            if (Rewards.Count > 0)
            {
                if (index < 0 || index >= Rewards.Count) return;
                Rewards[index].Apply(Run);
                Sfx.Ui(SoundId.UiReward);
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
            Sfx.Ui(SoundId.UiBuy);
            Player.RefreshBuild();
        }

        public void BuyFlask()
        {
            if (!AtRestPoint || Run.bonusHealthFlasks >= 2 || Run.souls < FlaskUpgradeCost) return;
            Run.souls -= FlaskUpgradeCost;
            Run.bonusHealthFlasks++;
            Run.healthFlasks++;
            Sfx.Ui(SoundId.UiBuy);
        }

        // ------------------------------------------------------------------ Sklep dusz

        public bool CanReroll => Screen == GameScreen.Reward && !RerollUsed && Rewards.Count > 0 && Run.souls >= SoulShop.RerollCost(Run, config.balance);

        public void RerollRewards()
        {
            if (!CanReroll) return;
            Run.souls -= SoulShop.RerollCost(Run, config.balance);
            Rewards = RewardGenerator.Generate(Run, Player.Build, Meta.BuildRewardPools(), config, rng);
            RerollUsed = true;
            Sfx.Ui(SoundId.UiBuy);
        }

        public void SpendAttributePoint(AttributeType a)
        {
            if (Run.SpendAttributePoint(a)) { Player.RefreshBuild(); Sfx.Ui(SoundId.UiLevelUp); }
        }

        public void UpgradeSkill(SpellInstance s)
        {
            if (SoulShop.TryUpgrade(s, Run, config.balance)) { Player.RefreshBuild(); Sfx.Ui(SoundId.UiBuy); }
        }

        public void BuyShopOffer()
        {
            if (!SoulShop.TryBuyOffer(ShopOffer, Run, config.balance)) return;
            Sfx.Ui(SoundId.UiBuy);
            CombatEvents.RaiseMessage($"Nowa umiejętność: {ShopOffer.displayName}", ShopOffer.color);
            ShopOffer = null;
            Player.RefreshBuild();
        }

        // ------------------------------------------------------------------ Śmierć / pauza

        void OnPlayerDied()
        {
            if (Run == null || Run.IsFinished) return;
            Run.IsFinished = true;
            if (!IsEnemyTrial) Meta.RecordDeath();
            Sfx.Ui(SoundId.PlayerDeath);
            deathTimer = 1.8f;
        }

        /// <summary>Czytnik wejścia gracza (przypisania przycisków, podpowiedzi w HUD).</summary>
        public PlayerInputReader Input { get; private set; }
        GameScreen screenBeforeControls;

        public void ShowControls()
        {
            screenBeforeControls = Screen;
            SetScreen(GameScreen.Controls);
        }

        public void CloseControls()
        {
            Input.CancelRebind();
            SaveBindings();
            SetScreen(screenBeforeControls == GameScreen.Controls ? GameScreen.MainMenu : screenBeforeControls);
        }

        public void SaveBindings()
        {
            Meta.Profile.bindingOverrides = Input.SaveOverrides();
            Meta.Save();
        }

        /// <summary>Zmienia głośność (0 – ogólna, 1 – muzyka, 2 – efekty) o krok i zapisuje w profilu.</summary>
        public void ChangeVolume(int channel, float delta)
        {
            var p = Meta.Profile;
            float Step(float v) => Mathf.Clamp01(Mathf.Round((v + delta) * 10f) / 10f);
            if (channel == 0) p.masterVolume = Step(p.masterVolume);
            else if (channel == 1) p.musicVolume = Step(p.musicVolume);
            else if (channel == 3) { p.rumbleStrength = Step(p.rumbleStrength); Rumble.Strength = p.rumbleStrength; if (p.rumbleStrength > 0) Rumble.Play(0.5f, 0.5f, 0.2f); }
            else p.sfxVolume = Step(p.sfxVolume);
            Audio.SetVolumes(p.masterVolume, p.musicVolume, p.sfxVolume);
            Meta.Save();
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
                if (!IsEnemyTrial) Meta.RecordDeath();
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
            StopPortals();
            DebugKillAllEnemies();
            CompleteFloor();
        }
    }
}
