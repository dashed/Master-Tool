using System;
using EFT;
using MasterTool.Core;
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
                float newValue = SustenanceLogic.ComputeNewValue(energy.Current, energy.Maximum, true);
                if (newValue != energy.Current)
                    localPlayer.ActiveHealthController.ChangeEnergy(newValue);
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
