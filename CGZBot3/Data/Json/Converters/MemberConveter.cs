using Newtonsoft.Json;

namespace CGZBot3.Data.Json.Converters
{
	internal class MemberConveter : JsonConverter<IMember>
	{
		private readonly IServer server;


		public MemberConveter(IServer server)
		{
			this.server = server;
		}


		public override IMember? ReadJson(JsonReader reader, Type objectType, IMember? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			//I can't..... await..... NOOOOOOOOOOOOOO
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord member. It is muttable object");

			if (reader.Value is null) return null;
			else return server.GetMemberAsync(BitConverter.ToUInt64(reader.ReadAsBytes())).Result;
		}

		public override void WriteJson(JsonWriter writer, IMember? value, JsonSerializer serializer)
		{
			writer.WriteValue(value?.Id);
		}
	}
}
