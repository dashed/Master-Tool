using EFT;
using MasterTool.Config;
using MasterTool.Features.DoorUnlock;
using MasterTool.Features.Teleport;
using UnityEngine;

namespace MasterTool.UI
{
    /// <summary>
    /// Draws the main IMGUI mod menu window with tabbed sections for general settings,
    /// ESP configuration, visual toggles, and hotkey display. Supports window resizing.
    /// </summary>
    public class ModMenu
    {
        private int _selectedTab;
        private readonly string[] _tabNames = { "Geral", "ESP Players", "ESP Itens", "ESP Quest/Wish", "Visual", "Troll", "Configs" };
        private Vector2 _mainScroll;
        private Vector2 _itemFilterScroll;

        private bool _isResizing;
        private Vector2 _resizeStartMouse;
        private Rect _resizeStartRect;
        private const float ResizeGripSize = 18f;
        private const float MinWindowWidth = 400f;
        private const float MinWindowHeight = 500f;
        private const float DragBarHeight = 22f;

        /// <summary>
        /// Renders the mod menu content inside a GUI.Window callback. Draws the tab toolbar,
        /// the active tab's controls, and handles window resize via the bottom-right grip.
        /// </summary>
        /// <param name="id">The GUI window ID.</param>
        /// <param name="windowRect">The current window rectangle (used for layout and resize).</param>
        /// <param name="gameWorld">The current game world, passed to tabs that need it.</param>
        /// <param name="localPlayer">The local player, passed to tabs that need it.</param>
        public void Draw(int id, Rect windowRect, GameWorld gameWorld, Player localPlayer)
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            GUILayout.Space(10);

            GUI.DragWindow(new Rect(0, 0, windowRect.width, DragBarHeight));
            GUILayout.BeginArea(new Rect(10, DragBarHeight + 5, windowRect.width - 20, windowRect.height - DragBarHeight - 20));
            _mainScroll = GUILayout.BeginScrollView(_mainScroll);

            switch (_selectedTab)
            {
                case 0:
                    DrawGeneralTab();
                    break;
                case 1:
                    DrawPlayerEspTab();
                    break;
                case 2:
                    DrawItemEspTab();
                    break;
                case 3:
                    DrawQuestEspTab();
                    break;
                case 4:
                    DrawVisualTab(gameWorld, localPlayer);
                    break;
                case 5:
                    DrawTrollTab(gameWorld, localPlayer);
                    break;
                case 6:
                    DrawConfigsTab();
                    break;
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            HandleResize(windowRect);
        }

        private void DrawGeneralTab()
        {
            GUILayout.Space(20);
            GUILayout.Label("<b>--- GENERAL ---</b>");
            PluginConfig.GodModeEnabled.Value = GUILayout.Toggle(
                PluginConfig.GodModeEnabled.Value,
                $" GodMode [{PluginConfig.ToggleGodModeHotkey.Value}]"
            );
            if (!PluginConfig.GodModeEnabled.Value)
            {
                GUILayout.Label($"Damage Received: {PluginConfig.DamageReductionPercent.Value}%");
                PluginConfig.DamageReductionPercent.Value = (int)
                    GUILayout.HorizontalSlider(PluginConfig.DamageReductionPercent.Value, 0f, 100f);
            }
            PluginConfig.Keep1HealthEnabled.Value = GUILayout.Toggle(PluginConfig.Keep1HealthEnabled.Value, " Keep 1 Health (Anti-Lethal)");
            if (PluginConfig.Keep1HealthEnabled.Value)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Protect: ", GUILayout.Width(60));
                if (GUILayout.Button(PluginConfig.Keep1HealthSelection.Value))
                {
                    PluginConfig.Keep1HealthSelection.Value = PluginConfig.Keep1HealthSelection.Value == "All" ? "Head And Thorax" : "All";
                }
                GUILayout.EndHorizontal();
            }
            PluginConfig.IgnoreHeadshots.Value = GUILayout.Toggle(PluginConfig.IgnoreHeadshots.Value, " Ignore Headshot Damage");
            if (!PluginConfig.IgnoreHeadshots.Value)
            {
                GUILayout.Label($"Head Damage: {PluginConfig.HeadDamagePercent.Value}%");
                PluginConfig.HeadDamagePercent.Value = (int)GUILayout.HorizontalSlider(PluginConfig.HeadDamagePercent.Value, 0f, 100f);
            }
            GUILayout.Label($"Enemy Damage: {PluginConfig.EnemyDamageMultiplier.Value:F1}x");
            PluginConfig.EnemyDamageMultiplier.Value = GUILayout.HorizontalSlider(PluginConfig.EnemyDamageMultiplier.Value, 1f, 20f);
            PluginConfig.InfiniteStaminaEnabled.Value = GUILayout.Toggle(
                PluginConfig.InfiniteStaminaEnabled.Value,
                $" Infinite Stamina [{PluginConfig.ToggleStaminaHotkey.Value}]"
            );
            PluginConfig.NoWeightEnabled.Value = GUILayout.Toggle(
                PluginConfig.NoWeightEnabled.Value,
                $" No Weight Penalties [{PluginConfig.ToggleWeightHotkey.Value}]"
            );
            if (PluginConfig.NoWeightEnabled.Value)
            {
                GUILayout.Label($"Weight: {PluginConfig.WeightPercent.Value}%");
                PluginConfig.WeightPercent.Value = (int)GUILayout.HorizontalSlider(PluginConfig.WeightPercent.Value, 0f, 100f);
            }
            PluginConfig.InfiniteEnergyEnabled.Value = GUILayout.Toggle(
                PluginConfig.InfiniteEnergyEnabled.Value,
                $" Infinite Energy [{PluginConfig.ToggleEnergyHotkey.Value}]"
            );
            PluginConfig.InfiniteHydrationEnabled.Value = GUILayout.Toggle(
                PluginConfig.InfiniteHydrationEnabled.Value,
                $" Infinite Hydration [{PluginConfig.ToggleHydrationHotkey.Value}]"
            );
            PluginConfig.NoFallDamageEnabled.Value = GUILayout.Toggle(
                PluginConfig.NoFallDamageEnabled.Value,
                $" No Fall Damage [{PluginConfig.ToggleFallDamageHotkey.Value}]"
            );
            PluginConfig.StatusWindowEnabled.Value = GUILayout.Toggle(
                PluginConfig.StatusWindowEnabled.Value,
                $" Show Status Window [{PluginConfig.ToggleStatusHotkey.Value}]"
            );
            PluginConfig.ShowWeaponInfo.Value = GUILayout.Toggle(
                PluginConfig.ShowWeaponInfo.Value,
                $" Show Weapon Info in Status [{PluginConfig.ToggleWeaponInfoHotkey.Value}]"
            );
            GUILayout.Space(10);
            if (GUILayout.Button($"Unlock All Doors in Raid [{PluginConfig.ToggleUnlockDoorsHotkey.Value}]"))
                DoorUnlockFeature.UnlockAll();

            GUILayout.Space(10);
            GUILayout.Label("<b>--- COD MODE ---</b>");
            PluginConfig.CodModeEnabled.Value = GUILayout.Toggle(
                PluginConfig.CodModeEnabled.Value,
                $" COD Mode (Auto-Heal) [{PluginConfig.ToggleCodModeHotkey.Value}]"
            );
            if (PluginConfig.CodModeEnabled.Value)
            {
                GUILayout.Label($"Heal Rate: {PluginConfig.CodModeHealRate.Value:F0} HP/cycle");
                PluginConfig.CodModeHealRate.Value = GUILayout.HorizontalSlider(PluginConfig.CodModeHealRate.Value, 1f, 100f);
                GUILayout.Label($"Heal Delay: {PluginConfig.CodModeHealDelay.Value:F0}s");
                PluginConfig.CodModeHealDelay.Value = GUILayout.HorizontalSlider(PluginConfig.CodModeHealDelay.Value, 0f, 600f);
                PluginConfig.CodModeRemoveEffects.Value = GUILayout.Toggle(
                    PluginConfig.CodModeRemoveEffects.Value,
                    " Remove Effects (experimental)"
                );
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>--- RELOAD SPEED ---</b>");
            PluginConfig.ReloadSpeedEnabled.Value = GUILayout.Toggle(
                PluginConfig.ReloadSpeedEnabled.Value,
                $" Reload Speed [{PluginConfig.ToggleReloadSpeedHotkey.Value}]"
            );
            if (PluginConfig.ReloadSpeedEnabled.Value)
            {
                GUILayout.Label($"Load Time: {PluginConfig.ReloadLoadTime.Value:F2} (default 0.85)");
                PluginConfig.ReloadLoadTime.Value = GUILayout.HorizontalSlider(PluginConfig.ReloadLoadTime.Value, 0.01f, 2f);
                GUILayout.Label($"Unload Time: {PluginConfig.ReloadUnloadTime.Value:F2} (default 0.30)");
                PluginConfig.ReloadUnloadTime.Value = GUILayout.HorizontalSlider(PluginConfig.ReloadUnloadTime.Value, 0.01f, 2f);
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>--- PERFORMANCE ---</b>");
            PluginConfig.PerformanceMode.Value = GUILayout.Toggle(
                PluginConfig.PerformanceMode.Value,
                $" Enable Bot Distance Culling [{PluginConfig.ToggleCullingHotkey.Value}]"
            );
            GUILayout.Label($"Bot Render Distance: {PluginConfig.BotRenderDistance.Value:F0}m");
            PluginConfig.BotRenderDistance.Value = GUILayout.HorizontalSlider(PluginConfig.BotRenderDistance.Value, 50f, 1000f);

            GUILayout.Space(10);
            GUILayout.Label("<b>--- FOV BY WEAPON ---</b>");
            PluginConfig.WeaponFovEnabled.Value = GUILayout.Toggle(PluginConfig.WeaponFovEnabled.Value, " Enable Weapon Category FOV");
            if (PluginConfig.WeaponFovEnabled.Value)
            {
                DrawFovSlider("Default FOV", PluginConfig.FovDefault);
                DrawFovSlider("Pistol FOV", PluginConfig.FovPistol);
                DrawFovSlider("SMG FOV", PluginConfig.FovSMG);
                DrawFovSlider("Assault Rifle FOV", PluginConfig.FovAssaultRifle);
                DrawFovSlider("Shotgun FOV", PluginConfig.FovShotgun);
                DrawFovSlider("Sniper FOV", PluginConfig.FovSniper);
            }
        }

        private void DrawPlayerEspTab()
        {
            GUILayout.Space(20);
            GUILayout.Label("<b>--- PLAYER ESP ---</b>");
            PluginConfig.EspEnabled.Value = GUILayout.Toggle(
                PluginConfig.EspEnabled.Value,
                $" Enable Player ESP (Text) [{PluginConfig.ToggleEspHotkey.Value}]"
            );
            GUILayout.Label($"ESP Transparency: {PluginConfig.EspTextAlpha.Value:F2}");
            PluginConfig.EspTextAlpha.Value = GUILayout.HorizontalSlider(PluginConfig.EspTextAlpha.Value, 0.1f, 1.0f);
            GUILayout.Label($"ESP Font Size: {PluginConfig.EspFontSize.Value}");
            PluginConfig.EspFontSize.Value = (int)GUILayout.HorizontalSlider(PluginConfig.EspFontSize.Value, 1f, 24f);
            GUILayout.Label($"Max Distance: {PluginConfig.EspMaxDistance.Value:F0}m");
            PluginConfig.EspMaxDistance.Value = GUILayout.HorizontalSlider(PluginConfig.EspMaxDistance.Value, 50f, 1000f);
            GUILayout.Label($"Update Rate (FPS): {1f / PluginConfig.EspUpdateInterval.Value:F0}");
            float pFps = GUILayout.HorizontalSlider(1f / PluginConfig.EspUpdateInterval.Value, 1f, 60f);
            PluginConfig.EspUpdateInterval.Value = 1f / pFps;

            PluginConfig.EspLineOfSightOnly.Value = GUILayout.Toggle(PluginConfig.EspLineOfSightOnly.Value, " Line of Sight Only");

            GUILayout.Label("<b>--- CHAMS SETTINGS ---</b>");
            PluginConfig.ChamsEnabled.Value = GUILayout.Toggle(
                PluginConfig.ChamsEnabled.Value,
                $" Enable Player Chams (Models) [{PluginConfig.ToggleChamsHotkey.Value}]"
            );

            GUILayout.Space(5);
            GUILayout.Label("<b>--- COLORS & TRANSPARENCY (RGB) ---</b>");
            PluginConfig.ColorBear.Value = ColorPicker.Draw("BEAR", PluginConfig.ColorBear.Value);
            PluginConfig.ColorUsec.Value = ColorPicker.Draw("USEC", PluginConfig.ColorUsec.Value);
            PluginConfig.ColorSavage.Value = ColorPicker.Draw("SAVAGE / SCAV", PluginConfig.ColorSavage.Value);
            PluginConfig.ColorBoss.Value = ColorPicker.Draw("BOSS", PluginConfig.ColorBoss.Value);
        }

        private void DrawItemEspTab()
        {
            GUILayout.Space(20);
            GUILayout.Label("<b>--- ITEM & CONTAINER ESP ---</b>");
            PluginConfig.ItemEspEnabled.Value = GUILayout.Toggle(
                PluginConfig.ItemEspEnabled.Value,
                $" Enable Loose Item ESP [{PluginConfig.ToggleItemEspHotkey.Value}]"
            );
            PluginConfig.ContainerEspEnabled.Value = GUILayout.Toggle(
                PluginConfig.ContainerEspEnabled.Value,
                $" Enable Container Item ESP [{PluginConfig.ToggleContainerEspHotkey.Value}]"
            );
            GUILayout.Label("Filter (Name or ID, comma separated):");

            _itemFilterScroll = GUILayout.BeginScrollView(_itemFilterScroll, GUILayout.Height(60), GUILayout.ExpandWidth(true));
            PluginConfig.ItemEspFilter.Value = GUILayout.TextArea(
                PluginConfig.ItemEspFilter.Value,
                GUILayout.Width(260),
                GUILayout.ExpandHeight(true)
            );
            GUILayout.EndScrollView();

            GUILayout.Space(5);
            GUILayout.Label($"Max Distance: {PluginConfig.ItemEspMaxDistance.Value:F0}m");
            PluginConfig.ItemEspMaxDistance.Value = GUILayout.HorizontalSlider(PluginConfig.ItemEspMaxDistance.Value, 5f, 500f);
            GUILayout.Label($"Update Rate (FPS): {1f / PluginConfig.ItemEspUpdateInterval.Value:F0}");
            float iFps = GUILayout.HorizontalSlider(1f / PluginConfig.ItemEspUpdateInterval.Value, 1f, 60f);
            PluginConfig.ItemEspUpdateInterval.Value = 1f / iFps;
            GUILayout.Label($"Item Font Size: {PluginConfig.ItemEspFontSize.Value}");
            PluginConfig.ItemEspFontSize.Value = (int)GUILayout.HorizontalSlider(PluginConfig.ItemEspFontSize.Value, 6f, 20f);
        }

        private void DrawQuestEspTab()
        {
            GUILayout.Space(20);
            GUILayout.Label("<b>--- QUEST ESP ---</b>");
            PluginConfig.QuestEspEnabled.Value = GUILayout.Toggle(
                PluginConfig.QuestEspEnabled.Value,
                $" Enable Quest Item ESP [{PluginConfig.ToggleQuestEspHotkey.Value}]"
            );
            GUILayout.Label($"Quest ESP Distance: {PluginConfig.QuestEspMaxDistance.Value:F0}m");
            PluginConfig.QuestEspMaxDistance.Value = GUILayout.HorizontalSlider(PluginConfig.QuestEspMaxDistance.Value, 10f, 500f);
            PluginConfig.ColorQuestItem.Value = ColorPicker.Draw("Quest Items", PluginConfig.ColorQuestItem.Value);
            GUILayout.Label($"Update FPS: {PluginConfig.QuestEspFps.Value}");
            PluginConfig.QuestEspFps.Value = (int)GUILayout.HorizontalSlider(PluginConfig.QuestEspFps.Value, 1f, 60f);
        }

        private void DrawVisualTab(GameWorld gameWorld, Player localPlayer)
        {
            GUILayout.Space(20);
            GUILayout.Label("<b>--- OP FEATURES ---</b>");
            PluginConfig.ThermalVisionEnabled.Value = GUILayout.Toggle(PluginConfig.ThermalVisionEnabled.Value, " Thermal Vision");
            PluginConfig.NightVisionEnabled.Value = GUILayout.Toggle(PluginConfig.NightVisionEnabled.Value, " Night Vision");
            if (GUILayout.Button("Teleport Filtered Items to Me"))
                TeleportFeature.TeleportFilteredItemsToPlayer(gameWorld, localPlayer);
        }

        private void DrawTrollTab(GameWorld gameWorld, Player localPlayer)
        {
            GUILayout.Space(20);
            PluginConfig.SpeedhackEnabled.Value = GUILayout.Toggle(PluginConfig.SpeedhackEnabled.Value, " Speedhack");
            if (PluginConfig.SpeedhackEnabled.Value)
            {
                GUILayout.Label($"Speed: {PluginConfig.SpeedMultiplier.Value:F1}x");
                PluginConfig.SpeedMultiplier.Value = GUILayout.HorizontalSlider(PluginConfig.SpeedMultiplier.Value, 1f, 10f);
            }
            GUILayout.Space(10);
            GUILayout.Label("<b>--- FLY MODE (NOCLIP) ---</b>");
            PluginConfig.FlyModeEnabled.Value = GUILayout.Toggle(
                PluginConfig.FlyModeEnabled.Value,
                $" Fly Mode [{PluginConfig.ToggleFlyModeHotkey.Value}]"
            );
            if (PluginConfig.FlyModeEnabled.Value)
            {
                GUILayout.Label($"Fly Speed: {PluginConfig.FlySpeed.Value:F1}");
                PluginConfig.FlySpeed.Value = GUILayout.HorizontalSlider(PluginConfig.FlySpeed.Value, 1f, 50f);
                GUILayout.Label("Controls: WASD + Space(up) + Ctrl(down)");
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>--- TELEPORT & SPAWN ---</b>");
            if (GUILayout.Button("Teleport All Enemies to Me"))
                TeleportFeature.TeleportEnemiesToPlayer(gameWorld, localPlayer);

            GUILayout.Space(5);
            GUILayout.Label("<b>--- PLAYER TELEPORT ---</b>");
            if (GUILayout.Button("Save Position"))
                PlayerTeleportFeature.SavePosition(localPlayer);
            if (GUILayout.Button("Load Position" + (PlayerTeleportFeature.HasSavedPosition ? " (Saved)" : "")))
                PlayerTeleportFeature.LoadPosition(localPlayer);
            if (GUILayout.Button("Teleport to Surface (Rescue)"))
                PlayerTeleportFeature.TeleportToSurface(localPlayer);

            GUILayout.Label("<b>--- FUN ---</b>");

            PluginConfig.BigHeadModeEnabled.Value = GUILayout.Toggle(PluginConfig.BigHeadModeEnabled.Value, " Big Head Mode");
            if (PluginConfig.BigHeadModeEnabled.Value)
            {
                GUILayout.Label($"Head Size: {PluginConfig.HeadSizeMultiplier.Value:F1}x");
                PluginConfig.HeadSizeMultiplier.Value = GUILayout.HorizontalSlider(PluginConfig.HeadSizeMultiplier.Value, 1f, 5f);
            }
        }

        private void DrawConfigsTab()
        {
            GUILayout.Space(20);
            GUILayout.Label("<b>--- HOTKEYS (Customizable in .cfg) ---</b>");
            GUILayout.Label($"Menu: {PluginConfig.ToggleUiHotkey.Value}");
            GUILayout.Label($"Status Window: {PluginConfig.ToggleStatusHotkey.Value}");
            GUILayout.Label($"GodMode: {PluginConfig.ToggleGodModeHotkey.Value}");
            GUILayout.Label($"Stamina: {PluginConfig.ToggleStaminaHotkey.Value}");
            GUILayout.Label($"Weight: {PluginConfig.ToggleWeightHotkey.Value}");
            GUILayout.Label($"Player ESP: {PluginConfig.ToggleEspHotkey.Value}");
            GUILayout.Label($"Item ESP: {PluginConfig.ToggleItemEspHotkey.Value}");
            GUILayout.Label($"Container ESP: {PluginConfig.ToggleContainerEspHotkey.Value}");
            GUILayout.Label($"Culling: {PluginConfig.ToggleCullingHotkey.Value}");
            GUILayout.Label($"Unlock Doors: {PluginConfig.ToggleUnlockDoorsHotkey.Value}");
        }

        private static void DrawFovSlider(string label, BepInEx.Configuration.ConfigEntry<float> config)
        {
            GUILayout.Label($"{label}: {config.Value:F0}");
            config.Value = GUILayout.HorizontalSlider(config.Value, 50f, 120f);
        }

        private void HandleResize(Rect windowRect)
        {
            var gripRect = new Rect(windowRect.width - ResizeGripSize, windowRect.height - ResizeGripSize, ResizeGripSize, ResizeGripSize);
            GUI.Box(gripRect, "///");
            var e = Event.current;
            if (e.type == EventType.MouseDown && gripRect.Contains(e.mousePosition))
            {
                _isResizing = true;
                _resizeStartMouse = e.mousePosition;
                _resizeStartRect = windowRect;
                e.Use();
            }
            if (_isResizing && e.type == EventType.MouseDrag)
            {
                var delta = e.mousePosition - _resizeStartMouse;
                windowRect.width = Mathf.Max(MinWindowWidth, _resizeStartRect.width + delta.x);
                windowRect.height = Mathf.Max(MinWindowHeight, _resizeStartRect.height + delta.y);
                e.Use();
            }
            if (e.type == EventType.MouseUp)
                _isResizing = false;
        }
    }
}
