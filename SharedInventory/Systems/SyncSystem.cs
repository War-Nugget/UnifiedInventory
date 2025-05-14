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

            syncTimer += 1.0 / 60.0;                    // accumulate seconds
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

            if (!TeamInventorySystem.TeamInventories.TryGetValue(team, out var slots))
                return;

            // Overwrite the local inventory with the shared slots
            for (int i = 0; i < Main.LocalPlayer.inventory.Length && i < slots.Length; i++)
                Main.LocalPlayer.inventory[i] = slots[i].Item.Clone();
        }
    }
}
 