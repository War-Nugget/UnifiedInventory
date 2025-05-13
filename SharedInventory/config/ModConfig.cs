using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace UnifiedInventory.SharedInventory.Config
{
    public class UnifiedInventoryConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Label("Sync Interval (seconds)")]
        [Tooltip("How often the team host should sync the shared inventory in memory.")]
        [Range(1, 10)]
        [DefaultValue(2)]
        public int SyncIntervalSeconds { get; set; }

        [Label("Enable Shared Inventory")]
        [Tooltip("Enable or disable the shared inventory system entirely.")]
        [DefaultValue(true)]
        public bool EnableSharedInventory { get; set; }
    }
}
