using DidiFrame.Entities.Message.Components;
using DidiFrame.Exceptions;
using DSharpPlus.EventArgs;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using DidiFrame.Culture;

namespace DidiFrame.Client.DSharp.DiscordServer
{
	public sealed class InteractionDispatcherFactory : IDisposable
	{
		private const int TemporalModeDeleteTimeoutInMinutes = 20;
		private static readonly EventId InteractionHandlerErrorID = new(10, "InteractionHandlerError");


		private readonly Dictionary<MessageIdentitify, MessageSubscribers> subscribers = new();
		private readonly ILogger<DSharpClient> logger;
		private readonly Server server;
		private readonly object syncRoot = new();
		private readonly ModalHelper modalHelper;

		
		public InteractionDispatcherFactory(Server server)
		{
			server.SourceClient.BaseClient.ComponentInteractionCreated += InteractionCreated;
			logger = server.SourceClient.Logger;
			this.server = server;
			modalHelper = new ModalHelper(server.SourceClient, server.SourceClient.Localizer, server.SourceClient.ModalValidator);
		}


		private void ClearTemporalSubsribers()
		{
			lock (syncRoot)
			{
				var markedToDelete = new List<MessageIdentitify>();

				foreach (var item in subscribers)
					if (item.Value.IsTemporal && DateTime.UtcNow - item.Value.CreationTimespamp >= TimeSpan.FromMinutes(TemporalModeDeleteTimeoutInMinutes))
						markedToDelete.Add(item.Key);

				foreach (var toDelete in markedToDelete)
					subscribers.Remove(toDelete);
			}
		}

		public void DisposeInstance(ulong msgId, DiscordChannel channel)
		{
			lock (syncRoot)
			{
				ClearTemporalSubsribers();
				subscribers.Remove(new(msgId, channel.Id));
			}
		}

		public void ResetInstance(ulong msgId, DiscordChannel channel)
		{
			lock (syncRoot)
			{
				ClearTemporalSubsribers();
				var id = new MessageIdentitify(msgId, channel.Id);
				if (subscribers.ContainsKey(id)) subscribers.Remove(id);
			}
		}

		public void ClearSubscribers(DiscordChannel channel)
		{
			lock (syncRoot)
			{
				ClearTemporalSubsribers();
				var toRemove = subscribers.Where(s => s.Key.ChannelId == channel.Id).ToArray();
				foreach (var item in toRemove) subscribers.Remove(item.Key);
			}
		}

		public IInteractionDispatcher CreateInstance(Message message)
		{
			lock (syncRoot)
			{
				ClearTemporalSubsribers();

				var id = new MessageIdentitify(message.Id, message.BaseChannel.Id);
				MessageSubscribers msgSubs;

				if (subscribers.ContainsKey(id))
				{
					msgSubs = subscribers[id];
					if (msgSubs.IsTemporal)
						throw new InvalidOperationException($"Interaction dispatcher for message {message.Id} in {message.BaseChannel.Id} cannot be getted " +
							"in non-temporal mode because it already created in temporal mode");
				}
				else
				{
					msgSubs = new MessageSubscribers(isTemporal: false);
					subscribers.Add(id, msgSubs);
				}

				return new Instance(this, message, msgSubs.Handlers);
			}
		}

		public IInteractionDispatcher CreateInstanceInTemporalMode(Message message)
		{
			lock (syncRoot)
			{
				ClearTemporalSubsribers();

				var id = new MessageIdentitify(message.Id, message.BaseChannel.Id);
				MessageSubscribers msgSubs;

				if (subscribers.ContainsKey(id))
				{
					msgSubs = subscribers[id];
					if (msgSubs.IsTemporal)
						throw new InvalidOperationException($"Interaction dispatcher for message {message.Id} in {message.BaseChannel.Id} cannot be getted " +
							"in temporal mode because it already created in non-temporal mode");
				}
				else
				{
					msgSubs = new MessageSubscribers(isTemporal: true);
					subscribers.Add(id, msgSubs);
				}

				return new Instance(this, message, msgSubs.Handlers);
			}
		}

		private Task InteractionCreated(DiscordClient client, ComponentInteractionCreateEventArgs args)
		{
			ClearTemporalSubsribers();

			EventHolder[] holders;

			lock (syncRoot)
			{
				var id = new MessageIdentitify(args.Message.Id, args.Channel.Id);

				if (!subscribers.ContainsKey(id))
					return Task.CompletedTask;
				holders = subscribers[id].Handlers.ToArray();
			}

			Task.Run(() =>
			{
				try
				{
					server.SourceClient.CultureProvider?.SetupCulture(server.CreateWrap());

					foreach (var item in holders)
					{
						var result = item.Handle(args);
						if (result is null) continue;
						else
						{
							var interactionResult = result.Result;

							switch (interactionResult.ResultType)
							{
								case ComponentInteractionResult.Type.None:
									args.Interaction.CreateResponseAsync(InteractionResponseType.Pong).Wait();
									break;
								case ComponentInteractionResult.Type.Message:
									args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
										new DiscordInteractionResponseBuilder(MessageConverter.ConvertUp(result.Result.GetRespondMessage())).AsEphemeral()).Wait();

									var subscriber = interactionResult.GetInteractionDispatcherSubcriber();
									if (subscriber is not null)
									{
										var responce = args.Interaction.GetOriginalResponseAsync().Result;
										var interactionDispatcher = CreateInstanceInTemporalMode(new Message(responce.Id, () => responce, (TextChannelBase)server.GetChannel(responce.ChannelId)));
										subscriber(interactionDispatcher);
									}
									break;
								case ComponentInteractionResult.Type.Modal:
									modalHelper.CreateModalAsync(args.Interaction, interactionResult.GetModal()).Wait();
									break;
								default:
									throw new NotSupportedException();
							}

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

		public void Dispose()
		{
			modalHelper.Dispose();
		}


		private sealed class Instance : IInteractionDispatcher
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

		private sealed class EventHolder<TComponent> : EventHolder where TComponent : IInteractionComponent
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

		private struct MessageSubscribers
		{
			public MessageSubscribers()
			{
				Handlers = new();
				CreationTimespamp = DateTime.UtcNow;
				IsTemporal = false;
			}

			public MessageSubscribers(bool isTemporal)
			{
				Handlers = new();
				CreationTimespamp = DateTime.UtcNow;
				IsTemporal = isTemporal;
			}


			public List<EventHolder> Handlers { get; }

			public DateTime CreationTimespamp { get; }

			public bool IsTemporal { get; }
		}
	}
}
