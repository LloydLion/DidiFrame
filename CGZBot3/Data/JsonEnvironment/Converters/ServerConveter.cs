using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Data.JsonEnvironment.Converters
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
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord server. It is immuttable object");

			if (reader.Value is null) return null;
			else
			{
				var id = (ulong)(long)reader.Value;
				reader.Read();
				return client.Servers.Single(s => s.Id == id);
			}
		}

		public override void WriteJson(JsonWriter writer, IServer? value, JsonSerializer serializer)
		{
			writer.WriteValue(value?.Id);
		}
	}
}
