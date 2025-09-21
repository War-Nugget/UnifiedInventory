using System;
using System.Drawing;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using UnifiedInventory.SharedInventory.Database;
using UnifiedInventory.SharedInventory.Systems;
using UnifiedInventory.SharedInventory.UI;
using UnifiedInventory.SharedInventory.Utils; // for UI refresh hook

namespace UnifiedInventory.SharedInventory.Network
{
    public class InventoryNetworkSystem : ModSystem
    {
        public static bool HasReceivedFullSync = false;
        public enum PacketType : byte
        {
            SyncInventory = 0,
            ModifySlot = 1,
            RequestFullSync = 2    // new: clients ask server for full refresh
        }

        
        /// Broadcast the full shared‐inventory for a given team.
       
        public static void SendInventory(int teamID, int toClient = -1, int ignoreClient = -1)
        {
            if (!TeamInventorySystem.SharedInventories.TryGetValue(teamID, out var slots))
                return;

            var packet = ModContent.GetInstance<UnifiedInventory>().GetPacket();
            packet.Write((byte)PacketType.SyncInventory);
            packet.Write((byte)teamID); // now include teamID
            packet.Write((byte)slots.Length);

            foreach (var slot in slots)
            {
                packet.Write((byte)slot.SlotIndex);
                ItemIO.Send(slot.Item, packet, writeStack: true, writeFavorite: true);
            }

            // 🧪 Debug: Confirm we're sending the inventory sync
            string target = toClient == -1 ? "all clients" : $"client {toClient}";
            // Main.NewText($"[SERVER] Sending full inventory sync for Team {teamID} to {target}", 
            //             Microsoft.Xna.Framework.Color.LightBlue);

            packet.Send(toClient, ignoreClient);
        }


    
        /// Tell the server “I changed one slot of my team.”

        public static void SendSlotChange(int teamID, int slotIndex, Item item)
        {
            var packet = ModContent.GetInstance<UnifiedInventory>().GetPacket();
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
            var packet = ModContent.GetInstance<UnifiedInventory>().GetPacket();
            packet.Write((byte)PacketType.RequestFullSync);
            packet.Write(teamID);
            packet.Send();
        }

        public void ReceivePacket(BinaryReader reader, int whoAmI)
        {
            var msg = (PacketType)reader.ReadByte();
            if (msg == PacketType.RequestFullSync && Main.netMode != NetmodeID.Server)
                return;

            if (msg == PacketType.SyncInventory && Main.netMode != NetmodeID.MultiplayerClient)
                return;

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
                    // Clients apply full snapshot
                    if (Main.netMode != NetmodeID.MultiplayerClient) return;

                    int team = reader.ReadByte();
                    int length = reader.ReadByte();

                    if (!TeamInventorySystem.SharedInventories.TryGetValue(team, out var arr))
                    {
                        arr = new InventorySlotData[TeamInventorySystem.MaxSlots];
                        for (int i = 0; i < arr.Length; i++)
                            arr[i] = new InventorySlotData(i, null);
                        TeamInventorySystem.SharedInventories[team] = arr;
                    }

                    for (int i = 0; i < length && i < arr.Length; i++)
                    {
                        byte slotIndex = reader.ReadByte();
                        var item = new Item();
                        ItemIO.Receive(item, reader, readStack: true, readFavorite: true);

                        // assign into the existing slot rather than replace it
                        arr[slotIndex].Item = item;
                    }

                    // ✅ NEW: adopt the server snapshot locally so no diffs get pushed back
                    if (Main.LocalPlayer.team == team)
                    {
                        InventoryUtils.ApplySlotData(
                            Main.LocalPlayer.inventory,
                            TeamInventorySystem.SharedInventories[team]   // IEnumerable<InventorySlotData>
                        );
                    }

                    SharedInventoryUI.Instance?.Refresh();   // force UI redraw
                    HasReceivedFullSync = true;
                    break;
                }

                case PacketType.ModifySlot:
                {
                    int team = reader.ReadInt32();
                    int slotIndex = reader.ReadInt32();
                    var item = new Item();
                    ItemIO.Receive(item, reader, readStack: true, readFavorite: true);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        // sanity: only accept from correct-team players
                        var sender = Main.player[whoAmI];
                        if (sender.team != team) return;

                        // server updates its master copy…
                        TeamInventorySystem.SharedInventories[team][slotIndex].Item = item;

                        // ✅ CHANGE: rebroadcast to everyone, including origin
                        var rebroadcast = ModContent.GetInstance<UnifiedInventory>().GetPacket();
                        rebroadcast.Write((byte)PacketType.ModifySlot);
                        rebroadcast.Write(team);
                        rebroadcast.Write(slotIndex);
                        ItemIO.Send(item, rebroadcast, writeStack: true, writeFavorite: true);
                        rebroadcast.Send(toClient: -1); // <— removed ignoreClient: whoAmI
                    }
                    else
                    {
                        // client applies shared → local
                        TeamInventorySystem.SharedInventories[team][slotIndex].Item = item;

                        if (Main.LocalPlayer.team == team)
                        {
                            InventoryUtils.ApplySlotData(
                                Main.LocalPlayer.inventory,
                                TeamInventorySystem.SharedInventories[team]
                            );
                        }

                        SharedInventoryUI.Instance?.Refresh();
                    }
                    break;
                }
            }
        }
    }
}
