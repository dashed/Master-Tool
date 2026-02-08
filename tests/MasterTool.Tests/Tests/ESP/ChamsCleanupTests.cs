using System.Collections.Generic;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for the ChamsManager dictionary cleanup logic.
/// Duplicates the purge pattern since Unity Renderer objects
/// cannot be instantiated from net9.0 tests.
/// Uses a FakeRenderer with an IsDestroyed flag to simulate
/// Unity's destroyed-object behavior (where obj == null is true).
/// </summary>
[TestFixture]
public class ChamsCleanupTests
{
    /// <summary>
    /// Simulates a Unity Renderer that can be "destroyed".
    /// In real Unity, destroyed objects compare equal to null via
    /// a custom == operator. Here we use an explicit flag.
    /// </summary>
    private class FakeRenderer
    {
        public int Id { get; }
        public bool IsDestroyed { get; set; }

        public FakeRenderer(int id)
        {
            Id = id;
        }
    }

    private Dictionary<FakeRenderer, string> _entries;

    /// <summary>
    /// Mirrors PurgeDestroyedEntries() from ChamsManager.
    /// In real code, the check is <c>kv.Key == null</c> (Unity's custom null).
    /// Here we check <c>kv.Key.IsDestroyed</c> as the equivalent.
    /// </summary>
    private void PurgeDestroyedEntries()
    {
        var dead = new List<FakeRenderer>();
        foreach (var kv in _entries)
        {
            if (kv.Key.IsDestroyed)
            {
                dead.Add(kv.Key);
            }
        }

        foreach (var r in dead)
        {
            _entries.Remove(r);
        }
    }

    [SetUp]
    public void SetUp()
    {
        _entries = new Dictionary<FakeRenderer, string>();
    }

    [Test]
    public void EmptyDictionary_CleanupDoesNothing()
    {
        PurgeDestroyedEntries();

        Assert.That(_entries.Count, Is.EqualTo(0));
    }

    [Test]
    public void AllAlive_NothingRemoved()
    {
        var r1 = new FakeRenderer(1);
        var r2 = new FakeRenderer(2);
        var r3 = new FakeRenderer(3);
        _entries[r1] = "shaderA";
        _entries[r2] = "shaderB";
        _entries[r3] = "shaderC";

        PurgeDestroyedEntries();

        Assert.That(_entries.Count, Is.EqualTo(3));
        Assert.That(_entries.ContainsKey(r1), Is.True);
        Assert.That(_entries.ContainsKey(r2), Is.True);
        Assert.That(_entries.ContainsKey(r3), Is.True);
    }

    [Test]
    public void AllDestroyed_AllRemoved()
    {
        var r1 = new FakeRenderer(1) { IsDestroyed = true };
        var r2 = new FakeRenderer(2) { IsDestroyed = true };
        _entries[r1] = "shaderA";
        _entries[r2] = "shaderB";

        PurgeDestroyedEntries();

        Assert.That(_entries.Count, Is.EqualTo(0));
    }

    [Test]
    public void MixedEntries_OnlyDestroyedRemoved()
    {
        var alive1 = new FakeRenderer(1);
        var dead1 = new FakeRenderer(2) { IsDestroyed = true };
        var alive2 = new FakeRenderer(3);
        var dead2 = new FakeRenderer(4) { IsDestroyed = true };
        _entries[alive1] = "shaderA";
        _entries[dead1] = "shaderB";
        _entries[alive2] = "shaderC";
        _entries[dead2] = "shaderD";

        PurgeDestroyedEntries();

        Assert.That(_entries.Count, Is.EqualTo(2));
        Assert.That(_entries.ContainsKey(alive1), Is.True);
        Assert.That(_entries.ContainsKey(alive2), Is.True);
        Assert.That(_entries[alive1], Is.EqualTo("shaderA"));
        Assert.That(_entries[alive2], Is.EqualTo("shaderC"));
    }

    [Test]
    public void MultipleCleanups_Idempotent()
    {
        var alive = new FakeRenderer(1);
        var dead = new FakeRenderer(2) { IsDestroyed = true };
        _entries[alive] = "shaderA";
        _entries[dead] = "shaderB";

        PurgeDestroyedEntries();
        Assert.That(_entries.Count, Is.EqualTo(1));

        // Second cleanup — nothing to remove
        PurgeDestroyedEntries();
        Assert.That(_entries.Count, Is.EqualTo(1));
        Assert.That(_entries.ContainsKey(alive), Is.True);
    }

    [Test]
    public void EntriesAddedBetweenCleanups_NewDeadPurged()
    {
        var r1 = new FakeRenderer(1);
        _entries[r1] = "shaderA";

        // First cleanup — nothing dead
        PurgeDestroyedEntries();
        Assert.That(_entries.Count, Is.EqualTo(1));

        // Player despawns
        r1.IsDestroyed = true;

        // New player added
        var r2 = new FakeRenderer(2);
        _entries[r2] = "shaderB";

        // Second cleanup — removes dead r1, keeps alive r2
        PurgeDestroyedEntries();
        Assert.That(_entries.Count, Is.EqualTo(1));
        Assert.That(_entries.ContainsKey(r2), Is.True);
    }

    [Test]
    public void RendererDestroyedAfterAdd_PurgedOnNextCleanup()
    {
        var r1 = new FakeRenderer(1);
        var r2 = new FakeRenderer(2);
        _entries[r1] = "shaderA";
        _entries[r2] = "shaderB";

        // Both alive — cleanup does nothing
        PurgeDestroyedEntries();
        Assert.That(_entries.Count, Is.EqualTo(2));

        // r1 destroyed mid-raid
        r1.IsDestroyed = true;

        // Cleanup removes only r1
        PurgeDestroyedEntries();
        Assert.That(_entries.Count, Is.EqualTo(1));
        Assert.That(_entries.ContainsKey(r2), Is.True);
        Assert.That(_entries[r2], Is.EqualTo("shaderB"));
    }

    [Test]
    public void ManyDestroyedEntries_AllCleaned()
    {
        // Simulate a long raid with many despawned players
        var alive = new FakeRenderer(0);
        _entries[alive] = "active";

        for (int i = 1; i <= 50; i++)
        {
            _entries[new FakeRenderer(i) { IsDestroyed = true }] = $"stale_{i}";
        }

        Assert.That(_entries.Count, Is.EqualTo(51));

        PurgeDestroyedEntries();

        Assert.That(_entries.Count, Is.EqualTo(1));
        Assert.That(_entries.ContainsKey(alive), Is.True);
    }
}
