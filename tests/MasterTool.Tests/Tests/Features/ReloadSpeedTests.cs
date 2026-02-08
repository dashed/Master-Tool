using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the reload speed state machine logic.
/// Uses <see cref="ReloadDefaults"/> from MasterTool.Core (shared library).
/// State-tracking tests remain local as they test integration behavior.
/// </summary>
[TestFixture]
public class ReloadSpeedTests
{
    /// <summary>
    /// Simulates whether the mod has forced custom reload values.
    /// </summary>
    private bool _modForced;

    /// <summary>
    /// Simulates the weapon's load time.
    /// </summary>
    private float _loadTime;

    /// <summary>
    /// Simulates the weapon's unload time.
    /// </summary>
    private float _unloadTime;

    /// <summary>
    /// Mirrors the state-tracking logic from ReloadSpeedFeature.Apply.
    /// </summary>
    private void Apply(bool enabled, float configLoad, float configUnload)
    {
        if (enabled)
        {
            _loadTime = configLoad;
            _unloadTime = configUnload;
            _modForced = true;
        }
        else if (_modForced)
        {
            _loadTime = ReloadDefaults.DefaultLoadTime;
            _unloadTime = ReloadDefaults.DefaultUnloadTime;
            _modForced = false;
        }
    }

    [SetUp]
    public void SetUp()
    {
        _modForced = false;
        _loadTime = ReloadDefaults.DefaultLoadTime;
        _unloadTime = ReloadDefaults.DefaultUnloadTime;
    }

    [Test]
    public void Enable_SetsModForced()
    {
        Apply(true, 0.5f, 0.1f);
        Assert.That(_modForced, Is.True);
    }

    [Test]
    public void Enable_AppliesCustomValues()
    {
        Apply(true, 0.5f, 0.1f);
        Assert.That(_loadTime, Is.EqualTo(0.5f));
        Assert.That(_unloadTime, Is.EqualTo(0.1f));
    }

    [Test]
    public void Disable_AfterForced_RestoresDefaults()
    {
        Apply(true, 0.5f, 0.1f);
        Apply(false, 0.5f, 0.1f);
        Assert.That(_loadTime, Is.EqualTo(ReloadDefaults.DefaultLoadTime));
        Assert.That(_unloadTime, Is.EqualTo(ReloadDefaults.DefaultUnloadTime));
        Assert.That(_modForced, Is.False);
    }

    [Test]
    public void Disable_WhenNotForced_DoesNothing()
    {
        _loadTime = 0.99f; // some non-default
        Apply(false, 0.5f, 0.1f);
        Assert.That(_loadTime, Is.EqualTo(0.99f), "Should not interfere with game state");
        Assert.That(_modForced, Is.False);
    }

    [Test]
    public void MultipleCycles_WorkCorrectly()
    {
        Apply(true, 0.3f, 0.05f);
        Assert.That(_modForced, Is.True);
        Apply(false, 0.3f, 0.05f);
        Assert.That(_modForced, Is.False);
        Apply(true, 0.1f, 0.02f);
        Assert.That(_loadTime, Is.EqualTo(0.1f));
        Assert.That(_unloadTime, Is.EqualTo(0.02f));
    }

    [Test]
    public void DefaultValues_AreCorrect()
    {
        Assert.That(ReloadDefaults.DefaultLoadTime, Is.EqualTo(0.85f));
        Assert.That(ReloadDefaults.DefaultUnloadTime, Is.EqualTo(0.3f));
    }

    [Test]
    public void Enable_WithDefaultValues_SetsModForced()
    {
        Apply(true, ReloadDefaults.DefaultLoadTime, ReloadDefaults.DefaultUnloadTime);
        Assert.That(_modForced, Is.True);
        Assert.That(_loadTime, Is.EqualTo(ReloadDefaults.DefaultLoadTime));
    }

    [Test]
    public void Enable_MinValues_AppliesCorrectly()
    {
        Apply(true, 0.01f, 0.01f);
        Assert.That(_loadTime, Is.EqualTo(0.01f));
        Assert.That(_unloadTime, Is.EqualTo(0.01f));
    }

    [Test]
    public void Enable_MaxValues_AppliesCorrectly()
    {
        Apply(true, 2f, 2f);
        Assert.That(_loadTime, Is.EqualTo(2f));
        Assert.That(_unloadTime, Is.EqualTo(2f));
    }
}
