using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using MasterTool.Config;
using MasterTool.Plugin;
using UnityEngine;

namespace MasterTool.Features.Performance
{
    /// <summary>
    /// Deactivates bot GameObjects beyond a configurable distance to reduce rendering overhead.
    /// Tracks which bots the mod deactivated so that only those are re-enabled when the feature
    /// is toggled off — avoiding interference with game-side deactivations.
    /// </summary>
    public static class CullingFeature
    {
        private static readonly HashSet<int> _modDeactivatedBots = new HashSet<int>();
        private static bool _errorLogged;

        /// <summary>
        /// Enables or disables bot GameObjects based on their distance to the local player.
        /// When performance mode is turned off, only bots that were deactivated by this feature
        /// are re-enabled — game-side deactivations are left untouched.
        /// </summary>
        /// <param name="gameWorld">The current game world containing registered players.</param>
        /// <param name="localPlayer">The local player used as the distance reference point.</param>
        public static void Apply(GameWorld gameWorld, Player localPlayer)
        {
            if (gameWorld == null || localPlayer == null)
                return;
            try
            {
                var players = gameWorld.RegisteredPlayers;
                foreach (var p in players)
                {
                    if (p == null || p.IsYourPlayer)
                        continue;

                    GameObject botObj = (p as Component)?.gameObject;
                    if (botObj == null)
                        continue;

                    int id = p.GetHashCode();

                    if (PluginConfig.PerformanceMode.Value)
                    {
                        float dist = Vector3.Distance(localPlayer.Transform.position, p.Transform.position);
                        bool shouldRender = dist <= PluginConfig.BotRenderDistance.Value;

                        if (!shouldRender && botObj.activeSelf)
                        {
                            botObj.SetActive(false);
                            _modDeactivatedBots.Add(id);
                        }
                        else if (shouldRender && !botObj.activeSelf && _modDeactivatedBots.Contains(id))
                        {
                            botObj.SetActive(true);
                            _modDeactivatedBots.Remove(id);
                        }
                    }
                    else if (_modDeactivatedBots.Contains(id))
                    {
                        botObj.SetActive(true);
                        _modDeactivatedBots.Remove(id);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[Culling] {ex.Message}");
                    _errorLogged = true;
                }
            }
        }
    }
}
