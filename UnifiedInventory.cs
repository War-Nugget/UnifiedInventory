using System;
using Terraria;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

using UnifiedInventory.SharedInventory.Utils;
using UnifiedInventory.SharedInventory.Database;
using UnifiedInventory.SharedInventory.Systems;
using UnifiedInventory.SharedInventory.Network;

using System.IO;



namespace UnifiedInventory
{
    public class UnifiedInventory : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            // Delegate all shared-inventory packets to your NetworkSystem
            Mod.GetModSystem<InventoryNetworkSystem>().HandlePacket(reader, whoAmI);
        }

        public override void Load()
        {
            // any global setup you need
        }

        public override void Unload()
        {
            // clean up static refs if necessary
        }
    }
}