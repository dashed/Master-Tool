using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the energy and hydration feature decision logic.
/// Duplicates the pure value-clamping logic since ActiveHealthController
/// cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class EnergyHydrationTests
{
    /// <summary>
    /// Mirrors the decision in EnergyFeature/HydrationFeature:
    /// Returns the new value to set. If enabled and current &lt; max, returns max.
    /// Otherwise returns current (no change).
    /// </summary>
    private static float ComputeNewValue(float current, float maximum, bool featureEnabled)
    {
        if (!featureEnabled)
        {
            return current;
        }

        if (current >= maximum)
        {
            return current;
        }

        return maximum;
    }

    // --- Energy tests ---

    [Test]
    public void Energy_Enabled_BelowMax_SetsToMax()
    {
        Assert.That(ComputeNewValue(50f, 100f, true), Is.EqualTo(100f));
    }

    [Test]
    public void Energy_Enabled_AtMax_NoChange()
    {
        Assert.That(ComputeNewValue(100f, 100f, true), Is.EqualTo(100f));
    }

    [Test]
    public void Energy_Disabled_BelowMax_NoChange()
    {
        Assert.That(ComputeNewValue(50f, 100f, false), Is.EqualTo(50f));
    }

    [Test]
    public void Energy_Disabled_AtMax_NoChange()
    {
        Assert.That(ComputeNewValue(100f, 100f, false), Is.EqualTo(100f));
    }

    [Test]
    public void Energy_Enabled_AtZero_SetsToMax()
    {
        Assert.That(ComputeNewValue(0f, 100f, true), Is.EqualTo(100f));
    }

    [Test]
    public void Energy_Enabled_NearMax_SetsToMax()
    {
        Assert.That(ComputeNewValue(99.5f, 100f, true), Is.EqualTo(100f));
    }

    // --- Hydration tests (same logic, different context) ---

    [Test]
    public void Hydration_Enabled_BelowMax_SetsToMax()
    {
        Assert.That(ComputeNewValue(30f, 100f, true), Is.EqualTo(100f));
    }

    [Test]
    public void Hydration_Disabled_BelowMax_NoChange()
    {
        Assert.That(ComputeNewValue(30f, 100f, false), Is.EqualTo(30f));
    }

    // --- Edge cases ---

    [Test]
    public void ZeroMaximum_Enabled_ReturnsZero()
    {
        Assert.That(ComputeNewValue(0f, 0f, true), Is.EqualTo(0f));
    }

    [Test]
    public void NegativeCurrent_Enabled_SetsToMax()
    {
        Assert.That(ComputeNewValue(-5f, 100f, true), Is.EqualTo(100f));
    }
}
