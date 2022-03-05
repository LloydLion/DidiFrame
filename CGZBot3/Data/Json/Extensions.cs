using Newtonsoft.Json;
using System.Text;

namespace CGZBot3.Data.Json
{
	internal static class Extensions
	{
		public static T Deserialize<T>(this JsonSerializer serializer, string content)
		{
			using var memstr = new MemoryStream();
			using var reader = new StreamReader(memstr);
			using var json = new JsonTextReader(reader);

			return serializer.Deserialize<T>(json) ?? throw new ImpossibleVariantException();
		}

		public static string Serialize<T>(this JsonSerializer serializer, T obj)
		{
			using var memstr = new MemoryStream();
			using var writer = new StreamWriter(memstr);
			using var json = new JsonTextWriter(writer);

			serializer.Serialize(json, obj);

			return Encoding.Default.GetString(memstr.ToArray());
		}
	}
}
