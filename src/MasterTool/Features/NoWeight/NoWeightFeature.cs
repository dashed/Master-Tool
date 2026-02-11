using System;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using MasterTool.Config;
using MasterTool.Core;
using MasterTool.Plugin;
using MasterTool.Utils;

namespace MasterTool.Features.NoWeight
{
    /// <summary>
    /// Removes weight penalties by patching the inventory weight calculation.
    /// When enabled, sets all equipment weight to zero so the player never
    /// reaches overweight or critically overweight thresholds.
    /// </summary>
    public static class NoWeightFeature
    {
        /// <summary>
        /// Applies the Harmony prefix patch on the inventory weight calculation method.
        /// </summary>
        /// <param name="harmony">The Harmony instance used to install patches.</param>
        public static void PatchAll(Harmony harmony)
        {
            try
            {
                var method = ReflectionHelper.RequireMethod(
                    typeof(InventoryEquipment),
                    nameof(InventoryEquipment.smethod_1),
                    "NoWeightFeature — weight calc"
                );
                if (method == null)
                {
                    return;
                }

                var prefix = new HarmonyMethod(
                    typeof(NoWeightFeature).GetMethod(nameof(WeightPrefix), BindingFlags.Static | BindingFlags.NonPublic)
                );
                harmony.Patch(method, prefix: prefix);
            }
            catch (Exception ex)
            {
                MasterToolPlugin.Log?.LogWarning($"[NoWeight] Failed to patch weight calculation: {ex.Message}");
            }
        }

        private static bool WeightPrefix(ref float __result)
        {
            var newWeight = WeightLogic.ComputeWeight(__result, PluginConfig.NoWeightEnabled.Value, PluginConfig.WeightPercent.Value);
            if (newWeight.HasValue)
            {
                __result = newWeight.Value;
                return false;
            }

            return true;
        }
    }
}
