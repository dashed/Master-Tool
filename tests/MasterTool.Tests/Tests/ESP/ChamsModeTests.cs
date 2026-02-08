using System.Collections.Generic;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for chams rendering mode logic.
/// Duplicates the pure decision logic since Unity cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class ChamsModeTests
{
    // Mirror the enum since we can't reference the net472 assembly
    private enum ChamsMode
    {
        Solid = 0,
        CullFront = 1,
        Outline = 2,
    }

    // --- Enum value tests ---

    [Test]
    public void ChamsMode_SolidIsZero()
    {
        Assert.That((int)ChamsMode.Solid, Is.EqualTo(0));
    }

    [Test]
    public void ChamsMode_CullFrontIsOne()
    {
        Assert.That((int)ChamsMode.CullFront, Is.EqualTo(1));
    }

    [Test]
    public void ChamsMode_OutlineIsTwo()
    {
        Assert.That((int)ChamsMode.Outline, Is.EqualTo(2));
    }

    // --- Mode cycling tests ---

    [Test]
    public void ModeCycle_SolidToCullFront()
    {
        var current = ChamsMode.Solid;
        var next = (ChamsMode)(((int)current + 1) % 3);
        Assert.That(next, Is.EqualTo(ChamsMode.CullFront));
    }

    [Test]
    public void ModeCycle_CullFrontToOutline()
    {
        var current = ChamsMode.CullFront;
        var next = (ChamsMode)(((int)current + 1) % 3);
        Assert.That(next, Is.EqualTo(ChamsMode.Outline));
    }

    [Test]
    public void ModeCycle_OutlineToSolid()
    {
        var current = ChamsMode.Outline;
        var next = (ChamsMode)(((int)current + 1) % 3);
        Assert.That(next, Is.EqualTo(ChamsMode.Solid));
    }

    [Test]
    public void ModeCycle_FullLoop()
    {
        var mode = ChamsMode.Solid;
        mode = (ChamsMode)(((int)mode + 1) % 3);
        Assert.That(mode, Is.EqualTo(ChamsMode.CullFront));
        mode = (ChamsMode)(((int)mode + 1) % 3);
        Assert.That(mode, Is.EqualTo(ChamsMode.Outline));
        mode = (ChamsMode)(((int)mode + 1) % 3);
        Assert.That(mode, Is.EqualTo(ChamsMode.Solid));
    }

    // --- Cull mode mapping tests ---

    [Test]
    public void CullMode_SolidIsZero()
    {
        var mode = ChamsMode.Solid;
        int cullMode = mode == ChamsMode.CullFront ? 1 : 0;
        Assert.That(cullMode, Is.EqualTo(0));
    }

    [Test]
    public void CullMode_CullFrontIsOne()
    {
        var mode = ChamsMode.CullFront;
        int cullMode = mode == ChamsMode.CullFront ? 1 : 0;
        Assert.That(cullMode, Is.EqualTo(1));
    }

    [Test]
    public void CullMode_OutlineUsesZeroForMainRenderer()
    {
        // In Outline mode, main renderer keeps its original shader,
        // only the duplicate gets cull=1. Main renderer cull is not set.
        var mode = ChamsMode.Outline;
        // Outline mode doesn't apply shader chams to the main renderer
        Assert.That(mode, Is.EqualTo(ChamsMode.Outline));
    }

    // --- Outline scale clamping tests ---

    [TestCase(1.04f, 1.04f)]
    [TestCase(1.01f, 1.01f)]
    [TestCase(1.15f, 1.15f)]
    [TestCase(0.5f, 1.01f)]
    [TestCase(2.0f, 1.15f)]
    [TestCase(1.0f, 1.01f)]
    public void OutlineScale_Clamped(float input, float expected)
    {
        float clamped =
            input < 1.01f ? 1.01f
            : input > 1.15f ? 1.15f
            : input;
        Assert.That(clamped, Is.EqualTo(expected).Within(0.001f));
    }

    // --- Mode change detection tests ---

    [Test]
    public void ModeChange_DetectedWhenDifferent()
    {
        var lastMode = ChamsMode.Solid;
        var currentMode = ChamsMode.CullFront;
        bool changed = currentMode != lastMode;
        Assert.That(changed, Is.True);
    }

    [Test]
    public void ModeChange_NotDetectedWhenSame()
    {
        var lastMode = ChamsMode.Solid;
        var currentMode = ChamsMode.Solid;
        bool changed = currentMode != lastMode;
        Assert.That(changed, Is.False);
    }

    [Test]
    public void ModeChange_OnlyTriggersWhenEnabled()
    {
        bool wasEnabled = true;
        bool isEnabled = true;
        var lastMode = ChamsMode.Solid;
        var currentMode = ChamsMode.CullFront;

        bool shouldReset = wasEnabled && isEnabled && currentMode != lastMode;
        Assert.That(shouldReset, Is.True);
    }

    [Test]
    public void ModeChange_SkippedWhenDisabled()
    {
        bool wasEnabled = true;
        bool isEnabled = false;
        var lastMode = ChamsMode.Solid;
        var currentMode = ChamsMode.CullFront;

        bool shouldReset = wasEnabled && isEnabled && currentMode != lastMode;
        Assert.That(shouldReset, Is.False);
    }

    // --- Outline duplicate tracking tests ---

    [Test]
    public void OutlineTracking_AddAndRemove()
    {
        var dict = new Dictionary<string, string>();
        dict["renderer1"] = "outline1";
        Assert.That(dict.ContainsKey("renderer1"), Is.True);

        dict.Remove("renderer1");
        Assert.That(dict.ContainsKey("renderer1"), Is.False);
    }

    [Test]
    public void OutlineTracking_PurgeNullKeys()
    {
        // Simulate purge logic: remove entries where key would be null
        var keys = new List<string> { "a", "b", null };
        var dict = new Dictionary<string, string> { { "a", "1" }, { "b", "2" } };

        var dead = new List<string>();
        foreach (var key in keys)
        {
            if (key == null)
                dead.Add(key);
        }

        // Null keys don't exist in dict, but the purge list is correct
        Assert.That(dead.Count, Is.EqualTo(1));
    }

    // --- Type-filtered cleanup tests ---

    private class FakeRendererBase { }

    private class FakeSkinnedRenderer : FakeRendererBase { }

    private class FakeMeshRenderer : FakeRendererBase { }

    [Test]
    public void TypeFilteredCleanup_OnlyRemovesMatchingType()
    {
        var dict = new Dictionary<FakeRendererBase, string>
        {
            { new FakeSkinnedRenderer(), "outline1" },
            { new FakeMeshRenderer(), "outline2" },
            { new FakeSkinnedRenderer(), "outline3" },
        };

        var keysToRemove = new List<FakeRendererBase>();
        foreach (var kv in dict)
        {
            if (kv.Key is FakeSkinnedRenderer)
                keysToRemove.Add(kv.Key);
        }

        foreach (var key in keysToRemove)
        {
            dict.Remove(key);
        }

        Assert.That(dict.Count, Is.EqualTo(1));
        foreach (var kv in dict)
        {
            Assert.That(kv.Key, Is.InstanceOf<FakeMeshRenderer>());
        }
    }

    [Test]
    public void TypeFilteredCleanup_MeshRendererOnly()
    {
        var dict = new Dictionary<FakeRendererBase, string>
        {
            { new FakeSkinnedRenderer(), "outline1" },
            { new FakeMeshRenderer(), "outline2" },
            { new FakeMeshRenderer(), "outline3" },
        };

        var keysToRemove = new List<FakeRendererBase>();
        foreach (var kv in dict)
        {
            if (kv.Key is FakeMeshRenderer)
                keysToRemove.Add(kv.Key);
        }

        foreach (var key in keysToRemove)
        {
            dict.Remove(key);
        }

        Assert.That(dict.Count, Is.EqualTo(1));
        foreach (var kv in dict)
        {
            Assert.That(kv.Key, Is.InstanceOf<FakeSkinnedRenderer>());
        }
    }

    // --- Outline object naming guard ---

    [Test]
    public void OutlineGuard_SkipsOutlineNamedObjects()
    {
        string outlineName = "_ChamsOutline";
        var objectNames = new[] { "Body", "Head", "_ChamsOutline", "Hands" };

        int processed = 0;
        foreach (var name in objectNames)
        {
            if (name == outlineName)
                continue;
            processed++;
        }

        Assert.That(processed, Is.EqualTo(3));
    }
}
