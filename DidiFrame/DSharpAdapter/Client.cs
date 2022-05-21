using DidiFrame.Culture;
using DSharpPlus;
using DSharpPlus.Exceptions;

namespace DidiFrame.DSharpAdapter
{
	/// <summary>
	/// Discord client based on DSharpPlus library
	/// </summary>
	public class Client : IClient
	{
		private readonly static EventId MessageSentHandlerExceptionID = new(44, "MessageSentHandlerException");
		private readonly static EventId SafeOperationErrorID = new(23, "SafeOperationError");
		private readonly static EventId SafeOperationCriticalErrorID = new(22, "SafeOperationCriticalError");


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
		/// <param name="options">Configuration of DSharp client</param>
		/// <param name="factory">Loggers for DSharp client</param>
		/// <param name="cultureProvider">Culture provider for event thread culture</param>
		public Client(IOptions<Options> options, ILoggerFactory factory, IServerCultureProvider cultureProvider)
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
			CultureProvider = cultureProvider;
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

		public Task AwaitForExit()
		{
			return Task.Delay(-1);
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
				await client.GetUserAsync(SelfAccount.Id);
			}
			catch (Exception)
			{
				await Task.Delay(450);
				goto reset;
			}
		}

		/// <summary>
		/// Do safe opration under discord client
		/// </summary>
		/// <param name="operation">Operation delegate</param>
		public void DoSafeOperation(Action operation)
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
				if (ex is AggregateException ar) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					goto reset;
				}
				else
				{
					logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");
					throw;
				}
			}
		}

		/// <summary>
		/// Do safe opration under discord client with result
		/// </summary>
		/// <typeparam name="TReturn">Type of result</typeparam>
		/// <param name="operation">Operation delegate</param>
		/// <returns>Operation result</returns>
		public TReturn DoSafeOperation<TReturn>(Func<TReturn> operation)
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
				if (ex is AggregateException ar) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					goto reset;
				}
				else
				{
					logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");
					throw;
				}
			}
		}

		/// <summary>
		/// Do safe async opration under discord client
		/// </summary>
		/// <param name="operation">Async operation delegate</param>
		/// <returns>Wait task</returns>
		public async Task DoSafeOperationAsync(Func<Task> operation)
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
				if (ex is AggregateException ar) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					goto reset;
				}
				else
				{
					logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");
					throw;
				}
			}
		}

		/// <summary>
		/// Do safe async opration under discord client with result
		/// </summary>
		/// <typeparam name="TReturn">Type of result</typeparam>
		/// <param name="operation">Async operation delegate</param>
		/// <returns>Async operation result</returns>
		public async Task<TReturn> DoSafeOperationAsync<TReturn>(Func<Task<TReturn>> operation)
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
				if (ex is AggregateException ar) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					goto reset;
				}
				else
				{
					logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");
					throw;
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
	}
}
