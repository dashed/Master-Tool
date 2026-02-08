using System;
using System.Linq;
using EFT;
using MasterTool.Config;
using UnityEngine;

namespace MasterTool.Features.Teleport
{
    /// <summary>
    /// Provides one-shot teleportation actions for enemies and filtered loot items.
    /// </summary>
    public static class TeleportFeature
    {
        /// <summary>
        /// Teleports all alive non-local players to a position 3 meters in front of the local player.
        /// </summary>
        /// <param name="gameWorld">The current game world containing registered players.</param>
        /// <param name="localPlayer">The local player whose position is the teleport destination.</param>
        public static void TeleportEnemiesToPlayer(GameWorld gameWorld, Player localPlayer)
        {
            if (gameWorld == null || localPlayer == null)
                return;

            Vector3 targetPos = localPlayer.Transform.position + (localPlayer.Transform.forward * 3f);

            foreach (var player in gameWorld.RegisteredPlayers)
            {
                if (player == null || player.IsYourPlayer || !player.HealthController.IsAlive)
                    continue;
                player.Transform.position = targetPos;
            }
        }

        /// <summary>
        /// Teleports all loose loot items matching the item ESP filter to 0.5m above the local player.
        /// If no filter is set, all loose loot is teleported.
        /// </summary>
        /// <param name="gameWorld">The current game world containing loot items.</param>
        /// <param name="localPlayer">The local player whose position is the teleport destination.</param>
        public static void TeleportFilteredItemsToPlayer(GameWorld gameWorld, Player localPlayer)
        {
            if (gameWorld == null || localPlayer == null)
                return;

            Vector3 targetPos = localPlayer.Transform.position + Vector3.up * 0.5f;
            string[] filters = PluginConfig
                .ItemEspFilter.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim().ToLower())
                .ToArray();

            var lootItems = gameWorld.LootItems;
            for (int i = 0; i < lootItems.Count; i++)
            {
                var loot = lootItems.GetByIndex(i);
                if (loot == null || loot.Item == null)
                    continue;

                string name = loot.Item.ShortName.Localized().ToLower();
                if (filters.Length > 0 && !filters.Any(f => name.Contains(f)))
                    continue;

                loot.transform.position = targetPos;
            }
        }
    }
}
