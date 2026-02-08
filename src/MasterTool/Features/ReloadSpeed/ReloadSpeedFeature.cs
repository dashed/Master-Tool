using System;
using Comfort.Common;
using MasterTool.Config;
using MasterTool.Core;
using MasterTool.Plugin;

namespace MasterTool.Features.ReloadSpeed
{
    public static class ReloadSpeedFeature
    {
        public const float DefaultLoadTime = ReloadDefaults.DefaultLoadTime;
        public const float DefaultUnloadTime = ReloadDefaults.DefaultUnloadTime;

        private static bool _modForced;
        private static bool _errorLogged;

        public static void Apply(bool enabled)
        {
            try
            {
                if (enabled)
                {
                    var config = Singleton<BackendConfigSettingsClass>.Instance;
                    if (config == null)
                        return;
                    config.BaseLoadTime = PluginConfig.ReloadLoadTime.Value;
                    config.BaseUnloadTime = PluginConfig.ReloadUnloadTime.Value;
                    _modForced = true;
                }
                else if (_modForced)
                {
                    var config = Singleton<BackendConfigSettingsClass>.Instance;
                    if (config != null)
                    {
                        config.BaseLoadTime = DefaultLoadTime;
                        config.BaseUnloadTime = DefaultUnloadTime;
                    }
                    _modForced = false;
                }
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[ReloadSpeed] {ex.Message}");
                    _errorLogged = true;
                }
            }
        }
    }
}
