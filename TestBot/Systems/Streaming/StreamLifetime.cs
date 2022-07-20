using DidiFrame.Lifetimes;
using DidiFrame.Utils;

namespace TestBot.Systems.Streaming
{
	public class StreamLifetime : AbstractFullLifetime<StreamState, StreamModel>
	{
		public const string StartStreamButtonId = "start_bnt";
		public const string FinishStreamButtonId = "finish_bnt";


		private readonly WaitFor waitForStartButton = new();
		private readonly WaitFor waitForFinishButton = new();
		private readonly IStringLocalizer<StreamLifetime> localizer;
		private readonly UIHelper uiHelper;


		public StreamLifetime(ILogger<StreamLifetime> logger, IStringLocalizer<StreamLifetime> localizer, UIHelper uiHelper) : base(logger)
		{
			this.localizer = localizer;
			this.uiHelper = uiHelper;
		}


		public void Replan(DateTime newStartDate)
		{
			using var holder = GetBase();
			var b = holder.Object;

			if (b.State == StreamState.Running)
				throw new InvalidOperationException("Stream already has started");

			using var baseObj = GetBase();

			baseObj.Object.PlanedStartTime = newStartDate;
		}

		public void Rename(string newName)
		{
			if (string.IsNullOrWhiteSpace(newName))
				throw new ArgumentException("Name was white space", nameof(newName));


			using var baseObj = GetBase();

			baseObj.Object.Name = newName;
		}

		public string GetName()
		{
			return GetBaseAuto(s => s.Name);
		}

		public IMember GetOwner()
		{
			return GetBaseAuto(s => s.Owner);
		}

		public void Move(string newPlace)
		{
			if (string.IsNullOrWhiteSpace(newPlace))
				throw new ArgumentException("Place was white space", nameof(newPlace));


			using var baseObj = GetBase();

			baseObj.Object.Place = newPlace;
		}

		private void AttachEvents(IMessage message)
		{
			using var holder = GetBase();
			var b = holder.Object;

			switch (b.State)
			{
				case StreamState.Announced:
					break;
				case StreamState.WaitingForStreamer:
					var di = message.GetInteractionDispatcher();
					di.Attach<MessageButton>(StartStreamButtonId, ctx =>
					{
						if (ctx.Invoker == b.Owner)
						{
							waitForStartButton.Callback();
							return Task.FromResult(new ComponentInteractionResult(new MessageSendModel(localizer["StartOk"])));
						}
						else return Task.FromResult(new ComponentInteractionResult(new MessageSendModel(localizer["StartFail"])));
					});
					break;
				case StreamState.Running:
					var di2 = message.GetInteractionDispatcher();
					di2.Attach<MessageButton>(FinishStreamButtonId, ctx =>
					{
						if (ctx.Invoker == b.Owner)
						{
							waitForFinishButton.Callback();
							return Task.FromResult(new ComponentInteractionResult(new MessageSendModel(localizer["FinishOk"])));
						}
						else return Task.FromResult(new ComponentInteractionResult(new MessageSendModel(localizer["FinishFail"])));
					});
					break;
				default:
					break;
			}
		}

		private MessageSendModel CreateReportMessage()
		{
			using var holder = GetBase();
			var b = holder.Object;

			return b.State switch
			{
				StreamState.Announced => uiHelper.CreateAnnouncedReport(b.Name, b.Owner, b.PlanedStartTime),
				StreamState.WaitingForStreamer => uiHelper.CreateWaitingStreamerReport(b.Name, b.Owner, b.PlanedStartTime),
				StreamState.Running => uiHelper.CreateRunningReport(b.Name, b.Owner, b.Place),
				_ => throw new ImpossibleVariantException()
			};
		}

		protected override void OnBuild(StreamModel initialBase, ILifetimeContext<StreamModel> context)
		{
			var controller = new SelectObjectContoller<StreamModel, MessageAliveHolder.Model>(context.AccessBase(), s => s.ReportMessage);
			AddReport(new MessageAliveHolder(controller, true, CreateReportMessage, AttachEvents));

			AddTransit(StreamState.Announced, StreamState.WaitingForStreamer, () => DateTime.UtcNow >= GetBaseAuto(s => s.PlanedStartTime));
			AddTransit(StreamState.WaitingForStreamer, StreamState.Announced, () => DateTime.UtcNow < GetBaseAuto(s => s.PlanedStartTime));
			AddTransit(StreamState.WaitingForStreamer, StreamState.Running, (token) => waitForStartButton.Await(token));
			AddTransit(StreamState.Running, null, (token) => waitForFinishButton.Await(token));
		}
	}
}
