using DidiFrame.Culture;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// Discord client based on DSharpPlus library
	/// </summary>
	public class Client : IClient
	{
		internal const string ChannelName = "Channel";
		internal const string MemberName = "Member";
		internal const string UserName = "User";
		internal const string ServerName = "Server";
		internal const string MessageName = "Message";
		private readonly static EventId SafeOperationErrorID = new(23, "SafeOperationError");
		private readonly static EventId SafeOperationCriticalErrorID = new(22, "SafeOperationCriticalError");
		private readonly static EventId NoServerConnectionID = new(21, "NoServerConnection");
		private readonly static EventId ServerCreatedEventErrorID = new(98, "ServerCreatedEventError");
		private readonly static EventId ServerRemovedEventErrorID = new(97, "ServerRemovedEventError");


		private readonly DiscordClient client;
		private readonly List<Server> servers = new();
		private Task? serverListUpdateTask;
		private readonly CancellationTokenSource cts = new();
		private readonly ILogger<Client> logger;
		private readonly Lazy<IUser> selfAccount;
		private readonly Options options;
		private readonly IChannelMessagesCacheFactory messagesCacheFactory;


		public event ServerCreatedEventHandler? ServerCreated;
		public event ServerRemovedEventHandler? ServerRemoved;


		/// <inheritdoc/>
		public IReadOnlyCollection<IServer> Servers => servers;

		/// <inheritdoc/>
		public IUser SelfAccount => selfAccount.Value;
		/// <summary>
		/// Base client from DSharpPlus library
		/// </summary>
		public DiscordClient BaseClient => client;

		/// <summary>
		/// Culture info provider that using in event to setup culture
		/// </summary>
		public IServerCultureProvider CultureProvider { get; }

		/// <inheritdoc/>
		public IServiceProvider Services { get; }

		internal ILogger<Client> Logger => logger;


		/// <summary>
		/// Creates instance of DidiFrame.DSharpAdapter.Client
		/// </summary>
		/// <param name="servicesForExtensions">Services that will be used for extensions</param>
		/// <param name="options">Configuration of DSharp client (DidiFrame.DSharpAdapter.Client.Options)</param>
		/// <param name="factory">Loggers for DSharp client</param>
		/// <param name="cultureProvider">Culture provider for event thread culture</param>
		public Client(IServiceProvider servicesForExtensions, IOptions<Options> options, ILoggerFactory factory, IServerCultureProvider? cultureProvider = null, IChannelMessagesCacheFactory? messagesCacheFactory = null)
		{
			this.options = options.Value;
			logger = factory.CreateLogger<Client>();

			client = new DiscordClient(new DiscordConfiguration
			{
				Token = options.Value.Token,
				AutoReconnect = true,
				HttpTimeout = new TimeSpan(0, 1, 0),
				TokenType = TokenType.Bot,
				LoggerFactory = factory,
				Intents = DiscordIntents.All
			});

			CultureProvider = cultureProvider ?? new GagCultureProvider(new CultureInfo("en-US"));
			selfAccount = new(() => new User(client.CurrentUser.Id, () => client.CurrentUser, this));
			Services = servicesForExtensions;
			this.messagesCacheFactory = messagesCacheFactory ?? new ChannelMessagesCache.Factory(options.Value.CacheOptions
				?? throw new ArgumentException("Cache options can't be null if no custom messages cache factory provided", nameof(options)));
		}


		/// <inheritdoc/>
		public async Task AwaitForExit()
		{
			int ticks = 0;
			while (ticks < 5)
			{
				await Task.Delay(new TimeSpan(0, 5, 0));

				try
				{
					//Demo operation
					await client.GetUserAsync(SelfAccount.Id, updateCache: true);
					ticks = 0;
				}
				catch (Exception)
				{
					ticks++;
				}
			}
		}

		/// <inheritdoc/>
		public void Connect()
		{
			client.ConnectAsync().Wait();
			Thread.Sleep(5000);
			serverListUpdateTask = CreateServerListUpdateTask(cts.Token);
		}

		private async Task CreateServerListUpdateTask(CancellationToken token)
		{
			while (token.IsCancellationRequested == false)
			{
				var temp = servers.ToList();
				servers.Clear();

				foreach (var server in client.Guilds)
				{
					var maybe = temp.SingleOrDefault(s => s.Id == server.Key);
					if (maybe is not null)
					{
						servers.Add(maybe);
						temp.Remove(maybe);
					}
					else
					{
						var serverObj = new Server(server.Value, this, options.ServerOptions, messagesCacheFactory.Create(server.Value, this));
						servers.Add(serverObj);
						onServerCreated(serverObj);
					}
				}

				foreach (var item in temp)
				{
					item.Dispose();
					onServerRemoved(item);
				}

				await Task.Delay(new TimeSpan(5, 0, 0), token);
			}



			void onServerCreated(Server server)
			{
				try
				{
					ServerCreated?.Invoke(server);
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Warning, ServerCreatedEventErrorID, ex, "Exception in event handler for server creation with id {ServerId}", server.Id);
				}
			}

			void onServerRemoved(Server server)
			{
				try
				{
					ServerRemoved?.Invoke(server);
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Warning, ServerRemovedEventErrorID, ex, "Exception in event handler for server removement with id {ServerId}", server.Id);
				}
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			cts.Cancel();
			serverListUpdateTask?.Wait();
			client.Dispose();
		}

		/// <summary>
		/// Checks connection to discord server and if fail awaits it
		/// </summary>
		/// <returns>Wait task that will be complited only when connection will be alive</returns>
		public async Task CheckAndAwaitConnectionAsync()
		{
		reset:
			try
			{
				//Demo operation
				await client.GetUserAsync(SelfAccount.Id, updateCache: true);
			}
			catch (Exception)
			{
				logger.Log(LogLevel.Warning, NoServerConnectionID, "No connection to discord server! Waiting 450ms");
				await Task.Delay(450);
				goto reset;
			}
		}

		///// <summary>
		///// Do safe opration under discord client
		///// </summary>
		///// <param name="operation">Operation delegate</param>
		internal void DoSafeOperation(Action operation, NotFoundInfo? nfi = null)
		{
		reset:
			CheckAndAwaitConnectionAsync().Wait();

			try
			{
				operation();
			}
			catch (Exception ex)
			{
				var pex = ex;
				if (ex is AggregateException ar && ar.InnerException is not null) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					else Thread.Sleep(1000);
					goto reset;
				}
				else
				{
					logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");

					if (nfi.HasValue && pex is NotFoundException)
						throw new DiscordObjectNotFoundException(nfi.Value.ObjectType, nfi.Value.ObjectId, nfi.Value.ObjectName);
					else if (pex is UnauthorizedException) throw new NotEnoughPermissionsException(pex.Message);
					else throw new InternalDiscordException(pex.Message, pex);
				}
			}
		}

		///// <summary>
		///// Do safe opration under discord client with result
		///// </summary>
		///// <typeparam name="TReturn">Type of result</typeparam>
		///// <param name="operation">Operation delegate</param>
		///// <returns>Operation result</returns>
		internal TReturn DoSafeOperation<TReturn>(Func<TReturn> operation, NotFoundInfo? nfi = null)
		{
		reset:
			CheckAndAwaitConnectionAsync().Wait();

			try
			{
				return operation();
			}
			catch (Exception ex)
			{
				var pex = ex;
				if (ex is AggregateException ar && ar.InnerException is not null) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					else Thread.Sleep(1000);
					goto reset;
				}
				else
				{
					logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");

					if (nfi.HasValue && pex is NotFoundException)
						throw new DiscordObjectNotFoundException(nfi.Value.ObjectType, nfi.Value.ObjectId, nfi.Value.ObjectName);
					else if (pex is UnauthorizedException) throw new NotEnoughPermissionsException(pex.Message);
					else throw new InternalDiscordException(pex.Message, pex);
				}
			}
		}

		///// <summary>
		///// Do safe async opration under discord client
		///// </summary>
		///// <param name="operation">Async operation delegate</param>
		///// <returns>Wait task</returns>
		internal async Task DoSafeOperationAsync(Func<Task> operation, NotFoundInfo? nfi = null)
		{
		reset:
			await CheckAndAwaitConnectionAsync();

			try
			{
				await operation();
			}
			catch (Exception ex)
			{
				var pex = ex;
				if (ex is AggregateException ar && ar.InnerException is not null) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					else Thread.Sleep(1000);
					goto reset;
				}
				else
				{
					logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");

					if (nfi.HasValue && pex is NotFoundException)
						throw new DiscordObjectNotFoundException(nfi.Value.ObjectType, nfi.Value.ObjectId, nfi.Value.ObjectName);
					else if (pex is UnauthorizedException) throw new NotEnoughPermissionsException(pex.Message);
					else throw new InternalDiscordException(pex.Message, pex);
				}
			}
		}

		///// <summary>
		///// Do safe async opration under discord client with result
		///// </summary>
		///// <typeparam name="TReturn">Type of result</typeparam>
		///// <param name="operation">Async operation delegate</param>
		///// <returns>Async operation result</returns>
		internal async Task<TReturn> DoSafeOperationAsync<TReturn>(Func<Task<TReturn>> operation, NotFoundInfo? nfi = null)
		{
		reset:
			await CheckAndAwaitConnectionAsync();

			try
			{
				return await operation();
			}
			catch (Exception ex)
			{
				var pex = ex;
				if (ex is AggregateException ar && ar.InnerException is not null) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					else Thread.Sleep(1000);
					goto reset;
				}
				else
				{
					logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");

					if (nfi.HasValue && pex is NotFoundException)
						throw new DiscordObjectNotFoundException(nfi.Value.ObjectType, nfi.Value.ObjectId, nfi.Value.ObjectName);
					else if (pex is UnauthorizedException) throw new NotEnoughPermissionsException(pex.Message);
					else throw new InternalDiscordException(pex.Message, pex);
				}
			}
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

			public Server.Options ServerOptions { get; set; } = new();

			public ChannelMessagesCache.Options? CacheOptions { get; set; } = new();
		}


		internal struct NotFoundInfo
		{
			public NotFoundInfo(string objectType, ulong objectId, string? objectName = null)
			{
				ObjectType = objectType;
				ObjectId = objectId;
				ObjectName = objectName;
			}


			public string ObjectType { get; }

			public string? ObjectName { get; }

			public ulong ObjectId { get; }
		}
	}
}
