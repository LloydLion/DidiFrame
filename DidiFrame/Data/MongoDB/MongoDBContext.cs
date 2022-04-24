using DidiFrame.Data.JsonEnvironment.Converters;
using DidiFrame.Utils;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Text;

namespace DidiFrame.Data.MongoDB
{
	internal class MongoDBContext
	{
		private readonly EventId CollectionElementParseErrorID = new(41, "CollectionElementParseError");


		private readonly IMongoDatabase db;
		private readonly Dictionary<IServer, Dictionary<string, object>> cache = new();
		private readonly ThreadLocker<IServer> dbLocker = new();
		private readonly ThreadLocker<IServer> opLocker = new();
		private readonly ILogger logger;
		private readonly IClient client;


		public MongoDBContext(ILogger logger, IClient client, string connectionString, string dbName)
		{
			var dbClient = new MongoClient(connectionString);
			db = dbClient.GetDatabase(dbName);
			this.logger = logger;
			this.client = client;
		}


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

			PushChangesAsync(server, key);
		}

		public TModel Load<TModel>(IServer server, string key) where TModel : class
		{
			return (TModel)cache[server][key];
		}

		public TModel Load<TModel>(IServer server, string key, IModelFactory<TModel> factory) where TModel : class
		{
			if (cache.ContainsKey(server) == false) cache.Add(server, new());
			var serverCache = cache[server];
			if (serverCache.ContainsKey(key) == false) serverCache.Add(key, factory.CreateDefault());
			return (TModel)serverCache[key];
		}

		private async void PushChangesAsync(IServer server, string key)
		{
			var cnames = await db.ListCollectionNames().ToListAsync();

			using (dbLocker.Lock(server))
			{
				if (!cnames.Contains(server.Id.ToString()))
					await db.CreateCollectionAsync(server.Id.ToString());
				var collection = db.GetCollection<BsonDocument>(server.Id.ToString());

				var obj = cache[server][key];
				var ser = CreateSerializer(server);
				var jcont = JToken.FromObject(obj, ser);
				var model = new SaveModel(key, obj.GetType(), (JContainer)jcont);
				var json = new StringBuilder();
				ser.Serialize(new JsonTextWriter(new StringWriter(json)), model);
				var raw = json.ToString();

				await collection.DeleteManyAsync(new BsonDocument("Key", new BsonString(key)));
				await collection.InsertOneAsync(BsonDocument.Parse(raw));
			}
		}

		public async Task LoadAllAsync()
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
					var ser = CreateSerializer(server, (_1, _2, _3) => { isRepatchJson = true; });
					var model = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveModel>(json);
					if (model is null) continue;

					var data = model.Document.ToObject(model.Type, ser);
					if (data is null) continue;

					serverCache.Add(model.Key, data);

					if (isRepatchJson) PushChangesAsync(server, model.Key);
				}
			}

			cursor.Dispose();
		}

		private static JsonSerializer CreateSerializerInternal(IServer server, Action<JContainer, string, Exception> invalidCollectionElementCallback)
		{
			var ret = new JsonSerializer();

			ret.Converters.Add(new AbstractConveter());

			ret.Converters.Add(new CategoryConveter(server));
			ret.Converters.Add(new ChannelConverter(server));
			ret.Converters.Add(new MemberConveter(server));
			ret.Converters.Add(new RoleConverter(server));
			ret.Converters.Add(new MessageConverter(server));
			ret.Converters.Add(new ServerConveter(server.Client));
			ret.Converters.Add(new StringEnumConverter());

			ret.Converters.Add(new SafeCollectionConveter(invalidCollectionElementCallback));

			return ret;
		}

		private JsonSerializer CreateSerializer(IServer server)
		{
			return CreateSerializerInternal(server, DefaultInvalidCollectionElementCallback);
		}

		private JsonSerializer CreateSerializer(IServer server, Action<JContainer, string, Exception> invalidCollectionElementCallback)
		{
			var deleg = new Action<JContainer, string, Exception>(invalidCollectionElementCallback);
			deleg += DefaultInvalidCollectionElementCallback;
			return CreateSerializerInternal(server, deleg);
		}

		private void DefaultInvalidCollectionElementCallback(JContainer root, string str, Exception ex)
		{
			logger.Log(LogLevel.Warning, CollectionElementParseErrorID, ex, "Enable to convert object:\n{Json}", root.SelectToken(str));
		}


		private record SaveModel(string Key, Type Type, JContainer Document);
	}
}
