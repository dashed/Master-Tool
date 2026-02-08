using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for <see cref="PeacefulLogic.ShouldBlockEnemy"/>.
/// Verifies that enemy registration is only blocked when peaceful mode is enabled
/// AND the target is the local player — bot-vs-bot AI must remain unaffected.
/// </summary>
[TestFixture]
public class PeacefulModeTests
{
    [TestCase(false, false, false, Description = "Peaceful off, not local player - don't block")]
    [TestCase(false, true, false, Description = "Peaceful off, local player - don't block")]
    [TestCase(true, false, false, Description = "Peaceful on, not local player - don't block")]
    [TestCase(true, true, true, Description = "Peaceful on, local player - block")]
    public void ShouldBlockEnemy_ReturnsExpected(bool peacefulEnabled, bool isLocalPlayer, bool expected)
    {
        Assert.That(PeacefulLogic.ShouldBlockEnemy(peacefulEnabled, isLocalPlayer), Is.EqualTo(expected));
    }

    [Test]
    public void PeacefulMode_OnlyBlocksLocalPlayer()
    {
        Assert.That(PeacefulLogic.ShouldBlockEnemy(true, true), Is.True);
        Assert.That(PeacefulLogic.ShouldBlockEnemy(true, false), Is.False);
    }

    [Test]
    public void PeacefulMode_Disabled_NeverBlocks()
    {
        Assert.That(PeacefulLogic.ShouldBlockEnemy(false, true), Is.False);
        Assert.That(PeacefulLogic.ShouldBlockEnemy(false, false), Is.False);
    }
}
