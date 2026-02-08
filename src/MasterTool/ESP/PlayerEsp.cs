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

        /// <summary>
        /// Scans registered players, filters by distance and alive status, and populates <see cref="Targets"/>
        /// with screen-space position data. Throttled by <see cref="PluginConfig.EspUpdateInterval"/>.
        /// </summary>
        /// <param name="gameWorld">The current game world instance.</param>
        /// <param name="mainCamera">The active camera for world-to-screen projection.</param>
        /// <param name="localPlayer">The local player (excluded from targets).</param>
        public void Update(GameWorld gameWorld, Camera mainCamera, Player localPlayer)
        {
            if (Time.time < _nextUpdate) return;
            _nextUpdate = Time.time + PluginConfig.EspUpdateInterval.Value;

            Targets.Clear();

            if (gameWorld == null || mainCamera == null || !PluginConfig.EspEnabled.Value) return;

            var players = gameWorld.RegisteredPlayers;
            if (players == null) return;

            foreach (var player in players)
            {
                if (player is Player playerClass)
                {
                    if (playerClass.IsYourPlayer || !playerClass.HealthController.IsAlive) continue;

                    float dist = Vector3.Distance(mainCamera.transform.position, playerClass.Transform.position);
                    if (dist > PluginConfig.EspMaxDistance.Value) continue;

                    Color textColor = PlayerUtils.GetPlayerColor(playerClass);
                    textColor.a = PluginConfig.EspTextAlpha.Value;

                    Vector3 screenPos = mainCamera.WorldToScreenPoint(playerClass.Transform.position + Vector3.up * 1.8f);
                    if (screenPos.z > 0)
                    {
                        Targets.Add(new EspTarget
                        {
                            ScreenPosition = new Vector2(screenPos.x, Screen.height - screenPos.y),
                            Distance = dist,
                            Nickname = playerClass.Profile.Nickname,
                            Side = PlayerUtils.GetPlayerTag(playerClass),
                            Color = textColor
                        });
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
