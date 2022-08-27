using DidiFrame.Data.ContextBased;
using DidiFrame.Clients;
using DidiFrame.Utils;
using DidiFrame.Utils.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DidiFrame.Data.Json
{
	/// <summary>
	/// Context for json files based data management
	/// </summary>
	public class JsonContext : IDataContext
	{
		private readonly JsonCache cache;
		private readonly ILogger logger;


		/// <summary>
		/// Creates new DidiFrame.Data.Json.JsonContext using parameters that context-based approach require
		/// </summary>
		/// <param name="options">Creation options</param>
		/// <param name="contextType">Type of context: state or settings</param>
		/// <param name="logger">Logger</param>
		/// <param name="_">Won't be used, is here only because that context based factories require</param>
		/// <exception cref="ArgumentNullException">If required option is null</exception>
		public JsonContext(DataOptions options, ContextType contextType, ILogger logger, IServiceProvider services)
		{
			var option = (contextType == ContextType.Settings ? options.Settings?.BaseDirectory : options.States?.BaseDirectory) ?? throw new ArgumentNullException(nameof(options));
			cache = new JsonCache(option, logger);
			this.logger = logger;

			//Context and notifier is global objects, we can don't ubsubcribe
			var notify = services.GetRequiredService<IServersNotify>();
			notify.ServerRemoved += OnServerRemoved;
		}


		/// <inheritdoc/>
		public TModel Load<TModel>(IServer server, string key, IModelFactory<TModel>? factory = null) where TModel : class
		{
			if (factory is null) return Load<TModel>(server, key);
			else
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
							cache.PutAsync(GetFileForServer(server), key, model, serializer);
						}
					});
				}

				return model;
			}
		}
		
		private TModel Load<TModel>(IServer server, string key) where TModel : class
		{
			var path = GetFileForServer(server);
			bool isRepatchCollection = false;
			var serializer = JsonSerializerFactory.CreateWithConverters(server, logger, (obj, str, ex) => { isRepatchCollection = true; });

			var model = cache.Get<TModel>(path, key, serializer, out var task);
			if (task is not null) task.ContinueWith(s =>
			{
				if (isRepatchCollection)
				{
					cache.PutAsync(GetFileForServer(server), key, model, serializer);
				}
			});

			return model;
		}

		/// <inheritdoc/>
		public void Put<TModel>(IServer server, string key, TModel model) where TModel : class
		{
			cache.PutAsync(GetFileForServer(server), key, model, JsonSerializerFactory.CreateWithConverters(server, logger));
		}

		/// <inheritdoc/>
		public Task PreloadDataAsync()
		{
			return cache.LoadAllAsync();
		}

		private void OnServerRemoved(IServer server)
		{
			cache.Delete(GetFileForServer(server));
		}

		private static string GetFileForServer(IServer server) => $"{server.Id}.json";
	}
}
