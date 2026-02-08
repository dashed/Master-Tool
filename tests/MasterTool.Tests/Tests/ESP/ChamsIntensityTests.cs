using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for the ChamsManager color intensity and opacity scaling logic.
/// Uses <see cref="ChamsLogic"/> from MasterTool.Core (shared library).
/// </summary>
[TestFixture]
public class ChamsIntensityTests
{
    // === Intensity-only tests ===

    [Test]
    public void FullIntensity_ColorUnchanged()
    {
        var color = new Color(1f, 0f, 0f);
        var result = ChamsLogic.ApplyIntensity(color, 1.0f);

        Assert.That(result.R, Is.EqualTo(1f).Within(0.001f));
        Assert.That(result.G, Is.EqualTo(0f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0f).Within(0.001f));
        Assert.That(result.A, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void HalfIntensity_ColorHalved()
    {
        var color = new Color(1f, 0.5f, 0.25f);
        var result = ChamsLogic.ApplyIntensity(color, 0.5f);

        Assert.That(result.R, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.G, Is.EqualTo(0.25f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0.125f).Within(0.001f));
        Assert.That(result.A, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void MinIntensity_ClampedToTenPercent()
    {
        var color = new Color(1f, 1f, 1f);
        var result = ChamsLogic.ApplyIntensity(color, 0.0f);

        Assert.That(result.R, Is.EqualTo(0.1f).Within(0.001f), "Should clamp to 0.1 minimum");
        Assert.That(result.G, Is.EqualTo(0.1f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0.1f).Within(0.001f));
    }

    [Test]
    public void NegativeIntensity_ClampedToTenPercent()
    {
        var color = new Color(1f, 0f, 0f);
        var result = ChamsLogic.ApplyIntensity(color, -5f);

        Assert.That(result.R, Is.EqualTo(0.1f).Within(0.001f));
    }

    [Test]
    public void OverOneIntensity_ClampedToOne()
    {
        var color = new Color(0.5f, 0.5f, 0.5f);
        var result = ChamsLogic.ApplyIntensity(color, 2.0f);

        Assert.That(result.R, Is.EqualTo(0.5f).Within(0.001f), "Should clamp to 1.0 maximum");
        Assert.That(result.G, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void AlphaPreserved_AtAllIntensities()
    {
        var color = new Color(1f, 1f, 1f, 0.8f);

        Assert.That(ChamsLogic.ApplyIntensity(color, 1.0f).A, Is.EqualTo(0.8f).Within(0.001f));
        Assert.That(ChamsLogic.ApplyIntensity(color, 0.5f).A, Is.EqualTo(0.8f).Within(0.001f));
        Assert.That(ChamsLogic.ApplyIntensity(color, 0.1f).A, Is.EqualTo(0.8f).Within(0.001f));
    }

    [Test]
    public void DefaultIntensity_HalfBrightness()
    {
        // Config default is 0.5
        var red = new Color(1f, 0f, 0f);
        var result = ChamsLogic.ApplyIntensity(red, 0.5f);

        Assert.That(result.R, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.G, Is.EqualTo(0f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0f).Within(0.001f));
    }

    // === Opacity tests ===

    [Test]
    public void FullOpacity_AlphaIsOne()
    {
        var color = new Color(1f, 0f, 0f);
        var result = ChamsLogic.ApplyIntensityAndOpacity(color, 1.0f, 1.0f);

        Assert.That(result.A, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void HalfOpacity_AlphaIsHalf()
    {
        var color = new Color(1f, 0f, 0f);
        var result = ChamsLogic.ApplyIntensityAndOpacity(color, 1.0f, 0.5f);

        Assert.That(result.A, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void MinOpacity_ClampedToTenPercent()
    {
        var color = new Color(1f, 1f, 1f);
        var result = ChamsLogic.ApplyIntensityAndOpacity(color, 1.0f, 0.0f);

        Assert.That(result.A, Is.EqualTo(0.1f).Within(0.001f), "Should clamp to 0.1 minimum");
    }

    [Test]
    public void NegativeOpacity_ClampedToTenPercent()
    {
        var color = new Color(1f, 0f, 0f);
        var result = ChamsLogic.ApplyIntensityAndOpacity(color, 1.0f, -3f);

        Assert.That(result.A, Is.EqualTo(0.1f).Within(0.001f));
    }

    [Test]
    public void OverOneOpacity_ClampedToOne()
    {
        var color = new Color(1f, 0f, 0f);
        var result = ChamsLogic.ApplyIntensityAndOpacity(color, 1.0f, 5.0f);

        Assert.That(result.A, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void Opacity_OverridesOriginalAlpha()
    {
        // Original color has alpha 0.8, but opacity should override it
        var color = new Color(1f, 0f, 0f, 0.8f);
        var result = ChamsLogic.ApplyIntensityAndOpacity(color, 1.0f, 0.3f);

        Assert.That(result.A, Is.EqualTo(0.3f).Within(0.001f));
    }

    [Test]
    public void Opacity_DoesNotAffectRGB()
    {
        var color = new Color(1f, 0.5f, 0.25f);
        var result = ChamsLogic.ApplyIntensityAndOpacity(color, 1.0f, 0.3f);

        Assert.That(result.R, Is.EqualTo(1f).Within(0.001f));
        Assert.That(result.G, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0.25f).Within(0.001f));
    }

    [Test]
    public void IntensityAndOpacity_BothApplied()
    {
        var color = new Color(1f, 1f, 1f);
        var result = ChamsLogic.ApplyIntensityAndOpacity(color, 0.5f, 0.7f);

        Assert.That(result.R, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.G, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.A, Is.EqualTo(0.7f).Within(0.001f));
    }

    [Test]
    public void DefaultOpacity_FullyOpaque()
    {
        // Config default for opacity is 1.0
        var color = new Color(1f, 0f, 0f);
        var result = ChamsLogic.ApplyIntensityAndOpacity(color, 0.5f, 1.0f);

        Assert.That(result.A, Is.EqualTo(1f).Within(0.001f));
        Assert.That(result.R, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void MinIntensityMinOpacity_BothClamped()
    {
        var color = new Color(1f, 1f, 1f);
        var result = ChamsLogic.ApplyIntensityAndOpacity(color, 0.0f, 0.0f);

        Assert.That(result.R, Is.EqualTo(0.1f).Within(0.001f));
        Assert.That(result.G, Is.EqualTo(0.1f).Within(0.001f));
        Assert.That(result.B, Is.EqualTo(0.1f).Within(0.001f));
        Assert.That(result.A, Is.EqualTo(0.1f).Within(0.001f));
    }
}
