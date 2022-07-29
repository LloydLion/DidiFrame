using DidiFrame.Culture;
using DidiFrame.UserCommands.Pipeline;
using FluentValidation;

namespace DidiFrame.UserCommands.Executing
{
	/// <summary>
	/// Executor of user commands, main element of command pipeline
	/// </summary>
	public class DefaultUserCommandsExecutor : AbstractUserCommandPipelineMiddleware<ValidatedUserCommandContext, UserCommandResult>
	{
		private static readonly EventId CommandStartID = new(32, "CommandStart");
		private static readonly EventId InternalErrorID = new(36, "InternalError");


		private readonly Options options;
		private readonly IValidator<UserCommandContext> ctxValidator;
		private readonly ILogger<DefaultUserCommandsExecutor> logger;
		private readonly IServerCultureProvider cultureProvider;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Executing.DefaultUserCommandsExecutor
		/// </summary>
		/// <param name="ctxValidator">Validator for DidiFrame.UserCommands.Models.UserCommandContext</param>
		/// <param name="options">Option for executor (DidiFrame.UserCommands.Executing.DefaultUserCommandsExecutor.Options)</param>
		/// <param name="logger">Logger to log command operations</param>
		/// <param name="cultureProvider">Culture provider to provide culture into commands' handlers</param>
		public DefaultUserCommandsExecutor(IValidator<UserCommandContext> ctxValidator, IOptions<Options> options, ILogger<DefaultUserCommandsExecutor> logger, IServerCultureProvider cultureProvider)
		{
			this.options = options.Value;
			this.ctxValidator = ctxValidator;
			this.logger = logger;
			this.cultureProvider = cultureProvider;
		}


		/// <inheritdoc/>
		public override UserCommandResult? Process(ValidatedUserCommandContext ctx, UserCommandPipelineContext pipelineContext)
		{
			ctxValidator.ValidateAndThrow(ctx);

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
							RespondMessage = createExcetionMessage(ex),
							Exception = ex
						};
					}

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
				/// <summary>
				/// Disables any actions
				/// </summary>
				Disable,
				/// <summary>
				/// Sends simple error message without any debug info
				/// </summary>
				EnableWithoutDebugInfo,
				/// <summary>
				/// Sends message with exception type and its message
				/// </summary>
				EnableWithExceptionsTypeAndMessage,
				/// <summary>
				/// Sends message with full exception info
				/// </summary>
				EnableWithFullExceptionInfo
			}
		}
	}
}
