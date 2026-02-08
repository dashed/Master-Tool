using UnityEngine;

namespace MasterTool.UI
{
    /// <summary>
    /// Provides an inline IMGUI color picker with separate R, G, B sliders.
    /// </summary>
    public static class ColorPicker
    {
        /// <summary>
        /// Draws a labeled inline color picker with red, green, and blue sliders.
        /// Alpha is always set to 1.
        /// </summary>
        /// <param name="label">The display label shown above the sliders.</param>
        /// <param name="color">The current color value to display and edit.</param>
        /// <returns>The updated color after user interaction.</returns>
        public static Color Draw(string label, Color color)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"<b>{label}</b>");
            GUILayout.BeginHorizontal();

            GUILayout.Label("R", GUILayout.Width(15));
            float r = GUILayout.HorizontalSlider(color.r, 0f, 1f, GUILayout.Width(60));

            GUILayout.Label("G", GUILayout.Width(15));
            float g = GUILayout.HorizontalSlider(color.g, 0f, 1f, GUILayout.Width(60));

            GUILayout.Label("B", GUILayout.Width(15));
            float b = GUILayout.HorizontalSlider(color.b, 0f, 1f, GUILayout.Width(60));

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            return new Color(r, g, b, 1f);
        }
    }
}
