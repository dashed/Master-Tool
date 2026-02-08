using System;

namespace MasterTool.Core;

/// <summary>
/// Pure healing logic extracted from CodModeFeature for shared testing.
/// </summary>
public static class HealingLogic
{
    public static bool ShouldHeal(float timeSinceHit, float healDelay)
    {
        return timeSinceHit >= healDelay;
    }

    public static float CalculateHealAmount(float current, float maximum, float healRate)
    {
        if (current >= maximum)
        {
            return 0f;
        }

        return Math.Min(healRate, maximum - current);
    }

    public static bool ShouldHealBodyPart(float current, float maximum)
    {
        return current > 0f && current < maximum;
    }
}
