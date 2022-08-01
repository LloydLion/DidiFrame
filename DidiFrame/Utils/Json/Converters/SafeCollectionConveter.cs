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
			return ContainsOrIsGeneric(objectType, typeof(ICollection<>));
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var isDic = ContainsOrIsGeneric(objectType, typeof(IDictionary<,>));

			object returnObj;

			var generic = ExtractConstructedGeneric(objectType, typeof(ICollection<>));
			var collectionType = generic.GetGenericArguments()[0];

			if (objectType.IsInterface)
			{
				if (isDic)
					//collectionType is KeyValuePair
					returnObj = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(collectionType.GetGenericArguments())) ?? throw new ImpossibleVariantException();
				else returnObj = Activator.CreateInstance(typeof(List<>).MakeGenericType(collectionType)) ?? throw new ImpossibleVariantException();
			}
			else returnObj = Activator.CreateInstance(objectType) ?? throw new ImpossibleVariantException();

			var addMethod = typeof(ICollection<>).MakeGenericType(collectionType).GetMethod("Add") ?? throw new NullReferenceException();

			var jarr = JArray.Load(reader);

			foreach (var el in jarr)
				try
				{
					var obj = el.ToObject(collectionType, serializer);
					addMethod.Invoke(returnObj, new[] { obj ?? throw new NullReferenceException() });
				}
				catch (Exception ex)
				{
					invalidElementCallback(el.Parent ?? throw new ImpossibleVariantException(), el.Path, ex);
				}

			return returnObj;
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is null)
				writer.WriteNull();
			else
			{
				var array = new JArray();

				foreach (var item in (IEnumerable)value)
					array.Add(JToken.FromObject(item, serializer));

				array.WriteTo(writer);
			}
		}

		private static bool ContainsOrIsGeneric(Type originType, Type targetType)
		{
			var isType = originType.IsGenericType && originType.GetGenericTypeDefinition() == targetType;
			var contains = originType.GetInterfaces().Any(s => s.IsGenericType && s.GetGenericTypeDefinition() == targetType);
			return isType || contains;
		}

		private static Type ExtractConstructedGeneric(Type originType, Type targetType)
		{
			var isType = originType.IsGenericType && originType.GetGenericTypeDefinition() == targetType;
			if (isType) return originType;

			var contains = originType.GetInterfaces().First(s => s.IsGenericType && s.GetGenericTypeDefinition() == targetType);
			return contains;
		}
	}
}
