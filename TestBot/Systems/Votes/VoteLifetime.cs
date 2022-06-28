using DidiFrame.Data.Lifetime;
using DidiFrame.Entities.Message.Components;
using DidiFrame.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Systems.Votes
{
	internal class VoteLifetime : ILifetime<VoteModel>
	{
		private readonly VoteModel model;
		private readonly IServiceProvider services;
		private readonly MessageAliveHolder message;
		private ILifetimeStateUpdater<VoteModel>? updater;


		private ILifetimeStateUpdater<VoteModel> Updater => updater ?? throw new NullReferenceException();


		public VoteLifetime(VoteModel model, IServiceProvider services)
		{
			this.model = model;
			this.services = services;
			message = new MessageAliveHolder(model.Message, true, CreateMessage, MessageWorker);
		}


		public VoteModel GetBaseClone()
		{
			return new VoteModel(model.Creator, model.Options, model.Title, model.Message, model.Id);
		}

		public void Run(ILifetimeStateUpdater<VoteModel> updater)
		{
			this.updater = updater;

			message.AutoMessageCreated += AutoMessageCreated;

			if (message.IsExist) message.ProcessMessage();
			else message.CheckAsync().Wait();
		}

		private void AutoMessageCreated(IMessage obj)
		{
			Updater.Update(this);
		}

		private MessageSendModel CreateMessage()
		{
			var options = model.Options.Select(s => new MessageSelectMenuOption($"{s.Key} [{s.Value}]", s.Key, $"Vote for {s.Key} option [{s.Value}]")).ToArray();

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
				var state = (MessageSelectMenuState)(ctx.ComponentState ?? throw new NullReferenceException());
				var select = state.SelectedValues.Single();
				model.Options[select]++;
				await this.message.Update();
				Updater.Update(this);
				return new(new("Ok! You vote " + select));
			});

			id.Attach<MessageButton>("closeBnt", (ctx) =>
			{
				if (ctx.Invoker != model.Creator)
					return Task.FromResult<ComponentInteractionResult>(new(new("You aren't invoker")));
				else
				{
					this.message.Dispose();
					Updater.Finish(this);
					return Task.FromResult<ComponentInteractionResult>(new(new("Vote finished")));
				}
			});
		}
	}
}
