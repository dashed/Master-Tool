using BepInEx.Configuration;
using EFT.HealthSystem;
using UnityEngine;

namespace MasterTool.Config
{
    /// <summary>
    /// Central configuration store holding all BepInEx config entries for the plugin.
    /// All fields are populated during <see cref="Initialize"/> and read by feature modules at runtime.
    /// </summary>
    public static class PluginConfig
    {
        /// <summary>
        /// Cached reference to the local player's health controller, set by MasterToolPlugin.
        /// Used by DamagePatches to identify which health controller belongs to the local player.
        /// </summary>
        public static ActiveHealthController LocalActiveHealthController;

        // --- General Settings ---
        public static ConfigEntry<bool> GodModeEnabled;
        public static ConfigEntry<bool> InfiniteStaminaEnabled;
        public static ConfigEntry<bool> NoWeightEnabled;
        public static ConfigEntry<bool> InfiniteEnergyEnabled;
        public static ConfigEntry<bool> InfiniteHydrationEnabled;
        public static ConfigEntry<bool> NoFallDamageEnabled;
        public static ConfigEntry<bool> CodModeEnabled;
        public static ConfigEntry<float> CodModeHealRate;
        public static ConfigEntry<float> CodModeHealDelay;
        public static ConfigEntry<bool> CodModeRemoveEffects;
        public static ConfigEntry<bool> ReloadSpeedEnabled;
        public static ConfigEntry<float> ReloadLoadTime;
        public static ConfigEntry<float> ReloadUnloadTime;

        // --- Fly Mode ---
        public static ConfigEntry<bool> FlyModeEnabled;
        public static ConfigEntry<float> FlySpeed;

        public static ConfigEntry<int> DamageReductionPercent;
        public static ConfigEntry<bool> Keep1HealthEnabled;
        public static ConfigEntry<string> Keep1HealthSelection;
        public static ConfigEntry<bool> IgnoreHeadshots;
        public static ConfigEntry<int> HeadDamagePercent;
        public static ConfigEntry<float> EnemyDamageMultiplier;
        public static ConfigEntry<int> WeightPercent;
        public static ConfigEntry<bool> StatusWindowEnabled;
        public static ConfigEntry<bool> ShowWeaponInfo;
        public static ConfigEntry<KeyboardShortcut> ToggleWeaponInfoHotkey;

        // --- Hotkeys ---
        public static ConfigEntry<KeyboardShortcut> ToggleUiHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleStatusHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleGodModeHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleStaminaHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleWeightHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleEspHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleItemEspHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleContainerEspHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleCullingHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleUnlockDoorsHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleQuestEspHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleEnergyHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleHydrationHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleFallDamageHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleCodModeHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleReloadSpeedHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleFlyModeHotkey;
        public static ConfigEntry<KeyboardShortcut> SavePositionHotkey;
        public static ConfigEntry<KeyboardShortcut> LoadPositionHotkey;
        public static ConfigEntry<KeyboardShortcut> SurfaceTeleportHotkey;

        // --- Player ESP Settings ---
        public static ConfigEntry<bool> EspEnabled;
        public static ConfigEntry<float> EspUpdateInterval;
        public static ConfigEntry<float> EspMaxDistance;
        public static ConfigEntry<bool> EspLineOfSightOnly;
        public static ConfigEntry<float> EspTextAlpha;
        public static ConfigEntry<int> EspFontSize;
        public static ConfigEntry<Color> ColorBear;
        public static ConfigEntry<Color> ColorUsec;
        public static ConfigEntry<Color> ColorSavage;
        public static ConfigEntry<Color> ColorBoss;
        public static ConfigEntry<bool> ChamsEnabled;
        public static ConfigEntry<float> ChamsIntensity;
        public static ConfigEntry<KeyboardShortcut> ToggleChamsHotkey;
        public static ConfigEntry<bool> LootChamsEnabled;
        public static ConfigEntry<Color> LootChamsColor;

        // --- Movement ---
        public static ConfigEntry<bool> SpeedhackEnabled;
        public static ConfigEntry<float> SpeedMultiplier;

        // --- Visual ---
        public static ConfigEntry<bool> ThermalVisionEnabled;
        public static ConfigEntry<bool> NightVisionEnabled;
        public static ConfigEntry<bool> BigHeadModeEnabled;
        public static ConfigEntry<float> HeadSizeMultiplier;

        // --- Item & Container ESP Settings ---
        public static ConfigEntry<bool> ItemEspEnabled;
        public static ConfigEntry<bool> ContainerEspEnabled;
        public static ConfigEntry<string> ItemEspFilter;
        public static ConfigEntry<float> ItemEspMaxDistance;
        public static ConfigEntry<float> ItemEspUpdateInterval;
        public static ConfigEntry<Color> ColorItem;
        public static ConfigEntry<Color> ColorContainer;
        public static ConfigEntry<int> ItemEspFontSize;

        // --- Quest ESP Settings ---
        public static ConfigEntry<bool> QuestEspEnabled;
        public static ConfigEntry<float> QuestEspMaxDistance;
        public static ConfigEntry<Color> ColorQuestItem;
        public static ConfigEntry<Color> ColorQuestZone;
        public static ConfigEntry<int> QuestEspFps;

        // --- FOV by Weapon Category Settings ---
        public static ConfigEntry<bool> WeaponFovEnabled;
        public static ConfigEntry<float> FovPistol;
        public static ConfigEntry<float> FovSMG;
        public static ConfigEntry<float> FovAssaultRifle;
        public static ConfigEntry<float> FovShotgun;
        public static ConfigEntry<float> FovSniper;
        public static ConfigEntry<float> FovMelee;
        public static ConfigEntry<float> FovDefault;

        // --- Performance Settings ---
        public static ConfigEntry<bool> PerformanceMode;
        public static ConfigEntry<float> BotRenderDistance;

        /// <summary>
        /// Binds all configuration entries to the given BepInEx config file.
        /// Must be called once during plugin startup before any feature reads config values.
        /// </summary>
        /// <param name="config">The BepInEx <see cref="ConfigFile"/> provided by the plugin.</param>
        public static void Initialize(ConfigFile config)
        {
            string hotkeyDesc = "Hotkey to toggle this feature. Use Unity KeyCode names (e.g., Keypad0, Keypad1, Insert, Home, PageUp).";

            // General Binds
            GodModeEnabled = config.Bind("General", "GodMode", false, "Player takes no damage.");
            InfiniteStaminaEnabled = config.Bind("General", "Infinite Stamina", false, "Unlimited stamina and breath.");
            NoWeightEnabled = config.Bind("General", "No Weight", false, "Removes weight penalties.");
            InfiniteEnergyEnabled = config.Bind("General", "Infinite Energy", false, "Energy never drains.");
            InfiniteHydrationEnabled = config.Bind("General", "Infinite Hydration", false, "Hydration never drains.");
            NoFallDamageEnabled = config.Bind("General", "No Fall Damage", false, "Eliminates fall damage.");
            CodModeEnabled = config.Bind("General", "COD Mode", false, "Auto-heal after not taking damage.");
            CodModeHealRate = config.Bind(
                "General",
                "COD Mode Heal Rate",
                10f,
                new ConfigDescription("HP healed per cycle (every 60 frames).", new AcceptableValueRange<float>(1f, 100f))
            );
            CodModeHealDelay = config.Bind(
                "General",
                "COD Mode Heal Delay",
                10f,
                new ConfigDescription("Seconds after last damage before healing starts.", new AcceptableValueRange<float>(0f, 600f))
            );
            CodModeRemoveEffects = config.Bind(
                "General",
                "COD Mode Remove Effects",
                false,
                "Auto-remove bleeds and fractures during heal (experimental)."
            );
            ReloadSpeedEnabled = config.Bind("General", "Reload Speed", false, "Adjust magazine reload speed.");
            ReloadLoadTime = config.Bind(
                "General",
                "Reload Load Time",
                0.85f,
                new ConfigDescription("Magazine load time (lower = faster, default 0.85).", new AcceptableValueRange<float>(0.01f, 2f))
            );
            ReloadUnloadTime = config.Bind(
                "General",
                "Reload Unload Time",
                0.3f,
                new ConfigDescription("Magazine unload time (lower = faster, default 0.3).", new AcceptableValueRange<float>(0.01f, 2f))
            );
            FlyModeEnabled = config.Bind("General", "Fly Mode", false, "Noclip flight mode — move freely through walls and terrain.");
            FlySpeed = config.Bind(
                "General",
                "Fly Speed",
                10f,
                new ConfigDescription("Flight speed in fly mode.", new AcceptableValueRange<float>(1f, 50f))
            );
            DamageReductionPercent = config.Bind(
                "General",
                "Damage Reduction %",
                100,
                new ConfigDescription(
                    "Percentage of damage received (100 = full, 0 = none). Only applies when GodMode is off.",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
            Keep1HealthEnabled = config.Bind(
                "General",
                "Keep 1 Health",
                false,
                "Prevent body parts from being destroyed by clamping HP above 3."
            );
            Keep1HealthSelection = config.Bind(
                "General",
                "Keep 1 Health Selection",
                "All",
                new ConfigDescription("Which body parts to protect.", new AcceptableValueList<string>("All", "Head And Thorax"))
            );
            IgnoreHeadshots = config.Bind("General", "Ignore Headshots", false, "Completely ignore headshot damage.");
            HeadDamagePercent = config.Bind(
                "General",
                "Head Damage %",
                100,
                new ConfigDescription(
                    "Percentage of headshot damage received (100 = full, 0 = none).",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
            EnemyDamageMultiplier = config.Bind(
                "General",
                "Enemy Damage Multiplier",
                1f,
                new ConfigDescription(
                    "Multiply damage dealt to enemies (1 = normal, 20 = 20x damage).",
                    new AcceptableValueRange<float>(1f, 20f)
                )
            );
            WeightPercent = config.Bind(
                "General",
                "Weight Percent",
                0,
                new ConfigDescription(
                    "Weight percentage when No Weight is enabled (0 = weightless, 50 = half weight).",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
            StatusWindowEnabled = config.Bind("General", "Status Window", true, "Show the mini status window.");

            ShowWeaponInfo = config.Bind("General", "Show Weapon Info", true, "Show current weapon and ammo in status window.");
            ToggleWeaponInfoHotkey = config.Bind(
                "Hotkeys",
                "12. Toggle Weapon Info",
                new KeyboardShortcut(KeyCode.L),
                "Hotkey to toggle weapon info display."
            );

            // Hotkeys
            ToggleUiHotkey = config.Bind("Hotkeys", "01. Toggle UI", new KeyboardShortcut(KeyCode.Insert), hotkeyDesc);
            ToggleStatusHotkey = config.Bind("Hotkeys", "02. Toggle Status Window", new KeyboardShortcut(KeyCode.Keypad0), hotkeyDesc);
            ToggleGodModeHotkey = config.Bind("Hotkeys", "03. Toggle GodMode", new KeyboardShortcut(KeyCode.Keypad1), hotkeyDesc);
            ToggleStaminaHotkey = config.Bind("Hotkeys", "04. Toggle Stamina", new KeyboardShortcut(KeyCode.Keypad2), hotkeyDesc);
            ToggleWeightHotkey = config.Bind("Hotkeys", "06. Toggle Weight", new KeyboardShortcut(KeyCode.Keypad4), hotkeyDesc);
            ToggleEspHotkey = config.Bind("Hotkeys", "07. Toggle Player ESP", new KeyboardShortcut(KeyCode.Keypad5), hotkeyDesc);
            ToggleItemEspHotkey = config.Bind("Hotkeys", "08. Toggle Item ESP", new KeyboardShortcut(KeyCode.Keypad6), hotkeyDesc);
            ToggleContainerEspHotkey = config.Bind(
                "Hotkeys",
                "09. Toggle Container ESP",
                new KeyboardShortcut(KeyCode.Keypad7),
                hotkeyDesc
            );
            ToggleCullingHotkey = config.Bind("Hotkeys", "10. Toggle Culling", new KeyboardShortcut(KeyCode.Keypad8), hotkeyDesc);
            ToggleUnlockDoorsHotkey = config.Bind("Hotkeys", "11. Unlock All Doors", new KeyboardShortcut(KeyCode.Keypad9), hotkeyDesc);
            ToggleQuestEspHotkey = config.Bind("Hotkeys", "14. Toggle Quest ESP", new KeyboardShortcut(KeyCode.Keypad3), hotkeyDesc);
            ToggleEnergyHotkey = config.Bind("Hotkeys", "15. Toggle Energy", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            ToggleHydrationHotkey = config.Bind("Hotkeys", "16. Toggle Hydration", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            ToggleFallDamageHotkey = config.Bind("Hotkeys", "17. Toggle Fall Damage", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            ToggleCodModeHotkey = config.Bind("Hotkeys", "18. Toggle COD Mode", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            ToggleReloadSpeedHotkey = config.Bind("Hotkeys", "19. Toggle Reload Speed", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            ToggleFlyModeHotkey = config.Bind("Hotkeys", "20. Toggle Fly Mode", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            SavePositionHotkey = config.Bind("Hotkeys", "21. Save Position", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            LoadPositionHotkey = config.Bind("Hotkeys", "22. Load Position", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            SurfaceTeleportHotkey = config.Bind("Hotkeys", "23. Teleport to Surface", new KeyboardShortcut(KeyCode.None), hotkeyDesc);

            // Movement
            SpeedhackEnabled = config.Bind("Movement", "Speedhack", false, "Move faster.");
            SpeedMultiplier = config.Bind("Movement", "Speed Multiplier", 2f, "Speed multiplier.");

            // Visuals
            ThermalVisionEnabled = config.Bind("Visuals", "Thermal Vision", false, "Thermal vision.");
            NightVisionEnabled = config.Bind("Visuals", "Night Vision", false, "Toggle Night Vision.");
            BigHeadModeEnabled = config.Bind("Visuals", "Big Head Mode", false, "Enlarge enemy heads.");
            HeadSizeMultiplier = config.Bind("Visuals", "Head Size", 3f, "How big the heads should be.");

            // Player ESP
            EspEnabled = config.Bind("ESP Players", "Enabled", false, "Show players/bots.");
            EspTextAlpha = config.Bind("ESP Players", "Text Alpha", 1.0f, "Text Alpha.");
            EspFontSize = config.Bind("ESP Players", "Font Size", 12, "Text Size.");
            EspUpdateInterval = config.Bind("ESP Players", "Update Interval", 0.05f, "Update rate for player ESP.");
            EspMaxDistance = config.Bind("ESP Players", "Max Distance", 400f, "Max distance for players.");
            EspLineOfSightOnly = config.Bind(
                "ESP Players",
                "Line of Sight Only",
                false,
                "Only show ESP labels for players you have direct line of sight to."
            );
            ColorBear = config.Bind("ESP Players", "Color BEAR", Color.red, "Color for BEAR faction.");
            ColorUsec = config.Bind("ESP Players", "Color USEC", Color.blue, "Color for USEC faction.");
            ColorSavage = config.Bind("ESP Players", "Color Savage", Color.yellow, "Color for Scavs/Bots.");
            ColorBoss = config.Bind("ESP Players", "Color Boss", new Color(0.5f, 0f, 0.5f), "Color for Bosses.");

            ChamsEnabled = config.Bind("ESP Players", "Chams Enabled", false, "Enable colored models.");
            ChamsIntensity = config.Bind("ESP Players", "Chams Intensity", 0.5f, "Brightness of Chams colors (0.1 to 1.0).");
            ToggleChamsHotkey = config.Bind("Hotkeys", "13. Toggle Chams", new KeyboardShortcut(KeyCode.K), "Hotkey to toggle Chams.");

            LootChamsEnabled = config.Bind(
                "ESP Items",
                "Loot Chams Enabled",
                false,
                "Enable colored overlays on loot items for through-wall visibility."
            );
            LootChamsColor = config.Bind("ESP Items", "Loot Chams Color", Color.green, "Color for loot item chams.");

            // Item & Container ESP
            ItemEspEnabled = config.Bind("ESP Items", "Enabled", false, "Show loose loot.");
            ContainerEspEnabled = config.Bind("ESP Containers", "Enabled", false, "Show items inside containers.");
            ItemEspFilter = config.Bind("ESP Items", "Filter", "", "Filter by name or ID (comma separated).");
            ItemEspMaxDistance = config.Bind("ESP Items", "Max Distance", 100f, "Max distance for items.");
            ItemEspUpdateInterval = config.Bind("ESP Items", "Update Interval", 0.5f, "Update rate for item ESP.");
            ColorItem = config.Bind("ESP Items", "Color", Color.green, "Color for loose items.");
            ColorContainer = config.Bind("ESP Containers", "Color", new Color(1f, 0.5f, 0f), "Color for container items.");
            ItemEspFontSize = config.Bind("ESP Items", "Font Size", 10, "Itens Text Size.");

            // Quest ESP
            QuestEspEnabled = config.Bind("ESP Quests", "Enabled", false, "Show quest items and objectives.");
            QuestEspMaxDistance = config.Bind("ESP Quests", "Max Distance", 200f, "Max distance for quest ESP.");
            ColorQuestItem = config.Bind("ESP Quests", "Quest Item Color", new Color(1f, 0.84f, 0f), "Color for quest items (gold).");
            ColorQuestZone = config.Bind("ESP Quests", "Quest Zone Color", new Color(0f, 1f, 0.5f), "Color for quest zones (green).");
            QuestEspFps = config.Bind(
                "ESP Quests",
                "Update FPS",
                15,
                new ConfigDescription(
                    "Update rate in frames per second. Higher is smoother but uses more resources.",
                    new AcceptableValueRange<int>(1, 60)
                )
            );

            // FOV by Weapon Category
            WeaponFovEnabled = config.Bind("FOV Settings", "Enable Weapon FOV", false, "Automatically adjust FOV based on weapon type.");
            FovDefault = config.Bind(
                "FOV Settings",
                "Default FOV",
                75f,
                new ConfigDescription("FOV when no weapon equipped.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovPistol = config.Bind(
                "FOV Settings",
                "Pistol FOV",
                65f,
                new ConfigDescription("FOV for pistols.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovSMG = config.Bind(
                "FOV Settings",
                "SMG FOV",
                70f,
                new ConfigDescription("FOV for SMGs.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovAssaultRifle = config.Bind(
                "FOV Settings",
                "Assault Rifle FOV",
                75f,
                new ConfigDescription("FOV for assault rifles.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovShotgun = config.Bind(
                "FOV Settings",
                "Shotgun FOV",
                70f,
                new ConfigDescription("FOV for shotguns.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovSniper = config.Bind(
                "FOV Settings",
                "Sniper FOV",
                80f,
                new ConfigDescription("FOV for sniper/marksman rifles.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovMelee = config.Bind(
                "FOV Settings",
                "Melee FOV",
                60f,
                new ConfigDescription("FOV for melee weapons.", new AcceptableValueRange<float>(50f, 120f))
            );

            // Performance
            PerformanceMode = config.Bind("Performance", "Enable Distance Culling", true, "Only render bots within distance.");
            BotRenderDistance = config.Bind("Performance", "Bot Render Distance", 500f, "Distance to stop rendering bots.");
        }
    }
}
