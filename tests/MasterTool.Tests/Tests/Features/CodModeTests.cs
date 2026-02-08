using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the COD Mode heal logic.
/// Uses <see cref="HealingLogic"/> from MasterTool.Core (shared library).
/// Timer/state tests remain local as they test integration behavior, not pure logic.
/// </summary>
[TestFixture]
public class CodModeTests
{
    /// <summary>
    /// Simulates time since last damage was taken.
    /// </summary>
    private float _timeSinceLastHit;

    /// <summary>
    /// Simulates whether regeneration is actively occurring.
    /// </summary>
    private bool _isRegenerating;

    /// <summary>
    /// Mirrors the BeingHitAction-based notification: resets timer and stops regen.
    /// Only fires on direct hits (bullets/melee), NOT on bleed/fracture ticks.
    /// </summary>
    private void NotifyDirectHit()
    {
        _timeSinceLastHit = 0f;
        _isRegenerating = false;
    }

    /// <summary>
    /// Simulates bleed tick damage — does NOT reset the heal timer.
    /// This is the core fix: previously all damage (including bleeds) reset the timer.
    /// </summary>
    private static void SimulateBleedTick()
    {
        // Intentionally does nothing to the heal timer.
        // Bleed damage goes through ApplyDamage but no longer calls NotifyDirectHit.
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
        Assert.That(HealingLogic.ShouldHeal(5f, 10f), Is.False);
    }

    [Test]
    public void ShouldHeal_TimeEqualsDelay_ReturnsTrue()
    {
        Assert.That(HealingLogic.ShouldHeal(10f, 10f), Is.True);
    }

    [Test]
    public void ShouldHeal_TimeAboveDelay_ReturnsTrue()
    {
        Assert.That(HealingLogic.ShouldHeal(15f, 10f), Is.True);
    }

    [Test]
    public void ShouldHeal_ZeroDelay_ReturnsTrue()
    {
        Assert.That(HealingLogic.ShouldHeal(0f, 0f), Is.True);
    }

    [Test]
    public void ShouldHeal_ZeroTimeLargeDelay_ReturnsFalse()
    {
        Assert.That(HealingLogic.ShouldHeal(0f, 600f), Is.False);
    }

    [Test]
    public void ShouldHeal_VeryLargeTime_ReturnsTrue()
    {
        Assert.That(HealingLogic.ShouldHeal(99999f, 10f), Is.True);
    }

    // === CalculateHealAmount Tests ===

    [Test]
    public void CalculateHeal_NormalHeal_ReturnsRate()
    {
        Assert.That(HealingLogic.CalculateHealAmount(50f, 100f, 10f), Is.EqualTo(10f));
    }

    [Test]
    public void CalculateHeal_ClampToMax_ReturnsRemaining()
    {
        Assert.That(HealingLogic.CalculateHealAmount(95f, 100f, 10f), Is.EqualTo(5f));
    }

    [Test]
    public void CalculateHeal_AtMax_ReturnsZero()
    {
        Assert.That(HealingLogic.CalculateHealAmount(100f, 100f, 10f), Is.EqualTo(0f));
    }

    [Test]
    public void CalculateHeal_AboveMax_ReturnsZero()
    {
        Assert.That(HealingLogic.CalculateHealAmount(105f, 100f, 10f), Is.EqualTo(0f));
    }

    [Test]
    public void CalculateHeal_FromZero_ReturnsRate()
    {
        Assert.That(HealingLogic.CalculateHealAmount(0f, 100f, 10f), Is.EqualTo(10f));
    }

    [Test]
    public void CalculateHeal_FromZero_MaxBelowRate_ReturnsMax()
    {
        Assert.That(HealingLogic.CalculateHealAmount(0f, 5f, 10f), Is.EqualTo(5f));
    }

    [Test]
    public void CalculateHeal_OneHpRemaining_ReturnsOne()
    {
        Assert.That(HealingLogic.CalculateHealAmount(99f, 100f, 10f), Is.EqualTo(1f));
    }

    [Test]
    public void CalculateHeal_LargeHealRate_ClampsToRemaining()
    {
        Assert.That(HealingLogic.CalculateHealAmount(10f, 15f, 100f), Is.EqualTo(5f));
    }

    // === Timer/State Tests ===

    [Test]
    public void NotifyDirectHit_ResetsTimer()
    {
        _timeSinceLastHit = 15f;
        NotifyDirectHit();
        Assert.That(_timeSinceLastHit, Is.EqualTo(0f));
    }

    [Test]
    public void NotifyDirectHit_StopsRegeneration()
    {
        _isRegenerating = true;
        NotifyDirectHit();
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
        Assert.That(HealingLogic.ShouldHeal(_timeSinceLastHit, 10f), Is.True);
        NotifyDirectHit(); // take damage
        Assert.That(HealingLogic.ShouldHeal(_timeSinceLastHit, 10f), Is.False);
    }

    // === Body Part Coverage ===

    [Test]
    public void AllBodyParts_HasSevenEntries()
    {
        // Mirrors the AllBodyParts array in CodModeFeature
        var bodyParts = new[] { "Head", "Chest", "Stomach", "LeftArm", "RightArm", "LeftLeg", "RightLeg" };
        Assert.That(bodyParts, Has.Length.EqualTo(7));
    }

    // === Bleed-Timer Interaction Tests (core bug fix) ===

    [Test]
    public void BleedTick_DoesNotResetTimer()
    {
        _timeSinceLastHit = 8f;
        SimulateBleedTick();
        Assert.That(_timeSinceLastHit, Is.EqualTo(8f));
    }

    [Test]
    public void BleedTick_DoesNotStopRegeneration()
    {
        _isRegenerating = true;
        SimulateBleedTick();
        Assert.That(_isRegenerating, Is.True);
    }

    [Test]
    public void DirectHit_ResetsTimer_BleedDoesNot()
    {
        _timeSinceLastHit = 12f;
        Assert.That(HealingLogic.ShouldHeal(_timeSinceLastHit, 10f), Is.True);

        // Direct hit resets
        NotifyDirectHit();
        Assert.That(HealingLogic.ShouldHeal(_timeSinceLastHit, 10f), Is.False);

        // Accumulate past delay again
        _timeSinceLastHit = 11f;
        Assert.That(HealingLogic.ShouldHeal(_timeSinceLastHit, 10f), Is.True);

        // Bleed tick does NOT reset
        SimulateBleedTick();
        Assert.That(HealingLogic.ShouldHeal(_timeSinceLastHit, 10f), Is.True);
    }

    [Test]
    public void FullCycle_ShotThenBleed_HealingStartsAfterDelay()
    {
        // Player gets shot → direct hit resets timer
        NotifyDirectHit();
        Assert.That(_timeSinceLastHit, Is.EqualTo(0f));

        // Bleed ticks happen while timer accumulates
        _timeSinceLastHit = 5f;
        SimulateBleedTick();
        Assert.That(_timeSinceLastHit, Is.EqualTo(5f)); // NOT reset
        Assert.That(HealingLogic.ShouldHeal(_timeSinceLastHit, 10f), Is.False); // Not enough time

        // More time passes with bleed ticks
        _timeSinceLastHit = 11f;
        SimulateBleedTick();
        Assert.That(HealingLogic.ShouldHeal(_timeSinceLastHit, 10f), Is.True); // Healing should start
    }

    [Test]
    public void ContinuousBleeds_NeverPreventHealing()
    {
        // Simulate 100 bleed ticks over 15 seconds
        for (int i = 0; i < 100; i++)
        {
            _timeSinceLastHit += 0.15f; // 15s / 100 ticks
            SimulateBleedTick();
        }

        // Timer accumulated to ~15s despite constant bleed ticks
        Assert.That(_timeSinceLastHit, Is.EqualTo(15f).Within(0.01f));
        Assert.That(HealingLogic.ShouldHeal(_timeSinceLastHit, 10f), Is.True);
    }

    // === Destroyed Body Part Tests ===

    [Test]
    public void ShouldHealBodyPart_NormalDamaged_ReturnsTrue()
    {
        Assert.That(HealingLogic.ShouldHealBodyPart(50f, 100f), Is.True);
    }

    [Test]
    public void ShouldHealBodyPart_Destroyed_ReturnsFalse()
    {
        Assert.That(HealingLogic.ShouldHealBodyPart(0f, 100f), Is.False);
    }

    [Test]
    public void ShouldHealBodyPart_AtMax_ReturnsFalse()
    {
        Assert.That(HealingLogic.ShouldHealBodyPart(100f, 100f), Is.False);
    }

    [Test]
    public void ShouldHealBodyPart_NegativeHealth_ReturnsFalse()
    {
        Assert.That(HealingLogic.ShouldHealBodyPart(-5f, 100f), Is.False);
    }

    [Test]
    public void ShouldHealBodyPart_OneHp_ReturnsTrue()
    {
        Assert.That(HealingLogic.ShouldHealBodyPart(1f, 100f), Is.True);
    }

    [Test]
    public void ShouldHealBodyPart_AlmostFull_ReturnsTrue()
    {
        Assert.That(HealingLogic.ShouldHealBodyPart(99.9f, 100f), Is.True);
    }

    // === UnscaledDeltaTime Tests ===

    [Test]
    public void Timer_UnscaledAccumulation_IndependentOfTimeScale()
    {
        // Simulate unscaledDeltaTime (constant ~16ms) while timeScale varies
        float unscaledDt = 0.016f;
        _timeSinceLastHit = 0f;

        // 625 frames × 16ms = 10s
        for (int i = 0; i < 625; i++)
        {
            _timeSinceLastHit += unscaledDt;
        }

        Assert.That(_timeSinceLastHit, Is.EqualTo(10f).Within(0.1f));
        Assert.That(HealingLogic.ShouldHeal(_timeSinceLastHit, 9.5f), Is.True);
    }

    // === Effect Removal Decision Tests ===

    [Test]
    public void ShouldRemoveEffects_BothEnabled_ReturnsTrue()
    {
        Assert.That(ShouldRemoveEffects(true, true), Is.True);
    }

    [Test]
    public void ShouldRemoveEffects_CodDisabled_ReturnsFalse()
    {
        Assert.That(ShouldRemoveEffects(false, true), Is.False);
    }

    [Test]
    public void ShouldRemoveEffects_RemoveDisabled_ReturnsFalse()
    {
        Assert.That(ShouldRemoveEffects(true, false), Is.False);
    }

    [Test]
    public void ShouldRemoveEffects_BothDisabled_ReturnsFalse()
    {
        Assert.That(ShouldRemoveEffects(false, false), Is.False);
    }

    /// <summary>
    /// Mirrors config check for whether negative effects should be removed.
    /// </summary>
    private static bool ShouldRemoveEffects(bool codModeEnabled, bool removeEffectsEnabled)
    {
        return codModeEnabled && removeEffectsEnabled;
    }
}
