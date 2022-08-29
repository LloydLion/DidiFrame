using DidiFrame.ClientExtensions;
using DidiFrame.Clients;
using DidiFrame.Culture;

namespace DidiFrame.Testing.Client
{
	public sealed class Client : IClient
	{
		private readonly Dictionary<ulong, Server> servers = new();
		private readonly ExtensionContextFactory extensionContextFactory;
		private readonly IReadOnlyCollection<IClientExtensionFactory> clientExtensions;
		private ulong nextId = 0;


		public Client(IReadOnlyCollection<IClientExtensionFactory> clientExtensions, IReadOnlyCollection<IServerExtensionFactory> serverExtensions, string userName = "Test bot")
		{
			TestSelfAccount = new User(this, userName, true);
			extensionContextFactory = new();
			this.clientExtensions = clientExtensions;
			ServerExtensions = serverExtensions;
		}

		public Client(string userName = "Test bot") : this(Array.Empty<IClientExtensionFactory>(), Array.Empty<IServerExtensionFactory>(), userName) { }


		public IReadOnlyCollection<IServer> Servers => servers.Values;

		public IUser SelfAccount => TestSelfAccount;

		public User TestSelfAccount { get; }

		public IServerCultureProvider? CultureProvider { get; private set; } = null;

		public IReadOnlyCollection<IServerExtensionFactory> ServerExtensions { get; }


		public event ServerCreatedEventHandler? ServerCreated;

		public event ServerRemovedEventHandler? ServerRemoved;


		public Task AwaitForExit()
		{
			return Task.Delay(-1);
		}

		public Task ConnectAsync()
		{
			return Task.CompletedTask;
		}

		public TExtension CreateExtension<TExtension>() where TExtension : class
		{
			var extensionFactory = (IClientExtensionFactory<TExtension>)clientExtensions.Single(s => s is IClientExtensionFactory<TExtension> factory && factory.TargetClientType.IsInstanceOfType(this));
			return extensionFactory.Create(this, extensionContextFactory.CreateInstance<TExtension>());
		}

		public void Dispose()
		{
			//No disposing need for this client
		}

		public IServer GetServer(ulong id)
		{
			return servers[id];
		}

		public Server GetTestServer(ulong id)
		{
			return servers[id];
		}

		public void SetupCultureProvider(IServerCultureProvider? cultureProvider)
		{
			CultureProvider = cultureProvider;
		}

		public Server CreateServer(string serverName = "Some server")
		{
			var server = new Server(this, serverName);
			servers.Add(server.Id, server);
			ServerCreated?.Invoke(server);
			return server;
		}

		public void DeleteServer(Server server)
		{
			server.DeleteInternal();
			servers.Remove(server.Id);
			ServerRemoved?.Invoke(server);
		}

		public ulong GenerateNextId()
		{
			return nextId++;
		}


		private sealed class ExtensionContextFactory
		{
			private readonly Dictionary<Type, object> extensionsData = new();


			public IClientExtensionContext<TExtension> CreateInstance<TExtension>()
			{
				return new ExtensionsContext<TExtension>(extensionsData);
			}


			private sealed class ExtensionsContext<TExtension> : IClientExtensionContext<TExtension>
			{
				private readonly Dictionary<Type, object> extensionsData;


				public ExtensionsContext(Dictionary<Type, object> extensionsData)
				{
					this.extensionsData = extensionsData;
				}


				public object? GetExtensionData()
				{
					return extensionsData.ContainsKey(typeof(TExtension)) ? extensionsData[typeof(TExtension)] : null;
				}

				public void SetExtensionData(object data)
				{
					if (extensionsData.ContainsKey(typeof(TExtension)) == false)
						extensionsData.Add(typeof(TExtension), data);
					else extensionsData[typeof(TExtension)] = data;
				}
			}
		}
	}
}
