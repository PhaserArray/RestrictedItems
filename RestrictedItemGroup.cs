using System.Collections.Generic;
using System.Xml.Serialization;

namespace PhaserArray.RestrictedItems
{
	public class RestrictedItemGroup
	{
		public RestrictedItemGroup() { }

		public RestrictedItemGroup(List<ushort> IDs, List<RestrictedItemGroupPermissions> PermissionGroups)
		{
			this.IDs = IDs;
			this.PermissionGroups = PermissionGroups;
		}

		[XmlArrayItem(ElementName = "ID")]
		public List<ushort> IDs;

		[XmlArrayItem(ElementName = "PermissionGroup")]
		public List<RestrictedItemGroupPermissions> PermissionGroups;
	}

	public class RestrictedItemGroupPermissions
	{
		public RestrictedItemGroupPermissions() { }

		public RestrictedItemGroupPermissions(List<string> Permissions)
		{
			this.Permissions = Permissions;
		}

		[XmlElement(ElementName = "Permission")]
		public List<string> Permissions;
	}
}
