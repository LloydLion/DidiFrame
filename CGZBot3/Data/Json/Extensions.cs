using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CGZBot3.Data.Json
{
	internal static class Extensions
	{
		public static string Serialize<T>(this JsonSerializer serializer, T? obj)
		{
			var strWriter = new StringWriter();
			using var json = new JsonTextWriter(strWriter);

			serializer.Serialize(json, obj);

			return strWriter.GetStringBuilder().ToString();
		}

		public static T Deserialize<T>(this JsonSerializer serializer, string str)
		{
			if (string.IsNullOrWhiteSpace(str)) str = "{}";
			var stringReader = new StringReader(str);
			using var json = new JsonTextReader(stringReader);

			return serializer.Deserialize<T>(json) ?? throw new ImpossibleVariantException();
		}
	}
}
