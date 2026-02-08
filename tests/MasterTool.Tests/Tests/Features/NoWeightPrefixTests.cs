using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the NoWeight prefix decision logic.
/// Uses <see cref="WeightLogic"/> from MasterTool.Core.
/// </summary>
[TestFixture]
public class NoWeightPrefixTests
{
    [Test]
    public void Enabled_SetsResultToZero()
    {
        float result = 42f;

        bool shouldBlock = WeightLogic.ShouldBlockWeight(noWeightEnabled: true);
        bool runOriginal = !shouldBlock;
        if (shouldBlock)
        {
            result = 0f;
        }

        Assert.That(result, Is.EqualTo(0f), "Should set weight to zero");
        Assert.That(runOriginal, Is.False, "Should skip original method");
    }

    [Test]
    public void Disabled_LeavesResultUnchanged()
    {
        float result = 42f;

        bool shouldBlock = WeightLogic.ShouldBlockWeight(noWeightEnabled: false);
        bool runOriginal = !shouldBlock;
        if (shouldBlock)
        {
            result = 0f;
        }

        Assert.That(result, Is.EqualTo(42f), "Should not modify result");
        Assert.That(runOriginal, Is.True, "Should let original method run");
    }

    [Test]
    public void Enabled_AlreadyZero_StaysZero()
    {
        float result = 0f;

        bool shouldBlock = WeightLogic.ShouldBlockWeight(noWeightEnabled: true);
        bool runOriginal = !shouldBlock;
        if (shouldBlock)
        {
            result = 0f;
        }

        Assert.That(result, Is.EqualTo(0f));
        Assert.That(runOriginal, Is.False);
    }

    [Test]
    public void RapidToggle_EnableDisableEnable()
    {
        float result = 50f;

        // Enable — zero it
        if (WeightLogic.ShouldBlockWeight(noWeightEnabled: true))
        {
            result = 0f;
        }
        Assert.That(result, Is.EqualTo(0f));

        // Disable — pass through (result stays whatever it was)
        result = 50f;
        bool shouldBlock = WeightLogic.ShouldBlockWeight(noWeightEnabled: false);
        bool runOriginal = !shouldBlock;
        Assert.That(result, Is.EqualTo(50f));
        Assert.That(runOriginal, Is.True);

        // Enable again — zero it
        if (WeightLogic.ShouldBlockWeight(noWeightEnabled: true))
        {
            result = 0f;
        }
        Assert.That(result, Is.EqualTo(0f));
    }
}
