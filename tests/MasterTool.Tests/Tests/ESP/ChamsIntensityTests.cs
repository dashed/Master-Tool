using System;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for the ChamsManager color intensity scaling logic.
/// Duplicates the pure color math since Unity Color/Material
/// cannot be used from net9.0 tests.
/// </summary>
[TestFixture]
public class ChamsIntensityTests
{
    /// <summary>
    /// Simple RGBA color struct mirroring UnityEngine.Color for testing.
    /// </summary>
    private struct FakeColor
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public FakeColor(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    /// <summary>
    /// Mirrors the intensity scaling logic from ChamsManager.ApplyChams().
    /// Clamps intensity to [0.1, 1.0], multiplies RGB, preserves alpha.
    /// </summary>
    private static FakeColor ApplyIntensity(FakeColor color, float intensity)
    {
        float clamped = Math.Clamp(intensity, 0.1f, 1f);
        return new FakeColor(color.R * clamped, color.G * clamped, color.B * clamped, color.A);
    }

    [Test]
    public void FullIntensity_ColorUnchanged()
    {
        var color = new FakeColor(1f, 0f, 0f);
        var result = ApplyIntensity(color, 1.0f);

        Assert.That(result.R, Is.EqualTo(1f).Within(0.001f));
        Assert.That(result.G, Is.EqualTo(0f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0f).Within(0.001f));
        Assert.That(result.A, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void HalfIntensity_ColorHalved()
    {
        var color = new FakeColor(1f, 0.5f, 0.25f);
        var result = ApplyIntensity(color, 0.5f);

        Assert.That(result.R, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.G, Is.EqualTo(0.25f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0.125f).Within(0.001f));
        Assert.That(result.A, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void MinIntensity_ClampedToTenPercent()
    {
        var color = new FakeColor(1f, 1f, 1f);
        var result = ApplyIntensity(color, 0.0f);

        Assert.That(result.R, Is.EqualTo(0.1f).Within(0.001f), "Should clamp to 0.1 minimum");
        Assert.That(result.G, Is.EqualTo(0.1f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0.1f).Within(0.001f));
    }

    [Test]
    public void NegativeIntensity_ClampedToTenPercent()
    {
        var color = new FakeColor(1f, 0f, 0f);
        var result = ApplyIntensity(color, -5f);

        Assert.That(result.R, Is.EqualTo(0.1f).Within(0.001f));
    }

    [Test]
    public void OverOneIntensity_ClampedToOne()
    {
        var color = new FakeColor(0.5f, 0.5f, 0.5f);
        var result = ApplyIntensity(color, 2.0f);

        Assert.That(result.R, Is.EqualTo(0.5f).Within(0.001f), "Should clamp to 1.0 maximum");
        Assert.That(result.G, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void AlphaPreserved_AtAllIntensities()
    {
        var color = new FakeColor(1f, 1f, 1f, 0.8f);

        Assert.That(ApplyIntensity(color, 1.0f).A, Is.EqualTo(0.8f).Within(0.001f));
        Assert.That(ApplyIntensity(color, 0.5f).A, Is.EqualTo(0.8f).Within(0.001f));
        Assert.That(ApplyIntensity(color, 0.1f).A, Is.EqualTo(0.8f).Within(0.001f));
    }

    [Test]
    public void DefaultIntensity_HalfBrightness()
    {
        // Config default is 0.5
        var red = new FakeColor(1f, 0f, 0f);
        var result = ApplyIntensity(red, 0.5f);

        Assert.That(result.R, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.G, Is.EqualTo(0f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0f).Within(0.001f));
    }
}
