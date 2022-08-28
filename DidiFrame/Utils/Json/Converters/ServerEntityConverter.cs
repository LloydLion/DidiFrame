using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DidiFrame.Utils.Json.Converters
{
	internal class ServerEntityConverter<TEntity> : JsonConverter<TEntity> where TEntity : class, IServerEntity
	{
		private readonly IServer server;
		private readonly Func<TEntity, ulong?> idGetter;
		private readonly Func<ulong?, IServer, TEntity> byIdRestorer;


		public ServerEntityConverter(IServer server, Func<TEntity, ulong?> idGetter, Func<ulong?, IServer, TEntity> byIdRestorer)
		{
			this.server = server;
			this.idGetter = idGetter;
			this.byIdRestorer = byIdRestorer;
		}

		public ServerEntityConverter(IServer server, Func<TEntity, ulong> idGetter, Func<ulong, IServer, TEntity> byIdRestorer)
		{
			this.server = server;
			this.idGetter = s => idGetter(s);
			this.byIdRestorer = (id, server) => byIdRestorer(id ?? throw new NullReferenceException(), server);
		}


		public override TEntity? ReadJson(JsonReader reader, Type objectType, TEntity? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord entiy. It is immuttable object");

			if (reader.Value is null)
				return null;
			else if (reader.TokenType == JsonToken.StartObject)
			{
				var model = JObject.Load(reader).ToObject<HelpModel>();
				return byIdRestorer((ulong?)model.Id, server.Client.GetServer(model.Server));
			}
			else
			{
				var id = (ulong)(long)reader.Value;
				reader.Read();
				return byIdRestorer(id, server);
			}
		}

		public override void WriteJson(JsonWriter writer, TEntity? value, JsonSerializer serializer)
		{
			if (value is null)
			{
				writer.WriteNull();
			}
			else if (Equals(value.Server, server))
			{
				writer.WriteValue((long?)idGetter(value));
			}
			else
			{
				var model = new HelpModel((long?)idGetter(value), value.Server.Id);
				JObject.FromObject(model).WriteTo(writer);
			}
		}


		private record struct HelpModel(long? Id, ulong Server);
	}
}
