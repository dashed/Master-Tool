using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using MasterTool.Config;
using MasterTool.Models;
using UnityEngine;

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
                        ProcessLoot(loot.transform.position, loot.Item, PluginConfig.ColorItem.Value, filters, mainCamera, localPlayer);
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
                        ProcessLoot(containerPos, item, PluginConfig.ColorContainer.Value, filters, mainCamera, localPlayer, true);
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
            bool isContainer = false
        )
        {
            float dist = Vector3.Distance(localPlayer.Transform.position, pos);
            if (dist > PluginConfig.ItemEspMaxDistance.Value)
                return;

            string name = item.ShortName.Localized();
            string id = item.TemplateId;

            bool matches = filters.Length == 0;
            if (!matches)
            {
                foreach (var f in filters)
                {
                    if (name.ToLower().Contains(f) || id.ToLower().Contains(f))
                    {
                        matches = true;
                        break;
                    }
                }
            }
            if (!matches)
                return;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(pos + Vector3.up * 0.5f);
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
