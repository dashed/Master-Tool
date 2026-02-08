using MasterTool.Config;
using UnityEngine;

namespace MasterTool.UI
{
    /// <summary>
    /// Lazily initializes and caches GUIStyle instances used by ESP overlays and the status window.
    /// Font sizes are kept in sync with config values each frame via <see cref="EnsureInitialized"/>.
    /// </summary>
    public class GuiStyles
    {
        public GUIStyle EspLabel { get; private set; }
        public GUIStyle ItemEspLabel { get; private set; }
        public GUIStyle StatusBox { get; private set; }

        private bool _espInitialized;
        private bool _itemEspInitialized;
        private bool _statusInitialized;

        /// <summary>
        /// Creates GUIStyle instances on first call and synchronizes font sizes with config on every call.
        /// Must be called at the start of each OnGUI frame.
        /// </summary>
        public void EnsureInitialized()
        {
            if (!_espInitialized)
            {
                EspLabel = new GUIStyle(GUI.skin.label)
                {
                    fontSize = PluginConfig.EspFontSize.Value,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                };
                _espInitialized = true;
            }

            if (!_itemEspInitialized)
            {
                ItemEspLabel = new GUIStyle(EspLabel);
                _itemEspInitialized = true;
            }

            if (!_statusInitialized)
            {
                StatusBox = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 11 };
                StatusBox.normal.textColor = Color.white;
                _statusInitialized = true;
            }

            EspLabel.fontSize = PluginConfig.EspFontSize.Value;
            ItemEspLabel.fontSize = PluginConfig.ItemEspFontSize.Value;
        }
    }
}
