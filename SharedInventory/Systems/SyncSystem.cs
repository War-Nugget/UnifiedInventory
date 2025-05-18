using Terraria;
using Terraria.ModLoader;
using UnifiedInventory.SharedInventory.Config;
using UnifiedInventory.SharedInventory.Systems; // for TeamSyncTracker & TeamInventorySystem
using UnifiedInventory.SharedInventory.Network;

namespace UnifiedInventory.SharedInventory.Systems
{
    public class SyncSystem : ModSystem
    {
        public override void PostUpdatePlayers()
        {
            var config = ModContent.GetInstance<UnifiedInventoryConfig>();
            if (!config.EnableSharedInventory)
                return;

            int team = Main.LocalPlayer.team;
            if (team <= 0)
                return;

            if (!TeamInventorySystem.SharedInventories.TryGetValue(team, out var slots))
                return;

            var inventory = Main.LocalPlayer.inventory;
            bool isHost = TeamSyncTracker.IsTeamHost(team, Main.myPlayer);


            // ✅ 2. ANYONE: if player is interacting, push local → shared if different
            if (Main.playerInventory)
            {
                for (int i = 0; i < inventory.Length && i < slots.Length; i++)
                {
                    var local = inventory[i];
                    var shared = slots[i].Item;

                    if (local.netID != shared.netID || local.stack != shared.stack || local.prefix != shared.prefix)
                    {
                        slots[i].Item = local.Clone(); // update shared slot
                        InventoryNetworkSystem.SendSlotChange(team, i, local); // broadcast change
                    }
                }
            }
        }
    }
}
