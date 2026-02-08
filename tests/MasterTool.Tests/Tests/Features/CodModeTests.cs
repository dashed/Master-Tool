using System;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the COD Mode heal logic used by CodModeFeature.
/// Duplicates the pure heal-calculation and timer logic since
/// ActiveHealthController cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class CodModeTests
{
    /// <summary>
    /// Mirrors the heal-delay check in CodModeFeature.
    /// Returns true when enough time has passed since the last hit.
    /// </summary>
    private static bool ShouldHeal(float timeSinceHit, float healDelay)
    {
        return timeSinceHit >= healDelay;
    }

    /// <summary>
    /// Mirrors the heal-amount calculation in CodModeFeature.
    /// Clamps the heal to the remaining HP gap.
    /// </summary>
    private static float CalculateHealAmount(float current, float maximum, float healRate)
    {
        if (current >= maximum)
        {
            return 0f;
        }

        return Math.Min(healRate, maximum - current);
    }

    /// <summary>
    /// Simulates time since last damage was taken.
    /// </summary>
    private float _timeSinceLastHit;

    /// <summary>
    /// Simulates whether regeneration is actively occurring.
    /// </summary>
    private bool _isRegenerating;

    /// <summary>
    /// Mirrors the damage-notification logic: resets timer and stops regen.
    /// </summary>
    private void NotifyDamage()
    {
        _timeSinceLastHit = 0f;
        _isRegenerating = false;
    }

    [SetUp]
    public void SetUp()
    {
        _timeSinceLastHit = 0f;
        _isRegenerating = false;
    }

    // === ShouldHeal Tests ===

    [Test]
    public void ShouldHeal_TimeBelowDelay_ReturnsFalse()
    {
        Assert.That(ShouldHeal(5f, 10f), Is.False);
    }

    [Test]
    public void ShouldHeal_TimeEqualsDelay_ReturnsTrue()
    {
        Assert.That(ShouldHeal(10f, 10f), Is.True);
    }

    [Test]
    public void ShouldHeal_TimeAboveDelay_ReturnsTrue()
    {
        Assert.That(ShouldHeal(15f, 10f), Is.True);
    }

    [Test]
    public void ShouldHeal_ZeroDelay_ReturnsTrue()
    {
        Assert.That(ShouldHeal(0f, 0f), Is.True);
    }

    [Test]
    public void ShouldHeal_ZeroTimeLargeDelay_ReturnsFalse()
    {
        Assert.That(ShouldHeal(0f, 600f), Is.False);
    }

    [Test]
    public void ShouldHeal_VeryLargeTime_ReturnsTrue()
    {
        Assert.That(ShouldHeal(99999f, 10f), Is.True);
    }

    // === CalculateHealAmount Tests ===

    [Test]
    public void CalculateHeal_NormalHeal_ReturnsRate()
    {
        Assert.That(CalculateHealAmount(50f, 100f, 10f), Is.EqualTo(10f));
    }

    [Test]
    public void CalculateHeal_ClampToMax_ReturnsRemaining()
    {
        Assert.That(CalculateHealAmount(95f, 100f, 10f), Is.EqualTo(5f));
    }

    [Test]
    public void CalculateHeal_AtMax_ReturnsZero()
    {
        Assert.That(CalculateHealAmount(100f, 100f, 10f), Is.EqualTo(0f));
    }

    [Test]
    public void CalculateHeal_AboveMax_ReturnsZero()
    {
        Assert.That(CalculateHealAmount(105f, 100f, 10f), Is.EqualTo(0f));
    }

    [Test]
    public void CalculateHeal_FromZero_ReturnsRate()
    {
        Assert.That(CalculateHealAmount(0f, 100f, 10f), Is.EqualTo(10f));
    }

    [Test]
    public void CalculateHeal_FromZero_MaxBelowRate_ReturnsMax()
    {
        Assert.That(CalculateHealAmount(0f, 5f, 10f), Is.EqualTo(5f));
    }

    [Test]
    public void CalculateHeal_OneHpRemaining_ReturnsOne()
    {
        Assert.That(CalculateHealAmount(99f, 100f, 10f), Is.EqualTo(1f));
    }

    [Test]
    public void CalculateHeal_LargeHealRate_ClampsToRemaining()
    {
        Assert.That(CalculateHealAmount(10f, 15f, 100f), Is.EqualTo(5f));
    }

    // === Timer/State Tests ===

    [Test]
    public void NotifyDamage_ResetsTimer()
    {
        _timeSinceLastHit = 15f;
        NotifyDamage();
        Assert.That(_timeSinceLastHit, Is.EqualTo(0f));
    }

    [Test]
    public void NotifyDamage_StopsRegeneration()
    {
        _isRegenerating = true;
        NotifyDamage();
        Assert.That(_isRegenerating, Is.False);
    }

    [Test]
    public void Timer_AccumulatesCorrectly()
    {
        _timeSinceLastHit += 0.016f; // ~60fps frame
        _timeSinceLastHit += 0.016f;
        _timeSinceLastHit += 0.016f;
        Assert.That(_timeSinceLastHit, Is.EqualTo(0.048f).Within(0.001f));
    }

    [Test]
    public void FullCycle_DamageResetsHealProgress()
    {
        _timeSinceLastHit = 12f; // past 10s delay
        Assert.That(ShouldHeal(_timeSinceLastHit, 10f), Is.True);
        NotifyDamage(); // take damage
        Assert.That(ShouldHeal(_timeSinceLastHit, 10f), Is.False);
    }

    // === Body Part Coverage ===

    [Test]
    public void AllBodyParts_HasSevenEntries()
    {
        // Mirrors the AllBodyParts array in CodModeFeature
        var bodyParts = new[] { "Head", "Chest", "Stomach", "LeftArm", "RightArm", "LeftLeg", "RightLeg" };
        Assert.That(bodyParts, Has.Length.EqualTo(7));
    }
}
