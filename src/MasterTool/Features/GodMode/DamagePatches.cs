using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using MasterTool.Config;

namespace MasterTool.Features.GodMode
{
    /// <summary>
    /// Harmony patches that intercept damage methods on the local player,
    /// blocking all incoming damage when God Mode is enabled.
    /// </summary>
    public static class DamagePatches
    {
        /// <summary>
        /// Applies all damage-blocking Harmony prefix patches.
        /// </summary>
        /// <param name="harmony">The Harmony instance used to install patches.</param>
        public static void PatchAll(Harmony harmony)
        {
            TryPatchDamageMethod(harmony, typeof(Player), "ApplyDamageInfo", nameof(BlockDamagePrefix_Player));
            TryPatchDamageMethod(harmony, typeof(Player), "ApplyDamage", nameof(BlockDamagePrefix_Player));
            TryPatchDamageMethod(harmony, typeof(ActiveHealthController), "ApplyDamage", nameof(BlockDamagePrefix_ActiveHealthController));
        }

        private static void TryPatchDamageMethod(Harmony harmony, Type type, string methodName, string prefixMethodName)
        {
            try
            {
                var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                    return;
                var prefix = new HarmonyMethod(
                    typeof(DamagePatches).GetMethod(prefixMethodName, BindingFlags.Static | BindingFlags.NonPublic)
                );
                harmony.Patch(method, prefix: prefix);
            }
            catch { }
        }

        private static bool BlockDamagePrefix_Player(Player __instance)
        {
            return !PluginConfig.GodModeEnabled.Value || !__instance.IsYourPlayer;
        }

        private static bool BlockDamagePrefix_ActiveHealthController(ActiveHealthController __instance, ref float __result)
        {
            if (
                PluginConfig.GodModeEnabled.Value
                && PluginConfig.LocalActiveHealthController != null
                && ReferenceEquals(__instance, PluginConfig.LocalActiveHealthController)
            )
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }
}
