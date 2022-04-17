using CGZBot3.Entities;
using CGZBot3.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestProject.Environment.Client
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


		public Task<IChannel> CreateChannelAsync(ChannelCreationModel creationModel)
		{
			var channel = creationModel.ChannelType switch
			{
				ChannelType.TextCompatible => new TextChannel(creationModel.Name, this),
				ChannelType.Voice => new VoiceChannel(creationModel.Name, this),
				_ => new Channel(creationModel.Name, this)
			};

			BaseChannels.Add(channel);

			return Task.FromResult((IChannel)channel);
		}

		public bool Equals(IServerEntity? other) => other is IChannelCategory cat && Equals(cat);

		public bool Equals(IChannelCategory? other) => other is ChannelCategory cat && cat.Id == Id;
	}
}