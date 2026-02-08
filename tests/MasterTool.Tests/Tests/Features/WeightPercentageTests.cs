using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the enhanced NoWeight feature with percentage-based weight reduction.
/// Duplicates the pure prefix logic since Harmony patches cannot be
/// referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class WeightPercentageTests
{
    /// <summary>
    /// Mirrors the WeightPrefix logic from NoWeightFeature.
    /// Returns the modified weight result. Returns null if original method should run.
    /// </summary>
    private static float? ComputeWeight(float originalWeight, bool noWeightEnabled, int weightPercent)
    {
        if (!noWeightEnabled)
        {
            return null; // original method runs
        }

        return originalWeight * (weightPercent / 100f);
    }

    [Test]
    public void Disabled_ReturnsNull_OriginalRuns()
    {
        Assert.That(ComputeWeight(50f, false, 0), Is.Null);
    }

    [Test]
    public void Enabled_Percent0_Weightless()
    {
        Assert.That(ComputeWeight(50f, true, 0), Is.EqualTo(0f));
    }

    [Test]
    public void Enabled_Percent50_HalfWeight()
    {
        Assert.That(ComputeWeight(100f, true, 50), Is.EqualTo(50f));
    }

    [Test]
    public void Enabled_Percent100_FullWeight()
    {
        Assert.That(ComputeWeight(100f, true, 100), Is.EqualTo(100f));
    }

    [Test]
    public void Enabled_Percent25_QuarterWeight()
    {
        Assert.That(ComputeWeight(80f, true, 25), Is.EqualTo(20f));
    }

    [Test]
    public void Enabled_ZeroOriginalWeight_StaysZero()
    {
        Assert.That(ComputeWeight(0f, true, 50), Is.EqualTo(0f));
    }

    [Test]
    public void Disabled_AnyPercent_OriginalRuns()
    {
        Assert.That(ComputeWeight(50f, false, 50), Is.Null);
    }
}
