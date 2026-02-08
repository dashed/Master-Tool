using System.Collections.Generic;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the big head state machine logic used by BigHeadFeature.
/// Duplicates the pure state-tracking logic since Unity bone transforms
/// cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class BigHeadStateTests
{
    /// <summary>
    /// Simulates a player's head bone localScale (as a single float for simplicity,
    /// since BigHeadFeature always sets uniform x=y=z).
    /// </summary>
    private class FakePlayer
    {
        public int Id { get; }
        public float HeadScale { get; set; }
        public bool IsAlive { get; set; }

        public FakePlayer(int id, float headScale = 1f, bool isAlive = true)
        {
            Id = id;
            HeadScale = headScale;
            IsAlive = isAlive;
        }
    }

    /// <summary>Tracks which players the mod scaled (mirrors _modScaledPlayers).</summary>
    private HashSet<int> _modScaledPlayers;

    /// <summary>
    /// Mirrors the state-tracking logic from BigHeadFeature.Apply for a single player.
    /// </summary>
    private void ApplyBigHead(FakePlayer player, bool bigHeadEnabled, float sizeMultiplier)
    {
        if (bigHeadEnabled && player.IsAlive)
        {
            player.HeadScale = sizeMultiplier;
            _modScaledPlayers.Add(player.Id);
        }
        else if (_modScaledPlayers.Contains(player.Id))
        {
            player.HeadScale = 1f;
            _modScaledPlayers.Remove(player.Id);
        }
    }

    [SetUp]
    public void SetUp()
    {
        _modScaledPlayers = new HashSet<int>();
    }

    // --- Core state machine tests ---

    [Test]
    public void ToggleOff_ModNeverScaled_DoesNotTouchState()
    {
        var player = new FakePlayer(1, headScale: 1f);

        ApplyBigHead(player, bigHeadEnabled: false, sizeMultiplier: 3f);

        Assert.That(player.HeadScale, Is.EqualTo(1f), "Should not touch player the mod never scaled");
        Assert.That(_modScaledPlayers, Does.Not.Contain(player.Id));
    }

    [Test]
    public void ToggleOff_NonStandardScale_NotOverridden()
    {
        // Simulate another mod or the game setting a non-standard head scale
        var player = new FakePlayer(1, headScale: 1.5f);

        ApplyBigHead(player, bigHeadEnabled: false, sizeMultiplier: 3f);

        Assert.That(player.HeadScale, Is.EqualTo(1.5f), "Should not override non-standard scale from other sources");
        Assert.That(_modScaledPlayers, Does.Not.Contain(player.Id));
    }

    [Test]
    public void ToggleOn_ScalesHead()
    {
        var player = new FakePlayer(1);

        ApplyBigHead(player, bigHeadEnabled: true, sizeMultiplier: 3f);

        Assert.That(player.HeadScale, Is.EqualTo(3f), "Should scale head to multiplier");
        Assert.That(_modScaledPlayers, Does.Contain(player.Id), "Should track as mod-scaled");
    }

    [Test]
    public void ToggleOnThenOff_ResetsOnlyTracked()
    {
        var player = new FakePlayer(1);

        // Turn on
        ApplyBigHead(player, bigHeadEnabled: true, sizeMultiplier: 3f);
        Assert.That(player.HeadScale, Is.EqualTo(3f));

        // Turn off — should reset
        ApplyBigHead(player, bigHeadEnabled: false, sizeMultiplier: 3f);
        Assert.That(player.HeadScale, Is.EqualTo(1f), "Should reset to 1x on disable");
        Assert.That(_modScaledPlayers, Does.Not.Contain(player.Id), "Should untrack after reset");
    }

    [Test]
    public void ToggleOff_SubsequentFrames_DoNothing()
    {
        var player = new FakePlayer(1);

        // Scale then unscale
        ApplyBigHead(player, bigHeadEnabled: true, sizeMultiplier: 3f);
        ApplyBigHead(player, bigHeadEnabled: false, sizeMultiplier: 3f);
        Assert.That(player.HeadScale, Is.EqualTo(1f));

        // Another mod changes scale after we released control
        player.HeadScale = 2f;

        // More frames with toggle OFF
        ApplyBigHead(player, bigHeadEnabled: false, sizeMultiplier: 3f);
        ApplyBigHead(player, bigHeadEnabled: false, sizeMultiplier: 3f);

        Assert.That(player.HeadScale, Is.EqualTo(2f), "Should not interfere after cleanup");
    }

    [Test]
    public void PlayerDies_WhileScaled_ResetsHead()
    {
        var player = new FakePlayer(1, isAlive: true);

        // Scale head
        ApplyBigHead(player, bigHeadEnabled: true, sizeMultiplier: 3f);
        Assert.That(player.HeadScale, Is.EqualTo(3f));
        Assert.That(_modScaledPlayers, Does.Contain(player.Id));

        // Player dies — BigHeadEnabled is still true but IsAlive is false
        player.IsAlive = false;
        ApplyBigHead(player, bigHeadEnabled: true, sizeMultiplier: 3f);

        Assert.That(player.HeadScale, Is.EqualTo(1f), "Should reset dead player's head");
        Assert.That(_modScaledPlayers, Does.Not.Contain(player.Id));
    }

    [Test]
    public void MultiplePlayers_IndependentTracking()
    {
        var p1 = new FakePlayer(1);
        var p2 = new FakePlayer(2);
        var p3 = new FakePlayer(3);

        // Scale all
        ApplyBigHead(p1, bigHeadEnabled: true, sizeMultiplier: 3f);
        ApplyBigHead(p2, bigHeadEnabled: true, sizeMultiplier: 3f);
        ApplyBigHead(p3, bigHeadEnabled: true, sizeMultiplier: 3f);

        Assert.That(_modScaledPlayers.Count, Is.EqualTo(3));

        // Toggle off — all should reset
        ApplyBigHead(p1, bigHeadEnabled: false, sizeMultiplier: 3f);
        ApplyBigHead(p2, bigHeadEnabled: false, sizeMultiplier: 3f);
        ApplyBigHead(p3, bigHeadEnabled: false, sizeMultiplier: 3f);

        Assert.That(p1.HeadScale, Is.EqualTo(1f));
        Assert.That(p2.HeadScale, Is.EqualTo(1f));
        Assert.That(p3.HeadScale, Is.EqualTo(1f));
        Assert.That(_modScaledPlayers.Count, Is.EqualTo(0));
    }

    [Test]
    public void RapidToggle_OnOffOnOff_HandlesCorrectly()
    {
        var player = new FakePlayer(1);

        // ON
        ApplyBigHead(player, bigHeadEnabled: true, sizeMultiplier: 3f);
        Assert.That(player.HeadScale, Is.EqualTo(3f));

        // OFF
        ApplyBigHead(player, bigHeadEnabled: false, sizeMultiplier: 3f);
        Assert.That(player.HeadScale, Is.EqualTo(1f));

        // ON again
        ApplyBigHead(player, bigHeadEnabled: true, sizeMultiplier: 3f);
        Assert.That(player.HeadScale, Is.EqualTo(3f));

        // OFF again
        ApplyBigHead(player, bigHeadEnabled: false, sizeMultiplier: 3f);
        Assert.That(player.HeadScale, Is.EqualTo(1f));
        Assert.That(_modScaledPlayers.Count, Is.EqualTo(0));
    }

    [Test]
    public void MixedScenario_ScaledAndUnscaledPlayers()
    {
        var untouched = new FakePlayer(1, headScale: 1f);
        var otherMod = new FakePlayer(2, headScale: 1.5f); // Non-standard scale
        var modScaled = new FakePlayer(3);

        // Only scale modScaled
        ApplyBigHead(modScaled, bigHeadEnabled: true, sizeMultiplier: 3f);
        // Others just get toggle-off check
        ApplyBigHead(untouched, bigHeadEnabled: false, sizeMultiplier: 3f);
        ApplyBigHead(otherMod, bigHeadEnabled: false, sizeMultiplier: 3f);

        Assert.That(modScaled.HeadScale, Is.EqualTo(3f));
        Assert.That(untouched.HeadScale, Is.EqualTo(1f), "Untouched player stays at 1x");
        Assert.That(otherMod.HeadScale, Is.EqualTo(1.5f), "Other mod's scale preserved");

        // Now toggle off for all
        ApplyBigHead(modScaled, bigHeadEnabled: false, sizeMultiplier: 3f);
        ApplyBigHead(untouched, bigHeadEnabled: false, sizeMultiplier: 3f);
        ApplyBigHead(otherMod, bigHeadEnabled: false, sizeMultiplier: 3f);

        Assert.That(modScaled.HeadScale, Is.EqualTo(1f), "Mod-scaled player reset");
        Assert.That(untouched.HeadScale, Is.EqualTo(1f), "Untouched stays 1x");
        Assert.That(otherMod.HeadScale, Is.EqualTo(1.5f), "Other mod's scale still preserved");
    }

    [Test]
    public void SizeMultiplierChanges_WhileEnabled()
    {
        var player = new FakePlayer(1);

        ApplyBigHead(player, bigHeadEnabled: true, sizeMultiplier: 3f);
        Assert.That(player.HeadScale, Is.EqualTo(3f));

        // User changes multiplier slider
        ApplyBigHead(player, bigHeadEnabled: true, sizeMultiplier: 5f);
        Assert.That(player.HeadScale, Is.EqualTo(5f), "Should update to new multiplier");
        Assert.That(_modScaledPlayers, Does.Contain(player.Id));
    }

    [Test]
    public void RepeatedToggleOff_NeverInterferes()
    {
        // Many frames with toggle OFF and non-standard scale
        var player = new FakePlayer(1, headScale: 2f);

        for (int i = 0; i < 100; i++)
        {
            ApplyBigHead(player, bigHeadEnabled: false, sizeMultiplier: 3f);
        }

        Assert.That(player.HeadScale, Is.EqualTo(2f), "Should never override non-mod scale");
        Assert.That(_modScaledPlayers.Count, Is.EqualTo(0));
    }
}
