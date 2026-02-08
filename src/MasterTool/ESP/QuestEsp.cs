using System;
using System.Collections.Generic;
using EFT;
using EFT.Interactive;
using EFT.Quests;
using MasterTool.Config;
using MasterTool.Models;
using MasterTool.Plugin;
using MasterTool.Utils;
using UnityEngine;

namespace MasterTool.ESP
{
    /// <summary>
    /// Highlights items related to active quests by scanning loose loot and containers
    /// for template IDs that match current quest conditions.
    /// Call <see cref="Update"/> each frame, then <see cref="Render"/> during OnGUI.
    /// </summary>
    public class QuestEsp
    {
        public List<QuestEspTarget> Targets { get; } = new List<QuestEspTarget>();
        private float _nextUpdate;
        private bool _errorLogged;
        private TriggerWithId[] _cachedTriggers;
        private float _nextTriggerRefresh;

        /// <summary>
        /// Builds the set of quest-relevant item template IDs from the player's active quests,
        /// then scans loose loot and containers for matches. Throttled by <see cref="PluginConfig.QuestEspFps"/>.
        /// </summary>
        /// <param name="gameWorld">The current game world instance.</param>
        /// <param name="mainCamera">The active camera for world-to-screen projection.</param>
        /// <param name="localPlayer">The local player whose quest data is inspected.</param>
        /// <param name="cachedContainers">Pre-cached array of lootable containers from <see cref="ItemEsp"/>.</param>
        public void Update(GameWorld gameWorld, Camera mainCamera, Player localPlayer, LootableContainer[] cachedContainers)
        {
            if (Time.time < _nextUpdate)
                return;
            _nextUpdate = Time.time + (1f / PluginConfig.QuestEspFps.Value);

            Targets.Clear();
            if (gameWorld == null || mainCamera == null || localPlayer == null)
                return;

            try
            {
                var profile = localPlayer.Profile;
                if (profile == null)
                    return;

                var questsData = profile.QuestsData;
                if (questsData == null)
                    return;

                HashSet<string> questItemIds = new HashSet<string>();
                HashSet<string> questZoneIds = new HashSet<string>();

                foreach (var quest in questsData)
                {
                    if (quest == null)
                        continue;
                    bool isStarted = quest.Status == EFT.Quests.EQuestStatus.Started;
                    bool isReadyToFinish = quest.Status == EFT.Quests.EQuestStatus.AvailableForFinish;

                    if (!isStarted && !isReadyToFinish)
                        continue;

                    var template = quest.Template;
                    if (template?.Conditions == null)
                        continue;

                    foreach (var conditionGroup in template.Conditions)
                    {
                        if (conditionGroup.Value == null)
                            continue;
                        foreach (var condition in conditionGroup.Value)
                        {
                            var targetItems = ReflectionUtils.GetConditionTargetItems(condition);
                            foreach (var itemId in targetItems)
                            {
                                questItemIds.Add(itemId);
                            }

                            switch (condition)
                            {
                                case ConditionLeaveItemAtLocation loc:
                                    if (!string.IsNullOrEmpty(loc.zoneId))
                                        questZoneIds.Add(loc.zoneId);
                                    break;
                                case ConditionPlaceBeacon beacon:
                                    if (!string.IsNullOrEmpty(beacon.zoneId))
                                        questZoneIds.Add(beacon.zoneId);
                                    break;
                                case ConditionVisitPlace visit:
                                    if (!string.IsNullOrEmpty(visit.target))
                                        questZoneIds.Add(visit.target);
                                    break;
                                case ConditionLaunchFlare flare:
                                    if (!string.IsNullOrEmpty(flare.zoneID))
                                        questZoneIds.Add(flare.zoneID);
                                    break;
                            }
                        }
                    }
                }

                if (questItemIds.Count == 0 && questZoneIds.Count == 0)
                    return;

                ScanLooseItems(gameWorld, mainCamera, localPlayer, questItemIds);
                ScanContainers(mainCamera, localPlayer, cachedContainers, questItemIds);

                if (questZoneIds.Count > 0)
                    ScanZones(mainCamera, localPlayer, questZoneIds);
            }
            catch (Exception ex)
            {
                if (!_errorLogged)
                {
                    MasterToolPlugin.Log?.LogWarning($"[QuestESP] {ex.Message}");
                    _errorLogged = true;
                }
            }
        }

        private void ScanLooseItems(GameWorld gameWorld, Camera mainCamera, Player localPlayer, HashSet<string> questItemIds)
        {
            var lootItems = gameWorld.LootItems;
            if (lootItems == null)
                return;

            for (int i = 0; i < lootItems.Count; i++)
            {
                var loot = lootItems.GetByIndex(i);
                if (loot == null || loot.Item == null)
                    continue;

                float dist = Vector3.Distance(localPlayer.Transform.position, loot.transform.position);
                if (dist > PluginConfig.QuestEspMaxDistance.Value)
                    continue;

                if (!questItemIds.Contains(loot.Item.TemplateId))
                    continue;

                Vector3 screenPos = mainCamera.WorldToScreenPoint(loot.transform.position);
                if (screenPos.z > 0)
                {
                    float screenY = Screen.height - screenPos.y;
                    if (!EspRenderer.IsOnScreen(screenPos.x, screenY, Screen.width, Screen.height, 50f))
                        continue;
                    Targets.Add(
                        new QuestEspTarget
                        {
                            ScreenPosition = new Vector2(screenPos.x, screenY),
                            Distance = dist,
                            Name = "[QUEST] " + loot.Item.ShortName.Localized(),
                            Color = PluginConfig.ColorQuestItem.Value,
                            IsZone = false,
                        }
                    );
                }
            }
        }

        private void ScanContainers(
            Camera mainCamera,
            Player localPlayer,
            LootableContainer[] cachedContainers,
            HashSet<string> questItemIds
        )
        {
            if (cachedContainers == null)
                return;

            foreach (var container in cachedContainers)
            {
                if (container == null || container.ItemOwner?.RootItem == null)
                    continue;

                float dist = Vector3.Distance(localPlayer.Transform.position, container.transform.position);
                if (dist > PluginConfig.QuestEspMaxDistance.Value)
                    continue;

                var items = container.ItemOwner.RootItem.GetAllItems();
                foreach (var item in items)
                {
                    if (item == container.ItemOwner.RootItem)
                        continue;
                    if (!questItemIds.Contains(item.TemplateId))
                        continue;

                    Vector3 screenPos = mainCamera.WorldToScreenPoint(container.transform.position);
                    if (screenPos.z > 0)
                    {
                        float screenY = Screen.height - screenPos.y;
                        if (!EspRenderer.IsOnScreen(screenPos.x, screenY, Screen.width, Screen.height, 50f))
                            continue;
                        Targets.Add(
                            new QuestEspTarget
                            {
                                ScreenPosition = new Vector2(screenPos.x, screenY),
                                Distance = dist,
                                Name = "[QUEST-C] " + item.ShortName.Localized(),
                                Color = PluginConfig.ColorQuestItem.Value,
                                IsZone = false,
                            }
                        );
                    }
                }
            }
        }

        private void ScanZones(Camera mainCamera, Player localPlayer, HashSet<string> questZoneIds)
        {
            if (Time.time >= _nextTriggerRefresh || _cachedTriggers == null)
            {
                _cachedTriggers = UnityEngine.Object.FindObjectsOfType<TriggerWithId>();
                _nextTriggerRefresh = Time.time + 30f;
            }

            var triggers = _cachedTriggers;
            if (triggers == null)
                return;

            foreach (var trigger in triggers)
            {
                if (trigger == null || string.IsNullOrEmpty(trigger.Id))
                    continue;
                if (!questZoneIds.Contains(trigger.Id))
                    continue;

                float dist = Vector3.Distance(localPlayer.Transform.position, trigger.transform.position);
                if (dist > PluginConfig.QuestEspMaxDistance.Value)
                    continue;

                Vector3 screenPos = mainCamera.WorldToScreenPoint(trigger.transform.position);
                if (screenPos.z > 0)
                {
                    float screenY = Screen.height - screenPos.y;
                    if (!EspRenderer.IsOnScreen(screenPos.x, screenY, Screen.width, Screen.height, 50f))
                        continue;
                    Targets.Add(
                        new QuestEspTarget
                        {
                            ScreenPosition = new Vector2(screenPos.x, screenY),
                            Distance = dist,
                            Name = "[ZONE] " + trigger.Id,
                            Color = PluginConfig.ColorQuestZone.Value,
                            IsZone = true,
                        }
                    );
                }
            }
        }

        /// <summary>
        /// Draws quest item name and distance labels for each tracked quest target.
        /// Must be called from OnGUI.
        /// </summary>
        /// <param name="style">The <see cref="GUIStyle"/> used for quest ESP text rendering.</param>
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
