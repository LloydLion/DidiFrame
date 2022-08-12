using DidiFrame.Entities.Message.Components;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus.EventArgs;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Emzi0767.Utilities;
using DidiFrame.Culture;

namespace DidiFrame.Clients.DSharp.DiscordServer
{
	internal class InteractionDispatcherFactory
	{
		private static readonly EventId InteractionHandlerErrorID = new(10, "InteractionHandlerError");


		private readonly Dictionary<MessageIdentitify, List<EventHolder>> subs = new();
		private readonly ILogger<Client> logger;
		private readonly Server server;


		public InteractionDispatcherFactory(Server server)
		{
			server.SourceClient.BaseClient.ComponentInteractionCreated += InteractionCreated;
			logger = server.SourceClient.Logger;
			this.server = server;
		}


		public void DisposeInstance(ulong msgId, DiscordChannel channel)
		{
			lock (this)
			{
				subs.Remove(new(msgId, channel.Id));
			}
		}

		public void ResetInstance(ulong msgId, DiscordChannel channel)
		{
			lock (this)
			{
				var id = new MessageIdentitify(msgId, channel.Id);
				if (subs.ContainsKey(id)) subs[id].Clear();
			}
		}

		public void ClearSubscribers(DiscordChannel channel)
		{
			lock (this)
			{
				var toRemove = subs.Where(s => s.Key.ChannelId == channel.Id).ToArray();
				foreach (var item in toRemove) subs.Remove(item.Key);
			}
		}

		public IInteractionDispatcher CreateInstance(Message message)
		{
			lock (this)
			{
				var id = new MessageIdentitify(message.Id, message.BaseChannel.Id);

				if (!subs.ContainsKey(id)) subs.Add(id, new());
				var list = subs[id];
				return new Instance(this, message, list);
			}
		}

		private Task InteractionCreated(DiscordClient client, ComponentInteractionCreateEventArgs args)
		{
			EventHolder[] holders;

			lock (this)
			{
				var id = new MessageIdentitify(args.Message.Id, args.Channel.Id);

				if (!subs.ContainsKey(id))
					return Task.CompletedTask;
				holders = subs[id].ToArray();
			}

			Task.Run(() =>
			{
				try
				{
					server.SourceClient.CultureProvider?.SetupCulture(server);

					foreach (var item in holders)
					{
						var result = item.Handle(args);
						if (result is null) continue;
						else
						{
							args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
								new DiscordInteractionResponseBuilder(MessageConverter.ConvertUp(result.Result.Respond)).AsEphemeral()).Wait();

							break;
						}
					}
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, InteractionHandlerErrorID, ex, "Exception in component interaction handler");
				}
			});

			return Task.CompletedTask;
		}


		private class Instance : IInteractionDispatcher
		{
			private readonly object syncRoot;
			private readonly Message message;
			private readonly List<EventHolder> holders;


			public Instance(object syncRoot, Message message, List<EventHolder> holders)
			{
				this.syncRoot = syncRoot;
				this.message = message;
				this.holders = holders;
			}


			public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
			{
				lock (syncRoot)
				{
					holders.Add(new EventHolder<TComponent>(message, id, callback));
				}
			}

			public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
			{
				lock (syncRoot)
				{
					holders.RemoveAll(s => s is EventHolder<TComponent> ev && (ev.Id, ev.Callback) == (id, callback));
				}
			}
		}

		private class EventHolder<TComponent> : EventHolder where TComponent : IInteractionComponent
		{
			private readonly Message message;
			private readonly string id;
			private readonly AsyncInteractionCallback<TComponent> callback;


			public EventHolder(Message message, string id, AsyncInteractionCallback<TComponent> callback)
			{
				this.message = message;
				this.id = id;
				this.callback = callback;
			}


			public string Id => id;

			public AsyncInteractionCallback<TComponent> Callback => callback;


			public override Task<ComponentInteractionResult>? Handle(ComponentInteractionCreateEventArgs args)
			{
				var server = message.TextChannel.Server;
				var componentsRows = message.SendModel.ComponentsRows ?? throw new ImpossibleVariantException();
				var components = componentsRows.SelectMany(s => s.Components);

				if (args.Id != id) return null;

				var component = args.Interaction.Data.ComponentType switch
				{
					ComponentType.Button => components.Single(s => s is MessageButton bnt && bnt.Id == id),
					ComponentType.Select => components.Single(s => s is MessageSelectMenu menu && menu.Id == id),
					_ => throw new NotSupportedException()
				};

				if (component is not TComponent) return null;


				IComponentState<TComponent>? state = null;

				if (component is MessageSelectMenu)
					state = (IComponentState<TComponent>)new MessageSelectMenuState(args.Interaction.Data.Values);


				return callback.Invoke(new ComponentInteractionContext<TComponent>(server.GetMember(args.User.Id), message, (TComponent)component, state));
			}
		}

		private abstract class EventHolder
		{
			public abstract Task<ComponentInteractionResult>? Handle(ComponentInteractionCreateEventArgs args);
		}

		private struct MessageIdentitify
		{
			public MessageIdentitify(ulong messageId, ulong channelId)
			{
				MessageId = messageId;
				ChannelId = channelId;
			}


			public ulong MessageId { get; }

			public ulong ChannelId { get; }
		}
	}
}
