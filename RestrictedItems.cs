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

namespace PhaserArray.RestrictedItems
{
	public class RestrictedItems : RocketPlugin<RestrictedItemsConfiguration>
	{
		private static RestrictedItemsConfiguration Config;
		private float LastCheck;
		private HashSet<UnturnedPlayer> CheckQueue;
		private Dictionary<ushort, List<List<string>>> AllRestrictedItems;

		protected override void Load()
		{
			Config = Configuration.Instance;
			LastCheck = Time.time;
			CheckQueue = new HashSet<UnturnedPlayer>();
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
			U.Events.OnPlayerDisconnected += OnPlayerDisconnected;

			Logger.Log("Loaded " + AllRestrictedItems.Count.ToString() + " restricted items!");
			Logger.Log("Plugin Loaded!");
		}

		protected override void Unload()
		{
			U.Events.OnPlayerConnected -= OnPlayerConnected;
			UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnPlayerInventoryAdded;
			U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
		}

		public void QueuePlayer(UnturnedPlayer player)
		{
			if (!player.IsAdmin && !player.HasPermission(Config.ExemptPermission))
			{
				CheckQueue.Add(player);
			}
		}

		private void OnPlayerConnected(UnturnedPlayer player)
		{
			QueuePlayer(player);
		}

		private void OnPlayerInventoryAdded(UnturnedPlayer player, Rocket.Unturned.Enumerations.InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
		{
			// You can't just remove the item here because that can cause issues.
			QueuePlayer(player);
		}

		private void OnPlayerDisconnected(UnturnedPlayer player)
		{
			CheckQueue.Remove(player);
		}

		public void Update()
		{
			if (Time.time - LastCheck > Config.CheckInterval)
			{
				LastCheck = Time.time;
				foreach (var QueuedPlayer in CheckQueue)
				{
					CheckInventory(QueuedPlayer);
					CheckQueue.Remove(QueuedPlayer);
				}
			}
			//if (Level.isLoaded && Provider.clients.Count > 0)
			//{
			//	if (Time.time - LastCheck > Config.CheckInterval)
			//	{
			//		LastCheck = Time.time;
			//		foreach (var client in Provider.clients)
			//		{
			//			if (!UnturnedPlayer.FromSteamPlayer(client).IsAdmin && !UnturnedPlayer.FromSteamPlayer(client).HasPermission(Config.ExemptPermission))
			//			{
			//				CheckInventory(client.player);
			//			}
			//		}
			//	}
			//}
		}

		public void CheckInventory(UnturnedPlayer uPlayer)
		{
			var player = uPlayer.Player;
			try
			{
				// This might have issues if the clothing item isn't there or something.
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

				// This pretty much always causes a nullpointer or something.
				for (byte page = 0; page < PlayerInventory.PAGES; page++)
				{
					for (byte index = 0; index < player.inventory.getItemCount(page); index++)
					{
						if (!CanUseItem(uPlayer, player.inventory.getItem(page, index).item.id))
						{
							UnturnedChat.Say(uPlayer, Translate("restricteditem_removed", UnturnedItems.GetItemAssetById(player.inventory.getItem(page, index).item.id).itemName), Color.red);
							player.inventory.removeItem(page, index);
						}
					}
				}
			}
			catch { }
		}

		public bool CanUseItem(UnturnedPlayer player, ushort ID)
		{
			if (IsRestrictedItem(ID))
			{
				if (!AllRestrictedItems.ContainsKey(ID))
				{
					return false;
				}
				else
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
				}
			}
			else
			{
				return true;
			}
			return false;
		}

		public bool IsRestrictedItem(ushort ID)
		{
			if (ID == 0)
			{
				return false;
			}
			if (Config.UnlistedAreRestricted)
			{
				return true;
			}
			if (AllRestrictedItems.ContainsKey(ID))
			{
				return true;
			}
			return false;	
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