namespace TestBot.Systems.Test.ClientExtensions.NewsChannels
{
	internal interface INewsChannelExtension
	{
		public bool CheckIfIsNewsChannel(ITextChannel channel);

		public INewsChannel AsNewsChannel(ITextChannel channel);
	}
}
