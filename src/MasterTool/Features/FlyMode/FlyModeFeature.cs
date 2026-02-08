using System;
using EFT;
using MasterTool.Config;
using MasterTool.Core;
using MasterTool.Plugin;
using MasterTool.Utils;
using UnityEngine;
using KeyCode = UnityEngine.KeyCode;

namespace MasterTool.Features.FlyMode
{
    public static class FlyModeFeature
    {
        private static bool _modForced;
        private static bool _errorLogged;
        private static CharacterController _cachedController;

        public static void Apply(Player localPlayer, Camera mainCamera)
        {
            try
            {
                if (!PluginConfig.FlyModeEnabled.Value)
                {
                    if (_modForced)
                        Restore(localPlayer);
                    return;
                }

                if (mainCamera == null)
                    return;

                // On first enable: disable CharacterController for noclip
                if (!_modForced)
                {
                    _cachedController = localPlayer.gameObject.GetComponent<CharacterController>();
                    if (_cachedController != null)
                        _cachedController.enabled = false;
                    _modForced = true;
                }

                // Read input
                float inputH = Input.GetAxis("Horizontal");
                float inputV = Input.GetAxis("Vertical");
                float inputUp = Input.GetKey(KeyCode.Space) ? 1f : 0f;
                float inputDown = Input.GetKey(KeyCode.LeftControl) ? 1f : 0f;

                var camTransform = mainCamera.transform;
                var coreMove = MovementLogic.CalculateFlyMovement(
                    camTransform.forward.ToVec3(),
                    camTransform.right.ToVec3(),
                    inputH,
                    inputV,
                    inputUp,
                    inputDown,
                    PluginConfig.FlySpeed.Value,
                    Time.deltaTime
                );
                localPlayer.Transform.position += coreMove.ToVector3();
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[FlyMode] {ex.Message}");
                    _errorLogged = true;
                }
            }
        }

        private static void Restore(Player localPlayer)
        {
            try
            {
                if (_cachedController != null)
                    _cachedController.enabled = true;
            }
            catch (Exception ex)
            {
                MasterToolPlugin.Log?.LogDebug($"[FlyMode] Restore error: {ex.Message}");
            }
            _cachedController = null;
            _modForced = false;
        }
    }
}
