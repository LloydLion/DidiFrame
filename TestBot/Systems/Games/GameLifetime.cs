using DidiFrame.Lifetimes;
using DidiFrame.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Games
{
	public class GameLifetime : AbstractFullLifetime<GameState, GameModel>
	{
		public const string JoinGameButtonId = "join_bnt";
		public const string ExitGameButtonId = "exit_bnt";
		public const string StartGameButtonId = "start_bnt";
		public const string FinishGameButtonId = "finish_bnt";


		private readonly WaitFor waitForStartButton = new();
		private readonly WaitFor waitForFinishButton = new();
		private readonly UIHelper uiHelper;
		private readonly IStringLocalizer<GameLifetime> localizer;


		public GameLifetime(ILogger<GameLifetime> logger, UIHelper uiHelper, IStringLocalizer<GameLifetime> localizer) : base(logger)
		{
			this.uiHelper = uiHelper;
			this.localizer = localizer;
		}


		public IMember GetCreator()
		{
			using var b = GetBase();
			return b.Object.Creator;
		}

		public string GetName()
		{
			using var b = GetBase();
			return b.Object.Name;
		}

		public void Invite(IReadOnlyCollection<IMember> members)
		{
			using var baseObj = GetBase();
			foreach (var member in members)
				if (member.Server != baseObj.Object.Server)
					throw new ArgumentException("Some member from other server", nameof(members));

			foreach (var member in members)
				if (baseObj.Object.Invited.Contains(member) == false)
					baseObj.Object.Invited.Add(member);

			if (baseObj.Object.Invited.Contains(baseObj.Object.Creator))
				baseObj.Object.Invited.Remove(baseObj.Object.Creator);
		}

		protected override void OnBuild(GameModel initialBase, ILifetimeContext<GameModel> context)
		{
			Validate(initialBase);

			var controller = new SelectObjectContoller<GameModel, MessageAliveHolder.Model>(context.AccessBase(), s => s.ReportMessage);
			AddReport(new MessageAliveHolder(controller, true, CreateReportMessage, AttachEvents));

			AddTransit(GameState.WaitingPlayers, GameState.WaitingCreator, WaitingPlayersWaitingCreatorTransit);
			AddTransit(GameState.WaitingCreator, GameState.WaitingPlayers, () => !WaitingPlayersWaitingCreatorTransit());
			AddTransit(GameState.WaitingCreator, GameState.Running, waitForStartButton.Await);
			AddTransit(GameState.Running, null, waitForFinishButton.Await);
		}

		public void ClearInvites()
		{
			using var baseObj = GetBase();
			baseObj.Object.Invited.Clear();
		}

		public void Rename(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name was whitespace", nameof(name));

			using var baseObj = GetBase();
			baseObj.Object.Name = name;
		}

		public void ChangeDescription(string description)
		{
			if (string.IsNullOrWhiteSpace(description))
				throw new ArgumentException("Description was whitespace", nameof(description));

			using var baseObj = GetBase();
			baseObj.Object.Description = description;
		}

		public void ChangeStartCondition(int startAtMembers, bool waitEveryoneInvited)
		{
			if (startAtMembers <= 0)
				throw new ArgumentException("StartAtMembers was negative or zero", nameof(startAtMembers));

			using var baseObj = GetBase();
			baseObj.Object.StartAtMembers = startAtMembers;
			baseObj.Object.WaitEveryoneInvited = waitEveryoneInvited;
		}

		private bool WaitingPlayersWaitingCreatorTransit()
		{
			using var holder = GetBase();
			var b = holder.Object;
			var cond1 = b.InGame.Count >= b.StartAtMembers - 1;
			var cond2 = b.WaitEveryoneInvited == false || b.Invited.All(s => b.InGame.Contains(s));
			return cond1 && cond2;
		}

		private void AttachEvents(IMessage message)
		{
			using var holder = GetBase();
			var b = holder.Object;

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
				using var holder = GetBase();
				var b = holder.Object;

				if (b.InGame.Contains(ctx.Invoker)) return new ComponentInteractionResult(new MessageSendModel(localizer["AlreadyInGame"]));
				else if (b.Creator == ctx.Invoker) return new ComponentInteractionResult(new MessageSendModel(localizer["CreatorAlreadyInGame"]));
				else
				{
					b.InGame.Add(ctx.Invoker);
					return new ComponentInteractionResult(new MessageSendModel(localizer["JoinOk"]));
				}
			}

			ComponentInteractionResult exitHandler(ComponentInteractionContext<MessageButton> ctx)
			{
				using var b = GetBase();

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
				using var holder = GetBase();
				var b = holder.Object;

				if (ctx.Invoker == b.Creator)
				{
					waitForStartButton.Callback();
					return new ComponentInteractionResult(new MessageSendModel(localizer["StartOk"]));
				}
				else return new ComponentInteractionResult(new MessageSendModel(localizer["StartFail"]));
			}

			ComponentInteractionResult finishHandler(ComponentInteractionContext<MessageButton> ctx)
			{
				using var holder = GetBase();
				var b = holder.Object;

				if (ctx.Invoker == b.Creator)
				{
					waitForFinishButton.Callback();
					return new ComponentInteractionResult(new MessageSendModel(localizer["FinishOk"]));
				}
				else return new ComponentInteractionResult(new MessageSendModel(localizer["FinishFail"]));
			}

			AsyncInteractionCallback<MessageButton> adaptate(Func<ComponentInteractionContext<MessageButton>, ComponentInteractionResult> func) => (ctx) => Task.FromResult(func(ctx));
		}

		public IReadOnlyCollection<IMember> GetInvited()
		{
			return (IReadOnlyCollection<IMember>)GetBaseAuto(s => s.Invited);
		}

		private MessageSendModel CreateReportMessage()
		{
			using var holder = GetBase();
			var b = holder.Object;

			return b.State switch
			{
				GameState.WaitingPlayers => uiHelper.CreateWatingForPlayersReport(b.Name, b.Creator, (IReadOnlyCollection<IMember>)b.Invited,
					(IReadOnlyCollection<IMember>)b.InGame, b.WaitEveryoneInvited, b.StartAtMembers, b.Description),
				GameState.WaitingCreator => uiHelper.CreataWaitingForCreatorReport(b.Name, b.Creator, (IReadOnlyCollection<IMember>)b.Invited,
					(IReadOnlyCollection<IMember>)b.InGame, b.WaitEveryoneInvited, b.StartAtMembers, b.Description),
				GameState.Running => uiHelper.CreateRunningReport(b.Name, b.Creator, (IReadOnlyCollection<IMember>)b.InGame, b.Description),
				_ => throw new ImpossibleVariantException()
			};
		}

		private static void Validate(GameModel baseObj)
		{
			foreach (var member in baseObj.Invited)
				if (member.Server != baseObj.Server)
					throw new ArgumentException("Some invited member was from other server", nameof(baseObj));
				else if (member == baseObj.Creator)
					throw new ArgumentException("Some invited member was creator", nameof(baseObj));
			foreach (var member in baseObj.InGame)
				if (member.Server != baseObj.Server)
					throw new ArgumentException("Some in-game member from other server", nameof(baseObj));
				else if (member == baseObj.Creator)
					throw new ArgumentException("Some in-game member was creator", nameof(baseObj));
			if (string.IsNullOrWhiteSpace(baseObj.Name))
				throw new ArgumentException("Name was whitespace", nameof(baseObj));
			if (string.IsNullOrWhiteSpace(baseObj.Description))
				throw new ArgumentException("Description was whitespace", nameof(baseObj));
			if (baseObj.StartAtMembers <= 0)
				throw new ArgumentException("StartAtMembers was negative or zero", nameof(baseObj));
		}
	}
}
