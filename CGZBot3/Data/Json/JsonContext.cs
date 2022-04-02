using CGZBot3.Data.Json.Converters;
using CGZBot3.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace CGZBot3.Data.Json
{
	internal class JsonContext
	{
		private readonly EventId CollectionElementParseErrorID = new(41, "CollectionElementParseError");
		private readonly EventId FileSaveErrorID = new(21, "FileSaveError");

		private readonly ThreadLocker<IServer> locker = new();
		private readonly JsonCache cache;
		private readonly ILogger logger;


		public JsonContext(string directoryPath, ILogger logger)
		{
			cache = new JsonCache(directoryPath);
			this.logger = logger;
		}


		public TModel Load<TModel>(IServer server, string key, IModelFactory<TModel> factory) where TModel : class
		{
			using (locker.Lock(server))
			{
				var path = GetFileForServer(server);
				bool isRepatchCollection = false;
				var serializer = CreateSerializer(server, (obj, str, ex) => { isRepatchCollection = true; });

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
		
		public TModel Load<TModel>(IServer server, string key) where TModel : class
		{
			using (locker.Lock(server))
			{
				var path = GetFileForServer(server);
				bool isRepatchCollection = false;
				var serializer = CreateSerializer(server, (obj, str, ex) => { isRepatchCollection = true; });

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
				await cache.PutAsync(GetFileForServer(server), key, model, CreateSerializer(server));
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

		public Task LoadAllAsync()
		{
			return cache.LoadAllAsync();
		}

		private static string GetFileForServer(IServer server) => $"{server.Id}.json";

		private static JsonSerializer CreateSerializerInternal(IServer server, Action<JContainer, string, Exception> invalidCollectionElementCallback)
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
	}
}
