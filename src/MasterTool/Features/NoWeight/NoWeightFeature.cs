using System;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using MasterTool.Config;

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
                var method = AccessTools.Method(typeof(InventoryEquipment), nameof(InventoryEquipment.smethod_1));
                if (method == null)
                {
                    return;
                }

                var prefix = new HarmonyMethod(
                    typeof(NoWeightFeature).GetMethod(nameof(WeightPrefix), BindingFlags.Static | BindingFlags.NonPublic)
                );
                harmony.Patch(method, prefix: prefix);
            }
            catch { }
        }

        private static bool WeightPrefix(ref float __result)
        {
            if (PluginConfig.NoWeightEnabled.Value)
            {
                __result = 0f;
                return false;
            }

            return true;
        }
    }
}
