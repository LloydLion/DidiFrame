namespace DidiFrame.Interfaces
{
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
		public static ITextChannel AsText(this IChannel channel) => (ITextChannel)channel;

		/// <summary>
		/// Tries cast IChannel object to IVoiceChannel. Can throw exception if fail
		/// </summary>
		/// <param name="channel">Uncasted IVoiceChannel</param>
		/// <returns>Casted IVoiceChannel</returns>
		/// <exception cref="InvalidCastException">If channel can't be casted to IVoiceChannel</exception>
		public static IVoiceChannel AsVoice(this IChannel channel) => (IVoiceChannel)channel;

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
				_ => null,
			};
		}
	}
}
