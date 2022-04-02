using CGZBot3.Data.Lifetime;
using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Components;
using CGZBot3.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Streaming
{
	public class StreamLifetime : AbstractStateBasedLifetime<StreamState, StreamModel>
	{
		public const string StartStreamButtonId = "start_bnt";
		public const string FinishStreamButtonId = "finish_bnt";


		private readonly WaitFor waitForStartButton = new();
		private readonly WaitFor waitForFinishButton = new();
		private readonly IStringLocalizer<StreamLifetime> localizer;
		private readonly UIHelper uiHelper;
		private readonly MessageAliveHolder reportHolder;


		public StreamLifetime(StreamModel baseObj, IServiceProvider services) : base(services, baseObj)
		{
			localizer = services.GetRequiredService<IStringLocalizer<StreamLifetime>>();
			uiHelper = services.GetRequiredService<UIHelper>();

			reportHolder = new MessageAliveHolder(baseObj.ReportMessage, true, CreateReportMessage);
			reportHolder.AutoMessageCreated += OnReportCreated;


			AddTransit(StreamState.Announced, StreamState.WaitingForStreamer, () => DateTime.UtcNow >= GetBaseDirect().PlanedStartTime);
			AddTransit(StreamState.WaitingForStreamer, StreamState.Running, (token) => waitForStartButton.Await(token));
			AddTransit(StreamState.Running, null, (token) => waitForFinishButton.Await(token));

			AddHandler(StreamState.WaitingForStreamer, OnStateChanged);
			AddHandler(StreamState.Running, OnStateChanged);
		}


		protected override void OnRun(StreamState state)
		{
			if (reportHolder.IsExist) AttachEvents(reportHolder.Message);
			else reportHolder.CheckAsync().Wait();
		}

		private void OnReportCreated(IMessage obj)
		{
			GetUpdater().Update(this);
		}

		private void OnStateChanged(StreamState state)
		{
			using var b = GetBase();
			reportHolder.Update().Wait();
		}

		protected override void OnDispose()
		{
			reportHolder.DeleteAsync().Wait();
			reportHolder.Dispose();
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

		private async Task<IMessage> CreateReportMessage(ITextChannel channel)
		{
			var b = GetBaseDirect();
			var report = b.State switch
			{
				StreamState.Announced => uiHelper.CreateAnnouncedReport(b.Name, b.Owner, b.PlanedStartTime),
				StreamState.WaitingForStreamer => uiHelper.CreateWaitingStreamerReport(b.Name, b.Owner, b.PlanedStartTime),
				StreamState.Running => uiHelper.CreateRunningReport(b.Name, b.Owner, b.Place),
				_ => throw new ImpossibleVariantException()
			};

			var message = await channel.SendMessageAsync(report);
			AttachEvents(message);
			return message;
		}
	}
}
