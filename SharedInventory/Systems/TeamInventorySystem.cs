using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using UnifiedInventory.SharedInventory.Database; // for InventorySlotData
using System.Linq;   

namespace UnifiedInventory.SharedInventory.Systems
{
    /// <summary>
    /// Keeps an in-memory array of InventorySlotData for each team,
    /// and saves/loads it with the world.
    /// </summary>
    public class TeamInventorySystem : ModSystem
    {
        public const int MaxSlots = 40;  // adjust to however many shared slots you want
        public const int MaxTeams = 6;   // Terraria has teams 0–5

        /// <summary>
        /// teamID → that team’s shared-inventory slots
        /// </summary>
        public static Dictionary<int, InventorySlotData[]> TeamInventories = new();

        public override void OnWorldLoad()
        {
            // initialize fresh empty arrays for every team
            TeamInventories.Clear();
            for (int team = 0; team < MaxTeams; team++)
            {
                var slots = new InventorySlotData[MaxSlots];
                for (int i = 0; i < MaxSlots; i++)
                    slots[i] = new InventorySlotData(i, null);
                TeamInventories[team] = slots;
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // turn each TeamInventories entry into a TagCompound
            var list = new List<TagCompound>();
            foreach (var kvp in TeamInventories)
            {
                list.Add(new TagCompound {
                    ["teamID"] = kvp.Key,
                    ["items"] = kvp.Value
                    .Select(slot => ItemIO.Save(slot.Item))
                    .ToList()
                });
            }
            tag["sharedInventories"] = list;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // reset to empty first
            OnWorldLoad();

            if (!tag.ContainsKey("sharedInventories")) return;

            foreach (var tc in tag.GetList<TagCompound>("sharedInventories"))
            {
                int teamID = tc.GetInt("teamID");
                var saved = tc.GetList<TagCompound>("items");
                var slots = new InventorySlotData[MaxSlots];
                for (int i = 0; i < MaxSlots; i++)
                {
                    var itemData = i < saved.Count ? saved[i] : null;
                    var item     = itemData is null ? new Item() : ItemIO.Load(itemData);
                    slots[i]     = new InventorySlotData(i, item);
                }
                TeamInventories[teamID] = slots;
            }
        }
    }
}
