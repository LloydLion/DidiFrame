using DidiFrame.Data.ContextBased;
using DidiFrame.Utils;
using DidiFrame.Utils.Json;

namespace DidiFrame.Data.Json
{
	internal class JsonContext : IDataContext
	{
		private static readonly EventId FileSaveErrorID = new(21, "FileSaveError");

		private readonly ThreadLocker<IServer> locker = new();
		private readonly JsonCache cache;
		private readonly ILogger logger;


		public JsonContext(DataOptions options, ContextType contextType, ILogger logger, IServiceProvider _)
		{
			cache = new JsonCache((contextType == ContextType.Settings ? options.Settings?.BaseDirectory : options.States?.BaseDirectory) ?? throw new ArgumentNullException(nameof(options)));
			this.logger = logger;
		}


		public TModel Load<TModel>(IServer server, string key, IModelFactory<TModel>? factory = null) where TModel : class
		{
			if (factory is null) return Load<TModel>(server, key);

			using (locker.Lock(server))
			{
				var path = GetFileForServer(server);
				bool isRepatchCollection = false;
				var serializer = JsonSerializerFactory.CreateWithConverters(server, logger, (obj, str, ex) => { isRepatchCollection = true; });

				TModel model;

				if (cache.Has(path, key) == false)
				{
					model = factory.CreateDefault();
					cache.PutAsync(path, key, model, serializer);
				}
				else
				{
					model = cache.Get<TModel>(path, key, serializer, out var task);
					if (task is not null) task.ContinueWith(s =>
					{
						if (isRepatchCollection)
						{
							//Loaded collection will not contain errors
							PrivatePut(server, key, model);
						}
					});
				}

				return model;
			}
		}
		
		private TModel Load<TModel>(IServer server, string key) where TModel : class
		{
			using (locker.Lock(server))
			{
				var path = GetFileForServer(server);
				bool isRepatchCollection = false;
				var serializer = JsonSerializerFactory.CreateWithConverters(server, logger, (obj, str, ex) => { isRepatchCollection = true; });

				var model = cache.Get<TModel>(path, key, serializer, out var task);
				if (task is not null) task.ContinueWith(s =>
				{
					if (isRepatchCollection)
					{
						//Loaded collection will not contain errors
						PrivatePut(server, key, model);
					}
				});

				return model;
			}
		}

		private async void PrivatePut<TModel>(IServer server, string key, TModel model) where TModel : class
		{
			try
			{
				await cache.PutAsync(GetFileForServer(server), key, model, JsonSerializerFactory.CreateWithConverters(server, logger));
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Warning, FileSaveErrorID, ex, "Enable save changes into files");
			}
		}

		public void Put<TModel>(IServer server, string key, TModel model) where TModel : class
		{
			using (locker.Lock(server))
			{
				PrivatePut(server, key, model);
			}
		}

		public Task PreloadDataAsync()
		{
			return cache.LoadAllAsync();
		}

		private static string GetFileForServer(IServer server) => $"{server.Id}.json";
	}
}
