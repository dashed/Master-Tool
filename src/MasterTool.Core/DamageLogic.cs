using System;

namespace MasterTool.Core;

/// <summary>
/// Pure damage-related logic extracted from DamagePatches for shared testing.
/// </summary>
public static class DamageLogic
{
    /// <summary>
    /// Determines whether the original damage method should be blocked (skipped).
    /// Returns true if god mode is enabled and the target is the local player.
    /// </summary>
    public static bool ShouldBlockForPlayer(bool godModeEnabled, bool? isYourPlayer)
    {
        if (isYourPlayer == null)
        {
            return false;
        }

        if (!isYourPlayer.Value)
        {
            return false;
        }

        return godModeEnabled;
    }

    /// <summary>
    /// Returns the damage value after god mode modification.
    /// Zeroes damage if god mode is enabled and the target is the local player.
    /// </summary>
    public static float ApplyDamageModification(float originalDamage, bool godModeEnabled, bool? isYourPlayer)
    {
        if (isYourPlayer == null || !isYourPlayer.Value)
        {
            return originalDamage;
        }

        if (godModeEnabled)
        {
            return 0f;
        }

        return originalDamage;
    }

    /// <summary>
    /// Computes the modified damage for the local player with all damage reduction features.
    /// </summary>
    public static float ComputeLocalPlayerDamage(
        float originalDamage,
        bool godMode,
        bool isHead,
        bool ignoreHeadshots,
        int headDamagePercent,
        int damageReductionPercent,
        bool keep1Health,
        string keep1Selection,
        float bodyPartCurrentHp,
        bool isChest
    )
    {
        if (godMode)
        {
            return 0f;
        }

        if (isHead && ignoreHeadshots)
        {
            return 0f;
        }

        float damage = originalDamage;

        if (isHead && headDamagePercent < 100)
        {
            damage *= headDamagePercent / 100f;
        }

        if (damageReductionPercent < 100)
        {
            damage *= damageReductionPercent / 100f;
        }

        if (keep1Health)
        {
            bool shouldProtect = keep1Selection == "All" || (keep1Selection == "Head And Thorax" && (isHead || isChest));

            if (shouldProtect && (bodyPartCurrentHp - damage) < 3f)
            {
                damage = (float)Math.Max(0.0, bodyPartCurrentHp - 3f);
            }
        }

        return damage;
    }

    /// <summary>
    /// Computes the enemy damage with a multiplier for non-local players.
    /// </summary>
    public static float ComputeEnemyDamage(float originalDamage, float multiplier)
    {
        if (multiplier > 1f)
        {
            return originalDamage * multiplier;
        }

        return originalDamage;
    }
}
