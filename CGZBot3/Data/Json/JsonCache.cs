using CGZBot3.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CGZBot3.Data.Json
{
	internal class JsonCache
	{
		private readonly Dictionary<string, Dictionary<string, object>> states = new();
		private readonly Dictionary<string, Dictionary<string, JContainer>> initialData = new();
		private readonly string basePath;
		private static readonly ThreadLocker<JsonCache> globalLocker = new();
		private readonly ThreadLocker<string> saveLocker = new();


		public JsonCache(string basePath)
		{
			this.basePath = basePath;
		}


		public async Task LoadAllAsync()
		{
			var files = Directory.GetFiles(basePath);
			var tasks = new List<Task<(string, Dictionary<string, JContainer>)>>();

			foreach (var file in files)
				tasks.Add(load(file));

			var results = await Task.WhenAll(tasks);

			foreach (var result in results) initialData.Add(result.Item1, result.Item2);

			async Task<(string, Dictionary<string, JContainer>)> load(string file)
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
			using (globalLocker.Lock(this))
			{
				return PutDirectAsync(path, key, value, serializer);
			}
		}

		public TModel Get<TModel>(string path, string key, JsonSerializer serializer, out Task? putTask) where TModel : class
		{
			using (globalLocker.Lock(this))
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
			return (states.ContainsKey(path) && states[path].ContainsKey(key)) ||
			(initialData.ContainsKey(path) && initialData[path].ContainsKey(key));
		}

		public async Task SaveAsync(string path, JsonSerializer serializer)
		{
			using (saveLocker.Lock(path))
			{
				//Serialization - fast process, deserialization - slow
				
				var baseDic = initialData.ContainsKey(path) ? initialData[path].ToDictionary(s => s.Key, s => s.Value) : new();
				
				foreach (var l in states[path])
				{
					var jobj = (JContainer)JToken.FromObject(l.Value, serializer);
					if (baseDic.ContainsKey(l.Key)) baseDic.Remove(l.Key);
					baseDic.Add(l.Key, jobj);
				}

				var str = serializer.Serialize(baseDic);
				var file = Path.Combine(basePath, path);
				await File.WriteAllTextAsync(file, str);
			}
		}
	}
}
