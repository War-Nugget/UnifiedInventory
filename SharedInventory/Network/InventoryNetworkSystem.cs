using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using UnifiedInventory.SharedInventory.Systems;
using UnifiedInventory.SharedInventory.UI; // for UI refresh hook

namespace UnifiedInventory.SharedInventory.Network
{
    public class InventoryNetworkSystem : ModSystem
    {
        public enum PacketType : byte
        {
            SyncInventory   = 0,
            ModifySlot      = 1,
            RequestFullSync = 2    // new: clients ask server for full refresh
        }

        /// <summary>
        /// Broadcast the full shared‐inventory for a given team.
        /// </summary>
        public static void SendInventory(int teamID, int toClient = -1, int ignoreClient = -1)
        {
            if (!TeamInventorySystem.TeamInventories.TryGetValue(teamID, out var slots))
                return;

            var packet = ModContent.GetInstance<UnifiedInventory>()
                                   .GetPacket();
            packet.Write((byte)PacketType.SyncInventory);
            packet.Write((byte)teamID);                      // now include teamID
            packet.Write((byte)slots.Length);
            foreach (var slot in slots)
            {
                packet.Write((byte)slot.SlotIndex);
                ItemIO.Send(slot.Item, packet, writeStack: true, writeFavorite: true);
            }
            packet.Send(toClient, ignoreClient);
        }

        /// <summary>
        /// Tell the server “I changed one slot of my team.”
        /// </summary>
        public static void SendSlotChange(int teamID, int slotIndex, Item item)
        {
            var packet = ModContent.GetInstance<UnifiedInventory>()
                                   .GetPacket();
            packet.Write((byte)PacketType.ModifySlot);
            packet.Write(teamID);
            packet.Write(slotIndex);
            ItemIO.Send(item, packet, writeStack: true, writeFavorite: true);
            packet.Send(); // always to server
        }

        /// <summary>
        /// Ask the server to send you the latest full inventory.
        /// </summary>
        public static void RequestFullSync(int teamID)
        {
            var packet = ModContent.GetInstance<UnifiedInventory>()
                                   .GetPacket();
            packet.Write((byte)PacketType.RequestFullSync);
            packet.Write(teamID);
            packet.Send();
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            var msg = (PacketType)reader.ReadByte();
            switch (msg)
            {
                case PacketType.RequestFullSync:
                    {
                        if (Main.netMode != NetmodeID.Server) return;
                        int team = reader.ReadInt32();
                        SendInventory(team, toClient: whoAmI);
                        break;
                    }

                case PacketType.SyncInventory:
                    {
                        // both server (rarely) and clients could technically receive this,
                        // but we only act on it in clients:
                        if (Main.netMode != NetmodeID.MultiplayerClient) return;

                        int team   = reader.ReadByte();
                        int length = reader.ReadByte();
                        if (!TeamInventorySystem.TeamInventories.TryGetValue(team, out var arr))
                            return;

                        for (int i = 0; i < length && i < arr.Length; i++)
                        {
                            byte slotIndex = reader.ReadByte();
                            var item = new Item();
                            ItemIO.Receive(item, reader, readStack: true, readFavorite: true);

                            // assign into the existing slot rather than replace it
                            arr[slotIndex].Item = item;
                        }

                        SharedInventoryUI.Instance?.Refresh();   // force UI redraw
                        break;
                    }

                case PacketType.ModifySlot:
                    {
                        int team      = reader.ReadInt32();
                        int slotIndex = reader.ReadInt32();
                        var item      = new Item();
                        ItemIO.Receive(item, reader, readStack: true, readFavorite: true);

                        if (Main.netMode == NetmodeID.Server)
                        {
                            // sanity: only accept from correct-team players
                            var sender = Main.player[whoAmI];
                            if (sender.team != team) return;

                            // server updates its master copy…
                            TeamInventorySystem.TeamInventories[team][slotIndex].Item = item;

                            // …and rebroadcasts to everyone (including origin)
                            var rebroadcast = ModContent.GetInstance<UnifiedInventory>()
                                                        .GetPacket();
                            rebroadcast.Write((byte)PacketType.ModifySlot);
                            rebroadcast.Write(team);
                            rebroadcast.Write(slotIndex);
                            ItemIO.Send(item, rebroadcast, writeStack: true, writeFavorite: true);
                            rebroadcast.Send(toClient: -1, ignoreClient: whoAmI);
                        }
                        else
                        {
                            // client just updates its local mirror
                            TeamInventorySystem.TeamInventories[team][slotIndex].Item = item;
                            SharedInventoryUI.Instance?.Refresh();
                        }
                        break;
                    }
            }
        }
    }
}
