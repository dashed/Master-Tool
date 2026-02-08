using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.UI;

/// <summary>
/// Tests for hotkey rebinding logic used by the mod menu Hotkeys tab.
/// Uses <see cref="RebindLogic"/> from MasterTool.Core (shared library).
/// </summary>
[TestFixture]
public class HotkeyRebindTests
{
    // --- ShouldAcceptKey tests ---

    [Test]
    public void ShouldAcceptKey_ValidKey_ReturnsTrue()
    {
        // KeyCode.A = 97
        Assert.That(RebindLogic.ShouldAcceptKey(true, true, 97), Is.True);
    }

    [Test]
    public void ShouldAcceptKey_NotRebinding_ReturnsFalse()
    {
        Assert.That(RebindLogic.ShouldAcceptKey(false, true, 97), Is.False);
    }

    [Test]
    public void ShouldAcceptKey_NotKeyDown_ReturnsFalse()
    {
        Assert.That(RebindLogic.ShouldAcceptKey(true, false, 97), Is.False);
    }

    [Test]
    public void ShouldAcceptKey_KeyCodeNone_ReturnsFalse()
    {
        // KeyCode.None = 0
        Assert.That(RebindLogic.ShouldAcceptKey(true, true, 0), Is.False);
    }

    [Test]
    public void ShouldAcceptKey_AllFalse_ReturnsFalse()
    {
        Assert.That(RebindLogic.ShouldAcceptKey(false, false, 0), Is.False);
    }

    // --- RebindState tests ---

    [Test]
    public void RebindState_NoActiveRebind_IsIdle()
    {
        Assert.That(RebindLogic.GetRebindState(false), Is.EqualTo(RebindState.Idle));
    }

    [Test]
    public void RebindState_ActiveRebind_IsWaiting()
    {
        Assert.That(RebindLogic.GetRebindState(true), Is.EqualTo(RebindState.WaitingForKey));
    }

    // --- RebindOutcome tests ---

    [Test]
    public void RebindOutcome_ValidKey_Accepts()
    {
        // KeyCode.F1 = 282
        Assert.That(RebindLogic.GetRebindOutcome(true, true, 282), Is.EqualTo(RebindOutcome.Accept));
    }

    [Test]
    public void RebindOutcome_Escape_Cancels()
    {
        // KeyCode.Escape = 27
        Assert.That(RebindLogic.GetRebindOutcome(true, true, 27), Is.EqualTo(RebindOutcome.Cancel));
    }

    [Test]
    public void RebindOutcome_NotRebinding_Ignores()
    {
        Assert.That(RebindLogic.GetRebindOutcome(false, true, 97), Is.EqualTo(RebindOutcome.Ignore));
    }

    [Test]
    public void RebindOutcome_KeyCodeNone_Ignores()
    {
        Assert.That(RebindLogic.GetRebindOutcome(true, true, 0), Is.EqualTo(RebindOutcome.Ignore));
    }

    [Test]
    public void RebindOutcome_NotKeyDown_Ignores()
    {
        Assert.That(RebindLogic.GetRebindOutcome(true, false, 97), Is.EqualTo(RebindOutcome.Ignore));
    }

    // --- Hotkey label list tests ---

    [Test]
    public void HotkeyLabels_Has22Entries()
    {
        Assert.That(RebindLogic.HotkeyLabels.Length, Is.EqualTo(22));
    }

    [Test]
    public void HotkeyLabels_ContainsMenu()
    {
        Assert.That(RebindLogic.HotkeyLabels, Does.Contain("Menu"));
    }

    [Test]
    public void HotkeyLabels_ContainsGodMode()
    {
        Assert.That(RebindLogic.HotkeyLabels, Does.Contain("God Mode"));
    }

    [Test]
    public void HotkeyLabels_ContainsFlyMode()
    {
        Assert.That(RebindLogic.HotkeyLabels, Does.Contain("Fly Mode"));
    }

    [Test]
    public void HotkeyLabels_ContainsTeleportSurface()
    {
        Assert.That(RebindLogic.HotkeyLabels, Does.Contain("Teleport Surface"));
    }

    [Test]
    public void HotkeyLabels_AllUnique()
    {
        var set = new System.Collections.Generic.HashSet<string>(RebindLogic.HotkeyLabels);
        Assert.That(set.Count, Is.EqualTo(RebindLogic.HotkeyLabels.Length));
    }

    // --- State transition scenario tests ---

    [Test]
    public void FullRebindCycle_IdleToWaitingToIdle()
    {
        // Start idle
        bool hasActiveRebind = false;
        Assert.That(RebindLogic.GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.Idle));

        // User clicks Rebind
        hasActiveRebind = true;
        Assert.That(RebindLogic.GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.WaitingForKey));

        // Key accepted -> back to idle
        var outcome = RebindLogic.GetRebindOutcome(hasActiveRebind, true, 282);
        Assert.That(outcome, Is.EqualTo(RebindOutcome.Accept));
        hasActiveRebind = false;
        Assert.That(RebindLogic.GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.Idle));
    }

    [Test]
    public void CancelRebindCycle_IdleToWaitingToIdle()
    {
        bool hasActiveRebind = false;
        Assert.That(RebindLogic.GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.Idle));

        hasActiveRebind = true;
        Assert.That(RebindLogic.GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.WaitingForKey));

        // Escape cancels
        var outcome = RebindLogic.GetRebindOutcome(hasActiveRebind, true, 27);
        Assert.That(outcome, Is.EqualTo(RebindOutcome.Cancel));
        hasActiveRebind = false;
        Assert.That(RebindLogic.GetRebindState(hasActiveRebind), Is.EqualTo(RebindState.Idle));
    }

    // --- Frame delay guard tests ---

    [Test]
    public void FrameGuard_SameFrame_RejectsInput()
    {
        // Rebind started on frame 100, current frame is also 100
        Assert.That(RebindLogic.IsFrameReadyForInput(100, 100), Is.False);
    }

    [Test]
    public void FrameGuard_NextFrame_AcceptsInput()
    {
        // Rebind started on frame 100, current frame is 101
        Assert.That(RebindLogic.IsFrameReadyForInput(101, 100), Is.True);
    }

    [Test]
    public void FrameGuard_ManyFramesLater_AcceptsInput()
    {
        Assert.That(RebindLogic.IsFrameReadyForInput(200, 100), Is.True);
    }

    [Test]
    public void FrameGuard_DefaultNegativeOne_AcceptsFrame0()
    {
        // Default _rebindStartFrame is -1, so even frame 0 should pass
        Assert.That(RebindLogic.IsFrameReadyForInput(0, -1), Is.True);
    }

    [Test]
    public void ShouldAcceptKeyWithFrameGuard_SameFrame_ReturnsFalse()
    {
        // Valid key on same frame as rebind start -> rejected
        Assert.That(RebindLogic.ShouldAcceptKeyWithFrameGuard(true, true, 282, 50, 50), Is.False);
    }

    [Test]
    public void ShouldAcceptKeyWithFrameGuard_NextFrame_ValidKey_ReturnsTrue()
    {
        // Valid key one frame after rebind start -> accepted
        Assert.That(RebindLogic.ShouldAcceptKeyWithFrameGuard(true, true, 282, 51, 50), Is.True);
    }

    [Test]
    public void ShouldAcceptKeyWithFrameGuard_NextFrame_NotRebinding_ReturnsFalse()
    {
        Assert.That(RebindLogic.ShouldAcceptKeyWithFrameGuard(false, true, 282, 51, 50), Is.False);
    }

    [Test]
    public void ShouldAcceptKeyWithFrameGuard_NextFrame_KeyCodeNone_ReturnsFalse()
    {
        Assert.That(RebindLogic.ShouldAcceptKeyWithFrameGuard(true, true, 0, 51, 50), Is.False);
    }

    [Test]
    public void FullRebindCycle_WithFrameDelay_RejectsThenAccepts()
    {
        // Simulate: click Rebind at frame 100
        int rebindStartFrame = 100;
        bool isRebinding = true;

        // Same frame: spurious F13 (keyCode=124) should be rejected
        Assert.That(RebindLogic.ShouldAcceptKeyWithFrameGuard(isRebinding, true, 124, 100, rebindStartFrame), Is.False);

        // Next frame: real key press (F1=282) should be accepted
        Assert.That(RebindLogic.ShouldAcceptKeyWithFrameGuard(isRebinding, true, 282, 101, rebindStartFrame), Is.True);
    }
}
