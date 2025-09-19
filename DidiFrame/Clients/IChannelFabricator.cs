namespace DidiFrame.Clients
{
	public interface IChannelFabricator
	{
		public ValueTask<ITextChannel> Text(string name);

		public ValueTask<IVoiceChannel> Voice(string name);

		public ValueTask<IStageChannel> Stage(string name);

		public ValueTask<INewsChannel> News(string name);

		public ValueTask<IForumChannel> Forum(string name);
	}
}
