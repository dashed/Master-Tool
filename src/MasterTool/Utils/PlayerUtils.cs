using EFT;
using MasterTool.Config;
using UnityEngine;

namespace MasterTool.Utils
{
    /// <summary>
    /// Provides helper methods for classifying players by faction and role.
    /// </summary>
    public static class PlayerUtils
    {
        /// <summary>
        /// Returns the configured ESP color for a player based on their faction side and bot role.
        /// Boss-type scavs receive a distinct color from regular scavs.
        /// </summary>
        /// <param name="player">The player to classify.</param>
        /// <returns>The faction/role color from <see cref="PluginConfig"/>.</returns>
        public static Color GetPlayerColor(Player player)
        {
            if (player.Side == EPlayerSide.Savage)
            {
                if (
                    player.Profile.Info.Settings.Role != WildSpawnType.assault
                    && player.Profile.Info.Settings.Role != WildSpawnType.marksman
                )
                {
                    return PluginConfig.ColorBoss.Value;
                }
                return PluginConfig.ColorSavage.Value;
            }

            if (player.Side == EPlayerSide.Bear)
            {
                return PluginConfig.ColorBear.Value;
            }

            if (player.Side == EPlayerSide.Usec)
            {
                return PluginConfig.ColorUsec.Value;
            }

            return Color.white;
        }

        /// <summary>
        /// Returns a short uppercase faction/role tag string for display (e.g. "BEAR", "USEC", "SCAV", "BOSS").
        /// </summary>
        /// <param name="player">The player to classify.</param>
        /// <returns>A short tag string representing the player's faction or role.</returns>
        public static string GetPlayerTag(Player player)
        {
            if (player.Side == EPlayerSide.Savage)
            {
                var role = player.Profile.Info.Settings.Role;
                if (role != WildSpawnType.assault && role != WildSpawnType.marksman)
                {
                    return "BOSS";
                }
                return "SCAV";
            }
            return player.Side.ToString().ToUpper();
        }
    }
}
