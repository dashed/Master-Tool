namespace MasterTool.Core;

/// <summary>
/// Pure chams-related logic extracted from ChamsManager for shared testing.
/// </summary>
public static class ChamsLogic
{
    /// <summary>
    /// Represents the chams material property state set during ApplyChams.
    /// </summary>
    public struct ChamsMaterialState
    {
        public int ZTest;
        public int ZWrite;
        public int RenderQueue;
        public bool ForceRenderingOff;
        public bool AllowOcclusionWhenDynamic;
        public int Cull;
    }

    /// <summary>
    /// Cycles to the next chams rendering mode (Solid -> CullFront -> Outline -> Solid).
    /// </summary>
    public static ChamsMode CycleMode(ChamsMode current)
    {
        return (ChamsMode)(((int)current + 1) % 3);
    }

    /// <summary>
    /// Applies intensity scaling to a color. Clamps intensity to [0.1, 1.0],
    /// multiplies RGB, preserves alpha.
    /// </summary>
    public static Color ApplyIntensity(Color color, float intensity)
    {
        float clamped =
            intensity < 0.1f ? 0.1f
            : intensity > 1f ? 1f
            : intensity;
        return new Color(color.R * clamped, color.G * clamped, color.B * clamped, color.A);
    }

    /// <summary>
    /// Applies intensity and opacity scaling to a color. Clamps both to [0.1, 1.0],
    /// multiplies RGB by intensity, sets alpha to opacity.
    /// </summary>
    public static Color ApplyIntensityAndOpacity(Color color, float intensity, float opacity)
    {
        float clampedIntensity =
            intensity < 0.1f ? 0.1f
            : intensity > 1f ? 1f
            : intensity;
        float clampedOpacity =
            opacity < 0.1f ? 0.1f
            : opacity > 1f ? 1f
            : opacity;
        return new Color(color.R * clampedIntensity, color.G * clampedIntensity, color.B * clampedIntensity, clampedOpacity);
    }

    /// <summary>
    /// Determines whether loot chams should be applied based on distance.
    /// </summary>
    public static bool ShouldApplyLootChams(bool lootChamsEnabled, float distanceSq, float maxDistance)
    {
        float maxDistSq = maxDistance * maxDistance;
        return lootChamsEnabled && distanceSq <= maxDistSq;
    }

    /// <summary>
    /// Returns true when chams transitions from enabled to disabled.
    /// </summary>
    public static bool ShouldResetOnToggle(bool wasEnabled, bool isEnabled)
    {
        return wasEnabled && !isEnabled;
    }

    /// <summary>
    /// Gets the material property state for applying shader chams.
    /// </summary>
    public static ChamsMaterialState GetApplyChamsState(ChamsMode mode = ChamsMode.Solid)
    {
        int cullMode = mode == ChamsMode.CullFront ? 1 : 0;
        return new ChamsMaterialState
        {
            ZTest = 8, // CompareFunction.Always
            ZWrite = 0,
            RenderQueue = 4000,
            ForceRenderingOff = false,
            AllowOcclusionWhenDynamic = false,
            Cull = cullMode,
        };
    }

    /// <summary>
    /// Gets the material state for an outline duplicate (always CullFront).
    /// </summary>
    public static ChamsMaterialState GetOutlineDuplicateState()
    {
        return new ChamsMaterialState
        {
            ZTest = 8,
            ZWrite = 0,
            RenderQueue = 4000,
            ForceRenderingOff = false,
            AllowOcclusionWhenDynamic = false,
            Cull = 1, // Always cull front faces on outline duplicate
        };
    }

    /// <summary>
    /// Gets the reset value for AllowOcclusionWhenDynamic.
    /// </summary>
    public static bool GetResetAllowOcclusion()
    {
        return true;
    }

    /// <summary>
    /// Gets the material property state for loot chams (same structure as player chams).
    /// </summary>
    public static ChamsMaterialState GetLootChamsState()
    {
        return new ChamsMaterialState
        {
            ZTest = 8, // CompareFunction.Always
            ZWrite = 0,
            RenderQueue = 4000,
            ForceRenderingOff = false,
            AllowOcclusionWhenDynamic = false,
            Cull = 0,
        };
    }
}
