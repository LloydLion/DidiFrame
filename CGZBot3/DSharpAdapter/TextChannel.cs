using DSharpPlus;
using DSharpPlus.Entities;
using ChannelType = DSharpPlus.ChannelType;

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
				if (messageSendModel.MessageEmbed is null)
				{
					var embed = new DiscordEmbedBuilder
					{
						Color = DiscordColor.Gold,
						Title = messageSendModel.Content
					};

					builder.AddEmbed(embed);
				}
				else
				{
					builder.WithContent(messageSendModel.Content);

					var baseEmbed = messageSendModel.MessageEmbed;

					var embed = new DiscordEmbedBuilder
					{
						Title = baseEmbed.Title,
						Description = baseEmbed.Description,
						Color = baseEmbed.Color.GetDSharp(),
						Timestamp = baseEmbed.Metadata.Timestamp,
						Author = new DiscordEmbedBuilder.EmbedAuthor()
							{ IconUrl = baseEmbed.Metadata.AuthorIconUrl, Name = baseEmbed.Metadata.AuthorName, Url = baseEmbed.Metadata.AuthorPersonalUrl },
						ImageUrl = baseEmbed.Metadata.DisplayImageUrl
					};

					builder.AddEmbed(embed);
				}
			}));
		}
	}
}
