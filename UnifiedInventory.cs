using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using UnifiedInventory.SharedInventory.Network;
using UnifiedInventory.SharedInventory.UI;

namespace UnifiedInventory
{
    public class UnifiedInventory : Mod
    {
        public static UnifiedInventory Instance { get; private set; }
        private UserInterface _sharedInterface;

        public override void Load()
        {
            // Set up our UIState
            Instance = this;
            _sharedInterface = new UserInterface();
            _sharedInterface.SetState(new SharedInventoryUI());
        }

        public override void Unload()
        {
            // Tear down to avoid leaks on reload
            Instance = null;
            SharedInventoryUI.Instance = null;
            _sharedInterface = null;
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            ModContent.GetInstance<InventoryNetworkSystem>().ReceivePacket(reader, whoAmI);
        }

        // ───────────────────────────────────────────────────────────────────────────
        // 1) This runs *before* each draw tick, to let our UIState update logic run.
        public void UpdateUI(GameTime gameTime)
        {
            // Only update when we actually have a UI to draw
            _sharedInterface?.Update(gameTime);
        }
    }
}
