using System;
using Rocket.API;
using UnityEngine;
using SDG.Unturned;
using Rocket.Unturned;
using System.Threading;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using Rocket.Unturned.Events;
using Rocket.API.Collections;
using System.Collections.Generic;
using Rocket.Unturned.Enumerations;

namespace PhaserArray.RestrictedItems
{
	public class RestrictedItems : RocketPlugin<RestrictedItemsConfiguration>
	{
		private const string version = "v1.2";

		private RestrictedItemsConfiguration Config;
		private Dictionary<ushort, List<List<string>>> AllRestrictedItems;
		private List<RestrictedItemsQueueItem> RestrictedItemsQueue;

		protected override void Load()
		{
			Config = Configuration.Instance;
			AllRestrictedItems = new Dictionary<ushort, List<List<string>>>();
			RestrictedItemsQueue = new List<RestrictedItemsQueueItem>();

			// Loads the config into a nicely indexable lowercase dictionary.
			Logger.Log("Loading restricted items...");
			foreach (var RestrictedItemGroup in Config.RestrictedItemGroups)
			{
				foreach (var PermissionGroup in RestrictedItemGroup.PermissionGroups)
				{
					foreach (var ID in RestrictedItemGroup.IDs)
					{
						if (AllRestrictedItems.ContainsKey(ID))
						{
							AllRestrictedItems[ID].Add(PermissionGroup.Permissions.ConvertAll(s => s.ToLower()));
						}
						else
						{
							var toBeAdded = new List<List<string>>();
							toBeAdded.Add(PermissionGroup.Permissions.ConvertAll(s => s.ToLower()));
							AllRestrictedItems.Add(ID, toBeAdded);
						}
					}
				}
			}

			U.Events.OnPlayerConnected += OnPlayerConnected;
			UnturnedPlayerEvents.OnPlayerInventoryAdded += OnPlayerInventoryAdded;
			UnturnedPlayerEvents.OnPlayerWear += OnPlayerWear;

			Logger.Log("Loaded " + AllRestrictedItems.Count.ToString() + " restricted items!");
			Logger.Log("Restricted items " + version + " Loaded!");
		}
		
		protected override void Unload()
		{
			U.Events.OnPlayerConnected -= OnPlayerConnected;
			UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnPlayerInventoryAdded;
			UnturnedPlayerEvents.OnPlayerWear -= OnPlayerWear;
		}

		private void Update()
		{
			if (RestrictedItemsQueue.Count > 0)
			{
				foreach (var restrictedItem in RestrictedItemsQueue)
				{
					var index = restrictedItem.player.Inventory.getIndex(restrictedItem.page, restrictedItem.itemJar.x, restrictedItem.itemJar.y);
					if (restrictedItem.player.Inventory.getItem(restrictedItem.page, index) != null)
					{
						if (restrictedItem.player.Inventory.getItem(restrictedItem.page, index).item.id == restrictedItem.itemJar.item.id)
						{
							restrictedItem.player.Inventory.removeItem(restrictedItem.page, index);
							UnturnedChat.Say(restrictedItem.player, Translate("restricteditem_removed", UnturnedItems.GetItemAssetById(restrictedItem.itemJar.item.id).itemName), Color.red);
						}
					}
				}
				RestrictedItemsQueue.Clear();
			}
		}

		private void OnPlayerConnected(UnturnedPlayer player)
		{
			// There is one minor graphical issue, sometimes the item gets removed but it still appears in inventory as a ghost item. ¯\_(ツ)_/¯
			CheckInventory(player);
		}

		private void OnPlayerInventoryAdded(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar itemJar)
		{
			if (!CanUseItem(player, itemJar.item.id))
			{
				// A queue seems to avoid a lot of the issues I was having when I tried to remove the items here.
				RestrictedItemsQueue.Add(new RestrictedItemsQueueItem(player, Convert.ToByte(inventoryGroup), itemJar));
			}
		}

		private void OnPlayerWear(UnturnedPlayer player, UnturnedPlayerEvents.Wearables wear, ushort id, byte? quality)
		{
			if (!CanUseItem(player, id))
			{
				new Thread(() =>
				{
					Thread.CurrentThread.IsBackground = true;
					// Okay so this could also work without the delay as long as it is in another thread?
					// No clue why this is the case, but I'll leave the delay in for safety?
					Thread.Sleep(30);

					var clothing = player.Player.clothing;
					switch (wear)
					{
						case UnturnedPlayerEvents.Wearables.Hat:
							clothing.askWearHat(0, 0, new byte[0], true);
							break;
						case UnturnedPlayerEvents.Wearables.Mask:
							clothing.askWearMask(0, 0, new byte[0], true);
							break;
						case UnturnedPlayerEvents.Wearables.Vest:
							clothing.askWearVest(0, 0, new byte[0], true);
							break;
						case UnturnedPlayerEvents.Wearables.Pants:
							clothing.askWearPants(0, 0, new byte[0], true);
							break;
						case UnturnedPlayerEvents.Wearables.Shirt:
							clothing.askWearShirt(0, 0, new byte[0], true);
							break;
						case UnturnedPlayerEvents.Wearables.Glasses:
							clothing.askWearGlasses(0, 0, new byte[0], true);
							break;
						case UnturnedPlayerEvents.Wearables.Backpack:
							clothing.askWearBackpack(0, 0, new byte[0], true);
							break;
						default:
							break;
					}
				}).Start();
			}
		}

		public void CheckInventory(UnturnedPlayer player)
		{
			var clothing = player.Player.clothing;

			if (!CanUseItem(player, clothing.backpack))
			{
				clothing.askWearBackpack(0, 0, new byte[0], true);
			}
			if (!CanUseItem(player, clothing.vest))
			{
				clothing.askWearVest(0, 0, new byte[0], true);
			}
			if (!CanUseItem(player, clothing.shirt))
			{
				clothing.askWearShirt(0, 0, new byte[0], true);
			}
			if (!CanUseItem(player, clothing.pants))
			{
				clothing.askWearPants(0, 0, new byte[0], true);
			}
			if (!CanUseItem(player, clothing.hat))
			{
				clothing.askWearHat(0, 0, new byte[0], true);
			}
			if (!CanUseItem(player, clothing.mask))
			{
				clothing.askWearMask(0, 0, new byte[0], true);
			}
			if (!CanUseItem(player, clothing.glasses))
			{
				clothing.askWearGlasses(0, 0, new byte[0], true);
			}

			for (byte page = 0; page < PlayerInventory.PAGES - 1; page++)
			{
				for (byte index = player.Inventory.getItemCount(page); index-- > 0;)
				{
					if (!CanUseItem(player, player.Inventory.getItem(page, index).item.id))
					{
						UnturnedChat.Say(player, Translate("restricteditem_removed", UnturnedItems.GetItemAssetById(player.Inventory.getItem(page, index).item.id).itemName), Color.red);
						player.Inventory.removeItem(page, index);
					}
				}
			}
		}

		public bool CanUseItem(UnturnedPlayer player, ushort ID)
		{
			if (player.IsAdmin || player.HasPermission(Config.ExemptPermission))
			{
				return true;
			}
			
			if (ID == 0)
			{
				return true;
			}

			if (AllRestrictedItems.ContainsKey(ID))
			{
				foreach (var PermissionGroup in AllRestrictedItems[ID])
				{
					// I tried to use HasPermissions, but that seemed to
					// return true if one of the perms was true, not all.
					var hasPerm = true;
					foreach (var Permission in PermissionGroup)
					{
						if (!player.HasPermission(Permission))
						{
							hasPerm = false;
						}
					}
					if (hasPerm)
					{
						return true;
					}
				}
				return false;
			}
			else
			{
				if (Config.UnlistedAreRestricted)
				{
					return false;
				}
			}

			return true;
		}

		public override TranslationList DefaultTranslations
		{
			get
			{
				return new TranslationList()
				{
					{"restricteditem_removed", "Item not permitted: {0}"}
				};
			}
		}
	}
}