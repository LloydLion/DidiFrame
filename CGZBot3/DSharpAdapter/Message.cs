using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.DSharpAdapter
{
	internal class Message : IMessage
	{
		private readonly DiscordMessage message;
		private readonly DiscordClient client;


		public MessageSendModel SendModel { get; }

		public string Id => message.Id.ToString();

		public ITextChannel TextChannel => new TextChannel(message.Channel, client);


		public Message(DiscordMessage message, DiscordClient client, MessageSendModel sendModel)
		{
			this.message = message;
			this.client = client;
			SendModel = sendModel;
		}


		public bool Equals(IMessage? other) => other is Message msg && msg.Id == Id;
	}
}
