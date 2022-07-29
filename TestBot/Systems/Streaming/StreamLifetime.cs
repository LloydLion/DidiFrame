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
			if (GetStateMachine().CurrentState == StreamState.Running)
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

		public string GetName() => GetBaseProperty(s => s.Name);

		public IMember GetOwner() => GetBaseProperty(s => s.Owner);

		public void Move(string newPlace)
		{
			if (string.IsNullOrWhiteSpace(newPlace))
				throw new ArgumentException("Place was white space", nameof(newPlace));


			using var baseObj = GetBase();

			baseObj.Object.Place = newPlace;
		}

		private void AttachEvents(StreamModel parameter, IMessage message, bool isModified)
		{
			if (isModified) message.ResetInteractionDispatcher();

			switch (parameter.State)
			{
				case StreamState.Announced:
					break;
				case StreamState.WaitingForStreamer:
					var di = message.GetInteractionDispatcher();
					di.Attach<MessageButton>(StartStreamButtonId, ctx =>
					{
						if (ctx.Invoker.Equals((IUser)parameter.Owner))
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
						if (ctx.Invoker.Equals((IUser)parameter.Owner))
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

		private MessageSendModel CreateReportMessage(StreamModel parameter)
		{
			return parameter.State switch
			{
				StreamState.Announced => uiHelper.CreateAnnouncedReport(parameter.Name, parameter.Owner, parameter.PlanedStartTime),
				StreamState.WaitingForStreamer => uiHelper.CreateWaitingStreamerReport(parameter.Name, parameter.Owner, parameter.PlanedStartTime),
				StreamState.Running => uiHelper.CreateRunningReport(parameter.Name, parameter.Owner, parameter.Place),
				_ => throw new ImpossibleVariantException()
			};
		}

		protected override void OnBuild(StreamModel initialBase)
		{
			AddReport(new MessageAliveHolder<StreamModel>(s => s.ReportMessage, CreateReportMessage, AttachEvents));

			AddTransit(StreamState.Announced, StreamState.WaitingForStreamer, () => DateTime.UtcNow >= GetBaseProperty(s => s.PlanedStartTime));
			AddTransit(StreamState.WaitingForStreamer, StreamState.Announced, () => DateTime.UtcNow < GetBaseProperty(s => s.PlanedStartTime));
			AddTransit(StreamState.WaitingForStreamer, StreamState.Running, (token) => waitForStartButton.Await(token));
			AddTransit(StreamState.Running, null, (token) => waitForFinishButton.Await(token));
		}
	}
}
