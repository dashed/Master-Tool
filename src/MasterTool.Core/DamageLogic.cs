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
    /// <param name="originalDamage">Raw incoming damage.</param>
    /// <param name="godMode">Whether god mode is enabled (zeroes all damage).</param>
    /// <param name="bodyPart">Which body part is being hit.</param>
    /// <param name="ignoreHeadshots">Whether headshot damage is fully blocked.</param>
    /// <param name="headDamagePercent">Percentage of head damage to apply (0-100).</param>
    /// <param name="damageReductionPercent">Global damage reduction percentage (0-100).</param>
    /// <param name="keep1Health">Whether Keep 1 Health clamping is enabled.</param>
    /// <param name="shouldProtectPart">Whether this specific body part is protected (pre-computed via BodyPartProtection.ShouldProtect).</param>
    /// <param name="bodyPartCurrentHp">Current HP of the body part being damaged.</param>
    public static float ComputeLocalPlayerDamage(
        float originalDamage,
        bool godMode,
        BodyPart bodyPart,
        bool ignoreHeadshots,
        int headDamagePercent,
        int damageReductionPercent,
        bool keep1Health,
        bool shouldProtectPart,
        float bodyPartCurrentHp
    )
    {
        if (godMode)
        {
            return 0f;
        }

        bool isHead = bodyPart == BodyPart.Head;

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

        if (keep1Health && shouldProtectPart && (bodyPartCurrentHp - damage) < 3f)
        {
            damage = (float)Math.Max(0.0, bodyPartCurrentHp - 3f);
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
