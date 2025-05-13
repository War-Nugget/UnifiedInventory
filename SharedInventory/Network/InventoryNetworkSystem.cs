using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.IO;
using UnifiedInventory.SharedInventory.Database;    // for InventorySlotData
using UnifiedInventory.SharedInventory.Systems;     // for TeamInventorySystem

namespace UnifiedInventory.SharedInventory.Network
{
    public class InventoryNetworkSystem : ModSystem
    {
        public enum PacketType : byte
        {
            SyncInventory
        }

        /// <summary>
        /// Sends the full shared‐inventory array for this player’s team.
        /// </summary>
        public static void SendInventory(Player player, int toWho = -1, int ignore = -1)
        {
            int teamID = player.team;
            if (!TeamInventorySystem.TeamInventories.TryGetValue(teamID, out var slots))
                return;

            // Create packet and write header
            ModPacket packet = ModContent.GetInstance<UnifiedInventory>().GetPacket();
            packet.Write((byte)PacketType.SyncInventory);

            // Write slot count
            packet.Write((byte)slots.Length);

            // Serialize each slot: index + full Item data
            foreach (var slot in slots)
            {
                packet.Write(slot.SlotIndex);
                ItemIO.Send(slot.Item, packet, writeStack: true, writeFavorite: true);
            }

            packet.Send(toWho, ignore);
        }

        /// <summary>
        /// Receives a SyncInventory packet and populates the local array.
        /// </summary>
        public void HandlePacket(BinaryReader reader, int whoAmI)
        {
            var packetType = (PacketType)reader.ReadByte();
            if (packetType == PacketType.SyncInventory)
            {
                byte count = reader.ReadByte();
                int teamID = Main.player[whoAmI].team;
                var slots = TeamInventorySystem.TeamInventories[teamID];

                for (int i = 0; i < count; i++)
                {
                    int slotIndex = reader.ReadInt32();
                    var item = new Item();
                    ItemIO.Receive(item, reader, readStack: true, readFavorite: true);
                    slots[slotIndex].Item = item;
                }
            }
        }
    }
}
