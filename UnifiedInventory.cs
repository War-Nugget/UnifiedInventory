using System;
using Terraria;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

using UnifiedInventory.Utils;
using UnifiedInventory.Database;
using UnifiedInventory.Systems;

using System.IO;



namespace UnifiedInventory
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class UnifiedInventory : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			var packetType = (InventoryNetworkSystem.PacketType)reader.ReadByte();

			switch (packetType)
			{
				case InventoryNetworkSystem.PacketType.SyncInventory:
					int count = reader.ReadByte();
					var slotData = new List<InventorySlotData>();

					for (int i = 0; i < count; i++)
					{
						slotData.Add(new InventorySlotData
						{
							SlotIndex = reader.ReadInt32(),
							ItemID = reader.ReadInt32(),
							Stack = reader.ReadInt32(),
							Prefix = reader.ReadInt32()
						});
					}

					// Apply to player who sent or received the packet
					var player = Main.player[whoAmI];
					InventoryUtils.ApplySlotData(player.inventory, slotData);
					break;
			}
		}
		public override void Load()
		{
			// Optional: any global setup (e.g., logging, singletons, DB init fallback)
		}

		public override void Unload()
		{
			// Optional cleanup (set static references to null, etc.)
		}


	}
}
