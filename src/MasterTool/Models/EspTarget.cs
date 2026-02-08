using UnityEngine;

namespace MasterTool.Models
{
    /// <summary>
    /// Data model representing a player ESP target projected to screen space.
    /// </summary>
    public class EspTarget
    {
        public Vector2 ScreenPosition;
        public float Distance;
        public string Nickname;
        public string Side;
        public Color Color;
    }
}
