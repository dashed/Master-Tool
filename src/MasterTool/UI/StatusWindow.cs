using System;
using EFT;
using EFT.InventoryLogic;
using MasterTool.Config;
using MasterTool.Plugin;
using UnityEngine;

namespace MasterTool.UI
{
    /// <summary>
    /// Renders a small HUD overlay showing the on/off state of major features
    /// and optionally the current weapon name and ammo count.
    /// </summary>
    public class StatusWindow
    {
        private Rect _rect = new Rect(Screen.width - 210, 20, 200, 165);
        private bool _errorLogged;

        /// <summary>
        /// Draws the status window box showing feature toggles and optional weapon information.
        /// Must be called from OnGUI.
        /// </summary>
        /// <param name="style">The box <see cref="GUIStyle"/> used for the status window background.</param>
        /// <param name="localPlayer">The local player, used to read weapon info when enabled.</param>
        public void Draw(GUIStyle style, Player localPlayer)
        {
            string status = "<b>[ MOD STATUS ]</b>\n";
            status +=
                $"GodMode: <color={(PluginConfig.GodModeEnabled.Value ? "green" : "red")}>{(PluginConfig.GodModeEnabled.Value ? "ON" : "OFF")}</color>\n";
            status +=
                $"Stamina: <color={(PluginConfig.InfiniteStaminaEnabled.Value ? "green" : "red")}>{(PluginConfig.InfiniteStaminaEnabled.Value ? "ON" : "OFF")}</color>\n";
            status +=
                $"Weight: <color={(PluginConfig.NoWeightEnabled.Value ? "green" : "red")}>{(PluginConfig.NoWeightEnabled.Value ? "ON" : "OFF")}</color>\n";
            status +=
                $"Energy: <color={(PluginConfig.InfiniteEnergyEnabled.Value ? "green" : "red")}>{(PluginConfig.InfiniteEnergyEnabled.Value ? "ON" : "OFF")}</color>\n";
            status +=
                $"Hydration: <color={(PluginConfig.InfiniteHydrationEnabled.Value ? "green" : "red")}>{(PluginConfig.InfiniteHydrationEnabled.Value ? "ON" : "OFF")}</color>\n";
            status +=
                $"Fall Dmg: <color={(PluginConfig.NoFallDamageEnabled.Value ? "green" : "red")}>{(PluginConfig.NoFallDamageEnabled.Value ? "ON" : "OFF")}</color>\n";
            status +=
                $"ESP: <color={(PluginConfig.EspEnabled.Value ? "green" : "red")}>{(PluginConfig.EspEnabled.Value ? "ON" : "OFF")}</color>\n";
            status +=
                $"Quest ESP: <color={(PluginConfig.QuestEspEnabled.Value ? "green" : "red")}>{(PluginConfig.QuestEspEnabled.Value ? "ON" : "OFF")}</color>\n";

            if (PluginConfig.ShowWeaponInfo.Value && localPlayer != null)
            {
                try
                {
                    var handsController = localPlayer.HandsController;
                    if (handsController != null && handsController.Item is Weapon weapon)
                    {
                        string weaponName = weapon.ShortName.Localized();
                        int currentAmmo = weapon.GetCurrentMagazineCount();
                        int maxAmmo = weapon.GetMaxMagazineCount();

                        status += $"\n<b>[ WEAPON ]</b>\n";
                        status += $"Name: <color=yellow>{weaponName}</color>\n";
                        status += $"Ammo: <color=cyan>{currentAmmo}/{maxAmmo}</color>";
                    }
                }
                catch (Exception ex)
                {
                    if (!_errorLogged)
                    {
                        MasterToolPlugin.Log?.LogWarning($"[StatusWindow] {ex.Message}");
                        _errorLogged = true;
                    }
                }
            }

            float height = PluginConfig.ShowWeaponInfo.Value ? 260 : 200;
            _rect.height = height;
            GUI.Box(_rect, status, style);
        }
    }
}
