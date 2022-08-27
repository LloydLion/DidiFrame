using Newtonsoft.Json;

namespace DidiFrame.Utils.Json.Converters.ClientObjects
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

			return currentServer;
		}

		public override void WriteJson(JsonWriter writer, IServer? value, JsonSerializer serializer)
		{
			writer.WriteValue(value?.Id);
		}
	}
}
