using DidiFrame.Culture;
using DidiFrame.UserCommands.Pipeline;

namespace DidiFrame.UserCommands.Executing
{
	/// <summary>
	/// Executor of user commands, main element of command pipeline
	/// </summary>
	public class DefaultUserCommandsExecutor : AbstractUserCommandPipelineMiddleware<ValidatedUserCommandContext, UserCommandResult>
	{
		private static readonly EventId CommandStartID = new (32, "CommandStart");
		private static readonly EventId CommandCompliteID = new (33, "CommandComplite");
		private static readonly EventId InternalErrorID = new(36, "InternalError");


		private readonly Options options;
		private readonly ILogger<DefaultUserCommandsExecutor> logger;
		private readonly IServerCultureProvider cultureProvider;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Executing.DefaultUserCommandsExecutor
		/// </summary>
		/// <param name="options">Option for executor</param>
		/// <param name="logger">Logger to log command operations</param>
		/// <param name="cultureProvider">Culture provider to provide culture into commands' handlers</param>
		public DefaultUserCommandsExecutor(IOptions<Options> options, ILogger<DefaultUserCommandsExecutor> logger, IServerCultureProvider cultureProvider)
		{
			this.options = options.Value;
			this.logger = logger;
			this.cultureProvider = cultureProvider;
		}


		public override UserCommandResult? Process(ValidatedUserCommandContext ctx, UserCommandPipelineContext pipelineContext)
		{
			try
			{
				UserCommandResult result;

				using (logger.BeginScope("Command: {CommandName}", ctx.Command.Name))
				{
					cultureProvider.SetupCulture(ctx.Invoker.Server);

					logger.Log(LogLevel.Debug, CommandStartID, "Command executing started");

					try
					{
						result = ctx.Command.Handler(ctx).Result;
					}
					catch (Exception ex)
					{
						result = new UserCommandResult(UserCommandCode.UnspecifiedError)
						{
							RespondMessage = createExcetionMessage(ex)
						};
					}

					logger.Log(LogLevel.Debug, CommandCompliteID, "Command executed with code {ResultCode}", result.Code);

					return result;
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
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, InternalErrorID, ex, "Exception while handling the command (Internal)");
				pipelineContext.DropPipeline();
				return null;
			}
		}


		/// <summary>
		/// Options for DidiFrame.UserCommands.Executing.DefaultUserCommandsExecutor
		/// </summary>
		public class Options
		{
			/// <summary>
			/// Message type when command's handler throws a exception
			/// </summary>
			public UnspecifiedErrorMessageBehavior UnspecifiedErrorMessage { get; set; } = UnspecifiedErrorMessageBehavior.EnableWithExceptionsTypeAndMessage;


			/// <summary>
			/// Type of behavior to exception throwing by command's handler
			/// </summary>
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
