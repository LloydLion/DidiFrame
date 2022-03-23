﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CGZBot3.Data.Json.Converters
{
	internal class MessageConverter : JsonConverter<IMessage>
	{
		private readonly IServer server;


		public MessageConverter(IServer server)
		{
			this.server = server;
		}


		public override IMessage? ReadJson(JsonReader reader, Type objectType, IMessage? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			//I can't..... await..... NOOOOOOOOOOOOOO
			if (hasExistingValue) throw new InvalidOperationException("Enable to populate discord message. It is immuttable object");

			var msg = serializer.Deserialize<Message?>(reader);
			if (msg is null) return null;
			else
			{
				var channel = server.GetChannelAsync(msg.Value.ChannelId).Result.AsText();
				var messages = channel.GetMessagesAsync().Result;
				return messages.First(s => s.Id == msg.Value.Id);
			}
		}

		public override void WriteJson(JsonWriter writer, IMessage? value, JsonSerializer serializer)
		{
			if (value is not null)
				serializer.Serialize(writer, JObject.FromObject(new Message { Id = value.Id, ChannelId = value.TextChannel.Id }));
			else writer.WriteNull();
		}


		private struct Message
		{
			public ulong Id { get; set; }

			public ulong ChannelId { get; set; }
		}
	}
}
