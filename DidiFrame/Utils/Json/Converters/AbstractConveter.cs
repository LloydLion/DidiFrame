using DidiFrame.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;

namespace DidiFrame.Utils.Json.Converters
{
	internal class AbstractConveter : JsonConverter<IDataModel>
	{
		public override void WriteJson(JsonWriter writer, IDataModel? value, JsonSerializer serializer)
		{
			if (value is null)
			{
				writer.WriteNull();
			}
			else
			{
				var type = value.GetType();
				var properties = IDataModel.EnumerateProperties(type);

				var jobj = new JObject();

				foreach (var item in properties.Select(s => s.Property))
				{
					var val = item.GetValue(value);
					jobj.Add(item.Name, val is null ? null : JToken.FromObject(val, serializer));
				}

				jobj.WriteTo(writer);
			}
		}

		public override IDataModel? ReadJson(JsonReader reader, Type objectType, IDataModel? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var jobj = JToken.ReadFrom(reader);

			var properties = IDataModel.EnumerateProperties(objectType);


			if (existingValue is null)
			{
				var ctorInfo = objectType.GetConstructors().SingleOrDefault(s => s.GetCustomAttribute<SerializationConstructorAttribute>() is not null);
				if (ctorInfo is null) ctorInfo = objectType.GetConstructors().Single();

				var octor = properties.Where(s => s.AssignationTarget.TargetType == PropertyAssignationTarget.Type.Constructor).ToArray();
				Array.Sort(octor, (a, b) => a.AssignationTarget.GetConstructorParameterPosition() - b.AssignationTarget.GetConstructorParameterPosition());

				var ctorArgs = new object?[octor.Length];

				for (int i = 0; i < octor.Length; i++)
				{
					var maybe = jobj[octor[i].Property.Name];
					if (maybe is null) continue;
					ctorArgs[i] = maybe.ToObject(octor[i].Property.PropertyType, serializer);
				}

				existingValue = (IDataModel)ctorInfo.Invoke(ctorArgs);
			}

			foreach (var item in properties.Where(s => s.AssignationTarget.TargetType == PropertyAssignationTarget.Type.SetAccessor).Select(s => s.Property))
			{
				var maybe = jobj[item.Name];
				if (maybe is null) continue;
				item.SetValue(existingValue, maybe.ToObject(item.PropertyType, serializer));
			}

			return existingValue;
		}
	}
}
