using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the enhanced NoWeight feature with percentage-based weight reduction.
/// Uses <see cref="WeightLogic"/> from MasterTool.Core.
/// </summary>
[TestFixture]
public class WeightPercentageTests
{
    [Test]
    public void Disabled_ReturnsNull_OriginalRuns()
    {
        Assert.That(WeightLogic.ComputeWeight(50f, false, 0), Is.Null);
    }

    [Test]
    public void Enabled_Percent0_Weightless()
    {
        Assert.That(WeightLogic.ComputeWeight(50f, true, 0), Is.EqualTo(0f));
    }

    [Test]
    public void Enabled_Percent50_HalfWeight()
    {
        Assert.That(WeightLogic.ComputeWeight(100f, true, 50), Is.EqualTo(50f));
    }

    [Test]
    public void Enabled_Percent100_FullWeight()
    {
        Assert.That(WeightLogic.ComputeWeight(100f, true, 100), Is.EqualTo(100f));
    }

    [Test]
    public void Enabled_Percent25_QuarterWeight()
    {
        Assert.That(WeightLogic.ComputeWeight(80f, true, 25), Is.EqualTo(20f));
    }

    [Test]
    public void Enabled_ZeroOriginalWeight_StaysZero()
    {
        Assert.That(WeightLogic.ComputeWeight(0f, true, 50), Is.EqualTo(0f));
    }

    [Test]
    public void Disabled_AnyPercent_OriginalRuns()
    {
        Assert.That(WeightLogic.ComputeWeight(50f, false, 50), Is.Null);
    }
}
