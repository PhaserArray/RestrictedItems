using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PhaserArray.RestrictedItems
{
	public class RestrictedItemsConfiguration : IRocketPluginConfiguration
	{
		public bool UnlistedAreRestricted;

		public float CheckInterval;

		public string ExemptPermission;

		[XmlArrayItem(ElementName = "RestrictedItemGroup")]
		public List<RestrictedItemGroup> RestrictedItemGroups;
			
		public void LoadDefaults()
		{
			UnlistedAreRestricted = false;

			CheckInterval = 1.0f;

			ExemptPermission = "restricteditems.exempt";

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
								"restricteditems.shadowstalker"
							}
						),
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"restricteditems.railguns",
								"restricteditems.weapons"
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
								"restricteditems.fishingrods",
								"restricteditems.tools"
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
								"restricteditems.railguns",
								"restricteditems.weapons",
								"restricteditems.special"
							}
						),
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"restricteditems.shadowstalkermkii"
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
								"restricteditems.fishingrods",
								"restricteditems.tools",
								"restricteditems.special"
							}
						),
						new RestrictedItemGroupPermissions(
							new List<string>
							{
								"restricteditems.upgradedfishingrod"
							}
						)
					}
				)
			};
		}
	}
}
