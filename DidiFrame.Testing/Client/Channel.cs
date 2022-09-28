using DidiFrame.Exceptions;
using DidiFrame.Clients;
using System.Runtime.CompilerServices;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test IChannel implementation
	/// </summary>
	public class Channel : IChannel, IServerDeletable
	{
		private readonly ChannelCategory baseCategory;
		private readonly string name;


		internal Channel(string name, ChannelCategory category)
		{
			this.name = name;
			baseCategory = category;
			Id = category.BaseServer.BaseClient.GenerateNextId();
		}


		/// <inheritdoc/>
		public string Name => GetIfExist(name);

		/// <inheritdoc/>
		public ulong Id { get; }

		/// <inheritdoc/>
		public IChannelCategory Category => GetIfExist(baseCategory);

		/// <inheritdoc/>
		public IServer Server => GetIfExist(baseCategory).BaseServer;

		/// <summary>
		/// Base server that contains it
		/// </summary>
		public Server BaseServer => GetIfExist(baseCategory).BaseServer;

		/// <summary>
		/// Base category that contains channel
		/// </summary>
		public ChannelCategory BaseCategory => GetIfExist(baseCategory);

		/// <inheritdoc/>
		public bool IsExist { get; private set; } = true;


		/// <inheritdoc/>
		public Task DeleteAsync()
		{
			BaseServer.DeleteChannel(this);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => other is IChannel channel && Equals(channel);

		/// <inheritdoc/>
		public bool Equals(IChannel? other) => other is Channel channel && channel.Id == Id;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as IChannel);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		void IServerDeletable.DeleteInternal() => IsExist = true;

		private protected TValue GetIfExist<TValue>(TValue value, [CallerMemberName] string nameOfCaller = "")
		{
			if (IsExist == false)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return value;
		}
	}
}
