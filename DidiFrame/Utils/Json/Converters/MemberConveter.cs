using Newtonsoft.Json;

namespace DidiFrame.Utils.Json.Converters
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
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord member. It is immuttable object");

			if (reader.Value is null) return null;
			else
			{
				var id = (ulong)(long)reader.Value;
				reader.Read();
				return server.GetMember(id);
			}
		}

		public override void WriteJson(JsonWriter writer, IMember? value, JsonSerializer serializer)
		{
			writer.WriteValue(value?.Id);
		}
	}
}
