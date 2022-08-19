namespace TestBot.Systems.Test.ClientExtensions.NewsChannels
{
	internal static class ExtensionsMethods
	{
		public static INewsChannel AsNewsChannel(this ITextChannel channel)
		{
			return channel.Server.CreateExtension<INewsChannelExtension>().AsNewsChannel(channel);
		}

		public static bool CheckIfIsNewsChannel(this ITextChannel channel)
		{
			return channel.Server.CreateExtension<INewsChannelExtension>().CheckIfIsNewsChannel(channel);
		}
	}
}
