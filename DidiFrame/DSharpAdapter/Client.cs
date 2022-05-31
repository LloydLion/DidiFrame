using DidiFrame.Culture;
using DSharpPlus;
using DSharpPlus.Exceptions;
using System.Globalization;

namespace DidiFrame.DSharpAdapter
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
		private readonly static EventId MessageSentHandlerExceptionID = new(44, "MessageSentHandlerException");
		private readonly static EventId SafeOperationErrorID = new(23, "SafeOperationError");
		private readonly static EventId SafeOperationCriticalErrorID = new(22, "SafeOperationCriticalError");
		private readonly static EventId NoServerConnectionID = new(21, "NoServerConnection");


		private readonly DiscordClient client;
		private readonly List<Server> servers = new();
		private Task? serverListUpdateTask;
		private readonly CancellationTokenSource cts = new();
		private readonly ILogger<Client> logger;

		public event MessageSentEventHandler? MessageSent;

		public event MessageDeletedEventHandler? MessageDeleted;


		public IReadOnlyCollection<IServer> Servers => servers;

		public IUser SelfAccount => new User(client.CurrentUser, this);

		/// <summary>
		/// Base client from DSharpPlus library
		/// </summary>
		public DiscordClient BaseClient => client;

		internal IServerCultureProvider CultureProvider { get; }


		/// <summary>
		/// Creates instance of DidiFrame.DSharpAdapter.Client
		/// </summary>
		/// <param name="options">Configuration of DSharp client (DidiFrame.DSharpAdapter.Client.Options)</param>
		/// <param name="factory">Loggers for DSharp client</param>
		/// <param name="cultureProvider">Culture provider for event thread culture</param>
		public Client(IOptions<Options> options, ILoggerFactory factory, IServerCultureProvider? cultureProvider = null)
		{
			var opt = options.Value;
			logger = factory.CreateLogger<Client>();

			client = new DiscordClient(new DiscordConfiguration
			{
				Token = opt.Token,
				AutoReconnect = true,
				HttpTimeout = new TimeSpan(0, 1, 0),
				TokenType = TokenType.Bot,
				LoggerFactory = factory,
				Intents = DiscordIntents.All
			});

			CultureProvider = cultureProvider ?? new GagCultureProvider(new CultureInfo("en-US"));
		}

		//Must be invoked from TextChannel objects
		internal void OnMessageCreated(Message message)
		{
			try
			{
				MessageSent?.Invoke(this, message);
			}
			catch (Exception ex)
			{
				client?.Logger.Log(LogLevel.Warning, MessageSentHandlerExceptionID, ex, "Execution of event handler for message sent event finished with exception");
			}
		}

		internal void OnMessageDeleted(Message message)
		{
			try
			{
				MessageDeleted?.Invoke(this, message);
			}
			catch (Exception ex)
			{
				client?.Logger.Log(LogLevel.Warning, MessageSentHandlerExceptionID, ex, "Execution of event handler for message deleted event finished with exception");
			}
		}

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
					else servers.Add(new Server(server.Value, this));
				}

				foreach (var item in temp) item.Dispose();

				await Task.Delay(new TimeSpan(5, 0, 0), token);
			}
		}

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

		/// <summary>
		/// Do safe opration under discord client
		/// </summary>
		/// <param name="operation">Operation delegate</param>
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

		/// <summary>
		/// Do safe opration under discord client with result
		/// </summary>
		/// <typeparam name="TReturn">Type of result</typeparam>
		/// <param name="operation">Operation delegate</param>
		/// <returns>Operation result</returns>
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

		/// <summary>
		/// Do safe async opration under discord client
		/// </summary>
		/// <param name="operation">Async operation delegate</param>
		/// <returns>Wait task</returns>
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

		/// <summary>
		/// Do safe async opration under discord client with result
		/// </summary>
		/// <typeparam name="TReturn">Type of result</typeparam>
		/// <param name="operation">Async operation delegate</param>
		/// <returns>Async operation result</returns>
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
			public string Token { get; set; } = "";
		}


		public struct NotFoundInfo
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
