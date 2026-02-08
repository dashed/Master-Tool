using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

[TestFixture]
public class ChamsDistanceTests
{
    // --- ShouldApplyPlayerChams ---

    [Test]
    public void PlayerChams_Enabled_NotYourPlayer_Alive_WithinDistance_ReturnsTrue()
    {
        Assert.That(ChamsLogic.ShouldApplyPlayerChams(true, false, true, 50f, 100f), Is.True);
    }

    [Test]
    public void PlayerChams_ExactlyAtMaxDistance_ReturnsTrue()
    {
        Assert.That(ChamsLogic.ShouldApplyPlayerChams(true, false, true, 100f, 100f), Is.True);
    }

    [Test]
    public void PlayerChams_BeyondMaxDistance_ReturnsFalse()
    {
        Assert.That(ChamsLogic.ShouldApplyPlayerChams(true, false, true, 100.1f, 100f), Is.False);
    }

    [Test]
    public void PlayerChams_Disabled_ReturnsFalse()
    {
        Assert.That(ChamsLogic.ShouldApplyPlayerChams(false, false, true, 50f, 100f), Is.False);
    }

    [Test]
    public void PlayerChams_IsYourPlayer_ReturnsFalse()
    {
        Assert.That(ChamsLogic.ShouldApplyPlayerChams(true, true, true, 50f, 100f), Is.False);
    }

    [Test]
    public void PlayerChams_NotAlive_ReturnsFalse()
    {
        Assert.That(ChamsLogic.ShouldApplyPlayerChams(true, false, false, 50f, 100f), Is.False);
    }

    [Test]
    public void PlayerChams_ZeroDistance_ReturnsTrue()
    {
        Assert.That(ChamsLogic.ShouldApplyPlayerChams(true, false, true, 0f, 100f), Is.True);
    }

    // --- ShouldApplyLootChams ---

    [Test]
    public void LootChams_Enabled_WithinDistance_ReturnsTrue()
    {
        // 50m distance: distSq = 2500, max = 100m
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 50f * 50f, 100f), Is.True);
    }

    [Test]
    public void LootChams_ExactlyAtMaxDistance_ReturnsTrue()
    {
        // 100m distance: distSq = 10000, max = 100m, maxDistSq = 10000
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 100f * 100f, 100f), Is.True);
    }

    [Test]
    public void LootChams_BeyondMaxDistance_ReturnsFalse()
    {
        // 150m distance: distSq = 22500, max = 100m, maxDistSq = 10000
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 150f * 150f, 100f), Is.False);
    }

    [Test]
    public void LootChams_Disabled_ReturnsFalse()
    {
        Assert.That(ChamsLogic.ShouldApplyLootChams(false, 50f * 50f, 100f), Is.False);
    }

    [Test]
    public void LootChams_ZeroDistance_ReturnsTrue()
    {
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 0f, 100f), Is.True);
    }

    [Test]
    public void LootChams_VeryLargeDistance_ReturnsFalse()
    {
        // 10000m distance: distSq = 100_000_000, max = 500m
        Assert.That(ChamsLogic.ShouldApplyLootChams(true, 10000f * 10000f, 500f), Is.False);
    }
}
