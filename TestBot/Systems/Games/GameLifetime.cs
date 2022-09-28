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


		public IMember GetCreator() => GetBaseProperty(s => s.Creator);

		public string GetName() => GetBaseProperty(s => s.Name);

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

		protected override void PrepareStateMachine(GameModel initialBase, InitialDataBuilder builder)
		{
			Validate(initialBase);

			AddReport(new MessageAliveHolder<GameModel>(s => s.ReportMessage, CreateReportMessage, AttachEvents), builder);

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
			using var holder = GetReadOnlyBase();
			var b = holder.Object;
			var cond1 = b.InGame.Count >= b.StartAtMembers - 1;
			var cond2 = b.WaitEveryoneInvited == false || b.Invited.All(s => b.InGame.Contains(s));
			return cond1 && cond2;
		}

		private void AttachEvents(GameModel parameter, IMessage message, bool isModified)
		{
			if (isModified) message.ResetInteractionDispatcher();

			//Every state contains components
			var di = message.GetInteractionDispatcher();

			switch (parameter.State)
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
				var holder = GetReadOnlyBase();
				var b = holder.Object;

				if (b.InGame.Contains(ctx.Invoker))
				{
					holder.Dispose();
					return ComponentInteractionResult.CreateWithMessage(new(localizer["AlreadyInGame"]));
				}
				else if (b.Creator.Equals((IUser)ctx.Invoker))
				{
					holder.Dispose();
					return ComponentInteractionResult.CreateWithMessage(new(localizer["CreatorAlreadyInGame"]));
				}
				else
				{

					//If all ok
					holder.Dispose();
					using var writableHolder = GetBase();
					writableHolder.Object.InGame.Add(ctx.Invoker);
					return ComponentInteractionResult.CreateWithMessage(new(localizer["JoinOk"]));
				}
			}

			ComponentInteractionResult exitHandler(ComponentInteractionContext<MessageButton> ctx)
			{
				var holder = GetReadOnlyBase();

				if (holder.Object.Creator == ctx.Invoker)
				{
					holder.Dispose();
					return ComponentInteractionResult.CreateWithMessage(new(localizer["CreatorMustBeInGame"]));
				}
				else if (!holder.Object.InGame.Contains(ctx.Invoker))
				{
					holder.Dispose();
					return ComponentInteractionResult.CreateWithMessage(new(localizer["AlreadyOutGame"]));
				}
				else
				{
					//If all ok
					holder.Dispose();
					using var writableHolder = GetBase();
					writableHolder.Object.InGame.Remove(ctx.Invoker);
					return ComponentInteractionResult.CreateWithMessage(new(localizer["ExitOk"]));
				}
			}

			ComponentInteractionResult startHandler(ComponentInteractionContext<MessageButton> ctx)
			{
				using var holder = GetReadOnlyBase();
				var b = holder.Object;

				if (ctx.Invoker.Equals((IUser)b.Creator))
				{
					waitForStartButton.Callback();
					return ComponentInteractionResult.CreateWithMessage(new(localizer["StartOk"]));
				}
				else return ComponentInteractionResult.CreateWithMessage(new(localizer["StartFail"]));
			}

			ComponentInteractionResult finishHandler(ComponentInteractionContext<MessageButton> ctx)
			{
				using var holder = GetReadOnlyBase();
				var b = holder.Object;

				if (ctx.Invoker.Equals((IUser)b.Creator))
				{
					waitForFinishButton.Callback();
					return ComponentInteractionResult.CreateWithMessage(new(localizer["FinishOk"]));
				}
				else return ComponentInteractionResult.CreateWithMessage(new(localizer["FinishFail"]));
			}

			AsyncInteractionCallback<MessageButton> adaptate(Func<ComponentInteractionContext<MessageButton>, ComponentInteractionResult> func) => (ctx) => Task.FromResult(func(ctx));
		}

		public IReadOnlyCollection<IMember> GetInvited() => (IReadOnlyCollection<IMember>)GetBaseProperty(s => s.Invited);

		private MessageSendModel CreateReportMessage(GameModel parameter)
		{
			return parameter.State switch
			{
				GameState.WaitingPlayers => uiHelper.CreateWatingForPlayersReport(parameter.Name, parameter.Creator, (IReadOnlyCollection<IMember>)parameter.Invited,
					(IReadOnlyCollection<IMember>)parameter.InGame, parameter.WaitEveryoneInvited, parameter.StartAtMembers, parameter.Description),
				GameState.WaitingCreator => uiHelper.CreataWaitingForCreatorReport(parameter.Name, parameter.Creator, (IReadOnlyCollection<IMember>)parameter.Invited,
					(IReadOnlyCollection<IMember>)parameter.InGame, parameter.WaitEveryoneInvited, parameter.StartAtMembers, parameter.Description),
				GameState.Running => uiHelper.CreateRunningReport(parameter.Name, parameter.Creator, (IReadOnlyCollection<IMember>)parameter.InGame, parameter.Description),
				_ => throw new ImpossibleVariantException()
			};
		}

		private static void Validate(GameModel baseObj)
		{
			foreach (var member in baseObj.Invited)
				if (member.Server.Equals(baseObj.Server) == false)
					throw new ArgumentException("Some invited member was from other server", nameof(baseObj));
				else if (member.Equals(baseObj.Creator))
					throw new ArgumentException("Some invited member was creator", nameof(baseObj));
			foreach (var member in baseObj.InGame)
				if (member.Server.Equals(baseObj.Server) == false)
					throw new ArgumentException("Some in-game member from other server", nameof(baseObj));
				else if (member.Equals(baseObj.Creator))
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
