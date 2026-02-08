using EFT;
using MasterTool.Config;
using UnityEngine;

namespace MasterTool.Features.BigHeadMode
{
    /// <summary>
    /// Scales the head bone of all non-local alive players to a configurable multiplier,
    /// making enemy heads larger and easier to target. Resets to normal size when disabled.
    /// </summary>
    public static class BigHeadFeature
    {
        /// <summary>
        /// Applies or resets head bone scaling on all registered non-local players.
        /// </summary>
        /// <param name="gameWorld">The current game world containing registered players.</param>
        public static void Apply(GameWorld gameWorld)
        {
            if (gameWorld == null) return;

            foreach (var player in gameWorld.RegisteredPlayers)
            {
                if (player == null || player.IsYourPlayer) continue;
                var head = player.PlayerBones.Head.Original;
                if (head == null) continue;

                if (PluginConfig.BigHeadModeEnabled.Value && player.HealthController.IsAlive)
                {
                    float size = PluginConfig.HeadSizeMultiplier.Value;
                    head.localScale = new Vector3(size, size, size);
                }
                else
                {
                    head.localScale = Vector3.one;
                }
            }
        }
    }
}
