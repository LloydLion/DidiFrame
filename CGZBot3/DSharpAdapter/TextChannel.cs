using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.DSharpAdapter
{
	internal class TextChannel : Channel, ITextChannel
	{
		private readonly DiscordChannel channel;


		public TextChannel(DiscordChannel channel, Server server) : base(channel, server)
		{
			if(!new[] { ChannelType.Text, ChannelType.Group, ChannelType.News, ChannelType.Private }.Contains(channel.Type))
			{
				throw new InvalidOperationException("Channel must be text");
			}
			this.channel = channel;
		}


		public async Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			return new Message(owner: this, sendModel: messageSendModel, message: await channel.SendMessageAsync(builder =>
			{
				var embed = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Gold,
					Title = messageSendModel.Content
				};

				builder.AddEmbed(embed);
			}));
		}
	}
}
