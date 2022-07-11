using DidiFrame.Entities.Message.Components;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus.EventArgs;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	internal class InteractionDispatcherFactory
	{
		private readonly Dictionary<ulong, List<EventHolder>> subs = new();


		public InteractionDispatcherFactory(Server server)
		{
			server.SourceClient.BaseClient.ComponentInteractionCreated += InteractionCreated;
		}


		public void DisposeInstance(ulong msgId)
		{
			lock (this)
			{
				subs.Remove(msgId);
			}
		}

		public void ResetInstance(ulong msgId)
		{
			lock (this)
			{
				if (subs.ContainsKey(msgId)) subs[msgId].Clear();
			}
		}

		public IInteractionDispatcher CreateInstance(Message message)
		{
			lock (this)
			{
				//if (server.Client.SelfAccount.Id != message.Author.Id)
				//	throw new ArgumentException("Enable to create InteractionDispatcher if message sent not by bot");
				if (!subs.ContainsKey(message.Id)) subs.Add(message.Id, new());
				var list = subs[message.Id];
				return new Instance(this, message, list);
			}
		}

		private Task InteractionCreated(DiscordClient client, ComponentInteractionCreateEventArgs args)
		{
			lock (this)
			{
				if (!subs.ContainsKey(args.Message.Id))
					return Task.CompletedTask;

				foreach (var item in subs[args.Message.Id])
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

				return Task.CompletedTask;
			}
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
	}
}
