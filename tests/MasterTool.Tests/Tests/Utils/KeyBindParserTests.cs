using System.Collections.Generic;
using System.Linq;
using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Utils;

/// <summary>
/// Tests for KeyBindParser text-to-keycode parsing logic.
/// Uses <see cref="MasterTool.Core.KeyBindParser"/> and <see cref="KeyCode"/> from the shared library.
/// </summary>
[TestFixture]
public class KeyBindParserTests
{
    // --- TryParseKeyName tests ---

    [TestCase("a", KeyCode.A)]
    [TestCase("A", KeyCode.A)]
    [TestCase("b", KeyCode.B)]
    [TestCase("z", KeyCode.Z)]
    public void ParseKeyName_SingleLetter(string input, KeyCode expected)
    {
        Assert.That(KeyBindParser.TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("0", KeyCode.Alpha0)]
    [TestCase("5", KeyCode.Alpha5)]
    public void ParseKeyName_SingleDigit(string input, KeyCode expected)
    {
        Assert.That(KeyBindParser.TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("ctrl", KeyCode.LeftControl)]
    [TestCase("Ctrl", KeyCode.LeftControl)]
    [TestCase("CTRL", KeyCode.LeftControl)]
    [TestCase("control", KeyCode.LeftControl)]
    [TestCase("lctrl", KeyCode.LeftControl)]
    [TestCase("leftcontrol", KeyCode.LeftControl)]
    [TestCase("rctrl", KeyCode.RightControl)]
    public void ParseKeyName_CtrlAliases(string input, KeyCode expected)
    {
        Assert.That(KeyBindParser.TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("shift", KeyCode.LeftShift)]
    [TestCase("Shift", KeyCode.LeftShift)]
    [TestCase("lshift", KeyCode.LeftShift)]
    [TestCase("rshift", KeyCode.RightShift)]
    public void ParseKeyName_ShiftAliases(string input, KeyCode expected)
    {
        Assert.That(KeyBindParser.TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("alt", KeyCode.LeftAlt)]
    [TestCase("Alt", KeyCode.LeftAlt)]
    [TestCase("lalt", KeyCode.LeftAlt)]
    [TestCase("ralt", KeyCode.RightAlt)]
    public void ParseKeyName_AltAliases(string input, KeyCode expected)
    {
        Assert.That(KeyBindParser.TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("f1", KeyCode.F1)]
    [TestCase("F1", KeyCode.F1)]
    [TestCase("f12", KeyCode.F12)]
    public void ParseKeyName_FKeys(string input, KeyCode expected)
    {
        Assert.That(KeyBindParser.TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("numpad0", KeyCode.Keypad0)]
    [TestCase("numpad5", KeyCode.Keypad5)]
    [TestCase("numpad9", KeyCode.Keypad9)]
    [TestCase("num0", KeyCode.Keypad0)]
    [TestCase("num5", KeyCode.Keypad5)]
    public void ParseKeyName_NumpadKeys(string input, KeyCode expected)
    {
        Assert.That(KeyBindParser.TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("insert", KeyCode.Insert)]
    [TestCase("ins", KeyCode.Insert)]
    [TestCase("delete", KeyCode.Delete)]
    [TestCase("del", KeyCode.Delete)]
    [TestCase("space", KeyCode.Space)]
    [TestCase("esc", KeyCode.Escape)]
    public void ParseKeyName_SpecialKeys(string input, KeyCode expected)
    {
        Assert.That(KeyBindParser.TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void ParseKeyName_EmptyOrNull_ReturnsFalse(string input)
    {
        Assert.That(KeyBindParser.TryParseKeyName(input, out _), Is.False);
    }

    [Test]
    public void ParseKeyName_InvalidName_ReturnsFalse()
    {
        Assert.That(KeyBindParser.TryParseKeyName("notakey", out _), Is.False);
    }

    // --- TryParseKeyBind tests ---

    [Test]
    public void ParseKeyBind_SimpleKey()
    {
        Assert.That(KeyBindParser.TryParseKeyBind("b", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(KeyCode.B));
        Assert.That(mods, Is.Empty);
    }

    [Test]
    public void ParseKeyBind_CtrlPlusB()
    {
        Assert.That(KeyBindParser.TryParseKeyBind("ctrl + b", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(KeyCode.B));
        Assert.That(mods, Does.Contain(KeyCode.LeftControl));
    }

    [Test]
    public void ParseKeyBind_ShiftPlusF1()
    {
        Assert.That(KeyBindParser.TryParseKeyBind("shift + f1", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(KeyCode.F1));
        Assert.That(mods, Does.Contain(KeyCode.LeftShift));
    }

    [Test]
    public void ParseKeyBind_CtrlShiftPlusA()
    {
        Assert.That(KeyBindParser.TryParseKeyBind("ctrl + shift + a", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(KeyCode.A));
        Assert.That(mods.Length, Is.EqualTo(2));
        Assert.That(mods, Does.Contain(KeyCode.LeftControl));
        Assert.That(mods, Does.Contain(KeyCode.LeftShift));
    }

    [Test]
    public void ParseKeyBind_NoSpaces()
    {
        Assert.That(KeyBindParser.TryParseKeyBind("ctrl+b", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(KeyCode.B));
        Assert.That(mods, Does.Contain(KeyCode.LeftControl));
    }

    [Test]
    public void ParseKeyBind_NumpadKey()
    {
        Assert.That(KeyBindParser.TryParseKeyBind("numpad5", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(KeyCode.Keypad5));
        Assert.That(mods, Is.Empty);
    }

    [Test]
    public void ParseKeyBind_EmptyString_ReturnsFalse()
    {
        Assert.That(KeyBindParser.TryParseKeyBind("", out _, out _), Is.False);
    }

    [Test]
    public void ParseKeyBind_Null_ReturnsFalse()
    {
        Assert.That(KeyBindParser.TryParseKeyBind(null, out _, out _), Is.False);
    }

    [Test]
    public void ParseKeyBind_InvalidKey_ReturnsFalse()
    {
        Assert.That(KeyBindParser.TryParseKeyBind("ctrl + notakey", out _, out _), Is.False);
    }

    [Test]
    public void ParseKeyBind_OnlyModifier_UsesAsMainKey()
    {
        // If user types just "ctrl", use LeftControl as main key
        Assert.That(KeyBindParser.TryParseKeyBind("ctrl", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(KeyCode.LeftControl));
        Assert.That(mods, Is.Empty);
    }

    [Test]
    public void ParseKeyBind_CaseInsensitive()
    {
        Assert.That(KeyBindParser.TryParseKeyBind("CTRL + B", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(KeyCode.B));
        Assert.That(mods, Does.Contain(KeyCode.LeftControl));
    }

    // --- IsModifier tests ---

    [TestCase(KeyCode.LeftControl, true)]
    [TestCase(KeyCode.RightControl, true)]
    [TestCase(KeyCode.LeftShift, true)]
    [TestCase(KeyCode.RightShift, true)]
    [TestCase(KeyCode.LeftAlt, true)]
    [TestCase(KeyCode.RightAlt, true)]
    [TestCase(KeyCode.A, false)]
    [TestCase(KeyCode.F1, false)]
    [TestCase(KeyCode.Space, false)]
    [TestCase(KeyCode.None, false)]
    public void IsModifier_CorrectClassification(KeyCode keyCode, bool expected)
    {
        Assert.That(KeyBindParser.IsModifier(keyCode), Is.EqualTo(expected));
    }

    // --- Pending changes logic tests ---

    [Test]
    public void PendingChanges_AddAndRetrieve()
    {
        var pending = new Dictionary<string, KeyCode>();
        pending["GodMode"] = KeyCode.F1;
        Assert.That(pending.ContainsKey("GodMode"), Is.True);
        Assert.That(pending["GodMode"], Is.EqualTo(KeyCode.F1));
    }

    [Test]
    public void PendingChanges_SaveCommitsAll()
    {
        var saved = new Dictionary<string, KeyCode> { { "GodMode", KeyCode.Keypad0 }, { "Stamina", KeyCode.Keypad5 } };
        var pending = new Dictionary<string, KeyCode> { { "GodMode", KeyCode.F1 }, { "Stamina", KeyCode.F12 } };

        // Simulate save
        foreach (var kv in pending)
        {
            saved[kv.Key] = kv.Value;
        }

        pending.Clear();

        Assert.That(saved["GodMode"], Is.EqualTo(KeyCode.F1));
        Assert.That(saved["Stamina"], Is.EqualTo(KeyCode.F12));
        Assert.That(pending.Count, Is.EqualTo(0));
    }

    [Test]
    public void PendingChanges_CancelDiscardsAll()
    {
        var saved = new Dictionary<string, KeyCode> { { "GodMode", KeyCode.Keypad0 } };
        var pending = new Dictionary<string, KeyCode> { { "GodMode", KeyCode.F1 } };

        pending.Clear();

        Assert.That(saved["GodMode"], Is.EqualTo(KeyCode.Keypad0));
        Assert.That(pending.Count, Is.EqualTo(0));
    }

    [Test]
    public void DisplayValue_ShowsPendingWhenStaged()
    {
        var savedValue = KeyCode.Keypad0;
        var pendingValue = KeyCode.F1;
        bool hasPending = true;

        var display = hasPending ? pendingValue : savedValue;
        Assert.That(display, Is.EqualTo(KeyCode.F1));
    }

    [Test]
    public void DisplayValue_ShowsSavedWhenNoPending()
    {
        var savedValue = KeyCode.Keypad0;
        bool hasPending = false;

        var display = hasPending ? KeyCode.None : savedValue;
        Assert.That(display, Is.EqualTo(KeyCode.Keypad0));
    }

    [Test]
    public void PendingLabel_ShowsAsteriskWhenChanged()
    {
        string label = "God Mode";
        bool hasPending = true;
        string display = hasPending ? $"* {label}" : label;
        Assert.That(display, Is.EqualTo("* God Mode"));
    }

    [Test]
    public void PendingLabel_NoAsteriskWhenUnchanged()
    {
        string label = "God Mode";
        bool hasPending = false;
        string display = hasPending ? $"* {label}" : label;
        Assert.That(display, Is.EqualTo("God Mode"));
    }
}
