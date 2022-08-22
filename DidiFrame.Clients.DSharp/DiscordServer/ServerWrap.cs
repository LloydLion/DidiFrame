using DidiFrame.Clients.DSharp;
using DidiFrame.Exceptions;
using DSharpPlus.Entities;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp.DiscordServer
{
	public class ServerWrap : IServer
	{
		private readonly DSharpClient client;
		private readonly ulong id;


		public ServerWrap(Server server)
		{
			client = server.SourceClient;
			id = server.Id;
		}


		public IClient Client => client;

		public DSharpClient SourceClient => client;

		public string Name => GetServer().Name;

		public ulong Id => id;

		public DiscordGuild Guild => GetServer().Guild;

		public bool IsClosed => client.GetRawServer(id) is null;


		public event MessageSentEventHandler? MessageSent
		{ add => GetServer().MessageSent += value; remove => GetServer().MessageSent -= value; }

		public event MessageDeletedEventHandler? MessageDeleted
		{ add => GetServer().MessageDeleted += value; remove => GetServer().MessageDeleted -= value; }

		public event ServerObjectDeletedEventHandler? ChannelDeleted
		{ add => GetServer().ChannelDeleted += value; remove => GetServer().ChannelDeleted -= value; }

		public event ServerObjectDeletedEventHandler? MemberDeleted
		{ add => GetServer().MemberDeleted += value; remove => GetServer().MemberDeleted -= value; }

		public event ServerObjectDeletedEventHandler? RoleDeleted
		{ add => GetServer().RoleDeleted += value; remove => GetServer().RoleDeleted -= value; }

		public event ServerObjectDeletedEventHandler? CategoryDeleted
		{ add => GetServer().CategoryDeleted += value; remove => GetServer().CategoryDeleted -= value; }

		public event ServerObjectCreatedEventHandler<IChannel>? ChannelCreated
		{ add => GetServer().ChannelCreated += value; remove => GetServer().ChannelCreated -= value; }

		public event ServerObjectCreatedEventHandler<IMember>? MemberCreated
		{ add => GetServer().MemberCreated += value; remove => GetServer().MemberCreated -= value; }

		public event ServerObjectCreatedEventHandler<IRole>? RoleCreated
		{ add => GetServer().RoleCreated += value; remove => GetServer().RoleCreated -= value; }

		public event ServerObjectCreatedEventHandler<IChannelCategory>? CategoryCreated
		{ add => GetServer().CategoryCreated += value; remove => GetServer().CategoryCreated -= value; }


		public bool Equals(IServer? other) => other is ServerWrap && id == other.Id && client == other.Client;

		public override bool Equals(object? obj) => Equals(obj as ServerWrap);

		public override int GetHashCode() => HashCode.Combine(client, id);

		public TExtension CreateExtension<TExtension>() where TExtension : class => GetServer().CreateExtension<TExtension>();

		public IReadOnlyCollection<IChannelCategory> GetCategories() => GetServer().GetCategories();

		public IChannelCategory GetCategory(ulong? id) => GetServer().GetCategory(id);

		public IChannel GetChannel(ulong id) => GetServer().GetChannel(id);

		public IReadOnlyCollection<IChannel> GetChannels() => GetServer().GetChannels();

		public IMember GetMember(ulong id) => GetServer().GetMember(id);

		public IReadOnlyCollection<IMember> GetMembers() => GetServer().GetMembers();

		public IRole GetRole(ulong id) => GetServer().GetRole(id);

		public IReadOnlyCollection<IRole> GetRoles() => GetServer().GetRoles();

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
