using CGZBot3.Data.Lifetime;
using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Components;
using CGZBot3.Utils;
using CGZBot3.Utils.StateMachine;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Games
{
	public class GameLifetime : AbstractStateBasedLifetime<GameState, GameModel>
	{
		public const string JoinGameButtonId = "join_bnt";
		public const string ExitGameButtonId = "exit_bnt";
		public const string StartGameButtonId = "start_bnt";
		public const string FinishGameButtonId = "finish_bnt";


		private readonly WaitFor waitForStartButton = new();
		private readonly WaitFor waitForFinishButton = new();
		private readonly WaitFor waitForCancel = new();
		private readonly MessageAliveHolder reportHolder;


		private readonly UIHelper uiHelper;
		private readonly IStringLocalizer<GameLifetime> localizer;


		public GameLifetime(GameModel baseObj, IServiceProvider services) : base(services, baseObj)
		{
			uiHelper = services.GetRequiredService<UIHelper>();
			localizer = services.GetRequiredService<IStringLocalizer<GameLifetime>>();
			reportHolder = new MessageAliveHolder(baseObj.ReportMessage, true, CreateReportMessage);
			reportHolder.AutoMessageCreated += OnReportCreated;

			ExternalBaseChanged += OnExternalBaseChanged;


			AddTransit(new ResetTransitWorker<GameState>(null, waitForCancel.Await));
			AddTransit(GameState.WaitingPlayers, GameState.WaitingCreator, WaitingPlayersWaitingCreatorTransit);
			AddTransit(GameState.WaitingCreator, GameState.WaitingPlayers, () => !WaitingPlayersWaitingCreatorTransit());
			AddTransit(GameState.WaitingCreator, GameState.Running, waitForStartButton.Await);
			AddTransit(GameState.Running, null, waitForFinishButton.Await);

			AddHandler(GameState.WaitingPlayers, OnStateChanged);
			AddHandler(GameState.WaitingCreator, OnStateChanged);
			AddHandler(GameState.Running, OnStateChanged);
		}


		public void Cancel()
		{
			waitForCancel.Callback();
		}

		private async void OnExternalBaseChanged(GameModel _)
		{
			await reportHolder.Update();
		}

		protected override void OnRun(GameState state)
		{
			if (reportHolder.IsExist) AttachEvents(reportHolder.Message);
			else reportHolder.CheckAsync().Wait();
		}

		protected override void OnDispose()
		{
			reportHolder.Dispose();
		}

		private bool WaitingPlayersWaitingCreatorTransit()
		{
			var b = GetBaseDirect();
			var cond1 = b.InGame.Count >= b.StartAtMembers - 1;
			var cond2 = b.WaitEveryoneInvited == false || b.Invited.All(s => b.InGame.Contains(s));
			return cond1 && cond2;
		}

		private void OnReportCreated(IMessage obj) => GetUpdater().Update(this);

		private void OnStateChanged(GameState oldState) => reportHolder.Update().Wait();

		private void AttachEvents(IMessage message)
		{
			var b = GetBaseDirect();

			//Every state contains components
			var di = message.GetInteractionDispatcher();

			switch (b.State)
			{
				case GameState.WaitingPlayers:
					di.Attach(JoinGameButtonId, adaptate(joinHandler));
					di.Attach(ExitGameButtonId, adaptate(exitHandler));
					break;
				case GameState.WaitingCreator:
					di.Attach(JoinGameButtonId, adaptate(joinHandler));
					di.Attach(ExitGameButtonId, adaptate(exitHandler));
					di.Attach(StartGameButtonId, adaptate(startHandler));
					break;
				case GameState.Running:
					di.Attach(JoinGameButtonId, adaptate(joinHandler));
					di.Attach(FinishGameButtonId, adaptate(finishHandler));
					break;
				default:
					break;
			}


			ComponentInteractionResult joinHandler(ComponentInteractionContext<MessageButton> ctx)
			{
				using var b = GetBase(); //It is not bug!! These is musn't be GetBaseProtected()

				if (b.Object.InGame.Contains(ctx.Invoker)) return new ComponentInteractionResult(new MessageSendModel(localizer["AlreadyInGame"]));
				else if (b.Object.Creator == ctx.Invoker) return new ComponentInteractionResult(new MessageSendModel(localizer["CreatorAlreadyInGame"]));
				else
				{
					b.Object.InGame.Add(ctx.Invoker);
					return new ComponentInteractionResult(new MessageSendModel(localizer["JoinOk"]));
				}
			}

			ComponentInteractionResult exitHandler(ComponentInteractionContext<MessageButton> ctx)
			{
				using var b = GetBase(); //It is not bug!! These is musn't be GetBaseProtected()

				if (b.Object.Creator == ctx.Invoker) return new ComponentInteractionResult(new MessageSendModel(localizer["CreatorMustBeInGame"]));
				else if (!b.Object.InGame.Contains(ctx.Invoker)) return new ComponentInteractionResult(new MessageSendModel(localizer["AlreadyOutGame"]));
				else
				{
					b.Object.InGame.Remove(ctx.Invoker);
					return new ComponentInteractionResult(new MessageSendModel(localizer["ExitOk"]));
				}
			}

			ComponentInteractionResult startHandler(ComponentInteractionContext<MessageButton> ctx)
			{
				var b = GetBaseDirect();
				if (ctx.Invoker == b.Creator)
				{
					waitForStartButton.Callback();
					return new ComponentInteractionResult(new MessageSendModel(localizer["StartOk"]));
				}
				else return new ComponentInteractionResult(new MessageSendModel(localizer["StartFail"]));
			}

			ComponentInteractionResult finishHandler(ComponentInteractionContext<MessageButton> ctx)
			{
				var b = GetBaseDirect();
				if (ctx.Invoker == b.Creator)
				{
					waitForFinishButton.Callback();
					return new ComponentInteractionResult(new MessageSendModel(localizer["FinishOk"]));
				}
				else return new ComponentInteractionResult(new MessageSendModel(localizer["FinishFail"]));
			}

			AsyncInteractionCallback<MessageButton> adaptate(Func<ComponentInteractionContext<MessageButton>, ComponentInteractionResult> func) => (ctx) => Task.FromResult(func(ctx));
		}

		private async Task<IMessage> CreateReportMessage(ITextChannel channel)
		{
			var b = GetBaseDirect();

			var sendModel = b.State switch
			{
				GameState.WaitingPlayers => uiHelper.CreateWatingForPlayersReport(b.Name, b.Creator, (IReadOnlyCollection<IMember>)b.Invited,
					(IReadOnlyCollection<IMember>)b.InGame, b.WaitEveryoneInvited, b.StartAtMembers, b.Description),
				GameState.WaitingCreator => uiHelper.CreataWaitingForCreatorReport(b.Name, b.Creator, (IReadOnlyCollection<IMember>)b.Invited,
					(IReadOnlyCollection<IMember>)b.InGame, b.WaitEveryoneInvited, b.StartAtMembers, b.Description),
				GameState.Running => uiHelper.CreateRunningReport(b.Name, b.Creator, (IReadOnlyCollection<IMember>)b.InGame, b.Description),
				_ => throw new ImpossibleVariantException()
			};

			var message = await channel.SendMessageAsync(sendModel);
			AttachEvents(message);

			return message;
		}
	}
}
