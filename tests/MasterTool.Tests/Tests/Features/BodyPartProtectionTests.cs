using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

[TestFixture]
public class BodyPartProtectionTests
{
    // --- All mode ---

    [Test]
    public void All_ProtectsHead()
    {
        Assert.That(BodyPartProtection.ShouldProtect("All", BodyPart.Head, null), Is.True);
    }

    [Test]
    public void All_ProtectsChest()
    {
        Assert.That(BodyPartProtection.ShouldProtect("All", BodyPart.Chest, null), Is.True);
    }

    [Test]
    public void All_ProtectsStomach()
    {
        Assert.That(BodyPartProtection.ShouldProtect("All", BodyPart.Stomach, null), Is.True);
    }

    [Test]
    public void All_ProtectsLeftArm()
    {
        Assert.That(BodyPartProtection.ShouldProtect("All", BodyPart.LeftArm, null), Is.True);
    }

    [Test]
    public void All_ProtectsRightArm()
    {
        Assert.That(BodyPartProtection.ShouldProtect("All", BodyPart.RightArm, null), Is.True);
    }

    [Test]
    public void All_ProtectsLeftLeg()
    {
        Assert.That(BodyPartProtection.ShouldProtect("All", BodyPart.LeftLeg, null), Is.True);
    }

    [Test]
    public void All_ProtectsRightLeg()
    {
        Assert.That(BodyPartProtection.ShouldProtect("All", BodyPart.RightLeg, null), Is.True);
    }

    // --- HeadAndThorax mode ---

    [Test]
    public void HeadAndThorax_ProtectsHead()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Head And Thorax", BodyPart.Head, null), Is.True);
    }

    [Test]
    public void HeadAndThorax_ProtectsChest()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Head And Thorax", BodyPart.Chest, null), Is.True);
    }

    [Test]
    public void HeadAndThorax_DoesNotProtectStomach()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Head And Thorax", BodyPart.Stomach, null), Is.False);
    }

    [Test]
    public void HeadAndThorax_DoesNotProtectLeftArm()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Head And Thorax", BodyPart.LeftArm, null), Is.False);
    }

    [Test]
    public void HeadAndThorax_DoesNotProtectRightArm()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Head And Thorax", BodyPart.RightArm, null), Is.False);
    }

    [Test]
    public void HeadAndThorax_DoesNotProtectLeftLeg()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Head And Thorax", BodyPart.LeftLeg, null), Is.False);
    }

    [Test]
    public void HeadAndThorax_DoesNotProtectRightLeg()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Head And Thorax", BodyPart.RightLeg, null), Is.False);
    }

    // --- Vitals mode ---

    [Test]
    public void Vitals_ProtectsHead()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Vitals", BodyPart.Head, null), Is.True);
    }

    [Test]
    public void Vitals_ProtectsChest()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Vitals", BodyPart.Chest, null), Is.True);
    }

    [Test]
    public void Vitals_ProtectsStomach()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Vitals", BodyPart.Stomach, null), Is.True);
    }

    [Test]
    public void Vitals_DoesNotProtectLeftArm()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Vitals", BodyPart.LeftArm, null), Is.False);
    }

    [Test]
    public void Vitals_DoesNotProtectRightArm()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Vitals", BodyPart.RightArm, null), Is.False);
    }

    [Test]
    public void Vitals_DoesNotProtectLeftLeg()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Vitals", BodyPart.LeftLeg, null), Is.False);
    }

    [Test]
    public void Vitals_DoesNotProtectRightLeg()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Vitals", BodyPart.RightLeg, null), Is.False);
    }

    // --- Custom mode ---

    [Test]
    public void Custom_AllTrue_ProtectsAll()
    {
        var custom = new[] { true, true, true, true, true, true, true };
        foreach (BodyPart part in System.Enum.GetValues(typeof(BodyPart)))
        {
            Assert.That(BodyPartProtection.ShouldProtect("Custom", part, custom), Is.True, $"Expected {part} to be protected");
        }
    }

    [Test]
    public void Custom_AllFalse_ProtectsNone()
    {
        var custom = new[] { false, false, false, false, false, false, false };
        foreach (BodyPart part in System.Enum.GetValues(typeof(BodyPart)))
        {
            Assert.That(BodyPartProtection.ShouldProtect("Custom", part, custom), Is.False, $"Expected {part} to not be protected");
        }
    }

    [Test]
    public void Custom_Mixed_OnlySelectedProtected()
    {
        // Only Head and Chest enabled
        var custom = new[] { true, true, false, false, false, false, false };
        Assert.That(BodyPartProtection.ShouldProtect("Custom", BodyPart.Head, custom), Is.True);
        Assert.That(BodyPartProtection.ShouldProtect("Custom", BodyPart.Chest, custom), Is.True);
        Assert.That(BodyPartProtection.ShouldProtect("Custom", BodyPart.Stomach, custom), Is.False);
        Assert.That(BodyPartProtection.ShouldProtect("Custom", BodyPart.LeftArm, custom), Is.False);
        Assert.That(BodyPartProtection.ShouldProtect("Custom", BodyPart.RightLeg, custom), Is.False);
    }

    [Test]
    public void Custom_NullArray_ReturnsFalse()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Custom", BodyPart.Head, null), Is.False);
    }

    [Test]
    public void Custom_ShortArray_ReturnsFalseForOutOfBounds()
    {
        // Array only has 3 elements, LeftArm (index 3) is out of bounds
        var custom = new[] { true, true, true };
        Assert.That(BodyPartProtection.ShouldProtect("Custom", BodyPart.LeftArm, custom), Is.False);
        Assert.That(BodyPartProtection.ShouldProtect("Custom", BodyPart.RightLeg, custom), Is.False);
    }

    // --- Unknown selection ---

    [Test]
    public void UnknownSelection_ReturnsFalse()
    {
        Assert.That(BodyPartProtection.ShouldProtect("Nonsense", BodyPart.Head, null), Is.False);
    }

    // --- CycleSelection ---

    [Test]
    public void CycleSelection_FullCycle()
    {
        string current = "All";
        current = BodyPartProtection.CycleSelection(current);
        Assert.That(current, Is.EqualTo("Head And Thorax"));

        current = BodyPartProtection.CycleSelection(current);
        Assert.That(current, Is.EqualTo("Vitals"));

        current = BodyPartProtection.CycleSelection(current);
        Assert.That(current, Is.EqualTo("Custom"));

        current = BodyPartProtection.CycleSelection(current);
        Assert.That(current, Is.EqualTo("All"));
    }

    [Test]
    public void CycleSelection_UnknownReturnsAll()
    {
        Assert.That(BodyPartProtection.CycleSelection("Unknown"), Is.EqualTo("All"));
    }
}
