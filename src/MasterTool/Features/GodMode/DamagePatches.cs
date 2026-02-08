using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using MasterTool.Config;
using MasterTool.Features.CodMode;
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
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var prefix = new HarmonyMethod(
                    typeof(DamagePatches).GetMethod(prefixMethodName, BindingFlags.Static | BindingFlags.NonPublic)
                );
                foreach (var method in methods)
                {
                    if (method.Name == methodName)
                    {
                        try
                        {
                            harmony.Patch(method, prefix: prefix);
                        }
                        catch (Exception ex)
                        {
                            MasterToolPlugin.Log?.LogWarning($"[GodMode] Failed to patch {type.Name}.{methodName} overload: {ex.Message}");
                        }
                    }
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
            return !PluginConfig.GodModeEnabled.Value || !__instance.IsYourPlayer;
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
                // 1. GodMode: zero all damage
                if (PluginConfig.GodModeEnabled.Value)
                {
                    damage = 0f;
                    return true;
                }

                // 2. Headshot ignore: zero head damage entirely
                if (bodyPart == EBodyPart.Head && PluginConfig.IgnoreHeadshots.Value)
                {
                    damage = 0f;
                    return true;
                }

                // 3. Head-specific damage percentage
                if (bodyPart == EBodyPart.Head && PluginConfig.HeadDamagePercent.Value < 100)
                {
                    damage *= PluginConfig.HeadDamagePercent.Value / 100f;
                }

                // 4. Global damage reduction percentage
                if (PluginConfig.DamageReductionPercent.Value < 100)
                {
                    damage *= PluginConfig.DamageReductionPercent.Value / 100f;
                }

                // 5. Keep 1 Health: prevent lethal damage
                if (PluginConfig.Keep1HealthEnabled.Value)
                {
                    var currentHealth = __instance.GetBodyPartHealth(bodyPart, false);
                    bool shouldProtect =
                        PluginConfig.Keep1HealthSelection.Value == "All"
                        || (
                            PluginConfig.Keep1HealthSelection.Value == "Head And Thorax"
                            && (bodyPart == EBodyPart.Head || bodyPart == EBodyPart.Chest)
                        );

                    if (shouldProtect && (currentHealth.Current - damage) < 3f)
                    {
                        damage = Math.Max(0f, currentHealth.Current - 3f);
                    }
                }

                CodModeFeature.NotifyDamage();

                return true;
            }

            // --- ENEMY PLAYER ---
            if (___Player != null && !___Player.IsYourPlayer)
            {
                if (PluginConfig.EnemyDamageMultiplier.Value > 1f)
                {
                    damage *= PluginConfig.EnemyDamageMultiplier.Value;
                }
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
