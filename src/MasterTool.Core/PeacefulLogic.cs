namespace MasterTool.Core;

/// <summary>
/// Pure logic for the Peaceful Mode feature. Determines whether bot AI should
/// be prevented from registering the local player as an enemy.
/// </summary>
public static class PeacefulLogic
{
    /// <summary>
    /// Returns true if the enemy registration should be blocked.
    /// Only blocks when peaceful mode is enabled AND the target is the local player,
    /// so bot-vs-bot AI remains unaffected.
    /// </summary>
    public static bool ShouldBlockEnemy(bool peacefulEnabled, bool isLocalPlayer)
    {
        return peacefulEnabled && isLocalPlayer;
    }
}
