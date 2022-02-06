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
		private readonly TextChannel owner;


		public MessageSendModel SendModel { get; }

		public ulong Id => message.Id;

		public ITextChannel TextChannel => owner;


		public Message(DiscordMessage message, TextChannel owner, MessageSendModel sendModel)
		{
			this.message = message;
			this.owner = owner;
			SendModel = sendModel;
		}


		public bool Equals(IMessage? other) => other is Message msg && msg.Id == Id;
	}
}
