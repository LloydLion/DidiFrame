using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Clients;
using System.Runtime.CompilerServices;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test IChannelCategory implementation
	/// </summary>
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


		/// <inheritdoc/>
		public string? Name => GetIfExist(name);

		/// <inheritdoc/>
		public ulong? Id { get; }

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannel> Channels => (IReadOnlyCollection<IChannel>)GetIfExist(baseChannels);

		/// <summary>
		/// Colleciton of base channel that associated with this category
		/// </summary>
		public IReadOnlyCollection<Channel> BaseChannels => (IReadOnlyCollection<Channel>)GetIfExist(baseChannels);

		/// <inheritdoc/>
		public IServer Server => BaseServer;

		/// <summary>
		/// Base server that contains it
		/// </summary>
		public Server BaseServer { get; }

		/// <inheritdoc/>
		public bool IsExist { get; private set; } = true;


		/// <inheritdoc/>
		public Task<IChannel> CreateChannelAsync(ChannelCreationModel creationModel)
		{
			var channel = GetIfExist(BaseServer).AddChannel(this, creationModel);

			return Task.FromResult((IChannel)channel);
		}

		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => other is IChannelCategory cat && Equals(cat);

		/// <inheritdoc/>
		public bool Equals(IChannelCategory? other) => other is ChannelCategory cat && cat.Id == Id;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as IChannelCategory);

		/// <inheritdoc/>
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