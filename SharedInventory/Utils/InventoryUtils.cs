using System.Collections.Generic;
using Terraria;
using UnifiedInventory.SharedInventory.Database;  // where InventorySlotData lives

namespace UnifiedInventory.SharedInventory.Utils
{
    public static class InventoryUtils
    {
        /// <summary>
        /// Wraps each Item in the inventory into an InventorySlotData (with its slot index).
        /// Clones each Item so you don’t accidentally mutate the source array.
        /// </summary>
        public static InventorySlotData[] ToSlotData(Item[] inventory)
        {
            var slots = new InventorySlotData[inventory.Length];
            for (int i = 0; i < inventory.Length; i++)
            {
                // Clone so that future changes to the slot don’t affect the original array
                slots[i] = new InventorySlotData(i, inventory[i].Clone());
            }
            return slots;
        }

        /// <summary>
        /// Applies a set of InventorySlotData back into a raw Item[].
        /// Each slot’s Item is cloned into the target inventory at the matching index.
        /// </summary>
        public static void ApplySlotData(Item[] inventory, IEnumerable<InventorySlotData> data)
        {
            foreach (var slot in data)
            {
                int i = slot.SlotIndex;
                if (i < 0 || i >= inventory.Length)
                    continue;

                // Replace with a clone of the slot’s Item
                inventory[i] = slot.Item.Clone();
            }
        }
    }
}
