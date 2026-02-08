using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for the LOS layer mask initialization logic.
/// Uses <see cref="EspLogic"/> from MasterTool.Core (shared library).
/// </summary>
[TestFixture]
public class LineOfSightTests
{
    [Test]
    public void ComputeLayerMask_AllLayersFound_CombinesBits()
    {
        // Typical EFT layers: HighPoly=12, LowPoly=11, Terrain=8
        int result = EspLogic.ComputeLayerMask(12, 11, 8);

        Assert.That(result & (1 << 12), Is.Not.Zero, "HighPolyCollider bit should be set");
        Assert.That(result & (1 << 11), Is.Not.Zero, "LowPolyCollider bit should be set");
        Assert.That(result & (1 << 8), Is.Not.Zero, "Terrain bit should be set");
        Assert.That(result, Is.EqualTo((1 << 12) | (1 << 11) | (1 << 8)));
    }

    [Test]
    public void ComputeLayerMask_NoLayersFound_ReturnsFallback()
    {
        int result = EspLogic.ComputeLayerMask(-1, -1, -1);

        Assert.That(result, Is.EqualTo(EspLogic.FallbackMask));
    }

    [Test]
    public void ComputeLayerMask_PartialLayersFound_OnlySetsFoundBits()
    {
        // Only terrain found at layer 8
        int result = EspLogic.ComputeLayerMask(-1, -1, 8);

        Assert.That(result, Is.EqualTo(1 << 8));
        Assert.That(result, Is.Not.EqualTo(EspLogic.FallbackMask));
    }

    [Test]
    public void ComputeLayerMask_SingleLayerFound_DoesNotUseFallback()
    {
        int result = EspLogic.ComputeLayerMask(12, -1, -1);

        Assert.That(result, Is.EqualTo(1 << 12));
    }

    [Test]
    public void FallbackMask_HasExpectedBitsSet()
    {
        // The fallback mask 0x02251800 should have specific bits set
        // corresponding to known EFT collision layers
        Assert.That(EspLogic.FallbackMask, Is.GreaterThan(0));
        Assert.That(EspLogic.FallbackMask & (1 << 11), Is.Not.Zero, "Bit 11 should be set in fallback");
        Assert.That(EspLogic.FallbackMask & (1 << 12), Is.Not.Zero, "Bit 12 should be set in fallback");
    }

    [Test]
    public void ComputeLayerMask_Layer0_SetsCorrectBit()
    {
        // Edge case: layer 0 should still work (1 << 0 = 1)
        int result = EspLogic.ComputeLayerMask(0, -1, -1);

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void ComputeLayerMask_DuplicateLayers_NoDuplication()
    {
        // If two layer names resolve to the same index, OR is idempotent
        int result = EspLogic.ComputeLayerMask(12, 12, 12);

        Assert.That(result, Is.EqualTo(1 << 12));
    }

    [Test]
    public void ComputeLayerMask_MaxLayer31_SetsHighBit()
    {
        // Unity supports layers 0-31
        int result = EspLogic.ComputeLayerMask(31, -1, -1);

        Assert.That(result, Is.EqualTo(1 << 31));
        Assert.That(result, Is.LessThan(0), "Bit 31 makes int negative (sign bit)");
    }
}
