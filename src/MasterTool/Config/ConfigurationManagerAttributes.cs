using System;
using BepInEx.Configuration;

namespace MasterTool.Config
{
    /// <summary>
    /// Convention-based attributes class for BepInEx ConfigurationManager.
    /// ConfigurationManager discovers this by class name via reflection —
    /// no assembly reference needed.
    /// </summary>
    internal sealed class ConfigurationManagerAttributes
    {
        /// <summary>Custom GUI drawer replacing the default value editor.</summary>
        public Action<ConfigEntryBase> CustomDrawer;

        /// <summary>Hide the "Reset" button next to this entry.</summary>
        public bool? HideDefaultButton;

        /// <summary>Display order within the section (higher = shown first).</summary>
        public int? Order;
    }
}
