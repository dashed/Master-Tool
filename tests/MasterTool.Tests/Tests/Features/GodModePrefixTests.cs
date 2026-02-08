using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the GodMode prefix decision logic.
/// Duplicates the pure blocking decision since Harmony patch methods
/// and EFT types cannot be used from net9.0 tests.
/// </summary>
[TestFixture]
public class GodModePrefixTests
{
    /// <summary>
    /// Mirrors the shared GodMode prefix logic from DamagePatches.
    /// Used by Kill, DestroyBodyPart, DoFracture, and DoBleed prefixes.
    /// Returns true if the original method should be BLOCKED (skipped).
    /// </summary>
    private static bool ShouldBlockForPlayer(bool godModeEnabled, bool? isYourPlayer)
    {
        if (isYourPlayer == null) // null player
        {
            return false;
        }

        if (!isYourPlayer.Value) // not local player
        {
            return false;
        }

        return godModeEnabled;
    }

    /// <summary>
    /// Mirrors the ApplyDamage prefix logic that zeroes damage.
    /// Returns the damage value after modification.
    /// </summary>
    private static float ApplyDamageModification(float originalDamage, bool godModeEnabled, bool? isYourPlayer)
    {
        if (isYourPlayer == null || !isYourPlayer.Value)
        {
            return originalDamage;
        }

        if (godModeEnabled)
        {
            return 0f;
        }

        return originalDamage;
    }

    // --- ShouldBlock tests ---

    [Test]
    public void ShouldBlock_GodModeOn_LocalPlayer_ReturnsTrue()
    {
        Assert.That(ShouldBlockForPlayer(true, true), Is.True);
    }

    [Test]
    public void ShouldBlock_GodModeOff_LocalPlayer_ReturnsFalse()
    {
        Assert.That(ShouldBlockForPlayer(false, true), Is.False);
    }

    [Test]
    public void ShouldBlock_GodModeOn_NonLocalPlayer_ReturnsFalse()
    {
        Assert.That(ShouldBlockForPlayer(true, false), Is.False);
    }

    [Test]
    public void ShouldBlock_GodModeOff_NonLocalPlayer_ReturnsFalse()
    {
        Assert.That(ShouldBlockForPlayer(false, false), Is.False);
    }

    [Test]
    public void ShouldBlock_GodModeOn_NullPlayer_ReturnsFalse()
    {
        Assert.That(ShouldBlockForPlayer(true, null), Is.False);
    }

    [Test]
    public void ShouldBlock_GodModeOff_NullPlayer_ReturnsFalse()
    {
        Assert.That(ShouldBlockForPlayer(false, null), Is.False);
    }

    // --- ApplyDamage modification tests ---

    [Test]
    public void ApplyDamage_GodModeOn_LocalPlayer_ZeroesDamage()
    {
        Assert.That(ApplyDamageModification(50f, true, true), Is.EqualTo(0f));
    }

    [Test]
    public void ApplyDamage_GodModeOff_LocalPlayer_KeepsDamage()
    {
        Assert.That(ApplyDamageModification(50f, false, true), Is.EqualTo(50f));
    }

    [Test]
    public void ApplyDamage_GodModeOn_NonLocalPlayer_KeepsDamage()
    {
        Assert.That(ApplyDamageModification(50f, true, false), Is.EqualTo(50f));
    }

    [Test]
    public void ApplyDamage_GodModeOn_NullPlayer_KeepsDamage()
    {
        Assert.That(ApplyDamageModification(50f, true, null), Is.EqualTo(50f));
    }

    [Test]
    public void ApplyDamage_ZeroDamage_Unchanged()
    {
        Assert.That(ApplyDamageModification(0f, false, true), Is.EqualTo(0f));
    }
}
