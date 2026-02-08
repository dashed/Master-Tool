using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

[TestFixture]
public class SpeedhackTests
{
    [Test]
    public void Forward_Movement_Produces_Forward_Displacement()
    {
        var direction = new Vec3(0, 0, 1);
        var result = SpeedhackLogic.ComputeDisplacement(direction, 1f, 1f);
        Assert.That(result.z, Is.GreaterThan(0));
        Assert.That(result.x, Is.EqualTo(0).Within(0.01f));
        Assert.That(result.y, Is.EqualTo(0).Within(0.01f));
    }

    [Test]
    public void Zero_Direction_Produces_Zero_Displacement()
    {
        var direction = Vec3.Zero;
        var result = SpeedhackLogic.ComputeDisplacement(direction, 5f, 0.016f);
        Assert.That(result.x, Is.EqualTo(0).Within(0.01f));
        Assert.That(result.y, Is.EqualTo(0).Within(0.01f));
        Assert.That(result.z, Is.EqualTo(0).Within(0.01f));
    }

    [Test]
    public void Multiplier_Scales_Linearly()
    {
        var direction = new Vec3(0, 0, 1);
        var result1 = SpeedhackLogic.ComputeDisplacement(direction, 2f, 1f);
        var result2 = SpeedhackLogic.ComputeDisplacement(direction, 4f, 1f);
        Assert.That(result2.z, Is.EqualTo(result1.z * 2f).Within(0.01f));
    }

    [Test]
    public void DeltaTime_Scales_Linearly()
    {
        var direction = new Vec3(0, 0, 1);
        var result1 = SpeedhackLogic.ComputeDisplacement(direction, 1f, 0.016f);
        var result2 = SpeedhackLogic.ComputeDisplacement(direction, 1f, 0.032f);
        Assert.That(result2.z, Is.EqualTo(result1.z * 2f).Within(0.01f));
    }

    [Test]
    public void Internal_5x_Scaling_Factor()
    {
        var direction = new Vec3(1, 0, 0);
        var result = SpeedhackLogic.ComputeDisplacement(direction, 1f, 1f);
        // direction(1) * multiplier(1) * deltaTime(1) * 5 = 5
        Assert.That(result.x, Is.EqualTo(5f).Within(0.01f));
    }

    [Test]
    public void Backward_Direction_Produces_Negative_Displacement()
    {
        var direction = new Vec3(0, 0, -1);
        var result = SpeedhackLogic.ComputeDisplacement(direction, 1f, 1f);
        Assert.That(result.z, Is.LessThan(0));
    }

    [Test]
    public void Diagonal_Direction_Preserves_Direction()
    {
        var direction = new Vec3(1, 0, 1);
        var result = SpeedhackLogic.ComputeDisplacement(direction, 1f, 1f);
        // Both x and z should be positive and equal (same input components)
        Assert.That(result.x, Is.GreaterThan(0));
        Assert.That(result.z, Is.GreaterThan(0));
        Assert.That(result.x, Is.EqualTo(result.z).Within(0.01f));
    }

    [Test]
    public void Large_Multiplier_Produces_Large_Displacement()
    {
        var direction = new Vec3(0, 0, 1);
        var result = SpeedhackLogic.ComputeDisplacement(direction, 100f, 1f);
        // 1 * 100 * 1 * 5 = 500
        Assert.That(result.z, Is.EqualTo(500f).Within(0.01f));
    }

    [Test]
    public void Very_Small_DeltaTime_Produces_Near_Zero()
    {
        var direction = new Vec3(0, 0, 1);
        var result = SpeedhackLogic.ComputeDisplacement(direction, 1f, 0.0001f);
        // 1 * 1 * 0.0001 * 5 = 0.0005
        Assert.That(result.z, Is.EqualTo(0.0005f).Within(0.0001f));
    }

    [Test]
    public void Magnitude_Equals_Direction_Times_Multiplier_Times_DeltaTime_Times_5()
    {
        var direction = new Vec3(1, 2, 3);
        float multiplier = 3f;
        float deltaTime = 0.5f;
        var result = SpeedhackLogic.ComputeDisplacement(direction, multiplier, deltaTime);
        float expectedScale = multiplier * deltaTime * 5f;
        Assert.That(result.x, Is.EqualTo(direction.x * expectedScale).Within(0.01f));
        Assert.That(result.y, Is.EqualTo(direction.y * expectedScale).Within(0.01f));
        Assert.That(result.z, Is.EqualTo(direction.z * expectedScale).Within(0.01f));
    }
}
