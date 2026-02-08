using NUnit.Framework;

namespace MasterTool.Tests.Tests.Utils;

[TestFixture]
public class FovMappingTests
{
    // Default FOV values matching PluginConfig defaults
    private const float FovPistol = 60f;
    private const float FovSMG = 65f;
    private const float FovAssaultRifle = 70f;
    private const float FovShotgun = 55f;
    private const float FovSniper = 50f;
    private const float FovDefault = 75f;

    /// <summary>
    /// Standalone copy of the weapon class to FOV mapping logic
    /// from VisionFeature.GetFovForCurrentWeapon, using fixed config values.
    /// </summary>
    private static float MapWeaponClassToFov(string weaponClass)
    {
        switch (weaponClass?.ToLower())
        {
            case "pistol":
                return FovPistol;
            case "smg":
                return FovSMG;
            case "assaultrifle":
            case "assaultcarbine":
                return FovAssaultRifle;
            case "shotgun":
                return FovShotgun;
            case "marksmanrifle":
            case "sniperrifle":
                return FovSniper;
            case "machinegun":
                return FovAssaultRifle;
            default:
                return FovDefault;
        }
    }

    [TestCase("pistol", FovPistol)]
    [TestCase("Pistol", FovPistol)]
    [TestCase("PISTOL", FovPistol)]
    public void MapWeaponClassToFov_Pistol_ReturnsPistolFov(string input, float expected)
    {
        Assert.That(MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("smg", FovSMG)]
    [TestCase("SMG", FovSMG)]
    public void MapWeaponClassToFov_Smg_ReturnsSmgFov(string input, float expected)
    {
        Assert.That(MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("assaultrifle", FovAssaultRifle)]
    [TestCase("AssaultRifle", FovAssaultRifle)]
    [TestCase("assaultcarbine", FovAssaultRifle)]
    [TestCase("AssaultCarbine", FovAssaultRifle)]
    public void MapWeaponClassToFov_AssaultRifle_ReturnsAssaultRifleFov(string input, float expected)
    {
        Assert.That(MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("shotgun", FovShotgun)]
    [TestCase("Shotgun", FovShotgun)]
    public void MapWeaponClassToFov_Shotgun_ReturnsShotgunFov(string input, float expected)
    {
        Assert.That(MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("marksmanrifle", FovSniper)]
    [TestCase("MarksmanRifle", FovSniper)]
    [TestCase("sniperrifle", FovSniper)]
    [TestCase("SniperRifle", FovSniper)]
    public void MapWeaponClassToFov_Sniper_ReturnsSniperFov(string input, float expected)
    {
        Assert.That(MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("machinegun", FovAssaultRifle)]
    [TestCase("MachineGun", FovAssaultRifle)]
    public void MapWeaponClassToFov_MachineGun_ReturnsAssaultRifleFov(string input, float expected)
    {
        Assert.That(MapWeaponClassToFov(input), Is.EqualTo(expected));
    }

    [TestCase("flamethrower")]
    [TestCase("grenade_launcher")]
    [TestCase("unknownweapon")]
    [TestCase("")]
    public void MapWeaponClassToFov_UnknownWeapon_ReturnsDefaultFov(string input)
    {
        Assert.That(MapWeaponClassToFov(input), Is.EqualTo(FovDefault));
    }

    [Test]
    public void MapWeaponClassToFov_Null_ReturnsDefaultFov()
    {
        Assert.That(MapWeaponClassToFov(null), Is.EqualTo(FovDefault));
    }
}
