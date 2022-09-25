using DidiFrame.ClientExtensions;
using DidiFrame.Clients;
using DidiFrame.Culture;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test IClient implementation
	/// </summary>
	public sealed class Client : IClient
	{
		private readonly Dictionary<ulong, Server> servers = new();
		private readonly ExtensionContextFactory extensionContextFactory;
		private readonly IReadOnlyCollection<IClientExtensionFactory> clientExtensions;
		private ulong nextId = 0;


		/// <summary>
		/// Creates new instance of DidiFrame.Testing.Client.Client
		/// </summary>
		/// <param name="clientExtensions">Collection of client extensions</param>
		/// <param name="serverExtensions">Collection of server extensions</param>
		/// <param name="userName">User name of bot</param>
		public Client(IReadOnlyCollection<IClientExtensionFactory> clientExtensions, IReadOnlyCollection<IServerExtensionFactory> serverExtensions, string userName = "Test bot")
		{
			TestSelfAccount = new User(this, userName, true);
			extensionContextFactory = new();
			this.clientExtensions = clientExtensions;
			ServerExtensions = serverExtensions;
		}

		/// <summary>
		/// Creates new instance of DidiFrame.Testing.Client.Client
		/// </summary>
		/// <param name="userName">User name of bot</param>
		public Client(string userName = "Test bot") : this(Array.Empty<IClientExtensionFactory>(), Array.Empty<IServerExtensionFactory>(), userName) { }


		/// <inheritdoc/>
		public IReadOnlyCollection<IServer> Servers => servers.Values;

		/// <inheritdoc/>
		public IUser SelfAccount => TestSelfAccount;

		/// <summary>
		/// Base bot's account in discord
		/// </summary>
		public User TestSelfAccount { get; }

		/// <summary>
		/// Client's culture provider for server
		/// </summary>
		public IServerCultureProvider? CultureProvider { get; private set; } = null;

		/// <summary>
		/// Collection of server extensions
		/// </summary>
		public IReadOnlyCollection<IServerExtensionFactory> ServerExtensions { get; }


		/// <inheritdoc/>
		public event ServerCreatedEventHandler? ServerCreated;

		/// <inheritdoc/>
		public event ServerRemovedEventHandler? ServerRemoved;


		/// <inheritdoc/>
		public Task AwaitForExit()
		{
			return Task.Delay(-1);
		}

		/// <inheritdoc/>
		public Task ConnectAsync()
		{
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public TExtension CreateExtension<TExtension>() where TExtension : class
		{
			var extensionFactory = (IClientExtensionFactory<TExtension>)clientExtensions.Single(s => s is IClientExtensionFactory<TExtension> factory && factory.TargetClientType.IsInstanceOfType(this));
			return extensionFactory.Create(this, extensionContextFactory.CreateInstance<TExtension>());
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			//No disposing need for this client
		}

		/// <inheritdoc/>
		public IServer GetServer(ulong id)
		{
			return servers[id];
		}

		/// <summary>
		/// Gets base server by id
		/// </summary>
		/// <param name="id">Id of server to get</param>
		/// <returns>Target server</returns>
		public Server GetTestServer(ulong id)
		{
			return servers[id];
		}

		/// <inheritdoc/>
		public void SetupCultureProvider(IServerCultureProvider? cultureProvider)
		{
			CultureProvider = cultureProvider;
		}

		/// <summary>
		/// Creates server in test client
		/// </summary>
		/// <param name="serverName">Name of server</param>
		/// <returns>New test server instance</returns>
		public Server CreateServer(string serverName = "Some server")
		{
			var server = new Server(this, serverName);
			servers.Add(server.Id, server);
			ServerCreated?.Invoke(server);
			return server;
		}

		/// <summary>
		/// Deletes server from client
		/// </summary>
		/// <param name="server">Server to delete</param>
		public void DeleteServer(Server server)
		{
			server.DeleteInternal();
			servers.Remove(server.Id);
			ServerRemoved?.Invoke(server);
		}

		/// <summary>
		/// Generates new global id
		/// </summary>
		/// <returns>New global id</returns>
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
