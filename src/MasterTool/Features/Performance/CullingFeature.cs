using Comfort.Common;
using EFT;
using MasterTool.Config;
using UnityEngine;

namespace MasterTool.Features.Performance
{
    /// <summary>
    /// Deactivates bot GameObjects beyond a configurable distance to reduce rendering overhead.
    /// Re-activates them when they come back within range or when performance mode is disabled.
    /// </summary>
    public static class CullingFeature
    {
        /// <summary>
        /// Enables or disables bot GameObjects based on their distance to the local player.
        /// When performance mode is off, all bots are re-enabled.
        /// </summary>
        /// <param name="gameWorld">The current game world containing registered players.</param>
        /// <param name="localPlayer">The local player used as the distance reference point.</param>
        public static void Apply(GameWorld gameWorld, Player localPlayer)
        {
            if (gameWorld == null || localPlayer == null) return;
            try
            {
                var players = gameWorld.RegisteredPlayers;
                foreach (var p in players)
                {
                    if (p == null || p.IsYourPlayer) continue;

                    GameObject botObj = (p as Component)?.gameObject;
                    if (botObj == null) continue;

                    bool shouldRender = true;
                    if (PluginConfig.PerformanceMode.Value)
                    {
                        float dist = Vector3.Distance(localPlayer.Transform.position, p.Transform.position);
                        shouldRender = dist <= PluginConfig.BotRenderDistance.Value;
                    }

                    if (botObj.activeSelf != shouldRender)
                        botObj.SetActive(shouldRender);
                }
            }
            catch { }
        }
    }
}
