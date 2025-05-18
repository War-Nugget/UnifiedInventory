using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace UnifiedInventory.SharedInventory.Config
{
    public class UnifiedInventoryConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Label("Enable Shared Inventory")]
        [Tooltip("Enable or disable the shared inventory system entirely.")]
        [DefaultValue(true)]
        public bool EnableSharedInventory { get; set; }

    }
}
// 