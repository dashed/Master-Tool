using System;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the Phase 2 damage modification logic in DamagePatches.
/// Duplicates the pure damage calculation since Harmony patches and
/// EFT types cannot be used from net9.0 tests.
/// </summary>
[TestFixture]
public class DamageReductionTests
{
    /// <summary>
    /// Mirrors the enhanced BlockDamagePrefix_ActiveHealthController logic.
    /// Returns the modified damage value for the LOCAL player.
    /// </summary>
    private static float ComputeLocalPlayerDamage(
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
                damage = Math.Max(0f, bodyPartCurrentHp - 3f);
            }
        }

        return damage;
    }

    /// <summary>
    /// Mirrors the enemy damage multiplier logic for NON-local players.
    /// </summary>
    private static float ComputeEnemyDamage(float originalDamage, float multiplier)
    {
        if (multiplier > 1f)
        {
            return originalDamage * multiplier;
        }

        return originalDamage;
    }

    // --- GodMode tests (existing behavior, verify still works) ---

    [Test]
    public void GodMode_ZeroesDamage()
    {
        Assert.That(ComputeLocalPlayerDamage(50f, true, false, false, 100, 100, false, "All", 100f, false), Is.EqualTo(0f));
    }

    [Test]
    public void GodMode_OverridesEverything()
    {
        // GodMode should zero even with other features enabled
        Assert.That(ComputeLocalPlayerDamage(50f, true, true, true, 50, 50, true, "All", 10f, false), Is.EqualTo(0f));
    }

    // --- Headshot ignore tests ---

    [Test]
    public void IgnoreHeadshots_HeadShot_ZeroesDamage()
    {
        Assert.That(ComputeLocalPlayerDamage(100f, false, true, true, 100, 100, false, "All", 35f, false), Is.EqualTo(0f));
    }

    [Test]
    public void IgnoreHeadshots_BodyShot_Unchanged()
    {
        Assert.That(ComputeLocalPlayerDamage(100f, false, false, true, 100, 100, false, "All", 100f, false), Is.EqualTo(100f));
    }

    // --- Head damage percentage tests ---

    [Test]
    public void HeadDamagePercent_50_HalvesDamage()
    {
        Assert.That(ComputeLocalPlayerDamage(100f, false, true, false, 50, 100, false, "All", 35f, false), Is.EqualTo(50f));
    }

    [Test]
    public void HeadDamagePercent_0_ZeroesDamage()
    {
        Assert.That(ComputeLocalPlayerDamage(100f, false, true, false, 0, 100, false, "All", 35f, false), Is.EqualTo(0f));
    }

    [Test]
    public void HeadDamagePercent_BodyShot_NotApplied()
    {
        Assert.That(ComputeLocalPlayerDamage(100f, false, false, false, 50, 100, false, "All", 100f, false), Is.EqualTo(100f));
    }

    // --- Damage reduction percentage tests ---

    [Test]
    public void DamageReduction_50_HalvesDamage()
    {
        Assert.That(ComputeLocalPlayerDamage(100f, false, false, false, 100, 50, false, "All", 100f, false), Is.EqualTo(50f));
    }

    [Test]
    public void DamageReduction_0_ZeroesDamage()
    {
        Assert.That(ComputeLocalPlayerDamage(100f, false, false, false, 100, 0, false, "All", 100f, false), Is.EqualTo(0f));
    }

    [Test]
    public void DamageReduction_100_Unchanged()
    {
        Assert.That(ComputeLocalPlayerDamage(100f, false, false, false, 100, 100, false, "All", 100f, false), Is.EqualTo(100f));
    }

    // --- Combined head + global reduction ---

    [Test]
    public void HeadDamage50_GlobalReduction50_Stacks()
    {
        // 100 * 0.5 (head) * 0.5 (global) = 25
        Assert.That(ComputeLocalPlayerDamage(100f, false, true, false, 50, 50, false, "All", 35f, false), Is.EqualTo(25f));
    }

    // --- Keep 1 Health tests ---

    [Test]
    public void Keep1Health_All_PreventsLethalDamage()
    {
        // HP = 10, damage = 50 -> would kill. Should clamp to 10 - 3 = 7
        Assert.That(ComputeLocalPlayerDamage(50f, false, false, false, 100, 100, true, "All", 10f, false), Is.EqualTo(7f));
    }

    [Test]
    public void Keep1Health_All_NonLethalDamage_Unchanged()
    {
        // HP = 100, damage = 20 -> 80 remaining, above 3. No clamping.
        Assert.That(ComputeLocalPlayerDamage(20f, false, false, false, 100, 100, true, "All", 100f, false), Is.EqualTo(20f));
    }

    [Test]
    public void Keep1Health_HeadAndThorax_HeadProtected()
    {
        Assert.That(ComputeLocalPlayerDamage(50f, false, true, false, 100, 100, true, "Head And Thorax", 10f, false), Is.EqualTo(7f));
    }

    [Test]
    public void Keep1Health_HeadAndThorax_ChestProtected()
    {
        Assert.That(ComputeLocalPlayerDamage(50f, false, false, false, 100, 100, true, "Head And Thorax", 10f, true), Is.EqualTo(7f));
    }

    [Test]
    public void Keep1Health_HeadAndThorax_LegNotProtected()
    {
        // Leg (not head, not chest) -- should NOT be protected
        Assert.That(ComputeLocalPlayerDamage(50f, false, false, false, 100, 100, true, "Head And Thorax", 10f, false), Is.EqualTo(50f));
    }

    [Test]
    public void Keep1Health_HpAlreadyLow_ZeroDamage()
    {
        // HP = 3, damage = 10 -> max(0, 3-3) = 0
        Assert.That(ComputeLocalPlayerDamage(10f, false, false, false, 100, 100, true, "All", 3f, false), Is.EqualTo(0f));
    }

    [Test]
    public void Keep1Health_HpBelow3_ZeroDamage()
    {
        // HP = 1, damage = 10 -> max(0, 1-3) = 0
        Assert.That(ComputeLocalPlayerDamage(10f, false, false, false, 100, 100, true, "All", 1f, false), Is.EqualTo(0f));
    }

    // --- Enemy damage multiplier tests ---

    [Test]
    public void EnemyMultiplier_1_Unchanged()
    {
        Assert.That(ComputeEnemyDamage(50f, 1f), Is.EqualTo(50f));
    }

    [Test]
    public void EnemyMultiplier_5_Multiplied()
    {
        Assert.That(ComputeEnemyDamage(50f, 5f), Is.EqualTo(250f));
    }

    [Test]
    public void EnemyMultiplier_20_MaxMultiplied()
    {
        Assert.That(ComputeEnemyDamage(10f, 20f), Is.EqualTo(200f));
    }
}
