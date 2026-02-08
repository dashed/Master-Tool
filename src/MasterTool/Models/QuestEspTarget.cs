using UnityEngine;

namespace MasterTool.Models
{
    /// <summary>
    /// Data model representing a quest-related ESP target (item or zone) projected to screen space.
    /// </summary>
    public class QuestEspTarget
    {
        public Vector2 ScreenPosition;
        public float Distance;
        public string Name;
        public Color Color;
        public bool IsZone;
    }
}
