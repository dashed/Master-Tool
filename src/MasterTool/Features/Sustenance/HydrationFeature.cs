using System;
using EFT;
using MasterTool.Plugin;

namespace MasterTool.Features.Sustenance
{
    /// <summary>
    /// Keeps the local player's hydration at maximum capacity each frame.
    /// </summary>
    public static class HydrationFeature
    {
        private static bool _errorLogged;

        /// <summary>
        /// Sets hydration to maximum for the given player when it drops below max.
        /// </summary>
        /// <param name="localPlayer">The local player whose hydration is restored.</param>
        public static void Apply(Player localPlayer)
        {
            try
            {
                var hydration = localPlayer.ActiveHealthController.Hydration;
                if (hydration.Current < hydration.Maximum)
                    localPlayer.ActiveHealthController.ChangeHydration(hydration.Maximum);
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[Hydration] {ex.Message}");
                    _errorLogged = true;
                }
            }
        }
    }
}
