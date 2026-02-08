using System;
using EFT;
using MasterTool.Plugin;
using UnityEngine;

namespace MasterTool.Features.Teleport
{
    public static class PlayerTeleportFeature
    {
        private static Vector3? _savedPosition;
        private static Quaternion? _savedRotation;

        public static bool HasSavedPosition => _savedPosition.HasValue;

        public static void SavePosition(Player localPlayer)
        {
            if (localPlayer == null)
                return;
            _savedPosition = localPlayer.Transform.position;
            _savedRotation = localPlayer.Transform.rotation;
        }

        public static void LoadPosition(Player localPlayer)
        {
            if (localPlayer == null || !_savedPosition.HasValue)
                return;
            try
            {
                localPlayer.Transform.position = _savedPosition.Value;
                if (_savedRotation.HasValue)
                    localPlayer.Transform.rotation = _savedRotation.Value;
            }
            catch (Exception ex)
            {
                MasterToolPlugin.Log?.LogWarning($"[Teleport] Failed to load position: {ex.Message}");
            }
        }

        public static void TeleportToSurface(Player localPlayer)
        {
            if (localPlayer == null)
                return;
            try
            {
                Vector3 origin = CalculateRayOrigin(localPlayer.Transform.position);
                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1000f))
                {
                    localPlayer.Transform.position = hit.point + Vector3.up * 0.5f;
                }
            }
            catch (Exception ex)
            {
                MasterToolPlugin.Log?.LogWarning($"[Teleport] Failed to teleport to surface: {ex.Message}");
            }
        }

        internal static Vector3 CalculateRayOrigin(Vector3 playerPosition)
        {
            return new Vector3(playerPosition.x, playerPosition.y + 500f, playerPosition.z);
        }

        internal static void ClearSavedPosition()
        {
            _savedPosition = null;
            _savedRotation = null;
        }
    }
}
