using System;
using Steamworks;
using Rocket.API;
using UnityEngine;
using SDG.Unturned;
using Rocket.Unturned;
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
		private const string version = "v1.3";

		private RestrictedItemsConfiguration Config;
		private Dictionary<ushort, List<List<string>>> AllRestrictedItems;

		private Dictionary<CSteamID, float> CheckQueue;

		protected override void Load()
		{
			Config = Configuration.Instance;
			AllRestrictedItems = new Dictionary<ushort, List<List<string>>>();

			CheckQueue = new Dictionary<CSteamID, float>();

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
			if (CheckQueue.Count > 0)
			{
				var keys = new List<CSteamID>(CheckQueue.Keys);
				foreach (var key in keys)
				{
					if (CheckQueue[key] <= Time.time)
					{
						var player = UnturnedPlayer.FromCSteamID(key);
						if (player.Player != null)
						{
							if (!IsExempt(player))
							{
								CheckInventory(player);
							}
						}
						CheckQueue.Remove(key);
					}
				}
			}
		}

		// If the player logs in,
		// it will queue an inventory search.
		private void OnPlayerConnected(UnturnedPlayer player)
		{
			QueuePlayerCheck(player);
		}

		// If a player wears anything restricted,
		// it will queue an inventory search.
		private void OnPlayerWear(UnturnedPlayer player, UnturnedPlayerEvents.Wearables wear, ushort id, byte? quality)
		{
			if (IsRestrictedItem(id))
			{
				QueuePlayerCheck(player);
			}
		}

		// If player picks up anything restricted,
		// it will queue an inventory search.
		private void OnPlayerInventoryAdded(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar itemJar)
		{
			if (IsRestrictedItem(itemJar.item.id))
			{
				QueuePlayerCheck(player);
			}
			else if (UnturnedItems.GetItemAssetById(itemJar.item.id).GetType() == typeof(ItemGunAsset))
			{
				for (int i = 0; i <= 8; i += 2)
				{
					if (IsRestrictedItem(BitConverter.ToUInt16(itemJar.item.state, i)))
					{
						QueuePlayerCheck(player);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Queues the player for an inventory check.
		/// </summary>
		/// <param name="player"></param>
		public void QueuePlayerCheck(UnturnedPlayer player)
		{
			if (!CheckQueue.ContainsKey(player.CSteamID))
			{
				// I am only using CSteamID for this because
				// for some reason using the player as the key
				// causes some weird issues with ContainsKey intermittently
				// returning false when it clearly does contain it.
				// ¯\_(ツ)_/¯
				CheckQueue.Add(player.CSteamID, Time.time + Config.RemoveDelay);
			}
		}

		/// <summary>
		/// Scans the player's clothing, items and weapon attachments. Will unequip clothing and remove any illegal items and attachments.
		/// </summary>
		/// <param name="player"></param>
		public void CheckInventory(UnturnedPlayer player)
		{
			var clothing = player.Player.clothing;

			if (!CanUseItem(player, clothing.backpack))
			{
				clothing.askWearBackpack(0, 0, new byte[0], false);
			}
			if (!CanUseItem(player, clothing.vest))
			{
				clothing.askWearVest(0, 0, new byte[0], false);
			}
			if (!CanUseItem(player, clothing.shirt))
			{
				clothing.askWearShirt(0, 0, new byte[0], false);
			}
			if (!CanUseItem(player, clothing.pants))
			{
				clothing.askWearPants(0, 0, new byte[0], false);
			}
			if (!CanUseItem(player, clothing.hat))
			{
				clothing.askWearHat(0, 0, new byte[0], false);
			}
			if (!CanUseItem(player, clothing.mask))
			{
				clothing.askWearMask(0, 0, new byte[0], false);
			}
			if (!CanUseItem(player, clothing.glasses))
			{
				clothing.askWearGlasses(0, 0, new byte[0], false);
			}

			for (byte page = 0; page < PlayerInventory.PAGES - 1; page++)
			{
				for (byte index = player.Inventory.getItemCount(page); index --> 0;)
				{
					var itemJar = player.Inventory.getItem(page, index);

					if (!CanUseItem(player, itemJar.item.id))
					{
						UnturnedChat.Say(player, Translate("restricteditem_remove_item", UnturnedItems.GetItemAssetById(itemJar.item.id).itemName), Color.red);
						player.Inventory.removeItem(page, index);
					}
					else if (UnturnedItems.GetItemAssetById(itemJar.item.id).GetType() == typeof(ItemGunAsset))
					{
						for (int i = 0; i <= 8; i += 2)
						{
							var id = BitConverter.ToUInt16(itemJar.item.state, i);

							if (!CanUseItem(player, id))
							{
								if (player.Player.equipment.itemID == itemJar.item.id && player.Player.equipment.equippedPage == page)
								{
									player.Player.equipment.dequip();
								}

								itemJar.item.state[i] = 0;
								itemJar.item.state[i + 1] = 0;
								if (i == 8)
								{
									itemJar.item.state[10] = 0;
								}

								UnturnedChat.Say(player, Translate("restricteditem_remove_attachment", UnturnedItems.GetItemAssetById(id).itemName), Color.red);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns true if the player is allowed to use the item.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool CanUseItem(UnturnedPlayer player, ushort id)
		{
			if (IsRestrictedItem(id))
			{
				List<List<string>> PermissionGroups;
				if (AllRestrictedItems.TryGetValue(id, out PermissionGroups))
				{
					foreach (var PermissionGroup in PermissionGroups)
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

		/// <summary>
		/// Returns true if the item is restricted.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool IsRestrictedItem(ushort id)
		{
			if (id == 0)
			{
				return false;
			}
			if (Config.UnlistedAreRestricted)
			{
				return true;
			}
			if (AllRestrictedItems.ContainsKey(id))
			{
				return true;
			}
			return false;	
		}

		/// <summary>
		/// Returns true if the player is exempt from the plugin's effects.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool IsExempt(UnturnedPlayer player)
		{
			return (player.IsAdmin || player.HasPermission(Config.ExemptPermission));
		}

		public override TranslationList DefaultTranslations
		{
			get
			{
				return new TranslationList()
				{
					{"restricteditem_remove_item", "Item not permitted: {0}"},
					{"restricteditem_remove_attachment", "Attachment not permitted: {0}"}
				};
			}
		}
	}
}