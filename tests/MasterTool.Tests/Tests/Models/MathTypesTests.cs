using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Models;

/// <summary>
/// Tests for <see cref="Vec3"/>, <see cref="Vec2"/>, and <see cref="Color"/> math types.
/// </summary>
[TestFixture]
public class MathTypesTests
{
    // --- Vec3 tests ---

    [Test]
    public void Vec3_Constructor_SetsXYZ()
    {
        var v = new Vec3(1f, 2f, 3f);

        Assert.That(v.x, Is.EqualTo(1f));
        Assert.That(v.y, Is.EqualTo(2f));
        Assert.That(v.z, Is.EqualTo(3f));
    }

    [Test]
    public void Vec3_Add_Operator_SumsComponents()
    {
        var a = new Vec3(1f, 2f, 3f);
        var b = new Vec3(4f, 5f, 6f);

        var result = a + b;

        Assert.That(result.x, Is.EqualTo(5f));
        Assert.That(result.y, Is.EqualTo(7f));
        Assert.That(result.z, Is.EqualTo(9f));
    }

    [Test]
    public void Vec3_Multiply_ScalarRight_ScalesComponents()
    {
        var v = new Vec3(1f, 2f, 3f);

        var result = v * 2f;

        Assert.That(result.x, Is.EqualTo(2f));
        Assert.That(result.y, Is.EqualTo(4f));
        Assert.That(result.z, Is.EqualTo(6f));
    }

    [Test]
    public void Vec3_Multiply_ScalarLeft_ScalesComponents()
    {
        var v = new Vec3(1f, 2f, 3f);

        var result = 2f * v;

        Assert.That(result.x, Is.EqualTo(2f));
        Assert.That(result.y, Is.EqualTo(4f));
        Assert.That(result.z, Is.EqualTo(6f));
    }

    [Test]
    public void Vec3_SqrMagnitude_ReturnsSquaredLength()
    {
        var v = new Vec3(3f, 4f, 0f);

        Assert.That(v.SqrMagnitude, Is.EqualTo(25f));
    }

    [Test]
    public void Vec3_Magnitude_ReturnsLength()
    {
        var v = new Vec3(3f, 4f, 0f);

        Assert.That(v.Magnitude, Is.EqualTo(5f).Within(0.001f));
    }

    [Test]
    public void Vec3_Magnitude_UnitVector_IsOne()
    {
        var v = new Vec3(1f, 0f, 0f);

        Assert.That(v.Magnitude, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void Vec3_Normalized_ScaledVector_HasMagnitudeOne()
    {
        var v = new Vec3(3f, 4f, 5f);

        var n = v.Normalized;

        Assert.That(n.Magnitude, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void Vec3_Normalized_ZeroVector_ReturnsZero()
    {
        var v = new Vec3(0f, 0f, 0f);

        var n = v.Normalized;

        Assert.That(n.x, Is.EqualTo(0f));
        Assert.That(n.y, Is.EqualTo(0f));
        Assert.That(n.z, Is.EqualTo(0f));
    }

    [Test]
    public void Vec3_Normalized_PreservesDirection()
    {
        var v = new Vec3(6f, 8f, 0f);

        var n = v.Normalized;

        // Original ratio x:y = 6:8 = 3:4, normalized should preserve this
        float ratio = n.x / n.y;
        Assert.That(ratio, Is.EqualTo(6f / 8f).Within(0.001f));
    }

    [Test]
    public void Vec3_Zero_Property_IsAllZeros()
    {
        var v = Vec3.Zero;

        Assert.That(v.x, Is.EqualTo(0f));
        Assert.That(v.y, Is.EqualTo(0f));
        Assert.That(v.z, Is.EqualTo(0f));
    }

    [Test]
    public void Vec3_Up_Property_IsUnitY()
    {
        var v = Vec3.Up;

        Assert.That(v.x, Is.EqualTo(0f));
        Assert.That(v.y, Is.EqualTo(1f));
        Assert.That(v.z, Is.EqualTo(0f));
    }

    // --- Vec2 tests ---

    [Test]
    public void Vec2_Constructor_SetsXY()
    {
        var v = new Vec2(10f, 20f);

        Assert.That(v.X, Is.EqualTo(10f));
        Assert.That(v.Y, Is.EqualTo(20f));
    }

    [Test]
    public void Vec2_Default_IsZero()
    {
        var v = default(Vec2);

        Assert.That(v.X, Is.EqualTo(0f));
        Assert.That(v.Y, Is.EqualTo(0f));
    }

    // --- Color tests ---

    [Test]
    public void Color_Constructor_WithAlpha_SetsRGBA()
    {
        var c = new Color(0.2f, 0.4f, 0.6f, 0.8f);

        Assert.That(c.R, Is.EqualTo(0.2f).Within(0.001f));
        Assert.That(c.G, Is.EqualTo(0.4f).Within(0.001f));
        Assert.That(c.B, Is.EqualTo(0.6f).Within(0.001f));
        Assert.That(c.A, Is.EqualTo(0.8f).Within(0.001f));
    }

    [Test]
    public void Color_Constructor_DefaultAlpha_IsOne()
    {
        var c = new Color(0.5f, 0.5f, 0.5f);

        Assert.That(c.A, Is.EqualTo(1f));
    }

    [Test]
    public void Color_Constructor_ZeroRGB_WithAlpha()
    {
        var c = new Color(0f, 0f, 0f, 0.5f);

        Assert.That(c.R, Is.EqualTo(0f));
        Assert.That(c.G, Is.EqualTo(0f));
        Assert.That(c.B, Is.EqualTo(0f));
        Assert.That(c.A, Is.EqualTo(0.5f));
    }

    [Test]
    public void Color_Fields_AreSettable()
    {
        var c = new Color(0f, 0f, 0f);
        c.R = 1f;
        c.G = 0.5f;
        c.B = 0.25f;
        c.A = 0.75f;

        Assert.That(c.R, Is.EqualTo(1f));
        Assert.That(c.G, Is.EqualTo(0.5f));
        Assert.That(c.B, Is.EqualTo(0.25f));
        Assert.That(c.A, Is.EqualTo(0.75f));
    }
}
