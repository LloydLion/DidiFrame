using DidiFrame.Culture;
using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.Utils;

namespace DidiFrame.UserCommands.Executing
{
	public class DefaultUserCommandsExecutor : IUserCommandsExecutor
	{
		private static readonly EventId CommandStartID = new (32, "CommandStart");
		private static readonly EventId CommandCompliteID = new (33, "CommandComplite");
		private static readonly EventId CallbackDoneID = new (35, "CallbackDone");
		private static readonly EventId MessageSentID = new(34, "MessageSent");
		private static readonly EventId InternalErrorID = new(36, "InternalError");


		private readonly Options options;
		private readonly ILogger<DefaultUserCommandsExecutor> logger;
		private readonly IServerCultureProvider cultureProvider;
		private readonly IStringLocalizer<DefaultUserCommandsExecutor> localizer;
		private readonly IUserCommandContextConverter converter;
		private readonly IServiceProvider services;
		private readonly ThreadLocker<IServer> threadLocker = new();


		public DefaultUserCommandsExecutor(
			IOptions<Options> options,
			ILogger<DefaultUserCommandsExecutor> logger,
			IServerCultureProvider cultureProvider,
			IStringLocalizer<DefaultUserCommandsExecutor> localizer,
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
			try
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
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, InternalErrorID, ex, "Exception while handling the command (Internal)");
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
