using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Utils;

/// <summary>
/// Tests for the weapon class to FOV mapping logic.
/// Uses <see cref="VisionLogic"/> from MasterTool.Core.
/// </summary>
[TestFixture]
public class FovMappingTests
{
    [TestCase("pistol", VisionLogic.FovPistol)]
    [TestCase("Pistol", VisionLogic.FovPistol)]
    [TestCase("PISTOL", VisionLogic.FovPistol)]
    public void MapWeaponClassToFov_Pistol_ReturnsPistolFov(string input, float expected)
    {
        Assert.That(VisionLogic.MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("smg", VisionLogic.FovSmg)]
    [TestCase("SMG", VisionLogic.FovSmg)]
    public void MapWeaponClassToFov_Smg_ReturnsSmgFov(string input, float expected)
    {
        Assert.That(VisionLogic.MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("assaultrifle", VisionLogic.FovAssaultRifle)]
    [TestCase("AssaultRifle", VisionLogic.FovAssaultRifle)]
    [TestCase("assaultcarbine", VisionLogic.FovAssaultRifle)]
    [TestCase("AssaultCarbine", VisionLogic.FovAssaultRifle)]
    public void MapWeaponClassToFov_AssaultRifle_ReturnsAssaultRifleFov(string input, float expected)
    {
        Assert.That(VisionLogic.MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("shotgun", VisionLogic.FovShotgun)]
    [TestCase("Shotgun", VisionLogic.FovShotgun)]
    public void MapWeaponClassToFov_Shotgun_ReturnsShotgunFov(string input, float expected)
    {
        Assert.That(VisionLogic.MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("marksmanrifle", VisionLogic.FovSniper)]
    [TestCase("MarksmanRifle", VisionLogic.FovSniper)]
    [TestCase("sniperrifle", VisionLogic.FovSniper)]
    [TestCase("SniperRifle", VisionLogic.FovSniper)]
    public void MapWeaponClassToFov_Sniper_ReturnsSniperFov(string input, float expected)
    {
        Assert.That(VisionLogic.MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("machinegun", VisionLogic.FovAssaultRifle)]
    [TestCase("MachineGun", VisionLogic.FovAssaultRifle)]
    public void MapWeaponClassToFov_MachineGun_ReturnsAssaultRifleFov(string input, float expected)
    {
        Assert.That(VisionLogic.MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("flamethrower")]
    [TestCase("grenade_launcher")]
    [TestCase("unknownweapon")]
    [TestCase("")]
    public void MapWeaponClassToFov_UnknownWeapon_ReturnsDefaultFov(string input)
    {
        Assert.That(VisionLogic.MapWeaponClassToFov(input), Is.EqualTo(VisionLogic.FovDefault));
    }

    [Test]
    public void MapWeaponClassToFov_Null_ReturnsDefaultFov()
    {
        Assert.That(VisionLogic.MapWeaponClassToFov(null), Is.EqualTo(VisionLogic.FovDefault));
    }

    [Test]
    public void MapWeaponClassToFov_MeleeItem_ReturnsMeleeFov()
    {
        Assert.That(VisionLogic.MapWeaponClassToFov(null, isMelee: true), Is.EqualTo(VisionLogic.FovMelee));
    }

    [Test]
    public void MapWeaponClassToFov_MeleeWithWeaponClass_ReturnsMeleeFov()
    {
        // Melee flag takes priority over any weapClass
        Assert.That(VisionLogic.MapWeaponClassToFov("pistol", isMelee: true), Is.EqualTo(VisionLogic.FovMelee));
    }
}
