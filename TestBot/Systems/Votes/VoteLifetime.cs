using DidiFrame.Lifetimes;
using DidiFrame.Utils;

namespace TestBot.Systems.Votes
{
	internal class VoteLifetime : ILifetime<VoteModel>
	{
		private MessageAliveHolder? message;
		private ILifetimeContext<VoteModel>? context;


		private ILifetimeContext<VoteModel> Context => context ?? throw new NullReferenceException();

		private MessageAliveHolder Message => message ?? throw new NullReferenceException();


		public VoteLifetime()
		{

		}


		public void Run(VoteModel initialBase, ILifetimeContext<VoteModel> context)
		{
			try
			{
				this.context = context;
				var controller = new SelectObjectContoller<VoteModel, MessageAliveHolder.Model>(context.AccessBase(), s => s.Message);
				message = new MessageAliveHolder(controller, true, CreateMessage, MessageWorker);

				message.StartupMessageAsync().Wait();
			}
			catch (Exception ex)
			{
				context.FinalizeLifetime(ex);
			}
		}

		private MessageSendModel CreateMessage()
		{
			using var holder = Context.AccessBase().Open();

			var options = holder.Object.Options.Select(s => new MessageSelectMenuOption($"{s.Key} [{s.Value}]", s.Key, $"Vote for {s.Key} option [{s.Value}]")).ToArray();

			return new("Do your choose")
			{
				ComponentsRows = new[]
				{
					new MessageComponentsRow(new IComponent[]
					{
						new MessageSelectMenu("menu", options, "Placeholder")
					}),

					new MessageComponentsRow(new IComponent[]
					{
						new MessageButton("closeBnt", "Close vote", ButtonStyle.Danger)
					})
				}
			};
		}

		private void MessageWorker(IMessage message)
		{
			var id = message.GetInteractionDispatcher();

			id.Attach<MessageSelectMenu>("menu", async (ctx) =>
			{
				string select;

				using (var holder = Context.AccessBase().Open())
				{
					var state = (MessageSelectMenuState)(ctx.ComponentState ?? throw new NullReferenceException());
					select = state.SelectedValues.Single();
					holder.Object.Options[select]++;
				}

				await Message.Update();
				return new(new("Ok! You vote " + select));
			});

			id.Attach<MessageButton>("closeBnt", async (ctx) =>
			{
				try
				{
					using (var holder = Context.AccessBase().Open())
					{
						if (ctx.Invoker != holder.Object.Creator)
							return new(new("You aren't invoker"));
					}

					await Message.DeleteAsync();
					Message.Dispose();
				}
				catch (Exception ex)
				{
					Context.FinalizeLifetime(ex);
				}

				Context.FinalizeLifetime();
				return new(new("Vote finished"));
			});
		}
	}
}
