using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DidiFrame.Utils.Json.Converters
{
	internal class MessageConverter : JsonConverter<IMessage>
	{
		public override IMessage? ReadJson(JsonReader reader, Type objectType, IMessage? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord message. It is immuttable object");

			var msg = JObject.Load(reader).ToObject<Message?>();
			if (msg is null) return null;
			else return msg.Value.Channel.GetMessageAsync((ulong)msg.Value.Id);
		}

		public override void WriteJson(JsonWriter writer, IMessage? value, JsonSerializer serializer)
		{
			if (value is not null)
				JObject.FromObject(new Message { Id = (long)value.Id, Channel = value.TextChannel }).WriteTo(writer);
			else writer.WriteNull();
		}


		private struct Message
		{
			public long Id { get; set; }

			public ITextChannelBase Channel { get; set; }
		}
	}
}
