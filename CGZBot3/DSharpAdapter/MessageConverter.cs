using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Components;
using CGZBot3.Entities.Message.Embed;
using DSharpPlus.Entities;
using System.Text;

namespace CGZBot3.DSharpAdapter
{
	internal class MessageConverter
	{
		public async Task<MessageSendModel> ConvertDownAsync(DiscordMessage message, Server server)
		{
			var content = message.Content;

			List<MessageFile>? files = null;

			if(message.Attachments.Count != 0)
			{
				files = new List<MessageFile>();

				var web = new HttpClient();
				foreach (var file in message.Attachments)
				{
					var request = await web.GetAsync(file.Url);
					var stream = request.Content.ReadAsStream();
					files.Add(new MessageFile(file.FileName, new StreamReader(stream)));
				}
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

		public async Task<Message> SendAsync(MessageSendModel messageSendModel, TextChannel channel)
		{
			var builder = new DiscordMessageBuilder();


			if (messageSendModel.Files is not null)
				foreach (var file in messageSendModel.Files)
				{
					var stream = new MemoryStream(Encoding.Default.GetBytes(await file.Reader.ReadToEndAsync()));
					builder.WithFile(file.FileName, stream);
				}

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
					var discord = channel.BaseServer.ComponentsRegistry.AddComponent(component, (id) => component switch
					{
						MessageButton bnt => new DiscordButtonComponent(bnt.Style.GetDSharp(), id, bnt.Text, bnt.Disabled),
						MessageLinkButton bnt => new DiscordLinkButtonComponent(bnt.Url, bnt.Text, bnt.Disabled),
						MessageSelectMenu menu => new DiscordSelectComponent(id, menu.PlaceHolder,
							menu.Options.Select(s => new DiscordSelectComponentOption(s.Labеl, channel.BaseServer.ComponentsRegistry.AddMenuOptionValue(s.Value), s.Description)), menu.Disabled, menu.MinOptions, menu.MaxOptions),
						_ => throw new NotSupportedException($"Component with a type {component.GetType()} is not supported")
					});

					builder.AddComponents(discord);
				}
			}

			return new Message(await channel.BaseChannel.SendMessageAsync(builder), channel, messageSendModel);
		}
	}
}
