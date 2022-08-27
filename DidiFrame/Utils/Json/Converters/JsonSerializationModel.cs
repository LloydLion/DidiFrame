using DidiFrame.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DidiFrame.Utils.Json.Converters
{
	internal sealed class JsonSerializationModel : ISerializationModel
	{
		private readonly JObject jobj;
		private readonly JsonSerializer serializer;


		public JsonSerializationModel(JObject jobj, JsonSerializer serializer)
		{
			this.jobj = jobj;
			this.serializer = serializer;
		}


		public IDataCollection? ReadCollection(Type typeToRead, string propertyName)
		{
			return (IDataCollection?)(jobj.Property(propertyName)?.ToObject(typeToRead, serializer));
		}

		public IDataModel? ReadModel(Type typeToRead, string propertyName)
		{
			return (IDataModel?)(jobj.Property(propertyName)?.ToObject(typeToRead, serializer));
		}

		public object? ReadPrimitive(Type typeToRead, string propertyName)
		{
			return jobj.Property(propertyName)?.ToObject(typeToRead, serializer);
		}
	}
}
