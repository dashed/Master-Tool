using System;
using System.Reflection;
using BSG.CameraEffects;
using EFT;
using EFT.InventoryLogic;
using MasterTool.Config;
using MasterTool.Core;
using UnityEngine;

namespace MasterTool.Features.Vision
{
    /// <summary>
    /// Controls thermal vision, night vision, and per-weapon-category FOV adjustments
    /// by toggling camera effect components and smoothly lerping the camera field of view.
    /// </summary>
    public class VisionFeature
    {
        private bool _modForcedNvOn;
        private bool _modForcedThermalOn;

        private static PropertyInfo _isAimingProp;
        private static bool _isAimingSearched;

        /// <summary>
        /// Manages the thermal vision camera effect. When the mod toggle is ON, forces thermal
        /// vision active. When toggled OFF, performs a one-time cleanup. When the mod toggle is
        /// OFF and the mod didn't force it, does nothing — allowing vanilla thermal to work.
        /// </summary>
        /// <param name="mainCamera">The active game camera. Ignored if null.</param>
        public void UpdateThermalVision(Camera mainCamera)
        {
            if (mainCamera == null)
                return;
            var thermal = mainCamera.GetComponent<ThermalVision>();
            if (thermal == null)
                return;

            if (PluginConfig.ThermalVisionEnabled.Value)
            {
                if (!thermal.On)
                    thermal.On = true;
                _modForcedThermalOn = true;
            }
            else if (_modForcedThermalOn)
            {
                thermal.On = false;
                _modForcedThermalOn = false;
            }
        }

        /// <summary>
        /// Manages the night vision camera effect. When the mod toggle is ON, forces night
        /// vision active. When toggled OFF, performs a one-time cleanup. When the mod toggle is
        /// OFF and the mod didn't force it, does nothing — allowing vanilla NVGs to work.
        /// </summary>
        /// <param name="mainCamera">The active game camera. Ignored if null.</param>
        public void UpdateNightVision(Camera mainCamera)
        {
            if (mainCamera == null)
                return;
            var nv = mainCamera.GetComponent<NightVision>();
            if (nv == null)
                return;

            if (PluginConfig.NightVisionEnabled.Value)
            {
                if (!nv.On)
                    nv.On = true;
                _modForcedNvOn = true;
            }
            else if (_modForcedNvOn)
            {
                nv.On = false;
                _modForcedNvOn = false;
            }
        }

        /// <summary>
        /// Sets the camera FOV based on the currently equipped weapon category.
        /// Should be called in LateUpdate to run after the game's camera updates.
        /// </summary>
        /// <param name="mainCamera">The active game camera.</param>
        /// <param name="localPlayer">The local player whose equipped weapon determines the target FOV.</param>
        public void UpdateWeaponFov(Camera mainCamera, Player localPlayer)
        {
            if (mainCamera == null || localPlayer == null)
                return;

            if (
                !VisionLogic.ShouldOverrideFov(
                    PluginConfig.WeaponFovEnabled.Value,
                    IsPlayerAiming(localPlayer),
                    PluginConfig.FovOverrideAds.Value
                )
            )
                return;

            float targetFov = GetFovForCurrentWeapon(localPlayer);
            mainCamera.fieldOfView = targetFov;
        }

        /// <summary>
        /// Detects if the player is aiming down sights via ProceduralWeaponAnimation.IsAiming.
        /// Uses reflection since the exact type may vary across game versions.
        /// </summary>
        internal static bool IsPlayerAiming(Player player)
        {
            try
            {
                if (!_isAimingSearched)
                {
                    _isAimingSearched = true;
                    var pwaField = typeof(Player).GetField(
                        "ProceduralWeaponAnimation",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                    if (pwaField != null)
                    {
                        var pwaType = pwaField.FieldType;
                        _isAimingProp = pwaType.GetProperty(
                            "IsAiming",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                        );
                    }
                }

                if (_isAimingProp == null)
                    return false;

                var pwa = typeof(Player)
                    .GetField("ProceduralWeaponAnimation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.GetValue(player);
                if (pwa == null)
                    return false;

                return (bool)_isAimingProp.GetValue(pwa);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the configured FOV value for the weapon category currently held by the player.
        /// Falls back to the default FOV if no weapon is equipped or the category is unrecognized.
        /// </summary>
        /// <param name="localPlayer">The local player to inspect.</param>
        /// <returns>The target FOV value for the current weapon category.</returns>
        public static float GetFovForCurrentWeapon(Player localPlayer)
        {
            if (localPlayer == null || localPlayer.HandsController == null)
                return PluginConfig.FovDefault.Value;

            var item = localPlayer.HandsController.Item;
            if (item == null)
                return PluginConfig.FovDefault.Value;

            string weaponClass = null;
            bool isMelee = false;

            if (item is Weapon weapon)
            {
                var weaponTemplate = weapon.Template;
                if (weaponTemplate == null)
                    return PluginConfig.FovDefault.Value;

                weaponClass = weaponTemplate.weapClass;
            }
            else if (item.GetType().Name.IndexOf("Knife", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isMelee = true;
            }

            return VisionLogic.MapWeaponClassToFov(
                weaponClass,
                isMelee,
                PluginConfig.FovPistol.Value,
                PluginConfig.FovSMG.Value,
                PluginConfig.FovAssaultRifle.Value,
                PluginConfig.FovShotgun.Value,
                PluginConfig.FovSniper.Value,
                PluginConfig.FovDefault.Value,
                PluginConfig.FovMelee.Value
            );
        }
    }
}
