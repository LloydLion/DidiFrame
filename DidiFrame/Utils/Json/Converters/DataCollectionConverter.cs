using DidiFrame.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Linq;

namespace DidiFrame.Utils.Json.Converters
{
	internal class DataCollectionConverter : JsonConverter<IDataCollection>
	{
		public DataCollectionConverter()
		{

		}


		public override IDataCollection? ReadJson(JsonReader reader, Type objectType, IDataCollection? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (hasExistingValue)
				throw new InvalidOperationException("Enable to populate this object");

			if (reader.Value is null)
				return null;

			var array = JArray.Load(reader);

			var elementType = objectType.GetInterfaceMap(typeof(IDataCollection<>)).InterfaceType.GetGenericArguments().Single();
			var asDataModel = elementType.IsAssignableTo(typeof(IDataModel));

			var output = (IList)(typeof(List<>).MakeGenericType(elementType).GetConstructor(new[] { typeof(int) })?.Invoke(new object[] { array.Count }) ?? throw new ImpossibleVariantException());

			foreach (var token in array)
			{
				if (asDataModel)
					output.Add(new JsonSerializationModel((JObject)token, serializer).RestoreModel(elementType));
				else output.Add(token.ToObject(elementType));
			}

			return output.RestoreCollection(objectType);
		}

		public override void WriteJson(JsonWriter writer, IDataCollection? value, JsonSerializer serializer)
		{
			if (value is null)
			{
				writer.WriteNull();
				return;
			}


			var asDataModel = value.ElementType.IsAssignableTo(typeof(IDataModel));

			foreach (var item in value)
			{
				if (asDataModel)
				{
					var dataModel = (IDataModel)item;
					var jobj = new JObject();
					var builder = new JsonSerializationModelBuilder(jobj, serializer);

					dataModel.SerializeTo(builder);
					jobj.WriteTo(writer);
				}
				else serializer.Serialize(writer, item);
			}
		}
	}
}
