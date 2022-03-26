using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Components;
using CGZBot3.Entities.Message.Embed;
using DSharpPlus.Entities;
using System.Text;

namespace CGZBot3.DSharpAdapter
{
	internal class MessageConverter
	{
		public MessageSendModel ConvertDown(DiscordMessage message)
		{
			var content = message.Content;

			List<MessageFile>? files = null;

			if(message.Attachments.Count != 0)
			{
				files = new List<MessageFile>();

				var web = new HttpClient();
				var tasks = new List<Task>();
				foreach (var file in message.Attachments)
				{
					async Task get() { files.Add(new MessageFile(file.FileName, new StreamReader(await web.GetStreamAsync(file.Url)))); }
					tasks.Add(get());
				}

				Task.WaitAll(tasks.ToArray());
			}

			MessageEmbed? embed = null;

			if (message.Embeds.Count != 0)
			{
				var baseEmbed = message.Embeds[0];

				embed = new MessageEmbed(baseEmbed.Title, baseEmbed.Description, new Color(baseEmbed.Color.Value.R, baseEmbed.Color.Value.G, baseEmbed.Color.Value.B),
					baseEmbed.Fields.Select(s => new EmbedField(s.Name, s.Value)).ToArray(), new EmbedMeta()
					{
						AuthorIconUrl = baseEmbed.Author.IconUrl.ToString(),
						AuthorPersonalUrl = baseEmbed.Author.Url.ToString(),
						AuthorName = baseEmbed.Author.Name,
						DisplayImageUrl = baseEmbed.Image.Url.ToString(),
						Timestamp =  baseEmbed.Timestamp?.DateTime
					});
			}

			return new MessageSendModel(content) { Files = files, MessageEmbed = embed };
		}

		public DiscordMessageBuilder ConvertUp(MessageSendModel messageSendModel)
		{
			var builder = new DiscordMessageBuilder();


			if (messageSendModel.Files is not null)
				foreach (var file in messageSendModel.Files)
				{
					var stream = new MemoryStream(Encoding.Default.GetBytes(file.Reader.ReadToEnd()));
					builder.WithFile(file.FileName, stream);
				}

			//If no embed and text is small print it in embed else print it in content
			if (messageSendModel.MessageEmbed is null && messageSendModel.Content is not null && messageSendModel.Content.Length < 200)
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
				builder.WithContent(messageSendModel.Content); //If null all ok

				var baseEmbed = messageSendModel.MessageEmbed;

				if (baseEmbed is not null)
				{
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
			}

			if (messageSendModel.Components is not null)
			{
				foreach (var component in messageSendModel.Components)
				{
					DiscordComponent discord = component switch
					{
						MessageButton bnt => new DiscordButtonComponent(bnt.Style.GetDSharp(), bnt.Id, bnt.Text, bnt.Disabled),
						MessageLinkButton bnt => new DiscordLinkButtonComponent(bnt.Url, bnt.Text, bnt.Disabled),
						MessageSelectMenu menu => new DiscordSelectComponent(menu.Id, menu.PlaceHolder,
							menu.Options.Select(s => new DiscordSelectComponentOption(s.Labеl, s.Value, s.Description)), menu.Disabled, menu.MinOptions, menu.MaxOptions),
						_ => throw new NotSupportedException($"Component with a type {component.GetType()} is not supported")
					};

					builder.AddComponents(discord);
				}
			}

			return builder;
		}
	}
}
