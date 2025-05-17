using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.UI;
using UnifiedInventory;

namespace UnifiedInventory.SharedInventory.Systems
{
    public class SharedInventoryUISystem : ModSystem
    {
        public override void UpdateUI(GameTime gameTime)
        {
            UnifiedInventory.Instance?.UpdateUI(gameTime);
        }

        // public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        // {
        //     UnifiedInventory.Instance?.ModifyInterfaceLayers(layers);
        // }
    }
}
