using System.Collections.Generic;
using EFT;
using MasterTool.Config;
using UnityEngine;

namespace MasterTool.Features.BigHeadMode
{
    /// <summary>
    /// Scales the head bone of all non-local alive players to a configurable multiplier,
    /// making enemy heads larger and easier to target. Tracks which players the mod scaled
    /// so that only those are reset when disabled — avoiding interference with game-side
    /// or other-mod bone scaling.
    /// </summary>
    public static class BigHeadFeature
    {
        private static readonly HashSet<int> _modScaledPlayers = new HashSet<int>();

        /// <summary>
        /// Applies or resets head bone scaling on all registered non-local players.
        /// When disabled, only resets players that were previously scaled by this feature.
        /// </summary>
        /// <param name="gameWorld">The current game world containing registered players.</param>
        public static void Apply(GameWorld gameWorld)
        {
            if (gameWorld == null)
                return;

            foreach (var player in gameWorld.RegisteredPlayers)
            {
                if (player == null || player.IsYourPlayer)
                    continue;
                var head = player.PlayerBones.Head.Original;
                if (head == null)
                    continue;

                int id = player.GetHashCode();

                if (PluginConfig.BigHeadModeEnabled.Value && player.HealthController.IsAlive)
                {
                    float size = PluginConfig.HeadSizeMultiplier.Value;
                    head.localScale = new Vector3(size, size, size);
                    _modScaledPlayers.Add(id);
                }
                else if (_modScaledPlayers.Contains(id))
                {
                    head.localScale = Vector3.one;
                    _modScaledPlayers.Remove(id);
                }
            }
        }
    }
}
