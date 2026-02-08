using System.Collections.Generic;
using EFT;
using MasterTool.Config;
using MasterTool.Models;
using MasterTool.Utils;
using UnityEngine;

namespace MasterTool.ESP
{
    /// <summary>
    /// Tracks all non-local alive players within range and projects their positions to screen space.
    /// Call <see cref="Update"/> each frame to refresh target data, then <see cref="Render"/> during OnGUI.
    /// </summary>
    public class PlayerEsp
    {
        public List<EspTarget> Targets { get; } = new List<EspTarget>();
        private float _nextUpdate;
        private static int _losLayerMask = -1;

        private static void InitLayerMask()
        {
            if (_losLayerMask != -1)
                return;

            int mask = 0;
            int hp = LayerMask.NameToLayer("HighPolyCollider");
            int lp = LayerMask.NameToLayer("LowPolyCollider");
            int terrain = LayerMask.NameToLayer("Terrain");

            if (hp >= 0)
                mask |= 1 << hp;
            if (lp >= 0)
                mask |= 1 << lp;
            if (terrain >= 0)
                mask |= 1 << terrain;

            // Fallback: known EFT collision layer bitmask
            _losLayerMask = mask != 0 ? mask : 0x02251800;
        }

        /// <summary>
        /// Checks whether the camera has an unobstructed line of sight to the target player's head.
        /// Uses Physics.Linecast against static geometry layers (terrain, walls, buildings).
        /// </summary>
        internal static bool HasLineOfSight(Camera camera, Player target)
        {
            Vector3 origin = camera.transform.position;

            var headBone = target.PlayerBones?.Head?.Original;
            Vector3 destination = headBone != null ? headBone.position : target.Transform.position + Vector3.up * 1.6f;

            // Offset origin slightly toward target to avoid local player collider self-hit
            origin += (destination - origin).normalized * 0.15f;

            InitLayerMask();

            if (!Physics.Linecast(origin, destination, out RaycastHit hit, _losLayerMask, QueryTriggerInteraction.Ignore))
            {
                return true; // Nothing blocked the path
            }

            // Something was hit — visible only if we hit the target itself
            return hit.transform.root == target.Transform.Original;
        }

        /// <summary>
        /// Computes the world position for ESP text, preferring head bone when available.
        /// </summary>
        internal static Vector3 GetEspWorldPosition(Vector3? headBonePos, Vector3 transformPos)
        {
            if (headBonePos.HasValue)
                return headBonePos.Value + Vector3.up * 0.2f;
            return transformPos + Vector3.up * 1.8f;
        }

        /// <summary>
        /// Scans registered players, filters by distance and alive status, and populates <see cref="Targets"/>
        /// with screen-space position data. Throttled by <see cref="PluginConfig.EspUpdateInterval"/>.
        /// </summary>
        /// <param name="gameWorld">The current game world instance.</param>
        /// <param name="mainCamera">The active camera for world-to-screen projection.</param>
        /// <param name="localPlayer">The local player (excluded from targets).</param>
        public void Update(GameWorld gameWorld, Camera mainCamera, Player localPlayer)
        {
            if (Time.time < _nextUpdate)
                return;
            _nextUpdate = Time.time + PluginConfig.EspUpdateInterval.Value;

            Targets.Clear();

            if (gameWorld == null || mainCamera == null || !PluginConfig.EspEnabled.Value)
                return;

            var players = gameWorld.RegisteredPlayers;
            if (players == null)
                return;

            foreach (var player in players)
            {
                if (player is Player playerClass)
                {
                    if (playerClass.IsYourPlayer || !playerClass.HealthController.IsAlive)
                        continue;

                    float dist = Vector3.Distance(mainCamera.transform.position, playerClass.Transform.position);
                    if (dist > PluginConfig.EspMaxDistance.Value)
                        continue;

                    if (PluginConfig.EspLineOfSightOnly.Value && !HasLineOfSight(mainCamera, playerClass))
                        continue;

                    Color textColor = PlayerUtils.GetPlayerColor(playerClass);
                    textColor.a = PluginConfig.EspTextAlpha.Value;

                    var headBone = playerClass.PlayerBones?.Head?.Original;
                    Vector3? headPos = headBone != null ? (Vector3?)headBone.position : null;
                    Vector3 worldPos = GetEspWorldPosition(headPos, playerClass.Transform.position);
                    Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
                    if (screenPos.z > 0)
                    {
                        Targets.Add(
                            new EspTarget
                            {
                                ScreenPosition = new Vector2(screenPos.x, Screen.height - screenPos.y),
                                Distance = dist,
                                Nickname = playerClass.Profile.Nickname,
                                Side = PlayerUtils.GetPlayerTag(playerClass),
                                Color = textColor,
                            }
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Draws nickname, faction tag, and distance labels for each tracked player target.
        /// Must be called from OnGUI.
        /// </summary>
        /// <param name="style">The <see cref="GUIStyle"/> used for ESP text rendering.</param>
        public void Render(GUIStyle style)
        {
            foreach (var target in Targets)
            {
                string text = $"{target.Nickname}\n[{target.Side}]\n{target.Distance:F1}m";
                EspRenderer.DrawTextWithShadow(target.ScreenPosition, text, target.Color, style);
            }
        }
    }
}
