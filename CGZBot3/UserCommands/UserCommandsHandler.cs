using CGZBot3.Culture;
using CGZBot3.UserCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.UserCommands
{
	public class UserCommandsHandler : IUserCommandsHandler
	{
		private static readonly EventId CommandCompliteID = new (33, "CommandComplite");
		private static readonly EventId CallbackDoneID = new (35, "CallbackDone");
		private static readonly EventId MessageSentID = new(34, "MessageSent");


		private readonly Options options;
		private readonly IValidator<UserCommandContext> ctxVal;
		private readonly ILogger<UserCommandsHandler> logger;
		private readonly IServerCultureProvider cultureProvider;


		public UserCommandsHandler(IOptions<Options> options, IValidator<UserCommandContext> ctxVal, ILogger<UserCommandsHandler> logger, IServerCultureProvider cultureProvider)
		{
			this.options = options.Value;
			this.ctxVal = ctxVal;
			this.logger = logger;
			this.cultureProvider = cultureProvider;
		}


		public async Task HandleAsync(UserCommandContext ctx, Action<UserCommandResult> callback)
		{
			UserCommandResult result;

			ctxVal.ValidateAndThrow(ctx);

			using (logger.BeginScope("Command: {CommandName}", ctx.Command.Name))
			{
				ctx.AddLogger(logger);

				cultureProvider.SetupCulture(ctx.Invoker.Server);

				try
				{
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
						text =	"Command excecution finished with error\n" +
								$"Error: {ex}\n" +
								"Code: " + nameof(UserCommandCode.UnspecifiedError);
						break;
					case Options.UnspecifiedErrorMessageBehavior.EnableWithFullExceptionInfo:
						text =	"Command excecution finished with error\n" +
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
