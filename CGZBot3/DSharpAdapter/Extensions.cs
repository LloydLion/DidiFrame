using DPermissions = DSharpPlus.Permissions;
using AbsPermissions = CGZBot3.Entities.Permissions;

namespace CGZBot3.DSharpAdapter
{
	internal static class Extensions
	{
		public static AbsPermissions GetAbstract(this DPermissions dPermissions)
		{
			return (AbsPermissions)(long)dPermissions;
		}

		public static DPermissions GetDSharp(this AbsPermissions absPermissions)
		{
			return (DPermissions)(long)absPermissions;
		}
	}
}
