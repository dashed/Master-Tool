using System.Collections.Generic;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the culling state machine logic used by CullingFeature.
/// Duplicates the pure state-tracking logic since Unity GameObjects
/// cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class CullingStateTests
{
    /// <summary>
    /// Simulates a bot's GameObject.activeSelf property.
    /// </summary>
    private class FakeBot
    {
        public int Id { get; }
        public bool ActiveSelf { get; private set; }

        public FakeBot(int id, bool active = true)
        {
            Id = id;
            ActiveSelf = active;
        }

        public void SetActive(bool value)
        {
            ActiveSelf = value;
        }
    }

    /// <summary>Tracks which bots the mod deactivated (mirrors _modDeactivatedBots).</summary>
    private HashSet<int> _modDeactivatedBots;

    /// <summary>
    /// Mirrors the state-tracking logic from CullingFeature.Apply for a single bot.
    /// </summary>
    private void ApplyCulling(FakeBot bot, bool performanceModeOn, bool inRange)
    {
        if (performanceModeOn)
        {
            if (!inRange && bot.ActiveSelf)
            {
                bot.SetActive(false);
                _modDeactivatedBots.Add(bot.Id);
            }
            else if (inRange && !bot.ActiveSelf && _modDeactivatedBots.Contains(bot.Id))
            {
                bot.SetActive(true);
                _modDeactivatedBots.Remove(bot.Id);
            }
        }
        else if (_modDeactivatedBots.Contains(bot.Id))
        {
            bot.SetActive(true);
            _modDeactivatedBots.Remove(bot.Id);
        }
    }

    [SetUp]
    public void SetUp()
    {
        _modDeactivatedBots = new HashSet<int>();
    }

    // --- Core state machine tests ---

    [Test]
    public void PerformanceOff_GameDeactivatedBot_NotTouched()
    {
        // Game deactivated this bot (e.g., dead, despawned, LOD)
        var bot = new FakeBot(1, active: false);

        ApplyCulling(bot, performanceModeOn: false, inRange: true);

        Assert.That(bot.ActiveSelf, Is.False, "Should not re-enable a bot the mod didn't deactivate");
        Assert.That(_modDeactivatedBots, Does.Not.Contain(bot.Id));
    }

    [Test]
    public void PerformanceOn_BotOutOfRange_Deactivated()
    {
        var bot = new FakeBot(1, active: true);

        ApplyCulling(bot, performanceModeOn: true, inRange: false);

        Assert.That(bot.ActiveSelf, Is.False, "Should deactivate bot beyond range");
        Assert.That(_modDeactivatedBots, Does.Contain(bot.Id), "Should track as mod-deactivated");
    }

    [Test]
    public void PerformanceOn_BotInRange_StaysActive()
    {
        var bot = new FakeBot(1, active: true);

        ApplyCulling(bot, performanceModeOn: true, inRange: true);

        Assert.That(bot.ActiveSelf, Is.True, "Should stay active when in range");
        Assert.That(_modDeactivatedBots, Does.Not.Contain(bot.Id));
    }

    [Test]
    public void PerformanceOn_BotReturnsToRange_Reactivated()
    {
        var bot = new FakeBot(1, active: true);

        // Bot goes out of range
        ApplyCulling(bot, performanceModeOn: true, inRange: false);
        Assert.That(bot.ActiveSelf, Is.False);
        Assert.That(_modDeactivatedBots, Does.Contain(bot.Id));

        // Bot comes back in range
        ApplyCulling(bot, performanceModeOn: true, inRange: true);
        Assert.That(bot.ActiveSelf, Is.True, "Should re-enable bot that returns to range");
        Assert.That(_modDeactivatedBots, Does.Not.Contain(bot.Id), "Should untrack after re-enabling");
    }

    [Test]
    public void PerformanceOnThenOff_ModDeactivatedBot_Reenabled()
    {
        var bot = new FakeBot(1, active: true);

        // Mod deactivates bot (out of range)
        ApplyCulling(bot, performanceModeOn: true, inRange: false);
        Assert.That(bot.ActiveSelf, Is.False);

        // Toggle off — should re-enable bot the mod deactivated
        ApplyCulling(bot, performanceModeOn: false, inRange: true);
        Assert.That(bot.ActiveSelf, Is.True, "Should re-enable mod-deactivated bot on toggle off");
        Assert.That(_modDeactivatedBots, Does.Not.Contain(bot.Id));
    }

    [Test]
    public void PerformanceOff_SubsequentFrames_DoNothing()
    {
        var bot = new FakeBot(1, active: true);

        // Mod deactivates, then toggle off (cleanup)
        ApplyCulling(bot, performanceModeOn: true, inRange: false);
        ApplyCulling(bot, performanceModeOn: false, inRange: true);
        Assert.That(bot.ActiveSelf, Is.True);

        // Game deactivates bot after mod released control
        bot.SetActive(false);

        // More frames with performance mode OFF
        ApplyCulling(bot, performanceModeOn: false, inRange: true);
        ApplyCulling(bot, performanceModeOn: false, inRange: true);

        Assert.That(bot.ActiveSelf, Is.False, "Should not interfere with game state after cleanup");
    }

    [Test]
    public void PerformanceOn_GameDeactivatedBot_NotReactivated()
    {
        // Game deactivated this bot before mod was involved
        var bot = new FakeBot(1, active: false);

        // Performance mode on, bot would be in range
        ApplyCulling(bot, performanceModeOn: true, inRange: true);

        Assert.That(bot.ActiveSelf, Is.False, "Should not re-enable a bot the game deactivated");
        Assert.That(_modDeactivatedBots, Does.Not.Contain(bot.Id));
    }

    [Test]
    public void PerformanceOn_AlreadyDeactivatedBot_OutOfRange_NotDoubleTracked()
    {
        // Bot already inactive (e.g., game deactivated it)
        var bot = new FakeBot(1, active: false);

        ApplyCulling(bot, performanceModeOn: true, inRange: false);

        // Bot is already inactive, so mod should NOT track it (it didn't deactivate it)
        Assert.That(bot.ActiveSelf, Is.False);
        Assert.That(_modDeactivatedBots, Does.Not.Contain(bot.Id), "Should not track a bot that was already inactive");
    }

    [Test]
    public void MultipleBots_IndependentTracking()
    {
        var bot1 = new FakeBot(1, active: true);
        var bot2 = new FakeBot(2, active: true);
        var bot3 = new FakeBot(3, active: true);

        // Bot1 out of range, bot2 in range, bot3 out of range
        ApplyCulling(bot1, performanceModeOn: true, inRange: false);
        ApplyCulling(bot2, performanceModeOn: true, inRange: true);
        ApplyCulling(bot3, performanceModeOn: true, inRange: false);

        Assert.That(bot1.ActiveSelf, Is.False);
        Assert.That(bot2.ActiveSelf, Is.True);
        Assert.That(bot3.ActiveSelf, Is.False);
        Assert.That(_modDeactivatedBots.Count, Is.EqualTo(2));

        // Toggle off — only bot1 and bot3 should be re-enabled
        ApplyCulling(bot1, performanceModeOn: false, inRange: true);
        ApplyCulling(bot2, performanceModeOn: false, inRange: true);
        ApplyCulling(bot3, performanceModeOn: false, inRange: true);

        Assert.That(bot1.ActiveSelf, Is.True);
        Assert.That(bot2.ActiveSelf, Is.True);
        Assert.That(bot3.ActiveSelf, Is.True);
        Assert.That(_modDeactivatedBots.Count, Is.EqualTo(0));
    }

    [Test]
    public void RapidToggle_OnOffOnOff_HandlesCorrectly()
    {
        var bot = new FakeBot(1, active: true);

        // ON — out of range
        ApplyCulling(bot, performanceModeOn: true, inRange: false);
        Assert.That(bot.ActiveSelf, Is.False);

        // OFF — cleanup
        ApplyCulling(bot, performanceModeOn: false, inRange: true);
        Assert.That(bot.ActiveSelf, Is.True);

        // ON again — out of range
        ApplyCulling(bot, performanceModeOn: true, inRange: false);
        Assert.That(bot.ActiveSelf, Is.False);

        // OFF again — cleanup
        ApplyCulling(bot, performanceModeOn: false, inRange: true);
        Assert.That(bot.ActiveSelf, Is.True);
        Assert.That(_modDeactivatedBots.Count, Is.EqualTo(0));
    }

    [Test]
    public void PerformanceOn_BotOscillatesInAndOutOfRange()
    {
        var bot = new FakeBot(1, active: true);

        // Out of range
        ApplyCulling(bot, performanceModeOn: true, inRange: false);
        Assert.That(bot.ActiveSelf, Is.False);

        // Back in range
        ApplyCulling(bot, performanceModeOn: true, inRange: true);
        Assert.That(bot.ActiveSelf, Is.True);

        // Out again
        ApplyCulling(bot, performanceModeOn: true, inRange: false);
        Assert.That(bot.ActiveSelf, Is.False);

        // Back in
        ApplyCulling(bot, performanceModeOn: true, inRange: true);
        Assert.That(bot.ActiveSelf, Is.True);
        Assert.That(_modDeactivatedBots, Does.Not.Contain(bot.Id));
    }

    [Test]
    public void MixedScenario_GameAndModDeactivations()
    {
        var gamBot = new FakeBot(1, active: false); // Game deactivated
        var modBot = new FakeBot(2, active: true); // Will be mod-deactivated

        // Mod deactivates modBot, gamBot already inactive
        ApplyCulling(gamBot, performanceModeOn: true, inRange: false);
        ApplyCulling(modBot, performanceModeOn: true, inRange: false);

        Assert.That(gamBot.ActiveSelf, Is.False);
        Assert.That(modBot.ActiveSelf, Is.False);
        Assert.That(_modDeactivatedBots, Does.Not.Contain(gamBot.Id), "Game-deactivated bot should not be tracked");
        Assert.That(_modDeactivatedBots, Does.Contain(modBot.Id), "Mod-deactivated bot should be tracked");

        // Toggle off — only modBot should be re-enabled
        ApplyCulling(gamBot, performanceModeOn: false, inRange: true);
        ApplyCulling(modBot, performanceModeOn: false, inRange: true);

        Assert.That(gamBot.ActiveSelf, Is.False, "Game-deactivated bot must stay inactive");
        Assert.That(modBot.ActiveSelf, Is.True, "Mod-deactivated bot should be re-enabled");
    }
}
