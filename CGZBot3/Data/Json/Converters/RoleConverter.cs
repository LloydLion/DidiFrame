using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Data.Json.Converters
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
			//I can't..... await..... NOOOOOOOOOOOOOO
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord role. It is muttable object");

			if (reader.Value is null) return null;
			else return server.GetRoleAsync(BitConverter.ToUInt64(reader.ReadAsBytes())).Result;
		}

		public override void WriteJson(JsonWriter writer, IRole? value, JsonSerializer serializer)
		{
			writer.WriteValue(value?.Id);
		}
	}
}
