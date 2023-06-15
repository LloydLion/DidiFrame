using DidiFrame.Exceptions;
using DidiFrame.Threading;
using DidiFrame.Utils.RoutedEvents;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;
using System.Text;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// Discord client based on DSharpPlus library
	/// </summary>
	public sealed class DSharpClient : IClient, IDisposable
	{
		private readonly RoutedEventTreeNode routedEventTreeNode;
		private readonly Dictionary<ulong, Server> servers = new();
		private readonly DiscordClient discordClient;
		private readonly IThreadingSystem threading;
		private readonly IManagedThread clientThread;
		private readonly IManagedThreadExecutionQueue clientThreadQueue;


		public DSharpClient(IOptions<Options> options, ILoggerFactory loggerFactory, IThreadingSystem threading)
		{
			Logger = loggerFactory.CreateLogger<DSharpClient>();
			this.threading = threading;


			routedEventTreeNode = new RoutedEventTreeNode(this);
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
#if DEBUG
			discordClient.MessageCreated += ProcessDebugCommand;
#endif

			clientThread = threading.CreateNewThread();
			clientThreadQueue = clientThread.CreateNewExecutionQueue("main");
			clientThread.Begin(clientThreadQueue);
		}

		public IReadOnlyCollection<IServer> Servers
		{
			get
			{
				ThrowUnlessConnected();
				return servers.Values;
			}
		}

		public ConnectStatus CurrentConnectStatus { get; private set; }

		public DiscordClient BaseClient => discordClient;

		internal ILogger<DSharpClient> Logger { get; }

		internal RoutedEventTreeNode RoutedEventTreeNode => routedEventTreeNode;


		public async ValueTask ConnectAsync()
		{
			if (CurrentConnectStatus != ConnectStatus.NoConnection)
				throw new InvalidOperationException("Enable to connect twice");

			await discordClient.ConnectAsync();

			Thread.Sleep(5000);

			foreach (var guild in discordClient.Guilds)
			{
				await InitializeGuild(guild.Value);
			}

			CurrentConnectStatus = ConnectStatus.Connected;
		}

		public async Task AwaitForExit()
		{
			ThrowUnlessConnected();

			int ticks = 0;
			while (ticks < 5)
			{
				await Task.Delay(new TimeSpan(0, 5, 0));

				try
				{
					//Demo operation
					await discordClient.GetUserAsync(discordClient.CurrentUser.Id, updateCache: true);
					ticks = 0;
				}
				catch (Exception)
				{
					ticks++;
				}
			}

			CurrentConnectStatus = ConnectStatus.Disconnected;
		}

		public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs => routedEventTreeNode.RemoveListener(routedEvent, handler);

		public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs => routedEventTreeNode.AddListener(routedEvent, handler);

		public void Dispose()
		{
			discordClient.Dispose();
			clientThreadQueue.Dispatch(clientThread.Stop);
		}

		public void ThrowUnlessConnected()
		{
			if (CurrentConnectStatus != ConnectStatus.Connected)
				throw new InvalidOperationException("Enable to do this operation if client hasn't connected");
		}

		public async Task<TResult> DoDiscordOperation<TResult>(Func<Task<TResult>> asyncOperation, Func<TResult, Task> asyncEffector, ServerObject serverObject)
		{
			if (serverObject.BaseServer.IsClosed)
				throw new ServerClosedException("Enable to do discord operation", serverObject.BaseServer);

			if (serverObject.IsExists == false)
				throw new DiscordObjectNotFoundException(serverObject.GetType().Name, serverObject.Id, serverObject.Name);

			await AwaitConnection();

			TResult result;

			try
			{
				result = await asyncOperation();
			}
			catch (Exception ex)
			{
				var iex = ex is AggregateException aex ? aex.InnerException : ex;

				if (iex is NotFoundException)
				{
					await serverObject.MakeDeletedAsync();

					throw new DiscordObjectNotFoundException(serverObject.GetType().Name, serverObject.Id, serverObject.Name);
				}

				if (iex is UnauthorizedException unauthorizedException)
				{
					throw new NotEnoughPermissionsException("Bot don't have permission to do this discord operation", unauthorizedException);
				}

				throw new InternalDiscordException("Discord operation failed", iex);
			}

			await asyncEffector(result);

			return result;
		}

		public async Task AwaitConnection()
		{
			while (true)
			{
				try
				{
					await discordClient.GetUserAsync(discordClient.CurrentUser.Id, updateCache: true);
					return;
				}
				catch (Exception)
				{
					await Task.Delay(new TimeSpan(0, 0, 30));
				}
			}
		}

		private async Task OnGuildCreated(DiscordClient sender, GuildCreateEventArgs e)
		{
			await InitializeGuild(e.Guild);
		}

		private async Task OnGuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
		{
			if (servers.TryGetValue(e.Guild.Id, out var server))
			{
				await server.ShutdownAsync();
				servers.Remove(e.Guild.Id);
			}
			else throw new Exception($"Invalid event recived, no server with id {e.Guild.Id} was registered");
		}

		private Task InitializeGuild(DiscordGuild guild)
		{
			return clientThreadQueue.DispatchAsync(() =>
			{
				var thread = threading.CreateNewThread();

				var server = new Server(this, guild, thread);

				servers.Add(guild.Id, server);

				return server.InitiateStartupProcedure();
			});
		}

#if DEBUG
		private async Task ProcessDebugCommand(DiscordClient sender, MessageCreateEventArgs e)
		{
			if (e.Message.Content != $".didiFrame.{discordClient.CurrentUser.Id}.showVSS")
				return;

			if (e.Guild is null)
			{
				await e.Message.RespondAsync("Enable to locale server where command called");
				return;
			}

			if (servers.TryGetValue(e.Guild.Id, out var server))
			{
				string message = await server.WorkQueue.DispatchAsync(() =>
				{
					var members = server.ListMembers();
					var membersList = string.Join("\n", members);

					var roles = server.ListRoles();
					var rolesList = string.Join("\n", roles);

					return $"Server: {server}\n\nMembers [{members.Count}]:\n{membersList}\n\nRoles [{roles.Count}]:\n{rolesList}";
				});

				var file = Encoding.UTF8.GetBytes(message);
				var memoryStream = new MemoryStream(file);

				await e.Message.RespondAsync(builder =>
				{
					builder.WithFile("VSS.txt", memoryStream, resetStreamPosition: true);
				});
			}
			else
			{
				await e.Message.RespondAsync($"There is no server with id {e.Guild.Id} in server list");
			}
		}
#endif


		/// <summary>
		/// Options of DSharp client
		/// </summary>
		public class Options
		{
			/// <summary>
			/// Discord's bot token, see discord documentation
			/// </summary>
			public string Token { get; set; } = "";
		}

		/// <summary>
		/// Represents client state
		/// </summary>
		public enum ConnectStatus
		{
			/// <summary>
			/// New client, no connection to server
			/// </summary>
			NoConnection,
			/// <summary>
			/// Connected to discord server
			/// </summary>
			Connected,
			/// <summary>
			/// Session closed, client closed
			/// </summary>
			Disconnected,
		}
	}
}
