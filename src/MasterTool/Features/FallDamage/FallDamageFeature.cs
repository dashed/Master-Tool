using System;
using EFT;
using MasterTool.Core;
using MasterTool.Plugin;

namespace MasterTool.Features.FallDamage
{
    /// <summary>
    /// Eliminates fall damage by setting the safe fall height to an extreme value.
    /// Tracks state to restore the original value when disabled.
    /// </summary>
    public static class FallDamageFeature
    {
        private static bool _modForced;
        private static bool _errorLogged;

        /// <summary>
        /// When enabled, sets FallSafeHeight to an extreme value so falls never cause damage.
        /// When disabled after being forced, restores the default safe height.
        /// </summary>
        /// <param name="localPlayer">The local player whose fall damage is controlled.</param>
        /// <param name="enabled">Whether no-fall-damage should be active.</param>
        public static void Apply(Player localPlayer, bool enabled)
        {
            try
            {
                if (enabled)
                {
                    localPlayer.ActiveHealthController.FallSafeHeight = FallDamageDefaults.SafeHeight;
                    _modForced = true;
                }
                else if (_modForced)
                {
                    localPlayer.ActiveHealthController.FallSafeHeight = FallDamageDefaults.DefaultHeight;
                    _modForced = false;
                }
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[FallDamage] {ex.Message}");
                    _errorLogged = true;
                }
            }
        }
    }
}
