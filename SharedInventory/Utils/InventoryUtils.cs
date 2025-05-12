using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;
using System.IO;
using UnifiedInventory.SharedInventory.Database;
using UnifiedInventory.SharedInventory.Utils;

namespace UnifiedInventory.SharedInventory.Utils
{
    public static class InventoryUtils
    {
        public static List<InventorySlotData> ToSlotData(Item[] inventory)
        {
            var list = new List<InventorySlotData>();
            for (int i = 0; i < inventory.Length; i++)
            {
                var item = inventory[i];
                list.Add(new InventorySlotData
                {
                    SlotIndex = i,
                    ItemID = item.netID,
                    Stack = item.stack,
                    Prefix = item.prefix
                });
            }
            return list;
        }

        public static void ApplySlotData(Item[] inventory, List<InventorySlotData> data)
        {
            foreach (var slot in data)
            {
                inventory[slot.SlotIndex] = new Item();
                inventory[slot.SlotIndex].SetDefaults(slot.ItemID);
                inventory[slot.SlotIndex].stack = slot.Stack;
                inventory[slot.SlotIndex].prefix = (byte)slot.Prefix;
            }
        }
    }
}