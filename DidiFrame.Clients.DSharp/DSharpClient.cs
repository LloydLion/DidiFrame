using DidiFrame.Threading;
using DidiFrame.Utils.RoutedEvents;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS;
using DidiFrame.Clients.DSharp.Operations;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// Discord client based on DSharpPlus library
	/// </summary>
	public sealed class DSharpClient : IClient, IDisposable
	{
		private readonly RoutedEventTreeNode routedEventTreeNode;
		private readonly Dictionary<ulong, DSharpServer> servers = new();
		private readonly DiscordClient discordClient;
		private readonly DiscordApiConnection apiConnection;
		private readonly IThreadingSystem threading;
		private readonly IVssCore vssCore;
		private readonly IManagedThread clientThread;
		private readonly IManagedThreadExecutionQueue clientThreadQueue;
		private readonly DebugUserInterface debugUI;
		private readonly Options options;


		public DSharpClient(IOptions<Options> options, ILoggerFactory loggerFactory, IThreadingSystem threading, IVssCore vssCore)
		{
			Logger = loggerFactory.CreateLogger<DSharpClient>();
			LoggerFactory = loggerFactory;

			this.threading = threading;
			this.vssCore = vssCore;
			this.options = options.Value;

			discordClient = new DiscordClient(new DiscordConfiguration()
			{
				MessageCacheSize = 0,
				AlwaysCacheMembers = false,

				Token = options.Value.Token,
				AutoReconnect = true,
				HttpTimeout = new TimeSpan(0, 1, 0),
				TokenType = TokenType.Bot,
				LoggerFactory = loggerFactory,
				Intents = DiscordIntents.All
			});

			discordClient.GuildCreated += OnGuildCreated;
			discordClient.GuildDeleted += OnGuildDeleted;

			clientThread = threading.CreateNewThread();
			clientThreadQueue = clientThread.CreateNewExecutionQueue("main");
			clientThread.Begin(clientThreadQueue);

			routedEventTreeNode = new RoutedEventTreeNode(this);

			apiConnection = new DiscordApiConnection(discordClient);

			debugUI = new DebugUserInterface(this);

			DiscordOperationBroker = new DiscordOperationBroker(this);
		}


		public IReadOnlyCollection<IServer> Servers => BaseServers;

		public IReadOnlyCollection<DSharpServer> BaseServers
		{
			get
			{
				apiConnection.ThrowUnlessConnected();
				return servers.Values;
			}
		}

		public DiscordClient DiscordClient => discordClient;

		internal ILogger<DSharpClient> Logger { get; }

		internal ILoggerFactory LoggerFactory { get; }

		internal RoutedEventTreeNode RoutedEventTreeNode => routedEventTreeNode;

		internal IManagedThreadExecutionQueue ClientThreadQueue => clientThreadQueue;

		public DiscordOperationBroker DiscordOperationBroker { get; }


		public async ValueTask ConnectAsync()
		{
			await apiConnection.ConnectAsync();

			foreach (var guild in discordClient.Guilds)
				await InitializeGuild(guild.Value);

			if (options.IsDebugEnabled)
				debugUI.Enable();
		}

		public async Task AwaitForExit()
		{
			await apiConnection.AwaitForExit();

			foreach (var server in servers.Values)
				await ShutdownServer(server);

			clientThreadQueue.Dispatch(clientThread.Stop);
		}

		public DSharpServer? GetBaseServer(ulong id)
		{
			servers.TryGetValue(id, out var val);
			return val;
		}

		public Task AwaitConnection() => apiConnection.AwaitConnection();

		public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs => routedEventTreeNode.RemoveListener(routedEvent, handler);

		public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs => routedEventTreeNode.AddListener(routedEvent, handler);

		public void Dispose()
		{
			discordClient.Dispose();
		}

		private async Task OnGuildCreated(DiscordClient sender, GuildCreateEventArgs e)
		{
			await InitializeGuild(e.Guild);
		}

		private async Task OnGuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
		{
			if (servers.TryGetValue(e.Guild.Id, out var server))
			{
				await ShutdownServer(server);
				servers.Remove(e.Guild.Id);
			}
			else throw new Exception($"Invalid event received, no server with id {e.Guild.Id} was registered");
		}

		private Task InitializeGuild(DiscordGuild guild)
		{
			return clientThreadQueue.AwaitDispatchAsync(() =>
			{
				var thread = threading.CreateNewThread();

				var server = new DSharpServer(this, guild, thread, vssCore);

				servers.Add(guild.Id, server);

				return server.InitiateStartupProcedure();
			});
		}

		private Task ShutdownServer(DSharpServer server)
		{
			return clientThreadQueue.AwaitDispatchAsync(async () =>
			{
				try
				{
					await server.ShutdownAsync();
				}
				catch (Exception ex)
				{
					Logger.Log(LogLevel.Error, ex, "Enable to Shutdown server {Server}", server.ToString());
				}
			});
		}


		/// <summary>
		/// Options of DSharp client
		/// </summary>
		public class Options
		{
			/// <summary>
			/// Discord's bot token, see discord documentation
			/// </summary>
			public string Token { get; set; } = "";

			public bool IsDebugEnabled { get; set; }
			//Can be overridden from configuration file, these is only default value
#if DEBUG
			= true;
#else
			= false;
#endif
		}
	}
}
