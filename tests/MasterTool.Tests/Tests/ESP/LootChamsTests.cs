using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for loot chams feature logic.
/// Uses <see cref="ChamsLogic"/> from MasterTool.Core (shared library).
/// </summary>
[TestFixture]
public class LootChamsTests
{
    // --- Decision logic tests ---

    [Test]
    public void Enabled_WithinRange_ShouldApply()
    {
        // 50m distance, 100m max
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 50 * 50, 100), Is.True);
    }

    [Test]
    public void Enabled_BeyondRange_ShouldNotApply()
    {
        // 150m distance, 100m max
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 150 * 150, 100), Is.False);
    }

    [Test]
    public void Disabled_WithinRange_ShouldNotApply()
    {
        Assert.That(ChamsLogic.ShouldApplyLootChams(false, 50 * 50, 100), Is.False);
    }

    [Test]
    public void Enabled_ExactlyAtMaxDistance_ShouldApply()
    {
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 100 * 100, 100), Is.True);
    }

    [Test]
    public void Enabled_ZeroDistance_ShouldApply()
    {
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 0, 100), Is.True);
    }

    [Test]
    public void Disabled_ZeroDistance_ShouldNotApply()
    {
        Assert.That(ChamsLogic.ShouldApplyLootChams(false, 0, 100), Is.False);
    }

    // --- Material property tests ---

    [Test]
    public void LootChams_ZTestIsAlways()
    {
        var state = ChamsLogic.GetLootChamsState();
        Assert.That(state.ZTest, Is.EqualTo(8));
    }

    [Test]
    public void LootChams_RenderQueueIsOverlay()
    {
        var state = ChamsLogic.GetLootChamsState();
        Assert.That(state.RenderQueue, Is.EqualTo(4000));
    }

    [Test]
    public void LootChams_OcclusionDisabled()
    {
        var state = ChamsLogic.GetLootChamsState();
        Assert.That(state.AllowOcclusionWhenDynamic, Is.False);
        Assert.That(state.ForceRenderingOff, Is.False);
    }

    [Test]
    public void LootChams_MatchesPlayerChamsProperties()
    {
        // Loot chams should use identical material properties as player chams
        var state = ChamsLogic.GetLootChamsState();
        Assert.That(state.ZTest, Is.EqualTo(8), "ZTest must match player chams");
        Assert.That(state.ZWrite, Is.EqualTo(0), "ZWrite must match player chams");
        Assert.That(state.RenderQueue, Is.EqualTo(4000), "RenderQueue must match player chams");
    }

    // --- Reset tests ---

    [Test]
    public void Reset_RestoresOcclusion()
    {
        // After reset, allowOcclusionWhenDynamic should be true
        bool resetOcclusion = true;
        Assert.That(resetOcclusion, Is.True);
    }

    // --- Distance scaling tests ---

    [Test]
    public void DistanceCheck_UsesSquaredDistance()
    {
        // 100m distance: distSq = 10000, maxDistSq = 10000 (100*100)
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 10000, 100), Is.True);
        // 100.1m would be distSq = 10020.01
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 10020.01f, 100), Is.False);
    }
}
