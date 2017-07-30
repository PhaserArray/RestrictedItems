using Rocket.API;
using Rocket.API.Serialisation;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.Core.Permissions;

namespace PhaserArray.RestrictedItems
{
    public class RestrictedItems : RocketPlugin<RestrictedItemsConfiguration>
	{
		private static RestrictedItems Instance;
		private static RestrictedItemsConfiguration Config;

		private Dictionary<ushort, List<List<string>>> AllRestrictedItems;

		protected override void Load()
		{
			Instance = this;
			Config = Instance.Configuration.Instance;

			AllRestrictedItems = new Dictionary<ushort, List<List<string>>>();

			// Loads the config into a nicely indexable dictionary.
			// I tried to do it without 3 nested loops and...
			// it worked but it encountered some weird behavior
			// and this didn't so I'm going with this.
			Logger.Log("Loading restricted items...");
			foreach (var RestrictedItemGroup in Config.RestrictedItemGroups)
			{
				foreach (var PermissionGroup in RestrictedItemGroup.PermissionGroups)
				{
					foreach (var ID in RestrictedItemGroup.IDs)
					{
						if (AllRestrictedItems.ContainsKey(ID))
						{
							AllRestrictedItems[ID].Add(PermissionGroup.Permissions);
						}
						else
						{
							var toBeAdded = new List<List<string>>();
							toBeAdded.Add(PermissionGroup.Permissions);
							AllRestrictedItems.Add(ID, toBeAdded);
						}
					}
				}
			}
			Logger.Log("Loaded " + AllRestrictedItems.Count.ToString() + " restricted items!");

			Logger.Log("=====");
			foreach (var item in AllRestrictedItems)
			{
				Logger.Log(item.Key.ToString());
				Logger.Log(item.Value.Count().ToString());
				Logger.Log(" -----");
				Logger.Log("  ---");
				foreach (var permissionGroup in item.Value)
				{
					foreach (var perm in permissionGroup)
					{
						Logger.Log("   " + perm);
					}
					Logger.Log("  ---");
				}
				Logger.Log(" -----");
				Logger.Log("=====");
			}

			UnturnedPlayerEvents.OnPlayerInventoryAdded += OnPlayerInventoryAdded;
			Logger.Log("Plugin Loaded!");
		}

		public void OnPlayerInventoryAdded(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
		{
			Logger.Log("InvGrp " + inventoryGroup.ToString());
			Logger.Log("InvIndx " + inventoryIndex.ToString());
			Logger.Log("P size x " + P.size_x.ToString());
			Logger.Log("P size y " + P.size_y.ToString());
			Logger.Log("P x " + P.x.ToString());
			Logger.Log("P y " + P.y.ToString());
			Logger.Log("P item " + P.item.ToString());
		}
	}
}