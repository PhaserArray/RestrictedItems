using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhaserArray.RestrictedItems
{
	class RestrictedItemsQueueItem
	{
		public RestrictedItemsQueueItem(UnturnedPlayer player, byte page, ItemJar itemJar)
		{
			this.player = player;
			this.page = page;
			this.itemJar = itemJar;
		}

		public UnturnedPlayer player;
		public byte page;
		public ItemJar itemJar;
	}
}
