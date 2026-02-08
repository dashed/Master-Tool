using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the fall damage state machine logic used by FallDamageFeature.
/// Uses constants from <see cref="FallDamageDefaults"/> in MasterTool.Core.
/// Duplicates the state-tracking logic since ActiveHealthController
/// cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class FallDamageStateTests
{
    /// <summary>
    /// Simulates ActiveHealthController.FallSafeHeight.
    /// </summary>
    private float _fallSafeHeight;

    /// <summary>
    /// Simulates the _modForced flag.
    /// </summary>
    private bool _modForced;

    /// <summary>
    /// Mirrors the state-tracking logic from FallDamageFeature.Apply.
    /// </summary>
    private void ApplyFallDamage(bool enabled)
    {
        if (enabled)
        {
            _fallSafeHeight = FallDamageDefaults.SafeHeight;
            _modForced = true;
        }
        else if (_modForced)
        {
            _fallSafeHeight = FallDamageDefaults.DefaultHeight;
            _modForced = false;
        }
    }

    [SetUp]
    public void SetUp()
    {
        _fallSafeHeight = FallDamageDefaults.DefaultHeight;
        _modForced = false;
    }

    [Test]
    public void Enabled_SetsSafeHeight()
    {
        ApplyFallDamage(true);
        Assert.That(_fallSafeHeight, Is.EqualTo(FallDamageDefaults.SafeHeight));
        Assert.That(_modForced, Is.True);
    }

    [Test]
    public void Disabled_NotForced_DoesNotTouch()
    {
        _fallSafeHeight = 5f; // Some custom game value
        ApplyFallDamage(false);
        Assert.That(_fallSafeHeight, Is.EqualTo(5f), "Should not interfere with game state");
        Assert.That(_modForced, Is.False);
    }

    [Test]
    public void EnabledThenDisabled_Resets()
    {
        ApplyFallDamage(true);
        Assert.That(_fallSafeHeight, Is.EqualTo(FallDamageDefaults.SafeHeight));

        ApplyFallDamage(false);
        Assert.That(_fallSafeHeight, Is.EqualTo(FallDamageDefaults.DefaultHeight));
        Assert.That(_modForced, Is.False);
    }

    [Test]
    public void Disabled_AfterReset_DoesNotInterfere()
    {
        ApplyFallDamage(true);
        ApplyFallDamage(false);

        _fallSafeHeight = 3f; // Game changes it
        ApplyFallDamage(false);
        Assert.That(_fallSafeHeight, Is.EqualTo(3f), "Should not interfere after cleanup");
    }

    [Test]
    public void MultipleFramesEnabled_KeepsSafeHeight()
    {
        for (int i = 0; i < 10; i++)
        {
            ApplyFallDamage(true);
        }

        Assert.That(_fallSafeHeight, Is.EqualTo(FallDamageDefaults.SafeHeight));
        Assert.That(_modForced, Is.True);
    }

    [Test]
    public void RapidToggle_HandlesCorrectly()
    {
        ApplyFallDamage(true);
        Assert.That(_fallSafeHeight, Is.EqualTo(FallDamageDefaults.SafeHeight));

        ApplyFallDamage(false);
        Assert.That(_fallSafeHeight, Is.EqualTo(FallDamageDefaults.DefaultHeight));

        ApplyFallDamage(true);
        Assert.That(_fallSafeHeight, Is.EqualTo(FallDamageDefaults.SafeHeight));

        ApplyFallDamage(false);
        Assert.That(_fallSafeHeight, Is.EqualTo(FallDamageDefaults.DefaultHeight));
    }

    [Test]
    public void Disabled_DefaultState_NoChange()
    {
        ApplyFallDamage(false);
        Assert.That(_fallSafeHeight, Is.EqualTo(FallDamageDefaults.DefaultHeight));
        Assert.That(_modForced, Is.False);
    }

    [Test]
    public void Enabled_GameChangesHeight_ModOverrides()
    {
        ApplyFallDamage(true);
        _fallSafeHeight = 2f; // Game changes it

        ApplyFallDamage(true);
        Assert.That(_fallSafeHeight, Is.EqualTo(FallDamageDefaults.SafeHeight), "Mod should re-override");
    }

    [Test]
    public void MultipleDisabledFrames_NeverInterferes()
    {
        _fallSafeHeight = 5f;
        for (int i = 0; i < 100; i++)
        {
            ApplyFallDamage(false);
        }

        Assert.That(_fallSafeHeight, Is.EqualTo(5f));
    }
}
