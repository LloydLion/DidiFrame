using DidiFrame.Clients.DSharp.Entities;
using DidiFrame.Clients.DSharp.DiscordServer;
using DidiFrame.Culture;
using DidiFrame.Entities.Message;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// Discord client based on DSharpPlus library
	/// </summary>
	public sealed class Client : IClient
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
		private readonly static EventId NewServerCreatedID = new(11, "NewServerCreated");
		private readonly static EventId ServerRemovedID = new(12, "ServerRemoved");


		private readonly DiscordClient client;
		private readonly Dictionary<ulong, Server> servers = new();
		private readonly AutoResetEvent serversSyncRoot = new(true);
		private readonly CancellationTokenSource cts = new();
		private readonly ILogger<Client> logger;
		private readonly Lazy<IUser> selfAccount;
		private readonly Options options;
		private readonly IChannelMessagesCacheFactory messagesCacheFactory;
		private readonly IServiceProvider services;
		private Task? serverListUpdateTask;
		private ConnectStatus connectStatus = ConnectStatus.NoConnection;
		private IServerCultureProvider? cultureProvider;


		/// <inheritdoc/>
		public event ServerCreatedEventHandler? ServerCreated;

		/// <inheritdoc/>
		public event ServerRemovedEventHandler? ServerRemoved;


		/// <inheritdoc/>
		public IReadOnlyCollection<IServer> Servers
		{
			get
			{
				ThrowUnlessConnected();
				return servers.Values;
			}
		}

		/// <inheritdoc/>
		public IUser SelfAccount
		{
			get
			{
				ThrowUnlessConnected();
				return selfAccount.Value;
			}
		}

		/// <summary>
		/// Base client from DSharpPlus library
		/// </summary>
		public DiscordClient BaseClient => client;

		/// <summary>
		/// Culture info provider that using in event to setup culture
		/// </summary>
		public IServerCultureProvider? CultureProvider
			{ get => cultureProvider; private set => cultureProvider = value; }

		/// <inheritdoc/>
		public IServiceProvider Services
		{
			get
			{
				ThrowUnlessConnected();
				return services;
			}
		}

		internal ILogger<Client> Logger => logger;

		internal IValidator<MessageSendModel> MessageSendModelValidator { get; }


		/// <summary>
		/// Creates instance of DidiFrame.DSharpAdapter.Client
		/// </summary>
		/// <param name="servicesForExtensions">Services that will be used for extensions</param>
		/// <param name="options">Configuration of DSharp client (DidiFrame.DSharpAdapter.Client.Options)</param>
		/// <param name="factory">Loggers for DSharp client</param>
		/// <param name="messageSendModelValidator">Validator for DidiFrame.Entities.Message.MessageSendModel</param>
		/// <param name="messagesCacheFactory">Optional custom factory for server's channel messages caches</param>
		public Client(IServiceProvider servicesForExtensions,
			IOptions<Options> options,
			ILoggerFactory factory,
			IValidator<MessageSendModel> messageSendModelValidator,
			IChannelMessagesCacheFactory? messagesCacheFactory = null)
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

			selfAccount = new(() => new User(client.CurrentUser.Id, () => client.CurrentUser, this));
			services = servicesForExtensions;
			MessageSendModelValidator = messageSendModelValidator;
			this.messagesCacheFactory = messagesCacheFactory ?? new ChannelMessagesCache.Factory(options.Value.CacheOptions
				?? throw new ArgumentException("Cache options can't be null if no custom messages cache factory provided", nameof(options)));
		}


		/// <inheritdoc/>
		public IServer GetServer(ulong id)
		{
			ThrowUnlessConnected();
			return servers[id];
		}

		/// <inheritdoc/>
		public void SetupCultureProvider(IServerCultureProvider? cultureProvider)
		{
			if (connectStatus != ConnectStatus.Connected)
				throw new InvalidOperationException("Cannot await for exit if client hasn't connected");

			if (CultureProvider is not null)
				throw new InvalidOperationException("Enable to setup culture provider twice");
			CultureProvider = cultureProvider;
		}

		/// <inheritdoc/>
		public async Task AwaitForExit()
		{
			if (connectStatus != ConnectStatus.Connected)
				throw new InvalidOperationException("Cannot await for exit if client hasn't connected");

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
		public async Task ConnectAsync()
		{
			if (connectStatus != ConnectStatus.NoConnection)
				throw new InvalidOperationException("Cannot connect already connected client");

			await client.ConnectAsync();
			await Task.Delay(5000);

			client.GuildCreated += Client_GuildCreated;
			client.GuildDeleted += Client_GuildDeleted;

			await UpdateServerList();

			serverListUpdateTask = CreateServerListUpdateTask(cts.Token);
			connectStatus = ConnectStatus.Connected;
		}

		private Task Client_GuildDeleted(DiscordClient sender, DSharpPlus.EventArgs.GuildDeleteEventArgs e)
		{
			if (connectStatus != ConnectStatus.Connected) return Task.CompletedTask;

			using (serversSyncRoot.WaitAndCreateDisposable())
			{
				if (servers.ContainsKey(e.Guild.Id))
				{
					var server = servers[e.Guild.Id];
					servers.Remove(e.Guild.Id);
					server.Dispose();
					OnServerRemoved(server);
				}
			}

			return Task.CompletedTask;
		}

		private Task Client_GuildCreated(DiscordClient sender, DSharpPlus.EventArgs.GuildCreateEventArgs e)
		{
			if (connectStatus != ConnectStatus.Connected) return Task.CompletedTask;

			using (serversSyncRoot.WaitAndCreateDisposable())
			{
				if (servers.ContainsKey(e.Guild.Id) == false)
				{
					servers.Add(e.Guild.Id, Server.CreateServerAsync(e.Guild, this, options.ServerOptions, messagesCacheFactory.Create(e.Guild, this)).Result);
					OnServerCreated(servers[e.Guild.Id]);
				}
			}

			return Task.CompletedTask;
		}

		private async Task CreateServerListUpdateTask(CancellationToken token)
		{
			while (token.IsCancellationRequested == false)
			{
				await UpdateServerList();

				if (token.IsCancellationRequested) return;

				await Task.Delay(new TimeSpan(5, 0, 0), token);
			}
		}

		private async Task UpdateServerList()
		{
			using (serversSyncRoot.WaitAndCreateDisposable())
			{
				var temp = servers.ToDictionary(s => s.Key, s => s.Value);
				servers.Clear();

				foreach (var server in client.Guilds.Select(s => s.Value))
				{
					if (temp.TryGetValue(server.Id, out var maybe))
					{
						servers.Add(maybe.Id, maybe);
						temp.Remove(maybe.Id);
					}
					else
					{
						var cache = messagesCacheFactory.Create(server, this);
						var serverObj = await Server.CreateServerAsync(server, this, options.ServerOptions, cache);
						servers.Add(serverObj.Id, serverObj);
						OnServerCreated(serverObj);
					}
				}

				foreach (var item in temp.Select(s => s.Value))
				{
					item.Dispose();
					OnServerRemoved(item);
				}
			}
		}

		private void OnServerCreated(Server server)
		{
			logger.Log(LogLevel.Debug, NewServerCreatedID, "New server created with id {ServerId} and name \"{ServerName}\"", server.Id, server.Name);

			try
			{
				ServerCreated?.Invoke(server);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Warning, ServerCreatedEventErrorID, ex, "Exception in event handler for server creation with id {ServerId}", server.Id);
			}
		}

		private void OnServerRemoved(Server server)
		{
			logger.Log(LogLevel.Debug, ServerRemovedID, "Server removed with id {ServerId} and name \"{ServerName}\"", server.Id, server.Name);

			try
			{
				ServerRemoved?.Invoke(server);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Warning, ServerRemovedEventErrorID, ex, "Exception in event handler for server removement with id {ServerId}", server.Id);
			}
		}

		public void ThrowUnlessConnected()
		{
			if (connectStatus != ConnectStatus.Connected)
			{
				throw new InvalidOperationException("Enable to do this operation if client hasn't connected");
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

#pragma warning disable S907 //goto statement use
		/// <summary>
		/// Checks connection to discord server and if fail awaits it
		/// </summary>
		/// <returns>Wait task that will be complited only when connection will be alive</returns>
		public async Task CheckAndAwaitConnectionAsync()
		{
			if (connectStatus != ConnectStatus.Connected)
				return;

		reset:
			try
			{
				//Demo operation
				await client.GetUserAsync(SelfAccount.Id, updateCache: true);
			}
			catch (Exception)
			{
				logger.Log(LogLevel.Warning, NoServerConnectionID, "No connection to discord server! Waiting 1000ms");
				await Task.Delay(1000);
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
#pragma warning restore S907 //goto statement use


		/// <summary>
		/// Options of DSharp client
		/// </summary>
		public class Options
		{
			/// <summary>
			/// Discord's bot token, see discord documentation
			/// </summary>
			public string Token { get; set; } = "";

			/// <summary>
			/// Options for each server
			/// </summary>
			public Server.Options ServerOptions { get; set; } = new();

			/// <summary>
			/// Options for default channel messages caches, if you use custom fill with null
			/// </summary>
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

		private enum ConnectStatus
		{
			NoConnection,
			Connected,
			Disconnected,
		}
	}
}
