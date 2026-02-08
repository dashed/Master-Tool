using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using MasterTool.Config;
using MasterTool.Plugin;

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
            // Player-level belt-and-suspenders patches
            TryPatchDamageMethod(harmony, typeof(Player), "ApplyDamageInfo", nameof(BlockDamagePrefix_Player));
            TryPatchDamageMethod(harmony, typeof(Player), "ApplyDamage", nameof(BlockDamagePrefix_Player));

            // ActiveHealthController patches
            TryPatchDamageMethod(
                harmony,
                typeof(ActiveHealthController),
                nameof(ActiveHealthController.ApplyDamage),
                nameof(BlockDamagePrefix_ActiveHealthController)
            );
            TryPatchDamageMethod(harmony, typeof(ActiveHealthController), nameof(ActiveHealthController.Kill), nameof(BlockKillPrefix));
            TryPatchDamageMethod(
                harmony,
                typeof(ActiveHealthController),
                nameof(ActiveHealthController.DestroyBodyPart),
                nameof(BlockDestroyBodyPartPrefix)
            );
            TryPatchDamageMethod(
                harmony,
                typeof(ActiveHealthController),
                nameof(ActiveHealthController.DoFracture),
                nameof(BlockDoFracturePrefix)
            );

            // DoBleed is private, so we need AccessTools
            TryPatchDamageMethod(harmony, AccessTools.Method(typeof(ActiveHealthController), "DoBleed"), nameof(BlockDoBleedPrefix));
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
            catch (Exception ex)
            {
                MasterToolPlugin.Log?.LogWarning($"[GodMode] Failed to patch {type.Name}.{methodName}: {ex.Message}");
            }
        }

        private static void TryPatchDamageMethod(Harmony harmony, MethodBase method, string prefixMethodName)
        {
            try
            {
                if (method == null)
                    return;
                var prefix = new HarmonyMethod(
                    typeof(DamagePatches).GetMethod(prefixMethodName, BindingFlags.Static | BindingFlags.NonPublic)
                );
                harmony.Patch(method, prefix: prefix);
            }
            catch (Exception ex)
            {
                MasterToolPlugin.Log?.LogWarning($"[GodMode] Failed to patch {method?.DeclaringType?.Name}.{method?.Name}: {ex.Message}");
            }
        }

        private static bool BlockDamagePrefix_Player(Player __instance)
        {
            return !PluginConfig.GodModeEnabled.Value || !__instance.IsYourPlayer;
        }

        private static bool BlockDamagePrefix_ActiveHealthController(Player ___Player, ref float damage)
        {
            if (___Player == null || !___Player.IsYourPlayer)
            {
                return true;
            }

            if (PluginConfig.GodModeEnabled.Value)
            {
                damage = 0f;
            }

            return true;
        }

        private static bool BlockKillPrefix(Player ___Player)
        {
            if (___Player == null || !___Player.IsYourPlayer)
            {
                return true;
            }

            return !PluginConfig.GodModeEnabled.Value;
        }

        private static bool BlockDestroyBodyPartPrefix(Player ___Player)
        {
            if (___Player == null || !___Player.IsYourPlayer)
            {
                return true;
            }

            return !PluginConfig.GodModeEnabled.Value;
        }

        private static bool BlockDoFracturePrefix(Player ___Player)
        {
            if (___Player == null || !___Player.IsYourPlayer)
            {
                return true;
            }

            return !PluginConfig.GodModeEnabled.Value;
        }

        private static bool BlockDoBleedPrefix(Player ___Player)
        {
            if (___Player == null || !___Player.IsYourPlayer)
            {
                return true;
            }

            return !PluginConfig.GodModeEnabled.Value;
        }
    }
}
