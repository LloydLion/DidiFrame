using CGZBot3.Interfaces;
using System.Threading.Tasks;

namespace TestProject.TestAdapter
{
	internal class Channel : IChannel
	{
		public Channel(string name, ChannelCategory category)
		{
			Name = name;
			BaseCategory = category;
			Id = category.BaseServer.BaseClient.GenerateId();
		}


		public string Name { get; }

		public ulong Id { get; }

		public IChannelCategory Category => BaseCategory;

		public IServer Server => BaseServer;

		public Server BaseServer => BaseCategory.BaseServer;

		public ChannelCategory BaseCategory { get; }

		public Task DeleteAsync()
		{
			BaseCategory.BaseChannels.Remove(this);
			return Task.CompletedTask;
		}


		public bool Equals(IServerEntity? other) => other is IChannel channel && Equals(channel);

		public bool Equals(IChannel? other) => other is Channel channel && channel.Id == Id;
	}
}
