using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using UnifiedInventory.SharedInventory.Systems;
using UnifiedInventory.SharedInventory.Network;
using UnifiedInventory.SharedInventory.UI;
using Terraria.ID;

namespace UnifiedInventory.SharedInventory.Players
{
    public class SharedInventoryPlayer : ModPlayer
    {
        private int lastTeam = 0;
        private Item[] lastInventorySnapshot;

        public override void PostUpdate()
        {
         
            if (Player.team > 0 && Player.team != lastTeam)
            {
                lastTeam = Player.team;

                if (Main.netMode == NetmodeID.Server)
                {
                    TeamSyncTracker.RegisterTeamHost(Player.team, Player.whoAmI);

                    if (TeamSyncTracker.IsTeamHost(Player.team, Player.whoAmI))
                    {
                        SeedSharedArray();
                        lastInventorySnapshot = CloneInventory(Player.inventory);
                        InventoryNetworkSystem.SendInventory(Player.team);
                    }
                }

                SharedInventoryUI.Instance?.Refresh();

                if (Player.whoAmI == Main.myPlayer && !TeamSyncTracker.IsTeamHost(Player.team, Player.whoAmI))
                {
                    InventoryNetworkSystem.RequestFullSync(Player.team);
                }
            }

            // Host rebroadcasts changes
            if (Player.team > 0 && TeamSyncTracker.IsTeamHost(Player.team, Player.whoAmI))
            {
                
                bool interactingWithInventory = Main.playerInventory;

                if (!interactingWithInventory && InventoryChanged())
                {
                    SeedSharedArray();
                    InventoryNetworkSystem.SendInventory(Player.team);
                    lastInventorySnapshot = CloneInventory(Player.inventory);
                }
            }
        }

        public void OnEnterWorld(Player player)
        {
            if (player.whoAmI == Main.myPlayer && player.team > 0)
            {
                // create + initialize your SharedInventoryUI exactly once
                if (SharedInventoryUI.Instance == null)
                {
                    var ui = new SharedInventoryUI();
                    ui.Activate();        // this wires up the UIItemSlots
                    ui.OnInitialize();    // builds your slots array
                }

                // now you can request the full sync…
                InventoryNetworkSystem.RequestFullSync(player.team);
            }
        }

        private void SeedSharedArray()
        {
            var sharedSlots = TeamInventorySystem.SharedInventories[Player.team];
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
