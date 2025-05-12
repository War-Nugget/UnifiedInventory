using Terraria;
using Terraria.ModLoader;
using UnifiedInventory.SharedInventory.Systems;
using UnifiedInventory.SharedInventory.Utils;
using UnifiedInventory.SharedInventory.Database;

namespace UnifiedInventory.SharedInventory.Players
{
    public class SharedInventoryPlayer : ModPlayer
    {
        private int lastTeam = 0;
        private Item[] lastInventorySnapshot;

        public override void PostUpdate()
        {
            // 1. Detect team join
            if (Player.team > 0 && Player.team != lastTeam)
            {
                TeamSyncTracker.RegisterTeamHost(Player.team, Player.whoAmI);
                lastTeam = Player.team;
            }

            // 2. If host of team, check for inventory changes
            if (Player.team > 0 && TeamSyncTracker.IsTeamHost(Player.team, Player.whoAmI))
            {
                if (InventoryChanged())
                {
                    var slots = InventoryUtils.ToSlotData(Player.inventory);
                    SqlInventoryManager.SaveInventory(Player.team, slots);
                    lastInventorySnapshot = (Item[])Player.inventory.Clone(); // Deep copy not required for netID/stack/prefix tracking
                }
            }
        }
        public override void OnEnterWorld()
        {
            if (Player.team > 0)
            {
                var data = SqlInventoryManager.LoadInventory(Player.team);
                InventoryUtils.ApplySlotData(Player.inventory, data);

                var teamColor = Player.team switch
                {
                    1 => Microsoft.Xna.Framework.Color.Red,
                    2 => Microsoft.Xna.Framework.Color.Green,
                    3 => Microsoft.Xna.Framework.Color.Blue,
                    4 => Microsoft.Xna.Framework.Color.Yellow,
                    5 => Microsoft.Xna.Framework.Color.Purple,
                    _ => Microsoft.Xna.Framework.Color.White
                };

                Main.NewText($"[SharedInventory] Synced inventory for Team {GetTeamName(Player.team)}", teamColor);

            }
        }


        private bool InventoryChanged()
        {
            if (lastInventorySnapshot == null || lastInventorySnapshot.Length != Player.inventory.Length)
                return true;

            for (int i = 0; i < Player.inventory.Length; i++)
            {
                var a = Player.inventory[i];
                var b = lastInventorySnapshot[i];

                if (a.netID != b?.netID || a.stack != b?.stack || a.prefix != b?.prefix)
                    return true;
            }

            return false;
        }
        private string GetTeamName(int team)
        {
            return team switch
            {
                1 => "Red",
                2 => "Green",
                3 => "Blue",
                4 => "Yellow",
                5 => "Purple",
                _ => "None"
            };
        }

    }
}
