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
		private static readonly EventId FailedToDeleteCollectionID = new(47, "FailedToDeleteCollection");
		private static readonly EventId InvalidMongoTaskID = new(11, "InvalidMongoTask");


		private readonly Dictionary<ulong, Dictionary<string, object>> states = new();
		private readonly Dictionary<ulong, Dictionary<string, JContainer>> initialData = new();
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

		public void Delete(ulong id)
		{
			lock (states)
			{
				lock (initialData)
				{
					states.Remove(id);
					initialData.Remove(id);

					dispatcher.QueueTask(new DatabaseWriteDispatcher.DeleteTask(id));
				}
			}
		}

		public Task PutAsync<TModel>(ulong serverId, string key, TModel value, JsonSerializer serializer) where TModel : class
		{
			lock (states)
			{
				if (states.ContainsKey(serverId) == false) states.Add(serverId, new());
				var file = states[serverId];
				if (file.ContainsKey(key)) file.Remove(key);
				file.Add(key, value);

				return SaveAsync(serverId, key, serializer);
			}
		}

		public TModel Get<TModel>(ulong serverId, string key, JsonSerializer serializer, out Task? putTask) where TModel : class
		{
			lock (states)
			{
				if (states.ContainsKey(serverId) && states[serverId].ContainsKey(key))
				{
					putTask = null;
					return (TModel)states[serverId][key];
				}
				else
				{
					lock (initialData)
					{
						var final = initialData[serverId][key].ToObject<TModel>(serializer)
							?? throw new NullReferenceException("Key present in json, but contains null");
						putTask = PutAsync(serverId, key, final, serializer);
						return final;
					}
				}
			}
		}

		public bool Has(ulong serverId, string key)
		{
			lock (states)
			{
				lock (initialData)
				{
					return (states.ContainsKey(serverId) && states[serverId].ContainsKey(key)) ||
						(initialData.ContainsKey(serverId) && initialData[serverId].ContainsKey(key));
				}
			}
		}

		public Task SaveAsync(ulong serverId, string key, JsonSerializer serializer)
		{
			var toSave = states[serverId][key];

			return dispatcher.QueueTask(new DatabaseWriteDispatcher.WriteTask(serverId, key, getJson));

			JContainer getJson() => (JContainer)JToken.FromObject(toSave, serializer);
		}


		private sealed class DatabaseWriteDispatcher
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

			public Task QueueTask(CollectionTask task)
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

						if (task.Task is WriteTask wt)
						{
							try
							{
								var collection = database.GetCollection<BsonDocument>(wt.Collection.ToString());

								var json = wt.JsonSource.Invoke();

								var item = new MongoItem(wt.Key, json);

								collection.DeleteOne(new BsonDocument("Key", new BsonString(wt.Key)));
								collection.InsertOne(BsonDocument.Parse(JsonConvert.SerializeObject(item)), new InsertOneOptions() { Comment = "Created at " + DateTime.Now.ToString() });

							}
							catch (Exception ex)
							{
								logger.Log(LogLevel.Error, FailedToSaveDocumentID, ex, "Enable to write state/settings to MongoDb for server {ServerId} and key {DataKey}", wt.Collection, wt.Key);
							}
						}
						else if (task.Task is DeleteTask dt)
						{
							try
							{
								database.DropCollection(dt.Collection.ToString());
							}
							catch (Exception ex)
							{
								logger.Log(LogLevel.Error, FailedToDeleteCollectionID, ex, "Enable to delete state/settings from MongoDb for server {ServerId}", dt.Collection);
							}
						}
						else
						{
							logger.Log(LogLevel.Error, InvalidMongoTaskID, "Invalid type of mongo task. Enable to execute: {TypeOfTask}", task.Task.GetType().FullName);
						}

						task.TaskCompletionSource.SetResult();
					}
				}
			}


			public record WriteTask(ulong Collection, string Key, Func<JContainer> JsonSource) : CollectionTask;

			public record DeleteTask(ulong Collection) : CollectionTask;

			public record CollectionTask();

			private sealed record ScheduleItem(CollectionTask Task, TaskCompletionSource TaskCompletionSource);

			private sealed record MongoItem(string Key, JContainer Container);
		}
	}
}
