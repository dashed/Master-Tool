using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for EspRenderer.IsOnScreen screen bounds validation logic.
/// Duplicates the pure helper since Unity cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class EspScreenBoundsTests
{
    /// <summary>
    /// Duplicates EspRenderer.IsOnScreen: checks if screen coordinates are within
    /// screen bounds plus a margin to filter extreme off-screen positions.
    /// </summary>
    private static bool IsOnScreen(float x, float y, float screenWidth, float screenHeight, float margin)
    {
        return x >= -margin && x <= screenWidth + margin && y >= -margin && y <= screenHeight + margin;
    }

    private const float W = 1920f;
    private const float H = 1080f;
    private const float M = 50f;

    // --- Center of screen ---

    [Test]
    public void CenterOfScreen_IsOnScreen()
    {
        Assert.That(IsOnScreen(960, 540, W, H, M), Is.True);
    }

    // --- Corners ---

    [Test]
    public void TopLeft_IsOnScreen()
    {
        Assert.That(IsOnScreen(0, 0, W, H, M), Is.True);
    }

    [Test]
    public void BottomRight_IsOnScreen()
    {
        Assert.That(IsOnScreen(W, H, W, H, M), Is.True);
    }

    // --- Within margin ---

    [Test]
    public void SlightlyOffLeftEdge_WithinMargin_IsOnScreen()
    {
        Assert.That(IsOnScreen(-30, 540, W, H, M), Is.True);
    }

    [Test]
    public void SlightlyOffRightEdge_WithinMargin_IsOnScreen()
    {
        Assert.That(IsOnScreen(W + 30, 540, W, H, M), Is.True);
    }

    [Test]
    public void SlightlyOffTopEdge_WithinMargin_IsOnScreen()
    {
        Assert.That(IsOnScreen(960, -30, W, H, M), Is.True);
    }

    [Test]
    public void SlightlyOffBottomEdge_WithinMargin_IsOnScreen()
    {
        Assert.That(IsOnScreen(960, H + 30, W, H, M), Is.True);
    }

    // --- Beyond margin ---

    [Test]
    public void FarOffLeft_BeyondMargin_IsNotOnScreen()
    {
        Assert.That(IsOnScreen(-100, 540, W, H, M), Is.False);
    }

    [Test]
    public void FarOffRight_BeyondMargin_IsNotOnScreen()
    {
        Assert.That(IsOnScreen(W + 100, 540, W, H, M), Is.False);
    }

    [Test]
    public void FarOffTop_BeyondMargin_IsNotOnScreen()
    {
        Assert.That(IsOnScreen(960, -100, W, H, M), Is.False);
    }

    [Test]
    public void FarOffBottom_BeyondMargin_IsNotOnScreen()
    {
        Assert.That(IsOnScreen(960, H + 100, W, H, M), Is.False);
    }

    // --- Extreme values (the actual glitch case) ---

    [Test]
    public void ExtremeNegativeX_IsNotOnScreen()
    {
        Assert.That(IsOnScreen(-5000, 540, W, H, M), Is.False);
    }

    [Test]
    public void ExtremePositiveX_IsNotOnScreen()
    {
        Assert.That(IsOnScreen(10000, 540, W, H, M), Is.False);
    }

    [Test]
    public void ExtremeNegativeY_IsNotOnScreen()
    {
        Assert.That(IsOnScreen(960, -5000, W, H, M), Is.False);
    }

    [Test]
    public void ExtremePositiveY_IsNotOnScreen()
    {
        Assert.That(IsOnScreen(960, 10000, W, H, M), Is.False);
    }

    // --- Exact margin boundary ---

    [Test]
    public void ExactlyAtMarginEdge_IsOnScreen()
    {
        Assert.That(IsOnScreen(-M, 540, W, H, M), Is.True);
        Assert.That(IsOnScreen(W + M, 540, W, H, M), Is.True);
    }

    [Test]
    public void JustBeyondMarginEdge_IsNotOnScreen()
    {
        Assert.That(IsOnScreen(-M - 1, 540, W, H, M), Is.False);
        Assert.That(IsOnScreen(W + M + 1, 540, W, H, M), Is.False);
    }

    // --- Zero margin ---

    [Test]
    public void ZeroMargin_OnEdge_IsOnScreen()
    {
        Assert.That(IsOnScreen(0, 0, W, H, 0), Is.True);
        Assert.That(IsOnScreen(W, H, W, H, 0), Is.True);
    }

    [Test]
    public void ZeroMargin_SlightlyOff_IsNotOnScreen()
    {
        Assert.That(IsOnScreen(-1, 0, W, H, 0), Is.False);
    }
}
