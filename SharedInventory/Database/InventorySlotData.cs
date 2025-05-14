using Terraria;
using Terraria.ModLoader.IO;

namespace UnifiedInventory.SharedInventory.Database
{
    public class InventorySlotData
    {
        public int SlotIndex { get; set; }

        public Item Item { get; set; }

        public InventorySlotData() { }

        public InventorySlotData(int slotIndex, Item item)
        {
            SlotIndex = slotIndex;
            Item     = item?.Clone() ?? new Item();
        }

        public TagCompound Save()
            => new TagCompound {
                ["SlotIndex"] = SlotIndex,
                ["Item"]      = ItemIO.Save(Item)
            };

        public static InventorySlotData Load(TagCompound tag)
        {
            var idx  = tag.GetInt("SlotIndex");
            var data = tag.GetCompound("Item");
            var itm  = ItemIO.Load(data);
            return new InventorySlotData(idx, itm);
        }
    }
}
