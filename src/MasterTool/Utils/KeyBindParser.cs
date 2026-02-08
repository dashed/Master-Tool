using System;
using System.Linq;
using UnityEngine;
using CoreKeyCode = MasterTool.Core.KeyCode;
using CoreParser = MasterTool.Core.KeyBindParser;

namespace MasterTool.Utils
{
    /// <summary>
    /// Thin wrapper around <see cref="MasterTool.Core.KeyBindParser"/> that converts
    /// between <see cref="MasterTool.Core.KeyCode"/> and <see cref="UnityEngine.KeyCode"/>.
    /// All parsing logic lives in MasterTool.Core.
    /// </summary>
    public static class KeyBindParser
    {
        internal static bool TryParseKeyName(string name, out KeyCode keyCode)
        {
            bool result = CoreParser.TryParseKeyName(name, out CoreKeyCode coreKey);
            keyCode = (KeyCode)(int)coreKey;
            return result;
        }

        internal static bool TryParseKeyBind(string input, out KeyCode mainKey, out KeyCode[] modifiers)
        {
            bool result = CoreParser.TryParseKeyBind(input, out CoreKeyCode coreMain, out CoreKeyCode[] coreMods);
            mainKey = (KeyCode)(int)coreMain;
            modifiers = coreMods.Select(m => (KeyCode)(int)m).ToArray();
            return result;
        }

        internal static string FormatKeyBind(KeyCode mainKey, KeyCode[] modifiers)
        {
            return CoreParser.FormatKeyBind((CoreKeyCode)(int)mainKey, modifiers.Select(m => (CoreKeyCode)(int)m).ToArray());
        }

        internal static bool IsModifier(KeyCode keyCode)
        {
            return CoreParser.IsModifier((CoreKeyCode)(int)keyCode);
        }
    }
}
