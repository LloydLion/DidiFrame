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
	/// <summary>
	/// Extensions to work with DSharp's objects
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Converts DSharp permissions enum to DidiFrame permissions enum
		/// </summary>
		/// <param name="dPermissions">DSharp permissions enum</param>
		/// <returns>DidiFrame permissions enum</returns>
		public static AbsPermissions GetAbstract(this DPermissions dPermissions)
		{
			return (AbsPermissions)(long)dPermissions;
		}

		/// <summary>
		/// Converts DSharp button style to DidiFrame button style
		/// </summary>
		/// <param name="style">DSharp button style</param>
		/// <returns>DidiFrame button style</returns>
		public static AbsButtonStyle GetAbstract(this DButtonStyle style)
		{
			return (AbsButtonStyle)(int)style;
		}

		/// <summary>
		/// Converts DSharp channel type to DidiFrame channel type
		/// </summary>
		/// <param name="dChannelType">DSharp channel type</param>
		/// <returns>DidiFrame channel type</returns>
		public static AbsChannelType? GetAbstract(this DChannelType dChannelType)
		{
			return dChannelType switch
			{
				DChannelType.Text or DChannelType.Group or DChannelType.News or DChannelType.Private => AbsChannelType.TextCompatible,
				DChannelType.Voice => AbsChannelType.Voice,
				_ => null
			};
		}

		/// <summary>
		/// Converts DidiFrame permissions enum to DSharo permissions enum
		/// </summary>
		/// <param name="absPermissions">DiDiFrame permissions enum</param>
		/// <returns>DSharp permissions enum</returns>
		public static DPermissions GetDSharp(this AbsPermissions absPermissions)
		{
			return (DPermissions)(long)absPermissions;
		}

		/// <summary>
		/// Converts DidiFrame channel type to DSharp channel type
		/// </summary>
		/// <param name="absChannelType">DidiFram channel type</param>
		/// <returns>DSharp channel type</returns>
		/// <exception cref="NotSupportedException">Channel type is not supported by method</exception>
		public static DChannelType GetDSharp(this AbsChannelType absChannelType)
		{
			return absChannelType switch
			{
				AbsChannelType.TextCompatible => DChannelType.Text,
				AbsChannelType.Voice => DChannelType.Voice,
				_ => throw new NotSupportedException()
			};
		}

		/// <summary>
		/// Converts DidiFrame color to DSharp color
		/// </summary>
		/// <param name="color">DidiFrame color</param>
		/// <returns>DSharp color</returns>
		public static DiscordColor GetDSharp(this Color color)
		{
			return new DiscordColor(color.Red, color.Green, color.Blue);
		}

		/// <summary>
		/// Converts DidiFrame button style to DSharp button style
		/// </summary>
		/// <param name="style">DidiFrame button style</param>
		/// <returns>DSharp button style</returns>
		public static DButtonStyle GetDSharp(this AbsButtonStyle style)
		{
			return (DButtonStyle)(int)style;
		}
	}
}
