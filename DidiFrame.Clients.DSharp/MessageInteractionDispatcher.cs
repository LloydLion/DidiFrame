using DidiFrame.Entities.Message.Components;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IInteractionDispatcher
	/// </summary>
	public class MessageInteractionDispatcher : IInteractionDispatcher, IDisposable
	{
		private readonly Message message;
		private readonly List<EventHolder> holders = new();


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.MessageInteractionDispatcher using DSharp message wrap
		/// </summary>
		/// <param name="message">DSharp message wrap</param>
		/// <exception cref="ArgumentException">If message doesn't contain any components</exception>
		/// <exception cref="ArgumentException">If message have been sent by not bot</exception>
		public MessageInteractionDispatcher(Message message)
		{
			this.message = message;
			if (message.TextChannel.Server.Client.SelfAccount.Id != message.Author.Id)
				throw new ArgumentException("Enable to create InteractionDispatcher if message sent not by bot");
			if (message.SendModel.ComponentsRows is null || message.SendModel.ComponentsRows.Any() == false)
				throw new ArgumentException("Enable to create InteractionDispatcher if message no contains components");
			message.BaseChannel.BaseServer.SourceClient.BaseClient.ComponentInteractionCreated += OnInteractionCreated;
		}


		/// <inheritdoc/>
		public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
		{
			holders.Add(new EventHolder<TComponent>(message, id, callback));
		}

		/// <inheritdoc/>
		public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
		{
			holders.RemoveAll(s => s is EventHolder<TComponent> ev && (ev.Id, ev.Callback) == (id, callback));
		}

		private async Task OnInteractionCreated(DiscordClient client, ComponentInteractionCreateEventArgs args)
		{
			foreach (var holder in holders)
			{
				var result = holder.Handle(args);
				if (result is null) continue;
				else
				{
					var ir = await result;

					var msgBuilder = MessageConverter.ConvertUp(ir.Respond);

					var inb = new DiscordInteractionResponseBuilder(msgBuilder) { IsEphemeral = true };

					await message.BaseChannel.BaseServer.SourceClient.DoSafeOperationAsync(async () =>
						await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, inb));

					break;
				}
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			message.BaseChannel.BaseServer.SourceClient.BaseClient.ComponentInteractionCreated -= OnInteractionCreated;
			GC.SuppressFinalize(this);
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
