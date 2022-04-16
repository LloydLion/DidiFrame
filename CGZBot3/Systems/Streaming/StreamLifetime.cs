using CGZBot3.Data.Lifetime;
using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Components;
using CGZBot3.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Streaming
{
	public class StreamLifetime : AbstractFullLifetime<StreamState, StreamModel>
	{
		public const string StartStreamButtonId = "start_bnt";
		public const string FinishStreamButtonId = "finish_bnt";


		private readonly WaitFor waitForStartButton = new();
		private readonly WaitFor waitForFinishButton = new();
		private readonly IStringLocalizer<StreamLifetime> localizer;
		private readonly UIHelper uiHelper;


		public StreamLifetime(StreamModel baseObj, IServiceProvider services) : base(services, baseObj)
		{
			localizer = services.GetRequiredService<IStringLocalizer<StreamLifetime>>();
			uiHelper = services.GetRequiredService<UIHelper>();

			AddReport(new MessageAliveHolder(baseObj.ReportMessage, true, CreateReportMessage, AttachEvents));


			AddTransit(StreamState.Announced, StreamState.WaitingForStreamer, () => DateTime.UtcNow >= GetBaseDirect().PlanedStartTime);
			AddTransit(StreamState.WaitingForStreamer, StreamState.Announced, () => DateTime.UtcNow < GetBaseDirect().PlanedStartTime);
			AddTransit(StreamState.WaitingForStreamer, StreamState.Running, (token) => waitForStartButton.Await(token));
			AddTransit(StreamState.Running, null, (token) => waitForFinishButton.Await(token));
		}


		public void Replan(DateTime newStartDate)
		{
			if (GetBaseDirect().State == StreamState.Running)
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

		public void Move(string newPlace)
		{
			if (string.IsNullOrWhiteSpace(newPlace))
				throw new ArgumentException("Place was white space", nameof(newPlace));


			using var baseObj = GetBase();

			baseObj.Object.Place = newPlace;
		}

		private void AttachEvents(IMessage message)
		{
			switch (GetBaseDirect().State)
			{
				case StreamState.Announced:
					break;
				case StreamState.WaitingForStreamer:
					var di = message.GetInteractionDispatcher();
					di.Attach<MessageButton>(StartStreamButtonId, ctx =>
					{
						if (ctx.Invoker == GetBaseDirect().Owner)
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
						if (ctx.Invoker == GetBaseDirect().Owner)
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
			var b = GetBaseDirect();
			return b.State switch
			{
				StreamState.Announced => uiHelper.CreateAnnouncedReport(b.Name, b.Owner, b.PlanedStartTime),
				StreamState.WaitingForStreamer => uiHelper.CreateWaitingStreamerReport(b.Name, b.Owner, b.PlanedStartTime),
				StreamState.Running => uiHelper.CreateRunningReport(b.Name, b.Owner, b.Place),
				_ => throw new ImpossibleVariantException()
			};
		}
	}
}
