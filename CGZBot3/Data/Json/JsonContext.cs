using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace CGZBot3.Data.Json
{
	internal class JsonContext
	{
		private readonly string directoryPath;


		public JsonContext(string directoryPath)
		{
			this.directoryPath = directoryPath;
		}


		public TModel Load<TModel>(IServer server, string key, IModelFactory<TModel> factory) where TModel : class
		{
			var path = GetFileFromServer(server);

			if (!File.Exists(path)) return PutDefault(server, key, factory);
			else
			{
				var fs = File.Open(path, FileMode.Open, FileAccess.Read);
				var content = new StreamReader(fs).ReadToEnd();
				fs.Close();
				fs.Dispose();

				var jobj = (JObject)(JsonConvert.DeserializeObject(content) ?? throw new ImpossibleVariantException());

				var model = jobj.GetValue(key)?.ToObject<TModel>();

				if (model is null) return PutDefault(server, key, factory);
				else return model;
			}
		}
		
		public TModel Load<TModel>(IServer server, string key) where TModel : class
		{
			var path = GetFileFromServer(server);

			var fs = File.Open(path, FileMode.Open, FileAccess.Read);
			var content = new StreamReader(fs).ReadToEnd();
			fs.Close();
			fs.Dispose();

			var jobj = (JObject)(JsonConvert.DeserializeObject(content) ?? throw new ImpossibleVariantException());

			var model = jobj.GetValue(key)?.ToObject<TModel>() ?? throw new ArgumentException("Key dom't present in json or section contains invalid data", nameof(key));

			return model;
		}

		public void Put<TModel>(IServer server, string key, TModel model) where TModel : class
		{
			if (model is null) throw new ArgumentNullException(nameof(model));

			var file = GetFileFromServer(server);

			if (!File.Exists(file))
			{
				var nfs = File.Create(file);
				var writer = new StreamWriter(nfs);
				writer.Write("{}");
				writer.Flush();
				nfs.Close();
			}

			JObject jobj;

			{
				var fs = File.Open(file, FileMode.Open, FileAccess.Read);
				var content = new StreamReader(fs).ReadToEnd();
				fs.Close();
				fs.Dispose();

				jobj = (JObject)(JsonConvert.DeserializeObject(content) ?? throw new ImpossibleVariantException());
			}

			if(jobj.ContainsKey(key)) jobj.Remove(key);
			jobj.Add(key, model is IEnumerable en ? JArray.FromObject(en.Cast<object>().ToArray()) : JObject.FromObject(model));

			{
				var fs = File.Open(file, FileMode.Create, FileAccess.Write);
				var writer = new StreamWriter(fs);

				writer.Write(JsonConvert.SerializeObject(jobj));
				writer.Flush();
				fs.Close();
				fs.Dispose();
			}
		}

		public TModel PutDefault<TModel>(IServer server, string key, IModelFactory<TModel> factory) where TModel : class
		{
			var model = factory.CreateDefault();
			Put(server, key, model);
			return model;
		}

		public void Delete(IServer server, string key)
		{
			var file = GetFileFromServer(server);

			if (!File.Exists(file)) return;

			JObject jobj;

			{
				using var fs = File.Open(file, FileMode.Open, FileAccess.Read);
				var content = new StreamReader(fs).ReadToEnd();

				jobj = (JObject)(JsonConvert.DeserializeObject(content) ?? throw new ImpossibleVariantException());
			}

			jobj.Remove(key);

			{
				var fs = File.Open(file, FileMode.Create, FileAccess.Write);
				var writer = new StreamWriter(fs);
				fs.Close();
				fs.Dispose();

				writer.Write(JsonConvert.SerializeObject(jobj));
				writer.Flush();
			}
		}

		public void DeleteAll(IServer server)
		{
			var file = GetFileFromServer(server);

			if (!File.Exists(file)) return;

			File.Delete(file);
		}

		private string GetFileFromServer(IServer server) => $"{directoryPath}/{server.Id}.json";
	}
}
