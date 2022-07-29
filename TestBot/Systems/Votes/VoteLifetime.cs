using DidiFrame.Lifetimes;
using DidiFrame.Utils;

namespace TestBot.Systems.Votes
{
	internal class VoteLifetime : ILifetime<VoteModel>
	{
		private MessageAliveHolder<VoteModel>? message;
		private ILifetimeContext<VoteModel>? context;


		private ILifetimeContext<VoteModel> Context => context ?? throw new NullReferenceException();

		private MessageAliveHolder<VoteModel> Message => message ?? throw new NullReferenceException();


		public VoteLifetime()
		{

		}


		public void Run(VoteModel initialBase, ILifetimeContext<VoteModel> context)
		{
			try
			{
				this.context = context;
				message = new MessageAliveHolder<VoteModel>(s => s.Message, CreateMessage, MessageWorker);

				message.StartupAsync(initialBase).Wait();
			}
			catch (Exception ex)
			{
				context.CrashPipeline(ex, true);
			}
		}

		private MessageSendModel CreateMessage(VoteModel parameter)
		{
			var options = parameter.Options.Select(s => new MessageSelectMenuOption($"{s.Key} [{s.Value}]", s.Key, $"Vote for {s.Key} option [{s.Value}]")).ToArray();

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

		private void MessageWorker(VoteModel parameter, IMessage message, bool isModified)
		{
			if (isModified) return;

			var id = message.GetInteractionDispatcher();

			id.Attach<MessageSelectMenu>("menu", async (ctx) =>
			{
				string select;

				using var holder = Context.AccessBase().Open();
				var state = (MessageSelectMenuState)(ctx.ComponentState ?? throw new NullReferenceException());
				select = state.SelectedValues.Single();
				holder.Object.Options[select]++;
				await Message.UpdateAsync(holder.Object);

				return new(new("Ok! You vote " + select));
			});

			id.Attach<MessageButton>("closeBnt", async (ctx) =>
			{
				try
				{
					using var holder = Context.AccessBase().Open();
					if (ctx.Invoker.Equals((IUser)holder.Object.Creator) == false)
						return new(new("You aren't invoker"));
					await Message.FinalizeAsync(holder.Object);
				}
				catch (Exception ex)
				{
					Context.CrashPipeline(ex, false);
				}

				Context.FinalizeLifetime();
				return new(new("Vote finished"));
			});
		}
	}
}
