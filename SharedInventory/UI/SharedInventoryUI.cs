using System;
using System.Linq;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using UnifiedInventory.SharedInventory.Systems;
using UnifiedInventory.SharedInventory.Network;
using UnifiedInventory.SharedInventory.Utils;
using System.Drawing;
using UnifiedInventory.SharedInventory.Database;

namespace UnifiedInventory.SharedInventory.UI
{
    public class SharedInventoryUI : UIState
    {

        private UIItemSlot[] slots;
        private Item[] sharedItems;       // ← declare the backing array here
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
                return;

            var slotData = TeamInventorySystem.SharedInventories[teamID];

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

            // Update sharedItems to current state
            int teamID = Main.LocalPlayer.team;
            if (teamID > 0 && TeamInventorySystem.SharedInventories.TryGetValue(teamID, out var slotData))
            {
                sharedItems = slotData.Select(s => s.Item).ToArray();
            }

            OnInitialize();
        }


        // private void Slot_OnClick(UIMouseEvent evt, UIElement listeningElement)
        // {
        //     Main.NewText("[Debug] Slot_OnClick was called!");
        //     int index = Array.IndexOf(slots, (UIItemSlot)listeningElement);
        //     if (index < 0) return;

        //     if (Main.LocalPlayer.team <= 0)
        //     {
        //         Main.NewText("You must be on a team to move shared items.");
        //         return;
        //     }

        //     Item clickedItem = sharedItems[index];

        //     // Update shared inventory memory
        //     TeamInventorySystem.SharedInventories[Main.LocalPlayer.team][index].Item = clickedItem.Clone();

        //     // Send to server
        //     Main.NewText($"[DEBUG] Calling SendSlotChange for team {Main.LocalPlayer.team}, slot {index}",  Microsoft.Xna.Framework.Color.LightBlue);

        //     InventoryNetworkSystem.SendSlotChange(
        //         Main.LocalPlayer.team,
        //         index,
        //         clickedItem
        //     );
        //     //     InventoryUtils.ApplySlotData(
        //     //     Main.LocalPlayer.inventory,
        //     //     TeamInventorySystem.SharedInventories[Main.LocalPlayer.team]
        //     // );
        //     Main.NewText($"[Debug] Sent slot change for team {Main.LocalPlayer.team}, slot {index}");

        //     //  change in the player's local inventory
        //     Main.LocalPlayer.inventory[index] = clickedItem.Clone();
        // }
        private void Slot_OnClick(UIMouseEvent evt, UIElement listeningElement)
        {   
            int index = Array.IndexOf(slots, (UIItemSlot)listeningElement);
            if (index < 0 || Main.LocalPlayer.team <= 0) return;

            // What the player is holding (hotbar pickup or placing into slot)
            Item held = Main.mouseItem;
            // What used to live in this shared slot
            Item old  = slotData[index].Item.Clone();

            // 1) Swap them
            slotData[index].Item    = held.Clone();    // write into the real shared data
            Main.mouseItem          = old;             // give the player back whatever was here
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

    }
}
