using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;
using System.IO;
using UnifiedInventory.Database;

public class SyncSystem : ModSystem
{
    private double syncTimer = 0;

    public override void PostUpdatePlayers()
    {
        syncTimer += 1.0 / 60.0; // Assuming 60 FPS

        if (syncTimer >= 2.0)
        {
            syncTimer = 0;
            var data = SqlInventoryManager.LoadInventory();
            InventoryUtils.ApplySlotData(Main.LocalPlayer.inventory, data);
        }
    }
}
