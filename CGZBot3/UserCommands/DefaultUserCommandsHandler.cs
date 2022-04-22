using CGZBot3.Culture;
using CGZBot3.Entities.Message;
using CGZBot3.Utils;

namespace CGZBot3.UserCommands
{
	public class DefaultUserCommandsHandler : IUserCommandsHandler
	{
		private static readonly EventId CommandStartID = new (32, "CommandStart");
		private static readonly EventId CommandCompliteID = new (33, "CommandComplite");
		private static readonly EventId CallbackDoneID = new (35, "CallbackDone");
		private static readonly EventId MessageSentID = new(34, "MessageSent");


		private readonly Options options;
		private readonly ILogger<DefaultUserCommandsHandler> logger;
		private readonly IServerCultureProvider cultureProvider;
		private readonly IStringLocalizer<DefaultUserCommandsHandler> localizer;
		private readonly IUserCommandContextConverter converter;
		private readonly IServiceProvider services;
		private readonly ThreadLocker<IServer> threadLocker = new();


		public DefaultUserCommandsHandler(
			IOptions<Options> options,
			ILogger<DefaultUserCommandsHandler> logger,
			IServerCultureProvider cultureProvider,
			IStringLocalizer<DefaultUserCommandsHandler> localizer,
			IUserCommandContextConverter converter,
			IServiceProvider services)
		{
			this.options = options.Value;
			this.logger = logger;
			this.cultureProvider = cultureProvider;
			this.localizer = localizer;
			this.converter = converter;
			this.services = services;
		}


		public async Task HandleAsync(UserCommandPreContext preCtx, Action<UserCommandResult> callback)
		{
			using (threadLocker.Lock(preCtx.Channel.Server))
			{
				UserCommandResult result;

				using (logger.BeginScope("Command: {CommandName}", preCtx.Command.Name))
				{
					preCtx.AddLogger(logger);

					cultureProvider.SetupCulture(preCtx.Invoker.Server);

					logger.Log(LogLevel.Debug, CommandStartID, "Command executing started");

					foreach (var argument in preCtx.Command.Arguments)
					{
						foreach (var validator in argument.PreValidators)
						{
							var tf = validator.Validate(services, preCtx, argument, preCtx.Arguments[argument]);
							if (tf is null) continue;
							else
							{
								var content = localizer["ValidationErrorMessage", preCtx.Command.Localizer[$"{preCtx.Command.Name}.{argument.Name}:{tf}", preCtx.Arguments[argument]]];
								callback(new UserCommandResult(UserCommandCode.InvalidInput) { RespondMessage = new MessageSendModel(content) });
								return;
							}
						}
					}

					var ctx = converter.Convert(preCtx);
					ctx.AddLogger(logger);

					foreach (var filter in ctx.Command.InvokerFilters)
					{
						var tf = filter.Filter(ctx);
						if (tf is null) continue;
						else
						{
							var content = localizer["ByFilterBlockedMessage", ctx.Command.Localizer[$"{ctx.Command.Name}:{tf.LocaleKey}"]];
							callback(new UserCommandResult(tf.Code) { RespondMessage = new MessageSendModel(content) });
							return;
						}
					}

					foreach (var argument in ctx.Command.Arguments)
					{
						foreach (var validator in argument.Validators)
						{
							var tf = validator.Validate(services, ctx, argument, ctx.Arguments[argument]);
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
