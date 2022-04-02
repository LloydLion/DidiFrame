using CGZBot3.Entities.Message;
using CGZBot3.UserCommands;
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
		public Task<UserCommandResult> CreateStream(UserCommandContext ctx, string name, DateTime plannedStartDate, string place)
		{
			if (place.StartsWith("dc#")) place = localizer["InDiscordPlace", place];
			else place = localizer["ExternalPlace", place];

			systemCore.AnnounceStream(name, ctx.Invoker, plannedStartDate.ToUniversalTime(), place);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamCreated", name]) });
		}

		[Command("replanstream")]
		public Task<UserCommandResult> ReplanStream(UserCommandContext ctx, string name, DateTime plannedStartDate)
		{
			var lt = systemCore.GetLifetime(name, ctx.Invoker);

			using var baseObj = lt.GetBase();
			{
				if (baseObj.Object.State == StreamState.Running)
					return Task.FromResult(new UserCommandResult(UserCommandCode.OtherUserError)
					{ RespondMessage = new MessageSendModel(localizer["EnableToReplanStream", name]) });

				baseObj.Object.PlanedStartTime = plannedStartDate;
			}

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamReaplaned", name]) });
		}

		[Command("renamestream")]
		public Task<UserCommandResult> RenameStream(UserCommandContext ctx, string name, string newName)
		{
			var lt = systemCore.GetLifetime(name, ctx.Invoker);

			using (var baseObj = lt.GetBase())
			{
				baseObj.Object.Name = newName;
			}

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamRenamed", name]) });
		}

		[Command("movestream")]
		public Task<UserCommandResult> MoveStream(UserCommandContext ctx, string name, string newPlace)
		{
			var lt = systemCore.GetLifetime(name, ctx.Invoker);

			if (newPlace.StartsWith("dc#")) newPlace = localizer["InDiscordPlace", newPlace];
			else newPlace = localizer["ExternalPlace", newPlace];

			using var baseObj = lt.GetBase();
			{
				baseObj.Object.Place = newPlace;
			}

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["StreamRenamed", name]) });
		}
	}
}
