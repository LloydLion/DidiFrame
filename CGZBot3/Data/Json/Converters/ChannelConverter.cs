using Newtonsoft.Json;

namespace CGZBot3.Data.Json.Converters
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
			//I can't..... await..... NOOOOOOOOOOOOOO
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord channel. It is muttable object");

			if (reader.Value is null) return null;
			else return server.GetChannelAsync(BitConverter.ToUInt64(reader.ReadAsBytes())).Result;
		}

		public override void WriteJson(JsonWriter writer, IChannel? value, JsonSerializer serializer)
		{
			writer.WriteValue(value?.Id);
		}
	}
}
