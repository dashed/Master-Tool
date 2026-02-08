using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MasterTool.Config;
using MasterTool.ESP;
using MasterTool.Features.BigHeadMode;
using MasterTool.Features.CodMode;
using MasterTool.Features.DoorUnlock;
using MasterTool.Features.FallDamage;
using MasterTool.Features.FlyMode;
using MasterTool.Features.GodMode;
using MasterTool.Features.InfiniteStamina;
using MasterTool.Features.NoWeight;
using MasterTool.Features.PeacefulMode;
using MasterTool.Features.Performance;
using MasterTool.Features.ReloadSpeed;
using MasterTool.Features.Speedhack;
using MasterTool.Features.Sustenance;
using MasterTool.Features.Teleport;
using MasterTool.Features.Vision;
using MasterTool.UI;
using UnityEngine;

namespace MasterTool.Plugin
{
    /// <summary>
    /// Main BepInEx plugin entry point. Initializes config, applies Harmony patches,
    /// and orchestrates per-frame updates for all feature and ESP modules.
    /// </summary>
    [BepInPlugin("com.master.tools", "Advanced SPT Mod Menu", "2.28.0")]
    public sealed class MasterToolPlugin : BaseUnityPlugin
    {
        internal static MasterToolPlugin Instance;
        internal static ManualLogSource Log;
        private const int WindowId = 987654;

        // State
        private Harmony _harmony;
        private readonly GameState _gameState = new GameState();
        private bool _showUi;
        private bool _wasPeacefulEnabled;
        private Rect _windowRect = new Rect(25, 25, 500, 750);

        // Modules
        private readonly VisionFeature _vision = new VisionFeature();
        private readonly PlayerEsp _playerEsp = new PlayerEsp();
        private readonly ItemEsp _itemEsp = new ItemEsp();
        private readonly QuestEsp _questEsp = new QuestEsp();
        private readonly ChamsManager _chams = new ChamsManager();
        private readonly ModMenu _modMenu = new ModMenu();
        private readonly StatusWindow _statusWindow = new StatusWindow();
        private readonly GuiStyles _styles = new GuiStyles();

        internal static void ToggleModMenu()
        {
            if (Instance != null)
            {
                Instance._showUi = !Instance._showUi;
            }
        }

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            PluginConfig.Initialize(Config);

            _harmony = new Harmony("com.master.tools");
            DamagePatches.PatchAll(_harmony);
            NoWeightFeature.PatchAll(_harmony);
            PeacefulPatches.PatchAll(_harmony);
            _chams.Initialize();

            Logger.LogInfo("[MasterTool] Loaded. INSERT: UI, Numpad 0-9: Toggles.");
        }

        private void OnDestroy()
        {
            try
            {
                _harmony?.UnpatchSelf();
            }
            catch (Exception ex)
            {
                Log?.LogWarning($"[MasterTool] Harmony unpatch failed: {ex.Message}");
            }
        }

        private void Update()
        {
            HandleHotkeys();

            _gameState.Update();
            PluginConfig.LocalActiveHealthController = _gameState.LocalActiveHealthController;

            var localPlayer = _gameState.LocalPlayer;
            var gameWorld = _gameState.GameWorld;
            var mainCamera = _gameState.MainCamera;

            if (localPlayer == null)
                return;

            if (PluginConfig.PeacefulModeEnabled.Value && !_wasPeacefulEnabled)
                PeacefulPatches.ClearPlayerFromAllBots(gameWorld, localPlayer);
            _wasPeacefulEnabled = PluginConfig.PeacefulModeEnabled.Value;

            if (PluginConfig.InfiniteStaminaEnabled.Value)
                StaminaFeature.Apply(localPlayer);

            if (PluginConfig.InfiniteEnergyEnabled.Value)
                EnergyFeature.Apply(localPlayer);

            if (PluginConfig.InfiniteHydrationEnabled.Value)
                HydrationFeature.Apply(localPlayer);

            FallDamageFeature.Apply(localPlayer, PluginConfig.NoFallDamageEnabled.Value);

            if (PluginConfig.CodModeEnabled.Value)
                CodModeFeature.Apply(localPlayer);

            ReloadSpeedFeature.Apply(PluginConfig.ReloadSpeedEnabled.Value);

            FlyModeFeature.Apply(localPlayer, mainCamera);

            CullingFeature.Apply(gameWorld, localPlayer);

            if (PluginConfig.EspEnabled.Value)
                _playerEsp.Update(gameWorld, mainCamera, localPlayer);

            _chams.Update(gameWorld, mainCamera);
            _chams.UpdateLootChams(gameWorld, mainCamera, localPlayer);

            if (PluginConfig.ItemEspEnabled.Value || PluginConfig.ContainerEspEnabled.Value)
                _itemEsp.Update(gameWorld, mainCamera, localPlayer);

            if (PluginConfig.QuestEspEnabled.Value)
                _questEsp.Update(gameWorld, mainCamera, localPlayer, _itemEsp.CachedContainers);

            if (PluginConfig.SpeedhackEnabled.Value)
                SpeedhackFeature.Apply(localPlayer);

            _vision.UpdateThermalVision(mainCamera);
            _vision.UpdateNightVision(mainCamera);

            if (gameWorld != null)
                BigHeadFeature.Apply(gameWorld);
        }

        private void LateUpdate()
        {
            var localPlayer = _gameState.LocalPlayer;
            var mainCamera = _gameState.MainCamera;

            if (localPlayer == null || mainCamera == null)
                return;

            _vision.UpdateWeaponFov(mainCamera, localPlayer);
        }

        private void OnGUI()
        {
            _styles.EnsureInitialized();

            var localPlayer = _gameState.LocalPlayer;
            var mainCamera = _gameState.MainCamera;
            var gameWorld = _gameState.GameWorld;

            if (localPlayer != null && mainCamera != null)
            {
                if (PluginConfig.EspEnabled.Value)
                    _playerEsp.Render(_styles.EspLabel);

                if (PluginConfig.ItemEspEnabled.Value || PluginConfig.ContainerEspEnabled.Value)
                    _itemEsp.Render(_styles.ItemEspLabel);

                if (PluginConfig.StatusWindowEnabled.Value)
                    _statusWindow.Draw(_styles.StatusBox, localPlayer);
            }

            if (PluginConfig.QuestEspEnabled.Value)
                _questEsp.Render(_styles.ItemEspLabel);

            if (_showUi)
            {
                _windowRect = GUI.Window(
                    WindowId,
                    _windowRect,
                    id => _modMenu.Draw(id, _windowRect, gameWorld, localPlayer),
                    "Advanced SPT Mod Menu"
                );
            }
        }

        private void HandleHotkeys()
        {
            if (PluginConfig.ToggleChamsHotkey.Value.IsDown())
                PluginConfig.ChamsEnabled.Value = !PluginConfig.ChamsEnabled.Value;
            if (PluginConfig.ToggleUiHotkey.Value.IsDown())
                _showUi = !_showUi;
            if (PluginConfig.ToggleStatusHotkey.Value.IsDown())
                PluginConfig.StatusWindowEnabled.Value = !PluginConfig.StatusWindowEnabled.Value;
            if (PluginConfig.ToggleWeaponInfoHotkey.Value.IsDown())
                PluginConfig.ShowWeaponInfo.Value = !PluginConfig.ShowWeaponInfo.Value;
            if (PluginConfig.ToggleGodModeHotkey.Value.IsDown())
                PluginConfig.GodModeEnabled.Value = !PluginConfig.GodModeEnabled.Value;
            if (PluginConfig.ToggleStaminaHotkey.Value.IsDown())
                PluginConfig.InfiniteStaminaEnabled.Value = !PluginConfig.InfiniteStaminaEnabled.Value;
            if (PluginConfig.ToggleWeightHotkey.Value.IsDown())
                PluginConfig.NoWeightEnabled.Value = !PluginConfig.NoWeightEnabled.Value;
            if (PluginConfig.ToggleEspHotkey.Value.IsDown())
                PluginConfig.EspEnabled.Value = !PluginConfig.EspEnabled.Value;
            if (PluginConfig.ToggleItemEspHotkey.Value.IsDown())
                PluginConfig.ItemEspEnabled.Value = !PluginConfig.ItemEspEnabled.Value;
            if (PluginConfig.ToggleContainerEspHotkey.Value.IsDown())
                PluginConfig.ContainerEspEnabled.Value = !PluginConfig.ContainerEspEnabled.Value;
            if (PluginConfig.ToggleCullingHotkey.Value.IsDown())
                PluginConfig.PerformanceMode.Value = !PluginConfig.PerformanceMode.Value;
            if (PluginConfig.ToggleUnlockDoorsHotkey.Value.IsDown())
                DoorUnlockFeature.UnlockAll();
            if (PluginConfig.ToggleQuestEspHotkey.Value.IsDown())
                PluginConfig.QuestEspEnabled.Value = !PluginConfig.QuestEspEnabled.Value;
            if (PluginConfig.ToggleEnergyHotkey.Value.IsDown())
                PluginConfig.InfiniteEnergyEnabled.Value = !PluginConfig.InfiniteEnergyEnabled.Value;
            if (PluginConfig.ToggleHydrationHotkey.Value.IsDown())
                PluginConfig.InfiniteHydrationEnabled.Value = !PluginConfig.InfiniteHydrationEnabled.Value;
            if (PluginConfig.ToggleFallDamageHotkey.Value.IsDown())
                PluginConfig.NoFallDamageEnabled.Value = !PluginConfig.NoFallDamageEnabled.Value;
            if (PluginConfig.ToggleCodModeHotkey.Value.IsDown())
                PluginConfig.CodModeEnabled.Value = !PluginConfig.CodModeEnabled.Value;
            if (PluginConfig.ToggleReloadSpeedHotkey.Value.IsDown())
                PluginConfig.ReloadSpeedEnabled.Value = !PluginConfig.ReloadSpeedEnabled.Value;
            if (PluginConfig.ToggleFlyModeHotkey.Value.IsDown())
                PluginConfig.FlyModeEnabled.Value = !PluginConfig.FlyModeEnabled.Value;
            if (PluginConfig.SavePositionHotkey.Value.IsDown())
                PlayerTeleportFeature.SavePosition(_gameState.LocalPlayer);
            if (PluginConfig.LoadPositionHotkey.Value.IsDown())
                PlayerTeleportFeature.LoadPosition(_gameState.LocalPlayer);
            if (PluginConfig.SurfaceTeleportHotkey.Value.IsDown())
                PlayerTeleportFeature.TeleportToSurface(_gameState.LocalPlayer);
            if (PluginConfig.TogglePeacefulModeHotkey.Value.IsDown())
                PluginConfig.PeacefulModeEnabled.Value = !PluginConfig.PeacefulModeEnabled.Value;
        }
    }
}
