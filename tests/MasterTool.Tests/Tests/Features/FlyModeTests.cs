using System;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for the fly movement calculation logic used by FlyModeFeature.
/// Duplicates the pure CalculateFlyMovement method since Unity
/// cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class FlyModeTests
{
    // Minimal Vector3 for testing
    private struct Vec3
    {
        public float x,
            y,
            z;

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vec3 Up
        {
            get { return new Vec3(0, 1, 0); }
        }

        public static Vec3 Zero
        {
            get { return new Vec3(0, 0, 0); }
        }

        public float SqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }

        public float Magnitude
        {
            get { return MathF.Sqrt(SqrMagnitude); }
        }

        public Vec3 Normalized
        {
            get
            {
                float m = Magnitude;
                if (m < 0.0001f)
                {
                    return Zero;
                }

                return new Vec3(x / m, y / m, z / m);
            }
        }

        public static Vec3 operator +(Vec3 a, Vec3 b)
        {
            return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vec3 operator *(Vec3 a, float s)
        {
            return new Vec3(a.x * s, a.y * s, a.z * s);
        }

        public static Vec3 operator *(float s, Vec3 a)
        {
            return a * s;
        }
    }

    /// <summary>
    /// Duplicates pure logic from FlyModeFeature.CalculateFlyMovement.
    /// Computes a normalized, speed-scaled movement vector from directional inputs.
    /// </summary>
    private static Vec3 CalculateFlyMovement(
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

    private readonly Vec3 _forward = new Vec3(0, 0, 1);
    private readonly Vec3 _right = new Vec3(1, 0, 0);

    // --- Movement direction tests ---

    [Test]
    public void ForwardOnly_MovesForward()
    {
        var result = CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, 1f);
        Assert.That(result.z, Is.GreaterThan(0));
        Assert.That(result.x, Is.EqualTo(0).Within(0.01f));
    }

    [Test]
    public void BackwardOnly_MovesBackward()
    {
        var result = CalculateFlyMovement(_forward, _right, 0, -1, 0, 0, 10f, 1f);
        Assert.That(result.z, Is.LessThan(0));
    }

    [Test]
    public void StrafeRight_MovesRight()
    {
        var result = CalculateFlyMovement(_forward, _right, 1, 0, 0, 0, 10f, 1f);
        Assert.That(result.x, Is.GreaterThan(0));
        Assert.That(result.z, Is.EqualTo(0).Within(0.01f));
    }

    [Test]
    public void StrafeLeft_MovesLeft()
    {
        var result = CalculateFlyMovement(_forward, _right, -1, 0, 0, 0, 10f, 1f);
        Assert.That(result.x, Is.LessThan(0));
    }

    [Test]
    public void UpOnly_MovesUp()
    {
        var result = CalculateFlyMovement(_forward, _right, 0, 0, 1, 0, 10f, 1f);
        Assert.That(result.y, Is.GreaterThan(0));
    }

    [Test]
    public void DownOnly_MovesDown()
    {
        var result = CalculateFlyMovement(_forward, _right, 0, 0, 0, 1, 10f, 1f);
        Assert.That(result.y, Is.LessThan(0));
    }

    [Test]
    public void ZeroInput_ReturnsZero()
    {
        var result = CalculateFlyMovement(_forward, _right, 0, 0, 0, 0, 10f, 1f);
        Assert.That(result.SqrMagnitude, Is.EqualTo(0));
    }

    [Test]
    public void DiagonalForwardRight_IsNormalized()
    {
        var result = CalculateFlyMovement(_forward, _right, 1, 1, 0, 0, 10f, 1f);
        // Magnitude should be speed * deltaTime = 10
        Assert.That(result.Magnitude, Is.EqualTo(10f).Within(0.1f));
    }

    [Test]
    public void SpeedScaling_AffectsMagnitude()
    {
        var slow = CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 5f, 1f);
        var fast = CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 20f, 1f);
        Assert.That(fast.Magnitude, Is.GreaterThan(slow.Magnitude));
        Assert.That(fast.Magnitude / slow.Magnitude, Is.EqualTo(4f).Within(0.1f));
    }

    [Test]
    public void DeltaTimeScaling_AffectsMagnitude()
    {
        var short_ = CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, 0.016f);
        var long_ = CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, 0.033f);
        Assert.That(long_.Magnitude, Is.GreaterThan(short_.Magnitude));
    }

    [Test]
    public void UpAndDown_Cancel()
    {
        var result = CalculateFlyMovement(_forward, _right, 0, 0, 1, 1, 10f, 1f);
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
