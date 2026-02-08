using UnityEngine;

namespace MasterTool.Models
{
    /// <summary>
    /// Data model representing an item or container ESP target projected to screen space.
    /// </summary>
    public class ItemEspTarget
    {
        public Vector2 ScreenPosition;
        public float Distance;
        public string Name;
        public Color Color;
    }
}
