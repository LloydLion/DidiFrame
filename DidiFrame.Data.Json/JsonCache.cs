using DidiFrame.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace DidiFrame.Data.Json
{
	internal class JsonCache
	{
		private static readonly EventId FileSaveErrorID = new(21, "FileSaveError");


		private readonly Dictionary<string, Dictionary<string, object>> states = new();
		private readonly Dictionary<string, Dictionary<string, JContainer>> initialData = new();
		private readonly string basePath;
		private readonly ThreadLocker<string> fileLocker = new();
		private readonly FileWriteDispatcher dispatcher;


		public JsonCache(string basePath, ILogger logger)
		{
			this.basePath = basePath;
			dispatcher = new(logger);
		}


		public async Task LoadAllAsync()
		{
			var files = Directory.GetFiles(basePath);
			var tasks = new List<Task<(string, Dictionary<string, JContainer>)>>();

			foreach (var file in files)
				tasks.Add(load(file));

			var results = await Task.WhenAll(tasks);

			foreach (var result in results) initialData.Add(result.Item1, result.Item2);

			static async Task<(string, Dictionary<string, JContainer>)> load(string file)
			{
				var content = await File.ReadAllTextAsync(file);
				var obj = JsonConvert.DeserializeObject<Dictionary<string, JContainer?>>(content)
					?? throw new NullReferenceException("Json value was null");

				foreach (var item in obj.ToDictionary(s => s.Key, s => s.Value))
					if (item.Value is null) obj.Remove(item.Key);

				return (file.Split(Path.DirectorySeparatorChar).Last(), obj as Dictionary<string, JContainer>);
			}
		}

		private async Task PutDirectAsync<TModel>(string path, string key, TModel value, JsonSerializer serializer) where TModel : class
		{
			if (states.ContainsKey(path) == false) states.Add(path, new());
			var file = states[path];
			if (file.ContainsKey(key)) file.Remove(key);
			file.Add(key, value);

			await SaveAsync(path, serializer);
		}

		public Task PutAsync<TModel>(string path, string key, TModel value, JsonSerializer serializer) where TModel : class
		{
			using (fileLocker.Lock(path))
			{
				return PutDirectAsync(path, key, value, serializer);
			}
		}

		public TModel Get<TModel>(string path, string key, JsonSerializer serializer, out Task? putTask) where TModel : class
		{
			using (fileLocker.Lock(path))
			{
				if (states.ContainsKey(path) && states[path].ContainsKey(key))
				{
					putTask = null;
					return (TModel)states[path][key];
				}
				else
				{
					var final = initialData[path][key].ToObject<TModel>(serializer)
						?? throw new NullReferenceException("Key present in json, but contains null");
					putTask = PutDirectAsync(path, key, final, serializer);
					return final;
				}
			}
		}

		public bool Has(string path, string key)
		{
			using (fileLocker.Lock(path))
			{
				return (states.ContainsKey(path) && states[path].ContainsKey(key)) ||
					(initialData.ContainsKey(path) && initialData[path].ContainsKey(key));
			}
		}

		public Task SaveAsync(string path, JsonSerializer serializer)
		{
			var baseDic = initialData.ContainsKey(path) ? initialData[path].ToDictionary(s => s.Key, s => s.Value) : new();

			foreach (var l in states[path])
			{
				var jobj = (JContainer)JToken.FromObject(l.Value, serializer);
				if (baseDic.ContainsKey(l.Key)) baseDic.Remove(l.Key);
				baseDic.Add(l.Key, jobj);
			}

			var str = serializer.Serialize(baseDic);
			var file = Path.Combine(basePath, path);
			return dispatcher.QueueTask(new(file, Encoding.Default.GetBytes(str)));
		}


		private class FileWriteDispatcher
		{
			private readonly Queue<ScheduleItem> tasks = new();
			private readonly Thread thread;
			private readonly AutoResetEvent tasksWaiter = new(false);
			private readonly ILogger logger;
			private bool isClosed = false;


			public FileWriteDispatcher(ILogger logger)
			{
				thread = new Thread(Executor);
				thread.Start();
				this.logger = logger;
			}


			~FileWriteDispatcher()
			{
				isClosed = true;
				tasksWaiter.Set();
				thread.Join();
			}

			public Task QueueTask(FileTask task)
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
							File.WriteAllBytes(task.Task.Path, task.Task.Bytes);
						}
						catch (Exception ex)
						{
							logger.Log(LogLevel.Warning, FileSaveErrorID, ex, "Enable save changes into file at {FilePath}", task.Task.Path);
						}

						task.TaskCompletionSource.SetResult();
					}
				}
			}


			public record FileTask(string Path, byte[] Bytes);

			private record ScheduleItem(FileTask Task, TaskCompletionSource TaskCompletionSource);
		}
	}
}
