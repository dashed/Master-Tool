namespace MasterTool.Core;

public static class VisionLogic
{
    // Default FOV values matching PluginConfig defaults
    public const float FovPistol = 60f;
    public const float FovSmg = 65f;
    public const float FovAssaultRifle = 70f;
    public const float FovShotgun = 55f;
    public const float FovSniper = 50f;
    public const float FovDefault = 75f;
    public const float FovMelee = 60f;

    /// <summary>
    /// Returns true if the mod should override the camera FOV this frame.
    /// </summary>
    public static bool ShouldOverrideFov(bool fovEnabled, bool isAiming, bool overrideAds)
    {
        if (!fovEnabled)
        {
            return false;
        }

        if (isAiming && !overrideAds)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Maps a weapon class string to the corresponding FOV value using default config values.
    /// </summary>
    public static float MapWeaponClassToFov(string weaponClass, bool isMelee = false)
    {
        return MapWeaponClassToFov(weaponClass, isMelee, FovPistol, FovSmg, FovAssaultRifle, FovShotgun, FovSniper, FovDefault, FovMelee);
    }

    /// <summary>
    /// Maps a weapon class string to the corresponding FOV value using the provided config values.
    /// </summary>
    public static float MapWeaponClassToFov(
        string weaponClass,
        bool isMelee,
        float fovPistol,
        float fovSmg,
        float fovAssaultRifle,
        float fovShotgun,
        float fovSniper,
        float fovDefault,
        float fovMelee
    )
    {
        if (isMelee)
        {
            return fovMelee;
        }

        switch (weaponClass?.ToLower())
        {
            case "pistol":
                return fovPistol;
            case "smg":
                return fovSmg;
            case "assaultrifle":
            case "assaultcarbine":
                return fovAssaultRifle;
            case "shotgun":
                return fovShotgun;
            case "marksmanrifle":
            case "sniperrifle":
                return fovSniper;
            case "machinegun":
                return fovAssaultRifle;
            default:
                return fovDefault;
        }
    }
}
