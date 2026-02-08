using System;
using EFT;
using MasterTool.Plugin;

namespace MasterTool.Features.Sustenance
{
    /// <summary>
    /// Keeps the local player's energy at maximum capacity each frame.
    /// </summary>
    public static class EnergyFeature
    {
        private static bool _errorLogged;

        /// <summary>
        /// Sets energy to maximum for the given player when it drops below max.
        /// </summary>
        /// <param name="localPlayer">The local player whose energy is restored.</param>
        public static void Apply(Player localPlayer)
        {
            try
            {
                var energy = localPlayer.ActiveHealthController.Energy;
                if (energy.Current < energy.Maximum)
                    localPlayer.ActiveHealthController.ChangeEnergy(energy.Maximum);
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[Energy] {ex.Message}");
                    _errorLogged = true;
                }
            }
        }
    }
}
