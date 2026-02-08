namespace MasterTool.Core;

public enum RebindState
{
    Idle,
    WaitingForKey,
}

public enum RebindOutcome
{
    Ignore,
    Accept,
    Cancel,
}

public static class RebindLogic
{
    /// <summary>
    /// Known hotkey labels that must be present in the rebind list.
    /// </summary>
    public static readonly string[] HotkeyLabels =
    {
        "Menu",
        "Status Window",
        "God Mode",
        "Stamina",
        "Weight",
        "Energy",
        "Hydration",
        "No Fall Damage",
        "COD Mode",
        "Reload Speed",
        "Fly Mode",
        "Player ESP",
        "Item ESP",
        "Container ESP",
        "Quest ESP",
        "Chams",
        "Culling",
        "Unlock Doors",
        "Weapon Info",
        "Save Position",
        "Load Position",
        "Teleport Surface",
    };

    /// <summary>
    /// Determines whether a key event should be accepted during hotkey rebinding.
    /// KeyCode.None = 0.
    /// </summary>
    public static bool ShouldAcceptKey(bool isRebinding, bool isKeyDown, int keyCode)
    {
        return isRebinding && isKeyDown && keyCode != 0;
    }

    /// <summary>
    /// Frame-delay guard: key events are only accepted when the current frame is
    /// strictly after the frame where rebinding started.
    /// </summary>
    public static bool IsFrameReadyForInput(int currentFrame, int rebindStartFrame)
    {
        return currentFrame > rebindStartFrame;
    }

    /// <summary>
    /// Combined check: rebinding active, frame delay passed, and valid key event.
    /// </summary>
    public static bool ShouldAcceptKeyWithFrameGuard(bool isRebinding, bool isKeyDown, int keyCode, int currentFrame, int rebindStartFrame)
    {
        return isRebinding && IsFrameReadyForInput(currentFrame, rebindStartFrame) && isKeyDown && keyCode != 0;
    }

    /// <summary>
    /// Determines the outcome when a key is pressed during rebinding.
    /// Escape (keyCode=27) cancels, other valid keys are accepted.
    /// </summary>
    public static RebindOutcome GetRebindOutcome(bool isRebinding, bool isKeyDown, int keyCode)
    {
        if (!isRebinding || !isKeyDown || keyCode == 0)
        {
            return RebindOutcome.Ignore;
        }

        // KeyCode.Escape = 27
        if (keyCode == 27)
        {
            return RebindOutcome.Cancel;
        }

        return RebindOutcome.Accept;
    }

    /// <summary>
    /// Returns the rebind state based on whether there is an active rebind.
    /// </summary>
    public static RebindState GetRebindState(bool hasActiveRebind)
    {
        return hasActiveRebind ? RebindState.WaitingForKey : RebindState.Idle;
    }
}
