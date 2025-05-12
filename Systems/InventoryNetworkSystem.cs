using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;
using System.IO;
using UnifiedInventory.Database;
using UnifiedInventory.Utils;
using System.Collections.Generic;


namespace UnifiedInventory.Systems
{
    public class InventoryNetworkSystem : ModSystem
    {
        public enum PacketType : byte
        {
            SyncInventory
        }

        public override void OnModLoad()
        {
            if (!Main.dedServ)
                return;

            ModNetHandler.Register(PacketType.SyncInventory, HandleSyncInventory);
        }

        public static void SendInventory(Player player, int toWho = -1, int ignore = -1)
        {
            var slotData = InventoryUtils.ToSlotData(player.inventory);

            ModPacket packet = ModContent.GetInstance<UnifiedInventory>().GetPacket();
            packet.Write((byte)PacketType.SyncInventory);
            packet.Write((byte)slotData.Count);

            foreach (var slot in slotData)
            {
                packet.Write(slot.SlotIndex);
                packet.Write(slot.ItemID);
                packet.Write(slot.Stack);
                packet.Write(slot.Prefix);
            }

            packet.Send(toWho, ignore);
        }
    }
}
