using Terraria;
using Terraria.ModLoader;
using UnifiedInventory.SharedInventory.Config;
using UnifiedInventory.SharedInventory.Systems; // for TeamSyncTracker & TeamInventorySystem

namespace UnifiedInventory.SharedInventory.Systems
{
    public class SyncSystem : ModSystem
    {
        private double syncTimer = 0;

        public override void PostUpdatePlayers()
        {
            var config = ModContent.GetInstance<UnifiedInventoryConfig>();
            if (!config.EnableSharedInventory)
                return;

            syncTimer += 1.0 / 60.0;  // accumulate seconds
            if (syncTimer < config.SyncIntervalSeconds)
                return;
            syncTimer = 0;

            int team = Main.LocalPlayer.team;
            if (team <= 0)
                return;

            // Only non-hosts apply the shared array to their own inventory
            int? hostId = TeamSyncTracker.GetTeamHost(team);
            if (!hostId.HasValue || TeamSyncTracker.IsTeamHost(team, Main.myPlayer))
                return;

            if (!config.ForceHostInventory)
                return;

            if (!TeamInventorySystem.TeamInventories.TryGetValue(team, out var slots))
                return;

            // Only overwrite slots that have changed
            var inventory = Main.LocalPlayer.inventory;
            for (int i = 0; i < inventory.Length && i < slots.Length; i++)
            {
                var local = inventory[i];
                var shared = slots[i].Item;

                // Compare by netID, stack, and prefix before cloning
                if (local.netID != shared.netID || local.stack != shared.stack || local.prefix != shared.prefix)
                {
                    inventory[i] = shared.Clone();
                }
            }
        }
    }
}
