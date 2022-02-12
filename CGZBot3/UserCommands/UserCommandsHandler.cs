using CGZBot3.UserCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.UserCommands
{
	internal class UserCommandsHandler : IUserCommandsHandler
	{
		private readonly Options options;
		private readonly IValidator<UserCommandContext> ctxVal;
		private readonly ILogger<UserCommandsHandler> logger;


		public UserCommandsHandler(IOptions<Options> options, IValidator<UserCommandContext> ctxVal, ILogger<UserCommandsHandler> logger)
		{
			this.options = options.Value;
			this.ctxVal = ctxVal;
			this.logger = logger;
		}


		public async Task HandleAsync(UserCommandContext ctx)
		{
			UserCommandResult result;

			ctxVal.ValidateAndThrow(ctx);

			using(logger.BeginScope("cmd: {CommandName}", ctx.Command.Name))

			try
			{
				result = await ctx.Command.Handler.Invoke(ctx);
			}
			catch(Exception ex)
			{
				result = new UserCommandResult(UserCommandCode.UnspecifiedError)
				{
					RespondMessage = createExcetionMessage(ex)
				};
			}

			logger.Log(LogLevel.Trace, new EventId(33, "Command complite"), "Command executed with code {ResultCode}", result.Code);

			if(result.RespondMessage != null)
			{
				await ctx.Channel.SendMessageAsync(result.RespondMessage);
				logger.Log(LogLevel.Trace, new EventId(32, "Message sent"), "Message sent with content: {Content}", result.RespondMessage.Content);
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
