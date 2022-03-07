using CGZBot3.GlobalEvents;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Data.Json
{
	internal class ServersStatesRepository : IServersStatesRepository
	{
		private readonly static EventId ContextOperationErrorID = new(11, "ContextOperationError");
		private readonly static EventId OperationFatalID = new(13, "OperationFatal");


		private readonly JsonContext context;
		private readonly IServiceProvider provider;
		private readonly ILogger<ServersStatesRepository> logger;

		public ServersStatesRepository(IOptions<Options> options, IServiceProvider provider, ILogger<ServersStatesRepository> logger)
		{
			context = new JsonContext(options.Value.BaseDirectory);
			this.provider = provider;
			this.logger = logger;
		}


		public void DeleteServer(IServer server, string key)
		{
			DoContextOpertionSafe<object?>(() => { context.Delete(server, key); return null; }, "Can't delete state key");
		}

		public void DeleteServer(IServer server)
		{
			DoContextOpertionSafe<object?>(() => { context.DeleteAll(server); return null; }, "Can't delete all server state");
		}

		public Task PreloadDataAsync()
		{
			return context.LoadAllAsync();
		}

		public TModel GetOrCreate<TModel>(IServer server, string key) where TModel : class
		{
			var factory = provider.GetRequiredService<IModelFactory<TModel>>();
			return DoContextOpertionSafe(() => context.Load(server, key, factory), "Can't load state object");
		}

		public void Update<TModel>(IServer server, TModel state, string key) where TModel : class
		{
			DoContextOpertionSafe<object?>(() => { context.Put(server, key, state); return null; }, "Can't put state object");
		}

		private TResult DoContextOpertionSafe<TResult>(Func<TResult> action, string logString)
		{
			var exceptions = new List<Exception>();

			for (int i = 0; i < 5; i++)
			{
				try
				{
					return action();
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Warning, ContextOperationErrorID, ex, "{LogString}. Retry in 5s", logString);
					exceptions.Add(ex);
				}

				Thread.Sleep(5000);
			}

			logger.Log(LogLevel.Error, OperationFatalID, "{LogString}. Operation skip!", logString);
			throw new AggregateException(exceptions);
		}


		public class Options
		{
			public string BaseDirectory { get; set; } = "";
		}
	}
}
