using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PhaserArray.RestrictedItems
{
	public class RestrictedItemsConfiguration : IRocketPluginConfiguration
	{
		public bool AdminsExempt = false;

		[XmlArrayItem(ElementName = "RestrictedItemGroup")]
		public List<RestrictedItemGroup> RestrictedItemGroups;
			
		public void LoadDefaults()
		{
			RestrictedItemGroups = new List<RestrictedItemGroup>()
			{
				new RestrictedItemGroup(
					new List<ushort>()
					{
						300,
						302
					},
					new List<RestrictedItemGroupPermissions>()
					{
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"RestrictedItems.Shadowstalker"
							}
						),
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"RestrictedItems.Railguns",
								"RestrictedItems.Weapons"
							}
						)
					}
				),
				new RestrictedItemGroup(
					new List<ushort>()
					{
						506,
						503,
						507,
						508
					},
					new List<RestrictedItemGroupPermissions>()
					{
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"RestrictedItems.FishingRods",
								"RestrictedItems.Tools"
							}
						)
					}
				),
				new RestrictedItemGroup(
					new List<ushort>()
					{
						1441,
						1442,
						1443
					},
					new List<RestrictedItemGroupPermissions>()
					{
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"RestrictedItems.Railguns",
								"RestrictedItems.Weapons",
								"RestrictedItems.Special"
							}
						),
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"RestrictedItems.ShadowstalkerMkII"
							}
						)
					}
				),
				new RestrictedItemGroup(
					new List<ushort>()
					{
						1432
					},
					new List<RestrictedItemGroupPermissions>()
					{
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"RestrictedItems.FishingRods",
								"RestrictedItems.Tools",
								"RestrictedItems.Special"
							}
						),
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"RestrictedItems.UpgradedFishingRod"
							}
						)
					}
				),
				new RestrictedItemGroup(
					new List<ushort>()
					{
						300,
						302,
						1432,
						1441,
						1442,
						1443
					},
					new List<RestrictedItemGroupPermissions>()
					{
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"RestrictedItems.Everything"
							}
						)
					}
				)
			};
		}
	}
}
