using CGZBot3.Data.Json.Converters;
using CGZBot3.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace CGZBot3.Data.Json
{
	internal class JsonContext
	{
		private readonly ThreadLocker<IServer> locker = new();
		private readonly FileCache cache;


		public JsonContext(string directoryPath)
		{
			cache = new FileCache(directoryPath, "{}");
		}


		public TModel Load<TModel>(IServer server, string key, IModelFactory<TModel> factory) where TModel : class
		{
			using (locker.Lock(server))
			{
				var path = GetFileForServer(server);
				var content = cache.GetString(path);

				var serializer = CreateSerializer(server);

				var jobj = serializer.Deserialize<JObject>(content);

				var model = jobj.GetValue(key)?.ToObject<TModel>(serializer);

				if (model is null) return PutDefault(server, key, factory);
				else return model;
			}
		}
		
		public TModel Load<TModel>(IServer server, string key) where TModel : class
		{
			using (locker.Lock(server))
			{
				var path = GetFileForServer(server);
				var content = cache.GetString(path);

				var serializer = CreateSerializer(server);

				var jobj = serializer.Deserialize<JObject>(content);

				var model = jobj.GetValue(key)?.ToObject<TModel>(serializer) ?? throw new ArgumentException("Key don't present in json or section contains invalid data", nameof(key));

				return model;
			}
		}

		private async void PrivatePut<TModel>(IServer server, string key, TModel model) where TModel : class
		{
			if (model is null) throw new ArgumentNullException(nameof(model));

			var path = GetFileForServer(server);
			var serializer = CreateSerializer(server);

			var content = cache.GetString(path);
			var jobj = serializer.Deserialize<JObject>(content);

			if (jobj.ContainsKey(key)) jobj.Remove(key);
			jobj.Add(key, model is IEnumerable en ? JArray.FromObject(en.Cast<object>().ToArray(), serializer) : JObject.FromObject(model, serializer));

			cache.Put(path, serializer.Serialize(jobj));
			await cache.SaveAsync();
		}

		public TModel PutDefault<TModel>(IServer server, string key, IModelFactory<TModel> factory) where TModel : class
		{
			var model = factory.CreateDefault();
			PrivatePut(server, key, model);
			return model;
		}

		public void Put<TModel>(IServer server, string key, TModel model) where TModel : class
		{
			using (locker.Lock(server))
			{
				PrivatePut(server, key, model);
			}
		}

		public Task LoadAllAsync()
		{
			return cache.LoadAllAsync();
		}

		public async void Delete(IServer server, string key)
		{
			using (locker.Lock(server))
			{
				var path = GetFileForServer(server);
				var serializer = CreateSerializer(server);

				var content = cache.GetString(path);
				var jobj = serializer.Deserialize<JObject>(content);

				jobj.Remove(key);

				cache.Put(path, serializer.Serialize(jobj));
				await cache.SaveAsync();
			}
		}

		public void DeleteAll(IServer server)
		{
			using (locker.Lock(server))
			{
				var path = GetFileForServer(server);
				cache.PutDefault(path);
			}
		}

		private static string GetFileForServer(IServer server) => $"{server.Id}.json";

		private static JsonSerializer CreateSerializer(IServer server)
		{
			var ret = new JsonSerializer()
			{
				Formatting = Formatting.Indented,
			};

			ret.Converters.Add(new AbstractConveter());

			ret.Converters.Add(new CategoryConveter(server));
			ret.Converters.Add(new ChannelConverter(server));
			ret.Converters.Add(new MemberConveter(server));
			ret.Converters.Add(new RoleConverter(server));
			ret.Converters.Add(new ServerConveter(server.Client));

			ret.Converters.Add(new SafeCollectionConveter((str, ex) => { }));

			return ret;
		}
	}
}
