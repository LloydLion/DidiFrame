using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace DidiFrame.Utils.Json.Converters
{
	internal class SafeCollectionConveter : JsonConverter
	{
		private readonly Action<JContainer, string, Exception> invalidElementCallback;


		public SafeCollectionConveter(Action<JContainer, string, Exception> invalidElementCallback)
		{
			this.invalidElementCallback = invalidElementCallback;
		}


		public override bool CanConvert(Type objectType)
		{
			var isDic = objectType.IsAssignableTo(typeof(IDictionary)) || objectType.GetInterfaces().Any(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
				|| (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));
			return objectType.IsInterface && objectType.GetInterfaces().Any(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(IEnumerable<>)) && !isDic;
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var generic = objectType.GetInterfaces().First(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			var collectionType = generic.GetGenericArguments()[0];
			var list = (IList)(Activator.CreateInstance(typeof(List<>).MakeGenericType(collectionType)) ?? throw new ImpossibleVariantException());

			var jarr = JArray.Load(reader);

			foreach (var el in jarr)
				try
				{
					var obj = el.ToObject(collectionType, serializer);
					list.Add(obj);
				}
				catch (Exception ex)
				{
					invalidElementCallback(el.Parent ?? throw new ImpossibleVariantException(), el.Path, ex);
				}

			return list;
		}

		public override bool CanWrite => false;

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
