using CGZBot3.Culture;
using CGZBot3.Entities.Message;
using CGZBot3.Utils;

namespace CGZBot3.UserCommands
{
	public class UserCommandsHandler : IUserCommandsHandler
	{
		private static readonly EventId CommandStartID = new (32, "CommandStart");
		private static readonly EventId CommandCompliteID = new (33, "CommandComplite");
		private static readonly EventId CallbackDoneID = new (35, "CallbackDone");
		private static readonly EventId MessageSentID = new(34, "MessageSent");


		private readonly Options options;
		private readonly ILogger<UserCommandsHandler> logger;
		private readonly IServerCultureProvider cultureProvider;
		private readonly IStringLocalizer<UserCommandsHandler> localizer;
		private readonly ThreadLocker<IServer> threadLocker = new();


		public UserCommandsHandler(IOptions<Options> options, ILogger<UserCommandsHandler> logger, IServerCultureProvider cultureProvider, IStringLocalizer<UserCommandsHandler> localizer)
		{
			this.options = options.Value;
			this.logger = logger;
			this.cultureProvider = cultureProvider;
			this.localizer = localizer;
		}


		public async Task HandleAsync(UserCommandContext ctx, Action<UserCommandResult> callback)
		{
			using (threadLocker.Lock(ctx.Channel.Server))
			{
				UserCommandResult result;

				using (logger.BeginScope("Command: {CommandName}", ctx.Command.Name))
				{
					ctx.AddLogger(logger);

					cultureProvider.SetupCulture(ctx.Invoker.Server);

					logger.Log(LogLevel.Debug, CommandStartID, "Command executing started");

					foreach (var argument in ctx.Command.Arguments)
					{
						foreach (var validator in argument.Validators)
						{
							var tf = validator.Validate(ctx, argument, ctx.Arguments[argument]);
							if (tf is null) continue;
							else
							{
								var content = localizer["ValidationErrorMessage", ctx.Command.Localizer[$"{ctx.Command.Name}.{argument.Name}:{tf}", ctx.Arguments[argument]]];
								callback(new UserCommandResult(UserCommandCode.InvalidInput) { RespondMessage = new MessageSendModel(content) });
								return;
							}
						}
					}

					try
					{
						using (logger.BeginScope("Internal"))
							result = await ctx.Command.Handler.Invoke(ctx);
					}
					catch (Exception ex)
					{
						result = new UserCommandResult(UserCommandCode.UnspecifiedError)
						{
							RespondMessage = createExcetionMessage(ex)
						};
					}

					logger.Log(LogLevel.Debug, CommandCompliteID, "Command executed with code {ResultCode}", result.Code);

					callback(result);

					logger.Log(LogLevel.Trace, CallbackDoneID, "Callback done");

					if (result.RespondMessage is not null)
						logger.Log(LogLevel.Trace, MessageSentID, "Message sent with content: {Content}", result.RespondMessage.Content);
				}


				MessageSendModel? createExcetionMessage(Exception ex)
				{
					string text;
					switch (options.UnspecifiedErrorMessage)
					{
						case Options.UnspecifiedErrorMessageBehavior.Disable:
							return null;
						case Options.UnspecifiedErrorMessageBehavior.EnableWithoutDebugInfo:
							text = "Command excecution finished with error\nCode: " + nameof(UserCommandCode.UnspecifiedError);
							break;
						case Options.UnspecifiedErrorMessageBehavior.EnableWithExceptionsTypeAndMessage:
							text = "Command excecution finished with error\n" +
									$"Error: {ex}\n" +
									"Code: " + nameof(UserCommandCode.UnspecifiedError);
							break;
						case Options.UnspecifiedErrorMessageBehavior.EnableWithFullExceptionInfo:
							text = "Command excecution finished with error\n" +
									$"Error: {ex}\n" +
									$"Stack: {ex.StackTrace}" +
									$"InnerException: {ex.InnerException?.ToString() ?? "No inner exception"}\n" +
									$"InnerExceptionStack: {ex.InnerException?.StackTrace ?? "No inner exception"}\n" +
									"Code: " + nameof(UserCommandCode.UnspecifiedError);
							break;
						default: throw new Exception(); //Never be
					}

					return new MessageSendModel(text);
				}
			}
		}


		public class Options
		{
			public UnspecifiedErrorMessageBehavior UnspecifiedErrorMessage { get; } = UnspecifiedErrorMessageBehavior.EnableWithExceptionsTypeAndMessage;


			public enum UnspecifiedErrorMessageBehavior
			{
				Disable,
				EnableWithoutDebugInfo,
				EnableWithExceptionsTypeAndMessage,
				EnableWithFullExceptionInfo
			}
		}
	}
}
