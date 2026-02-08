using System;
using EFT;
using MasterTool.Plugin;

namespace MasterTool.Features.InfiniteStamina
{
    /// <summary>
    /// Keeps the local player's stamina, hand stamina, and oxygen at maximum capacity each frame.
    /// </summary>
    public static class StaminaFeature
    {
        private static bool _errorLogged;

        /// <summary>
        /// Sets stamina, hand stamina, and oxygen to their total capacity for the given player.
        /// </summary>
        /// <param name="localPlayer">The local player whose physical stats are restored.</param>
        public static void Apply(Player localPlayer)
        {
            try
            {
                var stamina = localPlayer.Physical.Stamina;
                var hands = localPlayer.Physical.HandsStamina;
                var oxygen = localPlayer.Physical.Oxygen;
                if (stamina != null)
                    stamina.Current = stamina.TotalCapacity;
                if (hands != null)
                    hands.Current = hands.TotalCapacity;
                if (oxygen != null)
                    oxygen.Current = oxygen.TotalCapacity;
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[Stamina] {ex.Message}");
                    _errorLogged = true;
                }
            }
        }
    }
}
