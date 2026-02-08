using EFT;
using MasterTool.Config;
using UnityEngine;

namespace MasterTool.Features.Speedhack
{
    /// <summary>
    /// Applies additional positional displacement each frame to increase the player's movement speed
    /// beyond the normal game limits.
    /// </summary>
    public static class SpeedhackFeature
    {
        /// <summary>
        /// Adds extra movement in the player's facing direction scaled by the configured speed multiplier.
        /// </summary>
        /// <param name="localPlayer">The local player to move. Ignored if null.</param>
        public static void Apply(Player localPlayer)
        {
            if (localPlayer == null)
                return;

            Vector3 moveDir =
                localPlayer.Transform.rotation
                * new Vector3(localPlayer.MovementContext.MovementDirection.x, 0, localPlayer.MovementContext.MovementDirection.y);

            localPlayer.Transform.position += moveDir * (PluginConfig.SpeedMultiplier.Value * Time.deltaTime * 5f);
        }
    }
}
