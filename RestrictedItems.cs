using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Items;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;
using Rocket.Unturned.Events;
using Rocket.Unturned.Permissions;
using System;
using Rocket.Unturned;
using System.Threading;
using Rocket.Unturned.Enumerations;

namespace PhaserArray.RestrictedItems
{
	public class RestrictedItems : RocketPlugin<RestrictedItemsConfiguration>
	{

		private const string version = "v1.1";

		private static RestrictedItemsConfiguration Config;
		private Dictionary<ushort, List<List<string>>> AllRestrictedItems;

		protected override void Load()
		{
			Config = Configuration.Instance;
			AllRestrictedItems = new Dictionary<ushort, List<List<string>>>();

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

		private void OnPlayerConnected(UnturnedPlayer player)
		{
			// Wait for player to fully load in.
			CheckInventory(player);
		}

		private void OnPlayerWear(UnturnedPlayer player, UnturnedPlayerEvents.Wearables wear, ushort id, byte? quality)
		{
			if (!CanUseItem(player, id))
			{
				new Thread(() =>
				{
					Thread.CurrentThread.IsBackground = true;
					// Delay is necessary, if you unequip immediately
					// the item will not get unequipped at all.
					Thread.Sleep(2000);
					try
					{
						switch (wear)
						{
							case UnturnedPlayerEvents.Wearables.Hat:
								player.Player.clothing.askWearHat(0, 0, new byte[0], true);
								break;
							case UnturnedPlayerEvents.Wearables.Mask:
								player.Player.clothing.askWearMask(0, 0, new byte[0], true);
								break;
							case UnturnedPlayerEvents.Wearables.Vest:
								player.Player.clothing.askWearVest(0, 0, new byte[0], true);
								break;
							case UnturnedPlayerEvents.Wearables.Pants:
								player.Player.clothing.askWearPants(0, 0, new byte[0], true);
								break;
							case UnturnedPlayerEvents.Wearables.Shirt:
								player.Player.clothing.askWearShirt(0, 0, new byte[0], true);
								break;
							case UnturnedPlayerEvents.Wearables.Glasses:
								player.Player.clothing.askWearGlasses(0, 0, new byte[0], true);
								break;
							case UnturnedPlayerEvents.Wearables.Backpack:
								player.Player.clothing.askWearBackpack(0, 0, new byte[0], true);
								break;
							default:
								break;
						}
					}
					catch { }
				}).Start();
			}
		}


		private void OnPlayerInventoryAdded(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
		{
			if (!CanUseItem(player, P.item.id))
			{
				if (player.Player.equipment.itemID == P.item.id)
				{
					player.Player.equipment.dequip();
				}
				new Thread(() =>
				{
					Thread.CurrentThread.IsBackground = true;
					// So this delay is needed between dequipping the
					// equipped tool and removing it to reduce weird errors.
					// It also seems to reduce weird graphical glitches
					// in the inventory when removing stuff;
					Thread.Sleep(2000);
					try
					{
						// Oddly enough the inventoryIndex is less reliable than the getIndex version.
						player.Player.inventory.removeItem(Convert.ToByte(inventoryGroup), player.Player.inventory.getIndex(Convert.ToByte(inventoryGroup), P.x, P.y));
						UnturnedChat.Say(player, Translate("restricteditem_removed", UnturnedItems.GetItemAssetById(P.item.id).itemName), Color.red);
					}
					catch { }
				}).Start();
			}
		}

		public void CheckInventory(UnturnedPlayer uPlayer)
		{
			var player = uPlayer.Player;
			
			if (!CanUseItem(uPlayer, player.clothing.backpack))
			{
				player.clothing.askWearBackpack(0, 0, new byte[0], true);
			}
			if (!CanUseItem(uPlayer, player.clothing.vest))
			{
				player.clothing.askWearVest(0, 0, new byte[0], true);
			}
			if (!CanUseItem(uPlayer, player.clothing.shirt))
			{
				player.clothing.askWearShirt(0, 0, new byte[0], true);
			}
			if (!CanUseItem(uPlayer, player.clothing.pants))
			{
				player.clothing.askWearPants(0, 0, new byte[0], true);
			}
			if (!CanUseItem(uPlayer, player.clothing.hat))
			{
				player.clothing.askWearHat(0, 0, new byte[0], true);
			}
			if (!CanUseItem(uPlayer, player.clothing.mask))
			{
				player.clothing.askWearMask(0, 0, new byte[0], true);
			}
			if (!CanUseItem(uPlayer, player.clothing.glasses))
			{
				player.clothing.askWearGlasses(0, 0, new byte[0], true);
			}
			
			for (byte page = 0; page < PlayerInventory.PAGES-1; page++)
			{
				// This should check for if the page exists.
				for (byte index = player.inventory.getItemCount(page); index-- > 0;)
				{
					if (!CanUseItem(uPlayer, player.inventory.getItem(page, index).item.id))
					{
						UnturnedChat.Say(uPlayer, Translate("restricteditem_removed", UnturnedItems.GetItemAssetById(player.inventory.getItem(page, index).item.id).itemName), Color.red);
						player.inventory.removeItem(page, index);
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