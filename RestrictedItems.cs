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

namespace PhaserArray.RestrictedItems
{
	public class RestrictedItems : RocketPlugin<RestrictedItemsConfiguration>
	{
		private static RestrictedItems Instance;
		private static RestrictedItemsConfiguration Config;

		private float LastCheck;

		private Dictionary<ushort, List<List<string>>> AllRestrictedItems;

		protected override void Load()
		{
			Instance = this;
			Config = Instance.Configuration.Instance;

			LastCheck = Time.time;

			AllRestrictedItems = new Dictionary<ushort, List<List<string>>>();

			// Loads the config into a nicely indexable lowercase dictionary.
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
			Logger.Log("Loaded " + AllRestrictedItems.Count.ToString() + " restricted items!");

			Logger.Log("Plugin Loaded!");
		}
		
		public void Update()
		{
			if (Level.isLoaded && Provider.clients.Count > 0)
			{
				if (Time.time - LastCheck > Config.CheckInterval)
				{
					LastCheck = Time.time;
					foreach (var client in Provider.clients)
					{
						if (!UnturnedPlayer.FromSteamPlayer(client).IsAdmin && !UnturnedPlayer.FromSteamPlayer(client).HasPermission(Config.ExemptPermission))
						{
							CheckInventory(client.player);
						}
					}
				}
			}
		}

		public void CheckInventory(Player player)
		{
			var uPlayer = UnturnedPlayer.FromPlayer(player);

			// This might have issues if the clothing item isn't there or something.
			try
			{
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
			}
			catch { }

			// This pretty much always causes a nullpointer or something.
			try
			{
				for (byte page = 0; page < PlayerInventory.PAGES; page++)
				{
					for (byte index = 0; index < player.inventory.getItemCount(page); index++)
					{
						if (!CanUseItem(uPlayer, player.inventory.getItem(page, index).item.id))
						{
							UnturnedChat.Say(uPlayer, Instance.Translate("restricteditem_removed", UnturnedItems.GetItemAssetById(player.inventory.getItem(page, index).item.id).itemName), Color.red);
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
						if (player.HasPermissions(PermissionGroup))
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