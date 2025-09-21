using Terraria;
using Terraria.ModLoader;
using UnifiedInventory.SharedInventory.Config;
using UnifiedInventory.SharedInventory.Systems; // for TeamSyncTracker & TeamInventorySystem
using UnifiedInventory.SharedInventory.Network;
using Terraria.ID;

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

            // Clients: only mirror server/shared -> local; never push from here.
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (!InventoryNetworkSystem.HasReceivedFullSync)
                    return;

                if (!TeamInventorySystem.SharedInventories.TryGetValue(team, out var slots))
                    return;

                var inv = Main.LocalPlayer.inventory;

                for (int i = 0; i < inv.Length && i < slots.Length; i++)
                {
                    var shared = slots[i].Item;
                    var local  = inv[i];

                    // If local differs from authoritative shared, adopt shared.
                    if (local.netID != shared.netID || local.stack != shared.stack || local.prefix != shared.prefix)
                    {
                        inv[i] = shared.Clone(); // adopt server state without sending any packet
                    }
                }

                return;
            }

            // Server: do nothing here. The server's authoritative updates & rebroadcasts
            // happen inside InventoryNetworkSystem.ReceivePacket (ModifySlot/SyncInventory).
            // Leaving this empty avoids redundant or conflicting writes.
        }
    }
}
