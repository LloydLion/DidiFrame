using DidiFrame.Lifetimes;
using DidiFrame.Entities.Message;
using DidiFrame.Entities.Message.Components;
using DidiFrame.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Discussion
{
	internal class DiscussionChannelLifetime : ILifetime<DiscussionChannel>
	{
		public const string ConfirmButtonId = "confirn_bnt";
		public const string CloseButtonId = "close_bnt";


		private readonly WaitFor waitForConfirm = new();
		private readonly WaitFor waitForClose = new();
		private readonly IStringLocalizer<DiscussionChannelLifetime> localizer;
		private Task? runTask;
		private readonly CancellationTokenSource cts = new();


		public DiscussionChannelLifetime(IStringLocalizer<DiscussionChannelLifetime> localizer)
		{
			this.localizer = localizer;
		}

		~DiscussionChannelLifetime()
		{
			cts.Cancel();
			runTask?.Wait();
		}


		public void Run(DiscussionChannel initialBase, ILifetimeContext<DiscussionChannel> context)
		{
			var id = initialBase.AskMessage.GetInteractionDispatcher();

			var closeSuccess = new ComponentInteractionResult(new MessageSendModel(localizer["CloseSuccess"]));
			var confirmSuccess = new ComponentInteractionResult(new MessageSendModel(localizer["ConfirmSuccess"]));
			var closeFail = new ComponentInteractionResult(new MessageSendModel(localizer["CloseFail"]));
			var confirmFail = new ComponentInteractionResult(new MessageSendModel(localizer["ConfirmFail"]));

			id.Attach<MessageButton>(ConfirmButtonId, (ctx) => { waitForConfirm.Callback(); return Task.FromResult(confirmSuccess); });
			id.Attach<MessageButton>(CloseButtonId, (ctx) => { waitForClose.Callback(); return Task.FromResult(closeSuccess); });

			var token = cts.Token;

			runTask = Task.Run(() =>
			{
				try
				{
					var index = Task.WaitAny(new Task[] { waitForClose.Await(), waitForConfirm.Await() }, token);
					if (cts.IsCancellationRequested) return;

					using var baseObject = context.AccessBase().Open();

					//Close
					if (index == 0)
					{
						baseObject.Object.Channel.DeleteAsync().Wait();
					}
					//Confirm
					else
					{
						baseObject.Object.AskMessage.DeleteAsync().Wait();
					}

					context.FinalizeLifetime();
				}
				catch (Exception ex)
				{
					context.FinalizeLifetime(ex);
				}
			}, token);
		}
	}
}
