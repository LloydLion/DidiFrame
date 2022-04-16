using CGZBot3.Entities.Message;
using CGZBot3.Systems.Streaming.Validators;
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


		[Command("stream")]
		public Task<UserCommandResult> CreateStream(UserCommandContext ctx, [Validator(typeof(GreaterThen), typeof(CommandHandler), nameof(GetNow))] DateTime plannedStartDate,
			[Validator(typeof(NormalString))] string place, [Validator(typeof(NormalString))][Validator(typeof(StreamExist), true)] string name)
		{
			if (place.StartsWith("dc#")) place = localizer["InDiscordPlace", place];
			else place = localizer["ExternalPlace", place];

			systemCore.AnnounceStream(name, ctx.Invoker, plannedStartDate.ToUniversalTime(), place);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamCreated", name]) });
		}

		[Command("replanstream")]
		public Task<UserCommandResult> ReplanStream(UserCommandContext ctx, DateTime plannedStartDate,
			[Validator(typeof(StreamExistAndInvokerIsOwner))] string name)
		{
			var stream = systemCore.GetStream(ctx.Invoker.Server, name);

			stream.Replan(plannedStartDate);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamReplanned", name]) });
		}

		[Command("renamestream")]
		public Task<UserCommandResult> RenameStream(UserCommandContext ctx, [Validator(typeof(StreamExistAndInvokerIsOwner))] string name,
			[Validator(typeof(NormalString))][Validator(typeof(StreamExist), true)] string newName)
		{
			var stream = systemCore.GetStream(ctx.Invoker.Server, name);

			stream.Rename(newName);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamRenamed", name]) });
		}

		[Command("movestream")]
		public Task<UserCommandResult> MoveStream(UserCommandContext ctx, [Validator(typeof(NormalString))] string newPlace, [Validator(typeof(StreamExistAndInvokerIsOwner))] string name)
		{
			var stream = systemCore.GetStream(ctx.Invoker.Server, name);

			if (newPlace.StartsWith("dc#")) newPlace = localizer["InDiscordPlace", newPlace];
			else newPlace = localizer["ExternalPlace", newPlace];

			stream.Move(newPlace);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamMoved", name]) });
		}


		private static IComparable GetNow(UserCommandContext _) => DateTime.Now + new TimeSpan(0, 5, 0);
	}
}
