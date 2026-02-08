using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the NoWeight prefix decision logic.
/// Duplicates the pure prefix logic since Harmony patches
/// cannot be exercised from net9.0 tests.
/// </summary>
[TestFixture]
public class NoWeightPrefixTests
{
    /// <summary>
    /// Mirrors the WeightPrefix logic from NoWeightFeature.
    /// Returns true if the original method should run, false to skip it.
    /// Sets result to 0 when the feature is enabled.
    /// </summary>
    private static bool WeightPrefix(bool noWeightEnabled, ref float result)
    {
        if (noWeightEnabled)
        {
            result = 0f;
            return false;
        }

        return true;
    }

    [Test]
    public void Enabled_SetsResultToZero()
    {
        float result = 42f;

        bool runOriginal = WeightPrefix(noWeightEnabled: true, ref result);

        Assert.That(result, Is.EqualTo(0f), "Should set weight to zero");
        Assert.That(runOriginal, Is.False, "Should skip original method");
    }

    [Test]
    public void Disabled_LeavesResultUnchanged()
    {
        float result = 42f;

        bool runOriginal = WeightPrefix(noWeightEnabled: false, ref result);

        Assert.That(result, Is.EqualTo(42f), "Should not modify result");
        Assert.That(runOriginal, Is.True, "Should let original method run");
    }

    [Test]
    public void Enabled_AlreadyZero_StaysZero()
    {
        float result = 0f;

        bool runOriginal = WeightPrefix(noWeightEnabled: true, ref result);

        Assert.That(result, Is.EqualTo(0f));
        Assert.That(runOriginal, Is.False);
    }

    [Test]
    public void RapidToggle_EnableDisableEnable()
    {
        float result = 50f;

        // Enable — zero it
        WeightPrefix(noWeightEnabled: true, ref result);
        Assert.That(result, Is.EqualTo(0f));

        // Disable — pass through (result stays whatever it was)
        result = 50f;
        bool runOriginal = WeightPrefix(noWeightEnabled: false, ref result);
        Assert.That(result, Is.EqualTo(50f));
        Assert.That(runOriginal, Is.True);

        // Enable again — zero it
        WeightPrefix(noWeightEnabled: true, ref result);
        Assert.That(result, Is.EqualTo(0f));
    }
}
