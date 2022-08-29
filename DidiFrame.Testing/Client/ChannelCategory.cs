using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Clients;
using System.Runtime.CompilerServices;

namespace DidiFrame.Testing.Client
{
	public class ChannelCategory : IChannelCategory, IServerDeletable
	{
		private readonly string? name;
		private readonly ICollection<Channel> baseChannels = new List<Channel>();


		internal ChannelCategory(string? name, Server server)
		{
			this.name = name;
			if (name is not null) Id = server.BaseClient.GenerateNextId();
			BaseServer = server;
		}


		public string? Name => GetIfExist(name);

		public ulong? Id { get; }

		public IReadOnlyCollection<IChannel> Channels => (IReadOnlyCollection<IChannel>)GetIfExist(baseChannels);

		public IReadOnlyCollection<Channel> BaseChannels => (IReadOnlyCollection<Channel>)GetIfExist(baseChannels);

		public IServer Server => BaseServer;

		public Server BaseServer { get; }

		public bool IsExist { get; private set; } = true;


		public Task<IChannel> CreateChannelAsync(ChannelCreationModel creationModel)
		{
			var channel = GetIfExist(BaseServer).AddChannel(this, creationModel);

			return Task.FromResult((IChannel)channel);
		}

		public bool Equals(IServerEntity? other) => other is IChannelCategory cat && Equals(cat);

		public bool Equals(IChannelCategory? other) => other is ChannelCategory cat && cat.Id == Id;

		public override bool Equals(object? obj) => Equals(obj as IChannelCategory);

		public override int GetHashCode() => Id.GetHashCode();

		void IServerDeletable.DeleteInternal()
		{
			IsExist = false;
		}

		internal void AddChannel(Channel channel)
		{
			GetIfExist(baseChannels).Add(channel);
		}

		internal void DeleteChannel(Channel channel)
		{
			GetIfExist(baseChannels).Remove(channel);
		}

		private TValue GetIfExist<TValue>(TValue value, [CallerMemberName] string nameOfCaller = "")
		{
			if (IsExist == false)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return value;
		}
	}
}