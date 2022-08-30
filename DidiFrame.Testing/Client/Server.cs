using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Clients;
using System.Runtime.CompilerServices;
using DidiFrame.ClientExtensions;

namespace DidiFrame.Testing.Client
{
	public class Server : IServer
	{
		private readonly Dictionary<ulong, Role> roles = new();
		private readonly Dictionary<ulong, Member> members = new();
		private readonly Dictionary<ulong, ChannelCategory> cats = new();
		private readonly Client baseClient;
		private readonly ExtensionContextFactory extensionContextFactory = new();


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


		public IReadOnlyCollection<Role> Roles => roles.Values;

		public IReadOnlyCollection<Member> Members => members.Values;

		public IReadOnlyCollection<ChannelCategory> Categories => cats.Values;

		public bool IsClosed { get; private set; } = false;


		internal Server(Client client, string name)
		{
			baseClient = client;
			Name = name;
			cats.Add(0, new ChannelCategory(null, this));
			Id = client.GenerateNextId();

			members.Add(client.TestSelfAccount.Id, new Member(this, client.TestSelfAccount, Permissions.All));
		}


		public bool Equals(IServer? other) => other is Server server && !IsClosed && !server.IsClosed && server.Id == Id;

		public override bool Equals(object? obj) => Equals(obj as IServer);

		public override int GetHashCode() => Id.GetHashCode();

		public Member AddMember(string userName, bool isBot, Permissions permissions)
		{
			ThrowIfClosed();

			var member = new Member(this, userName, isBot, permissions);
			members.Add(member.Id, member);

			try { MemberCreated?.Invoke(member, false); }
			catch (Exception) { /*No logging*/ }

			return member;
		}

		public Role AddRole(Permissions permissions, string name)
		{
			ThrowIfClosed();

			var role = new Role(permissions, name, this);
			roles.Add(role.Id, role);

			try { RoleCreated?.Invoke(role, false); }
			catch (Exception) { /*No logging*/ }

			return role;
		}

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

		public Channel AddChannel(ChannelCreationModel creationModel) => AddChannel(cats[0], creationModel);

		public TextThread AddThread(TextChannel textChannel, string threadName)
		{
			ThrowIfClosed();

			var thread = new TextThread(threadName, textChannel);
			textChannel.AddThreadInternal(thread);

			try { ChannelCreated?.Invoke(thread, false); }
			catch (Exception) { /*No logging*/ }

			return thread;
		}

		public ChannelCategory AddCategory(string categoryName)
		{
			ThrowIfClosed();

			var category = new ChannelCategory(categoryName, this);
			cats.Add(category.Id ?? 0, category);

			try { CategoryCreated?.Invoke(category, false); }
			catch (Exception) { /*No logging*/ }

			return category;
		}

		public void DeleteMember(Member member)
		{
			ThrowIfClosed();

			members.Remove(member.Id);
			DeleteObject(member);

			try { MemberDeleted?.Invoke(this, member.Id); }
			catch (Exception) { /*No logging*/ }
		}

		public void DeleteRole(Role role)
		{
			ThrowIfClosed();

			roles.Remove(role.Id);
			DeleteObject(role);

			try { RoleCreated?.Invoke(role, false); }
			catch (Exception) { /*No logging*/ }
		}

		public void DeleteChannel(Channel channel)
		{
			ThrowIfClosed();

			if (channel is TextThread thread)
				thread.BaseParent.DeleteThreadInternal(thread);
			else channel.BaseCategory.DeleteChannel(channel);

			DeleteObject(channel);

			try { ChannelDeleted?.Invoke(this, channel.Id); }
			catch (Exception) { /*No logging*/ }
		}

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

		public IReadOnlyCollection<IMember> GetMembers()
		{
			ThrowIfClosed();
			return members.Values;
		}

		public IMember GetMember(ulong id)
		{
			ThrowIfClosed();
			return members[id];
		}

		public IReadOnlyCollection<IChannelCategory> GetCategories()
		{
			ThrowIfClosed();
			return cats.Values;
		}

		public IChannelCategory GetCategory(ulong? id)
		{
			ThrowIfClosed();
			return cats[id ?? 0];
		}

		public IReadOnlyCollection<IChannel> GetChannels()
		{
			ThrowIfClosed();
			return cats.Values.SelectMany(s => s.Channels).ToArray();
		}

		public IChannel GetChannel(ulong id)
		{
			ThrowIfClosed();
			return cats.Values.Single(s => s.Channels.Any(s => s.Id == id)).Channels.Single(s => s.Id == id);
		}

		public IReadOnlyCollection<IRole> GetRoles()
		{
			ThrowIfClosed();
			return roles.Values;
		}

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