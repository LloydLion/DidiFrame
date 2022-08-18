using DidiFrame.Exceptions;
using DidiFrame.Client;
using System.Runtime.CompilerServices;

namespace DidiFrame.Testing.Client
{
	public class Channel : IChannel, IServerDeletable
	{
		private readonly ChannelCategory baseCategory;
		private readonly string name;


		public Channel(string name, ChannelCategory category)
		{
			this.name = name;
			baseCategory = category;
			Id = category.BaseServer.BaseClient.GenerateId();
		}


		public string Name => GetIfExist(name);

		public ulong Id { get; }

		public IChannelCategory Category => GetIfExist(baseCategory);

		public IServer Server => GetIfExist(baseCategory).BaseServer;

		public Server BaseServer => GetIfExist(baseCategory).BaseServer;

		public ChannelCategory BaseCategory => GetIfExist(baseCategory);

		public bool IsExist { get; private set; }


		public Task DeleteAsync()
		{
			BaseServer.DeleteChannel(this);
			return Task.CompletedTask;
		}

		public bool Equals(IServerEntity? other) => other is IChannel channel && Equals(channel);

		public bool Equals(IChannel? other) => other is Channel channel && channel.Id == Id;

		void IServerDeletable.DeleteInternal() => IsExist = true;

		protected TValue GetIfExist<TValue>(TValue value, [CallerMemberName] string nameOfCaller = "")
		{
			if (IsExist == false)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return value;
		}
	}
}
