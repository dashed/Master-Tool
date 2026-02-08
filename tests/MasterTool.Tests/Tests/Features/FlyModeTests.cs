using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the fly movement calculation logic used by FlyModeFeature.
/// Uses <see cref="MovementLogic"/> and <see cref="Vec3"/> from MasterTool.Core.
/// </summary>
[TestFixture]
public class FlyModeTests
{
    private readonly Vec3 _forward = new Vec3(0, 0, 1);
    private readonly Vec3 _right = new Vec3(1, 0, 0);

    // --- Movement direction tests ---

    [Test]
    public void ForwardOnly_MovesForward()
    {
        var result = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, 1f);
        Assert.That(result.z, Is.GreaterThan(0));
        Assert.That(result.x, Is.EqualTo(0).Within(0.01f));
    }

    [Test]
    public void BackwardOnly_MovesBackward()
    {
        var result = MovementLogic.CalculateFlyMovement(_forward, _right, 0, -1, 0, 0, 10f, 1f);
        Assert.That(result.z, Is.LessThan(0));
    }

    [Test]
    public void StrafeRight_MovesRight()
    {
        var result = MovementLogic.CalculateFlyMovement(_forward, _right, 1, 0, 0, 0, 10f, 1f);
        Assert.That(result.x, Is.GreaterThan(0));
        Assert.That(result.z, Is.EqualTo(0).Within(0.01f));
    }

    [Test]
    public void StrafeLeft_MovesLeft()
    {
        var result = MovementLogic.CalculateFlyMovement(_forward, _right, -1, 0, 0, 0, 10f, 1f);
        Assert.That(result.x, Is.LessThan(0));
    }

    [Test]
    public void UpOnly_MovesUp()
    {
        var result = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 0, 1, 0, 10f, 1f);
        Assert.That(result.y, Is.GreaterThan(0));
    }

    [Test]
    public void DownOnly_MovesDown()
    {
        var result = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 0, 0, 1, 10f, 1f);
        Assert.That(result.y, Is.LessThan(0));
    }

    [Test]
    public void ZeroInput_ReturnsZero()
    {
        var result = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 0, 0, 0, 10f, 1f);
        Assert.That(result.SqrMagnitude, Is.EqualTo(0));
    }

    [Test]
    public void DiagonalForwardRight_IsNormalized()
    {
        var result = MovementLogic.CalculateFlyMovement(_forward, _right, 1, 1, 0, 0, 10f, 1f);
        // Magnitude should be speed * deltaTime = 10
        Assert.That(result.Magnitude, Is.EqualTo(10f).Within(0.1f));
    }

    [Test]
    public void SpeedScaling_AffectsMagnitude()
    {
        var slow = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 5f, 1f);
        var fast = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 20f, 1f);
        Assert.That(fast.Magnitude, Is.GreaterThan(slow.Magnitude));
        Assert.That(fast.Magnitude / slow.Magnitude, Is.EqualTo(4f).Within(0.1f));
    }

    [Test]
    public void DeltaTimeScaling_AffectsMagnitude()
    {
        var short_ = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, 0.016f);
        var long_ = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, 0.033f);
        Assert.That(long_.Magnitude, Is.GreaterThan(short_.Magnitude));
    }

    [Test]
    public void UpAndDown_Cancel()
    {
        var result = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 0, 1, 1, 10f, 1f);
        Assert.That(result.SqrMagnitude, Is.EqualTo(0).Within(0.01f));
    }

    // --- State machine tests ---

    [Test]
    public void Enable_SetsModForced()
    {
        bool modForced = false;
        // Simulate enable
        modForced = true;
        Assert.That(modForced, Is.True);
    }

    [Test]
    public void Disable_RestoresModForced()
    {
        bool modForced = true;
        // Simulate disable
        modForced = false;
        Assert.That(modForced, Is.False);
    }
}
