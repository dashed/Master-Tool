using BepInEx;
using EFT.InventoryLogic;
using HarmonyLib;
using MasterTool.Config;
using MasterTool.ESP;
using MasterTool.Features.BigHeadMode;
using MasterTool.Features.DoorUnlock;
using MasterTool.Features.GodMode;
using MasterTool.Features.InfiniteStamina;
using MasterTool.Features.Performance;
using MasterTool.Features.Speedhack;
using MasterTool.Features.Vision;
using MasterTool.UI;
using UnityEngine;

namespace MasterTool.Plugin
{
    /// <summary>
    /// Main BepInEx plugin entry point. Initializes config, applies Harmony patches,
    /// and orchestrates per-frame updates for all feature and ESP modules.
    /// </summary>
    [BepInPlugin("com.master.tools", "Advanced SPT Mod Menu", "2.1.2")]
    public sealed class MasterToolPlugin : BaseUnityPlugin
    {
        internal static MasterToolPlugin Instance;
        private const int WindowId = 987654;

        // State
        private Harmony _harmony;
        private readonly GameState _gameState = new GameState();
        private bool _showUi;
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

        private void Awake()
        {
            Instance = this;

            PluginConfig.Initialize(Config);

            _harmony = new Harmony("com.master.tools");
            DamagePatches.PatchAll(_harmony);
            _chams.Initialize();

            Logger.LogInfo("[MasterTool] Loaded. INSERT: UI, Numpad 0-9: Toggles.");
        }

        private void OnDestroy()
        {
            try { _harmony?.UnpatchSelf(); } catch { }
        }

        private void Update()
        {
            HandleHotkeys();

            _gameState.Update();
            PluginConfig.LocalActiveHealthController = _gameState.LocalActiveHealthController;

            var localPlayer = _gameState.LocalPlayer;
            var gameWorld = _gameState.GameWorld;
            var mainCamera = _gameState.MainCamera;

            if (localPlayer == null) return;

            if (PluginConfig.InfiniteStaminaEnabled.Value)
                StaminaFeature.Apply(localPlayer);

            CullingFeature.Apply(gameWorld, localPlayer);

            if (PluginConfig.EspEnabled.Value)
                _playerEsp.Update(gameWorld, mainCamera, localPlayer);

            _chams.Update(gameWorld, mainCamera);

            if (PluginConfig.ItemEspEnabled.Value || PluginConfig.ContainerEspEnabled.Value)
                _itemEsp.Update(gameWorld, mainCamera, localPlayer);

            if (PluginConfig.QuestEspEnabled.Value)
                _questEsp.Update(gameWorld, mainCamera, localPlayer, _itemEsp.CachedContainers);

            if (PluginConfig.SpeedhackEnabled.Value)
                SpeedhackFeature.Apply(localPlayer);

            _vision.UpdateThermalVision(mainCamera);

            if (localPlayer.HandsController == null) return;
            if (!(localPlayer.HandsController.Item is Weapon)) return;

            _vision.UpdateNightVision(mainCamera);
            _vision.UpdateWeaponFov(mainCamera, localPlayer);

            if (gameWorld != null)
                BigHeadFeature.Apply(gameWorld);
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
                _windowRect = GUI.Window(WindowId, _windowRect, id =>
                    _modMenu.Draw(id, _windowRect, gameWorld, localPlayer),
                    "Advanced SPT Mod Menu");
            }
        }

        private void HandleHotkeys()
        {
            if (PluginConfig.ToggleChamsHotkey.Value.IsDown()) PluginConfig.ChamsEnabled.Value = !PluginConfig.ChamsEnabled.Value;
            if (PluginConfig.ToggleUiHotkey.Value.IsDown()) _showUi = !_showUi;
            if (PluginConfig.ToggleStatusHotkey.Value.IsDown()) PluginConfig.StatusWindowEnabled.Value = !PluginConfig.StatusWindowEnabled.Value;
            if (PluginConfig.ToggleWeaponInfoHotkey.Value.IsDown()) PluginConfig.ShowWeaponInfo.Value = !PluginConfig.ShowWeaponInfo.Value;
            if (PluginConfig.ToggleGodModeHotkey.Value.IsDown()) PluginConfig.GodModeEnabled.Value = !PluginConfig.GodModeEnabled.Value;
            if (PluginConfig.ToggleStaminaHotkey.Value.IsDown()) PluginConfig.InfiniteStaminaEnabled.Value = !PluginConfig.InfiniteStaminaEnabled.Value;
            if (PluginConfig.ToggleWeightHotkey.Value.IsDown()) PluginConfig.NoWeightEnabled.Value = !PluginConfig.NoWeightEnabled.Value;
            if (PluginConfig.ToggleEspHotkey.Value.IsDown()) PluginConfig.EspEnabled.Value = !PluginConfig.EspEnabled.Value;
            if (PluginConfig.ToggleItemEspHotkey.Value.IsDown()) PluginConfig.ItemEspEnabled.Value = !PluginConfig.ItemEspEnabled.Value;
            if (PluginConfig.ToggleContainerEspHotkey.Value.IsDown()) PluginConfig.ContainerEspEnabled.Value = !PluginConfig.ContainerEspEnabled.Value;
            if (PluginConfig.ToggleCullingHotkey.Value.IsDown()) PluginConfig.PerformanceMode.Value = !PluginConfig.PerformanceMode.Value;
            if (PluginConfig.ToggleUnlockDoorsHotkey.Value.IsDown()) DoorUnlockFeature.UnlockAll();
            if (PluginConfig.ToggleQuestEspHotkey.Value.IsDown()) PluginConfig.QuestEspEnabled.Value = !PluginConfig.QuestEspEnabled.Value;
        }
    }
}
