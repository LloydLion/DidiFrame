using CGZBot3.Entities.Message;
using CGZBot3.Systems.Streaming.CommandEvironment;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation.Validators;
using CGZBot3.UserCommands.Loader.Reflection;

namespace CGZBot3.Systems.Streaming
{
	internal class CommandHandler : ICommandsHandler
	{
		private readonly ISystemCore systemCore;
		private readonly IStringLocalizer<CommandHandler> localizer;


		public CommandHandler(ISystemCore systemCore, IStringLocalizer<CommandHandler> localizer)
		{
			this.systemCore = systemCore;
			this.localizer = localizer;
		}


		[Command("stream create")]
		public Task<UserCommandResult> CreateStream(UserCommandContext ctx, [Validator(typeof(GreaterThen), typeof(CommandHandler), nameof(GetNow))] DateTime plannedStartDate,
			[Validator(typeof(NormalString))] string place, [Validator(typeof(NormalString))][Validator(typeof(StreamExist), true)] string name)
		{
			if (place.StartsWith("dc#")) place = localizer["InDiscordPlace", place];
			else place = localizer["ExternalPlace", place];

			systemCore.AnnounceStream(name, ctx.Invoker, plannedStartDate.ToUniversalTime(), place);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamCreated", name]) });
		}

		[Command("stream replan")]
		public Task<UserCommandResult> ReplanStream(UserCommandContext _, DateTime plannedStartDate,
			[Validator(typeof(StreamExistAndInvokerIsOwner))] StreamLifetime stream)
		{
			stream.Replan(plannedStartDate);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamReplanned", stream.GetBaseClone().Name]) });
		}

		[Command("stream rename")]
		public Task<UserCommandResult> RenameStream(UserCommandContext _, [Validator(typeof(StreamExistAndInvokerIsOwner))] StreamLifetime stream,
			[Validator(typeof(NormalString))][Validator(typeof(StreamExist), true)] string newName)
		{
			stream.Rename(newName);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamRenamed", stream.GetBaseClone().Name]) });
		}

		[Command("stream move")]
		public Task<UserCommandResult> MoveStream(UserCommandContext _, [Validator(typeof(NormalString))] string newPlace, [Validator(typeof(StreamExistAndInvokerIsOwner))] StreamLifetime stream)
		{
			if (newPlace.StartsWith("dc#")) newPlace = localizer["InDiscordPlace", newPlace];
			else newPlace = localizer["ExternalPlace", newPlace];

			stream.Move(newPlace);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamMoved", stream.GetBaseClone().Name]) });
		}


		private static IComparable GetNow(UserCommandContext _) => DateTime.Now + new TimeSpan(0, 5, 0);
	}
}
