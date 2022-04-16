using CGZBot3.Data.Lifetime;
using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Components;
using CGZBot3.Utils;
using CGZBot3.Utils.StateMachine;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Games
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


		public GameLifetime(GameModel baseObj, IServiceProvider services) : base(services, baseObj)
		{
			Validate(baseObj);


			uiHelper = services.GetRequiredService<UIHelper>();
			localizer = services.GetRequiredService<IStringLocalizer<GameLifetime>>();


			AddReport(new MessageAliveHolder(baseObj.ReportMessage, true, CreateReportMessage, AttachEvents));

			AddTransit(GameState.WaitingPlayers, GameState.WaitingCreator, WaitingPlayersWaitingCreatorTransit);
			AddTransit(GameState.WaitingCreator, GameState.WaitingPlayers, () => !WaitingPlayersWaitingCreatorTransit());
			AddTransit(GameState.WaitingCreator, GameState.Running, waitForStartButton.Await);
			AddTransit(GameState.Running, null, waitForFinishButton.Await);
		}


		public void Invite(IReadOnlyCollection<IMember> members)
		{
			foreach (var member in members)
				if (member.Server != GetBaseDirect().Server)
					throw new ArgumentException("Some member from other server", nameof(members));

			using var baseObj = GetBase();
			foreach (var member in members)
				if (baseObj.Object.Invited.Contains(member) == false)
					baseObj.Object.Invited.Add(member);

			if (baseObj.Object.Invited.Contains(baseObj.Object.Creator))
				baseObj.Object.Invited.Remove(baseObj.Object.Creator);
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
			var b = GetBaseDirect();
			var cond1 = b.InGame.Count >= b.StartAtMembers - 1;
			var cond2 = b.WaitEveryoneInvited == false || b.Invited.All(s => b.InGame.Contains(s));
			return cond1 && cond2;
		}

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

		private MessageSendModel CreateReportMessage()
		{
			var b = GetBaseDirect();

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

		private void Validate(GameModel baseObj)
		{
			foreach (var member in baseObj.Invited)
				if (member.Server != baseObj.Server)
					throw new ArgumentException("Some invited member was from other server", nameof(baseObj));
				else if (member == baseObj.Creator)
					throw new ArgumentException("Some invited member was creator", nameof(baseObj));
			foreach (var member in baseObj.InGame)
				if (member.Server != GetBaseDirect().Server)
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
