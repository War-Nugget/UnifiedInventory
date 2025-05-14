using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.IO;
using UnifiedInventory.SharedInventory.Systems;
using Terraria.ID; // for TeamInventorySystem

namespace UnifiedInventory.SharedInventory.Network
{
    public class InventoryNetworkSystem : ModSystem
    {
        public enum PacketType : byte
        {
            SyncInventory = 0,
            ModifySlot     = 1
        }

        /// <summary>
        /// Send the full shared‐inventory for this player’s team (useful on join / team-switch).
        /// </summary>
        public static void SendInventory(Player player, int toWho = -1, int ignore = -1)
        {
            int teamID = player.team;
            if (!TeamInventorySystem.TeamInventories.TryGetValue(teamID, out var slots))
                return;

            var packet = ModContent.GetInstance<UnifiedInventory>().GetPacket();
            packet.Write((byte)PacketType.SyncInventory);
            packet.Write((byte)slots.Length);
            foreach (var slot in slots)
            {
                packet.Write((byte)slot.SlotIndex);
                ItemIO.Send(slot.Item, packet, writeStack: true, writeFavorite: true);
            }
            packet.Send(toWho, ignore);
        }

        /// <summary>
        /// Call this from your UI when a single slot changes.
        /// </summary>
        public static void SendSlotChange(int teamID, int slotIndex, Item item)
        {
            var packet = ModContent.GetInstance<UnifiedInventory>().GetPacket();
            packet.Write((byte)PacketType.ModifySlot);
            packet.Write(teamID);
            packet.Write(slotIndex);
            // ← use writeFavorite, not writeDefaults
            ItemIO.Send(item, packet, writeStack: true, writeFavorite: true);
            packet.Send(); // to server
        }


        /// <summary>
        /// Entry point for all incoming packets. Hook this up in your Mod.HandlePacket.
        /// </summary>
        public void ReceivePacket(BinaryReader reader, int whoAmI) {
            var msg = (PacketType)reader.ReadByte();

            if (msg == PacketType.ModifySlot) {
                int team      = reader.ReadInt32();
                int slotIndex = reader.ReadInt32();

                if (Main.netMode == NetmodeID.Server) {
                    var sender = Main.player[whoAmI];
                    if (sender.team != team) return;

                    // ← use readFavorite instead of readDefaults
                    var item = new Item();
                    ItemIO.Receive(item, reader, readStack: true, readFavorite: true);
                    TeamInventorySystem.TeamInventories[team][slotIndex] = item;

                    // rebroadcast: use writeFavorite instead of writeDefaults
                    var rebroadcast = ModContent
                    .GetInstance<UnifiedInventory>()
                    .GetPacket();
                    rebroadcast.Write((byte)PacketType.ModifySlot);
                    rebroadcast.Write(team);
                    rebroadcast.Write(slotIndex);
                    ItemIO.Send(item, rebroadcast, writeStack: true, writeFavorite: true);
                    rebroadcast.Send(toClient: -1, ignoreClient: whoAmI);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient) {
                    var item = new Item();
                    // ← same change here
                    ItemIO.Receive(item, reader, readStack: true, readFavorite: true);
                    TeamInventorySystem.TeamInventories[team][slotIndex] = item;
                    // TODO: refresh your UI
                }
            }
            else if (msg == PacketType.SyncInventory && Main.netMode == NetmodeID.MultiplayerClient) {
                int length = reader.ReadByte();
                int team   = Main.LocalPlayer.team;
                var arr    = TeamInventorySystem.TeamInventories[team];

                for (int i = 0; i < length && i < arr.Length; i++) {
                    byte slotIndex = reader.ReadByte();
                    var item = new Item();
                    // ← and here too
                    ItemIO.Receive(item, reader, readStack: true, readFavorite: true);
                    arr[slotIndex] = item;
                }
                // TODO: refresh your UI
            }
        

            else if (msg == PacketType.SyncInventory && Main.netMode == NetmodeID.MultiplayerClient)
            {
                int length = reader.ReadByte();
                int team   = Main.LocalPlayer.team;
                var arr    = TeamInventorySystem.TeamInventories[team];

                for (int i = 0; i < length && i < arr.Length; i++)
                {
                    byte slotIndex = reader.ReadByte();
                    var item = new Item();
                    ItemIO.Receive(item, reader, readStack: true, readFavorite: true);
                    arr[slotIndex] = item;
                }
                // TODO: refresh your UI if open
            }
        }
    }
}
