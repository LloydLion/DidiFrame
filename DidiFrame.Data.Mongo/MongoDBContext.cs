using DidiFrame.Data.ContextBased;
using DidiFrame.Interfaces;
using DidiFrame.Utils;
using DidiFrame.Utils.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace DidiFrame.Data.MongoDB
{
	/// <summary>
	/// Context for mongo database data management
	/// </summary>
	public class MongoDBContext : IDataContext
	{
		private readonly IMongoDatabase db;
		private readonly Dictionary<IServer, Dictionary<string, object>> cache = new();
		private readonly ThreadLocker<IServer> dbLocker = new();
		private readonly ThreadLocker<IServer> opLocker = new();
		private readonly ILogger logger;
		private readonly IClient client;


		/// <summary>
		/// Creates new DidiFrame.Data.MongoDB.MongoDBContext using parameters that context-based approach require
		/// </summary>
		/// <param name="options">Creation options</param>
		/// <param name="contextType">Type of context: state or settings</param>
		/// <param name="logger">Logger</param>
		/// <param name="services">Service provider to provide addititional data</param>
		/// <exception cref="ArgumentNullException">If required option is null</exception>
		public MongoDBContext(DataOptions options, ContextBased.ContextType contextType, ILogger logger, IServiceProvider services)
		{
			DataOptions.DataOption? option = contextType == ContextBased.ContextType.States ? options.States : options.Settings;

			if (option is null) throw new ArgumentNullException(nameof(options));

			var dbClient = new MongoClient(option.ConnectionString);
			db = dbClient.GetDatabase(option.DatabaseName);
			this.logger = logger;
			client = services.GetRequiredService<IClient>();
		}


		/// <inheritdoc/>
		public void Put<TModel>(IServer server, string key, TModel model) where TModel : class
		{
			using (opLocker.Lock(server))
			{
				if (!cache.ContainsKey(server))
					cache.Add(server, new());
				var serverCache = cache[server];
				if (serverCache.ContainsKey(key))
					serverCache[key] = model;
				else serverCache.Add(key, model);
			}

			PushChangesAsync(server, key, typeof(TModel));
		}

		private TModel Load<TModel>(IServer server, string key) where TModel : class
		{
			return (TModel)cache[server][key];
		}

		/// <inheritdoc/>
		public TModel Load<TModel>(IServer server, string key, IModelFactory<TModel>? factory = null) where TModel : class
		{
			if (factory is null) return Load<TModel>(server, key);

			if (cache.ContainsKey(server) == false) cache.Add(server, new());
			var serverCache = cache[server];
			if (serverCache.ContainsKey(key) == false) serverCache.Add(key, factory.CreateDefault());
			return (TModel)serverCache[key];
		}

		private async void PushChangesAsync(IServer server, string key, Type ttype)
		{
			var cnames = await db.ListCollectionNames().ToListAsync();

			using (dbLocker.Lock(server))
			{
				if (!cnames.Contains(server.Id.ToString()))
					await db.CreateCollectionAsync(server.Id.ToString());
				var collection = db.GetCollection<BsonDocument>(server.Id.ToString());

				var obj = cache[server][key];
				var ser = JsonSerializerFactory.CreateWithConverters(server, logger);
				var jcont = JToken.FromObject(obj, ser);
				var model = new SaveModel(key, ttype, (JContainer)jcont);
				var json = new StringBuilder();
				ser.Serialize(new JsonTextWriter(new StringWriter(json)), model);
				var raw = json.ToString();

				await collection.DeleteManyAsync(new BsonDocument("Key", new BsonString(key)));
				await collection.InsertOneAsync(BsonDocument.Parse(raw));
			}
		}

		/// <inheritdoc/>
		public async Task PreloadDataAsync()
		{
			var cursor = db.ListCollectionNames();
			foreach (var colName in await cursor.ToListAsync())
			{
				if (!client.Servers.Any(s => s.Id.ToString() == colName)) continue;

				var collection = db.GetCollection<BsonDocument>(colName);
				var server = client.Servers.Single(s => s.Id.ToString() == colName);

				cache.Add(server, new());
				var serverCache = cache[server];

				foreach (var document in await collection.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync())
				{
					var isRepatchJson = false;

					var clone = document.ToBsonDocument();
					clone.Remove("_id");
					var json = clone.ToJson(new JsonWriterSettings() { OutputMode = JsonOutputMode.RelaxedExtendedJson });
					var ser = JsonSerializerFactory.CreateWithConverters(server, logger, (_1, _2, _3) => { isRepatchJson = true; });
					var model = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveModel>(json);
					if (model is null) continue;

					var data = model.Document.ToObject(model.Type, ser);
					if (data is null) continue;

					serverCache.Add(model.Key, data);

					if (isRepatchJson) PushChangesAsync(server, model.Key, model.Type);
				}
			}

			cursor.Dispose();
		}


		private record SaveModel(string Key, Type Type, JContainer Document);
	}
}
