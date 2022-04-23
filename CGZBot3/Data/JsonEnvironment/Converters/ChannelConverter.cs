using Newtonsoft.Json;

namespace CGZBot3.Data.JsonEnvironment.Converters
{
	internal class ChannelConverter : JsonConverter<IChannel>
	{
		private readonly IServer server;


		public ChannelConverter(IServer server)
		{
			this.server = server;
		}


		public override IChannel? ReadJson(JsonReader reader, Type objectType, IChannel? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord channel. It is immuttable object");

			if (reader.Value is null) return null;
			else
			{
				var id = (ulong)(long)reader.Value;
				reader.Read();
				return server.GetChannel(id);
			}
		}

		public override void WriteJson(JsonWriter writer, IChannel? value, JsonSerializer serializer)
		{
			writer.WriteValue(value?.Id);
		}
	}
}
