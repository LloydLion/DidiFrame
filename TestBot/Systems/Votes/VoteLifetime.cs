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
				message = new MessageAliveHolder(initialBase.Message, modelGetter, true, CreateMessage, MessageWorker);

				message.StartupMessageAsync(initialBase).Wait();
			}
			catch (Exception ex)
			{
				context.CrashPipeline(ex, true);
			}


			ObjectHolder<MessageAliveHolder.Model> modelGetter(object? parameter)
			{
				return parameter is null ? Context.AccessBase().Open().SelectHolder(s => s.Message)
					: new ObjectHolder<MessageAliveHolder.Model>(((VoteModel)parameter).Message, _ => { });
			}
		}

		private MessageSendModel CreateMessage(object? parameter)
		{
			using var holder = parameter is null ? Context.AccessBase().Open() : new ObjectHolder<VoteModel>((VoteModel)parameter, _ => { });
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

		private void MessageWorker(object? parameter, IMessage message)
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
					await Message.Update(holder.Object);
				}

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
						await Message.DeleteAsync(holder.Object);
					}

					Message.Dispose();
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
