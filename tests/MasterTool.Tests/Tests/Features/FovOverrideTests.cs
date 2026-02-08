using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for FOV override logic: ADS skip decision, direct assignment behavior,
/// and LateUpdate frame ordering.
/// Uses <see cref="VisionLogic"/> from MasterTool.Core.
/// </summary>
[TestFixture]
public class FovOverrideTests
{
    /// <summary>
    /// Simulates a frame where the game sets FOV and then our LateUpdate overrides it.
    /// Returns the final FOV value after both updates.
    /// </summary>
    private static float SimulateFrameFov(float gameFov, float modTargetFov, bool shouldOverride)
    {
        // Game sets FOV during its update
        float currentFov = gameFov;

        // Our LateUpdate runs after
        if (shouldOverride)
        {
            currentFov = modTargetFov;
        }

        return currentFov;
    }

    // --- ADS skip decision tests ---

    [Test]
    public void Enabled_NotAiming_ShouldOverride()
    {
        Assert.That(VisionLogic.ShouldOverrideFov(true, false, false), Is.True);
    }

    [Test]
    public void Enabled_Aiming_OverrideAdsOn_ShouldOverride()
    {
        Assert.That(VisionLogic.ShouldOverrideFov(true, true, true), Is.True);
    }

    [Test]
    public void Enabled_Aiming_OverrideAdsOff_ShouldNotOverride()
    {
        Assert.That(VisionLogic.ShouldOverrideFov(true, true, false), Is.False);
    }

    [Test]
    public void Disabled_NotAiming_ShouldNotOverride()
    {
        Assert.That(VisionLogic.ShouldOverrideFov(false, false, false), Is.False);
    }

    [Test]
    public void Disabled_Aiming_ShouldNotOverride()
    {
        Assert.That(VisionLogic.ShouldOverrideFov(false, true, false), Is.False);
    }

    [Test]
    public void Disabled_Aiming_OverrideAdsOn_ShouldNotOverride()
    {
        Assert.That(VisionLogic.ShouldOverrideFov(false, true, true), Is.False);
    }

    // --- Direct assignment tests (no lerp) ---

    [Test]
    public void DirectAssignment_SetsExactValue()
    {
        // Game sets 50 (ADS), our target is 85 — result should be exactly 85
        float result = SimulateFrameFov(50f, 85f, true);
        Assert.That(result, Is.EqualTo(85f));
    }

    [Test]
    public void DirectAssignment_NoLerpDelay()
    {
        // Even with large difference, no partial lerp — instant
        float result = SimulateFrameFov(30f, 120f, true);
        Assert.That(result, Is.EqualTo(120f));
    }

    [Test]
    public void AdsSkip_PreservesGameFov()
    {
        // When ADS skip is active, game's FOV should remain
        float result = SimulateFrameFov(50f, 85f, false);
        Assert.That(result, Is.EqualTo(50f));
    }

    // --- LateUpdate ordering tests ---

    [Test]
    public void LateUpdate_OverridesGameUpdate()
    {
        // Simulate: Update sets FOV to 65 (crouch), LateUpdate sets to 85
        float afterUpdate = 65f; // Game's crouch FOV
        float afterLateUpdate = SimulateFrameFov(afterUpdate, 85f, true);
        Assert.That(afterLateUpdate, Is.EqualTo(85f), "LateUpdate should win over Update");
    }

    [Test]
    public void LateUpdate_StanceChange_NoFlicker()
    {
        // Simulate multiple frames during stance transition
        float[] gameStanceFovs = { 75f, 70f, 65f, 65f }; // Standing -> crouching
        float modTarget = 85f;

        foreach (float gameFov in gameStanceFovs)
        {
            float result = SimulateFrameFov(gameFov, modTarget, true);
            Assert.That(result, Is.EqualTo(85f), $"Frame with game FOV {gameFov} should still be 85");
        }
    }

    [Test]
    public void LateUpdate_AdsTransition_SkipsCorrectly()
    {
        // Frame 1: Not aiming — override
        Assert.That(SimulateFrameFov(75f, 85f, VisionLogic.ShouldOverrideFov(true, false, false)), Is.EqualTo(85f));
        // Frame 2: ADS starts — skip (game zooms to 50)
        Assert.That(SimulateFrameFov(50f, 85f, VisionLogic.ShouldOverrideFov(true, true, false)), Is.EqualTo(50f));
        // Frame 3: ADS ends — override again
        Assert.That(SimulateFrameFov(75f, 85f, VisionLogic.ShouldOverrideFov(true, false, false)), Is.EqualTo(85f));
    }

    // --- Edge cases ---

    [Test]
    public void SameTargetAsGame_NoChange()
    {
        float result = SimulateFrameFov(75f, 75f, true);
        Assert.That(result, Is.EqualTo(75f));
    }

    [Test]
    public void AdsOverrideOn_StillOverrides()
    {
        // User wants custom FOV even during ADS
        float result = SimulateFrameFov(50f, 85f, VisionLogic.ShouldOverrideFov(true, true, true));
        Assert.That(result, Is.EqualTo(85f), "Override ADS should force mod FOV during ADS");
    }

    [Test]
    public void DisabledDuringAds_GameFovPreserved()
    {
        // Feature disabled — game FOV should always win
        float result = SimulateFrameFov(50f, 85f, VisionLogic.ShouldOverrideFov(false, true, true));
        Assert.That(result, Is.EqualTo(50f));
    }
}
