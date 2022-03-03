using CGZBot3.Entities.Message;
using DSharpPlus.Entities;
using System.Text;

namespace CGZBot3.DSharpAdapter
{
	internal class MessageConverter
	{
		public MessageSendModel ConvertDown(DiscordMessage message)
		{
			return new MessageSendModel(message.Content);
		}

		public async Task<DiscordMessageBuilder> ConvertUpAsync(MessageSendModel messageSendModel)
		{
			var builder = new DiscordMessageBuilder();


			if (messageSendModel.Files is not null)
				foreach (var file in messageSendModel.Files)
				{
					var stream = new MemoryStream(Encoding.Default.GetBytes(await file.Reader.ReadToEndAsync()));
					builder.WithFile(file.FileName, stream);
				}

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

				foreach (var field in baseEmbed.Fields) embed.AddField(field.Name, field.Value);

				builder.AddEmbed(embed);
			}

			return builder;
		}
	}
}
