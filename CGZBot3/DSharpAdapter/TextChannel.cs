using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Components;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Text;

namespace CGZBot3.DSharpAdapter
{
	internal class TextChannel : Channel, ITextChannel
	{
		private readonly DiscordChannel channel;
		private readonly Server server;
		private readonly ComponentObserver observer;


		public TextChannel(DiscordChannel channel, Server server) : base(channel, server)
		{
			if(channel.Type.GetAbstract() != Entities.ChannelType.TextCompatible )
			{
				throw new InvalidOperationException("Channel must be text");
			}

			this.channel = channel;
			this.server = server;
			observer = new(this);

			server.SourceClient.BaseClient.ComponentInteractionCreated += observer.OnInteratctionCreated;
			server.SourceClient.BaseClient.MessageDeleted += observer.OnMessageDeleted;
		}


		public async Task<IReadOnlyCollection<IMessage>> GetMessagesAsync(int count = -1)
		{
			return (await channel.GetMessagesAsync(count == -1 ? 1000 : count)).Select(s => new Message(s, this, new MessageSendModel(s.Content))).ToArray();
		}

		public async Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			
			var builder = new DiscordMessageBuilder();

			if(messageSendModel.Components is not null)
				foreach (var component in messageSendModel.Components)
					if (component is MessageComponentsRow row)
						builder.AddComponents(row.Components.Select(s => getComponent(s)).ToArray());
					else builder.AddComponents(getComponent(component));

			DiscordComponent getComponent(IComponent nonRowComponent)
			{
				if (nonRowComponent is MessageLinkButton linkButton)
					return new DiscordLinkButtonComponent(linkButton.Url, linkButton.Text, linkButton.Disabled);
				else if (nonRowComponent is MessageButton button)
				{
					var uid = observer.GenerateNewUid(nonRowComponent);
					return new DiscordButtonComponent(button.Style.GetDSharp(), uid, button.Text, button.Disabled);
				}
				else if (nonRowComponent is MessageSelectMenu menu)
				{
					var uid = observer.GenerateNewUid(nonRowComponent);
					var options = menu.Options.Select(s =>
					{
						var optionId = observer.GenerateNewOptionUid(s);
						return new DiscordSelectComponentOption(s.Labеl, optionId, s.Description);
					}).ToArray();

					return new DiscordSelectComponent(uid, menu.PlaceHolder, options, menu.Disabled, menu.MinOptions, menu.MaxOptions);
				}
				else throw new NotSupportedException("Message contains unsupported component");
			}

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

			var msg = new Message(owner: this, sendModel: messageSendModel, message: await channel.SendMessageAsync(builder));
			observer.Observe(msg);
			return msg;
		}


		private class ComponentObserver
		{
			private readonly List<Message> messages = new();
			private int nextId = 0;
			private readonly Dictionary<string, IComponent> componentsIds = new();
			private readonly Dictionary<string, MessageSelectMenuOption> optionsIds = new();
			private readonly TextChannel owner;


			public ComponentObserver(TextChannel owner)
			{
				this.owner = owner;
			}


			public void Observe(Message message)
			{
				messages.Add(message);
			}

			public string GenerateNewUid(IComponent component)
			{
				var uid = "component-" + nextId++;
				componentsIds.Add(uid, component);
				return uid;
			}

			public string GenerateNewOptionUid(MessageSelectMenuOption option)
			{
				var uid = "option-" + nextId++;
				optionsIds.Add(uid, option);
				return uid;
			}

			public async Task OnInteratctionCreated(DiscordClient _, ComponentInteractionCreateEventArgs args)
			{
				var msg = messages.SingleOrDefault(s => s.Id == args.Message.Id);
				if (msg is null) return;

				var component = componentsIds[args.Id];
				var member = await owner.server.GetMemberAsync(args.User.Id);

				if (component is MessageButton button)
				{
					await button.HandleAsync(new ComponentInteractionContext<MessageButton>(member, msg, button, null));
				}
				else if (component is MessageSelectMenu menu)
				{
					var options = args.Values.Select(s => optionsIds[s]).ToArray();
					var state = new MessageSelectMenuState(options);
					await menu.HandleAsync(new ComponentInteractionContext<MessageSelectMenu>(member, msg, menu, state));
				}
				else throw new NotSupportedException("Message contains unsupported component");
			}

			public Task OnMessageDeleted(DiscordClient _, MessageDeleteEventArgs args)
			{
				var msg = messages.SingleOrDefault(s => s.Id == args.Message.Id);
				if (msg is null || msg.SendModel.Components is null) return Task.CompletedTask;

				foreach (var component in msg.SendModel.Components)
				{
					if (component is MessageSelectMenu menu)
						foreach (var option in menu.Options)
						{
							var id = optionsIds.Single(s => s.Value == option).Key;
							optionsIds.Remove(id);
						}

					var cid = componentsIds.Single(s => s.Value == component).Key;
					componentsIds.Remove(cid);
				}

				messages.Remove(msg);

				return Task.CompletedTask;
			}
		}
	}
}
