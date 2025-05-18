using System;
using System.Linq;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using UnifiedInventory.SharedInventory.Systems;
using UnifiedInventory.SharedInventory.Network;

using UnifiedInventory.SharedInventory.Database;
using Microsoft.Xna.Framework;

namespace UnifiedInventory.SharedInventory.UI
{
    public class SharedInventoryUI : UIState
    {

        private UIItemSlot[] slots;
        private Item[] sharedItems;       // ← declare the backing array here
        private Item[] previousItems;
        private InventorySlotData[] slotData;
        

        // layout constants
        private const int Rows = 5;
        private const int Columns = 10;
        private const int SlotSize = 50;
        private const int Padding = 5;

        

        public static SharedInventoryUI Instance { get; set; }


        public override void OnInitialize()
        {
            base.OnInitialize();
            Instance = this;

            int teamID = Main.LocalPlayer.team;
            if (teamID <= 0 || !TeamInventorySystem.SharedInventories.TryGetValue(teamID, out slotData))
            {
                slots = null;
                return;
            }

            previousItems = new Item[slotData.Length];
            for (int i = 0; i < slotData.Length; i++)
                previousItems[i] = slotData[i].Item.Clone();

            slots = new UIItemSlot[Rows * Columns];
            for (int i = 0; i < slots.Length; i++)
            {
                // Bounds check in case shared array isn't fully sized
                Item itemRef = i < slotData.Length ? slotData[i].Item : new Item();

                var slot = new UIItemSlot(slotData, i, ItemSlot.Context.InventoryItem);

                int row = i / Columns;
                int col = i % Columns;
                slot.Left.Set(col * (SlotSize + Padding), 0f);
                slot.Top.Set(row * (SlotSize + Padding), 0f);

                slot.OnLeftClick += Slot_OnClick;
                slot.OnRightClick += Slot_OnClick;

                Append(slot);
                slots[i] = slot;
            }
        }

        public void Refresh()
        {
            RemoveAllChildren();

            int teamID = Main.LocalPlayer.team;

            // 2) Re-bind slotData & sharedItems from the live system, or empty if no team
            if (teamID > 0 && TeamInventorySystem.SharedInventories.TryGetValue(teamID, out var freshData))
            {
                this.slotData = freshData;
                sharedItems = this.slotData.Select(s => s.Item).ToArray();
            }
            else
            {
                // Not on a team: clear out
                this.slotData = Array.Empty<InventorySlotData>();
                sharedItems = Array.Empty<Item>();
            }
            OnInitialize();
        }



        private void Slot_OnClick(UIMouseEvent evt, UIElement listeningElement)
        {
            int index = Array.IndexOf(slots, (UIItemSlot)listeningElement);
            if (index < 0 || index >= slotData.Length || Main.LocalPlayer.team <= 0)
                return;

            Item held = Main.mouseItem;
            Item old = slotData[index].Item.Clone();

            // Swap the items
            slotData[index].Item = held.Clone();
            Main.mouseItem = old;
            Main.LocalPlayer.inventory[index] = slotData[index].Item.Clone();

            // Send update to server
            InventoryNetworkSystem.SendSlotChange(Main.LocalPlayer.team, index, slotData[index].Item);

            // Update cached previous item
            previousItems[index] = slotData[index].Item.Clone();

            // Refresh visuals only (not full rebuild)
            slots[index].SetItem(slotData[index].Item);
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            int teamID = Main.LocalPlayer.team;
            if (teamID <= 0 || slotData == null || sharedItems == null || previousItems == null)
                return;

            for (int i = 0; i < slotData.Length && i < previousItems.Length; i++)
            {
                if (!ItemEquals(slotData[i].Item, previousItems[i]))
                {
                    InventoryNetworkSystem.SendSlotChange(teamID, i, slotData[i].Item);
                    previousItems[i] = slotData[i].Item.Clone();
                }
            }
        }

        // Utility function to compare items safely
        private bool ItemEquals(Item a, Item b)
        {
            return a?.type == b?.type &&
                a?.stack == b?.stack &&
                a?.prefix == b?.prefix;
        }

    }
}
