﻿namespace DidiFrame.Clients
{
	/// <summary>
	/// Extensions for DidiFrame.Interfaces namespace
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Gets member on server from user
		/// </summary>
		/// <param name="server">Target server</param>
		/// <param name="user">User for search</param>
		/// <returns>Associated with given user member</returns>
		public static IMember GetMember(this IServer server, IUser user)
		{
			return server.GetMember(user.Id);
		}

		/// <summary>
		/// Tries cast IChannel object to ITextChannel. Can throw exception if fail
		/// </summary>
		/// <param name="channel">Uncasted ITextChannel</param>
		/// <returns>Casted ITextChannel</returns>
		/// <exception cref="InvalidCastException">If channel can't be casted to ITextChannel</exception>
		public static ITextChannelBase AsText(this IChannel channel) => (ITextChannelBase)channel;

		/// <summary>
		/// Tries cast IChannel object to IVoiceChannel. Can throw exception if fail
		/// </summary>
		/// <param name="channel">Uncasted IVoiceChannel</param>
		/// <returns>Casted IVoiceChannel</returns>
		/// <exception cref="InvalidCastException">If channel can't be casted to IVoiceChannel</exception>
		public static IVoiceChannel AsVoice(this IChannel channel) => (IVoiceChannel)channel;

		/// <summary>
		/// Tries cast IChannel object to IForumChannel. Can throw exception if fail
		/// </summary>
		/// <param name="channel">Uncasted IForumChannel</param>
		/// <returns>Casted IForumChannel</returns>
		/// <exception cref="InvalidCastException">If channel can't be casted to IForumChannel</exception>
		public static IForumChannel AsForum(this IChannel channel) => (IForumChannel)channel;

		/// <summary>
		/// Gets channel type
		/// </summary>
		/// <param name="channel">Target channel</param>
		/// <returns>Object of enum that associated with channel type ot null if channel has unspecified type</returns>
		public static ChannelType? GetChannelType(this IChannel channel)
		{
			return channel switch
			{
				ITextChannel => ChannelType.TextCompatible,
				IVoiceChannel => ChannelType.Voice,
				IForumChannel => ChannelType.Forum,
				_ => null,
			};
		}
	}
}
