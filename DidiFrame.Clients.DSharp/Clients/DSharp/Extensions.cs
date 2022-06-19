using DPermissions = DSharpPlus.Permissions;
using AbsPermissions = DidiFrame.Entities.Permissions;
using DChannelType = DSharpPlus.ChannelType;
using AbsChannelType = DidiFrame.Entities.ChannelType;
using DButtonStyle= DSharpPlus.ButtonStyle;
using AbsButtonStyle = DidiFrame.Entities.Message.Components.ButtonStyle;
using DSharpPlus.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Entities;

namespace DidiFrame.Clients.DSharp
{
	public static class Extensions
	{
		public static AbsPermissions GetAbstract(this DPermissions dPermissions)
		{
			return (AbsPermissions)(long)dPermissions;
		}

		public static DPermissions GetDSharp(this AbsPermissions absPermissions)
		{
			return (DPermissions)(long)absPermissions;
		}

		public static DChannelType GetDSharp(this AbsChannelType absChannelType)
		{
			return absChannelType switch
			{
				AbsChannelType.TextCompatible => DChannelType.Text,
				AbsChannelType.Voice => DChannelType.Voice,
				_ => throw new ImpossibleVariantException()
			};
		}

		public static AbsChannelType? GetAbstract(this DChannelType dChannelType)
		{
			return dChannelType switch
			{
				DChannelType.Text or DChannelType.Group or DChannelType.News or	DChannelType.Private => AbsChannelType.TextCompatible,
				DChannelType.Voice => AbsChannelType.Voice,
				_ => null
			};
		}

		public static DiscordColor GetDSharp(this Color color)
		{
			return new DiscordColor(color.Red, color.Green, color.Blue);
		}

		public static DButtonStyle GetDSharp(this AbsButtonStyle style)
		{
			return (DButtonStyle)(int)style;
		}

		public static AbsButtonStyle GetAbstract(this DButtonStyle style)
		{
			return (AbsButtonStyle)(int)style;
		}
	}
}
