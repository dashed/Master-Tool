namespace MasterTool.Core;

public static class WeightLogic
{
    /// <summary>
    /// Returns true if the weight method should be blocked (i.e., weight set to zero).
    /// </summary>
    public static bool ShouldBlockWeight(bool noWeightEnabled)
    {
        return noWeightEnabled;
    }

    /// <summary>
    /// Computes the modified weight based on the percentage setting.
    /// Returns null if the feature is disabled (original method should run).
    /// </summary>
    public static float? ComputeWeight(float originalWeight, bool noWeightEnabled, int weightPercent)
    {
        if (!noWeightEnabled)
        {
            return null;
        }

        return originalWeight * (weightPercent / 100f);
    }
}
