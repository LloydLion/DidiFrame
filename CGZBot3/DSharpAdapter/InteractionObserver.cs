using CGZBot3.Entities.Message.Components;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace CGZBot3.DSharpAdapter
{
	internal class InteractionObserver : IInteractionObserver
	{
		private readonly Server server;
		private readonly Dictionary<IComponent, AsyncInteractionCallback<IComponent>> handlers = new(); 


		public InteractionObserver(Server server)
		{
			this.server = server;
		}


		public void Observe<TComponent>(TComponent component, AsyncInteractionCallback<TComponent> callback) where TComponent : IComponent
		{
			handlers.Add(component, (ctx) => callback(new ComponentInteractionContext<TComponent>(ctx.Invoker, ctx.Message, (TComponent)ctx.Component, ctx.ComponentState as IComponentState<TComponent>)));
		}

		public async Task OnInteractionCreated(DiscordClient _, ComponentInteractionCreateEventArgs args)
		{
			if (args.Interaction.GuildId != server.Id) return;

			server.ComponentsRegistry.GetComponent(args.Interaction.);
		}
	}
}