using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Clients;
using System.Runtime.CompilerServices;
using DidiFrame.ClientExtensions;

namespace DidiFrame.Testing.Client
{
	/// <inheritdoc/>
	public class Server : IServer
	{
		private readonly Dictionary<ulong, Role> roles = new();
		private readonly Dictionary<ulong, Member> members = new();
		private readonly Dictionary<ulong, ChannelCategory> cats = new();
		private readonly Client baseClient;
		private readonly ExtensionContextFactory extensionContextFactory = new();


		/// <inheritdoc/>
		public event MessageSentEventHandler? MessageSent;

		/// <inheritdoc/>
		public event MessageDeletedEventHandler? MessageDeleted;

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? ChannelDeleted;

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? MemberDeleted;

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? RoleDeleted;

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? CategoryDeleted;

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IChannel>? ChannelCreated;

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IMember>? MemberCreated;

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IRole>? RoleCreated;

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IChannelCategory>? CategoryCreated;


		/// <inheritdoc/>
		public IClient Client { get { ThrowIfClosed(); return baseClient; } }

		/// <summary>
		/// Base client that contains this server
		/// </summary>
		public Client BaseClient { get { ThrowIfClosed(); return baseClient; } }

		/// <inheritdoc/>
		public string Name { get; }

		/// <inheritdoc/>
		public ulong Id { get; }


		/// <inheritdoc/>
		public IReadOnlyCollection<Role> Roles => roles.Values;

		/// <inheritdoc/>
		public IReadOnlyCollection<Member> Members => members.Values;

		/// <inheritdoc/>
		public IReadOnlyCollection<ChannelCategory> Categories => cats.Values;

		/// <inheritdoc/>
		public bool IsClosed { get; private set; } = false;


		internal Server(Client client, string name)
		{
			baseClient = client;
			Name = name;
			cats.Add(0, new ChannelCategory(null, this));
			Id = client.GenerateNextId();

			members.Add(client.TestSelfAccount.Id, new Member(this, client.TestSelfAccount, Permissions.All));
		}


		/// <inheritdoc/>
		public bool Equals(IServer? other) => other is Server server && !IsClosed && !server.IsClosed && server.Id == Id;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as IServer);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		/// <summary>
		/// Creates and adds member to server
		/// </summary>
		/// <param name="userName">User name</param>
		/// <param name="isBot">If new member is bot</param>
		/// <param name="permissions">Permission of new member</param>
		/// <returns>New member instance</returns>
		public Member AddMember(string userName, bool isBot, Permissions permissions)
		{
			ThrowIfClosed();

			var member = new Member(this, userName, isBot, permissions);
			members.Add(member.Id, member);

			try { MemberCreated?.Invoke(member, false); }
			catch (Exception) { /*No logging*/ }

			return member;
		}

		/// <summary>
		/// Creates and adds role to server
		/// </summary>
		/// <param name="permissions">Permissions that role grants</param>
		/// <param name="name">Role name</param>
		/// <returns>New role instance</returns>
		public Role AddRole(Permissions permissions, string name)
		{
			ThrowIfClosed();

			var role = new Role(permissions, name, this);
			roles.Add(role.Id, role);

			try { RoleCreated?.Invoke(role, false); }
			catch (Exception) { /*No logging*/ }

			return role;
		}

		/// <summary>
		/// Creates and adds channel to server
		/// </summary>
		/// <param name="category">Target category</param>
		/// <param name="creationModel">Model to create channel</param>
		/// <returns>New channel instance</returns>
		public Channel AddChannel(ChannelCategory category, ChannelCreationModel creationModel)
		{
			ThrowIfClosed();

			var channel = creationModel.ChannelType switch
			{
				ChannelType.TextCompatible => new TextChannel(creationModel.Name, category),
				ChannelType.Voice => new VoiceChannel(creationModel.Name, category),
				_ => new Channel(creationModel.Name, category)
			};

			category.AddChannel(channel);

			try { ChannelCreated?.Invoke(channel, false); }
			catch (Exception) { /*No logging*/ }

			return channel;
		}

		/// <summary>
		/// Creates and adds channel to server to global category
		/// </summary>
		/// <param name="creationModel">Model to create channel</param>
		/// <returns>New channel instance</returns>
		public Channel AddChannel(ChannelCreationModel creationModel) => AddChannel(cats[0], creationModel);

		/// <summary>
		/// Creates and adds thread to server to some channel
		/// </summary>
		/// <param name="textChannel">Target text channel</param>
		/// <param name="threadName">Thread name</param>
		/// <returns>New thread instance</returns>
		public TextThread AddThread(TextChannel textChannel, string threadName)
		{
			ThrowIfClosed();

			var thread = new TextThread(threadName, textChannel, textChannel.Threads);
			textChannel.Threads.AddThreadInternal(thread);

			try { ChannelCreated?.Invoke(thread, false); }
			catch (Exception) { /*No logging*/ }

			return thread;
		}

		/// <summary>
		/// Creates and adds category to server
		/// </summary>
		/// <param name="categoryName">Category name</param>
		/// <returns>New category instance</returns>
		public ChannelCategory AddCategory(string categoryName)
		{
			ThrowIfClosed();

			var category = new ChannelCategory(categoryName, this);
			cats.Add(category.Id ?? 0, category);

			try { CategoryCreated?.Invoke(category, false); }
			catch (Exception) { /*No logging*/ }

			return category;
		}

		/// <summary>
		/// Removes member from server
		/// </summary>
		/// <param name="member">Member to remove</param>
		public void DeleteMember(Member member)
		{
			ThrowIfClosed();

			members.Remove(member.Id);
			DeleteObject(member);

			try { MemberDeleted?.Invoke(this, member.Id); }
			catch (Exception) { /*No logging*/ }
		}

		/// <summary>
		/// Removes role from server
		/// </summary>
		/// <param name="role">Role to remove</param>
		public void DeleteRole(Role role)
		{
			ThrowIfClosed();

			roles.Remove(role.Id);
			DeleteObject(role);

			try { RoleDeleted?.Invoke(this, role.Id); }
			catch (Exception) { /*No logging*/ }
		}

		/// <summary>
		/// Removes channel from server
		/// </summary>
		/// <param name="channel">Channel to remove</param>
		public void DeleteChannel(Channel channel)
		{
			ThrowIfClosed();

			if (channel is TextThread thread)
				thread.BaseContainer.DeleteThreadInternal(thread);
			else channel.BaseCategory.DeleteChannel(channel);

			DeleteObject(channel);

			try { ChannelDeleted?.Invoke(this, channel.Id); }
			catch (Exception) { /*No logging*/ }
		}

		/// <summary>
		/// Removes category from server
		/// </summary>
		/// <param name="category">Category to remove</param>
		public void DeleteCategory(ChannelCategory category)
		{
			var id = category.Id ?? throw new ArgumentException("Enable to delete global category");

			ThrowIfClosed();

			cats.Remove(id);
			DeleteObject(category);

			try { CategoryDeleted?.Invoke(this, id); }
			catch (Exception) { /*No logging*/ }
		}

		private static void DeleteObject(IServerDeletable serverDeletable) => serverDeletable.DeleteInternal();

		/// <inheritdoc/>
		public IReadOnlyCollection<IMember> GetMembers()
		{
			ThrowIfClosed();
			return members.Values;
		}

		/// <inheritdoc/>
		public IMember GetMember(ulong id)
		{
			ThrowIfClosed();
			return members[id];
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannelCategory> GetCategories()
		{
			ThrowIfClosed();
			return cats.Values;
		}

		/// <inheritdoc/>
		public IChannelCategory GetCategory(ulong? id)
		{
			ThrowIfClosed();
			return cats[id ?? 0];
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannel> GetChannels()
		{
			ThrowIfClosed();
			return cats.Values.SelectMany(s => s.Channels).ToArray();
		}

		/// <inheritdoc/>
		public IChannel GetChannel(ulong id)
		{
			ThrowIfClosed();
			return cats.Values.Single(s => s.Channels.Any(s => s.Id == id)).Channels.Single(s => s.Id == id);
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<IRole> GetRoles()
		{
			ThrowIfClosed();
			return roles.Values;
		}

		/// <inheritdoc/>
		public IRole GetRole(ulong id)
		{
			ThrowIfClosed();
			return roles[id];
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

		/// <inheritdoc/>
		public TExtension CreateExtension<TExtension>() where TExtension : class
		{
			var factory = (IServerExtensionFactory<TExtension>)baseClient.ServerExtensions.Single(s => s is IServerExtensionFactory<TExtension> factory && factory.TargetServerType.IsInstanceOfType(this));
			return factory.Create(this, extensionContextFactory.CreateInstance<TExtension>());
		}

		internal void OnMessageCreated(Message message, bool isModified)
		{
			try { MessageSent?.Invoke(Client, message, isModified); }
			catch (Exception) { /*No logging*/ }
		}

		internal void OnMessageDeleted(TextChannelBase textChannel, ulong messageId)
		{
			try { MessageDeleted?.Invoke(Client, textChannel, messageId); }
			catch (Exception) { /*No logging*/ }
		}


		private sealed class ExtensionContextFactory : IDisposable
		{
			private readonly Dictionary<Type, object> dataStore = new();
			private readonly List<IDisposable> toDispose = new();


			public IServerExtensionContext<TExtension> CreateInstance<TExtension>() where TExtension : class
			{
				return new Instance<TExtension>(dataStore);
			}

			public void Dispose()
			{
				foreach (var item in toDispose)
					item.Dispose();
			}

			private sealed class Instance<TExtension> : IServerExtensionContext<TExtension>, IDisposable where TExtension : class
			{
				private readonly Dictionary<Type, object> dataStore;
				private Action? callback;


				public Instance(Dictionary<Type, object> dataStore)
				{
					this.dataStore = dataStore;
				}


				public void Dispose()
				{
					callback?.Invoke();
				}

				public object? GetExtensionData() => dataStore.ContainsKey(typeof(TExtension)) ? dataStore[typeof(TExtension)] : null;

				public void SetExtensionData(object data)
				{
					if (dataStore.ContainsKey(typeof(TExtension)))
						dataStore[typeof(TExtension)] = data;
					else dataStore.Add(typeof(TExtension), data);
				}

				public void SetReleaseCallback(Action callback)
				{
					this.callback = callback;
				}
			}
		}
	}
}