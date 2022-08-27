using DidiFrame.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Utils.Json.Converters
{
	internal class DataModelConverter : JsonConverter<IDataModel>
	{
		public override IDataModel? ReadJson(JsonReader reader, Type objectType, IDataModel? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (hasExistingValue)
				throw new InvalidOperationException("Enable to populate this object");

			if (reader.Value is null)
				return null;

			var jobj = JObject.Load(reader);
			var model = new JsonSerializationModel(jobj, serializer);
			return model.RestoreModel(objectType);
		}

		public override void WriteJson(JsonWriter writer, IDataModel? value, JsonSerializer serializer)
		{
			if (value is null)
			{
				writer.WriteNull();
				return;
			}

			var jobj = new JObject();
			var builder = new JsonSerializationModelBuilder(jobj, serializer);
			value.SerializeTo(builder);
			jobj.WriteTo(writer);
		}
	}
}
