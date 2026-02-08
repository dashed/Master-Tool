using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using MasterTool.Config;
using MasterTool.Core;
using MasterTool.Models;
using MasterTool.Utils;
using UnityEngine;
using Color = UnityEngine.Color;

namespace MasterTool.ESP
{
    /// <summary>
    /// Scans and renders ESP overlays for loose loot items and items inside containers.
    /// Supports name/ID filtering and configurable distance limits.
    /// Call <see cref="Update"/> each frame, then <see cref="Render"/> during OnGUI.
    /// </summary>
    public class ItemEsp
    {
        public List<ItemEspTarget> Targets { get; } = new List<ItemEspTarget>();

        private readonly HashSet<string> _wishlistIds = new HashSet<string>();

        private static int _losLayerMask = -1;

        private static void InitLayerMask()
        {
            if (_losLayerMask != -1)
                return;
            int hp = LayerMask.NameToLayer("HighPolyCollider");
            int lp = LayerMask.NameToLayer("LowPolyCollider");
            int terrain = LayerMask.NameToLayer("Terrain");
            _losLayerMask = EspLogic.ComputeLayerMask(hp, lp, terrain);
        }

        internal static bool HasLineOfSightToPosition(Camera camera, Vector3 destination)
        {
            Vector3 origin = camera.transform.position;
            origin += (destination - origin).normalized * 0.15f;
            InitLayerMask();
            return !Physics.Linecast(origin, destination, _losLayerMask, QueryTriggerInteraction.Ignore);
        }

        public LootableContainer[] CachedContainers { get; private set; }

        private float _nextUpdate;
        private float _nextContainerCacheRefresh;
        private const float ContainerCacheInterval = 300.0f;

        /// <summary>
        /// Scans loose loot and container items, applies name/ID filters, and populates
        /// <see cref="Targets"/> with screen-space data. Throttled by <see cref="PluginConfig.ItemEspUpdateInterval"/>.
        /// </summary>
        /// <param name="gameWorld">The current game world instance.</param>
        /// <param name="mainCamera">The active camera for world-to-screen projection.</param>
        /// <param name="localPlayer">The local player used for distance calculations.</param>
        public void Update(GameWorld gameWorld, Camera mainCamera, Player localPlayer)
        {
            if (Time.time < _nextUpdate)
                return;
            _nextUpdate = Time.time + PluginConfig.ItemEspUpdateInterval.Value;

            Targets.Clear();
            if (gameWorld == null || mainCamera == null)
                return;

            _wishlistIds.Clear();
            if (PluginConfig.ItemEspWishlistOnly.Value)
            {
                try
                {
                    var wm = localPlayer.Profile.WishlistManager;
                    if (wm != null)
                    {
                        var wishlist = wm.GetWishlist();
                        if (wishlist != null)
                        {
                            foreach (var kvp in wishlist)
                                _wishlistIds.Add((string)kvp.Key);
                        }
                    }
                }
                catch (Exception) { }
            }

            string[] filters = PluginConfig
                .ItemEspFilter.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim().ToLower())
                .ToArray();

            if (PluginConfig.ItemEspEnabled.Value)
            {
                var lootItems = gameWorld.LootItems;
                if (lootItems != null)
                {
                    for (int i = 0; i < lootItems.Count; i++)
                    {
                        var loot = lootItems.GetByIndex(i);
                        if (loot == null || loot.Item == null)
                            continue;
                        ProcessLoot(
                            loot.transform.position,
                            loot.Item,
                            PluginConfig.ColorItem.Value,
                            filters,
                            mainCamera,
                            localPlayer,
                            _wishlistIds
                        );
                    }
                }
            }

            if (PluginConfig.ContainerEspEnabled.Value)
            {
                if (CachedContainers == null || Time.time >= _nextContainerCacheRefresh)
                {
                    CachedContainers = UnityEngine.Object.FindObjectsOfType<LootableContainer>();
                    _nextContainerCacheRefresh = Time.time + ContainerCacheInterval;
                }

                Vector3 playerPos = localPlayer.Transform.position;
                float maxDistSq = PluginConfig.ItemEspMaxDistance.Value * PluginConfig.ItemEspMaxDistance.Value;

                foreach (var container in CachedContainers)
                {
                    if (container == null)
                        continue;
                    Vector3 containerPos = container.transform.position;
                    float distSq = (containerPos - playerPos).sqrMagnitude;
                    if (distSq > maxDistSq)
                        continue;
                    if (container.ItemOwner == null || container.ItemOwner.RootItem == null)
                        continue;

                    var items = container.ItemOwner.RootItem.GetAllItems();
                    foreach (var item in items)
                    {
                        if (item == container.ItemOwner.RootItem)
                            continue;
                        ProcessLoot(
                            containerPos,
                            item,
                            PluginConfig.ColorContainer.Value,
                            filters,
                            mainCamera,
                            localPlayer,
                            _wishlistIds,
                            true
                        );
                    }
                }
            }
        }

        private void ProcessLoot(
            Vector3 pos,
            Item item,
            Color color,
            string[] filters,
            Camera mainCamera,
            Player localPlayer,
            HashSet<string> wishlistIds,
            bool isContainer = false
        )
        {
            float dist = Vector3.Distance(localPlayer.Transform.position, pos);
            if (dist > PluginConfig.ItemEspMaxDistance.Value)
                return;

            if (PluginConfig.ItemEspLineOfSightOnly.Value && !HasLineOfSightToPosition(mainCamera, pos))
                return;

            string name = item.ShortName.Localized();
            string id = item.TemplateId;

            bool matchesFilter = filters.Length == 0;
            if (!matchesFilter)
            {
                foreach (var f in filters)
                {
                    if (name.ToLower().Contains(f) || id.ToLower().Contains(f))
                    {
                        matchesFilter = true;
                        break;
                    }
                }
            }

            bool isInWishlist = wishlistIds.Contains(id);
            if (!WishlistLogic.ShouldShowItem(PluginConfig.ItemEspWishlistOnly.Value, isInWishlist, matchesFilter))
                return;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(EspLogic.GetItemEspWorldPosition(pos.ToVec3()).ToVector3());
            if (screenPos.z > 0)
            {
                screenPos.y = Screen.height - screenPos.y;
                if (!EspRenderer.IsOnScreen(screenPos.x, screenPos.y, Screen.width, Screen.height, 50f))
                    return;
                Targets.Add(
                    new ItemEspTarget
                    {
                        ScreenPosition = screenPos,
                        Distance = dist,
                        Name = isContainer ? $"[C] {name}" : name,
                        Color = color,
                    }
                );
            }
        }

        /// <summary>
        /// Draws item name and distance labels for each tracked item target.
        /// Must be called from OnGUI.
        /// </summary>
        /// <param name="style">The <see cref="GUIStyle"/> used for item ESP text rendering.</param>
        public void Render(GUIStyle style)
        {
            foreach (var target in Targets)
            {
                string text = $"{target.Name}\n{target.Distance:F1}m";
                EspRenderer.DrawTextWithShadow(target.ScreenPosition, text, target.Color, style);
            }
        }
    }
}
