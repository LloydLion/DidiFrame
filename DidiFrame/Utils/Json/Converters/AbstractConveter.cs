using DidiFrame.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;

namespace DidiFrame.Utils.Json.Converters
{
	internal class AbstractConveter : JsonConverter
	{
		private static readonly Type[] unsupportedTypes = new[] { typeof(IChannel), typeof(IMember), typeof(IServer), typeof(IChannelCategory), typeof(IRole), typeof(IMessage), typeof(MessageSendModel) };


		public override bool CanConvert(Type objectType)
		{
			var result = !((objectType.Assembly != (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()) &&
				objectType.Assembly != Assembly.GetExecutingAssembly()) ||
				objectType.IsValueType ||
				typeof(IEnumerable).IsAssignableFrom(objectType) ||
				unsupportedTypes.Any(s => s.IsAssignableFrom(objectType)));
			return result;
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var jobj = JToken.ReadFrom(reader);

			var (settable, ctor) = GetProperties(objectType, out var ctorInfo);


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

				existingValue = ctorInfo.Invoke(ctorArgs);
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
				var (settable, ctor) = GetProperties(type, out _);

				var jobj = new JObject();

				foreach (var item in settable.Concat(ctor))
				{
					var val = item.GetValue(value);
					jobj.Add(item.Name, val is null ? null : JToken.FromObject(val, serializer));
				}

				jobj.WriteTo(writer);
			}
		}

		private static (PropertyInfo[] settable, PropertyInfo[] ctor) GetProperties(Type type, out ConstructorInfo ctorInfo)
		{
			var settable = type.GetProperties().Where(s => s.CanRead && s.CanWrite && s.GetCustomAttribute<ConstructorAssignablePropertyAttribute>() == null).ToArray();
			var ctor = type.GetProperties().Where(s => s.CanRead && s.GetCustomAttribute<ConstructorAssignablePropertyAttribute>() != null).ToArray();
			ctorInfo = type.GetConstructor(ctor.Select(s => s.PropertyType).ToArray()) ?? throw new ArgumentNullException("No ctor was found");

			return (settable, ctor);
		}
	}
}
