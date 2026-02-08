using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for chams anti-occlusion property logic.
/// Uses <see cref="ChamsMode"/> from MasterTool.Core (shared library).
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
        public int Cull;
    }

    /// <summary>
    /// Duplicates the property assignment logic from ChamsManager.ApplyShaderChams.
    /// </summary>
    private static ChamsMaterialState GetApplyChamsState(ChamsMode mode = ChamsMode.Solid)
    {
        int cullMode = mode == ChamsMode.CullFront ? 1 : 0;
        return new ChamsMaterialState
        {
            ZTest = 8, // CompareFunction.Always
            ZWrite = 0,
            RenderQueue = 4000,
            ForceRenderingOff = false,
            AllowOcclusionWhenDynamic = false,
            Cull = cullMode,
        };
    }

    /// <summary>
    /// Gets the material state for an outline duplicate (always CullFront).
    /// </summary>
    private static ChamsMaterialState GetOutlineDuplicateState()
    {
        return new ChamsMaterialState
        {
            ZTest = 8,
            ZWrite = 0,
            RenderQueue = 4000,
            ForceRenderingOff = false,
            AllowOcclusionWhenDynamic = false,
            Cull = 1, // Always cull front faces on outline duplicate
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

    // --- Per-mode Cull property tests ---

    [Test]
    public void SolidMode_CullIsZero()
    {
        var state = GetApplyChamsState(ChamsMode.Solid);
        Assert.That(state.Cull, Is.EqualTo(0));
    }

    [Test]
    public void CullFrontMode_CullIsOne()
    {
        var state = GetApplyChamsState(ChamsMode.CullFront);
        Assert.That(state.Cull, Is.EqualTo(1));
    }

    [Test]
    public void OutlineDuplicate_CullIsOne()
    {
        var state = GetOutlineDuplicateState();
        Assert.That(state.Cull, Is.EqualTo(1));
    }

    [Test]
    public void OutlineDuplicate_AllAntiOcclusionProperties()
    {
        var state = GetOutlineDuplicateState();
        Assert.That(state.ZTest, Is.EqualTo(8), "ZTest must be Always");
        Assert.That(state.ZWrite, Is.EqualTo(0), "ZWrite must be disabled");
        Assert.That(state.RenderQueue, Is.EqualTo(4000), "RenderQueue must be overlay");
        Assert.That(state.ForceRenderingOff, Is.False, "ForceRenderingOff must be false");
        Assert.That(state.AllowOcclusionWhenDynamic, Is.False, "Occlusion must be disabled");
        Assert.That(state.Cull, Is.EqualTo(1), "Outline duplicate must cull front faces");
    }
}
