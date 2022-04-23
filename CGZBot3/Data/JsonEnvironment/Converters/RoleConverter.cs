using Newtonsoft.Json;

namespace CGZBot3.Data.JsonEnvironment.Converters
{
	internal class RoleConverter : JsonConverter<IRole>
	{
		private readonly IServer server;


		public RoleConverter(IServer server)
		{
			this.server = server;
		}


		public override IRole? ReadJson(JsonReader reader, Type objectType, IRole? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord role. It is immuttable object");

			if (reader.Value is null) return null;
			else
			{
				var id = (ulong)(long)reader.Value;
				reader.Read();
				return server.GetRole(id);
			}
		}

		public override void WriteJson(JsonWriter writer, IRole? value, JsonSerializer serializer)
		{
			writer.WriteValue(value?.Id);
		}
	}
}
