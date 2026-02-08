using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Utils;

/// <summary>
/// Tests for KeyBindParser text-to-keycode parsing logic.
/// Mirrors the pure parsing logic since Unity KeyCode can't be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class KeyBindParserTests
{
    // Mirror KeyCode values used in tests (from UnityEngine.KeyCode enum)
    private const int None = 0;
    private const int A = 97;
    private const int B = 98;
    private const int Z = 122;
    private const int Alpha0 = 48;
    private const int Alpha5 = 53;
    private const int F1 = 282;
    private const int F12 = 293;
    private const int LeftControl = 306;
    private const int RightControl = 305;
    private const int LeftShift = 304;
    private const int RightShift = 303;
    private const int LeftAlt = 308;
    private const int RightAlt = 307;
    private const int Insert = 277;
    private const int Delete = 127;
    private const int Space = 32;
    private const int Keypad0 = 256;
    private const int Keypad5 = 261;
    private const int Keypad9 = 265;
    private const int Escape = 27;

    private static readonly Dictionary<string, int> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ctrl", LeftControl },
        { "control", LeftControl },
        { "lctrl", LeftControl },
        { "leftctrl", LeftControl },
        { "leftcontrol", LeftControl },
        { "rctrl", RightControl },
        { "rightctrl", RightControl },
        { "rightcontrol", RightControl },
        { "shift", LeftShift },
        { "lshift", LeftShift },
        { "leftshift", LeftShift },
        { "rshift", RightShift },
        { "rightshift", RightShift },
        { "alt", LeftAlt },
        { "lalt", LeftAlt },
        { "leftalt", LeftAlt },
        { "ralt", RightAlt },
        { "rightalt", RightAlt },
        { "insert", Insert },
        { "ins", Insert },
        { "delete", Delete },
        { "del", Delete },
        { "space", Space },
        { "esc", Escape },
        { "escape", Escape },
        { "numpad0", Keypad0 },
        { "numpad5", Keypad5 },
        { "numpad9", Keypad9 },
        { "num0", Keypad0 },
        { "num5", Keypad5 },
        { "num9", Keypad9 },
        { "f1", F1 },
        { "f12", F12 },
    };

    private static readonly HashSet<int> ModifierKeys = new() { LeftControl, RightControl, LeftShift, RightShift, LeftAlt, RightAlt };

    /// <summary>
    /// Mirrors KeyBindParser.TryParseKeyName logic.
    /// </summary>
    private static bool TryParseKeyName(string name, out int keyCode)
    {
        keyCode = None;
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        name = name.Trim();

        if (Aliases.TryGetValue(name, out keyCode))
        {
            return true;
        }

        // Single letter (KeyCode uses lowercase ASCII 97-122)
        if (name.Length == 1 && char.IsLetter(name[0]))
        {
            char lower = char.ToLowerInvariant(name[0]);
            if (lower >= 'a' && lower <= 'z')
            {
                keyCode = lower;
                return true;
            }
        }

        // Single digit
        if (name.Length == 1 && char.IsDigit(name[0]))
        {
            keyCode = Alpha0 + (name[0] - '0');
            return true;
        }

        return false;
    }

    /// <summary>
    /// Mirrors KeyBindParser.TryParseKeyBind logic.
    /// </summary>
    private static bool TryParseKeyBind(string input, out int mainKey, out int[] modifiers)
    {
        mainKey = None;
        modifiers = Array.Empty<int>();

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var parts = input.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
        var parsedKeys = new List<int>();

        foreach (var part in parts)
        {
            if (!TryParseKeyName(part, out var keyCode))
            {
                return false;
            }

            parsedKeys.Add(keyCode);
        }

        if (parsedKeys.Count == 0)
        {
            return false;
        }

        var mods = new List<int>();
        int main = None;

        for (int i = parsedKeys.Count - 1; i >= 0; i--)
        {
            if (main == None && !ModifierKeys.Contains(parsedKeys[i]))
            {
                main = parsedKeys[i];
            }
            else
            {
                mods.Add(parsedKeys[i]);
            }
        }

        if (main == None && mods.Count > 0)
        {
            main = mods[mods.Count - 1];
            mods.RemoveAt(mods.Count - 1);
        }

        mainKey = main;
        modifiers = mods.ToArray();
        return mainKey != None;
    }

    /// <summary>
    /// Mirrors KeyBindParser.IsModifier logic.
    /// </summary>
    private static bool IsModifier(int keyCode)
    {
        return ModifierKeys.Contains(keyCode);
    }

    // --- TryParseKeyName tests ---

    [TestCase("a", A)]
    [TestCase("A", A)]
    [TestCase("b", B)]
    [TestCase("z", Z)]
    public void ParseKeyName_SingleLetter(string input, int expected)
    {
        Assert.That(TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("0", Alpha0)]
    [TestCase("5", Alpha5)]
    public void ParseKeyName_SingleDigit(string input, int expected)
    {
        Assert.That(TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("ctrl", LeftControl)]
    [TestCase("Ctrl", LeftControl)]
    [TestCase("CTRL", LeftControl)]
    [TestCase("control", LeftControl)]
    [TestCase("lctrl", LeftControl)]
    [TestCase("leftcontrol", LeftControl)]
    [TestCase("rctrl", RightControl)]
    public void ParseKeyName_CtrlAliases(string input, int expected)
    {
        Assert.That(TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("shift", LeftShift)]
    [TestCase("Shift", LeftShift)]
    [TestCase("lshift", LeftShift)]
    [TestCase("rshift", RightShift)]
    public void ParseKeyName_ShiftAliases(string input, int expected)
    {
        Assert.That(TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("alt", LeftAlt)]
    [TestCase("Alt", LeftAlt)]
    [TestCase("lalt", LeftAlt)]
    [TestCase("ralt", RightAlt)]
    public void ParseKeyName_AltAliases(string input, int expected)
    {
        Assert.That(TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("f1", F1)]
    [TestCase("F1", F1)]
    [TestCase("f12", F12)]
    public void ParseKeyName_FKeys(string input, int expected)
    {
        Assert.That(TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("numpad0", Keypad0)]
    [TestCase("numpad5", Keypad5)]
    [TestCase("numpad9", Keypad9)]
    [TestCase("num0", Keypad0)]
    [TestCase("num5", Keypad5)]
    public void ParseKeyName_NumpadKeys(string input, int expected)
    {
        Assert.That(TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("insert", Insert)]
    [TestCase("ins", Insert)]
    [TestCase("delete", Delete)]
    [TestCase("del", Delete)]
    [TestCase("space", Space)]
    [TestCase("esc", Escape)]
    public void ParseKeyName_SpecialKeys(string input, int expected)
    {
        Assert.That(TryParseKeyName(input, out var result), Is.True);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void ParseKeyName_EmptyOrNull_ReturnsFalse(string input)
    {
        Assert.That(TryParseKeyName(input, out _), Is.False);
    }

    [Test]
    public void ParseKeyName_InvalidName_ReturnsFalse()
    {
        Assert.That(TryParseKeyName("notakey", out _), Is.False);
    }

    // --- TryParseKeyBind tests ---

    [Test]
    public void ParseKeyBind_SimpleKey()
    {
        Assert.That(TryParseKeyBind("b", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(B));
        Assert.That(mods, Is.Empty);
    }

    [Test]
    public void ParseKeyBind_CtrlPlusB()
    {
        Assert.That(TryParseKeyBind("ctrl + b", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(B));
        Assert.That(mods, Does.Contain(LeftControl));
    }

    [Test]
    public void ParseKeyBind_ShiftPlusF1()
    {
        Assert.That(TryParseKeyBind("shift + f1", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(F1));
        Assert.That(mods, Does.Contain(LeftShift));
    }

    [Test]
    public void ParseKeyBind_CtrlShiftPlusA()
    {
        Assert.That(TryParseKeyBind("ctrl + shift + a", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(A));
        Assert.That(mods.Length, Is.EqualTo(2));
        Assert.That(mods, Does.Contain(LeftControl));
        Assert.That(mods, Does.Contain(LeftShift));
    }

    [Test]
    public void ParseKeyBind_NoSpaces()
    {
        Assert.That(TryParseKeyBind("ctrl+b", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(B));
        Assert.That(mods, Does.Contain(LeftControl));
    }

    [Test]
    public void ParseKeyBind_NumpadKey()
    {
        Assert.That(TryParseKeyBind("numpad5", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(Keypad5));
        Assert.That(mods, Is.Empty);
    }

    [Test]
    public void ParseKeyBind_EmptyString_ReturnsFalse()
    {
        Assert.That(TryParseKeyBind("", out _, out _), Is.False);
    }

    [Test]
    public void ParseKeyBind_Null_ReturnsFalse()
    {
        Assert.That(TryParseKeyBind(null, out _, out _), Is.False);
    }

    [Test]
    public void ParseKeyBind_InvalidKey_ReturnsFalse()
    {
        Assert.That(TryParseKeyBind("ctrl + notakey", out _, out _), Is.False);
    }

    [Test]
    public void ParseKeyBind_OnlyModifier_UsesAsMainKey()
    {
        // If user types just "ctrl", use LeftControl as main key
        Assert.That(TryParseKeyBind("ctrl", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(LeftControl));
        Assert.That(mods, Is.Empty);
    }

    [Test]
    public void ParseKeyBind_CaseInsensitive()
    {
        Assert.That(TryParseKeyBind("CTRL + B", out var main, out var mods), Is.True);
        Assert.That(main, Is.EqualTo(B));
        Assert.That(mods, Does.Contain(LeftControl));
    }

    // --- IsModifier tests ---

    [TestCase(LeftControl, true)]
    [TestCase(RightControl, true)]
    [TestCase(LeftShift, true)]
    [TestCase(RightShift, true)]
    [TestCase(LeftAlt, true)]
    [TestCase(RightAlt, true)]
    [TestCase(A, false)]
    [TestCase(F1, false)]
    [TestCase(Space, false)]
    [TestCase(None, false)]
    public void IsModifier_CorrectClassification(int keyCode, bool expected)
    {
        Assert.That(IsModifier(keyCode), Is.EqualTo(expected));
    }

    // --- Pending changes logic tests ---

    [Test]
    public void PendingChanges_AddAndRetrieve()
    {
        var pending = new Dictionary<string, int>();
        pending["GodMode"] = F1;
        Assert.That(pending.ContainsKey("GodMode"), Is.True);
        Assert.That(pending["GodMode"], Is.EqualTo(F1));
    }

    [Test]
    public void PendingChanges_SaveCommitsAll()
    {
        var saved = new Dictionary<string, int> { { "GodMode", Keypad0 }, { "Stamina", Keypad5 } };
        var pending = new Dictionary<string, int> { { "GodMode", F1 }, { "Stamina", F12 } };

        // Simulate save
        foreach (var kv in pending)
        {
            saved[kv.Key] = kv.Value;
        }

        pending.Clear();

        Assert.That(saved["GodMode"], Is.EqualTo(F1));
        Assert.That(saved["Stamina"], Is.EqualTo(F12));
        Assert.That(pending.Count, Is.EqualTo(0));
    }

    [Test]
    public void PendingChanges_CancelDiscardsAll()
    {
        var saved = new Dictionary<string, int> { { "GodMode", Keypad0 } };
        var pending = new Dictionary<string, int> { { "GodMode", F1 } };

        pending.Clear();

        Assert.That(saved["GodMode"], Is.EqualTo(Keypad0));
        Assert.That(pending.Count, Is.EqualTo(0));
    }

    [Test]
    public void DisplayValue_ShowsPendingWhenStaged()
    {
        int savedValue = Keypad0;
        int pendingValue = F1;
        bool hasPending = true;

        int display = hasPending ? pendingValue : savedValue;
        Assert.That(display, Is.EqualTo(F1));
    }

    [Test]
    public void DisplayValue_ShowsSavedWhenNoPending()
    {
        int savedValue = Keypad0;
        bool hasPending = false;

        int display = hasPending ? 0 : savedValue;
        Assert.That(display, Is.EqualTo(Keypad0));
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
