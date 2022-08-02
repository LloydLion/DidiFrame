using DidiFrame.Utils;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace DidiFrame.Data.Mongo
{
	internal class MongoCache
	{
		private static readonly EventId FailedToLoadDocumentID = new(45, "FailedToLoadDocument");
		private static readonly EventId FailedToSaveDocumentID = new(46, "FailedToSaveDocument");


		private readonly Dictionary<ulong, Dictionary<string, object>> states = new();
		private readonly Dictionary<ulong, Dictionary<string, JContainer>> initialData = new();
		private readonly ThreadLocker<(ulong, string)> globalLocker = new();
		private readonly DatabaseWriteDispatcher dispatcher;
		private readonly IMongoDatabase db;
		private readonly ILogger logger;


		public MongoCache(IMongoDatabase db, ILogger logger)
		{
			this.db = db;
			this.logger = logger;
			dispatcher = new(db, logger);
		}


		public async Task LoadAllAsync()
		{
			var cursor = db.ListCollectionNames();
			foreach (var colName in await cursor.ToListAsync())
			{
				if (ulong.TryParse(colName, out var value) == false) continue;

				var dic = new Dictionary<string, JContainer>();
				initialData.Add(value, dic);

				var collection = db.GetCollection<BsonDocument>(colName);

				foreach (var document in await collection.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync())
				{
					try
					{
						var clone = document.ToBsonDocument();
						clone.Remove("_id");
						var id = clone.GetElement("Key").Value.AsString;
						clone.Remove("Key");
						var json = clone.ToJson(new JsonWriterSettings() { OutputMode = JsonOutputMode.RelaxedExtendedJson });
						var parse = (JContainer)(JObject.Parse(json).GetValue("Container") ?? throw new NullReferenceException());

						dic.Add(id, parse);
					}
					catch (Exception ex)
					{
						logger.Log(LogLevel.Error, FailedToLoadDocumentID, ex, "Failed to load some document in collection {CollectionName}", colName);
					}
				}
			}

			cursor.Dispose();
		}

		private async Task PutDirectAsync<TModel>(ulong serverId, string key, TModel value, JsonSerializer serializer) where TModel : class
		{
			if (states.ContainsKey(serverId) == false) states.Add(serverId, new());
			var file = states[serverId];
			if (file.ContainsKey(key)) file.Remove(key);
			file.Add(key, value);

			await SaveAsync(serverId, key, serializer);
		}

		public Task PutAsync<TModel>(ulong serverId, string key, TModel value, JsonSerializer serializer) where TModel : class
		{
			using (globalLocker.Lock((serverId, key)))
			{
				return PutDirectAsync(serverId, key, value, serializer);
			}
		}

		public TModel Get<TModel>(ulong serverId, string key, JsonSerializer serializer, out Task? putTask) where TModel : class
		{
			using (globalLocker.Lock((serverId, key)))
			{
				if (states.ContainsKey(serverId) && states[serverId].ContainsKey(key))
				{
					putTask = null;
					return (TModel)states[serverId][key];
				}
				else
				{
					var final = initialData[serverId][key].ToObject<TModel>(serializer)
						?? throw new NullReferenceException("Key present in json, but contains null");
					putTask = PutDirectAsync(serverId, key, final, serializer);
					return final;
				}
			}
		}

		public bool Has(ulong serverId, string key)
		{
			return (states.ContainsKey(serverId) && states[serverId].ContainsKey(key)) ||
				(initialData.ContainsKey(serverId) && initialData[serverId].ContainsKey(key));
		}

		public Task SaveAsync(ulong serverId, string key, JsonSerializer serializer)
		{
			return dispatcher.QueueTask(new(serverId, key, getJson));

			JContainer getJson() => (JContainer)JToken.FromObject(states[serverId][key], serializer);
		}


		private class DatabaseWriteDispatcher
		{
			private readonly Queue<ScheduleItem> tasks = new();
			private readonly Thread thread;
			private readonly AutoResetEvent tasksWaiter = new(false);
			private readonly IMongoDatabase database;
			private readonly ILogger logger;
			private bool isClosed = false;


			public DatabaseWriteDispatcher(IMongoDatabase database, ILogger logger)
			{
				thread = new Thread(Executor);
				thread.Start();
				this.database = database;
				this.logger = logger;
			}


			~DatabaseWriteDispatcher()
			{
				isClosed = true;
				tasksWaiter.Set();
				thread.Join();
			}

			public Task QueueTask(WriteTask task)
			{
				lock (tasks)
				{
					var source = new TaskCompletionSource();
					tasks.Enqueue(new(task, source));
					tasksWaiter.Set();
					return source.Task;
				}
			}

			private void Executor()
			{
				while (true)
				{
					tasksWaiter.WaitOne();
					while (tasks.Count > 0)
					{
						if (isClosed) return;

						ScheduleItem task;
						lock (tasks)
						{
							task = tasks.Dequeue();
						}

						try
						{
							var collection = database.GetCollection<BsonDocument>(task.Task.Collection.ToString());

							var json = task.Task.JsonSource.Invoke();

							var item = new MongoItem(task.Task.Key, json);

							collection.DeleteOne(new BsonDocument("Key", new BsonString(task.Task.Key)));
							collection.InsertOne(BsonDocument.Parse(JsonConvert.SerializeObject(item)), new InsertOneOptions() { Comment = "Created at " + DateTime.Now.ToString() });

						}
						catch (Exception ex)
						{
							logger.Log(LogLevel.Error, FailedToSaveDocumentID, ex, "Enable to write state/settings to MongoDb for server {ServerId} and key {DataKey}", task.Task.Collection, task.Task.Key);
						}

						task.TaskCompletionSource.SetResult();
					}
				}
			}


			public record WriteTask(ulong Collection, string Key, Func<JContainer> JsonSource);

			private record ScheduleItem(WriteTask Task, TaskCompletionSource TaskCompletionSource);

			private record MongoItem(string Key, JContainer Container);
		}
	}
}
