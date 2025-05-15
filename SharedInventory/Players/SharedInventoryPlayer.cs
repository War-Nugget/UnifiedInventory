using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using UnifiedInventory.SharedInventory.Systems;
using UnifiedInventory.SharedInventory.Network;

namespace UnifiedInventory.SharedInventory.Players
{
    public class SharedInventoryPlayer : ModPlayer
    {
        private int lastTeam = 0;
        private Item[] lastInventorySnapshot;

        public override void PostUpdate()
        {
            // 1) Detect when we join/switch teams
            if (Player.team > 0 && Player.team != lastTeam)
            {
                TeamSyncTracker.RegisterTeamHost(Player.team, Player.whoAmI);
                lastTeam = Player.team;

                // If *we* are the new host, seed the shared array and broadcast it immediately
                if (TeamSyncTracker.IsTeamHost(Player.team, Player.whoAmI))
                {
                    SeedSharedArray();
                    lastInventorySnapshot = CloneInventory(Player.inventory);
                    InventoryNetworkSystem.SendInventory(Player.team);
                }
            }

            // 2) If *we* are the host, watch for local changes and re-broadcast
            if (Player.team > 0 && TeamSyncTracker.IsTeamHost(Player.team, Player.whoAmI))
            {
                if (InventoryChanged())
                {
                    SeedSharedArray();
                    InventoryNetworkSystem.SendInventory(Player.team);
                    lastInventorySnapshot = CloneInventory(Player.inventory);
                }
            }
        }

        public override void OnEnterWorld()
        {
            if (Player.team > 0 && !TeamSyncTracker.IsTeamHost(Player.team, Player.whoAmI))
            {
                // Non-hosts wait for the SyncInventory packet to populate their UI
                Main.NewText(
                    $"[SharedInventory] Synced inventory for Team {GetTeamName(Player.team)}",
                    GetTeamColor(Player.team)
                );
            }
        }

        private void SeedSharedArray()
        {
            var sharedSlots = TeamInventorySystem.TeamInventories[Player.team];
            // Copy each slot from Player.inventory → sharedSlots[i].Item
            for (int i = 0; i < Player.inventory.Length && i < sharedSlots.Length; i++)
                sharedSlots[i].Item = Player.inventory[i].Clone();
        }

        private Item[] CloneInventory(Item[] inv)
        {
            var clone = new Item[inv.Length];
            for (int i = 0; i < inv.Length; i++)
                clone[i] = inv[i].Clone();
            return clone;
        }

        private bool InventoryChanged()
        {
            if (lastInventorySnapshot == null || lastInventorySnapshot.Length != Player.inventory.Length)
                return true;

            for (int i = 0; i < Player.inventory.Length; i++)
            {
                var a = Player.inventory[i];
                var b = lastInventorySnapshot[i];
                if (a.netID != b.netID || a.stack != b.stack || a.prefix != b.prefix)
                    return true;
            }

            return false;
        }

        private string GetTeamName(int team) => team switch
        {
            1 => "Red",
            2 => "Green",
            3 => "Blue",
            4 => "Yellow",
            5 => "Purple",
            _ => "None"
        };

        private Color GetTeamColor(int team) => team switch
        {
            1 => Color.Red,
            2 => Color.Green,
            3 => Color.Blue,
            4 => Color.Yellow,
            5 => Color.Purple,
            _ => Color.White
        };

       
    }
}
