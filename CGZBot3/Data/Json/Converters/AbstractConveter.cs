using CGZBot3.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;

namespace CGZBot3.Data.Json.Converters
{
	internal class AbstractConveter : JsonConverter
	{
		private static readonly Type[] unsupportedTypes = new[] { typeof(IChannel), typeof(IMember), typeof(IServer), typeof(IChannelCategory), typeof(IRole) };


		public override bool CanConvert(Type objectType)
		{
			return !(objectType.Assembly != Assembly.GetExecutingAssembly() || typeof(IEnumerable).IsAssignableFrom(objectType) || unsupportedTypes.Any(s => s.IsAssignableFrom(objectType)));
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var jobj = JToken.ReadFrom(reader);

			var (settable, ctor) = GetProperties(objectType);


			if (existingValue is null)
			{
				var octor = ctor.OrderBy(s => (s.GetCustomAttribute<ConstructorAssignablePropertyAttribute>() ?? throw new ImpossibleVariantException()).ParameterPosition).ToArray();
				var ctorArgs = new object?[octor.Length];

				for (int i = 0; i < octor.Length; i++)
				{
					var maybe = jobj[octor[i].Name];
					if (maybe is null) continue;
					ctorArgs[i] = maybe.ToObject(octor[i].PropertyType, serializer);
				}

				existingValue = Activator.CreateInstance(objectType, ctorArgs);
			}

			foreach (var item in settable)
			{
				var maybe = jobj[item.Name];
				if (maybe is null) continue;
				item.SetValue(existingValue, maybe.ToObject(item.PropertyType));
			}

			return existingValue;
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is null)
			{
				writer.WriteNull();
			}
			else
			{
				var type = value.GetType();
				var (settable, ctor) = GetProperties(type);

				var jobj = new JObject();

				foreach (var item in settable.Concat(ctor))
				{
					var val = item.GetValue(value);
					jobj.Add(item.Name, val is null ? null : JToken.FromObject(val, serializer));
				}

				serializer.Serialize(writer, jobj);
			}
		}

		private (PropertyInfo[] settable, PropertyInfo[] ctor) GetProperties(Type type)
		{
			var settable = type.GetProperties().Where(s => s.CanRead && s.CanWrite && s.GetCustomAttribute<ConstructorAssignablePropertyAttribute>() == null).ToArray();
			var ctor = type.GetProperties().Where(s => s.CanRead && s.GetCustomAttribute<ConstructorAssignablePropertyAttribute>() != null).ToArray();

			return (settable, ctor);
		}
	}
}
