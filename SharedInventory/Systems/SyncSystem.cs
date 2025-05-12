using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;
using System.IO;

using UnifiedInventory.SharedInventory.Utils;
using UnifiedInventory.SharedInventory.Database;
using UnifiedInventory.SharedInventory.Systems;
using UnifiedInventory.SharedInventory.Config;

namespace UnifiedInventory.SharedInventory.Systems
{
    public class SyncSystem : ModSystem
    {
        private double syncTimer = 0;

        public override void PostUpdatePlayers()
        {
            int interval = ModContent.GetInstance<UnifiedInventoryConfig>().SyncIntervalSeconds;
            bool enabled = ModContent.GetInstance<UnifiedInventoryConfig>().EnableSharedInventory;
            syncTimer += 1.0 / 60.0; // Assuming 60 FPS

            if (enabled && Main.myPlayer == TeamSyncTracker.GetTeamHost(Main.LocalPlayer.team))
            {
                var data = SqlInventoryManager.LoadInventory(Main.LocalPlayer.team);
                InventoryUtils.ApplySlotData(Main.LocalPlayer.inventory, data);
            }
        }
    }
}
