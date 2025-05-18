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
            // 1) Grab your InventorySlotData[] for the current team
            int teamID = Main.LocalPlayer.team;
            if (teamID <= 0)
            {
                slots = null;
                sharedItems = null;
                return;
            }
            this.slotData = TeamInventorySystem.SharedInventories[teamID];

            // 2) Extract the Item[] that UIItemSlot needs
            sharedItems = this.slotData.Select(s => s.Item).ToArray();

            previousItems = sharedItems.Select(item => item.Clone()).ToArray();

            // 3) Create a UIItemSlot for each array index
            slots = new UIItemSlot[Rows * Columns];
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = new UIItemSlot(sharedItems, i, ItemSlot.Context.InventoryItem);

                // position in a grid
                int row = i / Columns;
                int col = i % Columns;
                slot.Left.Set(col * (SlotSize + Padding), 0f);
                slot.Top.Set(row * (SlotSize + Padding), 0f);

                // subscribe to click events
                slot.OnLeftClick += Slot_OnClick;
                slot.OnRightClick += Slot_OnClick; // optional
                                                   // Main.NewText("[DEBUG] Slot_OnClick triggered", Microsoft.Xna.Framework.Color.Yellow);


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
            if (index < 0 || Main.LocalPlayer.team <= 0) return;

            // What the player is holding (hotbar pickup or placing into slot)
            Item held = Main.mouseItem;
            // What used to live in this shared slot
            Item old = slotData[index].Item.Clone();

            // 1) Swap them
            slotData[index].Item = held.Clone();    // write into the real shared data
            Main.mouseItem = old;             // give the player back whatever was here
            Main.LocalPlayer.inventory[index] = slotData[index].Item.Clone();

            // 2) Broadcast the change
            InventoryNetworkSystem.SendSlotChange(
                Main.LocalPlayer.team,
                index,
                slotData[index].Item
            );

            // 3) Immediately rebuild the UI so it shows the new item
            Refresh();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            int teamID = Main.LocalPlayer.team;
            if (teamID <= 0 || slotData == null || sharedItems == null || previousItems == null)
                return;

            for (int i = 0; i < sharedItems.Length; i++)
            {
                if (!ItemEquals(sharedItems[i], previousItems[i]))
                {
                    // Send updated item
                    InventoryNetworkSystem.SendSlotChange(teamID, i, sharedItems[i]);
                    previousItems[i] = sharedItems[i].Clone();  // update cached state
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
