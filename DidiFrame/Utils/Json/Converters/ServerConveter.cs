using Newtonsoft.Json;

namespace DidiFrame.Utils.Json.Converters
{
	internal class ServerConveter : JsonConverter<IServer>
	{
		private readonly IServer currentServer;


		public ServerConveter(IServer currentServer)
		{
			this.currentServer = currentServer;
		}


		public override IServer? ReadJson(JsonReader reader, Type objectType, IServer? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord server. It is immuttable object");

			if (reader.Value is null) return null;
			else
			{
				var id = (ulong)(long)reader.Value;
				return id == 0 ? currentServer : currentServer.Client.GetServer(id);
			}
		}

		public override void WriteJson(JsonWriter writer, IServer? value, JsonSerializer serializer)
		{
			if (Equals(currentServer, value)) writer.WriteValue(0);
			else writer.WriteValue((long?)(value?.Id));
		}
	}
}
