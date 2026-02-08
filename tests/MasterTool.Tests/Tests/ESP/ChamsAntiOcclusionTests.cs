using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for chams anti-occlusion property logic.
/// Duplicates the pure property-setting decisions since Unity cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class ChamsAntiOcclusionTests
{
    /// <summary>
    /// Represents the chams material property state set during ApplyChams.
    /// </summary>
    private struct ChamsMaterialState
    {
        public int ZTest;
        public int ZWrite;
        public int RenderQueue;
        public bool ForceRenderingOff;
        public bool AllowOcclusionWhenDynamic;
    }

    /// <summary>
    /// Duplicates the property assignment logic from ChamsManager.ApplyChams.
    /// </summary>
    private static ChamsMaterialState GetApplyChamsState()
    {
        return new ChamsMaterialState
        {
            ZTest = 8, // CompareFunction.Always
            ZWrite = 0,
            RenderQueue = 4000,
            ForceRenderingOff = false,
            AllowOcclusionWhenDynamic = false,
        };
    }

    /// <summary>
    /// Duplicates the property restoration logic from ChamsManager.ResetChams.
    /// </summary>
    private static bool GetResetAllowOcclusion()
    {
        return true;
    }

    // --- Apply chams property tests ---

    [Test]
    public void ApplyChams_ZTestIsAlways()
    {
        var state = GetApplyChamsState();
        // CompareFunction.Always = 8
        Assert.That(state.ZTest, Is.EqualTo(8));
    }

    [Test]
    public void ApplyChams_ZWriteDisabled()
    {
        var state = GetApplyChamsState();
        Assert.That(state.ZWrite, Is.EqualTo(0));
    }

    [Test]
    public void ApplyChams_RenderQueueIsOverlay()
    {
        var state = GetApplyChamsState();
        // 4000 = overlay queue, renders after all geometry
        Assert.That(state.RenderQueue, Is.EqualTo(4000));
    }

    [Test]
    public void ApplyChams_ForceRenderingOff_IsFalse()
    {
        var state = GetApplyChamsState();
        Assert.That(state.ForceRenderingOff, Is.False);
    }

    [Test]
    public void ApplyChams_OcclusionDisabled()
    {
        var state = GetApplyChamsState();
        Assert.That(state.AllowOcclusionWhenDynamic, Is.False);
    }

    [Test]
    public void ApplyChams_RenderQueueAboveGeometry()
    {
        var state = GetApplyChamsState();
        // Standard geometry queue is 2000, transparent is 3000
        Assert.That(state.RenderQueue, Is.GreaterThan(3000));
    }

    // --- Reset chams property tests ---

    [Test]
    public void ResetChams_RestoresOcclusion()
    {
        var allowOcclusion = GetResetAllowOcclusion();
        Assert.That(allowOcclusion, Is.True);
    }

    // --- State transition tests ---

    [Test]
    public void ApplyThenReset_OcclusionCyclesCorrectly()
    {
        // Apply: occlusion disabled
        var applyState = GetApplyChamsState();
        Assert.That(applyState.AllowOcclusionWhenDynamic, Is.False);

        // Reset: occlusion restored
        var resetOcclusion = GetResetAllowOcclusion();
        Assert.That(resetOcclusion, Is.True);
    }

    [Test]
    public void ApplyChams_AllPropertiesSetTogether()
    {
        var state = GetApplyChamsState();
        // All anti-occlusion properties must be set together for reliable through-wall rendering
        Assert.That(state.ZTest, Is.EqualTo(8), "ZTest must be Always");
        Assert.That(state.ZWrite, Is.EqualTo(0), "ZWrite must be disabled");
        Assert.That(state.RenderQueue, Is.EqualTo(4000), "RenderQueue must be overlay");
        Assert.That(state.ForceRenderingOff, Is.False, "ForceRenderingOff must be false");
        Assert.That(state.AllowOcclusionWhenDynamic, Is.False, "Occlusion must be disabled");
    }
}
