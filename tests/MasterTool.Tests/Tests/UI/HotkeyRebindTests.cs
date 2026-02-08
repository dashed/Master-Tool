using NUnit.Framework;

namespace MasterTool.Tests.Tests.UI;

/// <summary>
/// Tests for hotkey rebinding logic used by the mod menu Hotkeys tab.
/// Duplicates the pure decision methods since Unity/BepInEx cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class HotkeyRebindTests
{
    /// <summary>
    /// Duplicates ModMenu.ShouldAcceptKey: determines whether a key event should be accepted
    /// during hotkey rebinding. KeyCode.None = 0.
    /// </summary>
    private static bool ShouldAcceptKey(bool isRebinding, bool isKeyDown, int keyCode)
    {
        return isRebinding && isKeyDown && keyCode != 0;
    }

    /// <summary>
    /// Rebind state: idle (no active rebind) or waiting for key input.
    /// </summary>
    private enum RebindState
    {
        Idle,
        WaitingForKey,
    }

    private static RebindState GetRebindState(bool hasActiveRebind)
    {
        return hasActiveRebind ? RebindState.WaitingForKey : RebindState.Idle;
    }

    /// <summary>
    /// Determines the outcome when a key is pressed during rebinding.
    /// Escape (keyCode=27) cancels, other valid keys are accepted.
    /// </summary>
    private enum RebindOutcome
    {
        Ignore,
        Accept,
        Cancel,
    }

    private static RebindOutcome GetRebindOutcome(bool isRebinding, bool isKeyDown, int keyCode)
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

    // Known hotkey labels that must be present in the rebind list
    private static readonly string[] ExpectedHotkeyLabels =
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

    // --- ShouldAcceptKey tests ---

    [Test]
    public void ShouldAcceptKey_ValidKey_ReturnsTrue()
    {
        // KeyCode.A = 97
        Assert.That(ShouldAcceptKey(true, true, 97), Is.True);
    }

    [Test]
    public void ShouldAcceptKey_NotRebinding_ReturnsFalse()
    {
        Assert.That(ShouldAcceptKey(false, true, 97), Is.False);
    }

    [Test]
    public void ShouldAcceptKey_NotKeyDown_ReturnsFalse()
    {
        Assert.That(ShouldAcceptKey(true, false, 97), Is.False);
    }

    [Test]
    public void ShouldAcceptKey_KeyCodeNone_ReturnsFalse()
    {
        // KeyCode.None = 0
        Assert.That(ShouldAcceptKey(true, true, 0), Is.False);
    }

    [Test]
    public void ShouldAcceptKey_AllFalse_ReturnsFalse()
    {
        Assert.That(ShouldAcceptKey(false, false, 0), Is.False);
    }

    // --- RebindState tests ---

    [Test]
    public void RebindState_NoActiveRebind_IsIdle()
    {
        Assert.That(GetRebindState(false), Is.EqualTo(RebindState.Idle));
    }

    [Test]
    public void RebindState_ActiveRebind_IsWaiting()
    {
        Assert.That(GetRebindState(true), Is.EqualTo(RebindState.WaitingForKey));
    }

    // --- RebindOutcome tests ---

    [Test]
    public void RebindOutcome_ValidKey_Accepts()
    {
        // KeyCode.F1 = 282
        Assert.That(GetRebindOutcome(true, true, 282), Is.EqualTo(RebindOutcome.Accept));
    }

    [Test]
    public void RebindOutcome_Escape_Cancels()
    {
        // KeyCode.Escape = 27
        Assert.That(GetRebindOutcome(true, true, 27), Is.EqualTo(RebindOutcome.Cancel));
    }

    [Test]
    public void RebindOutcome_NotRebinding_Ignores()
    {
        Assert.That(GetRebindOutcome(false, true, 97), Is.EqualTo(RebindOutcome.Ignore));
    }

    [Test]
    public void RebindOutcome_KeyCodeNone_Ignores()
    {
        Assert.That(GetRebindOutcome(true, true, 0), Is.EqualTo(RebindOutcome.Ignore));
    }

    [Test]
    public void RebindOutcome_NotKeyDown_Ignores()
    {
        Assert.That(GetRebindOutcome(true, false, 97), Is.EqualTo(RebindOutcome.Ignore));
    }

    // --- Hotkey label list tests ---

    [Test]
    public void HotkeyLabels_Has22Entries()
    {
        Assert.That(ExpectedHotkeyLabels.Length, Is.EqualTo(22));
    }

    [Test]
    public void HotkeyLabels_ContainsMenu()
    {
        Assert.That(ExpectedHotkeyLabels, Does.Contain("Menu"));
    }

    [Test]
    public void HotkeyLabels_ContainsGodMode()
    {
        Assert.That(ExpectedHotkeyLabels, Does.Contain("God Mode"));
    }

    [Test]
    public void HotkeyLabels_ContainsFlyMode()
    {
        Assert.That(ExpectedHotkeyLabels, Does.Contain("Fly Mode"));
    }

    [Test]
    public void HotkeyLabels_ContainsTeleportSurface()
    {
        Assert.That(ExpectedHotkeyLabels, Does.Contain("Teleport Surface"));
    }

    [Test]
    public void HotkeyLabels_AllUnique()
    {
        var set = new System.Collections.Generic.HashSet<string>(ExpectedHotkeyLabels);
        Assert.That(set.Count, Is.EqualTo(ExpectedHotkeyLabels.Length));
    }

    // --- State transition scenario tests ---

    [Test]
    public void FullRebindCycle_IdleToWaitingToIdle()
    {
        // Start idle
        bool hasActiveRebind = false;
        Assert.That(GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.Idle));

        // User clicks Rebind
        hasActiveRebind = true;
        Assert.That(GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.WaitingForKey));

        // Key accepted → back to idle
        var outcome = GetRebindOutcome(hasActiveRebind, true, 282);
        Assert.That(outcome, Is.EqualTo(RebindOutcome.Accept));
        hasActiveRebind = false;
        Assert.That(GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.Idle));
    }

    [Test]
    public void CancelRebindCycle_IdleToWaitingToIdle()
    {
        bool hasActiveRebind = false;
        Assert.That(GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.Idle));

        hasActiveRebind = true;
        Assert.That(GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.WaitingForKey));

        // Escape cancels
        var outcome = GetRebindOutcome(hasActiveRebind, true, 27);
        Assert.That(outcome, Is.EqualTo(RebindOutcome.Cancel));
        hasActiveRebind = false;
        Assert.That(GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.Idle));
    }
}
