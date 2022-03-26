namespace CGZBot3.Interfaces
{
	public static class Extensions
	{
		public static IMember GetMember(this IServer server, IUser user)
		{
			return server.GetMember(user.Id);
		}

		public static ITextChannel AsText(this IChannel channel) => (ITextChannel)channel;

		public static IVoiceChannel AsVoice(this IChannel channel) => (IVoiceChannel)channel;

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
