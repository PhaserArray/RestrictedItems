using System.Collections.Generic;
using System.Xml.Serialization;

namespace PhaserArray.RestrictedItems
{
	public class RestrictedItemGroup
	{
		public RestrictedItemGroup() { }

		public RestrictedItemGroup(List<ushort> itemIDs, List<RestrictedItemGroupPermissions> permissionGroups)
		{
			IDs = itemIDs;
			PermissionGroups = permissionGroups;
		}

		[XmlArrayItem(ElementName = "ID")]
		public List<ushort> IDs;

		[XmlArrayItem(ElementName = "PermissionGroup")]
		public List<RestrictedItemGroupPermissions> PermissionGroups;
	}

	public class RestrictedItemGroupPermissions
	{
		public RestrictedItemGroupPermissions() { }

		public RestrictedItemGroupPermissions(List<string> permissions)
		{
			Permissions = permissions;
		}

		[XmlElement(ElementName = "Permission")]
		public List<string> Permissions;
	}
}
