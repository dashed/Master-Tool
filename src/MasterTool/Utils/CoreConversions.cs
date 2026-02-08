using MasterTool.Core;
using UnityEngine;
using CoreColor = MasterTool.Core.Color;
using UnityColor = UnityEngine.Color;

namespace MasterTool.Utils
{
    /// <summary>
    /// Extension methods for converting between MasterTool.Core types and Unity types.
    /// Used when plugin code delegates to Core logic that operates on Core's Vec3/Color.
    /// </summary>
    public static class CoreConversions
    {
        public static Vec3 ToVec3(this Vector3 v)
        {
            return new Vec3(v.x, v.y, v.z);
        }

        public static Vec3? ToVec3Nullable(this Vector3? v)
        {
            return v.HasValue ? new Vec3(v.Value.x, v.Value.y, v.Value.z) : (Vec3?)null;
        }

        public static Vector3 ToVector3(this Vec3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static CoreColor ToCoreColor(this UnityColor c)
        {
            return new CoreColor(c.r, c.g, c.b, c.a);
        }

        public static UnityColor ToUnityColor(this CoreColor c)
        {
            return new UnityColor(c.R, c.G, c.B, c.A);
        }
    }
}
