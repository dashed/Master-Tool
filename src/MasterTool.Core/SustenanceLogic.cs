namespace MasterTool.Core;

public static class SustenanceLogic
{
    /// <summary>
    /// Computes the new energy/hydration value.
    /// If enabled and current is below max, returns max. Otherwise returns current.
    /// </summary>
    public static float ComputeNewValue(float current, float maximum, bool featureEnabled)
    {
        if (!featureEnabled)
        {
            return current;
        }

        if (current >= maximum)
        {
            return current;
        }

        return maximum;
    }
}
