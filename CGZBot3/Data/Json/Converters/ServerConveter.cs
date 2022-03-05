using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Data.Json.Converters
{
	internal class ServerConveter : JsonConverter<IServer>
	{
		private readonly IClient client;


		public ServerConveter(IClient client)
		{
			this.client = client;
		}


		public override IServer? ReadJson(JsonReader reader, Type objectType, IServer? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord server. It is muttable object");

			if (reader.Value is null) return null;
			else return client.Servers.Single(s => s.Id == BitConverter.ToUInt64(reader.ReadAsBytes()));
		}

		public override void WriteJson(JsonWriter writer, IServer? value, JsonSerializer serializer)
		{
			writer.WriteValue(value?.Id);
		}
	}
}
