using System;
using Terraria;

namespace UnifiedInventory.SharedInventory.Database
{
    /// <summary>
    /// Represents one slot in a shared‐inventory array.
    /// </summary>
    public class InventorySlotData
    {
        /// <summary>
        /// Which index (0–MaxSlots-1) this belongs to.
        /// </summary>
        public int SlotIndex { get; set; }

        /// <summary>
        /// The actual Item in this slot. Never null.
        /// </summary>
        public Item Item { get; set; }

        public InventorySlotData()
        {
            Item = new Item();  // empty slot
        }

        public InventorySlotData(int slotIndex, Item item)
        {
            SlotIndex = slotIndex;
            Item = item ?? new Item();
        }

        public static implicit operator InventorySlotData(Item v)
        {
            throw new NotImplementedException();
        }
    }
}
