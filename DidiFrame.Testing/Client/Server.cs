using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace DidiFrame.Testing.Client
{
	public class Server : IServer
	{
		private readonly static EventId EventHandlerErrorID = new(19, "EventHandlerError");


		private readonly List<Role> roles = new();
		private readonly List<Member> members = new();
		private readonly List<ChannelCategory> cats = new();
		private readonly Client baseClient;

		public event MessageSentEventHandler? MessageSent;
		public event MessageDeletedEventHandler? MessageDeleted;
		public event ServerObjectDeletedEventHandler? ChannelDeleted;
		public event ServerObjectDeletedEventHandler? MemberDeleted;
		public event ServerObjectDeletedEventHandler? RoleDeleted;
		public event ServerObjectDeletedEventHandler? CategoryDeleted;
		public event ServerObjectCreatedEventHandler<IChannel>? ChannelCreated;
		public event ServerObjectCreatedEventHandler<IMember>? MemberCreated;
		public event ServerObjectCreatedEventHandler<IRole>? RoleCreated;
		public event ServerObjectCreatedEventHandler<IChannelCategory>? CategoryCreated;


		public IClient Client { get { ThrowIfClosed(); return baseClient; } }

		public Client BaseClient { get { ThrowIfClosed(); return baseClient; } }

		public string Name { get; }

		public ulong Id { get; }


		public IReadOnlyCollection<Role> Roles => roles;

		public IReadOnlyCollection<Member> Members => members;

		public IReadOnlyCollection<ChannelCategory> Categories => cats;

		public bool IsClosed { get; private set; } = false;


		public Server(Client client, string name)
		{
			baseClient = client;
			Name = name;
			AddMember(client.BaseSelfAccount, Permissions.All);
			cats.Add(new ChannelCategory(null, this));
			Id = client.GenerateId();
		}


		public bool Equals(IServer? other) => other is Server server && !IsClosed && !server.IsClosed && server.Id == Id;

		public Member AddMember(User user, Permissions permissions)
		{
			ThrowIfClosed();
			var member = new Member(this, user, permissions);
			members.Add(member);

			try { MemberCreated?.Invoke(member, false); }
			catch (Exception ex)
			{ baseClient.Logger.Log(LogLevel.Error, EventHandlerErrorID, ex, "Error in event handler for IMember ({EntityId}) creation in {ServerName}", member.Id, Name); }

			return member;
		}

		public void AddRole(Role role)
		{
			ThrowIfClosed();
			roles.Add(role);
			try { RoleCreated?.Invoke(role, false); }
			catch (Exception ex)
			{ baseClient.Logger.Log(LogLevel.Error, EventHandlerErrorID, ex, "Error in event handler for IRole ({EntityId}) creation in {ServerName}", role.Id, Name); }
		}

		public void AddChannel(ChannelCategory category, Channel channel)
		{
			ThrowIfClosed();
			if (channel is not ITextThread)
			{
				category.AddChannel(channel);
				try { ChannelCreated?.Invoke(channel, false); }
				catch (Exception ex)
				{ baseClient.Logger.Log(LogLevel.Error, EventHandlerErrorID, ex, "Error in event handler for IChannel ({EntityId}) creation in {ServerName}", channel.Id, Name); }
			}
			else throw new ArgumentException("Enable to add thread channel", nameof(channel));
		}

		public void AddChannel(TextChannel textChannel, TextThread thread)
		{
			ThrowIfClosed();
			textChannel.AddThreadInternal(thread);
			try { ChannelCreated?.Invoke(thread, false); }
			catch (Exception ex)
			{ baseClient.Logger.Log(LogLevel.Error, EventHandlerErrorID, ex, "Error in event handler for IChannel ({EntityId}) creation in {ServerName}", thread.Id, Name); }
		}

		public void AddCategory(ChannelCategory category)
		{
			ThrowIfClosed();
			cats.Add(category);
			try { CategoryCreated?.Invoke(category, false); }
			catch (Exception ex)
			{ baseClient.Logger.Log(LogLevel.Error, EventHandlerErrorID, ex, "Error in event handler for IChannelCategory ({EntityId}) creation in {ServerName}", category.Id, Name); }
		}

		public void DeleteMember(Member member)
		{
			ThrowIfClosed();
			members.Remove(member);
			DeleteObject(member);

			try { MemberDeleted?.Invoke(this, member.Id); }
			catch (Exception ex)
			{ baseClient.Logger.Log(LogLevel.Error, EventHandlerErrorID, ex, "Error in event handler for IMember ({EntityId}) creation in {ServerName}", member.Id, Name); }
		}

		public void DeleteRole(Role role)
		{
			ThrowIfClosed();
			roles.Add(role);
			DeleteObject(role);

			try { RoleCreated?.Invoke(role, false); }
			catch (Exception ex)
			{ baseClient.Logger.Log(LogLevel.Error, EventHandlerErrorID, ex, "Error in event handler for IRole ({EntityId}) creation in {ServerName}", role.Id, Name); }
		}

		public void DeleteChannel(Channel channel)
		{
			ThrowIfClosed();
			if (channel is TextThread thread)
				thread.BaseParent.DeleteThreadInternal(thread);
			else
				channel.BaseCategory.DeleteChannel(channel);

			DeleteObject(channel);

			try { ChannelDeleted?.Invoke(this, channel.Id); }
			catch (Exception ex)
			{ baseClient.Logger.Log(LogLevel.Error, EventHandlerErrorID, ex, "Error in event handler for IChannel ({EntityId}) creation in {ServerName}", channel.Id, Name); }
		}

		public void DeleteCategory(ChannelCategory category)
		{
			var id = category.Id ?? throw new ArgumentException("Enable to delete global category");

			ThrowIfClosed();
			cats.Remove(category);
			DeleteObject(category);

			try { CategoryDeleted?.Invoke(this, id); }
			catch (Exception ex)
			{ baseClient.Logger.Log(LogLevel.Error, EventHandlerErrorID, ex, "Error in event handler for IChannelCategory ({EntityId}) creation in {ServerName}", id, Name); }
		}

		private static void DeleteObject(IServerDeletable serverDeletable) => serverDeletable.DeleteInternal();

		public IReadOnlyCollection<IMember> GetMembers()
		{
			ThrowIfClosed();
			return members;
		}

		public IMember GetMember(ulong id)
		{
			ThrowIfClosed();
			return members.Single(s => s.Id == id);
		}

		public IReadOnlyCollection<IChannelCategory> GetCategories()
		{
			ThrowIfClosed();
			return cats;
		}

		public IChannelCategory GetCategory(ulong? id)
		{
			ThrowIfClosed();
			return cats.Single(s => s.Id == id);
		}

		public IReadOnlyCollection<IChannel> GetChannels()
		{
			ThrowIfClosed();
			return cats.SelectMany(s => s.Channels).ToArray();
		}

		public IChannel GetChannel(ulong id)
		{
			ThrowIfClosed();
			return cats.Single(s => s.Channels.Any(s => s.Id == id)).Channels.Single(s => s.Id == id);
		}

		public IReadOnlyCollection<IRole> GetRoles()
		{
			ThrowIfClosed();
			return roles;
		}

		public IRole GetRole(ulong id)
		{
			ThrowIfClosed();
			return roles.Single(s => s.Id == id);
		}

		internal void OnMessageDeleted(TextChannelBase textChannel, ulong messageId)
		{
			ThrowIfClosed();

			try
			{
				MessageDeleted?.Invoke(Client, textChannel, messageId);
			}
			catch (Exception ex)
			{

			}
		}

		internal void OnMessageCreated(Message message, bool isModified)
		{
			ThrowIfClosed();

			try
			{
				MessageSent?.Invoke(Client, message, isModified);
			}
			catch (Exception ex)
			{

			}
		}

		internal void DeleteInternal()
		{
			ThrowIfClosed();
			IsClosed = true;
		}

		private void ThrowIfClosed([CallerMemberName] string nameOfCaller = "")
		{
			if (IsClosed)
				throw new ObjectDoesNotExistException(nameOfCaller);
		}
	}
}