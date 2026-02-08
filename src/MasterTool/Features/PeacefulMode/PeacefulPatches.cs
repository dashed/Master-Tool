using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using MasterTool.Config;
using MasterTool.Core;
using MasterTool.Plugin;

namespace MasterTool.Features.PeacefulMode
{
    /// <summary>
    /// Harmony patches that prevent bots from recognizing the local player as an enemy.
    /// Uses four complementary patches for complete coverage:
    /// <list type="bullet">
    ///   <item>Prefix on <c>BotsGroup.AddEnemy</c> — blocks group-level enemy registration</item>
    ///   <item>Prefix on <c>BotMemoryClass.AddEnemy</c> — blocks individual bot memory</item>
    ///   <item>Postfix on <c>EnemyInfo.ShallKnowEnemy</c> — overrides knowledge checks</item>
    ///   <item>Postfix on <c>EnemyInfo.ShallKnowEnemyLate</c> — overrides late knowledge checks</item>
    /// </list>
    /// Approach inspired by SAIN mod's proven bot AI patching patterns.
    /// </summary>
    public static class PeacefulPatches
    {
        /// <summary>
        /// Registers all peaceful mode Harmony patches. Safe to call even if target
        /// methods don't exist — each patch is wrapped in try/catch.
        /// </summary>
        public static void PatchAll(Harmony harmony)
        {
            TryPatch(harmony, typeof(BotsGroup), "AddEnemy", nameof(BlockAddEnemyGroup), isPrefix: true);
            TryPatch(harmony, typeof(BotMemoryClass), "AddEnemy", nameof(BlockAddEnemyMemory), isPrefix: true);
            TryPatch(harmony, typeof(EnemyInfo), "ShallKnowEnemy", nameof(BlockShallKnowEnemy), isPrefix: false);
            TryPatch(harmony, typeof(EnemyInfo), "ShallKnowEnemyLate", nameof(BlockShallKnowEnemyLate), isPrefix: false);
        }

        private static void TryPatch(Harmony harmony, Type type, string methodName, string patchMethodName, bool isPrefix)
        {
            try
            {
                var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                    return;
                var patchMethod = new HarmonyMethod(
                    typeof(PeacefulPatches).GetMethod(patchMethodName, BindingFlags.Static | BindingFlags.NonPublic)
                );
                if (isPrefix)
                    harmony.Patch(method, prefix: patchMethod);
                else
                    harmony.Patch(method, postfix: patchMethod);
            }
            catch (Exception ex)
            {
                MasterToolPlugin.Log?.LogWarning($"[PeacefulMode] Failed to patch {type.Name}.{methodName}: {ex.Message}");
            }
        }

        private static PropertyInfo _aiDataProp;
        private static PropertyInfo _botOwnerProp;
        private static PropertyInfo _memoryProp;
        private static PropertyInfo _enemiesControllerProp;
        private static MethodInfo _deleteInfoMethod;
        private static MethodInfo _removeMethod;
        private static bool _reflectionResolved;

        /// <summary>
        /// Lazily resolves reflection handles for Player.AIData.BotOwner.Memory/EnemiesController.
        /// Cached after first call to avoid repeated reflection lookups.
        /// </summary>
        private static void ResolveReflection()
        {
            if (_reflectionResolved)
                return;
            _reflectionResolved = true;

            try
            {
                _aiDataProp = typeof(Player).GetProperty("AIData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (_aiDataProp != null)
                {
                    _botOwnerProp = _aiDataProp.PropertyType.GetProperty(
                        "BotOwner",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }
                if (_botOwnerProp != null)
                {
                    var botOwnerType = _botOwnerProp.PropertyType;
                    _memoryProp = botOwnerType.GetProperty("Memory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    _enemiesControllerProp = botOwnerType.GetProperty(
                        "EnemiesController",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }
                if (_memoryProp != null)
                {
                    _deleteInfoMethod = _memoryProp.PropertyType.GetMethod(
                        "DeleteInfoAboutEnemy",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }
                if (_enemiesControllerProp != null)
                {
                    _removeMethod = _enemiesControllerProp.PropertyType.GetMethod(
                        "Remove",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                }
            }
            catch (Exception ex)
            {
                MasterToolPlugin.Log?.LogWarning($"[PeacefulMode] Reflection setup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes the local player from all active bots' enemy lists.
        /// Called when peaceful mode is toggled on mid-raid to clear existing aggro.
        /// Uses reflection to access BotOwner.Memory.DeleteInfoAboutEnemy and
        /// BotOwner.EnemiesController.Remove without compile-time dependencies.
        /// </summary>
        public static void ClearPlayerFromAllBots(GameWorld gameWorld, Player localPlayer)
        {
            if (gameWorld == null || localPlayer == null)
                return;

            ResolveReflection();

            if (_aiDataProp == null || _botOwnerProp == null)
                return;

            var allPlayers = gameWorld.AllAlivePlayersList;
            if (allPlayers == null)
                return;

            for (int i = 0; i < allPlayers.Count; i++)
            {
                var bot = allPlayers[i];
                if (bot == null || bot.IsYourPlayer || !bot.IsAI)
                    continue;

                try
                {
                    var aiData = _aiDataProp.GetValue(bot);
                    if (aiData == null)
                        continue;
                    var botOwner = _botOwnerProp.GetValue(aiData);
                    if (botOwner == null)
                        continue;

                    if (_deleteInfoMethod != null)
                    {
                        var memory = _memoryProp?.GetValue(botOwner);
                        if (memory != null)
                            _deleteInfoMethod.Invoke(memory, new object[] { localPlayer });
                    }
                    if (_removeMethod != null)
                    {
                        var controller = _enemiesControllerProp?.GetValue(botOwner);
                        if (controller != null)
                            _removeMethod.Invoke(controller, new object[] { localPlayer });
                    }
                }
                catch (Exception ex)
                {
                    MasterToolPlugin.Log?.LogDebug($"[PeacefulMode] Error clearing bot {bot.name}: {ex.Message}");
                }
            }
        }

        /// <summary>Prefix: prevents BotsGroup from adding the local player as a group enemy.</summary>
        private static bool BlockAddEnemyGroup(IPlayer person)
        {
            if (PeacefulLogic.ShouldBlockEnemy(PluginConfig.PeacefulModeEnabled.Value, person.IsYourPlayer))
                return false;
            return true;
        }

        /// <summary>Prefix: prevents individual bot memory from registering the local player.</summary>
        private static bool BlockAddEnemyMemory(IPlayer enemy)
        {
            if (PeacefulLogic.ShouldBlockEnemy(PluginConfig.PeacefulModeEnabled.Value, enemy.IsYourPlayer))
                return false;
            return true;
        }

        /// <summary>Postfix: forces ShallKnowEnemy to return false for the local player.</summary>
        private static void BlockShallKnowEnemy(EnemyInfo __instance, ref bool __result)
        {
            if (PeacefulLogic.ShouldBlockEnemy(PluginConfig.PeacefulModeEnabled.Value, __instance.Person.IsYourPlayer))
                __result = false;
        }

        /// <summary>Postfix: forces ShallKnowEnemyLate to return false for the local player.</summary>
        private static void BlockShallKnowEnemyLate(EnemyInfo __instance, ref bool __result)
        {
            if (PeacefulLogic.ShouldBlockEnemy(PluginConfig.PeacefulModeEnabled.Value, __instance.Person.IsYourPlayer))
                __result = false;
        }
    }
}
