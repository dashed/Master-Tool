namespace MasterTool.Core;

public static class MovementLogic
{
    /// <summary>
    /// Computes a normalized, speed-scaled movement vector from directional inputs.
    /// </summary>
    public static Vec3 CalculateFlyMovement(
        Vec3 forward,
        Vec3 right,
        float inputH,
        float inputV,
        float inputUp,
        float inputDown,
        float speed,
        float deltaTime
    )
    {
        Vec3 move = forward * inputV + right * inputH + Vec3.Up * (inputUp - inputDown);
        if (move.SqrMagnitude < 0.001f)
        {
            return Vec3.Zero;
        }

        return move.Normalized * speed * deltaTime;
    }

    /// <summary>
    /// Places the ray origin 500 units above the player position for ground-detection raycasting.
    /// </summary>
    public static Vec3 CalculateRayOrigin(Vec3 playerPosition)
    {
        return new Vec3(playerPosition.x, playerPosition.y + 500f, playerPosition.z);
    }
}
