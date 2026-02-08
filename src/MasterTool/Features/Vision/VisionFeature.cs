using BSG.CameraEffects;
using EFT;
using EFT.InventoryLogic;
using MasterTool.Config;
using UnityEngine;

namespace MasterTool.Features.Vision
{
    /// <summary>
    /// Controls thermal vision, night vision, and per-weapon-category FOV adjustments
    /// by toggling camera effect components and smoothly lerping the camera field of view.
    /// </summary>
    public class VisionFeature
    {
        private float _originalFov = 75f;
        private bool _fovInitialized;

        /// <summary>
        /// Enables or disables the thermal vision camera effect based on the config toggle.
        /// </summary>
        /// <param name="mainCamera">The active game camera. Ignored if null.</param>
        public void UpdateThermalVision(Camera mainCamera)
        {
            if (mainCamera == null) return;
            var thermal = mainCamera.GetComponent<ThermalVision>();
            if (thermal != null && thermal.On != PluginConfig.ThermalVisionEnabled.Value)
            {
                thermal.On = PluginConfig.ThermalVisionEnabled.Value;
            }
        }

        /// <summary>
        /// Enables or disables the night vision camera effect based on the config toggle.
        /// </summary>
        /// <param name="mainCamera">The active game camera. Ignored if null.</param>
        public void UpdateNightVision(Camera mainCamera)
        {
            if (mainCamera == null) return;
            var nv = mainCamera.GetComponent<NightVision>();
            if (nv != null && nv.On != PluginConfig.NightVisionEnabled.Value)
            {
                nv.On = PluginConfig.NightVisionEnabled.Value;
            }
        }

        /// <summary>
        /// Smoothly adjusts the camera FOV based on the currently equipped weapon category.
        /// Restores the original FOV when the feature is disabled.
        /// </summary>
        /// <param name="mainCamera">The active game camera.</param>
        /// <param name="localPlayer">The local player whose equipped weapon determines the target FOV.</param>
        public void UpdateWeaponFov(Camera mainCamera, Player localPlayer)
        {
            if (mainCamera == null || localPlayer == null) return;

            if (PluginConfig.WeaponFovEnabled.Value)
            {
                if (!_fovInitialized)
                {
                    _originalFov = mainCamera.fieldOfView;
                    _fovInitialized = true;
                }

                float targetFov = GetFovForCurrentWeapon(localPlayer);
                if (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.1f)
                {
                    mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * 10f);
                }
            }
            else if (_fovInitialized)
            {
                mainCamera.fieldOfView = _originalFov;
                _fovInitialized = false;
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

            if (item is Weapon weapon)
            {
                var weaponTemplate = weapon.Template;
                if (weaponTemplate == null)
                    return PluginConfig.FovDefault.Value;

                var weaponType = weaponTemplate.weapClass;

                switch (weaponType?.ToLower())
                {
                    case "pistol": return PluginConfig.FovPistol.Value;
                    case "smg": return PluginConfig.FovSMG.Value;
                    case "assaultrifle":
                    case "assaultcarbine": return PluginConfig.FovAssaultRifle.Value;
                    case "shotgun": return PluginConfig.FovShotgun.Value;
                    case "marksmanrifle":
                    case "sniperrifle": return PluginConfig.FovSniper.Value;
                    case "machinegun": return PluginConfig.FovAssaultRifle.Value;
                    default: return PluginConfig.FovDefault.Value;
                }
            }

            return PluginConfig.FovDefault.Value;
        }
    }
}
