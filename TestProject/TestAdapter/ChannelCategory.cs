using CGZBot3.Interfaces;
using System.Collections.Generic;

namespace TestProject.TestAdapter
{
	
	internal class ChannelCategory : IChannelCategory
	{
		public ChannelCategory(string? name, Server server)
		{
			Name = name;
			if (name is not null) Id = server.BaseClient.GenerateId();
			BaseServer = server;
		}


		public string? Name { get; }

		public ulong? Id { get; }

		public IReadOnlyCollection<IChannel> Channels => (IReadOnlyCollection<IChannel>)BaseChannels;

		public IList<Channel> BaseChannels { get; } = new List<Channel>();

		public IServer Server => BaseServer;

		public Server BaseServer { get; }


		public bool Equals(IServerEntity? other) => other is IChannelCategory cat && Equals(cat);

		public bool Equals(IChannelCategory? other) => other is ChannelCategory cat && cat.Id == Id;
	}
}