using DidiFrame.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DidiFrame.Utils.Json.Converters
{
	internal sealed class JsonSerializationModelBuilder : ISerializationModelBuilder
	{
		private readonly JObject jobj;
		private readonly JsonSerializer serializer;


		public JsonSerializationModelBuilder(JObject jobj, JsonSerializer serializer)
		{
			this.jobj = jobj;
			this.serializer = serializer;
		}


		public void WriteCollection(string propertyName, IDataCollection? values)
		{
			jobj.Add(propertyName, values is null ? null : JToken.FromObject(values, serializer));
		}

		public void WriteModel(string propertyName, IDataModel? model)
		{
			jobj.Add(propertyName, model is null ? null : JToken.FromObject(model, serializer));
		}

		public void WritePrimitive(string propertyName, object? value)
		{
			jobj.Add(propertyName, value is null ? null : JToken.FromObject(value, serializer));
		}
	}
}
