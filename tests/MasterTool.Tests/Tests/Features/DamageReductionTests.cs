using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the Phase 2 damage modification logic in DamagePatches.
/// Uses <see cref="DamageLogic"/> from MasterTool.Core (shared library).
/// </summary>
[TestFixture]
public class DamageReductionTests
{
    // --- GodMode tests (existing behavior, verify still works) ---

    [Test]
    public void GodMode_ZeroesDamage()
    {
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(50f, true, BodyPart.Stomach, false, 100, 100, false, false, 100f), Is.EqualTo(0f));
    }

    [Test]
    public void GodMode_OverridesEverything()
    {
        // GodMode should zero even with other features enabled
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(50f, true, BodyPart.Head, true, 50, 50, true, true, 10f), Is.EqualTo(0f));
    }

    // --- Headshot ignore tests ---

    [Test]
    public void IgnoreHeadshots_HeadShot_ZeroesDamage()
    {
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(100f, false, BodyPart.Head, true, 100, 100, false, false, 35f), Is.EqualTo(0f));
    }

    [Test]
    public void IgnoreHeadshots_BodyShot_Unchanged()
    {
        Assert.That(
            DamageLogic.ComputeLocalPlayerDamage(100f, false, BodyPart.Stomach, true, 100, 100, false, false, 100f),
            Is.EqualTo(100f)
        );
    }

    // --- Head damage percentage tests ---

    [Test]
    public void HeadDamagePercent_50_HalvesDamage()
    {
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(100f, false, BodyPart.Head, false, 50, 100, false, false, 35f), Is.EqualTo(50f));
    }

    [Test]
    public void HeadDamagePercent_0_ZeroesDamage()
    {
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(100f, false, BodyPart.Head, false, 0, 100, false, false, 35f), Is.EqualTo(0f));
    }

    [Test]
    public void HeadDamagePercent_BodyShot_NotApplied()
    {
        Assert.That(
            DamageLogic.ComputeLocalPlayerDamage(100f, false, BodyPart.Stomach, false, 50, 100, false, false, 100f),
            Is.EqualTo(100f)
        );
    }

    // --- Damage reduction percentage tests ---

    [Test]
    public void DamageReduction_50_HalvesDamage()
    {
        Assert.That(
            DamageLogic.ComputeLocalPlayerDamage(100f, false, BodyPart.Stomach, false, 100, 50, false, false, 100f),
            Is.EqualTo(50f)
        );
    }

    [Test]
    public void DamageReduction_0_ZeroesDamage()
    {
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(100f, false, BodyPart.Stomach, false, 100, 0, false, false, 100f), Is.EqualTo(0f));
    }

    [Test]
    public void DamageReduction_100_Unchanged()
    {
        Assert.That(
            DamageLogic.ComputeLocalPlayerDamage(100f, false, BodyPart.Stomach, false, 100, 100, false, false, 100f),
            Is.EqualTo(100f)
        );
    }

    // --- Combined head + global reduction ---

    [Test]
    public void HeadDamage50_GlobalReduction50_Stacks()
    {
        // 100 * 0.5 (head) * 0.5 (global) = 25
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(100f, false, BodyPart.Head, false, 50, 50, false, false, 35f), Is.EqualTo(25f));
    }

    // --- Keep 1 Health tests ---

    [Test]
    public void Keep1Health_All_PreventsLethalDamage()
    {
        // HP = 10, damage = 50 -> would kill. Should clamp to 10 - 3 = 7
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(50f, false, BodyPart.Stomach, false, 100, 100, true, true, 10f), Is.EqualTo(7f));
    }

    [Test]
    public void Keep1Health_All_NonLethalDamage_Unchanged()
    {
        // HP = 100, damage = 20 -> 80 remaining, above 3. No clamping.
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(20f, false, BodyPart.Stomach, false, 100, 100, true, true, 100f), Is.EqualTo(20f));
    }

    [Test]
    public void Keep1Health_HeadAndThorax_HeadProtected()
    {
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(50f, false, BodyPart.Head, false, 100, 100, true, true, 10f), Is.EqualTo(7f));
    }

    [Test]
    public void Keep1Health_HeadAndThorax_ChestProtected()
    {
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(50f, false, BodyPart.Chest, false, 100, 100, true, true, 10f), Is.EqualTo(7f));
    }

    [Test]
    public void Keep1Health_HeadAndThorax_LegNotProtected()
    {
        // Leg (not head, not chest) -- should NOT be protected
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(50f, false, BodyPart.LeftLeg, false, 100, 100, true, false, 10f), Is.EqualTo(50f));
    }

    [Test]
    public void Keep1Health_HpAlreadyLow_ZeroDamage()
    {
        // HP = 3, damage = 10 -> max(0, 3-3) = 0
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(10f, false, BodyPart.Stomach, false, 100, 100, true, true, 3f), Is.EqualTo(0f));
    }

    [Test]
    public void Keep1Health_HpBelow3_ZeroDamage()
    {
        // HP = 1, damage = 10 -> max(0, 1-3) = 0
        Assert.That(DamageLogic.ComputeLocalPlayerDamage(10f, false, BodyPart.Stomach, false, 100, 100, true, true, 1f), Is.EqualTo(0f));
    }

    // --- Enemy damage multiplier tests ---

    [Test]
    public void EnemyMultiplier_1_Unchanged()
    {
        Assert.That(DamageLogic.ComputeEnemyDamage(50f, 1f), Is.EqualTo(50f));
    }

    [Test]
    public void EnemyMultiplier_5_Multiplied()
    {
        Assert.That(DamageLogic.ComputeEnemyDamage(50f, 5f), Is.EqualTo(250f));
    }

    [Test]
    public void EnemyMultiplier_20_MaxMultiplied()
    {
        Assert.That(DamageLogic.ComputeEnemyDamage(10f, 20f), Is.EqualTo(200f));
    }
}
