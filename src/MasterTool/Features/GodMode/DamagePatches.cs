using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using MasterTool.Config;
using MasterTool.Core;
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

            // ActiveHealthController patches — use TryPatchAllOverloads to handle
            // methods with multiple overloads (e.g., DoBleed in SPT 4.0)
            TryPatchAllOverloads(
                harmony,
                typeof(ActiveHealthController),
                nameof(ActiveHealthController.ApplyDamage),
                nameof(BlockDamagePrefix_ActiveHealthController)
            );
            TryPatchAllOverloads(harmony, typeof(ActiveHealthController), nameof(ActiveHealthController.Kill), nameof(BlockKillPrefix));
            TryPatchAllOverloads(
                harmony,
                typeof(ActiveHealthController),
                nameof(ActiveHealthController.DestroyBodyPart),
                nameof(BlockDestroyBodyPartPrefix)
            );
            TryPatchAllOverloads(
                harmony,
                typeof(ActiveHealthController),
                nameof(ActiveHealthController.DoFracture),
                nameof(BlockDoFracturePrefix)
            );
            TryPatchAllOverloads(harmony, typeof(ActiveHealthController), "DoBleed", nameof(BlockDoBleedPrefix));
        }

        /// <summary>
        /// Patches all overloads of a method by name. Handles AmbiguousMatchException
        /// that occurs when a method has multiple overloads (e.g., DoBleed in SPT 4.0).
        /// </summary>
        private static void TryPatchAllOverloads(Harmony harmony, Type type, string methodName, string prefixMethodName)
        {
            try
            {
                var methods = type.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
                );
                var prefix = new HarmonyMethod(
                    typeof(DamagePatches).GetMethod(prefixMethodName, BindingFlags.Static | BindingFlags.NonPublic)
                );
                int patched = 0;
                int failed = 0;
                foreach (var method in methods)
                {
                    if (method.Name == methodName)
                    {
                        try
                        {
                            harmony.Patch(method, prefix: prefix);
                            patched++;
                        }
                        catch (Exception)
                        {
                            failed++;
                        }
                    }
                }

                if (patched == 0 && failed > 0)
                {
                    MasterToolPlugin.Log?.LogWarning($"[GodMode] Failed to patch any {type.Name}.{methodName} overload ({failed} failed)");
                }
            }
            catch (Exception ex)
            {
                MasterToolPlugin.Log?.LogWarning($"[GodMode] Failed to find {type.Name}.{methodName}: {ex.Message}");
            }
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
            return !DamageLogic.ShouldBlockForPlayer(PluginConfig.GodModeEnabled.Value, __instance?.IsYourPlayer);
        }

        private static bool BlockDamagePrefix_ActiveHealthController(
            ActiveHealthController __instance,
            Player ___Player,
            ref float damage,
            EBodyPart bodyPart
        )
        {
            // --- LOCAL PLAYER ---
            if (___Player != null && ___Player.IsYourPlayer)
            {
                var currentHealth = __instance.GetBodyPartHealth(bodyPart, false);
                damage = DamageLogic.ComputeLocalPlayerDamage(
                    damage,
                    PluginConfig.GodModeEnabled.Value,
                    bodyPart == EBodyPart.Head,
                    PluginConfig.IgnoreHeadshots.Value,
                    PluginConfig.HeadDamagePercent.Value,
                    PluginConfig.DamageReductionPercent.Value,
                    PluginConfig.Keep1HealthEnabled.Value,
                    PluginConfig.Keep1HealthSelection.Value,
                    currentHealth.Current,
                    bodyPart == EBodyPart.Chest
                );
                return true;
            }

            // --- ENEMY PLAYER ---
            if (___Player != null && !___Player.IsYourPlayer)
            {
                damage = DamageLogic.ComputeEnemyDamage(damage, PluginConfig.EnemyDamageMultiplier.Value);
            }

            return true;
        }

        private static bool BlockKillPrefix(Player ___Player)
        {
            return !DamageLogic.ShouldBlockForPlayer(PluginConfig.GodModeEnabled.Value, ___Player?.IsYourPlayer);
        }

        private static bool BlockDestroyBodyPartPrefix(Player ___Player)
        {
            return !DamageLogic.ShouldBlockForPlayer(PluginConfig.GodModeEnabled.Value, ___Player?.IsYourPlayer);
        }

        private static bool BlockDoFracturePrefix(Player ___Player)
        {
            return !DamageLogic.ShouldBlockForPlayer(PluginConfig.GodModeEnabled.Value, ___Player?.IsYourPlayer);
        }

        private static bool BlockDoBleedPrefix(Player ___Player)
        {
            return !DamageLogic.ShouldBlockForPlayer(PluginConfig.GodModeEnabled.Value, ___Player?.IsYourPlayer);
        }
    }
}
