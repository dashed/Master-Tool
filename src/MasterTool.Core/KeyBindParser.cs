using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterTool.Core;

/// <summary>
/// Parses user-friendly key bind strings (e.g., "ctrl + b", "shift + f1") into KeyCode values.
/// Supports common aliases for modifier and special keys, case-insensitive.
/// </summary>
public static class KeyBindParser
{
    public static readonly Dictionary<string, KeyCode> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ctrl", KeyCode.LeftControl },
        { "control", KeyCode.LeftControl },
        { "lctrl", KeyCode.LeftControl },
        { "leftctrl", KeyCode.LeftControl },
        { "leftcontrol", KeyCode.LeftControl },
        { "rctrl", KeyCode.RightControl },
        { "rightctrl", KeyCode.RightControl },
        { "rightcontrol", KeyCode.RightControl },
        { "shift", KeyCode.LeftShift },
        { "lshift", KeyCode.LeftShift },
        { "leftshift", KeyCode.LeftShift },
        { "rshift", KeyCode.RightShift },
        { "rightshift", KeyCode.RightShift },
        { "alt", KeyCode.LeftAlt },
        { "lalt", KeyCode.LeftAlt },
        { "leftalt", KeyCode.LeftAlt },
        { "ralt", KeyCode.RightAlt },
        { "rightalt", KeyCode.RightAlt },
        { "insert", KeyCode.Insert },
        { "ins", KeyCode.Insert },
        { "delete", KeyCode.Delete },
        { "del", KeyCode.Delete },
        { "home", KeyCode.Home },
        { "end", KeyCode.End },
        { "pageup", KeyCode.PageUp },
        { "pgup", KeyCode.PageUp },
        { "pagedown", KeyCode.PageDown },
        { "pgdn", KeyCode.PageDown },
        { "space", KeyCode.Space },
        { "spacebar", KeyCode.Space },
        { "enter", KeyCode.Return },
        { "return", KeyCode.Return },
        { "esc", KeyCode.Escape },
        { "escape", KeyCode.Escape },
        { "tab", KeyCode.Tab },
        { "backspace", KeyCode.Backspace },
        { "capslock", KeyCode.CapsLock },
        { "numlock", KeyCode.Numlock },
        { "scrolllock", KeyCode.ScrollLock },
        { "printscreen", KeyCode.Print },
        { "print", KeyCode.Print },
        { "pause", KeyCode.Pause },
        { "up", KeyCode.UpArrow },
        { "down", KeyCode.DownArrow },
        { "left", KeyCode.LeftArrow },
        { "right", KeyCode.RightArrow },
        // Numpad
        { "numpad0", KeyCode.Keypad0 },
        { "numpad1", KeyCode.Keypad1 },
        { "numpad2", KeyCode.Keypad2 },
        { "numpad3", KeyCode.Keypad3 },
        { "numpad4", KeyCode.Keypad4 },
        { "numpad5", KeyCode.Keypad5 },
        { "numpad6", KeyCode.Keypad6 },
        { "numpad7", KeyCode.Keypad7 },
        { "numpad8", KeyCode.Keypad8 },
        { "numpad9", KeyCode.Keypad9 },
        { "num0", KeyCode.Keypad0 },
        { "num1", KeyCode.Keypad1 },
        { "num2", KeyCode.Keypad2 },
        { "num3", KeyCode.Keypad3 },
        { "num4", KeyCode.Keypad4 },
        { "num5", KeyCode.Keypad5 },
        { "num6", KeyCode.Keypad6 },
        { "num7", KeyCode.Keypad7 },
        { "num8", KeyCode.Keypad8 },
        { "num9", KeyCode.Keypad9 },
        { "numpadplus", KeyCode.KeypadPlus },
        { "numpadminus", KeyCode.KeypadMinus },
        { "numpadmultiply", KeyCode.KeypadMultiply },
        { "numpaddivide", KeyCode.KeypadDivide },
        { "numpadenter", KeyCode.KeypadEnter },
        { "numpadperiod", KeyCode.KeypadPeriod },
        // F-keys
        { "f1", KeyCode.F1 },
        { "f2", KeyCode.F2 },
        { "f3", KeyCode.F3 },
        { "f4", KeyCode.F4 },
        { "f5", KeyCode.F5 },
        { "f6", KeyCode.F6 },
        { "f7", KeyCode.F7 },
        { "f8", KeyCode.F8 },
        { "f9", KeyCode.F9 },
        { "f10", KeyCode.F10 },
        { "f11", KeyCode.F11 },
        { "f12", KeyCode.F12 },
        { "f13", KeyCode.F13 },
        { "f14", KeyCode.F14 },
        { "f15", KeyCode.F15 },
        // Mouse buttons
        { "mouse0", KeyCode.Mouse0 },
        { "mouse1", KeyCode.Mouse1 },
        { "mouse2", KeyCode.Mouse2 },
        { "mouse3", KeyCode.Mouse3 },
        { "mouse4", KeyCode.Mouse4 },
        { "mouse5", KeyCode.Mouse5 },
        { "mouse6", KeyCode.Mouse6 },
    };

    private static readonly HashSet<KeyCode> ModifierKeys = new()
    {
        KeyCode.LeftControl,
        KeyCode.RightControl,
        KeyCode.LeftShift,
        KeyCode.RightShift,
        KeyCode.LeftAlt,
        KeyCode.RightAlt,
    };

    /// <summary>
    /// Tries to parse a single key name (e.g., "ctrl", "f1", "a") into a KeyCode.
    /// Checks aliases first, then tries KeyCode enum parse, then single letter/digit.
    /// </summary>
    public static bool TryParseKeyName(string name, out KeyCode keyCode)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            keyCode = KeyCode.None;
            return false;
        }

        name = name.Trim();

        // Check aliases
        if (Aliases.TryGetValue(name, out keyCode))
        {
            return true;
        }

        // Single letter -> KeyCode.A through KeyCode.Z (lowercase ASCII 97-122)
        if (name.Length == 1 && char.IsLetter(name[0]))
        {
            char lower = char.ToLowerInvariant(name[0]);
            if (lower >= 'a' && lower <= 'z')
            {
                keyCode = (KeyCode)lower;
                return true;
            }
        }

        // Single digit -> KeyCode.Alpha0 through KeyCode.Alpha9
        if (name.Length == 1 && char.IsDigit(name[0]))
        {
            keyCode = (KeyCode)(KeyCode.Alpha0 + (name[0] - '0'));
            return true;
        }

        keyCode = KeyCode.None;
        return false;
    }

    /// <summary>
    /// Parses a key bind string like "ctrl + b" or "shift + f1" into a main key and modifiers.
    /// Returns true if parsing succeeded with at least a main key.
    /// </summary>
    public static bool TryParseKeyBind(string input, out KeyCode mainKey, out KeyCode[] modifiers)
    {
        mainKey = KeyCode.None;
        modifiers = Array.Empty<KeyCode>();

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var parts = input.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
        var parsedKeys = new List<KeyCode>();

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

        // Separate modifiers from the main key.
        // The main key is the last non-modifier, or the last key if all are modifiers.
        var mods = new List<KeyCode>();
        KeyCode main = KeyCode.None;

        for (int i = parsedKeys.Count - 1; i >= 0; i--)
        {
            if (main == KeyCode.None && !ModifierKeys.Contains(parsedKeys[i]))
            {
                main = parsedKeys[i];
            }
            else
            {
                mods.Add(parsedKeys[i]);
            }
        }

        // If all keys are modifiers, use the last one as main
        if (main == KeyCode.None && mods.Count > 0)
        {
            main = mods[mods.Count - 1];
            mods.RemoveAt(mods.Count - 1);
        }

        mainKey = main;
        modifiers = mods.ToArray();
        return mainKey != KeyCode.None;
    }

    /// <summary>
    /// Formats a KeyCode main key and modifiers into a display string like "Ctrl + B".
    /// </summary>
    public static string FormatKeyBind(KeyCode mainKey, KeyCode[] modifiers)
    {
        if (mainKey == KeyCode.None)
        {
            return "Not set";
        }

        if (modifiers == null || modifiers.Length == 0)
        {
            return FormatKeyName(mainKey);
        }

        var parts = modifiers.Select(FormatKeyName).Concat(new[] { FormatKeyName(mainKey) });
        return string.Join(" + ", parts);
    }

    /// <summary>
    /// Returns true if the given KeyCode is a modifier key (Ctrl, Shift, Alt).
    /// </summary>
    public static bool IsModifier(KeyCode keyCode)
    {
        return ModifierKeys.Contains(keyCode);
    }

    public static string FormatKeyName(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.LeftControl:
                return "Ctrl";
            case KeyCode.RightControl:
                return "RCtrl";
            case KeyCode.LeftShift:
                return "Shift";
            case KeyCode.RightShift:
                return "RShift";
            case KeyCode.LeftAlt:
                return "Alt";
            case KeyCode.RightAlt:
                return "RAlt";
            case KeyCode.Keypad0:
                return "Numpad 0";
            case KeyCode.Keypad1:
                return "Numpad 1";
            case KeyCode.Keypad2:
                return "Numpad 2";
            case KeyCode.Keypad3:
                return "Numpad 3";
            case KeyCode.Keypad4:
                return "Numpad 4";
            case KeyCode.Keypad5:
                return "Numpad 5";
            case KeyCode.Keypad6:
                return "Numpad 6";
            case KeyCode.Keypad7:
                return "Numpad 7";
            case KeyCode.Keypad8:
                return "Numpad 8";
            case KeyCode.Keypad9:
                return "Numpad 9";
            default:
                return keyCode.ToString();
        }
    }
}
