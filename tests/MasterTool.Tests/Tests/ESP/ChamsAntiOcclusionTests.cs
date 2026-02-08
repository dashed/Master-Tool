using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for chams anti-occlusion property logic.
/// Uses <see cref="ChamsLogic"/> from MasterTool.Core (shared library).
/// </summary>
[TestFixture]
public class ChamsAntiOcclusionTests
{
    // --- Apply chams property tests ---

    [Test]
    public void ApplyChams_ZTestIsAlways()
    {
        var state = ChamsLogic.GetApplyChamsState();
        // CompareFunction.Always = 8
        Assert.That(state.ZTest, Is.EqualTo(8));
    }

    [Test]
    public void ApplyChams_ZWriteDisabled()
    {
        var state = ChamsLogic.GetApplyChamsState();
        Assert.That(state.ZWrite, Is.EqualTo(0));
    }

    [Test]
    public void ApplyChams_RenderQueueIsOverlay()
    {
        var state = ChamsLogic.GetApplyChamsState();
        // 4000 = overlay queue, renders after all geometry
        Assert.That(state.RenderQueue, Is.EqualTo(4000));
    }

    [Test]
    public void ApplyChams_ForceRenderingOff_IsFalse()
    {
        var state = ChamsLogic.GetApplyChamsState();
        Assert.That(state.ForceRenderingOff, Is.False);
    }

    [Test]
    public void ApplyChams_OcclusionDisabled()
    {
        var state = ChamsLogic.GetApplyChamsState();
        Assert.That(state.AllowOcclusionWhenDynamic, Is.False);
    }

    [Test]
    public void ApplyChams_RenderQueueAboveGeometry()
    {
        var state = ChamsLogic.GetApplyChamsState();
        // Standard geometry queue is 2000, transparent is 3000
        Assert.That(state.RenderQueue, Is.GreaterThan(3000));
    }

    // --- Reset chams property tests ---

    [Test]
    public void ResetChams_RestoresOcclusion()
    {
        var allowOcclusion = ChamsLogic.GetResetAllowOcclusion();
        Assert.That(allowOcclusion, Is.True);
    }

    // --- State transition tests ---

    [Test]
    public void ApplyThenReset_OcclusionCyclesCorrectly()
    {
        // Apply: occlusion disabled
        var applyState = ChamsLogic.GetApplyChamsState();
        Assert.That(applyState.AllowOcclusionWhenDynamic, Is.False);

        // Reset: occlusion restored
        var resetOcclusion = ChamsLogic.GetResetAllowOcclusion();
        Assert.That(resetOcclusion, Is.True);
    }

    [Test]
    public void ApplyChams_AllPropertiesSetTogether()
    {
        var state = ChamsLogic.GetApplyChamsState();
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
        var state = ChamsLogic.GetApplyChamsState(ChamsMode.Solid);
        Assert.That(state.Cull, Is.EqualTo(0));
    }

    [Test]
    public void CullFrontMode_CullIsOne()
    {
        var state = ChamsLogic.GetApplyChamsState(ChamsMode.CullFront);
        Assert.That(state.Cull, Is.EqualTo(1));
    }

    [Test]
    public void OutlineDuplicate_CullIsOne()
    {
        var state = ChamsLogic.GetOutlineDuplicateState();
        Assert.That(state.Cull, Is.EqualTo(1));
    }

    [Test]
    public void OutlineDuplicate_AllAntiOcclusionProperties()
    {
        var state = ChamsLogic.GetOutlineDuplicateState();
        Assert.That(state.ZTest, Is.EqualTo(8), "ZTest must be Always");
        Assert.That(state.ZWrite, Is.EqualTo(0), "ZWrite must be disabled");
        Assert.That(state.RenderQueue, Is.EqualTo(4000), "RenderQueue must be overlay");
        Assert.That(state.ForceRenderingOff, Is.False, "ForceRenderingOff must be false");
        Assert.That(state.AllowOcclusionWhenDynamic, Is.False, "Occlusion must be disabled");
        Assert.That(state.Cull, Is.EqualTo(1), "Outline duplicate must cull front faces");
    }
}
