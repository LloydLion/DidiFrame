using TestBot.Systems.Streaming.CommandEvironment;
using DidiFrame.Entities.Message;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation.Validators;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.Utils.Dialogs;
using DidiFrame.Utils.Dialogs.Messages;

namespace TestBot.Systems.Streaming
{
	internal class CommandHandler : ICommandsHandler
	{
		private readonly ISystemCore systemCore;
		private readonly IStringLocalizer<CommandHandler> localizer;
		private readonly IStringLocalizerFactory localizerFactory;
		private readonly MessageCollection fond;


		public CommandHandler(ISystemCore systemCore, IStringLocalizer<CommandHandler> localizer, IStringLocalizerFactory localizerFactory)
		{
			this.systemCore = systemCore;
			this.localizer = localizer;
			this.localizerFactory = localizerFactory;
			fond = new MessageCollection(new Dictionary<string, IDialogMessageFactory>()
			{
				{ "selectStream", new DirectMessageFactory<ListSelectorMessage<StreamLifetime>>() },
				{ "textInput", new DirectMessageFactory<TextInputMessage<string>>() },
				{ "placeInput", new DirectMessageFactory<TextInputMessage<string>>() },
				{ "dateInput", new DirectMessageFactory<TextInputMessage<DateTime>>() },
			});
		}


		[Command("stream create")]
		public Task<UserCommandResult> CreateStream(UserCommandContext ctx)
		{
			var dialog = new Dialog(fond, new DialogCreationArgs(ctx.Channel, ctx.Invoker, localizerFactory), new Dictionary<string, object>()
			{
				{ "textInput", new { Text = (string)localizer["EnterName"],
					Controller = new Func<string, bool>((str) => !string.IsNullOrWhiteSpace(str)),
					Output = new DialogInterMessageOutput<string>("name", true), NextMessage = "placeInput" } },
				{ "placeInput", new { Text = (string)localizer["EnterPlace"],
					Controller = new Func<string, bool>((str) => !string.IsNullOrWhiteSpace(str)),
					Output = new DialogInterMessageOutput<string>("place", true), NextMessage = "dateInput" } },
				{ "dateInput", new { Text = (string)localizer["EnterPlannedDate"],
					Controller = new Func<string, bool>((str) => !string.IsNullOrWhiteSpace(str) && DateTime.TryParse(str, out _)),
					Parser = new Func<string, DateTime>((str) => DateTime.Parse(str)), Output = new DialogInterMessageOutput<DateTime>("date", true), NextMessage = (string?)null } },
			});

			dialog.AddFinalizer(async () =>
			{
				var name = dialog.Context.MessageLink.GetGlobalVariable<string>("name");
				var place = dialog.Context.MessageLink.GetGlobalVariable<string>("place");
				var plannedStartDate = dialog.Context.MessageLink.GetGlobalVariable<DateTime>("date");

				systemCore.AnnounceStream(name, ctx.Invoker, plannedStartDate, place);

				await ctx.Channel.SendMessageAsync(new MessageSendModel(localizer["StreamCreated", name]));
			});

			dialog.Start("textInput");

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful));
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
