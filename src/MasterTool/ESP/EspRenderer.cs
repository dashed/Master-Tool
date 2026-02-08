using UnityEngine;

namespace MasterTool.ESP
{
    /// <summary>
    /// Provides shared GUI rendering helpers for ESP text overlays.
    /// </summary>
    public static class EspRenderer
    {
        /// <summary>
        /// Draws a text label at the given screen position with a 1-pixel black shadow offset behind it.
        /// </summary>
        /// <param name="pos">Screen-space position (origin top-left).</param>
        /// <param name="text">The label text to draw.</param>
        /// <param name="color">Foreground text color.</param>
        /// <param name="style">The <see cref="GUIStyle"/> controlling font, size, and alignment.</param>
        public static void DrawTextWithShadow(Vector3 pos, string text, Color color, GUIStyle style)
        {
            Vector2 size = style.CalcSize(new GUIContent(text));
            Rect rect = new Rect(pos.x - size.x / 2, pos.y - size.y / 2, size.x, size.y);
            style.normal.textColor = Color.black;
            GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, style);
            style.normal.textColor = color;
            GUI.Label(rect, text, style);
        }

        /// <summary>
        /// Overload that accepts a <see cref="Vector2"/> position, delegating to the Vector3 variant.
        /// </summary>
        /// <param name="pos">Screen-space position as Vector2.</param>
        /// <param name="text">The label text to draw.</param>
        /// <param name="color">Foreground text color.</param>
        /// <param name="style">The <see cref="GUIStyle"/> controlling font, size, and alignment.</param>
        public static void DrawTextWithShadow(Vector2 pos, string text, Color color, GUIStyle style)
        {
            DrawTextWithShadow(new Vector3(pos.x, pos.y, 0), text, color, style);
        }

        /// <summary>
        /// Returns true if the given screen-space coordinates are within screen bounds plus a margin.
        /// Used to filter out ESP targets that would render at extreme off-screen positions.
        /// </summary>
        internal static bool IsOnScreen(float x, float y, float screenWidth, float screenHeight, float margin)
        {
            return x >= -margin && x <= screenWidth + margin && y >= -margin && y <= screenHeight + margin;
        }
    }
}
