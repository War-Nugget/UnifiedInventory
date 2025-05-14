using System;
using System.Linq;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using UnifiedInventory.SharedInventory.Systems;
using UnifiedInventory.SharedInventory.Network;

namespace UnifiedInventory.SharedInventory.UI
{
    public class SharedInventoryUI : UIState
    {
        private UIItemSlot[] slots;
        private Item[] sharedItems;       // ← declare the backing array here

        // layout constants
        private const int Rows     = 4;
        private const int Columns  = 10;
        private const int SlotSize = 50;
        private const int Padding  = 5;

        public override void OnInitialize()
        {
            base.OnInitialize();

            // 1) Grab your InventorySlotData[] for the current team
            int teamID = Main.LocalPlayer.team;
            if (teamID <= 0)
                return;

            var slotData = TeamInventorySystem.TeamInventories[teamID];

            // 2) Extract the Item[] that UIItemSlot needs
            sharedItems = slotData
                .Select(s => s.Item)
                .ToArray();

            // 3) Create a UIItemSlot for each array index
            slots = new UIItemSlot[Rows * Columns];
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = new UIItemSlot(sharedItems, i, ItemSlot.Context.InventoryItem);

                // position in a grid
                int row = i / Columns;
                int col = i % Columns;
                slot.Left.Set(col * (SlotSize + Padding),  0f);
                slot.Top .Set(row * (SlotSize + Padding),  0f);

                // subscribe to click events
                    slot.OnLeftClick  += Slot_OnClick;
                    slot.OnRightClick += Slot_OnClick; // optional


                Append(slot);
                slots[i] = slot;
            }
        }

        private void Slot_OnClick(UIMouseEvent evt, UIElement listeningElement)
        {
            int index = Array.IndexOf(slots, (UIItemSlot)listeningElement);
            if (index < 0) return;

            if (Main.LocalPlayer.team <= 0)
            {
                Main.NewText("You must be on a team to move shared items.");
                return;
            }

            // pull the Item out of our sharedItems[] backing array
            Item clickedItem = sharedItems[index];

            // fire the network update for that one slot
            InventoryNetworkSystem.SendSlotChange(
                Main.LocalPlayer.team,
                index,
                clickedItem
            );
        }
    }
}
