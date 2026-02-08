namespace MasterTool.Core;

/// <summary>
/// Pure speedhack displacement logic extracted from SpeedhackFeature for shared testing.
/// </summary>
public static class SpeedhackLogic
{
    /// <summary>
    /// Computes the extra positional displacement added each frame by the speedhack.
    /// The formula mirrors SpeedhackFeature.Apply: direction * multiplier * deltaTime * 5.
    /// </summary>
    public static Vec3 ComputeDisplacement(Vec3 moveDirection, float multiplier, float deltaTime)
    {
        return moveDirection * (multiplier * deltaTime * 5f);
    }
}
