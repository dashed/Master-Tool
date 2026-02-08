using System;
using BepInEx.Configuration;
using EFT.HealthSystem;
using MasterTool.Core;
using MasterTool.Plugin;
using UnityEngine;
using Color = UnityEngine.Color;
using KeyCode = UnityEngine.KeyCode;

namespace MasterTool.Config
{
    /// <summary>
    /// Central configuration store holding all BepInEx config entries for the plugin.
    /// All fields are populated during <see cref="Initialize"/> and read by feature modules at runtime.
    ///
    /// Sections use numbered prefixes ("01. Damage", "02. Survival", etc.) to control
    /// display order in the BepInEx F12 configuration manager, which sorts alphabetically.
    /// </summary>
    public static class PluginConfig
    {
        /// <summary>
        /// Cached reference to the local player's health controller, set by MasterToolPlugin.
        /// Used by DamagePatches to identify which health controller belongs to the local player.
        /// </summary>
        public static ActiveHealthController LocalActiveHealthController;

        // --- 01. Damage ---
        public static ConfigEntry<bool> GodModeEnabled;
        public static ConfigEntry<int> DamageReductionPercent;
        public static ConfigEntry<bool> Keep1HealthEnabled;
        public static ConfigEntry<string> Keep1HealthSelection;

        // Per-body-part protection for Custom mode
        public static ConfigEntry<bool> ProtectHead;
        public static ConfigEntry<bool> ProtectChest;
        public static ConfigEntry<bool> ProtectStomach;
        public static ConfigEntry<bool> ProtectLeftArm;
        public static ConfigEntry<bool> ProtectRightArm;
        public static ConfigEntry<bool> ProtectLeftLeg;
        public static ConfigEntry<bool> ProtectRightLeg;

        public static ConfigEntry<bool> IgnoreHeadshots;
        public static ConfigEntry<int> HeadDamagePercent;
        public static ConfigEntry<float> EnemyDamageMultiplier;

        // --- 02. Survival ---
        public static ConfigEntry<bool> InfiniteStaminaEnabled;
        public static ConfigEntry<bool> NoWeightEnabled;
        public static ConfigEntry<int> WeightPercent;
        public static ConfigEntry<bool> InfiniteEnergyEnabled;
        public static ConfigEntry<bool> InfiniteHydrationEnabled;
        public static ConfigEntry<bool> NoFallDamageEnabled;

        // --- 03. Healing ---
        public static ConfigEntry<bool> CodModeEnabled;
        public static ConfigEntry<float> CodModeHealRate;
        public static ConfigEntry<float> CodModeHealDelay;
        public static ConfigEntry<bool> CodModeRemoveEffects;

        // --- 04. Weapons ---
        public static ConfigEntry<bool> ReloadSpeedEnabled;
        public static ConfigEntry<float> ReloadLoadTime;
        public static ConfigEntry<float> ReloadUnloadTime;

        // --- 05. Movement ---
        public static ConfigEntry<bool> SpeedhackEnabled;
        public static ConfigEntry<float> SpeedMultiplier;
        public static ConfigEntry<bool> FlyModeEnabled;
        public static ConfigEntry<float> FlySpeed;

        // --- 06. FOV ---
        public static ConfigEntry<bool> WeaponFovEnabled;
        public static ConfigEntry<float> FovDefault;
        public static ConfigEntry<float> FovPistol;
        public static ConfigEntry<float> FovSMG;
        public static ConfigEntry<float> FovAssaultRifle;
        public static ConfigEntry<float> FovShotgun;
        public static ConfigEntry<float> FovSniper;
        public static ConfigEntry<float> FovMelee;
        public static ConfigEntry<bool> FovOverrideAds;

        // --- 07. ESP Players ---
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

        // --- 08. Chams ---
        public static ConfigEntry<bool> ChamsEnabled;
        public static ConfigEntry<ChamsMode> ChamsRenderMode;
        public static ConfigEntry<float> ChamsIntensity;
        public static ConfigEntry<float> ChamsOpacity;
        public static ConfigEntry<float> OutlineScale;
        public static ConfigEntry<float> ChamsMaxDistance;
        public static ConfigEntry<bool> LootChamsEnabled;
        public static ConfigEntry<ChamsMode> LootChamsRenderMode;
        public static ConfigEntry<Color> LootChamsColor;
        public static ConfigEntry<float> LootChamsMaxDistance;

        // --- 09. ESP Items ---
        public static ConfigEntry<bool> ItemEspEnabled;
        public static ConfigEntry<bool> ContainerEspEnabled;
        public static ConfigEntry<string> ItemEspFilter;
        public static ConfigEntry<float> ItemEspMaxDistance;
        public static ConfigEntry<float> ItemEspUpdateInterval;
        public static ConfigEntry<Color> ColorItem;
        public static ConfigEntry<Color> ColorContainer;
        public static ConfigEntry<int> ItemEspFontSize;
        public static ConfigEntry<bool> ItemEspLineOfSightOnly;
        public static ConfigEntry<bool> ItemEspWishlistOnly;

        // --- 10. ESP Quests ---
        public static ConfigEntry<bool> QuestEspEnabled;
        public static ConfigEntry<float> QuestEspMaxDistance;
        public static ConfigEntry<Color> ColorQuestItem;
        public static ConfigEntry<Color> ColorQuestZone;
        public static ConfigEntry<int> QuestEspFps;
        public static ConfigEntry<bool> QuestEspLineOfSightOnly;

        // --- 11. Visual ---
        public static ConfigEntry<bool> ThermalVisionEnabled;
        public static ConfigEntry<bool> NightVisionEnabled;
        public static ConfigEntry<bool> BigHeadModeEnabled;
        public static ConfigEntry<float> HeadSizeMultiplier;

        // --- 12. Performance ---
        public static ConfigEntry<bool> PerformanceMode;
        public static ConfigEntry<float> BotRenderDistance;

        // --- 13. UI ---
        public static ConfigEntry<bool> StatusWindowEnabled;
        public static ConfigEntry<bool> ShowWeaponInfo;

        // --- 14. Hotkeys ---
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
        public static ConfigEntry<KeyboardShortcut> ToggleWeaponInfoHotkey;
        public static ConfigEntry<KeyboardShortcut> ToggleChamsHotkey;
        public static ConfigEntry<KeyboardShortcut> SavePositionHotkey;
        public static ConfigEntry<KeyboardShortcut> LoadPositionHotkey;
        public static ConfigEntry<KeyboardShortcut> SurfaceTeleportHotkey;

        /// <summary>
        /// Section name constants for BepInEx config organization.
        /// Numbered prefixes control alphabetical sort order in the F12 config manager.
        /// </summary>
        public static class Sections
        {
            public const string ModMenu = ConfigSections.ModMenu;
            public const string Damage = ConfigSections.Damage;
            public const string Survival = ConfigSections.Survival;
            public const string Healing = ConfigSections.Healing;
            public const string Weapons = ConfigSections.Weapons;
            public const string Movement = ConfigSections.Movement;
            public const string Fov = ConfigSections.Fov;
            public const string EspPlayers = ConfigSections.EspPlayers;
            public const string Chams = ConfigSections.Chams;
            public const string EspItems = ConfigSections.EspItems;
            public const string EspQuests = ConfigSections.EspQuests;
            public const string Visual = ConfigSections.Visual;
            public const string Performance = ConfigSections.Performance;
            public const string Ui = ConfigSections.Ui;
            public const string Hotkeys = ConfigSections.Hotkeys;
        }

        /// <summary>
        /// Binds all configuration entries to the given BepInEx config file.
        /// Must be called once during plugin startup before any feature reads config values.
        /// </summary>
        /// <param name="config">The BepInEx <see cref="ConfigFile"/> provided by the plugin.</param>
        public static void Initialize(ConfigFile config)
        {
            string hotkeyDesc = "Hotkey to toggle this feature. Use Unity KeyCode names (e.g., Keypad0, Keypad1, Insert, Home, PageUp).";

            // --- 00. Mod Menu ---
            config.Bind(
                Sections.ModMenu,
                "Open Mod Menu",
                false,
                new ConfigDescription(
                    "Click the button to open the in-game mod menu.",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        CustomDrawer = DrawOpenModMenuButton,
                        HideDefaultButton = true,
                        Order = int.MaxValue,
                    }
                )
            );

            // --- 01. Damage ---
            GodModeEnabled = config.Bind(Sections.Damage, "GodMode", false, "Player takes no damage.");
            DamageReductionPercent = config.Bind(
                Sections.Damage,
                "Damage Reduction %",
                100,
                new ConfigDescription(
                    "Percentage of damage received (100 = full, 0 = none). Only applies when GodMode is off.",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
            Keep1HealthEnabled = config.Bind(
                Sections.Damage,
                "Keep 1 Health",
                false,
                "Prevent body parts from being destroyed by clamping HP above 3."
            );
            Keep1HealthSelection = config.Bind(
                Sections.Damage,
                "Keep 1 Health Selection",
                "All",
                new ConfigDescription(
                    "Which body parts to protect.",
                    new AcceptableValueList<string>("All", "Head And Thorax", "Vitals", "Custom")
                )
            );
            ProtectHead = config.Bind(Sections.Damage, "Protect Head", true, "Protect Head in Custom mode.");
            ProtectChest = config.Bind(Sections.Damage, "Protect Chest", true, "Protect Chest (Thorax) in Custom mode.");
            ProtectStomach = config.Bind(Sections.Damage, "Protect Stomach", true, "Protect Stomach in Custom mode.");
            ProtectLeftArm = config.Bind(Sections.Damage, "Protect Left Arm", true, "Protect Left Arm in Custom mode.");
            ProtectRightArm = config.Bind(Sections.Damage, "Protect Right Arm", true, "Protect Right Arm in Custom mode.");
            ProtectLeftLeg = config.Bind(Sections.Damage, "Protect Left Leg", true, "Protect Left Leg in Custom mode.");
            ProtectRightLeg = config.Bind(Sections.Damage, "Protect Right Leg", true, "Protect Right Leg in Custom mode.");
            IgnoreHeadshots = config.Bind(Sections.Damage, "Ignore Headshots", false, "Completely ignore headshot damage.");
            HeadDamagePercent = config.Bind(
                Sections.Damage,
                "Head Damage %",
                100,
                new ConfigDescription(
                    "Percentage of headshot damage received (100 = full, 0 = none).",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
            EnemyDamageMultiplier = config.Bind(
                Sections.Damage,
                "Enemy Damage Multiplier",
                1f,
                new ConfigDescription(
                    "Multiply damage dealt to enemies (1 = normal, 20 = 20x damage).",
                    new AcceptableValueRange<float>(1f, 20f)
                )
            );

            // --- 02. Survival ---
            InfiniteStaminaEnabled = config.Bind(Sections.Survival, "Infinite Stamina", false, "Unlimited stamina and breath.");
            NoWeightEnabled = config.Bind(Sections.Survival, "No Weight", false, "Removes weight penalties.");
            WeightPercent = config.Bind(
                Sections.Survival,
                "Weight Percent",
                0,
                new ConfigDescription(
                    "Weight percentage when No Weight is enabled (0 = weightless, 50 = half weight).",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
            InfiniteEnergyEnabled = config.Bind(Sections.Survival, "Infinite Energy", false, "Energy never drains.");
            InfiniteHydrationEnabled = config.Bind(Sections.Survival, "Infinite Hydration", false, "Hydration never drains.");
            NoFallDamageEnabled = config.Bind(Sections.Survival, "No Fall Damage", false, "Eliminates fall damage.");

            // --- 03. Healing ---
            CodModeEnabled = config.Bind(Sections.Healing, "COD Mode", false, "Auto-heal after not taking damage.");
            CodModeHealRate = config.Bind(
                Sections.Healing,
                "COD Mode Heal Rate",
                10f,
                new ConfigDescription("HP healed per cycle (every 60 frames).", new AcceptableValueRange<float>(1f, 100f))
            );
            CodModeHealDelay = config.Bind(
                Sections.Healing,
                "COD Mode Heal Delay",
                10f,
                new ConfigDescription("Seconds after last damage before healing starts.", new AcceptableValueRange<float>(0f, 600f))
            );
            CodModeRemoveEffects = config.Bind(
                Sections.Healing,
                "COD Mode Remove Effects",
                false,
                "Auto-remove bleeds and fractures during heal (experimental)."
            );

            // --- 04. Weapons ---
            ReloadSpeedEnabled = config.Bind(Sections.Weapons, "Reload Speed", false, "Adjust magazine reload speed.");
            ReloadLoadTime = config.Bind(
                Sections.Weapons,
                "Reload Load Time",
                0.85f,
                new ConfigDescription("Magazine load time (lower = faster, default 0.85).", new AcceptableValueRange<float>(0.01f, 2f))
            );
            ReloadUnloadTime = config.Bind(
                Sections.Weapons,
                "Reload Unload Time",
                0.3f,
                new ConfigDescription("Magazine unload time (lower = faster, default 0.3).", new AcceptableValueRange<float>(0.01f, 2f))
            );

            // --- 05. Movement ---
            SpeedhackEnabled = config.Bind(Sections.Movement, "Speedhack", false, "Move faster.");
            SpeedMultiplier = config.Bind(Sections.Movement, "Speed Multiplier", 2f, "Speed multiplier.");
            FlyModeEnabled = config.Bind(
                Sections.Movement,
                "Fly Mode",
                false,
                "Noclip flight mode — move freely through walls and terrain."
            );
            FlySpeed = config.Bind(
                Sections.Movement,
                "Fly Speed",
                10f,
                new ConfigDescription("Flight speed in fly mode.", new AcceptableValueRange<float>(1f, 50f))
            );

            // --- 06. FOV ---
            WeaponFovEnabled = config.Bind(Sections.Fov, "Enable Weapon FOV", false, "Automatically adjust FOV based on weapon type.");
            FovDefault = config.Bind(
                Sections.Fov,
                "Default FOV",
                75f,
                new ConfigDescription("FOV when no weapon equipped.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovPistol = config.Bind(
                Sections.Fov,
                "Pistol FOV",
                65f,
                new ConfigDescription("FOV for pistols.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovSMG = config.Bind(
                Sections.Fov,
                "SMG FOV",
                70f,
                new ConfigDescription("FOV for SMGs.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovAssaultRifle = config.Bind(
                Sections.Fov,
                "Assault Rifle FOV",
                75f,
                new ConfigDescription("FOV for assault rifles.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovShotgun = config.Bind(
                Sections.Fov,
                "Shotgun FOV",
                70f,
                new ConfigDescription("FOV for shotguns.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovSniper = config.Bind(
                Sections.Fov,
                "Sniper FOV",
                80f,
                new ConfigDescription("FOV for sniper/marksman rifles.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovMelee = config.Bind(
                Sections.Fov,
                "Melee FOV",
                60f,
                new ConfigDescription("FOV for melee weapons.", new AcceptableValueRange<float>(50f, 120f))
            );
            FovOverrideAds = config.Bind(
                Sections.Fov,
                "Override FOV During ADS",
                false,
                "Keep custom FOV even when aiming down sights. When OFF, ADS uses the game's native zoom."
            );

            // --- 07. ESP Players ---
            EspEnabled = config.Bind(Sections.EspPlayers, "Enabled", false, "Show players/bots.");
            EspTextAlpha = config.Bind(Sections.EspPlayers, "Text Alpha", 1.0f, "Text Alpha.");
            EspFontSize = config.Bind(Sections.EspPlayers, "Font Size", 12, "Text Size.");
            EspUpdateInterval = config.Bind(Sections.EspPlayers, "Update Interval", 0.05f, "Update rate for player ESP.");
            EspMaxDistance = config.Bind(Sections.EspPlayers, "Max Distance", 400f, "Max distance for players.");
            EspLineOfSightOnly = config.Bind(
                Sections.EspPlayers,
                "Line of Sight Only",
                false,
                "Only show ESP labels for players you have direct line of sight to."
            );
            ColorBear = config.Bind(Sections.EspPlayers, "Color BEAR", Color.red, "Color for BEAR faction.");
            ColorUsec = config.Bind(Sections.EspPlayers, "Color USEC", Color.blue, "Color for USEC faction.");
            ColorSavage = config.Bind(Sections.EspPlayers, "Color Savage", Color.yellow, "Color for Scavs/Bots.");
            ColorBoss = config.Bind(Sections.EspPlayers, "Color Boss", new Color(0.5f, 0f, 0.5f), "Color for Bosses.");

            // --- 08. Chams ---
            ChamsEnabled = config.Bind(Sections.Chams, "Player Chams Enabled", false, "Enable colored models on players/bots.");
            ChamsRenderMode = config.Bind(
                Sections.Chams,
                "Player Chams Mode",
                ChamsMode.Solid,
                "Rendering mode: Solid (flat color), CullFront (hollow silhouette), Outline (normal model + colored edge)."
            );
            ChamsIntensity = config.Bind(Sections.Chams, "Intensity", 0.5f, "Brightness of Chams colors (0.1 to 1.0).");
            ChamsOpacity = config.Bind(
                Sections.Chams,
                "Opacity",
                1f,
                "Opacity/transparency of Chams (0.1 = nearly invisible, 1.0 = fully opaque)."
            );
            OutlineScale = config.Bind(
                Sections.Chams,
                "Outline Scale",
                1.04f,
                new ConfigDescription(
                    "Scale factor for outline duplicate meshes. Larger = thicker outline.",
                    new AcceptableValueRange<float>(1.01f, 1.15f)
                )
            );
            ChamsMaxDistance = config.Bind(Sections.Chams, "Chams Max Distance", 300f, "Max distance for player chams.");
            LootChamsEnabled = config.Bind(
                Sections.Chams,
                "Loot Chams Enabled",
                false,
                "Enable colored overlays on loot items for through-wall visibility."
            );
            LootChamsRenderMode = config.Bind(
                Sections.Chams,
                "Loot Chams Mode",
                ChamsMode.Solid,
                "Rendering mode for loot chams: Solid (flat color), CullFront (hollow silhouette), Outline (normal model + colored edge)."
            );
            LootChamsColor = config.Bind(Sections.Chams, "Loot Chams Color", Color.green, "Color for loot item chams.");
            LootChamsMaxDistance = config.Bind(Sections.Chams, "Loot Chams Max Distance", 100f, "Max distance for loot chams.");

            // --- 09. ESP Items ---
            ItemEspEnabled = config.Bind(Sections.EspItems, "Enabled", false, "Show loose loot.");
            ContainerEspEnabled = config.Bind(Sections.EspItems, "Container ESP Enabled", false, "Show items inside containers.");
            ItemEspFilter = config.Bind(Sections.EspItems, "Filter", "", "Filter by name or ID (comma separated).");
            ItemEspMaxDistance = config.Bind(Sections.EspItems, "Max Distance", 100f, "Max distance for items.");
            ItemEspUpdateInterval = config.Bind(Sections.EspItems, "Update Interval", 0.5f, "Update rate for item ESP.");
            ColorItem = config.Bind(Sections.EspItems, "Item Color", Color.green, "Color for loose items.");
            ColorContainer = config.Bind(Sections.EspItems, "Container Color", new Color(1f, 0.5f, 0f), "Color for container items.");
            ItemEspFontSize = config.Bind(Sections.EspItems, "Font Size", 10, "Item text size.");
            ItemEspLineOfSightOnly = config.Bind(
                Sections.EspItems,
                "Line of Sight Only",
                false,
                "Only show items visible to the camera (not behind walls)."
            );
            ItemEspWishlistOnly = config.Bind(Sections.EspItems, "Wishlist Only", false, "Only show items that are in your wishlist.");

            // --- 10. ESP Quests ---
            QuestEspEnabled = config.Bind(Sections.EspQuests, "Enabled", false, "Show quest items and objectives.");
            QuestEspMaxDistance = config.Bind(Sections.EspQuests, "Max Distance", 200f, "Max distance for quest ESP.");
            ColorQuestItem = config.Bind(Sections.EspQuests, "Quest Item Color", new Color(1f, 0.84f, 0f), "Color for quest items (gold).");
            ColorQuestZone = config.Bind(Sections.EspQuests, "Quest Zone Color", new Color(0f, 1f, 0.5f), "Color for quest zones (green).");
            QuestEspFps = config.Bind(
                Sections.EspQuests,
                "Update FPS",
                15,
                new ConfigDescription(
                    "Update rate in frames per second. Higher is smoother but uses more resources.",
                    new AcceptableValueRange<int>(1, 60)
                )
            );
            QuestEspLineOfSightOnly = config.Bind(
                Sections.EspQuests,
                "Line of Sight Only",
                false,
                "Only show quest items and zones visible to the camera (not behind walls)."
            );

            // --- 11. Visual ---
            ThermalVisionEnabled = config.Bind(Sections.Visual, "Thermal Vision", false, "Thermal vision.");
            NightVisionEnabled = config.Bind(Sections.Visual, "Night Vision", false, "Toggle Night Vision.");
            BigHeadModeEnabled = config.Bind(Sections.Visual, "Big Head Mode", false, "Enlarge enemy heads.");
            HeadSizeMultiplier = config.Bind(Sections.Visual, "Head Size", 3f, "How big the heads should be.");

            // --- 12. Performance ---
            PerformanceMode = config.Bind(Sections.Performance, "Enable Distance Culling", true, "Only render bots within distance.");
            BotRenderDistance = config.Bind(Sections.Performance, "Bot Render Distance", 500f, "Distance to stop rendering bots.");

            // --- 13. UI ---
            StatusWindowEnabled = config.Bind(Sections.Ui, "Status Window", true, "Show the mini status window.");
            ShowWeaponInfo = config.Bind(Sections.Ui, "Show Weapon Info", true, "Show current weapon and ammo in status window.");

            // --- 14. Hotkeys ---
            ToggleUiHotkey = config.Bind(Sections.Hotkeys, "01. Toggle UI", new KeyboardShortcut(KeyCode.Insert), hotkeyDesc);
            ToggleStatusHotkey = config.Bind(
                Sections.Hotkeys,
                "02. Toggle Status Window",
                new KeyboardShortcut(KeyCode.Keypad0),
                hotkeyDesc
            );
            ToggleGodModeHotkey = config.Bind(Sections.Hotkeys, "03. Toggle GodMode", new KeyboardShortcut(KeyCode.Keypad1), hotkeyDesc);
            ToggleStaminaHotkey = config.Bind(Sections.Hotkeys, "04. Toggle Stamina", new KeyboardShortcut(KeyCode.Keypad2), hotkeyDesc);
            ToggleWeightHotkey = config.Bind(Sections.Hotkeys, "06. Toggle Weight", new KeyboardShortcut(KeyCode.Keypad4), hotkeyDesc);
            ToggleEspHotkey = config.Bind(Sections.Hotkeys, "07. Toggle Player ESP", new KeyboardShortcut(KeyCode.Keypad5), hotkeyDesc);
            ToggleItemEspHotkey = config.Bind(Sections.Hotkeys, "08. Toggle Item ESP", new KeyboardShortcut(KeyCode.Keypad6), hotkeyDesc);
            ToggleContainerEspHotkey = config.Bind(
                Sections.Hotkeys,
                "09. Toggle Container ESP",
                new KeyboardShortcut(KeyCode.Keypad7),
                hotkeyDesc
            );
            ToggleCullingHotkey = config.Bind(Sections.Hotkeys, "10. Toggle Culling", new KeyboardShortcut(KeyCode.Keypad8), hotkeyDesc);
            ToggleUnlockDoorsHotkey = config.Bind(
                Sections.Hotkeys,
                "11. Unlock All Doors",
                new KeyboardShortcut(KeyCode.Keypad9),
                hotkeyDesc
            );
            ToggleWeaponInfoHotkey = config.Bind(
                Sections.Hotkeys,
                "12. Toggle Weapon Info",
                new KeyboardShortcut(KeyCode.L),
                "Hotkey to toggle weapon info display."
            );
            ToggleChamsHotkey = config.Bind(
                Sections.Hotkeys,
                "13. Toggle Chams",
                new KeyboardShortcut(KeyCode.K),
                "Hotkey to toggle Chams."
            );
            ToggleQuestEspHotkey = config.Bind(Sections.Hotkeys, "14. Toggle Quest ESP", new KeyboardShortcut(KeyCode.Keypad3), hotkeyDesc);
            ToggleEnergyHotkey = config.Bind(Sections.Hotkeys, "15. Toggle Energy", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            ToggleHydrationHotkey = config.Bind(Sections.Hotkeys, "16. Toggle Hydration", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            ToggleFallDamageHotkey = config.Bind(
                Sections.Hotkeys,
                "17. Toggle Fall Damage",
                new KeyboardShortcut(KeyCode.None),
                hotkeyDesc
            );
            ToggleCodModeHotkey = config.Bind(Sections.Hotkeys, "18. Toggle COD Mode", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            ToggleReloadSpeedHotkey = config.Bind(
                Sections.Hotkeys,
                "19. Toggle Reload Speed",
                new KeyboardShortcut(KeyCode.None),
                hotkeyDesc
            );
            ToggleFlyModeHotkey = config.Bind(Sections.Hotkeys, "20. Toggle Fly Mode", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            SavePositionHotkey = config.Bind(Sections.Hotkeys, "21. Save Position", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            LoadPositionHotkey = config.Bind(Sections.Hotkeys, "22. Load Position", new KeyboardShortcut(KeyCode.None), hotkeyDesc);
            SurfaceTeleportHotkey = config.Bind(
                Sections.Hotkeys,
                "23. Teleport to Surface",
                new KeyboardShortcut(KeyCode.None),
                hotkeyDesc
            );
        }

        public static bool[] GetCustomProtectionArray()
        {
            return new bool[]
            {
                ProtectHead.Value,
                ProtectChest.Value,
                ProtectStomach.Value,
                ProtectLeftArm.Value,
                ProtectRightArm.Value,
                ProtectLeftLeg.Value,
                ProtectRightLeg.Value,
            };
        }

        private static void DrawOpenModMenuButton(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Open Mod Menu", GUILayout.ExpandWidth(true)))
            {
                MasterToolPlugin.ToggleModMenu();
            }
        }
    }
}
