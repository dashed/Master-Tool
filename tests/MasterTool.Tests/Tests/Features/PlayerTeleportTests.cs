using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for player teleport save/load position state and ray origin calculation.
/// Duplicates the pure logic since Unity assemblies cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class PlayerTeleportTests
{
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
    }

    /// <summary>
    /// Simulates PlayerTeleportFeature saved position state.
    /// </summary>
    private Vec3? _savedPosition;

    private bool HasSavedPosition
    {
        get { return _savedPosition.HasValue; }
    }

    private void SavePosition(Vec3 position)
    {
        _savedPosition = position;
    }

    private Vec3? LoadPosition()
    {
        return _savedPosition;
    }

    private void ClearSavedPosition()
    {
        _savedPosition = null;
    }

    /// <summary>
    /// Duplicates CalculateRayOrigin: places the ray origin 500 units above
    /// the player position for ground-detection raycasting.
    /// </summary>
    private static Vec3 CalculateRayOrigin(Vec3 playerPosition)
    {
        return new Vec3(playerPosition.x, playerPosition.y + 500f, playerPosition.z);
    }

    [SetUp]
    public void SetUp()
    {
        _savedPosition = null;
    }

    [Test]
    public void InitialState_NoSavedPosition()
    {
        Assert.That(HasSavedPosition, Is.False);
    }

    [Test]
    public void SavePosition_SetsHasSavedPosition()
    {
        SavePosition(new Vec3(10, 20, 30));
        Assert.That(HasSavedPosition, Is.True);
    }

    [Test]
    public void SavePosition_StoresCorrectValues()
    {
        SavePosition(new Vec3(10, 20, 30));
        var loaded = LoadPosition();
        Assert.That(loaded.HasValue, Is.True);
        Assert.That(loaded.Value.x, Is.EqualTo(10f));
        Assert.That(loaded.Value.y, Is.EqualTo(20f));
        Assert.That(loaded.Value.z, Is.EqualTo(30f));
    }

    [Test]
    public void LoadPosition_WithoutSave_ReturnsNull()
    {
        var loaded = LoadPosition();
        Assert.That(loaded.HasValue, Is.False);
    }

    [Test]
    public void MultipleSaves_OverwritesPrevious()
    {
        SavePosition(new Vec3(1, 2, 3));
        SavePosition(new Vec3(10, 20, 30));
        var loaded = LoadPosition();
        Assert.That(loaded.Value.x, Is.EqualTo(10f));
    }

    [Test]
    public void ClearSavedPosition_RemovesSave()
    {
        SavePosition(new Vec3(10, 20, 30));
        ClearSavedPosition();
        Assert.That(HasSavedPosition, Is.False);
    }

    [Test]
    public void RayOrigin_IsAbovePlayer()
    {
        var origin = CalculateRayOrigin(new Vec3(10, -5, 30));
        Assert.That(origin.y, Is.EqualTo(495f));
        Assert.That(origin.x, Is.EqualTo(10f));
        Assert.That(origin.z, Is.EqualTo(30f));
    }

    [Test]
    public void RayOrigin_PreservesXZ()
    {
        var origin = CalculateRayOrigin(new Vec3(100, 50, 200));
        Assert.That(origin.x, Is.EqualTo(100f));
        Assert.That(origin.z, Is.EqualTo(200f));
    }

    [Test]
    public void RayOrigin_NegativeY_StillAdds500()
    {
        var origin = CalculateRayOrigin(new Vec3(0, -100, 0));
        Assert.That(origin.y, Is.EqualTo(400f));
    }

    [Test]
    public void SaveAndClearCycle_WorksCorrectly()
    {
        SavePosition(new Vec3(1, 2, 3));
        Assert.That(HasSavedPosition, Is.True);
        ClearSavedPosition();
        Assert.That(HasSavedPosition, Is.False);
        SavePosition(new Vec3(4, 5, 6));
        Assert.That(HasSavedPosition, Is.True);
        Assert.That(LoadPosition().Value.x, Is.EqualTo(4f));
    }
}
