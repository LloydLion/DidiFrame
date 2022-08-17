using DidiFrame.Data.ContextBased;
using DidiFrame.Interfaces;
using DidiFrame.Utils.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DidiFrame.Data.Mongo
{
	/// <summary>
	/// Context for mongo database data management
	/// </summary>
	public class MongoDBContext : IDataContext
	{
		private readonly ILogger logger;
		private readonly MongoCache cache;


		/// <summary>
		/// Creates new DidiFrame.Data.MongoDB.MongoDBContext using parameters that context-based approach require
		/// </summary>
		/// <param name="options">Creation options</param>
		/// <param name="contextType">Type of context: state or settings</param>
		/// <param name="logger">Logger</param>
		/// <param name="_">Service provider to provide addititional data</param>
		/// <exception cref="ArgumentNullException">If required option is null</exception>
		public MongoDBContext(DataOptions options, ContextType contextType, ILogger logger, IServiceProvider services)
		{
			DataOptions.DataOption? option = contextType == ContextType.States ? options.States : options.Settings;

			if (option is null) throw new ArgumentNullException(nameof(options));

			var dbClient = new MongoClient(option.ConnectionString);
			var db = dbClient.GetDatabase(option.DatabaseName);
			this.logger = logger;

			//Context and notifier is global objects, we can don't ubsubcribe
			var notify = services.GetRequiredService<IServersNotify>();
			notify.ServerRemoved += OnServerRemoved;

			cache = new(db, logger);
		}


		private void OnServerRemoved(IServer server)
		{
			cache.Delete(server.Id);
		}

		/// <inheritdoc/>
		public async void Put<TModel>(IServer server, string key, TModel model) where TModel : class
		{
			var serializer = JsonSerializerFactory.CreateWithConverters(server, logger);

			await cache.PutAsync(server.Id, key, model, serializer);
		}

		private TModel Load<TModel>(IServer server, string key) where TModel : class
		{
			var id = server.Id;
			bool isRepatchJson = false;

			var serializer = JsonSerializerFactory.CreateWithConverters(server, logger, (_1, _2, _3) => isRepatchJson = true);

			var result = cache.Get<TModel>(id, key, serializer, out var task);

			if (task is not null) task.ContinueWith(s =>
			{
				if (isRepatchJson)
				{
					cache.PutAsync(id, key, result, serializer);
				}
			});

			return result;
		}

		/// <inheritdoc/>
		public TModel Load<TModel>(IServer server, string key, IModelFactory<TModel>? factory = null) where TModel : class
		{
			if (factory is null) return Load<TModel>(server, key);
			else
			{
				var id = server.Id;
				bool isRepatchJson = false;

				var serializer = JsonSerializerFactory.CreateWithConverters(server, logger, (obj, str, ex) => { isRepatchJson = true; });

				TModel model;

				if (cache.Has(id, key) == false)
				{
					model = factory.CreateDefault();
					cache.PutAsync(id, key, model, serializer);
				}
				else
				{
					model = cache.Get<TModel>(id, key, serializer, out var task);
					if (task is not null) task.ContinueWith(s =>
					{
						if (isRepatchJson)
						{
							cache.PutAsync(id, key, model, serializer);
						}
					});
				}

				return model;
			}
		}

		/// <inheritdoc/>
		public Task PreloadDataAsync() => cache.LoadAllAsync();
	}
}
