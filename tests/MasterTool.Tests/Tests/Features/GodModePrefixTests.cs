using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the GodMode prefix decision logic.
/// Uses <see cref="DamageLogic"/> from MasterTool.Core (shared library).
/// </summary>
[TestFixture]
public class GodModePrefixTests
{
    // --- ShouldBlock tests ---

    [Test]
    public void ShouldBlock_GodModeOn_LocalPlayer_ReturnsTrue()
    {
        Assert.That(DamageLogic.ShouldBlockForPlayer(true, true), Is.True);
    }

    [Test]
    public void ShouldBlock_GodModeOff_LocalPlayer_ReturnsFalse()
    {
        Assert.That(DamageLogic.ShouldBlockForPlayer(false, true), Is.False);
    }

    [Test]
    public void ShouldBlock_GodModeOn_NonLocalPlayer_ReturnsFalse()
    {
        Assert.That(DamageLogic.ShouldBlockForPlayer(true, false), Is.False);
    }

    [Test]
    public void ShouldBlock_GodModeOff_NonLocalPlayer_ReturnsFalse()
    {
        Assert.That(DamageLogic.ShouldBlockForPlayer(false, false), Is.False);
    }

    [Test]
    public void ShouldBlock_GodModeOn_NullPlayer_ReturnsFalse()
    {
        Assert.That(DamageLogic.ShouldBlockForPlayer(true, null), Is.False);
    }

    [Test]
    public void ShouldBlock_GodModeOff_NullPlayer_ReturnsFalse()
    {
        Assert.That(DamageLogic.ShouldBlockForPlayer(false, null), Is.False);
    }

    // --- ApplyDamage modification tests ---

    [Test]
    public void ApplyDamage_GodModeOn_LocalPlayer_ZeroesDamage()
    {
        Assert.That(DamageLogic.ApplyDamageModification(50f, true, true), Is.EqualTo(0f));
    }

    [Test]
    public void ApplyDamage_GodModeOff_LocalPlayer_KeepsDamage()
    {
        Assert.That(DamageLogic.ApplyDamageModification(50f, false, true), Is.EqualTo(50f));
    }

    [Test]
    public void ApplyDamage_GodModeOn_NonLocalPlayer_KeepsDamage()
    {
        Assert.That(DamageLogic.ApplyDamageModification(50f, true, false), Is.EqualTo(50f));
    }

    [Test]
    public void ApplyDamage_GodModeOn_NullPlayer_KeepsDamage()
    {
        Assert.That(DamageLogic.ApplyDamageModification(50f, true, null), Is.EqualTo(50f));
    }

    [Test]
    public void ApplyDamage_ZeroDamage_Unchanged()
    {
        Assert.That(DamageLogic.ApplyDamageModification(0f, false, true), Is.EqualTo(0f));
    }
}
