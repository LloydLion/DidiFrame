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


			List<MessageEmbed>? embeds = null;

			if (message.Embeds.Any())
			{
				embeds = new();
				foreach (var baseEmbed in message.Embeds)
					embeds.Add(new MessageEmbed(baseEmbed.Title, baseEmbed.Description, new Color(baseEmbed.Color.Value.R, baseEmbed.Color.Value.G, baseEmbed.Color.Value.B),
						baseEmbed.Fields?.Select(s => new EmbedField(s.Name, s.Value))?.ToArray() ?? Array.Empty<EmbedField>(), new EmbedMeta()
						{
							AuthorIconUrl = baseEmbed.Author?.IconUrl?.ToString(),
							AuthorPersonalUrl = baseEmbed.Author?.Url?.ToString(),
							AuthorName = baseEmbed.Author?.Name,
							DisplayImageUrl = baseEmbed.Image?.Url?.ToString(),
							Timestamp =  baseEmbed.Timestamp?.DateTime
						}));
			}


			List<MessageComponentsRow>? components = null;

			if (message.Components.Any())
			{
				components = new();
				foreach (var actionRow in message.Components)
				{
					var temp = new List<IComponent>();
					foreach (var component in actionRow.Components)
					{
						IComponent? value = component switch
						{
							DiscordButtonComponent bnt => new MessageButton(bnt.CustomId, bnt.Label, bnt.Style.GetAbstract(), bnt.Disabled),
							DiscordLinkButtonComponent link => new MessageLinkButton(link.Label, link.Url, link.Disabled),
							DiscordSelectComponent menu => new MessageSelectMenu(menu.CustomId, menu.Options.Select(s => new MessageSelectMenuOption(s.Label, s.Value, s.Description)).ToArray(),
								menu.Placeholder, menu.MinimumSelectedValues ?? 1, menu.MaximumSelectedValues ?? 1, menu.Disabled),
							_ => null
						};

						if (value is null) continue;
						else temp.Add(value);
					}

					if (temp.Any() == false) continue;
					components.Add(new MessageComponentsRow(temp));
				}
			}

			return new MessageSendModel(content) { Files = files, MessageEmbeds = embeds, ComponentsRows = components };
		}

		public DiscordMessageBuilder ConvertUp(MessageSendModel messageSendModel)
		{
			var builder = new DiscordMessageBuilder();


			builder.WithContent(messageSendModel.Content); //If null all ok

			if (messageSendModel.Files is not null)
				foreach (var file in messageSendModel.Files)
				{
					var stream = new MemoryStream(Encoding.Default.GetBytes(file.Reader.ReadToEnd()));
					builder.WithFile(file.FileName, stream);
				}

			if (messageSendModel.MessageEmbeds is not null)
			{
				foreach (var baseEmbed in messageSendModel.MessageEmbeds)
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

			if (messageSendModel.ComponentsRows is not null)
			{
				foreach (var row in messageSendModel.ComponentsRows)
				{
					var components = new List<DiscordComponent>();
					foreach (var component in row.Components)
						components.Add(component switch
						{
							MessageButton bnt => new DiscordButtonComponent(bnt.Style.GetDSharp(), bnt.Id, bnt.Text, bnt.Disabled),
							MessageLinkButton bnt => new DiscordLinkButtonComponent(bnt.Url, bnt.Text, bnt.Disabled),
							MessageSelectMenu menu => new DiscordSelectComponent(menu.Id, menu.PlaceHolder,
								menu.Options.Select(s => new DiscordSelectComponentOption(s.Labеl, s.Value, s.Description)), menu.Disabled, menu.MinOptions, menu.MaxOptions),
							_ => throw new NotSupportedException($"Component with a type {component.GetType()} is not supported")
						});

					builder.AddComponents(components);
				}
			}

			return builder;
		}
	}
}
