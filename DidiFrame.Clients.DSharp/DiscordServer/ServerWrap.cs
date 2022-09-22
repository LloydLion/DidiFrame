using DidiFrame.Exceptions;
using DSharpPlus.Entities;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp.DiscordServer
{
	/// <summary>
	/// Wrap between real server object and other dshap wraps
	/// </summary>
	public class ServerWrap : IServer
	{
		private readonly DSharpClient client;
		private readonly ulong id;


		/// <summary>
		/// Creates new instance of server wrap from real server
		/// </summary>
		/// <param name="server">Real server object</param>
		public ServerWrap(Server server)
		{
			client = server.SourceClient;
			id = server.Id;
		}


		/// <inheritdoc/>
		public IClient Client => client;

		/// <summary>
		/// Source client that contains this server
		/// </summary>
		public DSharpClient SourceClient => client;

		/// <inheritdoc/>
		public string Name => GetServer().Name;

		/// <inheritdoc/>
		public ulong Id => id;

		/// <summary>
		/// This server raw DSharpPlus object
		/// </summary>
		public DiscordGuild Guild => GetServer().Guild;

		/// <inheritdoc/>
		public bool IsClosed => client.GetRawServer(id) is null;


		/// <inheritdoc/>
		public event MessageSentEventHandler? MessageSent
		{ add => GetServer().MessageSent += value; remove => GetServer().MessageSent -= value; }

		/// <inheritdoc/>
		public event MessageDeletedEventHandler? MessageDeleted
		{ add => GetServer().MessageDeleted += value; remove => GetServer().MessageDeleted -= value; }

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? ChannelDeleted
		{ add => GetServer().ChannelDeleted += value; remove => GetServer().ChannelDeleted -= value; }

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? MemberDeleted
		{ add => GetServer().MemberDeleted += value; remove => GetServer().MemberDeleted -= value; }

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? RoleDeleted
		{ add => GetServer().RoleDeleted += value; remove => GetServer().RoleDeleted -= value; }

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? CategoryDeleted
		{ add => GetServer().CategoryDeleted += value; remove => GetServer().CategoryDeleted -= value; }

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IChannel>? ChannelCreated
		{ add => GetServer().ChannelCreated += value; remove => GetServer().ChannelCreated -= value; }

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IMember>? MemberCreated
		{ add => GetServer().MemberCreated += value; remove => GetServer().MemberCreated -= value; }

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IRole>? RoleCreated
		{ add => GetServer().RoleCreated += value; remove => GetServer().RoleCreated -= value; }

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IChannelCategory>? CategoryCreated
		{ add => GetServer().CategoryCreated += value; remove => GetServer().CategoryCreated -= value; }


		/// <inheritdoc/>
		public bool Equals(IServer? other) => other is ServerWrap && id == other.Id && client == other.Client;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as ServerWrap);

		/// <inheritdoc/>
		public override int GetHashCode() => HashCode.Combine(client, id);

		/// <inheritdoc/>
		public TExtension CreateExtension<TExtension>() where TExtension : class => GetServer().CreateExtension<TExtension>();

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannelCategory> GetCategories() => GetServer().GetCategories();

		/// <inheritdoc/>
		public IChannelCategory GetCategory(ulong? id) => GetServer().GetCategory(id);

		/// <inheritdoc/>
		public IChannel GetChannel(ulong id) => GetServer().GetChannel(id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannel> GetChannels() => GetServer().GetChannels();

		/// <inheritdoc/>
		public IMember GetMember(ulong id) => GetServer().GetMember(id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IMember> GetMembers() => GetServer().GetMembers();

		/// <inheritdoc/>
		public IRole GetRole(ulong id) => GetServer().GetRole(id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IRole> GetRoles() => GetServer().GetRoles();

		/// <summary>
		/// Creates temporal interaction dispatcher for given message
		/// </summary>
		/// <param name="message">Unregisted message</param>
		/// <returns>New temporal interaction dispatcher</returns>
		public IInteractionDispatcher CreateTemporalDispatcherForUnregistedMessage(DiscordMessage message) => GetServer().CreateTemporalDispatcherForUnregistedMessage(message);

		internal void AddMessageSentEventHandler(DiscordChannel channel, MessageSentEventHandler? handler) => GetServer().AddMessageSentEventHandler(channel, handler);

		internal void AddMessageDeletedEventHandler(DiscordChannel channel, MessageDeletedEventHandler? handler) => GetServer().AddMessageDeletedEventHandler(channel, handler);

		internal void RemoveMessageSentEventHandler(DiscordChannel channel, MessageSentEventHandler? handler) => GetServer().RemoveMessageSentEventHandler(channel, handler);

		internal void RemoveMessageDeletedEventHandler(DiscordChannel channel, MessageDeletedEventHandler? handler) => GetServer().RemoveMessageDeletedEventHandler(channel, handler);

		internal IChannelMessagesCache GetMessagesCache() => GetServer().GetMessagesCache();

		internal IReadOnlyCollection<ITextThread> GetThreadsFor(TextChannel channel) => GetServer().GetThreadsFor(channel);

		internal IInteractionDispatcher GetInteractionDispatcherFor(Message message) => GetServer().GetInteractionDispatcherFor(message);

		internal void ResetInteractionDispatcherFor(ulong messageId, DiscordChannel channel) => GetServer().ResetInteractionDispatcherFor(messageId, channel);

		internal void CacheChannel(DiscordChannel channel) => GetServer().CacheChannel(channel);

		internal void CacheMessage(DiscordMessage message) => GetServer().CacheMessage(message);

		private Server GetServer([CallerMemberName] string nameOfCaller = "")
		{
			return client.GetRawServer(id) ?? throw new ObjectDoesNotExistException(nameOfCaller);
		}
	}
}
