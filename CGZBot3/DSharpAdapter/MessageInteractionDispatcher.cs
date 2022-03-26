using CGZBot3.Entities.Message.Components;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace CGZBot3.DSharpAdapter
{
	internal class MessageInteractionDispatcher : IInteractionDispatcher, IDisposable
	{
		private readonly Message message;
		private readonly List<EventHolder> holders = new();
		private readonly MessageConverter converter;


		public MessageInteractionDispatcher(Message message)
		{
			this.message = message;
			if (message.SendModel.Components is null || message.SendModel.Components.Count == 0) throw new ArgumentException("Enable to created InteractionDispatcher if message no contains components");
			message.BaseChannel.BaseServer.SourceClient.BaseClient.ComponentInteractionCreated += OnInteractionCreated;
			converter = new MessageConverter();
		}


		public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IComponent
		{
			throw new NotImplementedException();
		}

		public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IComponent
		{
			throw new NotImplementedException();
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

					var msgBuilder = converter.ConvertUp(ir.Respond);

					var inb = new DiscordInteractionResponseBuilder(msgBuilder) { IsEphemeral = true };

					await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, inb);

					break;
				}
			}
		}

		public void Dispose()
		{
			message.BaseChannel.BaseServer.SourceClient.BaseClient.ComponentInteractionCreated -= OnInteractionCreated;
		}


		private class EventHolder<TComponent> : EventHolder where TComponent : IComponent
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


			public override Task<ComponentInteractionResult>? Handle(ComponentInteractionCreateEventArgs args)
			{
				var server = message.TextChannel.Server;
				var components = message.SendModel.Components ?? throw new ImpossibleVariantException();
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
