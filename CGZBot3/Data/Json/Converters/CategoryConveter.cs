using Newtonsoft.Json;

namespace CGZBot3.Data.Json.Converters
{
	internal class CategoryConveter : JsonConverter<IChannelCategory>
	{
		private readonly IServer server;


		public CategoryConveter(IServer server)
		{
			this.server = server;
		}


		public override IChannelCategory? ReadJson(JsonReader reader, Type objectType, IChannelCategory? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord category. It is immuttable object");

			if (reader.Value is null) return null;
			else
			{
				var id = (ulong)(long)reader.Value;
				reader.Read();
				return server.GetCategory(id);
			}
		}

		public override void WriteJson(JsonWriter writer, IChannelCategory? value, JsonSerializer serializer)
		{
			writer.WriteValue(value?.Id);
		}
	}
}
