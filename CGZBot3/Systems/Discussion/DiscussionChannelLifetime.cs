using CGZBot3.Data.Lifetime;
using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Components;
using CGZBot3.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Discussion
{
	internal class DiscussionChannelLifetime : ILifetime<DiscussionChannel>
	{
		public const string ConfirmButtonId = "confirn_bnt";
		public const string CloseButtonId = "close_bnt";


		private readonly WaitFor waitForConfirm = new();
		private readonly WaitFor waitForClose = new();
		private readonly DiscussionChannel baseObj;
		private readonly IStringLocalizer<DiscussionChannelLifetime> localizer;
		private Task? runTask;
		private readonly CancellationTokenSource cts = new();


		public DiscussionChannelLifetime(DiscussionChannel baseObj, IServiceProvider services)
		{
			this.baseObj = baseObj;
			localizer = services.GetRequiredService<IStringLocalizer<DiscussionChannelLifetime>>();
		}

		~DiscussionChannelLifetime()
		{
			cts.Cancel();
			runTask?.Wait();
		}


		public ObjectHolder<DiscussionChannel> GetBase()
		{
			return new ObjectHolder<DiscussionChannel>(baseObj, (_) => { });
		}

		public DiscussionChannel GetBaseClone()
		{
			return baseObj;
		}

		public void Run(ILifetimeStateUpdater<DiscussionChannel> updater)
		{
			var id = baseObj.AskMessage.GetInteractionDispatcher();

			var closeSuccess = new ComponentInteractionResult(new MessageSendModel(localizer["CloseSuccess"]));
			var confirmSuccess = new ComponentInteractionResult(new MessageSendModel(localizer["ConfirmSuccess"]));
			var closeFail = new ComponentInteractionResult(new MessageSendModel(localizer["CloseFail"]));
			var confirmFail = new ComponentInteractionResult(new MessageSendModel(localizer["ConfirmFail"]));

			id.Attach<MessageButton>(ConfirmButtonId, (ctx) => { waitForConfirm.Callback(); return Task.FromResult(confirmSuccess); });
			id.Attach<MessageButton>(CloseButtonId, (ctx) => { waitForClose.Callback(); return Task.FromResult(closeSuccess); });

			var token = cts.Token;

			runTask = Task.Run(() =>
			{
				var index = Task.WaitAny(new Task[] { waitForClose.Await(), waitForConfirm.Await() }, token);
				if (cts.IsCancellationRequested) return;

				//Close
				if (index == 0)
				{
					baseObj.Channel.DeleteAsync().Wait();
				}
				//Confirm
				else
				{
					baseObj.AskMessage.DeleteAsync().Wait();
				}

				updater.Finish(this);

			}, token);
		}
	}
}
