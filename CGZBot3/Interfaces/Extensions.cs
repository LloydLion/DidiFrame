namespace CGZBot3.Interfaces
{
	public static class Extensions
	{
		public static Task<IMember> GetMemberAsync(this IServer server, IUser user)
		{
			return server.GetMemberAsync(user.Id);
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
