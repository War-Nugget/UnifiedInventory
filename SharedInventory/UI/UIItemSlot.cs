using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnifiedInventory.SharedInventory.Database;

namespace UnifiedInventory.SharedInventory.UI
{
    public class UIItemSlot : UIElement
    {
        private readonly InventorySlotData[] data;
        private readonly int index;
        private readonly int context;


        public UIItemSlot(InventorySlotData[] data, int index, int context)
        {
            this.data = data;
            this.index = index;
            this.context = context;

            Width.Set(50f, 0f);
            Height.Set(50f, 0f);
        }

        public Item GetItem() => data[index].Item;

        public void SetItem(Item item) => data[index].Item = item;

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            var item = GetItem();
            var position = GetDimensions().Position();

            // Draw the item slot
            ItemSlot.Draw(spriteBatch, ref item, context, position);

            // Update the slot (if needed)
            SetItem(item);
        }
    }
}
