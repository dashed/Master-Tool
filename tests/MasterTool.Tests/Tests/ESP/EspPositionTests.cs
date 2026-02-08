using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP;

/// <summary>
/// Tests for ESP world position calculation logic.
/// Duplicates the pure GetEspWorldPosition method since Unity cannot be referenced from net9.0 tests.
/// </summary>
[TestFixture]
public class EspPositionTests
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
    /// Duplicates PlayerEsp.GetEspWorldPosition: prefers head bone position (+ 0.2 Y offset)
    /// over transform position (+ 1.8 Y offset).
    /// </summary>
    private static Vec3 GetEspWorldPosition(Vec3? headBonePos, Vec3 transformPos)
    {
        if (headBonePos.HasValue)
        {
            return new Vec3(headBonePos.Value.x, headBonePos.Value.y + 0.2f, headBonePos.Value.z);
        }

        return new Vec3(transformPos.x, transformPos.y + 1.8f, transformPos.z);
    }

    /// <summary>
    /// Duplicates ItemEsp item position offset: adds 0.5 Y to lift labels above ground.
    /// </summary>
    private static Vec3 GetItemEspWorldPosition(Vec3 itemPos)
    {
        return new Vec3(itemPos.x, itemPos.y + 0.5f, itemPos.z);
    }

    // --- Player ESP position tests ---

    [Test]
    public void HeadBoneAvailable_UsesHeadBonePosition()
    {
        var headBone = new Vec3(10, 5, 20);
        var transform = new Vec3(10, 0, 20);
        var result = GetEspWorldPosition(headBone, transform);
        Assert.That(result.y, Is.EqualTo(5.2f).Within(0.01f));
    }

    [Test]
    public void HeadBoneAvailable_AddsSmallYOffset()
    {
        var headBone = new Vec3(0, 10, 0);
        var result = GetEspWorldPosition(headBone, new Vec3(0, 0, 0));
        // 0.2f above head bone
        Assert.That(result.y, Is.EqualTo(10.2f).Within(0.01f));
    }

    [Test]
    public void HeadBoneAvailable_PreservesXZ()
    {
        var headBone = new Vec3(15, 8, 25);
        var result = GetEspWorldPosition(headBone, new Vec3(15, 0, 25));
        Assert.That(result.x, Is.EqualTo(15f));
        Assert.That(result.z, Is.EqualTo(25f));
    }

    [Test]
    public void NoHeadBone_UsesTransformWithLargeOffset()
    {
        var result = GetEspWorldPosition(null, new Vec3(10, 0, 20));
        Assert.That(result.y, Is.EqualTo(1.8f).Within(0.01f));
    }

    [Test]
    public void NoHeadBone_FallbackPreservesXZ()
    {
        var result = GetEspWorldPosition(null, new Vec3(50, 10, 100));
        Assert.That(result.x, Is.EqualTo(50f));
        Assert.That(result.z, Is.EqualTo(100f));
    }

    [Test]
    public void NoHeadBone_FallbackAdds1Point8()
    {
        var result = GetEspWorldPosition(null, new Vec3(0, -5, 0));
        Assert.That(result.y, Is.EqualTo(-3.2f).Within(0.01f));
    }

    [Test]
    public void HeadBoneOffset_SmallerThanFallbackOffset()
    {
        // Head bone adds 0.2, fallback adds 1.8
        var headBone = new Vec3(0, 0, 0);
        var headResult = GetEspWorldPosition(headBone, new Vec3(0, 0, 0));
        var fallbackResult = GetEspWorldPosition(null, new Vec3(0, 0, 0));
        Assert.That(headResult.y, Is.EqualTo(0.2f).Within(0.01f));
        Assert.That(fallbackResult.y, Is.EqualTo(1.8f).Within(0.01f));
        Assert.That(headResult.y, Is.LessThan(fallbackResult.y));
    }

    [Test]
    public void HeadBoneAtDifferentPositionThanTransform()
    {
        // Head bone can be at a different XZ than transform (e.g., leaning)
        var headBone = new Vec3(12, 8, 22);
        var transform = new Vec3(10, 0, 20);
        var result = GetEspWorldPosition(headBone, transform);
        Assert.That(result.x, Is.EqualTo(12f));
        Assert.That(result.z, Is.EqualTo(22f));
    }

    // --- Item ESP position tests ---

    [Test]
    public void ItemPosition_AddsHalfMeterOffset()
    {
        var result = GetItemEspWorldPosition(new Vec3(10, 0, 20));
        Assert.That(result.y, Is.EqualTo(0.5f).Within(0.01f));
    }

    [Test]
    public void ItemPosition_PreservesXZ()
    {
        var result = GetItemEspWorldPosition(new Vec3(50, 3, 100));
        Assert.That(result.x, Is.EqualTo(50f));
        Assert.That(result.z, Is.EqualTo(100f));
    }

    [Test]
    public void ItemPosition_NegativeY_StillAddsOffset()
    {
        var result = GetItemEspWorldPosition(new Vec3(0, -2, 0));
        Assert.That(result.y, Is.EqualTo(-1.5f).Within(0.01f));
    }

    [Test]
    public void ItemPosition_OffsetIsConsistent()
    {
        var pos1 = GetItemEspWorldPosition(new Vec3(0, 0, 0));
        var pos2 = GetItemEspWorldPosition(new Vec3(0, 100, 0));
        float offset1 = pos1.y - 0f;
        float offset2 = pos2.y - 100f;
        Assert.That(offset1, Is.EqualTo(offset2).Within(0.01f));
        Assert.That(offset1, Is.EqualTo(0.5f).Within(0.01f));
    }
}
