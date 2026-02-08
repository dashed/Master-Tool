namespace MasterTool.Core;

public static class EspLogic
{
    public const int FallbackMask = 0x02251800;

    /// <summary>
    /// Computes the LOS layer mask from resolved layer indices (-1 if not found).
    /// Falls back to <see cref="FallbackMask"/> when no layers are found.
    /// </summary>
    public static int ComputeLayerMask(int highPoly, int lowPoly, int terrain)
    {
        int mask = 0;
        if (highPoly >= 0)
        {
            mask |= 1 << highPoly;
        }

        if (lowPoly >= 0)
        {
            mask |= 1 << lowPoly;
        }

        if (terrain >= 0)
        {
            mask |= 1 << terrain;
        }

        return mask != 0 ? mask : FallbackMask;
    }

    /// <summary>
    /// Returns the ESP world position for a player: prefers head bone position (+ 0.2 Y offset)
    /// over transform position (+ 1.8 Y offset).
    /// </summary>
    public static Vec3 GetEspWorldPosition(Vec3? headBonePos, Vec3 transformPos)
    {
        if (headBonePos.HasValue)
        {
            return new Vec3(headBonePos.Value.x, headBonePos.Value.y + 0.2f, headBonePos.Value.z);
        }

        return new Vec3(transformPos.x, transformPos.y + 1.8f, transformPos.z);
    }

    /// <summary>
    /// Returns the ESP world position for an item: adds 0.5 Y to lift labels above ground.
    /// </summary>
    public static Vec3 GetItemEspWorldPosition(Vec3 itemPos)
    {
        return new Vec3(itemPos.x, itemPos.y + 0.5f, itemPos.z);
    }
}
