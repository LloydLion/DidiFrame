﻿using DidiFrame.Lifetimes;
using DidiFrame.Utils;

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

			var closeSuccess = ComponentInteractionResult.CreateWithMessage(new MessageSendModel(localizer["CloseSuccess"]));
			var confirmSuccess = ComponentInteractionResult.CreateWithMessage(new MessageSendModel(localizer["ConfirmSuccess"]));
			var closeFail = ComponentInteractionResult.CreateWithMessage(new MessageSendModel(localizer["CloseFail"]));
			var confirmFail = ComponentInteractionResult.CreateWithMessage(new MessageSendModel(localizer["ConfirmFail"]));

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
				}
				catch (Exception) { }
				finally
				{
					context.FinalizeLifetime();
				}
			}, token);
		}

		public void Update() { }

		public void Destroy() { }

		public void Dispose() { }
	}
}
