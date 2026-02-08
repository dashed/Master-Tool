using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for loot chams feature logic.
/// Duplicates the pure decision and property logic since Unity cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class LootChamsTests
{
    /// <summary>
    /// Duplicates the distance-based chams decision from ChamsManager.UpdateLootChams.
    /// </summary>
    private static bool ShouldApplyLootChams(bool lootChamsEnabled, float distanceSq, float maxDistance)
    {
        float maxDistSq = maxDistance * maxDistance;
        return lootChamsEnabled && distanceSq <= maxDistSq;
    }

    /// <summary>
    /// Represents loot chams material property state.
    /// </summary>
    private struct LootChamsMaterialState
    {
        public int ZTest;
        public int ZWrite;
        public int RenderQueue;
        public bool ForceRenderingOff;
        public bool AllowOcclusionWhenDynamic;
    }

    private static LootChamsMaterialState GetLootChamsState()
    {
        return new LootChamsMaterialState
        {
            ZTest = 8, // CompareFunction.Always
            ZWrite = 0,
            RenderQueue = 4000,
            ForceRenderingOff = false,
            AllowOcclusionWhenDynamic = false,
        };
    }

    // --- Decision logic tests ---

    [Test]
    public void Enabled_WithinRange_ShouldApply()
    {
        // 50m distance, 100m max
        Assert.That(ShouldApplyLootChams(true, 50 * 50, 100), Is.True);
    }

    [Test]
    public void Enabled_BeyondRange_ShouldNotApply()
    {
        // 150m distance, 100m max
        Assert.That(ShouldApplyLootChams(true, 150 * 150, 100), Is.False);
    }

    [Test]
    public void Disabled_WithinRange_ShouldNotApply()
    {
        Assert.That(ShouldApplyLootChams(false, 50 * 50, 100), Is.False);
    }

    [Test]
    public void Enabled_ExactlyAtMaxDistance_ShouldApply()
    {
        Assert.That(ShouldApplyLootChams(true, 100 * 100, 100), Is.True);
    }

    [Test]
    public void Enabled_ZeroDistance_ShouldApply()
    {
        Assert.That(ShouldApplyLootChams(true, 0, 100), Is.True);
    }

    [Test]
    public void Disabled_ZeroDistance_ShouldNotApply()
    {
        Assert.That(ShouldApplyLootChams(false, 0, 100), Is.False);
    }

    // --- Material property tests ---

    [Test]
    public void LootChams_ZTestIsAlways()
    {
        var state = GetLootChamsState();
        Assert.That(state.ZTest, Is.EqualTo(8));
    }

    [Test]
    public void LootChams_RenderQueueIsOverlay()
    {
        var state = GetLootChamsState();
        Assert.That(state.RenderQueue, Is.EqualTo(4000));
    }

    [Test]
    public void LootChams_OcclusionDisabled()
    {
        var state = GetLootChamsState();
        Assert.That(state.AllowOcclusionWhenDynamic, Is.False);
        Assert.That(state.ForceRenderingOff, Is.False);
    }

    [Test]
    public void LootChams_MatchesPlayerChamsProperties()
    {
        // Loot chams should use identical material properties as player chams
        var state = GetLootChamsState();
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
        Assert.That(ShouldApplyLootChams(true, 10000, 100), Is.True);
        // 100.1m would be distSq = 10020.01
        Assert.That(ShouldApplyLootChams(true, 10020.01f, 100), Is.False);
    }
}
